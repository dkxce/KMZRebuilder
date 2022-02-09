using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace KMZRebuilder
{
    public partial class Selection_Filter : Form
    {
        KMZRebuilederForm parent = null;
        ContentViewer cparent = null;
        KMFile kfile;
        KMLayer klayer;
        XmlNodeList placemarks;

        int total;
        int filtered;
        List<int> ids = new List<int>();
        public List<PointF> LoadedRoute = new List<PointF>();
        public List<PointF> LoadedPoly = new List<PointF>();

        public Selection_Filter(Form parent, KMFile km, KMLayer kl)
        {
            if(parent is KMZRebuilederForm)
                this.parent = (KMZRebuilederForm)parent;
            if (parent is ContentViewer)
                this.cparent = (ContentViewer)parent;
            
            InitializeComponent();
            marksFilter.SelectedIndex = 0;

            kfile = km;
            if (kfile != null) this.Text += " to `" + kfile.kmldocName + "`";
            klayer = kl;
            if (klayer != null) this.Text += " to `" + klayer.name + "`";

            List();

            Reset();
        }

        private void List()
        {
            placemarks = kfile.kmlDoc.SelectNodes("kml/Document/Folder/Placemark");
            if (klayer != null) placemarks = kfile.kmlDoc.SelectNodes("kml/Document/Folder")[klayer.id].SelectNodes("Placemark");
        }

        public void Up()
        {
            label1.Text = String.Format("Total placemarks: {0}", total);
            filtered = ids.Count;
            int todel = total - filtered;
            string del_text = "delete";
            if (APPD.Visible) del_text = "mark/check";
            label3.Text = todel > 0 ? String.Format("Placemarks with filter: {0} to keep, {1} to " + del_text, filtered, todel) : "---";
            button9.Enabled = button11.Enabled = todel > 0;
            label3.ForeColor = total == filtered ? Color.Black : Color.Maroon;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Regex reg = new Regex(textBox1.Text.Trim(),checkBox1.Checked ? RegexOptions.IgnoreCase : RegexOptions.None);
            
            ids.Clear();
            for (int i = 0; i < placemarks.Count; i++)
                if (placemarks[i].HasChildNodes)
                {
                    bool reg_name = false;
                    bool reg_desc = false;

                    XmlNode nn = placemarks[i].SelectSingleNode("name");
                    if (nn != null)
                    {
                        if (nn.HasChildNodes)
                        {
                            string nam = nn.ChildNodes[0].Value;
                            if (reg.IsMatch(nam)) reg_name = true;
                        };
                    };

                    string description = "";
                    try { description = placemarks[i].SelectSingleNode("description").ChildNodes[0].Value; }
                    catch { };
                    if ((!String.IsNullOrEmpty(description)) && (reg.IsMatch(description)))
                        reg_desc = true;

                    bool add = false;

                    if ((ApplyTo.SelectedIndex == 0) && reg_name) add = true;               // name only
                    if ((ApplyTo.SelectedIndex == 1) && reg_desc) add = true;               // desc only
                    if ((ApplyTo.SelectedIndex == 2) && (reg_name || reg_desc)) add = true; // name OR desc
                    if ((ApplyTo.SelectedIndex == 3) && (reg_name && reg_desc)) add = true; // name AND desc
                    if ((ApplyTo.SelectedIndex == 4) && (reg_name != reg_desc)) add = true; // name AND desc

                    if (add) ids.Add(i);               
                };            
            Up();
            button6.Enabled = true;
        }

        private void Reset()
        {
            ids.Clear();
            for (int i = 0; i < placemarks.Count; i++) ids.Add(i);
            total = placemarks.Count;
            filtered = ids.Count;
            Up();
            button6.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Regex reg = new Regex(textBox1.Text.Trim(), checkBox1.Checked ? RegexOptions.IgnoreCase : RegexOptions.None);

            ids.Clear();
            for (int i = 0; i < placemarks.Count; i++)
            {
                if (placemarks[i].HasChildNodes)
                {
                    bool reg_name = false;
                    bool reg_desc = false;

                    XmlNode nn = placemarks[i].SelectSingleNode("name");
                    if (nn != null)
                    {
                        if (nn.HasChildNodes)
                        {
                            string nam = nn.ChildNodes[0].Value;
                            if (reg.IsMatch(nam)) reg_name = true;
                        };
                    };

                    string description = "";
                    try { description = placemarks[i].SelectSingleNode("description").ChildNodes[0].Value; }
                    catch { };
                    if ((!String.IsNullOrEmpty(description)) && (reg.IsMatch(description)))
                        reg_desc = true;

                    bool add = false;

                    if ((ApplyTo.SelectedIndex == 0) && reg_name) add = true;               // name only
                    if ((ApplyTo.SelectedIndex == 1) && reg_desc) add = true;               // desc only
                    if ((ApplyTo.SelectedIndex == 2) && (reg_name || reg_desc)) add = true; // name OR desc
                    if ((ApplyTo.SelectedIndex == 3) && (reg_name && reg_desc)) add = true; // name AND desc
                    if ((ApplyTo.SelectedIndex == 4) && (reg_name != reg_desc)) add = true; // name AND desc

                    if (!add) ids.Add(i);
                }
                else ids.Add(i);
            };            
            Up();
            button6.Enabled = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {            
            Reset();            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".kml,.gpx";
            ofd.Filter = "KML, GPX & SHP files (*.kml;*.gpx;*.shp)|*.kml;*.gpx;*.shp";
            if (ofd.ShowDialog() == DialogResult.OK)
                loadroute(ofd.FileName);
            ofd.Dispose();
        }

        private void loadroute(string filename)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

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
            if (Path.GetExtension(filename).ToLower() == ".shp")
            {
                fs.Position = 32;
                int tof = fs.ReadByte();
                if ((tof == 3))
                {
                    fs.Position = 104;
                    byte[] ba = new byte[4];
                    fs.Read(ba, 0, ba.Length);
                    if (BitConverter.IsLittleEndian) Array.Reverse(ba);
                    int len = BitConverter.ToInt32(ba, 0) * 2;
                    fs.Read(ba, 0, ba.Length);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                    tof = BitConverter.ToInt32(ba, 0);
                    if ((tof == 3))
                    {
                        fs.Position += 32;
                        fs.Read(ba, 0, ba.Length);
                        if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                        if (BitConverter.ToInt32(ba, 0) == 1)
                        {
                            fs.Read(ba, 0, ba.Length);
                            if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                            int pCo = BitConverter.ToInt32(ba, 0);
                            fs.Read(ba, 0, ba.Length);
                            if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                            if (BitConverter.ToInt32(ba, 0) == 0)
                            {
                                ba = new byte[8];
                                for (int i = 0; i < pCo; i++)
                                {
                                    PointF ap = new PointF();
                                    fs.Read(ba, 0, ba.Length);
                                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                                    ap.X = (float)BitConverter.ToDouble(ba, 0);
                                    fs.Read(ba, 0, ba.Length);
                                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                                    ap.Y = (float)BitConverter.ToDouble(ba, 0);
                                    LoadedRoute.Add(ap);
                                };
                            };
                        };
                    };
                };
            };
            sr.Close();
            fs.Close();

            label7.Text = "Route (path) has " + LoadedRoute.Count.ToString() + " points";
            marksFilter.Enabled = LoadedRoute.Count > 1;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            dosort(true);
            button6.Enabled = true;
        }

        private void dosort(bool inside)
        {
            if (marksFilter.SelectedIndex == -1) return;
            if (LoadedRoute.Count < 2) return;

            double rad = (double)inrad.Value;

            ids.Clear();
            for (int itm = 0; itm < placemarks.Count; itm++)
            {
                bool skip = true;

                XmlNode cn = placemarks[itm].SelectSingleNode("Point/coordinates");
                if (cn != null)
                {
                    string[] llz = cn.ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (llz.Length > 2)
                    {
                        double x = double.Parse(llz[0], System.Globalization.CultureInfo.InvariantCulture);
                        double y = double.Parse(llz[1], System.Globalization.CultureInfo.InvariantCulture);

                        double length2 = double.MaxValue;
                        int side2 = 0;
                        for (int i = 1; i < LoadedRoute.Count; i++)
                        {
                            PointF op;
                            int side;
                            double d = DistanceFromPointToLine(new PointF((float)x, (float)y), LoadedRoute[i - 1], LoadedRoute[i], out op, out side);
                            if (d < length2)
                            {
                                length2 = d;
                                side2 = side;
                            };
                        };

                        if (length2 <= rad)
                        {
                            if (marksFilter.SelectedIndex < 1)
                                skip = false;
                            else
                                if ((marksFilter.SelectedIndex == 1) && (side2 <= 0))
                                    skip = false;
                                else
                                    if ((marksFilter.SelectedIndex == 2) && (side2 > 0)) skip = false;
                        };
                    };
                };

                if ((inside) && (!skip)) ids.Add(itm);
                if ((!inside) && (skip)) ids.Add(itm);
            };
            Up();
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

        private void button7_Click(object sender, EventArgs e)
        {
            dosort(false);
            button6.Enabled = true;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (this.parent != null)
            {
                for (int i = placemarks.Count - 1; i >= 0; i--)
                    if (!ids.Contains(i))
                        placemarks[i].ParentNode.RemoveChild(placemarks[i]);

                List();
                parent.ReloadAfterFilter(kfile, klayer);
                this.Focus();
                Reset();
            }
            else
            {
                
                List<int> ids2del = new List<int>();
                for (int i = placemarks.Count - 1; i >= 0; i--)
                    if (!ids.Contains(i))
                        ids2del.Add(i);

                cparent.ApplyFilter(ids.ToArray(), ids2del.ToArray(), APPS.Checked);
                this.Focus();
                Reset();
            };
        }

        private void button10_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".kml";
            ofd.Filter = "KML & ESRI Shape files (*.kml;*.shp)|*.kml;*.shp|BBBike Extract Text with Url (*.txt)|*.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
                loadpoly(ofd.FileName);
            ofd.Dispose();
        }

        private void loadpoly(string filename)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            textBox2.Text = filename;
            LoadedPoly.Clear();

            System.IO.FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            if (System.IO.Path.GetExtension(filename).ToLower() == ".txt")
            {
                try
                {
                    StreamReader sr = new StreamReader(fs, Encoding.ASCII);
                    string line = sr.ReadLine();
                    if (line.StartsWith("[BBIKE_EXTRACT_LINK]")) line = sr.ReadLine();
                    Regex rx = new Regex(@"coords=(?<coords>[^=\r\n\&\#]*)", RegexOptions.IgnoreCase);
                    Match mx = rx.Match(line);
                    if (!mx.Success) return;
                    string coords = mx.Groups["coords"].Value.ToUpper().Replace("%2C", ",").Replace("%7C", "|");
                    if (coords.Length == 0) return;
                    string[] coord = coords.Split(new char[] { '|' });
                    foreach (string c in coord)
                    {
                        string[] xy = c.Split(new char[] { ',' });
                        PointF pf = new PointF((float)double.Parse(xy[0], System.Globalization.CultureInfo.InvariantCulture), (float)double.Parse(xy[1], System.Globalization.CultureInfo.InvariantCulture));
                        LoadedPoly.Add(pf);
                    };
                    sr.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadedPoly.Clear();
                    return;
                };
            };
            if (Path.GetExtension(filename).ToLower() == ".kml")
            {
                System.IO.StreamReader sr = new StreamReader(fs);
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
                            LoadedPoly.Add(new PointF(float.Parse(xyz[0], ni), float.Parse(xyz[1], ni)));
                        };
                };
                sr.Close();
            };
            if (Path.GetExtension(filename).ToLower() == ".shp")
            {
                fs.Position = 32;
                int tof = fs.ReadByte();
                if ((tof == 5))
                {
                    fs.Position = 104;
                    byte[] ba = new byte[4];
                    fs.Read(ba, 0, ba.Length);
                    if (BitConverter.IsLittleEndian) Array.Reverse(ba);
                    int len = BitConverter.ToInt32(ba, 0) * 2;
                    fs.Read(ba, 0, ba.Length);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                    tof = BitConverter.ToInt32(ba, 0);
                    if ((tof == 5))
                    {
                        fs.Position += 32;
                        fs.Read(ba, 0, ba.Length);
                        if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                        if (BitConverter.ToInt32(ba, 0) == 1)
                        {
                            fs.Read(ba, 0, ba.Length);
                            if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                            int pCo = BitConverter.ToInt32(ba, 0);
                            fs.Read(ba, 0, ba.Length);
                            if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                            if (BitConverter.ToInt32(ba, 0) == 0)
                            {
                                ba = new byte[8];
                                for (int i = 0; i < pCo; i++)
                                {
                                    PointF ap = new PointF();
                                    fs.Read(ba, 0, ba.Length);
                                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                                    ap.X = (float)BitConverter.ToDouble(ba, 0);
                                    fs.Read(ba, 0, ba.Length);
                                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                                    ap.Y = (float)BitConverter.ToDouble(ba, 0);
                                    LoadedPoly.Add(ap);
                                };
                            };
                        };
                    };
                };
            };
            fs.Close();

            label8.Text = "Polygon has " + LoadedPoly.Count.ToString() + " points";
            marksFilter.Enabled = LoadedPoly.Count > 1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            dopoly(true);
            button6.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dopoly(false);
            button6.Enabled = true;
        }

        private void dopoly(bool inside)
        {
            if (LoadedPoly.Count < 2) return;
            PointF[] poly = LoadedPoly.ToArray();

            double rad = (double)inrad.Value;

            ids.Clear();
            for (int itm = 0; itm < placemarks.Count; itm++)
            {
                bool skip = true;

                XmlNode cn = placemarks[itm].SelectSingleNode("Point/coordinates");
                if (cn != null)
                {
                    string[] llz = cn.ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (llz.Length > 2)
                    {
                        double x = double.Parse(llz[0], System.Globalization.CultureInfo.InvariantCulture);
                        double y = double.Parse(llz[1], System.Globalization.CultureInfo.InvariantCulture);

                        if (PointInPolygon(new PointF((float)x, (float)y), poly, 1E-09))
                            skip = false;
                    };
                };

                if ((inside) && (!skip)) ids.Add(itm);
                if ((!inside) && (skip)) ids.Add(itm);
            };
            Up();
        }

        private static bool PointInPolygon(PointF point, PointF[] polygon, double EPS)
        {
            int count, up;
            count = 0;
            for (int i = 0; i < polygon.Length - 1; i++)
            {
                up = CRS(point, polygon[i], polygon[i + 1], EPS);
                if (up >= 0)
                    count += up;
                else
                    break;
            };
            up = CRS(point, polygon[polygon.Length - 1], polygon[0], EPS);
            if (up >= 0)
                return Convert.ToBoolean((count + up) & 1);
            else
                return false;
        }

        private static int CRS(PointF P, PointF A1, PointF A2, double EPS)
        {
            double x;
            int res = 0;
            if (Math.Abs(A1.Y - A2.Y) < EPS)
            {
                if ((Math.Abs(P.Y - A1.Y) < EPS) && ((P.X - A1.X) * (P.X - A2.X) < 0.0)) res = -1;
                return res;
            };
            if ((A1.Y - P.Y) * (A2.Y - P.Y) > 0.0) return res;
            x = A2.X - (A2.Y - P.Y) / (A2.Y - A1.Y) * (A2.X - A1.X);
            if (Math.Abs(x - P.X) < EPS)
            {
                res = -1;
            }
            else
            {
                if (x < P.X)
                {
                    res = 1;
                    if ((Math.Abs(A1.Y - P.Y) < EPS) && (A1.Y < A2.Y)) res = 0;
                    else
                        if ((Math.Abs(A2.Y - P.Y) < EPS) && (A2.Y < A1.Y)) res = 0;
                };
            };
            return res;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://en.wikipedia.org/wiki/Regular_expression");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            button9_Click(sender, e);
            Close();
        }

        private void Selection_Filter_Load(object sender, EventArgs e)
        {
            ApplyTo.SelectedIndex = 0;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(button12, new Point(0, 0));
        }

        private void defaultRegexForNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = @"^([_\w\s\.\,\-\№\#\:\/\\\!\?" + "\"" + @"'`\*\^\(\)\[\]\@\$\%\+]*)$";
            ApplyTo.SelectedIndex = 0;
        }

        private void descriptionWebSiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = @"website=([^\r\n]+)";
            ApplyTo.SelectedIndex = 1;
        }

        private void descriptionEmailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = @"email=([^\r\n]+)";
            ApplyTo.SelectedIndex = 1;
        }

        private void descriptionPhoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = @"phone=([^\r\n]+)";
            ApplyTo.SelectedIndex = 1;
        }
    }
}