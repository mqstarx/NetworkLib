using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkLib
{
    public delegate void TcpMiddleDataRecieved(object obj, TcpModuleMiddleLevel tcp);
    public delegate void TcpSendProgress(int uid, int all_count, int cur);
    public class TcpModuleMiddleLevel
    {
        //Протокол: 0-й байт идентификатор пакета 24
        //следующие 2 байта длина пакета без учета заголовака
        //далее объект класса сериализованный TcpSession


        private TcpModuleLowLevel m_TcpLow;
        //private SendQueue m_SendQueue;
        private List<TcpSession> m_SendQueue;
        private List<object[]> m_RecievedQueue;

        public event TcpMiddleDataRecieved DataRecieved;
        public event EventHandler ConnectionLost;
        public event TcpSendProgress TcpSendProgress;

        public TcpModuleMiddleLevel()
        {
            m_TcpLow = new TcpModuleLowLevel();
           
            m_TcpLow.DataRecieved += M_TcpLow_DataRecieved;
            m_SendQueue = new List<TcpSession>();
            m_RecievedQueue = new List<object[]>();
        }

        private void M_TcpLow_DataRecieved(byte[] buffer, TcpModuleLowLevel tcpClient)
        {
            if (buffer[0] == 24)
            {
                TcpSession tcp_session = (TcpSession)ObjectFromByteArray(buffer, 3, CalculatePacketLen(new byte[] { buffer[1], buffer[2] }));
              

                bool uid_find = false;
                for (int i = 0; i < m_RecievedQueue.Count; i++)
                {
                    if ((int)m_RecievedQueue[i][0] == tcp_session.PacketsUid) // если пакеты с таким  uid уже есть
                    {
                        uid_find = true;
                        if (((List<TcpSession>)m_RecievedQueue[i][1]).Count + 1 == tcp_session.PacketsCount) // если все пакеты пришли
                        {
                            byte[] obj_array = new byte[tcp_session.AllPacketsLen];
                            for (int j = 0; j < ((List<TcpSession>)m_RecievedQueue[i][1]).Count; j++)
                            {
                                if(j>0)
                                Array.Copy(((List<TcpSession>)m_RecievedQueue[i][1])[j].Packet, 0, obj_array, j* ((List<TcpSession>)m_RecievedQueue[i][1])[j-1].Packet.Length, ((List<TcpSession>)m_RecievedQueue[i][1])[j].Packet.Length);
                                else
                                    Array.Copy(((List<TcpSession>)m_RecievedQueue[i][1])[j].Packet, 0, obj_array,0, ((List<TcpSession>)m_RecievedQueue[i][1])[j].Packet.Length);

                            }
                            Array.Copy(tcp_session.Packet, 0, obj_array, obj_array.Length - tcp_session.Packet.Length,tcp_session.Packet.Length);
                            object recieved_object = ObjectFromByteArray(obj_array, 0, obj_array.Length);
                            if (DataRecieved != null && recieved_object != null)
                                DataRecieved(recieved_object, this);

                            m_RecievedQueue.RemoveAt(i);
                            break;
                        }
                        else
                        {
                            ((List<TcpSession>)m_RecievedQueue[i][1]).Add(tcp_session);
                            tcp_session.IsPacketDelivered = true;
                            if (tcpClient == null) // значит сообщение пришло от сервера к клиенту
                            {

                                SendPacket(tcp_session);
                            }
                            else
                            {
                                SendPacket(tcp_session, tcpClient);
                            }
                            
                        }
                    }
                }
                if (m_RecievedQueue.Count == 0 && !tcp_session.IsPacketDelivered )
                {
                    if (!uid_find)
                    {
                        m_RecievedQueue.Add(new object[] { tcp_session.PacketsUid, new List<TcpSession>() });
                        ((List<TcpSession>)m_RecievedQueue[0][1]).Add(tcp_session);
                    }
                    tcp_session.IsPacketDelivered = true;
                    if (tcpClient == null) // значит сообщение пришло от сервера к клиенту
                    {

                        SendPacket(tcp_session);
                    }
                    else
                    {
                        SendPacket(tcp_session, tcpClient);
                    }
                }
                if (tcp_session.IsPacketDelivered && !uid_find) // если пришло поверждение отправки пакета
                {
                    for (int i = 0; i < m_SendQueue.Count; i++)
                    {
                        if (m_SendQueue[i].PacketsUid == tcp_session.PacketsUid && m_SendQueue[i].CurrentPacketNumber == tcp_session.CurrentPacketNumber)
                        {
                            m_SendQueue.RemoveAt(i);
                            for(int j=0;j<m_SendQueue.Count; j++)
                            {
                                if (tcpClient == null) // значит сообщение пришло от сервера к клиенту
                                {

                                    SendPacket(m_SendQueue[j]);
                                }
                                else
                                {
                                    SendPacket(m_SendQueue[j], tcpClient);
                                }
                                if (TcpSendProgress != null)
                                    TcpSendProgress(tcp_session.PacketsUid, tcp_session.PacketsCount, tcp_session.CurrentPacketNumber);
                                break;
                            }

                            break;
                        }
                    }
                }




            }

        }

        public void StrarServer(int port)
        {
            m_TcpLow.StartServer(port);
        }
        public void ConnectToServer(string ip,int port)
        {
            m_TcpLow.ConnectionLost += M_TcpLow_ConnectionLost;
            m_TcpLow.ConnectToServer(ip, port);
        }

        public void StopServer()
        {
            m_TcpLow.StopServer();
        }
        public void Disconnect()
        {
            m_TcpLow.DisconnectFromServer();
        }

        public void SendObject(object obj)
        {
            if (obj != null)
            {
                int uid = new Random((int)DateTime.Now.Ticks).Next();
                                         
                byte[] obj_arr = GetByteArrayOfObject(obj);
                int all_packet_len = obj_arr.Length;
                int packets_count = all_packet_len / (m_TcpLow.BufferSize - 500); // вычисляем кол-во целых пакетов
                if (all_packet_len % (m_TcpLow.BufferSize - 500) > 0) // если остаток от деления больше 0, значит добавляем еще один пакет
                    packets_count += 1;
               
                
                for(int i=0;i<packets_count;i++)
                {
                    TcpSession tcp_add = new TcpSession();
                    tcp_add.AllPacketsLen = all_packet_len;
                    tcp_add.CurrentPacketNumber = i;
                    tcp_add.IsPacketDelivered = false;
                    tcp_add.PacketsCount = packets_count;
                    tcp_add.PacketsUid = uid;
                    tcp_add.Packet = new byte[m_TcpLow.BufferSize - 500];
                    if (obj_arr.Length - i * tcp_add.Packet.Length < tcp_add.Packet.Length)
                    {
                        tcp_add.Packet = new byte[obj_arr.Length - i * tcp_add.Packet.Length];                    
                    }
                    
                    if(i>0)
                        Array.Copy(obj_arr,  i * m_SendQueue[i-1].Packet.Length, tcp_add.Packet, 0, tcp_add.Packet.Length);
                    else
                        Array.Copy(obj_arr, 0, tcp_add.Packet, 0, tcp_add.Packet.Length);
                    m_SendQueue.Add(tcp_add);

                  
                   
                   
                }
                if (m_SendQueue.Count >0)
                {
                    SendPacket(m_SendQueue[0]);

                }


            }
        }
        
        private void SendPacket(TcpSession packet)
        {
            Thread.Sleep(50);
            byte[] packet_arr = GetByteArrayOfObject(packet);
            byte[] buffer = new byte[packet_arr.Length+3];
           
            buffer[0] = 24;
            Array.Copy(TcpModuleMiddleLevel.GetPacketHeader(buffer.Length-3), 0, buffer, 1, 2);
            Array.Copy(packet_arr, 0, buffer, 3, packet_arr.Length);
            m_TcpLow.SendData(buffer);

        }
        private void SendPacket(TcpSession packet,TcpModuleLowLevel tcp)
        {
            byte[] packet_arr = GetByteArrayOfObject(packet);
            byte[] buffer = new byte[packet_arr.Length + 3];

            buffer[0] = 24;
            Array.Copy(TcpModuleMiddleLevel.GetPacketHeader(buffer.Length - 3), 0, buffer, 1, 2);
            Array.Copy(packet_arr, 0, buffer, 3, packet_arr.Length);
            tcp.SendData(buffer);

        }
        public  byte[] GetByteArrayOfObject(object obj)
        {
            BinaryFormatter _bf = new BinaryFormatter();
            //_bf.TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.XsdString;
            MemoryStream _ms = new MemoryStream();
            _bf.Serialize(_ms, obj);
            byte[] res = new byte[_ms.Length];
            _ms.Position = 0;
            _ms.Read(res, 0, res.Length);
            _ms.Close();

           // object test = ObjectFromByteArray(res,0,res.Length);
            return res;
        }
        public object ObjectFromByteArray(byte[] array,int header_offset,int len)
        {
            BinaryFormatter _bf = new BinaryFormatter();
           // _bf.TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.XsdString;
            MemoryStream _ms = new MemoryStream(len);
            _ms.Write(array, header_offset, len);
            _ms.Position = 0;
            object res = _bf.Deserialize(_ms);
           
            _ms.Close();
            return res;
        }
        private void M_TcpLow_ConnectionLost(object sender, EventArgs e)
        {
            if (ConnectionLost != null)
                ConnectionLost(null, null);
        }
        public static int CalculatePacketLen(byte[] header)
        {
            
            int res = 0;
            res = (res | header[0]) << 8;
            res = (res | header[1]);
          

            return res;

        }
        public static byte[] GetPacketHeader(int len)
        {
            return new byte[] {  (byte)((len & 65280) >> 8), (byte)(len & 255) };

        }

    }
    [Serializable]
    public class TcpSession
    {
        private int m_PacketsUid;
        private int m_PacketsCount;
        private int m_CurrentPacketNumber;
        private int m_AllPacketsLen;
        private byte[] m_Packet;
        private bool m_IsPacketDelivered;

        public int PacketsUid
        {
            get
            {
                return m_PacketsUid;
            }

            set
            {
                m_PacketsUid = value;
            }
        }

        public int PacketsCount
        {
            get
            {
                return m_PacketsCount;
            }

            set
            {
                m_PacketsCount = value;
            }
        }

        public int CurrentPacketNumber
        {
            get
            {
                return m_CurrentPacketNumber;
            }

            set
            {
                m_CurrentPacketNumber = value;
            }
        }

        public int AllPacketsLen
        {
            get
            {
                return m_AllPacketsLen;
            }

            set
            {
                m_AllPacketsLen = value;
            }
        }

        public byte[] Packet
        {
            get
            {
                return m_Packet;
            }

            set
            {
                m_Packet = value;
            }
        }

        public bool IsPacketDelivered
        {
            get
            {
                return m_IsPacketDelivered;
            }

            set
            {
                m_IsPacketDelivered = value;
            }
        }
    }

  
}
