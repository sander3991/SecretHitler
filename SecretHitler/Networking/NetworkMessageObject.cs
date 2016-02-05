using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkMessageObject : NetworkObject
    {
        public string Username { get; private set; }
        public NetworkMessageObject(string username, string message, bool sendToServer = true)
            : base(sendToServer ? ServerCommands.Message : ServerCommands.ReceiveMessage, message)
        {
            Username = username;
        }
        private NetworkMessageObject() { }
        public class MessageObjectReader : AbstractObjectReader<NetworkMessageObject>
        {
            public override byte[] EncodeObject(NetworkMessageObject obj)
            {
                List<byte> bytes = Header(obj);
                bytes.AddRange(Encoding.UTF32.GetBytes($"{EncodeString(obj.Username)}{SEPERATOR}{EncodeString(obj.Message)}"));
                return bytes.ToArray();
            }
            public override NetworkMessageObject DecodeObject(byte[] bytes)
            {
                var obj = new NetworkMessageObject();
                DecodeHeader(obj, bytes);
                int lastByte = FindLastByte(bytes, byteSize: 4);//+= 4 UTF32
                var str = Encoding.UTF32.GetString(bytes, CONTENTINDEX, lastByte - 5);
                var split = str.Split(SEPERATOR);
                obj.Username = split[0];
                obj.Message = split[1];
                return obj;
            }
        }
    }
}
