using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    class Deck
    {
        private Stack<Card> cards;

        public Deck(params Card[] cards)
        {
            this.cards = new Stack<Card>(cards);
        }

        public Card GetCard(bool removeFromStack = true)
            => removeFromStack ? cards.Pop() : cards.Peek();

        public void ReturnCard(Card card)
            => cards.Push(card);

    }
}
