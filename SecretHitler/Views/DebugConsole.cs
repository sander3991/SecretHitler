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
            {
                policyButtons[i].Tag = (byte)i;
            }
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

        private void UnsubPolicyCard(Button btn)
        {
            btn.Enabled = false;
            if (btn.Text == "Veto")
                btn.Click -= PickVeto;
            else
                btn.Click -= PickPolicyCard;
        }

        private void PickPolicyCard(object sender, EventArgs e)
        {
            if (selectedPlayer == null) return;
            var btn = sender as Button;
            ServerCommands command = serverGameState.Chancellor == selectedPlayer ? ServerCommands.ChancellorPolicyCardPicked : ServerCommands.PresidentPolicyCardPicked;
            SendToServer(new NetworkByteObject(command, (byte)btn.Tag));
            foreach (var policyBtn in policyButtons)
                UnsubPolicyCard(policyBtn);

        }

        private void PickVeto(object sender, EventArgs e)
        {
            foreach(var policyBtn in policyButtons)
                UnsubPolicyCard(policyBtn);
            SendToServer(new NetworkObject(ServerCommands.ChancellorRequestVeto));
        }

        private void getPolicyChoice_Click(object sender, EventArgs e)
        {
            if (serverGameState.CurrentlyPicked == null) return;
            for (var i = 0; i < serverGameState.CurrentlyPicked.Length; i++)
            {
                policyButtons[i].Enabled = true;
                if (serverGameState.CurrentlyPicked[i] is CardPolicyFascist)
                {
                    policyButtons[i].Text = "Fascist";
                    policyButtons[i].Click += PickPolicyCard;
                }
                else if (serverGameState.CurrentlyPicked[i] is CardPolicyLiberal)
                {
                    policyButtons[i].Text = "Liberal";
                    policyButtons[i].Click += PickPolicyCard;
                }
                else
                {
                    policyButtons[i].Text = "Veto";
                    policyButtons[i].Click += PickVeto;
                }

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
            => server.ServerMessageHandler.HandleMessage(new NetworkObject(ServerCommands.PresidentActionExamineResponse), null, selectedPlayer);

        private void confirmKillPlayer_Click(object sender, EventArgs e)
            => awaitingPlayerSelection =
                (Player player) => server.ServerMessageHandler.HandleMessage(new NetworkPlayerObject(ServerCommands.PresidentActionKillResponse, player), null, selectedPlayer);

        private void choosePresidentBtn_Click(object sender, EventArgs e)
            => awaitingPlayerSelection =
                (Player player) => server.ServerMessageHandler.HandleMessage(new NetworkPlayerObject(ServerCommands.PresidentActionChoosePresidentResponse, player), null, selectedPlayer);

        private void investigatePlayerBtn_Click(object sender, EventArgs e)
        {
            awaitingPlayerSelection =
                (Player player) => server.ServerMessageHandler.HandleMessage(new NetworkPlayerObject(ServerCommands.PresidentActionInvestigatePresidentResponse, player), null, selectedPlayer);
        }
        private void Veto(bool yes)
            => server.ServerMessageHandler.HandleMessage(new NetworkBoolObject(ServerCommands.PresidentRequestVetoAllowed, yes), null, selectedPlayer);
        private void vetoBtnYes_Click(object sender, EventArgs e)
            => Veto(true);

        private void vetoBtnNo_Click(object sender, EventArgs e)
            => Veto(false);

        private void startBtn_Click(object sender, EventArgs e)
        {
            server.GameState.EndGame();
            server.GameState.StartGame();
        }
    }
}
