using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSmartTools.Geometry;
namespace GisSmartTools.Data
{
    /**
     * feature集合类
     * 提供对于多个feature所构成的集合的增删改查功能
     * */
    public class FeatureCollection 
    {
       public List<Feature> featureList;
       public int count;

       public FeatureCollection(List<Feature> features)
       {
           this.featureList = features;
           if(this.featureList==null)this.featureList=new List<Feature>();
           this.count = this.featureList.Count;
       }
       public FeatureCollection() : this(null) { }

       public Feature GetFeatureByID(int ID)
       {
           return featureList.Find(
                    delegate(Feature feature)
                    {
                        return feature.featureID == ID;
                    }
                    );
       }

       public int DeleteFeaturebyID(int ID)
       {
           int dc = featureList.RemoveAll(
               delegate(Feature feature)
               {
                   return feature.featureID == ID;
               }
               );
           this.count = this.featureList.Count;
           return dc;
       }

        public FeatureCollection GetFeaturesByID(List<int> IDs)
        {
            List<Feature> featuresFound = featureList.FindAll(
                     delegate(Feature feature)
                     {
                         foreach (int id in IDs)
                         {
                             if (feature.featureID == id) return true;
                         }
                         return false;
                     }
                     );
            return new FeatureCollection(featuresFound);
        }

        public void InsertFeatures(FeatureCollection collection)
        {
            if(collection!=null&& collection.featureList.Count!=0)
            {
                this.featureList.AddRange(collection.featureList);
                this.count += collection.featureList.Count;
            }
        }
        public void InsertFeature(Feature feature)
        {
            featureList.Add(feature);
            this.count=featureList.Count;
        }
        public Rectangle getEnvelop()
        {
            if (this.count == 0) return new Rectangle(0, 0, 0, 0);
            Rectangle rect = new Rectangle(Utils.double_max, Utils.double_max, Utils.double_min, Utils.double_min);
            foreach(Feature feature in featureList)
            {
                OSGeo.OGR.wkbGeometryType featType = feature.geometry.geometryType;
                switch (featType)
                {
                    case OSGeo.OGR.wkbGeometryType.wkbPoint:
                            PointD point = (PointD)feature.geometry;
                            if (point.X < rect.minX) rect.minX = point.X;
                            if (point.X > rect.maxX) rect.maxX = point.X;
                            if (point.Y < rect.minY) rect.minY = point.Y;
                            if (point.Y > rect.maxY) rect.maxY = point.Y;
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbLineString:
                            SimplePolyline line = (SimplePolyline)feature.geometry;
                            if (line.minX < rect.minX) rect.minX = line.minX;
                            if (line.maxX > rect.maxX) rect.maxX = line.maxX;
                            if (line.minY < rect.minY) rect.minY = line.minY;
                            if (line.maxY > rect.maxY) rect.maxY = line.maxY;
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                        Polyline mline = (Polyline)feature.geometry;
                            if (mline.minX < rect.minX) rect.minX = mline.minX;
                            if (mline.maxX > rect.maxX) rect.maxX = mline.maxX;
                            if (mline.minY < rect.minY) rect.minY = mline.minY;
                            if (mline.maxY > rect.maxY) rect.maxY = mline.maxY;
                        break;

                    case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                            SimplePolygon polygon = (SimplePolygon)feature.geometry;
                            if (polygon.minX < rect.minX) rect.minX = polygon.minX;
                            if (polygon.maxX > rect.maxX) rect.maxX = polygon.maxX;
                            if (polygon.minY < rect.minY) rect.minY = polygon.minY;
                            if (polygon.maxY > rect.maxY) rect.maxY = polygon.maxY;
                        break;
                    case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
                         Polygon mpolygon = (Polygon)feature.geometry;
                            if (mpolygon.minX < rect.minX) rect.minX = mpolygon.minX;
                            if (mpolygon.maxX > rect.maxX) rect.maxX = mpolygon.maxX;
                            if (mpolygon.minY < rect.minY) rect.minY = mpolygon.minY;
                            if (mpolygon.maxY > rect.maxY) rect.maxY = mpolygon.maxY;
                        break;
                }
            }
            return rect;
           
        }
    }
}
