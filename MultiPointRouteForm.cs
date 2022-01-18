using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace KMZRebuilder
{
    public partial class MultiPointRouteForm : Form
    {
        public List<NaviMapNet.MapPoint> OnMapPoints = new List<NaviMapNet.MapPoint>();

        object temp;
        int trackingItem;
        bool trackingChecked;

        public MultiPointRouteForm()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        private const int WS_SYSMENU = 0x80000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= ~WS_SYSMENU;
                return cp;
            }
        }     

        public void Clear()
        {
            this.pbox.Items.Clear();
        }

        public int Count
        {
            get
            {
                return pbox.Items.Count;
            }
        }

        public List<KeyValuePair<string, PointF>> Points
        {
            get
            {
                List<KeyValuePair<string, PointF>> cpts = new List<KeyValuePair<string, PointF>>();
                for (int i = 0; i < pbox.CheckedItems.Count; i++)
                {
                    KeyValuePair<string, PointF> p = (KeyValuePair<string, PointF>)pbox.CheckedItems[i];
                    cpts.Add(p);
                };
                return cpts;
            }
            set
            {
                InitPoints(value);
            }
        }

        public void AddPoint(KeyValuePair<string, PointF> point, NaviMapNet.MapPoint mapPoint)
        {
            pbox.Items.Add(new KeyValuePair<string, PointF>(String.Format("{0:00} - {1}", pbox.Items.Count + 1, point.Key), point.Value), true);
            OnMapPoints.Add(mapPoint);
        }

        private void InitPoints(List<KeyValuePair<string, PointF>> points)
        {
            pbox.Items.Clear();
            if ((points == null) || (points.Count == 0)) return;
            for (int i = 0; i < points.Count; i++)
                pbox.Items.Add(points[i], true);
        }

        public void MoveUp()
        {
            MoveItem(-1);
        }

        public void MoveDown()
        {
            MoveItem(1);
        }

        public void MoveItem(int direction)
        {
            if (pbox.SelectedItem == null || pbox.SelectedIndex < 0) return;
            int newIndex = pbox.SelectedIndex + direction;
            if (newIndex < 0 || newIndex >= pbox.Items.Count) return;
            object selected = pbox.SelectedItem;
            pbox.Items.Remove(selected);
            pbox.Items.Insert(newIndex, selected);
            pbox.SelectedIndex = newIndex;
            pbox.SetItemChecked(newIndex, true);
        }

        private void pbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Alt)
            {
                if (e.KeyValue == 38) { MoveUp(); e.Handled = false; };
                if (e.KeyValue == 40) { MoveDown(); e.Handled = false; };
            };
        }

        private void pbox_MouseDown(object sender, MouseEventArgs e)
        {
            if (pbox.SelectedIndex < 0) return;

            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {                
                pbox.Cursor = Cursors.Hand;
                trackingItem = pbox.SelectedIndex;
                if (trackingItem >= 0)
                {
                    temp = pbox.Items[pbox.SelectedIndex];
                    trackingChecked = pbox.GetItemChecked(pbox.SelectedIndex);
                    pbox.MovingItem = temp;
                };
            };
        }

        private void pbox_MouseUp(object sender, MouseEventArgs e)
        {
            if (pbox.SelectedIndex < 0) return;
            if (temp != null)
            {                
                int tempInd = pbox.SelectedIndex;
                if ((tempInd >= 0) && (trackingItem != tempInd))
                {
                    pbox.Items.RemoveAt(trackingItem);
                    pbox.Items.Insert(tempInd, temp);
                    pbox.SelectedIndex = tempInd;
                    pbox.SetItemChecked(tempInd, trackingChecked);
                };
                pbox.Cursor = Cursors.Default;
                temp = null;
                pbox.MovingItem = null;
            };
        }

        private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < pbox.Items.Count; i++)
                pbox.SetItemChecked(i, true);
        }

        private void checkNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < pbox.Items.Count; i++)
                pbox.SetItemChecked(i, false);
        }

        private void inverseCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < pbox.Items.Count; i++)
                pbox.SetItemChecked(i, !pbox.GetItemChecked(i));
        }

        private void deleteCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = pbox.Items.Count - 1; i >= 0; i--)
                if(pbox.GetItemChecked(i))
                    pbox.Items.RemoveAt(i);
        }

        private void deleteUncheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = pbox.Items.Count - 1; i >= 0; i--)
                if (!pbox.GetItemChecked(i))
                    pbox.Items.RemoveAt(i);
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pbox.Items.Clear();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            exportRoutePointsToolStripMenuItem.Enabled = pbox.Items.Count > 0;
        }

        private void exportRoutePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pbox.Items.Count == 0) return;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Export Route Points";
            sfd.Filter = "KMZ files (*.kml)|*.kml|KMZ files (*.kmz)|*.kmz";
            sfd.FileName = "ExportedRoutePoints.kml";
            string fName = null;
            int filter = 0;
            if (sfd.ShowDialog() == DialogResult.OK) { fName = sfd.FileName; filter = sfd.FilterIndex; };
            sfd.Dispose();
            if (String.IsNullOrEmpty(fName)) return;
            if (filter == 1) SaveToKML(fName);
            if (filter == 2) SaveToKMZ(fName);
        }

        public void SaveToKML(string fName)
        {
            System.IO.FileStream fs = new System.IO.FileStream(fName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
            sw.WriteLine("<kml xmlns='http://www.opengis.net/kml/2.2'><Document>");
            sw.WriteLine("<name>Exported Route Points</name><Folder><name>Exported Route Points</name>");
            foreach(KeyValuePair<string, PointF> np in this.Points)
            {
                sw.WriteLine("<Placemark><name><![CDATA[" + np.Key + "]]></name>");
                sw.WriteLine("<description><![CDATA[" + np.Value.ToString() + "]]></description>");
                sw.WriteLine("<Point><coordinates>" + np.Value.X.ToString().Replace(",", ".") + "," + np.Value.Y.ToString().Replace(",", ".") + ",0.0</coordinates></Point></Placemark>");
            };
            sw.WriteLine("</Folder>");
            sw.WriteLine("</Document></kml>");
            sw.Close();
            fs.Close();
        }

        public void SaveToKMZ(string fName)
        {
            string inzip = fName + ".kml";
            SaveToKML(inzip);

            FileStream fsOut = File.Create(fName);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);
            zipStream.SetComment("Created by KMZRebuilder");
            zipStream.SetLevel(3);
            // doc.kml
            {
                FileInfo fi = new FileInfo(inzip);
                ZipEntry newEntry = new ZipEntry("doc.kml");
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity
                newEntry.Size = fi.Length;
                zipStream.PutNextEntry(newEntry);

                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(fi.FullName))
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                zipStream.CloseEntry();
            };
            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();

            File.Delete(inzip);
        }

        private void importRoutePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Import Route Points";
            ofd.Filter = "KML & KMZ files (*.kml,*.kmz)|*.kml;*.kmz";
            string fName = null;
            if (ofd.ShowDialog() == DialogResult.OK) fName = ofd.FileName;
            ofd.Dispose();
            if (String.IsNullOrEmpty(fName)) return;
            LoadFromKMLZ(fName);            
        }

        public void LoadFromKMLZ(string fName)
        {
            FileStream fs = new FileStream(fName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            string xml = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            xml = RemoveXMLNamespaces(xml);
            XmlDocument kmlDoc = new XmlDocument();
            kmlDoc.LoadXml(xml);
            XmlNodeList xnf = kmlDoc.SelectNodes("kml/Document/Folder/Placemark");
            foreach (XmlNode xp in xnf)
            {
                XmlNode xnn = xp.SelectNodes("Point/coordinates")[0];
                if (xp != null)
                {
                    string[] llz = xnn.ChildNodes[0].Value.Replace("\r", "").Replace("\n", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    string name = "NoName";
                    try { name = xnn.ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value; }
                    catch { };
                    PointF pf = new PointF((float)double.Parse(llz[0], System.Globalization.CultureInfo.InvariantCulture), (float)double.Parse(llz[1], System.Globalization.CultureInfo.InvariantCulture));
                    pbox.Items.Add(new KeyValuePair<string, PointF>(name, pf), true);
                    OnMapPoints.Add(pf);
                };
            };
        }

        public static string RemoveXMLNamespaces(string xml)
        {
            string outerXml = xml;
            { // "
                string xmlnsPattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\"(?<url>[^\\\"]*)\\\"";
                MatchCollection matchCol = Regex.Matches(outerXml, xmlnsPattern);
                foreach (Match match in matchCol)
                    outerXml = outerXml.Replace(match.ToString(), "");
            };
            {// '
                string xmlnsPattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\'(?<url>[^\\\']*)\\\'";
                MatchCollection matchCol = Regex.Matches(outerXml, xmlnsPattern);
                foreach (Match match in matchCol)
                    outerXml = outerXml.Replace(match.ToString(), "");
            };
            {
                string xmlnsPattern = "<kml[^>]*?>";
                MatchCollection matchCol = Regex.Matches(outerXml, xmlnsPattern);
                foreach (Match match in matchCol)
                    outerXml = outerXml.Replace(match.ToString(), "<kml>");
            };
            return outerXml;
        }

    }


    public class MCLB : CheckedListBox
    {
        public object MovingItem = null;
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Size checkSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
            int dx = (e.Bounds.Height - checkSize.Width) / 2;

            e.DrawBackground();

            if (e.Index >= 0)
            {
                try
                {
                    bool isChecked = GetItemChecked(e.Index);
                    bool isSelected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);
                    if (isChecked)
                    {
                        e.Graphics.DrawString("v", new Font(e.Font, FontStyle.Bold), isSelected ? SystemBrushes.HighlightText : SystemBrushes.WindowText, new PointF(dx + 1, e.Bounds.Top));
                    }
                    else
                    {
                        if (isSelected)
                            e.Graphics.FillRectangle(Brushes.DarkSlateBlue, e.Bounds);
                        else
                            e.Graphics.FillRectangle(Brushes.Yellow, e.Bounds);
                    };

                    Pen myp = new Pen(new SolidBrush(Color.FromArgb(230, 230, 250)));
                    myp.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawLine(myp, new Point(e.Bounds.Left, e.Bounds.Bottom - 1), new Point(e.Bounds.Right, e.Bounds.Bottom - 1));
                    int offset = e.Bounds.Left + e.Font.Height + 2;
                    e.Graphics.DrawLine(myp, new Point(offset, e.Bounds.Top), new Point(offset, e.Bounds.Bottom - 1));
                    offset += 5;

                    string name = this.Items[e.Index].ToString();                                        
                    if ((MovingItem != null) && isSelected)
                    {
                        e.Graphics.FillRectangle(Brushes.Fuchsia, e.Bounds);
                        if (isChecked) e.Graphics.DrawString("v", new Font(e.Font, FontStyle.Bold), isSelected ? SystemBrushes.HighlightText : SystemBrushes.WindowText, new PointF(dx + 1, e.Bounds.Top));
                        name = MovingItem.ToString();
                    };
                    e.Graphics.DrawString(name, e.Font, isSelected ? SystemBrushes.HighlightText : Brushes.Black, new Rectangle(offset, e.Bounds.Top, e.Bounds.Width - offset, e.Bounds.Height), StringFormat.GenericDefault);
                }
                catch { };
            };
        }
    }
}