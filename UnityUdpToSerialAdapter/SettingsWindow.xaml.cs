using System.Windows;
using System;

namespace SerialToLanTransmitter
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private const string WINDOW_NAME = "Settings";

        public event Action ReopenSerialPortThrown;
        public event Action ReconnectPortThrown;

        public SettingsWindow()
        {
            InitializeComponent();
            ShowSettings();
            Title = WINDOW_NAME;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void ShowSettings()
        {
            remoteIpTextBox.Text = SettingsManager.Instance.data.remoteIp;
            remotePortTextBox.Text = SettingsManager.Instance.data.remotePort.ToString();
            localPortTextBox.Text = SettingsManager.Instance.data.localPort.ToString();
            serialPortTextBox.Text = SettingsManager.Instance.data.serialPort;
            outputCheckBox.IsChecked = SettingsManager.Instance.data.outputMessages;
        }

        private void SaveSettings()
        {
            SettingsManager.Instance.data.remoteIp = remoteIpTextBox.Text;
            SettingsManager.Instance.SetRemotePort(remotePortTextBox.Text);
            SettingsManager.Instance.SetLocalPort(localPortTextBox.Text);
            SettingsManager.Instance.data.serialPort = serialPortTextBox.Text;
            SettingsManager.Instance.data.outputMessages = (outputCheckBox.IsChecked == true);

            SettingsManager.Instance.SaveData();
            Title = WINDOW_NAME;
        }

        private void SettingChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Title = WINDOW_NAME + "*";
        }

        private void OutputSettingChanged(object sender, RoutedEventArgs e)
        {
            Title = WINDOW_NAME + "*";
        }

        private void reopenSerialPortButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.data.serialPort = serialPortTextBox.Text;

            if (ReopenSerialPortThrown != null)
                ReopenSerialPortThrown.Invoke();
        }

        private void reconnectButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Instance.data.remoteIp = remoteIpTextBox.Text;
            SettingsManager.Instance.SetRemotePort(remotePortTextBox.Text);
            SettingsManager.Instance.SetLocalPort(localPortTextBox.Text);

            if (ReconnectPortThrown != null)
                ReconnectPortThrown.Invoke();
        }

        private void settingsWindow_Closed(object sender, EventArgs e)
        {
            ReopenSerialPortThrown = null;
            ReconnectPortThrown = null;
        }
    }
}
