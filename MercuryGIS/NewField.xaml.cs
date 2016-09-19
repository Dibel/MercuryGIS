﻿using System;
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
    /// NewFiedl.xaml 的交互逻辑
    /// </summary>
    public partial class NewField : Window
    {
        public string fieldName;
        public string fieldType;
        public NewField()
        {
            InitializeComponent();
            comboBox.Items.Add("System.Double");
            comboBox.Items.Add("System.Int32");
            comboBox.Items.Add("System.String");
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            
            fieldName = textBox.Text;
            fieldType = comboBox.SelectedItem as string;
            DialogResult = true;
        }
    }
}
