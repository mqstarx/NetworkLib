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
using SimpleTCP;

namespace TestServer
{
    public partial class Server : Form
    {
       
      

        SimpleTcpServer tcp_server;
        public Server()
        {
            InitializeComponent();
          

            tcp_server = new SimpleTcpServer();
            tcp_server.DataReceived += Tcp_server_DataReceived;
            tcp_server.ClientConnected += Tcp_server_ClientConnected;

        
        }

        private void Tcp_server_ClientConnected(object sender, TcpClient e)
        {
            
        }

        private void Tcp_server_DataReceived(object sender, SimpleTCP.Message e)
        {
            e.Reply(e.Data);
        }

      

       

        private void button1_Click(object sender, EventArgs e)
        {
            tcp_server.Start(5454);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tcp_server.Stop();
        }
    }
}
