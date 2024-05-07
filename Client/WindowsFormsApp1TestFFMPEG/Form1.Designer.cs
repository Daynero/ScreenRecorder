namespace WindowsFormsApp1TestFFMPEG
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pbRec = new System.Windows.Forms.Button();
            this.tmrRecord = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // pbRec
            // 
            this.pbRec.Location = new System.Drawing.Point(60, 60);
            this.pbRec.Name = "pbRec";
            this.pbRec.Size = new System.Drawing.Size(206, 83);
            this.pbRec.TabIndex = 0;
            this.pbRec.Text = "Start Recording";
            this.pbRec.UseVisualStyleBackColor = true;
            this.pbRec.Click += new System.EventHandler(this.pbRec_Click);
            // 
            // tmrRecord
            // 
            this.tmrRecord.Interval = 15;
            this.tmrRecord.Tick += new System.EventHandler(this.tmrRecord_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(336, 210);
            this.Controls.Add(this.pbRec);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button pbRec;
        private System.Windows.Forms.Timer tmrRecord;
    }
}

