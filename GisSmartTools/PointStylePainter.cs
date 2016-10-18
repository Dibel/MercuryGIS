using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Geometry;
using GisSmartTools.Support;
using System.Drawing;
namespace GisSmartTools.Support
{
    public enum PointStyle
    {
        CIRCLE_FILL = 1,
        CIRCLE_HOLLOW = 2,
        RECT_FILL = 3,
        RECT_HOLLOW = 4,
        TRIANGLE = 5,
        CIRCLE_POINT = 6,
        CIRCLE_RECT = 7,
    }
    
    class PointStylePainter
    {
        
        public static void PaintFillCircle(Graphics g,Brush brush,PointF position,float size)
        {
            g.FillEllipse(brush, position.X, position.Y, size,size);
        }
        public static void PaintHollowCircle(Graphics g,Pen pen,PointF position,float size)
        {
            g.DrawEllipse(pen, position.X, position.Y, size, size);
        }
         public static void PaintFillRect(Graphics g,Brush brush,PointF position,float size)
        {
            g.FillRectangle(brush, position.X - size / 2, position.Y - size / 2,  size,  size);
        }

         public static void PaintHollowRect(Graphics g,Pen pen,PointF position,float size)
         {
             g.DrawRectangle(pen, position.X - size / 2, position.Y - size / 2, size, size);
         }
        public static void PaintFillTriangle(Graphics g,Brush brush,PointF position,float size)
        {
            PointF[] points = new PointF[3];
            points[0] = new PointF((float)position.X, (float)(position.Y - 1.154 * size));
            points[1] = new PointF((float)(position.X - size), (float)(position.Y + 0.577 * size));
            points[2] = new PointF((float)(position.X + size), (float)(position.Y + 0.577 * size));
            g.FillPolygon(brush, points);
        }

        public static void PaintHollowTriangle(Graphics g,Pen pen,PointF position,float size)
        {
            PointF[] points = new PointF[3];
            points[0] = new PointF((float)position.X, (float)(position.Y - 1.154 * size));
            points[1] = new PointF((float)(position.X - size), (float)(position.Y + 0.577 * size));
            points[2] = new PointF((float)(position.X + size), (float)(position.Y + 0.577 * size));
            g.DrawPolygon(pen, points);
        }

        public static void PaintCircle_Point(Graphics g,Pen pen,Brush brush, PointF position,float size)
        {
            g.DrawEllipse(pen, position.X, position.Y, size, size);
            g.FillEllipse(brush, position.X+size/8*3, position.Y+size/8*3, size / 4, size / 4);
        }

        public static void PaintCircle_Rect(Graphics g,Pen pen,PointF position,float size)
        {
            g.DrawEllipse(pen, position.X, position.Y, size, size);
            g.DrawRectangle(pen, position.X + size / 4, position.Y + size / 4, size / 2, size / 2);
        }
    }
}
