namespace SecretHitler.Views
{
    partial class ServerClientDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnJoin = new System.Windows.Forms.Button();
            this.btnHost = new System.Windows.Forms.Button();
            this.labelIp = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnConfirmJoin = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.errorLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(190, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Would you like to host or join a server?";
            // 
            // btnJoin
            // 
            this.btnJoin.Location = new System.Drawing.Point(30, 69);
            this.btnJoin.Name = "btnJoin";
            this.btnJoin.Size = new System.Drawing.Size(81, 23);
            this.btnJoin.TabIndex = 1;
            this.btnJoin.Text = "Join";
            this.btnJoin.UseVisualStyleBackColor = true;
            this.btnJoin.Click += new System.EventHandler(this.btnJoin_Click);
            // 
            // btnHost
            // 
            this.btnHost.Location = new System.Drawing.Point(143, 69);
            this.btnHost.Name = "btnHost";
            this.btnHost.Size = new System.Drawing.Size(75, 23);
            this.btnHost.TabIndex = 2;
            this.btnHost.Text = "Host";
            this.btnHost.UseVisualStyleBackColor = true;
            this.btnHost.Click += new System.EventHandler(this.btnHost_Click);
            // 
            // labelIp
            // 
            this.labelIp.AutoSize = true;
            this.labelIp.Location = new System.Drawing.Point(12, 161);
            this.labelIp.Name = "labelIp";
            this.labelIp.Size = new System.Drawing.Size(58, 13);
            this.labelIp.TabIndex = 3;
            this.labelIp.Text = "IP Address";
            this.labelIp.Visible = false;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(15, 177);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(121, 20);
            this.textBox1.TabIndex = 4;
            this.textBox1.Text = "92.109.183.86";
            this.textBox1.Visible = false;
            // 
            // btnConfirmJoin
            // 
            this.btnConfirmJoin.Location = new System.Drawing.Point(142, 174);
            this.btnConfirmJoin.Name = "btnConfirmJoin";
            this.btnConfirmJoin.Size = new System.Drawing.Size(75, 23);
            this.btnConfirmJoin.TabIndex = 5;
            this.btnConfirmJoin.Text = "Join";
            this.btnConfirmJoin.UseVisualStyleBackColor = true;
            this.btnConfirmJoin.Visible = false;
            this.btnConfirmJoin.Click += new System.EventHandler(this.btnConfirmJoin_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Username";
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(18, 115);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(199, 20);
            this.textBoxUsername.TabIndex = 7;
            // 
            // errorLabel
            // 
            this.errorLabel.AutoSize = true;
            this.errorLabel.ForeColor = System.Drawing.Color.Red;
            this.errorLabel.Location = new System.Drawing.Point(15, 145);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(0, 13);
            this.errorLabel.TabIndex = 8;
            // 
            // ServerClientDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(262, 227);
            this.ControlBox = false;
            this.Controls.Add(this.errorLabel);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnConfirmJoin);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.labelIp);
            this.Controls.Add(this.btnHost);
            this.Controls.Add(this.btnJoin);
            this.Controls.Add(this.label1);
            this.Name = "ServerClientDialog";
            this.Text = "ServerClientDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnJoin;
        private System.Windows.Forms.Button btnHost;
        private System.Windows.Forms.Label labelIp;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnConfirmJoin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label errorLabel;
    }
}