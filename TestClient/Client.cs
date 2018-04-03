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
using SimpleTCP;
namespace TestClient
{
    public partial class Client : Form
    {

        SimpleTcpClient tcpclient;
        public Client()
        {
            InitializeComponent();
            tcpclient = new SimpleTcpClient();
            tcpclient.DataReceived += Tcpclient_DataReceived;
            
        }

        private void Tcpclient_DataReceived(object sender, SimpleTCP.Message e)
        {
            //throw new NotImplementedException();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tcpclient.Connect("192.168.100.13", 5454);
           // tcp.Connect("192.168.100.13",5454);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            tcpclient.Disconnect();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[10000];
            for (int i = 0; i < data.Length; i++)
                data[i] = 0x34;
            tcpclient.Write(data);
        }
    }
}
