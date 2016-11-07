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
using GisSmartTools;
using GisSmartTools.Data;

namespace MercuryGIS
{
    /// <summary>
    /// QueryResult.xaml 的交互逻辑
    /// </summary>
    public partial class QueryResult : Window
    {
        private FeatureCollection source;
        private List<List<int>> list;
        public QueryResult()
        {
            InitializeComponent();
        }

        public QueryResult(FeatureCollection source)
        {
            InitializeComponent();
            this.source = source;
            //list = map.Map.QueryByLocat(map.queryRect);
            TreeViewItem item = new TreeViewItem() { Header="Feature ID" };
            item.IsExpanded = true;
            for (int j = 0; j < source.featureList.Count; j++)
            {
                TreeViewItem node = new TreeViewItem() { Header = source.featureList[j].featureID.ToString() };
                node.IsExpanded = true;
                item.Items.Add(node);
            }
            treeView.Items.Add(item);
            //for (int i = 0; i < list.Count; i++)
            //{
            //    if (list[i] == null)
            //    {
            //        continue;
            //    }
            //    //TreeViewItem item = new TreeViewItem() { DataContext = map.Map.GetLayer(i), Header = map.Map.GetLayer(i).Name };
            //    //item.IsExpanded = true;

            //    //treeView.Items.Add(item);
            //}
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (TreeViewItem)treeView.SelectedItem;
            int id = Convert.ToInt32(item.Header);
            Feature feature = source.GetFeatureByID(id);
            Schema schema = feature.schema;
            List<DataRowModel> models = new List<DataRowModel>();
            DataRowModel geometrymodel = new DataRowModel();
            geometrymodel.字段名 = "地理要素类型";
            switch (feature.geometry.geometryType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    geometrymodel.值 = "点";
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    geometrymodel.值 = "线";
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                    geometrymodel.值 = "多线";
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    geometrymodel.值 = "面";
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
                    geometrymodel.值 = "多面";
                    break;
            }
            //geometrymodel.数据类型 = layer.dataset.table.Columns[i].DataType.ToString();
            models.Add(geometrymodel);
            foreach (string attributename in feature.attributes.Keys)
            {
                DataRowModel model = new DataRowModel();
                model.字段名 = attributename;
                model.值 = feature.GetArrtributeByName(attributename).ToString();
                //OSGeo.OGR.FieldDefn fdn = null;
                //schema.fields.TryGetValue(attributename, out fdn);
                model.数据类型 = schema.GetArrtributeTypeByName(attributename);
                models.Add(model);
            }

            dataGrid.ItemsSource = models;

            try
            {
                //int id = Convert.ToInt32(item.Header);
                //MercuryGISData.Layer layer = ((TreeViewItem)item.Parent).DataContext as MercuryGISData.Layer;
                //System.Data.DataRow row = layer.GetDataRow(id);
                //List<string> fields = layer.GetFields();
                //List<DataRowModel> models = new List<DataRowModel>();
                //for (int i = 0; i < fields.Count; i++)
                //{
                //    DataRowModel model = new DataRowModel();
                //    model.字段名 = fields[i];
                //    model.值 = row[i].ToString();
                //    model.数据类型 = layer.dataset.table.Columns[i].DataType.ToString();
                //    models.Add(model);
                //}
                //dataGrid.ItemsSource = models;
            }
            catch (Exception)
            {

            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }
    }

    public class DataRowModel
    {
        public string 字段名 { get; set; }
        public string 值 { get; set; }
        public string 数据类型 { get; set; }
    }
}
