using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSmartTools.Geometry
{
    /**CirCle
     * 用于表示圆的Geometry子类
     * */
    [Serializable]
    public class Circle : Geometry
    {
        public PointD center;
        public double radius;

        public Circle(double x, double y, double r)
        {
            this.geometryType = OSGeo.OGR.wkbGeometryType.wkbUnknown;
            this.center = new PointD(x,y);
            this.radius = r;
        }
        public Circle() : this(0, 0, 0) { }

        public Circle Clone()
        {
            return new Circle(this.center.X, this.center.Y, this.radius);
        }
       
    }
}
