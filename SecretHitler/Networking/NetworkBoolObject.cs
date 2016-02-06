using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkBoolObject : NetworkObject
    {
        public bool Value { get; private set; }
        private NetworkBoolObject() { }
        public NetworkBoolObject(ServerCommands command, bool value)
            :base(command)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"Bool Object: {Value.ToString()}";
        }

        public class BoolObjectReader : AbstractObjectReader<NetworkBoolObject>
        {
            public override byte[] EncodeObject(NetworkBoolObject obj, List<byte> bytes)
            {
                bytes.Add((byte)(obj.Value ? 1 : 0));
                if (obj.Message != null)
                    bytes.AddRange(Encoding.ASCII.GetBytes(obj.Message));
                return bytes.ToArray();
            }
            public override NetworkBoolObject DecodeObject(byte[] bytes, bool serverSide)
            {
                var boolObj = new NetworkBoolObject();
                DecodeHeader(boolObj, bytes);
                boolObj.Value = bytes[CONTENTINDEX] == 1;
                if(bytes.Length > CONTENTINDEX + 1 && bytes[CONTENTINDEX + 1] != 0)
                {
                    var lastByte = FindLastByte(bytes, CONTENTINDEX + 1);
                    boolObj.Message = Encoding.ASCII.GetString(bytes, CONTENTINDEX + 1, lastByte - CONTENTINDEX - 1);
                }
                return boolObj;
            }
        }
    }
}
