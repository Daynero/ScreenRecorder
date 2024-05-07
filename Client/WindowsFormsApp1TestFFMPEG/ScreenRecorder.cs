using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Accord.Video.FFMPEG;
using System.Windows.Forms;

namespace WindowsFormsApp1TestFFMPEG
{
    internal class ScreenRecorder
    {
        //Video variables:
        private Rectangle bounds;
        int screenWidth = 2560;
        int screenHeight = 1440;
        private string outputPath = "";
        private string tempPath = "";
        private int fileCount = 1;
        private List<string> inputImageSequence = new List<string>();

        //File variables:
        private string audioName = "mic.wav";
        private string videoName = "video.mp4";
        private string finalName = "FinalVideo.mp4";

        //Time variable:
        Stopwatch watch = new Stopwatch();

        //Audio variables:
        public static class NativeMethods
        {
            [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        }

        public ScreenRecorder(string outPath)
        {
            //Create temporary folder for screenshots:
            CreateTempFolder("tempScreens");

            //Set variables:

            outputPath = outPath;
        }

        //Create temporary folder:   
        private void CreateTempFolder(string name)
        {
            string pathName = $"C://{name}";
            Directory.CreateDirectory(pathName);
            tempPath = pathName;
        }

        //Change final video name:
        public void setVideoName(string name)
        {
            finalName = name;
        }

        //Delete all files and directory:
        private void DeletePath(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            //Delete each file:
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            //Delete the path:
            foreach (string dir in dirs)
            {
                DeletePath(dir);
            }

            Directory.Delete(targetDir, false);
        }

        //Delete all files except the one specified:
        private void DeleteFilesExcept(string targetDir, string excDir)
        {
            string[] files = Directory.GetFiles(targetDir);

            //Delete each file except specified:
            foreach (string file in files)
            {
                if (file != excDir)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }
        }

        //Clean up program on crash:
        public void cleanUp()
        {
            if (Directory.Exists(tempPath))
            {
                DeletePath(tempPath);
            }
        }

        //Return elapsed time:
        public string getElapsed()
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
        }

        //Record video:
        public void RecordVideo()
        {
            //Keep track of time:
            watch.Start();
            bounds = Screen.PrimaryScreen.Bounds;
            using (Bitmap bitmap = new Bitmap(screenWidth, screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    //Add screen to bitmap:
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }
                //Save screenshot:
                string name = tempPath + "//screenshot-" + fileCount + ".png";
                bitmap.Save(name, ImageFormat.Png);
                inputImageSequence.Add(name);
                fileCount++;

                //Dispose of bitmap:
                bitmap.Dispose();
            }
        }

        //Record audio:
        public void RecordAudio()
        {
            NativeMethods.record("open new Type waveaudio Alias recsound", "", 0, 0);
            NativeMethods.record("record recsound", "", 0, 0);
        }

        //Save audio file:
        private void SaveAudio()
        {
            string audioPath = "save recsound " + outputPath + "//" + audioName;
            NativeMethods.record(audioPath, "", 0, 0);
            NativeMethods.record("close recsound", "", 0, 0);
        }

        //Save video file:
        private void SaveVideo(int width, int height, int frameRate)
        {
            using (VideoFileWriter vFWriter = new VideoFileWriter())
            {
                //Create new video file:
                vFWriter.Open(outputPath + "//" + videoName, width, height, frameRate, VideoCodec.MPEG4);

                //Make each screenshot into a video frame:
                foreach (string imageLocation in inputImageSequence)
                {
                    Bitmap imageFrame = System.Drawing.Image.FromFile(imageLocation) as Bitmap;
                    vFWriter.WriteVideoFrame(imageFrame);
                    imageFrame.Dispose();
                }

                //Close:
                vFWriter.Close();
            }
        }

        //Combine video and audio files:
        private void CombineVideoAndAudio(string video, string audio)
        {
            //FFMPEG command to combine video and audio:
            string args = $"/c ffmpeg -i \"{video}\" -i \"{audio}\" -shortest {finalName}";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                FileName = "cmd.exe",
                WorkingDirectory = outputPath,
                Arguments = args
            };

            //Execute command:
            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }

        public void Stop()
        {
            //Stop watch:
            watch.Stop();

            //Video variables:
            int width = bounds.Width;
            int height = bounds.Height;
            int frameRate = 10;

            //Save audio:
            SaveAudio();

            //Save video:
            SaveVideo(screenWidth, screenHeight, frameRate);

            //Combine audio and video files:
            CombineVideoAndAudio(videoName, audioName);

            //Delete the screenshots and temporary folder:
            DeletePath(tempPath);

            //Delete separated video and audio files:
            DeleteFilesExcept(outputPath, outputPath + "\\" + finalName);
        }
    }
}
