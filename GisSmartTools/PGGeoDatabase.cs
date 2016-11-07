using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.PGIOyichao;
using GisSmartTools.RS;
using OSGeo.OGR;
using OSGeo.GDAL;
using OSGeo.OSR;
namespace GisSmartTools.Data
{

    class PGGeoDatabase:GeoDatabase
    {
       // public Dictionary<String, FeatureSource> featureSources;     //数据
      //  public Dictionary<String, String> pathRef;  //表名和文件路径的对应,key为layername，value为filepath
        public List<string> tablenamesindb;//给定数据库连接参数后，自动载入当前数据库中已有table的名称，但并没有读取其中的数据  这一列表只存储table name，但不确定是否已经读取了其中数据
        private PostGisIO pgio = null;
        private SHPGeoDataBase shpgeodb  = null;
         
        public static string server = "127.0.0.1";
        public static string port = "5432";
        public static string user = "postgres";
        public static string passwd = "1234";
        public static string database = "test";

        private PGGeoDatabase(PostGisIO pgio)
        {
            this.featureSources = new Dictionary<string, FeatureSource>();
            this.pathRef = new Dictionary<string, string>();
            this.tablenamesindb = new List<string>();
            this.pgio = pgio;
            this.tablenamesindb = pgio.GetTableNames();
        }

        public override String AddFeatureSource(String tablename)
        {
            if (this.pathRef.ContainsKey(tablename)) return null;
            FeatureSource featuresource = this.pgio.ReadFromTable(tablename);
            this.pathRef.Add(tablename, tablename);
            this.featureSources.Add(tablename, featuresource);
            return tablename;
        }

        /// <summary>
        /// 创建一个featuresource,但是要保证schema里面的Name和tablename一致
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public override FeatureSource CreateFeatureSource(Schema schema, String path)
        {
            if (this.pathRef.ContainsKey(schema.name)) return null;
            FeatureSource resultSrc = new FeatureSource(schema, new FeatureCollection());
            this.featureSources.Add(schema.name, resultSrc);
            this.pathRef.Add(schema.name, schema.name);
            return resultSrc;
        }


        public override  Boolean SaveAll()
        {
            Boolean flag = true;
            for (int i = 0; i < this.pathRef.Count; ++i)
            {
                flag = flag && SaveToFile(this.pathRef.ElementAt(i).Key);
            }
            return flag;
        }


        //将内存中一个图层的数据保存在文件中
        public override  Boolean SaveToFile(String tablename)
         {
             if (!this.featureSources.ContainsKey(tablename)) return false;
            int srid = 3857;
            FeatureSource featuresource = this.featureSources[tablename];
            OSGeo.OSR.SpatialReference sr = featuresource.schema.rs.spetialReference;
            if(sr!=null)
            {
                srid = 3857;
            }
            return this.pgio.Save2Table(this.featureSources[tablename], srid);

         }
        /// <summary>
        /// 给定shapefile的path，读取数据featuresource并加入到postgisgeodatabase中，返回该featuresource的表名字
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string AddSHPFeatureSource(string path, string guid)
        {
            if(shpgeodb==null)
            {
                shpgeodb = SHPGeoDataBase.GetGeoDataBase(new List<string>());
            }
            string tablename = shpgeodb.AddFeatureSource(path);
            if (this.featureSources.ContainsKey(tablename))
            {
                return null;
            }
            var source = shpgeodb.featureSources[tablename];
            source.schema.name = guid;
            this.featureSources.Add(guid, source);
            this.pathRef.Add(guid, guid);
            return tablename;
        }
        /// <summary>
        /// 存储shp对应的featuresource，可以将postgis原生featuresource转存为shapefile
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public Boolean SaveSHPFeatureSource(string tablename,string path, string filename)
        {
            if(this.featureSources.ContainsKey(tablename))
            {
                return SHPGeoDataBase.SaveFeatureSource2File(this.featureSources[tablename], path, filename);

            }
            return false;
        }

        /// <summary>
        /// 给定数据库连接参数和数据库中需要载入的表名，生成一个PGDataBase对象
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <param name="list_tablename"></param>
        /// <returns></returns>
        public static PGGeoDatabase GetGeoDatabase(string server, string port, string user, string password, string database,List<string> list_tablename)
        {
            PGGeoDatabase pgdatabase = new PGGeoDatabase(new PostGisIO(server, port, user, password, database));
            foreach(string tablename in list_tablename)
            {
                if(!pgdatabase.tablenamesindb.Contains(tablename))
                {
                    continue;
                }
                FeatureSource featuresource = pgdatabase.pgio.ReadFromTable(tablename);
                if(featuresource==null)
                {
                    continue;
                }
                else
                {
                    pgdatabase.featureSources.Add(tablename, featuresource);
                    pgdatabase.pathRef.Add(tablename, tablename);
                }
            }
            return pgdatabase;
        }
        
       
       

    }
}
