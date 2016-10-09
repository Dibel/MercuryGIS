using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSmartTools.Geometry
{
    /**
     * SimplePolyline
     * 表示一个简单折线的Geometry子类
     * 要创建，请：
     * *创建PointD列表
     * *向其中加入所有点
     * *将该列表作为参数传递给构造函数
     * */
    [Serializable]
    public class SimplePolyline : Geometry
    {
        public readonly List<PointD> points;
        public readonly int pointCount;
        public readonly double minX, minY, maxX, maxY;

        public SimplePolyline(List<PointD> points)
        {
            this.geometryType = OSGeo.OGR.wkbGeometryType.wkbLineString;
            this.points = points;
            if(this.points==null)this.points = new List<PointD>();
            this.pointCount = this.points.Count;

            if (this.pointCount == 0)
            {
                this.minX = 0;
                this.minY = 0;
                this.maxX = 0;
                this.maxY = 0;
                return;
            }
            double SP_minx, SP_miny, SP_maxx, SP_maxy;
            SP_maxx = this.points[0].X;
            SP_maxy = this.points[0].Y;
            SP_minx = SP_maxx;
            SP_miny = SP_maxy;
            foreach (PointD SP_point in this.points)
            {
                if (SP_point.X > SP_maxx) SP_maxx = SP_point.X;
                if (SP_point.X < SP_minx) SP_minx = SP_point.X;
                if (SP_point.Y > SP_maxy) SP_maxy = SP_point.Y;
                if (SP_point.Y < SP_miny) SP_miny = SP_point.Y;
            }
            this.minX = SP_minx;
            this.minY = SP_miny;
            this.maxX = SP_maxx;
            this.maxY = SP_maxy;
        }
        public SimplePolyline() : this(null) { }

        public Rectangle getEnvelop()
        {
            return new Rectangle(this.minX,this.minY,this.maxX,this.maxY);
        }


    }
}
