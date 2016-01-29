using SecretHitler.Logic;
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
        public static readonly Size DEFAULTSIZE = new Size(276, 200);
        protected GameState state;
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

        public override void Draw(Graphics g)
        {
            g.DrawRectangle(Pens.Tomato, DrawLocation);
            g.DrawString(
                $"Player {ID}: {(state.SeatedPlayers[ID-1] == null ? "Unoccupied" : state.SeatedPlayers[ID-1].Name)}",
                SystemFonts.DefaultFont,
                Brushes.Black,
                Location);
        }
    }
    public class PlayAreaVertical : PlayArea
    {
        public PlayAreaVertical(GameState state, int ID)
            : base(state, ID)
        { }
        public override Size Size
        {
            get { return new Size(base.Size.Height, base.Size.Width);}
        }
    }
}
