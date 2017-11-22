using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisLocalization.Models.Algorithms
{
    public class HistogramEqualization : ITransformAlgorithm
    {
        public void Transform(Bitmap bmp)
        {

            Bitmap renderedImage = bmp;

            uint pixels = (uint)renderedImage.Height * (uint)renderedImage.Width;
            decimal Const = 255 / (decimal)pixels;

            int x, y, R, G, B;

            ImageStatistics statistics = new ImageStatistics(renderedImage);

            //Create histogram arrays for R,G,B channels

            var histograms = statistics.GetHistograms();
            int[] cdfR = histograms.Item1;
            int[] cdfG = histograms.Item2;
            int[] cdfB = histograms.Item3;

            //Convert arrays to cumulative distribution frequency data
            for (int r = 1; r <= 255; r++)
            {
                cdfR[r] = cdfR[r] + cdfR[r - 1];
                cdfG[r] = cdfG[r] + cdfG[r - 1];
                cdfB[r] = cdfB[r] + cdfB[r - 1];
            }

            for (y = 0; y < renderedImage.Height; y++)
            {
                for (x = 0; x < renderedImage.Width; x++)
                {
                    Color pixelColor = renderedImage.GetPixel(x, y);

                    R = (int)((decimal)cdfR[pixelColor.R] * Const);
                    G = (int)((decimal)cdfG[pixelColor.G] * Const);
                    B = (int)((decimal)cdfB[pixelColor.B] * Const);

                    Color newColor = Color.FromArgb(R, G, B);
                    renderedImage.SetPixel(x, y, newColor);
                }
            }

        }
    }
}
