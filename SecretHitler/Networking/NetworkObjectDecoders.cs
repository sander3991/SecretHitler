using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public static class NetworkObjectDecoders
    {
        static NetworkObjectDecoders()
        {
            var messageDecoder = new NetworkMessageObject.MessageObjectReader();
            RegisterDecoder(ServerCommands.Message, messageDecoder);
            RegisterDecoder(ServerCommands.ReceiveMessage, messageDecoder);
            var playerDecoder = new NetworkNewPlayerObject.NewPlayerObjectReader();
            RegisterDecoder(ServerCommands.PlayerConnected, playerDecoder);
            RegisterDecoder(ServerCommands.PlayerDisconnected, playerDecoder);
            var cardDecoder = new NetworkCardObject.CardObjectReader();
            RegisterDecoder(ServerCommands.AnnounceCard, cardDecoder);
            RegisterDecoder(ServerCommands.SendGameState, new NetworkGameStateObject.GameStateObjectReader());
            RegisterDecoder(ServerCommands.RevealRole, new NetworkRevealRoleObject.RevealRoleObjectReader());
            RegisterDecoder(ServerCommands.Multiple, new NetworkMultipleObject.MultipleObjectReader());
        }
        private static Dictionary<ServerCommands, INetworkReader> decoders = new Dictionary<ServerCommands, INetworkReader>();
        private static INetworkReader DefaultDecoder = new NetworkObject.DefaultObjectReader();
        public static void RegisterDecoder(ServerCommands command, INetworkReader decoder)
        {
            if (decoders.ContainsKey(command))
                throw new ArgumentException("Command is allready registered!", nameof(command));
            decoders.Add(command, decoder);
        }
        public static INetworkReader GetDecoder(this ServerCommands command)
        {
            if (decoders.ContainsKey(command))
                return decoders[command];
            return DefaultDecoder;
        }
        public static NetworkObject Receive(Socket socket)
        {
            try
            {
                byte[] response = new byte[socket.SendBufferSize];
                socket.Receive(response);
                ServerCommands command = (ServerCommands)response[0];
                return command.GetDecoder().GenerateObject(response);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
