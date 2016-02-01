using SecretHitler.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkPlayerObject : NetworkObject
    {
        public Player Player { get; private set; }
        public NetworkPlayerObject(ServerCommands command, Player player)
            :base(command)
        {
            Player = player;
        }
        private NetworkPlayerObject() { }
        public class PlayerObjectReader : DefaultObjectReader
        {
            public override byte[] GenerateByteStream(NetworkObject obj)
            {
                var playerObj = obj as NetworkPlayerObject;
                if (playerObj == null) return base.GenerateByteStream(obj);
                var bytes = Header(playerObj);
                bytes.AddRange(Encoding.ASCII.GetBytes(playerObj.Player.Name));
                return bytes.ToArray();
            }
            public override NetworkObject GenerateObject(byte[] bytes)
            {
                var playerObj = new NetworkPlayerObject();
                DecodeHeader(playerObj, bytes);
                var lastIndex = FindLastByte(bytes, CONTENTINDEX) - 1;
                playerObj.Player = Player.GetPlayer(Encoding.ASCII.GetString(bytes, CONTENTINDEX, FindLastByte(bytes, CONTENTINDEX) - CONTENTINDEX));
                return playerObj;
            }
        }
    }
}
