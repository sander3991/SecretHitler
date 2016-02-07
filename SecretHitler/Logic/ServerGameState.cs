using System;
using SecretHitler.Networking;
using SecretHitler.Objects;

namespace SecretHitler.Logic
{
    public enum Vote : byte
    {
        NoVote = 0,
        Ja = 1,
        Nein = 2,
        Dead = 3
    }
    public class ServerGameState : GameState
    {
        public Server Server { get; }
        private static int[] presidentOrder = new int[] { 0, 8, 4, 5, 6, 7, 9, 3, 2, 1 };
        internal Vote[] votes;
        internal bool AreVoting { get; set; }
        internal bool PresidentPicking { get; set; } = false;
        internal bool ChancellorPicking { get; set; } = false;
        internal CardPolicy[] CurrentlyPicked { get; set; }
        internal ServerCommands AwaitingPresidentAction { get; set; }
        public ServerGameState(Server server)
            : base()
        {
            Server = server;
            votes = new Vote[10];
            for (var i = 0; i < 6; i++)
                DrawPile.AddCard(new CardPolicyLiberal());
            for (var i = 0; i < 11; i++)
                DrawPile.AddCard(new CardPolicyFascist());
            DrawPile.Shuffle();
        }
        public void SetVote(Player player, bool yes)
        {
            var playerPos = GetPlayerPos(player.Name);
            if (player.Dead)
                votes[playerPos] = Vote.Dead;
            else if (playerPos != -1)
                votes[playerPos] = yes ? Vote.Ja : Vote.Nein;
        }
        public void ResetVotes()
        {
            for (var i = 0; i < votes.Length; i++)
            {
                var player = SeatedPlayers[i];
                if (player == null || !player.Dead)
                    votes[i] = Vote.NoVote;
                else
                    votes[i] = Vote.Dead;
            }
        }
        public bool EveryoneVoted()
        {
            var playerCount = PlayerCount;
            for (var i = 0; i < playerCount; i++)
                if (votes[i] == Vote.NoVote)
                    return false;
            return true;
        }
        public int SeatPlayer(Player player)
        {
            for (var i = 0; i < SeatedPlayers.Length; i++)
                if (SeatedPlayers[i] == null)
                {
                    SeatedPlayers[i] = player;
                    return i;
                }
            return -1;
        }

        internal Vote[] GetVotes()
        {
            var votes = new Vote[PlayerCount];
            for(var i = 0; i < votes.Length; i++)
            {
                if (this.votes[i] == Vote.NoVote) throw new InvalidOperationException("Not all votes are in yet!");
                votes[i] = this.votes[i];
            }
            return votes;
        }

        internal void ReturnPolicyCards(CardPolicy[] cards)
        {
            for (var i = cards.Length - 1; i >= 0; i--)
                DrawPile.AddCard(cards[i]);
        }

        internal bool VotePassed()
        {
            var playerCount = PlayerCount;
            var yesCounter = 0;
            var noCounter = 0;
            for(var i = 0; i < playerCount; i++)
            {
                if (votes[i] == Vote.NoVote)
                    throw new InvalidOperationException("Not all votes are in yet!");
                else if (votes[i] == Vote.Ja)
                    yesCounter++;
                else if(votes[i] == Vote.Nein)
                    noCounter++;
                //else dead do nothing
            }
            return yesCounter > noCounter;
        }

        public Player GetNextPresident()
        {
            var i = 0;
            bool nextIsPresident = false;
            while(true)
            {
                if(SeatedPlayers[presidentOrder[i]] != null && !SeatedPlayers[presidentOrder[i]].Dead)
                {
                    if (nextIsPresident)
                        return SeatedPlayers[presidentOrder[i]];

                    if (SeatedPlayers[presidentOrder[i]] == President)
                        nextIsPresident = true;
                }
                i = (i + 1) % presidentOrder.Length;
            }
        }
    }
}
