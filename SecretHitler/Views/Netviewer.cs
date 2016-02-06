using SecretHitler.Logic;
using SecretHitler.Networking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretHitler.Views
{
    public partial class Netviewer : Form
    {
        private Server server;
        private Client client;
        public Netviewer()
        {
            InitializeComponent();
        }
        public Netviewer(Client client)
            : this(null, client)
        { }
        public Netviewer(Server server)
            : this(server, null)
        { }
        public Netviewer(Server server, Client client)
            :this()
        {
            this.server = server;
            this.client = client;
            if(client != null && client.ReceiveHandler != null)
            {
                client.ReceiveHandler.OnReceive += ClientReceive;
                client.OnSent += ClientSent;
            }
            if (server != null)
            {
                server.ServerMessageHandler.OnReceive += ServerReceive;
                server.OnSent += ServerSent;
            }
        }

        private void AddMessage(string from, string to, NetworkObject obj)
        {
            if (dataGridView.InvokeRequired)
                dataGridView.Invoke(new Action<string, string, NetworkObject>(AddMessage), from, to, obj);
            else
            {
                dataGridView.Rows.Add(obj.ID, obj.Command, obj.GetType().Name, from, to, Stringify(obj));
            }
        }

        private void ClientReceive(NetworkObject obj)
        {
            AddMessage("Server", "Client", obj);
        }

        private void ClientSent(NetworkObject obj)
        {
            AddMessage("Client", "Server", obj);
        }

        private void ServerReceive(Player player, NetworkObject obj)
        {
            AddMessage(player.Name, "Server", obj);
        }
        private void ServerSent(Player player, NetworkObject obj)
        {
            AddMessage("Server", player.Name, obj);
        }
        private string Stringify(NetworkObject obj)
        {
            return obj.ToString();
        }

        private void Netviewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server != null)
            {
                server.ServerMessageHandler.OnReceive -= ServerReceive;
                server.OnSent -= ServerSent;
            }
            if(client != null)
            {
                client.OnSent -= ClientSent;
                client.ReceiveHandler.OnReceive -= ClientReceive;
            }
            
        }
    }
}
