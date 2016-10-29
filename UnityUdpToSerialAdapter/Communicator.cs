namespace SerialToLanTransmitter
{
    abstract class Communicator
    {
        public abstract void Send(string message);
    }
}
