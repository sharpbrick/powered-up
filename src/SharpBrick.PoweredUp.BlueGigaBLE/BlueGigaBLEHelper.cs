using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SharpBrick.PoweredUp.BlueGigaBLE
{
    /// <summary>
    /// Some static helper-functions needed in the BlueGigaBLE-Bluetooth-classes
    /// </summary>
    public class BlueGigaBLEHelper
    {
        /// <summary>
        /// Converts a GUID into a Byte[] without rotating some bytes like Miscorsoft does. the GUID.ToByteArray()-function reverts the order of some parts; 
        /// the Advertising needs it in correct order, so we have to do it on our own
        /// </summary>
        /// <param name="guid">The Guid to be converted</param>
        /// <returns></returns>
        public static byte[] ConvertUUIDToByteArray(Guid guid)
        {
            var result = new byte[16];
            var style = NumberStyles.HexNumber;
            var myguidstr = guid.ToString();
            myguidstr = myguidstr.Replace("-", "");
            for (var i = 0; i < 16; i++)
            {
                result[i] = byte.Parse(myguidstr.Substring(i * 2, 2), style);
            }
            return result;
        }

        /// <summary>
        /// Converts a Byte[] of a UUID (service/characteristic) into a GUID the way Microsoft works with them
        /// </summary>
        /// <param name="inbytes">the Byte[] holding the received UUID (little endian, no scrumble; direct from the line</param>
        /// <returns></returns>
        public static Guid ConvertByteArrayToGuid(byte[] inbytes)
        {
            Guid result;
            inbytes = inbytes.Reverse().ToArray();
            var reorderdBytes = new byte[16];
            //full UUIDs (128bit)
            if (inbytes.Length == 16)
            {
                reorderdBytes[0] = inbytes[3];
                reorderdBytes[1] = inbytes[2];
                reorderdBytes[2] = inbytes[1];
                reorderdBytes[3] = inbytes[0];
                reorderdBytes[4] = inbytes[5];
                reorderdBytes[5] = inbytes[4];
                reorderdBytes[6] = inbytes[7];
                reorderdBytes[7] = inbytes[6];
                for (var i = 8; i < 16; i++)
                {
                    reorderdBytes[i] = inbytes[i];
                }
            }
            //16-bit UUIDS
            //Or, to put it more simply, the 16 - bit Attribute UUID replaces the x’s in the following:
            //      0000xxxx - 0000 - 1000 - 8000 - 00805F9B34FB
            if (inbytes.Length == 2)
            {
                reorderdBytes = new byte[] { 0x00, 0x00, inbytes[1], inbytes[0], 0x00, 0x00, 0x10, 0x00, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB };
            }
            //32-bits UUIDS
            //In addition, the 32 - bit Attribute UUID replaces the x's in the following:
            //      xxxxxxxx - 0000 - 1000 - 8000 - 00805F9B34FB
            if (inbytes.Length == 4)
            {
                reorderdBytes = new byte[] { inbytes[3], inbytes[2], inbytes[1], inbytes[0], 0x00, 0x00, 0x10, 0x00, 0x80, 0x00, 0x00, 0x80, 0x5F, 0x9B, 0x34, 0xFB };
            }
            result = new Guid(reorderdBytes);
            return result;
        }

        /// <summary>
        /// Display a Byte[] in a Hexadecimal format with spaces
        /// </summary>
        /// <param name="ba">the byte[] to show as Hex</param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
            {
                _ = hex.AppendFormat("{0:x2} ", b);
            }
            return hex.ToString();
        }

        /// <summary>
        /// Convert a ulong into a Byte-Array
        /// </summary>
        /// <param name="myulong">the ulong to be converted</param>
        /// <returns></returns>
        public static byte[] UlongTo6ByteArray(ulong myulong)
        {
            var result = new byte[6];
            for (var i = 0; i < 6; i++)
            {
                result[i] = (byte)(myulong % 256);
                myulong /= 256;
            }
            return result;
        }

        /// <summary>
        /// Convert a ulong into a BYte-Array
        /// </summary>
        /// <param name="myulong">the ulong to be converted</param>
        /// <returns></returns>
        public static ulong ByteArrayToUlong(byte[] mybytes)
        {
            ulong result = 0;
            if (mybytes.Length != 6)
            {
                throw new Exception("ByteArray for function ByteArrayToUlong must be excatly 6 bytes long!");
            }
            for (var i = 0; i < 6; i++)
            {
                result += mybytes[i] * (ulong)Math.Pow(256, i);
            }
            return result;
        }

        /// <summary>
        /// Convert a Byte[] into a String of decimals separted by spaces
        /// </summary>
        /// <param name="ba">the Byte[] to be converted</param>
        /// <returns></returns>
        public static string ByteArrayToNumberString(byte[] ba)
        {
            var number = new StringBuilder();
            foreach (var b in ba)
            {
                _ = number.AppendFormat("{0:D} ", b);
            }
            return number.ToString();
        }
    }
}
