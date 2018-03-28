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

    public delegate void EventHandlerLowLevelNet(byte[] buffer, TcpModuleLowLevel tcpClient);
    public class TcpModuleLowLevel
    {
        private TcpListener m_TcpListener;
        private TcpClient m_TcpClient;

        private Thread m_ServerThread;
        private Thread m_ReadThread;
        private int m_Port;
        private string m_ServerIp;
        private string m_Info;
        private bool m_StopServerFlag = false;

        public event EventHandlerLowLevelNet DataRecieved;
        public event EventHandler Error;
        public event EventHandler ConnectionLost;
       
        private int m_BufferSize = 60000;

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

        public TcpModuleLowLevel()
        {
           
        }

        public void StartServer(int port)
        {
            if (m_TcpListener == null)
            {
                try
                {
                    m_Port = port;
                    m_Info = "OnServer";
                    m_TcpListener = new TcpListener(IPAddress.Any, m_Port);
                    m_TcpListener.Start();
                    m_ServerThread = new Thread(new ThreadStart(StartServerThread));
                    m_ServerThread.Start();
                }
                catch(Exception e)
                {
                    if (Error != null)
                        Error(m_Info + " : " + e.Message, null);
                }
            }
        }
        public void StopServer()
        {
            if (m_TcpListener != null)
            {
                m_StopServerFlag = true;

               
                if (m_TcpClient != null)
                {
                    m_TcpClient.GetStream().Close();
                    m_TcpClient.Close();
                }

                m_TcpListener.Stop();
                m_TcpListener = null;
                if(m_ServerThread!=null)
                m_ServerThread.Abort();
            }
      
        }
      
      
        private void StartServerThread()
        {

            try
            {
               
               
                while (!m_StopServerFlag)
                {
                    NetworkStream stream=null;
                    try
                    {
                        m_TcpClient = m_TcpListener.AcceptTcpClient();
                        m_TcpClient.SendBufferSize = m_BufferSize;
                        m_TcpClient.ReceiveBufferSize = m_BufferSize;
                        stream= m_TcpClient.GetStream();
                    }
                    catch
                    { }
                   
                    bool is_error = false;
                    int r=0;
                    while (!is_error && !m_StopServerFlag)
                    {

                        byte[] buffer = new byte[m_BufferSize];
                        try
                        {
                           r=   stream.Read(buffer, 0, buffer.Length);
                        }
                        catch (Exception e)
                        {
                            if (Error != null)
                                Error(m_Info + " : " + e.Message, null);
                            is_error = true;
                        }
                        finally
                        {
                            
                            if (DataRecieved != null && !is_error && r>0)
                                DataRecieved(buffer,this);
                            if (r == 0)
                                is_error = true;
                        }
                    }
                    
                }
            }
            catch (Exception e)
            {

                if (Error != null)
                    Error(m_Info + " : " + e.Message, null);
            }
        }
        public void ConnectToServer(string ip,int port)
        {
            try
            {
                m_Info = "OnClient";
                m_Port = port;
                m_ServerIp = ip;
                m_TcpClient = new TcpClient();

                m_TcpClient.Connect(m_ServerIp, m_Port);
                m_ReadThread = new Thread(new ThreadStart(ReadThread));
                m_ReadThread.Start();
            }
            catch(Exception e)
            {
                if (Error != null)
                    Error(m_Info+" : "+ e.Message, null);
            }
        }
        private void ReadThread()
        {
            
            try
            {
                byte[] buffer = new byte[m_BufferSize];
                m_TcpClient.SendBufferSize = m_BufferSize;
                m_TcpClient.ReceiveBufferSize = m_BufferSize;
                NetworkStream stream = m_TcpClient.GetStream();
                bool is_error = false;
                int r = 0;
                while (!is_error)
                {
                    try
                    {
                        r = stream.Read(buffer, 0, buffer.Length);
                    }
                    catch(Exception e)
                    {
                        if (Error != null)
                           Error(m_Info + " : " + e.Message, null);
                        is_error = true;
                    }
                    finally
                    {
                        if (DataRecieved != null && !is_error && r > 0)
                            DataRecieved(buffer, null);
                        if (r == 0)
                        {
                            is_error = true;
                            if (ConnectionLost != null)
                                ConnectionLost(null, null);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                if (Error != null)
                    Error(m_Info + " : " + e.Message, null);
            }

        }
        public void SendData(byte[] data)
        {
            if(m_TcpClient!=null && m_TcpClient.Connected)
            {
                NetworkStream ns = m_TcpClient.GetStream();    
                ns.Write(data,0,data.Length);
              
            }
        }
        public void DisconnectFromServer()
        {
            if (m_TcpClient != null && m_TcpClient.Connected)
            {
                m_TcpClient.Close();
                m_ReadThread.Abort();
            }
        }

    }
}
