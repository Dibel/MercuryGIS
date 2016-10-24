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
        public static double M_PI = 3.14159265;
        public PointD sourceToTarget(PointD sourcepointD)
        {
            double target_x = sourcepointD.X * 20037508 / 180;
            double y = Math.Log(Math.Tan((90 + sourcepointD.Y) * M_PI / 360)) / (M_PI / 180);
            double target_y = y * 20037508.34 / 180;
            return new PointD(target_x, target_y);
        }
        public PointD targetToSource(PointD targetpointD)
        {
            double x = targetpointD.X / 20037508.34 * 180;
            double y = targetpointD.Y / 20037508.34 * 180;
            y = 180 / M_PI * (2 * Math.Atan(Math.Exp(y * M_PI / 180)) - M_PI / 2);
            return new PointD(x, y);
        }
    }

    public class RSTransform_WGS84_WEBMOCARTO2 : RSTransform
    {
        public static double M_PI = 3.14159265;
        public PointD sourceToTarget(PointD sourcepointD)
        {
            double target_x = sourcepointD.X * 20037508 / 180;
            double y = Math.Log(Math.Tan((90 + sourcepointD.Y) * M_PI / 360)) / (M_PI / 180);
            double target_y = y * 20037508.34 / 180;
            return new PointD(target_x, target_y);
        }
        public PointD targetToSource(PointD targetpointD)
        {
            double x = targetpointD.X / 20037508.34 * 180;
            double y = targetpointD.Y / 20037508.34 * 180;
            y = 180 / M_PI * (2 * Math.Atan(Math.Exp(y * M_PI / 180)) - M_PI / 2);
            return new PointD(x, y);
        }
    }
    public class RSTransform_NO_TRANSTRAM:RSTransform
    {
        public static double M_PI = 3.14159265;
        public PointD sourceToTarget(PointD sourcepointD)
        {
            return sourcepointD;
        }
        public PointD targetToSource(PointD targetpointD)
        {
            return targetpointD;
        }
    }
}
