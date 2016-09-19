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
    /// SelectField.xaml 的交互逻辑
    /// </summary>
    public partial class SelectField : Window
    {
        public string field;
        public SelectField()
        {
            InitializeComponent();
        }

        public SelectField(List<string> list)
        {
            InitializeComponent();
            comboBox.ItemsSource = list;
            comboBox.SelectedIndex = 0;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            field = comboBox.SelectedItem.ToString();
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
