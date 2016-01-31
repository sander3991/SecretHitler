using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    abstract class Board : GameObject
    {
        protected static readonly Size DEFAULTSIZE = new Size(600, 214);
        protected abstract Image Picture { get; }
        public override void Draw(Graphics g, BitmapRotateType rotate = BitmapRotateType.None)
            => g.DrawImageUnscaled(Picture, DrawLocation);
    }
    class FascistBoard : Board
    {
        private static Image[] boards = new Image[]
        {
            Properties.Resources.fascist_board_56_2color_border_final.CutToSize(DEFAULTSIZE),
            Properties.Resources.fascist_board_78_2color_border_final.CutToSize(DEFAULTSIZE),
            Properties.Resources.fascist_board_910_2color_border_final.CutToSize(DEFAULTSIZE)
        };
        protected override Image Picture
        {
            get { return boards[boardid]; }
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
        private static Image board = Properties.Resources.liberal_board_2colors_border_final.CutToSize(DEFAULTSIZE);
        protected override Image Picture { get { return board; } }
    }
}
