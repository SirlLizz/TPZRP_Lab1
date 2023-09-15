using System.Drawing;
using System.Drawing.Imaging;

namespace Server
{
    public static class Convolve
    {
        unsafe public class FastBitmap
        {
            private struct PixelData
            {
                public byte blue;
                public byte green;
                public byte red;
                public byte alpha;

                public override string ToString()
                {
                    return "(" + alpha.ToString() + ", " + red.ToString() + ", " + green.ToString() + ", " + blue.ToString() + ")";
                }
            }

            private Bitmap workingBitmap = null;
            private int width = 0;
            private BitmapData bitmapData = null;
            private Byte* pBase = null;

            public FastBitmap(Bitmap inputBitmap)
            {
                workingBitmap = inputBitmap;
            }

            public void LockImage()
            {
                Rectangle bounds = new Rectangle(Point.Empty, workingBitmap.Size);

                width = (int)(bounds.Width * sizeof(PixelData));
                if (width % 4 != 0) width = 4 * (width / 4 + 1);

                //Блокировка изображения
                bitmapData = workingBitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                pBase = (Byte*)bitmapData.Scan0.ToPointer();
            }

            private PixelData* pixelData = null;

            public Color GetPixel(int x, int y)
            {
                pixelData = (PixelData*)(pBase + y * width + x * sizeof(PixelData));
                return Color.FromArgb(pixelData->alpha, pixelData->red, pixelData->green, pixelData->blue);
            }

            public Color GetPixelNext()
            {
                pixelData++;
                return Color.FromArgb(pixelData->alpha, pixelData->red, pixelData->green, pixelData->blue);
            }

            public void SetPixel(int x, int y, Color color)
            {
                PixelData* data = (PixelData*)(pBase + y * width + x * sizeof(PixelData));
                data->alpha = color.A;
                data->red = color.R;
                data->green = color.G;
                data->blue = color.B;
            }

            public void UnlockImage()
            {
                workingBitmap.UnlockBits(bitmapData);
                bitmapData = null;
                pBase = null;
            }
        }

        public static Image Apply(Image input, float[,] filter)
        {
            //Найти центр фильтра
            int xMiddle = (int)Math.Floor(filter.GetLength(0) / 2.0);
            int yMiddle = (int)Math.Floor(filter.GetLength(1) / 2.0);

            //Создать новый образ
            Bitmap output = new Bitmap(input.Width, input.Height);

            FastBitmap reader = new FastBitmap((Bitmap)input);
            FastBitmap writer = new FastBitmap(output);
            reader.LockImage();
            writer.LockImage();



            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    float r = 0;
                    float g = 0;
                    float b = 0;
                    float k = 0;

                    //Применить фильтр
                    for (int xFilter = 0; xFilter < filter.GetLength(0); xFilter++)
                    {
                        for (int yFilter = 0; yFilter < filter.GetLength(1); yFilter++)
                        {
                            int x0 = x - xMiddle + xFilter;
                            int y0 = y - yMiddle + yFilter;

                            //Только если в границах
                            if (x0 >= 0 && x0 < input.Width &&
                                y0 >= 0 && y0 < input.Height)
                            {
                                Color clr = reader.GetPixel(x0, y0);

                                r += clr.R * filter[xFilter, yFilter];
                                g += clr.G * filter[xFilter, yFilter];
                                b += clr.B * filter[xFilter, yFilter];
                            }

                            k += filter[xFilter, yFilter];
                        }
                    }

                    //Нормализовать (основной)
                    r /= k;
                    if (r > 255)
                        r = 255;
                    g /= k;
                    if (g > 255)
                        g = 255;
                    b /= k;
                    if (b > 255)
                        b = 255;

                    if (r < 0)
                        r = 0;
                    if (g < 0)
                        g = 0;
                    if (b < 0)
                        b = 0;

                    //Установка пикселя
                    writer.SetPixel(x, y, Color.FromArgb((int)r, (int)g, (int)b));
                }
            }

            reader.UnlockImage();
            writer.UnlockImage();

            return (Image)output;
        }
    }
}
