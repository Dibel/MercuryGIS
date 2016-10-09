using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;
using GisSmartTools.Geometry;
using GisSmartTools.Support;

namespace GisSmartTools
{
    /// <summary>
    /// FeatureBoard
    /// 要素画板
    /// 用于编辑要素时。此类一个对应一个图层
    /// 对于单多边形，仅支持单环
    /// </summary>
    public class FeatureBoard
    {
        #region 属性
        private Layer editedLayer;  //接受编辑的图层
        private int minFid; //此次编辑的可用FID段的下限
        private int maxFid; //当前已用FID的最大值
        private FeatureCollection _features;   //编辑的要素，包含所有添加的、要被替换的、要被删除的要素
        private List<PointD> _inputTmp;   //输入时直接输入点到此列表,与Featuresource同坐标系统
        private List<int> _deletedFID;    //删除列表
        #endregion


        public FeatureBoard(Layer layer)
        {
            this.editedLayer = layer;
            this._features = new FeatureCollection();
            this._inputTmp = new List<PointD>();
            this._deletedFID = new List<int>();
            this.minFid = this.GetMaxFID(this.editedLayer.featuresource.features.featureList)+1;
            this.maxFid = this.minFid - 1;
        }

        #region 索引器
        /// <summary>
        /// 被编辑的要素
        /// </summary>
        public FeatureCollection editedFeatrues
        {
            get { return this._features; }
        }
        /// <summary>
        /// 正在被编辑的图形
        /// </summary>
        public List<PointD> geometryBeingEdited
        {
            get { return this._inputTmp; }
        }
        /// <summary>
        /// 此次编辑中被删除的要素列表
        /// </summary>
        public List<int> deletedFID
        {
            get { return this._deletedFID; }
        }

        #endregion

        #region 对外接口

        /// <summary>
        /// 向当前正在输入的图形中添加点
        /// </summary>
        /// <param name="point"></param>
        public void AddPoint(PointD point)
        {
            this._inputTmp.Add(point);
        }

        /// <summary>
        /// 向当前正输入的图形中添加点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void AddPoint(double x, double y)
        {
            this._inputTmp.Add(new PointD(x, y));
        }

        /// <summary>
        /// 完成编辑并将当前编辑的图形作为要素加入列表
        /// </summary>
        public void Complete()
        {
            Feature tmpFeat = new Feature(this.maxFid + 1, this.editedLayer.featuresource.schema, null);
            switch (this.editedLayer.featuresource.schema.geometryType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    if(this._inputTmp.Count>0)tmpFeat.SetGeometry(this._inputTmp.ElementAt(0));
                    this._features.InsertFeature(tmpFeat);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    if (this._inputTmp.Count < 2) break;
                    SimplePolyline tmpSL = new SimplePolyline(this._inputTmp);
                    tmpFeat.SetGeometry(tmpSL);
                    this._features.InsertFeature(tmpFeat);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    if (this._inputTmp.Count < 3) break;
                    List<SimplePolyline> tmpSLL = new List<SimplePolyline>();
                    tmpSLL.Add(new SimplePolyline(this._inputTmp));
                    SimplePolygon tmpSG = new SimplePolygon(tmpSLL);
                    tmpFeat.SetGeometry(tmpSG);
                    this._features.InsertFeature(tmpFeat);
                    break;
            }
            //prepare for another input
            this._inputTmp = new List<PointD>();
        }

        /// <summary>
        /// 完成编辑并将当前图形作为一个子图形加入一个要素,只能加入此次编辑的要素中
        /// 这个过程中，被加入要素如果是SimplePolygon/line类型则会被转为MultiPolygon/line类型
        /// 如果被加入的是点要素，则只完成并新建一个点要素
        /// </summary>
        /// <param name="FID"></param>
        public void CompleteAndAddTo(int FID)
        {
            Feature tmpFeat;
            switch (this.editedLayer.featuresource.schema.geometryType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    this.Complete();
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    if (this._inputTmp.Count < 2) { this._inputTmp = new List<PointD>(); break; }
                    SimplePolyline tmpSL = new SimplePolyline(this._inputTmp);
                    tmpFeat = this._features.GetFeatureByID(FID);
                    //if this id do not correspond to a feature, we'll give up the inputted data
                    if (tmpFeat == null)
                    {
                        this._inputTmp = new List<PointD>();
                        return;
                    }
                    Polyline tmpML;
                    List<SimplePolyline> tmpSLs = new List<SimplePolyline>();
                    switch (tmpFeat.geometry.geometryType)
                    {
                        case OSGeo.OGR.wkbGeometryType.wkbLineString:
                            tmpSLs.Add((SimplePolyline)tmpFeat.geometry);
                            break;
                        case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                            foreach (SimplePolyline pl in ((Polyline)tmpFeat.geometry).childPolylines)
                            {
                                tmpSLs.Add(pl);
                            }
                            break;
                    }
                    tmpSLs.Add(tmpSL);
                    tmpML = new Polyline(tmpSLs);
                    tmpFeat.SetGeometry(tmpML);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    if (this._inputTmp.Count < 3) { this._inputTmp = new List<PointD>(); break; }
                    List<SimplePolyline> tmpSLL = new List<SimplePolyline>();
                    tmpSLL.Add(new SimplePolyline(this._inputTmp));
                    SimplePolygon tmpSG = new SimplePolygon(tmpSLL);
                    tmpFeat = this._features.GetFeatureByID(FID);
                    //if this id do not correspond to a feature, we'll give up the inputted data
                    if (tmpFeat == null)
                    {
                        this._inputTmp = new List<PointD>();
                        return;
                    }
                    Polygon tmpMG;
                    List<SimplePolygon> tmpSGs = new List<SimplePolygon>();
                    switch (tmpFeat.geometry.geometryType)
                    {
                        case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                            tmpSGs.Add((SimplePolygon)tmpFeat.geometry);
                            break;
                        case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
                            foreach (SimplePolygon pg in ((Polygon)tmpFeat.geometry).childPolygons)
                            {
                                tmpSGs.Add(pg);
                            }
                            break;
                    }
                    tmpSGs.Add(tmpSG);
                    tmpMG = new Polygon(tmpSGs);
                    tmpFeat.SetGeometry(tmpMG);
                    //prepare for another input
                    this._inputTmp = new List<PointD>();
                    break;
            }
        }

        /// <summary>
        /// 按id删除要素
        /// 要素可以是新编辑的，也可以是原本就存在于图层的
        /// 当且仅当新编辑的要素和图层原有要素中均不含有该FID的要素时返回false
        /// </summary>
        /// <param name="FID"></param>
        public bool DeleteByFID(int FID)
        {
            if (FID < minFid)   //说明是原图层中的要素
            {
                //check whether the feature with this fid had been deleted
                foreach (int id in this._deletedFID)
                {
                    if (id == FID) return true;
                }

                //get the feature with this fid from the layer being edited
                Feature tmpFeat = this.editedLayer.featuresource.features.GetFeatureByID(FID);
                if (tmpFeat == null) return false;
                this._features.InsertFeature(new Feature(tmpFeat.featureID,tmpFeat.schema,null));
                this._deletedFID.Add(FID);
                return true;
            }
            else    //说明是新编辑的
            {
                if (0 == this._features.DeleteFeaturebyID(FID)) return false;
                return true;
            }
        }

        /// <summary>
        /// 撤销，撤销对上一次更改的要素的所有更改
        /// 例如，输入了十个点组成一个多边形，执行undo会撤销所有这十个点
        /// </summary>
        public void UnDo()
        {
            if (this._inputTmp.Count != 0)  //when editing, clear the last point inputed
            {
                this._inputTmp.RemoveAt(this._inputTmp.Count-1);
                return;
            }
            if (this.editedFeatrues.count <= 0) return;
            //if the geometry of the last edited feature is empty, which means the last step is deleting a feature
            if (this._features.featureList.Count < 1) return;
            Feature lastF = this._features.featureList.ElementAt(this._features.featureList.Count - 1);
            //delete the record in _deletedFID
            if (lastF.geometry == null)
            {
                this._deletedFID.RemoveAll(
                    delegate(int id)
                    {
                        return id == lastF.featureID;
                    }
                    );
            }
            //delete the last edited feature to undo
            this._features.featureList.RemoveAt(this._features.featureList.Count - 1);
        }


        public void EndEdit()
        {
            if (this._inputTmp.Count != 0) this.Complete();
            foreach (Feature feat in this._features.featureList)
            {
                if (feat.featureID < this.minFid)
                {
                    if (feat.geometry != null)
                    {
                        this.editedLayer.featuresource.features.DeleteFeaturebyID(feat.featureID);
                        this.editedLayer.featuresource.features.InsertFeature(feat);
                    }
                    else
                    {
                        bool flag = false;
                        foreach (int i in this._deletedFID)
                        {
                            if (i == feat.featureID)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                        {
                            this.editedLayer.featuresource.features.DeleteFeaturebyID(feat.featureID);
                        }
                    }
                }
                else
                {
                    this.editedLayer.featuresource.features.InsertFeature(feat);
                }
            }
        }


        #endregion 对外接口

        #region 私有方法

        private int GetMaxFID(List<Feature> features)
        {
            int res=0;
            foreach (Feature feat in features)
            {
                if (feat.featureID > res) res = feat.featureID;
            }
            return res;
        }


        #endregion 私有方法

    }
}
