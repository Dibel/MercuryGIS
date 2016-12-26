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
    /// Coord.xaml 的交互逻辑
    /// </summary>
    public partial class Coord : Window
    {
        public int X, Y;
        public Coord()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                X = Convert.ToInt32(textBox.Text);
                Y = Convert.ToInt32(textBox_Copy.Text);
                this.DialogResult = true;
            }
            catch
            {
                MessageBox.Show("输入坐标有误，请重新输入");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
