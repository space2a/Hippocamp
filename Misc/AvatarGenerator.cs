using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Re_Hippocamp.Misc
{

    public static class AvatarGenerator
    {

        public static int LEYEMax = 12;
        public static int REYEMax = 13;
        public static int RMouthMax = 15;

        public static BitmapSource generateAvatar(int leye = -1, int reye = -1, int mouth = -1, int fcolor = -1)
        {
            bool custom = false;
            int color = new Random().Next(0, 360);
            if (fcolor != -1)
            {
                color = fcolor;
                custom = true;
            }

            float colorac = 0.85f;

            Thread.Sleep(!custom ? 20 : 0);
            Bitmap lEye = new Bitmap(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Avatar/LEyes/"+ ( leye == -1 ? new Random().Next(1, LEYEMax + 1) : leye) + ".png")).Stream);
            Thread.Sleep(!custom ? 20 : 0);
            Bitmap rEye = new Bitmap(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Avatar/REyes/"+ (reye == -1 ? new Random().Next(1, REYEMax + 1) : reye) + ".png")).Stream);
            Thread.Sleep(!custom ? 20 : 0);
            Bitmap mouthb = new Bitmap(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Avatar/Mouths/" + (mouth == -1 ? new Random().Next(1, RMouthMax + 1) : mouth) + ".png")).Stream);

            Bitmap bitmap = new Bitmap(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Avatar/Base.png")).Stream); // <-- base
            for (int y = 0; y < bitmap.Height; y++)
            {
                var ac = HsvToColor(color, 0.6, colorac);
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var c = bitmap.GetPixel(x, y);
                    if (c.A != 0)
                        bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(c.A, ac.R, ac.G, ac.B));
                }

                colorac += 0.0007f;
            }
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(lEye, 0, 0);
                g.DrawImage(rEye, 0, 0);
                g.DrawImage(mouthb, 0, 0);
                //g.DrawImage(image2, image1.Width, 0);
            }



            //bitmap.Save(new Random().Next(0, 10000) + "xx.png");


            //create the bitmapsource from the bitmap variable and returns it
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        static System.Drawing.Color HsvToColor(double h, double s, double v)
        {
            int r, g, b;
            HsvToRgb(h, s, v, out r, out g, out b);
            return System.Drawing.Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }

        static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }

        static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }


    }

}
