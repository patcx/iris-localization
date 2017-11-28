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

            int hMid;
            int vMid;

            int pupilR = FindPupil(out hMid, out vMid);
            int irisR = FindIris(hMid, vMid, pupilR);

            imageResult.DrawCircle(hMid, vMid, pupilR);
            imageResult.DrawCircle(hMid, vMid, irisR);

        }

        int FindIris(int hPupilMid, int vPupilMid, int pupilR)
        {
            int clipingRadius = 5 * pupilR;
            imageDebugBitmap = ClipImage(imageResult, new Rectangle(hPupilMid - clipingRadius, vPupilMid - clipingRadius, 2 * clipingRadius, 2 * clipingRadius));
            statistics = new ImageStatistics(imageDebugBitmap);


            //imageDebugBitmap.ApplyTransform(new HistogramEqualization());
            imageDebugBitmap.ApplyTransform(new Binaryzation(1, hPupilMid - pupilR, hPupilMid + pupilR));

            int factor = pupilR / 5;
            imageDebugBitmap.ApplyTransform(new Dilation(factor));
            imageDebugBitmap.ApplyTransform(new Erosion(factor));


            int irisR1 = 0, irisR2 = 0, irisR3 = 0;
            for(int i=0; i<imageDebugBitmap.Width/2; i++)
            {
                if (imageDebugBitmap.GetPixel(i, imageDebugBitmap.Height/2).R != 0)
                {
                    imageDebugBitmap.SetPixel(i, imageDebugBitmap.Height / 2, Color.Red);
                    irisR1++;
                }
                else
                {
                    break;
                }
            }

            for (int i = imageDebugBitmap.Width-1; i > imageDebugBitmap.Width / 2; i--)
            {
                if (imageDebugBitmap.GetPixel(i, imageDebugBitmap.Height / 2).R != 0)
                {
                    imageDebugBitmap.SetPixel(i, imageDebugBitmap.Height / 2, Color.Red);
                    irisR2++;
                }
                else
                {
                    break;
                }
            }

            for (int i = imageDebugBitmap.Height-1; i >= imageDebugBitmap.Height/2; i--)
            {
                if (imageDebugBitmap.GetPixel(imageDebugBitmap.Height / 2, i).R != 0)
                {
                    imageDebugBitmap.SetPixel(imageDebugBitmap.Height / 2, i, Color.Red);
                    irisR3++;
                }
                else
                {
                    break;
                }
            }


            UpdateProjections();
            DrawProjection();

            irisR1 = imageDebugBitmap.Width / 2 - irisR1;
            irisR2 = imageDebugBitmap.Width / 2 - irisR2;
            irisR3 = imageDebugBitmap.Width / 2 - irisR3;

            var radiuses = new int[] { irisR1, irisR2, irisR3 };
            int avg = (int)radiuses.Average();

            radiuses = radiuses.OrderByDescending(x => Math.Abs(x - avg)).Skip(1).ToArray();

            return (int)radiuses.Average();
        }

        private Bitmap ClipImage(Bitmap bmp, Rectangle rect)
        {
            if (rect.X < 0)
                rect.X = 0;
            if (rect.Y < 0)
                rect.Y = 0;
            if (rect.X + rect.Width > bmp.Width)
                rect.Width = bmp.Width - rect.X;
            if (rect.Y + rect.Height > bmp.Height)
                rect.Height = bmp.Height - rect.Y;

            return bmp.Clone(rect, bmp.PixelFormat);
        }


        int FindPupil(out int hMid, out int vMid)
        {
            imageDebugBitmap.ApplyTransform(new Grayscale());
            imageDebugBitmap.ApplyTransform(new Binaryzation(6));

            UpdateProjections();
            ClearBitmapByProjectionValue(verticalProjection.Average(), horizontalProjection.Average());

            int vStart;
            int vEnd;
            var height = GetLongestSegment(verticalProjection, out vStart, out vEnd);
            vMid = (vStart + vEnd) / 2;

            int hStart;
            int hEnd;
            var width = GetLongestSegment(horizontalProjection, out hStart, out hEnd);
            hMid = (hStart + hEnd) / 2;

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
            }

            DrawProjection();

            return radius;
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
                imageDebugBitmap.DrawLine(i, 0, i, imageDebugBitmap.Height - 1, Color.White);
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

        void DrawProjection()
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
