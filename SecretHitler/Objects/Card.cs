﻿using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Objects
{
    public abstract class Card : GameObject
    {
        public static readonly Size DEFAULTCARDSIZE = new Size(72, 100);
        public abstract Bitmap Front { get; }
        public abstract Bitmap Back { get; }
        public bool Flipped { get; set; }
        public event Action<Card> OnClick;
        public sealed override Size Size
        {
            get { return DEFAULTCARDSIZE; }
            set { throw new InvalidOperationException("Card size may not be adjusted"); }
        }
        public sealed override void Draw(Graphics g)
        {
            if(OnClick != null)
                g.FillRectangle(PlayArea.BackgroundBrush, Location.X - 2, Location.Y - 2, Size.Width + 4, Size.Height + 4);
            g.DrawImageUnscaled((Flipped ? Back : Front), DrawLocation);
        }

        internal void Clicked()
        {
            OnClick?.Invoke(this);
        }
    }
}
