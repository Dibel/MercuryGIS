using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Geometry;
using System.Text.RegularExpressions;
using GisSmartTools.Data;

namespace GisSmartTools.PGIOyichao
{
    public class WKT2Feature
    {
        public static Geometry.Geometry WKT2Geometry(String wkt)
        {
            Geometry.Geometry geometry = null;
            string wkt1 = wkt.Trim();
            if (wkt1.StartsWith("POINT"))
            {
                geometry = WKT2PointD(wkt1);
                geometry.geometryType = OSGeo.OGR.wkbGeometryType.wkbPoint;
            }
            else if (wkt1.StartsWith("LINESTRING"))
            {
                geometry = WKT2SimplePolyline(wkt1);
                geometry.geometryType = OSGeo.OGR.wkbGeometryType.wkbLineString;
            }
            else if (wkt1.StartsWith("POLYGON"))
            {
                geometry = WKT2SimplePolygon(wkt1);
                geometry.geometryType = OSGeo.OGR.wkbGeometryType.wkbPolygon;
            }
            else if (wkt1.StartsWith("MULTILINESTRING"))
            {
                geometry = WKT2Polyline(wkt1);
                geometry.geometryType = OSGeo.OGR.wkbGeometryType.wkbMultiLineString;
            }
            else
            {
                geometry = WKT2Polygon(wkt1);
                geometry.geometryType = OSGeo.OGR.wkbGeometryType.wkbMultiPolygon;
            }
            return geometry;
        }

        public static PointD WKT2PointD(string wkt){
            double x = 0;
            double y = 0;
            try
            {
                Regex reg = new Regex(@"\s*POINT\((.*)\)\s*");
                Match match = reg.Match(wkt);
                return String2PointD(match.Groups[1].Value);
            }
            catch
            {
                return new PointD(x, y);
            }
        }

        public static SimplePolyline WKT2SimplePolyline(string wkt)
        {
            try
            {
                Regex reg = new Regex(@"\s*LINESTRING\((.*)\)\s*");
                Match match = reg.Match(wkt);
                string line = match.Groups[1].Value;
                string[] points = line.Split(",".ToCharArray());
                List<PointD> plist = new List<PointD>();
                foreach (string element in points)
                {
                    plist.Add(String2PointD(element));
                }
                return new SimplePolyline(plist);
            }
            catch
            {
                return new SimplePolyline();
            }
        }

        public static SimplePolygon WKT2SimplePolygon(string wkt)
        {
            try
            {
                Regex reg = new Regex(@"\s*POLYGON\((.*)\)\s*");
                Match match = reg.Match(wkt);
                string lines = match.Groups[1].Value;
                Regex reg1 = new Regex(@"\s*(\([\d\.\s,-]*\))\s*");
                MatchCollection matches = reg1.Matches(lines);
                List<SimplePolyline> plist = new List<SimplePolyline>();
                foreach (Match m in matches)
                {
                    plist.Add(WKT2SimplePolyline("LINESTRING"+m.Groups[1].Value));
                }
                return new SimplePolygon(plist);
            }
            catch
            {
                return new SimplePolygon();
            }
        }

        public static Polyline WKT2Polyline(string wkt)
        {
            try
            {
                Regex reg = new Regex(@"\s*MULTILINESTRING\((.*)\)\s*");
                Match match = reg.Match(wkt);
                string lines = match.Groups[1].Value;
                Regex reg1 = new Regex(@"\s*(\([\d\.\s,-]*\))\s*");
                MatchCollection matches = reg1.Matches(lines);
                List<SimplePolyline> plist = new List<SimplePolyline>();
                foreach (Match m in matches)
                {
                    plist.Add(WKT2SimplePolyline("LINESTRING" + m.Groups[1].Value));
                }
                return new Polyline(plist);
            }
            catch
            {
                return new Polyline();
            }
        }

        public static Polygon WKT2Polygon(string wkt)
        {
            try
            {
                Regex reg = new Regex(@"\s*MULTIPOLYGON\((.*)\)\s*");
                Match match = reg.Match(wkt);
                string lines = match.Groups[1].Value;
                Regex reg1 = new Regex(@"\s*(\((,?\([\d\.,\s-]*\))*\))\s*");
                MatchCollection matches = reg1.Matches(lines);
                List<SimplePolygon> plist = new List<SimplePolygon>();
                foreach (Match m in matches)
                {
                    plist.Add(WKT2SimplePolygon("POLYGON" + m.Groups[1].Value));
                }
                return new Polygon(plist);
            }
            catch
            {
                return new Polygon();
            }
        }

        public static PointD String2PointD(string str)
        {
            double x = 0;
            double y = 0;
            try
            {
                Regex reg = new Regex(@"\s*([\d\.-]+)\s+([\d\.-]+)\s*");
                Match match = reg.Match(str);
                x = double.Parse(match.Groups[1].Value);
                y = double.Parse(match.Groups[2].Value);
                return new PointD(x, y);
            }
            catch
            {
                return new PointD(x, y);
            }
        }

    }
}
