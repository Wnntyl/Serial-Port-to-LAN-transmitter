using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SerialToLanTransmitter
{
    class UdpCommunicator : Communicator
    {
        public const string DEFAULT_IP = "127.0.0.1";
        public const int DEFAULT_REMOTE_PORT = 48801;
        public const int DEFAULT_LOCAL_PORT = 48800;

        public event Action<string> DataReceived;
        public event Action<string> LogThrown;

        private UdpClient _receivingUdpClient;
        private IPAddress _remoteIPAddress;
        private int _remotePort;
        private int _localPort;
        private bool _isRunning;

        public UdpCommunicator(Action<string> logReader = null)
        {
            LogThrown += logReader;
            Run(DEFAULT_IP, DEFAULT_REMOTE_PORT, DEFAULT_LOCAL_PORT);
        }

        public UdpCommunicator(string remoteIPAddress, int remotePort, int localPort, Action<string> logReader = null)
        {
            LogThrown += logReader;
            Run(remoteIPAddress, remotePort, localPort);
        }

        private void Run(string remoteIPAddress, int remotePort, int localPort)
        {
            if (!IPAddress.TryParse(remoteIPAddress, out _remoteIPAddress))
            {
                Log(string.Format("Can't parse \"{0}\" IP address! Therefore \"{1}\" now in use.", remoteIPAddress, DEFAULT_IP));
                _remoteIPAddress = IPAddress.Parse(DEFAULT_IP);
            }

            SetupPorts(remotePort, localPort);

            _isRunning = true;

            if(StartReceive())
                Log("Udp communication has started");
        }

        private void SetupPorts(int remotePort, int localPort)
        {
            if (remotePort > 0)
            {
                _remotePort = remotePort;
            }
            else
            {
                Log("Remote port must be greater then 0! Remote port value has been set to " + DEFAULT_REMOTE_PORT);
                _remotePort = DEFAULT_REMOTE_PORT;
            }

            if (localPort > 0)
            {
                _localPort = localPort;
            }
            else
            {
                Log("Local port must be greater then 0! Local port value has been set to " + DEFAULT_LOCAL_PORT);
                _localPort = DEFAULT_LOCAL_PORT;
            }

            if (_remotePort == localPort)
            {
                Log("Remote port is equal to local port! It's not allowed. Setting up defaul values.");
                _remotePort = DEFAULT_REMOTE_PORT;
                _localPort = DEFAULT_LOCAL_PORT;
            }
        }

        public override void Send(string datagram)
        {
            var endPoint = new IPEndPoint(_remoteIPAddress, _remotePort);
            var sender = new UdpClient();

            try
            {
                var bytes = Encoding.UTF8.GetBytes(datagram);
                sender.Send(bytes, bytes.Length, endPoint);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
            finally
            {
                sender.Close();
            }
        }

        private bool StartReceive()
        {
            try
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), _localPort);
                _receivingUdpClient = new UdpClient(ipEndPoint);
                var udpState = new UdpState()
                {
                    client = _receivingUdpClient,
                    ipEndPoint = ipEndPoint
                };

                _receivingUdpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                return false;
            }
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                var udpState = asyncResult.AsyncState as UdpState;
                var receiveBytes = udpState.client.EndReceive(asyncResult, ref udpState.ipEndPoint);
                udpState.client.Close();

                if (_isRunning)
                    StartReceive();

                var receiveData = Encoding.ASCII.GetString(receiveBytes);

                if (DataReceived != null)
                    DataReceived.Invoke(receiveData);
            }
            catch (ObjectDisposedException e)
            {
                //Log("Receiving has ended.");
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        public void Terminate()
        {
            _isRunning = false;

            try
            {
                if (_receivingUdpClient != null)
                    _receivingUdpClient.Close();
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
            DataReceived = null;

            Log("Udp communication is being terminated..");
        }

        public void Reconnect(string remoteIPAddress, int remotePort, int localPort)
        {
            Terminate();
            Run(remoteIPAddress, remotePort, localPort);
        }

        private void Log(string logMsg)
        {
            //Console.WriteLine("Log: " + logMsg);

            if (LogThrown == null)
                return;

            LogThrown.Invoke(logMsg);
        }
    }
}
