using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Geometry;
using GisSmartTools.Data;
namespace GisSmartTools.Filter
{
    /**
     * Filter_Intersect
     * 判断Feature是否满足与给定Feature之间是否相交
     * 待实现方法：
     * Evaluate
     * */
    [Serializable]
    public class Filter_Intersect : Filter
    {
        public  Geometry.Geometry geometry ;
        Filter_Intersect(Geometry.Geometry geo)
        {
            this.geometry = geo;
        }

        public string GetDescription()
        {
            return "Intersect";
        }
        public Boolean Evaluate(Feature feature) 
        {
            //to do
            return true;
        }

    }
}
