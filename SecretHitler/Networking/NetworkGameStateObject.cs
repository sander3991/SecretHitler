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

        public class GameStateObjectReader : DefaultObjectReader
        {
            public override byte[] GenerateByteStream(NetworkObject obj)
            {
                var gameStateObj = obj as NetworkGameStateObject;
                if (gameStateObj == null) return base.GenerateByteStream(obj);
                var seatedPlayers = gameStateObj.GameState.SeatedPlayers;
                string[] players = new string[seatedPlayers.Length];
                for (var i = 0; i < seatedPlayers.Length; i++)
                    players[i] = seatedPlayers[i] == null ? null : EncodeString(seatedPlayers[i].Name);
                gameStateObj.Message = string.Join(SEPERATOR.ToString(), players);
                var bytes = base.GenerateByteStream(gameStateObj);
                gameStateObj.Message = null;
                return bytes;
            }
            public override NetworkObject GenerateObject(byte[] bytes)
            {
                var gameStateObj = new NetworkGameStateObject();
                DecodeHeader(gameStateObj, bytes);
                var lastByte = FindLastByte(bytes);
                var str = Encoding.ASCII.GetString(bytes, CONTENTINDEX, lastByte - CONTENTINDEX);
                var players = str.Split(SEPERATOR);
                var gameState = new GameState();
                gameState.SeatedPlayers = new Player[10];
                for(var i = 0; i < players.Length; i++)
                    gameState.SeatedPlayers[i] = string.IsNullOrEmpty(players[i]) ? null : Player.GetPlayer(players[i]);
                gameStateObj.GameState = gameState;
                return gameStateObj;
            }
        }
    }
}
