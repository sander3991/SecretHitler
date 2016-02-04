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
        private static readonly Brush BalloonBrush = new SolidBrush(Color.White);
        private static readonly Pen BalloonBorderPen = new Pen(Color.Black);
        private static readonly Brush Brush = new SolidBrush(Color.Black);
        private static readonly Font Font = new Font("Calibri", 18);
        public string Text { get; private set; }
        public Player Player { get; private set; }

        public TextObject(string str, Player player)
        {
            Player = player;
            Text = str;
            DetonateTimer = new TimeSpan(0, 0, 0, 2, 500);
        }
        private void CutupString(Graphics g)
        {
            var split = Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for(var i = 0; i < split.Length; i++)
            {
                SizeF rect;
                while ((rect = g.MeasureString(split[i], Font)).Width > PlayArea.DEFAULTSIZE.Width)
                {
                    for(var j = split[i].Length / 2; j < split[i].Length; j++)
                    {
                        if (split[i][j] == ' ')
                        {
                            split[i] = $"{split[i].Substring(0, j)}{Environment.NewLine}{split[i].Substring(j + 1, split[i].Length - j - 1)}";
                            break;
                        }
                    }
                }
            }
            Text = string.Join(Environment.NewLine, split);
        }
        public override void Draw(Graphics g)
        {
            const int paddingTop = 5;
            const int paddingSide = 5;
            Size rect;
            while ((rect = Size.Round(g.MeasureString(Text, Font))).Width > PlayArea.DEFAULTSIZE.Width)
                CutupString(g);
            //draw top, bottom and middle in 1 rectangle
            g.FillRectangle(BalloonBrush, Location.X, Location.Y - paddingTop, rect.Width, rect.Height + paddingTop * 2);
            g.DrawLine(BalloonBorderPen, Location.X, Location.Y - paddingTop, Location.X + rect.Width, Location.Y - paddingTop);
            g.DrawLine(BalloonBorderPen, Location.X, Location.Y + rect.Height + paddingTop, Location.X + rect.Width, Location.Y + rect.Height + paddingTop);

            //draw left
            g.FillRectangle(BalloonBrush, Location.X - paddingSide, Location.Y, paddingSide, rect.Height);
            g.DrawLine(BalloonBorderPen, Location.X - paddingSide, Location.Y, Location.X - paddingSide, Location.Y + rect.Height);
            //draw right
            g.FillRectangle(BalloonBrush, Location.X + rect.Width, Location.Y, paddingSide, rect.Height);
            g.DrawLine(BalloonBorderPen, Location.X + rect.Width + paddingSide, Location.Y, Location.X + rect.Width + paddingSide, Location.Y + rect.Height);
            //draw rounded TopLeft corner
            var topLeft = new Rectangle(Location.X - paddingSide, Location.Y - paddingTop, paddingSide * 2, paddingTop * 2);
            g.FillPie(BalloonBrush, topLeft, 180, 90);
            g.DrawArc(BalloonBorderPen, topLeft, 180, 90);
            //draw rounded BottomLeft corner
            var bottomLeft = new Rectangle(Location.X - paddingSide, Location.Y + rect.Height - paddingTop, topLeft.Width, topLeft.Height);
            g.FillPie(BalloonBrush, bottomLeft, 90, 90);
            g.DrawArc(BalloonBorderPen, bottomLeft, 90, 90);
            //draw rounded TopRight corner
            var topRight = new Rectangle(Location.X + rect.Width - paddingSide, Location.Y - paddingTop, paddingSide * 2, paddingTop * 2);
            g.FillPie(BalloonBrush, topRight, 270, 90);
            g.DrawArc(BalloonBorderPen, topRight, 270, 90);
            //draw rounded BottomRight corner
            var bottomRight = new Rectangle(Location.X + rect.Width - paddingSide, Location.Y + rect.Height - paddingTop, paddingSide * 2, paddingTop * 2);
            g.FillPie(BalloonBrush, bottomRight, 0, 90);
            g.DrawArc(BalloonBorderPen, bottomRight, 0, 90);
            //draw string in balloon
            g.DrawString(Text, Font, Brush, Location);
        }
    }
}
