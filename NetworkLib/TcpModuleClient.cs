using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib
{
    public class TcpModuleClient
    {
        private Socket _client;
        private int m_BufferSize = 8192;
        public event EventHandler Error;
        public event EventHandlerRecievedObject Recieved;
        public event EventHandler Connected;
        public event EventHandler DataSended;
        public TcpModuleClient()
        { }

        public void Connect(string ip,int port)
        {
            // Connect to a remote device.  
            try
            {
                
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);

                // Create a TCP/IP socket.  
               _client = new Socket(SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                _client.BeginConnect(remoteEP, ConnectCallback, new SocketData(_client, new byte[m_BufferSize]));
            

            }
            catch (Exception e)
            {
                if (Error != null)
                    Error(e.Message, null);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            SocketData s = (SocketData)ar.AsyncState;
            if(s.Socket.Connected)
            {
                if (Connected != null)
                    Connected("Connected", null);
                s.Socket.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None,ReadCallBack, s);
            }

        }

        private void ReadCallBack(IAsyncResult ar)
        {
            SocketData s = (SocketData)ar.AsyncState;
            try
            {
                int r = ((SocketData)ar.AsyncState).Socket.EndReceive(ar);
                if (r > 0)
                {
                    byte[] real_buff = new byte[r];

                    Array.Copy(s.Buffer, real_buff, r);




                    s.Data.AddRange(real_buff);

                    if (SocketData.FindFlag(real_buff, SocketData.FlagEnd, false))
                    {

                        // if (real_crc[0] == s.Data[SocketData.FlagBegin.Length] && real_crc[1] == s.Data[SocketData.FlagBegin.Length + 1] && real_crc[2] == s.Data[SocketData.FlagBegin.Length + 2] && real_crc[3] == s.Data[SocketData.FlagBegin.Length + 3])
                        //  {
                        byte[] send = new byte[s.Data.Count - SocketData.FlagBegin.Length - SocketData.FlagEnd.Length - 4];
                        Array.Copy(s.Data.ToArray(), SocketData.FlagBegin.Length + 4, send, 0, send.Length);
                        if (Recieved != null)
                            Recieved(SocketData.ObjectFromByteArray(send, 0, send.Length), s);
                        send = null;
                        s.Data.Clear();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        //  }
                    }
                }
                s.Socket.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, ReadCallBack, s);
            }
            catch (Exception e)
            {
                if (Error != null)
                    Error(e.Message, null);
            }

        }
        public void Send( object data)
        {
            try
            {
                byte[] obj = SocketData.GetByteArrayOfObject(data);
                byte[] data_send = new byte[obj.Length + SocketData.FlagBegin.Length + SocketData.FlagEnd.Length + 4];
                Array.Copy(SocketData.FlagBegin, 0, data_send, 0, SocketData.FlagBegin.Length);
                Array.Copy(SocketData.CalculateCrc(obj), 0, data_send, SocketData.FlagBegin.Length, 4);
                Array.Copy(obj, 0, data_send, SocketData.FlagBegin.Length + 4, obj.Length);
                Array.Copy(SocketData.FlagEnd, 0, data_send, data_send.Length - SocketData.FlagEnd.Length, SocketData.FlagEnd.Length);

                if (_client.Connected)
                {
                    _client.BeginSend(data_send, 0, data_send.Length, SocketFlags.None, SendCallback, new SocketData(_client, data_send));
                }

            }
            catch (Exception e)
            {

                if (Error != null)
                    Error(e.Message, null);
            }

        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {

                  SocketData handler = (SocketData)ar.AsyncState;
                /*  int bytesSent = handler.Socket.EndSend(ar);
                  handler.Socket.Shutdown(SocketShutdown.Both);
                  handler.Socket.Close();*/
                if (DataSended != null)
                    DataSended(SocketData.ObjectFromByteArray(handler.Buffer, 0, handler.Buffer.Length),null);

            }
            catch (Exception e)
            {

                if (Error != null)
                    Error(e.Message, null);
            }
        }

        public void Stop()
        {
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }
    }
}
