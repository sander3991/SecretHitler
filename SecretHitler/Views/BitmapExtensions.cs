using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Views
{
    public static class ImageExtensions
    {
        public static Bitmap CutToSize(this Image image, Size size) => new Bitmap(image, size);
        public static Bitmap FromSprite(this Image image, Size size, int spriteWidth, int spriteHeight, int index)
        {
            var bitmap = new Bitmap(size.Width, size.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                int x = index % spriteWidth;
                int y = index / spriteWidth;
                int width = image.Width / spriteWidth;
                int height = image.Height / spriteHeight;
                var destRect = new Rectangle(0, 0, size.Width, size.Height);
                var srcRect = new Rectangle(width * x, height * y, width, height);
                graphics.DrawImage(image, destRect: destRect, srcRect: srcRect, srcUnit: GraphicsUnit.Pixel);
            }
            return bitmap;
        }


        public static bool IsPointIn(this Rectangle r, Point p)
            => p.X >= r.X && p.X <= (r.X + r.Width) && p.Y >= r.Y && p.Y <= (r.Y + r.Height);
        public static Point RelativeTo(this Point point, Point relativeTo)
            => new Point(point.X - relativeTo.X, point.Y - relativeTo.Y);
        public static Size Rotate(this Size rect)
            => new Size(rect.Height, rect.Width);
    }
}
