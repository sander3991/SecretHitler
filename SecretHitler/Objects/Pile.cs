using SecretHitler.Views;
using System;
using System.Drawing;

namespace SecretHitler.Objects
{
    abstract class Pile : GameObject
    {
        public static readonly Size DEFAULTPILESIZE = new Size(80, 120);
        public abstract Bitmap Image { get; }
        public override Size Size
        {
            get
            {
                return DEFAULTPILESIZE;
            }

            set
            {
                throw new InvalidOperationException("You cannot change the Size of a pile");
            }
        }

        public override void Draw(Graphics g)
            => g.DrawImageUnscaled(Image, DrawLocation);
    }
    class PileDiscard : Pile
    {
        private static Bitmap img = Properties.Resources.discard_pile.CutToSize(DEFAULTPILESIZE);
        public override Bitmap Image
        {
            get
            {
                return img;
            }
        }
    }
    class PileDraw : Pile
    {
        private static Bitmap img = Properties.Resources.draw_pile.CutToSize(DEFAULTPILESIZE);
        public override Bitmap Image
        {
            get
            {
                return img;
            }
        }
    }
}