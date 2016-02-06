using SecretHitler.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkGameStateObject : NetworkObject
    {
        public GameState GameState { get; private set; }
        private NetworkGameStateObject() { }
        public NetworkGameStateObject(ServerCommands command, GameState state)
            :base(command)
        {
            GameState = state;
        }
        public override string ToString()
        {
            return "GameState object";
        }
        public class GameStateObjectReader : AbstractObjectReader<NetworkGameStateObject>
        {
            public override byte[] EncodeObject(NetworkGameStateObject obj, List<byte> bytes)
            {
                var seatedPlayers = obj.GameState.SeatedPlayers;
                string[] players = new string[seatedPlayers.Length];
                for (var i = 0; i < seatedPlayers.Length; i++)
                    players[i] = seatedPlayers[i] == null ? null : EncodeString(seatedPlayers[i].Name);
                bytes.AddRange(Encoding.ASCII.GetBytes(string.Join(SEPERATOR.ToString(), players)));
                return bytes.ToArray();
            }
            public override NetworkGameStateObject DecodeObject(byte[] bytes, bool serverSide)
            {
                var gameStateObj = new NetworkGameStateObject();
                DecodeHeader(gameStateObj, bytes);
                var lastByte = FindLastByte(bytes);
                var str = Encoding.ASCII.GetString(bytes, CONTENTINDEX, lastByte - CONTENTINDEX);
                var players = str.Split(SEPERATOR);
                var gameState = new NetworkGameState();
                gameState.SeatedPlayers = new Player[10];
                for(var i = 0; i < players.Length; i++)
                    gameState.SeatedPlayers[i] = string.IsNullOrEmpty(players[i]) ? null : Player.GetPlayer(players[i]);
                gameStateObj.GameState = gameState;
                return gameStateObj;
            }
        }
    }
}
