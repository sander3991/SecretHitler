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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Game));
            this.startBtn = new System.Windows.Forms.Button();
            this.startGameError = new System.Windows.Forms.Label();
            this.hiddenButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.chatBox = new System.Windows.Forms.PictureBox();
            this.chatBar = new SecretHitler.Views.ChatBar();
            this.gamePanel = new SecretHitler.Views.GamePanel();
            this.playerMsg = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chatBox)).BeginInit();
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
            // statusLabel
            // 
            this.statusLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.statusLabel.AutoSize = true;
            this.statusLabel.BackColor = System.Drawing.Color.Transparent;
            this.statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(79)))), ((int)(((byte)(106)))));
            this.statusLabel.Location = new System.Drawing.Point(421, 205);
            this.statusLabel.MaximumSize = new System.Drawing.Size(700, 80);
            this.statusLabel.MinimumSize = new System.Drawing.Size(700, 80);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(700, 80);
            this.statusLabel.TabIndex = 7;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chatBox
            // 
            this.chatBox.Image = global::SecretHitler.Properties.Resources.speechballoon;
            this.chatBox.Location = new System.Drawing.Point(1512, 12);
            this.chatBox.Name = "chatBox";
            this.chatBox.Size = new System.Drawing.Size(26, 24);
            this.chatBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.chatBox.TabIndex = 6;
            this.chatBox.TabStop = false;
            this.chatBox.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // chatBar
            // 
            this.chatBar.Location = new System.Drawing.Point(433, 465);
            this.chatBar.Name = "chatBar";
            this.chatBar.Size = new System.Drawing.Size(684, 42);
            this.chatBar.TabIndex = 4;
            this.chatBar.Visible = false;
            // 
            // gamePanel
            // 
            this.gamePanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gamePanel.BackgroundImage")));
            this.gamePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gamePanel.Location = new System.Drawing.Point(0, 0);
            this.gamePanel.Name = "gamePanel";
            this.gamePanel.Size = new System.Drawing.Size(1550, 729);
            this.gamePanel.TabIndex = 1;
            // 
            // playerMsg
            // 
            this.playerMsg.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.playerMsg.AutoSize = true;
            this.playerMsg.BackColor = System.Drawing.Color.Transparent;
            this.playerMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playerMsg.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(143)))), ((int)(((byte)(79)))), ((int)(((byte)(106)))));
            this.playerMsg.Location = new System.Drawing.Point(421, 445);
            this.playerMsg.MaximumSize = new System.Drawing.Size(700, 80);
            this.playerMsg.MinimumSize = new System.Drawing.Size(700, 80);
            this.playerMsg.Name = "playerMsg";
            this.playerMsg.Size = new System.Drawing.Size(700, 80);
            this.playerMsg.TabIndex = 8;
            this.playerMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1463, 42);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "debug";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1463, 71);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 10;
            this.button2.Text = "net";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Game
            // 
            this.ClientSize = new System.Drawing.Size(1550, 729);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.playerMsg);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.chatBox);
            this.Controls.Add(this.hiddenButton);
            this.Controls.Add(this.chatBar);
            this.Controls.Add(this.startGameError);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.gamePanel);
            this.Name = "Game";
            this.Load += new System.EventHandler(this.Game_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chatBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private GamePanel gamePanel;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.Label startGameError;
        private ChatBar chatBar;
        private System.Windows.Forms.Button hiddenButton;
        private System.Windows.Forms.PictureBox chatBox;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label playerMsg;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}

