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
        public static readonly Size DEFAULTSIZE = new Size(320, 200);
        private RotateType rotateType;
        protected GameState state;
        protected Player Player { get { return state.SeatedPlayers[ID]; } }
        public int ID { get; private set; }
        public Point PreviousPlacardLoc { get; private set; }
        public Point CurrentPlacardLoc { get; private set; }
        public override Rectangle ClickLocation { get { return Rotate(DrawLocation, rotateType); } }
        public static PlacardChancellor PlacardChancellor = new PlacardChancellor();
        public static PlacardPrevChancellor PlacardPrevChancellor = new PlacardPrevChancellor();
        public static PlacardPresident PlacardPresident = new PlacardPresident();
        public static PlacardPrevPresident PlacardPrevPresident = new PlacardPrevPresident();
        private Placard current;
        private Placard previous;
        public Placard Current
        {
            get { return current; }
            set { current = value; if(current != null) current.Location = Location; }
        }
        public Placard Previous
        {
            get { return previous; }
            set { previous = value; if(previous != null) previous.Location = new Point(Location.X + DEFAULTSIZE.Width - Placard.DEFAULTSIZE.Width, Location.Y); }
        }
        private bool isHovering = false;
        public event Action<PlayArea> OnClick;
        private static Brush fontBrush = new SolidBrush(Color.FromArgb(67, 122, 87));
        private static Brush backgroundBrush = new SolidBrush(Color.FromArgb(101, 150, 119));
        private Font playerNameFont;

        public override Size Size
        {
            get { return DEFAULTSIZE; }
            set { throw new InvalidOperationException("Can't set size"); }
        }

        public PlayArea(GameState state, int ID)
        {
            this.state = state;
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
                    g.FillRectangle(backgroundBrush, DrawLocation);
                DrawPlayerName(g);
                DrawPlayerHand(g);
                g.EndContainer(container);
            }
        }
        private void DefineLocations(PlayerHand hand)
        {
            int yLoc = Location.Y + Size.Height - Card.DEFAULTCARDSIZE.Height;
            int xSpacing  = (Size.Width - Card.DEFAULTCARDSIZE.Width * 4) / 3 + Card.DEFAULTCARDSIZE.Width;
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
            var offset = r.Size.Height / 2  - r.Size.Width / 2;
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
                foreach (var card in new Card[] { hand.Membership, hand.Role, hand.Yes, hand.No })
                    if (new Rectangle(card.Location, Card.DEFAULTCARDSIZE).IsPointIn(clickPoint))
                    {
                        if (!isLeftClick)
                            card.Flipped = !card.Flipped;

                    }
            }
            OnClick?.Invoke(this);
        }

        public void OnHover() => isHovering = true;
        public void OnHoverLeave() => isHovering = false;
    }
}
