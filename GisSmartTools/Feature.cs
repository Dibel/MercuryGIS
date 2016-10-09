using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Geometry;
using GisSmartTools.Data;

namespace GisSmartTools.Data
{
    /**
     * Feature
     * 要素类
     * 对数据表中一条记录的封装
     * */

    public class Feature
    {
        public int featureID;
        public Geometry.Geometry geometry; //几何图形对象
        public Dictionary<string,Object> attributes;  //存储非几何的属性数据
        public Schema schema;         //该feature的类型
        public bool visible = true;

        public Feature(int id,Schema schema,Geometry.Geometry geometry,Dictionary<string,Object> attributes)
        {
            this.featureID = id;
            this.schema = schema;
            this.geometry = geometry;
            this.attributes = attributes;
        }
        public Feature(int id, Schema schema, Geometry.Geometry geometry) : this(id, schema, geometry, new Dictionary<string, Object>()) { }

        public void SetAttribute(Dictionary<string, Object> attributes)
        {
            this.attributes = attributes;
        }

        /// <summary>
        /// 删除属性
        /// 慎用，为保证图层字段一致性，原则上仅在GeoDatabase内调用，要清空某一属性，请使用EraseAttribute
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attribute"></param>
        public void AddAttribute(String name,Object attribute)
        {
            attributes.Add(name, attribute);
        }

        
        
        /// <summary>
        /// 删除属性
        /// 慎用，为保证图层字段一致性，原则上仅在GeoDatabase内调用，要清空某一属性，请使用EraseAttribute
        /// </summary>
        /// <param name="name"></param>
        public void DeleteAttribute(String name)
        {
            attributes.Remove(name);
        }

        /// <summary>
        /// 修改属性
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attribute"></param>
        public void ReviseAttribute(String name, Object attribute)
        {
            attributes[name] = attribute;
        }

        /// <summary>
        /// 擦除属性值，与delete不同，这会留下一个值为空的属性
        /// </summary>
        /// <param name="name"></param>
        public void EraseAttribute(String name)
        {
            attributes[name] = null;
        }

        public Object GetArrtributeByName(String name)
        {
            
            Object value;
            if (name.Equals("FeatureID")) return featureID;
            if(attributes.TryGetValue(name, out value))return value;
            return null;
        }

        public void SetGeometry(GisSmartTools.Geometry.Geometry newGeometry)
        {
            this.geometry = newGeometry;
        }

        

    }
}
