using Newtonsoft.Json;
using SecretHitler.Logic;
using SecretHitler.Objects;
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
    public class Server
    {
        delegate void SetTextCallback(string txt);
        private Game game;
        internal ServerGameState GameState { get; private set; }
        public event Action<Player, NetworkObject> OnSent;
        private TcpListener serverSocket;
        private PingSockets pinger;
        private Thread serverThread;
        private Dictionary<Player, TcpClient> connectedClients = new Dictionary<Player, TcpClient>();
        public ServerMessageHandler ServerMessageHandler { get; }

        private Server(Game game, GamePanel panel, Client client)
        {
            this.game = game;
            GameState = new ServerGameState(this);
            for (var i = 1; i < 5; i++)
                GameState.SeatedPlayers[i] = Player.GetPlayerServerSide($"Temp {i}");
            ServerMessageHandler = new ServerMessageHandler(this, GameState);
        }
        public static Server Instance { get; private set; }
        public bool Running { get; private set; }

        internal static Server GetInstance(Game game, GamePanel panel, Client client)
        {
            if (game == null)
                throw new ArgumentException("Game may not be null!", nameof(game));
            if (Instance == null)
                Instance = new Server(game, panel, client);
            return Instance;
        }

        private void SendMessage(Player player, NetworkObject obj)
        {
            obj.Send(GetPlayerClient(player));
            OnSent?.Invoke(player, obj);
        }

        public void Start()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, SecretHitlerGame.DEFAULTPORT);
            serverSocket = new TcpListener(endPoint);
            serverSocket.Start();
            serverThread = new Thread(new ThreadStart(Run));
            serverThread.Name = "Server Thread";
            serverThread.Start();
            pinger = new PingSockets(this);
            game.FormClosing += CloseConnections;
        }

        private void CloseConnections(object sender, FormClosingEventArgs e)
        {
            Stop();
            lock (connectedClients)
                foreach (var connected in connectedClients.Values)
                    connected.Close();
            serverSocket.Stop();
            pinger.Stop();
        }

        private void Run()
        {
            if (Running) throw new ServerRunningException();
            Running = true;
            while (Running)
            {
                try
                {
                    var tcpClient = serverSocket.AcceptTcpClient();
                    ConfigureTcpClient(tcpClient);
                    var networkObject = DecodeNetworkObjects.Receive(tcpClient, true);
                    var newUser = Player.GetPlayerServerSide(networkObject.Message);
                    if (GameState.PlayerCount == 10)
                    {
                        var fullmsg = new NetworkObject(ServerCommands.Full);
                        fullmsg.Send(tcpClient);
                        OnSent?.Invoke(newUser, fullmsg);
                        tcpClient.Close();
                        continue;
                    }
                    else if (GameState.PlayingGame)
                    {
                        var msg = new NetworkObject(ServerCommands.CurrentlyPlaying);
                        msg.Send(tcpClient);
                        OnSent?.Invoke(newUser, msg);
                        tcpClient.Close();
                        continue;
                    }
                    int i = 2;
                    lock (connectedClients)
                    {
                        while (connectedClients.ContainsKey(newUser))
                            newUser = Player.GetPlayerServerSide($"{networkObject.Message}_{i++}");
                        int seat = GameState.SeatPlayer(newUser);
                        var reply = new NetworkObject(ServerCommands.OK, newUser.Name, networkObject.ID);
                        reply.Send(tcpClient);
                        OnSent?.Invoke(newUser, reply);
                        Broadcast(new NetworkNewPlayerObject(ServerCommands.PlayerConnected, newUser, seat));
                        connectedClients.Add(newUser, tcpClient);
                    }
                    new SocketListener(ServerMessageHandler, tcpClient, newUser);
                }
                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                {
                    if (ex is SocketException && ((SocketException)ex).ErrorCode != 10004)
                        throw ex;
                }
            }
        }

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

        private void Broadcast(NetworkObject obj, TcpClient ignore = null, Player ignorePlayer = null)
        {
            lock (connectedClients)
                foreach (var player in connectedClients.Keys)
                {
                    var connected = connectedClients[player];
                    if (connected.Connected && (ignorePlayer == null || ignorePlayer != player) && (ignore == null || connected != ignore))
                        SendMessage(player, obj);
                }
        }

        internal void SendResponse(ServerResponse response)
        {
            foreach (var player in connectedClients.Keys)
                response.GetResponse(player)?.Send(connectedClients[player]);
        }

        public void Stop()
        {
            Running = false;
        }

        private TcpClient GetPlayerClient(Player player)
        {
            if(connectedClients.ContainsKey(player))
                return connectedClients[player];
            return null;
        }

        class SocketListener : BackgroundWorker
        {
            private TcpClient client;
            private ServerMessageHandler server;
            public Player Player { get; }
            public SocketListener(ServerMessageHandler server, TcpClient client, Player player)
            {
                this.client = client;
                this.server = server;
                Player = player;
                DoWork += OnReceived;
                RunWorkerAsync();
            }
            private void OnReceived(object sender, DoWorkEventArgs e)
            {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "Server Socket Listener";
                try
                {
                    while (client.Connected)
                    {
                        var request = DecodeNetworkObjects.Receive(client, true);
                        server.HandleMessage(request, client, Player);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        class PingSockets : BackgroundWorker
        {
            private bool enabled;
            private Server server;
            public PingSockets(Server server)
            {
                this.server = server;
                DoWork += Ping;
                enabled = true;
                RunWorkerAsync();
            }

            private void Ping(object sender, DoWorkEventArgs e)
            {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "Ping Thread";
                List<Player> disconnected = new List<Player>();
                try
                {
                    while (enabled)
                    {
                        lock (server.connectedClients)
                            foreach (var user in server.connectedClients.Keys)
                                if (!server.connectedClients[user].Connected)
                                    disconnected.Add(user);
                        if (disconnected.Count > 0)
                        {
                            lock (server.connectedClients)
                                foreach (var user in disconnected)
                                    server.connectedClients.Remove(user);
                            foreach (var user in disconnected)
                                server.Broadcast(new NetworkNewPlayerObject(ServerCommands.PlayerDisconnected, user, server.GameState.GetPlayerPos(user.Name)));
                            disconnected.Clear();
                        }
                        Thread.Sleep(250);
                    }
                }
                catch (Exception)
                {

                }
            }
            public void Stop() => enabled = false;
        }
    }

    public class ServerRunningException : Exception
    {
        public ServerRunningException() : base("A server is allready running!") { }
    }
}
