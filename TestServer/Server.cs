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



        TcpModuleServer tcpserver;
        public Server()
        {
            InitializeComponent();
            tcpserver = new TcpModuleServer();
            tcpserver.Error += Tcpserver_Error;
            tcpserver.Recieved += Tcpserver_Recieved;
          

        
        }

        private void Tcpserver_Recieved(object obj, SocketData data)
        {
           
        }

        private void Tcpserver_Error(object sender, EventArgs e)
        {
            Console.WriteLine(sender.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tcpserver.StartServer(5454);
        }

        
    }
}
