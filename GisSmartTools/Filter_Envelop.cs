using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Geometry;
using GisSmartTools.RS;
using GisSmartTools.Data;
using GisSmartTools.Topology;
using GisSmartTools.Test;
namespace GisSmartTools.Filter
{
    /**
     * Filter_Envelop
     * 专用于框选算法的Filter
     * 在投影坐标系上进行
     * 待完成方法：
     * Evaluate
     * */
    [Serializable]
    public class Filter_Envelop :Filter
    {
        public   Rectangle rect;      //地图坐标系中的rect
        public  RSTransform rsTransfrom;    //坐标系转换器
        public Filter_Envelop(Rectangle rect, RSTransform rstransfrom)
        {
            this.rect = rect;
            this.rsTransfrom = rstransfrom;
        }

        public string GetDescription()
        {
            return "Envelop";
        }
        public Boolean Evaluate(Feature feature)
        {
            if (feature.geometry == null) return false;
            switch (feature.geometry.geometryType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    PointD point_data = (PointD)feature.geometry;
                    PointD point_map = this.rsTransfrom.sourceToTarget(point_data);
                    return (SpatialTopology.IsPointInRectangle(point_map, this.rect));

                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    SimplePolygon polygon_data = (SimplePolygon)feature.geometry;
                    return (checksimplepolygon(polygon_data));

                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    SimplePolyline line_data = (SimplePolyline)feature.geometry;
                    return (checkpolyline(line_data));

                case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
                    Polygon mutipolygon_data = (Polygon)feature.geometry;
                    return (checkmutipolygon(mutipolygon_data));
                case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                    break;
            }
            //to do
            return false;
        }

        public bool checksimplepolygon(SimplePolygon polygon_data)
        {
            List<PointD> ring0_points_data = polygon_data.rings[0].points;
            List<PointD> ring0_points_map = new List<PointD>();
            for (int i = 0; i < ring0_points_data.Count; i++)
            {
                ring0_points_map.Add(this.rsTransfrom.sourceToTarget(ring0_points_data[i]));
            }
            SimplePolyline ring0 = new SimplePolyline(ring0_points_map);
            List<SimplePolyline> temp = new List<SimplePolyline>(); temp.Add(ring0);
            SimplePolygon polygon_map = new SimplePolygon(temp);
            return (SpatialTopology.IsSimplePolygonIntersectRect(polygon_map, this.rect));
        }

        public bool checkmutipolygon(Polygon mutipolygon_data)
        {
            for(int i =0;i<mutipolygon_data.childPolygons.Count;i++)
            {
                SimplePolygon simple = mutipolygon_data.childPolygons[i];
                if (checksimplepolygon(simple)) return true;
            }
            return false;
        }
        public bool checkpolyline(SimplePolyline polyline_data)
        {
            List<PointD> polyline_points_map = new List<PointD>();
            int pointcount = polyline_data.points.Count;
            for(int i =0 ;i<pointcount;i++)
            {
                polyline_points_map.Add(rsTransfrom.sourceToTarget(polyline_data.points[i]));
            }
            SimplePolyline polyline_map = new SimplePolyline(polyline_points_map);
            return (SpatialTopology.IsSimplePolyLineInRectangle(polyline_map, rect));
        }
        public bool checkmutipolyline(Polyline polyline)
        {
            for(int i = 0;i<polyline.childPolylines.Count;i++)
            {
                SimplePolyline simple = polyline.childPolylines[i];
                if (checkpolyline(simple)) return true;

            }
            return false;
        }
    }
}
