using System;
using System.Drawing;
using System.IO;
using System.Windows;

namespace Re_Hippocamp.Misc
{

    public static class SongThumbnail
    {
        public static byte[] createThumbnail(HSong hSong, HAlbum hAlbum)
        {

            Bitmap HPLogo = new Bitmap(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Images/HippocampLogoTN.png")).Stream);

            Bitmap bitmap = new Bitmap(256, 256); //album/song cover

            byte[] pData = MainWindow.getSongOrAlbumCover(hSong, hAlbum);
            if (pData == null) { pData = MainWindow.loadedProfile.Avatar; }
            using (var ms = new MemoryStream(pData))
            {
                bitmap = new Bitmap(ms);
            }

            //resize bitmap to 256x256!!!!

            Bitmap result = new Bitmap(256, 256);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bitmap, 0, 0, 256, 256);
            }

            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(HPLogo, 0, 0);
            }

            return ImageToByte(result);
        }

        public static byte[] ImageToByte(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

    }


}
