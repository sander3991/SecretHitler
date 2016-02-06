using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkVoteResultObject : NetworkObject
    {
        public bool[] Votes { get; private set; }
        public bool Passed { get; private set; }
        private NetworkVoteResultObject() { }
        public NetworkVoteResultObject(ServerCommands command, bool[] votes, bool passed)
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
                sb.Append(Votes[i] ? "T" : "F");
            sb.Append($" Passed: {Passed}");
            return sb.ToString();
        }
        public class VoteResultObjectDecoder : AbstractObjectReader<NetworkVoteResultObject>
        {
            private enum VoteType : byte { Yes = 2, No = 1}
            private byte ToVoteByte(bool b) => (byte)(b ? VoteType.Yes : VoteType.No);
            public override byte[] EncodeObject(NetworkVoteResultObject obj, List<byte> bytes)
            {
                for (var i = 0; i < obj.Votes.Length; i++)
                    bytes.Add(ToVoteByte(obj.Votes[i]));
                bytes.Add(ToVoteByte(obj.Passed));
                return bytes.ToArray();
            }
            public override NetworkVoteResultObject DecodeObject(byte[] bytes, bool serverSide)
            {
                var obj = new NetworkVoteResultObject();
                DecodeHeader(obj, bytes);
                var arrayLength = FindLastByte(bytes, CONTENTINDEX) - CONTENTINDEX;
                obj.Votes = new bool[arrayLength - 1];
                int i;
                for (i = 0; i < arrayLength - 1; i++)
                    obj.Votes[i] = bytes[CONTENTINDEX + i] == (byte)VoteType.Yes;
                obj.Passed = bytes[CONTENTINDEX + i] == (byte)VoteType.Yes;
                return obj;
            }
        }
    }
}
