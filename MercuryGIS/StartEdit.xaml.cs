using GisSmartTools.Support;
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

namespace MercuryGIS
{
    /// <summary>
    /// StartEdit.xaml 的交互逻辑
    /// </summary>
    public partial class StartEdit : Window
    {
        private mapcontent map;
        public Layer selectedlayer;
        public StartEdit(mapcontent map)
        {
            InitializeComponent();
            this.map = map;
            listBox.ItemsSource = map.layerlist;
            listBox.DisplayMemberPath = "Layername";
        }

        private void btnCancel_Click(Object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOk_Click(Object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                selectedlayer = listBox.SelectedItem as Layer;
                DialogResult = true;
            }
        }
    }
}
