using SecretHitler.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    /*public class NetworkFascistActionObject : NetworkObject
    {
        public Player Player { get; private set; }
        public byte Action { get; private set; }
        public NetworkFascistActionObject(ServerCommands command, Player player, byte action)
            :base(command)
        {
            Player = player;
            Action = action;
        }
        private NetworkFascistActionObject() { }

        public class FascistActionObjectReader : AbstractObjectReader<NetworkFascistActionObject>
        {
            public override byte[] EncodeObject(NetworkFascistActionObject obj, List<byte> bytes)
            {
                bytes.Add(obj.Action);
                bytes.AddRange(Encoding.ASCII.GetBytes(obj.Player.Name));
                return bytes.ToArray();
            }

            public override NetworkFascistActionObject DecodeObject(byte[] bytes, bool serverSide)
            {
                var obj = new NetworkFascistActionObject();
                DecodeHeader(obj, bytes);
                obj.Action = bytes[CONTENTINDEX];
                var playerName = Encoding.ASCII.GetString(bytes, CONTENTINDEX + 1, FindLastByte(bytes, CONTENTINDEX + 1) - CONTENTINDEX - 1);
                obj.Player = serverSide ? Player.GetPlayerServerSide(playerName) : Player.GetPlayer(playerName);
                return obj;
            }
        }
    }*/
}
