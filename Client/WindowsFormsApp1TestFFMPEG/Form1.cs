using System;
using System.Windows.Forms;

namespace WindowsFormsApp1TestFFMPEG
{
    public partial class Form1 : Form
    {
        private readonly ScreenRecorder _screenRec = new ScreenRecorder();
        private bool _isRecording = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void pbRec_Click(object sender, EventArgs e)
        {
            if (!_isRecording)
            {
                _screenRec.StartRecording();
                _isRecording = true;
                pbRec.Text = "Recording...";
            }
            else
            {
                _screenRec.StopRecording();
                _isRecording = false;
                pbRec.Text = "Start Recording";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_isRecording)
            {
                _screenRec.StopRecording();
                _isRecording = false;
                pbRec.Text = "Start Recording";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isRecording)
            {
                _screenRec.StopRecording();
            }
        }

        private void tmrRecord_Tick(object sender, EventArgs e)
        {
            // Не потрібно викликати _screenRec.TakeScreenShoot() тут
            // Цей метод викликається незалежно від таймера
        }
    }
}