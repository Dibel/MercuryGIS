using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MercuryGISData
{
    public abstract class Symbol
    {
        public Brush brush;
        public Pen pen;
        public double size;
        public abstract Drawing Render();
    }

    public class PointSymbol : Symbol
    {
        public PointSymbol()
        {
            brush = Brushes.Blue;
            pen = null;
            size = 2;
        }
        public override Drawing Render()
        {
            throw new NotImplementedException();
        }
    }

    public class LineSymbol : Symbol
    {
        public LineSymbol()
        {
            size = 2;
            brush = null;
            pen = new Pen(Brushes.Black, 2);
        }
        public override Drawing Render()
        {
            throw new NotImplementedException();
        }
    }

    public class PolygonSymbol : Symbol
    {
        public PolygonSymbol()
        {
            size = 2;
            brush = Brushes.DarkGreen;
            pen = new Pen(Brushes.DarkGoldenrod, 1);
        }
        public override Drawing Render()
        {
            throw new NotImplementedException();
        }
    }

    public class TextSymbol : Symbol
    {
        public Typeface typeface;
        public TextSymbol()
        {
            size = 10;
            typeface = new Typeface("Verdana");
            brush = Brushes.Black;
        }
        public override Drawing Render()
        {
            throw new NotImplementedException();
        }
    }
}
