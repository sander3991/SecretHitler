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
        CastVote = 0x0F,
        PlayerVoted = 0x10,
        AnnounceVotes = 0x11,
    }
    public class Server
    {

        delegate void SetTextCallback(string txt);
        private Game game;
        internal ServerGameState GameState { get; private set; }
        public event Action<Player, NetworkObject> OnReceive;
        private TcpListener serverSocket;
        private PingSockets pinger;
        private Thread serverThread;
        private Dictionary<string, TcpClient> connectedClients = new Dictionary<string, TcpClient>();

        private Server(Game game, GamePanel panel, Client client)
        {
            this.game = game;
            GameState = new ServerGameState(this);
            for (var i = 1; i < 10; i++)
                GameState.SeatedPlayers[i] = Player.GetPlayerServerSide($"Temp {i}");

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
            var playerCount = GameState.PlayerCount;
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
            var fascists = new List<Player>();
            //Shuffle and hand out decks
            decks.Shuffle();
            var sendMsgs = new NetworkMultipleObject[10];
            for (var i = 0; i < playerCount; i++)
            {
                var player = GameState.SeatedPlayers[i];
                player.Hand = decks[i];
                var sendToPlayer = new NetworkCardObject(ServerCommands.AnnounceCard, decks[i].Membership, decks[i].Role, decks[i].Yes, decks[i].No);
                if (decks[i].Role.IsFascist)
                    fascists.Add(player);
                if (decks[i].Role.IsHitler)
                    hitler = player;
                sendMsgs[i] = new NetworkMultipleObject(sendToPlayer);
                //Announce decks to player
            }
            var rand = new Random(Environment.TickCount * 5);
            var president = GameState.SeatedPlayers[rand.Next(playerCount)];
            GameState.SetPresident(president);
            var presidentMsg = new NetworkPlayerObject(ServerCommands.AnnouncePresident, president);

            //Announce Fascists to other party members
            for (var i = 0; i < playerCount; i++)
            {
                var multipleObjects = sendMsgs[i];
                var player = GameState.SeatedPlayers[i];
                if (player.Hand.Membership.IsFascist && (playerCount <= 6 || !player.Hand.Role.IsHitler))
                {
                    foreach (Player announcePlayer in fascists)
                        if (player != announcePlayer && connectedClients.ContainsKey(player.Name))
                            multipleObjects.AddObject(new NetworkRevealRoleObject(announcePlayer));
                }
                multipleObjects.AddObject(presidentMsg);
                if (connectedClients.ContainsKey(player.Name))
                    multipleObjects.Send(connectedClients[player.Name]);
            }
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
                    var networkObject = DecodeNetworkObjects.Receive(tcpClient);
                    var newUser = Player.GetPlayerServerSide(networkObject.Message);
                    if (GameState.PlayerCount == 10)
                    {
                        new NetworkObject(ServerCommands.Full).Send(tcpClient);
                        tcpClient.Close();
                        continue;
                    }
                    int i = 2;
                    lock (connectedClients)
                    {
                        while (connectedClients.ContainsKey(newUser.Name))
                            newUser = Player.GetPlayerServerSide($"{networkObject.Message}_{i++}");
                        int seat = GameState.SeatPlayer(newUser);
                        var reply = new NetworkObject(ServerCommands.OK, newUser.Name, networkObject.ID);
                        reply.Send(tcpClient);
                        Broadcast(new NetworkNewPlayerObject(ServerCommands.PlayerConnected, newUser, seat));
                        connectedClients.Add(newUser.Name, tcpClient);
                    }
                    new SocketListener(this, tcpClient, newUser);
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

        private void Broadcast(NetworkObject obj, TcpClient ignore = null)
        {
            lock (connectedClients)
                foreach (var connected in connectedClients.Values)
                    if (connected.Connected && (ignore == null || connected != ignore))
                        obj.Send(connected);

        }

        public void Stop()
        {
            Running = false;
        }

        internal void HandleMessage(NetworkObject request, TcpClient sentBy, Player player)
        {
            var multObj = new NetworkMultipleObject(new NetworkObject(ServerCommands.OK, null, request.ID));
            switch (request.Command)
            {
                case ServerCommands.Message:
                    var msgResponse = request as NetworkMessageObject;
                    var response = new NetworkMessageObject(msgResponse.Username, msgResponse.Message, sendToServer: false);
                    multObj.AddObject(response);
                    Broadcast(response, ignore: sentBy);
                    break;
                case ServerCommands.GetGameState:
                    multObj.AddObject(new NetworkGameStateObject(ServerCommands.SendGameState, GameState));
                    break;
                case ServerCommands.AnnounceChancellor:
                    var playerResponse = request as NetworkPlayerObject;
                    GameState.ResetVotes();
                    if (GameState.President == player)
                    {
                        GameState.SetChancellor(playerResponse.Player);
                        multObj.AddObject(playerResponse);
                        Broadcast(playerResponse, ignore: sentBy);
                    }
                    break;
                case ServerCommands.CastVote:
                    var boolObj = request as NetworkBoolObject;
                    GameState.SetVote(player, boolObj.Value);
                    var playerVoted = new NetworkPlayerObject(ServerCommands.PlayerVoted, player);
                    multObj.AddObject(playerVoted);
                    if (GameState.EveryoneVoted())
                    {
                        //everyone voted
                        var allMultObj = new NetworkMultipleObject(playerVoted);
                        var voteResult = new NetworkVoteResultObject(ServerCommands.AnnounceVotes, GameState.GetVotes());
                        allMultObj.AddObject(voteResult);
                        multObj.AddObject(voteResult);
                        //add object with all votes in result
                        Broadcast(allMultObj, ignore: sentBy);
                    }
                    else
                    {
                        Broadcast(playerVoted, ignore: sentBy);
                    }
                    break;
            }
            multObj.Send(sentBy);
            OnReceive?.Invoke(player, request);
        }

        class SocketListener : BackgroundWorker
        {
            private TcpClient client;
            private Server server;
            public Player Player { get; }
            public SocketListener(Server server, TcpClient socket, Player player)
            {
                this.client = socket;
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
                        var request = DecodeNetworkObjects.Receive(client);
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
                List<string> disconnected = new List<string>();
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
                                server.Broadcast(new NetworkNewPlayerObject(ServerCommands.PlayerDisconnected, Player.GetPlayerServerSide(user), server.GameState.GetPlayerPos(user)));
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
