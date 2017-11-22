using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using IrisLocalization.Helpers;
using IrisLocalization.Models.Algorithms;
using MahApps.Metro.Converters;

namespace IrisLocalization.Models
{
    public class ImageModel
    {
        private ImageStatistics statistics;

        private Bitmap imageResult;
        private Bitmap imageDebugBitmap;

        private Bitmap verticalProjectionBitmap;
        private Bitmap horizontalProjectionBitmap;

        private int[] verticalProjection;
        private int[] horizontalProjection;

        public ImageModel(string path)
        {
            imageResult = (Bitmap)Image.FromFile(path);
            imageDebugBitmap = (Bitmap)imageResult.Clone();
            statistics = new ImageStatistics(imageDebugBitmap);

            FindIris();
        }


        void FindIris()
        {
            imageDebugBitmap.ApplyTransform(new Grayscale());
            imageDebugBitmap.ApplyTransform(new Binaryzation(6));

            UpdateProjections();
            ClearBitmapByProjectionValue(verticalProjection.Average(), horizontalProjection.Average());

            int vStart;
            int vEnd;
            var height = GetLongestSegment(verticalProjection, out vStart, out vEnd);
            int vMid = (vStart + vEnd) / 2;

            int hStart;
            int hEnd;
            var width = GetLongestSegment(horizontalProjection, out hStart, out hEnd);
            int hMid = (hStart + hEnd) / 2;

            int radius = (height + width) / 4;

            if (Math.Abs(width - height) > (width + height) / 4)
            {
                ClearBitmapByProjectionValue(null, horizontalProjection.Where(x => x != 0).Average());
                UpdateProjections();
                ClearNotLongest();
                UpdateProjections();

                height = GetLongestSegment(verticalProjection, out vStart, out vEnd);
                vMid = (vStart + vEnd) / 2;

                width = GetLongestSegment(horizontalProjection, out hStart, out hEnd);
                hMid = (hStart + hEnd) / 2;

                radius = (height + width) / 4;

                imageResult.DrawCircle(hMid, vMid, radius);

            }
            else
            {
                imageResult.DrawCircle(hMid, vMid, radius);
            }

            GenerateProjection();
        }

        void UpdateProjections()
        {
            var projections = statistics.GetProjections();
            verticalProjection = projections.vertical;
            horizontalProjection = projections.horizontal;
        }

        void ClearNotLongest()
        {
            int hStart;
            int hEnd;
            var width = GetLongestSegment(horizontalProjection, out hStart, out hEnd);

            for (int i = 0; i < hStart; i++)
            {
                imageDebugBitmap.DrawLine(i,0,i, imageDebugBitmap.Height-1, Color.White);
            }
            for (int i = hEnd; i < imageDebugBitmap.Width; i++)
            {
                imageDebugBitmap.DrawLine(i, 0, i, imageDebugBitmap.Height - 1, Color.White);
            }
        }

       

        void ClearBitmapByProjectionValue(double? vAvg, double? hAvg)
        {
            if (vAvg != null)
            {
                for (int i = 0; i < verticalProjection.Length; i++)
                {
                    if (verticalProjection[i] < vAvg)
                    {
                        imageDebugBitmap.DrawLine(0, i, imageDebugBitmap.Width - 1, i, Color.White);
                        verticalProjection[i] = 0;
                    }
                }
            }

            if (hAvg != null)
            {
                for (int i = 0; i < horizontalProjection.Length; i++)
                {
                    if (horizontalProjection[i] < hAvg)
                    {
                        imageDebugBitmap.DrawLine(i, 0, i, imageDebugBitmap.Height - 1, Color.White);
                        horizontalProjection[i] = 0;
                    }
                }
            }
        }

        int GetLongestSegment(int[] proj, out int s, out int e)
        {
            List<int> beginings = new List<int>();
            List<int> endings = new List<int>();

            if (proj[0] > 0)
                beginings.Add(0);

            for (int i = 1; i < proj.Length - 1; i++)
            {
                if (proj[i - 1] == 0 && proj[i] > 0)
                    beginings.Add(i);

                if (proj[i] > 0 && proj[i + 1] == 0)
                    endings.Add(i);
            }

            if (proj[proj.Length - 1] > 0)
                endings.Add(proj.Length - 1);

            int max = 0;
            int index = -1;
            for (int i = 0; i < Math.Min(beginings.Count, endings.Count); i++)
            {
                if (endings[i] - beginings[i] > max)
                {
                    max = endings[i] - beginings[i];
                    index = i;
                }
            }

            s = 0;
            e = 0;

            if (index >= 0)
            {
                s = beginings[index];
                e = endings[index];
            }

            return max;
        }

        void GenerateProjection()
        {
            var vProj = verticalProjection;
            verticalProjectionBitmap = new Bitmap(vProj.Max(), vProj.Length);
            for (int i = 0; i < vProj.Length; i++)
            {
                verticalProjectionBitmap.DrawLine(0, i, vProj[i], i);
            }

            var hProj = horizontalProjection;
            horizontalProjectionBitmap = new Bitmap(hProj.Length, hProj.Max());
            for (int i = 0; i < hProj.Length; i++)
            {
                horizontalProjectionBitmap.DrawLine(i, 0, i, hProj[i]);
            }
        }

        public BitmapImage GetImageResult()
        {
            return imageResult.ToBitmapImage();
        }

        public BitmapImage GetDebugImage()
        {
            return imageDebugBitmap.ToBitmapImage();
        }

        public BitmapImage GetVerticalProjection()
        {
            return verticalProjectionBitmap.ToBitmapImage();
        }

        public BitmapImage GetHorizontalProjection()
        {
            return horizontalProjectionBitmap.ToBitmapImage();
        }
    }
}
