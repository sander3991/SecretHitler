using System;
using SecretHitler.Networking;
using SecretHitler.Objects;

namespace SecretHitler.Logic
{
    public class ServerGameState : GameState
    {
        public Server Server { get; }
        private static int[] presidentOrder = new int[] { 0, 8, 4, 5, 6, 7, 9, 3, 2, 1 };
        internal bool?[] votes;
        internal bool AreVoting { get; set; }
        internal bool PresidentPicking { get; set; } = false;
        internal bool ChancellorPicking { get; set; } = false;
        internal CardPolicy[] CurrentlyPicked { get; set; }
        public ServerGameState(Server server)
            : base()
        {
            Server = server;
            votes = new bool?[10];
            for (var i = 0; i < 6; i++)
                DrawPile.AddCard(new CardPolicyLiberal());
            for (var i = 0; i < 11; i++)
                DrawPile.AddCard(new CardPolicyFascist());
            DrawPile.Shuffle();
        }
        public void SetVote(Player player, bool yes)
        {
            var playerPos = GetPlayerPos(player.Name);
            if (playerPos != -1)
                votes[playerPos] = yes;
        }
        public void ResetVotes()
        {
            for (var i = 0; i < votes.Length; i++)
                votes[i] = null;
        }
        public bool EveryoneVoted()
        {
            var playerCount = PlayerCount;
            for (var i = 0; i < playerCount; i++)
                if (!votes[i].HasValue)
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

        internal bool[] GetVotes()
        {
            var votes = new bool[PlayerCount];
            for(var i = 0; i < votes.Length; i++)
            {
                if (!this.votes[i].HasValue) throw new InvalidOperationException("Not all votes are in yet!");
                votes[i] = this.votes[i].Value;
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
                if (!votes[i].HasValue) throw new InvalidOperationException("Not all votes are in yet!");
                if (votes[i].Value) yesCounter++; else noCounter++;
            }
            return yesCounter > noCounter;
        }

        public Player GetNextPresident()
        {
            var i = 0;
            bool nextIsPresident = false;
            while(true)
            {
                if(SeatedPlayers[presidentOrder[i]] != null)
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
