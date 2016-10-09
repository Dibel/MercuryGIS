using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Geometry;
namespace GisSmartTools.RS
{
    public interface RSTransform
    {
       PointD sourceToTarget(PointD sourcepointD);
        PointD targetToSource(PointD targetpointD);
    }
    public class RSTransform_WGS84_WEBMOCARTO:RSTransform
    {
      public   PointD sourceToTarget(PointD sourcepointD)
        {
            return sourcepointD;
        }
        public PointD targetToSource(PointD targetpointD)
        {
            return targetpointD;
        }
    }
}
