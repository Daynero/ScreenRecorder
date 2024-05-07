using System;
using System.Windows.Forms;

namespace WindowsFormsApp1TestFFMPEG
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        readonly ScreenRecorder _screenRec = new ScreenRecorder();

        private void pbRec_Click(object sender, EventArgs e)
        {
            tmrRecord.Start();
            pbRec.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pbRec.Hide();
            Application.Restart();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void tmrRecord_Tick(object sender, EventArgs e)
        {
            _screenRec.TakeScreenShoot();
        }
    }
}
