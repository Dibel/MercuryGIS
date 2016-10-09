using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * RSTransformFactory
 * 返回合适的RSTransform接口实现类
 * 待实现列表：
 * GetRSTransform
 **/
namespace GisSmartTools.RS
{
    public enum RS
    {
           
    }
    public class RSTransformFactory
    {
        public static  RSTransform getRSTransform (ReferenceSystem sourceRS, ReferenceSystem targetRS)
        {
            return null;
        }
    }
}
