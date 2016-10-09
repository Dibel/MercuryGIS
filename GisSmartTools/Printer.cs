using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GisSmartTools.Test
{
    public class Printer
    {
        public static void printnode(String node, String msg)
        {
            FileStream fs = new FileStream("e:/test.txt", FileMode.Append);
            StreamWriter writer = new StreamWriter(fs);
            writer.WriteLine (node + ":" + msg);
            writer.Flush();
            writer.Close();
        }
    }
}
