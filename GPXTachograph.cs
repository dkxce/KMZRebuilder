using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace GPX_Tacho
{
    public partial class GPXTachograph : Form
    {
        KMZRebuilder.Tachograph t;
        KMZRebuilder.GPXReader gpxr;
        string fileName;
        int btwH = 0;

        public GPXTachograph()
        {
            InitializeComponent();

            cbb.SelectedIndex = 0;

            pic5.Image = global::KMZRebuilder.Properties.Resources._05DRIVER;
            pic6.Image = global::KMZRebuilder.Properties.Resources._06START;
            pic7.Image = global::KMZRebuilder.Properties.Resources._07FINISH;
            pic8.Image = global::KMZRebuilder.Properties.Resources._08KMSTART;
            pic9.Image = global::KMZRebuilder.Properties.Resources._09KMFINISH;
            pic10.Image = global::KMZRebuilder.Properties.Resources._10STARTD;
            pic11.Image = global::KMZRebuilder.Properties.Resources._11FINISHD;
            pic12.Image = global::KMZRebuilder.Properties.Resources._12VEHICLE;
        }

        private void MainWindowForm_Load(object sender, EventArgs e)
        {
            btwH = Height - textBox1.Height;

            t = new KMZRebuilder.Tachograph(pictureBox1.Width, pictureBox1.Height);
            
            FillFormText();
            FillTachogrammDefaults();
            pictureBox1.Image = t.Graph;
        }

        private void âûõîäToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainWindowForm_Resize(object sender, EventArgs e)
        {
            textBox1.Height = Height - btwH;

            t.Resize(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = t.Graph;
        }

        private void FillFormText()
        {
            txt5.Text = t.DriverText;
            
            txt6.Text = t.StartPointText;
            txt7.Text = t.FinishPointText;
            
            txt10.Text = t.StartDateText;
            txt11.Text = t.FinishDateText;
            
            txt12.Text = t.VehicleText;
            
            txtOdoStart.Text = t.StartKMText;
            txtOdoEnd.Text = t.FinishKMText;
            coet.Checked = t.CustomFinishOdometerText;
            
            tta.Text = t.TrackStartTime.ToString();
            ttb.Text = t.TrackFinishTime.ToString();
        }

        private void FillTachogrammDefaults()
        {
            t.AlignTextToCenter = tCent.Checked;
            t.SunriseTime = DateTime.Parse(ct1.Text);
            t.SunriseVisible = cbb.SelectedIndex == 1 || cbb.SelectedIndex == 3;
            t.SunsetTime = DateTime.Parse(ct2.Text);
            t.SunsetVisible = cbb.SelectedIndex == 2 || cbb.SelectedIndex == 3;
        }
      
        private void OpenFile(string file)
        {
            this.fileName = file;

            try
            {
                gpxr = new KMZRebuilder.GPXReader(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Couldn't open file `{0}`!\r\nError: {1}",System.IO.Path.GetFileName(fileName),ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            t = new KMZRebuilder.Tachograph(pictureBox1.Width, pictureBox1.Height);//
            t.DriverText = gpxr.TrackName;
            if (t.DriverText == "") t.DriverText = System.IO.Path.GetFileNameWithoutExtension(fileName);
            t.SetTrack(gpxr.TrackSegments.ToArray());

            FillFormText();
            FillTachogrammDefaults();
            pictureBox1.Image = t.Graph;

            textBox1.Text = "";
            textBox1.Text += String.Format("Track: {0} \r\n", gpxr.TrackName);
            textBox1.Text += String.Format("Type: {0} \r\n", gpxr.TrackType);
            textBox1.Text += String.Format("Segments: {0} \r\n\r\n", gpxr.TrackSegments.Count);

            ttc.Items.Clear();
            ttc.Items.Add(String.Format("All: [{0:dd.MM.yy HH:mm} - {1:dd.MM.yy HH:mm}]", gpxr.MinTime, gpxr.MaxTime));
            for (int i = 0; i < gpxr.TrackSegments.Count; i++)
            {
                ttc.Items.Add(String.Format("{0} seg: [{1:dd.MM.yy HH:mm} - {2:dd.MM.yy HH:mm}]", i + 1, gpxr.TrackSegments[i].MinTime, gpxr.TrackSegments[i].MaxTime));
                textBox1.Text += String.Format("Segment: {0}, {1} points \r\n", i + 1, gpxr.TrackSegments[i].points.Length);
                textBox1.Text += String.Format("  Start: {0}\r\n", gpxr.TrackSegments[i].MinTime);
                textBox1.Text += String.Format("  Finish: {0}\r\n", gpxr.TrackSegments[i].MaxTime);
                textBox1.Text += String.Format("  Min altitude: {0}ì\r\n", (int)gpxr.TrackSegments[i].MinEle);
                textBox1.Text += String.Format("  Max altitude: {0}ì\r\n", (int)gpxr.TrackSegments[i].MaxEle);
                textBox1.Text += String.Format("  Min speed: {0}êì/÷\r\n", (int)gpxr.TrackSegments[i].MinSpeed);
                textBox1.Text += String.Format("  Avg speed: {0}êì/÷\r\n", (int)gpxr.TrackSegments[i].AvgSpeed);
                textBox1.Text += String.Format("  Max speed: {0}êì/÷\r\n", (int)gpxr.TrackSegments[i].MaxSpeed);
                textBox1.Text += String.Format("  Total length: {0}êì\r\n", ((int)(gpxr.TrackSegments[i].TotalLength * 100)) / 100);
                textBox1.Text += String.Format("  Total time: {0}\r\n\r\n", gpxr.TrackSegments[i].TotalTime);
            };

            string desc = gpxr.TrackDesctiprion.Replace("\r\n","\r\n  ").Replace("<p>", "").Replace("</p>", "\r\n  ");
            desc = Regex.Replace(desc, @"(<br>|<br />|<br/>|</ br>|</br>)", "\r\n  ");
            desc = Regex.Replace(desc, "<.*?>", string.Empty);
            textBox1.Text += "Description:\r\n  " + desc;

            ttc.SelectedIndex = 0;
            tta.Enabled = true;
            ttb.Enabled = true;
            ttc.Enabled = true;
            bbd.Enabled = true;

            rof.Enabled = true;
            cfmnu.Enabled = true;
        }

        private void îòêðûòüToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "GPX files (*.gpx)|*.gpx|All types (*.*)|*.*";
            ofd.DefaultExt = ".gpx";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            OpenFile(ofd.FileName);
            ofd.Dispose();            
        }

        private void ñîõðàíèòüÈçîáðàæåíèåToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".png";
            sfd.Filter = "PNG Files (*.png)|*.png|Jpeg Files (*.jpg)|*.jpg|Bitmap Files (*.bmp)|*.bmp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(sfd.FileName);
            };
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            bool[] uptext = new bool[] { 
                tta.Text != t.TrackStartTime.ToString(), 
                ttb.Text != t.TrackFinishTime.ToString(), 
                t.StartPointText != txt6.Text,
                t.FinishPointText != txt7.Text,
                t.StartDateText != txt10.Text,
                t.FinishDateText != txt11.Text,
                t.StartKMText != txtOdoStart.Text,
                t.FinishKMText != txtOdoEnd.Text
            };
            if (ttc.SelectedIndex == 0)
                t.SetTrack(gpxr.TrackSegments.ToArray());
            else
            {
                t.RemoveTrack();
                t.AddSegment(gpxr.TrackSegments[ttc.SelectedIndex - 1]);
            };

            t.DriverText = txt5.Text;
            t.VehicleText = txt12.Text;
            
            if (uptext[0]) t.TrackStartTime = DateTime.Parse(tta.Text);
            if (uptext[1]) t.TrackFinishTime = DateTime.Parse(ttb.Text);
            if (uptext[2]) t.StartPointText = txt6.Text;
            if (uptext[3]) t.FinishPointText = txt7.Text;
            if (uptext[4]) t.StartDateText = txt10.Text;
            if (uptext[5]) t.FinishDateText = txt11.Text;
            if (uptext[6]) t.StartKMText = txtOdoStart.Text;
            if (coet.Checked)
                t.FinishKMText = txtOdoEnd.Text;
            else
                t.CustomFinishOdometerText = false;

            FillFormText(); 
            FillTachogrammDefaults();
            pictureBox1.Image = t.Graph;            
        }

        private void clearAll(object sender, EventArgs e)
        {
            textBox1.Clear();
            ttc.Items.Clear();
            
            tta.Enabled = false;
            ttb.Enabled = false;
            ttc.Enabled = false;
            bbd.Enabled = false;            

            gpxr = null;
            t = new KMZRebuilder.Tachograph(pictureBox1.Width, pictureBox1.Height);
            
            FillFormText();
            FillTachogrammDefaults();
            pictureBox1.Image = t.Graph;
        }

        private void rof_Click(object sender, EventArgs e)
        {
            OpenFile(fileName);
        }

        private void coet_CheckedChanged(object sender, EventArgs e)
        {
            txtOdoEnd.Enabled = coet.Checked;
        }

        private void cb1_Click(object sender, EventArgs e)
        {
            
        }

        private void cb2_Click(object sender, EventArgs e)
        {
            
        }

        private void cbb_SelectedIndexChanged(object sender, EventArgs e)
        {
            ct1.Enabled = cbb.SelectedIndex == 1 || cbb.SelectedIndex == 3;
            ct2.Enabled = cbb.SelectedIndex == 2 || cbb.SelectedIndex == 3;
        }

        private void ttc_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1_Click_1(sender, e);
        }       
    }   
}