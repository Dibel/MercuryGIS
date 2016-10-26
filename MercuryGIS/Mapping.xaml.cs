using GisSmartTools.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfColorFontDialog;

namespace MercuryGIS
{

    public enum Status { PAN, NONE};
    /// <summary>
    /// Mapping.xaml 的交互逻辑
    /// </summary>
    public partial class Mapping : Image
    {
        public mapcontent map;
        private WriteableBitmap background;
        private WriteableBitmap bmp;
        private Point mCurMouseLocation;
        private int bitmap_X;
        private int bitmap_Y;
        public bool IsLegend = true;
        public bool IsTitle = true;
        public MapTitle title;
        private MapLegend legend;
        private MapElement selectedElement;
        private Status status;
        public Mapping()
        {
            InitializeComponent();
            MouseLeftButtonDown += new MouseButtonEventHandler(Mapping_MouseDown);
            MouseMove += new MouseEventHandler(Mapping_MouseMove);
            bitmap_X = 800;
            bitmap_Y = 500;
            bmp = BitmapFactory.New(bitmap_X, bitmap_Y);
            Source = bmp;
        }

        public FontInfo GetTitleFont()
        {
            var font = title.font;
            FontStyle style = FontStyles.Normal;
            if (font.IsItalic) style = FontStyles.Italic;
            FontWeight weight = FontWeights.Normal;
            if (font.IsBold) weight = FontWeights.Bold;
            return new FontInfo(new FontFamily(font.FontName), font.EmSize, style, FontStretches.Normal, weight, new SolidColorBrush(title.color));
        }

        public FontInfo GetLegendTitleFont()
        {
            var font = legend.titlefont;
            FontStyle style = FontStyles.Normal;
            if (font.IsItalic) style = FontStyles.Italic;
            FontWeight weight = FontWeights.Normal;
            if (font.IsBold) weight = FontWeights.Bold;
            return new FontInfo(new FontFamily(font.FontName), font.EmSize, style, FontStretches.Normal, weight, new SolidColorBrush(legend.titlecolor));
        }

        public FontInfo GetLegendTextFont()
        {
            var font = legend.font;
            FontStyle style = FontStyles.Normal;
            if (font.IsItalic) style = FontStyles.Italic;
            FontWeight weight = FontWeights.Normal;
            if (font.IsBold) weight = FontWeights.Bold;
            return new FontInfo(new FontFamily(font.FontName), font.EmSize, style, FontStretches.Normal, weight, new SolidColorBrush(legend.textcolor));
        }

        public void SetTitleFont(FontInfo info)
        {
            title.color = info.BrushColor.Color;
            double size = info.Size;
            string fontname = info.Family.Source;
            bool italic = false;
            bool bold = false;
            if (info.Style != FontStyles.Normal)
            {
                italic = true;
            }
            if(info.Weight != FontWeights.Normal)
            {
                bold = true;
            }
            var font = new PortableFontDesc(name: fontname, emsize: (int)size, isbold: bold, isitalic: italic, cleartype: true);
            title.font = font;
            Refresh();
        }

        public void SetLegendTitleFont(FontInfo info)
        {
            legend.titlecolor = info.BrushColor.Color;
            double size = info.Size;
            string fontname = info.Family.Source;
            bool italic = false;
            bool bold = false;
            if (info.Style != FontStyles.Normal)
            {
                italic = true;
            }
            if (info.Weight != FontWeights.Normal)
            {
                bold = true;
            }
            var font = new PortableFontDesc(name: fontname, emsize: (int)size, isbold: bold, isitalic: italic, cleartype: true);
            legend.titlefont = font;
            Refresh();
        }

        public void SetLegendTextFont(FontInfo info)
        {
            legend.textcolor = info.BrushColor.Color;
            double size = info.Size;
            string fontname = info.Family.Source;
            bool italic = false;
            bool bold = false;
            if (info.Style != FontStyles.Normal)
            {
                italic = true;
            }
            if (info.Weight != FontWeights.Normal)
            {
                bold = true;
            }
            var font = new PortableFontDesc(name: fontname, emsize: (int)size, isbold: bold, isitalic: italic, cleartype: true);
            legend.font = font;
            Refresh();
        }

        public void SetData(mapcontent map, WriteableBitmap bitmap)
        {
            this.map = map;
            this.background = bitmap.Clone();
            title = new MapTitle(map.name, new Point(30, 10));
            legend = new MapLegend(map, new Point(bitmap.Width *3 / 4, bitmap.Height *3 / 4));
            bmp = background.Clone();
            Refresh();
        }

        public void SetBmp(WriteableBitmap bitmap)
        {
            this.background = bitmap.Clone();
            bmp = background.Clone();
            Refresh();
        }

        public void Refresh()
        {
            //bmp = background.Clone();

            using (bmp.GetBitmapContext())
            {
                Rect rect = new Rect(0, 0, background.Width, background.Height);
                bmp.Blit(rect, background, rect, WriteableBitmapExtensions.BlendMode.None);
                if (IsLegend) legend.Paint(bmp);
                if (IsTitle) title.Paint(bmp);
            }
            //Thread.Sleep(1);
            //bmp.AddDirtyRect(new Int32Rect(0, 0, (int)background.Width, (int)background.PixelHeight));
            Source = bmp;
        }

        private void Mapping_MouseMove(Object sender, MouseEventArgs e)
        {
            Point loc = e.GetPosition(this);
            if (title != null && title.IsSelected(loc))
            {
                selectedElement = title;
                status = Status.PAN;
                Cursor = Cursors.SizeAll;

            }
            else if (legend != null && legend.IsSelected(loc))
            {
                selectedElement = legend;
                status = Status.PAN;
                Cursor = Cursors.SizeAll;
            }
            else
            {
                status = Status.NONE;
                Cursor = Cursors.Arrow;
                //status = SmartGis.status.imagepan;
                //Cursor = Cursors.Hand;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switch (status)
                {
                    case Status.PAN:
                        selectedElement.Pan(loc.X - mCurMouseLocation.X, loc.Y - mCurMouseLocation.Y);
                        mCurMouseLocation = loc;
                        Refresh();
                        break;
                }
            }
        }

        private void Mapping_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            mCurMouseLocation = e.GetPosition(this);
        }

        public bool Save(string path)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            try
            {
                using (Stream stream = File.Create(path))
                {
                    encoder.Save(stream);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class MapElement
    {
        public Point position;
        public float width = 100;
        public float height = 100;
        public bool IsSelected(Point mouseposition)
        {
            if (mouseposition.X >= position.X && mouseposition.X <= (position.X + width) && mouseposition.Y >= position.Y && mouseposition.Y <= (position.Y + height))
            {
                return true;
            }
            return false;
        }
        public void Pan(double offset_x, double offset_y)
        {
            this.position.X += offset_x;
            this.position.Y += offset_y;
        }
        public virtual void Paint(WriteableBitmap bmp) { }
    }
    public class MapTitle : MapElement
    {
        public string text = "标题";
        public PortableFontDesc font = new PortableFontDesc(name: "微软雅黑", emsize: 16, cleartype: true);
        public Color color = Colors.Black;
        public MapTitle(string text, Point position)
        {
            this.text = text;
            this.position = position;
        }

        public override void Paint(WriteableBitmap bmp)
        {
            bmp.DrawString((int)position.X, (int)position.Y, color, Colors.White, font, text);
            //base.PaintAdditional(g);
            //Brush brush = new SolidBrush(color);
            //g.DrawString(text, font, brush, position);
        }

    }

    public class MapLegend : MapElement
    {
        public mapcontent mapcontent;
        public PortableFontDesc titlefont = new PortableFontDesc(name: "黑体", emsize: 10, cleartype: true);
        public PortableFontDesc font = new PortableFontDesc(name: "微软雅黑", emsize: 8, cleartype: true);
        public Color titlecolor = Colors.Black;
        public Color textcolor = Colors.Black;
        private double cur_y = 0;

        public MapLegend(mapcontent mapcontent, Point position)
        {
            this.mapcontent = mapcontent;
            this.position = position;
        }

        private void paintRule(WriteableBitmap bmp, RenderRule rule, double cur_y)
        {
            Symbolizer geosym = rule.geometrysymbolizer;
            switch (geosym.sign)
            {
                case SymbolizerType.POINT:
                    pointsymbolizer pointsymbolizer = (pointsymbolizer)geosym;
                    int size = (int)pointsymbolizer.size;
                    Color color = (Color)pointsymbolizer.color;
                    Point screenPointF = new Point(position.X, cur_y);
                    switch (pointsymbolizer.pointstyle)
                    {
                        case PointStyle.CIRCLE_FILL:
                            PaintFillCircle(bmp, screenPointF, size, color);
                            break;
                        case PointStyle.CIRCLE_HOLLOW:
                            PaintHollowCircle(bmp, screenPointF, size, color);
                            break;
                        case PointStyle.CIRCLE_POINT:
                            PaintCircle_Point(bmp, screenPointF, size, color);
                            break;
                        case PointStyle.CIRCLE_RECT:
                            PaintCircle_Rect(bmp, screenPointF, size, color);
                            break;
                        case PointStyle.RECT_FILL:
                            PaintFillRect(bmp, screenPointF, size, color);
                            break;
                        case PointStyle.RECT_HOLLOW:
                            PaintHollowRect(bmp, screenPointF, size, color);
                            break;
                        case PointStyle.TRIANGLE:
                            PaintFillTriangle(bmp, screenPointF, size, color);
                            break;
                    }
                    if (rule.filter != null)
                    {
                        bmp.DrawString((int)position.X + 2 * (int)pointsymbolizer.size * 2 + 10, (int)cur_y, textcolor, Colors.White, font, rule.filter.GetDescription());
                        //g.DrawString(rule.filter.GetDescription(), SystemFonts.DefaultFont, Brushes.Black, new PointF(position.X + 2 * pointsymbolizer.size * 2 + 10, cur_y));
                    }
                    cur_y += (2 * pointsymbolizer.size + 5);
                    break;
                case SymbolizerType.LINE:
                    linesymbolizer lsymbolizer = (linesymbolizer)geosym;
                    //Pen lpen = new Pen(lsymbolizer.color, lsymbolizer.width);
                    //lpen.DashStyle = lsymbolizer.linestyle;
                    //g.DrawLine(lpen, position.X, cur_y, position.X + 30, cur_y);
                    bmp.DrawLineAa((int)position.X, (int)cur_y, (int)position.X + 30, (int)cur_y, lsymbolizer.color, (int)lsymbolizer.width);
                    if (rule.filter != null)
                    {
                        bmp.DrawString((int)position.X + 35, (int)cur_y, textcolor, Colors.White, font, rule.filter.GetDescription());
                        //g.DrawString(rule.filter.GetDescription(), SystemFonts.DefaultFont, Brushes.Black, new PointF(position.X + 35, cur_y));
                    }
                    cur_y += (20);
                    break;
                case SymbolizerType.POLYGON:
                    polygonsymbolizer symbolizer = (polygonsymbolizer)geosym;
                    //Pen ppen = new Pen(symbolizer.strokecolor, symbolizer.strokewidth);
                    //Brush pbrush = new SolidBrush(symbolizer.fillcolor);
                    int[] list = { (int)position.X, (int)cur_y, (int)position.X + 30, (int)cur_y, (int)position.X + 30, (int)cur_y + 15, (int)position.X, (int)cur_y + 15, (int)position.X, (int)cur_y };
                    //g.DrawRectangle(ppen, position.X, cur_y, 30, 15);
                    //g.FillRectangle(pbrush, position.X, cur_y, 30, 15);
                    bmp.DrawPolygon(list, symbolizer.strokecolor, (int)symbolizer.strokewidth);
                    bmp.FillPolygon(list, symbolizer.fillcolor);
                    if (rule.filter != null)
                    {
                        bmp.DrawString((int)position.X + 35, (int)cur_y, textcolor, Colors.White, font, rule.filter.GetDescription());
                        //g.DrawString(rule.filter.GetDescription(), SystemFonts.DefaultFont, Brushes.Black, new Point(position.X + 35, cur_y));
                    }
                    cur_y += (20);
                    break;
            }
        }

        public void PaintFillCircle(WriteableBitmap bmp, Point position, int size, Color color)
        {
            bmp.FillEllipseCentered((int)position.X, (int)position.Y, size, size, color);
        }
        public void PaintHollowCircle(WriteableBitmap bmp, Point position, int size, Color color)
        {
            bmp.DrawEllipseCentered((int)position.X, (int)position.Y, size, size, color);
        }
        public void PaintFillRect(WriteableBitmap bmp, Point position, int size, Color color)
        {
            //g.FillRectangle(brush, position.X - size / 2, position.Y - size / 2, size, size);
            bmp.FillRectangle((int)position.X - size / 2, (int)position.Y - size / 2, (int)position.X + size / 2, (int)position.Y + size / 2, color);
        }

        public void PaintHollowRect(WriteableBitmap bmp, Point position, int size, Color color)
        {
            //g.DrawRectangle(pen, position.X - size / 2, position.Y - size / 2, size, size);
            bmp.DrawRectangle((int)position.X - size / 2, (int)position.Y - size / 2, (int)position.X + size / 2, (int)position.Y + size / 2, color);
        }
        public void PaintFillTriangle(WriteableBitmap bmp, Point position, int size, Color color)
        {
            int[] points = new int[8];
            points[0] = (int)position.X;
            points[1] = (int)position.Y - (int)(1.154 * size);
            points[2] = (int)position.X - size;
            points[3] = (int)(position.Y + 0.577 * size);
            points[4] = (int)position.X + size;
            points[5] = (int)(position.Y + 0.577 * size);
            points[6] = (int)position.X;
            points[7] = points[1];

            //g.FillPolygon(brush, points);
            bmp.FillPolygon(points, color);
        }

        public void PaintHollowTriangle(WriteableBitmap bmp, Point position, int size, Color color)
        {
            int[] points = new int[6];
            points[0] = (int)position.X;
            points[1] = (int)position.Y - (int)(1.154 * size);
            points[2] = (int)position.X - size;
            points[3] = (int)(position.Y + 0.577 * size);
            points[4] = (int)position.X + size;
            points[5] = (int)(position.Y + 0.577 * size);
            bmp.DrawPolygon(points, color, 1);
        }

        public void PaintCircle_Point(WriteableBitmap bmp, Point position, int size, Color color)
        {
            //g.DrawEllipse(pen, position.X, position.Y, size, size);
            //g.FillEllipse(brush, position.X + size / 8 * 3, position.Y + size / 8 * 3, size / 4, size / 4);

            bmp.DrawEllipseCentered((int)position.X, (int)position.Y, size, size, color);
            bmp.FillEllipseCentered((int)position.X, (int)position.Y, size / 2, size / 2, color);
        }

        public void PaintCircle_Rect(WriteableBitmap bmp, Point position, int size, Color color)
        {
            //g.DrawEllipse(pen, position.X, position.Y, size, size);
            //g.DrawRectangle(pen, position.X + size / 4, position.Y + size / 4, size / 2, size / 2);

            bmp.DrawEllipseCentered((int)position.X, (int)position.Y, size, size, color);
            bmp.DrawRectangle((int)position.X + size / 4, (int)position.Y + size / 4, size / 2, size / 2, color);
        }
        public override void Paint(WriteableBitmap bmp)
        {
            cur_y = position.Y;
            //base.PaintAdditional(g);
            foreach (Layer layer in mapcontent.layerlist)
            {
                if (!layer.visible) continue;
                bmp.DrawString((int)position.X, (int)cur_y, titlecolor, Colors.White, titlefont, layer.Layername);
                //g.DrawString(layer.Layername, SystemFonts.DefaultFont, Brushes.Black, new Point(position.X, cur_y));
                cur_y += 25;
                GisSmartTools.Support.Style style = layer.style;
                foreach (RenderRule rule in style.rulelist)
                {
                    paintRule(bmp, rule, cur_y);
                    cur_y += (20);
                }
                if (style.rulelist.Count == 0)
                {
                    paintRule(bmp, style.defaultRule, cur_y);
                    cur_y += 30;
                }
            }
        }
    }
}
