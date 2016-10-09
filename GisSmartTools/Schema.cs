using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.RS;
using OSGeo.OGR;
using OSGeo.GDAL;
using OSGeo.OSR;
using GisSmartTools.RS;

namespace GisSmartTools.Data
{
    /**
     * Schema
     * 图层（数据表）元信息
     * 提供了某个图层的基本的元数据信息
     * */
    public class Schema
    {
        public  String name;
        public OSGeo.OGR.wkbGeometryType geometryType;
        public  Dictionary<string,FieldDefn> fields; //字段或列表或数组
        public  ReferenceSystem rs;   //空间坐标系
        public Schema(String name, OSGeo.OGR.wkbGeometryType type,ReferenceSystem spatialreference, Dictionary<string, FieldDefn> _fields)
        {
            this.name = name;
            this.geometryType = type;
            this.rs = spatialreference;
            this.fields = _fields;
        }
        public static Schema CreateSchema(string name,OSGeo.OGR.wkbGeometryType schematype)
        {
            OSGeo.OGR.wkbGeometryType type = wkbGeometryType.wkbPoint;
            switch (schematype)
            {
                case wkbGeometryType.wkbMultiPoint:
                    type = wkbGeometryType.wkbPoint;
                    break;
                case wkbGeometryType.wkbPoint:
                    type = wkbGeometryType.wkbPoint;
                    break;
                case wkbGeometryType.wkbPolygon:
                    type = wkbGeometryType.wkbPolygon;
                    break;
                case wkbGeometryType.wkbMultiPolygon:
                    type = wkbGeometryType.wkbPolygon;
                    break;
                case wkbGeometryType.wkbLineString:
                    type = wkbGeometryType.wkbLineString;
                    break;
                case wkbGeometryType.wkbMultiLineString:
                    type = wkbGeometryType.wkbLineString;
                    break;
            }
            return new Schema(name,type, new GRS(new SpatialReference(Osr.SRS_WKT_WGS84)), new Dictionary<string, FieldDefn>());
        }
         public String GetArrtributeTypeByName(String name) //根据属性名字得到属性的类型
        {
             FieldDefn fd;
             fields.TryGetValue(name, out fd);
             return fd.GetTypeName();
        }


        public void AppendField(FieldDefn field) //增加一个字段
        {
            fields.Add(field.GetName(), field);
        }

        public void AppendFields(FieldDefn[] fields) //增加一组字段
        {
            foreach (FieldDefn field in fields)
            {
                this.fields.Add(field.GetName(), field);
            }
        }

        public bool DeleteField(String fieldname) //根据字段名字删除该字段
        {
            return fields.Remove(fieldname);
        }

        //事件：
        public delegate void FieldAppendedHandle(object sender, FieldDefn[] field);
        public event FieldAppendedHandle FieldAppended;
        private void RaiseFieldAppended(object sender,FieldDefn[] field)
        {
            if (FieldAppended != null)
            {
                FieldAppended(sender, field);
            }
        }

        public delegate void FieldDeletedHandle(object sender, FieldDefn[] field);
        public event FieldDeletedHandle FieldDeleted;
        private void RaiseFieldDeleted(object sender, FieldDefn[] field)
        {
            if (FieldDeleted != null)
            {
                FieldDeleted(sender, field);
            }
        }

    }
}
