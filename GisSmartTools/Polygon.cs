using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GisSmartTools.Geometry
{
    /**
     * Polygon
     * 表示一个复杂折线的Geometry子类
     * 要创建，请：
     * *1) 创建SimplePolygon列表
     * *2) 对每个简单多边形循环，在每个循环里：
     * ***a) 生成和完成对应的简单多边形
     * ***b) 添加到SimplePolygon列表
     * 3) 将该列表作为参数传递给构造函数
     * */
    public class Polygon : Geometry
    {
        public readonly List<SimplePolygon> childPolygons;
        public readonly int childCount;
        public readonly double maxX, maxY, minX, minY;

        public Polygon(List<SimplePolygon> childPolygons)
        {
            this.geometryType = OSGeo.OGR.wkbGeometryType.wkbMultiPolygon;
            this.childPolygons = childPolygons;
            if (this.childPolygons == null) this.childPolygons = new List<SimplePolygon>();
            this.childCount = this.childPolygons.Count;

            if (this.childCount == 0)
            {
                this.minX = 0;
                this.minY = 0;
                this.maxX = 0;
                this.maxY = 0;
                return;
            }
            double SP_minx, SP_miny, SP_maxx, SP_maxy;
            SP_maxx = this.childPolygons[0].maxX;
            SP_maxy = this.childPolygons[0].maxY;
            SP_minx = this.childPolygons[0].minX;
            SP_miny = this.childPolygons[0].minY;
            foreach (SimplePolygon SP_line in this.childPolygons)
            {
                if (SP_line.maxY > SP_maxy)SP_maxy=SP_line.maxY;
                if (SP_line.maxX > SP_maxx)SP_maxx=SP_line.maxX;
                if(SP_line.minY<SP_miny)SP_miny=SP_line.minY;
                if(SP_line.minX<SP_minx)SP_minx=SP_line.minX;
            }
            this.minX = SP_minx;
            this.minY = SP_miny;
            this.maxX = SP_maxx;
            this.maxY = SP_maxy;
        }
        public Polygon() : this(null) { }

        public SimplePolygon GetChild(int index)
        {
            return this.childPolygons[index];
        }

        public Rectangle getEnvelop()
        {
            return new Rectangle(this.minX,this.minY,this.maxX,this.maxY);
        }

    }
}
