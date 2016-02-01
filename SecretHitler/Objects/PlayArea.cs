using SecretHitler.Logic;
using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public class PlayArea : GameObject
    {
        public static readonly Size DEFAULTSIZE = new Size(288, 200);
        public BitmapRotateType RotateType { get; private set; }
        protected GameState state;
        protected Player Player { get { return state.SeatedPlayers[ID]; } }
        protected int ID;
        public Point PreviousPlacardLoc { get; private set; }
        public Point CurrentPlacardLoc { get; private set; }

        public static PlacardChancellor PlacardChancellor = new PlacardChancellor();
        public static PlacardPrevChancellor PlacardPrevChancellor = new PlacardPrevChancellor();
        public static PlacardPresident PlacardPresident = new PlacardPresident();
        public static PlacardPrevPresident PlacardPrevPresident = new PlacardPrevPresident();

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

        public override void Draw(Graphics g, BitmapRotateType type = BitmapRotateType.None)
        {
            g.DrawRectangle(Pens.Tomato, DrawLocation);
            g.DrawString(
                $"Player {ID + 1}: {(Player == null ? "Unoccupied" : Player.Name)}",
                SystemFonts.DefaultFont,
                Brushes.Black,
                Location);
            if(Player != null)
            {
                var hand = Player.Hand;
                if (hand != null)
                {
                    if (hand.Membership.Location == new Point(0, 0))
                        DefineLocations(hand);
                    hand.Membership.Draw(g, RotateType);
                    hand.Role.Draw(g, RotateType);
                    hand.Yes.Draw(g, RotateType);
                    hand.No.Draw(g, RotateType);
                }
            }
        }
        private void DefineLocations(PlayerHand hand)
        {
            if (ID <= 3)
            {
                int yLoc = Location.Y + Size.Height - Card.DEFAULTCARDSIZE.Height;
                hand.Membership.Location = new Point(Location.X, yLoc);
                hand.Role.Location = new Point(Location.X + Card.DEFAULTCARDSIZE.Width, yLoc);
                hand.Yes.Location = new Point(Location.X + Card.DEFAULTCARDSIZE.Width * 2, yLoc);
                hand.No.Location = new Point(Location.X + Card.DEFAULTCARDSIZE.Width * 3, yLoc);

                CurrentPlacardLoc = new Point(Location.X, Location.Y);
                PreviousPlacardLoc = new Point(Location.X + Size.Width - Placard.DEFAULTSIZE.Width, Location.Y);

                RotateType = BitmapRotateType.None;
            }
            else if (ID <= 7)
            {
                hand.Membership.Location = new Point(Location.X + Card.DEFAULTCARDSIZE.Width * 3, Location.Y);
                hand.Role.Location = new Point(Location.X + Card.DEFAULTCARDSIZE.Width * 2, Location.Y);
                hand.Yes.Location = new Point(Location.X + Card.DEFAULTCARDSIZE.Width, Location.Y);
                hand.No.Location = new Point(Location.X, Location.Y);

                var placardYLoc = Location.Y + Size.Height - Placard.DEFAULTSIZE.Height;
                CurrentPlacardLoc = new Point(Location.X + Size.Width - Placard.DEFAULTSIZE.Width, placardYLoc);
                PreviousPlacardLoc = new Point(Location.X, placardYLoc);

                RotateType = BitmapRotateType.Half;
            }
            else if (ID == 8)
            {
                hand.Membership.Location = new Point(Location.X, Location.Y);
                hand.Role.Location = new Point(Location.X, Location.Y + Card.DEFAULTCARDSIZE.Width);
                hand.Yes.Location = new Point(Location.X, Location.Y + Card.DEFAULTCARDSIZE.Width * 2);
                hand.No.Location = new Point(Location.X, Location.Y + Card.DEFAULTCARDSIZE.Width * 3);

                var placardXLoc = Location.X + Size.Width - Placard.DEFAULTSIZE.Height;
                CurrentPlacardLoc = new Point(placardXLoc, Location.Y);
                PreviousPlacardLoc = new Point(placardXLoc, Location.Y + Size.Height - Placard.DEFAULTSIZE.Width);

                RotateType = BitmapRotateType.Left;
            }
            else //9
            {
                var xLoc = Location.X + Size.Width - Card.DEFAULTCARDSIZE.Height;
                hand.Membership.Location = new Point(xLoc, Location.Y + Card.DEFAULTCARDSIZE.Width * 3);
                hand.Role.Location = new Point(xLoc, Location.Y + Card.DEFAULTCARDSIZE.Width * 2);
                hand.Yes.Location = new Point(xLoc, Location.Y + Card.DEFAULTCARDSIZE.Width);
                hand.No.Location = new Point(xLoc, Location.Y);

                CurrentPlacardLoc = new Point(Location.X, Location.Y + Size.Height - Card.DEFAULTCARDSIZE.Width);
                PreviousPlacardLoc = new Point(Location.X, Location.Y);

                RotateType = BitmapRotateType.Right;
            }
        }

        public void OnClick(Point point, bool isLeftClick)
        {
            var hand = Player?.Hand;
            if(hand != null)
            {
                var size = ID >= 8 ? Card.DEFAULTCARDSIZE.Rotate() : Card.DEFAULTCARDSIZE;
                foreach (var card in new Card[] { hand.Membership, hand.Role, hand.Yes, hand.No })
                    if (new Rectangle(card.Location, size).IsPointIn(point))
                        if(!isLeftClick)
                            card.Flipped = !card.Flipped;
            }
        }
    }
    public class PlayAreaVertical : PlayArea
    {
        public PlayAreaVertical(GameState state, int ID)
            : base(state, ID)
        { }
        public override Size Size
        {
            get { return base.Size.Rotate(); }
        }
    }
}
