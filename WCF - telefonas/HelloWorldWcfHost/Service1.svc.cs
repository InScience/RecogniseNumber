using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using recognize;
using openalprnet;


namespace HelloWorldWcfHost
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "HelloWorldService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select HelloWorldService.svc or HelloWorldService.svc.cs at the Solution Explorer and start debugging.
   
        public class HelloWorldService : IHelloWorldService
        {

        public string client_id = "45829";
        public string api_key = "e930d67e83";
        public string clapi_key = "93bd3f28c8933b2b5442db990194b150";


        public HelloWorldData GetHelloData(HelloWorldData helloWorldData)
            {
                if (helloWorldData == null)
                {
                    throw new ArgumentNullException("helloWorldData");
                }

                if (helloWorldData.SayHello)
                {
                    helloWorldData.Name = String.Format("Hello World to {0}.", helloWorldData.Name);
                }
                return helloWorldData;
        }

        //public List<string> Recognize(byte[] byteArr)
        //{
        //    List<string> RecInfo = new List<string>();

        //    Bitmap Bit;
        //    Image img = byteArrayToImage(byteArr);

        //    CheckOrientation(img);
        //    Bit = resizeImageAspect(640, 480, img);
        //    byteArr = ConvertBitToJpeg(byteArr, Bit);

        //    recognizeProxy proxy = new recognize.recognizeProxy(client_id, api_key, clapi_key);
        //    // proxy.modeChange(RecognitionMode.multi);
        //    recognitionResponse response4 = proxy.recognize1(byteArr, RecognitionMode.single, false);

        //    string responseStatus = "Kažkas negerai";
        //    string responseMessage = "labai negerai";

        //    responseStatus = response4.status.ToString();
        //    responseMessage = response4.message;

        //    RecInfo.Add(responseStatus);
        //    RecInfo.Add(responseMessage);

        //    if (response4.objects != null)
        //    {
        //        foreach (var obj in response4.objects)
        //        {
        //            string id = obj.id;
        //            string name = obj.name;
        //            RecInfo.Add(id);
        //            RecInfo.Add(name);
        //        }
        //    }

        //    return RecInfo;
        //}


        public List<string> Recognize(byte[] byteArr)
        {
            List<string> RecInfo = new List<string>();

            Bitmap Bit;
            Image img = byteArrayToImage(byteArr);

            CheckOrientation(img);
            Bit = resizeImageAspect(640, 480, img);
            byteArr = ConvertBitToJpeg(byteArr, Bit);

            var alpr = new AlprNet("eu", @"C:\Users\Laurynas\Documents\Visual Studio 2015\Projects\ConsoleAlpr\ConsoleAlpr\bin\Debug\openalpr.conf", @"C:\Users\Laurynas\Documents\Visual Studio 2015\Projects\ConsoleAlpr\ConsoleAlpr\bin\Debug\runtime_data");
          
      
            if (!alpr.IsLoaded())
            {
                string fail = "OpenAlpr failed to load!";
                RecInfo.Add(fail);
                return RecInfo;
            }
          
            var results = alpr.Recognize(byteArr);
            int i = 0;
            string platenumber;
            foreach (var result in results.Plates)
            {
              
                foreach (var plate in result.TopNPlates)
                {
                  
                    platenumber = "Plate: " + plate.Characters + "Confidence: " + plate.OverallConfidence;
                    RecInfo.Add(platenumber);

                }
            }



            return RecInfo;
        }


        public string SayHelloTo(string name)
            {
                return string.Format("Hello World to you, {0}", name);
            }

            public string[] NameList(string[] name)
            {

               for(int i =0; i < name.Length; i++)
               {
              name[i] = string.Format("MODIFIED, {0}", name[i]);
              name[i] = addmore(name[i]);
               }

            return name;
            }

            string addmore(string name)
            {

            name = string.Format("DOUBLE MODYFIED, {0}", name);

            return name;
            }

             public byte[] ProcessImage(byte[] byteArray)
            {
            Bitmap Bit;
            Image img = byteArrayToImage(byteArray);


            CheckOrientation(img);

            Bit = ResizeImageStrech(img, 640, 480);

            byteArray = ConvertBitToJpeg(byteArray, Bit);


            return byteArray;
            }

        public static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public static byte[] ConvertBitToJpeg(byte[] bytearr, Bitmap Bit) {

            Image img = byteArrayToImage(bytearr);


            using (MemoryStream mStream = new MemoryStream())
            {
                Bit.Save(mStream, ImageFormat.Jpeg);
                bytearr = mStream.ToArray();
            }

            return bytearr;
        }

        static void CheckOrientation(Image img)
        {
            int orient = -1;

            try
            {
                int orientation = img.GetPropertyItem(274).Value[0];
                orient = orientation;

                if (orient > -1)
                {
                    img = RemoveOrientationExifTag(img);
                    Console.WriteLine("Orientation Tag pašalintas");
                }
            }

            catch
            {
                Console.WriteLine("Orientation Tag nerastas");
            }


        }



        static Image RemoveOrientationExifTag(Image Img)
        {
            if (Array.IndexOf(Img.PropertyIdList, 274) > -1)
            {
                var orientation = (int)Img.GetPropertyItem(274).Value[0];
                switch (orientation)
                {
                    case 1:
                        break;
                    case 2:
                        Img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        Img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        Img.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        Img.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        Img.RotateFlip(RotateFlipType.Rotate90FlipNone); // ištestuotas
                        break;
                    case 7:
                        Img.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        Img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                Img.RemovePropertyItem(274); //pašalinamas EXIF tagas
             //   Debug.WriteLine(Img.Width + " " + Img.Height);
            }
            return Img;
        }


        public static Bitmap ResizeImageStrech(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            float xDpi = image.HorizontalResolution;
            float yDpi = image.VerticalResolution;

            destImage.SetResolution(xDpi, yDpi);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        public static Bitmap resizeImageAspect(int newWidth, int newHeight, Image imgPhoto)
        {

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;

            if (sourceWidth < sourceHeight)   //  Jei vertikali
            {
                int buff = newWidth;

                newWidth = newHeight;
                newHeight = buff;
            }

            int sourceX = 0, sourceY = 0, destX = 0, destY = 0;
            float nPercent = 0, nPercentW = 0, nPercentH = 0;

            nPercentW = ((float)newWidth / (float)sourceWidth);
            nPercentH = ((float)newHeight / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((newWidth -
                          (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((newHeight -
                          (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(newWidth, newHeight,
                          System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                         imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(System.Drawing.Color.White);
            grPhoto.InterpolationMode =
            System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            imgPhoto.Dispose();

            return bmPhoto;
        }


    }

}
