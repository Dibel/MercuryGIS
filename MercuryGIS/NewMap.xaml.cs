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
    /// NewMap.xaml 的交互逻辑
    /// </summary>
    public partial class NewMap : Window
    {
        public string mapName;
        public string server;
        public string port;
        public string username;
        public string password;
        public string database;

        public NewMap()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(Object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOk_Click(Object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            mapName = textBox.Text;
            server = textbox_server.Text;
            port = textbox_port.Text;
            username = textbox_username.Text;
            password = textbox_password.Password;
            database = textbox_database.Text;
        }
    }
}
