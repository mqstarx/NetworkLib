using NetworkLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
{
    public partial class Client : Form
    {
        TcpModuleLowLevelS tcp;
       
        public Client()
        {
            InitializeComponent();
           
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
             tcp = new TcpModuleLowLevelS();
              tcp.DataRecieved += Tcp_DataRecieved2;
             // tcp.TcpSendProgress += Tcp_TcpSendProgress;
              //tcp.Error += Tcp_Error;

              tcp.ConnectToServer("192.168.100.13", 5454);
           
            
        }

        private void Tcp_DataRecieved2(byte[] buffer, TcpModuleLowLevelS tcpClient)
        {
           
        }

        private void Tcp_TcpSendProgress(int uid, int all_count, int cur)
        {
            this.Invoke((new Action(() => label1.Text=uid+"||  "+all_count+@" \ "+cur )));
        }

        private void Tcp_DataRecieved1(object obj, TcpModuleMiddleLevel tcp)
        {
           
        }

        private void Tcp_Error(object sender, EventArgs e)
        {
            this.Invoke((new Action(() => MessageBox.Show(sender.ToString()))));
        }

        private void Tcp_DataRecieved(byte[] buffer, TcpModuleLowLevel tcp)
        {
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //  TestObject obj = new TestObject();
            List<byte> arr = new List<byte>();
            for(int i=0;i<200000000;i++)
            {
                arr.Add(55);
            }
            tcp.SendDataFromClient(arr.ToArray());
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tcp.DisconnectFromServer();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }
    }
}
