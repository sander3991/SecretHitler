using SecretHitler.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkCardObject : NetworkObject
    {
        public Card[] Cards { get; private set; }
        public NetworkCardObject(ServerCommands command, params Card[] cards)
            : base(command)
        {
            Cards = cards;
        }
        protected NetworkCardObject() { }

        public class CardObjectReader : AbstractObjectReader<NetworkCardObject>
        {
            public override byte[] EncodeObject(NetworkCardObject obj)
            {
                var bytes = Header(obj);
                for (var i = 0; i < obj.Cards.Length; i++)
                    bytes.Add(obj.Cards[i].ToByte());
                return bytes.ToArray();

            }
            public override NetworkCardObject DecodeObject(byte[] bytes)
            {
                var cardObj = new NetworkCardObject();
                DecodeHeader(cardObj, bytes);
                List<Card> cards = new List<Card>();
                for(var i = CONTENTINDEX; HasByte(bytes, i); i++)
                    cards.Add(bytes[i].ToCard());
                cardObj.Cards = cards.ToArray();
                return cardObj;
            }
        }
    }
}
