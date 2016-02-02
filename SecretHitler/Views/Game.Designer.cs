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
            this.startBtn = new System.Windows.Forms.Button();
            this.startGameError = new System.Windows.Forms.Label();
            this.hiddenButton = new System.Windows.Forms.Button();
            this.chatBar = new SecretHitler.Views.ChatBar();
            this.gamePanel1 = new SecretHitler.Views.GamePanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // startBtn
            // 
            this.startBtn.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.startBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startBtn.Location = new System.Drawing.Point(695, 319);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(161, 61);
            this.startBtn.TabIndex = 2;
            this.startBtn.Text = "Start Game";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // startGameError
            // 
            this.startGameError.AutoSize = true;
            this.startGameError.ForeColor = System.Drawing.Color.Red;
            this.startGameError.Location = new System.Drawing.Point(794, 917);
            this.startGameError.Name = "startGameError";
            this.startGameError.Size = new System.Drawing.Size(0, 13);
            this.startGameError.TabIndex = 3;
            this.startGameError.Visible = false;
            // 
            // hiddenButton
            // 
            this.hiddenButton.Location = new System.Drawing.Point(13, 0);
            this.hiddenButton.Name = "hiddenButton";
            this.hiddenButton.Size = new System.Drawing.Size(0, 0);
            this.hiddenButton.TabIndex = 5;
            this.hiddenButton.Text = "button1";
            this.hiddenButton.UseVisualStyleBackColor = true;
            this.hiddenButton.Click += new System.EventHandler(this.OnEnterPressed);
            // 
            // chatBar
            // 
            this.chatBar.Location = new System.Drawing.Point(433, 465);
            this.chatBar.Name = "chatBar";
            this.chatBar.Size = new System.Drawing.Size(684, 42);
            this.chatBar.TabIndex = 4;
            this.chatBar.Visible = false;
            // 
            // gamePanel1
            // 
            this.gamePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gamePanel1.Location = new System.Drawing.Point(0, 0);
            this.gamePanel1.Name = "gamePanel1";
            this.gamePanel1.Size = new System.Drawing.Size(1550, 729);
            this.gamePanel1.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SecretHitler.Properties.Resources.speechballoon;
            this.pictureBox1.Location = new System.Drawing.Point(1512, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(26, 24);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // Game
            // 
            this.ClientSize = new System.Drawing.Size(1550, 729);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.hiddenButton);
            this.Controls.Add(this.chatBar);
            this.Controls.Add(this.startGameError);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.gamePanel1);
            this.Name = "Game";
            this.Load += new System.EventHandler(this.Game_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private GamePanel gamePanel1;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.Label startGameError;
        private ChatBar chatBar;
        private System.Windows.Forms.Button hiddenButton;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

