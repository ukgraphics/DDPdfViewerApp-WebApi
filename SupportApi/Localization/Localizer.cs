using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SupportApi.Localization
{
    public sealed class Localizer
    {

        public static ErrorMessages GetErrorMessages(string currentCulture = null)
        {
            if (string.IsNullOrEmpty(currentCulture))
                currentCulture = CultureInfo.CurrentCulture.Name;
            if (currentCulture == "ja-JP")
                return new ErrorMessages_Ja();
            else
                return new ErrorMessages();
        }
    }
}
