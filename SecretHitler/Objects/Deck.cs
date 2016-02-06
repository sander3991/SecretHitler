using SecretHitler.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public class Deck<T> : GameObject, IHooverable where T : Card
    {
        public static readonly Size DRAWPILESIZE = new Size(99, 137);
        private T[] cards;
        public Bitmap Background { get; private set; }
        public Point BackgroundOffset { get; private set; }
        private int nextCard;
        public int CardsRemaining { get { return nextCard + 1; } }
        public int DeckSize { get { return cards.Length; } }
        public bool FaceDown { get; set; } = true;
        private bool hover;
        private static StringFormat stringFlags = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        public Deck(params T[] cards)
            : this(cards, cards.Length)
        { }
        public Deck(int size)
            :this(new T[size], size)
        { }
        private Deck(T[] cards, int size)
        {
            this.cards = cards;
            for (nextCard = 0; nextCard + 1 < size && cards[nextCard + 1] != null; nextCard++) ;
            nextCard--;
            Size = Card.DEFAULTCARDSIZE;
        }


        public T GetCard(bool removeFromStack = true)
        {
            if (nextCard == -1)
                throw new InvalidOperationException("There are no cards remaining");
            T card = cards[nextCard];
            if (removeFromStack)
                cards[nextCard--] = null;
            return card;
        }

        public void AddCard(T card)
        {
            if ((nextCard + 1) == DeckSize)
                throw new InvalidOperationException("There are no more stack slots available");
            cards[++nextCard] = card;
        }

        public void SetBackground(Bitmap bitmap, Point? backgroundOffset = null)
        {
            BackgroundOffset = backgroundOffset.HasValue ? new Point(Location.X + backgroundOffset.Value.X, Location.Y + backgroundOffset.Value.Y) : Location;
            Background = bitmap;
        }

        public override void Draw(Graphics g)
        {
            const int cardStackHeight = 2;
            if(Background != null)
                g.DrawImageUnscaled(Background, BackgroundOffset);
            Rectangle stackLocation = new Rectangle(Location.X, Location.Y + Size.Height - cardStackHeight, Size.Width, 5);
            for(var i = 0; i < DeckSize && cards[i] != null; i++, stackLocation.Y -= cardStackHeight)
            {
                var img = FaceDown ? cards[i].Back : cards[i].Front;
                var srcRect = new Rectangle(0, img.Height - cardStackHeight, img.Width, cardStackHeight);
                if ((i + 1) == DeckSize || cards[i + 1] == null)
                    g.DrawImage(img, new Point(Location.X, Location.Y - i * cardStackHeight));
                else
                    g.DrawImage(img, destRect: stackLocation, srcRect: srcRect, srcUnit: GraphicsUnit.Pixel);
                g.DrawLine(Pens.Black, stackLocation.X, stackLocation.Y + stackLocation.Height - 1, stackLocation.X + stackLocation.Width, stackLocation.Y + stackLocation.Height - 1);
            }
            if (hover)
            {
                const int circleWidth = 30;
                var circleRect = new Rectangle(Location.X + Size.Width / 2 - circleWidth / 2, Location.Y + Size.Height / 2 - circleWidth / 2, circleWidth, circleWidth);
                g.FillEllipse(Brushes.White, circleRect);
                g.DrawEllipse(Pens.Black, circleRect);
                g.DrawString(CardsRemaining.ToString(), SystemFonts.DefaultFont, Brushes.Black, layoutRectangle: circleRect, format: stringFlags);
            }
            //g.DrawRectangle(Pens.Magenta, DrawLocation);
        }

        public void Shuffle()
        {
            cards.Shuffle();
            for(var i = 0; i < cards.Length; i++)
            {
                if(cards[i] == null)
                    for(var j = i; j < cards.Length; j++)
                        if(cards[j] != null)
                        {
                            cards[i] = cards[j];
                            cards[j] = null;
                            break;
                        }
            }
        }

        public void OnHover()
        {
            hover = true;
        }

        public void OnHoverLeave()
        {
            hover = false;
        }
    }
}
