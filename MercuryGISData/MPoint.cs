using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercuryGISData
{
    public class MPoint
    {
        private int id;
        private double x;
        private double y;
        private bool isSelected = false;
        public PointSymbol symbol = new PointSymbol(); 


        public MPoint(int id, double x, double y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
        }

        public void NewPoint(int id, double x, double y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
        }

        public void CopyPoint(MPoint inputPoint)
        {
            this.id = inputPoint.Id;
            this.x = inputPoint.X;
            this.y = inputPoint.Y;
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

        public double X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
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
