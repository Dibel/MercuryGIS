using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSGeo.GDAL;
using System.Windows.Media.Imaging;

namespace GisSmartTools
{
    public class Raster
    {
        public double[] buffer;
        public WriteableBitmap bmp;
        public int width;
        public int height;
        public Raster()
        {

        }

        public void ReadRaster(string path)
        {
            Gdal.AllRegister();
            Driver srcDrv = Gdal.GetDriverByName("GTiff");
            Dataset dataset = Gdal.Open(path, Access.GA_ReadOnly);
            Band band = dataset.GetRasterBand(1);
            width = dataset.RasterXSize;
            height = dataset.RasterYSize;
            buffer = new double[width * height];
            band.ReadRaster(0, 0, width, height, buffer, width, height, 0, 0);
            //double max;
            //double min;
            int temp;
            //band.GetMaximum(out max, out temp);
            //band.GetMinimum(out min, out temp);
            double[] result = new double[2];
            //band.ComputeRasterMinMax(result, 1);
            double minvalue = 100000;
            double maxvalue = -100000;
            for (int i = 0; i < width * height; i++)
            {
                if (minvalue > buffer[i]) minvalue = buffer[i];
                if (maxvalue < buffer[i]) maxvalue = buffer[i];
            }

            byte[] buf = new byte[width * height * 4];
            for (int i = 0; i < width * height; i++)
            {
                byte value = Convert.ToByte((buffer[i] - minvalue) * 255 / (maxvalue - minvalue));
                buf[i * 4] = value;
                buf[i * 4 + 1] = value;
                buf[i * 4 + 2] = value;
                buf[i * 4 + 3] = 255;
            }
            bmp = BitmapFactory.New(width, height);
            bmp.FromByteArray(buf);
        }
    }
}
