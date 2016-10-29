using System;
using System.Windows;


namespace SerialToLanTransmitter
{
    /// <summary>
    /// Interaction logic for TransmissionTestWindow.xaml
    /// </summary>
    public partial class TransmissionTestWindow : Window
    {
        public event Action<string, int, float> SendDataRequest;

        public TransmissionTestWindow()
        {
            InitializeComponent();
            messageTextBox.Focus();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            var message = messageTextBox.Text;

            int count = 0;
            if (!int.TryParse(countTextBox.Text, out count))
                count = 1;

            float delay = 0f;
            if (!float.TryParse(delayTextBox.Text, out delay))
                delay = 0.1f;

            SendDataRequest?.Invoke(message, count, delay);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SendDataRequest = null;
        }
    }
}
