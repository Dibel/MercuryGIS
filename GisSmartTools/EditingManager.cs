using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;
using GisSmartTools.Geometry;
using GisSmartTools.Support;
using GisSmartTools.Filter;
using System.Windows.Media;
using GisSmartTools.Topology;
namespace GisSmartTools
{
    public enum EditStatus
    {
        editing = 1,//正处于编辑输入状态
        finished = 2,//正处于完成编辑状态，此时可以进行选择拖动等
        selected = 3,//正处于选中状态，此时可能处于可移动状态和不可移动状态
    }
    public enum selectedstatus
    {
        selectable  = 0,//选中状态时的不可移动状态
        geometrymovable = 1,//选中状态时的可移动状态
        pointmovable = 3,
    }
    public class EditingManager
    {
        public string layername = "";
        public FeatureSource EditingFeaturesource;
        public OSGeo.OGR.wkbGeometryType geometrytype;
        public EditStatus editstatus = EditStatus.finished;
        //public FeatureCollection featurecollection_finishededit;//已经编辑完成的要素集合
        public FeatureCollection selectedFeatureCollection;
       public List<PointD> editingpoints;//正在编辑过程中的点的集合
        public List<List<PointD>> lists;
        public RS.RSTransform rstransform;
        public selectedstatus selectedstatus = selectedstatus.selectable;
        public FeatureCollection copycollection;
        public PointD selectedPoint;
        //渲染方式
        public Color trackingcolor = Colors.Green;
        public Color strokecolor = Colors.Red;
        public Color Fillcolor = Colors.Blue;

        public EditingManager(Layer layer,RS.RSTransform rstransfrom)
        {
            this.layername = layer.layername;
            this.EditingFeaturesource = layer.featuresource;
            //featurecollection_finishededit = new FeatureCollection();
            this.editingpoints = new List<PointD>();
            this.lists = new List<List<PointD>>();

            this.geometrytype =  this.EditingFeaturesource.schema.geometryType;
            this.rstransform = rstransfrom;
        }
        
        public void Addpoint(PointD map_point)
        {
            editingpoints.Add(map_point);
        }

        public void startedit()
        {
            editingpoints = new List<PointD>();
            lists = new List<List<PointD>>();
            this.editstatus = EditStatus.editing;
        }

        public void FinishEdit()
        {
            lists.Add(editingpoints);
            switch (geometrytype)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    foreach(List<PointD> onelist in lists)
                    {
                        for (int i = 0; i < onelist.Count; i++)
                        {
                            Feature pointfeature = new Feature(EditingFeaturesource.GetNextFeatureID(), EditingFeaturesource.schema, onelist[i]);
                            LogItem_CreateFeature logitem_create_point = new LogItem_CreateFeature(this.layername, pointfeature.featureID);
                            Utils.gislog.AddLog(logitem_create_point);
                            EditingFeaturesource.features.InsertFeature(pointfeature);
                        }
                    }
                        break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                     foreach(List<PointD> onelist in lists)
                     {
                         SimplePolyline simplepolyline = new SimplePolyline(onelist);
                         Feature linefeature = new Feature(EditingFeaturesource.GetNextFeatureID(), EditingFeaturesource.schema, simplepolyline);
                         LogItem_CreateFeature logitem_create_line = new LogItem_CreateFeature(this.layername, linefeature.featureID);
                         Utils.gislog.AddLog(logitem_create_line);
                         EditingFeaturesource.features.InsertFeature(linefeature);
                     }
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    List<SimplePolyline> rings = new List<SimplePolyline>();
                    foreach (List<PointD> onelist in lists)
                    {
                        //onelist.Add(onelist.First());
                        SimplePolyline onering = new SimplePolyline(onelist);
                        rings.Add(onering);
                    }
                        SimplePolygon simplepolygon = new SimplePolygon(rings);
                        Feature polygonfeature = new Feature(EditingFeaturesource.GetNextFeatureID(), EditingFeaturesource.schema, simplepolygon);
                        LogItem_CreateFeature logitem_create_polygon = new LogItem_CreateFeature(this.layername, polygonfeature.featureID);
                            Utils.gislog.AddLog(logitem_create_polygon);
                        EditingFeaturesource.features.InsertFeature(polygonfeature);
                    break;
            }
            this.editstatus = EditStatus.finished;
        }

        /// <summary>
        /// 将copycollection插入到featuresource中，插入之前修改其featureID
        /// </summary>
        public void PasteFeatureCollection()
        {
            if(this.copycollection!=null&& this.copycollection.featureList.Count!=0)
            {
                this.SelectedFeaturesPan(30, 30, this.copycollection);
                foreach(Feature feature in copycollection.featureList)
                {
                    feature.featureID = EditingFeaturesource.GetNextFeatureID();
                    this.EditingFeaturesource.features.InsertFeature(feature);
                }
                LogItem_CopyFeatures log_copyitem = new LogItem_CopyFeatures(this.layername, copycollection);
                Utils.gislog.AddLog(log_copyitem);
            }
        }
        public bool finishpart()
        {
            switch(geometrytype)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    lists.Add(editingpoints);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    if (editingpoints.Count < 2) return false;
                    lists.Add(editingpoints);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    if (editingpoints.Count < 3) return false;
                    //editingpoints.Add(editingpoints.First());
                    lists.Add(editingpoints);
                    break;
            }
            editingpoints = new List<PointD>();
            return true;
            
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="offsetx"></param>
        /// <param name="offsety"></param>
        public void SelectedFeaturesPan(double offsetx,double offsety,FeatureCollection collection)
        {
            if (collection == null || collection.featureList.Count == 0) return;
            foreach(Feature feature in collection.featureList)
            {
                Geometry.Geometry geo = feature.geometry;
                switch(geo.geometryType)
                {
                    case OSGeo.OGR.wkbGeometryType.wkbPoint:
                        PointD point = (PointD)geo;
                        point.X -= offsetx;
                        point.Y -= offsety;
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbLineString:
                        SimplePolyline line = (SimplePolyline)geo;
                        foreach (PointD ringpoint in line.points)
                        {
                            ringpoint.X -= offsetx;
                            ringpoint.Y -= offsety;
                        }
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                        SimplePolygon simplegon = (SimplePolygon)geo;
                       foreach(SimplePolyline ring in simplegon.rings)
                       {
                           foreach(PointD ringpoint in ring.points)
                           {
                               ringpoint.X -= offsetx;
                               ringpoint.Y -= offsety;
                           }
                       }
                       break;
                        
                }
            }
        }

        public void DeleteSelectedFeatures()
        {
            if (this.selectedFeatureCollection == null) return;
            LogItem_DeleteFeature log_deleteitem = new LogItem_DeleteFeature(this.layername, this.selectedFeatureCollection);
            Utils.gislog.AddLog(log_deleteitem);
            foreach(Feature feature in this.selectedFeatureCollection.featureList)
            {
                feature.visible = false;
            }
            selectedFeatureCollection = new FeatureCollection(new List<Feature>());
        }
        /// <summary>
        /// 重新输入
        /// </summary>
        public void ResumeCurrentInput()
        {
            editingpoints = new List<PointD>();
        }
        public void ResumeAllInput()
        {
            editingpoints = new List<PointD>();
            lists.Clear();
        }
        public void deletelastinputPoint()
        {
            if(editingpoints.Count>0)
            {
                editingpoints.Remove(editingpoints.Last());
            }
        }
        public bool CheckIsPointSelectedofSelectedFeatureCollection(PointD mouselocation,double interval)
        {
            if(selectedFeatureCollection==null||selectedFeatureCollection.featureList.Count==0) return false;
            foreach(Feature feature in selectedFeatureCollection.featureList)
            {
                switch(feature.geometry.geometryType)
                {
                    case OSGeo.OGR.wkbGeometryType.wkbPoint:
                        if(SpatialTopology.GetDistanceBetweenPoints((PointD)feature.geometry,mouselocation)<interval)
                        {
                            this.selectedPoint = (PointD)feature.geometry;
                            return true;
                        }
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbLineString:
                        SimplePolyline simpleline = (SimplePolyline)feature.geometry;
                        foreach(PointD linepoint in simpleline.points)
                        {
                            if (SpatialTopology.GetDistanceBetweenPoints(linepoint, mouselocation) < interval)
                            {
                                this.selectedPoint = linepoint;
                                return true;
                            }
                        }
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                        SimplePolygon simplepolygon = (SimplePolygon)feature.geometry;
                        foreach(SimplePolyline ring in simplepolygon.rings)
                        {
                            foreach (PointD linepoint in ring.points)
                            {
                                if (SpatialTopology.GetDistanceBetweenPoints(linepoint, mouselocation) < interval)
                                {
                                    this.selectedPoint = linepoint;
                                    return true;
                                }
                            }
                        }
                        break;
                }
            }
            this.selectedPoint = null; return false;
        }

    }
}
