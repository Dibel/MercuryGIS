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
using System.Data;

namespace MercuryGIS
{
    /// <summary>
    /// PropertyData.xaml 的交互逻辑
    /// </summary>
    public partial class PropertyData : Window
    {
        private DataTable dt;


        public PropertyData()
        {
            InitializeComponent();
        }

        public void SetTable(DataTable input)
        {
            dt = input;
            dataGrid.ItemsSource = dt.DefaultView;
        }

        private void addField_Click(object sender, RoutedEventArgs e)
        {
            NewField dialog = new NewField();
            if (dialog.ShowDialog() == true)
            {
                string name = dialog.fieldName;
                string type = dialog.fieldType;
                DataColumn col = new DataColumn(name, Type.GetType(type));
                dt.Columns.Add(col);
                dataGrid.ItemsSource = null;
                dataGrid.ItemsSource = dt.DefaultView;
                dataGrid.Items.Refresh();
                //DataGridTextColumn col1 = new DataGridTextColumn();
                //col1.Header = name;
                //dataGrid.Columns.Add(col1);
            }
        }

        private void deleteField_Click(object sender, RoutedEventArgs e)
        {
            var col = dataGrid.SelectedCells[0].Column;
            dataGrid.Columns.Remove(col);
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
