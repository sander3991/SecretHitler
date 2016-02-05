using System;
using SecretHitler.Objects;

namespace SecretHitler.Views
{
    partial class GamePanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.redraw = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // redraw
            // 
            this.redraw.Enabled = true;
            this.redraw.Interval = 5;
            this.redraw.Tick += new System.EventHandler(this.Redraw);
            // 
            // GamePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SecretHitler.Properties.Resources.background;
            this.DoubleBuffered = true;
            this.Name = "GamePanel";
            this.Size = new System.Drawing.Size(1264, 985);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer redraw;
    }
}
