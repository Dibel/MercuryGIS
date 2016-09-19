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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fluent;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using MercuryGISControl;
using MercuryGISData;
using System.IO;

namespace MercuryGIS
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {

        Layer curLayer;
        Layer backupLayer;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnCheckItem(object sender, RoutedEventArgs e)
        {
            //listView.Items[0].checkbox;
            for (int i = 0; i < listView.Items.Count; i++)
            {
                LayerModel model = (LayerModel)listView.Items[i];
                mapControl.Map.GetLayer(i).IsVisible = model.isChecked;
            }
            mapControl.Refresh();
        }

        private void Open_MouseDown(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Map File (.txt)|*.txt";
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                mapControl.Map.OpenMap(path);
                List<Layer> layers = mapControl.Map.GetLayers();
                for (int i = 0; i < mapControl.Map.LayerCount; i++)
                {
                    layers[i].Name = "test";
                    listView.Items.Add(new LayerModel
                    {
                        isChecked = true,
                        name = layers[i].Name,
                    });
                    //items.Add(layers[i].Name);
                }
                mapControl.Refresh();
                this.Title = "Mercury GIS - " + System.IO.Path.GetFileNameWithoutExtension(path);
                curLayer = layers[0];
            }
        }

        private void Save_MouseDown(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                mapControl.Map.SaveMap(path);
            }
        }

        //Export
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Export
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
            }
        }

        private void btnPan_Click(object sender, RoutedEventArgs e)
        {
            if (btnPan.IsChecked == true)
            {
                btnEdit.IsChecked = false;
                btnSelect.IsChecked = false;
                btnZoomIn.IsChecked = false;
                btnzoomOut.IsChecked = false;
                mapControl.MapMode = Mode.Pan;
                mapControl.Cursor = ((TextBlock)Resources["Pan"]).Cursor;
            }
            else
            {
                mapControl.MapMode = Mode.None;
                mapControl.Cursor = Cursors.Arrow;
            }
        }

        private void Data_Click(object sender, RoutedEventArgs e)
        {
            if (mapControl.MapMode == Mode.Edit || mapControl.MapMode == Mode.EditAdd)
            {
                PropertyData form = new PropertyData();
                int id = listView.SelectedIndex;
                form.SetTable(mapControl.Map.GetLayer(id).dataset.table);
                form.Show();
            }
            else
            {
                PropertyDataReadOnly form = new PropertyDataReadOnly();
                int id = listView.SelectedIndex;
                form.SetTable(mapControl.Map.GetLayer(id).dataset.table);
                form.Show();
            }
        }

        private void Property_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (btnEdit.IsChecked == true)
            {
                btnPan.IsChecked = false;
                btnSelect.IsChecked = false;
                btnZoomIn.IsChecked = false;
                btnzoomOut.IsChecked = false;
                // Backup a layer
                mapControl.MapMode = Mode.Edit;
                groupEdit.Visibility = Visibility.Visible;
                TabElement.IsSelected = true;
            }
            else
            {
                mapControl.MapMode = Mode.None;
            }
        }

        private void mapControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point scrPoint = e.GetPosition(mapControl);
            MPoint mapPoint = mapControl.ToMapPoint(scrPoint);
            location.Value = "(" + mapPoint.X.ToString() + ", " + mapPoint.Y.ToString() + ")";
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (btnZoomIn.IsChecked == true)
            {
                btnPan.IsChecked = false;
                btnSelect.IsChecked = false;
                btnzoomOut.IsChecked = false;
                btnEdit.IsChecked = false;
                mapControl.MapMode = Mode.ZoomIn;
                mapControl.Cursor = ((TextBlock)Resources["ZoomIn"]).Cursor;
            }
            else
            {
                mapControl.MapMode = Mode.None;
                mapControl.Cursor = Cursors.Arrow;
            }
        }

        private void btnzoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (btnzoomOut.IsChecked == true)
            {
                btnPan.IsChecked = false;
                btnSelect.IsChecked = false;
                btnZoomIn.IsChecked = false;
                btnEdit.IsChecked = false;
                mapControl.MapMode = Mode.ZoomOut;
                mapControl.Cursor = ((TextBlock)Resources["ZoomOut"]).Cursor;
            }
            else
            {
                mapControl.MapMode = Mode.None;
                mapControl.Cursor = Cursors.Arrow;
            }
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            Layer layer = mapControl.Map.GetLayer(listView.SelectedIndex);
            InputBox dialog = new InputBox(layer.Name);
            if (dialog.ShowDialog() == true)
            {
                layer.Name = dialog.Text;
                ((LayerModel)listView.SelectedItem).name = dialog.Text;
            }
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView.SelectedIndex != -1)
            {
                curLayer = mapControl.Map.GetLayer(listView.SelectedIndex);
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (btnSelect.IsChecked == true)
            {
                btnPan.IsChecked = false;
                btnZoomIn.IsChecked = false;
                btnEdit.IsChecked = false;
                btnzoomOut.IsChecked = false;
                mapControl.MapMode = Mode.Select;
                mapControl.Cursor = Cursors.Cross;
            }
            else
            {
                mapControl.MapMode = Mode.None;
                mapControl.Cursor = Cursors.Arrow;
            }
        }

        //Import shp file
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Shapefile File (.shp)|*.shp";
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                Layer newLayer = Importer.readshpfile(path);
                newLayer.Filename = path;
                newLayer.Name = System.IO.Path.GetFileNameWithoutExtension(path);
                mapControl.Map.Add(newLayer);
                listView.Items.Add(new LayerModel
                {
                    isChecked = true,
                    name = newLayer.Name,
                });
                mapControl.ScaleToLayer(newLayer);
                this.Title = "Mercury GIS - " + System.IO.Path.GetFileNameWithoutExtension(path);
                curLayer = newLayer;
            }
        }

        private void btnWholeScreen_Click(object sender, RoutedEventArgs e)
        {
            mapControl.ScaleToLayer(curLayer);
        }

        private void listView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (listView.SelectedIndex != -1)
            {
                curLayer = mapControl.Map.GetLayer(listView.SelectedIndex);
            }
        }


        private void btnAddPointLayer_Click(object sender, RoutedEventArgs e)
        {
            
            InputBox dialog = new InputBox();
            if (dialog.ShowDialog() == true)
            {
                PointLayer layer = new PointLayer();
                layer.Name = dialog.Text;
                mapControl.Map.Add(layer);
                listView.Items.Add(new LayerModel
                {
                    isChecked = true,
                    name = layer.Name,
                });
                curLayer = layer;
            }

        }

        private void btnAddLineLayer_Click(object sender, RoutedEventArgs e)
        {
            InputBox dialog = new InputBox();
            if (dialog.ShowDialog() == true)
            {
                LineLayer layer = new LineLayer();
                layer.Name = dialog.Text;
                mapControl.Map.Add(layer);
                listView.Items.Add(new LayerModel
                {
                    isChecked = true,
                    name = layer.Name,
                });
                curLayer = layer;
            }
        }

        private void btnAddPolygonLayer_Click(object sender, RoutedEventArgs e)
        {
            InputBox dialog = new InputBox();
            if (dialog.ShowDialog() == true)
            {
                PolygonLayer layer = new PolygonLayer();
                layer.Name = dialog.Text;
                mapControl.Map.Add(layer);
                listView.Items.Add(new LayerModel
                {
                    isChecked = true,
                    name = layer.Name,
                });
                curLayer = layer;
            }
        }

        private void btnEndEdit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(this, "是否保存编辑内容？", "提示", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                groupEdit.Visibility = Visibility.Collapsed;
                TabStart.IsSelected = true;
                btnEdit.IsChecked = false;
                mapControl.MapMode = Mode.None;
                mapControl.Cursor = Cursors.Arrow;
                if (curLayer.Filename != null)
                {
                    curLayer.SaveLayer(curLayer.Filename.Substring(0, curLayer.Filename.Length - 4));

                }
            }
            else if(result == MessageBoxResult.No)
            {
                groupEdit.Visibility = Visibility.Collapsed;
                TabStart.IsSelected = true;
                btnEdit.IsChecked = false;
                mapControl.MapMode = Mode.None;
                mapControl.Cursor = Cursors.Arrow;
                string path = curLayer.Filename;
                Layer newLayer = Importer.readshpfile(path);
                newLayer.Filename = path;
                //curLayer = newLayer;
                mapControl.Map.SetLayer(curLayer, newLayer);
                curLayer = newLayer;
                mapControl.ScaleToLayer(newLayer);
            }
        }

        private void btnAddElement_Click(object sender, RoutedEventArgs e)
        {
            mapControl.CurLayer = curLayer;
            mapControl.MapMode = Mode.EditAdd;
            mapControl.Cursor = Cursors.Cross;
        }

        private void btnMapping_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PNG File (.png)|*.png";
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                //RenderTargetBitmap bmp = new RenderTargetBitmap((int)mapControl.RenderSize.Width, (int)mapControl.RenderSize.Height, 96, 96, PixelFormats.Pbgra32);
                //bmp.Render(mapControl);
                Rect bounds = VisualTreeHelper.GetDescendantBounds(mapControl);
                double dpi = 96d;


                RenderTargetBitmap bmp = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);


                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    VisualBrush vb = new VisualBrush(mapControl);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
                }

                bmp.Render(dv);
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                using (Stream stream = File.Create(path))
                {
                    encoder.Save(stream);
                }
            }
            

        }

        private void btnMapLable_Click(object sender, RoutedEventArgs e)
        {
            SelectField form = new SelectField(curLayer.GetFields());
            if (form.ShowDialog() == true)
            {
                curLayer.IsLabelShown = true;
                curLayer.LabelField = form.field;
                mapControl.Refresh();
            }
        }

        private void btnQueryByAttri_Click(object sender, RoutedEventArgs e)
        {
            QueryAttribute form = new QueryAttribute(mapControl);
            form.Show();
        }

        private void btnQueryByLocat_Click(object sender, RoutedEventArgs e)
        {
            mapControl.MapMode = Mode.Select;
            mapControl.Cursor = Cursors.Cross;
            
        }

        private void mapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mapControl.MapMode == Mode.Select)
            {
                mapControl.Cursor = Cursors.Arrow;
                QueryResult form = new QueryResult(mapControl);
                form.Show();
            }
        }

        private void btnSymbolize_Click(object sender, RoutedEventArgs e)
        {
            Symbolize form = new Symbolize(mapControl);
            form.Show();
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            int id = listView.SelectedIndex;
            if (id > 0)
            {
                var temp = listView.Items[id];
                listView.Items.RemoveAt(id);
                listView.Items.Insert(id - 1, temp);
                mapControl.Map.MoveUpOneStep(id);
                mapControl.Refresh();
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            int id = listView.SelectedIndex;
            if (id < listView.Items.Count - 1)
            {
                var temp = listView.Items[id];
                listView.Items.RemoveAt(id);
                listView.Items.Insert(id + 1, temp);
                mapControl.Map.MoveDownOneStep(id);
                mapControl.Refresh();
            }
        }

        private void RibbonWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = row.ActualHeight;
            double width = this.Width - listView.RenderSize.Width;
            mapControl.Height = height;
            mapControl.Width = width;
        }
    }

    class LayerModel
    {
        public bool isChecked { get; set; }
        public string name { get; set; }
    }
}
