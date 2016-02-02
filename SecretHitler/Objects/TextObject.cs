using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHitler.Views;
using System.Drawing.Drawing2D;
using SecretHitler.Logic;

namespace SecretHitler.Objects
{
    class TextObject : GameObject
    {
        private static readonly Brush Brush = new SolidBrush(Color.DarkGray);
        private static readonly Font Font = new Font("Calibri", 18);
        public string Text { get; private set; }
        public Player Player { get; private set; }

        public TextObject(string str, Player player)
        {
            Player = player;
            Text = str;
            DetonateTimer = new TimeSpan(0, 0, 0, 2, 500);
        }
        public override void Draw(Graphics g, BitmapRotateType rotate = BitmapRotateType.None)
        {
            /*GraphicsPath p = new GraphicsPath();
            p.AddString(
                Text,
                FontFamily.GenericSansSerif,
                (int)FontStyle.Regular,
                24,
                Location,
                new StringFormat()
            );
            g.FillPath(Brushes.Black, p);
            //g.DrawPath(Pens.White, p);*/
            g.DrawString(Text, Font, Brush, Location);
        }
    }
}
