namespace SharpBrick.PoweredUp.Mobile
{
    public class NativeDeviceInfo
    {
        /// <summary>
        /// In Android, the MacAddressString will be returned
        /// In iOS the unique identifier is used
        /// </summary>
        public string DeviceIdentifier { get; set; }

        public byte[] MacAddress { get; set; }
     
        public string MacAddressString { get; set; }

        public ulong MacAddressNumeric { get; set; }
    }
}