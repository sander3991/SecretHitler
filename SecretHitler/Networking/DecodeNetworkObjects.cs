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
            var boolDecoder = new NetworkBoolObject.BoolObjectReader();
            var newPlayerDecoder = new NetworkNewPlayerObject.NewPlayerObjectReader();
            var cardDecoder = new NetworkCardObject.CardObjectReader();
            var playerDecoder = new NetworkPlayerObject.PlayerObjectReader();
            var byteDecoder = new NetworkByteObject.ByteObjectDecoder();
            RegisterDecoder(Message, messageDecoder);
            RegisterDecoder(ReceiveMessage, messageDecoder);
            RegisterDecoder(PlayerConnected, newPlayerDecoder);
            RegisterDecoder(PlayerDisconnected, newPlayerDecoder);
            RegisterDecoder(AnnounceCard, cardDecoder);
            RegisterDecoder(SendGameState, new NetworkGameStateObject.GameStateObjectReader());
            RegisterDecoder(RevealRole, new NetworkRevealRoleObject.RevealRoleObjectReader());
            RegisterDecoder(Multiple, new NetworkMultipleObject.MultipleObjectReader());
            RegisterDecoder(AnnouncePresident, playerDecoder);
            RegisterDecoder(AnnounceChancellor, playerDecoder);
            RegisterDecoder(CastVote, boolDecoder);
            RegisterDecoder(PlayerVoted, playerDecoder);
            RegisterDecoder(AnnounceVotes, new NetworkVoteResultObject.VoteResultObjectDecoder());
            RegisterDecoder(PolicyCardsDrawn, byteDecoder);
            RegisterDecoder(PresidentPickPolicyCard, cardDecoder);
            RegisterDecoder(ChancellorPickPolicyCard, cardDecoder);
            RegisterDecoder(PresidentPolicyCardPicked, byteDecoder);
            RegisterDecoder(ChancellorPolicyCardPicked, byteDecoder);
            RegisterDecoder(CardPlayed, cardDecoder);
            RegisterDecoder(RevealMembership, newPlayerDecoder);
            RegisterDecoder(KillPlayer, playerDecoder);
            RegisterDecoder(NotHitler, playerDecoder);
            RegisterDecoder(PresidentRequestVetoAllowed, boolDecoder);
            RegisterDecoder(AnnounceVetoResult, boolDecoder);

            RegisterDecoder(PresidentAction, byteDecoder);
            RegisterDecoder(PresidentDoingAction, playerDecoder);

            RegisterDecoder(PresidentActionExamine, cardDecoder);
            //Other Actions are plain objects

            //Examine object is a plain object notifying the server the player is done
            RegisterDecoder(PresidentActionKillResponse, playerDecoder);
            RegisterDecoder(PresidentActionChoosePresidentResponse, playerDecoder);
            RegisterDecoder(PresidentActionInvestigatePresidentResponse, playerDecoder);
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
        public static NetworkObject Receive(TcpClient client, bool serverSide)
        {
            try
            {
                byte[] response = new byte[client.ReceiveBufferSize];
                client.GetStream().Read(response, 0, client.ReceiveBufferSize);
                ServerCommands command = (ServerCommands)response[0];
                return command.GetDecoder().GenerateObject(response, serverSide);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
