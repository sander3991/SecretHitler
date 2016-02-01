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
        public GameState GameState { get; private set; }
        public ChatHandler Chat { get { return GameState.Chat; } }
        private delegate void SetTextDelegate(string text);
        public Game()
        {
            InitializeComponent();
        }
        public Game(ServerClientDialog dialog)
        {
            InitializeComponent();
            var client = Client.GetClient(this, dialog.Username);
            client.Name = dialog.Username;
            Server server = null;
            if (!dialog.Join)
            {
                //Host code
                server = Server.GetInstance(this, gamePanel1, client);
                server.Start();
                while (!server.Running) ;
                ChatHandler.Instance.AppendStatusMessage("Server started");
            }
            GameState = new GameState(gamePanel1, client, server);
            gamePanel1.InitializeState(GameState);
            client.Connect(dialog.IPAddress);
        }

        private void Game_Load(object sender, EventArgs e)
        {
            AcceptButton = hiddenButton;
            if(GameState.Server == null)
            {
                startBtn.Visible = false;
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if(GameState.PlayerCount < 0) //TODO: Change to 5
            {
                startGameError.Text = "A minimum of 5 players is required to launch the game";
                startGameError.Visible = true;
                return;
            }
            GameState.Server.LaunchGame();
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
    }
}
