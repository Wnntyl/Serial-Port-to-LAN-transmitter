using System;
using System.Threading.Tasks;
using System.Threading;

namespace SerialToLanTransmitter
{
    class SendHelper
    {
        private Communicator _communicator;

        public SendHelper(Communicator communicator)
        {
            _communicator = communicator;
        }

        public void Send(string message, int count, float delay)
        {
            if (_communicator == null)
                return;

            var data = new SendObject()
            {
                message = message,
                count = count,
                delay = delay
            };

            var task = new Task(SendAll, data);
            task.Start();
        }

        public void Send(string message)
        {
            if (_communicator == null)
                return;

            _communicator.Send(message);
        }

        private void SendAll(Object data)
        {
            var sendData = data as SendObject;

            for (var i = 0; i < sendData.count; i++)
            {
                _communicator.Send(sendData.message);
                var sleepTime = (int)(1000 * sendData.delay);
                Thread.Sleep(sleepTime);
            }
        }

        class SendObject
        {
            public string message;
            public int count;
            public float delay;
        }
    }
}
