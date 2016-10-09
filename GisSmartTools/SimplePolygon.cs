using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSmartTools.Geometry
{

    /**
     * SimplePolygon
     * 表示一个简单多边形的Geometry子类
     * 要创建，请：
     * *1) 创建SimplePolyline列表
     * *2) 对每个环循环，在每个循环里：
     * ***a) 创建PointD列表
     * ***b) 向其中加入该环所有点
     * ***c) 利用该列表创建SimplePolyline
     * ***d) 添加到SimplePolyline列表
     * 3) 将该列表作为参数传递给构造函数
     * *************************************************/
    [Serializable]
    public class SimplePolygon : Geometry
    {
        public readonly List<SimplePolyline> rings;
        public readonly int ringCount;
        public readonly double minX, minY, maxX, maxY;

        public SimplePolygon(List<SimplePolyline> rings)
        {
            this.geometryType = OSGeo.OGR.wkbGeometryType.wkbPolygon;
            this.rings = rings;
            if (this.rings == null) this.rings = new List<SimplePolyline>();
            this.ringCount = this.rings.Count;

            if (this.ringCount == 0)
            {
                this.minX = 0;
                this.minY = 0;
                this.maxX = 0;
                this.maxY = 0;
                return;
            }
            double SP_minx, SP_miny, SP_maxx, SP_maxy;
            SP_maxx = this.rings[0].maxX;
            SP_maxy = this.rings[0].maxY;
            SP_minx = this.rings[0].minX;
            SP_miny = this.rings[0].minY;
            foreach (SimplePolyline SP_sline in this.rings)
            {
                if (SP_sline.maxX > SP_maxx) SP_maxx = SP_sline.maxX;
                if (SP_sline.minX < SP_minx) SP_minx = SP_sline.minX;
                if (SP_sline.maxY > SP_maxy) SP_maxy = SP_sline.maxY;
                if (SP_sline.minY < SP_miny) SP_miny = SP_sline.minY;
            }

            this.minX = SP_minx;
            this.minY = SP_miny;
            this.maxX = SP_maxx;
            this.maxY = SP_maxy;
        }
        public SimplePolygon() : this(null) { }

        public Rectangle getEnvelop()
        {
            return new Rectangle(this.minX,this.minY,this.maxX,this.maxY);
        }



       
    }
}
