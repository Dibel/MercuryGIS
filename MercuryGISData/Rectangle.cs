using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercuryGISData
{
    public class Rectangle
    {
        public MPoint p1;
        public MPoint p2;
        public Rectangle(MPoint point1, MPoint point2)
        {
            p1 = point1;
            p2 = point2;
        }
    }
}
