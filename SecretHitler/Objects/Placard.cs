using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public abstract class Placard : GameObject
    {
        public static readonly Size DEFAULTSIZE = new Size(140, 48);
        public override Size Size
        {
            get { return DEFAULTSIZE; }
            set { throw new InvalidOperationException("You cannot change the size of a Placard"); }
        }
        public abstract Bitmap Picture { get; }
        public abstract Point DefaultLoc { get; }
        public BitmapRotateType RotateType { get; set; }
        public override void Draw(Graphics g, BitmapRotateType type = BitmapRotateType.None)
        {
            g.DrawImageUnscaled(Picture.GetRotatedBitmap(RotateType), DrawLocation);
        }
    }
    public class PlacardChancellor : Placard
    {
        public override Point DefaultLoc { get { return new Point(0, 0); } }
        private static Bitmap PICTURE = Properties.Resources.tex_placard_chancellor.CutToSize(DEFAULTSIZE);
        public override Bitmap Picture
        {
            get { return PICTURE; }
        }
    }
    public class PlacardPresident : Placard
    {
        public override Point DefaultLoc { get { return new Point(DEFAULTSIZE.Width, 0); } }
        private static Bitmap PICTURE = Properties.Resources.tex_placard_president.CutToSize(DEFAULTSIZE);
        public override Bitmap Picture
        {
            get { return PICTURE; }
        }
    }
    public class PlacardPrevChancellor : Placard
    {
        public override Point DefaultLoc { get { return new Point(DEFAULTSIZE.Width * 2, 0); } }
        private static Bitmap PICTURE = Properties.Resources.tex_quick_rules_chancellor_previously_elected.CutToSize(DEFAULTSIZE);
        public override Bitmap Picture
        {
            get { return PICTURE; }
        }
    }
    public class PlacardPrevPresident : Placard
    {
        public override Point DefaultLoc { get { return new Point(DEFAULTSIZE.Width * 3, 0); } }
        private static Bitmap PICTURE = Properties.Resources.tex_quick_rules_president_previously_elected.CutToSize(DEFAULTSIZE);
        public override Bitmap Picture
        {
            get { return PICTURE; }
        }
    }
}
