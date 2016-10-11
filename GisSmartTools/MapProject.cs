using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace GisSmartTools
{
    /// <summary>
    /// 本类对应于一个工程文件
    /// </summary>
    /// 
    [Serializable]
    public class MapProject
    {
        public string projectname = "";
        public string projectpath = "";
        public string server = "";
        public string port = "";
        public string username = "";
        public string password = "";
        public string database = "";

        public Dictionary<string, string> dic_datapath;
        public Dictionary<string, string> dic_stylepath;

        public bool IsSaved = false;

        public MapProject(string projectname)
        {
            this.projectname = projectname;
            dic_datapath = new Dictionary<string, string>();
            dic_stylepath = new Dictionary<string, string>();
        }

        //方法
        public static MapProject LoadMapProject(string projectpath)
        {
            try
            {
                FileStream fs = new FileStream(projectpath, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                MapProject mappro = (MapProject)formatter.Deserialize(fs);
                return mappro;
            }catch(Exception e)
            {
                return null;
            }
        }

        public static MapProject CreateMapProject(string projectname)
        {
            MapProject mappro = new MapProject(projectname);
            return mappro;
        }

        public bool SaveMapProject(string filepath)
        {
            try
            {
                MemoryStream mo = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(mo, this);
                byte[] data = mo.ToArray();
                FileStream filestream = new FileStream(filepath, FileMode.OpenOrCreate);
                filestream.Write(data, 0, data.Length);
                filestream.Flush(); filestream.Close();
                this.IsSaved = true;
                return true;
            }catch(Exception e)
            {
                return false;
            }

        }
    }
}
