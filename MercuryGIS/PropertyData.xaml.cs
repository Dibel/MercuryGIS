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
using GisSmartTools.Data;

namespace MercuryGIS
{
    /// <summary>
    /// PropertyData.xaml 的交互逻辑
    /// </summary>
    public partial class PropertyData : Window
    {
        private DataTable dt;
        public FeatureSource featuresource;

        public PropertyData()
        {
            InitializeComponent();
        }

        public void SetTable(DataTable input)
        {
            dt = input;
            dataGrid.ItemsSource = dt.DefaultView;
        }

        public void SetData(FeatureSource featuresource)
        {
            this.featuresource = featuresource;
            dt = new DataTable();
            Schema schema = featuresource.schema;
            int columncount = schema.fields.Count;
            dt.Columns.Add("featureID");
            foreach (string attributename in schema.fields.Keys)
            {
                dt.Columns.Add(attributename);
            }
            List<Feature> featurelist = featuresource.features.featureList;
            int featurecount = featurelist.Count;

            for (int i = 0; i < featurecount; i++)
            {
                var row = dt.NewRow();
                row[0] = featurelist[i].featureID;
                //dt[0, i].Value = featurelist[i].featureID;
                for (int j = 0; j < columncount; j++)
                {
                    row[j + 1] = featurelist[i].GetArrtributeByName(dt.Columns[j + 1].ColumnName);
                    //dataGridView1[j + 1, i].Value = featurelist[i].GetArrtributeByName(dataGridView1.Columns[j + 1].HeaderText);
                }
                dt.Rows.Add(row);
            }
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
                //FIXME
                featuresource.AppendField(name, OSGeo.OGR.FieldType.OFTString);
                //DataGridTextColumn col1 = new DataGridTextColumn();
                //col1.Header = name;
                //dataGrid.Columns.Add(col1);
            }
        }

        private void deleteField_Click(object sender, RoutedEventArgs e)
        {
            var col = dataGrid.SelectedCells[0].Column;
            featuresource.schema.DeleteField(col.Header.ToString());
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

        private void dataGrid_CellEditEnding(Object sender, DataGridCellEditEndingEventArgs e)
        {
            
            try
            {
                int rowid = e.Row.GetIndex();
                int colid = e.Column.DisplayIndex;
                //get fid
                int fid = Convert.ToInt32(dt.Rows[rowid][0]);
                //get fieldname
                String fn = (String)e.Column.Header;

                OSGeo.OGR.FieldDefn fdn = null;
                if (fn != null && featuresource.schema.fields.TryGetValue(fn, out fdn))
                {
                    if (fdn != null)
                    {
                        Object value = null;
                        switch (fdn.GetFieldType())
                        {
                            case OSGeo.OGR.FieldType.OFTInteger:
                                value = Convert.ToInt32(((TextBox)e.EditingElement).Text);
                                break;
                            case OSGeo.OGR.FieldType.OFTReal:
                                value = Convert.ToDouble(((TextBox)e.EditingElement).Text);
                                break;
                            case OSGeo.OGR.FieldType.OFTString:
                                value = ((TextBox)e.EditingElement).Text;
                                break;
                            case OSGeo.OGR.FieldType.OFTWideString:
                                value = ((TextBox)e.EditingElement).Text;
                                break;
                        }
                        if (value != null) featuresource.features.GetFeatureByID(fid).ReviseAttribute(fn, value);
                    }
                }
            }
            catch (InvalidCastException)
            {
                MessageBox.Show("新属性值必须与原属性值具有相同类型！");
            }
        }
    }
}
