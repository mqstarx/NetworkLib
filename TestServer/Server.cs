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
    public partial class Server : Form
    {
        TcpModule tcp;
       
        public Server()
        {
            InitializeComponent();
        
        }


        private void button1_Click(object sender, EventArgs e)
        {
               tcp = new  TcpModule();

            tcp.DataRecievedObject += Tcp_DataRecievedObject;
             //  tcp.Error += Tcp_Error;
               tcp.StartServer(5454);

           
        }

        private void Tcp_DataRecievedObject(object obj, TcpModule tcpClient)
        {
            
        }

        private void Tcp_DataRecieved1(byte[] buffer, TcpModule tcpClient)
        {
            
        }

      
        private void Tcp_Error(object sender, EventArgs e)
        {
            //this.Invoke((new Action(() => MessageBox.Show(sender.ToString()))));
        }

      

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {if(tcp!=null)
            tcp.StopServer();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (tcp != null)
                tcp.StopServer();
        }
    }
}
