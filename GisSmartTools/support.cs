using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using GisSmartTools.Filter;
using GisSmartTools.Data;
using GisSmartTools.RS;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using GisSmartTools.Geometry;
using System.IO;
using System.Collections;
using OSGeo.OGR;
using GisSmartTools.Test;
using System.Windows.Media.Imaging;

namespace GisSmartTools.Support
{

    #region 样式渲染类定义
    public enum SymbolizerType
    {
        POINT = 1,
        LINE = 2,
        POLYGON = 3,
        TEXT = 4,
    }
    public enum StyleType
    {
        SIMPLESTYLE = 1,
        UNIQUESTYLE = 2,
        RANKSTYLE  = 3,
        CUSTOMSTYLE = 4,
    }
   
    [Serializable]
    public class Symbolizer
    {
        public SymbolizerType sign;

    }


    [Serializable]
    public class pointsymbolizer : Symbolizer
    {
        // public int sign = Symbolizer.pointsymbolizer;
        public string label = "";
        public bool visible = true;
        public PointStyle pointstyle = PointStyle.CIRCLE_FILL;
        public float size = 10;
        public double offset_x = 0;
        public double offset_y = 0;
        [NonSerialized()]
        public Color color;

        public pointsymbolizer()
        {
            this.sign = SymbolizerType.POINT;
            color = Utils.GetDifferentColorbyList();
        }
        public static pointsymbolizer createDefaultpointsymbolizer()
        {
            pointsymbolizer psym = new pointsymbolizer();
            psym.sign = SymbolizerType.POINT;
            psym.label = "default";
            return psym;
        }
    }

    [Serializable]
    public class linesymbolizer : Symbolizer
    {
        // public int sign = Symbolizer.linesymbolizer;
        public string label = "";
        public bool visible = true;
        public System.Drawing.Drawing2D.DashStyle linestyle = System.Drawing.Drawing2D.DashStyle.Solid;
        public float width = 2;
        [NonSerialized()]
        public Color color;
        public linesymbolizer()
        {
            this.sign = SymbolizerType.LINE;
            color = Utils.GetDifferentColorbyList();
        }
        public static linesymbolizer createdefaultlinesymbolizer()
        {
            linesymbolizer lsym = new linesymbolizer();
            lsym.sign = SymbolizerType.LINE;
            lsym.label = "default";
            return lsym;
        }
    }

    [Serializable]
    public class polygonsymbolizer : Symbolizer
    {
        //  public int sign = Symbolizer.polygonsymbolizer;
        public string label = "";
        public bool visible = true;
        [NonSerialized()]
        public Color strokecolor;
        [NonSerialized()]
        public Color fillcolor;
        public float strokewidth = 2;
        public polygonsymbolizer()
        {
            this.sign = SymbolizerType.POLYGON;
            strokecolor = Utils.GetDifferentColorbyList();
            fillcolor = Utils.GetDifferentColorbyList();
        }
        public static polygonsymbolizer createdefalutpolygonsymbolizer()
        {
            polygonsymbolizer ponsym = new polygonsymbolizer();
            ponsym.sign = SymbolizerType.POLYGON;
            ponsym.label = "default";

            return ponsym;
        }
    }


    [Serializable]
    public class textsymbolizer : Symbolizer
    {
       /* //  public int sign = Symbolizer.textsymbolizer;
        //public Font font = new Font("宋体", 12, FontStyle.Regular);
        
        public FontFamily fontfamily;
        
        //public string fontfamily = "宋体";
        public float fontsize;
        //public int fontsize = 12;
        //public string fontstyle = "Rugular";
        public FontStyle fontstyle = FontStyle.Regular;*/
        public string attributename = "FeatureID";
        public bool visible = false;
        [NonSerialized()]
        public PortableFontDesc font;
        [NonSerialized()]
        public Color color = Colors.Black;
        public float offset_x = 0;
        public float offset_y = 5;
        public textsymbolizer()
        {
            this.sign = SymbolizerType.TEXT;
            font = new PortableFontDesc(emsize: 14);
            color = Colors.Black;
        }
        public static textsymbolizer createdefaulttextsymbolizer()
        {
            textsymbolizer textsym = new textsymbolizer();
            textsym.sign = SymbolizerType.TEXT;
            return textsym;
        }
    }


    [Serializable]
    public class RenderRule
    {
        public Filter.Filter filter;
        public Symbolizer geometrysymbolizer = null;
        public Symbolizer textsymbolizer = null;

        public RenderRule()
        {

        }
        public RenderRule(Filter.Filter filter, Symbolizer geometrysymbolizer, Symbolizer textsymbolizer)
        {
            this.filter = filter;
            this.geometrysymbolizer = geometrysymbolizer;
            this.textsymbolizer = textsymbolizer;
        }

        public static RenderRule createDefaultRule(SymbolizerType symbolizertype)
        {
            switch (symbolizertype)
            {
                case SymbolizerType.POINT:
                    return new RenderRule(null, pointsymbolizer.createDefaultpointsymbolizer(),new textsymbolizer());
                case SymbolizerType.LINE:
                    return new RenderRule(null, linesymbolizer.createdefaultlinesymbolizer(), new textsymbolizer());
                case SymbolizerType.POLYGON:
                    return new RenderRule(null, polygonsymbolizer.createdefalutpolygonsymbolizer(), new textsymbolizer());
            }
            return null;
        }
        public static RenderRule createDefaultRule(SymbolizerType symbolizertype,Color color)
        {
            switch (symbolizertype)
            {
                case SymbolizerType.POINT:
                    pointsymbolizer pointsym = pointsymbolizer.createDefaultpointsymbolizer();
                    pointsym.color = color;
                    return new RenderRule(null, pointsym, new textsymbolizer());
                case SymbolizerType.LINE:
                    linesymbolizer linesym = linesymbolizer.createdefaultlinesymbolizer();
                    linesym.color = color;
                    return new RenderRule(null, linesym, new textsymbolizer());
                case SymbolizerType.POLYGON:
                    polygonsymbolizer polygonsym = polygonsymbolizer.createdefalutpolygonsymbolizer();
                    polygonsym.fillcolor = color;
                    polygonsym.strokecolor = color;
                    return new RenderRule(null, polygonsym, new textsymbolizer());
            }
            return null;
        }
    }

    [Serializable]
    public class Style
    {
        public string name;
        public StyleType styletype;
        public List<RenderRule> rulelist;
        public RenderRule defaultRule;
        public Style()
        {

        }
        public Style(string name,StyleType type, RenderRule defaultRule)
        {
            this.name = name;
            this.styletype = type;
            rulelist = new List<RenderRule>();
            this.defaultRule = defaultRule;
        }

        public bool addRule(RenderRule rule)
        {
            rulelist.Add(rule);
            return true;
        }

        public void setDefalutRule(RenderRule defaultRule)
        {
            this.defaultRule = defaultRule;
        }

        
        public static Style parseStyleFile(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            Style style = (Style)formatter.Deserialize(fs);
            fs.Close();
            return style;

        }

        public bool toStyleFile(string path)
        {
            MemoryStream mo = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(mo, this);
            }
            catch (Exception)
            {
                throw;
            }
            byte[] data = mo.ToArray();
            FileStream filestream = new FileStream(path, FileMode.OpenOrCreate);
            filestream.Write(data, 0, data.Length);
            filestream.Flush(); filestream.Close();
            return true;
        }

        public void SetTextSymblizerInvisible()
        {
            if(defaultRule.textsymbolizer!=null)
            {
                ((textsymbolizer)defaultRule.textsymbolizer).visible = false;
            }
            for(int i =0;i<rulelist.Count;i++)
            {
                if(rulelist[i].textsymbolizer!=null)
                ((textsymbolizer)rulelist[i].textsymbolizer).visible = false;
            }
        }
        public void SetTextSymbolizerVisible()
        {
            if (defaultRule.textsymbolizer != null)
            {
                ((textsymbolizer)defaultRule.textsymbolizer).visible = true;
            }
            for (int i = 0; i < rulelist.Count; i++)
            {
                if (rulelist[i].textsymbolizer != null)
                    ((textsymbolizer)rulelist[i].textsymbolizer).visible = true;
            }
        }
        public static Style createCustomStyle(FeatureSource featuresource)
        {
            RenderRule rule = null;
            switch (featuresource.schema.geometryType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    rule = RenderRule.createDefaultRule(SymbolizerType.POINT);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    rule = RenderRule.createDefaultRule(SymbolizerType.LINE);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    rule = RenderRule.createDefaultRule(SymbolizerType.POLYGON);
                    break;
            }
            Style simplestyle = new Style("simplestyle", StyleType.CUSTOMSTYLE, rule);
            return simplestyle;
        }
        public static Style createRankStyle(FeatureSource featuresource,string attributename,int num , Color startcolor,Color endcolor)
        {
            Schema schema = featuresource.schema;
            SymbolizerType sign = SymbolizerType.POINT;
            switch(schema.geometryType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                   sign = SymbolizerType.POINT;
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    sign = SymbolizerType.LINE;
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    sign = SymbolizerType.POLYGON;
                    break;
            }
            FieldDefn fielddefn = null;
            if(schema.fields.TryGetValue(attributename,out fielddefn)||attributename.Equals("FeatureID"))
            {
                FieldType type = FieldType.OFTString;
               if(!attributename.Equals("FeatureID"))  type = fielddefn.GetFieldType();
               if (type == FieldType.OFTInteger || type == FieldType.OFTReal || attributename.Equals("FeatureID"))
                {
                    double maxvalue = double.MinValue;
                    double minvalue = double.MaxValue;
                    foreach(GisSmartTools.Data.Feature feature in featuresource.features.featureList)
                    {
                        double value = 0;
                        if (type == FieldType.OFTInteger || attributename.Equals("FeatureID")) value = (int)feature.GetArrtributeByName(attributename);
                        if (type == FieldType.OFTReal) value = (double)feature.GetArrtributeByName(attributename);
                        if (value > maxvalue) maxvalue = value;
                        if (value < minvalue) minvalue = value;
                    }
                    if (num == 0) return null;
                    double x = (maxvalue - minvalue) / num;
                    //逐个创建对应的rule
                    Color[] colorarray = Utils.GetLinearColorList(startcolor,endcolor,num);
                    double temp_minvalue = minvalue;
                    double temp_maxvalue = minvalue + x;
                    List<RenderRule> rulelist = new List<RenderRule>();
                   // Printer.printnode("min=" + minvalue + " max=" + maxvalue, "");

                    for(int i = 0;i<num;i++)
                    {
                        RenderRule rule = RenderRule.createDefaultRule(sign,colorarray[i]);
                        Filter_Larger larger = new Filter_Larger(attributename, temp_minvalue);
                        Filter_Less less = new Filter_Less(attributename, temp_maxvalue);
                        List<Filter.Filter> filterlist = new List<Filter.Filter>();
                        filterlist.Add(larger); filterlist.Add(less);
                        Filter_And andfilter = new Filter_And(filterlist);
                        rule.filter = andfilter;
                        rulelist.Add(rule);
                       // Printer.printnode("temp_" + i, "min=" + temp_minvalue + " max=" + temp_maxvalue);
                        temp_minvalue += x; temp_maxvalue += x;
                    }
                    Style rankstyle = new Style("RankStyle", StyleType.RANKSTYLE, RenderRule.createDefaultRule(sign,startcolor));
                    rankstyle.rulelist = rulelist;
                    Printer.printnode("rulist,count=", "" + rulelist.Count);
                    return rankstyle;
                }
            }
            return null;


        }
        public static Style createUniqueValueStyle(FeatureSource featuresource, string arrtributename)
        {
            //arrtributename = featuresource.schema.fields.Keys.First();//////????????????????????????????????????//
            Schema schema = featuresource.schema;
            SymbolizerType sign = SymbolizerType.POINT;
            switch(schema.geometryType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                   sign = SymbolizerType.POINT;
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    sign = SymbolizerType.LINE;
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    sign = SymbolizerType.POLYGON;
                    break;
            }
            //获取attributename 所对应的所有不同属性值
            HashSet<object> set = new HashSet<object>();
            List<GisSmartTools.Data.Feature> featurelist = featuresource.features.featureList;
            foreach(GisSmartTools.Data.Feature feature in featurelist)
            {
                set.Add(feature.GetArrtributeByName(arrtributename));
            }
            List<RenderRule> rulelist = new List<RenderRule>();
            foreach(object value in set)
            {
                RenderRule rule = RenderRule.createDefaultRule(sign);
                rule.filter = new Filter_Equals(arrtributename, value);
                rulelist.Add(rule);
            }
                 Style style = new Style("uniqueValueStyle",StyleType.UNIQUESTYLE,RenderRule.createDefaultRule(sign));
                 style.rulelist = rulelist;
                 return style;
        }


        public static Style createSimpleStyle(FeatureSource featuresource)
        {
            RenderRule rule = null;
            switch(featuresource.schema.geometryType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    rule = RenderRule.createDefaultRule(SymbolizerType.POINT);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    rule = RenderRule.createDefaultRule(SymbolizerType.LINE);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    rule = RenderRule.createDefaultRule(SymbolizerType.POLYGON);
                    break;
            }
            Style simplestyle = new Style("simplestyle",StyleType.SIMPLESTYLE ,rule);
            return simplestyle;
        }
    }


    #endregion

    #region layer mapcontent类定义

    public class Layer
    {
        public string descriptor;
        public FeatureSource featuresource;
        private string layername;
        public Style style;
        public bool sectable = true;
        public bool visible = true;

        public String Layername
        {
            get
            {
                return layername;
            }

            set
            {
                layername = value;
            }
        }

        public Layer(string layername, FeatureSource featuresource, Style style)
        {
            this.layername = layername;
            this.featuresource = featuresource;
            this.style = style;
        }

        public GisSmartTools.Geometry.Rectangle getEnvelopofLayer()
        {
            FeatureCollection collection = featuresource.features;
            return collection.getEnvelop();
        }

        /// <summary>
        ///  该函数负责将给定featuresource配以style并返回layer
        /// </summary>
        /// <param name="gdb"></param>
        /// <param name="datapath"></param>
        /// <param name="stylepath"></param>
        /// <param name="layername"></param>
        /// <returns></returns>
        public static Layer LoadLayer(FeatureSource featuresource,string layername, string stylepath)
        {
            Style style = null;
            try
            {
                if(stylepath!=null) style = Style.parseStyleFile(stylepath);
            }catch(Exception e)
            {

            }
            if (style == null) style = Style.createSimpleStyle(featuresource);
            Layer layer = new Layer(layername, featuresource, style);
            return layer;
        }

        
    }




  public  class mapcontent
    {
        public string name;
        public List<Layer> layerlist;
        public GeoDatabase gdb;
        public SRS srs;
        


        public bool CreateLayer(string name, OSGeo.OGR.wkbGeometryType type,string save_featuresourcepath,string save_stylepath)
        {
            FeatureSource featuresource = this.gdb.CreateFeatureSource(Schema.CreateSchema(name,type),save_featuresourcepath);
            Layer layer = Layer.LoadLayer(featuresource, name, save_stylepath);
            layerlist.Insert(0,layer);
            return true;
            
        }
      /// <summary>
      /// 用户向已有gdb中添加一个新的图层,该图层必须已存在shp文件
      /// 向该mapcontent中添加一个layer
      /// </summary>
      /// <param name="gdb"></param>
      /// <param name="layerpath"></param>
      /// <param name="stylepath"></param>
      /// <returns></returns>
        public string addLayer(string layerpath,string stylepath, string guid, string layername)
        {
            if (layerpath == null) return null;
            FeatureSource featuresource = null;
            PGGeoDatabase gdb = (PGGeoDatabase)this.gdb;
            //string layername =
            gdb.AddSHPFeatureSource(layerpath, guid);
            if (!this.gdb.featureSources.TryGetValue(guid, out featuresource))
            {
                return null;
            }
            Style style = null;
            try
            {
                if (stylepath != null) style = Style.parseStyleFile(stylepath);
            }
            catch (Exception e)
            {

            }
            if (style == null) style = Style.createSimpleStyle(featuresource);
            Layer layer = new Layer(layername, featuresource, style);
            this.layerlist.Insert(0, layer);
            return layername;
        }

        public bool RemoveLayer(string layername)
        {
            if(layername!=null)
            {
                for(int i=0;i<layerlist.Count;i++)
                {
                    if(layerlist[i].Layername.Equals(layername))
                    {
                        layerlist.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        public void MoveLayerTo(string layername, int targetindex)
        {
            Layer layer = GetLayerByName(layername);
            layerlist.Remove(layer);
            layerlist.Insert(targetindex, layer);

        }

        public void MoveLayerToBottom(string layername)
        {
            MoveLayerTo(layername, layerlist.Count-1);
        }


        public void MoveLayerToTop(string layername)
        {
            MoveLayerTo(layername, 0);
        }

        public void changeLayerOrder(List<string> layernamelist)
        {
            List<Layer> newlayerlist = new List<Layer>();
            for(int i=0;i<layernamelist.Count;i++)
            {
                newlayerlist.Add(this.GetLayerByName(layernamelist[i]));
            }
            this.layerlist = newlayerlist;

        }

        public Layer GetLayerByName(string layername)
        {
            if (layername != null)
            {
                for (int i = 0; i < layerlist.Count; i++)
                {
                    if (layerlist[i].Layername.Equals(layername))
                    {
                        return layerlist[i];
                    }
                }
            }
            return null;
        }

        public mapcontent(string mapname,GeoDatabase gdb,List<Layer> layerlist)
        {
            this.name = mapname;
            this.layerlist = layerlist;
            this.gdb = gdb;
            //???SRS怎么办
            
        }

         public static mapcontent createnewmapcontentofnolayer(string mapname, string server, string port, string username, string password, string database)
        {
            GeoDatabase gdb = PGGeoDatabase.GetGeoDatabase(server, port, username, password, database, new List<string>());
            List<Layer> layerlist = new List<Layer>();
            mapcontent map = new mapcontent(mapname, gdb,layerlist);
            return map;

        }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="gdb"></param>
      /// <param name="mapname"></param>
      /// <param name="stylepath">mapproject必须给相同数量的stylepath，如果没有value应该为null,key为layername</param>
      /// <returns></returns>
        public static mapcontent LoadMapContent(string mapname,Dictionary<string, string> datasourcepath,Dictionary<string,string> stylepath, string server, string port, string username, string password, string database)
        {
            List<Layer> layerlist = new List<Layer>();
            GeoDatabase gdb = PGGeoDatabase.GetGeoDatabase(server, port, username, password, database, datasourcepath.Values.ToList());
            //Dictionary<String, FeatureSource> dic = gdb.featureSources;
            foreach(string layername in datasourcepath.Keys)
            {
                //获取featuresoucre
                FeatureSource featuresource = null;
                if (!gdb.featureSources.TryGetValue(datasourcepath[layername], out featuresource)) continue;
                
                Layer layer = Layer.LoadLayer(featuresource, layername, stylepath[layername]);
                layerlist.Add(layer);
            }
            mapcontent map = new mapcontent(mapname, gdb,layerlist);
            return map;
        }

        public GisSmartTools.Geometry.Rectangle GetRefernectRectangle()
        {
            if (layerlist.Count == 0) return new Geometry.Rectangle(0, 0, 100,100);
            Geometry.Rectangle rect = new Geometry.Rectangle(Utils.double_max, Utils.double_max, Utils.double_min, Utils.double_min);
            foreach (Layer layer in layerlist)
            {
                Geometry.Rectangle layerrect = layer.getEnvelopofLayer();
               // if(layer.featuresource.schema.rs.spetialReference.)///////////////?????????????此处判断数据坐标系统与map坐标系统是否一致，如果不一致，需要转换
                //layerrect = transform...
                if (layerrect.minX < rect.minX) rect.minX = layerrect.minX;
                if (layerrect.maxX > rect.maxX) rect.maxX = layerrect.maxX;
                if (layerrect.minY < rect.minY) rect.minY = layerrect.minY;
                if (layerrect.maxY > rect.maxY) rect.maxY = layerrect.maxY;
            }
            return rect;
        }
        public bool Savemapcontent(Dictionary<string,string> dict_stylepath)
        {
            ///保存gdb 数据 保存到原来shp文件路径
            gdb.SaveAll();
            ///保存style数据 保存到mappro中存储的路径
            foreach (Layer layer in this.layerlist)
            {
                string name = layer.Layername;
                string stylepath = dict_stylepath[name];
                if (stylepath != null || stylepath != "")
                {
                    layer.style.toStyleFile(stylepath);
                }
            }
            return true;
        }
    }
    #endregion 
}
