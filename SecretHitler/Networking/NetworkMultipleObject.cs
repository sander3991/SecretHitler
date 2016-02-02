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

        public class MultipleObjectReader : DefaultObjectReader
        {
            public override byte[] GenerateByteStream(NetworkObject obj)
            {
                var mult = obj as NetworkMultipleObject;
                if (mult == null)
                    return base.GenerateByteStream(obj);
                if (mult.objects.Count == 0)
                    throw new InvalidOperationException("Cannot send an empty object!");
                if (mult.objects.Count == 1)
                    return mult.objects[0].Command.GetDecoder().GenerateByteStream(mult.objects[0]);
                var bytes = Header(mult);
                foreach(var netObj in mult.objects)
                {
                    var thisBytes = netObj.Command.GetDecoder().GenerateByteStream(netObj);
                    bytes.AddRange(BitConverter.GetBytes(thisBytes.Length));
                    bytes.AddRange(thisBytes);
                }
                return bytes.ToArray();
            }
            public override NetworkObject GenerateObject(byte[] bytes)
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
