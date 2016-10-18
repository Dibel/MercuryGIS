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
    /// Interaction logic for PointSymbolSelector.xaml
    /// </summary>
    public partial class PointSymbolSelector: Window
    {

        public SolidColorBrush brush;
        public RenderRule rule;
        private string[] pointstyle = { "实心圆", "实心方框", "三角形", "空心圆", "空心方框", "点圆", "铜钱型" };
        public PointSymbolSelector()
        {
            InitializeComponent();
        }

        public PointSymbolSelector(RenderRule rule)
        {
            InitializeComponent();
            comboBox.ItemsSource = pointstyle;
            this.rule = rule;
            pointsymbolizer pointsymbolizer = (pointsymbolizer)rule.geometrysymbolizer;
            textBox.Text = pointsymbolizer.size.ToString();
            outlineColor.Visibility = Visibility.Collapsed;
            outlineLabel.Visibility = Visibility.Collapsed;
            solidColor.Fill = new SolidColorBrush((Color)pointsymbolizer.color);
            SetPointStyle(pointsymbolizer.pointstyle);
        }

        public void SetPointStyle(PointStyle type)
        {
            switch (type)
            {
                case PointStyle.CIRCLE_FILL:
                    comboBox.SelectedIndex = 0;
                    break;
                case PointStyle.RECT_FILL:
                    comboBox.SelectedIndex = 1;
                    break;
                case PointStyle.TRIANGLE:
                    comboBox.SelectedIndex = 2;
                    break;
                case PointStyle.CIRCLE_HOLLOW:
                    comboBox.SelectedIndex = 3;
                    break;
                case PointStyle.RECT_HOLLOW:
                    comboBox.SelectedIndex = 4;
                    break;
                case PointStyle.CIRCLE_POINT:
                    comboBox.SelectedIndex = 5;
                    break;
                case PointStyle.CIRCLE_RECT:
                    comboBox.SelectedIndex = 6;
                    break;
            }
        }

        public PointStyle GetPointStyleofcombox()
        {
            switch (comboBox.SelectedIndex)
            {
                case 0: return PointStyle.CIRCLE_FILL;
                case 1: return PointStyle.RECT_FILL;
                case 2: return PointStyle.TRIANGLE;
                case 3: return PointStyle.CIRCLE_HOLLOW;
                case 4: return PointStyle.RECT_HOLLOW;
                case 5: return PointStyle.CIRCLE_POINT;
                case 6: return PointStyle.CIRCLE_RECT;
            }
            return PointStyle.CIRCLE_FILL;
        }

        //OK
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            pointsymbolizer pointsymbolizer = (pointsymbolizer)rule.geometrysymbolizer;
            pointsymbolizer.size = Convert.ToInt32(textBox.Text);
            pointsymbolizer.color = ((SolidColorBrush)solidColor.Fill).Color;
            brush = (SolidColorBrush)solidColor.Fill;
            pointsymbolizer.pointstyle = GetPointStyleofcombox();
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
