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
        public Chat Chat { get { return GameState.Chat; } }
        private delegate void SetTextDelegate(string text);
        public Game()
        {
            InitializeComponent();
        }
        public Game(ServerClientDialog dialog)
        {
            InitializeComponent();
            var client = Client.GetClient(this);
            client.Name = dialog.Username;
            Server server = null;
            if (!dialog.Join)
            {
                //Host code
                server = Server.GetInstance(this, gamePanel1, chat1, client);
                server.Start();
                while (!server.Running) ;
                chat1.AppendStatusMessage("Server started");
            }
            GameState = new GameState(gamePanel1, chat1, client, server);
            gamePanel1.InitializeState(GameState);
            client.Connect(dialog.IPAddress);
            chat1.GameState = GameState;
        }

        private void Game_Load(object sender, EventArgs e)
        {
        }
    }
}
