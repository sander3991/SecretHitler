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
        protected const int MSGINDEX = 5;
        protected const char SEPERATOR = '|';
        public uint ID { get; private set; }
        public ServerCommands Command { get; private set; }
        private byte[] bytes;
        public string Message { get; set; }
        public byte[] Bytes {
            get
            {
                if (bytes == null)
                    bytes = ConvertToBytes();
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
        private NetworkObject(byte[] bytes)
        {
            DecodeHeader(bytes);
            int lastByte;
            for (lastByte = MSGINDEX; lastByte < bytes.Length && bytes[lastByte] != 0; lastByte++) ;
            if(lastByte > MSGINDEX)
                Message = Encoding.ASCII.GetString(bytes, MSGINDEX, lastByte - MSGINDEX);
            this.bytes = bytes;
        }
        protected virtual byte[] ConvertToBytes()
        {
            List<byte> bytes = Header();
            if (Message != null)
                bytes.AddRange(Encoding.ASCII.GetBytes(Message));
            return bytes.ToArray();
        }
        protected List<byte> Header()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Command);
            bytes.AddRange(BitConverter.GetBytes(ID));
            return bytes;
        }
        protected void DecodeHeader(byte[] bytes)
        {
            Command = (ServerCommands)bytes[0];
            ID = BitConverter.ToUInt32(bytes, 1);

        }
        public void Send(Socket socket)
        {
            socket.Send(Bytes, Bytes.Length, SocketFlags.None);
        }
        public static NetworkObject Receive(Socket socket)
        {
            byte[] response = new byte[socket.SendBufferSize];
            socket.Receive(response);
            ServerCommands command = (ServerCommands)response[0];
            if (command != ServerCommands.Message)
                return new NetworkObject(response);
            else
                return new NetworkMessageObject(response);
        }
        protected string EncodeString(string str) => str.Replace("&", "&amp;").Replace(SEPERATOR.ToString(), "&#124;");
        protected string DecodeString(string str) => str.Replace("&#124;", SEPERATOR.ToString()).Replace("&amp;", "&");
    }

    public class NetworkMessageObject : NetworkObject
    {
        public string Username { get; set; }
        public NetworkMessageObject(string username, string message)
            : base(ServerCommands.Message, message)
        {
            Username = username;
        }
        public NetworkMessageObject(byte[] bytes)
        {
            DecodeHeader(bytes);
            int lastByte = MSGINDEX;
            for (lastByte = MSGINDEX; lastByte < bytes.Length && bytes[lastByte] != 0; lastByte += 4) ; //+= 4 UTF32
            var str = Encoding.UTF32.GetString(bytes, MSGINDEX, lastByte - 5);
            var split = str.Split(SEPERATOR);
            Username = split[0];
            Message = split[1];
        }
        protected override byte[] ConvertToBytes()
        {
            List<byte> bytes = Header();
            bytes.AddRange(Encoding.UTF32.GetBytes($"{EncodeString(Username)}{SEPERATOR}{EncodeString(Message)}"));
            return bytes.ToArray();
        }
    }
}
