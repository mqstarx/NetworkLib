using NetworkLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
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
            tcp.DataRecieved += Tcp_DataRecieved;
            tcp.Error += Tcp_Error;
            tcp.ConnectToServer("192.168.100.13", 5454);
            
        }

        private void Tcp_Error(object sender, EventArgs e)
        {
            this.Invoke((new Action(() => MessageBox.Show(sender.ToString()))));
        }

        private void Tcp_DataRecieved(object sender, EventArgs e)
        {
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[10];
           
            for(int i=0;i<buffer.Length;i++)
            {
                buffer[i] = 3;
            }
            tcp.SendData(buffer);
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
