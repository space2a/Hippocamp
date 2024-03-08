using Re_Hippocamp.Misc;

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Re_Hippocamp
{
    [Serializable]
    public static class BO
    {
        private static bool debugging = false;

        public static byte[] ObjectToByteArray(object obj, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (debugging)
            {
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                HC.WriteLine("from:" + memberName + "()     source:" + sourceFilePath + "     line:" + sourceLineNumber);
                Console.ResetColor();
            }
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }


        public static object ByteArrayToObject(byte[] arrBytes, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (debugging)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                HC.WriteLine(arrBytes.Length + "bytes " + "     from:" + memberName + "()     source:" + sourceFilePath + "     line:" + sourceLineNumber);
                Console.ResetColor();
            }
            try
            {
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                object obj = (object)binForm.Deserialize(memStream);

                return obj;
            }
            catch (Exception ex)
            {
                HC.WriteLine(ex.ToString());
                return null;
            }
        }

        private static int loadedImages = 0;
        private static int loadedImagesURI = 0;
        public static BitmapImage LoadImage(byte[] imageData, int res, [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            loadedImages++;
            if (debugging)
            {
                Console.BackgroundColor = ConsoleColor.Magenta;
                HC.WriteLine("LoadImage " + loadedImages + " " + imageData.Length + "bytes     resolution:" + res + "     from:" + memberName + "(???)     source:" + sourceFilePath + "     line:" + sourceLineNumber);
                Console.ResetColor();
            }
            if (imageData == null || imageData.Length == 0) return null; //need to return a total black/gray image
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                try
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.DecodePixelWidth = res;
                    image.DecodePixelHeight = res;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            image.Freeze();
            return image;
        }

        public static byte[] ImageSourceToBytes(BitmapEncoder encoder, ImageSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }

    }
}
