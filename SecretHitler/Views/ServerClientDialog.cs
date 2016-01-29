using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretHitler.Views
{
    public partial class ServerClientDialog : Form
    {
        public bool Join { get; set; }
        public string Username { get; set; }
        public IPAddress IPAddress { get; set; }
        public ServerClientDialog()
        {
            InitializeComponent();
        }
        private bool CheckUserName()
        {
            if (textBoxUsername.Text.Length >= 4)
            {
                return true;
            }
            errorLabel.Text = "The username must have a minimal length of 4 characters";
            return false;
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            if (CheckUserName())
            {
                labelIp.Show();
                textBox1.Show();
                btnConfirmJoin.Show();
            }
        }

        private void btnConfirmJoin_Click(object sender, EventArgs e)
        {
            if (CheckUserName())
            {
                Join = true;
                CloseForm();
            }
        }

        private void btnHost_Click(object sender, EventArgs e)
        {
            if (CheckUserName())
            {
                textBox1.Text = "127.0.0.1";
                CloseForm();
            }
        }
        private void CloseForm()
        {
            Username = textBoxUsername.Text;
            IPAddress = IPAddress.Parse(textBox1.Text);
            Close();
        }
    }
}
