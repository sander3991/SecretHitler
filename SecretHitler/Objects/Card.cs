using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public abstract class Card : GameObject
    {
        public static readonly Size DEFAULTCARDSIZE = new Size(72, 100);
        public abstract Image Front { get; }
        public abstract Image Back { get; }
        public bool Flipped { get; set; }
        public sealed override Size Size
        {
            get { return DEFAULTCARDSIZE; }
            set { throw new InvalidOperationException("Card size may not be adjusted"); }
        }
        public sealed override void Draw(Graphics g)
            => g.DrawImageUnscaled(Flipped ? Back : Front, DrawLocation);
    }
}
