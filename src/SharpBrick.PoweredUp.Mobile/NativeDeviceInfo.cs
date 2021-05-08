namespace SharpBrick.PoweredUp.Mobile
{
    public class NativeDeviceInfo
    {
        /// <summary>
        /// In Android, the MacAddressString will be returned
        /// In iOS the unique identifier is used
        /// </summary>
        public string DeviceIdentifier { get; set; }
    }
}