using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KMZRebuilder
{
    public partial class CSVTXTForm : Form
    {
        private EncodingInfo[] ei = Encoding.GetEncodings();
        private Stream fs = null;
        private bool preaload = true;

        public CSVTXTForm(Stream file)
        {
            InitializeComponent();

            fs = file;
            for (int i = 0; i < ei.Length; i++)
            {
                codepage.Items.Add(String.Format("{0} - {1}", ei[i].CodePage, ei[i].DisplayName));
                if (file is MemoryStream)
                {
                    if (ei[i].CodePage == 65001) codepage.SelectedIndex = i;
                }
                else
                {
                    if (ei[i].CodePage == 1251) codepage.SelectedIndex = i;
                };
            };
            if (codepage.SelectedIndex == -1) codepage.SelectedIndex = ei.Length - 1;
            delimiter.SelectedIndex = 0;
            if (file is MemoryStream)
            {
                this.Text = "Importing Clipboard ...";
                delimiter.SelectedIndex = delimiter.Items.Count - 1;
            };
            flh.SelectedIndex = 0;
            separator.SelectedIndex = 0;
            SD.FullRowSelect = true;
            preaload = false;
        }

        public EncodingInfo CodePage
        {
            get
            {
                return ei[codepage.SelectedIndex];
            }
        }

        private void OnChange()
        {
            if (preaload) return;
            CSVTXTForm form = this;

            fs.Position = 0;
            StreamReader sr = new StreamReader(fs, form.CodePage.GetEncoding());
            string[] sep = form.skipsw.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            char cd = form.delimiter.Text[0];
            if (form.delimiter.Text == "TAB") cd = '\t';
            bool he = form.flh.Text == "YES";
            int c = 20;
            form.SD.Clear();
            while ((!sr.EndOfStream) && (c > 0))
            {
                string line = sr.ReadLine().Trim();
                if (String.IsNullOrEmpty(line)) continue;
                bool skip = false;
                if ((sep != null) && (sep.Length > 0))
                    for (int i = 0; i < sep.Length; i++)
                        if (line.StartsWith(sep[i]))
                            skip = true;
                if (skip) continue;

                string[] cells = line.Split(new char[] { cd });
                while (form.SD.Columns.Count < cells.Length)
                {
                    form.SD.Columns.Add("COL " + (SD.Columns.Count + 1).ToString());
                    form.SD.Columns[form.SD.Columns.Count - 1].Width = 80;
                };
                if (he && (c == 20)) // header
                {
                    for (int i = 0; i < cells.Length; i++)
                        form.SD.Columns[i].Text = cells[i];
                    he = false;
                    continue;
                };
                ListViewItem lvi = new ListViewItem(cells[0]);
                if (cells.Length > 1)
                    for (int i = 1; i < cells.Length; i++)
                        lvi.SubItems.Add(cells[i]);
                form.SD.Items.Add(lvi);
                c--;
            };
            ////////
            {
                int sifn = fName.SelectedIndex;
                int sidd = fDesc.SelectedIndex;
                int silt = fLat.SelectedIndex;
                int siln = fLon.SelectedIndex;
                int sist = fStyle.SelectedIndex;
                form.fName.Items.Clear();
                form.fDesc.Items.Clear();
                form.fLat.Items.Clear();
                form.fLon.Items.Clear();
                form.fStyle.Items.Clear();
                form.fName.Items.Add("--NONE--");
                form.fDesc.Items.Add("--NONE--");
                form.fLat.Items.Add("--NONE--");
                form.fLon.Items.Add("--NONE--");
                form.fStyle.Items.Add("--NONE--");
                if(form.SD.Columns.Count > 0)
                    for (int i = 0; i < form.SD.Columns.Count; i++)
                    {
                        if ((sifn == -1) && (form.SD.Columns[i].Text.ToUpper() == "NAME")) sifn = i + 1;
                        if ((sidd == -1) && (form.SD.Columns[i].Text.ToUpper().StartsWith("DESC"))) sidd = i + 1;
                        if ((silt == -1) && (form.SD.Columns[i].Text.ToUpper().StartsWith("LAT"))) silt = i + 1;
                        if ((siln == -1) && (form.SD.Columns[i].Text.ToUpper().StartsWith("LON"))) siln = i + 1;
                        //if ((sist == -1) && (form.SD.Columns[i].Text.ToUpper().Contains("TYPE"))) sist = i + 1;
                        form.fName.Items.Add(form.SD.Columns[i].Text);
                        form.fDesc.Items.Add(form.SD.Columns[i].Text);
                        form.fLat.Items.Add(form.SD.Columns[i].Text);
                        form.fLon.Items.Add(form.SD.Columns[i].Text);
                        form.fStyle.Items.Add(form.SD.Columns[i].Text);
                    };
                if (sifn < form.fName.Items.Count) form.fName.SelectedIndex = sifn;
                if (sidd < form.fDesc.Items.Count) form.fDesc.SelectedIndex = sidd;
                if (silt < form.fLat.Items.Count) form.fLat.SelectedIndex = silt;
                if (siln < form.fLon.Items.Count) form.fLon.SelectedIndex = siln;
                if (sist < form.fStyle.Items.Count) form.fStyle.SelectedIndex = sist;
                
            };
            sr = null;
        }

        private void codepage_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnChange();
        }

        private void skipsw_TextChanged(object sender, EventArgs e)
        {
            OnChange();
        }

        private void CSVTXTForm_Shown(object sender, EventArgs e)
        {
            OnChange();
        }

        private void CSVTXTForm_Load(object sender, EventArgs e)
        {

        }

        private void fName_SelectedIndexChanged(object sender, EventArgs e)
        {
            OK.Enabled =
                (fName.SelectedIndex > 0) && (fLat.SelectedIndex > 0) && (fLon.SelectedIndex > 0);
        }

        private int sCol = -1;
        private void SD_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            sCol = e.Column;
            s1ToolStripMenuItem.Text = SD.Columns[sCol].Text + " as Field Name";
            s2ToolStripMenuItem.Text = SD.Columns[sCol].Text + " as Field Description";
            s3ToolStripMenuItem.Text = SD.Columns[sCol].Text + " as Field Latitude";
            s4ToolStripMenuItem.Text = SD.Columns[sCol].Text + " as Field Longitude";
            contextMenuStrip1.Show(Cursor.Position);
        }

        private void SD_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hti = SD.HitTest(e.Location);
            if (hti.Item == null) return;

            ListViewItem.ListViewSubItem subitem = hti.SubItem;
            for (int i = 0; i < hti.Item.SubItems.Count; i++)
                if (hti.Item.SubItems[i] == subitem)
                    sCol = i;
        }

        private void s1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fName.SelectedIndex = sCol + 1;
        }

        private void s2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fDesc.SelectedIndex = sCol + 1;
        }

        private void s3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fLat.SelectedIndex = sCol + 1;
        }

        private void s4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fLon.SelectedIndex = sCol + 1;
        }
    }
}