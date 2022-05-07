using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Classes;
using ZumenSearch.Views;
using ZumenSearch.Common;
using System.Drawing.Drawing2D;
using Windows.Data.Pdf;
using System.Threading.Tasks;

namespace ZumenSearch.Common
{
    class Methods
    {
        #region == 画像操作メソッド ==

        // バイト配列をImageオブジェクトに変換
        public static System.Drawing.Image ByteArrayToImage(byte[] b)
        {
            ImageConverter imgconv = new ImageConverter();
            System.Drawing.Image img = (System.Drawing.Image)imgconv.ConvertFrom(b);
            return img;
        }

        // Imageオブジェクトをバイト配列に変換
        public static byte[] ImageToByteArray(System.Drawing.Image img)
        {
            ImageConverter imgconv = new ImageConverter();
            byte[] b = (byte[])imgconv.ConvertTo(img, typeof(byte[]));
            return b;
        }

        // バイト配列をBitmapImageオブジェクトに変換（Imageに表示するSource）
        public static BitmapImage BitmapImageFromBytes(Byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                BitmapImage bmimage = new BitmapImage();
                bmimage.BeginInit();
                bmimage.CacheOption = BitmapCacheOption.OnLoad;
                bmimage.StreamSource = stream;
                bmimage.EndInit();
                return bmimage;
            }
        }

        public static Byte[] BitmapImageToByteArray(BitmapImage bitmapImage)
        {
            byte[] data = null;

            try
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                //encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage, null, null, null));// https://stackoverflow.com/questions/16941520/exception-on-bitmapframe-create-bug-in-wpf-framework
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }
                /*
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }
                */
            }
            catch (System.NotSupportedException ex)
            {
                Debug.WriteLine("NotSupportedException: Methods@BitmapImageToByteArray(), " + ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: Methods@BitmapImageToByteArray(), " + ex.Message);
            }

            return data;
        }
        
        // System.Drawing.Image をBitmapImageオブジェクトに変換
        public static BitmapImage BitmapImageFromImage(System.Drawing.Image img, ImageFormat imf)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, imf);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        // 画像ファイルの拡張子からImageFormat形式を返す。
        public static ImageFormat FileExtToImageFormat(string fileext)
        {
            switch (fileext.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".gif":
                    return ImageFormat.Gif;
                default:
                    throw new Exception(String.Format("取り扱わない画像ファイルフォーマット: {0}", fileext));
            }

        }

        // サムネイル画像を縦横比を保ったまま、かつパディング付きで作成する。
        public static System.Drawing.Image FixedSize(System.Drawing.Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            // 背景を白でベタ塗。
            grPhoto.Clear(System.Drawing.Color.White);
            grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        #endregion

        #region == PDF操作メソッド ==

        public async static Task<BitmapImage> BitmapImageFromPdf(Byte[] bytes)
        {
            Windows.Data.Pdf.PdfDocument pdfDocument;
            
            using (var stream = new MemoryStream(bytes))
            {
                pdfDocument = await Windows.Data.Pdf.PdfDocument.LoadFromStreamAsync(stream.AsRandomAccessStream());

                if (pdfDocument != null)
                {
                    // 1ページ目を読み込む
                    using (Windows.Data.Pdf.PdfPage page = pdfDocument.GetPage(0))
                    {
                        BitmapImage image = new BitmapImage();

                        using (var IMRAStream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                        {
                            await page.RenderToStreamAsync(IMRAStream);

                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = IMRAStream.AsStream();
                            image.EndInit();
                        }

                        return image;

                        /*
                        // save to file.
                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(image));

                        //string filePath = @"C:\Users\hoge\Desktop\test.jpg";
                        using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create, FileAccess.Write))
                        {
                            encoder.Save(fileStream);
                        }
                        */
                    }
                }
            }

            return null;
        }

        #endregion

    }



}
