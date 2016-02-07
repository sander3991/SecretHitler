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
        public event Action<ClientGameState> OnStart;
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
                DrawPile.AddCard(new CardPolicyUnknown());
            Window = game;
            this.panel = panel;
            OnStart += (ClientGameState state) =>
            {
                state.FascistActions = FascistAction.GetActionsForPlayers(state.PlayerCount);
            };
        }

        private void BindGetGamestate(Client obj)
        {
            obj.ReceiveHandler.OnReceive += OnMessageReceived;
            obj.RequestGameState();
        }

        internal override void StartGame()
        {
            base.StartGame();
            foreach (Player player in SeatedPlayers)
                if (player != null)
                {
                    player?.Hand.SetUnkown();
                    player.PlayArea.SetVoteCard(null);
                    player.PlayArea.SetNotHitler(null);
                }
            SetUnknownCards();
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
                    StartGame();
                    Me.Hand = new PlayerHand((obj as NetworkCardObject).Cards);
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
                    SetChancellor(null);
                    PreviousGovernmentElected = false;
                    if (President == Me)
                        PickPlayerAction(Client.ChooseChancellor);
                    break;
                case AnnounceChancellor:
                    SetChancellor((obj as NetworkPlayerObject).Player);
                    ClearVotes();
                    if(!Me.Dead)
                        PickTarotCard(Client.CastVote);
                    break;
                case PlayerVoted:
                    var playerVotedObj = obj as NetworkPlayerObject;
                    playerVotedObj.Player.PlayArea.SetVoteCard(new CardBallotUnknown());
                    break;
                case AnnounceVotes:
                    var announceVotes = obj as NetworkVoteResultObject;
                    PreviousGovernmentElected = announceVotes.Passed == Vote.Ja;
                    for (var i = 0; i < announceVotes.Votes.Length; i++)
                        if(announceVotes.Votes[i] != Vote.Dead)
                            SeatedPlayers[i].PlayArea.SetVoteCard(announceVotes.Votes[i] == Vote.Ja ? (CardBallot)new CardBallotYes() : new CardBallotNo());
                    break;
                case PolicyCardsDrawn:
                    var policyCardsDrawn = obj as NetworkByteObject;
                    GetPolicyCards(policyCardsDrawn.Value);
                    break;
                case PresidentPickPolicyCard:
                case ChancellorPickPolicyCard:
                    var policyCards = obj as NetworkCardObject;
                    Me.PlayArea.PickPolicyCard(policyCards.Cards);
                    break;
                case PresidentDiscarded:
                case ChancellorDiscarded:
                    DiscardPile.AddCard(new CardPolicyUnknown());
                    ClearVotes();
                    break;
                case CardPlayed:
                    PlayPolicy((obj as NetworkCardObject).Cards[0] as CardPolicy);
                    break;
                case PresidentActionChoosePresident:
                    if (President != Me) break;
                    PickPlayerAction(Client.PickNextPresident, isElection: false);
                    break;
                case PresidentActionExamine:
                    if (President != Me) break;
                    var cardObj = obj as NetworkCardObject;
                    Me.PlayArea.PickPolicyCard(cardObj.Cards, (Card card) => { Client.ConfirmPolicyRead(); });
                    break;
                case PresidentActionInvestigate:
                    if (President != Me) break;
                    PickPlayerAction(Client.InvestigatePlayer, isElection: false);
                    break;
                case PresidentActionKill:
                    if (President != Me) break;
                    PickPlayerAction(Client.KillPlayer, isElection: false);
                    break;
                case RevealMembership:
                    var revealMembership = obj as NetworkNewPlayerObject;
                    CardMembership membership;
                    if (revealMembership.SeatPos == 1)
                        membership = new CardMembershipFascist();
                    else
                        membership = new CardMembershipLiberal();
                    membership.Location = revealMembership.Player.Hand.Membership.Location;
                    revealMembership.Player.Hand.Membership = membership;
                    break;
                case ServerCommands.KillPlayer:
                    KillPlayer((obj as NetworkPlayerObject).Player);
                    break;
                case FascistWin:
                case LiberalWin:
                    Window.EndGame();
                    PlayingGame = false;
                    break;
                case ServerCommands.IncrementElectionTracker:
                    IncrementElectionTracker();
                    break;
                case ServerCommands.ResetElectionTracker:
                    ResetElectionTracker();
                    break;
                case NotHitler:
                    (obj as NetworkPlayerObject).Player.PlayArea.SetNotHitler(new CardNotHitler());
                    break;
                case PresidentConfirmVeto:
                    if (President == Me)
                        PickTarotCard(Client.ConfirmVeto);
                    break;
                case AnnounceVetoResult:
                    var result = (obj as NetworkBoolObject).Value;
                    if(Me == Chancellor)
                    {
                        if (result)
                            Me.PlayArea.RemovePolicyCards();
                        else
                            Me.PlayArea.RebindPolicyCards();
                    }
                    break;
            }
        }

        internal void RequestVeto()
        {
            Client.RequestVeto();
        }

        private void ClearVotes()
        {
            foreach (var area in PlayAreas)
                area.SetVoteCard(null);
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

        private void PickPlayerAction(
            Action<Player> callback,
            Func<Player, bool> success = null,
            Action<Player> onFail = null,
            bool isElection = true
        )
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
                        if (SeatedPlayers[i] != Me)
                            PlayAreas[i].OnClick -= internalAction;
                }
                else
                    onFail?.Invoke(player);
            };
            var playersBound = 0;
            for (var i = 0; i < PlayerCount; i++)
                if (MayElectPlayer(SeatedPlayers[i]) || (!isElection && SeatedPlayers[i] != Me && !SeatedPlayers[i].Dead))
                {
                    PlayAreas[i].OnClick += internalAction;
                    playersBound++;
                }
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

        internal void ReturnPolicyCards(int id)
        {
            Client.ReturnPolicyCard((byte)id, Me == President);
        }

        internal override void SetChancellor(Player player)
        {
            base.SetChancellor(player);
            SetPlacardValue(PlayArea.PlacardChancellor, Chancellor, true);
        }

        internal override void SetPresident(Player player)
        {
            base.SetPresident(player);
            SetPlacardValue(PlayArea.PlacardPresident, President, true);
        }

        internal override void SetPreviousChancellor(Player player)
        {
            base.SetPreviousChancellor(player);
            SetPlacardValue(PlayArea.PlacardPrevChancellor, PreviousChancellor, false);
        }

        internal override void SetPreviousPresident(Player player)
        {
            base.SetPreviousPresident(player);
            SetPlacardValue(PlayArea.PlacardPrevPresident, PreviousPresident, false);
        }

        public override void PlayPolicy(CardPolicy policy)
        {
            base.PlayPolicy(policy);
            if (policy is CardPolicyFascist)
                panel.FascistBoard.AddCard(policy as CardPolicyFascist);
            else if (policy is CardPolicyLiberal)
                panel.LiberalBoard.AddCard(policy as CardPolicyLiberal);
        }

        private void SetPlacardValue(Placard placard, Player value, bool current)
        {
            if (placard.CurrentArea != null)
            {
                if (current && placard.CurrentArea.Current == placard)
                    placard.CurrentArea.Current = null;
                else if (!current && placard.CurrentArea.Previous == placard)
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
