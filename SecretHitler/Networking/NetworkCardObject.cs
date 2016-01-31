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

        public class CardObjectReader : DefaultObjectReader
        {
            public override byte[] GenerateByteStream(NetworkObject obj)
            {
                var cardObj = obj as NetworkCardObject;
                if (cardObj == null) return base.GenerateByteStream(obj);
                var bytes = Header(cardObj);
                for (var i = 0; i < cardObj.Cards.Length; i++)
                    bytes.Add(cardObj.Cards[i].ToByte());
                return bytes.ToArray();

            }
            public override NetworkObject GenerateObject(byte[] bytes)
            {
                var cardObj = new NetworkCardObject();
                DecodeHeader(cardObj, bytes);
                List<Card> cards = new List<Card>();
                for(var i = CONTENTINDEX; bytes[i] != 0; i++)
                    cards.Add(bytes[i].ToCard());
                cardObj.Cards = cards.ToArray();
                return cardObj;
            }
        }
    }
}
