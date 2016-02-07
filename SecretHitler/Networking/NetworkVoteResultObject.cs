using SecretHitler.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkVoteResultObject : NetworkObject
    {
        public Vote[] Votes { get; private set; }
        public Vote Passed { get; private set; }
        private NetworkVoteResultObject() { }
        public NetworkVoteResultObject(ServerCommands command, Vote[] votes, Vote passed)
            :base(command)
        {
            Votes = votes;
            Passed = passed;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("VoteResult object: ");
            for (var i = 0; i < Votes.Length; i++)
            {
                sb.Append(Votes[i]);
                if (i + 1 < Votes.Length)
                    sb.Append('|');
            }
            sb.Append($" Passed: {Passed}");
            return sb.ToString();
        }
        public class VoteResultObjectDecoder : AbstractObjectReader<NetworkVoteResultObject>
        {
            public override byte[] EncodeObject(NetworkVoteResultObject obj, List<byte> bytes)
            {
                for (var i = 0; i < obj.Votes.Length; i++)
                    bytes.Add((byte)obj.Votes[i]);
                bytes.Add((byte)obj.Passed);
                return bytes.ToArray();
            }
            public override NetworkVoteResultObject DecodeObject(byte[] bytes, bool serverSide)
            {
                var obj = new NetworkVoteResultObject();
                DecodeHeader(obj, bytes);
                var arrayLength = FindLastByte(bytes, CONTENTINDEX) - CONTENTINDEX;
                obj.Votes = new Vote[arrayLength - 1];
                int i;
                for (i = 0; i < arrayLength - 1; i++)
                    obj.Votes[i] = (Vote)bytes[CONTENTINDEX + i];
                obj.Passed = (Vote)bytes[CONTENTINDEX + i];
                return obj;
            }
        }
    }
}
