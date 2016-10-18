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
    public partial class PropertyDataReadOnly : Window
    {
        DataTable dt;
        public PropertyDataReadOnly()
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
                    row[j+1] = featurelist[i].GetArrtributeByName(dt.Columns[j+1].ColumnName);
                    //dataGridView1[j + 1, i].Value = featurelist[i].GetArrtributeByName(dataGridView1.Columns[j + 1].HeaderText);
                }
                dt.Rows.Add(row);
            }
            dataGrid.ItemsSource = dt.DefaultView;
        }
    }
}
