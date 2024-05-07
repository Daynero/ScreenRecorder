using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1TestFFMPEG
{
    internal class ScreenRecorder
    {
        private Rectangle _bounds;
        private const int ScreenWidth = 2560;
        private const int ScreenHeight = 1440;
        private string _tempPath = "";
        private readonly List<string> _inputImageSequence = new List<string>();
        private int _fileCount;

        public ScreenRecorder()
        {
            CreateTempFolder("tempScreens");
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
                //Save screenshot:
                string name = _tempPath + "//screenshot-" + _fileCount + ".png";
                bitmap.Save(name, ImageFormat.Png);
                _inputImageSequence.Add(name);
                _fileCount++;

                //Dispose of bitmap:
                bitmap.Dispose();
            }
        }
    }
}
