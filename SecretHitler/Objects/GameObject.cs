﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public abstract class GameObject
    {
        public RotateFlipType Rotation { get; set; } = RotateFlipType.RotateNoneFlipNone;
        public Rectangle DrawLocation { get; set; }
        public Point Location
        {
            get
            {
                return DrawLocation.Location;
            }
            set
            {
                DrawLocation = new Rectangle(value, Size);
            }
        }
        public virtual Size Size
        {
            get { return DrawLocation.Size; }
            set { DrawLocation = new Rectangle(Location, value); }
        }
        public abstract void Draw(Graphics g);
    }
}
