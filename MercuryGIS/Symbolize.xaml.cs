using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GisSmartTools.Support;
using GisSmartTools.Data;
using OSGeo.OGR;
using System.Windows;
using Microsoft.Samples.CustomControls;

namespace MercuryGIS
{
    /// <summary>
    /// Interaction logic for Symbolize.xaml
    /// </summary>
    public partial class Symbolize : Window
    {

        private GisSmartTools.MapControlWPF map;
        private GisSmartTools.Support.Layer curLayer;
        private string curField;
        private System.Data.DataTable dataTable;
        private wkbGeometryType geotype;
        private List<RenderRule> rulelist;
        private GisSmartTools.Support.Style style;
        private FeatureSource featuresource;
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

        public Symbolize(GisSmartTools.MapControlWPF map)
        {
            InitializeComponent();
            this.map = map;
            layerList.ItemsSource = map.mapcontent.layerlist;
            layerList.DisplayMemberPath = "Layername";
            dataGrid.ItemsSource = list;
        }

        private void singleRadio_Click(object sender, RoutedEventArgs e)
        {
            if (singleRadio.IsChecked == true)
            {
                number.Visibility = Visibility.Hidden;
                numberLabel.Visibility = Visibility.Hidden;
                label.Visibility = Visibility.Hidden;
                label_Copy.Visibility = Visibility.Hidden;
                start.Visibility = Visibility.Hidden;
                end.Visibility = Visibility.Hidden;
            }
            else if (uniqueRadio.IsChecked == true)
            {
                number.Visibility = Visibility.Hidden;
                numberLabel.Visibility = Visibility.Hidden;
                label.Visibility = Visibility.Hidden;
                label_Copy.Visibility = Visibility.Hidden;
                start.Visibility = Visibility.Hidden;
                end.Visibility = Visibility.Hidden;
            }
            else
            {
                number.Visibility = Visibility.Visible;
                numberLabel.Visibility = Visibility.Visible;
                label.Visibility = Visibility.Visible;
                label_Copy.Visibility = Visibility.Visible;
                start.Visibility = Visibility.Visible;
                end.Visibility = Visibility.Visible;
            }
        }

        private void layerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curLayer = layerList.SelectedItem as GisSmartTools.Support.Layer;
            if (curLayer.style != null)
            {

                rulelist = curLayer.style.rulelist;
            }
            else
            {
                GisSmartTools.Support.Style newsimplestyle = GisSmartTools.Support.Style.createSimpleStyle(curLayer.featuresource);
                rulelist = newsimplestyle.rulelist;
            }
            featuresource = curLayer.featuresource;
            Schema schema = featuresource.schema;
            this.geotype = schema.geometryType;
            SetSelectedStyleType(curLayer.style.styletype, this.geotype, curLayer.style, curLayer.featuresource);

            //fieldList.ItemsSource = curLayer.GetFields();
            fieldList.ItemsSource = schema.fields.Keys.ToList();

            //foreach (string attributename in schema.fields.Keys)
            //{
            //    FieldDefn field = null;
            //    if (schema.fields.TryGetValue(attributename, out field))
            //    {
            //        if (field.GetFieldType() == FieldType.OFTReal || field.GetFieldType() == FieldType.OFTInteger)
            //            fieldList.Items.Add(attributename);
            //    }

            //}
        }

        /// <summary>
        /// 初始化时调用，设置初始的combox的值同时根据styletype初始化对应的styleview
        /// </summary>
        /// <param name="type"></param>
        /// <param name="geotype"></param>
        /// <param name="style"></param>
        /// <param name="featuresource"></param>
        private void SetSelectedStyleType(StyleType type, wkbGeometryType geotype, GisSmartTools.Support.Style style, FeatureSource featuresource)
        {
            switch (type)
            {
                case StyleType.SIMPLESTYLE:
                    singleRadio.IsChecked = true;
                    break;
                case StyleType.UNIQUESTYLE:
                    uniqueRadio.IsChecked = true;
                    break;
                case StyleType.RANKSTYLE:
                    classRadio.IsChecked = true;
                    break;
                case StyleType.CUSTOMSTYLE:
                    
                    break;
            }
            List<RenderRule> rulelist = style.rulelist;
            if (rulelist.Count != 0)
            {
                InitializeList(rulelist);
            }
        }

        private void fieldList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curField = fieldList.SelectedItem as string;

            if (singleRadio.IsChecked == true)
            {
                style = GisSmartTools.Support.Style.createSimpleStyle(curLayer.featuresource);
                RenderRule rule = style.defaultRule;
                rulelist = new List<RenderRule>();
                rulelist.Add(rule);
                InitializeList(rulelist);
                //list.Clear();
                //list.Add(new ColorModel { brush = new SolidColorBrush() , text="All", rule=rule, id = 0});
            }
            else if (uniqueRadio.IsChecked == true) {
                list.Clear();
                style = GisSmartTools.Support.Style.createUniqueValueStyle(featuresource, curField);
                rulelist = style.rulelist;
                //List<string> values = curLayer.GetAllValues(curField);
                InitializeList(rulelist);
                //for (int i = 0; i < values.Count; i++)
                //{
                //    Brush tempBrush = new SolidColorBrush(GetRandomColor());
                //    PolygonSymbol symbol = new PolygonSymbol();
                //    symbol.brush = tempBrush;
                //    list.Add(new ColorModel
                //    {
                //        brush = tempBrush,
                //        text = values[i],
                //        symbol = symbol,
                //        id = i
                //    });
                //}
            }
            else if (classRadio.IsChecked == true)
            {
                int classNum = Convert.ToInt32(number.Text);
                style = GisSmartTools.Support.Style.createRankStyle(featuresource, curField, classNum, new GisSmartTools.Color(255, 254, 240, 217), new GisSmartTools.Color(255, 179, 0, 0));
                rulelist = style.rulelist;
                InitializeList(rulelist);
                    //List<string> values = curLayer.GetAllValues(curField);
                    //List<double> doubles = new List<double>();
                    //for (int i = 0; i < values.Count; i++)
                    //{
                    //    doubles.Add(Convert.ToDouble(values[i]));
                    //}
                    //double max = doubles.Max();
                    //double min = doubles.Min();
                    //double length = (max - min) / classNum;
                    //list.Clear();
                    //double curValue = min;
                    
                    //for (int i = 0; i < classNum; i++)
                    //{
                    //    Brush tempBrush = new SolidColorBrush(GetRandomColor());

                    //    PolygonSymbol symbol = new PolygonSymbol();
                    //    symbol.brush = tempBrush;
                    //    list.Add(new ColorModel
                    //    {
                    //        brush = tempBrush,
                    //        text = curValue.ToString() + "~" + (curValue + length).ToString(),
                    //        symbol = symbol,
                    //        id = i
                    //    });
                    //    curValue += length;
                    //}
            }
        }

        private void InitializeList(List<RenderRule> rulelist)
        {
            list.Clear();
            if (rulelist.Count != 0)
            {
                foreach (RenderRule rule in rulelist)
                {
                    string text;
                    if (rule.filter != null)
                    {
                        text = rule.filter.GetDescription();
                    }
                    else
                    {
                        text = "";
                    }
                    switch (rule.geometrysymbolizer.sign)
                    {
                        case SymbolizerType.POINT:
                            var pointsym = (pointsymbolizer)rule.geometrysymbolizer;
                            var pointcolor = pointsym.color;
                            list.Add(new ColorModel { brush = new SolidColorBrush((Color)pointcolor), text = text, rule = rule });
                            break;
                        case SymbolizerType.LINE:
                            var linesym = (linesymbolizer)rule.geometrysymbolizer;
                            var linecolor = linesym.color;
                            list.Add(new ColorModel { brush = new SolidColorBrush((Color)linecolor), text = text, rule = rule });
                            break;
                        case SymbolizerType.POLYGON:
                            var polygonsym = (polygonsymbolizer)rule.geometrysymbolizer;
                            var fillcolor = polygonsym.fillcolor;
                            list.Add(new ColorModel { brush = new SolidColorBrush((Color)fillcolor), text = text, rule = rule });
                            break;
                        case SymbolizerType.TEXT:
                            break;
                    }
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
            GisSmartTools.Support.Style newstyle;
            if (singleRadio.IsChecked == true)
            {
                newstyle = new GisSmartTools.Support.Style(style.name, style.styletype, rulelist[0]);
                
            }
            else
            {
                RenderRule defaultrule = style.defaultRule;
                newstyle = new GisSmartTools.Support.Style(style.name, style.styletype, defaultrule);
                for (int i = 0; i < list.Count; i++)
                {
                    RenderRule rule = list[i].rule;
                    newstyle.addRule(rule);
                }
            }
            
            curLayer.style = newstyle;
            //if (singleRadio.IsChecked == true)
            //{
            //    switch (rulelist[0].geometrysymbolizer.sign)
            //    {
            //        case SymbolizerType.POINT:

            //            break;
            //        case SymbolizerType.LINE:
            //            LineLayer linelayer = (LineLayer)curLayer;
            //            for (int i = 0; i < linelayer.elements.Count; i++)
            //            {
            //                linelayer.elements[i].symbol = (LineSymbol)list[0].symbol;
            //            }
            //            break;
            //        case SymbolizerType.POLYGON:
            //            PolygonLayer polygonlayer = (PolygonLayer)curLayer;
            //            for (int i = 0; i < polygonlayer.elements.Count; i++)
            //            {
            //                polygonlayer.elements[i].symbol = (PolygonSymbol)list[0].symbol;
            //            }
            //            break;
            //        case SymbolizerType.TEXT:
            //            break;
            //        default:
            //            break;
            //    }
            //}
            //else if (uniqueRadio.IsChecked == true)
            //{
            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        List<int> ids = curLayer.QueryByAttri(curField + "='" + list[i].text + "'");
            //        curLayer.SetSymbol(ids, list[i].symbol);
            //    }
            //}
            //else if (classRadio.IsChecked == true)
            //{
            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        string[] values = list[i].text.Split('~');
            //        List<int> ids = curLayer.QueryByAttri(curField + " >= " + values[0] + " And " + curField + " <= " + values[1]);
            //        curLayer.SetSymbol(ids, list[i].symbol);
            //    }

            //}
            map.mapcontrol_refresh();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            btnApply_Click(sender, e);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
                RenderRule rule = list[id].rule;
                SymbolSelector form = new SymbolSelector(rule);
                if (form.ShowDialog() == true)
                {
                    list[id].brush = form.brush;
                    list[id].rule = form.rule;
                    dataGrid.Items.Refresh();
                }
            }
            //int id = Convert.ToInt32(((System.Windows.Shapes.Rectangle)e.Source).Name);
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int id = dataGrid.SelectedIndex;
            if (id != -1)
            {
                RenderRule rule = list[id].rule;
                switch (rule.geometrysymbolizer.sign)
                {
                    case SymbolizerType.POINT:
                        PointSymbolSelector form_point = new PointSymbolSelector(rule);
                        if (form_point.ShowDialog() == true)
                        {
                            list[id].brush = form_point.brush;
                            list[id].rule = form_point.rule;
                            dataGrid.Items.Refresh();
                        }
                        break;
                    case SymbolizerType.POLYGON:
                        PolygonSymbolSelector form_polygon = new PolygonSymbolSelector(rule);
                        if (form_polygon.ShowDialog() == true)
                        {
                            list[id].brush = form_polygon.brush;
                            list[id].rule = form_polygon.rule;
                            dataGrid.Items.Refresh();
                        }
                        break;
                    default:
                        SymbolSelector form = new SymbolSelector(rule);
                        if (form.ShowDialog() == true)
                        {
                            list[id].brush = form.brush;
                            list[id].rule = form.rule;
                            dataGrid.Items.Refresh();
                        }
                        break;
                }
                
            }
            //int id = Convert.ToInt32(((System.Windows.Shapes.Rectangle)e.Source).Name);

        }

        private void start_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            ColorPickerDialog picker = new ColorPickerDialog();
            Brush brush = start.Fill;
            picker.StartingColor = GetColor(brush);
            picker.Owner = this;
            if (picker.ShowDialog() == true)
            {
                start.Fill = new SolidColorBrush(picker.SelectedColor);
                int classNum = Convert.ToInt32(number.Text);
                style = GisSmartTools.Support.Style.createRankStyle(featuresource, curField, classNum, GetColor(start.Fill), GetColor(end.Fill));
                rulelist = style.rulelist;
                InitializeList(rulelist);
            }
        }

        private void end_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            ColorPickerDialog picker = new ColorPickerDialog();
            Brush brush = end.Fill;
            picker.StartingColor = GetColor(brush);
            picker.Owner = this;
            if (picker.ShowDialog() == true)
            {
                end.Fill = new SolidColorBrush(picker.SelectedColor);
                int classNum = Convert.ToInt32(number.Text);
                style = GisSmartTools.Support.Style.createRankStyle(featuresource, curField, classNum, GetColor(start.Fill), GetColor(end.Fill));
                rulelist = style.rulelist;
                InitializeList(rulelist);
            }
        }
        
    }

    public class ColorModel
    {
        public Brush brush { get; set; }
        public string text { get; set; }
        public GisSmartTools.Color color { get; set; }

        public RenderRule rule { get; set; }
        
    }
}
