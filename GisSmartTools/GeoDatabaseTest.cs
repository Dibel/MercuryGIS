using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;
using GisSmartTools.Geometry;

namespace GisSmartTools.Test
{
    public class GeoDatabaseTest
    {
        public void DoTest(String path)
        {

            //int m = DateTime.Now.Minute, s = DateTime.Now.Second, ms = DateTime.Now.Millisecond;
            //List<String> paths = new List<String>();
            //paths.Add(path);
            //GeoDatabase gdb = GeoDatabase.GetGeoDatabase(paths);
            //String schemaName = gdb.featureSources.ElementAt(0).Key;
            //long st = ms + s * 1000 + m * 60 * 1000;
            //FeatureSource fs = gdb.featureSources[schemaName];
            //m = DateTime.Now.Minute; s = DateTime.Now.Second; ms = DateTime.Now.Millisecond;
            //long ft = ms + s * 1000 + m * 60 * 1000;

            //if (fs != null)
            //{
            //    printnode("Time", "Start:" + st + "ms   finish:" + ft + "ms   length:" + (ft - st) + "ms");
            //    printnode("SchemaName", fs.schema.name);
            //    String wkt, attr;
            //    OSGeo.OSR.SpatialReference srf = fs.schema.rs.spetialReference;
            //    srf.ExportToWkt(out wkt);
            //    printnode("SpetialReference", wkt);
            //    Console.WriteLine("");
            //    Console.WriteLine("IsProjected:\t" + srf.IsProjected());
            //    Console.WriteLine("IsGeographic:\t" + srf.IsGeographic());
            //    Console.WriteLine("semiMajor:\t" + srf.GetSemiMajor());
            //    Console.WriteLine("semiMinor:\t" + srf.GetSemiMinor());
            //    Console.WriteLine("invFlat:\t" + srf.GetInvFlattening());
            //    int child = 0;
            //    Console.WriteLine("attr.PROJCS:\t" + srf.GetAttrValue("PROJCS", child));
            //    Console.WriteLine("attr.GEOGCS:\t" + srf.GetAttrValue("GEOGCS", child));
            //    Console.WriteLine("attr.DATUM:\t" + srf.GetAttrValue("DATUM", child));
            //   // Console.WriteLine("attr.SPHEROID:\t" + "0:" + srf.GetAttrValue("SPGEROID", 0) + "1:" + srf.GetAttrValue("SPGEROID", 1) + "2:" + srf.GetAttrValue("SPGEROID", 2));
            //    Console.WriteLine("attr.PROJECTION:\t" + srf.GetAttrValue("PROJECTION", child));
            //    Console.WriteLine("LinerUnitsName:\t" + srf.GetLinearUnitsName());
            //    Console.WriteLine("LinerUnits:\t" + srf.GetLinearUnits());

            //}

            //    OSGeo.OGR.wkbGeometryType type = fs.schema.geometryType;
            //    printnode("Features", "FeatureType:" + type.ToString());
            
            //    for (int i = 0; i < fs.features.count; ++i)
            //    {
            //        Feature feat = fs.features.featureList[i];
            //        OSGeo.OGR.wkbGeometryType featType = feat.geometry.geometryType;
            //        printnode("Feature", "FID:" + feat.featureID.ToString());
            //        int sc = feat.schema.fields.Count;
            //        for (int j = 0; j < sc; ++j)
            //        {
            //            OSGeo.OGR.FieldDefn field = feat.schema.fields.ElementAt(j).Value;
            //            Console.WriteLine("name:" + field.GetName() + "  type:" + field.GetTypeName() + "  value:" + feat.GetArrtributeByName(field.GetName()).ToString());

            //        }
            //        Console.WriteLine("Geometry: type:" + featType.ToString());
            //        switch (featType)
            //        {
            //            case OSGeo.OGR.wkbGeometryType.wkbPoint:
            //                GisSmartTools.Geometry.PointD point = (GisSmartTools.Geometry.PointD)feat.geometry;
            //                Console.WriteLine("x=  " + point.X + "\t\ty=  " + point.Y);
            //                break;
            //            case OSGeo.OGR.wkbGeometryType.wkbLineString:
            //                GisSmartTools.Geometry.SimplePolyline line = (GisSmartTools.Geometry.SimplePolyline)feat.geometry;
            //                foreach (GisSmartTools.Geometry.PointD po in line.points)
            //                {
            //                    Console.WriteLine("x=  " + po.X + "\t\ty=  " + po.Y);
            //                }
            //                break;
            //            case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
            //                GisSmartTools.Geometry.Polyline lines = (GisSmartTools.Geometry.Polyline)feat.geometry;
            //                foreach (SimplePolyline line2 in lines.childPolylines)
            //                {
            //                    printnode("AnotherLine", "");
            //                    foreach (GisSmartTools.Geometry.PointD point3 in line2.points)
            //                    {
            //                        Console.WriteLine("x=  " + point3.X + "\t\ty=  " + point3.Y);
            //                    }
            //                }
            //                break;
            //            case OSGeo.OGR.wkbGeometryType.wkbPolygon:
            //                GisSmartTools.Geometry.SimplePolygon gon = (GisSmartTools.Geometry.SimplePolygon)feat.geometry;
            //                foreach (SimplePolyline ring in gon.rings)
            //                {
            //                    printnode("AnotherRing", "");
            //                    foreach (GisSmartTools.Geometry.PointD point2 in ring.points)
            //                    {
            //                        Console.WriteLine("x=  " + point2.X + "\t\ty=  " + point2.Y);
            //                    }
            //                }
            //                break;
            //            case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
            //                GisSmartTools.Geometry.Polygon gons = (GisSmartTools.Geometry.Polygon)feat.geometry;
            //                foreach (GisSmartTools.Geometry.SimplePolygon cgon in gons.childPolygons)
            //                {
            //                    printnode("AnotherPolygon", "");
            //                    foreach (GisSmartTools.Geometry.SimplePolyline ring5 in cgon.rings)
            //                    {
            //                        printnode("AnotherRing", "");
            //                        foreach (GisSmartTools.Geometry.PointD point5 in ring5.points)
            //                        {
            //                            Console.WriteLine("x=  " + point5.X + "\t\ty=  " + point5.Y);
            //                        }
            //                    }
            //                }
            //                break;
            //        }
            //    }
            
            

            //save as
            //gdb.SaveAsToFile(schemaName, "G:\\haha.shp");

    }

        public static void printnode(String node, String msg)
        {
            Printer.printnode(node, msg);
        }

    }
}
