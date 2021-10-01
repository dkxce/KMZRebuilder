using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KMLReport
{
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text files (*.txt)|*.txt|All Types (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = new System.IO.FileStream(ofd.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                System.IO.StreamReader sr = new System.IO.StreamReader(fs, Encoding.UTF8);
                Repord.Text = sr.ReadToEnd();
                sr.Close();
                fs.Close();
            };
            ofd.Dispose();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "Text files (*.txt)|*.txt|All Types (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = new System.IO.FileStream(ofd.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, Encoding.UTF8);
                sw.Write(Repord.Text);
                sw.Close();
                fs.Close();
            };
            ofd.Dispose();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Repord.Text = ";ignore [FIELD_NAME] FIELD_TEXT REGEX\r\n#ignore\r\n\r\n[LAYER]\r\n{layer}\r\nregex=\r\n\r\n[NAME]\r\n{name}\r\nregex=\r\n\r\n[LATITUDE]\r\n{latitude}\r\nregex=\r\n\r\n[LONGITUDE]\r\n{longitude}\r\nregex=\r\n\r\n;[DESCRIPTION]\r\n{description}\r\nregex=\r\n\r\n[ZONE]\r\n{description}\r\nwebsite=[\\S\\s]+.(ru|ua|com)\r\n\r\n[WEBSITE]\r\n{description}\r\nregex=website=([\\S\\s][^\\r\\n]+)\r\n\r\n[PHONE]\r\n{description}\r\nregex=phone=([\\S\\s][^\\r\\n]+)\r\n\r\n[MAIL]\r\n{description}\r\nregex=email=([\\S\\s][^\\r\\n]+)";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Repord.Text = "";
        }
    }
}