using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace BenzinPriceAnalizer
{
    public partial class BenzinPriceAnalizerForm : Form
    {
        public List<YPlacemark> LoadedMarks = new List<YPlacemark>();        
        public List<PointF> LoadedRoute = new List<PointF>();

        public BenzinPriceAnalizerForm()
        {           
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            marksFilter.SelectedIndex = 0;
            bgFiles.AllowDrop = true;
            bgFiles.DragDrop += bpFiles_DragDrop;
            bgFiles.DragEnter += bpFiles_DragEnter;
        }

        private void bpFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void bpFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] droppedfiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in droppedfiles)
            {
                bool skip = false;
                foreach (object oj in bgFiles.Items)
                {
                    LBFile f = (LBFile)oj;
                    if (f.filename == file) skip = true;
                };

                if (skip) 
                    continue;
                else
                {
                    LBFile f = new LBFile(file);
                    bgFiles.Items.Add(f);
                    LoadMarks(f);
                };
            };
            AddStatus("Found " + LoadedMarks.Count.ToString() + " points in " + bgFiles.Items.Count.ToString() + " file(s)");
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            delone.Enabled = bgFiles.SelectedItems.Count > 0;
            delall.Enabled = bgFiles.Items.Count > 0;
        }

        private void delall_click(object sender, EventArgs e)
        {
            bgFiles.Items.Clear();
            LoadedMarks.Clear();
            AddStatus("Found " + LoadedMarks.Count.ToString() + " points in " + bgFiles.Items.Count.ToString() + " file(s)");
        }

        private void delmnu_click(object sender, EventArgs e)
        {
            if (bgFiles.SelectedItems.Count == 0) return;
            for (int i = bgFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                LBFile f = (LBFile)bgFiles.Items[bgFiles.SelectedIndices[i]];
                if (LoadedMarks.Count > 0)
                    for (int x = LoadedMarks.Count - 1; x >= 0; x--)
                        if (LoadedMarks[x].fromfile == f.filename)
                            LoadedMarks.RemoveAt(x);
                bgFiles.Items.RemoveAt(bgFiles.SelectedIndices[i]);
            };
            AddStatus("Found " + LoadedMarks.Count.ToString() + " points in " + bgFiles.Items.Count.ToString() + " file(s)");
        }

        private void addmnu_click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach(string file in ofd.FileNames)
                {
                    bool skip = false;
                    foreach (object oj in bgFiles.Items)
                    {
                        LBFile f = (LBFile)oj;
                        if (f.filename == file) skip = true;
                    };
                    if (skip) continue;
                    {
                        LBFile f = new LBFile(file);
                        bgFiles.Items.Add(f);
                        LoadMarks(f);
                    };
                };
                AddStatus("Found " + LoadedMarks.Count.ToString() + " points in " + bgFiles.Items.Count.ToString() + " file(s)");
            };
            ofd.Dispose();
        }

        private void LoadMarks(LBFile f)
        {
            AddStatus("Loading points from file: " + f.name);
            YPlacemark[] m = ReadFile(f).ToArray();
            AddStatus("Loaded " + m.Length.ToString() + " points from file: " + f.name);
            LoadedMarks.AddRange(m);                        
        }

        private void button1_Click(object sender, EventArgs e)
        {            
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".kml,.gpx";
            ofd.Filter = "KML, GPX (*.kml;*.gpx)|*.kml;*.gpx";
            if (ofd.ShowDialog() == DialogResult.OK)
                loadroute(ofd.FileName);
            ofd.Dispose();
        }
        

        private void button2_Click(object sender, EventArgs e)
        {            
            dosort();
        }

        private void loadroute(string filename)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            AddStatus("Loading route (path) from file: " + System.IO.Path.GetFileName(filename));
            routeFileBox.Text = filename;
            LoadedRoute.Clear();

            System.IO.FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            System.IO.StreamReader sr = new StreamReader(fs);
            if (System.IO.Path.GetExtension(filename).ToLower() == ".kml")
            {
                string file = sr.ReadToEnd();
                int si = file.IndexOf("<coordinates>");
                int ei = file.IndexOf("</coordinates>");
                string co = file.Substring(si + 13, ei - si - 13).Trim().Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
                string[] arr = co.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if ((arr != null) && (arr.Length > 0))
                    for (int i = 0; i < arr.Length; i++)
                    {
                        string[] xyz = arr[i].Split(new string[] { "," }, StringSplitOptions.None);
                        LoadedRoute.Add(new PointF(float.Parse(xyz[0], ni), float.Parse(xyz[1], ni)));
                    };
            };
            if (System.IO.Path.GetExtension(filename).ToLower() == ".gpx")
            {
                string file = sr.ReadToEnd();
                int si = 0;
                int ei = 0;
                si = file.IndexOf("<rtept", ei);
                ei = file.IndexOf(">", si);
                while (si > 0)
                {
                    string rtept = file.Substring(si + 7, ei - si - 7).Replace("\"", "").Replace("/", "").Trim();
                    int ssi = rtept.IndexOf("lat=");
                    int sse = rtept.IndexOf(" ", ssi);
                    if (sse < 0) sse = rtept.Length;
                    string lat = rtept.Substring(ssi + 4, sse - ssi - 4);
                    ssi = rtept.IndexOf("lon=");
                    sse = rtept.IndexOf(" ", ssi);
                    if (sse < 0) sse = rtept.Length;
                    string lon = rtept.Substring(ssi + 4, sse - ssi - 4);
                    LoadedRoute.Add(new PointF(float.Parse(lon, ni), float.Parse(lat, ni)));

                    si = file.IndexOf("<rtept", ei);
                    if (si > 0)
                        ei = file.IndexOf(">", si);
                };
            };
            sr.Close();
            fs.Close();

            label3.Text = "Route (path) has " + LoadedRoute.Count.ToString() + " points";
            marksFilter.Enabled = LoadedRoute.Count > 1;
            AddStatus("Route (path) with " + LoadedRoute.Count.ToString() + " points loaded");
        }

        private void dosort()
        {
            if (LoadedMarks.Count == 0) return;
            if (marksFilter.SelectedIndex == 0)
            {
                foreach (YPlacemark m in LoadedMarks) m.skip = false;
                AddStatus("Filtered " + LoadedMarks.Count.ToString() + " points of " + LoadedMarks.Count.ToString() + " in " + bgFiles.Items.Count.ToString() + " file(s)");
                return;
            };
            if (LoadedRoute.Count < 2) return;            
            
            double rad = (double)inrad.Value;

            if (marksFilter.SelectedIndex == 1) AddStatus("Select points in " + rad.ToString() + "m at buffer zone of the path");
            if (marksFilter.SelectedIndex == 2) AddStatus("Select points in " + inrad.Value.ToString() + "m at right of the path");
            if (marksFilter.SelectedIndex == 3) AddStatus("Select points in " + inrad.Value.ToString() + "m at left of the path");
            int notskipped = 0;
            foreach (YPlacemark m in LoadedMarks)
            { 
                m.skip = true;

                double length2 = double.MaxValue;
                int side2 = 0;
                for (int i = 1; i < LoadedRoute.Count; i++)
                {
                    PointF op;
                    int side;
                    double d = DistanceFromPointToLine(new PointF((float)m.x, (float)m.y), LoadedRoute[i - 1], LoadedRoute[i], out op, out side);
                    if (d < length2)
                    {
                        length2 = d;
                        side2 = side;
                    };
                };

                if (length2 <= rad)
                {
                    if (marksFilter.SelectedIndex == 1)
                        m.skip = false;
                    else
                        if ((marksFilter.SelectedIndex == 2) && (side2 <= 0))
                            m.skip = false;
                        else
                            if ((marksFilter.SelectedIndex == 3) && (side2 > 0)) m.skip = false;
                };

                if (!m.skip) notskipped++;
            };
            AddStatus("Filtered "+notskipped.ToString()+" points of " + LoadedMarks.Count.ToString() + " in " + bgFiles.Items.Count.ToString() + " file(s)");
        }

        private void doSaveAsIs()
        {
            if (LoadedMarks.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "KMZ (*.kmz)|*.kmz";
            sfd.DefaultExt = ".kmz";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                AddStatus("Saving filtered points: " + sfd.FileName);
                string title = "benzine-price analyzer data";
                if (marksFilter.SelectedIndex > 0) title = "AZS " + marksFilter.Text;
                InputBox("Saving", "Layer name:", ref title);
                File.Delete(sfd.FileName);
                System.IO.Directory.CreateDirectory(sfd.FileName);
                System.IO.Directory.CreateDirectory(sfd.FileName + @"\images\");
                System.IO.FileStream fs = new System.IO.FileStream(sfd.FileName + @"\doc.kml", System.IO.FileMode.Create, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
                sw.WriteLine("<kml xmlns='http://www.opengis.net/kml/2.2'><Document>");
                sw.WriteLine("<name>" + title + "</name><Folder><name>" + title + "</name>");
                int saved = 0;

                List<string> MarksIcons = new List<string>();

                foreach (YPlacemark m in LoadedMarks)
                {
                    if (m.skip) continue;
                    
                    sw.WriteLine("<Placemark><name>" + m.hintContent.Replace("<p class=price>", "").Replace("<br />", "").Replace("&","&amp;") + "</name>");
                    sw.WriteLine("<description><![CDATA[" + m.balloonContent.Replace("<p class=price>", "") + "]]></description>");
                    if (m.iconImageHref != String.Empty)
                    {
                        string ic = m.iconImageHref.Replace("/", "-").Replace(".", "-");
                        if (MarksIcons.IndexOf(ic) < 0)
                        {
                            MarksIcons.Add(ic);
                            GrabImage(m.iconImageHref, sfd.FileName + @"\images\" + ic + ".png");
                        };
                        sw.WriteLine("<styleUrl>#" + ic + "</styleUrl>");
                    };
                    sw.WriteLine("<Point><coordinates>" + m.x.ToString().Replace(",", ".") + "," + m.y.ToString().Replace(",", ".") + ",0.0</coordinates></Point></Placemark>");
                    
                    saved++;
                };
                sw.WriteLine("</Folder>");
                if (MarksIcons.Count > 0)
                    foreach (string icon in MarksIcons)
                    {
                        sw.WriteLine("<Style id='" + icon + "'><IconStyle><scale>0.6</scale><Icon>");
                        sw.WriteLine("<href>images/" + icon + ".png</href></Icon></IconStyle></Style>");
                    };
                sw.WriteLine("</Document></kml>");
                sw.Close();
                fs.Close();
                CreateZip(sfd.FileName);
                AddStatus("Saved " + saved.ToString() + " points of " + LoadedMarks.Count.ToString() + " and " + MarksIcons.Count.ToString() + " icons ");
            };
            sfd.Dispose();
        }

        private void doSaveGroupped()
        {
            if (LoadedMarks.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "KMZ (*.kmz)|*.kmz";
            sfd.DefaultExt = ".kmz";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                AddStatus("Saving points to file: " + sfd.FileName);
                string title = "benzine-price analyzer data";
                if (marksFilter.SelectedIndex > 0) title = marksFilter.Text;
                File.Delete(sfd.FileName);
                System.IO.Directory.CreateDirectory(sfd.FileName);
                System.IO.Directory.CreateDirectory(sfd.FileName + @"\images\");
                System.IO.FileStream fs = new System.IO.FileStream(sfd.FileName + @"\doc.kml", System.IO.FileMode.Create, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
                sw.WriteLine("<kml xmlns='http://www.opengis.net/kml/2.2'>");
                sw.WriteLine("\t<Document>");
                sw.WriteLine("\t\t<name>" + title + "</name>");
                int saved = 0;
                //////////
                List<Renm> pictures = new List<Renm>();
                foreach (YPlacemark m in LoadedMarks)
                {
                    if (m.skip) continue;
                    string href = m.iconImageHref;
                    string ic = href.Replace("/", "-").Replace(".", "-");
                    string hint = m.hintContent.Replace("<p class=price>", "").Replace("<br />", "");
                    int ind = -1;
                    if (pictures.Count > 0) for (int i = 0; i < pictures.Count; i++) if (pictures[i].ic == ic) ind = i;
                    if (ind < 0)
                    {
                        pictures.Add(new Renm(ic, "Noname " + pictures.Count.ToString(), hint));
                        if (ic != "")
                        {                            
                            GrabImage(m.iconImageHref, sfd.FileName + @"\images\" + ic + ".png");
                        };
                    }
                    else
                        pictures[ind].names.Add(hint);
                };
                //////////                
                if (pictures.Count > 0)
                {
                    KMZRebuilder.LayersRenamerForm sl = new KMZRebuilder.LayersRenamerForm();
                    for (int i = 0; i < pictures.Count; i++)
                    {
                        string od = pictures[i].names[0];
                        if (pictures[i].names.Count > 1)
                            for (int x = 1; x < pictures[i].names.Count; x++)
                                od = LCS(od, pictures[i].names[x]);
                        od = od.Trim();
                        if (od != String.Empty) pictures[i].title = od;
                        if (pictures[i].title.ToUpper().IndexOf("АЗС") < 0) pictures[i].title = "АЗС " + pictures[i].title;
                        pictures[i].title += " [" + pictures[i].names.Count.ToString()+"]";
                        if (marksFilter.SelectedIndex == 1) pictures[i].title += " at route";
                        if (marksFilter.SelectedIndex == 2) pictures[i].title += " by right";
                        if (marksFilter.SelectedIndex == 3) pictures[i].title += " by left";
                        Image im = Image.FromFile(sfd.FileName + @"\images\" + pictures[i].ic + ".png");
                        sl.images.Images.Add((Image)new Bitmap(im));
                        im.Dispose();
                        sl.layers.Items.Add(pictures[i].title, i);
                    };
                    if (sl.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < pictures.Count; i++)
                            pictures[i].title = sl.layers.Items[i].Text;
                    };
                    sl.Dispose();
                };
                //////////
                if (pictures.Count > 0)
                    for (int i = 0; i < pictures.Count; i++)
                    {
                        sw.WriteLine("\t\t<Folder>");
                        sw.WriteLine("\t\t\t<name>" + pictures[i].title + "</name>");
                        foreach (YPlacemark m in LoadedMarks)
                        {
                            if (m.skip) continue;

                            string href = m.iconImageHref;
                            string ic = m.iconImageHref.Replace("/", "-").Replace(".", "-");
                            if (ic != pictures[i].ic) continue;

                            string hint = m.hintContent.Replace("<p class=price>", "").Replace("<br />", "");
                            string desc = m.balloonContent.Replace("<p class=price>", "");

                            sw.WriteLine("<Placemark><name>" + hint.Replace("&", "&amp;") + "</name>");
                            sw.WriteLine("<description><![CDATA[" + desc + "]]></description>");
                            if (hint != String.Empty)
                                sw.WriteLine("<styleUrl>#" + ic + "</styleUrl>");
                            sw.WriteLine("<Point><coordinates>" + m.x.ToString().Replace(",", ".") + "," + m.y.ToString().Replace(",", ".") + ",0.0</coordinates></Point></Placemark>");

                            saved++;
                        };
                        sw.WriteLine("\t\t</Folder>");
                    };
                if (pictures.Count > 0)
                    for (int i = 0; i < pictures.Count; i++)
                    {
                        sw.WriteLine("<Style id='" + pictures[i].ic + "'><IconStyle><scale>0.6</scale><Icon>");
                        sw.WriteLine("<href>images/" + pictures[i].ic + ".png</href></Icon></IconStyle></Style>");
                    };
                sw.WriteLine("</Document></kml>");
                sw.Close();
                fs.Close();
                CreateZip(sfd.FileName);
                AddStatus("Saved " + saved.ToString() + " points of " + LoadedMarks.Count.ToString() + " and " + pictures.Count.ToString() + " icons ");
            };
            sfd.Dispose();
        }

        public static string LCS(string s1, string s2)
        {
            int[,] a = new int[s1.Length + 1, s2.Length + 1];
            int u = 0, v = 0;

            for (int i = 0; i < s1.Length; i++)
                for (int j = 0; j < s2.Length; j++)
                    if (s1[i] == s2[j])
                    {
                        a[i + 1, j + 1] = a[i, j] + 1;
                        if (a[i + 1, j + 1] > a[u, v])
                        {
                            u = i + 1;
                            v = j + 1;
                        }
                    }

            return s1.Substring(u - a[u, v], a[u, v]);
        }

        /// <summary>
        ///     Расчет расстояния от точки до линии
        /// </summary>
        /// <param name="pt">Искомая точка</param>
        /// <param name="lineStart">Нач точка линии</param>
        /// <param name="lineEnd">Кон точка линии</param>
        /// <param name="pointOnLine">точка на линии ближайшая к искомой</param>
        /// <param name="side">С какой стороны линии находится искомая точка (+ слева, - справа)</param>
        /// <returns>метры</returns>
        private static float DistanceFromPointToLine(PointF pt, PointF lineStart, PointF lineEnd, out PointF pointOnLine, out int side)
        {
            float dx = lineEnd.X - lineStart.X;
            float dy = lineEnd.Y - lineStart.Y;

            if ((dx == 0) && (dy == 0))
            {
                // line is a point
                // линия может быть с нулевой длиной после анализа TRA
                pointOnLine = lineStart;
                side = 0;
                //dx = pt.X - lineStart.X;
                //dy = pt.Y - lineStart.Y;                
                //return Math.Sqrt(dx * dx + dy * dy);
                return Utils.GetLengthMeters(pt.Y, pt.X, pointOnLine.Y, pointOnLine.X, false);
            };

            side = Math.Sign((lineEnd.X - lineStart.X) * (pt.Y - lineStart.Y) - (lineEnd.Y - lineStart.Y) * (pt.X - lineStart.X));

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - lineStart.X) * dx + (pt.Y - lineStart.Y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                pointOnLine = new PointF(lineStart.X, lineStart.Y);
                dx = pt.X - lineStart.X;
                dy = pt.Y - lineStart.Y;
            }
            else if (t > 1)
            {
                pointOnLine = new PointF(lineEnd.X, lineEnd.Y);
                dx = pt.X - lineEnd.X;
                dy = pt.Y - lineEnd.Y;
            }
            else
            {
                pointOnLine = new PointF(lineStart.X + t * dx, lineStart.Y + t * dy);
                dx = pt.X - pointOnLine.X;
                dy = pt.Y - pointOnLine.Y;
            };

            //return Math.Sqrt(dx * dx + dy * dy);
            return Utils.GetLengthMeters(pt.Y, pt.X, pointOnLine.Y, pointOnLine.X, false);
        }  

        private void GrabImage(string url, string file)
        {
            try
            {
                System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://benzin-price.ru/"+url);
                System.Net.WebResponse wres = wr.GetResponse();
                System.IO.FileStream fs = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                System.IO.Stream rs = wres.GetResponseStream();
                byte[] ba = new byte[4096];
                int read = -1;
                while ((read = rs.Read(ba, 0, ba.Length)) > 0)
                    fs.Write(ba, 0, read);
                wres.Close();
                fs.Close();
            }
            catch {};
        }

        private List<YPlacemark> ReadFile(LBFile f)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            Regex rvar = new Regex(@"var\s([0-9a-zA-Z]{33})\s=\s([.0-9]+);");
            Regex rtwo = new Regex(@"var\s([0-9a-zA-Z]{33})\s=\s([0-9a-zA-Z]{33})(.)([0-9a-zA-Z]{33});");
            Regex rxy = new Regex(@"([0-9a-zA-Z]{33})(.)([0-9a-zA-Z]{33}),([0-9a-zA-Z]{33})(.)([0-9a-zA-Z]{33})");
            Hashtable ht = new Hashtable();
            string placemark = "";
            List<YPlacemark> marks = new List<YPlacemark>();

            System.IO.FileStream fs = new System.IO.FileStream(f.filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.StreamReader sr = new System.IO.StreamReader(fs);
            byte fType = 0;
            while (!sr.EndOfStream)
            {
                string ln = sr.ReadLine();
                if (rvar.IsMatch(ln))
                {
                    MatchCollection mc = rvar.Matches(ln);
                    string var = mc[0].Groups[1].Value;
                    double val = double.Parse(mc[0].Groups[2].Value,ni);
                    ht.Add(var, val);
                };
                if(rtwo.IsMatch(ln))
                {
                    MatchCollection mc = rtwo.Matches(ln);
                    string var = mc[0].Groups[1].Value;
                    string varA = mc[0].Groups[2].Value;
                    string todo = mc[0].Groups[3].Value;
                    string varB = mc[0].Groups[4].Value;
                    if (todo == "*")
                        ht.Add(var, (double)ht[varA] * (double)ht[varB]);
                    if (todo == "/")
                        ht.Add(var, (double)ht[varA] / (double)ht[varB]);
                };

                // PLACEMARK
                if (ln.IndexOf("ymaps.Placemark") > 0)
                {
                    placemark += " ";
                };
                if ((ln.IndexOf("myCollection.add") > 0) && ((fType == 0) || (fType == 2)))
                {
                    fType = 2;
                    YPlacemark jp = new YPlacemark(f.filename);
                    int si = placemark.IndexOf("new ymaps.Placemark([");
                    int ei = placemark.IndexOf("]", si);
                    {
                        string xy = placemark.Substring(si + 21, ei - si - 21);
                        string[] xys = xy.Split(new char[] { ',' }, 2);
                        double X = 0; double.TryParse(xys[1],System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out X);
                        double Y = 0; double.TryParse(xys[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out Y);
                        jp.x = X;
                        jp.y = Y;
                    };
                    {
                        si = placemark.IndexOf("hintContent: \"<p class=price>");
                        ei = placemark.IndexOf("<br />", si);
                        jp.hintContent = placemark.Substring(si + 29, ei - si - 29);
                        si = placemark.IndexOf("balloonContentBody: \"<p class=price>");
                        ei = placemark.IndexOf("\",\t\t\t\t\ticonContent:", si);
                        jp.balloonContent = placemark.Substring(si + 36, ei - si - 36);
                    };
                    {
                        si = placemark.IndexOf("iconImageHref: '");
                        ei = placemark.IndexOf("',", si);
                        jp.iconImageHref = placemark.Substring(si + 16, ei - si - 16);
                    };
                    placemark = "";
                    marks.Add(jp);
                };
                if ((ln.IndexOf("geoObjects.add") > 0) && ((fType == 0) || (fType == 1)))
                {
                    fType = 1;
                    YPlacemark jp = new YPlacemark(f.filename);
                    int si = placemark.IndexOf("new ymaps.Placemark([");
                    int ei = placemark.IndexOf("]", si);
                    {
                        string xy = placemark.Substring(si + 21, ei - si - 21);
                        MatchCollection mc = rxy.Matches(xy);
                        string varYA = mc[0].Groups[1].Value;
                        string todoY = mc[0].Groups[2].Value;
                        string varYB = mc[0].Groups[3].Value;
                        string varXA = mc[0].Groups[4].Value;
                        string todoX = mc[0].Groups[5].Value;
                        string varXB = mc[0].Groups[6].Value;
                        double X = (double)ht[varXA] + (double)ht[varXB];
                        double Y = (double)ht[varYA] + (double)ht[varYB];
                        jp.x = X;
                        jp.y = Y;
                    };
                    {
                        si = placemark.IndexOf("hintContent: \"<p class=price>");
                        ei = placemark.IndexOf("<br />", si);
                        jp.hintContent = placemark.Substring(si + 29, ei - si - 29);
                        si = placemark.IndexOf("balloonContent: \"<p class=price>");
                        ei = placemark.IndexOf(",\t\t\t\ticonContent:", si);
                        jp.balloonContent = placemark.Substring(si + 32, ei - si - 32);
                    };
                    {
                        si = placemark.IndexOf("iconImageHref: '");
                        ei = placemark.IndexOf("',", si);
                        jp.iconImageHref = placemark.Substring(si + 16, ei - si - 16);
                    };
                    placemark = "";
                    marks.Add(jp);
                    //AddStatus(String.Format("Adding mark {3}: {0} {1} {2}", jp.Y, jp.X, jp.hintContent, marks.Count));
                };
                if (placemark.Length > 0)
                    placemark += ln;                

            };
            sr.Close();
            fs.Close();

            return marks;
        }

        // https://github.com/icsharpcode/SharpZipLib/wiki/Zip-Samples#Create_a_Zip_fromto_a_memory_stream_or_byte_array_1
        public void CreateZip(string filename)
        {
            FileStream fsOut = File.Create(filename+".tmp");
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);
            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
            // zipStream.Password = password;  // optional. Null is the same as not setting. Required if using AES.
            CompressFolder(filename + @"\", zipStream, filename.Length);
            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
            System.IO.DirectoryInfo di = new DirectoryInfo(filename);
            di.Delete(true);
            File.Move(filename + ".tmp", filename);
        }


        // Recurses down the folder structure
        //
        private void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }

        private void AddStatus(string txt)
        {
            status.Text += txt + "\r\n";
            status.Refresh();
            status.SelectionStart = status.Text.Length;
            status.ScrollToCaret();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            inrad.Enabled = (marksFilter.SelectedIndex > 0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!byImgs.Checked)
                doSaveAsIs();
            else
                doSaveGroupped();
        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        public static DialogResult InputBox(string title, string promptText, ref string value, Bitmap icon)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;
            if (icon != null) picture.Image = icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);
            picture.SetBounds(12, 72, 22, 22);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            value = textBox.Text;
            return dialogResult;
        }

    }

    public class YPlacemark
    {        
        public double x = 0;
        public double y = 0;

        public string hintContent = "";
        public string balloonContent = "";
        public string iconImageHref = "";

        public bool skip = false;
        public string fromfile = "";

        public YPlacemark(string filename)
        {
            this.fromfile = filename;
        }
    }

    public class Renm
    {
        public string ic;
        public string title;
        public List<string> names = new List<string>();

        public Renm(string image, string title, string name)
        {
            this.ic = image;
            this.title = title;
            this.names.Add(name);
        }
    }

    public class LBFile
    {
        public string filename = "";
        public string name {
            get
            {
                return System.IO.Path.GetFileName(filename);
            }
        }

        public LBFile(string filename)
        {
            this.filename = filename;
        }

        public override string ToString()
        {
            return name;
        }
    }

    public class Utils
    {
        // Рассчет расстояния       
        #region LENGTH
        public static float GetLengthMeters(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            // use fastest
            float result = (float)GetLengthMetersD(StartLat, StartLong, EndLat, EndLong, radians);

            if (float.IsNaN(result))
            {
                result = (float)GetLengthMetersC(StartLat, StartLong, EndLat, EndLong, radians);
                if (float.IsNaN(result))
                {
                    result = (float)GetLengthMetersE(StartLat, StartLong, EndLat, EndLong, radians);
                    if (float.IsNaN(result))
                        result = 0;
                };
            };

            return result;
        }

        // Slower
        public static uint GetLengthMetersA(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double D2R = Math.PI / 180;     // Преобразование градусов в радианы

            double a = 6378137.0000;     // WGS-84 Equatorial Radius (a)
            double f = 1 / 298.257223563;  // WGS-84 Flattening (f)
            double b = (1 - f) * a;      // WGS-84 Polar Radius
            double e2 = (2 - f) * f;      // WGS-84 Квадрат эксцентричности эллипсоида  // 1-(b/a)^2

            // Переменные, используемые для вычисления смещения и расстояния
            double fPhimean;                           // Средняя широта
            double fdLambda;                           // Разница между двумя значениями долготы
            double fdPhi;                           // Разница между двумя значениями широты
            double fAlpha;                           // Смещение
            double fRho;                           // Меридианский радиус кривизны
            double fNu;                           // Поперечный радиус кривизны
            double fR;                           // Радиус сферы Земли
            double fz;                           // Угловое расстояние от центра сфероида
            double fTemp;                           // Временная переменная, использующаяся в вычислениях

            // Вычисляем разницу между двумя долготами и широтами и получаем среднюю широту
            // предположительно что расстояние между точками << радиуса земли
            if (!radians)
            {
                fdLambda = (StartLong - EndLong) * D2R;
                fdPhi = (StartLat - EndLat) * D2R;
                fPhimean = ((StartLat + EndLat) / 2) * D2R;
            }
            else
            {
                fdLambda = StartLong - EndLong;
                fdPhi = StartLat - EndLat;
                fPhimean = (StartLat + EndLat) / 2;
            };

            // Вычисляем меридианные и поперечные радиусы кривизны средней широты
            fTemp = 1 - e2 * (sqr(Math.Sin(fPhimean)));
            fRho = (a * (1 - e2)) / Math.Pow(fTemp, 1.5);
            fNu = a / (Math.Sqrt(1 - e2 * (Math.Sin(fPhimean) * Math.Sin(fPhimean))));

            // Вычисляем угловое расстояние
            if (!radians)
            {
                fz = Math.Sqrt(sqr(Math.Sin(fdPhi / 2.0)) + Math.Cos(EndLat * D2R) * Math.Cos(StartLat * D2R) * sqr(Math.Sin(fdLambda / 2.0)));
            }
            else
            {
                fz = Math.Sqrt(sqr(Math.Sin(fdPhi / 2.0)) + Math.Cos(EndLat) * Math.Cos(StartLat) * sqr(Math.Sin(fdLambda / 2.0)));
            };
            fz = 2 * Math.Asin(fz);

            // Вычисляем смещение
            if (!radians)
            {
                fAlpha = Math.Cos(EndLat * D2R) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            }
            else
            {
                fAlpha = Math.Cos(EndLat) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            };
            fAlpha = Math.Asin(fAlpha);

            // Вычисляем радиус Земли
            fR = (fRho * fNu) / (fRho * sqr(Math.Sin(fAlpha)) + fNu * sqr(Math.Cos(fAlpha)));
            // Получаем расстояние
            return (uint)Math.Round(Math.Abs(fz * fR));
        }
        // Slowest
        public static uint GetLengthMetersB(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double fPhimean, fdLambda, fdPhi, fAlpha, fRho, fNu, fR, fz, fTemp, Distance,
                D2R = Math.PI / 180,
                a = 6378137.0,
                e2 = 0.006739496742337;
            if (radians) D2R = 1;

            fdLambda = (StartLong - EndLong) * D2R;
            fdPhi = (StartLat - EndLat) * D2R;
            fPhimean = (StartLat + EndLat) / 2.0 * D2R;

            fTemp = 1 - e2 * Math.Pow(Math.Sin(fPhimean), 2);
            fRho = a * (1 - e2) / Math.Pow(fTemp, 1.5);
            fNu = a / Math.Sqrt(1 - e2 * Math.Sin(fPhimean) * Math.Sin(fPhimean));

            fz = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(fdPhi / 2.0), 2) +
              Math.Cos(EndLat * D2R) * Math.Cos(StartLat * D2R) * Math.Pow(Math.Sin(fdLambda / 2.0), 2)));
            fAlpha = Math.Asin(Math.Cos(EndLat * D2R) * Math.Sin(fdLambda) / Math.Sin(fz));
            fR = fRho * fNu / (fRho * Math.Pow(Math.Sin(fAlpha), 2) + fNu * Math.Pow(Math.Cos(fAlpha), 2));
            Distance = fz * fR;

            return (uint)Math.Round(Distance);
        }
        // Average
        public static uint GetLengthMetersC(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double D2R = Math.PI / 180;
            if (radians) D2R = 1;
            double dDistance = Double.MinValue;
            double dLat1InRad = StartLat * D2R;
            double dLong1InRad = StartLong * D2R;
            double dLat2InRad = EndLat * D2R;
            double dLong2InRad = EndLong * D2R;

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            const double kEarthRadiusKms = 6378137.0000;
            dDistance = kEarthRadiusKms * c;

            return (uint)Math.Round(dDistance);
        }
        // Fastest
        public static double GetLengthMetersD(double sLat, double sLon, double eLat, double eLon, bool radians)
        {
            double EarthRadius = 6378137.0;

            double lon1 = radians ? sLon : DegToRad(sLon);
            double lon2 = radians ? eLon : DegToRad(eLon);
            double lat1 = radians ? sLat : DegToRad(sLat);
            double lat2 = radians ? eLat : DegToRad(eLat);

            return EarthRadius * (Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2)));
        }
        // Fastest
        public static double GetLengthMetersE(double sLat, double sLon, double eLat, double eLon, bool radians)
        {
            double EarthRadius = 6378137.0;

            double lon1 = radians ? sLon : DegToRad(sLon);
            double lon2 = radians ? eLon : DegToRad(eLon);
            double lat1 = radians ? sLat : DegToRad(sLat);
            double lat2 = radians ? eLat : DegToRad(eLat);

            /* This algorithm is called Sinnott's Formula */
            double dlon = (lon2) - (lon1);
            double dlat = (lat2) - (lat1);
            double a = Math.Pow(Math.Sin(dlat / 2), 2.0) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2.0);
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return EarthRadius * c;
        }
        private static double sqr(double val)
        {
            return val * val;
        }
        public static double DegToRad(double deg)
        {
            return (deg / 180.0 * Math.PI);
        }
        #endregion LENGTH
    }    


}