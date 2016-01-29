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

namespace SecretHitler.Views
{
    public partial class GamePanel : UserControl
    {
        private int counter = 0;
        public LinkedList<GameObject> Objects { get; private set; } = new LinkedList<GameObject>();
        public PlayArea[] PlayerAreas { get; } = new PlayArea[10];
        private GameState state;
        public GamePanel()
        {
            InitializeComponent();
        }
        public void InitializeState(GameState state)
        {
            this.state = state;
            GeneratePlayAreas(state);
        }
        private void GeneratePlayAreas(GameState state)
        {
            int horizontalSpacing = Width / 4;
            int playerAreaWidth = PlayArea.DEFAULTSIZE.Width;
            int padding = (horizontalSpacing - playerAreaWidth) / 2;
            int borderBottom = 780;
            int offsetBottom = borderBottom - PlayArea.DEFAULTSIZE.Height;
            int borderTop = 50;
            int paddingSide = 10;
            int offsetTop = borderTop + PlayArea.DEFAULTSIZE.Height;
            int locationVertical = (offsetBottom - offsetTop - playerAreaWidth) / 2 + offsetTop;
            for (var i = 0; i < PlayerAreas.Length; i++)
            {
                var area = i >= 8 ? new PlayAreaVertical(state, i + 1) : new PlayArea(state, i + 1);
                if (i < 8)
                    area.Location = new Point((i % 4) * horizontalSpacing + padding, i / 4 == 0 ? offsetBottom : 50);
                else
                    area.Location = new Point(i == 8 ? paddingSide : Width - paddingSide - PlayArea.DEFAULTSIZE.Height, locationVertical);
                area.Rotation = i < 4 ? RotateFlipType.RotateNoneFlipNone : (i < 8 ? RotateFlipType.Rotate180FlipNone : (i == 8 ? RotateFlipType.Rotate90FlipNone : RotateFlipType.Rotate270FlipNone));
                Objects.AddLast(area);
                PlayerAreas[i] = area;
            }

        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            foreach (var obj in Objects)
                obj.Draw(g);
        }
        private void Redraw(object sender, EventArgs e)
        {
            Invalidate();
            if (counter++ % 50 == 0)
                foreach (var go in Objects)
                {
                    if (go is Card)
                    {
                        var card = go as Card;
                        card.Flipped = !card.Flipped;
                    }
                }
        }
    }
}
