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
       
        TcpModuleServer tcpserv;
        public Server()
        {
            InitializeComponent();
            tcpserv = new TcpModuleServer();
            tcpserv.Error += Tcpserv_Error;
            tcpserv.Recieved += Tcpserv_Recieved;
        
        }

        private void Tcpserv_Recieved(object obj, SocketData data)
        {
            tcpserv.Send(data.Socket, obj);
        }

        private void Tcpserv_Error(object sender, EventArgs e)
        {
            this.Invoke((new Action(() => MessageBox.Show("OnServer"+sender.ToString()))));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tcpserv.StartServer(5454);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tcpserv.StopServer();
        }
    }
}
