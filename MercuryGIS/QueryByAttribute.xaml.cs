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
using MercuryGISControl;

namespace MercuryGIS
{
    /// <summary>
    /// Interaction logic for QueryAttribute.xaml
    /// </summary>
    public partial class QueryAttribute : Window
    {
        MapControl map;
        MercuryGISData.Layer curLayer;
        string curField;
        public QueryAttribute()
        {
            InitializeComponent();
        }

        public QueryAttribute(MapControl map)
        {
            InitializeComponent();
            this.map = map;
            layerList.ItemsSource = map.Map.GetLayers();
            layerList.DisplayMemberPath = "Name";
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curLayer = layerList.SelectedItem as MercuryGISData.Layer;
            fieldList.ItemsSource = curLayer.GetFields();
        }

        //Field list
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curField = fieldList.SelectedItem as string;
        }


        //Value list
        private void ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            valueList.ItemsSource = curLayer.GetAllValues(curField);
        }

        private void fieldList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (fieldList.SelectedIndex != -1)
            {
                textBox.Text += fieldList.SelectedItem as string;
            }
        }

        private void valueList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (valueList.SelectedIndex != -1)
            {
                textBox.Text += "'" + valueList.SelectedItem as string + "'";
            }
        }

        //Clear
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            textBox.Clear();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            textBox.Text += " " + btn.Content;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            curLayer.ClearSelection();
            string sql = textBox.Text;
            List<int> ids = curLayer.QueryByAttri(sql);
            curLayer.Select(ids);
            map.Refresh();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            btnApply_Click(sender, e);
            map.MapMode = Mode.Select;
            this.Close();
        }
    }
}
