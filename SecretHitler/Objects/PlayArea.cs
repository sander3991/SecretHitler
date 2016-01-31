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
        private BitmapRotateType rotateBitmap;
        protected GameState state;
        protected Player Player { get { return state.SeatedPlayers[ID]; } }
        protected int ID;
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
            var hand = Player?.Hand;
            if (hand != null)
            {
                if (hand.Membership.Location == new Point(0, 0))
                    DefineLocations(hand);
                hand.Membership.Draw(g, rotateBitmap);
                hand.Role.Draw(g, rotateBitmap);
                hand.Yes.Draw(g, rotateBitmap);
                hand.No.Draw(g, rotateBitmap);
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
                rotateBitmap = BitmapRotateType.None;
            }
            else if (ID <= 7)
            {
                hand.Membership.Location = new Point(Location.X + Card.DEFAULTCARDSIZE.Width * 3, Location.Y);
                hand.Role.Location = new Point(Location.X + Card.DEFAULTCARDSIZE.Width * 2, Location.Y);
                hand.Yes.Location = new Point(Location.X + Card.DEFAULTCARDSIZE.Width, Location.Y);
                hand.No.Location = new Point(Location.X, Location.Y);
                rotateBitmap = BitmapRotateType.Half;
            }
            else if (ID == 8)
            {
                hand.Membership.Location = new Point(Location.X, Location.Y);
                hand.Role.Location = new Point(Location.X, Location.Y + Card.DEFAULTCARDSIZE.Width);
                hand.Yes.Location = new Point(Location.X, Location.Y + Card.DEFAULTCARDSIZE.Width * 2);
                hand.No.Location = new Point(Location.X, Location.Y + Card.DEFAULTCARDSIZE.Width * 3);
                rotateBitmap = BitmapRotateType.Left;
            }
            else //9
            {
                var xLoc = Location.X + Size.Width - Card.DEFAULTCARDSIZE.Height;
                hand.Membership.Location = new Point(xLoc, Location.Y + Card.DEFAULTCARDSIZE.Width * 3);
                hand.Role.Location = new Point(xLoc, Location.Y + Card.DEFAULTCARDSIZE.Width * 2);
                hand.Yes.Location = new Point(xLoc, Location.Y + Card.DEFAULTCARDSIZE.Width);
                hand.No.Location = new Point(xLoc, Location.Y);
                rotateBitmap = BitmapRotateType.Right;
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
            get { return new Size(base.Size.Height, base.Size.Width); }
        }
    }
}
