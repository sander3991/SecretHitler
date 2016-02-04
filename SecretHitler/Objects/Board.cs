using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    abstract class Board : GameObject, IZoomable
    {
        public static readonly Size DEFAULTSIZE = new Size(450, 160);
        protected static readonly Size ZOOMSIZE = new Size(1547, 550);
        public override Size Size
        {
            get { return DEFAULTSIZE; }
            set { throw new InvalidOperationException("Can't change size of a board object"); }
        }
        protected abstract Bitmap Picture { get; }
        protected abstract Bitmap Raw { get; }
        public override void Draw(Graphics g)
            => g.DrawImageUnscaled(Picture, DrawLocation);

        public bool DrawZoomedIn(Bitmap bitmap, Point p)
        {
            Point locationOnBitmap = p.RelativeTo(Location);
            int xLoc = (int)((float)locationOnBitmap.X / DEFAULTSIZE.Width * ZOOMSIZE.Width);
            using (var g = Graphics.FromImage(bitmap))
                g.DrawImage(Raw, destRect: new Rectangle(new Point(), bitmap.Size), srcRect: new Rectangle(new Point(xLoc, 0), bitmap.Size), srcUnit: GraphicsUnit.Pixel);
            return true;
        }
    }
    class FascistBoard : Board
    {
        private static Bitmap[] raw;
        private static Bitmap[] boards;
        static FascistBoard()
        {
             var temp = new Bitmap[]
            {
                Properties.Resources.fascist_board_56_2color_border_final,
                Properties.Resources.fascist_board_78_2color_border_final,
                Properties.Resources.fascist_board_910_2color_border_final
            };
            raw = new Bitmap[temp.Length];
            boards = new Bitmap[temp.Length];
            for (var i = 0; i < temp.Length; i++)
            {
                raw[i] = temp[i].CutToSize(ZOOMSIZE);
                boards[i] = temp[i].CutToSize(DEFAULTSIZE);
                temp[i].Dispose();
            }
        }
        protected override Bitmap Picture
        {
            get { return boards[boardid]; }
        }
        protected override Bitmap Raw
        {
            get { return raw[boardid]; }
        }
        private int boardid;
        public FascistBoard(int player)
        {
            player = Math.Max(5, player);
            if (player > 10)
                throw new ArgumentException("Player count must not be higher then 10", nameof(player));
            boardid = (player - 5) / 2;
        }
    }
    class LiberalBoard : Board
    {
        private static Bitmap raw = Properties.Resources.liberal_board_2colors_border_final.CutToSize(ZOOMSIZE);
        private static Bitmap board = raw.CutToSize(DEFAULTSIZE);
        protected override Bitmap Picture { get { return board; } }
        protected override Bitmap Raw { get { return raw; } }
    }
}
