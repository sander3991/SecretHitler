using Newtonsoft.Json;
using SecretHitler.Networking;
using SecretHitler.Objects;
using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Logic
{
    public class GameState
    {
        public bool Started { get; private set; }
        public event Action<GameState> OnStart;
        private GamePanel panel;
        public Client Client { get; }
        public Server Server { get; }
        public ChatHandler Chat { get; }
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
        public Player President { get; private set; }
        public Player Chancellor { get; private set; }
        public Player PreviousPresident { get; private set; }
        public Player PreviousChancellor { get; private set; }
        public GameState() { }
        public GameState(GamePanel panel, Client client, Server server, bool isServerSide = false)
        {
            Client = client;
            Chat = ChatHandler.Initialize(this);
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
                    var newPlayerObj = obj as NetworkNewPlayerObject;
                    SeatedPlayers[newPlayerObj.SeatPos] = newPlayerObj.Player;
                    break;
                case ServerCommands.AnnounceCard:
                    SetPresident(null);
                    PreviousChancellor = null;
                    PreviousPresident = null;
                    SetPlacardValue(PlayArea.PlacardPrevChancellor, null, false);
                    SetPlacardValue(PlayArea.PlacardPrevPresident, null, false);
                    foreach (Player player in SeatedPlayers)
                        if (player != null) player?.Hand.SetUnkown();
                    Me.Hand = new PlayerHand((obj as NetworkCardObject).Cards);
                    SetUnknownCards();
                    Started = true;
                    OnStart?.Invoke(this);
                    break;
                case ServerCommands.RevealRole:
                    var revealObj = obj as NetworkRevealRoleObject;
                    revealObj.Player.Hand.Role = revealObj.SecretRole;
                    revealObj.Player.Hand.Membership = revealObj.SecretRole.IsFascist ? (CardMembership)new CardMembershipFascist() : new CardMembershipLiberal();
                    break;
                case ServerCommands.AnnouncePresident:
                    SetPresident((obj as NetworkPlayerObject).Player);
                    break;
                case ServerCommands.AnnounceChancellor:
                    SetChancellor((obj as NetworkPlayerObject).Player);
                    break;
            }
        }

        private void SetChancellor(Player player)
        {
            if (Chancellor != null)
            {
                PreviousChancellor = Chancellor;
                SetPlacardValue(PlayArea.PlacardPrevChancellor, PreviousChancellor, false);
            }
            Chancellor = player;
            SetPlacardValue(PlayArea.PlacardChancellor, Chancellor, true);
        }
        private void SetPresident(Player player)
        {
            if (President != null)
            {
                PreviousPresident = President;
                SetPlacardValue(PlayArea.PlacardPrevPresident, PreviousPresident, false);
            }
            President = player;
            SetPlacardValue(PlayArea.PlacardPresident, President, true);
            SetChancellor(null);
        }
        private void SetPlacardValue(Placard placard, Player value, bool current)
        {
            if (value == null)
            {
                placard.Location = placard.DefaultLoc;
                placard.RotateType = BitmapRotateType.None;
            }
            else
            {
                int seatPos = Array.IndexOf(SeatedPlayers, value);
                var playArea = panel.PlayerAreas[seatPos];
                placard.Location = current ? playArea.CurrentPlacardLoc : playArea.PreviousPlacardLoc;
                placard.RotateType = playArea.RotateType;
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
