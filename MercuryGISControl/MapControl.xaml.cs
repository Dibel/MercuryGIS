using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MercuryGISData;

namespace MercuryGISControl
{
    public enum Mode { Edit, Pan, ZoomIn, ZoomOut, None, Select, EditAdd };
    /// <summary>
    /// MapControl.xaml 的交互逻辑
    /// </summary>
    ///
    public partial class MapControl : Canvas
    {
        //Essential Variables
        private Mode mapMode = Mode.None;
        private Map map = new Map();
        private double ratio = 1.0;
        private double offsetX = 0;
        private double offsetY = 0;
        private Point preMouseLocation;
        private bool isSelecting = false;
        private Rect selectingRect;
        public Rectangle queryRect;
        private Layer curLayer;
        private Line tempLine = null;
        private Polygon tempPolygon = null;
        private PList tempPart = null;
        private List<List<Point>> trackingLine = new List<List<Point>>();
        private Point trackingPoint;
        

        public Mode MapMode
        {
            get
            {
                return mapMode;
            }

            set
            {
                mapMode = value;
            }
        }



        public double Ratio
        {
            get
            {
                return ratio;
            }

            set
            {
                ratio = value;
            }
        }

        public Map Map
        {
            get
            {
                return map;
            }

            set
            {
                map = value;
            }
        }

        public Layer CurLayer
        {
            get
            {
                return curLayer;
            }

            set
            {
                curLayer = value;
            }
        }

        public MapControl()
        {
            InitializeComponent();
            Background = Brushes.White;
        }

        public void ScaleToLayer(Layer layer)
        {
            double r1 = (layer.MaxX - layer.MinX) / Width;
            double r2 = (layer.MaxY - layer.MinY) / Height;

            double newRatio;
            if (r1 > r2)
            {
                newRatio = r1;
            }
            else
            {
                newRatio = r2;
            }
            ratio = newRatio;
            offsetX = layer.MinX / ratio;
            offsetY = Height - (layer.MaxY / ratio);
            Refresh();
        }

        public MPoint ToMapPoint(Point scrPoint)
        {
            double x = (scrPoint.X + offsetX) * ratio;
            double y = (this.Height - scrPoint.Y - offsetY) * ratio;
            return new MPoint(0 ,x, y);
        }

        public Point ToScrPoint(MPoint mapPoint)
        {
            double x = mapPoint.X / ratio - offsetX;
            double y = this.Height - (mapPoint.Y / ratio + offsetY);
            return new Point(x, y);
        }

        public void ZoomByCenter(Point center, double changed)
        {
            double tempRatio = ratio / changed;
            offsetX = offsetX - (1 - changed) * (center.X + offsetX);
            offsetY = offsetY - (1 - 1 / changed) * (this.Height - center.Y - offsetY);
            ratio = tempRatio;
        }


        public void Refresh()
        {
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            for (int i = 0; i < map.LayerCount; i++)
            {
                Layer renderLayer = map.GetLayer(i);
                if (renderLayer.IsVisible)
                {
                    renderLayer.Render(dc, ratio, offsetX, offsetY, this.Height);
                }
            }

            if (trackingLine.Count != 0)
            {
                Pen pen = new Pen(Brushes.Gray, 0.5);
                switch (curLayer.Type)
                {
                    case LayerType.Point:
                        break;
                    case LayerType.Line:
                        foreach (var plist in trackingLine)
                        {
                            for (int i = 0; i < plist.Count - 1; i++)
                            {
                                dc.DrawLine(pen, plist[i], plist[i + 1]);
                            }
                        }
                        dc.DrawLine(pen, trackingLine.Last().Last(), trackingPoint);
                        break;
                    case LayerType.Polygon:
                        for (int i = 0; i < trackingLine.Count; i++)
                        {
                            for (int j = 0; j < trackingLine[i].Count - 1; j++)
                            {
                                dc.DrawLine(pen, trackingLine[i][j], trackingLine[i][j + 1]);
                            }
                            if (i != trackingLine.Count - 1)
                            {
                                dc.DrawLine(pen, trackingLine[i].Last(), trackingLine[i].First());
                            }
                            else
                            {
                                dc.DrawLine(pen, trackingLine[i].Last(), trackingPoint);
                                dc.DrawLine(pen, trackingPoint, trackingLine[i].First());
                            }
                        }
                        break;
                    case LayerType.Text:
                        break;
                    default:
                        break;
                }
            }

            if (isSelecting)
            {
                DrawTrackingPolygon(dc);
            }
        }

        private void onMouseDown(object sender, MouseButtonEventArgs e)
        {
            //for (int i = 0; i < map.LayerCount; i++)
            //{
            //    map.GetLayer(i).ClearSelection();
            //}
            //Refresh();
            switch (mapMode)
            {
                case Mode.Edit:
                    break;
                case Mode.Pan:
                    preMouseLocation =  e.GetPosition(this);
                    break;
                case Mode.ZoomIn:
                    ZoomByCenter(e.GetPosition(this), 1.25);
                    Refresh();
                    break;
                case Mode.ZoomOut:
                    ZoomByCenter(e.GetPosition(this), 1 / 1.25);
                    Refresh();
                    break;
                case Mode.None:
                    break;
                case Mode.Select:
                    for (int i = 0; i < map.LayerCount; i++)
                    {
                        map.GetLayer(i).ClearSelection();
                    }
                    Refresh();
                    preMouseLocation = e.GetPosition(this);
                    break;
                case Mode.EditAdd:

                    Point curLocation = e.GetPosition(this);
                    //Double click to end the input process
                    if (e.ClickCount == 2)
                    {
                        switch (curLayer.Type)
                        {
                            case LayerType.Point:
                                break;
                            case LayerType.Line:
                                if (tempLine != null)
                                {
                                    tempLine.AddLine(tempPart);
                                    curLayer.Add(tempLine);
                                    tempPart = null;
                                    tempLine = null;
                                    trackingLine.Clear();
                                    Refresh();
                                }
                                break;
                            case LayerType.Polygon:
                                if (tempPolygon != null)
                                {
                                    tempPolygon.AddPolygon(tempPart);
                                    curLayer.Add(tempPolygon);
                                    tempPart = null;
                                    tempPolygon = null;
                                    trackingLine.Clear();
                                    Refresh();
                                }
                                break;
                            case LayerType.Text:
                                break;
                            default:
                                break;
                        }
                    }
                    else if(e.ClickCount == 1)
                    {
                        trackingPoint = curLocation;
                        switch (curLayer.Type)
                        {
                            case LayerType.Point:
                                curLayer.Add(ToMapPoint(curLocation));
                                Refresh();
                                break;
                            case LayerType.Line:
                                if (tempLine == null)
                                {
                                    tempLine = new Line(0);
                                    tempPart = new PList(0);
                                    tempPart.AddPoint(ToMapPoint(curLocation));
                                    trackingLine.Add(new List<Point>());
                                    trackingLine.Last().Add(curLocation);
                                }
                                else
                                {
                                    if (tempPart == null)
                                    {
                                        tempPart = new PList(0);
                                        tempPart.AddPoint(ToMapPoint(curLocation));
                                        trackingLine.Add(new List<Point>());
                                        trackingLine.Last().Add(curLocation);
                                    }
                                    else
                                    {
                                        tempPart.AddPoint(ToMapPoint(curLocation));
                                        trackingLine.Last().Add(curLocation);
                                    }
                                }
                                Refresh();
                                break;
                            case LayerType.Polygon:
                                if (tempPolygon == null)
                                {
                                    tempPolygon = new Polygon(0);
                                    tempPart = new PList(0);
                                    tempPart.AddPoint(ToMapPoint(curLocation));
                                    trackingLine.Add(new List<Point>());
                                    trackingLine.Last().Add(curLocation);
                                }
                                else
                                {
                                    if (tempPart == null)
                                    {
                                        tempPart = new PList(0);
                                        tempPart.AddPoint(ToMapPoint(curLocation));
                                        trackingLine.Add(new List<Point>());
                                        trackingLine.Last().Add(curLocation);
                                    }
                                    else
                                    {
                                        tempPart.AddPoint(ToMapPoint(curLocation));
                                        trackingLine.Last().Add(curLocation);
                                    }
                                }
                                Refresh();
                                break;
                            case LayerType.Text:
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            switch (MapMode)
            {
                case Mode.Edit:
                    break;
                case Mode.Pan:
                    break;
                case Mode.ZoomIn:
                    break;
                case Mode.ZoomOut:
                    break;
                case Mode.None:
                    break;
                case Mode.Select:
                    break;
                case Mode.EditAdd:
                    if (e.ClickCount == 1)
                    {
                        Point curLocation = e.GetPosition(this);
                        switch (curLayer.Type)
                        {
                            case LayerType.Point:
                                break;
                            case LayerType.Line:
                                if (tempPart != null)
                                {
                                    tempPart.AddPoint(ToMapPoint(curLocation));
                                    tempLine.AddLine(tempPart);
                                    tempPart = null;
                                    trackingLine.Last().Add(curLocation);
                                    Refresh();
                                }
                                break;
                            case LayerType.Polygon:
                                if (tempPart != null)
                                {
                                    tempPart.AddPoint(ToMapPoint(curLocation));
                                    tempPolygon.AddPolygon(tempPart);
                                    tempPart = null;
                                    trackingLine.Last().Add(curLocation);
                                    Refresh();
                                }
                                break;
                            case LayerType.Text:
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
            
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            switch (mapMode)
            {
                case Mode.Edit:
                    break;
                case Mode.Pan:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Point curLocation = e.GetPosition(this);
                        offsetX += preMouseLocation.X - curLocation.X;
                        offsetY += preMouseLocation.Y - curLocation.Y;
                        preMouseLocation = curLocation;
                        Refresh();
                        //Moving
                    }
                    break;
                case Mode.ZoomIn:
                    break;
                case Mode.ZoomOut:
                    break;
                case Mode.None:
                    break;
                case Mode.Select:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {

                        Point curLocation = e.GetPosition(this);
                        //DrawTrackingPolygon(preMouseLocation, curLocation);
                        selectingRect = new Rect(preMouseLocation, curLocation);

                        isSelecting = true;
                        Refresh();
                        //preMouseLocation = curLocation;
                    }
                    break;
                case Mode.EditAdd:
                    if (tempPart != null)
                    {
                        trackingPoint = e.GetPosition(this);
                        Refresh();
                    }
                    break;
                default:
                    break;
            }
        }

        private void DrawTrackingPolygon(DrawingContext dc)
        {
            dc.DrawRectangle(null, new Pen(Brushes.Gray, 2), selectingRect);   
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (mapMode)
            {
                case Mode.Edit:
                    break;
                case Mode.Pan:
                    break;
                case Mode.ZoomIn:
                    break;
                case Mode.ZoomOut:
                    break;
                case Mode.None:
                    break;
                case Mode.Select:
                    isSelecting = false;
                    queryRect = new Rectangle(
                        ToMapPoint(selectingRect.TopLeft), ToMapPoint(selectingRect.BottomRight));
                    for (int i = 0; i < map.LayerCount; i++)
                    {
                        Layer layer = map.GetLayer(i);
                        layer.Select(layer.QueryByLocat(queryRect));
                    }
                    Refresh();
                    break;
                default:
                    break;
            }
        }

    }
}
