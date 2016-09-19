using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.OleDb;

namespace MercuryGISData
{
    public class Importer
    {

        public static uint EndianConvert(uint a)
        {
            return ((((uint)(a) & 0xff000000) >> 24) | (((uint)(a) & 0x00ff0000) >> 8) | (((uint)(a) & 0x0000ff00) << 8) | (((uint)(a) & 0x000000ff) << 24));
        }
        /// <summary>
        /// 读点要素
        /// </summary>
        /// <param name="points"></param>
        /// <param name="br"></param>
        public static void readPoint(List<MPoint> points, BinaryReader br)
        {
            uint recordnumber;
            uint contentlength;
            int shapetype;
            MPoint Cmpoint;
            int i = 0;
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                recordnumber = EndianConvert(br.ReadUInt32());
                contentlength = EndianConvert(br.ReadUInt32());
                shapetype = br.ReadInt32();
                Cmpoint = new MPoint((int)recordnumber, 0, 0);
                Cmpoint.X = br.ReadDouble();
                Cmpoint.Y = br.ReadDouble();
                points.Add(Cmpoint);
                if (shapetype == 18)
                {
                    br.ReadDouble();
                    br.ReadDouble();
                }
                else if (shapetype == 28)
                {
                    br.ReadDouble();
                }
                i++;
            }
        }
        /// <summary>
        /// 读取多点要素
        /// </summary>
        /// <param name="mpoint"></param>
        public static void readMPoint(List<MultiPoint> multipoints, BinaryReader br)
        {
            uint recordnumber;
            uint contentlength;
            int shapetype;
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                int pointcount;
                MPoint Cpoint;
                List<MPoint> pointlist = new List<MPoint>();
                MultiPoint multipoint = new MultiPoint(0, pointlist);
                multipoint.Id = multipoints.Count + 1;
                recordnumber = EndianConvert(br.ReadUInt32());
                contentlength = EndianConvert(br.ReadUInt32());
                shapetype = br.ReadInt32();
                for (int box = 0; box < 4; box++)
                {
                    br.ReadDouble();
                }
                pointcount = br.ReadInt32();
                for (int i = 0; i < pointcount; i++)
                {
                    Cpoint = new MPoint(0, 0, 0);
                    Cpoint.X = br.ReadDouble();
                    Cpoint.Y = br.ReadDouble();
                    pointlist.Add(Cpoint);
                    if (shapetype == 18)
                    {
                        br.ReadDouble();
                        br.ReadDouble();
                    }
                    else if (shapetype == 28)
                    {
                        br.ReadDouble();
                    }
                    multipoint.AddPoint(Cpoint);
                }
                multipoints.Add(multipoint);
            }
        }
        /// <summary>
        /// 读取线要素
        /// </summary>
        /// <param name="multiLine"></param>
        /// <param name="br"></param>
        public static void readLine(List<Line> linelist, BinaryReader br)
        {
            uint recordnumber;
            uint contentlength;
            int shapetype;
            int pointcount;
            int numparts;
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                MPoint Cpoint;//点
                List<MPoint> mpointlist = new List<MPoint>();//点列
                PList pointlist = new PList(0, mpointlist);
                List<int> parts = new List<int>();
                recordnumber = EndianConvert(br.ReadUInt32());
                contentlength = EndianConvert(br.ReadUInt32());
                shapetype = br.ReadInt32();
                //for (int box = 0; box < 4; box++)
                //{
                //    br.ReadDouble();
                //}
                double minx = br.ReadDouble();
                double miny = br.ReadDouble();
                double maxx = br.ReadDouble();
                double maxy = br.ReadDouble();
                numparts = br.ReadInt32();
                pointcount = br.ReadInt32();
                for (int i = 0; i < numparts; i++)
                {
                    parts.Add(br.ReadInt32());
                }
                parts.Add(pointcount);
                Line lines = new Line((int)recordnumber);
                for (int i = 0; i < parts.Count - 1; i++)
                {
                    mpointlist = new List<MPoint>();
                    pointlist = new PList(0, mpointlist);
                    for (int j = 0; j < parts[i + 1] - parts[i]; j++)
                    {
                        Cpoint = new MPoint(0, 0, 0);
                        Cpoint.X = br.ReadDouble();
                        Cpoint.Y = br.ReadDouble();
                        pointlist.AddPoint(Cpoint);
                    }
                    List<PList> listplist = new List<PList>();
                    listplist.Add(pointlist);
                    lines.AddLine(pointlist);

                }

                lines.PointCount = pointcount;
                lines.LineCount = numparts;
                lines.MinX = minx;
                lines.MinY = miny;
                lines.MaxX = maxx;
                lines.MaxY = maxy;
                linelist.Add(lines);
            }
        }
        /// <summary>
        /// 读取多边形要素
        /// </summary>
        /// <param name="br"></param>
        public static void readPolygon(List<Polygon> polygonlayer, BinaryReader br)
        {
            uint recordnumber;
            uint contentlength;
            int shapetype;
            int pointcount;
            int numparts;
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                MPoint Cpoint;//点
                List<MPoint> mpointlist = new List<MPoint>();//点列
                PList pointlist = new PList(0, mpointlist);
                List<int> parts = new List<int>();
                recordnumber = EndianConvert(br.ReadUInt32());
                contentlength = EndianConvert(br.ReadUInt32());
                shapetype = br.ReadInt32();
                //for (int box = 0; box < 4; box++)
                //{
                //    br.ReadDouble();
                //}
                double minx = br.ReadDouble();
                double miny = br.ReadDouble();
                double maxx = br.ReadDouble();
                double maxy = br.ReadDouble();
                numparts = br.ReadInt32();
                pointcount = br.ReadInt32();
                for (int i = 0; i < numparts; i++)
                {
                    parts.Add(br.ReadInt32());
                }
                parts.Add(pointcount);
                Polygon polygons = new Polygon((int)recordnumber);
                for (int i = 0; i < parts.Count - 1; i++)
                {
                    mpointlist = new List<MPoint>();
                    pointlist = new PList(0, mpointlist);
                    for (int j = 0; j < parts[i + 1] - parts[i]; j++)
                    {
                        Cpoint = new MPoint(0, 0, 0);
                        Cpoint.X = br.ReadDouble();
                        Cpoint.Y = br.ReadDouble();
                        pointlist.AddPoint(Cpoint);
                    }
                    List<PList> listplist = new List<PList>();
                    listplist.Add(pointlist);
                    polygons.AddPolygon(pointlist);
                    
                }

                polygons.PointCount = pointcount;
                polygons.PolygonCount = numparts;
                polygons.MinX = minx;
                polygons.MinY = miny;
                polygons.MaxX = maxx;
                polygons.MaxY = maxy;
                polygonlayer.Add(polygons);
            }
        }

        /// <summary>
        /// 读取shapefile主程序
        /// </summary>
        /// <param name="filename"></param>
        public static Layer readshpfile(string filename)
        {

            //Read dbf file
            PropertyDataSet set = new PropertyDataSet();
            set.New();
            string path = Path.GetDirectoryName(filename) + "\\";
            string name = Path.GetFileNameWithoutExtension(filename) + ".dbf";
            OleDbConnection conn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=DBASE III;");
            conn.Open();
            string sqlStr = "Select * from " + name;

            OleDbDataAdapter myAdapter = new OleDbDataAdapter(sqlStr, conn);
            //Build the Update and Delete SQL Statements
            OleDbCommandBuilder myBuilder = new OleDbCommandBuilder(myAdapter);


            //Fill the DataSet with the Table 'bookstock'
            myAdapter.Fill(set.table);
            for (int i = 0; i < set.table.Rows.Count; i++)
            {
                set.table.Rows[i]["ID"] = i + 1;
            }
            conn.Close();

            //Layer newlayer=new Layer();
            FileStream sfilesream = new FileStream(filename, FileMode.Open);
            BinaryReader br = new BinaryReader(sfilesream);
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            uint filecode;
            uint[] unused = new uint[5];
            uint filelength;
            int version;
            int shapetype;
            double Xmin;
            double Ymin;
            double Xmax;
            double Ymax;
            double Zmin;
            double Zmax;
            double Mmin;
            double Mmax;
            filecode = EndianConvert(br.ReadUInt32());
            for (int i = 0; i < 5; i++)
            {
                unused[i] = EndianConvert(br.ReadUInt32());
            }
            filelength = EndianConvert(br.ReadUInt32());
            version = br.ReadInt32();
            shapetype = br.ReadInt32();
            Xmin = br.ReadDouble();
            Ymin = br.ReadDouble();
            Xmax = br.ReadDouble();
            Ymax = br.ReadDouble();
            Zmin = br.ReadDouble();
            Zmax = br.ReadDouble();
            Mmin = br.ReadDouble();
            Mmax = br.ReadDouble();

            switch (shapetype)
            {
                case 0:
                    break;
                case 1:
                    PointLayer pointslayer = new PointLayer();
                    //List<MPoint>points1=new List<MPoint>();
                    readPoint(pointslayer.elements, br);
                    pointslayer.dataset = set;
                    pointslayer.FindMinMax();
                    return pointslayer;
                case 3:
                    LineLayer linelayer = new LineLayer();
                    //List<PList> multiPointList = new List<PList>();
                    //Line lines = new Line(0,multiPointList);
                    readLine(linelayer.elements, br);
                    linelayer.dataset = set;
                    linelayer.FindMinMax();
                    return linelayer;
                case 5:
                    PolygonLayer polygonlayer = new PolygonLayer();
                    //List<PList> multiPointList5 = new List<PList>();
                    //Polygon polygons = new Polygon(0, multiPointList5);
                    readPolygon(polygonlayer.elements, br);
                    polygonlayer.dataset = set;
                    polygonlayer.FindMinMax();
                    return polygonlayer;
                case 8:
                    PointLayer pointslayer8 = new PointLayer();
                    //List<MPoint>points1=new List<MPoint>();
                    readPoint(pointslayer8.elements, br);
                    pointslayer8.dataset = set;
                    pointslayer8.FindMinMax();
                    return pointslayer8;
                case 11:
                    break;
                case 13:
                    break;
                case 15:
                    break;
                case 18:
                    break;
                case 21:
                    PointLayer pointslayer21 = new PointLayer();
                    //List<MPoint>points1=new List<MPoint>();
                    readPoint(pointslayer21.elements, br);
                    pointslayer21.dataset = set;
                    pointslayer21.FindMinMax();
                    return pointslayer21;
                case 23:
                    break;
                case 25:
                    break;
                case 28:
                    PointLayer pointslayer28 = new PointLayer();
                    //List<MPoint>points1=new List<MPoint>();
                    readPoint(pointslayer28.elements, br);
                    pointslayer28.dataset = set;
                    pointslayer28.FindMinMax();
                    return pointslayer28;
                case 31:
                    break;
            }
            br.Close();
            sfilesream.Close();
            return null;
        }
    }
}
