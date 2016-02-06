using SecretHitler.Logic;
using SecretHitler.Networking;
using SecretHitler.Views;
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
    public partial class Game : Form
    {
        public ClientGameState GameState { get; private set; }
        public ChatHandler Chat { get { return GameState.Chat; } }
        private Server server;
        private Client client;
        private delegate void SetTextDelegate(string text);
        public Game()
        {
            InitializeComponent();
        }
        public Game(ServerClientDialog dialog)
        {
            InitializeComponent();
            client = Client.GetClient(this, dialog.Username);
            client.Name = dialog.Username;
            if (!dialog.Join)
            {
                //Host code
                server = Server.GetInstance(this, gamePanel, client);
                server.Start();
                while (!server.Running) ;
            }
            GameState = new ClientGameState(gamePanel, client, this, server != null);
            gamePanel.InitializeState(GameState);
            client.Connect(dialog.IPAddress);
            statusLabel.Parent = gamePanel;
            playerMsg.Parent = gamePanel;
        }

        internal void SetStatusText(string txt)
        {
            if (statusLabel.InvokeRequired)
                statusLabel.Invoke(new Action<string>(SetStatusText), txt);
            else
                statusLabel.Text = txt;
        }
        internal void SetPlayerMessage(string txt)
        {
            if (playerMsg.InvokeRequired)
                playerMsg.Invoke(new Action<string>(SetPlayerMessage), txt);
            else
                playerMsg.Text = txt;
        }

        private void Game_Load(object sender, EventArgs e)
        {
            AcceptButton = hiddenButton;
            if(!GameState.IsHost)
                startBtn.Visible = false;
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if(GameState.PlayerCount < 5) //TODO: Change to 5
            {
                startGameError.Text = "A minimum of 5 players is required to launch the game";
                startGameError.Visible = true;
                return;
            }
            server.LaunchGame();
            startBtn.Hide();
        }

        internal void OnEnterPressed(object sender, EventArgs e)
        {
            if (chatBar.Visible)
            {
                GameState.Client.SendMessage(chatBar.Text);
                chatBar.Hide();
                chatBar.Close();
            }
            else
            {
                chatBar.Show();
                chatBar.InputField.Focus();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if(!ChatHistory.IsOpen)
            new ChatHistory().Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (server != null)
            {
                new DebugConsole(server, server.GameState, client, GameState).Show();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            new Netviewer(server, client).Show();
        }
    }
}
