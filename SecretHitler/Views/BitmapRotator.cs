using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
namespace SecretHitler.Views
{
    public enum BitmapRotateType
    {
        None,
        Left,
        Right,
        Half
    }
    public static class BitmapRotator
    {
        private static Dictionary<Bitmap, Dictionary<BitmapRotateType, Bitmap>> rotatedBitmaps = new Dictionary<Bitmap, Dictionary<BitmapRotateType, Bitmap>>();

        public static Bitmap GetRotatedBitmap(this Bitmap map, BitmapRotateType type)
        {
            if (type == BitmapRotateType.None) return map;
            if (!rotatedBitmaps.ContainsKey(map))
                rotatedBitmaps.Add(map, new Dictionary<BitmapRotateType, Bitmap>());
            if (!rotatedBitmaps[map].ContainsKey(type))
            {
                Bitmap bitmap = type == BitmapRotateType.Half ? new Bitmap(map.Width, map.Height) : new Bitmap(map.Height, map.Width);
                Action<Bitmap, Bitmap, int, int> AddPixel;
                switch (type)
                {
                    case BitmapRotateType.Left:
                        AddPixel = TranslateLeft; break;
                    case BitmapRotateType.Right:
                        AddPixel = TranslateRight; break;
                    case BitmapRotateType.Half:
                        AddPixel = TranslateHalf; break;
                    default:
                        AddPixel = null; break;
                }
                for (var x = 0; x < map.Width; x++)
                    for (var y = 0; y < map.Height; y++)
                        AddPixel(map, bitmap, x, y);

                rotatedBitmaps[map].Add(type, bitmap);
            }
            return rotatedBitmaps[map][type];
        }
        private static void TranslateLeft(Bitmap source, Bitmap target, int x, int y)
            => target.SetPixel(source.Height - y - 1, x, source.GetPixel(x, y));

        private static void TranslateRight(Bitmap source, Bitmap target, int x, int y)
            => target.SetPixel(y, source.Width - x - 1, source.GetPixel(x, y));

        private static void TranslateHalf(Bitmap source, Bitmap target, int x, int y)
            => target.SetPixel(source.Width - x - 1, source.Height - y - 1, source.GetPixel(x, y));
    }
}
*/