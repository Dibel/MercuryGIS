using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GisSmartTools.Data;
using GisSmartTools.Filter;
using GisSmartTools.Geometry;
using GisSmartTools.RS;
using GisSmartTools.Support;
using GisSmartTools.Topology;
using System.Threading;

namespace GisSmartTools
{

    public class ScreenPoint
    {
        public int X;
        public int Y;

        public ScreenPoint()
        {
        }

        public ScreenPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    /// <summary>
    /// MapControlWPF.xaml 的交互逻辑
    /// </summary>
    public partial class MapControlWPF : Image
    {

        #region 数据信息
        //显示信息
        private double moffsetX = -170D;//X方向上的偏移量(地图放大后超出视域范围，表示视域左上角点在地图上的对应坐标)
        private double moffsetY = 90D;
        private double DisplayScale = 0.8D;//显示比例尺的倒数   相当于课上的resolution
        private const double mcZoomRation = 1.2D;//地图放大系数 每次方法多少倍
        private int bitmap_X = 800;
        private int bitmap_Y = 800;
        private int interval = 2;//容县
        //private RSTransform rstransfrom = new RSTransform_NO_TRANSTRAM();

        //鼠标状态
        private Point mCurMouseLocation = new Point();//用于地图漫游时记录鼠标当前位置

        private Point mMouseLocation = new Point(0, 0);//用于记录刚刚按下的鼠标位置，之后用于获取屏幕矩形
        private System.Drawing.Rectangle screen_rect;

        private Point Editing_MouseLocation = new Point();
        private Cursor mCur_Cross = Cursors.Cross;
        //private Cursor nCur_Cross = new Cursor(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("MyMapObjects.Resources.Cross.ico"));
        private Cursor mCur_ZoomIn;
        private Cursor mCur_ZoomOut;
        private Cursor mCur_PanUp;

        //private Cursor mCur_Cross = new Cursor("Resource.Cross.ico");
        ////private Cursor nCur_Cross = new Cursor(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("MyMapObjects.Resources.Cross.ico"));
        //private Cursor mCur_ZoomIn = new Cursor("Resource.ZoomIn.ico");
        //private Cursor mCur_ZoomOut = new Cursor("Resource.ZoomOut.ico");
        //private Cursor mCur_PanUp = new Cursor("Resource.PanUp.ico");

        //private Cursor mCur_edit_select = new Cursor(typeof(GisSmartTools.MapControl), "Resource.edit_select.ico");
        //画选中的feature的预定义颜色
        private Color selectedFillColor = Colors.LightSkyBlue;
        private Color selectedStrokeColor = Colors.Blue;

        //状态信息
        private MapOptionStatus MapOption = MapOptionStatus.none;
        private bool IsEditing = false;
        //有关编辑的数据信息
        public EditingManager editmanager;

        //数据
        public mapcontent mapcontent;

        public Layer focuslayer = null;//当前正在编辑或正处于被选择的图层，默认为mapcontent 的0号图层
        public WriteableBitmap globalbmp = BitmapFactory.New(800, 500);
        public WriteableBitmap textbmp = BitmapFactory.New(800, 500);
        private WriteableBitmap bmp;
        private WriteableBitmap temp_textbmp;
        private WriteableBitmap linebmp;
        private WriteableBitmap gridbmp;
        #endregion

        #region 自定义事件区
        //代理（事件）定义去
        //框选点选查询结果事件处理
        public delegate void AfterSelectedFeatures(FeatureCollection collenction);
        public event AfterSelectedFeatures AfterSelectedFeaturesEvent;
        private void RaiseAfterSelectedFeaturesEvent(FeatureCollection collection)
        {
            if (AfterSelectedFeaturesEvent != null)
            {
                AfterSelectedFeaturesEvent(collection);
            }
        }
        //查询结果处理事件
        public delegate void AfterQueryFeatures(FeatureCollection collenction);
        public event AfterQueryFeatures AfterQueryFeaturesEvent;
        private void RaiseQueryFeaturesEvent(FeatureCollection collection)
        {
            if (AfterQueryFeaturesEvent != null)
            {
                AfterQueryFeaturesEvent(collection);
            }
        }
        //鼠标在picturebox上移动附加事件
        public delegate void MouseMove_OnPictureBox(object sender, MouseEventArgs e);
        public event MouseMove_OnPictureBox mousemoveEvent;
        private void RaiseMouseMove_PictureBoxEvent(object sender, MouseEventArgs e)
        {
            if (mousemoveEvent != null)
            {
                mousemoveEvent(sender, e);
            }
        }

        //比例尺系数发生变化时发生
        public delegate void DisplayScaleChangedHandl(object sender);
        public event DisplayScaleChangedHandl DisplayScaleChanged;
        private void RaiseDisplayScaleChanged(object sender)
        {
            if (DisplayScaleChanged != null)
            {
                DisplayScaleChanged(sender);
            }
        }

        #endregion

        public MapControlWPF()
        {
            InitializeComponent();
            this.MouseWheel += new MouseWheelEventHandler(MapControl_MouseWheel);
            this.MouseDown += new MouseButtonEventHandler(pictureBox1_MouseDown);
            this.MouseMove += new MouseEventHandler(pictureBox1_MouseMove);
            this.MouseUp += new MouseButtonEventHandler(pictureBox1_MouseUp);

            mCur_PanUp = ((TextBlock)Resources["Pan"]).Cursor;
            mCur_ZoomIn = ((TextBlock)Resources["ZoomIn"]).Cursor;
            mCur_ZoomOut = ((TextBlock)Resources["ZoomOut"]).Cursor;
            //Fixed size
            bitmap_X = 800;
            bitmap_Y = 500;
            InitializeBackground();
        }

        private void InitializeBackground()
        {
            gridbmp = BitmapFactory.New(bitmap_X, bitmap_Y);
            using (gridbmp.GetBitmapContext())
            {

                gridbmp.Clear(Colors.White);

                for (int i = 4; i < 801; i += 8)
                {
                    gridbmp.DrawLineAa(i, 0, 0, i, Colors.Black);
                    gridbmp.DrawLineAa(i, 802, 802, i, Colors.Black);
                }
                linebmp = gridbmp.Flip(WriteableBitmapExtensions.FlipMode.Vertical);
                gridbmp.Blit(new Rect(0, 0, 802, 802), linebmp, new Rect(0, 0, 802, 802), WriteableBitmapExtensions.BlendMode.ColorKeying);
            }
            gridbmp = gridbmp.Crop(0, 0, bitmap_X, bitmap_Y);
            linebmp = linebmp.Crop(0, 0, bitmap_X, bitmap_Y);
        }



        #region 实际渲染代码
        private void paintmap(mapcontent mapcontent)
        {
            if (mapcontent == null) return;
           
            using (bmp.GetBitmapContext())
            {
                using (temp_textbmp.GetBitmapContext())
                {
                    List<Layer> layerlist = mapcontent.layerlist;
                    for (int i = layerlist.Count - 1; i >= 0; i--)
                    {

                        Layer layer = layerlist[i];
                        if (layer.visible)
                        {
                            //获取targetSRS,sourceSRS,然后获取RSTransform对象///////????????????????????????????????????????????????
                            RSTransform rstransform = RSTransformFactory.getRSTransform(layer.getReference(), mapcontent.srs);
                            //RSTransform rstransform = new RSTransform_WGS84_WEBMOCARTO();
                            paintLayerbyStyle(layer, rstransform);
                        }

                    }
                }
            }
            
        }
        private void paintLayerbyStyle(Layer layer, RSTransform rstransform)
        {
            Support.Style style = layer.style;
            FeatureSource featuresource = layer.featuresource;
            foreach (Feature feature in featuresource.features.featureList)
            {
                if (!feature.visible) continue;
                switch (feature.geometry.geometryType)
                {
                    case OSGeo.OGR.wkbGeometryType.wkbPoint:
                        paintPointFeaturebyRule(feature, SelectRendererRule(feature, style), rstransform);
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbLineString:
                        paintLineFeaturebyRule(feature, SelectRendererRule(feature, style), rstransform);
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                        paintMutiLinebyRule(feature, SelectRendererRule(feature, style), rstransform);
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                        paintpolygonFeaturebyRule(feature, SelectRendererRule(feature, style), rstransform);
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
                        paintMultiPolygonbyRule(feature, SelectRendererRule(feature, style), rstransform);
                        break;
                }
            }
        }


        /// <summary>
        /// 点要素根据对应的RenderRule进行渲染
        /// </summary>
        /// <param name="g"></param>
        /// <param name="feature"></param>
        /// <param name="rule"></param>
        /// <param name="rstransform"></param>
        private void paintPointFeaturebyRule(Feature feature, RenderRule rule, RSTransform rstransform)
        {
            //渲染图形数据
            PointD originpoint = (PointD)feature.geometry;
            Symbolizer tempsymbolizer = rule.geometrysymbolizer;
            if (tempsymbolizer.sign != SymbolizerType.POINT) return;
            pointsymbolizer pointsymbolizer = (pointsymbolizer)tempsymbolizer;

            PointD mapPointD = (PointD)rstransform.sourceToTarget(originpoint);
            ScreenPoint screenPointF = FromMapPoint(mapPointD);
            int size = (int)pointsymbolizer.size;
            
            switch (pointsymbolizer.pointstyle)
            {
                case PointStyle.CIRCLE_FILL:
                    PaintFillCircle(screenPointF, size, pointsymbolizer.color);
                    break;
                case PointStyle.CIRCLE_HOLLOW:
                    PaintHollowCircle(screenPointF, size, pointsymbolizer.color);
                    break;
                case PointStyle.CIRCLE_POINT:
                    PaintCircle_Point(screenPointF, size, pointsymbolizer.color);
                    break;
                case PointStyle.CIRCLE_RECT:
                    PaintCircle_Rect(screenPointF, size, pointsymbolizer.color);
                    break;
                case PointStyle.RECT_FILL:
                    PaintFillRect(screenPointF, size, pointsymbolizer.color);
                    break;
                case PointStyle.RECT_HOLLOW:
                    PaintHollowRect(screenPointF, size, pointsymbolizer.color);
                    break;
                case PointStyle.TRIANGLE:
                    PaintFillTriangle(screenPointF, size, pointsymbolizer.color);
                    break;
            }

            //渲染注记
            if (rule.textsymbolizer != null && temp_textbmp != null)
            {
                textsymbolizer textsym = (textsymbolizer)rule.textsymbolizer;
                if (textsym.visible)
                {
                    //Brush textbrush = new SolidBrush(textsym.color);
                    // Font f = new Font(textsym.fontfamily, textsym.fontsize, textsym.fontstyle);
                    Object text = "";
                    if ((text = feature.GetArrtributeByName(textsym.attributename)) != null)
                    {
                        if (textsym.font == null) { textsym.font = new PortableFontDesc(emsize: 8); }
                        temp_textbmp.DrawString(screenPointF.X + (int)textsym.offset_x, screenPointF.Y + (int)textsym.offset_y, (System.Windows.Media.Color)textsym.color, Colors.White, textsym.font, text.ToString());
                    }
                }
            }

        }


        public  void PaintFillCircle(ScreenPoint position, int size, Color color)
        {
            bmp.FillEllipseCentered(position.X, position.Y, size, size, color);
        }
        public  void PaintHollowCircle(ScreenPoint position, int size, Color color)
        {
            bmp.DrawEllipseCentered(position.X, position.Y, size, size, color);
        }
        public  void PaintFillRect(ScreenPoint position, int size, Color color)
        {
            //g.FillRectangle(brush, position.X - size / 2, position.Y - size / 2, size, size);
            bmp.FillRectangle(position.X - size / 2, position.Y - size / 2, position.X + size / 2, position.Y + size / 2, color);
        }

        public  void PaintHollowRect(ScreenPoint position, int size, Color color)
        {
            //g.DrawRectangle(pen, position.X - size / 2, position.Y - size / 2, size, size);
            bmp.DrawRectangle(position.X - size / 2, position.Y - size / 2, position.X + size / 2, position.Y + size / 2, color);
        }
        public  void PaintFillTriangle(ScreenPoint position, int size, Color color)
        {
            int[] points = new int[8];
            points[0] = position.X;
            points[1] = position.Y - (int)(1.154 * size);
            points[2] = position.X - size;
            points[3] = (int)(position.Y + 0.577 * size);
            points[4] = position.X + size;
            points[5] = (int)(position.Y + 0.577 * size);
            points[6] = position.X;
            points[7] = points[1];

            //g.FillPolygon(brush, points);
            bmp.FillPolygon(points, color);
        }

        public void PaintHollowTriangle(ScreenPoint position, int size, Color color)
        {
            int[] points = new int[6];
            points[0] = position.X;
            points[1] = position.Y - (int)(1.154 * size);
            points[2] = position.X - size;
            points[3] = (int)(position.Y + 0.577 * size);
            points[4] = position.X + size;
            points[5] = (int)(position.Y + 0.577 * size);
            bmp.DrawPolygon(points, color, 1);
        }

        public  void PaintCircle_Point(ScreenPoint position, int size, Color color)
        {
            //g.DrawEllipse(pen, position.X, position.Y, size, size);
            //g.FillEllipse(brush, position.X + size / 8 * 3, position.Y + size / 8 * 3, size / 4, size / 4);

            bmp.DrawEllipseCentered(position.X, position.Y, size, size, color);
            bmp.FillEllipseCentered(position.X, position.Y, size / 2, size / 2, color);
        }

        public  void PaintCircle_Rect(ScreenPoint position, int size, Color color)
        {
            //g.DrawEllipse(pen, position.X, position.Y, size, size);
            //g.DrawRectangle(pen, position.X + size / 4, position.Y + size / 4, size / 2, size / 2);

            bmp.DrawEllipseCentered(position.X, position.Y, size, size, color);
            bmp.DrawRectangle(position.X + size / 4, position.Y + size / 4, size / 2, size / 2, color);
        }


        //Convert ScreenPoint array to int array
        public int[] ConvertScreenPoint(ScreenPoint[] points)
        {
            int[] array = new int[points.Length * 2];
            for(int i = 0; i < points.Length; i++)
            {
                array[i * 2] = points[i].X;
                array[i * 2 + 1] = points[i].Y;
            }
            return array;
        }

        public int[] ConvertScreenPoint_AddFirst(ScreenPoint[] points)
        {
            int[] array = new int[points.Length * 2 + 2];
            int i;
            for (i = 0; i < points.Length; i++)
            {
                array[i * 2] = points[i].X;
                array[i * 2 + 1] = points[i].Y;
            }
            array[i * 2] = points[0].X;
            array[i * 2 + 1] = points[0].Y;
            return array;
        }

        public int[] ConvertScreenPoint(List<ScreenPoint> points)
        {
            var array = new int[points.Count * 2];
            for (int i = 0; i < points.Count; i++)
            {
                array[i * 2] = points[i].X;
                array[i * 2 + 1] = points[i].Y;
            }
            return array;
        }

        /// <summary>
        /// 画多边形类
        /// </summary>
        /// <param name="g"></param>
        /// <param name="feature"></param>
        /// <param name="rule"></param>
        /// <param name="rstransform"></param>
        private void paintpolygonFeaturebyRule(Feature feature, RenderRule rule, RSTransform rstransform)
        {
            //渲染图形数据
            SimplePolygon polygon = (SimplePolygon)feature.geometry;
            polygonsymbolizer symbolizer = (polygonsymbolizer)rule.geometrysymbolizer;
            if (symbolizer.sign != SymbolizerType.POLYGON) return;
            //Pen pen = new Pen(symbolizer.strokecolor, symbolizer.strokewidth);
            //Brush brush = new SolidBrush(symbolizer.fillcolor);

            switch (symbolizer.polygonstyle)
            {
                case PolygonStyle.SOLID:
                    foreach (SimplePolyline ring in polygon.rings)
                    {
                        List<int> screenpointlist = new List<int>();
                        foreach (PointD datapoint in ring.points)
                        {
                            ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                            screenpointlist.Add(screenpoint.X);
                            screenpointlist.Add(screenpoint.Y);
                        }
                        screenpointlist.Add(screenpointlist[0]);
                        screenpointlist.Add(screenpointlist[1]);
                        bmp.FillPolygon(screenpointlist.ToArray(), symbolizer.fillcolor);
                        bmp.DrawPolylineAa(screenpointlist.ToArray(), symbolizer.strokecolor, (int)symbolizer.strokewidth);
                        //list.AddRange(screenpointlist);
                        //graphicspath.AddPolygon(screenpointlist.ToArray());
                    }
                    break;
                case PolygonStyle.LINE:
                    var tempbmp = BitmapFactory.New(bitmap_X, bitmap_Y);
                    var bg = BitmapFactory.New(bitmap_X, bitmap_Y);
                    foreach (SimplePolyline ring in polygon.rings)
                    {
                        List<int> screenpointlist = new List<int>();
                        foreach (PointD datapoint in ring.points)
                        {
                            ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                            screenpointlist.Add(screenpoint.X);
                            screenpointlist.Add(screenpoint.Y);
                        }
                        screenpointlist.Add(screenpointlist[0]);
                        screenpointlist.Add(screenpointlist[1]);
                        tempbmp.FillPolygon(screenpointlist.ToArray(), Colors.Black);

                        bmp.DrawPolylineAa(screenpointlist.ToArray(), symbolizer.strokecolor, (int)symbolizer.strokewidth);
                        //list.AddRange(screenpointlist);
                        //graphicspath.AddPolygon(screenpointlist.ToArray());
                    }
                    bg.Clear((System.Windows.Media.Color)symbolizer.fillcolor);
                    Rect rect = new Rect(0, 0, bitmap_X, bitmap_Y);
                    bg.Blit(rect, linebmp, rect, WriteableBitmapExtensions.BlendMode.Additive);
                    bg.Blit(rect, tempbmp, rect, WriteableBitmapExtensions.BlendMode.Mask);
                    bmp.Blit(rect, bg, rect, WriteableBitmapExtensions.BlendMode.Alpha);
                    break;
                case PolygonStyle.GRID:
                    var tempbmp1 = BitmapFactory.New(bitmap_X, bitmap_Y);
                    var bg1 = BitmapFactory.New(bitmap_X, bitmap_Y);
                    foreach (SimplePolyline ring in polygon.rings)
                    {
                        List<int> screenpointlist = new List<int>();
                        foreach (PointD datapoint in ring.points)
                        {
                            ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                            screenpointlist.Add(screenpoint.X);
                            screenpointlist.Add(screenpoint.Y);
                        }
                        screenpointlist.Add(screenpointlist[0]);
                        screenpointlist.Add(screenpointlist[1]);
                        tempbmp1.FillPolygon(screenpointlist.ToArray(), Colors.Black);

                        //bmp.DrawPolyline(screenpointlist.ToArray(), symbolizer.strokecolor);
                        bmp.DrawPolylineAa(screenpointlist.ToArray(), symbolizer.strokecolor, (int)symbolizer.strokewidth);
                        //list.AddRange(screenpointlist);
                        //graphicspath.AddPolygon(screenpointlist.ToArray());
                    }
                    bg1.Clear((System.Windows.Media.Color)symbolizer.fillcolor);
                    Rect rect1 = new Rect(0, 0, bitmap_X, bitmap_Y);
                    bg1.Blit(rect1, gridbmp, rect1, WriteableBitmapExtensions.BlendMode.Additive);
                    bg1.Blit(rect1, tempbmp1, rect1, WriteableBitmapExtensions.BlendMode.Mask);
                    bmp.Blit(rect1, bg1, rect1, WriteableBitmapExtensions.BlendMode.Alpha);
                    break;
            }

            //List<PointF> list = new List<PointF>();
            //System.Drawing.Drawing2D.GraphicsPath graphicspath = new System.Drawing.Drawing2D.GraphicsPath();
            
            
            //g.DrawPath(pen, graphicspath);
            //g.FillPath(brush, graphicspath);
            //pen.Dispose();
            //brush.Dispose();

            //渲染注记
            if (rule.textsymbolizer != null && temp_textbmp != null)
            {
                textsymbolizer textsym = (textsymbolizer)rule.textsymbolizer;
                if (textsym.visible)
                {
                    //Brush textbrush = new SolidBrush(textsym.color);
                    Object text = "";
                    //转换坐标
                    ScreenPoint minxy = FromMapPoint(rstransform.sourceToTarget(new PointD(polygon.minX, polygon.minY)));
                    ScreenPoint maxxy = FromMapPoint(rstransform.sourceToTarget(new PointD(polygon.maxX, polygon.maxY)));
                    if ((text = feature.GetArrtributeByName(textsym.attributename)) != null)
                    {
                        if (textsym.font == null) { textsym.font = new PortableFontDesc(emsize: 8); }
                        temp_textbmp.DrawString((int)((minxy.X + maxxy.X) / 2 + textsym.offset_x), (int)((minxy.Y + maxxy.Y) / 2 + textsym.offset_y), (System.Windows.Media.Color)textsym.color, Colors.White, textsym.font, text.ToString());
                        //text_g.DrawString(text.ToString(), textsym.font, textbrush, (float)((minxy.X + maxxy.X) / 2 + textsym.offset_x), (float)((minxy.Y + maxxy.Y) / 2 + textsym.offset_y));
                    }
                    //textbrush.Dispose();
                }
            }

        }

        private void paintMultiPolygonbyRule(Feature feature, RenderRule rule, RSTransform rstransform)
        {
            Geometry.Polygon polygon = (Geometry.Polygon)feature.geometry;
            polygonsymbolizer symbolizer = (polygonsymbolizer)rule.geometrysymbolizer;
            if (symbolizer.sign != SymbolizerType.POLYGON) return;
            //Pen pen = new Pen(symbolizer.strokecolor, symbolizer.strokewidth);
            //Brush brush = new SolidBrush(symbolizer.fillcolor);

            switch (symbolizer.polygonstyle)
            {
                case PolygonStyle.SOLID:
                    foreach (SimplePolygon simplepolygon in polygon.childPolygons)
                    {
                        List<int[]> list = new List<int[]>();
                        foreach (SimplePolyline ring in simplepolygon.rings)
                        {
                            List<int> screenpointlist = new List<int>();
                            foreach (PointD datapoint in ring.points)
                            {
                                ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                                screenpointlist.Add(screenpoint.X);
                                screenpointlist.Add(screenpoint.Y);
                            }
                            //bmp.DrawPolygon(screenpointlist.ToArray(), symbolizer.strokecolor, (int)symbolizer.strokewidth);
                            //bmp.FillPolygon(screenpointlist.ToArray(), symbolizer.fillcolor);
                            list.Add(screenpointlist.ToArray());
                        }
                        bmp.FillPolygonsEvenOdd(list.ToArray(), symbolizer.fillcolor);
                        foreach (SimplePolyline ring in simplepolygon.rings)
                        {
                            List<int> screenpointlist = new List<int>();
                            foreach (PointD datapoint in ring.points)
                            {
                                ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                                screenpointlist.Add(screenpoint.X);
                                screenpointlist.Add(screenpoint.Y);
                            }
                            bmp.DrawPolygon(screenpointlist.ToArray(), symbolizer.strokecolor, (int)symbolizer.strokewidth);
                        }
                        //g.DrawPolygon(pen, list.ToArray());
                        //g.FillPolygon(brush, list.ToArray());
                    }
                    break;
                case PolygonStyle.LINE:
                    var tempbmp = BitmapFactory.New(bitmap_X, bitmap_Y);
                    var bg = BitmapFactory.New(bitmap_X, bitmap_Y);
                    bg.Clear((System.Windows.Media.Color)symbolizer.fillcolor);
                    foreach (SimplePolygon simplepolygon in polygon.childPolygons)
                    {
                        List<int[]> list = new List<int[]>();
                        foreach (SimplePolyline ring in simplepolygon.rings)
                        {
                            List<int> screenpointlist = new List<int>();
                            foreach (PointD datapoint in ring.points)
                            {
                                ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                                screenpointlist.Add(screenpoint.X);
                                screenpointlist.Add(screenpoint.Y);
                            }
                            list.Add(screenpointlist.ToArray());
                        }
                        
                        tempbmp.FillPolygonsEvenOdd(list.ToArray(), Colors.Black);
                        foreach (SimplePolyline ring in simplepolygon.rings)
                        {
                            List<int> screenpointlist = new List<int>();
                            foreach (PointD datapoint in ring.points)
                            {
                                ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                                screenpointlist.Add(screenpoint.X);
                                screenpointlist.Add(screenpoint.Y);
                            }
                            bmp.DrawPolygon(screenpointlist.ToArray(), symbolizer.strokecolor, (int)symbolizer.strokewidth);
                        }
                    }

                    Rect rect = new Rect(0, 0, bitmap_X, bitmap_Y);
                    bg.Blit(rect, linebmp, rect, WriteableBitmapExtensions.BlendMode.Additive);
                    bg.Blit(rect, tempbmp, rect, WriteableBitmapExtensions.BlendMode.Mask);
                    bmp.Blit(rect, bg, rect, WriteableBitmapExtensions.BlendMode.Alpha);
                    break;
                case PolygonStyle.GRID:
                    var tempbmp1 = BitmapFactory.New(bitmap_X, bitmap_Y);
                    var bg1 = BitmapFactory.New(bitmap_X, bitmap_Y);
                    bg1.Clear((System.Windows.Media.Color)symbolizer.fillcolor);
                    foreach (SimplePolygon simplepolygon in polygon.childPolygons)
                    {
                        List<int[]> list = new List<int[]>();
                        foreach (SimplePolyline ring in simplepolygon.rings)
                        {
                            List<int> screenpointlist = new List<int>();
                            foreach (PointD datapoint in ring.points)
                            {
                                ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                                screenpointlist.Add(screenpoint.X);
                                screenpointlist.Add(screenpoint.Y);
                            }
                            list.Add(screenpointlist.ToArray());
                        }

                        tempbmp1.FillPolygonsEvenOdd(list.ToArray(), Colors.Black);
                        foreach (SimplePolyline ring in simplepolygon.rings)
                        {
                            List<int> screenpointlist = new List<int>();
                            foreach (PointD datapoint in ring.points)
                            {
                                ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                                screenpointlist.Add(screenpoint.X);
                                screenpointlist.Add(screenpoint.Y);
                            }
                            bmp.DrawPolygon(screenpointlist.ToArray(), symbolizer.strokecolor, (int)symbolizer.strokewidth);
                        }

                    }

                    Rect rect1 = new Rect(0, 0, bitmap_X, bitmap_Y);
                    bg1.Blit(rect1, gridbmp, rect1, WriteableBitmapExtensions.BlendMode.Additive);
                    bg1.Blit(rect1, tempbmp1, rect1, WriteableBitmapExtensions.BlendMode.Mask);
                    bmp.Blit(rect1, bg1, rect1, WriteableBitmapExtensions.BlendMode.Alpha);
                    break;
            }

            //pen.Dispose();
            //brush.Dispose();
            //渲染注记
            if (rule.textsymbolizer != null && temp_textbmp != null)
            {
                textsymbolizer textsym = (textsymbolizer)rule.textsymbolizer;
                if (textsym.visible)
                {
                    //Brush textbrush = new SolidBrush(textsym.color);
                    Object text = "";
                    //转换坐标
                    ScreenPoint minxy = FromMapPoint(rstransform.sourceToTarget(new PointD(polygon.minX, polygon.minY)));
                    ScreenPoint maxxy = FromMapPoint(rstransform.sourceToTarget(new PointD(polygon.maxX, polygon.maxY)));
                    if ((text = feature.GetArrtributeByName(textsym.attributename)) != null)
                    {
                        if (textsym.font == null) { textsym.font = new PortableFontDesc(emsize: 8); }
                        temp_textbmp.DrawString((int)((minxy.X + maxxy.X) / 2 + textsym.offset_x), (int)((minxy.Y + maxxy.Y) / 2 + textsym.offset_y), (System.Windows.Media.Color)textsym.color, Colors.White, textsym.font, text.ToString());
                        //text_g.DrawString(text.ToString(), textsym.font, textbrush, (float)((minxy.X + maxxy.X) / 2 + textsym.offset_x), (float)((minxy.Y + maxxy.Y) / 2 + textsym.offset_y));
                    }
                    //textbrush.Dispose();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="feature"></param>
        /// <param name="rule"></param>
        /// <param name="rstransform"></param>
        private void paintLineFeaturebyRule(Feature feature, RenderRule rule, RSTransform rstransform)
        {
            //渲染图形数据
            SimplePolyline line = (SimplePolyline)feature.geometry;
            linesymbolizer symbolizer = (linesymbolizer)rule.geometrysymbolizer;
            if (symbolizer.sign != SymbolizerType.LINE) return;
            //Pen pen = new Pen(symbolizer.color, symbolizer.width);
            //pen.DashStyle = symbolizer.linestyle;
            List<int> screenpointlist = new List<int>();
            foreach (PointD datapoint in line.points)
            {
                ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                screenpointlist.Add(screenpoint.X);
                screenpointlist.Add(screenpoint.Y);
            }
            //TODO: LineStyle
            //bmp.DrawCurve(screenpointlist.ToArray(), 0, symbolizer.color);
            bmp.DrawPolylineAa(screenpointlist.ToArray(), symbolizer.color, (int)symbolizer.width);
            //渲染注记
            if (rule.textsymbolizer != null && temp_textbmp != null)
            {
                textsymbolizer textsym = (textsymbolizer)rule.textsymbolizer;
                if (textsym.visible)
                {
                    //Brush textbrush = new SolidBrush(textsym.color);
                    // Font f = new Font(textsym.fontfamily, textsym.fontsize, textsym.fontstyle);
                    Object text = "";
                    //转换坐标
                    ScreenPoint minxy = FromMapPoint(rstransform.sourceToTarget(new PointD(line.minX, line.minY)));
                    ScreenPoint maxxy = FromMapPoint(rstransform.sourceToTarget(new PointD(line.maxX, line.maxY)));
                    if ((text = feature.GetArrtributeByName(textsym.attributename)) != null)
                    {
                        if (textsym.font == null) { textsym.font = new PortableFontDesc(emsize: 8); }
                        temp_textbmp.DrawString((int)((minxy.X + maxxy.X) / 2 + textsym.offset_x), (int)((minxy.Y + maxxy.Y) / 2 + textsym.offset_y), (System.Windows.Media.Color)textsym.color, Colors.White, textsym.font, text.ToString());
                        //text_g.DrawString(text.ToString(), textsym.font, textbrush, (float)((minxy.X + maxxy.X) / 2 + textsym.offset_x), (float)((minxy.Y + maxxy.Y) / 2 + textsym.offset_y));
                    }
                    //textbrush.Dispose();
                }
            }
        }

        private void paintMutiLinebyRule(Feature feature, RenderRule rule, RSTransform rstransform)
        {
            Geometry.Polyline mutiline = (Geometry.Polyline)feature.geometry;
            linesymbolizer symbolizer = (linesymbolizer)rule.geometrysymbolizer;
            if (symbolizer.sign != SymbolizerType.LINE) return;
            //Pen pen = new Pen(symbolizer.color, symbolizer.width);
            foreach (SimplePolyline line in mutiline.childPolylines)
            {
                List<int> screenpointlist = new List<int>();
                foreach (PointD datapoint in line.points)
                {
                    ScreenPoint screenpoint = FromMapPoint(rstransform.sourceToTarget(datapoint));
                    screenpointlist.Add(screenpoint.X);
                    screenpointlist.Add(screenpoint.Y);
                }
                bmp.DrawPolylineAa(screenpointlist.ToArray(), symbolizer.color, (int)symbolizer.width);
                //g.DrawLines(pen, screenpointlist.ToArray());
            }
            //pen.Dispose();
            //渲染注记
        }

        /// <summary>
        /// get current system time by millisecond
        /// </summary>
        /// <returns></returns>
        private long GetCurTime()
        {
            int m = DateTime.Now.Minute, s = DateTime.Now.Second, ms = DateTime.Now.Millisecond;
            return (ms + s * 1000 + m * 60 * 1000);
        }

        //双缓冲技术，在图片上渲染featurecollection，使用预定义的Rule
        /// <summary>
        /// 本函数外部可调用
        /// </summary>
        /// <param name="collection"></param>
        public WriteableBitmap paintadditionalfeaturecollection(FeatureCollection collection)
        {
            paintadditionalfeaturecollection(collection, this.selectedFillColor, this.selectedStrokeColor);
            if (bmp != null)
            {
                this.Source = bmp;
                //pictureBox1.Refresh();
            }
            return bmp;
        }

        //public WriteableBitmap paintadditionalfeaturecollection(FeatureCollection collection, Color FillColor, Color StrokeColor)
        //{
        //    if (collection == null || collection.featureList.Count == 0) return null;
        //    return paintadditionalfeaturecollection(collection, FillColor, StrokeColor);
        //}

        //双缓冲技术，在图片上渲染featurecollection，使用预定义的Rule和自定义的color
        /// <summary>
        /// 本函数外部可调用
        /// </summary>
        /// <param name="collection"></param>
        public WriteableBitmap paintadditionalfeaturecollection(FeatureCollection collection, Color FillColor, Color StrokeColor)
        {
            WriteableBitmap bmp = globalbmp.Clone();
            if (collection == null || collection.featureList.Count == 0) return bmp;
            if (bmp == null) return bmp;
            int featurecount = collection.featureList.Count;
            /////////////////????????????????????????/坐标系问题
            RSTransform rstransform = RSTransformFactory.getRSTransform(collection.featureList[0].schema.rs, mapcontent.srs);
            //RSTransform rstransform = new RSTransform_WGS84_WEBMOCARTO();
            using (bmp.GetBitmapContext())
            {
                foreach (Feature feature in collection.featureList)
                {
                    if (feature.geometry != null)
                        switch (feature.geometry.geometryType)
                        {
                            case OSGeo.OGR.wkbGeometryType.wkbPoint:
                                RenderRule pointrule = RenderRule.createDefaultRule(SymbolizerType.POINT);
                                pointsymbolizer pointsymbolizer = (pointsymbolizer)pointrule.geometrysymbolizer;
                                pointsymbolizer.color = FillColor;
                                paintPointFeaturebyRule(feature, pointrule, rstransform);
                                break;
                            case OSGeo.OGR.wkbGeometryType.wkbLineString:
                                RenderRule linerule = RenderRule.createDefaultRule(SymbolizerType.LINE);
                                linesymbolizer linesymbolizer = (linesymbolizer)linerule.geometrysymbolizer;
                                linesymbolizer.color = StrokeColor;
                                paintLineFeaturebyRule(feature, linerule, rstransform);
                                break;
                            case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                                RenderRule polygonrule = RenderRule.createDefaultRule(SymbolizerType.POLYGON);
                                polygonsymbolizer polygonsymbolizer = (polygonsymbolizer)polygonrule.geometrysymbolizer;
                                polygonsymbolizer.strokecolor = StrokeColor; polygonsymbolizer.fillcolor = FillColor;
                                paintpolygonFeaturebyRule(feature, polygonrule, rstransform);
                                break;
                            case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                                RenderRule linerule1 = RenderRule.createDefaultRule(SymbolizerType.LINE);
                                linesymbolizer linesymbolizer1 = (linesymbolizer)linerule1.geometrysymbolizer;
                                linesymbolizer1.color = StrokeColor;
                                paintMutiLinebyRule(feature, linerule1, rstransform);
                                break;
                            case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
                                RenderRule polygonrule1 = RenderRule.createDefaultRule(SymbolizerType.POLYGON);
                                polygonsymbolizer polygonsymbolizer1 = (polygonsymbolizer)polygonrule1.geometrysymbolizer;
                                polygonsymbolizer1.strokecolor = StrokeColor; polygonsymbolizer1.fillcolor = FillColor;
                                paintMultiPolygonbyRule(feature, polygonrule1, rstransform);
                                break;
                        }
                }
            }
            return bmp;
        }

        /// <summary>
        /// 画正在编辑的要素
        /// </summary>
        /// <param name="g"></param>
        /// <param name="transfrom"></param>
        public void drawtrackingbmp(EditingManager editmanager, RS.RSTransform transfrom, Point mousemovelocation)
        {


            if (editmanager == null) return;
            WriteableBitmap editbmp = globalbmp.Clone();

            using (editbmp.GetBitmapContext())
            {
                //Pen pen = new Pen(editmanager.strokecolor, 3);
                List<ScreenPoint[]> screenlist = new List<ScreenPoint[]>();
                foreach (List<PointD> onelist in editmanager.lists)
                {
                    ScreenPoint[] screenpoints = new ScreenPoint[onelist.Count];
                    for (int i = 0; i < onelist.Count; i++)
                    {
                        screenpoints[i] = FromMapPoint(transfrom.sourceToTarget(onelist[i]));
                    }
                    screenlist.Add(screenpoints);
                }
                //Brush brush = new SolidBrush(editmanager.Fillcolor);
                ScreenPoint[] lastscreenpoints = new ScreenPoint[editmanager.editingpoints.Count + 1];
                for (int i = 0; i < editmanager.editingpoints.Count; i++)
                {
                    lastscreenpoints[i] = FromMapPoint(transfrom.sourceToTarget(editmanager.editingpoints[i]));
                }
                lastscreenpoints[editmanager.editingpoints.Count] = new ScreenPoint((int)mousemovelocation.X, (int)mousemovelocation.Y);
                screenlist.Add(lastscreenpoints);
                switch (editmanager.geometrytype)
                {
                    case OSGeo.OGR.wkbGeometryType.wkbPoint:
                        foreach (ScreenPoint[] screenpoints in screenlist)
                        {
                            for (int i = 0; i < screenpoints.Length; i++)
                            {
                                //PaintFillRect(screenpoints[i], 10, editmanager.Fillcolor);
                                editbmp.FillRectangle(screenpoints[i].X - 10 / 2, screenpoints[i].Y - 10 / 2, screenpoints[i].X + 10 / 2, screenpoints[i].Y + 10 / 2, editmanager.Fillcolor);
                            }
                        }
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbLineString:
                        foreach (ScreenPoint[] screenpoints in screenlist)
                        {
                            for (int i = 0; i < screenpoints.Length; i++)
                            {
                                //PaintFillRect(screenpoints[i], 10, editmanager.Fillcolor);
                                editbmp.FillRectangle(screenpoints[i].X - 10 / 2, screenpoints[i].Y - 10 / 2, screenpoints[i].X + 10 / 2, screenpoints[i].Y + 10 / 2, editmanager.Fillcolor);

                            }
                            if (screenpoints.Length > 1)
                            {
                                editbmp.DrawPolylineAa(ConvertScreenPoint(screenpoints), editmanager.strokecolor, 3);
                                //g.DrawLines(pen, screenpoints);
                            }
                        }
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                        //System.Drawing.Drawing2D.GraphicsPath grapath = new System.Drawing.Drawing2D.GraphicsPath();
                        foreach (ScreenPoint[] screenpoints in screenlist)
                        {
                            for (int i = 0; i < screenpoints.Length; i++)
                            {
                                //PaintFillRect(screenpoints[i], 10, editmanager.Fillcolor);
                                editbmp.FillRectangle(screenpoints[i].X - 10 / 2, screenpoints[i].Y - 10 / 2, screenpoints[i].X + 10 / 2, screenpoints[i].Y + 10 / 2, editmanager.Fillcolor);
                            }
                            if (screenpoints.Length == 2) editbmp.DrawLineAa(screenpoints[0].X, screenpoints[0].Y, screenpoints[1].X, screenpoints[1].Y, editmanager.strokecolor, 3);
                            if (screenpoints.Length > 2)
                            {
                                editbmp.FillPolygon(ConvertScreenPoint_AddFirst(screenpoints), editmanager.Fillcolor);
                                //grapath.AddPolygon(screenpoints);
                            }
                        }
                        //g.FillPath(brush, grapath);
                        break;
                }
            }

            Thread.Sleep(10);
            this.Source = editbmp;
        }



        #endregion

        #region 工具函数


        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="mouselocation"></param>
        /// <returns></returns>
        private bool CheckIsMouseInFeatureCollection(FeatureCollection collection, Point mouselocation)
        {
            screen_rect = new System.Drawing.Rectangle((int)(mouselocation.X - 2), (int)mouselocation.Y - 2, 4, 4);
            GisSmartTools.Geometry.Rectangle maprect_select = ToMapRect(screen_rect);
            Filter_Envelop filter_edit_selected = new Filter_Envelop(maprect_select, editmanager.rstransform);
            for (int i = 0; i < collection.featureList.Count; i++)
            {
                if (filter_edit_selected.Evaluate(collection.featureList[i])) return true;
            }
            return false;
        }


        /// <summary>
        /// 窗口大小保持一致
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            this.bitmap_X = Convert.ToInt32(this.RenderSize.Width);
            this.bitmap_Y = Convert.ToInt32(this.RenderSize.Height);
            if (mapcontent != null)
                this.SetDefaultoffsetandDisplayScale(this.mapcontent);
            this.mapcontrol_refresh();
        }
        /// <summary>
        /// 选择出style中适合给定feature渲染的rule
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        private RenderRule SelectRendererRule(Feature feature, Support.Style style)
        {
            if (style.rulelist == null || style.rulelist.Count == 0) return style.defaultRule;
            foreach (RenderRule rule in style.rulelist)
            {
                if (rule.filter.Evaluate(feature))
                    return rule;
            }
            return style.defaultRule;
        }

        /// <summary>
        /// 将地图坐标转换为屏幕坐标
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public ScreenPoint FromMapPoint(PointD point)
        {
            ScreenPoint sPoint = new ScreenPoint();
            sPoint.X = Convert.ToInt32((point.X - moffsetX) / DisplayScale);

            sPoint.Y = Convert.ToInt32((moffsetY - point.Y) / DisplayScale);
            return sPoint;
        }

        /// <summary>
        /// 将屏幕坐标转换为地图坐标
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public PointD ToMapPoint(Point point)
        {
            PointD sPoint = new PointD();
            sPoint.X = point.X * DisplayScale + moffsetX;
            sPoint.Y = moffsetY - point.Y * DisplayScale;
            return sPoint;
        }

        public GisSmartTools.Geometry.Rectangle ToMapRect(System.Drawing.Rectangle screen_rect)
        {
            double minx = screen_rect.X * DisplayScale + moffsetX;
            double maxy = moffsetY - screen_rect.Y * DisplayScale;
            double maxx = (screen_rect.X + screen_rect.Width) * DisplayScale + moffsetX;
            double miny = moffsetY - (screen_rect.Y + screen_rect.Height) * DisplayScale;
            return new Geometry.Rectangle(minx, miny, maxx, maxy);
        }

        public System.Drawing.Rectangle ToScreenRect(GisSmartTools.Geometry.Rectangle map_rect)
        {
            int screen_rect_x = (int)((map_rect.minX - moffsetX) / DisplayScale);
            int  screen_rect_y = (int)((moffsetY -  map_rect.maxY)/ DisplayScale);
            int screen_width = (int)((map_rect.maxX - moffsetX)/ DisplayScale - screen_rect_x);
            int screen_height =(int)((moffsetY - map_rect.minY) / DisplayScale - screen_rect_y);
           return  new System.Drawing.Rectangle(screen_rect_x, screen_rect_y, screen_width, screen_height);
        }

       

        /// <summary>
        /// 以指定点为中心，以指定系数进行缩放
        /// 调用此方法后控件不会自动重新绘制，需要调用refresh方法才能实现重新绘制
        /// </summary>
        /// <param name="center"></param>
        /// <param name="ration"></param>
        public void ZoomByCenter(PointD center, double ration)
        {
            double sDisplayScale = DisplayScale / ration; //缩放后的比例尺
            double soffsetX = moffsetX + (1 - 1 / ration) * (center.X - moffsetX);
            double soffsetY = moffsetY + (1 - 1 / ration) * (center.Y - moffsetY);
            moffsetX = soffsetX;
            moffsetY = soffsetY;
            DisplayScale = sDisplayScale;
            RaiseDisplayScaleChanged(this);
        }

        public void zoominbyrectangle(System.Drawing.Rectangle screenrect)
        {
            GisSmartTools.Geometry.Rectangle rect = ToMapRect(screenrect);
            moffsetX = rect.minX;
            moffsetY = rect.maxY;
            double scale_x = (rect.maxX - rect.minX) / bitmap_X;
            double scale_y = (rect.maxY - rect.minY) / bitmap_Y;
            if (scale_x < scale_y) DisplayScale = scale_y;
            else DisplayScale = scale_x;
            RaiseDisplayScaleChanged(this);
        }

        #endregion

        #region 鼠标事件区
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            switch (MapOption)
            {
                case MapOptionStatus.ZoomIn:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        mMouseLocation = e.GetPosition(this);
                        screen_rect = new System.Drawing.Rectangle((int)mMouseLocation.X - interval, (int)mMouseLocation.Y - interval, 2 * interval, 2 * interval);
                    }
                    break;
                case MapOptionStatus.ZoomOut:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Point sMouseLocation = e.GetPosition(this);
                        PointD sMapPoint = ToMapPoint(sMouseLocation);
                        ZoomByCenter(sMapPoint, 1 / mcZoomRation);
                        mapcontrol_refresh();
                    }
                    break;
                case MapOptionStatus.Pan:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        mCurMouseLocation = e.GetPosition(this);
                    }
                    break;
                case MapOptionStatus.Identify:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        mMouseLocation = e.GetPosition(this);
                        screen_rect = new System.Drawing.Rectangle((int)mMouseLocation.X - interval, (int)mMouseLocation.Y - interval, 2 * interval, 2 * interval);
                    }
                    break;
                case MapOptionStatus.Delete:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        mMouseLocation = e.GetPosition(this);
                        screen_rect = new System.Drawing.Rectangle((int)mMouseLocation.X - interval, (int)mMouseLocation.Y - interval, 2 * interval, 2 * interval);
                    }
                    break;
                case MapOptionStatus.select:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        mMouseLocation = new Point(e.GetPosition(this).X, e.GetPosition(this).Y);
                        screen_rect = new System.Drawing.Rectangle((int)mMouseLocation.X - interval, (int)mMouseLocation.Y - interval, 2 * interval, 2 * interval);
                    }
                    break;
                case MapOptionStatus.Edit:
                    if (e.RightButton == MouseButtonState.Pressed)
                    {
                        //this.ContextMenu = picturebox_menu;
                        //picturebox_menu.Show(e.Location);
                    }
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        switch (editmanager.editstatus)
                        {
                            case EditStatus.editing:
                                break;
                            case EditStatus.finished:
                                mMouseLocation = e.GetPosition(this);
                                screen_rect = new System.Drawing.Rectangle((int)mMouseLocation.X - interval, (int)mMouseLocation.Y - interval, 2 * interval, 2 * interval);
                                break;
                            case EditStatus.selected:
                                mCurMouseLocation = e.GetPosition(this);
                                mMouseLocation = e.GetPosition(this);
                                screen_rect = new System.Drawing.Rectangle((int)mMouseLocation.X - interval, (int)mMouseLocation.Y - interval, 2 * interval, 2 * interval);
                                if (editmanager.selectedstatus == selectedstatus.selectable)
                                {
                                    editmanager.editstatus = EditStatus.finished;
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            switch (MapOption)
            {
                case MapOptionStatus.ZoomIn:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Point cur_mouseposition = e.GetPosition(this);
                        WriteableBitmap tempbmp = globalbmp.Clone();
                        using (tempbmp.GetBitmapContext())
                        {
                            double rect_x1 = mMouseLocation.X;
                            double rect_x2 = cur_mouseposition.X;
                            if (mMouseLocation.X > cur_mouseposition.X) {
                                rect_x1 = cur_mouseposition.X;
                                rect_x2 = mMouseLocation.X;
                            }
                            double rect_y1 = mMouseLocation.Y;
                            double rect_y2 = cur_mouseposition.Y;
                            if (mMouseLocation.Y > cur_mouseposition.Y) {
                                rect_y1 = cur_mouseposition.Y;
                                rect_y2 = mMouseLocation.Y;
                            }
                            double rect_width = Math.Abs(mMouseLocation.X - cur_mouseposition.X);
                            double rect_height = Math.Abs(mMouseLocation.Y - cur_mouseposition.Y);
                            screen_rect = new System.Drawing.Rectangle((int)rect_x1, (int)rect_y1, (int)rect_width, (int)rect_width);
                            tempbmp.DrawRectangle((int)rect_x1, (int)rect_y1, (int)rect_x2, (int)rect_y2, Colors.Black);
                        }
                        this.Source = tempbmp;
                    }
                    break;
                case MapOptionStatus.Pan:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        //PointD sPreScreenPoint = new PointD(mCurMouseLocation.X, mCurMouseLocation.Y);
                        PointD sPrePoint = ToMapPoint(mCurMouseLocation);
                        Point sCurScreenPoint = e.GetPosition(this);
                        PointD sCurPoint = ToMapPoint(sCurScreenPoint);
                        moffsetX += sPrePoint.X - sCurPoint.X;
                        moffsetY += sPrePoint.Y - sCurPoint.Y;
                        mapcontrol_refresh();
                        mCurMouseLocation.X = e.GetPosition(this).X;
                        mCurMouseLocation.Y = e.GetPosition(this).Y;
                    }
                    break;
                case MapOptionStatus.Identify:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        //Point cur_mouseposition = e.GetPosition(this);
                        //Bitmap tempbmp = (Bitmap)this.globalbmp.Clone();
                        //Graphics g = Graphics.FromImage(tempbmp);
                        //Pen pen = Pens.Black;
                        //float rect_x = mMouseLocation.X; if (mMouseLocation.X > cur_mouseposition.X) rect_x = cur_mouseposition.X;
                        //float rect_width = Math.Abs(mMouseLocation.X - cur_mouseposition.X);
                        //float rect_y = mMouseLocation.Y; if (mMouseLocation.Y > cur_mouseposition.Y) rect_y = cur_mouseposition.Y;
                        //float rect_height = Math.Abs(mMouseLocation.Y - cur_mouseposition.Y);
                        //screen_rect = new System.Drawing.Rectangle((int)rect_x, (int)rect_y, (int)rect_width, (int)rect_height);
                        //g.DrawRectangle(pen, screen_rect);
                        //pictureBox1.Image = tempbmp; pictureBox1.Refresh();
                        //g.Dispose();
                        Point cur_mouseposition = e.GetPosition(this);
                        WriteableBitmap tempbmp = globalbmp.Clone();
                        using (tempbmp.GetBitmapContext())
                        {
                            double rect_x1 = mMouseLocation.X;
                            double rect_x2 = cur_mouseposition.X;
                            if (mMouseLocation.X > cur_mouseposition.X)
                            {
                                rect_x1 = cur_mouseposition.X;
                                rect_x2 = mMouseLocation.X;
                            }
                            double rect_y1 = mMouseLocation.Y;
                            double rect_y2 = cur_mouseposition.Y;
                            if (mMouseLocation.Y > cur_mouseposition.Y)
                            {
                                rect_y1 = cur_mouseposition.Y;
                                rect_y2 = mMouseLocation.Y;
                            }
                            double rect_width = Math.Abs(mMouseLocation.X - cur_mouseposition.X);
                            double rect_height = Math.Abs(mMouseLocation.Y - cur_mouseposition.Y);
                            screen_rect = new System.Drawing.Rectangle((int)rect_x1, (int)rect_y1, (int)rect_width, (int)rect_width);
                            tempbmp.DrawRectangle((int)rect_x1, (int)rect_y1, (int)rect_x2, (int)rect_y2, Colors.Black);
                        }
                        Thread.Sleep(10);
                        this.Source = tempbmp;

                    }
                    break;
                case MapOptionStatus.select:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Point cur_mouseposition = e.GetPosition(this);
                        WriteableBitmap tempbmp = globalbmp.Clone();
                        using (tempbmp.GetBitmapContext())
                        {
                            double rect_x1 = mMouseLocation.X;
                            double rect_x2 = cur_mouseposition.X;
                            if (mMouseLocation.X > cur_mouseposition.X)
                            {
                                rect_x1 = cur_mouseposition.X;
                                rect_x2 = mMouseLocation.X;
                            }
                            double rect_y1 = mMouseLocation.Y;
                            double rect_y2 = cur_mouseposition.Y;
                            if (mMouseLocation.Y > cur_mouseposition.Y)
                            {
                                rect_y1 = cur_mouseposition.Y;
                                rect_y2 = mMouseLocation.Y;
                            }
                            double rect_width = Math.Abs(mMouseLocation.X - cur_mouseposition.X);
                            double rect_height = Math.Abs(mMouseLocation.Y - cur_mouseposition.Y);
                            screen_rect = new System.Drawing.Rectangle((int)rect_x1, (int)rect_y1, (int)rect_width, (int)rect_width);
                            tempbmp.DrawRectangle((int)rect_x1, (int)rect_y1, (int)rect_x2, (int)rect_y2, Colors.Black);
                        }
                        Thread.Sleep(10);
                        this.Source = tempbmp;
                    }
                    break;
                case MapOptionStatus.Delete:

                    break;
                case MapOptionStatus.Edit:
                    switch (editmanager.editstatus)
                    {
                        case EditStatus.editing:
                            this.Editing_MouseLocation = e.GetPosition(this);
                            drawtrackingbmp(editmanager, editmanager.rstransform, Editing_MouseLocation);
                            break;
                        case EditStatus.finished:
                            if (e.LeftButton == MouseButtonState.Pressed)
                            {
                                //Point cur_mouseposition = e.GetPosition(this);
                                //Bitmap tempbmp = (Bitmap)this.globalbmp.Clone();
                                //Graphics g = Graphics.FromImage(tempbmp);
                                //Pen pen = Pens.Black;
                                //float rect_x = mMouseLocation.X; if (mMouseLocation.X > cur_mouseposition.X) rect_x = cur_mouseposition.X;
                                //float rect_width = Math.Abs(mMouseLocation.X - cur_mouseposition.X);
                                //float rect_y = mMouseLocation.Y; if (mMouseLocation.Y > cur_mouseposition.Y) rect_y = cur_mouseposition.Y;
                                //float rect_height = Math.Abs(mMouseLocation.Y - cur_mouseposition.Y);
                                //screen_rect = new System.Drawing.Rectangle((int)rect_x, (int)rect_y, (int)rect_width, (int)rect_height);
                                //g.DrawRectangle(pen, screen_rect);
                                //pictureBox1.Image = tempbmp; pictureBox1.Refresh();
                                //g.Dispose();

                                Point cur_mouseposition = e.GetPosition(this);
                                WriteableBitmap tempbmp = globalbmp.Clone();
                                using (tempbmp.GetBitmapContext())
                                {
                                    double rect_x1 = mMouseLocation.X;
                                    double rect_x2 = cur_mouseposition.X;
                                    if (mMouseLocation.X > cur_mouseposition.X)
                                    {
                                        rect_x1 = cur_mouseposition.X;
                                        rect_x2 = mMouseLocation.X;
                                    }
                                    double rect_y1 = mMouseLocation.Y;
                                    double rect_y2 = cur_mouseposition.Y;
                                    if (mMouseLocation.Y > cur_mouseposition.Y)
                                    {
                                        rect_y1 = cur_mouseposition.Y;
                                        rect_y2 = mMouseLocation.Y;
                                    }
                                    double rect_width = Math.Abs(mMouseLocation.X - cur_mouseposition.X);
                                    double rect_height = Math.Abs(mMouseLocation.Y - cur_mouseposition.Y);
                                    screen_rect = new System.Drawing.Rectangle((int)rect_x1, (int)rect_y1, (int)rect_width, (int)rect_width);
                                    tempbmp.DrawRectangle((int)rect_x1, (int)rect_y1, (int)rect_x2, (int)rect_y2, Colors.Black);
                                }
                                this.Source = tempbmp;
                            }
                            break;
                        case EditStatus.selected:
                            if ((editmanager.selectedstatus == selectedstatus.geometrymovable) && (e.LeftButton == MouseButtonState.Pressed))
                            {
                                Point sPreScreenPoint = new Point(mCurMouseLocation.X, mCurMouseLocation.Y);
                                PointD sPrePoint = editmanager.rstransform.targetToSource(ToMapPoint(sPreScreenPoint));
                                Point sCurScreenPoint = new Point(e.GetPosition(this).X, e.GetPosition(this).Y);
                                PointD sCurPoint = editmanager.rstransform.targetToSource(ToMapPoint(sCurScreenPoint));
                                editmanager.SelectedFeaturesPan(sPrePoint.X - sCurPoint.X, sPrePoint.Y - sCurPoint.Y, editmanager.selectedFeatureCollection);
                                mapcontrol_refresh();
                                this.paintadditionalfeaturecollection(editmanager.selectedFeatureCollection);
                                mCurMouseLocation.X = e.GetPosition(this).X;
                                mCurMouseLocation.Y = e.GetPosition(this).Y;
                            }
                            if ((editmanager.selectedstatus == selectedstatus.pointmovable) && (e.LeftButton == MouseButtonState.Pressed))
                            {
                                Point sPreScreenPoint = new Point(mCurMouseLocation.X, mCurMouseLocation.Y);
                                PointD sPrePoint = editmanager.rstransform.targetToSource(ToMapPoint(sPreScreenPoint));
                                Point sCurScreenPoint = new Point(e.GetPosition(this).X, e.GetPosition(this).Y);
                                PointD sCurPoint = editmanager.rstransform.targetToSource(ToMapPoint(sCurScreenPoint));
                                editmanager.selectedPoint.X -= (sPrePoint.X - sCurPoint.X);
                                editmanager.selectedPoint.Y -= (sPrePoint.Y - sCurPoint.Y);
                                mapcontrol_refresh();
                                this.paintadditionalfeaturecollection(editmanager.selectedFeatureCollection);
                                mCurMouseLocation.X = e.GetPosition(this).X;
                                mCurMouseLocation.Y = e.GetPosition(this).Y;
                            }
                            if (CheckIsMouseInFeatureCollection(editmanager.selectedFeatureCollection, e.GetPosition(this)))
                            {
                                editmanager.selectedstatus = selectedstatus.geometrymovable; Cursor = mCur_PanUp;
                                if (editmanager.CheckIsPointSelectedofSelectedFeatureCollection(editmanager.rstransform.targetToSource(ToMapPoint(e.GetPosition(this))), 30))
                                {
                                    editmanager.selectedstatus = selectedstatus.pointmovable; Cursor = Cursors.SizeAll;
                                }

                            }
                            else { editmanager.selectedstatus = selectedstatus.selectable; Cursor = mCur_Cross; }

                            break;
                    }
                    break;
            }
            //RaiseMouseMove_PictureBoxEvent(sender, e);

        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {

        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {

           
            //RSTransform rstransform = new RSTransform_WGS84_WEBMOCARTO();
            switch (MapOption)
            {
                case MapOptionStatus.ZoomIn:
                    if ((screen_rect.Width <= 4 && screen_rect.Height <= 4))
                    {
                        Point upPoint = e.GetPosition(this);
                        PointD sMapPoint = ToMapPoint(upPoint);
                        ZoomByCenter(sMapPoint, mcZoomRation);
                        mapcontrol_refresh();
                    }
                    else
                    {
                        zoominbyrectangle(screen_rect);
                        mapcontrol_refresh();
                    }
                    break;
                case MapOptionStatus.Identify:
                    if (screen_rect.Width <= 4 && screen_rect.Height <= 4)
                    {
                        Point pts = e.GetPosition(this);
                        screen_rect = new System.Drawing.Rectangle((int)pts.X - interval, (int)pts.Y - interval, 2 * interval, 2 * interval);
                    }
                    GisSmartTools.Geometry.Rectangle maprect = ToMapRect(screen_rect);
                    if (mapcontent.layerlist.Count == 0) return;
                    if (focuslayer == null) focuslayer = mapcontent.layerlist[0];
                    //获取坐标转换接口函数
                    RSTransform rstransform = RSTransformFactory.getRSTransform(focuslayer.getReference(), mapcontent.srs);
                    Filter_Envelop filter = new Filter_Envelop(maprect, rstransform);
                    FeatureCollection collection = focuslayer.featuresource.GetFeatures(filter);
                    if (collection.featureList.Count != 0)
                    {
                        paintadditionalfeaturecollection(collection);
                        RaiseAfterSelectedFeaturesEvent(collection);
                    }
                    else
                    {
                        mapcontrol_refresh();
                    }
                    break;
                case MapOptionStatus.Pan:
                    break;
                case MapOptionStatus.Edit:
                    if (e.LeftButton == MouseButtonState.Released)
                    {
                        switch (editmanager.editstatus)
                        {
                            case EditStatus.editing:
                                editmanager.Addpoint(editmanager.rstransform.targetToSource(ToMapPoint(e.GetPosition(this))));
                                break;
                            case EditStatus.finished:
                                if (screen_rect.Width <= 4 && screen_rect.Height <= 4)
                                {
                                    Point pts = e.GetPosition(this);
                                    screen_rect = new System.Drawing.Rectangle((int)pts.X - interval, (int)pts.Y - interval, 2 * interval, 2 * interval);
                                }
                                GisSmartTools.Geometry.Rectangle maprect_select_edit = ToMapRect(screen_rect);
                                Layer tmplayer = this.mapcontent.GetLayerByName(editmanager.layername);
                                if (tmplayer == null)
                                {
                                    this.mapcontrol_refresh(); return;
                                }
                                //获取坐标转换接口函数
                                Filter_Envelop filter_select_edit = new Filter_Envelop(maprect_select_edit, editmanager.rstransform);
                                editmanager.selectedFeatureCollection = tmplayer.featuresource.GetFeatures(filter_select_edit);

                                if (editmanager.selectedFeatureCollection.featureList.Count != 0)
                                {
                                    paintadditionalfeaturecollection(editmanager.selectedFeatureCollection);
                                    editmanager.editstatus = EditStatus.selected;
                                }
                                else
                                {
                                    mapcontrol_refresh();
                                }

                                break;
                        }
                    }
                    break;
                case MapOptionStatus.select:
                    if (screen_rect.Width <= 4 && screen_rect.Height <= 4)
                    {
                        Point pts = e.GetPosition(this);
                        screen_rect = new System.Drawing.Rectangle((int)pts.X - interval, (int)pts.Y - interval, 2 * interval, 2 * interval);
                    }
                    GisSmartTools.Geometry.Rectangle maprect_select = ToMapRect(screen_rect);
                    if (mapcontent.layerlist.Count == 0) return;
                    if (focuslayer == null) focuslayer = mapcontent.layerlist[0];
                    //获取坐标转换接口函数
                    RSTransform rstransform1 = RSTransformFactory.getRSTransform(focuslayer.getReference(), mapcontent.srs);
                    Filter_Envelop filter_select = new Filter_Envelop(maprect_select, rstransform1);
                    FeatureCollection collection_select = focuslayer.featuresource.GetFeatures(filter_select);
                    if (collection_select.featureList.Count != 0)
                    {
                        paintadditionalfeaturecollection(collection_select);

                    }
                    else
                    {
                        mapcontrol_refresh();
                    }
                    break;
            }


        }
        /// <summary>
        /// 滚轮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if(MapOption == MapOptionStatus.ZoomIn||MapOption == MapOptionStatus.ZoomOut)
            if (true)
            {
                if (e.Delta > 0)
                {
                    Point upPoint = e.GetPosition(this);
                    PointD sMapPoint = ToMapPoint(upPoint);
                    ZoomByCenter(sMapPoint, mcZoomRation);
                    mapcontrol_refresh();
                }
                else
                {
                    Point sMouseLocation = e.GetPosition(this);
                    PointD sMapPoint = ToMapPoint(sMouseLocation);
                    ZoomByCenter(sMapPoint, 1 / mcZoomRation);
                    mapcontrol_refresh();
                }
            }

        }
        #endregion


        #region 外部可调用方法接口区
        /// <summary>
        /// 该函数用于重新刷新一遍mapcontrol中的mapcontent，所有图层将会重新渲染一遍，形成bitmap
        /// </summary>
        public void mapcontrol_refresh()
        {
            //Bitmap bmp = new Bitmap(bitmap_X, bitmap_Y);
            //Bitmap temp_textbmp = new Bitmap(bitmap_X, bitmap_Y);
            //Graphics g = Graphics.FromImage(bmp);
            //Graphics text_g = Graphics.FromImage(temp_textbmp);
            ////SetDefaultoffsetandDisplayScale(mapcontent);
            //paintmap(g, text_g, this.mapcontent);
            //text_g.Dispose();
            //g.DrawImage(temp_textbmp, 0, 0);
            //g.Dispose();
            //globalbmp = bmp;
            //pictureBox1.Image = globalbmp;
            //pictureBox1.Refresh();

            globalbmp.Clear();
            bmp = BitmapFactory.New(bitmap_X, bitmap_Y);
            temp_textbmp = BitmapFactory.New(bitmap_X, bitmap_Y);
            paintmap(this.mapcontent);
            Rect rect = new Rect(0, 0, bitmap_X, bitmap_Y);
            using (bmp.GetBitmapContext())
            {
                bmp.Blit(rect, temp_textbmp, rect);
                globalbmp.Blit(rect, bmp, rect);
            }
            //globalbmp = bmp;
            this.Source = globalbmp;
            //this.InvalidateVisual();
        }


        public double GetDisplayScale()
        {
            return DisplayScale;
        }
        /// <summary>
        /// 该函数赋予显示参数的初值，同时可以用于显示全图的功能
        /// </summary>
        /// <param name="mapcontent"></param>
        public void SetDefaultoffsetandDisplayScale(mapcontent mapcontent)
        {
            Geometry.Rectangle rect = mapcontent.GetRefernectRectangle();
            //Geometry.Rectangle rect = TransformRect(map_rect);
            if (rect.minX >= rect.maxX || rect.minY >= rect.maxY)
            {
                DisplayScale = 1;
                moffsetX = 0;
                moffsetY = 0;
                return;
            }
            double scale_x = (rect.maxX - rect.minX) / bitmap_X;
            double scale_y = (rect.maxY - rect.minY) / bitmap_Y;
            if (scale_x < scale_y) DisplayScale = scale_y;
            else DisplayScale = scale_x;
            moffsetX = rect.minX;
            moffsetY = rect.maxY;
        }

        //private Geometry.Rectangle TransformRect(Geometry.Rectangle map_rect)
        //{
        //    //PointD minxy = rstransfrom.sourceToTarget(new PointD(map_rect.minX, map_rect.minY));
        //    //PointD maxxy = rstransfrom.sourceToTarget(new PointD(map_rect.maxX, map_rect.maxY));
        //    //return new Geometry.Rectangle(minxy.X, minxy.Y, maxxy.X, maxxy.Y);
        //}

        public void ZoomIn()
        {

            MapOption = MapOptionStatus.ZoomIn;
            this.Cursor = mCur_ZoomIn;
        }
        public void ZoomOut()
        {
            MapOption = MapOptionStatus.ZoomOut;
            this.Cursor = mCur_ZoomOut;
        }
        public void Pan()
        {
            MapOption = MapOptionStatus.Pan;
            this.Cursor = mCur_PanUp;
            //this.Cursor = mCur_PanUp;
        }
        public void Identify()
        {
            MapOption = MapOptionStatus.Identify;
            this.Cursor = Cursors.Cross;
            //this.Cursor = mCur_Cross;
        }

        public void ReturnToNone()
        {
            MapOption = MapOptionStatus.none;
            this.Cursor = Cursors.Arrow;
        }

        public void Query_attribute(Layer layer, Filter.Filter filter)
        {
            FeatureCollection collection = layer.featuresource.GetFeatures(filter);
            paintadditionalfeaturecollection(collection);
            RaiseQueryFeaturesEvent(collection);
        }


        public void StartEdit(Layer layer)
        {
            if (IsEditing)
            {
                MapOption = MapOptionStatus.Edit;
                this.Cursor = mCur_Cross;
            }
            else if (layer != null)
            {
                MapOption = MapOptionStatus.Edit;
                this.Cursor = mCur_Cross;
                RSTransform transform = RSTransformFactory.getRSTransform(layer.getReference(), mapcontent.srs);
                //RSTransform transform = new RSTransform_WGS84_WEBMOCARTO();//????????????????????????????坐标系问题
                editmanager = new EditingManager(layer, transform);
                IsEditing = true;
            }
        }
        public void SelectFeatures()
        {
            MapOption = MapOptionStatus.select;
            this.Cursor = Cursors.Cross;
        }
        public void EndEdit()
        {
            MapOption = MapOptionStatus.Pan;
            this.Cursor = mCur_PanUp;
            IsEditing = false;
            Utils.gislog.ClearLog();
        }

        public void OperationUndo()
        {
            Utils.gislog.Recover(this.mapcontent);
            mapcontrol_refresh();
        }
        public void OperationRedo()
        {
            Utils.gislog.Forward(this.mapcontent);
            mapcontrol_refresh();
        }
        public void Edit_Copy_Feature()
        {
            if (MapOption == MapOptionStatus.Edit && editmanager.editstatus == EditStatus.selected)
            {
                editmanager.copycollection = editmanager.selectedFeatureCollection;

            }
        }
        public void Edit_Paste_Feature()
        {
            if (MapOption == MapOptionStatus.Edit)
            {
                editmanager.PasteFeatureCollection();
                mapcontrol_refresh();
            }
        }
        public void DeleteSelectedFeatures()
        {
            if (MapOption == MapOptionStatus.Edit && editmanager.editstatus == EditStatus.selected)
            {
                editmanager.DeleteSelectedFeatures();
                mapcontrol_refresh();
            }
        }
        /// <summary>
        /// 编辑状态下，正处于输入图形时删除上次输入的点
        /// </summary>
        public void Edit_DeleteLastInputPoint()
        {
            if (MapOption == MapOptionStatus.Edit && editmanager.editstatus == EditStatus.editing)
            {
                editmanager.deletelastinputPoint();
                drawtrackingbmp(editmanager, editmanager.rstransform, Editing_MouseLocation);
            }
        }
        public void Edit_ResumeCurrentInput()
        {
            if (MapOption == MapOptionStatus.Edit && editmanager.editstatus == EditStatus.editing)
            {
                editmanager.ResumeCurrentInput();
                drawtrackingbmp(editmanager, editmanager.rstransform, Editing_MouseLocation);
            }
        }

        public void Edit_ResumeAllInput()
        {
            if (MapOption == MapOptionStatus.Edit && editmanager.editstatus == EditStatus.editing)
            {
                editmanager.ResumeAllInput();
                drawtrackingbmp(editmanager, editmanager.rstransform, Editing_MouseLocation);
            }
        }
        public void setFocuslayer(String name)
        {
            foreach (Layer ly in this.mapcontent.layerlist)
            {
                if (ly.Layername.Equals(name)) this.focuslayer = ly;
            }

        }

        public MapOptionStatus mapOpt
        {
            get { return this.MapOption; }
        }




        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void StartEdit_Click(object sender, RoutedEventArgs e)
        {
            editmanager.startedit();
            Cursor = mCur_Cross;
        }

        private void FinishEdit_Click(object sender, RoutedEventArgs e)
        {
            editmanager.FinishEdit();
            Cursor = mCur_Cross;
            mapcontrol_refresh();
        }
        private void RestartEdit_Click(object sender, RoutedEventArgs e)
        {
            this.Edit_ResumeAllInput();
        }
        private void FinishPart_Click(object sender, RoutedEventArgs e)
        {
            bool sign = editmanager.finishpart();
            if (!sign) MessageBox.Show("您输入的点数不足,请继续输入");
        }

        #endregion

        private void Image_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            bitmap_Y = (int)e.NewSize.Height;
            bitmap_X = (int)e.NewSize.Width;
            globalbmp = BitmapFactory.New(bitmap_X, bitmap_Y);
            mapcontrol_refresh();
        }
    }
}
