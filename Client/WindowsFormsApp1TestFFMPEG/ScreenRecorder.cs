using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1TestFFMPEG
{
    internal class ScreenRecorder
    {
        private static readonly int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
        private static readonly int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
        private string _tempPath = "";
        private readonly Queue<string> _imageQueue = new Queue<string>();
        private bool _isRecording;

        public ScreenRecorder()
        {
            CreateTempFolder("tempScreens");
            _isRecording = false;
        }

        private void CreateTempFolder(string name)
        {
            string pathName = $"C://{name}";
            Directory.CreateDirectory(pathName);
            _tempPath = pathName;
        }

        public void StartRecording()
        {
            _isRecording = true;
            Task.Run(TakeScreenshots);
            Task.Run(SendImagesToServer);
        }

        public void StopRecording()
        {
            _isRecording = false;
        }

        private void TakeScreenshots()
        {
            while (_isRecording)
            {
                using (Bitmap bitmap = new Bitmap(ScreenWidth, ScreenHeight))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, new Size(ScreenWidth, ScreenHeight));
                    }
                    string name = _tempPath + "//screenshot-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
                    bitmap.Save(name, ImageFormat.Png);
                    _imageQueue.Enqueue(name);

                    bitmap.Dispose();
                }
            }
        }

        private async Task SendImagesToServer()
        {
            while (_isRecording || _imageQueue.Count > 0)
            {
                if (_imageQueue.Count > 0)
                {
                    string imagePath = _imageQueue.Peek();
                    byte[] imageBytes = File.ReadAllBytes(imagePath);

                    bool success = await SendImageToServer(imageBytes, "http://localhost:8080/");
                    if (success)
                    {
                        _imageQueue.Dequeue();
                        File.Delete(imagePath);
                    }
                    else
                    {
                        await Task.Delay(2000); // 2 seconds delay
                    }
                }
                else
                {
                    await Task.Delay(1000); // 1 second delay
                }
            }
        }

        private async Task<bool> SendImageToServer(byte[] imageBytes, string serverUrl)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverUrl);
                request.Method = "POST";
                request.ContentType = "application/octet-stream"; // Arbitrary data type

                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(imageBytes, 0, imageBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
