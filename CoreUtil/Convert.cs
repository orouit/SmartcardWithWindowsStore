/**
 * @author Olivier ROUIT
 * 
 * @license CPL, CodeProject license 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Util
{
    public static class Convert
    {
        public static string HexToString(byte hexValue)
        {
            return string.Format("{0:X02}", hexValue);
        }

        public static byte ParseByteToHex(string hexStr)
        {
            return byte.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);
        }

        public static string BufferToString(byte[] data)
        {
            StringBuilder text = new StringBuilder();

            if (data != null)
            {
                foreach (byte bData in data)
                {
                    text.AppendFormat("{0:X02}", bData);
                }
            }

            return text.ToString();
        }

        public static byte[] ParseBufferToHex(string data)
        {
            List<byte> buffer = new List<byte>();

            if (!string.IsNullOrEmpty(data))
            {
                StringBuilder bufferBld = new StringBuilder();

                foreach (char cData in data)
                {
                    if (cData != ' ')
                    {
                        bufferBld.Append(cData);
                    }
                }

                string bufferStrTmp = bufferBld.ToString();
                string bufferStr = bufferStrTmp.Length % 2 != 0 ? bufferStrTmp.Substring(0, bufferStrTmp.Length - 1) : bufferStrTmp;

                for (int n = 0; n < bufferStr.Length; n += 2)
                {
                    buffer.Add(ParseByteToHex(bufferStr.Substring(n, 2)));
                }
            }

            return buffer.ToArray();
        }
    }
}
