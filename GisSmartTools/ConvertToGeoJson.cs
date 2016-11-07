using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GisSmartTools.Geometry;
using GisSmartTools.Data;
using JGeometry = GeoJSON.Net.Geometry;
using JFeature=GeoJSON.Net.Feature;

namespace GisSmartTools
{
    public class ConvertToGeoJson
    {
        public static GeoJSON.Net.Geometry.Point Point2JG(PointD point)
        {
            JGeometry.GeographicPosition position=new JGeometry.GeographicPosition(point.Y,point.X);
            JGeometry.IPosition ip = position;
            JGeometry.Point jPoint = new JGeometry.Point(position);
            return jPoint;
        }

        private static JGeometry.LineString Points2LineString(List<PointD> points)
        {
            List<JGeometry.IPosition> ipositions = new List<JGeometry.IPosition>();
            var positions = (from point in points select new JGeometry.GeographicPosition(point.Y, point.X)).ToList();
            ipositions.AddRange(positions);
            JGeometry.LineString lineString = new JGeometry.LineString(ipositions);
            return lineString;
        }

        public static JGeometry.LineString Polyline2JG(SimplePolyline simplePolyline)
        {
            return Points2LineString(simplePolyline.points);
        }

        public static JGeometry.MultiLineString MultiPolyline2JG(Polyline polyline)
        {
            var linestrings = (from simplePolyline in polyline.childPolylines select Polyline2JG(simplePolyline)).ToList();
            JGeometry.MultiLineString multiLineString = new JGeometry.MultiLineString(linestrings);
            return multiLineString;
        }

        public static JGeometry.Polygon Polygon2JG(SimplePolygon simplePolygon)
        {
            var rings = (from ring in simplePolygon.rings select Polyline2JG(ring)).ToList();
            JGeometry.Polygon polygon = new JGeometry.Polygon(rings);
            return polygon;
        }

        public static JGeometry.MultiPolygon MultiPolygon2JG(Polygon polygon)
        {
            var polygons = (from simplePolygon in polygon.childPolygons select Polygon2JG(simplePolygon)).ToList();
            JGeometry.MultiPolygon multiPolygon = new JGeometry.MultiPolygon(polygons);
            return multiPolygon;
        }

        public static JGeometry.IGeometryObject Geometry2JG(Geometry.Geometry geom)
        {
            JGeometry.IGeometryObject igeometry;
            switch (geom.geometryType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    igeometry=Point2JG((PointD)geom);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                    igeometry = Polyline2JG((SimplePolyline)geom);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                    igeometry = Polygon2JG((SimplePolygon)geom);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                    igeometry = MultiPolyline2JG((Polyline)geom);
                    break;
                case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
                    igeometry = MultiPolygon2JG((Polygon)geom);
                    break;
                default:
                    igeometry = MultiPolygon2JG((Polygon)geom);
                    break;
            }

            return igeometry;
        }
        public static JFeature.Feature Feature2JG(Feature feature)
        {
            JGeometry.IGeometryObject igeometry=Geometry2JG(feature.geometry);
            JFeature.Feature jFeature = new JFeature.Feature(igeometry, feature.attributes, feature.featureID.ToString());
            return jFeature;
        }

        public static JFeature.FeatureCollection FeatureCollection2JG(FeatureCollection featureCollection)
        {
            var jFeatures=(from feature in featureCollection.featureList select Feature2JG(feature)).ToList();
            JFeature.FeatureCollection jFeatureCollection = new JFeature.FeatureCollection();
            jFeatureCollection.Features.AddRange(jFeatures);
            return jFeatureCollection;
        }

        public static string FeatureCollectionToGeoJson(FeatureCollection featureCollection)
        {
            JFeature.FeatureCollection jFeatureCollection = FeatureCollection2JG(featureCollection);
            string  actualJson = JsonConvert.SerializeObject(jFeatureCollection);
            return actualJson;
        }


    }
}
