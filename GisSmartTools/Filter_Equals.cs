using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;

namespace GisSmartTools.Filter
{
    /**
     * Filter_Equals
     * 判断属性值相等的过滤器
     * 待实现方法：
     * Evaluate
     * */
    [Serializable]
    public class Filter_Equals : Filter
    {
        public String name;     //属性名
        public Object value;    //属性值
        public Filter_Equals(String name, Object value)
        {
            this.name = name;
            this.value = value;
        }

        public string GetDescription()
        {
            return ("("+name+"="+value+")");
        }
        public Boolean Evaluate(Feature feature)
        {
            return value.Equals(feature.GetArrtributeByName(this.name));
        }
    }
}
