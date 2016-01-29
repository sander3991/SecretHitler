namespace SecretHitler.Views
{
    partial class Game
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chat1 = new SecretHitler.Views.Chat();
            this.gamePanel1 = new SecretHitler.Views.GamePanel();
            this.SuspendLayout();
            // 
            // chat1
            // 
            this.chat1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chat1.Location = new System.Drawing.Point(12, 802);
            this.chat1.Name = "chat1";
            this.chat1.Size = new System.Drawing.Size(473, 171);
            this.chat1.TabIndex = 0;
            this.chat1.Load += new System.EventHandler(this.Game_Load);
            // 
            // gamePanel1
            // 
            this.gamePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gamePanel1.Location = new System.Drawing.Point(0, 0);
            this.gamePanel1.Name = "gamePanel1";
            this.gamePanel1.Size = new System.Drawing.Size(1264, 985);
            this.gamePanel1.TabIndex = 1;
            // 
            // Game
            // 
            this.ClientSize = new System.Drawing.Size(1264, 985);
            this.Controls.Add(this.chat1);
            this.Controls.Add(this.gamePanel1);
            this.Name = "Game";
            this.ResumeLayout(false);

        }

        #endregion

        private Chat chat1;
        private GamePanel gamePanel1;
    }
}

