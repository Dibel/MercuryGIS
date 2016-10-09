using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Geometry;
using GisSmartTools.Test;
namespace GisSmartTools.Topology
{
    /**
     * SpatialTopology
     * 空间拓扑关系计算类
     * 所有方法待实现
     * */
    public class SpatialTopology
    {

        //判断两个外包矩形是否相交
        public static bool IsRectangleIntersect(Rectangle r1,Rectangle r2)
        {
            double nMaxLeft = 0;
            double nMaxTop = 0;
            double nMinRight = 0;
            double nMinBottom = 0;

            //计算两矩形可能的相交矩形的边界  
            nMaxLeft = r1.minX >= r2.minX ? r1.minX : r2.minX;
            nMaxTop = r1.minY >= r2.minY ? r1.minY : r2.minY;
            nMinRight = (r1.maxX) <= (r2.maxX) ? (r1.maxX) : (r2.maxX);
            nMinBottom = (r1.maxY) <= (r2.maxY) ? (r1.maxY) : (r2.maxY);
            // 判断是否相交  
            if ((nMaxLeft > nMinRight) || (nMaxTop > nMinBottom))
            {
                return false;
            }
            else
            {
                return true;
            }  
        }

        //判断两个矩形是否相互包含
        public static bool IsRectangle1InRectangle2(Rectangle r1,Rectangle r2)
        {
            if ((r1.minX >= r2.minX) && (r1.maxX <= r2.maxX) && (r1.minY >= r2.minY) && (r1.maxY <= r2.maxY)) return true;
            return false;
        }

        //将线段转化为对应的rect
        public static Rectangle getRectFromSegment(PointD p1,PointD p2)
        {
            double minx, miny, maxx, maxy;
            if(p1.X<p2.X) 
            {
                minx = p1.X; maxx = p2.X;
            }
            else
            {
                minx = p2.X; maxx = p1.X;
            }
            if (p1.Y < p2.Y) { miny = p1.Y; maxy = p2.Y; }
            else { miny = p2.Y; maxy = p1.Y; }
            return new Rectangle(minx, miny, maxx, maxy);
        }
        //判断线段与线段是否相交
        public static bool IsSegmentIntersectsegment(PointD p1,PointD p2,PointD q1,PointD q2)
        {
            //外包矩形排斥实验
            if (!IsRectangleIntersect(getRectFromSegment(p1,p2), getRectFromSegment(q1,q2))) return false;
            //判断p1,p2相对q跨立实验
            bool b1 = false, b2 = false;
            if( ((p1.X-q1.X)*(q2.Y-q1.Y)-(p1.Y-q1.Y)*(q2.X-q1.X))*((p2.X-q1.X)*(q2.Y-q1.Y)-(p2.Y-q1.Y)*(q2.X-q1.X))<0 ) b1=true;
            //判断q1,q2相对p跨立实验
            if (((q1.X - p1.X) * (p2.Y - p1.Y) - (q1.Y - p1.Y) * (p2.X - p1.X)) * ((q2.X - p1.X) * (p2.Y - p1.Y) - (q2.Y - p1.Y) * (p2.X - p1.X)) < 0) b2 = true;
            if (b1 && b2) return true;
            else return false;
        }
        //判断点是否在矩形内

        public static bool IsPointInRectangle(PointD point,Rectangle rect)
        {
            return ((point.X >= rect.minX) && (point.X <= rect.maxX)&& (point.Y >= rect.minY) && (point.Y <= rect.maxY));
        }
        public static bool IsSimplePolyLineInRectangle(SimplePolyline simplepolyline ,Rectangle rect)
        {
            //判断外包矩形，如果外包矩形不想交，则立即返回
            Rectangle polyrec = simplepolyline.getEnvelop();
            if(!IsRectangleIntersect(polyrec,rect)) return false;
            
            //判断每条线是否与矩形相交
            List<PointD> points = simplepolyline.points;
            int pointcount = points.Count;
            for (int i = 0; i < pointcount-1;i++ )
            {
                if(IsSegmentIntersectsegment(points[i],points[i+1],new PointD(rect.minX,rect.minY),new PointD(rect.minX,rect.maxY))) return true;
                if (IsSegmentIntersectsegment(points[i], points[i + 1], new PointD(rect.minX, rect.minY), new PointD(rect.maxX, rect.minY))) return true;
                if (IsSegmentIntersectsegment(points[i], points[i + 1], new PointD(rect.maxX, rect.minY), new PointD(rect.maxX, rect.maxY))) return true;
                if (IsSegmentIntersectsegment(points[i], points[i + 1], new PointD(rect.minX, rect.maxY), new PointD(rect.maxX, rect.maxY))) return true;

            }
            //判断点是否在矩形内,若一个点在矩形内，则该多边形在矩形内
            if (pointcount > 0)
            {
                PointD polygonpoint = points.First();
                if (IsPointInRectangle(polygonpoint, rect)) return true;
            }
                return false;
        }
        //判断多边形与矩形是否相交
        public static bool IsSimplePolygonIntersectRect(SimplePolygon polygon,Rectangle rect)
        {
            Rectangle polyrect = polygon.getEnvelop();
            if (!IsRectangleIntersect(polyrect, rect)) {  return false; }
            
            List<PointD> points = polygon.rings[0].points;
            int pointcount = points.Count;
            for (int i = 0; i < pointcount - 1; i++)
            {
                if (IsSegmentIntersectsegment(points[i], points[i + 1], new PointD(rect.minX, rect.minY), new PointD(rect.minX, rect.maxY))) return true;
                if (IsSegmentIntersectsegment(points[i], points[i + 1], new PointD(rect.minX, rect.minY), new PointD(rect.maxX, rect.minY))) return true;
                if (IsSegmentIntersectsegment(points[i], points[i + 1], new PointD(rect.maxX, rect.minY), new PointD(rect.maxX, rect.maxY))) return true;
                if (IsSegmentIntersectsegment(points[i], points[i + 1], new PointD(rect.minX, rect.maxY), new PointD(rect.maxX, rect.maxY))) return true;

            }
            //判断点是否在矩形内,若一个点在矩形内，则该多边形在矩形内
            if(pointcount>0)
            {
                PointD polygonpoint = polygon.rings[0].points.First();
                if (IsPointInRectangle(polygonpoint, rect)) return true;
            }
            //判断矩形框是否在多边形内
            PointD point_rect = new PointD((rect.minX + rect.maxX) / 2, (rect.maxY + rect.minY) / 2);
            if (IsPointInPolygon(point_rect, points)) return true;
            return false;
        }

        public static Boolean IsPointInPolygon(PointD pnt1, List<PointD> fencePnts)
        {
            int cnt = 0;
            for (int i = 0; i < fencePnts.Count-1; i++)
            {
                if(fencePnts[i].Y!=fencePnts[i+1].Y)
                {
                    double x = ((fencePnts[i + 1].X - fencePnts[i].X) / (fencePnts[i + 1].Y - fencePnts[i].Y) * (pnt1.Y - fencePnts[i].Y) + fencePnts[i].X);
                    double miny = fencePnts[i].Y,maxy = fencePnts[i+1].Y;
                    if(fencePnts[i].Y>fencePnts[i+1].Y) {miny=fencePnts[i+1].Y;maxy = fencePnts[i].Y;}
                    if ((x >= pnt1.X) && (pnt1.Y <= maxy) && (pnt1.Y > miny)) cnt++; 
                }
            }
            return (cnt % 2 > 0) ? true : false;
        }
        public static double GetDistanceBetweenPoints(PointD p1,PointD p2)
        {
            return Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
        }
        public static Boolean IsPointWithinBox()
        {
            return false;
        }
        public static Boolean IsMultiPointWithinBox()
        {
            return false;
        }
        public static Boolean IsTwoBoxesIntersect()
        {
            return false;
        }


        public static Boolean IsTwoSegmentIntersect()
        {
            return false;
        }

        public static Boolean IsSegmentIntersectBox()
        {
            return false;
        }
        public static Boolean IsSimplePolylinePartiallyWithinBox()
        {
            return false;
        }
        public static Boolean IsMultiPolylinePartiallyWithBox()
        {
            return false;
        }
        public static Boolean IsSimplePolygonPartiallyWithinBox()
        {
            return false;
        }
        public static Boolean IsMultiPolygonPartiallyWithBox()
        {
            return false;
        }





    }
}
