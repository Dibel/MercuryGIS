using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Geometry;

namespace GisSmartTools.PGIOyichao
{
    public class Feature2WKT
    {
        public static string Geometry2WKT(Geometry.Geometry geom)
        {
            switch (geom.geometryType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    return PointD2WKT((PointD)geom);
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    return SimplePolyline2WKT((SimplePolyline)geom);
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    return SimplePolygon2WKT((SimplePolygon)geom);
                case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                    return Polyline2WKT((Polyline)geom);
                default:
                    return Polygon2WKT((Polygon)geom);
            }
        }

        public static string PointD2WKT(PointD geom)
        {
            return "POINT(" + geom.X + " " + geom.Y + ")";
        }

        public static string PointD2String(PointD geom)
        {
            return geom.X + " " + geom.Y;
        }

        public static string SimplePolyline2WKT(SimplePolyline geom)
        {
            string s = "LINESTRING"+SimplePolyline2String(geom);
            return s;
        }

        public static string SimplePolyline2String(SimplePolyline geom)
        {
            string s = "(";
            List<PointD> points = geom.points;
            for (int i = 0; i < points.Count; i++)
            {
                if (i > 0)
                {
                    s += ",";
                }
                s += PointD2String(points[i]);
            }
            return s + ")";
        }

        public static string SimplePolygon2WKT(SimplePolygon geom)
        {
            string s = "POLYGON" + SimplePolygon2String(geom);
            return s;
        }

        public static string SimplePolygon2String(SimplePolygon geom)
        {
            string s = "(";
            List<SimplePolyline> rings = geom.rings;
            for (int i = 0; i < rings.Count; i++)
            {
                if (i > 0)
                {
                    s += ",";
                }
                s += SimplePolyline2String(rings[i]);
            }
            return s+")";
        }

        public static string Polyline2WKT(Polyline geom)
        {
            string s = "MULTILINESTRING(";
            List<SimplePolyline> childPolylines = geom.childPolylines;
            for (int i = 0; i < childPolylines.Count; i++)
            {
                if (i > 0)
                {
                    s += ",";
                }
                s += SimplePolyline2String(childPolylines[i]);
            }
            return s+")";
        }

        public static string Polygon2WKT(Polygon geom)
        {
            string s = "MULTIPOLYGON(";
            List<SimplePolygon> childPolygons = geom.childPolygons;
            for (int i = 0; i < childPolygons.Count; i++)
            {
                if (i > 0)
                {
                    s += ",";
                }
                s += SimplePolygon2String(childPolygons[i]);
            }
            return s + ")";
        }
    }
}
