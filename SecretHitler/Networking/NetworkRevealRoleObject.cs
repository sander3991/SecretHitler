using SecretHitler.Logic;
using SecretHitler.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkRevealRoleObject : NetworkObject
    {
        public Player Player { get; private set; }
        public CardSecretRole SecretRole { get; private set; }
        public NetworkRevealRoleObject(Player player)
            :base(ServerCommands.RevealRole)
        {
            Player = player;
            SecretRole = player.Hand.Role;
        }
        private NetworkRevealRoleObject() { }

        public class RevealRoleObjectReader : DefaultObjectReader
        {
            public override byte[] GenerateByteStream(NetworkObject obj)
            {
                var revealObj = obj as NetworkRevealRoleObject;
                if (revealObj == null)
                    return base.GenerateByteStream(obj);

                var bytes = Header(revealObj);
                bytes.Add(revealObj.SecretRole.ToByte());
                bytes.AddRange(Encoding.ASCII.GetBytes(revealObj.Player.Name));
                return bytes.ToArray();
            }
            public override NetworkObject GenerateObject(byte[] bytes)
            {
                var revealObj = new NetworkRevealRoleObject();
                DecodeHeader(revealObj, bytes);
                revealObj.SecretRole = bytes[CONTENTINDEX].ToCard() as CardSecretRole;
                var lastByte = FindLastByte(bytes, CONTENTINDEX + 1);
                var playerName = Encoding.ASCII.GetString(bytes, CONTENTINDEX + 1, lastByte - 1 - CONTENTINDEX);
                revealObj.Player = Player.GetPlayer(playerName);
                revealObj.Player.Hand.Role = revealObj.SecretRole;
                revealObj.Player.Hand.Membership = revealObj.SecretRole.IsFascist ? (CardMembership)new CardMembershipFascist() : new CardMembershipLiberal();
                return revealObj;
            }
        }
    }
}
