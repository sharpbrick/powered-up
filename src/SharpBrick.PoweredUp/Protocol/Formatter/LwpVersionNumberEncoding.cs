using System;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public static class LwpVersionNumberEncoding
    {
        // spec chapter: 3.5.7
        public static Version DecodeVersion(byte byte0, byte byte1)
        {
            var major = (byte1 >> 4) * 10 + (byte1 & 0b0000_1111);
            var minor = (byte0 >> 4) * 10 + (byte0 & 0b0000_1111);

            return new Version(major, minor);
        }
    }
}