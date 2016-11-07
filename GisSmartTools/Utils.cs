using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using OSGeo.OGR;
using OSGeo.OSR;
using System.IO;
namespace GisSmartTools.Data
{
    class Utils
    {
        public static GisLog gislog = new GisLog();
        public static double double_max = 9999999;
        public static double double_min = -9999999;
        //静态数据区
        //路径信息
        public static string __HOMEPATH__ = "e:/smartgis/";
        public static string __LOGPATH__ = __HOMEPATH__ + "log.txt";
        public static string __SPATIALREFERENCTPATH__ = __HOMEPATH__ + "reference.txt";
        

        //全局数据信息
        //public static List<SpatialReference> spatialreferences = new List<SpatialReference>();

        
        
        public static Color GetRandomColor()
        {
            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);
            //  对于C#的随机数，没什么好说的
            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);

            //  为了在白色背景上显示，尽量生成深色
            int int_Red = RandomNum_First.Next(256);
            int int_Green = RandomNum_Sencond.Next(256);
            int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;

            return new Color (255, (byte)int_Red, (byte)int_Green, (byte)int_Blue);
        }
        public static int colorindex = 0;
        public static Color[] colorarray = new Color[]
        {
            Colors.Black,
            Colors.DarkCyan,
            Colors.DarkMagenta,
            Colors.DeepPink,
            Colors.Indigo,
            Colors.AliceBlue,
            Colors.Aqua,
            Colors.Azure,
            Colors.BlanchedAlmond,
            Colors.BurlyWood,
            Colors.LightSeaGreen,
            Colors.Moccasin,
            Colors.Orange,
            Colors.Wheat,
            Colors.Yellow,
            Colors.IndianRed,
            Colors.Green,
            Colors.LightBlue,
            Colors.Gold,
            Colors.MediumSlateBlue,
            Colors.Cyan,
            Colors.PowderBlue,
            Colors.Khaki
        };
        public static Color GetDifferentColorbyList()
        {
            colorindex = (colorindex + 1) % colorarray.Length;
            return colorarray[colorindex];
        }


        public static Color[] GetLinearColorList(Color startcolor,Color endcolor,int num)
        {
            System.Drawing.Color start = startcolor;
            System.Drawing.Color end = endcolor;
            Color[] colorarray = new Color[num];

            double start_h, start_s, start_v;
            double end_h, end_s, end_v;
            ColorToHSV(start, out start_h, out start_s, out start_v);
            ColorToHSV(end, out end_h, out end_s, out end_v);

            double h = (end_h - start_h) / (num - 1);
            double s = (end_s - start_s) / (num - 1);
            double v = (end_v - start_v) / (num - 1);
            //int internal_A = (endcolor.a - startcolor.a) / (num - 1);
            //int internal_R = (endcolor.r- startcolor.r) / (num - 1);
            //int internal_G = (endcolor.g - startcolor.g) / (num - 1);
            //int internal_B = (endcolor.b - startcolor.b) / (num - 1);
            colorarray[0] = startcolor;
            colorarray[num - 1] = endcolor;
            for(int i = 1;i<num-1;i++)
            {
                colorarray[i] = ColorFromHSV(start_h + i * h, start_s + i * s, start_v + i * v);
                //colorarray[i] = new Color((byte)(startcolor.a + i * internal_A),
                //    (byte)(startcolor.r+i*internal_R), (byte)(startcolor.g+i*internal_G), (byte)(startcolor.b+i*internal_B));
            }
            return colorarray;
        }

        public static void ColorToHSV(System.Drawing.Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new Color(255, v, t, p);
            else if (hi == 1)
                return new Color(255, q, v, p);
            else if (hi == 2)
                return new Color(255, p, v, t);
            else if (hi == 3)
                return new Color(255, p, q, v);
            else if (hi == 4)
                return new Color(255, t, p, v);
            else
                return new Color(255, v, p, q);
        }


    }
}
