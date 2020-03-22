using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class ImageProcess
    {
        int counter = 0;

        /// <summary>
        /// 清空目的目錄下的所有檔案與目錄
        /// </summary>
        /// <param name="destPath">目錄路徑</param>
        public void Clean(string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var allImageFiles = Directory.GetFiles(destPath, "*", SearchOption.AllDirectories);

                foreach (var item in allImageFiles)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// 進行圖片的縮放作業
        /// </summary>
        /// <param name="sourcePath">圖片來源目錄路徑</param>
        /// <param name="destPath">產生圖片目的目錄路徑</param>
        /// <param name="scale">縮放比例</param>
        public Task ResizeImagesAsync(string sourcePath, string destPath, double scale)
        {
            Log("start-proccess");
            var tasks = new List<Task>();
            var allFiles = FindImages(sourcePath);

            foreach (var filePath in allFiles)
            {
                tasks.Add(ResizeImageAsync(filePath, destPath, scale));
            }

            return Task.WhenAll(tasks);
        }

        async Task ResizeImageAsync(string filePath, string destPath, double scale)
        {
            var index = counter++;
            Stopwatch sw = new Stopwatch();

            // < 20ms : dont need to be async
            Image imgPhoto = Image.FromFile(filePath);
            string imgName = Path.GetFileNameWithoutExtension(filePath);

            var options = new ResizeOptions
            {
                DestionatonPath = Path.Combine(destPath, imgName + ".jpg"),
                SourceWidth = imgPhoto.Width,
                SourceHeight = imgPhoto.Height,
                DestinationWidth = (int)(imgPhoto.Width * scale),
                DestionatonHeight = (int)(imgPhoto.Height * scale)
            };

            sw.Start();
            // 200 ~ 1500ms
            await ProcessBitmapAsync((Bitmap)imgPhoto, options, index);
            sw.Stop();
            Log("end-process-bitmap", index, sw.ElapsedMilliseconds);
            sw.Reset();
        }

        /// <summary>
        /// 找出指定目錄下的圖片
        /// </summary>
        /// <param name="srcPath">圖片來源目錄路徑</param>
        /// <returns></returns>
        public List<string> FindImages(string srcPath)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(srcPath, "*.jpeg", SearchOption.AllDirectories));
            return files;
        }

        Task ProcessBitmapAsync(Bitmap img, ResizeOptions options, int index)
        {
            return Task.Run(() =>
            {
                Log("start-process-bit", index);
                var newImg = ProcessBitmap(img, options);
                newImg.Save(options.DestionatonPath, ImageFormat.Jpeg);
            });
        }

        /// <summary>
        /// 針對指定圖片進行縮放作業
        /// </summary>
        /// <param name="img">圖片來源</param>
        /// <param name="srcWidth">原始寬度</param>
        /// <param name="srcHeight">原始高度</param>
        /// <param name="newWidth">新圖片的寬度</param>
        /// <param name="newHeight">新圖片的高度</param>
        /// <returns></returns>
        Bitmap ProcessBitmap(Bitmap img, ResizeOptions options)
        {
            Bitmap resizedbitmap = new Bitmap(options.DestinationWidth, options.DestionatonHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, options.DestinationWidth, options.DestionatonHeight),
                new Rectangle(0, 0, options.SourceWidth, options.SourceHeight),
                GraphicsUnit.Pixel);
            return resizedbitmap;
        }

        void Log(string Name, int index = -1, long ms = -1)
        {
            var ms_display = ms.ToString().PadRight(8);
            var index_display = index.ToString().PadRight(3);
            var name_display = Name.PadRight(30);
            var threadId_display = Thread.CurrentThread.ManagedThreadId.ToString().PadRight(3);
            Console.WriteLine($"{index_display}:{name_display}  >>>>>> TID: {threadId_display} >>> spent:{ms_display}");
        }
    }
}
