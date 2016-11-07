using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GisSmartTools;
using OSGeo.OGR;
using GisSmartTools.Geometry;

namespace GisSmartTools.Data
{

    public abstract class GeoDatabase
    {
        public Dictionary<String, FeatureSource> featureSources;     //数据
        public Dictionary<String, String> pathRef;  //表名和文件路径的对应,key为layername，value为filepath

        public abstract String AddFeatureSource(String path);
        public abstract FeatureSource CreateFeatureSource(Schema schema, String path);
        public abstract Boolean SaveAll();
        public abstract Boolean SaveToFile(string name);
    }
    /**
     * SHPGeoDataBase
     * 空间数据库类
     * 对包含空间数据和属性数据元信息的整体封装，提供存取基本地理数据的功能
     * 整存整取
     * 暂时只支持一个shapefile中只有一个图层的情况
     * 必须通过GetSHPGeoDataBase（）获取实例，不能直接新建实例
     * */
    public class SHPGeoDataBase:GeoDatabase
    {
        
        private Dictionary<String,DataSource> iDses;             //datasources of input files

        private SHPGeoDataBase()
        {
            featureSources = new Dictionary<String, FeatureSource>();
            pathRef = new Dictionary<String, String>();
            iDses = new Dictionary<string, DataSource>();
        }

        //给定路径，创建SHPGeoDataBase实例,并从数据库中读取所有图层的schema,必须不包含同名表，否则返回null
        public static SHPGeoDataBase GetGeoDataBase(List<String> pathList)
        {
            SHPGeoDataBase resultDB = new SHPGeoDataBase();
            foreach (String path in pathList)
            {
                FeatureSource tmpSrc = SHPGeoDataBase.GetFeaturesource(resultDB,path);
                if (resultDB.pathRef.ContainsKey(tmpSrc.schema.name)) return null;
                resultDB.pathRef.Add(tmpSrc.schema.name, path);
                resultDB.featureSources.Add(tmpSrc.schema.name, tmpSrc);
            }
            return resultDB;
        }

        //读取一个shapefile文件并将其中的第一个图层添加到当前SHPGeoDataBase
        public override String AddFeatureSource(String path)
        {
            FeatureSource tmpSrc = SHPGeoDataBase.GetFeaturesource(this,path);
            if (this.pathRef.ContainsKey(tmpSrc.schema.name)) return null;
            this.pathRef.Add(tmpSrc.schema.name, path);
            this.featureSources.Add(tmpSrc.schema.name, tmpSrc);
            return tmpSrc.schema.name;
        }

        //在内存中新建一个（显式调用save系列函数后才保存在磁盘）
        public override FeatureSource CreateFeatureSource(Schema schema, String path)
        {
            if (this.pathRef.ContainsKey(schema.name)) return null;
            FeatureSource resultSrc = new FeatureSource(schema, new FeatureCollection());
            this.featureSources.Add(schema.name, resultSrc);
            this.pathRef.Add(schema.name, path);
            return resultSrc;
        }


        #region FunctionsRead
        //从文件中读取数据
        private static FeatureSource GetFeaturesource(SHPGeoDataBase thisdb, String path)
        {
            OSGeo.OGR.DataSource ds = GetOGRDataSource(thisdb,path);

            //init schema
            OSGeo.OGR.Layer layer = ds.GetLayerByIndex(0);
            OSGeo.OGR.FeatureDefn fd = layer.GetLayerDefn();
            Int32 sIndex = path.LastIndexOf("\\");
            string sname = path.Substring(sIndex + 1, path.Length - sIndex - 1 - 4);
            int fieldcount = fd.GetFieldCount();
            Dictionary<String, FieldDefn> tmpField = new Dictionary<string,FieldDefn>();
            for (int i = 0; i < fieldcount; ++i)
            {
                OSGeo.OGR.FieldDefn field = fd.GetFieldDefn(i);
                tmpField.Add(field.GetName(), field);
            }
            GisSmartTools.RS.ReferenceSystem rfs;
            OSGeo.OSR.SpatialReference osrrf=layer.GetSpatialRef();
            if (osrrf.IsProjected() != 0) rfs = new GisSmartTools.RS.SRS(osrrf);
            else rfs = new GisSmartTools.RS.GRS(osrrf);
            Schema rs = new Schema(sname,layer.GetGeomType(),rfs, tmpField);
            //get featurecollection
            FeatureCollection fc = GetFeatureCollection(layer,rs);

            //close file
            //ds.Dispose();

            return new FeatureSource(rs, fc);
        }

        //get a ogr datasorce by path string
        private static OSGeo.OGR.DataSource GetOGRDataSource(SHPGeoDataBase thisdb, String path)
        {

            OSGeo.OGR.Ogr.RegisterAll();
            //to support chinese path
            //OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
            //to support chinese field name
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");
            OSGeo.OGR.Driver dr = OSGeo.OGR.Ogr.GetDriverByName("ESRI shapefile");
            if (dr == null) return null;
            //第二参数为0表明只读模式
            OSGeo.OGR.DataSource ds = dr.Open(path, 0);
            thisdb.iDses.Add(path, ds);
            return ds;
        }

        //read data really
        private static FeatureCollection GetFeatureCollection(OSGeo.OGR.Layer layer,GisSmartTools.Data.Schema schema)
        {
            FeatureCollection fc = new FeatureCollection();
            OSGeo.OGR.Feature feature;
            OSGeo.OGR.FeatureDefn posDefn = layer.GetLayerDefn();
            int fieldCount = posDefn.GetFieldCount();
            int iField = 0;

            //read all the features in layer
            while ((feature = layer.GetNextFeature()) != null)
            {
                OSGeo.OGR.Geometry geometry = feature.GetGeometryRef();
                GisSmartTools.Data.Feature feat = new Feature(feature.GetFID(), schema,null);
                //get and save feature's attributes
                for (iField = 0; iField < fieldCount; iField++)
                {
                    OSGeo.OGR.FieldDefn oField = posDefn.GetFieldDefn(iField);
                    Object objAttr ;
                    FieldType ft = oField.GetFieldType();
                    switch(ft)
                    {
                        case FieldType.OFTString:
                            objAttr=feature.GetFieldAsString(iField);
                            break;
                        case FieldType.OFTInteger:
                            objAttr=feature.GetFieldAsInteger(iField);
                            break;
                        case FieldType.OFTReal:
                            objAttr=feature.GetFieldAsDouble(iField);
                            break;
                        case FieldType.OFTWideString:
                            objAttr=feature.GetFieldAsString(iField);
                            break;
                        case FieldType.OFTStringList:
                            objAttr=feature.GetFieldAsStringList(iField);
                            break;
                        case FieldType.OFTIntegerList:
                            int outCount;
                            objAttr=feature.GetFieldAsIntegerList(iField,out outCount);
                            break;
                        case FieldType.OFTRealList:
                            int outCount2;
                            objAttr=feature.GetFieldAsDoubleList(iField,out outCount2);
                            break;
                        case FieldType.OFTWideStringList:
                            objAttr=feature.GetFieldAsStringList(iField);
                            break;
                        default:
                            objAttr=feature.GetFieldAsString(iField);
                            break;
                    }
                    feat.AddAttribute(oField.GetName(), objAttr);
                }

                //get geometry
                if (geometry != null)
                {
                    OSGeo.OGR.wkbGeometryType goetype = geometry.GetGeometryType();
                    switch (goetype)    //according to the type, we operate differently
                    {
                        case wkbGeometryType.wkbPoint:
                            if (geometry != null && goetype == wkbGeometryType.wkbPoint)
                            {
                                feat.geometry = new PointD(geometry.GetX(0), geometry.GetY(0));
                            }
                            //add feature to featureCollection
                            fc.InsertFeature(feat);
                            break;
                        case wkbGeometryType.wkbLineString:
                            List<PointD> pointlist = new List<PointD>();
                            int pointcount = geometry.GetPointCount();
                            for (int k = 0; k < pointcount; k++)
                            {
                                pointlist.Add(new PointD(geometry.GetX(k), geometry.GetY(k)));
                            }
                            feat.geometry = new SimplePolyline(pointlist);
                            //add feature to featureCollection
                            fc.InsertFeature(feat);
                            break;
                        case wkbGeometryType.wkbMultiLineString:
                            feat.geometry = GetPolyline(geometry);
                            fc.InsertFeature(feat);
                            break;
                        case wkbGeometryType.wkbPolygon:
                            feat.geometry = GetSimplePolygon(geometry);
                            //add feature to featureCollection
                            fc.InsertFeature(feat);
                            break;
                        case wkbGeometryType.wkbMultiPolygon:
                            feat.geometry = GetPolygon(geometry);
                            fc.InsertFeature(feat);
                            break;
                        default:
                            //we don't support another geometry types,
                            //so we don't load those records to avoid errors
                            break;
                    }
                }
                else Console.WriteLine("kkkkkkkkkkkkkkkkkkkkkkkkkkkkkk");
            }

            return fc;
        }

        /// <summary>
        /// 获取简单多边形
        /// </summary>
        /// <param name="geo"></param>
        /// <returns></returns>
        private static SimplePolygon GetSimplePolygon(OSGeo.OGR.Geometry geometry)
        {
            List<SimplePolyline> rings = new List<SimplePolyline>();
            int ringCount = geometry.GetGeometryCount();//子图形对象数目
            for (int i = 0; i < ringCount; ++i)
            {
                List<PointD> pointlist = new List<PointD>();
                //获取第i个子对象
                OSGeo.OGR.Geometry ring = geometry.GetGeometryRef(i);
                int pointcount2 = ring.GetPointCount();
                for (int k = 0; k < pointcount2; k++)
                {
                    pointlist.Add(new PointD(ring.GetX(k), ring.GetY(k)));
                }
                rings.Add(new SimplePolyline(pointlist));
            }
            return new SimplePolygon(rings);
        }

        /// <summary>
        /// 获取复杂多边形
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private static Polygon GetPolygon(OSGeo.OGR.Geometry geometry)
        {
            List<SimplePolygon> spolygons = new List<SimplePolygon>();
            int gonCount = geometry.GetGeometryCount();
            for (int i = 0; i < gonCount; ++i)
            {
                //获取第i个子对象
                OSGeo.OGR.Geometry gon = geometry.GetGeometryRef(i);
                spolygons.Add(GetSimplePolygon(gon));
            }
            return new Polygon(spolygons);
        }


        /// <summary>
        /// 获取复杂线
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private static Polyline GetPolyline(OSGeo.OGR.Geometry geometry)
        {
            List<SimplePolyline> lines = new List<SimplePolyline>();
            int lineCount = geometry.GetGeometryCount();    //子图形对象数目
            for (int i = 0; i < lineCount; ++i)
            {
                List<PointD> line = new List<PointD>();
                //获取第i个子对象
                OSGeo.OGR.Geometry sline = geometry.GetGeometryRef(i);
                int pointcount2 = sline.GetPointCount();
                for (int j = 0; j < pointcount2; ++j)
                {
                    line.Add(new PointD(sline.GetX(j), sline.GetY(j)));
                }
                lines.Add(new SimplePolyline(line));
            }
            return new Polyline(lines);
        }


        #endregion FunctionsRead



        #region savedata

        //将内存中所有图层的数据完全保存到数据库中
        public override Boolean SaveAll()
        {
            Boolean flag = true;
            for (int i = 0; i < this.pathRef.Count; ++i)
            {
                flag=flag&&SaveToFile(this.pathRef.ElementAt(i).Key);
            }
            return flag;
        }
        /// <summary>
        /// 给定一个shapefile featuresource存储到对应的path
        /// </summary>
        /// <param name="featuresource"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Boolean SaveFeatureSource2File(FeatureSource featuresource,string path, string filename)
        {
            Ogr.RegisterAll();
            //to support chinese path
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            //to support chinese field name
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");
            string strDriverName = "ESRI Shapefile";
            Driver oDriver = Ogr.GetDriverByName(strDriverName);
            if (oDriver == null)
            {
                //说明driver不可用
                return false;
            }
            String pathPrefix = System.IO.Path.GetDirectoryName(path);
            String pathPostfix = System.Guid.NewGuid().ToString();
            if (File.Exists(pathPrefix + "\\" + pathPostfix)) File.Delete(pathPrefix + "\\" + pathPostfix);

            //create a datasource, if successfully, a shp file would be created
            DataSource oDs = oDriver.CreateDataSource(pathPrefix + "\\" + pathPostfix, null);
            Schema tmpSchema = featuresource.schema;
            Layer layer = oDs.CreateLayer(filename, tmpSchema.rs.spetialReference, tmpSchema.geometryType, null);

            //Layer layer = oDs.CreateLayer(tmpSchema.name, tmpSchema.rs.spetialReference, tmpSchema.geometryType, null);

            //insert all the fields to layer
            //use index instead of field name to avoid the error caused by chinese words
            int fieldcount = tmpSchema.fields.Count;
            for (int i = 0; i < fieldcount; ++i)
            {
                layer.CreateField(tmpSchema.fields.ElementAt(i).Value, 1);
            }
            FeatureDefn fdef = layer.GetLayerDefn();

            FeatureCollection fc = featuresource.features;
            int fcount = fc.count;
            for (int i = 0; i < fcount; ++i)
            {
                GisSmartTools.Data.Feature tmpFeature=fc.featureList[i];
                if (!tmpFeature.visible) continue;
                //create a new feature
                OSGeo.OGR.Feature newFeature = new OSGeo.OGR.Feature(fdef);

                //set attribute
                newFeature.SetFID(tmpFeature.featureID);
                for (int j = 0; j < fieldcount; ++j)
                {
                    String fieldname = tmpSchema.fields.ElementAt(j).Key;
                    FieldType ft = tmpSchema.fields[fieldname].GetFieldType();
                    try{
                        switch (ft)
                        {

                            case FieldType.OFTString:
                                newFeature.SetField(j, (String)tmpFeature.attributes[fieldname]);
                                break;
                            case FieldType.OFTInteger:
                                newFeature.SetField(j, (int)tmpFeature.attributes[fieldname]);
                                break;
                            case FieldType.OFTReal:
                                newFeature.SetField(j, (double)tmpFeature.attributes[fieldname]);
                                break;
                            case FieldType.OFTWideString:
                                newFeature.SetField(j, (String)tmpFeature.attributes[fieldname]);
                                break;
                            default:
                                newFeature.SetField(j, (String)tmpFeature.attributes[fieldname]);
                                break;
                        }
                    }catch(Exception e)
                    {

                    } 
                }

                //get geometry
                OSGeo.OGR.wkbGeometryType featType = tmpFeature.geometry.geometryType;
                OSGeo.OGR.Geometry geo = new OSGeo.OGR.Geometry(featType);
                if(geo!=null)
                switch (featType)
                {
                    case OSGeo.OGR.wkbGeometryType.wkbPoint:
                        GisSmartTools.Geometry.PointD tmpPoint = (GisSmartTools.Geometry.PointD)tmpFeature.geometry;
                        geo.AddPoint(tmpPoint.X, tmpPoint.Y,0.00);
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbLineString:
                        GisSmartTools.Geometry.SimplePolyline tmpLine = (GisSmartTools.Geometry.SimplePolyline)tmpFeature.geometry;
                        foreach (GisSmartTools.Geometry.PointD po in tmpLine.points)
                        {
                            geo.AddPoint(po.X, po.Y, 0.00);
                        }
                        break;
                    case wkbGeometryType.wkbMultiLineString:
                        GisSmartTools.Geometry.Polyline lines = (GisSmartTools.Geometry.Polyline)tmpFeature.geometry;
                        foreach (SimplePolyline line in lines.childPolylines)
                        {
                            OSGeo.OGR.Geometry tmpgeo = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLineString);
                            foreach (GisSmartTools.Geometry.PointD point in line.points)
                            {
                                tmpgeo.AddPoint(point.X, point.Y, 0.00);
                            }
                            geo.AddGeometryDirectly(tmpgeo);
                        }
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                        GisSmartTools.Geometry.SimplePolygon gon = (GisSmartTools.Geometry.SimplePolygon)tmpFeature.geometry;
                        foreach (SimplePolyline ring7 in gon.rings)
                        {
                            OSGeo.OGR.Geometry tmpgeo = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                            foreach (GisSmartTools.Geometry.PointD point in ring7.points)
                            {
                                tmpgeo.AddPoint(point.X, point.Y, 0.00);
                            }
                            geo.AddGeometryDirectly(tmpgeo);
                        }
                            break;
                    case wkbGeometryType.wkbMultiPolygon:
                        GisSmartTools.Geometry.Polygon gons = (GisSmartTools.Geometry.Polygon)tmpFeature.geometry;
                        foreach (GisSmartTools.Geometry.SimplePolygon cgon in gons.childPolygons)
                        {
                            OSGeo.OGR.Geometry geogon = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);
                            foreach (GisSmartTools.Geometry.SimplePolyline ring6 in cgon.rings)
                            {
                                OSGeo.OGR.Geometry geoline = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                                foreach (GisSmartTools.Geometry.PointD point6 in ring6.points)
                                {
                                    geoline.AddPoint(point6.X, point6.Y, 0.00);
                                }
                                geogon.AddGeometryDirectly(geoline);
                            }
                            geo.AddGeometryDirectly(geogon);
                        }
                        break;
                    default:
                        break;

                }

                //set feature
                newFeature.SetGeometry(geo);
                //add to layer
                layer.CreateFeature(newFeature);
            }

            //call Dispose method to save to file
            oDs.Dispose();
            //enumerate all the files in the temp file directory and move them to the output directory
            string[] tmpName = System.IO.Directory.GetFiles(pathPrefix + "\\" + pathPostfix);
            foreach (string file in tmpName)
            {
                string decFile = pathPrefix + "\\" + Path.GetFileName(file);
                if (File.Exists(decFile)) File.Delete(decFile);
                File.Move(file, decFile);
            }
            Directory.Delete(pathPrefix + "\\" + pathPostfix, true);
            return true;
        }

        //将内存中一个图层的数据另存为在文件中
        private Boolean SaveAsToFile(String name, String path)
        {
            Ogr.RegisterAll();
            //to support chinese path
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            //to support chinese field name
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");
            string strDriverName = "ESRI Shapefile";
            Driver oDriver = Ogr.GetDriverByName(strDriverName);
            if (oDriver == null)
            {
                //说明driver不可用
                return false;
            }
            String pathPrefix = System.IO.Path.GetDirectoryName(path);
            String pathPostfix = System.Guid.NewGuid().ToString();
            if (File.Exists(pathPrefix + "\\" + pathPostfix)) File.Delete(pathPrefix + "\\" + pathPostfix);

            //create a datasource, if successfully, a shp file would be created
            DataSource oDs = oDriver.CreateDataSource(pathPrefix + "\\" + pathPostfix, null);
            Schema tmpSchema = this.featureSources[name].schema;
            Layer layer = oDs.CreateLayer(name, tmpSchema.rs.spetialReference, tmpSchema.geometryType, null);

            //insert all the fields to layer
            //use index instead of field name to avoid the error caused by chinese words
            int fieldcount = tmpSchema.fields.Count;
            for (int i = 0; i < fieldcount; ++i)
            {
                layer.CreateField(tmpSchema.fields.ElementAt(i).Value, 1);
            }
            FeatureDefn fdef = layer.GetLayerDefn();

            FeatureCollection fc = this.featureSources[name].features;
            int fcount = fc.count;
            for (int i = 0; i < fcount; ++i)
            {
                GisSmartTools.Data.Feature tmpFeature=fc.featureList[i];
                if (!tmpFeature.visible) continue;
                //create a new feature
                OSGeo.OGR.Feature newFeature = new OSGeo.OGR.Feature(fdef);

                //set attribute
                newFeature.SetFID(tmpFeature.featureID);
                for (int j = 0; j < fieldcount; ++j)
                {
                    String fieldname = tmpSchema.fields.ElementAt(j).Key;
                    FieldType ft = tmpSchema.fields[fieldname].GetFieldType();
                    try{
                        switch (ft)
                        {

                            case FieldType.OFTString:
                                newFeature.SetField(j, (String)tmpFeature.attributes[fieldname]);
                                break;
                            case FieldType.OFTInteger:
                                newFeature.SetField(j, (int)tmpFeature.attributes[fieldname]);
                                break;
                            case FieldType.OFTReal:
                                newFeature.SetField(j, (double)tmpFeature.attributes[fieldname]);
                                break;
                            case FieldType.OFTWideString:
                                newFeature.SetField(j, (String)tmpFeature.attributes[fieldname]);
                                break;
                            default:
                                newFeature.SetField(j, (String)tmpFeature.attributes[fieldname]);
                                break;
                        }
                    }catch(Exception e)
                    {

                    } 
                }

                //get geometry
                OSGeo.OGR.wkbGeometryType featType = tmpFeature.geometry.geometryType;
                OSGeo.OGR.Geometry geo = new OSGeo.OGR.Geometry(featType);
                if(geo!=null)
                switch (featType)
                {
                    case OSGeo.OGR.wkbGeometryType.wkbPoint:
                        GisSmartTools.Geometry.PointD tmpPoint = (GisSmartTools.Geometry.PointD)tmpFeature.geometry;
                        geo.AddPoint(tmpPoint.X, tmpPoint.Y,0.00);
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbLineString:
                        GisSmartTools.Geometry.SimplePolyline tmpLine = (GisSmartTools.Geometry.SimplePolyline)tmpFeature.geometry;
                        foreach (GisSmartTools.Geometry.PointD po in tmpLine.points)
                        {
                            geo.AddPoint(po.X, po.Y, 0.00);
                        }
                        break;
                    case wkbGeometryType.wkbMultiLineString:
                        GisSmartTools.Geometry.Polyline lines = (GisSmartTools.Geometry.Polyline)tmpFeature.geometry;
                        foreach (SimplePolyline line in lines.childPolylines)
                        {
                            OSGeo.OGR.Geometry tmpgeo = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLineString);
                            foreach (GisSmartTools.Geometry.PointD point in line.points)
                            {
                                tmpgeo.AddPoint(point.X, point.Y, 0.00);
                            }
                            geo.AddGeometryDirectly(tmpgeo);
                        }
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                        GisSmartTools.Geometry.SimplePolygon gon = (GisSmartTools.Geometry.SimplePolygon)tmpFeature.geometry;
                        foreach (SimplePolyline ring7 in gon.rings)
                        {
                            OSGeo.OGR.Geometry tmpgeo = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                            foreach (GisSmartTools.Geometry.PointD point in ring7.points)
                            {
                                tmpgeo.AddPoint(point.X, point.Y, 0.00);
                            }
                            geo.AddGeometryDirectly(tmpgeo);
                        }
                            break;
                    case wkbGeometryType.wkbMultiPolygon:
                        GisSmartTools.Geometry.Polygon gons = (GisSmartTools.Geometry.Polygon)tmpFeature.geometry;
                        foreach (GisSmartTools.Geometry.SimplePolygon cgon in gons.childPolygons)
                        {
                            OSGeo.OGR.Geometry geogon = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbPolygon);
                            foreach (GisSmartTools.Geometry.SimplePolyline ring6 in cgon.rings)
                            {
                                OSGeo.OGR.Geometry geoline = new OSGeo.OGR.Geometry(OSGeo.OGR.wkbGeometryType.wkbLinearRing);
                                foreach (GisSmartTools.Geometry.PointD point6 in ring6.points)
                                {
                                    geoline.AddPoint(point6.X, point6.Y, 0.00);
                                }
                                geogon.AddGeometryDirectly(geoline);
                            }
                            geo.AddGeometryDirectly(geogon);
                        }
                        break;
                    default:
                        break;

                }

                //set feature
                newFeature.SetGeometry(geo);
                //add to layer
                layer.CreateFeature(newFeature);
            }

            //call Dispose method to save to file
            oDs.Dispose();
            DataSource tds;
            if (this.iDses.TryGetValue(path, out tds))
            {
                tds.Dispose();
                this.iDses.Remove(path);
            }

            //enumerate all the files in the temp file directory and move them to the output directory
            string[] tmpName = System.IO.Directory.GetFiles(pathPrefix + "\\" + pathPostfix);
            foreach (string file in tmpName)
            {
                string decFile = pathPrefix + "\\" + Path.GetFileName(file);
                if (File.Exists(decFile)) File.Delete(decFile);
                File.Move(file, decFile);
            }
            Directory.Delete(pathPrefix + "\\" + pathPostfix, true);
            return true;

        }

        //将内存中一个图层的数据保存在文件中
        public override Boolean SaveToFile(String name)
        {
            String path;
            if(!(this.pathRef.TryGetValue(name,out path)))return false;
            return SaveAsToFile(name,path);
        }


        #endregion savedata

        #region events
        //事件
        public delegate void TableappendedHandle(object sender, string[] schemaName);
        public event TableappendedHandle Tableappended;
        private void RaiseTableappended(object sender, string[] schemaName)
        {
            if (Tableappended != null)
            {
                Tableappended(sender, schemaName);
            }
        }

        public delegate void TableDeletedHandle(object sender, string[] schemaName);
        public event TableDeletedHandle TableDeleted;
        private void RaiseTableDeleted(object sender, string[] schemaName)
        {
            if (TableDeleted!= null)
            {
                TableDeleted(sender, schemaName);
            }
        }
        #endregion events

    }
}
