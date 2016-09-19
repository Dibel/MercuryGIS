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
    /// QueryResult.xaml 的交互逻辑
    /// </summary>
    public partial class QueryResult : Window
    {
        private MercuryGISControl.MapControl map;
        private List<List<int>> list;
        public QueryResult()
        {
            InitializeComponent();
        }

        public QueryResult(MercuryGISControl.MapControl map)
        {
            InitializeComponent();
            this.map = map;
            list = map.Map.QueryByLocat(map.queryRect);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    continue;
                }
                TreeViewItem item = new TreeViewItem() { DataContext = map.Map.GetLayer(i), Header = map.Map.GetLayer(i).Name };
                item.IsExpanded = true;
                for (int j = 0; j < list[i].Count; j++)
                {
                    TreeViewItem node = new TreeViewItem() { Header = list[i][j] };
                    node.IsExpanded = true;
                    item.Items.Add(node);
                }
                treeView.Items.Add(item);
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (TreeViewItem)treeView.SelectedItem;
            try {
                int id = Convert.ToInt32(item.Header);
                MercuryGISData.Layer layer = ((TreeViewItem)item.Parent).DataContext as MercuryGISData.Layer;
                System.Data.DataRow row = layer.GetDataRow(id);
                List<string> fields = layer.GetFields();
                List<DataRowModel> models = new List<DataRowModel>();
                for (int i = 0; i < fields.Count; i++)
                {
                    DataRowModel model = new DataRowModel();
                    model.字段名 = fields[i];
                    model.值 = row[i].ToString();
                    model.数据类型 = layer.dataset.table.Columns[i].DataType.ToString();
                    models.Add(model);
                }
                dataGrid.ItemsSource = models;
            }
            catch (Exception)
            {

            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            map.MapMode = MercuryGISControl.Mode.None;
            map.Map.ClearSelection();
            map.Refresh();
        }
    }

    public class DataRowModel
    {
        public string 字段名 { get; set; }
        public string 值 { get; set; }
        public string 数据类型 { get; set; }
    }
}
