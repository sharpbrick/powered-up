namespace SharpBrick.PoweredUp.Mobile
{
    public interface INativeDeviceInfoProvider
    {
        NativeDeviceInfo GetNativeDeviceInfo(object device);
    }
}