using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace SerialToLanTransmitter
{
    class SerialCommunicator: Communicator
    {
        private const int RECEIVE_DELAY = 10;

        public event Action<string> DataReceived;
        public event Action<string> LogThrown;

        private SerialPort _serialPort;

        public SerialCommunicator(string portName, Action<string> logReader)
        {
            LogThrown += logReader;
            OpenPort(portName);
        }

        public void OpenPort(string serialPortName)
        {
            if (IsOpen)
            {
                var closeInfo = string.Format("Serial port \"{0}\" is open. Closing it..", _serialPort.PortName);
                Log(closeInfo);

                try
                {
                    _serialPort.Close();
                    Log("The port has been closed.");
                }
                catch (Exception e)
                {
                    Log(e.ToString());
                }
            }

            Log("Trying to open the serial port..");

            try
            {
                _serialPort = new SerialPort(serialPortName);
                _serialPort.BaudRate = 9600;
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;

                _serialPort.DataReceived += new SerialDataReceivedEventHandler(Receive);

                _serialPort.Open();
                var openInfo = string.Format("Serial port \"{0}\" has been opened.", _serialPort.PortName);
                Log(openInfo);
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        public void OpenPort()
        {
            if (_serialPort == null)
                return;

            OpenPort(_serialPort.PortName);
        }

        public bool IsOpen
        {
            get
            {
                if (_serialPort == null)
                    return false;

                return _serialPort.IsOpen;
            }
        }

        void Receive(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(RECEIVE_DELAY);
            var data = _serialPort.ReadLine();
            if (DataReceived != null)
                DataReceived.Invoke(data);
        }

        public override void Send(string message)
        {
            try
            {
                if (IsOpen)
                    _serialPort.Write(message);
                else
                    Log("Can't send the message. A serial port isn't open.");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void Log(string logMsg)
        {
            if (LogThrown == null)
                return;

            LogThrown.Invoke(logMsg);
        }

        public void Terminate()
        {
            if (IsOpen)
                _serialPort.Close();

            DataReceived = null;
            LogThrown = null;
        }
    }
}
