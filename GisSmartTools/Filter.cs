using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;
namespace GisSmartTools.Filter
{
    /**
     * Filter
     * 过滤器接口
     * 所有用作Filter的类均需实现该接口
     * */
    public interface Filter
    {
        //评估传入的feature是否满足该条件
        Boolean Evaluate(Feature feature);
        string GetDescription();
    }
}
