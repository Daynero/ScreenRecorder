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
        private Rectangle _bounds;
        private const int ScreenWidth = 2560;
        private const int ScreenHeight = 1440;
        private string _tempPath = "";
        private readonly Queue<string> _imageQueue = new Queue<string>();
        private bool _isSending;

        public ScreenRecorder()
        {
            CreateTempFolder("tempScreens");
            _isSending = false;
        }

        private void CreateTempFolder(string name)
        {
            string pathName = $"C://{name}";
            Directory.CreateDirectory(pathName);
            _tempPath = pathName;
        }

        public void TakeScreenShoot()
        {
            _bounds = Screen.PrimaryScreen.Bounds;
            using (Bitmap bitmap = new Bitmap(ScreenWidth, ScreenHeight))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    //Add screen to bitmap:
                    g.CopyFromScreen(new Point(_bounds.Left, _bounds.Top), Point.Empty, _bounds.Size);
                }
                //Save screenshot to queue:
                string name = _tempPath + "//screenshot-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
                bitmap.Save(name, ImageFormat.Png);
                _imageQueue.Enqueue(name);

                //Dispose of bitmap:
                bitmap.Dispose();
            }

            // Start sending images if not already sending
            if (!_isSending)
                SendNextImage();
        }

        private async void SendNextImage()
        {
            // If there are images in the queue
            if (_imageQueue.Count > 0)
            {
                _isSending = true;
                string imagePath = _imageQueue.Peek();
                byte[] imageBytes = File.ReadAllBytes(imagePath);

                // Try to send the image
                bool success = await SendImageToServer(imageBytes, "http://localhost:8080/");
                if (success)
                {
                    // Remove the sent image from the queue
                    _imageQueue.Dequeue();
                    // Continue sending next image
                    SendNextImage();
                }
                else
                {
                    // Retry sending the image after some time
                    await Task.Delay(5000); // 5 seconds delay
                    SendNextImage();
                }
            }
            else
            {
                // No more images in the queue, reset sending flag
                _isSending = false;
            }
        }

        private async Task<bool> SendImageToServer(byte[] imageBytes, string serverUrl)
        {
            try
            {
                // Create a request to the server
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverUrl);
                request.Method = "POST";
                request.ContentType = "application/octet-stream"; // Arbitrary data type

                // Get the stream to send data to the server
                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    // Send the image bytes
                    await requestStream.WriteAsync(imageBytes, 0, imageBytes.Length);
                }

                // Get the response after sending the request
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    // Get the status code of the response
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
