using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GisSmartTools.Data;
using GisSmartTools.Filter;
using GisSmartTools.Geometry;
using GisSmartTools.RS;
using GisSmartTools.Support;
using GisSmartTools.Topology;

namespace GisSmartTools
{
    public enum MapOptionStatus
    {
        ZoomIn,
        ZoomOut,
        Pan,
        select,
        Identify,
        Edit,
        Delete,
        none,
    }

    
    public partial class MapControl : UserControl
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
        //鼠标状态
        private PointF mCurMouseLocation = new PointF();//用于地图漫游时记录鼠标当前位置

        private PointF mMouseLocation = new PointF(0, 0);//用于记录刚刚按下的鼠标位置，之后用于获取屏幕矩形
        private System.Drawing.Rectangle screen_rect;

        private PointF Editing_MouseLocation = new PointF();
        private Cursor mCur_Cross = new Cursor(typeof(GisSmartTools.MapControl), "Resource.Cross.ico");
        //private Cursor nCur_Cross = new Cursor(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("MyMapObjects.Resources.Cross.ico"));
        private Cursor mCur_ZoomIn = new Cursor(typeof(GisSmartTools.MapControl), "Resource.ZoomIn.ico");
        private Cursor mCur_ZoomOut = new Cursor(typeof(GisSmartTools.MapControl), "Resource.ZoomOut.ico");
        private Cursor mCur_PanUp = new Cursor(typeof(GisSmartTools.MapControl), "Resource.PanUp.ico");
        
        //private Cursor mCur_edit_select = new Cursor(typeof(GisSmartTools.MapControl), "Resource.edit_select.ico");
        //画选中的feature的预定义颜色
        //private Color selectedFillColor = Color.LightSkyBlue;
        //private Color selectedStrokeColor = Color.Blue;

        //状态信息
        private MapOptionStatus MapOption = MapOptionStatus.none;
        private bool IsEditing = false;
        //有关编辑的数据信息
        public EditingManager editmanager;
        
        //数据
        public mapcontent mapcontent;
        
        public Layer focuslayer = null;//当前正在编辑或正处于被选择的图层，默认为mapcontent 的0号图层
        public Bitmap globalbmp = new Bitmap(500, 500);
        public Bitmap textbmp = new Bitmap(500, 500);
        #endregion
        #region 自定义事件区
        //代理（事件）定义去
        //框选点选查询结果事件处理
        public delegate void AfterSelectedFeatures(FeatureCollection collenction);
        public event AfterSelectedFeatures AfterSelectedFeaturesEvent;
        private void RaiseAfterSelectedFeaturesEvent(FeatureCollection collection)
        {
            if(AfterSelectedFeaturesEvent!=null)
            {
                AfterSelectedFeaturesEvent(collection);
            }
        }
        //查询结果处理事件
        public delegate void AfterQueryFeatures(FeatureCollection collenction);
        public event AfterQueryFeatures AfterQueryFeaturesEvent;
        private void RaiseQueryFeaturesEvent(FeatureCollection collection)
        {
            if(AfterQueryFeaturesEvent!=null)
            {
                AfterQueryFeaturesEvent(collection);
            }
        }
        //鼠标在picturebox上移动附加事件
        public delegate void MouseMove_OnPictureBox(object sender, MouseEventArgs e);
        public event MouseMove_OnPictureBox mousemoveEvent;
        private void RaiseMouseMove_PictureBoxEvent(object sender, MouseEventArgs e)
        {
            if(mousemoveEvent!=null)
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
        public MapControl()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(MapControl_MouseWheel);
            //pictureBox1.MouseWheel += new MouseEventHandler(MapControl_MouseWheel);
            bitmap_X = pictureBox1.Width;
            bitmap_Y = pictureBox1.Height;
        }


        #region 实际渲染代码
        private void paintmap(Graphics g,Graphics text_g, mapcontent mapcontent)
        {
            if (mapcontent == null) return;
            List<Layer> layerlist = mapcontent.layerlist;
            for (int i = layerlist.Count - 1; i >= 0; i--)
            {

                Layer layer = layerlist[i];
                if (layer.visible) 
                {
                    //获取targetSRS,sourceSRS,然后获取RSTransform对象///////????????????????????????????????????????????????
                    RSTransform rstransform = new RSTransform_WGS84_WEBMOCARTO();
                    paintLayerbyStyle(g,text_g, layer, rstransform);
                }
               
            }
        }
        private void paintLayerbyStyle(Graphics g, Graphics text_g, Layer layer, RSTransform rstransform)
        {
            Style style = layer.style;
            FeatureSource featuresource = layer.featuresource;
            foreach (Feature feature in featuresource.features.featureList)
            {
                if (!feature.visible) continue;
                switch (feature.geometry.geometryType)
                {
                    case OSGeo.OGR.wkbGeometryType.wkbPoint:
                        paintPointFeaturebyRule(g,text_g, feature, SelectRendererRule(feature, style), rstransform);
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbLineString:
                        paintLineFeaturebyRule(g,text_g, feature, SelectRendererRule(feature, style), rstransform);
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                        paintpolygonFeaturebyRule(g,text_g, feature, SelectRendererRule(feature, style), rstransform);
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
                        paintMultiPolygonbyRule(g,text_g, feature, SelectRendererRule(feature, style), rstransform);
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
        private void paintPointFeaturebyRule(Graphics g, Graphics text_g, Feature feature, RenderRule rule, RSTransform rstransform)
        {
            
            
        }

        /// <summary>
        /// 画多边形类
        /// </summary>
        /// <param name="g"></param>
        /// <param name="feature"></param>
        /// <param name="rule"></param>
        /// <param name="rstransform"></param>
        private void paintpolygonFeaturebyRule(Graphics g, Graphics text_g, Feature feature, RenderRule rule, RSTransform rstransform)
        {
            

        }

        private void paintMultiPolygonbyRule(Graphics g, Graphics text_g, Feature feature, RenderRule rule, RSTransform rstransform)
        {
            

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="feature"></param>
        /// <param name="rule"></param>
        /// <param name="rstransform"></param>
        private void paintLineFeaturebyRule(Graphics g, Graphics text_g, Feature feature, RenderRule rule, RSTransform rstransform)
        {
            
        }

        private void paintMutiLinebyRule(Graphics g, Graphics text_g, Feature feature, RenderRule rule, RSTransform rstransform)
        {
           
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
        public Bitmap paintadditionalfeaturecollection(FeatureCollection collection)
        {
            //Bitmap tempbmp = paintadditionalfeaturecollection(collection, this.selectedFillColor, this.selectedStrokeColor);
            //if (tempbmp != null)
            //{
            //    pictureBox1.Image = tempbmp;
            //    pictureBox1.Refresh();
            //}
            //return tempbmp;
            return new Bitmap(1, 1);
        }

        public Bitmap paintadditionalfeaturecollection(FeatureCollection collection, Color FillColor, Color StrokeColor)
        {
            if (collection == null || collection.featureList.Count == 0) return null;
            Bitmap tempbmp = (Bitmap)globalbmp.Clone();
            return paintadditionalfeaturecollection(collection, tempbmp, FillColor, StrokeColor);
        }

        //双缓冲技术，在图片上渲染featurecollection，使用预定义的Rule和自定义的color
        /// <summary>
        /// 本函数外部可调用
        /// </summary>
        /// <param name="collection"></param>
        public Bitmap paintadditionalfeaturecollection(FeatureCollection collection, Bitmap board, Color FillColor, Color StrokeColor)
        {
            
            return board;
        }

        /// <summary>
        /// 画正在编辑的要素
        /// </summary>
        /// <param name="g"></param>
        /// <param name="transfrom"></param>
        public void drawtrackingbmp(EditingManager editmanager, RS.RSTransform transfrom,PointF mousemovelocation)
        {
            

            

        }



        #endregion

        #region 工具函数

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="mouselocation"></param>
        /// <returns></returns>
        private bool CheckIsMouseInFeatureCollection(FeatureCollection collection,Point mouselocation)
        {
            screen_rect = new System.Drawing.Rectangle(mouselocation.X-2,mouselocation.Y-2,4,4);
            GisSmartTools.Geometry.Rectangle maprect_select = ToMapRect(screen_rect);
            Filter_Envelop filter_edit_selected = new Filter_Envelop(maprect_select, editmanager.rstransform);
            for(int i=0;i<collection.featureList.Count;i++)
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
            this.bitmap_X = pictureBox1.Width;
            this.bitmap_Y = pictureBox1.Height;
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
        private RenderRule SelectRendererRule(Feature feature, Style style)
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
        public PointF FromMapPoint(PointD point)
        {
            PointF sPoint = new PointF();
            sPoint.X = (float)((point.X - moffsetX) / DisplayScale);
            sPoint.Y = (float)((moffsetY - point.Y) / DisplayScale);
            return sPoint;
        }

        /// <summary>
        /// 将屏幕坐标转换为地图坐标
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public PointD ToMapPoint(PointD point)
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
            double maxx = (screen_rect.X+screen_rect.Width)*DisplayScale+moffsetX;
            double miny = moffsetY-(screen_rect.Y+screen_rect.Height)*DisplayScale;
            return new Geometry.Rectangle(minx, miny, maxx, maxy);
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
                    if (e.Button == MouseButtons.Left)
                    {
                        mMouseLocation = e.Location;
                        screen_rect = new System.Drawing.Rectangle(e.X - interval, e.Y - interval, 2*interval, 2*interval);
                    }
                    break;
                case MapOptionStatus.ZoomOut:
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        PointD sMouseLocation = new PointD(e.Location.X, e.Location.Y);
                        PointD sMapPoint = ToMapPoint(sMouseLocation);
                        ZoomByCenter(sMapPoint, 1 / mcZoomRation);
                        mapcontrol_refresh();
                    }
                    break;
                case MapOptionStatus.Pan:
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        mCurMouseLocation = e.Location;
                    }
                    break;
                case MapOptionStatus.Identify:
                    if (e.Button == MouseButtons.Left)
                    {
                        mMouseLocation = e.Location;
                        screen_rect = new System.Drawing.Rectangle(e.X - interval, e.Y - interval, 2*interval, 2*interval);
                    }
                    break;
                case MapOptionStatus.Delete:
                    if (e.Button == MouseButtons.Left)
                    {
                        mMouseLocation = e.Location;
                        screen_rect = new System.Drawing.Rectangle(e.X - interval, e.Y - interval, 2*interval, 2*interval);
                    }
                    break;
                case MapOptionStatus.select:
                    if (e.Button == MouseButtons.Left)
                    {
                        mMouseLocation = e.Location;
                        screen_rect = new System.Drawing.Rectangle(e.X - interval, e.Y - interval, 2*interval, 2*interval);
                    }
                    break;
                case MapOptionStatus.Edit:
                    if(e.Button== System.Windows.Forms.MouseButtons.Right)
                    {
                        pictureBox1.ContextMenuStrip = picturebox_menu;
                        picturebox_menu.Show(e.Location);
                    }
                    if(e.Button== System.Windows.Forms.MouseButtons.Left)
                    {
                        switch(editmanager.editstatus)
                        {
                            case EditStatus.editing:
                                break;
                            case EditStatus.finished:
                                 mMouseLocation = e.Location;
                                 screen_rect = new System.Drawing.Rectangle(e.X - interval, e.Y - interval, 2*interval, 2*interval);
                                break;
                            case EditStatus.selected:
                                mCurMouseLocation = e.Location;
                                mMouseLocation = e.Location;
                                screen_rect = new System.Drawing.Rectangle(e.X - interval, e.Y - interval, 2*interval, 2*interval);
                                if(editmanager.selectedstatus== selectedstatus.selectable)
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
            switch(MapOption)
            {
                case MapOptionStatus.ZoomIn:
                    if(e.Button==MouseButtons.Left)
                    {
                        PointF cur_mouseposition = e.Location;
                        Bitmap tempbmp = (Bitmap)this.globalbmp.Clone();
                        Graphics g = Graphics.FromImage(tempbmp);
                        Pen pen = Pens.Black;
                        float rect_x = mMouseLocation.X;if(mMouseLocation.X>cur_mouseposition.X) rect_x=cur_mouseposition.X;
                        float rect_width = Math.Abs(mMouseLocation.X-cur_mouseposition.X);
                        float rect_y = mMouseLocation.Y; if(mMouseLocation.Y>cur_mouseposition.Y) rect_y = cur_mouseposition.Y;
                        float rect_height = Math.Abs(mMouseLocation.Y-cur_mouseposition.Y);
                        screen_rect = new System.Drawing.Rectangle((int)rect_x, (int)rect_y, (int)rect_width, (int)rect_height);
                        g.DrawRectangle(pen,screen_rect);
                        pictureBox1.Image = tempbmp; pictureBox1.Refresh();
                        g.Dispose();
                        

                    }
                    break;
                case MapOptionStatus.Pan:
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        PointD sPreScreenPoint = new PointD(mCurMouseLocation.X, mCurMouseLocation.Y);
                        PointD sPrePoint = ToMapPoint(sPreScreenPoint);
                        PointD sCurScreenPoint = new PointD(e.Location.X, e.Location.Y);
                        PointD sCurPoint = ToMapPoint(sCurScreenPoint);
                        moffsetX += sPrePoint.X - sCurPoint.X;
                        moffsetY += sPrePoint.Y - sCurPoint.Y;
                        mapcontrol_refresh();
                        mCurMouseLocation.X = e.Location.X;
                        mCurMouseLocation.Y = e.Location.Y;
                    }
                    break;
                case MapOptionStatus.Identify:
                    if (e.Button == MouseButtons.Left)
                    {
                        PointF cur_mouseposition = e.Location;
                        Bitmap tempbmp = (Bitmap)this.globalbmp.Clone();
                        Graphics g = Graphics.FromImage(tempbmp);
                        Pen pen = Pens.Black;
                        float rect_x = mMouseLocation.X; if (mMouseLocation.X > cur_mouseposition.X) rect_x = cur_mouseposition.X;
                        float rect_width = Math.Abs(mMouseLocation.X - cur_mouseposition.X);
                        float rect_y = mMouseLocation.Y; if (mMouseLocation.Y > cur_mouseposition.Y) rect_y = cur_mouseposition.Y;
                        float rect_height = Math.Abs(mMouseLocation.Y - cur_mouseposition.Y);
                        screen_rect = new System.Drawing.Rectangle((int)rect_x, (int)rect_y, (int)rect_width, (int)rect_height);
                        g.DrawRectangle(pen, screen_rect);
                        pictureBox1.Image = tempbmp; pictureBox1.Refresh();
                        g.Dispose();


                    }
                    break;
                case MapOptionStatus.select:
                    if (e.Button == MouseButtons.Left)
                    {
                        PointF cur_mouseposition = e.Location;
                        Bitmap tempbmp = (Bitmap)this.globalbmp.Clone();
                        Graphics g = Graphics.FromImage(tempbmp);
                        Pen pen = Pens.Black;
                        float rect_x = mMouseLocation.X; if (mMouseLocation.X > cur_mouseposition.X) rect_x = cur_mouseposition.X;
                        float rect_width = Math.Abs(mMouseLocation.X - cur_mouseposition.X);
                        float rect_y = mMouseLocation.Y; if (mMouseLocation.Y > cur_mouseposition.Y) rect_y = cur_mouseposition.Y;
                        float rect_height = Math.Abs(mMouseLocation.Y - cur_mouseposition.Y);
                        screen_rect = new System.Drawing.Rectangle((int)rect_x, (int)rect_y, (int)rect_width, (int)rect_height);
                        g.DrawRectangle(pen, screen_rect);
                        pictureBox1.Image = tempbmp; pictureBox1.Refresh();
                        g.Dispose();


                    }
                    break;
                case MapOptionStatus.Delete:
                    
                    break;
                case MapOptionStatus.Edit:
                    switch(editmanager.editstatus)
                    {
                        case EditStatus.editing:
                            this.Editing_MouseLocation = e.Location;
                            drawtrackingbmp(editmanager, editmanager.rstransform,Editing_MouseLocation);
                            break;
                        case EditStatus.finished:
                            if (e.Button == MouseButtons.Left)
                            {
                                PointF cur_mouseposition = e.Location;
                                Bitmap tempbmp = (Bitmap)this.globalbmp.Clone();
                                Graphics g = Graphics.FromImage(tempbmp);
                                Pen pen = Pens.Black;
                                float rect_x = mMouseLocation.X; if (mMouseLocation.X > cur_mouseposition.X) rect_x = cur_mouseposition.X;
                                float rect_width = Math.Abs(mMouseLocation.X - cur_mouseposition.X);
                                float rect_y = mMouseLocation.Y; if (mMouseLocation.Y > cur_mouseposition.Y) rect_y = cur_mouseposition.Y;
                                float rect_height = Math.Abs(mMouseLocation.Y - cur_mouseposition.Y);
                                screen_rect = new System.Drawing.Rectangle((int)rect_x, (int)rect_y, (int)rect_width, (int)rect_height);
                                g.DrawRectangle(pen, screen_rect);
                                pictureBox1.Image = tempbmp; pictureBox1.Refresh();
                                g.Dispose();
                            }
                            break;
                        case EditStatus.selected:
                            if ((editmanager.selectedstatus==selectedstatus.geometrymovable)&&(e.Button == System.Windows.Forms.MouseButtons.Left))
                            {
                                PointD sPreScreenPoint = new PointD(mCurMouseLocation.X, mCurMouseLocation.Y);
                                PointD sPrePoint = editmanager.rstransform.targetToSource(ToMapPoint(sPreScreenPoint));
                                PointD sCurScreenPoint = new PointD(e.Location.X, e.Location.Y);
                                PointD sCurPoint = editmanager.rstransform.targetToSource(ToMapPoint(sCurScreenPoint));
                                editmanager.SelectedFeaturesPan(sPrePoint.X - sCurPoint.X, sPrePoint.Y - sCurPoint.Y,editmanager.selectedFeatureCollection);
                                mapcontrol_refresh();
                                this.paintadditionalfeaturecollection(editmanager.selectedFeatureCollection);
                                mCurMouseLocation.X = e.Location.X;
                                mCurMouseLocation.Y = e.Location.Y;
                            }
                            if((editmanager.selectedstatus== selectedstatus.pointmovable)&&(e.Button== System.Windows.Forms.MouseButtons.Left))
                            {
                                PointD sPreScreenPoint = new PointD(mCurMouseLocation.X, mCurMouseLocation.Y);
                                PointD sPrePoint = editmanager.rstransform.targetToSource(ToMapPoint(sPreScreenPoint));
                                PointD sCurScreenPoint = new PointD(e.Location.X, e.Location.Y);
                                PointD sCurPoint = editmanager.rstransform.targetToSource(ToMapPoint(sCurScreenPoint));
                                editmanager.selectedPoint.X -= (sPrePoint.X - sCurPoint.X);
                                editmanager.selectedPoint.Y -= (sPrePoint.Y - sCurPoint.Y);
                                mapcontrol_refresh();
                                this.paintadditionalfeaturecollection(editmanager.selectedFeatureCollection);
                                mCurMouseLocation.X = e.Location.X;
                                mCurMouseLocation.Y = e.Location.Y;
                            }
                            if (CheckIsMouseInFeatureCollection(editmanager.selectedFeatureCollection, e.Location))
                            {
                                editmanager.selectedstatus = selectedstatus.geometrymovable; Cursor = mCur_PanUp;
                                if(editmanager.CheckIsPointSelectedofSelectedFeatureCollection(editmanager.rstransform.targetToSource(ToMapPoint(new PointD(e.X,e.Y))),30))
                                {
                                    editmanager.selectedstatus = selectedstatus.pointmovable; Cursor = Cursors.SizeAll;
                                }
                                
                            }
                            else { editmanager.selectedstatus= selectedstatus.selectable; Cursor = mCur_Cross; }
                            
                            break;
                    }
                    break;
            }
            RaiseMouseMove_PictureBoxEvent(sender, e);

        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {

            RSTransform rstransform = new RSTransform_WGS84_WEBMOCARTO();
            switch(MapOption)
            {
                case MapOptionStatus.ZoomIn:
                    if((screen_rect.Width<=4&&screen_rect.Height<=4))
                    {
                        PointD upPoint = new PointD(e.X, e.Y);
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
                        screen_rect = new System.Drawing.Rectangle(e.X - interval, e.Y - interval, 2*interval, 2*interval);
                    }
                    GisSmartTools.Geometry.Rectangle maprect = ToMapRect(screen_rect);
                    if (mapcontent.layerlist.Count == 0) return;
                    if (focuslayer == null) focuslayer = mapcontent.layerlist[0];
                    //获取坐标转换接口函数
                    Filter_Envelop filter = new Filter_Envelop(maprect, rstransform);
                    FeatureCollection collection = focuslayer.featuresource.GetFeatures(filter);
                    if(collection.featureList.Count!=0)
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
                    if(e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        switch(editmanager.editstatus)
                        {
                            case EditStatus.editing:
                                editmanager.Addpoint(editmanager.rstransform.targetToSource(ToMapPoint(new PointD(e.X, e.Y))));
                                break;
                            case EditStatus.finished:
                                if (screen_rect.Width <= 4 && screen_rect.Height <= 4)
                                {
                                    screen_rect = new System.Drawing.Rectangle(e.X - interval, e.Y - interval, 2*interval, 2*interval);
                                }
                                GisSmartTools.Geometry.Rectangle maprect_select_edit = ToMapRect(screen_rect);
                                Layer tmplayer = this.mapcontent.GetLayerByName(editmanager.layername);
                                if(tmplayer==null) 
                                {
                                    this.mapcontrol_refresh();return;
                                }
                                //获取坐标转换接口函数
                                Filter_Envelop filter_select_edit = new Filter_Envelop(maprect_select_edit, rstransform);
                                editmanager.selectedFeatureCollection = tmplayer.featuresource.GetFeatures(filter_select_edit);
                                
                                if(editmanager.selectedFeatureCollection.featureList.Count!=0)
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
                        screen_rect = new System.Drawing.Rectangle(e.X - interval, e.Y - interval, 2*interval, 2*interval);
                    }
                    GisSmartTools.Geometry.Rectangle maprect_select = ToMapRect(screen_rect);
                    if (mapcontent.layerlist.Count == 0) return;
                    if (focuslayer == null) focuslayer = mapcontent.layerlist[0];
                    //获取坐标转换接口函数
                    Filter_Envelop filter_select = new Filter_Envelop(maprect_select, rstransform);
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
        private void MapControl_MouseWheel(object sender, MouseEventArgs e)
        {
            //if(MapOption == MapOptionStatus.ZoomIn||MapOption == MapOptionStatus.ZoomOut)
            if (true)
            {
                if (e.Delta > 0)
                {
                    PointD upPoint = new PointD(e.X, e.Y);
                    PointD sMapPoint = ToMapPoint(upPoint);
                    ZoomByCenter(sMapPoint, mcZoomRation);
                    mapcontrol_refresh();
                }
                else
                {
                    PointD sMouseLocation = new PointD(e.Location.X, e.Location.Y);
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
            Bitmap bmp = new Bitmap(bitmap_X, bitmap_Y);
            Bitmap temp_textbmp = new Bitmap(bitmap_X, bitmap_Y);
            Graphics g = Graphics.FromImage(bmp);
            Graphics text_g = Graphics.FromImage(temp_textbmp);
            //SetDefaultoffsetandDisplayScale(mapcontent);
            paintmap(g,text_g,this.mapcontent);
            text_g.Dispose();
            g.DrawImage(temp_textbmp, 0, 0);
            g.Dispose();
            globalbmp = bmp;
            pictureBox1.Image = globalbmp;
            pictureBox1.Refresh();
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
            if(rect.minX>=rect.maxX||rect.minY>=rect.maxY)
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
        }
        public void Identify()
        {
            MapOption = MapOptionStatus.Identify;
            this.Cursor = mCur_Cross;
        }

        public void Query_attribute(Layer layer , Filter.Filter filter)
        {
            FeatureCollection collection = layer.featuresource.GetFeatures(filter);
            paintadditionalfeaturecollection(collection);
            RaiseQueryFeaturesEvent(collection);
        }
       

        public void StartEdit(Layer layer)
        {
            if(IsEditing)
            {
                MapOption = MapOptionStatus.Edit;
                this.Cursor = mCur_Cross;
            }
            else if(layer!=null)
            {
                MapOption = MapOptionStatus.Edit;
                this.Cursor = mCur_Cross;
                RSTransform transform = new RSTransform_WGS84_WEBMOCARTO();//????????????????????????????坐标系问题
                editmanager = new EditingManager(layer, transform);
                IsEditing = true;
            }
        }
        public void SelectFeatures()
        {
            MapOption = MapOptionStatus.select;
            this.Cursor = mCur_Cross;
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
            if(MapOption== MapOptionStatus.Edit)
            {
                editmanager.PasteFeatureCollection();
                mapcontrol_refresh();
            }
        }
        public void DeleteSelectedFeatures()
        {
            if(MapOption== MapOptionStatus.Edit&& editmanager.editstatus == EditStatus.selected)
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
            if(MapOption== MapOptionStatus.Edit&& editmanager.editstatus== EditStatus.editing)
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

        private void 开始输入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editmanager.startedit();
            Cursor = mCur_Cross;
        }

        private void 完成输入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editmanager.FinishEdit();
            Cursor = mCur_Cross;
            mapcontrol_refresh();
        }
        private void 全部重新输入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Edit_ResumeAllInput();
        }
        private void 完成部分ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool sign = editmanager.finishpart();
            if (!sign) MessageBox.Show("您输入的点数不足,请继续输入");
        }

        #endregion

       






    }
}
