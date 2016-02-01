using SecretHitler.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class ChatHandler
    {
        public static ChatHandler Instance { get; private set; }
        private GameState state;
        private ChatHandler(GameState state)
        {
            this.state = state;
        }
        public static ChatHandler Initialize(GameState state)
        {
            if (Instance == null)
                Instance = new ChatHandler(state);
            return Instance;
        }
        internal void AppendStatusMessage(string v)
        {
            
        }
    }
}
