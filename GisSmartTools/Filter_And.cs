using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;
namespace GisSmartTools.Filter
{
    /**
     * Filter_And
     * 逻辑与
     * 待实现方法：
     * Evaluate
     * */
    [Serializable]
    public class Filter_And : Filter
    {
        private List<Filter> filterList;
        public Filter_And(List<Filter> filterlist)
        {
            this.filterList = filterlist;
        }
        public Boolean Evaluate(Feature feature)
        {
            Boolean res = true;
            foreach (Filter cf in filterList)
            {
                res = res && cf.Evaluate(feature);
                if (!res) break;
                
            }
            return res;
        }

        public string GetDescription()
        {
            if(filterList.Count==2)
            {
                if(filterList[0].GetType().Equals(typeof(Filter_Less))&&filterList[1].GetType().Equals(typeof(Filter_Larger)))
                {
                    Filter_Less less = (Filter_Less)filterList[0];
                    Filter_Larger larger = (Filter_Larger)filterList[1];
                    return "" + less.value + " -- " + larger.value;
                }
                if (filterList[1].GetType().Equals(typeof(Filter_Less)) && filterList[0].GetType().Equals(typeof(Filter_Larger)))
                {
                    Filter_Less less = (Filter_Less)filterList[1];
                    Filter_Larger larger = (Filter_Larger)filterList[0];
                    return "" + less.value + " -- " + larger.value;
                }
            }
            if (filterList == null || filterList.Count == 0) return "";
            string result = filterList[0].GetDescription();
            for (int i = 1; i < filterList.Count; i++)
            {
                result = result + "and" + filterList[i].GetDescription();
            }
            return result;
        }



    }
}
