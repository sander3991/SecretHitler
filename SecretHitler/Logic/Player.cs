﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Logic
{
    public class Player
    {
        public string Name { get; set; }
        public PlayerHand Hand { get; set; }
        private Player(string name)
        {
            Name = name;
        }

        private static Dictionary<string, Player> players = new Dictionary<string, Player>();
        private static Dictionary<string, Player> serverPlayers; //generated on first GetPlayerServerSide request

        public static Player GetPlayer(string name)
        {
            if (!players.ContainsKey(name))
                players.Add(name, new Player(name));
            return players[name];
        }
        public static Player GetPlayerServerSide(string name)
        {
            if (serverPlayers == null) serverPlayers = new Dictionary<string, Player>();
            if (!serverPlayers.ContainsKey(name))
                serverPlayers.Add(name, new Player(name));
            return serverPlayers[name];
        }
        public static void Clear()
        {
            players.Clear();
            serverPlayers?.Clear();
        }

        public override string ToString() => Name;
    }
}
