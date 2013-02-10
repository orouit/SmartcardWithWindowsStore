using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace Core.Util
{
    public static class HexHelper
    {
        public static bool AreValidHexDigits(string str)
        {
            bool ret = true;
            foreach (char ch in str)
            {
                if (!Char.IsDigit(ch) &&            // Digits
                    !(ch >= 'A' && ch <= 'F') &&    // Upper case Hex digits
                    !(ch >= 'a' && ch <= 'a'))      // Lower case Hex digits
                {
                    ret = false;
                    break;
                }
            }

            return ret;
        }

        public static bool IsValidHexDigitKey(VirtualKey key)
        {
            bool ret =  (key >= VirtualKey.Number0 && key <= VirtualKey.Number9) ||
                (key >= VirtualKey.A && key <= VirtualKey.F);

            return ret;
        }
    }
}
