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
        public GameState()
        {
            SeatedPlayers = new Player[10];
            DiscardPile = new Deck<CardPolicy>(17);
            DrawPile = new Deck<CardPolicy>(17);
        }

        protected bool CheckIfPlayerIsntPreviousGovernment(Player player) => player != PreviousChancellor && player != PreviousPresident;

        internal virtual void SetChancellor(Player player)
        {
            if (Chancellor != null)
                PreviousChancellor = Chancellor;
            Chancellor = player;
        }

        internal virtual void SetPresident(Player player)
        {
            if (President != null)
                PreviousPresident = President;
            President = player;
            SetChancellor(null);
        }
        
        

        public int GetPlayerPos(string user)
        {
            for(var i = 0; i < SeatedPlayers.Length; i++)
                if(SeatedPlayers[i]?.Name == user)
                    return i;
            return -1;
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
