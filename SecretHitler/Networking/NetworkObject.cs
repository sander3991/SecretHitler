using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{

    public class NetworkObject
    {
        private static uint NextID = 1;
        protected const char SEPERATOR = '|';
        public uint ID { get; private set; }
        public ServerCommands Command { get; private set; }
        private byte[] bytes;
        public string Message { get; set; }
        public byte[] Bytes {
            get
            {
                if (bytes == null)
                    bytes = Command.GetDecoder().GenerateByteStream(this);
                return bytes;
            }
        }
        public NetworkObject(ServerCommands command, string msg = null, uint? ID = null)
        {
            ID = ID.HasValue ? ID.Value : NextID++;
            Message = msg;
            Command = command;
        }
        protected NetworkObject() { }
        protected void DecodeHeader(byte[] bytes)
        {
            Command = (ServerCommands)bytes[0];
            ID = BitConverter.ToUInt32(bytes, 1);
        }
        public void Send(Socket socket)
            => socket.Send(Bytes);
        public class DefaultObjectReader : INetworkReader
        {
            protected const int CONTENTINDEX = 5;
            public virtual NetworkObject GenerateObject(byte[] bytes)
            {
                var obj = new NetworkObject();
                DecodeHeader(obj, bytes);
                int lastByte = FindLastByte(bytes);
                if (lastByte > CONTENTINDEX)
                    obj.Message = Encoding.ASCII.GetString(bytes, CONTENTINDEX, lastByte - CONTENTINDEX);
                return obj;
            }   
            protected void DecodeHeader(NetworkObject obj, byte[] bytes)
            {
                obj.Command = (ServerCommands)bytes[0];
                obj.ID = BitConverter.ToUInt32(bytes, 1);
                obj.bytes = bytes;
            }
            protected List<byte> Header(NetworkObject obj)
            {
                List<byte> bytes = new List<byte>();
                bytes.Add((byte)obj.Command);
                bytes.AddRange(BitConverter.GetBytes(obj.ID));
                return bytes;
            }

            protected int FindLastByte(byte[] bytes, int startIndex = CONTENTINDEX, int byteSize = 1)
            {
                int lastByte;
                for (lastByte = startIndex; lastByte < bytes.Length && bytes[lastByte] != 0; lastByte += byteSize) ;
                return lastByte;
            }

            protected string EncodeString(string str) => str.Replace("&", "&amp;").Replace(SEPERATOR.ToString(), "&#124;");
            protected string DecodeString(string str) => str.Replace("&#124;", SEPERATOR.ToString()).Replace("&amp;", "&");

            public virtual byte[] GenerateByteStream(NetworkObject obj)
            {
                var list = Header(obj);
                if (obj.Message != null)
                    list.AddRange(Encoding.ASCII.GetBytes(obj.Message));
                return list.ToArray();
            }
        }

    }

    
}
