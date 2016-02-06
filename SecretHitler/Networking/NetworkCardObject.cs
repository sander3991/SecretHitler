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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Card object with {Cards.Length} cards: ");
            for (var i = 0; i < Cards.Length; i++)
            {
                sb.Append(Cards[i].GetType().Name);
                if (i + 1 < Cards.Length)
                    sb.Append(" & ");
            }
            return sb.ToString();
        }
        public class CardObjectReader : AbstractObjectReader<NetworkCardObject>
        {
            public override byte[] EncodeObject(NetworkCardObject obj, List<byte> bytes)
            {
                for (var i = 0; i < obj.Cards.Length; i++)
                    bytes.Add(obj.Cards[i].ToByte());
                return bytes.ToArray();

            }
            public override NetworkCardObject DecodeObject(byte[] bytes, bool serverSide)
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
