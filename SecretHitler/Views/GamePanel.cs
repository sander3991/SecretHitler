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
        private GameState state;
        private Point mousePos;
        private IZoomable zoomable;
        public GamePanel()
        {
            InitializeComponent();
        }
        public void InitializeState(GameState state)
        {
            this.state = state;
            state.OnStart += DefineBoards;
            GeneratePlayAreas(state);
            GeneratePlacards();
        }

        private void DefineBoards(GameState obj)
        {
            var xLocation = Width / 2 - (Board.DEFAULTSIZE.Width / 2);
            lock (Objects)
            {
                Objects.AddFirst(new LiberalBoard() { Location = new Point(xLocation, 200) });
                Objects.AddFirst(new FascistBoard(obj.PlayerCount) { Location = new Point(xLocation, 200 + Board.DEFAULTSIZE.Height) });
            }
        }

        private void GeneratePlayAreas(GameState state)
        {
            int horizontalSpacing = Width / 4;
            int playerAreaWidth = PlayArea.DEFAULTSIZE.Width;
            int padding = (horizontalSpacing - playerAreaWidth) / 2;
            int borderBottom = 720;
            int offsetBottom = borderBottom - PlayArea.DEFAULTSIZE.Height;
            int borderTop = 0;
            int paddingSide = 10;
            int offsetTop = borderTop + PlayArea.DEFAULTSIZE.Height;
            int locationVertical = (offsetBottom - offsetTop - playerAreaWidth) / 2 + offsetTop;
            for (var i = 0; i < PlayerAreas.Length; i++)
            {
                var area = i >= 8 ? new PlayAreaVertical(state, i) : new PlayArea(state, i);
                if (i < 8)
                    area.Location = new Point((i % 4) * horizontalSpacing + padding, i / 4 == 0 ? offsetBottom : borderTop);
                else
                    area.Location = new Point(i == 8 ? paddingSide : Width - paddingSide - PlayArea.DEFAULTSIZE.Height, locationVertical);
                Objects.AddLast(area);
                PlayerAreas[i] = area;
            }
        }
        private void GeneratePlacards()
        {
            Objects.AddLast(PlayArea.PlacardChancellor);
            Objects.AddLast(PlayArea.PlacardPresident);
            Objects.AddLast(PlayArea.PlacardPrevPresident);
            Objects.AddLast(PlayArea.PlacardPrevChancellor);
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
            lock (Objects)
                foreach (var obj in Objects)
                    obj.Draw(g);
            DrawZoom(g);
        }

        private void DrawZoom(Graphics g)
        {
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
                    lock(lastBitmap)
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
                if (area.DrawLocation.IsPointIn(e.Location))
                    area.OnClick(e.Location, e.Button == MouseButtons.Left);
        }

        private void OnMouseHover(object sender, EventArgs e)
        {
            Console.WriteLine($"X: {mousePos.X} Y: {mousePos.Y}");

        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            mousePos = e.Location;
            foreach (var obj in Objects)
                if (obj.DrawLocation.IsPointIn(mousePos) && obj is IZoomable)
                {
                    if (zoomable == obj)
                        return;
                    zoomable = obj as IZoomable;
                    if (lastBitmap != null)
                        lock(lastBitmap)
                            lastBitmap = null;
                    Console.WriteLine($"{obj.GetType().Name} is now zoomable");
                    return;
                }
            zoomable = null;
        }
    }
}
