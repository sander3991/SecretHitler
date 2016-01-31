using Newtonsoft.Json;
using SecretHitler.Networking;
using SecretHitler.Objects;
using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Logic
{
    public class GameState
    {
        public bool Started { get; private set; }
        public event Action<GameState> OnStart;
        private GamePanel panel;
        public Chat Chat { get; }
        public Client Client { get; }
        public Server Server { get; }
        public int PlayerCount {
            get
            {
                int i = 0;
                for (i = 0; i < SeatedPlayers.Length; i++)
                    if (SeatedPlayers[i] == null)
                        break;
                return i;
            }
        }
        public Player[] SeatedPlayers { get; set; }
        public Player Me { get; private set; } //Client Side Only
        public GameState() { }
        public GameState(GamePanel panel, Chat chat, Client client, Server server, bool isServerSide = false)
        {
            Client = client;
            Chat = chat;
            Server = server;
            this.panel = panel;
            SeatedPlayers = new Player[10];
            if(!isServerSide)
                client.OnConnected += BindGetGamestate;
        }

        private void BindGetGamestate(Client obj)
        {
            obj.ReceiveHandler.OnReceive += OnMessageReceived;
            obj.RequestGameState();
        }

        private void OnMessageReceived(NetworkObject obj)
        {
            switch (obj.Command)
            {
                case ServerCommands.SendGameState:
                    var gameStateObj = obj as NetworkGameStateObject;
                    var gameState = gameStateObj.GameState;
                    for (var i = 0; i < gameState.SeatedPlayers.Length; i++)
                        if(gameState.SeatedPlayers[i] != null)
                        {
                            SeatedPlayers[i] = gameState.SeatedPlayers[i];
                            if (gameState.SeatedPlayers[i].Name == Client.Name)
                                Me = gameState.SeatedPlayers[i];
                        }
                    break;
                case ServerCommands.PlayerConnected:
                    var playerObj = obj as NetworkNewPlayerObject;
                    SeatedPlayers[playerObj.SeatPos] = playerObj.Player;
                    break;
                case ServerCommands.AnnounceCard:
                    foreach (Player player in SeatedPlayers)
                        player.Hand = null;
                    Me.Hand = new PlayerHand((obj as NetworkCardObject).Cards);
                    SetUnknownCards();
                    Started = true;
                    OnStart?.Invoke(this);
                    break;
            }
        }

        private void SetUnknownCards()
        {
            foreach (var player in SeatedPlayers)
                if (player != null && player.Hand == null)
                    player.Hand = new PlayerHand(new CardSecretRoleUnknown { Flipped = true }, new CardMembershipUnknown { Flipped = true }, flipped: true);
        }

        public int SeatPlayer(Player player)
        {
            for(var i = 0; i < SeatedPlayers.Length; i++)
                if(SeatedPlayers[i] == null)
                {
                    SeatedPlayers[i] = player;
                    return i;
                }
            return -1;
        }

        public int GetPlayerPos(string user)
        {
            for(var i = 0; i < SeatedPlayers.Length; i++)
                if(SeatedPlayers[i]?.Name == user)
                    return i;
            return -1;
        }
    }
}
