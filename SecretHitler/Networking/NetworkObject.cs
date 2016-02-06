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
        private uint id;
        public uint ID
        {
            get { return id; }
        }
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
            id = ID.HasValue ? ID.Value : NextID++;
            Message = msg;
            Command = command;
        }
        protected NetworkObject()
        {
            id = NextID++;
        }
        protected void DecodeHeader(byte[] bytes)
        {
            Command = (ServerCommands)bytes[0];
            id = BitConverter.ToUInt32(bytes, 1);
        }
        public void Send(TcpClient client)
        {
            if (client == null) return;
            client.GetStream().Write(Bytes, 0, Bytes.Length);
        }
        public override string ToString() => $"Default object: {(Message ?? "No message")}";
        public abstract class AbstractObjectReader<T> : INetworkReader where T : NetworkObject
        {
            protected const int CONTENTINDEX = 5;
            protected void DecodeHeader(NetworkObject obj, byte[] bytes)
            {
                obj.Command = (ServerCommands)bytes[0];
                obj.id = BitConverter.ToUInt32(bytes, 1);
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
                for (lastByte = startIndex; HasByte(bytes, lastByte); lastByte += byteSize) ;
                return lastByte;
            }
            protected bool HasByte(byte[] bytes, int index) => index < bytes.Length && bytes[index] != 0;
            public byte[] GenerateByteStream(NetworkObject obj)
            {
                if (!(obj is T)) throw new Exception("This reader is not ment for this type!");
                List<byte> bytes = Header(obj);
                return EncodeObject(obj as T, bytes);
            }

            public NetworkObject GenerateObject(byte[] bytes, bool serverSide) => DecodeObject(bytes, serverSide);
            protected string EncodeString(string str) => str.Replace("&", "&amp;").Replace(SEPERATOR.ToString(), "&#124;");
            protected string DecodeString(string str) => str.Replace("&#124;", SEPERATOR.ToString()).Replace("&amp;", "&");
            public abstract T DecodeObject(byte[] bytes, bool serverSide);
            public abstract byte[] EncodeObject(T obj, List<byte> header);
        }
        public class DefaultObjectReader : AbstractObjectReader<NetworkObject>
        {
            public override NetworkObject DecodeObject(byte[] bytes, bool serverSide)
            {
                var obj = new NetworkObject();
                DecodeHeader(obj, bytes);
                int lastByte = FindLastByte(bytes);
                if (lastByte > CONTENTINDEX)
                    obj.Message = Encoding.ASCII.GetString(bytes, CONTENTINDEX, lastByte - CONTENTINDEX);
                return obj;
            }   

            public override byte[] EncodeObject(NetworkObject obj, List<byte> header)
            {
                if (obj.Message != null)
                    header.AddRange(Encoding.ASCII.GetBytes(obj.Message));
                return header.ToArray();
            }
        }

    }

    
}
