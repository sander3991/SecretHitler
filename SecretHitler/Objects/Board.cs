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
    public abstract class Board : GameObject
    {
        public static readonly Size DEFAULTSIZE = new Size(450, 160);
        protected static readonly Size CARDSIZE = new Size(54, 75);
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
    public abstract class Board<T> : Board, IZoomable where T : Card
    {
        protected ClientGameState gameState;
        protected Board(ClientGameState gameState)
        {
            this.gameState = gameState;
        }
        public virtual void AddCard(T card)
        {
            const int defaultSpacing = 8;
            card.Location = new Point(GetXOffset() + (CARDSIZE.Width + defaultSpacing) * (GetTotalCount() - 1), Location.Y + (int)(DEFAULTSIZE.Height / 4.25) + 8);
        }
        protected abstract int GetXOffset();
        protected abstract int GetTotalCount();
    }
    public class FascistBoard : Board<CardPolicyFascist>
    {
        private static Bitmap[] raw;
        private static Bitmap[] boards;
        private static Bitmap FascistCardCutOut;
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
        public FascistBoard(ClientGameState gameState, int player)
            : base(gameState)
        {
            player = Math.Max(5, player);
            if (player > 10)
                throw new ArgumentException("Player count must not be higher then 10", nameof(player));
            boardid = (player - 5) / 2;
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);
            var fascistCards = gameState.PlayedFascistCards;
            if (FascistCardCutOut != null)
                for (var i = 0; i < fascistCards.Length && fascistCards[i] != null; i++)
                    g.DrawImageUnscaled(FascistCardCutOut, fascistCards[i].Location);
        }
        public override void AddCard(CardPolicyFascist card)
        {
            base.AddCard(card);
            if (FascistCardCutOut == null)
                FascistCardCutOut = card.Front.CutToSize(CARDSIZE);
        }
        protected override int GetXOffset() => Location.X + DEFAULTSIZE.Width / 11 + 5;
        protected override int GetTotalCount() => gameState.FascistsCardsPlayed;

    }
    public class LiberalBoard : Board<CardPolicyLiberal>
    {
        private static Bitmap raw = Properties.Resources.liberal_board_2colors_border_final.CutToSize(ZOOMSIZE);
        private static Bitmap board = raw.CutToSize(DEFAULTSIZE);
        private static Bitmap LiberalCardCutout;
        protected override Bitmap Picture { get { return board; } }
        protected override Bitmap Raw { get { return raw; } }

        public LiberalBoard(ClientGameState gameState)
            : base(gameState)
        { }
        public override void Draw(Graphics g)
        {
            base.Draw(g);
            var liberalCards = gameState.PlayedLiberalCards;
            if (LiberalCardCutout != null)
                for (var i = 0; i < liberalCards.Length && liberalCards[i] != null; i++)
                    g.DrawImage(LiberalCardCutout, liberalCards[i].Location);
            { //election tracker
                int yOffset = 35;
                int xOffset = 150;
                int spacing = 42;
                g.FillEllipse(Brushes.Yellow, Location.X + xOffset + gameState.ElectionTracker * spacing, Location.Y + Size.Height - yOffset, 15, 15);
            }
            g.DrawString(gameState.ElectionTracker.ToString(), SystemFonts.DefaultFont, Brushes.Black, Location.X, Location.Y);
        }

        public override void AddCard(CardPolicyLiberal card)
        {
            base.AddCard(card);
            if (LiberalCardCutout == null)
                LiberalCardCutout = card.Front.CutToSize(CARDSIZE);
        }

        protected override int GetXOffset() => Location.X + (int)(DEFAULTSIZE.Width / 6.5) + 5;

        protected override int GetTotalCount() => gameState.LiberalCardsPlayed;
    }
}
