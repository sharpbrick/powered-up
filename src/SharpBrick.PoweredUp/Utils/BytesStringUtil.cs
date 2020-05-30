using System.Globalization;
using System.Linq;

namespace SharpBrick.PoweredUp.Utils
{
    public static class BytesStringUtil
    {
        public static string DataToString(byte[] data)
            => string.Join("-", data.Select(b => $"{b:X2}"));
        public static byte[] StringToData(string messageAsString)
            => messageAsString.Split("-").Select(s => byte.Parse(s, NumberStyles.HexNumber)).ToArray();
    }
}