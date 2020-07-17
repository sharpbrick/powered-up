using System;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public static class VersionNumberEncoder
    {
        // spec chapter: 3.5.6
        public static Version Decode(int data)
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

        public static int Encode(Version version)
        {
            if (version.Major > 7 || version.Minor > 15 || version.Build > 99 || version.Revision > 9999)
            {
                throw new ArgumentException(nameof(version));
            }

            return (version.Major << 28) +
                    (version.Minor << 24) +
                    ((version.Build / 10) << 20) + ((version.Build % 10) << 16) +
                    ((version.Revision / 1000) << 12) +
                    (((version.Revision % 1000) / 100) << 8) +
                    (((version.Revision % 100) / 10) << 4) +
                    (((version.Revision % 10) / 1) << 0);
        }
    }
}