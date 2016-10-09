using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Data;
using GisSmartTools.Support;
using System.IO;
namespace GisSmartTools
{

    public enum GisOperationSign
    {
        DeleteFeature = 1,
        CreateFeature = 2,
        CopyFeature = 3,

    }
    public class LogItem
    {
        public GisOperationSign operationsign;
        public int time;
        virtual public void GisRecover(mapcontent map)
        {

        }
        virtual public string GetDescription()
        {
            return null;
        }
        virtual public void GisForward(mapcontent map)
        {

        }

    }
    public class LogItem_CopyFeatures:LogItem
    {
        private string layername;
        private List<int> CopyFeatureIDs;

        public LogItem_CopyFeatures(string layername, FeatureCollection collection)
        {
            this.layername = layername;
            this.CopyFeatureIDs = new List<int>();
            foreach(Feature feature in collection.featureList)
            {
                this.CopyFeatureIDs.Add(feature.featureID);

            }
        }
        public override void GisRecover(mapcontent map)
        {
            base.GisRecover(map);
            Layer layer = map.GetLayerByName(this.layername);
            if (layer == null) return;
            FeatureCollection collection = layer.featuresource.features;
            foreach(int featureid in this.CopyFeatureIDs)
            {
                collection.GetFeatureByID(featureid).visible = false;
            }

        }
        public override void GisForward(mapcontent map)
        {
            base.GisForward(map);
            Layer layer = map.GetLayerByName(this.layername);
            if (layer == null) return;
            FeatureCollection collection = layer.featuresource.features;
            foreach (int featureid in this.CopyFeatureIDs)
            {
                collection.GetFeatureByID(featureid).visible = true;
            }
        }
        public override string GetDescription()
        {
            //base.GetDescription();
            string str_ids = "";
            foreach(int id in this.CopyFeatureIDs)
            {
                str_ids=str_ids+","+id;
            }
            return "复制" + this.layername + "图层的" + str_ids + "要素";
        }
    }
    public class LogItem_DeleteFeature : LogItem
    {
        private string layername;
        private List<int> DeletedFeatureIDs;
        public LogItem_DeleteFeature(string layername,FeatureCollection collection)
        {
            this.layername = layername;
            this.DeletedFeatureIDs = new List<int>();
            foreach(Feature feature in collection.featureList)
            {
                this.DeletedFeatureIDs.Add(feature.featureID);

            }
        }
        public override void GisRecover(mapcontent map)
        {
            base.GisRecover(map);
            Layer layer = map.GetLayerByName(this.layername);
            if (layer == null) return;
            FeatureCollection collection = layer.featuresource.features;
            foreach(int featureid in this.DeletedFeatureIDs)
            {
                collection.GetFeatureByID(featureid).visible = true;
            }

        }
        public override void GisForward(mapcontent map)
        {
            base.GisForward(map);
            Layer layer = map.GetLayerByName(this.layername);
            if (layer == null) return;
            FeatureCollection collection = layer.featuresource.features;
            foreach (int featureid in this.DeletedFeatureIDs)
            {
                collection.GetFeatureByID(featureid).visible = false;
            }
        }
        public override string GetDescription()
        {
            //base.GetDescription();
            string str_ids = "";
            foreach(int id in this.DeletedFeatureIDs)
            {
                str_ids=str_ids+","+id;
            }
            return "删除" + this.layername + "图层的" + str_ids + "要素";
        }
    }
    public class LogItem_CreateFeature:LogItem
    {
        private string layername;
        private int createdfeatureid;

        public LogItem_CreateFeature(string layername,int createdfeatureid)
        {
            this.layername = layername;
            this.createdfeatureid = createdfeatureid;
        }

        public override void GisRecover(mapcontent map)
        {
            base.GisRecover(map);
            Layer layer = map.GetLayerByName(this.layername);
            if (layer == null) return;
            FeatureCollection collection = layer.featuresource.features;
            collection.GetFeatureByID(createdfeatureid).visible = false;
        }
        public override void GisForward(mapcontent map)
        {
            base.GisForward(map);
            Layer layer = map.GetLayerByName(this.layername);
            if (layer == null) return;
            FeatureCollection collection = layer.featuresource.features;
            collection.GetFeatureByID(createdfeatureid).visible = true;
        }
        public override string GetDescription()
        {
            //base.GetDescription();
            string str_ids = "" + createdfeatureid;
            return "添加了[" + this.layername + "]图层的 [" + str_ids + "] 要素";
        }
    }
    class GisLog
    {

        List<LogItem> list = new List<LogItem>();
        public int curindex = -1;
        public string logpath = "smartgis.txt";
        public StreamWriter writer;
        public GisLog()
        {
            FileStream filestream = new FileStream(logpath, FileMode.Append);
            writer = new StreamWriter(filestream);
        }
        public void Recover(mapcontent map)
        {
            if (curindex < 0) return;
            list[curindex].GisRecover(map);
            curindex--;
        }
        public void Forward(mapcontent map)
        {
            if(curindex<list.Count-1)
            {
                curindex++;
                list[curindex].GisForward(map);
            }
        }
        public void AddLog(LogItem logitem)
        {
            for (int i = curindex+1; i < list.Count;i++ )
            {
                list.RemoveAt(i);
            }
            list.Add(logitem);
            writer.WriteLine(logitem.GetDescription());
            writer.Flush();
            curindex++;
        }

        public void ClearLog()
        {
            curindex = -1;
            list.Clear();

        }

    }
}
