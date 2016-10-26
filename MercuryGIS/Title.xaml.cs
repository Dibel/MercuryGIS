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
    /// Title.xaml 的交互逻辑
    /// </summary>
    public partial class Title : Window
    {

        public FontInfo font;
        public string title;
        public Title()
        {
            InitializeComponent();
        }

        public Title(string title, FontInfo font)
        {
            this.font = font;
            this.title = title;
            InitializeComponent();
            textBox.Text = title;
            FontInfo.ApplyFont(textBox, font);
        }

        private void btnCancel_Click(Object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOk_Click(Object sender, RoutedEventArgs e)
        {
            title = textBox.Text;
            DialogResult = true;
        }

        private void button_Click(Object sender, RoutedEventArgs e)
        {
            ColorFontDialog dialog = new ColorFontDialog();
            dialog.Font = font;

            if (dialog.ShowDialog() == true)
            {
                font = dialog.Font;
                if (font != null)
                {
                    FontInfo.ApplyFont(textBox, font);
                }
            }
        }
    }
}
