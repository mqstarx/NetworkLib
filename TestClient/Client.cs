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
        TcpModuleClient tcp;
       
        public Client()
        {
            InitializeComponent();
            tcp = new TcpModuleClient();
            tcp.Error += Tcp_Error;
            tcp.Connected += Tcp_Connected;
            tcp.Recieved += Tcp_Recieved;
        }

        private void Tcp_Recieved(object obj, SocketData data)
        {
            this.Invoke((new Action(() => MessageBox.Show("OnClient DataRecieved" + obj.ToString()))));
        }

        private void Tcp_Connected(object sender, EventArgs e)
        {
            this.Invoke((new Action(() => MessageBox.Show("OnClient" + sender.ToString()))));
        }

        private void Tcp_Error(object sender, EventArgs e)
        {
            this.Invoke((new Action(() => MessageBox.Show("OnClient" + sender.ToString()))));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tcp.Connect("192.168.100.13",5454);

        }

        private void button3_Click(object sender, EventArgs e)
        {
           // tcp.Stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tcp.Send(new TestObject());
        }
    }
}
