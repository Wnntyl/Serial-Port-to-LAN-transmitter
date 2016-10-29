using System;

namespace SerialToLanTransmitter
{
    [Serializable]
    class SettingsData
    {
        public string remoteIp = UdpCommunicator.DEFAULT_IP;
        public int remotePort = UdpCommunicator.DEFAULT_REMOTE_PORT;
        public int localPort = UdpCommunicator.DEFAULT_LOCAL_PORT;
        public string serialPort = "COM1";
        public bool outputMessages = true;
    }
}