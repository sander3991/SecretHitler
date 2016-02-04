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
    public enum ServerCommands : byte
    {
        None = 0x00,
        OK = 0x01,
        Connect = 0x02,
        Message = 0x03,
        ReceiveMessage = 0x04,
        PlayerConnected = 0x05,
        PlayerDisconnected = 0x06,
        Full = 0x07,
        SendGameState = 0x08,
        GetGameState = 0x09,
        AnnounceCard = 0x0A,
        RevealRole = 0x0B,
        Multiple = 0x0C,
        AnnouncePresident = 0x0D,
        AnnounceChancellor = 0x0E,
    }
    public class Server
    {

        delegate void SetTextCallback(string txt);
        private Game game;
        private GameState gameState;
        private Socket serverSocket;
        private PingSockets pinger;
        private Thread serverThread;
        private Dictionary<string, Socket> connectedSockets = new Dictionary<string, Socket>();

        private Server(Game game, GamePanel panel, Client client)
        {
            this.game = game;
            gameState = new GameState(panel, client, this, isServer: true);
            for (var i = 1; i < 5; i++)
                gameState.SeatedPlayers[i] = Player.GetPlayerServerSide($"Temp {i}");

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

        public void LaunchGame()
        {
            var playerCount = gameState.PlayerCount;
            var fascistCount = playerCount % 2 == 0 ? playerCount / 2 - 1 : playerCount / 2;
            var liberalCount = playerCount - fascistCount;

            //Generate Decks

            PlayerHand[] decks = new PlayerHand[playerCount];
            var j = 0;
            for (var i = 0; i < fascistCount; i++, j++)
                decks[j] = new PlayerHand(new CardSecretRoleFascist(i), new CardMembershipFascist());
            for (var i = 0; i < liberalCount; i++, j++)
                decks[j] = new PlayerHand(new CardSecretRoleLiberal(i), new CardMembershipLiberal());

            Player hitler = null;
            List<Player> fascists = new List<Player>();
            //Shuffle and hand out decks
            decks.Shuffle();
            NetworkMultipleObject[] sendMsgs = new NetworkMultipleObject[10];
            for (var i = 0; i < playerCount; i++)
            {
                var player = gameState.SeatedPlayers[i];
                player.Hand = decks[i];
                var sendToPlayer = new NetworkCardObject(ServerCommands.AnnounceCard, decks[i].Membership, decks[i].Role, decks[i].Yes, decks[i].No);
                if (decks[i].Role.IsFascist)
                    fascists.Add(player);
                if (decks[i].Role.IsHitler)
                    hitler = player;
                sendMsgs[i] = new NetworkMultipleObject(sendToPlayer);
                //Announce decks to player
            }
            Random rand = new Random(Environment.TickCount * 5);
            var president = gameState.SeatedPlayers[rand.Next(playerCount)];
            gameState.SetPresidentServer(president);
            var presidentMsg = new NetworkPlayerObject(ServerCommands.AnnouncePresident, president);

            //Announce Fascists to other party members
            for (var i = 0; i < playerCount; i++)
            {
                var multipleObjects = sendMsgs[i];
                var player = gameState.SeatedPlayers[i];
                if (player.Hand.Membership.IsFascist && (playerCount <= 6 || !player.Hand.Role.IsHitler))
                {
                    foreach (Player announcePlayer in fascists)
                        if (player != announcePlayer && connectedSockets.ContainsKey(player.Name))
                            multipleObjects.AddObject(new NetworkRevealRoleObject(announcePlayer));
                }
                multipleObjects.AddObject(presidentMsg);
                if (connectedSockets.ContainsKey(player.Name))
                    multipleObjects.Send(connectedSockets[player.Name]);
            }
        }

        public void Start()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, SecretHitlerGame.DEFAULTPORT);
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newsock.Bind(endPoint);
            serverSocket = newsock;
            serverThread = new Thread(new ThreadStart(Run));
            serverThread.Name = "Server Thread";
            serverThread.Start();
            pinger = new PingSockets(this);
            game.FormClosing += CloseConnections;
        }

        private void CloseConnections(object sender, FormClosingEventArgs e)
        {
            Stop();
            lock (connectedSockets)
                foreach (var connected in connectedSockets.Values)
                    connected.Close();
            serverSocket.Close();
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
                    serverSocket.Listen(10);
                    var socket = serverSocket.Accept();
                    ConfigureSocket(socket);
                    var networkObject = NetworkObjectDecoders.Receive(socket);
                    var newUser = Player.GetPlayerServerSide(networkObject.Message);
                    if (gameState.PlayerCount == 10)
                    {
                        new NetworkObject(ServerCommands.Full).Send(socket);
                        socket.Disconnect(true);
                        continue;
                    }
                    int i = 2;
                    lock (connectedSockets)
                    {
                        while (connectedSockets.ContainsKey(newUser.Name))
                            newUser = Player.GetPlayerServerSide($"{networkObject.Message}_{i++}");
                        int seat = gameState.SeatPlayer(newUser);
                        var reply = new NetworkObject(ServerCommands.OK, newUser.Name, networkObject.ID);
                        reply.Send(socket);
                        Broadcast(new NetworkNewPlayerObject(ServerCommands.PlayerConnected, newUser, seat));
                        connectedSockets.Add(newUser.Name, socket);
                    }
                    new SocketListener(this, socket, newUser);
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
        private void Broadcast(NetworkObject obj, Socket ignore = null)
        {
            lock (connectedSockets)
                foreach (var connected in connectedSockets.Values)
                    if (connected.Connected && (ignore == null || connected != ignore))
                        obj.Send(connected);

        }

        public void Stop()
        {
            Running = false;
        }

        class SocketListener : BackgroundWorker
        {
            private static NetworkObject defaultResponse = new NetworkObject(ServerCommands.OK, null);
            private Socket socket;
            private Server server;
            public Player Username { get; }
            public SocketListener(Server server, Socket socket, Player player)
            {
                this.socket = socket;
                this.server = server;
                Username = player;
                DoWork += OnReceived;
                RunWorkerAsync();
            }
            private void OnReceived(object sender, DoWorkEventArgs e)
            {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "Server Socket Listener";
                try
                {
                    while (socket.Connected)
                    {
                        var request = NetworkObjectDecoders.Receive(socket);
                        var multObj = new NetworkMultipleObject(new NetworkObject(ServerCommands.OK, null, request.ID));
                        switch (request.Command)
                        {
                            case ServerCommands.Message:
                                var msgResponse = request as NetworkMessageObject;
                                var response = new NetworkMessageObject(msgResponse.Username, msgResponse.Message, sendToServer: false);
                                multObj.AddObject(response);
                                server.Broadcast(response, socket);
                                break;
                            case ServerCommands.GetGameState:
                                multObj.AddObject(new NetworkGameStateObject(ServerCommands.SendGameState, server.gameState));
                                break;
                            case ServerCommands.AnnounceChancellor:
                                var playerResponse = request as NetworkPlayerObject;
                                if (server.gameState.President == Username)
                                {
                                    server.gameState.SetChancellorServer(playerResponse.Player);
                                    multObj.AddObject(playerResponse);
                                    server.Broadcast(playerResponse, socket);
                                }
                                break;
                        }
                        multObj.Send(socket);
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
                List<string> disconnected = new List<string>();
                try
                {
                    while (enabled)
                    {
                        lock (server.connectedSockets)
                            foreach (var user in server.connectedSockets.Keys)
                                if (!server.connectedSockets[user].IsConnected())
                                    disconnected.Add(user);
                        if (disconnected.Count > 0)
                        {
                            lock (server.connectedSockets)
                                foreach (var user in disconnected)
                                    server.connectedSockets.Remove(user);
                            foreach (var user in disconnected)
                                server.Broadcast(new NetworkNewPlayerObject(ServerCommands.PlayerDisconnected, Player.GetPlayerServerSide(user), server.gameState.GetPlayerPos(user)));
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
