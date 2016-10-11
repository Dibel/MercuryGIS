using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GisSmartTools.PGIOyichao
{
    class String2Type
    {
        public static OSGeo.OGR.FieldType String2OGRFiledType(string type)
        {
            if (type.StartsWith("int"))
            {
                return OSGeo.OGR.FieldType.OFTInteger;
            }else if(type.StartsWith("float")){
                return OSGeo.OGR.FieldType.OFTReal;
            }else{
                return OSGeo.OGR.FieldType.OFTString;
            }
        }

        public static OSGeo.OGR.wkbGeometryType String2wkbGeometryType(string type)
        {
            if (type.Contains("POINT"))
            {
                return OSGeo.OGR.wkbGeometryType.wkbPoint;
            }
            else if (type.Contains("LINESTRING"))
            {
                return OSGeo.OGR.wkbGeometryType.wkbLineString;
            }
            else if (type.Contains("POLYGON"))
            {
                return OSGeo.OGR.wkbGeometryType.wkbPolygon;
            }
            else if (type.Contains("MULTILINESTRING"))
            {
                return OSGeo.OGR.wkbGeometryType.wkbMultiLineString;
            }
            else
            {
                return OSGeo.OGR.wkbGeometryType.wkbMultiPolygon;
            }
        }

    }
}
