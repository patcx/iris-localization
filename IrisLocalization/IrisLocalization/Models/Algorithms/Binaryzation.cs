using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrisLocalization.Models.Algorithms
{
    public class Binaryzation : ITransformAlgorithm
    {
        private float normalization;
        private int hSkipFrom;
        private int hSkipTo;


        public Binaryzation(float normalization, int hSkipFrom = -1, int hSkipTo = -1)
        {
            this.normalization = normalization <= 0 ? 1 : normalization;
            this.hSkipFrom = hSkipFrom;
            this.hSkipTo = hSkipTo;
        }

        public void Transform(Bitmap bmp)
        {
            float P = 0;

            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {
                    if(hSkipFrom > 0 && hSkipTo > 0)
                    {
                        if (hSkipFrom <= i && i <= hSkipTo)
                            continue;
                    }

                    var pixel = bmp.GetPixel(i, j);
                    P += pixel.R;
                }
            }

            P /= bmp.Height * bmp.Width;
            P /= normalization;

            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {
                    var pixel = bmp.GetPixel(i, j);
                    if (pixel.R > P)
                        bmp.SetPixel(i, j, Color.White);
                    else
                        bmp.SetPixel(i, j, Color.Black);

                }
            }
        }
    }
}
