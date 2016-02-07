using SecretHitler.Logic;
using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public class PlayArea : GameObject, IHooverable
    {
        private enum RotateType
        {
            None,
            Left,
            Right,
            Half
        }
        public static PlacardChancellor PlacardChancellor = new PlacardChancellor();
        public static PlacardPrevChancellor PlacardPrevChancellor = new PlacardPrevChancellor();
        public static PlacardPresident PlacardPresident = new PlacardPresident();
        public static PlacardPrevPresident PlacardPrevPresident = new PlacardPrevPresident();
        public static readonly Size DEFAULTSIZE = new Size(320, 200);
        public static Brush BackgroundBrush = new SolidBrush(Color.FromArgb(101, 150, 119));

        public event Action<PlayArea> OnClick;

        public override Rectangle ClickLocation { get { return Rotate(DrawLocation, rotateType); } }
        public override Size Size
        {
            get { return DEFAULTSIZE; }
            set { throw new InvalidOperationException("Can't set size"); }
        }
        public int ID { get; private set; }
        public Point PreviousPlacardLoc { get; private set; }
        public Point CurrentPlacardLoc { get; private set; }
        private Placard current;
        public Placard Current
        {
            get { return current; }
            set { current = value; if (current != null) current.Location = Location; }
        }
        private Placard previous;
        public Placard Previous
        {
            get { return previous; }
            set { previous = value; if (previous != null) previous.Location = new Point(Location.X + DEFAULTSIZE.Width - Placard.DEFAULTSIZE.Width, Location.Y); }
        }

        protected Player Player { get { return State.SeatedPlayers[ID]; } }
        protected ClientGameState State { get; }

        private CardNotHitler notHitler;
        private CardBallot votedCard;
        private RotateType rotateType;
        private bool isHovering = false;
        private static Brush fontBrush = new SolidBrush(Color.FromArgb(67, 122, 87));
        private static Bitmap DEATH = Properties.Resources.death.CutToSize(new Size(100, 100));
        private Font playerNameFont;
        private Card[] pickPolicyCards;

        public PlayArea(ClientGameState state, int ID)
        {
            State = state;
            this.ID = ID;
        }

        private Font DefinePlayerFont(Graphics g)
        {
            float maxSize = 60;
            Font font = new Font(family: SystemFonts.DefaultFont.FontFamily, emSize: maxSize);
            while (g.MeasureString(Player.Name, font).Width > DEFAULTSIZE.Width)
                font = new Font(family: SystemFonts.DefaultFont.FontFamily, emSize: --maxSize);
            return font;
        }

        private void DrawPlayerName(Graphics g)
        {
            if (playerNameFont == null)
                playerNameFont = DefinePlayerFont(g);
            Point p = Location;

            g.DrawString(
                Player.Name,
                playerNameFont,
                fontBrush,
                Location);
        }

        private void DrawPlayerHand(Graphics g)
        {
            var hand = Player.Hand;
            if (hand != null)
            {
                if (hand.Membership.Location == new Point(0, 0))
                    DefineLocations(hand);
                hand.Membership.Draw(g);
                hand.Role.Draw(g);
                hand.Yes.Draw(g);
                hand.No.Draw(g);
                current?.Draw(g);
                previous?.Draw(g);
            }
        }

        public void SetVoteCard(CardBallot card)
        {
            if (card != null)
                if (ID >= 3 && ID <= 4)
                    card.Location = new Point(Location.X, Location.Y - Card.DEFAULTCARDSIZE.Height / 2);
                else
                    card.Location = new Point(Location.X + Size.Width - Card.DEFAULTCARDSIZE.Width, Location.Y - Card.DEFAULTCARDSIZE.Height / 2);
            votedCard = card;
        }

        public void SetNotHitler(CardNotHitler card)
        {
            if(card != null)
                card.Location = new Point(Location.X + Size.Width / 2 - Card.DEFAULTCARDSIZE.Width / 2, Location.Y - Card.DEFAULTCARDSIZE.Height / 2);
            notHitler = card;
        }

        public override void Draw(Graphics g)
        {
            if (Player != null)
            {
                GraphicsContainer container = g.BeginContainer();
                g.TranslateTransform(Location.X + Size.Width / 2, Location.Y + Size.Height / 2);
                if (rotateType != RotateType.None)
                    g.RotateTransform(Angle(rotateType));
                g.TranslateTransform(-(Location.X + Size.Width / 2), -(Location.Y + Size.Height / 2));
                if (isHovering && OnClick != null)
                    g.FillRectangle(BackgroundBrush, DrawLocation);
                DrawPlayerName(g);
                notHitler?.Draw(g);
                if (Player.Dead)
                    g.DrawImageUnscaled(DEATH, new Point(Location.X + (Size.Width / 2) - 50, Location.Y));
                DrawPlayerHand(g);
                votedCard?.Draw(g);
                if (pickPolicyCards != null)
                    for (var i = 0; i < pickPolicyCards.Length; i++)
                        pickPolicyCards[i].Draw(g);
                g.EndContainer(container);
            }
        }
        private void DefineLocations(PlayerHand hand)
        {
            int yLoc = Location.Y + Size.Height - Card.DEFAULTCARDSIZE.Height;
            int xSpacing = (Size.Width - Card.DEFAULTCARDSIZE.Width * 4) / 3 + Card.DEFAULTCARDSIZE.Width;
            hand.Membership.Location = new Point(Location.X, yLoc);
            hand.Role.Location = new Point(Location.X + xSpacing, yLoc);
            hand.Yes.Location = new Point(Location.X + xSpacing * 2, yLoc);
            hand.No.Location = new Point(Location.X + xSpacing * 3, yLoc);

            CurrentPlacardLoc = new Point(Location.X, Location.Y);
            PreviousPlacardLoc = new Point(Location.X + Size.Width - Placard.DEFAULTSIZE.Width, Location.Y);
            if (ID <= 3)
                rotateType = RotateType.None;
            else if (ID <= 7)
                rotateType = RotateType.Half;
            else if (ID == 8)
                rotateType = RotateType.Left;
            else //9
                rotateType = RotateType.Right;

        }

        internal void PickPolicyCard(Card[] cards, Action<Card> callback = null)
        {
            var xSpacing = 15 * (cards.Length - 1);
            var totalXSpace = cards.Length * Card.DEFAULTCARDSIZE.Width + xSpacing;
            var xOffset = (DEFAULTSIZE.Width / 2 - totalXSpace / 2);
            for (var i = 0; i < cards.Length; i++)
            {
                cards[i].Location = new Point(Location.X + xOffset + i * (Card.DEFAULTCARDSIZE.Width + 15), Location.Y);
                Action<Card> onClick;
                if (callback == null)
                    onClick = PickPolicyCard;
                else
                    onClick = (Card card) => { pickPolicyCards = null; callback(card); };
                cards[i].OnClick += onClick;
            }
            pickPolicyCards = cards;
        }
        public void RebindPolicyCards()
        {
            foreach (var card in pickPolicyCards)
                card.OnClick += PickPolicyCard;
        }
        internal void RemovePolicyCards()
        {
            pickPolicyCards = null;
        }
        private void PickPolicyCard(Card obj)
        {
            var policyCards = pickPolicyCards;
            for (var i = 0; i < policyCards.Length; i++)
                if(obj == policyCards[i])
                {
                    if (policyCards[i].GetType() == typeof(CardPolicyVeto))
                    {
                        var j = 0;
                        pickPolicyCards = new CardPolicy[policyCards.Length - 1];
                        for (var i2 = 0; i2 < policyCards.Length; i2++)
                            if (i2 != i)
                            {
                                pickPolicyCards[j++] = policyCards[i2];
                                policyCards[i2].OnClick -= PickPolicyCard;
                            }
                        State.RequestVeto();
                    }
                    else
                    {
                        State.ReturnPolicyCards(i);
                        pickPolicyCards = null;
                    }
                }
        }

        internal void ResetState()
        {
            OnClick = null;
        }

        private int Angle(RotateType type, bool reverse = false)
            => rotateType == RotateType.None ? 0 : (rotateType == RotateType.Left ? (reverse ? 270 : 90) : (rotateType == RotateType.Half ? 180 : (reverse ? 90 : 270)));

        private Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        private Rectangle Rotate(Rectangle r, RotateType type)
        {

            if (type == RotateType.None || type == RotateType.Half) return r;
            var offset = r.Size.Height / 2 - r.Size.Width / 2;
            return new Rectangle(
                r.Location.X - offset,
                r.Location.Y + offset,
                r.Size.Height,
                r.Size.Width
            );
        }
        internal void Click(Point point, bool isLeftClick)
        {
            var hand = Player?.Hand;
            if (hand != null)
            {
                var clickPoint = point;
                if (rotateType != RotateType.None)
                {
                    var p = RotatePoint(point.RelativeTo(Location), new Point(Size.Width / 2, Size.Height / 2), Angle(rotateType, reverse: true));
                    clickPoint = new Point(Location.X + p.X, Location.Y + p.Y);
                }
                var cards = new List<Card> { hand.Membership, hand.Role, hand.Yes, hand.No };
                if (pickPolicyCards != null)
                    cards.AddRange(pickPolicyCards);
                foreach (var card in cards)
                    if (new Rectangle(card.Location, Card.DEFAULTCARDSIZE).IsPointIn(clickPoint))
                    {
                        if (!isLeftClick)
                            card.Flipped = !card.Flipped;
                        card.Clicked();

                    }
            }
            OnClick?.Invoke(this);
        }

        public void OnHover() => isHovering = true;
        public void OnHoverLeave() => isHovering = false;
    }
}
