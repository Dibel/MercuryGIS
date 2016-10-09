using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSmartTools.Geometry
{
    /**
     * Rectangle
     * 表示一个矩形的Geometry子类
     * */
    [Serializable]
    public class Rectangle : Geometry
    {
        public double minX, minY, maxX, maxY;

        public Rectangle(double minx, double miny, double maxx, double maxy)
        {
            this.geometryType = OSGeo.OGR.wkbGeometryType.wkbUnknown;
            this.minX = minx;
            this.minY = miny;
            this.maxX = maxx;
            this.maxY = maxy;
         
        }
        public Rectangle()
        {
            this.geometryType = OSGeo.OGR.wkbGeometryType.wkbUnknown;
        }

        public double getwidth()
        {
            return maxX - minX;
        }
        public double getheight()
        {
            return maxY - minY;
        }
        public PointD getcenter()
        {
            PointD point = new PointD((minX + maxX) / 2.00, (minY + maxY) / 2.00);
            return point;
        }
    
    }
}
