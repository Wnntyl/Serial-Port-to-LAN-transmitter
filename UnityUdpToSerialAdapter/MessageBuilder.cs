using System;

namespace SerialToLanTransmitter
{
    class MessageBuilder
    {
        private readonly char [] _trimChars = new char[] { ' ', '\n', '\r' };

        public string Build(string rawMessage)
        {
            var message = rawMessage.Trim(_trimChars);
            message = string.Format( "[MESSAGE at {0}]\r{1}", DateTime.Now, message);

            return message;
        }
    }
}
