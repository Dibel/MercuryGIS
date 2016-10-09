using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;
using GisSmartTools.Filter;
using GisSmartTools.Geometry;
namespace GisSmartTools.Data
{
    /**
     * FeatureSource
     * 相当于一张数据表的所有信息
     * 包含元信息和具体数据信息
     * 内存式管理数据
     * */
    public class FeatureSource
    {
        public Schema schema; //保存了空间表元数据的对象
        public FeatureCollection features;  //保存了该表的所有的feature对象，即所有的记录

        public FeatureSource(Schema schema,FeatureCollection features)
        {
            this.schema = schema;
            this.features = features;
        }
        public FeatureCollection GetFeatures(Filter.Filter filter)
        {
            List<Feature> resultlist = new List<Feature>();
            foreach(Feature tmpfeature in this.features.featureList)
            {
                if (!tmpfeature.visible) continue;
                if (filter.Evaluate(tmpfeature)) resultlist.Add(tmpfeature);
            }

            return new FeatureCollection(resultlist);
        }
        public int GetNextFeatureID()
        {
            if (this.features.featureList.Count == 0) return 0;
            return (this.features.featureList.Last().featureID + 1);
        }

        public bool AppendField(String field_name,OSGeo.OGR.FieldType field_type)
        {
            this.schema.AppendField(new OSGeo.OGR.FieldDefn(field_name, field_type));
            return true;
        }
        public bool DeleteField(String field_name)
        {
            this.schema.DeleteField(field_name);
            return true;
        }
        public bool UpdateFieldValue(string field_name,int featureid,object value)
        {
            if (!this.schema.fields.ContainsKey(field_name)) return false;
            Feature feature = this.features.GetFeatureByID(featureid);
            if (feature == null) return false;
            feature.AddAttribute(field_name, value);
            return true;
        }

        

    }
}
