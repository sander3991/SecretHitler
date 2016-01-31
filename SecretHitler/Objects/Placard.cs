using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    abstract class Placard : GameObject
    {
        protected static readonly Size DEFAULTSIZE = new Size(140, 48);
        public override Size Size
        {
            get { return DEFAULTSIZE; }
            set { throw new InvalidOperationException("You cannot change the size of a Placard"); }
        }
        public abstract Image Picture { get; }
        public override void Draw(Graphics g, BitmapRotateType type = BitmapRotateType.None)
        {
            g.DrawImageUnscaled(Picture, DrawLocation);
        }
    }
    class PlacardChancellor : Placard
    {
        private static Image PICTURE = Properties.Resources.tex_placard_chancellor.CutToSize(DEFAULTSIZE);
        public override Image Picture
        {
            get { return PICTURE; }
        }
    }
    class PlacardPresident : Placard
    {
        private static Image PICTURE = Properties.Resources.tex_placard_president.CutToSize(DEFAULTSIZE);
        public override Image Picture
        {
            get { return PICTURE; }
        }
    }
    class PlacardPrevChancellor : Placard
    {
        private static Image PICTURE = Properties.Resources.tex_quick_rules_chancellor_previously_elected.CutToSize(DEFAULTSIZE);
        public override Image Picture
        {
            get { return PICTURE; }
        }
    }
    class PlacardPrevPresident : Placard
    {
        private static Image PICTURE = Properties.Resources.tex_quick_rules_president_previously_elected.CutToSize(DEFAULTSIZE);
        public override Image Picture
        {
            get { return PICTURE; }
        }
    }
}
