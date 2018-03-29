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

    public delegate void EventHandlerLowLevelSNet(byte[] buffer, TcpModuleLowLevelS tcpClient);
    public class TcpModuleLowLevelS
    {
       

        private Thread m_ServerThread;
        private Thread m_ReadThread;

        private string m_Info;

        public event EventHandlerLowLevelSNet DataRecieved;
        public event EventHandler Error;
        public event EventHandler ConnectionLost;

        private  Socket m_Socket;
        private Socket m_Handler;
        private int m_BufferSize = 1024;

        public int BufferSize
        {
            get
            {
                return m_BufferSize;
            }

            set
            {
                m_BufferSize = value;
            }
        }

        public TcpModuleLowLevelS()
        {

        }

        public void StartServer(int port)
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, port);
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
              
                m_Socket.Bind(ipPoint);
               
                m_ServerThread = new Thread(new ThreadStart(StartServerThread));
                m_ServerThread.Start();
            }
            catch(Exception e)
            {
                m_Info = "on server err";
               
            }
        }
        public void StopServer()
        {

            m_Socket.Shutdown(SocketShutdown.Both);
            m_Socket = null;

        }


        private void StartServerThread()
        {
            m_Socket.Listen(10);
            while (true)
            {
                m_Handler =  m_Socket.Accept();
                int bytes = 0;
                List<byte> dataBuffer = new List<byte>();
                byte[] tmp_buff = new byte[m_BufferSize];
                while (m_Handler.Connected)
                {
                    do
                    {
                        bytes = m_Handler.Receive(tmp_buff);
                        dataBuffer.AddRange(tmp_buff);
                        Thread.Sleep(1);
                    }
                    while (m_Handler.Available > 0);

                    if (DataRecieved != null)
                    {
                        DataRecieved(dataBuffer.ToArray(), this);
                        dataBuffer.Clear();
                    }
                }
                
            }
        }
        public void ConnectToServer(string ip, int port)
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                m_Socket.Connect(ipPoint);
                m_ReadThread = new Thread(new ThreadStart(ReadThread));
                m_ReadThread.Start();
            }
            catch (Exception e)
            {
                m_Info = "on client err";

            }
        }
        private void ReadThread()
        {
            while (m_Socket.Connected)
            {
                int bytes = 0;
                List<byte> dataBuffer = new List<byte>();
                byte[] tmp_buff = new byte[m_BufferSize];
                do
                {
                    bytes = m_Socket.Receive(tmp_buff);
                    dataBuffer.AddRange(tmp_buff);
                    Thread.Sleep(1);
                }
                while (m_Socket.Available > 0);

                if (DataRecieved != null)
                {
                    DataRecieved(dataBuffer.ToArray(), null);
                    dataBuffer.Clear();
                }
            }
        }
        public void SendDataFromClient(byte[] data)
        {
            if(m_Socket.Connected)
                m_Socket.Send(data);
        }
        public void SendDataFromServer(byte[] data)
        {
            if (m_Handler!=null && m_Handler.Connected)
                m_Handler.Send(data);
        }
        public void DisconnectFromServer()
        {
            m_Socket.Disconnect(false);
        }

    }
}
