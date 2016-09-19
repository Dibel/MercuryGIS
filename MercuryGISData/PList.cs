using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercuryGISData
{
    public class PList
    {
        private int id;
        private double minX;
        private double minY;
        private double maxX;
        private double maxY;
        private int pointCount = 0;
        private List<MPoint> _list;

        public PList(int id)
        {
            this.id = id;
            _list = new List<MPoint>();
            minX = minY = 10000000000;
            maxX = maxY = -10000000000;
        }

        public PList(int id, List<MPoint> list)
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

        public void CopyPList(PList inputlist)
        {
            this.id = inputlist.Id;
            this.minX = inputlist.MinX;
            this.minY = inputlist.minY;
            this.maxX = inputlist.MaxX;
            this.maxY = inputlist.MaxY;
            this.pointCount = inputlist.PointCount;
            this._list = new List<MPoint>(inputlist.List);
        }

        public PList Clone(int id)
        {
            PList result = new PList(id, _list);
            return result;
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

        /// <summary>
        /// 在n之后插入点
        /// </summary>
        /// <param name="n"></param>
        /// <param name="point"></param>
        public void AddPoint(int n, MPoint point)
        {
            if (n < pointCount)
            {
                _list.Insert(n, point);
            }
        }

        public void EditPList(int n, double x, double y)
        {
            if (n < pointCount)
            {
                _list[n].X = x;
                _list[n].Y = y;
            }
            if (x > maxX)
            {
                maxX = x;
            }
            if (x < minX)
            {
                minX = x;
            }
            if (y > maxY)
            {
                maxY = y;
            }
            if (y < minY)
            {
                minY = y;
            }
        }

        public void EditPList(int n, MPoint point)
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

        public int Id
        {
            get
            {
                return id;
            }
        }

        public double MinX
        {
            get
            {
                return minX;
            }
        }

        public double MinY
        {
            get
            {
                return minY;
            }
        }

        public double MaxX
        {
            get
            {
                return maxX;
            }
        }

        public double MaxY
        {
            get
            {
                return maxY;
            }
        }

        public int PointCount
        {
            get
            {
                return pointCount;
            }
        }

        public List<MPoint> List
        {
            get
            {
                return _list;
            }
        }
    }
}
