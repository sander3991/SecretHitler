using SecretHitler.Networking;
using SecretHitler.Objects;
using SecretHitler.Views;
using System;
using System.Collections.Generic;
using static SecretHitler.Networking.ServerCommands;

namespace SecretHitler.Logic
{
    public class ClientGameState : GameState
    {
        private GamePanel panel;
        public Client Client { get; }
        public ChatHandler Chat { get; }
        public event Action<GameState> OnStart;
        public Player Me { get; private set; } //Client Side Only
        public LinkedList<GameObject> GameObjects { get { return panel.Objects; } }
        public PlayArea[] PlayAreas { get { return panel.PlayerAreas; } }
        public Game Window { get; private set; }
        public bool IsHost { get; }

        public ClientGameState(GamePanel panel, Client client, Game game, bool isHost)
            : base()
        {
            IsHost = isHost;
            Client = client;
            Chat = ChatHandler.Initialize(this);
            client.OnConnected += BindGetGamestate;
            panel.InitializePiles(DrawPile, DiscardPile);
            for (var i = 0; i < 17; i++)
                DrawPile.ReturnCard(new CardPolicyUnknown());
            Window = game;
            this.panel = panel;
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
                case SendGameState:
                    var gameStateObj = obj as NetworkGameStateObject;
                    var gameState = gameStateObj.GameState;
                    for (var i = 0; i < gameState.SeatedPlayers.Length; i++)
                        if (gameState.SeatedPlayers[i] != null)
                        {
                            SeatPlayer(gameState.SeatedPlayers[i], i);
                            if (SeatedPlayers[i].Name == Client.Name)
                                Me = SeatedPlayers[i];
                        }
                    break;
                case PlayerConnected:
                    var newPlayerObj = obj as NetworkNewPlayerObject;
                    SeatPlayer(newPlayerObj.Player, newPlayerObj.SeatPos);
                    break;
                case AnnounceCard:
                    SetPresident(null);
                    PreviousChancellor = null;
                    PreviousPresident = null;
                    SetPlacardValue(PlayArea.PlacardPrevChancellor, null, false);
                    SetPlacardValue(PlayArea.PlacardPrevPresident, null, false);
                    foreach (Player player in SeatedPlayers)
                        if (player != null) player?.Hand.SetUnkown();
                    Me.Hand = new PlayerHand((obj as NetworkCardObject).Cards);
                    SetUnknownCards();
                    OnStart?.Invoke(this);
                    break;
                case RevealRole:
                    var revealObj = obj as NetworkRevealRoleObject;
                    ResetState();
                    revealObj.Player.Hand.Role = revealObj.SecretRole;
                    revealObj.Player.Hand.Membership = revealObj.SecretRole.IsFascist ? (CardMembership)new CardMembershipFascist() : new CardMembershipLiberal();
                    break;
                case AnnouncePresident:
                    SetPresident((obj as NetworkPlayerObject).Player);
                    if (President == Me)
                    {
                        Window.SetPlayerMessage("Please pick your chancellor.");
                        PickPlayerAction(Client.ChooseChancellor, CheckIfPlayerIsntPreviousGovernment, (Player player) => Window.SetPlayerMessage($"You can't pick {player.Name} as your chancellor. Pick someone else!"));
                    }
                    else
                        Window.SetPlayerMessage($"Wait for {President.Name} to pick his/her chancellor.");
                    break;
                case AnnounceChancellor:
                    SetChancellor((obj as NetworkPlayerObject).Player);
                    Window.SetPlayerMessage($"Please vote for {President.Name} as President and {Chancellor.Name} as Chancellor");
                    PickTarotCard(Client.CastVote);
                    break;
                case PlayerVoted:
                    var playerVotedObj = obj as NetworkPlayerObject;
                    playerVotedObj.Player.PlayArea.SetVoteCard(new CardBallotUnknown());
                    break;
                case AnnounceVotes:
                    var announceVotes = obj as NetworkVoteResultObject;
                    for (var i = 0; i < announceVotes.Votes.Length; i++)
                        SeatedPlayers[i].PlayArea.SetVoteCard(announceVotes.Votes[i] ? (CardBallot)new CardBallotYes() : new CardBallotNo());
                    break;

            }
        }
        private void ResetState()
        {
            foreach (var area in PlayAreas)
                area.ResetState();
        }

        private void SeatPlayer(Player player, int pos)
        {
            SeatedPlayers[pos] = player;
            player.PlayArea = PlayAreas[pos];
        }

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
                        if (allowPickSelf || SeatedPlayers[i] != Me)
                            PlayAreas[i].OnClick -= internalAction;
                }
                else
                    onFail?.Invoke(player);
            };
            for (var i = 0; i < PlayerCount; i++)
                if (allowPickSelf || SeatedPlayers[i] != Me)
                    PlayAreas[i].OnClick += internalAction;
        }

        private void PickTarotCard(Action<bool> callback)
        {
            int playerCount = PlayerCount;
            Action<Card> internalAction = null;
            internalAction = (Card clickedCard) =>
            {
                callback(clickedCard is CardBallotYes);
                Me.Hand.Yes.OnClick -= internalAction;
                Me.Hand.No.OnClick -= internalAction;
            };
            Me.Hand.Yes.OnClick += internalAction;
            Me.Hand.No.OnClick += internalAction;
        }

        internal override void SetChancellor(Player player)
        {
            if (Chancellor != null)
                SetPlacardValue(PlayArea.PlacardPrevChancellor, PreviousChancellor, false);
            base.SetChancellor(player);
            SetPlacardValue(PlayArea.PlacardChancellor, Chancellor, true);
        }
        internal override void SetPresident(Player player)
        {
            if (President != null)
                SetPlacardValue(PlayArea.PlacardPrevPresident, President, false);
            base.SetPresident(player);
            SetPlacardValue(PlayArea.PlacardPresident, President, true);
        }
        private void SetPlacardValue(Placard placard, Player value, bool current)
        {
            if (placard.CurrentArea != null)
            {
                if (current)
                    placard.CurrentArea.Current = null;
                else
                    placard.CurrentArea.Previous = null;
            }
            if (value?.PlayArea != null)
            {
                placard.CurrentArea = value.PlayArea;
                if (current)
                    placard.CurrentArea.Current = placard;
                else
                    placard.CurrentArea.Previous = placard;
            }
        }
        private void SetUnknownCards()
        {
            foreach (var player in SeatedPlayers)
                if (player != null && player.Hand == null)
                    player.Hand = new PlayerHand(new CardSecretRoleUnknown { Flipped = true }, new CardMembershipUnknown { Flipped = true }, flipped: true);
        }
    }
}
