using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSmartTools.PGIOyichao
{
    public class Type2String
    {

        public static string wkbGeometryType2String(OSGeo.OGR.wkbGeometryType type)
        {
            switch (type)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    return "POINT";
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    return "LINESTRING";
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    return "POLYGON";
                case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                    return "MULTILINESTRING";
                default:
                    return "MULTIPOLYGON";
            }
        }

        public static string OGRFiledType2String(OSGeo.OGR.FieldType type)
        {
            switch (type)
            {
                case OSGeo.OGR.FieldType.OFTInteger:
                    return "int";
                case OSGeo.OGR.FieldType.OFTReal:
                    return "float";
                default:
                    return "varchar";
            }
        }

    }
}
