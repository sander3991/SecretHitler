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
        public LinkedList<GameObject> GameObjects { get { return panel.Objects; } }
        public PlayArea[] PlayAreas { get { return panel.PlayerAreas; } }
        public Game Window { get; private set; }
        private bool isServer;
        public GameState() { }
        public GameState(GamePanel panel, Client client, Server server, Game game = null, bool isServer = false)
        {
            Client = client;
            if(!isServer)
                Chat = ChatHandler.Initialize(this);
            Server = server;
            Window = game;
            this.panel = panel;
            SeatedPlayers = new Player[10];
            if(!isServer)
                client.OnConnected += BindGetGamestate;
            this.isServer = isServer;
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
                    if (President == Me)
                    {
                        Window.SetPlayerMessage("Please pick your chancellor.");
                        PickPlayerAction(Client.ChooseChancellor, CheckIfPlayerIsntPreviousGovernment, (Player player) => Window.SetPlayerMessage($"You can't pick {player.Name} as your chancellor. Pick someone else!"));
                    }
                    else
                        Window.SetPlayerMessage($"Wait for {President.Name} to pick his/her chancellor.");
                    break;
                case ServerCommands.AnnounceChancellor:
                    SetChancellor((obj as NetworkPlayerObject).Player);
                    Window.SetPlayerMessage($"Please vote for {President.Name} as President and {Chancellor.Name} as Chancellor");
                    break;
            }
        }

        private bool CheckIfPlayerIsntPreviousGovernment(Player player) => player != PreviousChancellor && player != PreviousPresident;

        private void PickPlayerAction(Action<Player> callback, Func<Player, bool> success = null, Action<Player> onFail = null, bool allowPickSelf = false)
        {
            int playerCount = PlayerCount;
            Action<PlayArea> internalAction = null;
            internalAction = (PlayArea area) =>
            {
                var player = SeatedPlayers[area.ID];
                if (success == null || success(player))
                {
                    callback(SeatedPlayers[area.ID]);
                    for (var i = 0; i < PlayerCount; i++)
                        if(allowPickSelf || SeatedPlayers[i] != Me)
                            PlayAreas[i].OnClick -= internalAction;
                }
                else
                    onFail?.Invoke(player);
            };
            for (var i = 0; i < PlayerCount; i++)
                if (allowPickSelf || SeatedPlayers[i] != Me)
                    PlayAreas[i].OnClick += internalAction;
        }

        internal void SetChancellorServer(Player player)
        {
            if (!isServer) throw new AccessViolationException("Only the server may access this command!");
            if (Chancellor != null)
                PreviousChancellor = Chancellor;
            Chancellor = player;
        }
        internal void SetPresidentServer(Player player)
        {
            if (!isServer) throw new AccessViolationException("Only the server may access this command!");
            if (President != null)
                PreviousPresident = President;
            President = player;
            SetChancellorServer(null);
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
            var seatPos = Array.IndexOf(SeatedPlayers, value);
            if (placard.CurrentArea != null)
            {
                if (current)
                    placard.CurrentArea.Current = null;
                else
                    placard.CurrentArea.Previous = null;
            }
            placard.CurrentArea = PlayAreas[seatPos];
            if (current)
                placard.CurrentArea.Current = placard;
            else
                placard.CurrentArea.Previous = placard;
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
