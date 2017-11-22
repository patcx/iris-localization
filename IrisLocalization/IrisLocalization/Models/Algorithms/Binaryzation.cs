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
        private int normalization;

        public Binaryzation(int normalization)
        {
            this.normalization = normalization <= 0 ? 1 : normalization;
        }

        public void Transform(Bitmap bmp)
        {
            var P = 0;

            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {
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
