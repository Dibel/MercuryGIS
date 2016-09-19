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

namespace MercuryGIS
{
    /// <summary>
    /// Interaction logic for SymbolSelector.xaml
    /// </summary>
    public partial class SymbolSelector : Window
    {

        private MercuryGISData.Symbol symbol;
        public SymbolSelector()
        {
            InitializeComponent();
        }

        public SymbolSelector(MercuryGISData.Symbol symbol)
        {
            InitializeComponent();
            this.symbol = symbol;
            
            textBox.Text = symbol.size.ToString();
            if (symbol.brush == null)
            {
                outlineColor.Visibility = Visibility.Collapsed;
                outlineLabel.Visibility = Visibility.Collapsed;
                solidColor.Fill = symbol.pen.Brush;
                return;
            }
            outlineColor.Fill = symbol.pen.Brush;
            solidColor.Fill = symbol.brush;

        }

        //OK
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            symbol.brush = solidColor.Fill;
            symbol.size = Convert.ToInt32(textBox.Text);

            symbol.pen = new Pen(outlineColor.Fill, symbol.size);
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
