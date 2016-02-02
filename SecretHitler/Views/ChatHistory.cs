using SecretHitler.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretHitler.Views
{
    public partial class ChatHistory : Form
    {
        public static bool IsOpen { get { return chatHistory != null; }  }
        private static ChatHistory chatHistory;
        public ChatHistory()
        {
            if(chatHistory != null)
                throw new InvalidOperationException();
            chatHistory = this;
            InitializeComponent();
        }

        private void ChatHistory_Load(object sender, EventArgs e)
        {
            var history = MessageHistory.Instance;
            foreach (var str in history)
                richTextBox1.AppendText($"{str}{Environment.NewLine}");
            history.OnNewMessage += OnNewMessage;
            richTextBox1.ScrollToCaret();
        }

        private void OnNewMessage(string str)
        {
            if (richTextBox1.InvokeRequired)
                richTextBox1.Invoke(new Action<string>(OnNewMessage), str);
            else
            {
                richTextBox1.AppendText($"{str}{Environment.NewLine}");
                richTextBox1.ScrollToCaret();
            }
        }

        private void ChatHistory_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageHistory.Instance.OnNewMessage -= OnNewMessage;
            chatHistory = null;
        }
    }
}
