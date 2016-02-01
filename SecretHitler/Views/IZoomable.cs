using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Views
{
    interface IZoomable
    {
        bool DrawZoomedIn(Bitmap bitmap, Point p);
    }
}
