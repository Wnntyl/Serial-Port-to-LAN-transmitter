using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.IO.Ports;

namespace SerialToLanTransmitter
{
    public static class Informator
    {
        public static string[] GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            var ipAddrList = new List<string>();
            foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            var ipAddress = ip.Address.ToString();
                            if(!string.IsNullOrEmpty(ipAddress))
                                ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }

            return ipAddrList.ToArray();
        }

        public static string[] GetAllSerialPorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}
