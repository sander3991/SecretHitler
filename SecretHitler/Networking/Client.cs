using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretHitler.Networking
{
    public class Client
    {
        private Socket socket;
        private Game game;
        private bool connected;
        private IPEndPoint ipEndPoint;
        public string Name { get; set; }
        public static Client Instance { get; private set; }
        public ReceiveMsgHandler ReceiveHandler { get; private set; }
        public event Action<Client> OnConnected;
        private Client(Game game)
        {
            this.game = game;;
        }

        public void Connect(IPAddress address)
        {
            ipEndPoint = new IPEndPoint(address, SecretHitlerGame.DEFAULTPORT);
            IPEndPoint clientPoint = new IPEndPoint(IPAddress.Any, SecretHitlerGame.DEFAULTPORT + 1);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(clientPoint);
            ConfigureSocket(socket);
            ReceiveHandler = new ReceiveMsgHandler(this);
            socket.Connect(ipEndPoint);
            ReceiveHandler.OnReceive += ConfirmConnected;
            ReceiveHandler.RunWorkerAsync();
            new SendMsgHandler(new NetworkObject(ServerCommands.Connect) { Message = Name }, this);
            int timeout = 0;
            while (!connected && timeout++ < 10)
                Thread.Sleep(100);
            if (!connected)
                throw new HttpListenerException();
            game.Chat.AppendStatusMessage($"Connected to server: {ipEndPoint}");
            OnConnected?.Invoke(this);
            game.FormClosing += CloseConnections;
        }

        internal void RequestGameState()
            => new SendMsgHandler(new NetworkObject(ServerCommands.GameState), this);

        private void ConfigureSocket(Socket socket)
        {
            socket.SendBufferSize = 8192;
            socket.ReceiveBufferSize = 8192;
        }

        private void CloseConnections(object sender, FormClosingEventArgs e)
        {
            socket.Close();
        }

        private void ConfirmConnected(NetworkObject obj)
        {
            connected = true;
            ReceiveHandler.OnReceive -= ConfirmConnected;
        }

        public static Client GetClient(Game game)
        {
            Instance = new Client(game);
            return Instance;
        }

        public void SendMessage(string str, Action<NetworkObject> callback = null)
            => new SendMsgHandler(new NetworkMessageObject(Name, str), this, callback);

        public class ReceiveMsgHandler : BackgroundWorker
        {
            private Client client;
            public event Action<NetworkObject> OnReceive;
            public ReceiveMsgHandler(Client client)
            {
                this.client = client;
                DoWork += ReceiveFromServer;
            }
            private void ReceiveFromServer(object obj, DoWorkEventArgs e)
            {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "Receive Message Handler";
                while (client.socket.Connected)
                {
                    var receive = NetworkObject.Receive(client.socket);
                    client.connected = true;
                    OnReceive?.Invoke(receive);
                }
            }
        }

        class SendMsgHandler : BackgroundWorker
        {
            private NetworkObject obj;
            private Client client;
            private Action<NetworkObject> callback;
            public SendMsgHandler(NetworkObject obj, Client client, Action<NetworkObject> callback = null)
            {
                this.obj = obj;
                this.client = client;
                this.callback = callback;
                DoWork += StartSendMessage;
                RunWorkerAsync();
            }
            private void StartSendMessage(object sender, DoWorkEventArgs e)
            {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "Send Message Handler";
                client.ReceiveHandler.OnReceive += ConfirmReceived;
                obj.Send(client.socket);
            }
            private void ConfirmReceived(NetworkObject response)
            {
                if(response.ID == obj.ID)
                {
                    client.ReceiveHandler.OnReceive -= ConfirmReceived;
                    callback?.Invoke(response);
                }
            }

        }
    }
}
