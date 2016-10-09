using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;
namespace GisSmartTools.Filter
{
    /**
     * Filter_Not
     * 逻辑飞
     * 待实现方法：
     * Evaluate
     * */
    [Serializable]
    public class Filter_Not : Filter
    {
        private Filter filter;
        public Filter_Not(Filter filter)
        {
            this.filter = filter;
        }

        public string GetDescription()
        {
            return "not " + filter.GetDescription();
        }
        public Boolean Evaluate(Feature feature)
        {
            return !this.filter.Evaluate(feature);
        }
    }
}
