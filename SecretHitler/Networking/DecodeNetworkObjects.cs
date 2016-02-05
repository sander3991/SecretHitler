using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static SecretHitler.Networking.ServerCommands;

namespace SecretHitler.Networking
{
    public static class DecodeNetworkObjects
    {
        static DecodeNetworkObjects()
        {
            var messageDecoder = new NetworkMessageObject.MessageObjectReader();
            RegisterDecoder(Message, messageDecoder);
            RegisterDecoder(ReceiveMessage, messageDecoder);
            var newPlayerDecoder = new NetworkNewPlayerObject.NewPlayerObjectReader();
            RegisterDecoder(PlayerConnected, newPlayerDecoder);
            RegisterDecoder(PlayerDisconnected, newPlayerDecoder);
            var cardDecoder = new NetworkCardObject.CardObjectReader();
            RegisterDecoder(AnnounceCard, cardDecoder);
            RegisterDecoder(SendGameState, new NetworkGameStateObject.GameStateObjectReader());
            RegisterDecoder(RevealRole, new NetworkRevealRoleObject.RevealRoleObjectReader());
            RegisterDecoder(Multiple, new NetworkMultipleObject.MultipleObjectReader());
            var playerDecoder = new NetworkPlayerObject.PlayerObjectReader();
            RegisterDecoder(AnnouncePresident, playerDecoder);
            RegisterDecoder(AnnounceChancellor, playerDecoder);
            var boolDecoder = new NetworkBoolObject.BoolObjectReader();
            RegisterDecoder(CastVote, boolDecoder);
            RegisterDecoder(PlayerVoted, playerDecoder);
            RegisterDecoder(AnnounceVotes, new NetworkVoteResultObject.VoteResultObjectDecoder());
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
