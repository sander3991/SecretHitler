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
                    DrawLocation = new Point(area.Location.X + area.Size.Width / 2, area.Location.Y + area.Size.Height / 2);
            }
        }
        public static ChatHandler Instance { get; private set; }
        private GameState state;
        private Dictionary<Player, PlayerArgs> TextObjects = new Dictionary<Player, PlayerArgs>();
        private ChatHandler(GameState state)
        {
            this.state = state;
            state.Client.OnConnected += Client_OnConnected;
        }

        private void Client_OnConnected(Client obj)
        {
            obj.ReceiveHandler.OnReceive += OnReceive;
        }

        public static ChatHandler Initialize(GameState state)
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
                    AppendMessage(Player.GetPlayer(msgObj.Username), msgObj.Message);
                    break;
            }
        }

        private void TextObj_OnDetonate(GameObject obj)
        {
            var txtObj = obj as TextObject;
            if (txtObj == null) return;
            var args = TextObjects[txtObj.Player];
            args.Object = null;
            if (args.QueuedMessages.Count > 0)
                AppendMessage(txtObj.Player, args.QueuedMessages.Pop());
        }

        internal void AppendStatusMessage(string v)
        {
            
        }
        private PlayArea FindPlayArea(Player player)
        {
            int index = state.GetPlayerPos(player.Name);
            return index == -1 ? null : state.PlayAreas[index];
        }
        internal void AppendMessage(Player player, string message)
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
