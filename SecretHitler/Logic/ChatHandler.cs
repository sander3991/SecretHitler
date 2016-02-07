using SecretHitler.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHitler.Networking;
using SecretHitler.Objects;
using System.Drawing;

namespace SecretHitler.Logic
{
    public class ChatHandler
    {
        class PlayerArgs
        {
            public TextObject Object { get; set; }
            public Point DrawLocation { get; }
            public Stack<string> QueuedMessages { get; } = new Stack<string>();
            public PlayerArgs(PlayArea area = null)
            {
                if (area != null)
                    DrawLocation = area.Location;
            }
        }
        public static ChatHandler Instance { get; private set; }
        private ClientGameState state;
        private Dictionary<Player, PlayerArgs> TextObjects = new Dictionary<Player, PlayerArgs>();
        private ChatHandler(ClientGameState state)
        {
            this.state = state;
            state.Client.OnConnected += Client_OnConnected;
        }

        private void Client_OnConnected(Client obj)
        {
            obj.ReceiveHandler.OnReceive += OnReceive;
        }

        public static ChatHandler Initialize(ClientGameState state)
        {
            if (Instance == null)
            {
                Instance = new ChatHandler(state);
            }
            return Instance;
        }

        private void OnReceive(NetworkObject obj)
        {
            switch (obj.Command)
            {
                case ServerCommands.ReceiveMessage:
                    var msgObj = obj as NetworkMessageObject;
                    CreatePlayerBubble(Player.GetPlayer(msgObj.Username), msgObj.Message);
                    break;
                case ServerCommands.PlayerConnected:
                    var connected = obj as NetworkNewPlayerObject;
                    CreatePlayerBubble(connected.Player, $"{connected.Player.Name} has connected");
                    break;
                case ServerCommands.PlayerDisconnected:
                    var disconnected = obj as NetworkNewPlayerObject;
                    CreatePlayerBubble(disconnected.Player, $"{disconnected.Player.Name} has lost connection");
                    break;
                case ServerCommands.AnnouncePresident:
                    var playerObj = obj as NetworkPlayerObject;
                    SetStatusMessage($"{playerObj.Player.Name} is now president");
                    if (playerObj.Player == state.Me)
                        SetInstruction("Pick your chancellor");
                    break;
                case ServerCommands.AnnounceChancellor:
                    var chancellorObj = obj as NetworkPlayerObject;
                    SetStatusMessage($"{state.President.Name} has chosen {chancellorObj.Player.Name} as his/her chancellor{Environment.NewLine}Waiting for votes");
                    if (!state.Me.Dead)
                        SetInstruction($"Cast your vote");
                    else
                        SetInstruction(null);
                    break;
                case ServerCommands.PresidentAction:
                    SetStatusMessage(string.Format(state.FascistActions[(obj as NetworkByteObject).Value].Description, state.President.Name));
                    break;
                case ServerCommands.PlayerVoted:
                    if ((obj as NetworkPlayerObject).Player == state.Me)
                        SetInstruction(null);
                    break;
                case ServerCommands.AnnounceVotes:
                    if ((obj as NetworkVoteResultObject).Passed == Vote.Ja)
                        SetStatusMessage("The vote has passed");
                    else
                        SetStatusMessage("The vote has not passed");
                    break;
                case ServerCommands.PresidentPickPolicyCard:
                case ServerCommands.ChancellorPickPolicyCard:
                    SetInstruction("Pick the Policy you want to discard");
                    break;
                case ServerCommands.PresidentDiscarded:
                case ServerCommands.ChancellorDiscarded:
                    SetInstruction(null);
                    break;
                case ServerCommands.PresidentActionKill:
                    SetInstruction("Choose the player who you would like to kill.");
                    break;
                case ServerCommands.PresidentActionExamine:
                    SetInstruction("Pick a card if you're done examining the cards.");
                    break;
                case ServerCommands.PresidentActionInvestigate:
                    SetInstruction("Choose the player whose membership you want to see.");
                    break;
                case ServerCommands.PresidentActionChoosePresident:
                    SetInstruction("Choose the next president");
                    break;
                case ServerCommands.LiberalWin:
                    SetStatusMessage($"The liberal party has won! {obj.Message}");
                    break;
                case ServerCommands.FascistWin:
                    SetStatusMessage($"The fascist party has won! {obj.Message}"); 
                    break;
                case ServerCommands.PresidentConfirmVeto:
                    SetStatusMessage($"{state.Chancellor.Name} has requested a veto.");
                    if (state.President == state.Me)
                        SetInstruction("Please pick Ja or Nein if you want to agree with the veto.");
                    break;
            }
        }
        private void SetInstruction(string txt)
        {
            state.Window.SetPlayerMessage(txt);
            if(txt != null)
                MessageHistory.Instance.AddHistory($"INSTRUCTION: {txt}");
        }
        private void SetStatusMessage(string txt)
        {
            state.Window.SetStatusText(txt);
            MessageHistory.Instance.AddHistory($"SERVER: {txt}");
        }

        private void TextObj_OnDetonate(GameObject obj)
        {
            var txtObj = obj as TextObject;
            if (txtObj == null) return;
            var args = TextObjects[txtObj.Player];
            args.Object = null;
            if (args.QueuedMessages.Count > 0)
                CreatePlayerBubble(txtObj.Player, args.QueuedMessages.Pop());
        }

        internal void AppendStatusMessage(string v)
        {
            MessageHistory.Instance.AddHistory(v);
        }
        private PlayArea FindPlayArea(Player player)
        {
            int index = state.GetPlayerPos(player.Name);
            return index == -1 ? null : state.PlayAreas[index];
        }
        internal void CreatePlayerBubble(Player player, string message)
        {
            if (!TextObjects.ContainsKey(player))
                TextObjects.Add(player, new PlayerArgs(FindPlayArea(player)));
            var args = TextObjects[player];
            if(args.Object != null)
                args.QueuedMessages.Push(message);
            else
            {
                var textObj = new TextObject(message, player);
                if (!args.DrawLocation.IsEmpty)
                    textObj.Location = TextObjects[player].DrawLocation;
                textObj.OnDetonate += TextObj_OnDetonate;
                lock (state.GameObjects)
                    state.GameObjects.AddLast(textObj);
                args.Object = textObj;
                MessageHistory.Instance.AddHistory($"[{player.Name}]: {message}");
            }
        }
    }
}
