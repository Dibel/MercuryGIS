using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;
namespace GisSmartTools.Filter
{
    /**
     * Filter_Larger
     * 判断属性值大于给定值的过滤器
     * 待实现方法：
     * Evaluate
     * */
    [Serializable]
    public class Filter_Larger : Filter
    {
        public String name;     //属性名
        public double value;    //属性值
        public Filter_Larger(String name, double value)
        {
            this.name = name;
            this.value = value;
        }

        public string GetDescription()
        {
            return "(" + name + ">" + value + ")";
        }
        public Boolean Evaluate(Feature feature)
        {
            if(this.name.Equals("FeatureID"))
            {
                if (feature.featureID > value) return true;
                else return false;
            }
            OSGeo.OGR.FieldType type = feature.schema.fields[this.name].GetFieldType();
            switch (type)
            {
                case OSGeo.OGR.FieldType.OFTInteger:
                    if ((int)feature.GetArrtributeByName(this.name) > this.value) return true;
                    else return false;
                case OSGeo.OGR.FieldType.OFTReal:
                    if ((double)feature.GetArrtributeByName(this.name) > this.value) return true;
                    else return false;
                default:
                    return false;
            }
        }
    }
}
