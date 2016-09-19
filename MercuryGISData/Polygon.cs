using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercuryGISData
{
    public class Polygon
    {
        //TODO: Max and Min values need to be updated
        private int id;
        private double minX;
        private double minY;
        private double maxX;
        private double maxY;
        private int pointCount = 0;
        private int polygonCount = 0;
        private List<PList> _list;
        private bool isSelected = false;
        public PolygonSymbol symbol = new PolygonSymbol();

        public Polygon(int id)
        {
            this.id = id;
            pointCount = 0;
            this._list = new List<PList>();
        }

        public Polygon(int id, List<PList> list)
        {
            this.id = id;
            this._list = new List<PList>(list);
            polygonCount = _list.Count;
            if (polygonCount != 0)
            {
                minX = maxX = _list[0].MinX;
                minY = maxY = _list[0].MinY;
            }
            pointCount = 0;

            foreach (var plist in _list)
            {
                pointCount += plist.PointCount;
                if (plist.MinX < minX)
                {
                    minX = plist.MinX;
                }
                if (plist.MinY < minY)
                {
                    minY = plist.MinY;
                }
                if (plist.MaxX > maxX)
                {
                    maxX = plist.MaxX;
                }
                if (plist.MaxY > maxY)
                {
                    maxY = plist.MaxY;
                }
            }
        }

        public void AddPolygon(PList line)
        {
            _list.Add(line);
            polygonCount++;
            pointCount += line.PointCount;
        }

        public void InsertPolygon(int n, PList line)
        {
            if (n < polygonCount)
            {
                _list.Insert(n, line);
                polygonCount++;
                pointCount += line.PointCount;
            }
        }

        public void DeletePolygon(int n)
        {
            if (n < polygonCount)
            {
                pointCount -= _list[n].PointCount;
                _list.RemoveAt(n);
                polygonCount--;
                
            }
        }


        public Rectangle GetEnvelope()
        {
            MPoint p1 = new MPoint(0, minX, minY);
            MPoint p2 = new MPoint(0, maxX, maxY);
            return new Rectangle(p1, p2);
        }

        public void AddPoint(int i, int n, MPoint input)
        {
            if (i < polygonCount)
            {
                if (n < _list[i].PointCount)
                {
                    _list[i].AddPoint(n, input);
                }
            }
        }

        public void EditPoint(int i, int n, MPoint input)
        {
            if (i < polygonCount)
            {
                if (n < _list[i].PointCount)
                {
                    _list[i].EditPList(n, input);
                }
            }
        }

        public void DeletePoint(int i, int n)
        {
            if (i < polygonCount)
            {
                if (n < _list[i].PointCount)
                {
                    _list[i].DeletePoint(n);
                }
            }
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

        public int PointCount
        {
            get
            {
                return pointCount;
            }

            set
            {
                pointCount = value;
            }
        }

        public int PolygonCount
        {
            get
            {
                return polygonCount;
            }

            set
            {
                polygonCount = value;
            }
        }

        internal List<PList> List
        {
            get
            {
                return _list;
            }

            set
            {
                _list = value;
            }
        }

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }

            set
            {
                isSelected = value;
            }
        }
    }
}
