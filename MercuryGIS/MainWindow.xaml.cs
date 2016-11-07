
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
using GisSmartTools.Data;
using GisSmartTools;
using GisSmartTools.Support;
using GisSmartTools.Geometry;
using WpfColorFontDialog;

namespace MercuryGIS
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        MapProject mappro = null;
        GeoDatabase gdb = null;
        //Layer curLayer;
        //Layer backupLayer;
        WfsServer wfsserver;
        public MainWindow()
        {
            InitializeComponent();
            mapControl.AfterSelectedFeaturesEvent += mapControl_AfterSelectedFeaturesEvent;
        }

        private void mapControl_AfterSelectedFeaturesEvent(FeatureCollection collection)
        {
            QueryResult form = new QueryResult(collection);
            form.Show();
        }

        public void OnCheckItem(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //treeView.Items[0].checkbox;
            LayerModel model = (LayerModel)sender;
            mapControl.mapcontent.GetLayerByName(model.Name).visible = model.IsChecked;
            //for (int i = 0; i < treeView.Items.Count; i++)
            //{
            //    LayerModel model = (LayerModel)treeView.Items[i];
            //    mapControl.mapcontent.GetLayerByName(model.Name).visible = model.IsChecked;
            //    //mapControl.Map.GetLayer(i).IsVisible = model.IsChecked;
            //}
            mapControl.mapcontrol_refresh();
        }

        private void Open_MouseDown(object sender, RoutedEventArgs e)
        {

            OpenFileDialog opendlg = new OpenFileDialog();
            opendlg.Filter = "smartgisproject(*.pro)|*.pro";
            if (opendlg.ShowDialog(this) == true)
            {
                mappro = MapProject.LoadMapProject(opendlg.FileName);
                if (mappro == null)
                {
                    //MessageBox.Show("打开地图失败！");
                    return;
                }

                mappro.projectpath = opendlg.FileName;
                List<string> datapathlist = mappro.dic_datapath.Values.ToList();
                mapControl.mapcontent = mapcontent.LoadMapContent(mappro.projectname, mappro.dic_datapath, mappro.dic_stylepath, mappro.server, mappro.port, mappro.username, mappro.password, mappro.database);
                this.gdb = mapControl.mapcontent.gdb;
                ShowTreeView(mapControl.mapcontent);
                mapControl.SetDefaultoffsetandDisplayScale(mapControl.mapcontent);
                mapControl.mapcontrol_refresh();
            }

            //OpenFileDialog dialog = new OpenFileDialog();
            //dialog.Filter = "Map File (.txt)|*.txt";
            //if (dialog.ShowDialog() == true)
            //{
            //    string path = dialog.FileName;
            //    mapControl.Map.OpenMap(path);
            //    List<Layer> layers = mapControl.Map.GetLayers();
            //    for (int i = 0; i < mapControl.Map.LayerCount; i++)
            //    {
            //        layers[i].Name = "test";
            //        treeView.Items.Add(new LayerModel(layers[i].Name));
            //        //items.Add(layers[i].Name);
            //    }
            //    mapControl.Refresh();
            //    this.Title = "Mercury GIS - " + System.IO.Path.GetFileNameWithoutExtension(path);
            //    curLayer = layers[0];
            //}


        }

        private void Save_MouseDown(object sender, RoutedEventArgs e)
        {
            if (mappro == null || gdb == null) { return; }
            if (mappro.projectpath == null || mappro.projectpath == "")
            {
                SaveFileDialog sSavefiledlg = new SaveFileDialog();
                sSavefiledlg.Filter = "MercuryGIS Project(*.pro)|*.pro";
                if (sSavefiledlg.ShowDialog(this) == true)
                {
                    mappro.projectpath = sSavefiledlg.FileName;
                }
            }
            mapControl.mapcontent.Savemapcontent(mappro.dic_stylepath);
            //按照treeview调整path的顺序
            ItemCollection nodes = treeView.Items;
            Dictionary<string, string> ordereddatapath = new Dictionary<string, string>();
            Dictionary<string, string> orderedstylepath = new Dictionary<string, string>();
            for (int i = 0; i < nodes.Count; i++)
            {
                LayerModel model = (LayerModel)nodes[i];
                ordereddatapath.Add(model.Name, mappro.dic_datapath[model.Name]);
                orderedstylepath.Add(model.Name, mappro.dic_stylepath[model.Name]);

            }
            mappro.dic_datapath = ordereddatapath;
            mappro.dic_stylepath = orderedstylepath;
            if (mappro.SaveMapProject(mappro.projectpath)) MessageBox.Show(this, "地图保存成功!", "保存地图");
            //SaveFileDialog dialog = new SaveFileDialog();
            //if (dialog.ShowDialog() == true)
            //{
            //    string path = dialog.FileName;
            //    mapControl.Map.SaveMap(path);
            //}
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
                mapControl.Pan();
                //mapControl.MapMode = Mode.Pan;
                //mapControl.Cursor = ((TextBlock)Resources["Pan"]).Cursor;
            }
            else
            {
                mapControl.ReturnToNone();
                //mapControl.MapMode = Mode.None;
                //mapControl.Cursor = Cursors.Arrow;
            }
        }

        private void Data_Click(object sender, RoutedEventArgs e)
        {
            if (mapControl.mapOpt == MapOptionStatus.Edit)
            {
                PropertyData form = new PropertyData();
                int id = treeView_selectedindex();
                if (id >= 0)
                {
                    LayerModel temp = (LayerModel)treeView.Items[id];
                    var layer = mapControl.mapcontent.GetLayerByName(temp.Name);
                    form.SetData(layer.featuresource);
                    //form.SetTable(mapControl.Map.GetLayer(id).dataset.table);
                    form.Show();
                }
                //form.SetTable(mapControl.Map.GetLayer(id).dataset.table);
            }
            else
            {
                PropertyDataReadOnly form = new PropertyDataReadOnly();
                int id = treeView_selectedindex();
                if (id >=0)
                {
                    LayerModel temp = (LayerModel)treeView.Items[id];
                    var layer = mapControl.mapcontent.GetLayerByName(temp.Name);
                    form.SetData(layer.featuresource);
                    //form.SetTable(mapControl.Map.GetLayer(id).dataset.table);
                    form.Show();
                }
                
            }
        }

        private void Property_Click(object sender, RoutedEventArgs e)
        {
            int id = treeView_selectedindex();
            if (id >= 0)
            {
                LayerModel temp = (LayerModel)treeView.Items[id];
                var layer = mapControl.mapcontent.GetLayerByName(temp.Name);
                MetaData form = new MetaData(layer.featuresource, temp.Name);
                form.Show();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            if (btnEdit.IsChecked == true)
            {
                
                StartEdit dialog = new StartEdit(mapControl.mapcontent);
                if (dialog.ShowDialog() == true)
                {
                    mapControl.StartEdit(dialog.selectedlayer);
                    btnPan.IsChecked = false;
                    btnSelect.IsChecked = false;
                    btnZoomIn.IsChecked = false;
                    btnzoomOut.IsChecked = false;
                    btnEdit.IsChecked = false;
                    // Backup a layer
                    //mapControl.MapMode = Mode.Edit;
                    groupEdit.Visibility = Visibility.Visible;
                    TabElement.IsSelected = true;
                }
            }
            else
            {
                groupEdit.Visibility = Visibility.Collapsed;
                //mapControl.MapMode = Mode.None;
            }
        }

        private void mapControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point scrPoint = e.GetPosition(mapControl);
            PointD mapPoint = mapControl.ToMapPoint(scrPoint);
            location.Value = "(" + mapPoint.X.ToString() + ", " + mapPoint.Y.ToString() + ")";
            if (groupMap.Visibility == Visibility.Visible)
            {
                mapping.SetBmp(mapControl.globalbmp);
            }
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (btnZoomIn.IsChecked == true)
            {
                btnPan.IsChecked = false;
                btnSelect.IsChecked = false;
                btnzoomOut.IsChecked = false;
                btnEdit.IsChecked = false;
                mapControl.ZoomIn();
                //mapControl.MapMode = Mode.ZoomIn;
                //mapControl.Cursor = ((TextBlock)Resources["ZoomIn"]).Cursor;
            }
            else
            {
                mapControl.ReturnToNone();
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
                mapControl.ZoomOut();
                //mapControl.Cursor = ((TextBlock)Resources["ZoomOut"]).Cursor;
            }
            else
            {
                mapControl.ReturnToNone();
            }
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            //Layer layer = mapControl.Map.GetLayer(treeView_selectedindex());
            //InputBox dialog = new InputBox(layer.Name);
            //if (dialog.ShowDialog() == true)
            //{
            //    layer.Name = dialog.Text;
            //    ((LayerModel)treeView.SelectedItem).Name = dialog.Text;
            //}
        }

        private void treeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (treeView_selectedindex() != -1)
            //{
            //    curLayer = mapControl.Map.GetLayer(treeView_selectedindex());
            //}
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (btnSelect.IsChecked == true)
            {
                btnPan.IsChecked = false;
                btnZoomIn.IsChecked = false;
                btnEdit.IsChecked = false;
                btnzoomOut.IsChecked = false;
                mapControl.SelectFeatures();
                //mapControl.MapMode = Mode.Select;
                //mapControl.Cursor = Cursors.Cross;
            }
            else
            {
                mapControl.ReturnToNone();
            }
        }

        //Import shp file
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog dialog = new OpenFileDialog();
            //dialog.Filter = "Shapefile File (.shp)|*.shp";
            //if (dialog.ShowDialog() == true)
            //{
            //    string path = dialog.FileName;
            //    if (path == null || path == "") return;
            //    string stylepath = path.Split('.')[0] + ".sld";
            //    string layername = mapControl.mapcontent.addLayer(path, stylepath);
            //    mappro.dic_datapath.Add(layername, path);
            //    mappro.dic_stylepath.Add(layername, stylepath);
            //    mapControl.SetDefaultoffsetandDisplayScale(mapControl.mapcontent);
            //    ShowTreeView(mapControl.mapcontent);
            //    mapControl.mapcontrol_refresh();
            //    //Layer newLayer = Importer.readshpfile(path);
            //    //newLayer.Filename = path;
            //    //newLayer.Name = System.IO.Path.GetFileNameWithoutExtension(path);
            //    //mapControl.Map.Add(newLayer);
            //    //treeView.Items.Add(new LayerModel(newLayer.Name));
            //    //mapControl.ScaleToLayer(newLayer);
            //    //this.Title = "Mercury GIS - " + System.IO.Path.GetFileNameWithoutExtension(path);
            //    //curLayer = newLayer;
            //}
        }

        private void btnWholeScreen_Click(object sender, RoutedEventArgs e)
        {
            mapControl.SetDefaultoffsetandDisplayScale(mapControl.mapcontent);
            mapControl.mapcontrol_refresh();
            //mapControl.ScaleToLayer(curLayer);
        }

        private void treeView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (treeView_selectedindex() != -1)
            //{
            //    curLayer = mapControl.Map.GetLayer(treeView_selectedindex());
            //}
        }


        private void btnAddPointLayer_Click(object sender, RoutedEventArgs e)
        {
            InputBox dialog = new InputBox();
            if (dialog.ShowDialog() == true)
            {
                mapControl.mapcontent.CreateLayer(dialog.Text, OSGeo.OGR.wkbGeometryType.wkbPoint, dialog.Text, "D:\\test.sld");
                mappro.dic_datapath.Add(dialog.Text, dialog.Text);
                mappro.dic_stylepath.Add(dialog.Text, "d:\\test.sld");

                mapControl.SetDefaultoffsetandDisplayScale(mapControl.mapcontent);
                ShowTreeView(mapControl.mapcontent);
                mapControl.mapcontrol_refresh();
                //PointLayer layer = new PointLayer();
                //layer.Name = dialog.Text;
                //mapControl.Map.Add(layer);
                //treeView.Items.Add(new LayerModel(layer.Name));
                //curLayer = layer;
            }

        }

        private void btnAddLineLayer_Click(object sender, RoutedEventArgs e)
        {
            InputBox dialog = new InputBox();
            if (dialog.ShowDialog() == true)
            {
                mapControl.mapcontent.CreateLayer(dialog.Text, OSGeo.OGR.wkbGeometryType.wkbLineString, dialog.Text, "d:\\test.sld");
                mappro.dic_datapath.Add(dialog.Text, dialog.Text);
                mappro.dic_stylepath.Add(dialog.Text, "d:\\test.sld");

                mapControl.SetDefaultoffsetandDisplayScale(mapControl.mapcontent);
                ShowTreeView(mapControl.mapcontent);
                mapControl.mapcontrol_refresh();
                //LineLayer layer = new LineLayer();
                //layer.Name = dialog.Text;
                //mapControl.Map.Add(layer);
                //treeView.Items.Add(new LayerModel(layer.Name));
                //curLayer = layer;
            }
        }

        private void btnAddPolygonLayer_Click(object sender, RoutedEventArgs e)
        {
            InputBox dialog = new InputBox();
            if (dialog.ShowDialog() == true)
            {
                mapControl.mapcontent.CreateLayer(dialog.Text, OSGeo.OGR.wkbGeometryType.wkbPolygon, dialog.Text, "d:\\test.sld");
                mappro.dic_datapath.Add(dialog.Text, dialog.Text);
                mappro.dic_stylepath.Add(dialog.Text, "d:\\test.sld");

                mapControl.SetDefaultoffsetandDisplayScale(mapControl.mapcontent);
                ShowTreeView(mapControl.mapcontent);
                mapControl.mapcontrol_refresh();
                //PolygonLayer layer = new PolygonLayer();
                //layer.Name = dialog.Text;
                //mapControl.Map.Add(layer);
                //treeView.Items.Add(new LayerModel(layer.Name));
                //curLayer = layer;
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
                mapControl.EndEdit();
                if (gdb.SaveToFile(mapControl.editmanager.layername))
                {
                    MessageBox.Show("编辑的数据已保存到文件");
                }
                //mapControl.MapMode = Mode.None;
                //mapControl.Cursor = Cursors.Arrow;
                //if (curLayer.Filename != null)
                //{
                //    curLayer.SaveLayer(curLayer.Filename.Substring(0, curLayer.Filename.Length - 4));

                //}
            }
            else if(result == MessageBoxResult.No)
            {
                groupEdit.Visibility = Visibility.Collapsed;
                TabStart.IsSelected = true;
                btnEdit.IsChecked = false;
                mapControl.EndEdit();
                //mapControl.MapMode = Mode.None;
                //mapControl.Cursor = Cursors.Arrow;
                //string path = curLayer.Filename;
                //Layer newLayer = Importer.readshpfile(path);
                //newLayer.Filename = path;
                ////curLayer = newLayer;
                //mapControl.Map.SetLayer(curLayer, newLayer);
                //curLayer = newLayer;
                //mapControl.ScaleToLayer(newLayer);
            }
        }

        private void btnAddElement_Click(object sender, RoutedEventArgs e)
        {
            //mapControl.CurLayer = curLayer;
            //mapControl.MapMode = Mode.EditAdd;
            //mapControl.Cursor = Cursors.Cross;
        }

        private void btnMapping_Click(object sender, RoutedEventArgs e)
        {

            mapping_view.IsEnabled = true;
            mapping_view.IsSelected = true;
            groupMap.Visibility = Visibility.Visible;
            TabStart.IsEnabled = false;
            TabMap.IsSelected = true;
            mapping.SetData(mapControl.mapcontent, mapControl.globalbmp);
            mapControl.Pan();
            //SaveFileDialog dialog = new SaveFileDialog();
            //dialog.Filter = "PNG File (.png)|*.png";
            //if (dialog.ShowDialog() == true)
            //{
            //    string path = dialog.FileName;
            //    //RenderTargetBitmap bmp = new RenderTargetBitmap((int)mapControl.RenderSize.Width, (int)mapControl.RenderSize.Height, 96, 96, PixelFormats.Pbgra32);
            //    //bmp.Render(mapControl);
            //    Rect bounds = VisualTreeHelper.GetDescendantBounds(mapControl);
            //    double dpi = 96d;


            //    RenderTargetBitmap bmp = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);


            //    DrawingVisual dv = new DrawingVisual();
            //    using (DrawingContext dc = dv.RenderOpen())
            //    {
            //        VisualBrush vb = new VisualBrush(mapControl);
            //        dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            //    }

            //    bmp.Render(dv);
            //    BitmapEncoder encoder = new PngBitmapEncoder();
            //    encoder.Frames.Add(BitmapFrame.Create(bmp));
            //    using (Stream stream = File.Create(path))
            //    {
            //        encoder.Save(stream);
            //    }
            //}
            

        }

        //文本注记
        private void btnMapLable_Click(object sender, RoutedEventArgs e)
        {
            GisSmartTools.Support.Layer curLayer = mapControl.focuslayer;
            if (curLayer == null) curLayer = mapControl.mapcontent.layerlist[0];
            SelectField form = new SelectField(curLayer.featuresource.schema.fields.Keys.ToList());
            if (form.ShowDialog() == true)
            {
                GisSmartTools.Support.Style style = curLayer.style;
                PortableFontDesc font = new PortableFontDesc();
                if (form.font != null)
                {
                    double size = form.font.Size;
                    string fontname = form.font.Family.Source;
                    bool italic = false;
                    bool bold = false;
                    if (form.font.Style != FontStyles.Normal)
                    {
                        italic = true;
                    }
                    if (form.font.Weight != FontWeights.Normal)
                    {
                        bold = true;
                    }
                    font = new PortableFontDesc(name: fontname, emsize: (int)size, isbold: bold, isitalic: italic, cleartype: false);
                }

                foreach (var rule in style.rulelist)
                {
                    textsymbolizer sym = (textsymbolizer)rule.textsymbolizer;
                    sym.visible = form.Checked;
                    sym.attributename = form.field;
                    if (form.font != null)
                    {
                        sym.color = form.font.BrushColor.Color;
                        sym.font = font;
                    }
                }
                if (style.rulelist.Count == 0)
                {
                    var rule = style.defaultRule;
                    textsymbolizer sym = (textsymbolizer)rule.textsymbolizer;
                    sym.visible = form.Checked;
                    sym.attributename = form.field;
                    if (form.font != null)
                    {
                        sym.color = form.font.BrushColor.Color;
                        sym.font = font;
                    }
                }
                mapControl.mapcontrol_refresh();
                //curLayer.IsLabelShown = true;
                //curLayer.LabelField = form.field;
                //mapControl.Refresh();
            }
        }

        private void btnQueryByAttri_Click(object sender, RoutedEventArgs e)
        {
            QueryAttribute form = new QueryAttribute(mapControl);
            form.Show();
        }

        private void btnQueryByLocat_Click(object sender, RoutedEventArgs e)
        {
            mapControl.Identify();
        }

        private void mapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (mapControl.MapMode == Mode.Select)
            //{
            //    mapControl.Cursor = Cursors.Arrow;
            //    QueryResult form = new QueryResult(mapControl);
            //    form.Show();
            //}
        }

        private void btnSymbolize_Click(object sender, RoutedEventArgs e)
        {
            Symbolize form = new Symbolize(mapControl);
            form.Show();
        }

        //调整图层顺序
        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            int id = treeView_selectedindex();
            if (id > 0)
            {
                LayerModel temp = (LayerModel)treeView.Items[id];
                treeView.Items.RemoveAt(id);
                treeView.Items.Insert(id - 1, temp);
                mapControl.mapcontent.MoveLayerTo(temp.Name, id - 1);
                mapControl.mapcontrol_refresh();
                //mapControl.Map.MoveUpOneStep(id);
                //mapControl.Refresh();
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            int id = treeView_selectedindex();
            if (id < treeView.Items.Count - 1)
            {
                LayerModel temp = (LayerModel)treeView.Items[id];
                treeView.Items.RemoveAt(id);
                treeView.Items.Insert(id + 1, temp);
                mapControl.mapcontent.MoveLayerTo(temp.Name, id + 1);
                mapControl.mapcontrol_refresh();
                //mapControl.Map.MoveDownOneStep(id);
                //mapControl.Refresh();
            }
        }

        private void RibbonWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = row.ActualHeight;
            double width = this.Width - treeView.RenderSize.Width;
            mapControl.Height = height;
            mapControl.Width = width;
        }

        private int treeView_selectedindex()
        {
            var item = treeView.SelectedItem;
            return treeView.Items.IndexOf(item);
        }

        private void New_MouseDown(Object sender, RoutedEventArgs e)
        {
            NewMap dialog = new NewMap();
            if (dialog.ShowDialog() == true)
            {
                string name = dialog.mapName;
                //创建工程对象
                mappro = MapProject.CreateMapProject(name);
                mappro.server = dialog.server;
                mappro.port = dialog.port;
                mappro.username = dialog.username;
                mappro.password = dialog.password;
                mappro.database = dialog.database;
                //创建mapcontent对象
                mapcontent mapcontent = mapcontent.createnewmapcontentofnolayer(name, dialog.server, dialog.port, dialog.username, dialog.password, dialog.database);
                gdb = mapcontent.gdb;
                mapControl.mapcontent = mapcontent;
                mapControl.mapcontrol_refresh();
                this.Title = "Mercury GIS - " + name;
                //刷新图层树
                //ShowTreeView(mapcontent);
            }
        }

        private void ShowTreeView(mapcontent mapcontent)
        {
            //TreeNode mapnode = new TreeNode(mapcontent.name);
            //mapnode.Checked = true;
            List<GisSmartTools.Support.Layer> layerlist = mapcontent.layerlist;
            LayerModel[] nodes = new LayerModel[layerlist.Count];
            //TreeNode[] childnodes = new TreeNode[layerlist.Count];

            treeView.Items.Clear();
            for (int i = 0; i < layerlist.Count; i++)
            {
                nodes[i] = new LayerModel(layerlist[i].Layername);
                nodes[i].PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnCheckItem);
                //nodes[i].Nodes.Add(GetTreeViewStyle(layerlist[i].style));
                if (layerlist[i].visible) nodes[i].IsChecked = true;
                else nodes[i].IsChecked = false;
                

                if (layerlist[i].visible) nodes[i].IsChecked = true;
                treeView.Items.Add(nodes[i]);
            }
            
            //treeView.AddRange(nodes);
            //treeView1.Nodes.Clear();
            //treeView1.Nodes.Add(mapnode);
            //treeView1.ExpandAll();
        }

        private void btnUndo_Click(Object sender, RoutedEventArgs e)
        {
            mapControl.OperationUndo();
        }

        private void btnRedo_Click(Object sender, RoutedEventArgs e)
        {
            mapControl.OperationRedo();
        }

        private void btnImportShp_Click(Object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Shapefile File (.shp)|*.shp";
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                if (path == null || path == "") return;
                string stylepath = path.Split('.')[0] + ".sld";
                string layername = System.IO.Path.GetFileNameWithoutExtension(path);
                string guid = GenerateGUID();
                mapControl.mapcontent.addLayer(path, stylepath, guid, layername);
                mappro.dic_datapath.Add(layername, guid);
                mappro.dic_stylepath.Add(layername, stylepath);
                mapControl.SetDefaultoffsetandDisplayScale(mapControl.mapcontent);
                ShowTreeView(mapControl.mapcontent);
                mapControl.mapcontrol_refresh();
            }
        }

        private string GenerateGUID()
        {
            Guid id = Guid.NewGuid();
            return ("g" + id).Replace("-", "");
        }

        private void treeView_SelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            int id = treeView_selectedindex();
            if (id >= 0)
            {
                LayerModel temp = (LayerModel)treeView.Items[id];
                var layer = mapControl.mapcontent.GetLayerByName(temp.Name);
                mapControl.focuslayer = layer;
            }
        }

        private void btnProjection_Click(Object sender, RoutedEventArgs e)
        {
            if (btnProjection.IsChecked == true)
            {

                mapControl.mapcontent.srs.srid = 5847;
                mapControl.SetDefaultoffsetandDisplayScale(mapControl.mapcontent);
                mapControl.mapcontrol_refresh();
            }
            else
            {
                mapControl.mapcontent.srs.srid = 0;
                mapControl.SetDefaultoffsetandDisplayScale(mapControl.mapcontent);
                mapControl.mapcontrol_refresh();
            }
        }

        private void btnTitle_Click(Object sender, RoutedEventArgs e)
        {
            if (btnTitle.IsChecked == true)
            {
                mapping.IsTitle = true;
            }
            else
            {
                mapping.IsTitle = false;
            }
            mapping.Refresh();
        }

        private void btnLegend_Click(Object sender, RoutedEventArgs e)
        {
            mapping.IsLegend = (bool)btnLegend.IsChecked;
            mapping.Refresh();
        }

        private void btnEditTitle_Click(Object sender, RoutedEventArgs e)
        {
            Title dialog = new Title(mapping.title.text, mapping.GetTitleFont());
            if (dialog.ShowDialog() == true)
            {
                string title = dialog.title;
                FontInfo info = dialog.font;
                mapping.title.text = title;
                mapping.SetTitleFont(info);
            }
        }

        private void btnEditLegend_Click(Object sender, RoutedEventArgs e)
        {
            
        }

        private void btnSaveMap_Click(Object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PNG File (.png)|*.png";
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                if (mapping.Save(path))
                {
                    MessageBox.Show("保存成功");
                }
                else
                {
                    MessageBox.Show("保存失败！");
                }
            }
        }

        private void btnReturn_Click(Object sender, RoutedEventArgs e)
        {
            mapping_view.IsEnabled = false;
            editing_view.IsSelected = true;
            groupMap.Visibility = Visibility.Collapsed;
            TabStart.IsEnabled = true;
            TabStart.IsSelected = true;
            //mapping.SetData(mapControl.mapcontent, mapControl.globalbmp);
            //mapControl.Pan();
        }

        private void mapping_view_IsActiveChanged(Object sender, EventArgs e)
        {
            if (mapping_view.IsActive && groupMap.Visibility == Visibility.Visible)
            {
                mapping.SetBmp(mapControl.globalbmp);
            }
        }

        private void btnEditLegendTitle_Click(Object sender, RoutedEventArgs e)
        {
            var font = mapping.GetLegendTitleFont();
            ColorFontDialog dialog = new ColorFontDialog();
            dialog.Font = font;

            if (dialog.ShowDialog() == true)
            {
                font = dialog.Font;
                if (font != null)
                {
                    mapping.SetLegendTitleFont(font);
                }
            }
        }

        private void btnEditLegendText_Click(Object sender, RoutedEventArgs e)
        {
            var font = mapping.GetLegendTextFont();
            ColorFontDialog dialog = new ColorFontDialog();
            dialog.Font = font;

            if (dialog.ShowDialog() == true)
            {
                font = dialog.Font;
                if (font != null)
                {
                    mapping.SetLegendTextFont(font);
                }
            }
        }

        private void btnWFS_Click(Object sender, RoutedEventArgs e)
        {
            if (wfsserver != null)
            {
                wfsserver.Stop();
                wfsserver.Dispose();
            }
            wfsserver = new WfsServer(9999);
            wfsserver.mapcontent = mapControl.mapcontent;
            //wfsserver.Stop();
            wfsserver.Run();
            System.Diagnostics.Process.Start("http://localhost:9999/MercuryGIS/wfs.html");
        }

        private void Export_Click(Object sender, RoutedEventArgs e)
        {
            int id = treeView_selectedindex();
            if (id >= 0)
            {
                LayerModel temp = (LayerModel)treeView.Items[id];
                var layer = mapControl.mapcontent.GetLayerByName(temp.Name);
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Shapefile (.shp)|*.shp";
                if (dialog.ShowDialog() == true)
                {
                    string path = dialog.FileName;
                    string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                    FeatureSource fs = layer.featuresource;
                    if (SHPGeoDataBase.SaveFeatureSource2File(fs, path, filename))
                    {
                        MessageBox.Show("保存成功");
                    }
                    else
                    {
                        MessageBox.Show("保存失败！");
                    }
                }
                //mapControl.Map.MoveUpOneStep(id);
                //mapControl.Refresh();
            }
        }

        private void mapControl_MouseWheel(Object sender, MouseWheelEventArgs e)
        {
            if (groupMap.Visibility == Visibility.Visible)
            {
                mapping.SetBmp(mapControl.globalbmp);
            }
        }
    }

    //class LayerModel
    //{
    //    public bool isChecked { get; set; }
    //    public string name { get; set; }
    //}
}
