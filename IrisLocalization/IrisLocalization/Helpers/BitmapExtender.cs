using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using IrisLocalization.Models;

namespace IrisLocalization.Helpers
{
    public static class BitmapExtender
    {
        public static void DrawCircle(this Bitmap bmp, int x, int y, int r)
        {
            using (Graphics grf = Graphics.FromImage(bmp))
            {
                using (Pen pen = new Pen(Color.Red))
                {
                    grf.DrawEllipse(pen, x-r, y-r, 2*r, 2*r);
                }
            }
        }

        public static void DrawLine(this Bitmap bmp, int x1, int y1, int x2, int y2, Color? color = null)
        {
            using (Graphics grf = Graphics.FromImage(bmp))
            {
                using (Pen pen = new Pen(color ?? Color.Black))
                {
                    grf.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        public static BitmapImage ToBitmapImage(this Bitmap bmp)
        {
            using (var memory = new MemoryStream())
            {
                bmp.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public static void ApplyTransform(this Bitmap bmp, ITransformAlgorithm algorithm)
        {
           algorithm.Transform(bmp);
        }
    }
}
