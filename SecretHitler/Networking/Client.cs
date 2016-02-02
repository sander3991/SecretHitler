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
        private string username;
        public string Name
        {
            get { return username; }
            set
            {
                if(username != value)
                {
                    username = value;
                    OnUsernameChanged?.Invoke(value);
                }
            }
        }
        public event Action<string> OnUsernameChanged;
        public static Client Instance { get; private set; }
        public ReceiveMsgHandler ReceiveHandler { get; private set; }
        public event Action<Client> OnConnected;
        private Client(Game game, string username = null)
        {
            this.game = game;
            this.username = username;
        }

        public void Connect(IPAddress address)
        {
            ipEndPoint = new IPEndPoint(address, SecretHitlerGame.DEFAULTPORT);
            IPEndPoint clientPoint = new IPEndPoint(IPAddress.Any, SecretHitlerGame.DEFAULTPORT + 1);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Bind(clientPoint);
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
            OnConnected?.Invoke(this);
            game.FormClosing += CloseConnections;
        }

        internal void RequestGameState()
            => new SendMsgHandler(new NetworkObject(ServerCommands.GetGameState), this);
        

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
            if(obj.Command == ServerCommands.OK)
            {
                Name = obj.Message;
                ReceiveHandler.OnReceive -= ConfirmConnected;
            }
        }

        public static Client GetClient(Game game, string username)
        {
            Instance = new Client(game, username);
            return Instance;
        }

        public void SendMessage(string str, Action<NetworkObject> callback = null)
        {
            if (string.IsNullOrEmpty(str)) return;
            new SendMsgHandler(new NetworkMessageObject(Name, str), this, callback);
        }

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
                    var receive = NetworkObjectDecoders.Receive(client.socket);
                    client.connected = true;
                    try
                    {
                        if (receive is NetworkMultipleObject)
                            foreach (var msg in (receive as NetworkMultipleObject).Objects)
                                OnReceive?.Invoke(msg);
                        else
                            OnReceive?.Invoke(receive);
                    }
                    catch (Exception)
                    {

                    }
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
