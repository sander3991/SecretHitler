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
        public class PlayerObjectReader : AbstractObjectReader<NetworkPlayerObject>
        {
            public override byte[] EncodeObject(NetworkPlayerObject obj)
            {
                var bytes = Header(obj);
                bytes.AddRange(Encoding.ASCII.GetBytes(obj.Player.Name));
                return bytes.ToArray();
            }
            public override NetworkPlayerObject DecodeObject(byte[] bytes)
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
