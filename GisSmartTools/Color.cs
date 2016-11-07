using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GisSmartTools
{
    [Serializable]
    public class Color
    {
        public byte a = 255;
        public byte r = 255;
        public byte g = 255;
        public byte b = 255;
        public Color()
        {
            a = r = g = b = 255;
        }

        public Color(byte a, byte r, byte g, byte b)
        {
            this.a = a;
            this.r = r;
            this.g = g;
            this.b = b;
        }
        public static implicit operator Color (System.Windows.Media.Color input)
        {
            return new Color(input.A, input.R, input.G, input.B);
        }

        public static explicit operator System.Windows.Media.Color(Color input)
        {
            return System.Windows.Media.Color.FromArgb(input.a, input.r, input.g, input.b);
        }

        public static implicit operator System.Drawing.Color(Color input)
        {
            return System.Drawing.Color.FromArgb(input.a, input.r, input.g, input.b);
        }

        public static implicit operator int (Color input)
        {
            var col = 0;

            if (input.a != 0)
            {
                var a = input.a + 1;
                col = (input.a << 24)
                  | ((byte)((input.r * a) >> 8) << 16)
                  | ((byte)((input.g * a) >> 8) << 8)
                  | ((byte)((input.b * a) >> 8));
            }

            return col;
        }
    }
}
