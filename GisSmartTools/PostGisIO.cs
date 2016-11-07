using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Data;
using System.Windows.Forms;
using GisSmartTools.RS;
using GisSmartTools.Data;

namespace GisSmartTools.PGIOyichao
{
    public class PostGisIO
    {
        public string server;
        public string port;
        public string user;
        public string password;
        public string database;

        //构造函数，指定连接参数
        public PostGisIO(string server, string port, string user, string password, string database)
        {
            this.server = server;
            this.port = port;
            this.user = user;
            this.password = password;
            this.database = database;
        }

        //指定表名读出成一个FeatureSource
        public FeatureSource ReadFromTable(string table)
        {
            string connStr = "Server="+server+";Port="+port+";UserId="+user+";Password="+password+";Database="+database+";";
            try
            {
                //获得图层类型和参考坐标系
                Dictionary<string, string> dt1 = GetSchemePara(table);
                OSGeo.OGR.wkbGeometryType geometryType = String2Type.String2wkbGeometryType(dt1["type"]);
                ReferenceSystem rs = new SRS(new OSGeo.OSR.SpatialReference(Int2SRText(int.Parse(dt1["srid"]))));

                NpgsqlConnection conn = new NpgsqlConnection(connStr);
                conn.Open();
                IDbCommand dbcmd = conn.CreateCommand();
                dbcmd.CommandText = "SELECT *,ST_AsText(geom) AS geom1 FROM \"" + table + "\"";
                IDataReader dr = dbcmd.ExecuteReader();

                Dictionary<string, OSGeo.OGR.FieldDefn> fields = new Dictionary<string, OSGeo.OGR.FieldDefn>();
                string[] fieldNames = new string[dr.FieldCount - 2];
                for (int i = 0; i < dr.FieldCount-2; i++)
                {
                    OSGeo.OGR.FieldDefn d=GetFieldDefn(dr.GetName(i),dr.GetDataTypeName(i));
                    fields.Add(dr.GetName(i),d);
                    fieldNames[i] = dr.GetName(i);
                }

                Schema schema = new Schema(table, geometryType, rs, fields);

                List<Feature> flst = new List<Feature>();
                while (dr.Read())
                {
                    int featureID = int.Parse(dr[0].ToString());
                    Geometry.Geometry geometry = WKT2Feature.WKT2Geometry(dr[dr.FieldCount - 1].ToString());
                    Dictionary<string, Object> attributes = new Dictionary<string, object>();
                    for (int i = 0; i < dr.FieldCount - 2; i++)
                    {
                        Object o;
                        switch (fields[fieldNames[i]].GetFieldType())
                        {
                            case OSGeo.OGR.FieldType.OFTInteger:
                                o = int.Parse(dr[i].ToString());
                                break;
                            case OSGeo.OGR.FieldType.OFTReal:
                                o = double.Parse(dr[i].ToString());
                                break;
                            default:
                                o = dr[i].ToString();
                                break;
                        }
                        attributes.Add(fieldNames[i], o);
                    }
                    Feature feature = new Feature(featureID, schema, geometry, attributes);
                    flst.Add(feature);
                }

                FeatureCollection fc = new FeatureCollection(flst);
                FeatureSource fs = new FeatureSource(schema, fc);
                return fs;
                
                conn.Close();
            }
            catch(Exception e)
            {
                return null;
            }
        }

        //把FeatureSource保存成一个数据库表，如果已经存在则删除原表，重新创建一个表，需要指定投影参考对应的整数
        public bool Save2Table(FeatureSource fs,int srid)
        {
            FeatureSource fc = fs;
            string connStr = "Server=" + server + ";Port=" + port + ";UserId=" + user + ";Password=" + password + ";Database=" + database + ";";
            try
            {
                if (HasTable(fc.schema.name))
                {
                    DropTable(fc.schema.name);
                }
                CreateTable(fc.schema,srid);

                NpgsqlConnection conn = new NpgsqlConnection(connStr);
                conn.Open();
                NpgsqlTransaction tran = conn.BeginTransaction();
                NpgsqlCommand dbcmd = conn.CreateCommand();
                
                List<Feature> flst = fc.features.featureList;
                for (int i = 0; i < flst.Count; i++)
                {
                    string sql = "INSERT INTO " + fc.schema.name + "(gid,";
                    string temp="";
                    int index = 0;
                    dbcmd.Parameters.Clear();
                    foreach (string field in flst[i].schema.fields.Keys)
                    {
                        if (field.Equals("gid") || field.Equals("geom") || field.Equals("geom1"))
                        {
                            continue;
                        }
                        sql += field + ",";
                        temp+=":p"+index+",";
                        dbcmd.Parameters.AddWithValue("p"+index,flst[i].attributes[field]);
                        index++;
                    }
                    if (Type2String.wkbGeometryType2String(fs.schema.geometryType) == "MULTIPOLYGON")
                    {

                        sql += "geom) VALUES('" + flst[i].featureID + "'," + temp + "st_multi(st_geomfromText('" + Feature2WKT.Geometry2WKT(flst[i].geometry) + "'," + srid + ")))";
                    }
                    else
                    {
                        sql += "geom) VALUES('" + flst[i].featureID + "'," + temp + "st_geomfromText('" + Feature2WKT.Geometry2WKT(flst[i].geometry) + "'," + srid + "))";
                    }
                    dbcmd.CommandText = sql;
                    dbcmd.ExecuteNonQuery();
                }
                tran.Commit();
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public OSGeo.OGR.FieldDefn GetFieldDefn(string fieldName, string type)
        {
            return new OSGeo.OGR.FieldDefn(fieldName, String2Type.String2OGRFiledType(type));
        }

        public Dictionary<string, string> GetSchemePara(string table)
        {
            Dictionary<string, string> rd = new Dictionary<string, string>();
            string connStr = "Server=" + server + ";Port=" + port + ";UserId=" + user + ";Password=" + password + ";Database=" + database + ";";
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connStr);
                conn.Open();
                IDbCommand dbcmd = conn.CreateCommand();
                dbcmd.CommandText = "SELECT type,srid,f_geometry_column FROM geometry_columns WHERE f_table_catalog = '"+database+"' AND f_table_name = '"+table+"'";
                IDataReader dr = dbcmd.ExecuteReader();
                if (dr.Read())
                {
                    rd.Add("type", dr[0].ToString());
                    rd.Add("srid", dr[1].ToString());
                    rd.Add("column", dr[2].ToString());
                }
                else
                {
                    rd.Add("type", "");
                    rd.Add("srid", "4326");
                    rd.Add("column", "geom");
                }
                conn.Close();
                return rd;
            }
            catch
            {
                rd.Add("type", "");
                rd.Add("srid", "4326");
                rd.Add("column", "geom");
                return rd;
            }
        }

        public string Int2SRText(int srid)
        {
            string srstr = "";
            string connStr = "Server=" + server + ";Port=" + port + ";UserId=" + user + ";Password=" + password + ";Database=" + database + ";";
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connStr);
                conn.Open();
                IDbCommand dbcmd = conn.CreateCommand();
                dbcmd.CommandText = "SELECT srtext FROM \"spatial_ref_sys\" WHRER srid="+srid+";";
                IDataReader dr = dbcmd.ExecuteReader();
                if(dr.Read())
                {
                    srstr = dr[0].ToString();
                }
                conn.Close();
                return srstr;
            }
            catch
            {
                return srstr;
            }
        }

        //查看数据库中是否有指定名字的表
        public bool HasTable(string table)
        {
            string connStr = "Server=" + server + ";Port=" + port + ";UserId=" + user + ";Password=" + password + ";Database=" + database + ";";
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connStr);
                conn.Open();
                IDbCommand dbcmd = conn.CreateCommand();
                dbcmd.CommandText = "SELECT * FROM \""+table+"\" LIMIT 1;";
                IDataReader dr = dbcmd.ExecuteReader();
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //删除指定名字的表，如果表不存在或者删除失败返回false
        public bool DropTable(string table)
        {
            string connStr = "Server=" + server + ";Port=" + port + ";UserId=" + user + ";Password=" + password + ";Database=" + database + ";";
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connStr);
                conn.Open();
                IDbCommand dbcmd = conn.CreateCommand();
                dbcmd.CommandText = "drop table "+table+";";
                dbcmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //根据schema和指定投影参考在数据库中创建一个表，创建失败返回false
        public bool CreateTable(Schema schema, int srid)
        {
            string connStr = "Server=" + server + ";Port=" + port + ";UserId=" + user + ";Password=" + password + ";Database=" + database + ";";
                string sql = "CREATE TABLE " + schema.name + "(gid  SERIAL PRIMARY KEY,";
                foreach (string field in schema.fields.Keys)
                {
                    if (field.Equals("gid") || field.Equals("geom") || field.Equals("geom1"))
                    {
                        continue;
                    }
                    sql += field + " " + Type2String.OGRFiledType2String(schema.fields[field].GetFieldType()) + ",";
                }
            sql += "geom geometry(" + Type2String.wkbGeometryType2String(schema.geometryType) + "," + srid + "));";
            //sql += "geom geometry(" + Type2String.wkbGeometryType2String(schema.geometryType) + "," + srid + "));";

            NpgsqlConnection conn = new NpgsqlConnection(connStr);
                conn.Open();
                IDbCommand dbcmd = conn.CreateCommand();
                dbcmd.CommandText = sql;
                dbcmd.ExecuteNonQuery();
                conn.Close();
                return true;
        }

        //得到数据库里面的表明列表
        public List<string> GetTableNames()
        {
            List<string> tns = new List<string>();
            string connStr = "Server=" + server + ";Port=" + port + ";UserId=" + user + ";Password=" + password + ";Database=" + database + ";";
            try
            {
                NpgsqlConnection conn = new NpgsqlConnection(connStr);
                conn.Open();
                IDbCommand dbcmd = conn.CreateCommand();
                dbcmd.CommandText = "SELECT f_table_name FROM geometry_columns WHERE f_table_catalog = '"+database+"' AND f_table_schema = 'public';";
                IDataReader dr = dbcmd.ExecuteReader();
                while (dr.Read())
                {
                    tns.Add(dr[0].ToString());
                }
                conn.Close();
                return tns;
            }
            catch
            {
                return tns;
            }
        }

    }
}
