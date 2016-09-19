using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using System.Windows.Shapes;
using MercuryGISData;

namespace MercuryGIS
{
    /// <summary>
    /// Interaction logic for Symbolize.xaml
    /// </summary>
    public partial class Symbolize : Window
    {

        private MercuryGISControl.MapControl map;
        private MercuryGISData.Layer curLayer;
        private string curField;
        private System.Data.DataTable dataTable;
        public ObservableCollection<Color> Items
        {
            get { return (ObservableCollection<Color>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<Color>), typeof(string));

        public ObservableCollection<ColorModel> list = new ObservableCollection<ColorModel>();

        public Symbolize()
        {
            InitializeComponent();
        }

        public Symbolize(MercuryGISControl.MapControl map)
        {
            InitializeComponent();
            this.map = map;
            layerList.ItemsSource = map.Map.GetLayers();
            layerList.DisplayMemberPath = "Name";
            dataGrid.ItemsSource = list;
        }

        private void singleRadio_Click(object sender, RoutedEventArgs e)
        {
            if (singleRadio.IsChecked == true)
            {
                number.Visibility = Visibility.Hidden;
                numberLabel.Visibility = Visibility.Hidden;
            }
            else if (uniqueRadio.IsChecked == true)
            {
                number.Visibility = Visibility.Hidden;
                numberLabel.Visibility = Visibility.Hidden;
            }
            else
            {
                number.Visibility = Visibility.Visible;
                numberLabel.Visibility = Visibility.Visible;
            }
        }

        private void layerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curLayer = layerList.SelectedItem as MercuryGISData.Layer;
            fieldList.ItemsSource = curLayer.GetFields();
        }

        private void fieldList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curField = fieldList.SelectedItem as string;
            if (singleRadio.IsChecked == true)
            {
                MercuryGISData.Symbol symbol = curLayer.GetSymbol();
                list.Clear();
                list.Add(new ColorModel { brush = symbol.brush, text="All", symbol=symbol, id = 0});
            }
            else if (uniqueRadio.IsChecked == true) {
                list.Clear();
                List<string> values = curLayer.GetAllValues(curField);
                for (int i = 0; i < values.Count; i++)
                {
                    Brush tempBrush = new SolidColorBrush(GetRandomColor());
                    PolygonSymbol symbol = new PolygonSymbol();
                    symbol.brush = tempBrush;
                    list.Add(new ColorModel
                    {
                        brush = tempBrush,
                        text = values[i],
                        symbol = symbol,
                        id = i
                    });
                }
            }
            else if (classRadio.IsChecked == true)
            {
                int classNum = Convert.ToInt32(number.Text);
                try
                {
                    List<string> values = curLayer.GetAllValues(curField);
                    List<double> doubles = new List<double>();
                    for (int i = 0; i < values.Count; i++)
                    {
                        doubles.Add(Convert.ToDouble(values[i]));
                    }
                    double max = doubles.Max();
                    double min = doubles.Min();
                    double length = (max - min) / classNum;
                    list.Clear();
                    double curValue = min;
                    
                    for (int i = 0; i < classNum; i++)
                    {
                        Brush tempBrush = new SolidColorBrush(GetRandomColor());

                        PolygonSymbol symbol = new PolygonSymbol();
                        symbol.brush = tempBrush;
                        list.Add(new ColorModel
                        {
                            brush = tempBrush,
                            text = curValue.ToString() + "~" + (curValue + length).ToString(),
                            symbol = symbol,
                            id = i
                        });
                        curValue += length;
                    }
                }
                catch (Exception)
                {
                    
                }
            }
        }

        public Color GetRandomColor()
        {
            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);
            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);
            
            int int_Red = RandomNum_First.Next(256);
            int int_Green = RandomNum_Sencond.Next(256);
            int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;

            return Color.FromRgb((byte)int_Red, (byte)int_Green, (byte)int_Blue);
        }

        private static Color GetColor(Brush br)
        {
            byte a = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).A;
            byte g = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).G;
            byte r = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).R;
            byte b = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).B;
            return Color.FromArgb(a, r, g, b);
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (singleRadio.IsChecked == true)
            {
                switch (curLayer.Type)
                {
                    case MercuryGISData.LayerType.Point:
                        PointLayer pointlayer = (PointLayer)curLayer;
                        for (int i = 0; i < pointlayer.elements.Count; i++)
                        {
                            pointlayer.elements[i].symbol = (PointSymbol)list[0].symbol;
                        }
                        break;
                    case MercuryGISData.LayerType.Line:
                        LineLayer linelayer = (LineLayer)curLayer;
                        for (int i = 0; i < linelayer.elements.Count; i++)
                        {
                            linelayer.elements[i].symbol = (LineSymbol)list[0].symbol;
                        }
                        break;
                    case MercuryGISData.LayerType.Polygon:
                        PolygonLayer polygonlayer = (PolygonLayer)curLayer;
                        for (int i = 0; i < polygonlayer.elements.Count; i++)
                        {
                            polygonlayer.elements[i].symbol = (PolygonSymbol)list[0].symbol;
                        }
                        break;
                    case MercuryGISData.LayerType.Text:
                        break;
                    default:
                        break;
                }
            }
            else if (uniqueRadio.IsChecked == true)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    List<int> ids = curLayer.QueryByAttri(curField + "='" + list[i].text + "'");
                    curLayer.SetSymbol(ids, list[i].symbol);
                }
            }
            else if (classRadio.IsChecked == true)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string[] values = list[i].text.Split('~');
                    List<int> ids = curLayer.QueryByAttri(curField + " >= " + values[0] + " And " + curField + " <= " + values[1]);
                    curLayer.SetSymbol(ids, list[i].symbol);
                }

            }
            map.Refresh();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            btnApply_Click(sender, e);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Brush brush = (((System.Windows.Shapes.Rectangle)e.Source).Fill);
            int id = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (brush == list[i].brush)
                {
                    id = i;
                }
            }
            if (id != -1)
            {
                Symbol symbol = list[id].symbol;
                SymbolSelector form = new SymbolSelector(symbol);
            }
            //int id = Convert.ToInt32(((System.Windows.Shapes.Rectangle)e.Source).Name);
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int id = dataGrid.SelectedIndex;
            if (id != -1)
            {
                Symbol symbol = list[id].symbol;
                SymbolSelector form = new SymbolSelector(symbol);
                if (form.ShowDialog() == true)
                {
                    list[id].brush = symbol.brush;
                    dataGrid.Items.Refresh();
                }
            }
            //int id = Convert.ToInt32(((System.Windows.Shapes.Rectangle)e.Source).Name);
            
        }
    }

    public class ColorModel
    {
        public Brush brush { get; set; }
        public string text { get; set; }

        public Symbol symbol { get; set; }

        public int id { get; set; }
    }
}
