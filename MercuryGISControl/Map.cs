using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MercuryGISData;
using System.IO;

namespace MercuryGISControl
{
    public class Map
    {
        private List<Layer> layers;
        public Map()
        {
            layers = new List<Layer>();
        }

        public void OpenMap(string filename)
        {
            string[] paths = File.ReadAllLines(filename);
            for (int i = 0; i < paths.Length; i++)
            {
                string[] splitResult = paths[i].Split(' ');
                switch (splitResult[0])
                {
                    case "Point":
                        PointLayer pointLayer = new PointLayer();
                        pointLayer.OpenLayer(splitResult[1]);
                        pointLayer.Id = i;
                        layers.Add(pointLayer);
                        break;
                    case "Line":
                        LineLayer lineLayer = new LineLayer();
                        lineLayer.OpenLayer(splitResult[1]);
                        lineLayer.Id = i;
                        layers.Add(lineLayer);
                        break;
                    case "Polygon":
                        PolygonLayer polygonLayer = new PolygonLayer();
                        polygonLayer.OpenLayer(splitResult[1]);
                        polygonLayer.Id = i;
                        layers.Add(polygonLayer);
                        break;
                    default:
                        break;
                }
            }
        }

        public void SaveMap(string filename)
        {
            string content = "";
            for (int i = 0; i < layers.Count; i++)
            {
                switch (layers[i].Type)
                {
                    case LayerType.Point:
                        content += "Point ";
                        break;
                    case LayerType.Line:
                        content += "Line ";
                        break;
                    case LayerType.Polygon:
                        content += "Polygon ";
                        break;
                    case LayerType.Text:
                        break;
                    default:
                        break;
                }

                var layerFilename = layers[i].Filename;
                content += layerFilename;
                if (i != LayerCount - 1) content += "\n";
                layers[i].SaveLayer(layerFilename);
            }
            File.WriteAllText(filename, content);
        }

        public List<Layer> GetLayers()
        {
            return layers;
        }

        public Layer GetLayer(int id)
        {
            return layers[id];
        }
        
        public void SetLayer(Layer oldLayer, Layer inputLayer)
        {
            layers[layers.IndexOf(oldLayer)] = inputLayer;
        }

        public List<Layer> GetShownLayers(List<int> ids)
        {
            List<Layer> result = new List<Layer>();
            for (int i = 0; i < ids.Count; i++)
            {
                result.Add(layers[i]);
            }
            return result;
        }
        
        public List<List<int>> QueryByLocat(Rectangle rect)
        {
            List<List<int>> result = new List<List<int>>();
            for (int i = 0; i < layers.Count; i++)
            {
                result.Add(layers[i].QueryByLocat(rect));
            }
            return result;
        }

        public void ClearSelection()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].ClearSelection();
            }
        }

        public void Add(Layer inputLayer)
        {
            inputLayer.Id = LayerCount;
            layers.Add(inputLayer);
        }

        public void Remove(int id)
        {
            if (id < LayerCount)
            {
                layers.RemoveAt(id);
            }
        }

        public void MoveLayerToTop(int id)
        {
            var layer = layers[id];
            layers.RemoveAt(id);
            layers.Insert(0, layer);
        }

        public void MoveLayerToBottom(int id)
        {
            var layer = layers[id];
            layers.RemoveAt(id);
            layers.Add(layer);
        }

        public void MoveUpOneStep(int id)
        {
            var layer = layers[id];
            layers.RemoveAt(id);
            layers.Insert(id-1, layer);
        }

        public void MoveDownOneStep(int id)
        {
            var layer = layers[id];
            layers.RemoveAt(id);
            layers.Insert(id+1, layer);
        }

        public int LayerCount
        {
            get { return layers.Count; }
        }
    }
}
