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
    public abstract class GameState
    {
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
        public Player President { get; protected set; }
        public Player Chancellor { get; protected set; }
        public Player PreviousPresident { get; protected set; }
        public Player PreviousChancellor { get; protected set; }
        public Deck<CardPolicy> DrawPile { get; protected set; }
        public Deck<CardPolicy> DiscardPile { get; protected set; }
        public bool PreviousGovernmentElected { get; set; }
        public int LiberalCardsPlayed { get; private set; }
        public int FascistsCardsPlayed { get; private set; }
        public FascistAction[] FascistActions { get; internal set; }
        public CardPolicyLiberal[] PlayedLiberalCards { get; private set; }
        public CardPolicyFascist[] PlayedFascistCards { get; private set; }
        public GameState()
        {
            SeatedPlayers = new Player[10];
            DiscardPile = new Deck<CardPolicy>(17);
            DrawPile = new Deck<CardPolicy>(17);
            PlayedLiberalCards = new CardPolicyLiberal[5];
            PlayedFascistCards = new CardPolicyFascist[6];
        }

        protected bool CheckIfPlayerIsntPreviousGovernment(Player player) => player != PreviousChancellor && player != PreviousPresident;

        internal virtual void SetChancellor(Player player)
        {
            if (PreviousGovernmentElected)
                SetPreviousChancellor(Chancellor);
            Chancellor = player;
        }

        internal virtual void SetPresident(Player player)
        {
            if (PreviousGovernmentElected)
                SetPreviousPresident(President);
            President = player;
        }
        
        internal virtual void SetPreviousPresident(Player player)
        {
            PreviousPresident = player;
        }

        internal virtual void SetPreviousChancellor(Player player)
        {
            PreviousChancellor = player;
        }

        public int GetPlayerPos(string user)
        {
            for(var i = 0; i < SeatedPlayers.Length; i++)
                if(SeatedPlayers[i]?.Name == user)
                    return i;
            return -1;
        }

        internal CardPolicy[] GetPolicyCards(int amount = 3)
        {
            if (DrawPile.CardsRemaining < amount)
            {
                while (DiscardPile.CardsRemaining > 0)
                    DrawPile.AddCard(DiscardPile.GetCard());
                DrawPile.Shuffle();
            }
            var policies = new CardPolicy[amount];
            for (var i = 0; i < amount; i++)
                policies[i] = DrawPile.GetCard();
            return policies;
        }

        public virtual void PlayPolicy(CardPolicy policy)
        {
            if (policy is CardPolicyFascist)
                PlayedFascistCards[FascistsCardsPlayed++] = policy as CardPolicyFascist;
            else if (policy is CardPolicyLiberal)
                PlayedLiberalCards[LiberalCardsPlayed++] = policy as CardPolicyLiberal;
        }

        public bool MayElectPlayer(Player player)
        {
            if (player == PreviousChancellor) return false;
            if (player == PreviousPresident && PlayerCount > 5) return false;
            if (player == President) return false;
            return true;
        }
    }

    public class NetworkGameState : GameState
    {
        public NetworkGameState()
            :base()
        {

        }
    }
}
