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
using SecretHitler.Logic;
using SecretHitler.Objects;

namespace SecretHitler.Networking
{
    public class Client
    {
        private TcpClient client;
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
        public event Action<NetworkObject> OnSent;

        public event Action<Client> OnConnected;
        private Client(Game game, string username = null)
        {
            this.game = game;
            this.username = username;
        }

        public void Connect(IPAddress address)
        {
            ipEndPoint = new IPEndPoint(address, SecretHitlerGame.DEFAULTPORT);
            IPEndPoint clientPoint = new IPEndPoint(IPAddress.Any, 0);
            client = new TcpClient(clientPoint);
            ConfigureTcpClient(client);
            ReceiveHandler = new ReceiveMsgHandler(this);
            client.Connect(ipEndPoint);
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
        private void ConfigureTcpClient(TcpClient client)
        {
            client.SendBufferSize = 8192;
            client.ReceiveBufferSize = 8192;
        }

        private void CloseConnections(object sender, FormClosingEventArgs e)
        {
            client.Close();
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
        public void CastVote(bool yes)
        {
            new SendMsgHandler(new NetworkBoolObject(ServerCommands.CastVote, yes), this);
        }
        public void SendMessage(string str, Action<NetworkObject> callback = null)
        {
            if (string.IsNullOrEmpty(str)) return;
            new SendMsgHandler(new NetworkMessageObject(Name, str), this, callback);
        }

        public void ChooseChancellor(Player player)
        {
            var playerObj = new NetworkPlayerObject(ServerCommands.AnnounceChancellor, player);
            new SendMsgHandler(playerObj, this);
        }

        internal void ReturnPolicyCard(byte picked, bool isPresident)
        {
            var byteObj = new NetworkByteObject(isPresident ? ServerCommands.PresidentPolicyCardPicked : ServerCommands.ChancellorPolicyCardPicked, picked);
            new SendMsgHandler(byteObj, this);
        }

        internal void PickNextPresident(Player obj)
        {
            var playerObj = new NetworkPlayerObject(ServerCommands.PresidentActionChoosePresidentResponse, obj);
            new SendMsgHandler(playerObj, this);
        }

        internal void ConfirmPolicyRead()
        {
            new SendMsgHandler(new NetworkObject(ServerCommands.PresidentActionExamineResponse), this);
        }
        internal void InvestigatePlayer(Player player)
        {
            new SendMsgHandler(new NetworkPlayerObject(ServerCommands.PresidentActionInvestigatePresidentResponse, player), this);
        }

        internal void KillPlayer(Player player)
        {
            new SendMsgHandler(new NetworkPlayerObject(ServerCommands.PresidentActionKillResponse, player), this);
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
                while (client.client.Connected)
                {
                    var receive = DecodeNetworkObjects.Receive(client.client, false);
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
            private bool received;
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
                int timeout = 0;
                while (!received)
                {
                    if(timeout >= 1)
                        Console.WriteLine($"Send message handler failed trying to send {obj.GetType().Name}");
                    obj.Send(client.client);
                    client.OnSent?.Invoke(obj);
                    Thread.Sleep(333);
                    if (++timeout == 10)
                        throw new TimeoutException("The message was not received!");
                }
            }
            private void ConfirmReceived(NetworkObject response)
            {
                if(response.ID == obj.ID)
                {
                    received = true;
                    client.ReceiveHandler.OnReceive -= ConfirmReceived;
                    callback?.Invoke(response);
                }
            }
        }
    }
}
