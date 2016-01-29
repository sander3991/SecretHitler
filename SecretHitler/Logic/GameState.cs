using Newtonsoft.Json;
using SecretHitler.Networking;
using SecretHitler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Logic
{
    public class GameState
    {
        private GamePanel panel;
        [JsonIgnore]
        public Chat Chat { get; }
        [JsonIgnore]
        public Client Client { get; }
        [JsonIgnore]
        public Server Server { get; }
        [JsonIgnore]
        public int PlayerCount {
            get
            {
                int i = 0;
                for (i = 0; i < SeatedPlayers.Length; i++)
                    if (SeatedPlayers[i] == null)
                        break;
                return i;
            }
        }
        public Player[] SeatedPlayers { get; set; }
        public GameState() { }
        public GameState(GamePanel panel, Chat chat, Client client, Server server)
        {
            Client = client;
            Chat = chat;
            Server = server;
            this.panel = panel;
            SeatedPlayers = new Player[10];
            client.OnConnected += BindGetGamestate;
        }

        private void BindGetGamestate(Client obj)
        {
            obj.ReceiveHandler.OnReceive += OnMessageReceived;
            obj.RequestGameState();
        }

        private void OnMessageReceived(NetworkObject obj)
        {
            if(obj.Command == ServerCommands.GameState)
            {
                var gameState = JsonConvert.DeserializeObject<GameState>(obj.Message);
                Console.WriteLine(gameState);
                for (var i = 0; i < gameState.SeatedPlayers.Length; i++)
                    SeatedPlayers[i] = gameState.SeatedPlayers[i];
            }
        }
        public bool SeatPlayer(Player player)
        {
            for(var i = 0; i < SeatedPlayers.Length; i++)
                if(SeatedPlayers[i] == null)
                {
                    SeatedPlayers[i] = player;
                    return true;
                }
            return false;
        }
    }
}
