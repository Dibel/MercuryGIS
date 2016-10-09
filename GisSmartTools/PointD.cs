using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
namespace GisSmartTools.Geometry
{
    /**
     * PointD
     * 以double坐标表示一个点的Geometry子类
     * */
    [Serializable]
    public class PointD : Geometry
    {
        public double X;
        public double Y;

        public PointD(double x, double y)
        {
            this.geometryType = OSGeo.OGR.wkbGeometryType.wkbPoint;
            this.X = x;
            this.Y = y;
        }
        public PointD() : this(0, 0) { }

        public PointD Clone()
        {
            return new PointD(this.X,this.Y);
        }

       

    };
}
