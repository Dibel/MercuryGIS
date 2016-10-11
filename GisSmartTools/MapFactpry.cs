using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GisSmartTools.Support;
using System.Windows.Media;

namespace GisSmartTools.MapFactory
{


    public class Additional
    {
        public PointF position;
        public float width = 200;
        public float height = 200;
        public  bool IsSelected(Point mouseposition)
        {
            if(mouseposition.X>=position.X&&mouseposition.X<=(position.X+width)&&mouseposition.Y>=position.Y&&mouseposition.Y<=(position.Y+height))
            {
                return true;
            }
            return false;
        }
        public  void Pan(float offset_x,float offset_y)
        {
            this.position.X += offset_x;
            this.position.Y += offset_y;
        }
        public virtual void PaintAdditional(Graphics g)
        {

        }
    }
    public class MapTitle:Additional
    {
        public string text = "标题";
        public Font font = SystemFonts.DefaultFont;
        public Color color = Colors.Black;
        public MapTitle(string text,PointF position)
        {
            this.text = text;
            this.position = position;
        }

        public override void PaintAdditional(Graphics g)
        {
            base.PaintAdditional(g);
            //Brush brush = new SolidBrush(color);
            //g.DrawString(text, font, brush, position);
        }

    }

    public class MapTULI:Additional
    {
        public mapcontent mapcontent;
        private float cur_y = 0;
        
        public MapTULI(mapcontent mapcontent,PointF position)
        {
            this.mapcontent = mapcontent;
            this.position = position;
        }

        private void paintRule(Graphics g, RenderRule rule,float cur_y)
        {
            Symbolizer geosym = rule.geometrysymbolizer;
            //switch (geosym.sign)
            //{
            //    case SymbolizerType.POINT:
            //        pointsymbolizer pointsymbolizer = (pointsymbolizer)geosym;
            //        Brush brush = new SolidBrush(pointsymbolizer.color);
            //        Pen pen = new Pen(brush);
            //        PointF screenPointF = new PointF(position.X, cur_y);
            //        switch (pointsymbolizer.pointstyle)
            //        {
            //            case PointStyle.CIRCLE_FILL:
            //                PointStylePainter.PaintFillCircle(g, brush, screenPointF, pointsymbolizer.size);
            //                break;
            //            case PointStyle.CIRCLE_HOLLOW:
            //                PointStylePainter.PaintHollowCircle(g, pen, screenPointF, pointsymbolizer.size);
            //                break;
            //            case PointStyle.CIRCLE_POINT:
            //                PointStylePainter.PaintCircle_Point(g, pen, brush, screenPointF, pointsymbolizer.size);
            //                break;
            //            case PointStyle.CIRCLE_RECT:
            //                PointStylePainter.PaintCircle_Rect(g, pen, screenPointF, pointsymbolizer.size);
            //                break;
            //            case PointStyle.RECT_FILL:
            //                PointStylePainter.PaintFillRect(g, brush, screenPointF, pointsymbolizer.size);
            //                break;
            //            case PointStyle.RECT_HOLLOW:
            //                PointStylePainter.PaintHollowRect(g, pen, screenPointF, pointsymbolizer.size);
            //                break;
            //            case PointStyle.TRIANGLE:
            //                PointStylePainter.PaintFillTriangle(g, brush, screenPointF, pointsymbolizer.size);
            //                break;
            //        }
            //        brush.Dispose();
            //        pen.Dispose();
            //        if(rule.filter!=null) g.DrawString(rule.filter.GetDescription(), SystemFonts.DefaultFont, Brushes.Black, new PointF(position.X + 2 * pointsymbolizer.size * 2 + 10, cur_y));
            //        cur_y += (2 * pointsymbolizer.size + 5);
            //        break;
            //    case SymbolizerType.LINE:
            //        linesymbolizer lsymbolizer = (linesymbolizer)geosym;
            //        Pen lpen = new Pen(lsymbolizer.color, lsymbolizer.width);
            //        lpen.DashStyle = lsymbolizer.linestyle;
            //        g.DrawLine(lpen, position.X, cur_y, position.X + 30, cur_y);
            //        if (rule.filter != null) g.DrawString(rule.filter.GetDescription(), SystemFonts.DefaultFont, Brushes.Black, new PointF(position.X + 35, cur_y));
            //        cur_y += (20);
            //        break;
            //    case SymbolizerType.POLYGON:
            //        polygonsymbolizer symbolizer = (polygonsymbolizer)geosym;
            //        Pen ppen = new Pen(symbolizer.strokecolor, symbolizer.strokewidth);
            //        Brush pbrush = new SolidBrush(symbolizer.fillcolor);
            //        g.DrawRectangle(ppen, position.X, cur_y, 30, 15);
            //        g.FillRectangle(pbrush, position.X, cur_y, 30, 15);
            //        if (rule.filter != null) g.DrawString(rule.filter.GetDescription(), SystemFonts.DefaultFont, Brushes.Black, new PointF(position.X + 35, cur_y));
            //        cur_y += (20);
            //        break;
            //}
        }
        public override void PaintAdditional(Graphics g)
        {
            cur_y = position.Y;
            base.PaintAdditional(g);
            foreach(Layer layer in mapcontent.layerlist)
            {
                //g.DrawString(layer.Layername, SystemFonts.DefaultFont, Brushes.Black, new PointF(position.X, cur_y));
                cur_y += 25;
                Style style = layer.style;
                foreach(RenderRule rule in style.rulelist)
                {
                    paintRule(g, rule, cur_y);
                    cur_y += (20);
                }
                paintRule(g, style.defaultRule, cur_y);
                cur_y += 30;
            }
        }
    }
    class MapFactpry
    {

    }
}
