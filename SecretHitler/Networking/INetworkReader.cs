using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHitler.Networking
{
    public interface INetworkReader
    {
        NetworkObject GenerateObject(byte[] bytes, bool serverSide);
        byte[] GenerateByteStream(NetworkObject obj);
    }
}
