using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisLocalization.Models
{
    public class ImageStatistics
    {
        private Bitmap bitmap;
        public ImageStatistics(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public (int[] r, int[] g, int[] b) GetHistograms()
        {
            int[] r = new int[256];
            int[] g = new int[256];
            int[] b = new int[256];
            for (int j = 0; j < bitmap.Height; j++)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    var pixel = bitmap.GetPixel(i, j);
                    r[pixel.R]++;
                    g[pixel.G]++;
                    b[pixel.B]++;
                }
            }

            return (r, g, b);
        }

        public (int[] vertical, int[] horizontal) GetProjections()
        {
            int[] vertical = new int[bitmap.Height];
            int[] horizontal = new int[bitmap.Width];
            for (int j = 0; j < bitmap.Height; j++)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    var pixel = bitmap.GetPixel(i, j);
                    if (pixel.R <= 0)
                    {
                        vertical[j]++;
                        horizontal[i]++;
                    }
                }
            }

            return (vertical, horizontal);
        }
    }
}
