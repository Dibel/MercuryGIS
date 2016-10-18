using GisSmartTools.Data;
using GisSmartTools.RS;
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
    /// MetaData.xaml 的交互逻辑
    /// </summary>
    public partial class MetaData : Window
    {
        public MetaData()
        {
            InitializeComponent();
        }
        public MetaData(FeatureSource featuresource, string layername)
        {
            InitializeComponent();
            Schema schema = featuresource.schema;
            textBox1.Text = layername;
            textBox1_Copy.Text = schema.name;
            GisSmartTools.Geometry.Rectangle rect = featuresource.features.getEnvelop();
            textBox_maxx.Text = rect.maxX.ToString("0.000");
            textBox_minx.Text = rect.minX.ToString("0.000");
            textBox_maxy.Text = rect.maxY.ToString("0.000");
            textBox_miny.Text = rect.minY.ToString("0.000");
            ReferenceSystem rs = schema.rs;
            string rstext = "";
            rs.spetialReference.ExportToWkt(out rstext);

            textBlock.Text = rstext;
        }
    }
}
