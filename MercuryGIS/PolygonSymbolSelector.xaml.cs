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
using System.Windows.Shapes;
using Microsoft.Samples.CustomControls;
using GisSmartTools.Support;

namespace MercuryGIS
{
    /// <summary>
    /// Interaction logic for PolygonSymbolSelector.xaml
    /// </summary>
    public partial class PolygonSymbolSelector : Window
    {

        public SolidColorBrush brush;
        public RenderRule rule;
        private List<string> list = new List<string> { "纯色", "斜线", "网格" };
        public PolygonSymbolSelector()
        {
            InitializeComponent();
        }

        public PolygonSymbolSelector(RenderRule rule)
        {
            InitializeComponent();
            this.rule = rule;
            comboBox.ItemsSource = list;
            polygonsymbolizer polygonsymbolizer = (polygonsymbolizer)rule.geometrysymbolizer;
            textBox.Text = polygonsymbolizer.strokewidth.ToString();
            outlineColor.Fill = new SolidColorBrush((Color)polygonsymbolizer.strokecolor);
            solidColor.Fill = new SolidColorBrush((Color)polygonsymbolizer.fillcolor);

            comboBox.SelectedIndex = (int)polygonsymbolizer.polygonstyle - 1;
        }

        //OK
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            polygonsymbolizer polygonsymbolizer = (polygonsymbolizer)rule.geometrysymbolizer;
            polygonsymbolizer.strokewidth = Convert.ToInt32(textBox.Text);
            polygonsymbolizer.strokecolor = ((SolidColorBrush)outlineColor.Fill).Color;
            polygonsymbolizer.fillcolor = ((SolidColorBrush)solidColor.Fill).Color;
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    polygonsymbolizer.polygonstyle = PolygonStyle.SOLID;
                    break;
                case 1:
                    polygonsymbolizer.polygonstyle = PolygonStyle.LINE;
                    break;
                case 2:
                    polygonsymbolizer.polygonstyle = PolygonStyle.GRID;
                    break;
                default:
                    polygonsymbolizer.polygonstyle = PolygonStyle.SOLID;
                    break;
            }
            brush = (SolidColorBrush)solidColor.Fill;
            DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ColorPickerDialog picker = new ColorPickerDialog();
            Brush brush = solidColor.Fill;
            picker.StartingColor = GetColor(brush);
            picker.Owner = this;
            if (picker.ShowDialog() == true)
            {
                solidColor.Fill = new SolidColorBrush(picker.SelectedColor);
            }
        }

        private static Color GetColor(Brush br)
        {
            byte a = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).A;
            byte g = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).G;
            byte r = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).R;
            byte b = ((Color)br.GetValue(SolidColorBrush.ColorProperty)).B;
            return Color.FromArgb(a, r, g, b);
        }

        private void outlineColor_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ColorPickerDialog picker = new ColorPickerDialog();
            Brush brush = outlineColor.Fill;
            picker.StartingColor = GetColor(brush);
            picker.Owner = this;
            if (picker.ShowDialog() == true)
            {
                outlineColor.Fill = new SolidColorBrush(picker.SelectedColor);
            }
        }
    }
}
