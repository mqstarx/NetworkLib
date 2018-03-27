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

        public event EventHandler DataRecieved;
        public event EventHandler Error;
      
        private int m_BufferSize = 8192;

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
            m_Port = port;
            m_Info = "OnServer";
            m_ServerThread = new Thread(new ThreadStart(StartServerThread));
            m_ServerThread.Start();
        }
        public void StopServer()
        {
            m_StopServerFlag = true;
            m_TcpListener.Stop();
            m_ServerThread.Abort();
        }
      
      
        private void StartServerThread()
        {

            try
            {
                m_TcpListener = new TcpListener(IPAddress.Any, m_Port);
                m_TcpListener.Start();
               
                while (!m_StopServerFlag)
                {
                   
                    m_TcpClient = m_TcpListener.AcceptTcpClient();
                    m_TcpClient.SendBufferSize = m_BufferSize;
                    m_TcpClient.ReceiveBufferSize = m_BufferSize;
                    NetworkStream stream = m_TcpClient.GetStream();
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
                                DataRecieved(new object[] { this, buffer }, null);
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
                            is_error = true;
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
