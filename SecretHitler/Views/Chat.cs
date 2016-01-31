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
using SecretHitler.Objects;

namespace SecretHitler.Views
{
    public partial class Chat : UserControl
    {
        public IButtonControl SendBtn { get { return sendBtn; } }
        private GameState game;
        public GameState GameState
        {
            set
            {
                game = value;
                game.Client.ReceiveHandler.OnReceive += AddText;
                game.Client.OnUsernameChanged += UsernameChanged;
            }
        }

        private void UsernameChanged(string username)
            => AppendLine($"Your username has been changed by the server into {username}");

        private void AddText(NetworkObject obj)
        {
            switch (obj.Command)
            {
                case ServerCommands.ReceiveMessage:
                    AddPlayerMsg(obj as NetworkMessageObject);
                    break;
                case ServerCommands.PlayerConnected:
                    AppendStatusMessage($"Player [{((NetworkNewPlayerObject)obj).Player.Name}] has connected.");
                    break;
                case ServerCommands.PlayerDisconnected:
                    AppendStatusMessage($"Player [{((NetworkNewPlayerObject)obj).Player.Name}] has disconnected");
                    break;
                case ServerCommands.AnnounceCard:
                    AppendStatusMessage("The server has dealt the cards");
                    var cardObj = obj as NetworkCardObject;
                    foreach(var card in cardObj.Cards)
                        if(card is CardSecretRole)
                        {
                            var role = card as CardSecretRole;
                            AppendStatusMessage($"You are a {(role.IsFascist ? "Fascist" : "Liberal")} and you are {(role.IsHitler ? string.Empty : "not")} hitler");
                            break;
                        }
                    break;
            }
        }

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
        private void AnnouncePlayerCard(CardSecretRole role)
        {
            if (outputText.InvokeRequired)
                Invoke(new Action<CardSecretRole>(AnnouncePlayerCard), role);
            else
            {
                Append("You are a ");
                outputText.SelectionColor = role.IsFascist ? Color.Red : Color.LightBlue;
                Append(role.IsFascist ? "Fascist" : "Liberal");
                AppendLine($" and you are {(role.IsHitler ? string.Empty : "not")} Hitler");
            }
        }
        private void AddPlayerMsg(NetworkMessageObject obj)
        {
            if (outputText.InvokeRequired)
                Invoke(new Action<NetworkMessageObject>(AddPlayerMsg), obj);
            else
            {
                Append("[");
                outputText.SelectionColor = Color.DarkBlue;
                Append(obj.Username);
                outputText.SelectionColor = Color.Black;
                AppendLine($"]: {obj.Message}");
            }
        }
        public void AppendStatusMessage(string status)
        {
            if (outputText.InvokeRequired)
                Invoke(new Action<string>(AppendStatusMessage), status);
            else
            {
                outputText.SelectionColor = Color.DarkBlue;
                AppendLine(status);
                outputText.SelectionColor = Color.Black;
            }
        }
        public void AppendLine(string input)
        {
            if (outputText.InvokeRequired)
                Invoke(new Action<string>(AppendLine), input);
            else
            {
                Append(input);
                AppendNewLine();
                outputText.ScrollToCaret();
            }
        }
        public void Append(string input)
        {
            if (outputText.InvokeRequired)
                Invoke(new Action<string>(Append), input);
            else
            {
                outputText.AppendText(input);
                outputText.ScrollToCaret();
            }

        }
        public void AppendNewLine() => outputText.AppendText(Environment.NewLine);

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

        private void Chat_Load(object sender, EventArgs e)
        {
            outputText.Height = Height - panel1.Height;
        }
    }
}
