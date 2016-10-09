using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;
namespace GisSmartTools.Filter
{
    /**
     * Filter_Or
     * 逻辑或
     * 待实现方法：
     * Evaluate
     * */
    [Serializable]
    public class Filter_Or : Filter
    {
        private List<Filter> filterList;
        public Filter_Or(List<Filter> filterlist)
        {
            this.filterList = filterlist;
        }
        public Boolean Evaluate(Feature feature)
        {
            Boolean res = false;
            foreach (Filter cf in filterList)
            {
                res = res || cf.Evaluate(feature);
                if (res) break;
            }
            return res;
        }

        public string GetDescription()
        {
            if (filterList == null || filterList.Count == 0) return "";
            string result = filterList[0].GetDescription();
            for(int i = 1;i<filterList.Count;i++)
            {
                result =result + "or"+filterList[i].GetDescription();
            }
            return result;
        }

    }
}
