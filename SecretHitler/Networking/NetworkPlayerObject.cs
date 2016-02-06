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
        public override string ToString() => $"Player object: {Player.Name}";
        public class PlayerObjectReader : AbstractObjectReader<NetworkPlayerObject>
        {
            public override byte[] EncodeObject(NetworkPlayerObject obj, List<byte> bytes)
            {
                bytes.AddRange(Encoding.ASCII.GetBytes(obj.Player.Name));
                return bytes.ToArray();
            }
            public override NetworkPlayerObject DecodeObject(byte[] bytes, bool serverSide)
            {
                var playerObj = new NetworkPlayerObject();
                DecodeHeader(playerObj, bytes);
                var lastIndex = FindLastByte(bytes, CONTENTINDEX) - 1;
                var playerName = Encoding.ASCII.GetString(bytes, CONTENTINDEX, FindLastByte(bytes, CONTENTINDEX) - CONTENTINDEX);
                playerObj.Player = serverSide ? Player.GetPlayerServerSide(playerName) : Player.GetPlayer(playerName);
                return playerObj;
            }
        }
    }
}
