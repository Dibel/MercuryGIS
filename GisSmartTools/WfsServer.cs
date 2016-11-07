using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using GisSmartTools.Data;

namespace GisSmartTools
{
    public class WfsServer : Qoollo.Net.Http.HttpServer
    {
        public GisSmartTools.Support.mapcontent mapcontent;

        
        public WfsServer(int port)
            : base(port)
        {
            Get["/MercuryGIS/api/getLayerNames"] = GetLayerNames;
            Post["/MercuryGIS/api/getLayer"] = PostLayers;
            Get["/MercuryGIS/api/getLayer"] = PostLayers;
            //Get["/MercuryGIS/api/getLayers"] = GetUsers;
            string path = System.IO.Directory.GetCurrentDirectory() + "\\web";
            ServeStatic(new DirectoryInfo(path), "MercuryGIS");
        }

        private string GetLayerNames(HttpListenerRequest arg)
        {
            string layerNames = "";
            foreach(GisSmartTools.Support.Layer layer in mapcontent.layerlist)
            {
                layerNames += layer.Layername + ",";
            }
            return layerNames.Substring(0, layerNames.Length - 1);
        }

        private string PostLayers(HttpListenerRequest httpRequest)
        {
            //Stream body = httpRequest.InputStream;
            //StreamReader reader = new System.IO.StreamReader(body, httpRequest.ContentEncoding);
            //string requestContent = reader.ReadToEnd();
            //return requestContent;
            string layername = httpRequest.Url.ToString().Split('=')[1];
            int index=0;
            for(;index<mapcontent.layerlist.Count;index++)
            {
                if(layername==mapcontent.layerlist[index].Layername)
                    break;
            }

            if(index==mapcontent.layerlist.Count)
                return null;

            FeatureCollection featureCollection = mapcontent.layerlist[index].featuresource.features;
            string actualJson = ConvertToGeoJson.FeatureCollectionToGeoJson(featureCollection);
            return actualJson;
        }
    }
}
