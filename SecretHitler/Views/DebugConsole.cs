using SecretHitler.Logic;
using SecretHitler.Networking;
using SecretHitler.Objects;
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
    public partial class DebugConsole : Form
    {
        private Button[] playerButtons;
        private Button[] policyButtons;
        public DebugConsole()
        {
            InitializeComponent();
            playerButtons = new Button[]
            {
                playerButton1, playerButton2, playerButton3, playerButton4, playerButton5, playerButton6, playerButton7, playerButton8, playerButton9, playerButton10
            };
            policyButtons = new Button[] { policyCard1, policyCard2, policyCard3 };
        }
        private Server server;
        private ServerGameState serverGameState;
        private Client client;
        private ClientGameState clientGameState;
        private Button selectedButton;
        private Player selectedPlayer;
        private Action<Player> awaitingPlayerSelection;
        public DebugConsole(Server server, ServerGameState serverGameState, Client client, ClientGameState clientGameState)
            :this()
        {
            this.server = server;
            this.serverGameState = serverGameState;
            this.client = client;
            this.clientGameState = clientGameState;
            for(var i = 0; i < serverGameState.SeatedPlayers.Length; i++)
            {
                if (serverGameState.SeatedPlayers[i] == null) continue;
                playerButtons[i].Enabled = true;
                playerButtons[i].Text = serverGameState.SeatedPlayers[i].Name;
                playerButtons[i].Tag = serverGameState.SeatedPlayers[i];
            }
            for (var i = 0; i < policyButtons.Length; i++)
                policyButtons[i].Tag = (byte)i;
        }
        private void PlayerButtonClick(object sender, EventArgs args)
        {
            if(awaitingPlayerSelection != null)
            {
                awaitingPlayerSelection((sender as Button).Tag as Player);
                awaitingPlayerSelection = null;
                return;
            }
            if (selectedButton != null)
                selectedButton.Enabled = true;
            var btn = sender as Button;
            btn.Enabled = false;
            selectedButton = btn;
            selectedPlayer = btn.Tag as Player;
            membershipLabel.Text = selectedPlayer.Hand.Membership.IsFascist ? "Fascist" : "Liberal";
            roleLabel.Text = selectedPlayer.Hand.Role.IsHitler ? "Hitler" : selectedPlayer.Hand.Role.IsFascist ? "Fascist" : "Liberal";
        }

        private void SendToServer(NetworkObject obj)
        {
            try
            {
                server.ServerMessageHandler.HandleMessage(obj, null, selectedPlayer);
            }
            catch (Exception)
            {

            }
        }

        private void CastVote(object sender, EventArgs e)
        {
            if (selectedPlayer == null) return;
            bool yes = sender == castVoteYes;
            SendToServer(new NetworkBoolObject(ServerCommands.CastVote, yes));
        }

        private void PickChancellor(object sender, EventArgs e)
        {
            awaitingPlayerSelection = 
                (Player player) => SendToServer(new NetworkPlayerObject(ServerCommands.AnnounceChancellor, player));
        }

        private void PickPolicyCard(object sender, EventArgs e)
        {
            if (selectedPlayer == null) return;
            var btn = sender as Button;
            ServerCommands command = serverGameState.CurrentlyPicked.Length == 3 ? ServerCommands.PresidentPolicyCardPicked : ServerCommands.ChancellorPolicyCardPicked;
            SendToServer(new NetworkByteObject(command, (byte)btn.Tag));
            foreach (var policyBtn in policyButtons)
                policyBtn.Enabled = false;
        }

        private void getPolicyChoice_Click(object sender, EventArgs e)
        {
            if (serverGameState.CurrentlyPicked == null) return;
            for (var i = 0; i < serverGameState.CurrentlyPicked.Length; i++)
            {
                policyButtons[i].Enabled = true;
                policyButtons[i].Text = serverGameState.CurrentlyPicked[i] is CardPolicyFascist ? "Fascist" : "Liberal";
            }
        }

        private void generatePolicies(object sender, EventArgs e)
        {
            for(var i = 0; i < 6; i++)
            {
                clientGameState.PlayPolicy(new CardPolicyFascist());
                if(i != 5)
                    clientGameState.PlayPolicy(new CardPolicyLiberal());
            }
        }

        private void confirmReadBtn_Click(object sender, EventArgs e)
        {
            server.ServerMessageHandler.HandleMessage(new NetworkObject(ServerCommands.PresidentActionExamineResponse), null, selectedPlayer);
        }

        private void confirmKillPlayer_Click(object sender, EventArgs e)
        {
            awaitingPlayerSelection =
                (Player player) => server.ServerMessageHandler.HandleMessage(new NetworkPlayerObject(ServerCommands.PresidentActionKillResponse, player), null, selectedPlayer);
        }

        private void choosePresidentBtn_Click(object sender, EventArgs e)
        {
            awaitingPlayerSelection =
                (Player player) => server.ServerMessageHandler.HandleMessage(new NetworkPlayerObject(ServerCommands.PresidentActionChoosePresidentResponse, player), null, selectedPlayer);
        }

        private void investigatePlayerBtn_Click(object sender, EventArgs e)
        {
            awaitingPlayerSelection =
                (Player player) => server.ServerMessageHandler.HandleMessage(new NetworkPlayerObject(ServerCommands.PresidentActionInvestigatePresidentResponse, player), null, selectedPlayer);
        }
    }
}
