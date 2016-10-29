using System;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Concurrent;
using System.Threading;

namespace SerialToLanTransmitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int COMMUNICATION_THREAD_SLEEP_TIME = 10;
        private UdpCommunicator _udpCommunicator;
        private SerialCommunicator _serialCommunicator;
        private MessageBuilder _messageBuilder = new MessageBuilder();
        private int _messageCount;
        private ConcurrentQueue<string> _udpQueue = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> _serialQueue = new ConcurrentQueue<string>();
        private bool _run = true;

        private TransmissionTestWindow _lanTransmissionWindow;
        private TransmissionTestWindow _comTransmissionWindow;
        private SettingsWindow _settingsWindow;

        private SendHelper _lanSendHelper;
        private SendHelper _serialSendHelper;

        public static MainWindow instance;

        public MainWindow()
        {
            InitializeComponent();
            instance = this;

            SettingsManager.Instance.LoadData();

            var tQueues = new Thread(new ThreadStart(QueuesHandler));
            tQueues.SetApartmentState(ApartmentState.STA);
            tQueues.IsBackground = true;
            tQueues.Start();

            _udpCommunicator = new UdpCommunicator(SettingsManager.Instance.data.remoteIp, SettingsManager.Instance.data.remotePort, SettingsManager.Instance.data.localPort, ShowMessage);
            _udpCommunicator.DataReceived += EnqueueLanMessage;

            _serialCommunicator = new SerialCommunicator(SettingsManager.Instance.data.serialPort, ShowMessage);
            _serialCommunicator.DataReceived += EnqueueSerialPortMessage;

            _lanSendHelper = new SendHelper(_udpCommunicator);
            _serialSendHelper = new SendHelper(_serialCommunicator);
        }

        private void EnqueueLanMessage(string message)
        {
            _udpQueue.Enqueue(message);
        }

        private void EnqueueSerialPortMessage(string message)
        {
            _serialQueue.Enqueue(message);
        }

        private void QueuesHandler()
        {
            while (_run)
            {
                string messageFromLAN = "";
                if (_udpQueue.TryDequeue(out messageFromLAN))
                {
                    if (!string.IsNullOrEmpty(messageFromLAN))
                    {
                        ShowMessage(messageFromLAN);
                        _serialSendHelper.Send(messageFromLAN);
                    }
                }

                string messageFromSerialPort = "";
                if (_serialQueue.TryDequeue(out messageFromSerialPort))
                {
                    if (!string.IsNullOrEmpty(messageFromSerialPort))
                    {
                        ShowMessage(messageFromSerialPort);
                        _lanSendHelper.Send(messageFromSerialPort);
                    }
                }

                Thread.Sleep(COMMUNICATION_THREAD_SLEEP_TIME);
            }
        }

        public void ShowMessage(string message)
        {
            //Console.WriteLine("ShowMessage: " + message);
            if (!SettingsManager.Instance.data.outputMessages)
                return;

            Action<string> showMsg = (msg) =>
            {
                //Console.WriteLine("showMsg: " + message);
                msg = _messageBuilder.Build(msg);

                logTextBox.AppendText(msg);
                logTextBox.AppendText(Environment.NewLine);

                if (_messageCount < int.MaxValue)
                    _messageCount++;
                SetMessageCount(_messageCount);
            };

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<string>(showMsg), message);
        }

        public static void Log(string message)
        {
            if (instance == null)
                return;

            instance.ShowMessage(message);
        }

        private void SetMessageCount(int count)
        {
            _messageCount = count;

            if (messageCounterLabel == null)
                return;

            messageCounterLabel.Content = "Message count: " + _messageCount;
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _run = false;

            if (_udpCommunicator != null)
                _udpCommunicator.Terminate();

            if (_serialCommunicator != null)
                _serialCommunicator.Terminate();
        }

        private void myIPMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string message = "";

            var ips = Informator.GetAllLocalIPv4(NetworkInterfaceType.Ethernet);
            AddIpsToMessage("Ethernet IPs:", ips, ref message);

            ips = Informator.GetAllLocalIPv4(NetworkInterfaceType.Wireless80211);
            AddIpsToMessage("Wireless IPs:", ips, ref message);

            ShowMessage(message);
            logTextBox.ScrollToEnd();
        }

        private void AddIpsToMessage(string title, string[] ips, ref string message)
        {
            if (ips != null && ips.Length > 0)
            {
                message += title + "\r";

                foreach (var ip in ips)
                    message += " - " + ip + "\r";
            }
        }

        private void logTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (autoscrollCheckBox == null)
                return;

            if (autoscrollCheckBox.IsChecked == true)
                logTextBox.ScrollToEnd();
        }

        private void lanTestMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_lanTransmissionWindow == null || !_lanTransmissionWindow.IsVisible)
            {
                _lanTransmissionWindow = new TransmissionTestWindow();
                _lanTransmissionWindow.Owner = this;
                _lanTransmissionWindow.Title = "LAN transmission test";
                var sendHelper = new SendHelper(_udpCommunicator);
                _lanTransmissionWindow.SendDataRequest += sendHelper.Send;
                _lanTransmissionWindow.Show();
            }

            _lanTransmissionWindow.Focus();
        }

        private void serialPortTestMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_comTransmissionWindow == null || !_comTransmissionWindow.IsVisible)
            {
                _comTransmissionWindow = new TransmissionTestWindow();
                _comTransmissionWindow.Owner = this;
                _comTransmissionWindow.Title = "COM transmission test";
                var sendHelper = new SendHelper(_serialCommunicator);
                _comTransmissionWindow.SendDataRequest += sendHelper.Send;
                _comTransmissionWindow.Show();
            }

            _comTransmissionWindow.Focus();
        }

        private void settingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow == null || !_settingsWindow.IsVisible)
            {
                _settingsWindow = new SettingsWindow();
                _settingsWindow.Owner = this;
                _settingsWindow.ReopenSerialPortThrown += ReopenSerialPort;
                _settingsWindow.ReconnectPortThrown += ReconnectLan;
                _settingsWindow.Show();
            }

            _settingsWindow.Focus();
        }

        private void ReopenSerialPort()
        {
            if (_serialCommunicator == null)
                return;

            _serialCommunicator.OpenPort(SettingsManager.Instance.data.serialPort);
        }

        private void ReconnectLan()
        {
            if (_udpCommunicator == null)
                return;

            _udpCommunicator.Reconnect(SettingsManager.Instance.data.remoteIp, SettingsManager.Instance.data.remotePort, SettingsManager.Instance.data.localPort);
            _udpCommunicator.DataReceived += EnqueueLanMessage;
        }

        private void serialPortsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ports = Informator.GetAllSerialPorts();

            var info = "Actual serial ports:\r";

            foreach (var portName in ports)
            {
                info += string.Format(" - {0}\r", portName);
            }

            ShowMessage(info);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            if (logTextBox == null)
                return;

            logTextBox.Document.Blocks.Clear();

            SetMessageCount(0);
        }
    }
}
