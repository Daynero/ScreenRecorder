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
        private bool _isRecording;
        private bool _isSending;

        public ScreenRecorder()
        {
            CreateTempFolder("tempScreens");
            _isRecording = false;
            _isSending = false;
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
            // Start taking screenshots
            Task.Run(() => TakeScreenshots());
            // Start sending images to the server
            Task.Run(() => SendImagesToServer());
        }

        public void StopRecording()
        {
            _isRecording = false;
        }

        private void TakeScreenshots()
        {
            while (_isRecording)
            {
                _bounds = Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(ScreenWidth, ScreenHeight))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // Add screen to bitmap
                        g.CopyFromScreen(new Point(_bounds.Left, _bounds.Top), Point.Empty, _bounds.Size);
                    }
                    // Save screenshot to queue
                    string name = _tempPath + "//screenshot-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
                    bitmap.Save(name, ImageFormat.Png);
                    _imageQueue.Enqueue(name);

                    // Dispose of bitmap
                    bitmap.Dispose();
                }
            }
        }

        private async Task SendImagesToServer()
        {
            // Continue sending images while the application is recording or there are images in the queue
            while (_isRecording || _imageQueue.Count > 0)
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
                        // Delete the sent image locally
                        File.Delete(imagePath);
                    }
                    else
                    {
                        // Retry sending the image after some time
                        await Task.Delay(5000); // 5 seconds delay
                    }
                }
                else
                {
                    // No more images in the queue, wait before checking again
                    await Task.Delay(1000); // 1 second delay
                }
            }

            // No more images in the queue and recording stopped, set sending flag to false
            _isSending = false;
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
