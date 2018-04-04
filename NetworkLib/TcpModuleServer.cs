using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkLib
{
    public class TcpModuleServer
    {


        private int m_BufferSize=8192;
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public event EventHandler Error;
        public event EventHandlerRecievedObject Recieved;
        private Socket _ServerSocket;
        public TcpModuleServer()
        {


        }
        public void StartServer(int port)
        {
            _ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, port);
            try
            {
                _ServerSocket.Bind(ipPoint);
                _ServerSocket.Listen(1000);
               _ServerSocket.BeginAccept(AcceptClient, new SocketData(_ServerSocket, new byte[m_BufferSize]));
             
            }
            catch(Exception e)
            {
                if (Error != null)
                    Error(e.Message, null);
            }
        }

        private void AcceptClient(IAsyncResult ar)
        { 
            try
            {
                SocketData s = (SocketData)ar.AsyncState;
                Socket _clientSocket = s.Socket.EndAccept(ar);
                _clientSocket.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, RecievedData, new SocketData(_clientSocket, s.Buffer));
                s.Socket.BeginAccept(AcceptClient, new SocketData(s.Socket, new byte[m_BufferSize]));
            }
            catch(Exception e)
            {
                if (Error != null)
                    Error(e.Message, null);
            }
            
        }
    
        private void RecievedData(IAsyncResult ar)
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
                        
                 
                            byte[] send = new byte[s.Data.Count - SocketData.FlagBegin.Length - SocketData.FlagEnd.Length - 4];
                            Array.Copy(s.Data.ToArray(), SocketData.FlagBegin.Length + 4, send, 0, send.Length);
                            if (Recieved != null)
                                Recieved(SocketData.ObjectFromByteArray(send, 0, send.Length), s);
                            send = null;
                            s.Data.Clear();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                     
                    }
                }
                s.Socket.BeginReceive(s.Buffer, 0, s.Buffer.Length, SocketFlags.None, RecievedData, s);
            }
            catch(Exception e)
            {
                if (Error != null)
                    Error(e.Message, null);
            }
        }

        public  void Send(Socket handler, object data)
        {
            try
            {
                byte[] obj = SocketData.GetByteArrayOfObject(data);
                byte[] data_send = new byte[obj.Length + SocketData.FlagBegin.Length + SocketData.FlagEnd.Length + 4];
                Array.Copy(SocketData.FlagBegin, 0, data_send, 0, SocketData.FlagBegin.Length);
                Array.Copy(SocketData.CalculateCrc(obj), 0, data_send, SocketData.FlagBegin.Length, 4);
                Array.Copy(obj, 0, data_send, SocketData.FlagBegin.Length + 4, obj.Length);
                Array.Copy(SocketData.FlagEnd, 0, data_send, data_send.Length - SocketData.FlagEnd.Length, SocketData.FlagEnd.Length);
               
                    if (handler.Connected)
                    {
                        handler.BeginSend(data_send,0,data_send.Length, SocketFlags.None,SendCallback,new SocketData(handler,data_send));
                    }
               
            }
            catch(Exception e)
            {
                
                if (Error!= null)
                    Error(e.Message, null);
            }
          
        }

        private  void SendCallback(IAsyncResult ar)
        {
          /*  try
            {

              /*  SocketData handler = (SocketData)ar.AsyncState;            
                int bytesSent = handler.Socket.EndSend(ar);
                handler.Socket.Shutdown(SocketShutdown.Both);
                handler.Socket.Close();

            }
            catch (Exception e)
            {

                if (Error != null)
                    Error(e.Message, null);
            }*/
        }

       /* public void StopServer()
        {
            
           // _ServerSocket.Shutdown(SocketShutdown.Both);
            //_ServerSocket.Close();

        }*/
    }
}
