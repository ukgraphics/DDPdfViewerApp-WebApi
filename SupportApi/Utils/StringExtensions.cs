using System;
using System.Collections.Generic;
using System.Text;

namespace SupportApi.Utils
{
    enum BooleanAliases
    {
        TRUE = 1,
        YES = 1,
        ON = 1,
        DA = 1,
        FALSE = 0,
        OFF = 0,
        NO = 0,
        NET = 0
    }

    public static class StringExtensions
    {

        public static bool ToBoolean(this string str, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(str)) return defaultValue;
            if (bool.TryParse(str, out bool boolVal)) return boolVal;
            if (Enum.TryParse(str.ToUpperInvariant(), out BooleanAliases val)) return Convert.ToBoolean((int)val);
            return defaultValue;
        }

    }
}
