using System;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public static class PropertyVersionNumberEncoding
    {
        // spec chapter: 3.5.6
        public static Version DecodeVersion(int data)
        {
            var major = (data & 0b0111_0000___0000_0000___0000_0000___0000_0000) >> 28;
            var minor = (data & 0b0000_1111___0000_0000___0000_0000___0000_0000) >> 24;
            var bugfix = ((data & 0b0000_0000___1111_0000___0000_0000___0000_0000) >> 20) * 10 + ((data & 0b0000_0000___0000_1111___0000_0000___0000_0000) >> 16);
            var build = ((data & 0b0000_0000___0000_0000___1111_0000___0000_0000) >> 12) * 1000 +
                        ((data & 0b0000_0000___0000_0000___0000_1111___0000_0000) >> 8) * 100 +
                        ((data & 0b0000_0000___0000_0000___0000_0000___1111_0000) >> 4) * 10 +
                        ((data & 0b0000_0000___0000_0000___0000_0000___0000_1111) >> 0) * 1;

            return new Version(major, minor, bugfix, build);
        }
    }
}