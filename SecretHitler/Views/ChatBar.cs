using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretHitler.Views
{
    public partial class ChatBar : UserControl
    {
        public override string Text { get { return textBox1.Text == placeHolderText ? null : textBox1.Text; ; } }
        public TextBox InputField { get { return textBox1; } }
        internal Game Game { private get; set; }
        private bool placeHolder = true;
        private string placeHolderText = "Enter your message...";
        private bool inClass = false;
        public ChatBar()
        {
            InitializeComponent();
            SetPlaceholder();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (inClass) return;
            if (placeHolder)
                RemovePlaceholder();
            if (textBox1.Text.Length == 0)
                SetPlaceholder();
        }
        private void SetPlaceholder()
        {
            inClass = true;
            textBox1.ForeColor = Color.DarkGray;
            textBox1.Text = placeHolderText;
            placeHolder = true;
            inClass = false;
        }
        private void RemovePlaceholder()
        {
            inClass = true;
            textBox1.ForeColor = Color.Black;
            textBox1.Text = textBox1.Text.Replace(placeHolderText, "");
            textBox1.SelectionStart = textBox1.Text.Length;
            placeHolder = false;
            inClass = false;
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == placeHolderText)
                textBox1.SelectionStart = 0;
        }
        public void Close()
        {
            SetPlaceholder();
        }
    }
}
