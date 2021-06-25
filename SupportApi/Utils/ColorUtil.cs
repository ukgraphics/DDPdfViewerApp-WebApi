using System.Drawing;
using System.Globalization;


namespace SupportApi.Utils
{
    public class ColorUtil
    {
        public static float[] ColorToRgba(Color color)
        {
            return new float[] { color.R, color.G, color.B, color.A / 255f };
        }
        public static string ColorToRgbCss(Color color)
        {
            var arr = ColorToRgba(color);
            if (arr[3] == 1)
            {
                return $"rgb({arr[0].ToString(CultureInfo.InvariantCulture)},{arr[1].ToString(CultureInfo.InvariantCulture)},{arr[2].ToString(CultureInfo.InvariantCulture)})";
            }
            else
            {
                return $"rgba({arr[0].ToString(CultureInfo.InvariantCulture)},{arr[1].ToString(CultureInfo.InvariantCulture)},{arr[2].ToString(CultureInfo.InvariantCulture)},{arr[3].ToString(CultureInfo.InvariantCulture)})";
            }
            
        }


        public static Color? HexToColor(string hexColor, Color? defaultValue = null)
        {
            if (hexColor.Length < 2)
            {
                return defaultValue;
            }
            if (hexColor.StartsWith("rgb"))
            {
                hexColor = hexColor.Substring(hexColor.IndexOf("(") + 1);
                hexColor = hexColor.Replace(")", "");
                var result = hexColor.Split(new char[] { ',' });
                int hr = result.Length > 0 ? int.Parse(result[0], CultureInfo.InvariantCulture) : 0;
                int hg = result.Length > 1 ? int.Parse(result[1], CultureInfo.InvariantCulture) : 0;
                int hb = result.Length > 2 ? int.Parse(result[2], CultureInfo.InvariantCulture) : 0;
                float ha = result.Length > 3 ? float.Parse(result[3], CultureInfo.InvariantCulture) : 1.0f;
                return Color.FromArgb((int)(ha * 255f), hr, hg, hb);
            }
            hexColor = hexColor.Replace("#", "");
            // 6桁か確認
            while (hexColor.Length < 6)
                hexColor = $"{hexColor}${hexColor[hexColor.Length - 1]}";
            // alphaが指定されているか確認
            while (hexColor.Length < 8)
                hexColor = $"{hexColor}F";
            int argb = int.Parse(hexColor, NumberStyles.HexNumber);
            var a = (byte)(argb & 0xff);
            var r = (byte)((argb & -16777216) >> 0x18);
            var g = (byte)((argb & 0xff0000) >> 0x10);
            var b = (byte)((argb & 0xff00) >> 8);
            Color c = Color.FromArgb(a, r, g, b);
            return c;
        }
    }
}
