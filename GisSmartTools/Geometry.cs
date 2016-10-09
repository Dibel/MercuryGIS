using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
namespace GisSmartTools.Geometry
{

    [Serializable]
    public class Geometry
    {
        public OSGeo.OGR.wkbGeometryType geometryType;
       public byte[] ToBytes()
        {
            //to do
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream mo = new MemoryStream();
            formatter.Serialize(mo, this);
            byte[] result = mo.ToArray();
            mo.Flush();
            mo.Close();
            return result;
        }
        static public Geometry GetGeometryFromBytes(byte[] bytes)
        {
            //to do
            MemoryStream mo = new MemoryStream(bytes);
            BinaryFormatter formater = new BinaryFormatter();
            Geometry geo_result = (Geometry)formater.Deserialize(mo);
            mo.Flush();
            mo.Close();
            return geo_result;
        }
    }
}
