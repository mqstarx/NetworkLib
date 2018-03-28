using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetworkLib;
using System.Net.Sockets;

namespace TestServer
{
    public partial class Form1 : Form
    {
        TcpModuleLowLevel tcp;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tcp = new TcpModuleLowLevel();
          
            tcp.DataRecieved += Tcp_DataRecievedToServer;
            tcp.Error += Tcp_Error;
            tcp.StartServer(5454);
        }

        private void Tcp_Error(object sender, EventArgs e)
        {
            //this.Invoke((new Action(() => MessageBox.Show(sender.ToString()))));
        }

        private void Tcp_DataRecievedToServer(byte[] buffer,TcpModuleLowLevel tcp)
        {
           

            tcp.SendData(new byte[] { 1, 2, 3, 4, 5 });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {if(tcp!=null)
            tcp.StopServer();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tcp.StopServer();
        }
    }
}
