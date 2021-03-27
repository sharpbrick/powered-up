namespace SharpBrick.PoweredUp.Mobile
{
    public interface INativeDevice
    {
        string MacAddress { get; }

        ulong MacAddressNumeric { get; }
    }
}
