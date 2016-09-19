using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercuryGISData
{
    public class Line
    {
        private int id;
        private double minX;
        private double minY;
        private double maxX;
        private double maxY;
        private int pointCount = 0;
        private int lineCount = 0;
        private List<PList> _list;
        private bool isSelected = false;
        public LineSymbol symbol = new LineSymbol();


        public Line(int id)
        {
            this.id = id;
            this._list = new List<PList>();

        }
        public Line(int id, List<PList> list)
        {
            this.id = id;
            this._list = new List<PList>(list);
            lineCount = _list.Count;
            if (lineCount != 0)
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

        public void AddLine(PList line)
        {
            _list.Add(line);
            lineCount++;
            pointCount += line.PointCount;
        }

        public void InsertLine(int n, PList line)
        {
            if (n < lineCount)
            {
                _list.Insert(n, line);
                lineCount++;
                pointCount += line.PointCount;
            }
        }

        public void DeleteLine(int n)
        {
            if (n < lineCount)
            {
                pointCount -= _list[n].PointCount;
                _list.RemoveAt(n);
                lineCount--;
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
            if (i < lineCount)
            {
                if (n < _list[i].PointCount)
                {
                    _list[i].AddPoint(n, input);
                }
            }
        }

        public void EditPoint(int i, int n, MPoint input)
        {
            if (i < lineCount)
            {
                if (n < _list[i].PointCount)
                {
                    _list[i].EditPList(n, input);
                }
            } 
        }

        public void DeletePoint(int i, int n)
        {
            if (i < lineCount)
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

        public int LineCount
        {
            get
            {
                return lineCount;
            }

            set
            {
                lineCount = value;
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
