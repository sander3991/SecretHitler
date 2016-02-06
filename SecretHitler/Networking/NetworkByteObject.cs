using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkByteObject : NetworkObject
    {
        public byte Value { get; private set; }
        public NetworkByteObject(ServerCommands command, byte value)
            :base(command)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"Byte object: {Value}";
        }
        private NetworkByteObject() { }
        public class ByteObjectDecoder : AbstractObjectReader<NetworkByteObject>
        {
            public override byte[] EncodeObject(NetworkByteObject obj, List<byte> bytes)
            {
                bytes.Add(obj.Value);
                return bytes.ToArray();
            }
            public override NetworkByteObject DecodeObject(byte[] bytes, bool serverSide)
            {
                var networkByteObject = new NetworkByteObject();
                DecodeHeader(networkByteObject, bytes);
                networkByteObject.Value = bytes[CONTENTINDEX];
                return networkByteObject;
            }
        }
    }
}
