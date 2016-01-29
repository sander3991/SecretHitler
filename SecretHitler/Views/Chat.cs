using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SecretHitler.Networking;
using SecretHitler.Logic;

namespace SecretHitler.Views
{
    public partial class Chat : UserControl
    {
        private GameState game;
        public GameState GameState
        {
            set
            {
                game = value;
                game.Client.ReceiveHandler.OnReceive += AddText;
            }
        }

        private void AddText(NetworkObject obj)
        {
            switch (obj.Command)
            {
                case ServerCommands.ReceiveMessage:
                    AppendLine(obj.Message);
                    break;
                case ServerCommands.PlayerConnected:
                    AppendStatusMessage($"Player [{obj.Message}] has connected.");
                    break;
                case ServerCommands.PlayerDisconnected:
                    AppendStatusMessage($"Player [{obj.Message}] has disconnected");
                    break;
            }
        }

        private delegate void SetTextDelegate(string text);
        public Chat()
        {
            InitializeComponent();
        }

        private void Game_Load(object sender, EventArgs e)
        {
            var serverOrClient = new ServerClientDialog();
            serverOrClient.ShowDialog();
            inputText.Enabled = true;
            sendBtn.Enabled = true;
        }
        public void AppendStatusMessage(string status)
        {
            if (outputText.InvokeRequired)
                Invoke(new SetTextDelegate(AppendStatusMessage), status);
            else
            {
                outputText.SelectionColor = Color.DarkBlue;
                outputText.AppendText($"{status}{Environment.NewLine}");
                outputText.SelectionColor = Color.Black;
            }
        }
        public void AppendLine(string input)
        {
            if (outputText.InvokeRequired)
                Invoke(new SetTextDelegate(AppendLine), input);
            else
                outputText.AppendText($"{input}{Environment.NewLine}");
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            var sendMsg = inputText.Text;
            game.Client.SendMessage(sendMsg, (NetworkObject response) => ClearText());
        }

        private void ClearText()
        {
            if (inputText.InvokeRequired)
                Invoke(new Action(ClearText));
            else
                inputText.Clear();
        }

        private void inputText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                sendBtn_Click(null, null);
        }

        private void Chat_Load(object sender, EventArgs e)
        {
            outputText.Height = Height - panel1.Height;
        }
    }
}
