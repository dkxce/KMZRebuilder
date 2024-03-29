using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace KMZRebuilder
{
    public partial class PolyCreator : Form
    {
        private WaitingBoxForm wbf = null;
        public NaviMapNet.MapLayer mapContent = null;
        public ToolTip mapTootTip = new ToolTip();
        private KMZRebuilederForm parent = null;

        private string SASPlanetCacheDir = @"C:\Program Files\SASPlanet\cache\osmmapMapnik\";
        private string UserDefindedUrl = @"http://tile.openstreetmap.org/{z}/{x}/{y}.png";
        private string UserDefindedFile = @"C:\nofile.mbtiles";

        public PolyCreator(KMZRebuilederForm parent)
        {
            this.parent = parent;
            Init();
        }

        public PolyCreator(KMZRebuilederForm parent, WaitingBoxForm waitBox)
        {
            this.parent = parent;
            this.wbf = waitBox;
            Init();
        }

        public PolyCreator(KMZRebuilederForm parent, WaitingBoxForm waitBox, System.Drawing.PointF[] points, bool closed)
        {
            this.parent = parent;
            this.wbf = waitBox;
            Init();

            if(points != null)
                inMapPoly.AddRange(points);

            if (inMapPoly.Count > 0)
            {
                cLR.Checked = false;
                NaviMapNet.MapPolygon mp = new NaviMapNet.MapPolygon(inMapPoly.ToArray());
                MView.CenterDegrees = mp.Center;
            };
            RedrawPoly();
        }

        MruList mru1;
        State state;
        private void Init()
        {
            InitializeComponent();
            mapTootTip.ShowAlways = true;

            mapContent = new NaviMapNet.MapLayer("mapContent");
            MView.MapLayers.Add(mapContent);

            string fn = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.stt";
            if (File.Exists(fn)) state = State.Load(fn);

            mru1 = new MruList(KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.drs", spcl, 10);
            mru1.FileSelected += new MruList.FileSelectedEventHandler(mru1_FileSelected);

            mapTootTip.ShowAlways = true;


            // LOAD NO MAP
            iStorages.Items.Add(new MapStore("[[*** No Map ***]]", "", null));

            // LOAD MAPS FROM FILE
            string mf = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.maps";
            if (File.Exists(mf))
            {
                MapStore[] mss = XMLSaved<MapStore[]>.Load(mf);
                if ((mss != null) && (mss.Length > 0))
                    iStorages.Items.AddRange(mss);
            };

            //iStorages.Items.Add("OSM Mapnik Render Tiles");
            //iStorages.Items.Add("OSM OpenVkarte Render Tiles");
            //iStorages.Items.Add("Wikimapia");

            //iStorages.Items.Add("OpenTopoMaps");
            //iStorages.Items.Add("Sputnik.ru");
            //iStorages.Items.Add("RUMAP");
            //iStorages.Items.Add("2GIS");
            //iStorages.Items.Add("ArcGIS ESRI");

            //iStorages.Items.Add("Nokia-Ovi");
            //iStorages.Items.Add("OviMap");
            //iStorages.Items.Add("OviMap Sputnik");
            //iStorages.Items.Add("OviMap Relief");
            //iStorages.Items.Add("OviMap Hybrid");

            //iStorages.Items.Add("Kosmosnimki.ru ScanEx 1");
            //iStorages.Items.Add("Kosmosnimki.ru ScanEx 2");
            //iStorages.Items.Add("Kosmosnimki.ru IRS Sat");

            //iStorages.Items.Add("Google Map");
            //iStorages.Items.Add("Google Sat");

            // LOAD USER-DEFINED MAPS
            iStorages.Items.Add(new MapStore("[[*** MBTiles file ***]]", "", NaviMapNet.NaviMapNetViewer.MapServices.Custom_MBTiles));
            iStorages.Items.Add(new MapStore("[[*** User-Defined Url ***]]", "", "URLDefined"));
            iStorages.Items.Add(new MapStore("[[*** SAS Planet Cache ***]]", "", "SASPlanet"));

            MView.NotFoundTileColor = Color.LightYellow;
            MView.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Custom_LocalFiles;
            MView.ImageSourceUrl = @"C:\Program Files\SASPlanet\cache\osmmapMapnik\";
            MView.WebRequestTimeout = 10000;
            MView.ZoomID = 10;
            MView.OnMapUpdate = new NaviMapNet.NaviMapNetViewer.MapEvent(MapUpdate);

            MView.UserDefinedGetTileUrl = new NaviMapNet.NaviMapNetViewer.GetTilePathCall(UserDefinedGetTileUrl);

            //MapViewer.DrawMap = true;
            //MapViewer.ReloadMap();

            //iStorages.SelectedIndex = iStorages.Items.Count - 2;    

            if (state != null)
            {
                SASPlanetCacheDir = state.SASDir;
                UserDefindedUrl = state.URL;
                UserDefindedFile = state.FILE;
                if (MView.ZoomID > 0)
                {
                    MView.CenterDegrees = new PointF((float)state.X, (float)state.Y);
                    MView.ZoomID = state.Z;
                };

                if (state.MapID < iStorages.Items.Count)
                    iStorages.SelectedIndex = state.MapID;
            };
        }

        private void mru1_FileSelected(string file_name)
        {
            SASPlanetCacheDir = ClearLastSlash(file_name);
            mru1.AddFile(SASPlanetCacheDir);

            if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
                iStorages_SelectedIndexChanged(this, null);
            else
                iStorages.SelectedIndex = iStorages.Items.Count - 1;
        }

        public string ClearLastSlash(string file_name)
        {
            if (file_name.Substring(file_name.Length - 1) == @"\")
                return file_name.Remove(file_name.Length - 1);
            return file_name;
        }


        private void iStorages_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapStore iS = (MapStore)iStorages.SelectedItem;

            MView.ImageSourceService = iS.Service;
            MView.ImageSourceType = iS.Source;
            MView.ImageSourceProjection = iS.Projection;

            if (iStorages.SelectedIndex < (iStorages.Items.Count - 1))
            {
                MView.UseDiskCache = true;
                MView.UserDefinedMapName = iS.CacheDir;

                if (iStorages.SelectedIndex == (iStorages.Items.Count - 2))
                    MView.ImageSourceUrl = UserDefindedUrl;
                else if (iStorages.SelectedIndex == (iStorages.Items.Count - 3))
                {
                    MView.UseDiskCache = false;
                    MView.ImageSourceUrl = UserDefindedFile;
                }
                else
                    MView.ImageSourceUrl = iS.Url;
            };

            if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
            {
                MView.UseDiskCache = false;
                MView.UserDefinedMapName = iS.CacheDir = @"LOCAL\" + SASPlanetCacheDir.Substring(SASPlanetCacheDir.LastIndexOf(@"\") + 1);
                MView.ImageSourceUrl = SASPlanetCacheDir;
            };

            iStorages.Refresh();
            MView.ReloadMap();
        }
        
        private void MapUpdate()
        {
            string lreq = MView.LastRequestedFile;
            if (lreq.Length > 50) lreq = "... " + lreq.Substring(lreq.Length - 50);
            toolStripStatusLabel1.Text = "Last Requested File: " + lreq;
            toolStripStatusLabel2.Text = MView.CenterDegreesLat.ToString().Replace(",", ".");
            toolStripStatusLabel3.Text = MView.CenterDegreesLon.ToString().Replace(",", ".");
        }

        private void MapViewer_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private List<PointF> inMapPoly = new List<PointF>();
        private PointF[] finalpoly = null;

        private void MapViewer_MouseClick(object sender, MouseEventArgs e)
        {
            if (moved) return;

            PointF clicked = MView.MousePositionDegrees;
            inMapPoly.Add(clicked);
            RedrawPoly();
        }

        
        public static DialogResult InputXY(bool changeXY, ref string value, ref string lat, ref string lon, ref string desc)
        {
            Form form = new Form();
            Label nameText = new Label();
            Label xText = new Label();
            Label yText = new Label();
            Label dText = new Label();
            TextBox nameBox = new TextBox();
            TextBox xBox = new TextBox();
            TextBox yBox = new TextBox();
            TextBox dBox = new TextBox();
            dBox.Multiline = true;
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = "Change placemark";
            nameText.Text = "Name:";
            nameBox.Text = value;
            xText.Text = "Longitude:";
            xBox.Text = lon;
            yText.Text = "Latitude:";
            yBox.Text = lat;
            dText.Text = "Description:";
            dBox.Text = desc;

            if (!changeXY) xBox.Enabled = yBox.Enabled = false;

            xBox.KeyPress += new KeyPressEventHandler(xy_KeyPress);
            yBox.KeyPress += new KeyPressEventHandler(xy_KeyPress);
            
            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            nameText.SetBounds(9, 10, 372, 13);
            nameBox.SetBounds(12, 26, 372, 20);
            yText.SetBounds(9, 50, 372, 13);
            yBox.SetBounds(12, 66, 372, 20);
            xText.SetBounds(9, 90, 372, 13);
            xBox.SetBounds(12, 106, 372, 20);
            dText.SetBounds(9, 130, 372, 13);
            dBox.SetBounds(12, 146, 372, 180);

            buttonOk.SetBounds(228, 337, 75, 23);
            buttonCancel.SetBounds(309, 337, 75, 23);
            
            nameText.AutoSize = true;
            nameBox.Anchor = nameBox.Anchor | AnchorStyles.Right;
            yBox.Anchor = yBox.Anchor | AnchorStyles.Right;
            xBox.Anchor = xBox.Anchor | AnchorStyles.Right;
            dBox.Anchor = dBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 370);
            form.Controls.AddRange(new Control[] { nameText, nameBox, yText, yBox, xText, xBox, dText, dBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, nameText.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            form.Dispose();
            if(dialogResult == DialogResult.OK)
            value = nameBox.Text;
            lat = yBox.Text;
            lon = xBox.Text;
            desc = dBox.Text;
            return dialogResult;
        }

        private static void xy_KeyPress(object sender, KeyPressEventArgs e)
        {
            // allows 0-9, backspace, and decimal, and -
            if (((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 8 && e.KeyChar != 46 && e.KeyChar != 45))
            {
                e.Handled = true;
                return;
            }

            // checks to make sure only 1 decimal is allowed
            if (e.KeyChar == 46)
            {
                if ((sender as TextBox).Text.IndexOf(e.KeyChar) != -1)
                    e.Handled = true;
            }

            // checks to make sure only 1 - is allowed
            if (e.KeyChar == 45)
            {
                if ((sender as TextBox).SelectionStart != 0)
                    e.Handled = true;
                if ((sender as TextBox).Text.IndexOf(e.KeyChar) != -1)
                    e.Handled = true;
            }
        }

        private string UserDefinedGetTileUrl(int x, int y, int z)
        {
            if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
                return SASPlanetCache(x, y, z + 1);
            return "";
        }
        
        private string SASPlanetCache(int x, int y, int z)
        {
            string basedir = String.Format(@"{1}\z{0}", z, SASPlanetCacheDir);
            if (!Directory.Exists(basedir)) return "none";

            string xDir = "x" + x.ToString();
            string[] xdirs = Directory.GetDirectories(basedir);
            if ((xdirs == null) || (xdirs.Length == 0)) return "none";
            foreach (string xdir in xdirs)
            {
                string xx = xdir + @"\x" + x.ToString();
                if (Directory.Exists(xx))
                {
                    string[] ydirs = Directory.GetDirectories(xx);
                    if ((ydirs == null) || (ydirs.Length == 0)) return "none";
                    foreach (string ydir in ydirs)
                    {
                        string yy = ydir + @"\y" + y.ToString() + ".png";
                        if (File.Exists(yy))
                            return yy;
                    };
                };
            };

            return "none";
        }

        private void ��������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string spcd = SASPlanetCacheDir;
            if (KMZRebuilederForm.InputBox("SAS Planet Cache", "Enter Cache Path Here:", ref spcd) == DialogResult.OK)
                SASPlanetCacheDir = ClearLastSlash(spcd);
            else
                return;

            if (Directory.Exists(SASPlanetCacheDir))
                mru1.AddFile(SASPlanetCacheDir);

            if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
                iStorages_SelectedIndexChanged(sender, e);
            else
                iStorages.SelectedIndex = iStorages.Items.Count - 1;
        }

        private void ��������UserDefinedUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string udu = UserDefindedUrl;
            if (KMZRebuilederForm.InputBox("User-Defined Url", "Enter Url Here:", ref udu) == DialogResult.OK)
            {
                UserDefindedUrl = udu;
                if (iStorages.SelectedIndex == (iStorages.Items.Count - 2))
                    iStorages_SelectedIndexChanged(sender, e);
                else
                    iStorages.SelectedIndex = iStorages.Items.Count - 2;
            };
        }

        private void button2_Click(object sender, EventArgs e)
        {
            inMapPoly.Clear();
            RedrawPoly();
        }

        private void RedrawPoly()
        {
            mapContent.Clear();
            finalpoly = null;
            string ltxt = "Inline points: ";

            if (inMapPoly.Count == 0)
            {
                MView.DrawOnMapData();
                ltxt += "0";
                cntLabel.Text = ltxt;
                return;
            };
            if (inMapPoly.Count == 1)
            {
                NaviMapNet.MapPoint ms = new NaviMapNet.MapPoint(inMapPoly[0]);
                ms.BodyColor = Color.Red;
                ms.BorderColor = Color.FromArgb(125, Color.Red);
                ms.SizePixels = new Size(16, 16);
                ms.Name = "Start";
                mapContent.Add(ms);
                ltxt += "1";
            }
            else
            {                
                if (!cLR.Checked)
                {
                    NaviMapNet.MapPolygon mp = new NaviMapNet.MapPolygon(inMapPoly.ToArray());
                    mp.Width = 2;
                    mp.Name = "MyPoly";
                    mp.Color = Color.FromArgb(125, Color.Red);
                    mp.BorderColor = Color.Red;
                    mapContent.Add(mp);
                    finalpoly = mp.Points;
                    ltxt += finalpoly.Length.ToString() + " / Polygon points: " + finalpoly.Length.ToString();
                }
                else
                {
                    cntLabel.Text = "Inline points: " + inMapPoly.Count.ToString() + " / Polygon points calculating...";
                    NaviMapNet.MapPolyLine ml = new NaviMapNet.MapPolyLine(inMapPoly.ToArray());
                    ml.Width = 2;
                    ml.Name = "MyLine";
                    ml.Color = Color.FromArgb(125, Color.Maroon);
                    ml.BorderColor = Color.Maroon;
                    PointF[] source = inMapPoly.ToArray();
                    int si = (selMethod.SelectedIndex - 1) % 6;
                    int ri = (selMethod.SelectedIndex - 1) / 6;
                    if (selMethod.SelectedIndex == 25) si = 5;
                    if ((ri == 1) || (ri == 3))
                    {
                        cntLabel.Text = "Inline points: optimizing... [" + inMapPoly.Count.ToString()+"]";
                        statusStrip2.Refresh();
                        source = PolyLineBuffer.PolyLineBufferCreator.Interpolate(source, (float)4.5, PolyLineBuffer.PolyLineBufferCreator.GeographicDistFunc);
                        cntLabel.Text = "Inline points: " + source.Length.ToString() + " [" + inMapPoly.Count.ToString() + "] / Polygon points calculating...";
                        statusStrip2.Refresh();
                        ltxt += source.Length.ToString() + " [" + inMapPoly.Count.ToString() + "]";
                    }
                    else
                    {
                        cntLabel.Text = "Inline points: " + source.Length.ToString() + " / Polygon points calculating...";
                        ltxt += source.Length.ToString();
                    };
                    statusStrip2.Refresh();
                    ltxt += " / Polygon points: ";
                    PolyLineBuffer.PolyLineBufferCreator.PolyResult pr = PolyLineBuffer.PolyLineBufferCreator.GetLineBufferPolygon(source, (int)cD.Value, cR.Checked, cL.Checked, PolyLineBuffer.PolyLineBufferCreator.GeographicDistFunc, si);
                    if ((ri == 2) || (ri == 3))
                    {                        
                        int desc_c = pr.polygon.Length;
                        cntLabel.Text = ltxt + "optimizing... [" + desc_c.ToString() + "]";
                        statusStrip2.Refresh();
                        pr.polygon = PolyLineBuffer.PolyLineBufferCreator.Interpolate(pr.polygon, (float)3.5, PolyLineBuffer.PolyLineBufferCreator.GeographicDistFunc);
                        ltxt += pr.polygon.Length.ToString() + " [" + desc_c.ToString() + "]";
                    }
                    else
                        ltxt += pr.polygon.Length.ToString();
                    cntLabel.Text = ltxt;
                    statusStrip2.Refresh();

                    NaviMapNet.MapPolygon mp = new NaviMapNet.MapPolygon(pr.polygon);
                    mp.Width = 2;
                    mp.Name = "MyPoly";
                    mp.Color = Color.FromArgb(125, Color.Red);
                    mp.BorderColor = Color.Red;
                    mapContent.Add(mp);
                    mapContent.Add(ml);
                    finalpoly = mp.Points;
                };

                NaviMapNet.MapPoint ms = new NaviMapNet.MapPoint(inMapPoly[0]);
                ms.BodyColor = Color.Red;
                ms.BorderColor = Color.FromArgb(125, Color.Red);
                ms.SizePixels = new Size(16, 16);
                ms.Name = "Start";
                mapContent.Add(ms);

                NaviMapNet.MapPoint me = new NaviMapNet.MapPoint(inMapPoly[inMapPoly.Count - 1]);
                me.BodyColor = Color.Green;
                me.BorderColor = Color.FromArgb(125, Color.Red);
                me.SizePixels = new Size(16, 16);
                me.Name = "End";
                mapContent.Add(me);
            };
            MView.DrawOnMapData();
            uint distim = PolyLineBuffer.PolyLineBufferCreator.GetDistInMeters(finalpoly, true);
            double square =  PolyLineBuffer.PolyLineBufferCreator.GetSquareInMeters(finalpoly);
            cntLabel.Text = ltxt;
            persqr.Text = 
                "P = " + (distim < 1000 ? distim.ToString() + " m" : ((double)distim / 1000.0).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km")
                +
                " / S = " + square.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km2";

        }
        
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            cD.Enabled = cL.Enabled = cR.Enabled = cLR.Checked;
            RedrawPoly();
        }

        private void cR_CheckedChanged(object sender, EventArgs e)
        {
            RedrawPoly();
        }

        private void cL_CheckedChanged(object sender, EventArgs e)
        {
            RedrawPoly();
        }

        private void cD_ValueChanged(object sender, EventArgs e)
        {
            RedrawPoly();
        }

        private bool moved = false;
        private void MView_MouseDown(object sender, MouseEventArgs e)
        {
            moved = false;
        }

        private void MView_MouseMove(object sender, MouseEventArgs e)
        {
            moved = true;

            PointF m = MView.MousePositionDegrees;
            toolStripStatusLabel4.Text = m.Y.ToString().Replace(",", ".");
            toolStripStatusLabel5.Text = m.X.ToString().Replace(",", ".");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (inMapPoly.Count > 0) inMapPoly.RemoveAt(inMapPoly.Count - 1);
            RedrawPoly();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "KML, GPX & Shape files (*.kml;*.gpx;*.shp;)|*.kml;*.gpx;*.shp|BBBike Extract Text with Url (*.txt)|*.txt";
            ofd.DefaultExt = "*.kml,*.gpx";            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                loadroute(ofd.FileName);
            };
            ofd.Dispose();
        }

        public static PointF[] LoadPolygonFromTextBBBike(string filename)
        {
            System.IO.FileStream fs = null;
            System.IO.StreamReader sr = null;
            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(fs);
                List<PointF> res = new List<PointF>();

                string line = sr.ReadLine();
                if (line.StartsWith("[BBIKE_EXTRACT_LINK]")) line = sr.ReadLine();
                Regex rx = new Regex(@"coords=(?<coords>[^=\r\n\&\#]*)", RegexOptions.IgnoreCase);
                Match mx = rx.Match(line);
                if (!mx.Success) return null;
                string coords = mx.Groups["coords"].Value.ToUpper().Replace("%2C", ",").Replace("%7C", "|");
                if (coords.Length == 0) return null;
                string[] coord = coords.Split(new char[] { '|' });
                foreach (string c in coord)
                {
                    string[] xy = c.Split(new char[] { ',' });
                    PointF pf = new PointF((float)double.Parse(xy[0], System.Globalization.CultureInfo.InvariantCulture), (float)double.Parse(xy[1], System.Globalization.CultureInfo.InvariantCulture));
                    res.Add(pf);
                };
                return res.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                if (sr != null) sr.Close();
                if (fs != null) fs.Close();
            };
        }

        private void loadroute(string filename)
        {
            System.IO.FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            System.IO.StreamReader sr = new StreamReader(fs);
            inMapPoly.Clear();

            if (System.IO.Path.GetExtension(filename).ToLower() == ".txt")
            {
                try
                {
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
                        inMapPoly.Add(pf);
                    };
                    cLR.Checked = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    inMapPoly.Clear();
                    return;
                };
            };

            if (System.IO.Path.GetExtension(filename).ToLower() == ".shp")
            {
                fs.Position = 32;
                int tof = fs.ReadByte();
                if ((tof == 3) || (tof == 5))
                {
                    fs.Position = 104;
                    byte[] ba = new byte[4];
                    fs.Read(ba, 0, ba.Length);
                    if(BitConverter.IsLittleEndian) Array.Reverse(ba);
                    int len = BitConverter.ToInt32(ba, 0) * 2;
                    fs.Read(ba, 0, ba.Length);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(ba);
                    tof = BitConverter.ToInt32(ba, 0);
                    if ((tof == 3) || (tof == 5))
                    {
                        if (tof == 3) cLR.Checked = true;
                        if (tof == 5) cLR.Checked = false;
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
                                    inMapPoly.Add(ap);
                                };
                            };
                        };
                    };
                };
            };
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
                        inMapPoly.Add(new PointF(float.Parse(xyz[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(xyz[1], System.Globalization.CultureInfo.InvariantCulture)));
                    };
            };
            if (System.IO.Path.GetExtension(filename).ToLower() == ".gpx")
            {
                string file = sr.ReadToEnd();
                int si = 0;
                int ei = 0;
                // rtept
                si = file.IndexOf("<rtept", ei);
                if(si > 0)
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
                    inMapPoly.Add(new PointF(float.Parse(lon, System.Globalization.CultureInfo.InvariantCulture), float.Parse(lat, System.Globalization.CultureInfo.InvariantCulture)));

                    si = file.IndexOf("<rtept", ei);
                    if (si > 0)
                        ei = file.IndexOf(">", si);
                };
                // trkpt
                si = file.IndexOf("<trkpt", ei);
                if (si > 0)
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
                    inMapPoly.Add(new PointF(float.Parse(lon, System.Globalization.CultureInfo.InvariantCulture), float.Parse(lat, System.Globalization.CultureInfo.InvariantCulture)));

                    si = file.IndexOf("<trkpt", ei);
                    if (si > 0)
                        ei = file.IndexOf(">", si);
                };
            };
            sr.Close();
            fs.Close();

            if (inMapPoly.Count > 0)
            {
                if (System.IO.Path.GetExtension(filename).ToLower() != ".shp")
                    cLR.Checked = false;
                NaviMapNet.MapPolygon mp = new NaviMapNet.MapPolygon(inMapPoly.ToArray());
                MView.CenterDegrees = mp.Center;
            };
            RedrawPoly();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SavePoly2File(finalpoly);
        }

        public static void SavePoly2File(PointF[] polygon)
        {
            if (polygon == null) return;
            if (polygon.Length < 2) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".kml";
            sfd.Filter = "KML files (*.kml)|*.kml|ESRI Shape files (*.shp)|*.shp|BBBike Extract Url Link (*.txt)|*.txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (sfd.FilterIndex == 1)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document>");
                    sw.WriteLine("<Placemark><name>My Polygon</name>");
                    sw.Write("<Polygon><extrude>1</extrude><outerBoundaryIs><LinearRing><coordinates>");
                    foreach (PointF p in polygon)
                        sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", p.X, p.Y));
                    sw.WriteLine("</coordinates></LinearRing></outerBoundaryIs></Polygon></Placemark>");
                    sw.WriteLine("</Document>");
                    sw.WriteLine("</kml>");
                    sw.Close();
                    fs.Close();
                };
                if (sfd.FilterIndex == 2)
                    Save2Shape(sfd.FileName, polygon);
                if (sfd.FilterIndex == 3) // BBike
                {
                    string url = Poly2TextUrl(polygon);
                    if (InputBox.Show("Export to URL", "Url BBBike Extract:", url) == DialogResult.OK)
                        Save2TextUrl(sfd.FileName, url);
                };
            };
            sfd.Dispose();
        }

        //
        // https://extract.bbbike.org/
        // ?sw_lng=39.82&sw_lat=42.17&ne_lng=43.411&ne_lat=43.623
        // &format=mapsforge-osm.zip
        // &coords=41.468%2C42.17%7C42.109%2C42.254%7C42.442%2C42.414%7C43.053%2C42.645%7C43.411%2C42.955%7C42.839%2C43.249%7C42.252%2C43.31%7C41.461%2C43.367%7C40.748%2C43.585%7C40.08%2C43.623%7C39.879%2C43.448%7C39.82%2C43.272%7C40.144%2C43.066%7C40.759%2C42.867%7C41.32%2C42.511
        // &city=Abhazia
        // &lang=ru
        //
        public static string Poly2TextUrl(PointF[] poly)
        {
            double xmin = double.MaxValue;
            double ymin = double.MaxValue;
            double xmax = double.MinValue;
            double ymax = double.MinValue;

            string pll = "";
            for (int i = 0; i < poly.Length; i++)
            {
                xmin = Math.Min(xmin, poly[i].X);
                ymin = Math.Min(ymin, poly[i].Y);
                xmax = Math.Max(xmax, poly[i].X);
                ymax = Math.Max(ymax, poly[i].Y);
                if (pll.Length > 0)
                    pll += "|";
                pll += String.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0},{1}", poly[i].X, poly[i].Y);
            };

            string url = "https://extract.bbbike.org/";
            url += String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "?sw_lng={0}&sw_lat={1}&ne_lng={2}&ne_lat={3}",
                xmin, ymin, xmax, ymax);
            url += "&format=mapsforge-osm.zip";
            url += "&coords=" + pll;
            url += "&city=Noname";
            url += "&lang=en";

            return url;
        }

        public static void Save2TextUrl(string filename, string url)
        {                                    
            if (filename == null) return;

            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.ASCII);
            sw.WriteLine("[BBIKE_EXTRACT_LINK]");
            sw.WriteLine(url);
            sw.Close();
            fs.Close();            
        }

        public static void Save2Shape(string filename, PointF[] poly)
        {
            double xmin = double.MaxValue;
            double ymin = double.MaxValue;
            double xmax = double.MinValue;
            double ymax = double.MinValue;

            for (int i = 0; i < poly.Length; i++)
            {
                xmin = Math.Min(xmin, poly[i].X);
                ymin = Math.Min(ymin, poly[i].Y);
                xmax = Math.Max(xmax, poly[i].X);
                ymax = Math.Max(ymax, poly[i].Y);
            };

            List<byte> arr = new List<byte>();
            arr.AddRange(Convert(BitConverter.GetBytes((int)9994), false)); // File Code
            arr.AddRange(new byte[20]);                                    // Not used
            arr.AddRange(Convert(BitConverter.GetBytes((int)((100 + 8 + 48 + 16 * poly.Length)/2)), false)); // File_Length / 2
            arr.AddRange(Convert(BitConverter.GetBytes((int)1000), true)); // Version 1000
            arr.AddRange(Convert(BitConverter.GetBytes((int)5), true)); // Polygon Type
            arr.AddRange(Convert(BitConverter.GetBytes((double)xmin), true));
            arr.AddRange(Convert(BitConverter.GetBytes((double)ymin), true));
            arr.AddRange(Convert(BitConverter.GetBytes((double)xmax), true));
            arr.AddRange(Convert(BitConverter.GetBytes((double)ymax), true));
            arr.AddRange(new byte[32]); // end of header

            arr.AddRange(Convert(BitConverter.GetBytes((int)1), false)); // rec number
            arr.AddRange(Convert(BitConverter.GetBytes((int)((48 + 16 * poly.Length) / 2)), false));// rec_length / 2
            arr.AddRange(Convert(BitConverter.GetBytes((int)5), true)); // rec type polygon
            arr.AddRange(Convert(BitConverter.GetBytes((double)xmin), true));
            arr.AddRange(Convert(BitConverter.GetBytes((double)ymin), true));
            arr.AddRange(Convert(BitConverter.GetBytes((double)xmax), true));
            arr.AddRange(Convert(BitConverter.GetBytes((double)ymax), true));
            arr.AddRange(Convert(BitConverter.GetBytes((int)1), true)); // 1 part
            arr.AddRange(Convert(BitConverter.GetBytes((int)poly.Length), true)); // 4 points
            arr.AddRange(Convert(BitConverter.GetBytes((int)0), true)); // start at 0 point

            for (int i = 0; i < poly.Length; i++)
            {
                arr.AddRange(Convert(BitConverter.GetBytes((double)poly[i].X), true)); // point 0 x
                arr.AddRange(Convert(BitConverter.GetBytes((double)poly[i].Y), true)); // point 0 y
            };
            
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            fs.Write(arr.ToArray(), 0, arr.Count);
            fs.Close();
        }

        public static byte[] Convert(byte[] ba, bool bigEndian)
        {
            if (BitConverter.IsLittleEndian != bigEndian) Array.Reverse(ba);
            return ba;
        }

        private void toolStripDropDownButton1_DropDownOpening(object sender, EventArgs e)
        {
            spcl.Enabled = mru1.Count > 0;
            showBBBikeExtractUrlToolStripMenuItem.Enabled = (finalpoly != null) && (finalpoly.Length > 2);
        }

        private void PolyCreator_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void PolyCreator_FormClosing(object sender, FormClosingEventArgs e)
        {
            state = new State();
            state.MapID = iStorages.SelectedIndex;
            state.SASDir = SASPlanetCacheDir;
            state.URL = UserDefindedUrl;
            state.FILE = UserDefindedFile;
            state.X = MView.CenterDegreesX;
            state.Y = MView.CenterDegreesY;
            state.Z = MView.ZoomID;
            string fn = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.stt";
            State.Save(fn, state);
        }

        private void ����������������SASPlanetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            try
            {
                fbd.SelectedPath = SASPlanetCacheDir;
            }
            catch { };
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                SASPlanetCacheDir = ClearLastSlash(fbd.SelectedPath);
                mru1.AddFile(SASPlanetCacheDir);
                if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
                    iStorages_SelectedIndexChanged(sender, e);
                else
                    iStorages.SelectedIndex = iStorages.Items.Count - 1;
            };
            fbd.Dispose();
        }

        private void PolyCreator_Load(object sender, EventArgs e)
        {
            selMethod.SelectedIndex = 1;
            MView.DrawMap = true;
        }

        private void showBBBikeExtractUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string url = Poly2TextUrl(finalpoly);
            InputBox.Show("URL BOX", "Url BBBike Extract:", url);
        }

        private void selMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selMethod.SelectedIndex == 0)
            {
                selMethod.SelectedIndex = 1;
                return;
            };
            if (selMethod.SelectedIndex < 25)
            {
                int si = (selMethod.SelectedIndex - 1) % 6;
                int ri = (selMethod.SelectedIndex - 1) / 6;
                if (si == 5)
                {
                    selMethod.SelectedIndex = selMethod.SelectedIndex + 1;
                    return;
                };
                if (((ri == 0) || (ri == 2)) && ((si >= 1) && (si <= 4)) && (inMapPoly.Count > 2000))
                {
                    selMethod.SelectedIndex = selMethod.SelectedIndex - si;
                    return;
                };
            };

            cD.Enabled = cL.Enabled = cR.Enabled = cLR.Checked;
            RedrawPoly();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (inMapPoly.Count < 2) return;
            PointF mp = inMapPoly[0];;
            inMapPoly.RemoveAt(0);
            inMapPoly.Add(mp);
            RedrawPoly();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (inMapPoly.Count < 2) return;
            PointF mp = inMapPoly[inMapPoly.Count-1];
            inMapPoly.RemoveAt(inMapPoly.Count - 1);
            inMapPoly.Insert(0,mp);
            RedrawPoly();
        }

        private void selmbtfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fName = null;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select MBTiles File";
            ofd.DefaultExt = ".mbtiles";
            ofd.Filter = "All supported files|*.mbtiles;*.sqlite;*.db;*.db3|All Types (*.*)|*.*";
            try { ofd.FileName = UserDefindedFile; }
            catch { };
            if (ofd.ShowDialog() == DialogResult.OK) fName = ofd.FileName;
            ofd.Dispose();

            if (!String.IsNullOrEmpty(fName))
            {
                UserDefindedFile = fName;
                if (iStorages.SelectedIndex == (iStorages.Items.Count - 3))
                    iStorages_SelectedIndexChanged(sender, e);
                else
                    iStorages.SelectedIndex = iStorages.Items.Count - 3;
            };
        }
    }
}