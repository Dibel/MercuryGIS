using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercuryGISData
{
    public class MultiPoint
    {
        private int id;
        private double minX;
        private double minY;
        private double maxX;
        private double maxY;
        private int pointCount = 0;
        private List<MPoint> _list;
        private bool isSelected = false;

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

        public List<MPoint> List
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

        public MultiPoint(int id, List<MPoint> list)
        {
            this.id = id;
            _list = new List<MPoint>(list);
            pointCount = _list.Count;
            if (pointCount != 0)
            {
                minX = maxX = _list[0].X;
                minY = maxY = _list[0].Y;
                foreach (var p in _list)
                {
                    if (p.X > maxX)
                    {
                        maxX = p.X;
                    }
                    if (p.X < minX)
                    {
                        minX = p.X;
                    }
                    if (p.Y > maxY)
                    {
                        maxY = p.Y;
                    }
                    if (p.Y < minY)
                    {
                        minY = p.Y;
                    }
                }
            }
        }

        public void AddPoint(MPoint point)
        {
            _list.Add(point);
            if (point.X > maxX)
            {
                maxX = point.X;
            }
            if (point.X < minX)
            {
                minX = point.X;
            }
            if (point.Y > maxY)
            {
                maxY = point.Y;
            }
            if (point.Y < minY)
            {
                minY = point.Y;
            }
            pointCount++;
        }

        public void EditPoint(int n, MPoint point)
        {
            if (n < pointCount)
            {
                _list[n].X = point.X;
                _list[n].Y = point.Y;
            }
            if (point.X > maxX)
            {
                maxX = point.X;
            }
            if (point.X < minX)
            {
                minX = point.X;
            }
            if (point.Y > maxY)
            {
                maxY = point.Y;
            }
            if (point.Y < minY)
            {
                minY = point.Y;
            }
        }

        public void DeletePoint(int n)
        {
            if (n < pointCount)
            {
                _list.RemoveAt(n);
            }
            foreach (var p in _list)
            {
                if (p.X > maxX)
                {
                    maxX = p.X;
                }
                if (p.X < minX)
                {
                    minX = p.X;
                }
                if (p.Y > maxY)
                {
                    maxY = p.Y;
                }
                if (p.Y < minY)
                {
                    minY = p.Y;
                }
            }
            pointCount--;
        }

        public Rectangle GetEnvelope()
        {
            MPoint p1 = new MPoint(0, minX, minY);
            MPoint p2 = new MPoint(0, maxX, maxY);
            return new Rectangle(p1, p2);
        }
    }
}
