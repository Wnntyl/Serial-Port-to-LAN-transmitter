using System.Net;
using System.Net.Sockets;

namespace SerialToLanTransmitter
{
    class UdpState
    {
        public UdpClient client;
        public IPEndPoint ipEndPoint;
    }
}
