using SecretHitler.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkNewPlayerObject : NetworkObject
    {
        public Player Player { get; private set; }
        public int SeatPos { get; private set; }
        private NetworkNewPlayerObject() { }
        public NetworkNewPlayerObject(ServerCommands command, Player player, int seatPos)
            :base(command)
        {
            Player = player;
            SeatPos = seatPos;
        }
        public class NewPlayerObjectReader : DefaultObjectReader
        {
            public override byte[] GenerateByteStream(NetworkObject obj)
            {
                var playerObj = obj as NetworkNewPlayerObject;
                if (playerObj == null)
                    return base.GenerateByteStream(obj);
                var bytes = Header(obj);
                bytes.AddRange(BitConverter.GetBytes(playerObj.SeatPos));
                bytes.AddRange(Encoding.ASCII.GetBytes(playerObj.Player.Name));
                return bytes.ToArray();

            }
            public override NetworkObject GenerateObject(byte[] bytes)
            {
                var obj = new NetworkNewPlayerObject();
                DecodeHeader(obj, bytes);
                obj.SeatPos = BitConverter.ToInt32(bytes, CONTENTINDEX);
                var lastByte = FindLastByte(bytes, CONTENTINDEX + 4);
                var playerName = Encoding.ASCII.GetString(bytes, CONTENTINDEX + 4, lastByte - CONTENTINDEX - 4);
                obj.Player = Player.GetPlayer(playerName);
                return obj;
            }
        }
    }
}
