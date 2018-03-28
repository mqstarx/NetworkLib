using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib
{
    public delegate void TcpMiddleDataRecieved(object obj, TcpModuleMiddleLevel tcp);
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

        public TcpModuleMiddleLevel()
        {
            m_TcpLow = new TcpModuleLowLevel();
           
            m_TcpLow.DataRecieved += M_TcpLow_DataRecieved;
            m_SendQueue = new List<TcpSession>();
            m_RecievedQueue = new List<object[]>();
        }

        private void M_TcpLow_DataRecieved(byte[] buffer, TcpModuleLowLevel tcpClient)
        {
           if(buffer[0]==24)
            {
                TcpSession tcp_session = (TcpSession)ObjectFromByteArray(buffer,3,CalculatePacketLen(new byte[] {buffer[1],buffer[2] }));

                bool uid_find = false;
                for(int i=0;i< m_RecievedQueue.Count;i++)
                {
                    if((int)m_RecievedQueue[i][0]==tcp_session.PacketsUid) // если пакеты с таким  uid уже есть
                    {
                        uid_find = true;
                        if( ((List<TcpSession>)m_RecievedQueue[i][1]).Count+1 == tcp_session.PacketsCount) // если все пакеты пришли
                        {
                            byte[] obj_array = new byte[tcp_session.AllPacketsLen];
                            for(int j=0; j<((List<TcpSession>)m_RecievedQueue[i][1]).Count;j++)
                            {
                                Array.Copy(((List<TcpSession>)m_RecievedQueue[i][1])[j].Packet, 0, obj_array, 0, ((List<TcpSession>)m_RecievedQueue[i][1])[j].Packet.Length);
                               

                            }
                            object recieved_object = ObjectFromByteArray(obj_array, 0, obj_array.Length);
                            if (DataRecieved != null && recieved_object!=null)
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
                                SendPacket(tcp_session,tcpClient);
                            }
                        }
                    }
                }
                if(tcp_session.IsPacketDelivered && !uid_find) // если пришло поверждение отправки пакета
                {
                    for(int i=0;i<m_SendQueue.Count;i++)
                    {
                        if(m_SendQueue[i].PacketsUid == tcp_session.PacketsUid && m_SendQueue[i].CurrentPacketNumber==tcp_session.CurrentPacketNumber)
                        {
                            m_SendQueue.RemoveAt(i);
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
                                         
                byte[] obj_arr = TcpModuleMiddleLevel.GetByteArrayOfObject(obj);
                int all_packet_len = obj_arr.Length;
                int packets_count = all_packet_len / (m_TcpLow.BufferSize - 3); // вычисляем кол-во целых пакетов
                if (all_packet_len % (m_TcpLow.BufferSize - 3) > 0) // если остаток от деления больше 0, значит добавляем еще один пакет
                    packets_count += 1;
               
                
                for(int i=0;i<packets_count;i++)
                {
                    TcpSession tcp_add = new TcpSession();
                    tcp_add.AllPacketsLen = all_packet_len;
                    tcp_add.CurrentPacketNumber = i;
                    tcp_add.IsPacketDelivered = false;
                    tcp_add.PacketsCount = packets_count;
                    tcp_add.PacketsUid = uid;
                    tcp_add.Packet = new byte[m_TcpLow.BufferSize - 3];
                    if (obj_arr.Length - i * tcp_add.Packet.Length < tcp_add.Packet.Length)
                    {
                        tcp_add.Packet = new byte[obj_arr.Length - i * tcp_add.Packet.Length];                    
                    }
                    
                        Array.Copy(obj_arr, i * tcp_add.Packet.Length, tcp_add.Packet, 0, tcp_add.Packet.Length);
                    m_SendQueue.Add(tcp_add);
                    if (m_SendQueue.Count == 1)
                    {
                        SendPacket(m_SendQueue[0]);

                    }
                }

            }
        }
        private void SendPacket(TcpSession packet)
        {
            byte[] buffer = new byte[m_TcpLow.BufferSize];
            buffer[0] = 24;
            Array.Copy(TcpModuleMiddleLevel.GetPacketHeader(packet.Packet.Length), 0, buffer, 1, 2);
            Array.Copy(TcpModuleMiddleLevel.GetByteArrayOfObject(packet), 0, buffer, 3, packet.Packet.Length);
            m_TcpLow.SendData(buffer);

        }
        private void SendPacket(TcpSession packet,TcpModuleLowLevel tcp)
        {
            byte[] buffer = new byte[m_TcpLow.BufferSize];
            buffer[0] = 24;
            Array.Copy(TcpModuleMiddleLevel.GetPacketHeader(packet.Packet.Length), 0, buffer, 1, 2);
            Array.Copy(TcpModuleMiddleLevel.GetByteArrayOfObject(packet), 0, buffer, 3, packet.Packet.Length);
            tcp.SendData(buffer);

        }
        public static byte[] GetByteArrayOfObject(object obj)
        {
            BinaryFormatter _bf = new BinaryFormatter();
            MemoryStream _ms = new MemoryStream();
            _bf.Serialize(_ms, obj);
            byte[] res =  _ms.ToArray();
            _ms.Close();
            return res;
        }
        public static object ObjectFromByteArray(byte[] array,int header_offset,int len)
        {
            BinaryFormatter _bf = new BinaryFormatter();
            MemoryStream _ms = new MemoryStream();
            _ms.Write(array, header_offset, len);
            object res = _bf.Deserialize(_ms);
           
            _ms.Close();
            return res;
        }
        private void M_TcpLow_ConnectionLost(object sender, EventArgs e)
        {
            
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
