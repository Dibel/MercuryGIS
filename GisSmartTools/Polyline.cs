using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSmartTools.Geometry
{
    /**
     * Polyline
     * 表示一个复杂折线的Geometry子类
     * 要创建，请：
     * *1) 创建SimplePolyline列表
     * *2) 对每个单线循环，在每个循环里：
     * ***a) 创建PointD列表
     * ***b) 向其中加入该单线所有点
     * ***c) 利用该列表创建SimplePolyline
     * ***d) 添加到SimplePolyline列表
     * 3) 将该列表作为参数传递给构造函数
     * */
    [Serializable]
    public class Polyline : Geometry
    {
        public readonly List<SimplePolyline> childPolylines;
        public readonly int childCount;
        public readonly double minX, minY, maxX, maxY;

        public Polyline(List<SimplePolyline> childPolylines)
        {
            this.geometryType = OSGeo.OGR.wkbGeometryType.wkbMultiLineString;
            this.childPolylines = childPolylines;
            if (this.childPolylines == null) this.childPolylines = new List<SimplePolyline>();
            this.childCount = this.childPolylines.Count;

            if (this.childCount == 0)
            {
                this.minX = 0;
                this.minY = 0;
                this.maxX = 0;
                this.maxY = 0;
                return;
            }
            double SP_minx, SP_miny, SP_maxx, SP_maxy;
            SP_maxx = this.childPolylines[0].maxX;
            SP_maxy = this.childPolylines[0].maxY;
            SP_minx = this.childPolylines[0].minX;
            SP_miny = this.childPolylines[0].minY;
            foreach (SimplePolyline SP_line in this.childPolylines)
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
        public Polyline() : this(null) { }

        public SimplePolyline GetChild(int index)
        {
            return this.childPolylines[index];
        }

        public Rectangle getEnvelop()
        {
            return new Rectangle(this.minX,this.minY,this.maxX,this.maxY);
        }

      
    }
}
