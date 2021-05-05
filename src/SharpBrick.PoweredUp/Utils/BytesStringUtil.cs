using System;
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

        public static string ToBitString(ushort data)
            => new(Enumerable.Range(0, 16).Reverse().Select(idx => (((1 << idx) & data) > 0) ? '1' : '0').ToArray());

        public static string ByteArrayToHexString(byte[] byteArray, string separator = ":")
            => string.Join(separator, byteArray.Select(b => b.ToString("X2")).ToArray());

        public static byte[] HexStringToByteArray(string hexString, string separator = ":")
            => hexString.Split(separator).Select(x => Convert.ToByte(x, 16)).ToArray();
        
        public static ulong HexStringToUInt64(string hexString, string separator = ":")
            => Convert.ToUInt64(hexString.Replace(separator, ""), 16);

        /// <summary>
        /// Convert a UInt64 into a Byte Array while keeping endianess out of the game.
        /// </summary>
        /// <param name="myulong">the ulong to be converted</param>
        /// <returns></returns>
        public static byte[] UInt64MacAddressToByteArray(ulong myulong)
        {
            var result = new byte[6];
            for (var i = 5; i >= 0; i--)
            {
                result[i] = (byte)(myulong % 256);
                myulong /= 256;
            }
            return result;
        }

    }
}