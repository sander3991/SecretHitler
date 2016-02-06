using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SecretHitler.Objects;
using SecretHitler.Logic;
using SecretHitler.Views;

namespace SecretHitler.Views
{
    public partial class GamePanel : UserControl
    {
        public LinkedList<GameObject> Objects { get; private set; } = new LinkedList<GameObject>();
        public PlayArea[] PlayerAreas { get; } = new PlayArea[10];
        public FascistBoard FascistBoard { get; private set; }
        public LiberalBoard LiberalBoard { get; private set; }
        private ClientGameState state;
        private Point mousePos;
        private IZoomable zoomable;
        private IHooverable hover;
        public GamePanel()
        {
            InitializeComponent();
        }
        public void InitializeState(ClientGameState state)
        {
            this.state = state;
            state.OnStart += DefineBoards;
            GeneratePlayAreas(state);
            GeneratePlacards();
        }

        private void DefineBoards(ClientGameState obj)
        {
            if (FascistBoard != null)
                lock (Objects)
                    Objects.Remove(FascistBoard);
            var xLocation = Width / 2;
            var yLocation = Height / 2 - (Board.DEFAULTSIZE.Height / 2);
            FascistBoard = new FascistBoard(obj, obj.PlayerCount) { Location = new Point(xLocation, yLocation) };
            lock (Objects)
            {
                if (LiberalBoard == null)
                {
                    LiberalBoard = new LiberalBoard(obj) { Location = new Point(xLocation - Board.DEFAULTSIZE.Width, yLocation) };
                    Objects.AddFirst(LiberalBoard);
                }
                Objects.AddFirst(FascistBoard);
            }
        }

        internal void InitializePiles(Deck<CardPolicy> drawPile, Deck<CardPolicy> discardPile)
        {
            var middleX = Width / 2;
            var yLocation = Height / 2 - (Board.DEFAULTSIZE.Height / 2) + 20;
            var boardSize = Board.DEFAULTSIZE;
            drawPile.Location = new Point(middleX - boardSize.Width - Card.DEFAULTCARDSIZE.Width - 20, yLocation);
            discardPile.Location = new Point(middleX + boardSize.Width + 20, yLocation);
            var drawSize = Deck<Card>.DRAWPILESIZE;
            var offset = new Point(-(drawSize.Width - Card.DEFAULTCARDSIZE.Width) / 2, -(drawSize.Height - Card.DEFAULTCARDSIZE.Height) / 2 + 5);
            drawPile.SetBackground(Properties.Resources.draw_pile.CutToSize(drawSize), offset);
            discardPile.SetBackground(Properties.Resources.discard_pile.CutToSize(drawSize), offset);
            lock (Objects)
            {
                Objects.AddFirst(drawPile);
                Objects.AddFirst(discardPile);
            }
        }

        private void GeneratePlayAreas(ClientGameState state)
        {
            int horizontalSpacing = Width / 4;
            int playerAreaWidth = PlayArea.DEFAULTSIZE.Width;
            int padding = (horizontalSpacing - playerAreaWidth) / 2;
            int borderBottom = 720;
            int offsetBottom = borderBottom - PlayArea.DEFAULTSIZE.Height;
            int borderTop = 0;
            int paddingSide = 10;
            int offsetTop = borderTop + PlayArea.DEFAULTSIZE.Height;
            int locationVertical = (offsetBottom - offsetTop - playerAreaWidth) / 2 + offsetTop + 60;
            for (var i = 0; i < PlayerAreas.Length; i++)
            {
                var area = new PlayArea(state, i);
                if (i < 8)
                    area.Location = new Point((i % 4) * horizontalSpacing + padding, i / 4 == 0 ? offsetBottom : borderTop);
                else
                    area.Location = new Point(i == 8 ? -50 : Width - paddingSide - PlayArea.DEFAULTSIZE.Height - 50, locationVertical);
                Objects.AddLast(area);
                PlayerAreas[i] = area;
            }
        }

        private void GeneratePiles()
        {

        }
        private void GeneratePlacards()
        {
            /*Objects.AddLast(PlayArea.PlacardChancellor);
            Objects.AddLast(PlayArea.PlacardPresident);
            Objects.AddLast(PlayArea.PlacardPrevPresident);
            Objects.AddLast(PlayArea.PlacardPrevChancellor);*/
            PlayArea.PlacardPresident.Location = new Point(Placard.DEFAULTSIZE.Width, 0);
            PlayArea.PlacardPrevChancellor.Location = new Point(Placard.DEFAULTSIZE.Width * 2, 0);
            PlayArea.PlacardPrevPresident.Location = new Point(Placard.DEFAULTSIZE.Width * 3, 0);
        }
        private Tuple<DateTime, Bitmap> lastBitmap;
        private bool generating = false;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            List<GameObject> rem = null;
            lock (Objects)
            {

                foreach (var obj in Objects)
                {
                    obj.Draw(g);
                    if (obj.DetonateTimer.HasValue && (DateTime.Now - obj.StartTime) > obj.DetonateTimer.Value)
                    {
                        rem = rem ?? new List<GameObject>();
                        rem.Add(obj);
                    }
                }
                if (rem != null)
                    foreach (var obj in rem)
                    {
                        Objects.Remove(obj);
                        obj.TriggerDetonate();
                    }
            }
            DrawZoom(g);
        }

        private void DrawZoom(Graphics g)
        {
            if (ModifierKeys != Keys.Shift) return;
            using (var bitmap = new Bitmap(550, 550))
                if (zoomable?.DrawZoomedIn(bitmap, mousePos) != null)
                    g.DrawImageUnscaled(bitmap, new Point(Math.Max(mousePos.X - 200, 0), Math.Max(mousePos.Y - 200, 0)));
        }

        private void DrawZoomBackgroundWorker(Graphics g)
        {
            if (zoomable != null)
            {
                if (lastBitmap == null || (DateTime.Now - lastBitmap.Item1).TotalMilliseconds > 100)
                {
                    if (!generating)
                    {
                        generating = true;
                        BackgroundWorker worker = new BackgroundWorker();
                        worker.DoWork += (object sender, DoWorkEventArgs args) =>
                        {
                            var bitmap = new Bitmap(550, 550);
                            if (zoomable.DrawZoomedIn(bitmap, mousePos))
                            {
                                if (lastBitmap != null)
                                    lock (lastBitmap)
                                    {
                                        lastBitmap.Item2.Dispose();
                                    }
                                lastBitmap = new Tuple<DateTime, Bitmap>(DateTime.Now, bitmap);
                            }
                            generating = false;
                        };
                        worker.RunWorkerAsync();
                    }

                }
                if (lastBitmap != null)
                    lock (lastBitmap)
                        g.DrawImageUnscaled(lastBitmap.Item2, new Point(Math.Max(mousePos.X - 200, 0), Math.Max(mousePos.Y - 200, 0)));
            }

        }
        private void Redraw(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnClick(object sender, MouseEventArgs e)
        {
            foreach (var area in PlayerAreas)
                if (area.ClickLocation.IsPointIn(e.Location))
                    area.Click(e.Location, e.Button == MouseButtons.Left);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            mousePos = e.Location;
            foreach (var obj in Objects)
                if (obj.ClickLocation.IsPointIn(mousePos))
                {
                    if (obj is IZoomable)
                    {
                        if (zoomable == obj)
                            return;
                        zoomable = obj as IZoomable;
                        if (lastBitmap != null)
                            lock (lastBitmap)
                                lastBitmap = null;
                    }
                    if (obj is IHooverable)
                    {
                        if (hover == obj)
                            return;
                        hover?.OnHoverLeave();
                        hover = obj as IHooverable;
                        hover.OnHover();
                    }
                    return;
                }
            zoomable = null;
        }
    }
}
