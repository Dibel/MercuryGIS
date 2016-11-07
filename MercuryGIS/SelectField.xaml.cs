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
using WpfColorFontDialog;

namespace MercuryGIS
{
    /// <summary>
    /// SelectField.xaml 的交互逻辑
    /// </summary>
    public partial class SelectField : Window
    {
        public string field;
        public bool Checked = true;
        public FontInfo font;
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
            if (checkBox.IsChecked == true)
            {
                Checked = true;
            }
            else
            {
                Checked = false;
            }
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void button_Click(Object sender, RoutedEventArgs e)
        {
            ColorFontDialog dialog = new ColorFontDialog();
            dialog.Font = FontInfo.GetControlFont(label);
            if (dialog.ShowDialog() == true)
            {
                font = dialog.Font;
            }
        }
    }
}
