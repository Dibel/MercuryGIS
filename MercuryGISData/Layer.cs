using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Globalization;

namespace MercuryGISData
{

    public enum LayerType {Point, Line, Polygon, Text };
    public abstract class Layer
    {
        private int id;
        protected string name;
        protected string filename;
        protected double minX;
        protected double minY;
        protected double maxX;
        protected double maxY;
        protected bool isVisible = true;
        protected int elementCount = 0;
        public PropertyDataSet dataset;
        protected LayerType type;
        protected double ratio = 1;
        protected double offsetX = 0;
        protected double offsetY = 0;
        protected double height = 0;
        protected string labelField;
        protected bool isLabelShown = false;
        protected TextSymbol labelSymbol = new TextSymbol();

        public bool IsLabelShown
        {
            get { return isLabelShown; }
            set { isLabelShown = value; }
        }

        public string LabelField
        {
            get { return labelField; }
            set { labelField = value; }
        }

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public LayerType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public string Filename
        {
            get
            {
                return filename;
            }

            set
            {
                filename = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public bool IsVisible
        {
            get
            {
                return isVisible;
            }

            set
            {
                isVisible = value;
            }
        }

        public double MinX
        {
            get
            {
                return minX;
            }

            set
            {
                minX = value;
            }
        }

        public double MinY
        {
            get
            {
                return minY;
            }

            set
            {
                minY = value;
            }
        }

        public double MaxX
        {
            get
            {
                return maxX;
            }

            set
            {
                maxX = value;
            }
        }

        public double MaxY
        {
            get
            {
                return maxY;
            }

            set
            {
                maxY = value;
            }
        }

        public abstract void Add(object element);
        public abstract void Remove(int id);

        public abstract void FindMinMax();

        public abstract void OpenLayer(string filename);
        public abstract void SaveLayer(string filename);
        public abstract void Render(DrawingContext dc, double ratio, double offsetX, double offsetY, double height);
        public List<int> QueryByAttri(string query)
        {
            return dataset.QueryByAttri(query);
        }
        public abstract List<int> QueryByLocat(Rectangle rect);
        public Rectangle GetEnvelope()
        {
            MPoint p1 = new MPoint(0, minX, minY);
            MPoint p2 = new MPoint(0, maxX, maxY);
            return new Rectangle(p1, p2);
        }

        public int GenerateUniqueID()
        {
            return elementCount + 1;
        }

        public abstract void Select(List<int> ids);

        public Point ToScrPoint(MPoint mapPoint)
        {
            double x = mapPoint.X / ratio - offsetX;
            double y = this.height - (mapPoint.Y / ratio + offsetY);
            return new Point(x, y);
        }

        public void CreateDataSet()
        {
            dataset.CreateDataset(filename + ".db");
        }

        public List<string> GetFields()
        {
            List<string> result = new List<string>();
            foreach (System.Data.DataColumn col in dataset.table.Columns)
            {
                result.Add(col.ColumnName);
            }
            return result;
        }

        public string GetDataSetValue(int id, string field)
        {
            return dataset.table.Rows[id - 1][field].ToString();
        }

        public List<string> GetAllValues(string field)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < dataset.table.Rows.Count; i++)
            {
                result.Add(dataset.table.Rows[i][field].ToString());
            }
            return result;
        }

        public System.Data.DataRow GetDataRow(int id)
        {
            return dataset.table.Rows[id - 1];
        }

        public abstract void ClearSelection();

        public abstract Symbol GetSymbol();
        public abstract List<Symbol> GetAllSymbols();

        public abstract void SetSymbol(List<int> ids, Symbol symbol);
    };



    public class PointLayer : Layer
    {
        public List<MPoint> elements;

        public PointLayer()
        {
            elements = new List<MPoint>();
            dataset = new PropertyDataSet();
            type = LayerType.Point;
        }

        public override void Add(object element)
        {
            MPoint point = (MPoint)element;
            point.Id = GenerateUniqueID();
            elements.Add(point);
            ElementCount++;
        }

        public override void Remove(int id)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Id == id)
                {
                    elements.RemoveAt(i);
                }
            }
            ElementCount--;
        }

        public List<MPoint> GetElements(List<int> ids)
        {
            List<MPoint> result = new List<MPoint>();
            for (int i = 0; i < elements.Count; i++)
            {
                for (int j = 0; j < ids.Count; j++)
                {
                    if (elements[i].Id == ids[i])
                    {
                        result.Add(elements[i]);
                    }
                }
            }
            return result;
        }
        
        public override void OpenLayer(string filename)
        {
            string json = File.ReadAllText(filename + ".json");
            elements = JsonConvert.DeserializeObject<List<MPoint>>(json);
            ElementCount = elements.Count;
            this.filename = filename;
            dataset.Open(filename + ".db");
        }

        public override List<int> QueryByLocat(Rectangle rect)
        {
            List<int> ids = new List<int>();
            for (int i = 0; i < ElementCount; i++)
            {
                if (elements[i].X < rect.p2.X && elements[i].X > rect.p1.X
                    && elements[i].Y > rect.p2.Y && elements[i].Y < rect.p1.Y)
                {
                    ids.Add(elements[i].Id);
                }
            }
            return ids;
        }


        public override void SaveLayer(string filename)
        {
            this.filename = filename;
            string output = JsonConvert.SerializeObject(elements);
            File.WriteAllText(filename + ".json", output);
            dataset.Save(filename + ".db");
        }

        public override void Select(List<int> ids)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                for (int j = 0; j < ids.Count; j++)
                {
                    if (elements[i].Id == ids[j])
                    {
                        elements[i].IsSelected = true;
                    }
                }
            }
        }

        public override void Render(DrawingContext dc, double ratio, double offsetX, double offsetY, double height)
        {
            this.ratio = ratio;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.height = height;
            for (int i = 0; i < ElementCount; i++)
            {
                if (elements[i].IsSelected)
                {
                    dc.DrawEllipse(elements[i].symbol.brush, new Pen(Brushes.Cyan, 5), ToScrPoint(elements[i]),
                        elements[i].symbol.size + 3, elements[i].symbol.size + 3);
                }
                else
                {
                    dc.DrawEllipse(elements[i].symbol.brush, elements[i].symbol.pen, ToScrPoint(elements[i]),
                        elements[i].symbol.size, elements[i].symbol.size);
                }
                if (isLabelShown)
                {
                    FormattedText text = new FormattedText(GetDataSetValue(elements[i].Id, labelField),
                        CultureInfo.GetCultureInfo("zh-CN"),
                        FlowDirection.LeftToRight,
                        labelSymbol.typeface,
                        labelSymbol.size,
                        labelSymbol.brush);
                    dc.DrawText(text, ToScrPoint(elements[i]));
                }
            }
        }

        public override void ClearSelection()
        {
            for (int i = 0; i < ElementCount; i++)
            {
                elements[i].IsSelected = false;
            }
        }

        public override void FindMinMax()
        {
            minX = maxX = elements[0].X;
            minY = maxY = elements[0].Y;
            for (int i = 0; i < ElementCount; i++)
            {
                if (elements[i].X < minX)
                {
                    minX = elements[i].X;
                }
                if (elements[i].X > maxX)
                {
                    maxX = elements[i].X;
                }
                if (elements[i].Y < minY)
                {
                    minY = elements[i].Y;
                }
                if (elements[i].Y > maxY)
                {
                    maxY = elements[i].Y;
                }
            }
        }

        public override Symbol GetSymbol()
        {
            return elements[0].symbol;
        }

        public override List<Symbol> GetAllSymbols()
        {
            List<Symbol> list = new List<Symbol>();
            for (int i = 0; i < elements.Count; i++)
            {
                if (list.Contains(elements[i].symbol))
                {
                    continue;
                }
                list.Add(elements[i].symbol);
            }
            return list;
        }

        public override void SetSymbol(List<int> ids, Symbol symbol)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (ids == null)
                {
                    return;
                }
                for (int j = 0; j < ids.Count; j++)
                {
                    if (elements[i].Id == ids[j])
                    {
                        elements[i].symbol.brush = symbol.brush;
                        elements[i].symbol.pen = symbol.pen;
                        elements[i].symbol.size = symbol.size;
                    }
                }
            }
        }

        public int ElementCount
        {
            get
            {
                elementCount = elements.Count;
                return elements.Count;
            }

            set
            {
                elementCount = value;
            }
        }
    }

    public class LineLayer : Layer
    {

        public List<Line> elements;

        public LineLayer()
        {
            elements = new List<Line>();
            dataset = new PropertyDataSet();
            type = LayerType.Line;
        }

        public override void Add(object element)
        {
            Line line = (Line)element;
            line.Id = GenerateUniqueID();
            elements.Add(line);
            ElementCount++;
        }

        public override void ClearSelection()
        {
            for (int i = 0; i < ElementCount; i++)
            {
                elements[i].IsSelected = false;
            }
        }

        public override void OpenLayer(string filename)
        {
            string json = File.ReadAllText(filename + ".json");
            elements = JsonConvert.DeserializeObject<List<Line>>(json);
            ElementCount = elements.Count;
            dataset.Open(filename + ".db");
        }

        public override List<int> QueryByLocat(Rectangle rect)
        {
            List<int> ids = new List<int>();
            for (int i = 0; i < ElementCount; i++)
            {
                for (int j = 0; j < elements[i].LineCount; j++)
                {

                    for (int k = 0; k < elements[i].List[j].PointCount; k++)
                    {
                        MPoint p = elements[i].List[j].List[k];
                        if (p.X >= rect.p1.X && p.X <= rect.p2.X && p.Y <= rect.p1.Y && p.Y >= rect.p2.Y)
                        {
                            ids.Add(elements[i].Id);
                            break;
                        }
                    }
                    if (ids.Count != 0 && ids.Last() == elements[i].Id)
                    {
                        break;
                    }

                }

            }
            return ids;
        }

        public override void Remove(int id)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Id == id)
                {
                    elements.RemoveAt(i);
                }
            }
            ElementCount--;
        }

        public override void Render(DrawingContext dc, double ratio, double offsetX, double offsetY, double height)
        {
            this.ratio = ratio;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.height = height;
            for (int i = 0; i < ElementCount; i++)
            {
                for (int j = 0; j < elements[i].LineCount; j++)
                {
                    for (int k = 0; k < elements[i].List[j].PointCount - 1; k++)
                    {
                        if (elements[i].IsSelected)
                        {
                            dc.DrawLine(new Pen(Brushes.Cyan, 5), ToScrPoint(elements[i].List[j].List[k]), ToScrPoint(elements[i].List[j].List[k + 1]));
                        }
                        dc.DrawLine(elements[i].symbol.pen, ToScrPoint(elements[i].List[j].List[k]), ToScrPoint(elements[i].List[j].List[k + 1]));
                    }
                }
                if (isLabelShown)
                {
                    FormattedText text = new FormattedText(GetDataSetValue(elements[i].Id, labelField),
                        CultureInfo.GetCultureInfo("zh-CN"),
                        FlowDirection.LeftToRight,
                        labelSymbol.typeface,
                        labelSymbol.size,
                        labelSymbol.brush);
                    dc.DrawText(text, ToScrPoint(elements[i].List[0].List[0]));
                }
            }
        }

        public override void SaveLayer(string filename)
        {
            string output = JsonConvert.SerializeObject(elements);
            File.WriteAllText(filename + ".json", output);
            dataset.Save(filename + ".db");
        }

        public override void Select(List<int> ids)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                for (int j = 0; j < ids.Count; j++)
                {
                    if (elements[i].Id == ids[j])
                    {
                        elements[i].IsSelected = true;
                    }
                }
            }
        }

        public override void FindMinMax()
        {
            minX = maxX = elements[0].MinX;
            minY = maxY = elements[0].MinY;
            for (int i = 0; i < ElementCount; i++)
            {
                if (elements[i].MinX < minX)
                {
                    minX = elements[i].MinX;
                }
                if (elements[i].MaxX > maxX)
                {
                    maxX = elements[i].MaxX;
                }
                if (elements[i].MinY < minY)
                {
                    minY = elements[i].MinY;
                }
                if (elements[i].MaxY > maxY)
                {
                    maxY = elements[i].MaxY;
                }
            }
        }

        public override Symbol GetSymbol()
        {
            return elements[0].symbol;
        }

        public override List<Symbol> GetAllSymbols()
        {
            List<Symbol> list = new List<Symbol>();
            for (int i = 0; i < elements.Count; i++)
            {
                if (list.Contains(elements[i].symbol))
                {
                    continue;
                }
                list.Add(elements[i].symbol);
            }
            return list;
        }

        public override void SetSymbol(List<int> ids, Symbol symbol)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (ids == null)
                {
                    return;
                }
                for (int j = 0; j < ids.Count; j++)
                {
                    if (elements[i].Id == ids[j])
                    {

                        elements[i].symbol.pen = symbol.pen;
                        elements[i].symbol.size = symbol.size;
                    }
                }
            }
        }

        public int ElementCount
        {
            get
            {
                elementCount = elements.Count;
                return elements.Count;
            }

            set
            {
                elementCount = value;
            }
        }
    }

    public class PolygonLayer : Layer
    {

        public List<Polygon> elements;

        public PolygonLayer()
        {
            elements = new List<Polygon>();
            dataset = new PropertyDataSet();
            type = LayerType.Polygon;
        }
        public override void Add(object element)
        {
            Polygon polygon = (Polygon)element;
            polygon.Id = GenerateUniqueID();
            elements.Add(polygon);
            ElementCount++;
        }

        public override void ClearSelection()
        {
            for (int i = 0; i < ElementCount; i++)
            {
                elements[i].IsSelected = false;
            }
        }

        public override void OpenLayer(string filename)
        {
            string json = File.ReadAllText(filename + ".json");
            elements = JsonConvert.DeserializeObject<List<Polygon>>(json);
            ElementCount = elements.Count;
            dataset.Open(filename + ".db");
        }

        public override List<int> QueryByLocat(Rectangle rect)
        {
            List<int> ids = new List<int>();
            for (int i = 0; i < ElementCount; i++)
            {
                for (int j = 0; j < elements[i].PolygonCount; j++)
                {

                    for (int k = 0; k < elements[i].List[j].PointCount; k++)
                    {
                        MPoint p = elements[i].List[j].List[k];
                        if (p.X >= rect.p1.X && p.X <= rect.p2.X && p.Y <= rect.p1.Y && p.Y >= rect.p2.Y)
                        {
                            ids.Add(elements[i].Id);
                            break;
                        }
                    }
                    if (ids.Count != 0 && ids.Last() == elements[i].Id)
                    {
                        break;
                    }
                }

            }
            return ids;
        }

        public override void Remove(int id)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Id == id)
                {
                    elements.RemoveAt(i);
                }
            }
            ElementCount--;
        }

        public override void Render(DrawingContext dc, double ratio, double offsetX, double offsetY, double height)
        {
            this.ratio = ratio;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.height = height;
            for (int i = 0; i < ElementCount; i++)
            {
                GeometryGroup group = new GeometryGroup();
                StreamGeometry streamGeometry;
                for (int j = 0; j < elements[i].PolygonCount; j++)
                {
                    streamGeometry = new StreamGeometry();
                    using (StreamGeometryContext geometryContext = streamGeometry.Open())
                    {
                        geometryContext.BeginFigure(ToScrPoint(elements[i].List[j].List[0]), true, true);
                        PointCollection points = new PointCollection();
                        for (int k = 1; k < elements[i].List[j].PointCount; k++)
                        {
                            Point scrPoint = ToScrPoint(elements[i].List[j].List[k]);
                            geometryContext.LineTo(scrPoint, true, true);
                            //points.Add(scrPoint);
                        }
                        //geometryContext.PolyLineTo(points, true, true);
                    }
                    streamGeometry.Freeze();
                    group.Children.Add(streamGeometry);
                }
                if (elements[i].IsSelected)
                {
                    dc.DrawGeometry(elements[i].symbol.brush,
                        new Pen(Brushes.Cyan, elements[i].symbol.pen.Thickness + 2), group);
                }
                dc.DrawGeometry(elements[i].symbol.brush, elements[i].symbol.pen, group);
                if (isLabelShown)
                {
                    FormattedText text = new FormattedText(GetDataSetValue(elements[i].Id, labelField),
                        CultureInfo.GetCultureInfo("zh-CN"),
                        FlowDirection.LeftToRight,
                        labelSymbol.typeface,
                        labelSymbol.size,
                        labelSymbol.brush);
                    dc.DrawText(text, ToScrPoint(elements[i].List[0].List[0]));
                }
            }
        }

        public override void SaveLayer(string filename)
        {
            string output = JsonConvert.SerializeObject(elements);
            File.WriteAllText(filename + ".json", output);
            dataset.Save(filename + ".db");
        }

        public override void Select(List<int> ids)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                for (int j = 0; j < ids.Count; j++)
                {
                    if (elements[i].Id == ids[j])
                    {
                        elements[i].IsSelected = true;
                    }
                }
            }
        }

        public override void FindMinMax()
        {
            minX = maxX = elements[0].MinX;
            minY = maxY = elements[0].MinY;
            for (int i = 0; i < ElementCount; i++)
            {
                if (elements[i].MinX < minX)
                {
                    minX = elements[i].MinX;
                }
                if (elements[i].MaxX > maxX)
                {
                    maxX = elements[i].MaxX;
                }
                if (elements[i].MinY < minY)
                {
                    minY = elements[i].MinY;
                }
                if (elements[i].MaxY > maxY)
                {
                    maxY = elements[i].MaxY;
                }
            }
        }

        public override Symbol GetSymbol()
        {
            return elements[0].symbol;
        }

        public override List<Symbol> GetAllSymbols()
        {
            List<Symbol> list = new List<Symbol>();
            for (int i = 0; i < elements.Count; i++)
            {
                if (list.Contains(elements[i].symbol))
                {
                    continue;
                }
                list.Add(elements[i].symbol);
            }
            return list;
        }

        public override void SetSymbol(List<int> ids, Symbol symbol)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (ids == null)
                {
                    return;
                }
                for (int j = 0; j < ids.Count; j++)
                {
                    if (elements[i].Id == ids[j])
                    {
                        elements[i].symbol = (PolygonSymbol)symbol;
                    }
                }
            }
        }

        public int ElementCount
        {
            get
            {
                elementCount = elements.Count;
                return elements.Count;
            }

            set
            {
                elementCount = value;
            }
        }
    }
}
