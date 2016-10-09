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

            return Color.FromRgb((byte)int_Red, (byte)int_Green, (byte)int_Blue);
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
            Color[] colorarray = new Color[num];
            int internal_A = (endcolor.A - startcolor.A) / (num-1);
            int internal_R = (endcolor.R- startcolor.R) / (num - 1);
            int internal_G = (endcolor.G - startcolor.G) / (num - 1);
            int internal_B = (endcolor.B - startcolor.B) / (num - 1);
            colorarray[0] = startcolor;
            colorarray[num - 1] = endcolor;
            for(int i = 1;i<num-1;i++)
            {
                colorarray[i] = Color.FromArgb((byte)(startcolor.A + i * internal_A),
                    (byte)(startcolor.R+i*internal_R), (byte)(startcolor.G+i*internal_G), (byte)(startcolor.B+i*internal_B));
            }
            return colorarray;
        }

        
    }
}
