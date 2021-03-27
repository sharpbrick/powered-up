namespace SharpBrick.PoweredUp.Mobile.Examples.Droid
{
    public class NativeDevice : INativeDevice
    {
        public string MacAddress { get; internal set; }
        public ulong MacAddressNumeric { get; internal set; }
    }
}