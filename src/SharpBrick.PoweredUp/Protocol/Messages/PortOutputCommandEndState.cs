namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public enum PortOutputCommandEndState : byte
    {
        Float = 0,
        Hold = 126,
        Brake = 127,
    }
}