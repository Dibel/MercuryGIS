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
using GisSmartTools.Support;
using GisSmartTools.Filter;

namespace MercuryGIS
{
    /// <summary>
    /// Interaction logic for QueryAttribute.xaml
    /// </summary>
    public partial class QueryAttribute : Window
    {
        MapControlWPF map;
        public Layer curLayer;
        string curField;
        public QueryAttribute()
        {
            InitializeComponent();
        }

        public QueryAttribute(MapControlWPF map)
        {
            InitializeComponent();
            this.map = map;
            layerList.ItemsSource = map.mapcontent.layerlist;
            layerList.DisplayMemberPath = "Layername";
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curLayer = layerList.SelectedItem as Layer;
            var featuresource = curLayer.featuresource;
            fieldList.ItemsSource = featuresource.schema.fields.Keys.ToList();
        }

        //Field list
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curField = fieldList.SelectedItem as string;
        }


        //Value list
        private void ListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HashSet<object> set = new HashSet<object>();
            foreach (GisSmartTools.Data.Feature feature in curLayer.featuresource.features.featureList)
            {
                set.Add(feature.GetArrtributeByName(curField));
            }
            valueList.ItemsSource = set.ToList();
            //valueList.ItemsSource = curLayer.GetAllValues(curField);
        }

        private void fieldList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (fieldList.SelectedIndex != -1)
            {
                textBox.Text += fieldList.SelectedItem as string;
            }
        }

        private void valueList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (valueList.SelectedIndex != -1)
            {
                textBox.Text += valueList.SelectedItem.ToString();
            }
        }

        //Clear
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            textBox.Clear();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button btn = e.Source as Button;
            textBox.Text += " " + btn.Content + " ";
        }

        Filter evaluate(string input)
        {
            Dictionary<string, int> priority = new Dictionary<string, int>();
            priority["and"] = 2;
            priority["or"] = 2;
            priority["not"] = 2;
            priority[">"] = 1;
            priority["<"] = 1;
            priority["="] = 1;
            priority["<>"] = 1;
            var list = input.Split(' ');
            Stack<string> stack = new Stack<string>();
            List<string> postfix = new List<string>();
            
            foreach (var item in list)
            {
                if (item != "" && item != "<>" && item != "not" && item != "(" && item != ")" && item != ">" && item != "<" && item != "=" && item != "and" && item != "or")
                {
                    postfix.Add(item);
                }
                else if (item == "(")
                {
                    stack.Push(item);
                }
                else if (item == ")")
                {
                    string op = stack.Pop();
                    while (op != "(")
                    {
                        postfix.Add(op);
                        op = stack.Pop();
                    }
                }
                else
                {
                    while (true)
                    {
                        if (stack.Count == 0 || stack.Peek() == "(" || priority[item] < priority[stack.Peek()])
                        {
                            stack.Push(item);
                            break;
                        }
                        else
                        {
                            postfix.Add(stack.Pop());
                        }
                    }
                }
            }
            while(stack.Count != 0)
            {
                postfix.Add(stack.Pop());
            }
            var s = new System.Collections.Stack();

            for (int i = 0; i < postfix.Count; i++)
            {
                if (postfix[i] == ">")
                {
                    double value = double.Parse(s.Pop() as string);
                    string name = s.Pop() as string;
                    Filter filter = new Filter_Larger(name, value);
                    s.Push(filter);
                }
                else if (postfix[i] == "<")
                {
                    double value = double.Parse(s.Pop() as string);
                    string name = s.Pop() as string;
                    Filter filter = new Filter_Less(name, value);
                    s.Push(filter);
                }
                else if (postfix[i] == "=")
                {
                    string str = s.Pop() as string;
                    double temp;
                    bool isNum = Double.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out temp);
                    object value = str;
                    if (isNum)
                    {
                        value = temp;
                    }
                    string name = s.Pop() as string;
                    Filter filter = new Filter_Equals(name, value);
                    s.Push(filter);
                }
                else if (postfix[i] == "<>")
                {
                    string str = s.Pop() as string;
                    double temp;
                    bool isNum = Double.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out temp);
                    object value = str;
                    if (isNum)
                    {
                        value = temp;
                    }
                    string name = s.Pop() as string;
                    Filter filter = new Filter_Not(new Filter_Equals(name, value));
                    s.Push(filter);
                }
                else if (postfix[i] == "and")
                {
                    Filter filter1 = s.Pop() as Filter;
                    Filter filter2 = s.Pop() as Filter;
                    Filter filter = new Filter_And(new List<Filter> { filter1, filter2 });
                    s.Push(filter);
                }
                else if (postfix[i] == "or")
                {
                    Filter filter1 = s.Pop() as Filter;
                    Filter filter2 = s.Pop() as Filter;
                    Filter filter = new Filter_Or(new List<Filter> { filter1, filter2 });
                    s.Push(filter);
                }
                else if (postfix[i] == "not")
                {
                    Filter filter = s.Pop() as Filter;
                    s.Push(new Filter_Not(filter));
                }
                else
                {
                    s.Push(postfix[i]);
                }
            }
            Filter result = s.Pop() as Filter;
            return result;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            var filter = evaluate(textBox.Text);
            map.mapcontrol_refresh();
            map.Query_attribute(curLayer, filter);
            //curLayer.ClearSelection();
            //string sql = textBox.Text;
            //List<int> ids = curLayer.QueryByAttri(sql);
            //curLayer.Select(ids);
            //map.Refresh();

        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            btnApply_Click(sender, e);
            //map.MapMode = Mode.Select;
            this.Close();
        }
    }
}
