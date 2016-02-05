using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public class NetworkMultipleObject : NetworkObject
    {
        private List<NetworkObject> objects;
        public NetworkObject[] Objects { get { return objects.ToArray(); } }
        public NetworkMultipleObject(params NetworkObject[] obj)
            :base(ServerCommands.Multiple)
        {
            objects = new List<NetworkObject>(obj);
        }
        private NetworkMultipleObject()
        {
            objects = new List<NetworkObject>();
        }
        public void AddObject(NetworkObject obj) => objects.Add(obj);

        public class MultipleObjectReader : AbstractObjectReader<NetworkMultipleObject>
        {
            public override byte[] EncodeObject(NetworkMultipleObject obj)
            {
                if (obj.objects.Count == 0)
                    throw new InvalidOperationException("Cannot send an empty object!");
                if (obj.objects.Count == 1)
                    return obj.objects[0].Command.GetDecoder().GenerateByteStream(obj.objects[0]);
                var bytes = Header(obj);
                foreach(var netObj in obj.objects)
                {
                    var thisBytes = netObj.Command.GetDecoder().GenerateByteStream(netObj);
                    bytes.AddRange(BitConverter.GetBytes(thisBytes.Length));
                    bytes.AddRange(thisBytes);
                }
                return bytes.ToArray();
            }
            public override NetworkMultipleObject DecodeObject(byte[] bytes)
            {
                var mult = new NetworkMultipleObject();
                DecodeHeader(mult, bytes);
                int nextLength = BitConverter.ToInt32(bytes, CONTENTINDEX);
                int startIndex = CONTENTINDEX + 4;
                while(nextLength != 0)
                {
                    var command = (ServerCommands)bytes[startIndex];
                    if (command == ServerCommands.None)
                        throw new InvalidOperationException("Received a None servercommand. Not allowed");
                    var newBytes = new byte[nextLength];
                    for (var i = 0; i < nextLength; i++)
                        newBytes[i] = bytes[startIndex + i];
                    mult.objects.Add(command.GetDecoder().GenerateObject(newBytes));
                    if(startIndex + nextLength + 4 >= bytes.Length)
                        break;
                    var prevLength = nextLength;
                    nextLength = BitConverter.ToInt32(bytes, startIndex + nextLength);
                    startIndex = startIndex + prevLength + 4;
                }
                return mult;
            }
        }
    }
}
