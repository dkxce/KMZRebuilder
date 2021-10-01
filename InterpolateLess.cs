using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace KMZRebuilder
{
    public partial class InterLessForm : Form
    {
        private WaitingBoxForm wbf = null;
        public NaviMapNet.MapLayer mapContent = null;
        public NaviMapNet.MapLayer mapInterpol = null;
        public ToolTip mapTootTip = new ToolTip();
        private KMZRebuilederForm parent = null;

        private string SASPlanetCacheDir = @"C:\Program Files\SASPlanet\cache\osmmapMapnik\";
        private string UserDefindedUrl = @"http://tile.openstreetmap.org/{z}/{x}/{y}.png";

        public InterLessForm(KMZRebuilederForm parent)
        {
            this.parent = parent;
            Init();
        }

        public InterLessForm(KMZRebuilederForm parent, WaitingBoxForm waitBox)
        {
            this.parent = parent;
            this.wbf = waitBox;
            Init();
        }

        MruList mru1;
        State state;
        private void Init()
        {
            InitializeComponent();
            mapTootTip.ShowAlways = true;

            mapInterpol = new NaviMapNet.MapLayer("mapInterpol");
            MView.MapLayers.Add(mapInterpol);
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
                if (MView.ZoomID > 0)
                {
                    MView.CenterDegrees = new PointF((float)state.X, (float)state.Y);
                    MView.ZoomID = state.Z;
                };

                if (state.MapID < iStorages.Items.Count)
                    iStorages.SelectedIndex = state.MapID;
            };

            usedMethod.SelectedIndex = 0;
        }

        public string ClearLastSlash(string file_name)
        {
            if (file_name.Substring(file_name.Length - 1) == @"\")
                return file_name.Remove(file_name.Length - 1);
            return file_name;
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
                else
                    MView.ImageSourceUrl = iS.Url;
            };

            if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
            {
                MView.UseDiskCache = false;
                MView.UserDefinedMapName = iS.CacheDir = @"LOCAL\" + SASPlanetCacheDir.Substring(SASPlanetCacheDir.LastIndexOf(@"\") + 1);
                MView.ImageSourceUrl = SASPlanetCacheDir;
            };

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

        private List<PointF> originalLine = new List<PointF>();        
        
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
            dBox.SetBounds(12, 146, 372, 80);

            buttonOk.SetBounds(228, 237, 75, 23);
            buttonCancel.SetBounds(309, 237, 75, 23);
            
            nameText.AutoSize = true;
            nameBox.Anchor = nameBox.Anchor | AnchorStyles.Right;
            yBox.Anchor = yBox.Anchor | AnchorStyles.Right;
            xBox.Anchor = xBox.Anchor | AnchorStyles.Right;
            dBox.Anchor = dBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 270);
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

        private void èçìåíèòüToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void èçìåíèòüUserDefinedUrlToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void DrawOriginalPoly()
        {
            mapInterpol.Clear();
            mapContent.Clear();            
            iterations.Items.Clear();

            if (originalLine.Count > 0)
            {
                NaviMapNet.MapPolyLine mp = new NaviMapNet.MapPolyLine(originalLine.ToArray());
                mp.Color = Color.Navy;
                mp.Width = 3;
                MView.CenterDegrees = mp.Center;
                mapContent.Add(mp);
                ListViewItem lvi = iterations.Items.Add("0");
                lvi.SubItems.Add("0.0");
                lvi.SubItems.Add(mp.PointsCount.ToString());                
            };
            MView.DrawOnMapData();
        }

        private Color[] CArr = new Color[] { Color.DarkViolet, Color.Fuchsia, Color.Red };
        private void DrawCustomPoly(float ma, PointF[] points)
        {
            if (points == null) return;
            if (points.Length == null) return;

            if (mapInterpol.ObjectsCount > 0)
                for (int i = 0; i < mapInterpol.ObjectsCount; i++)
                    mapInterpol[i].Visible = false;

            NaviMapNet.MapPolyLine mp = new NaviMapNet.MapPolyLine(points);
            mp.Color = CArr[iterations.Items.Count % 3];
            mp.Width = 9;
            mapInterpol.Add(mp);
            ListViewItem lvi = iterations.Items.Add(iterations.Items.Count.ToString());
            lvi.SubItems.Add(ma.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture));
            lvi.SubItems.Add(mp.PointsCount.ToString());
            
            lvi.Selected = true;
            lvi.Focused = true;
            
            // MView.DrawOnMapData();
        }


        private void MView_MouseMove(object sender, MouseEventArgs e)
        {
            PointF m = MView.MousePositionDegrees;
            toolStripStatusLabel4.Text = m.Y.ToString().Replace(",", ".");
            toolStripStatusLabel5.Text = m.X.ToString().Replace(",", ".");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "KML, GPX & Shape files (*.kml;*.gpx;*.shp)|*.kml;*.gpx;*.shp";
            ofd.DefaultExt = "*.kml,*.gpx";            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                loadroute(ofd.FileName);
            };
            ofd.Dispose();
        }

        public void loadroute(PointF[] route)
        {
            if (route == null) return;
            if (route.Length < 3) return;
            originalLine.AddRange(route);
            DrawOriginalPoly();
        }

        private void loadroute(string filename)
        {
            System.IO.FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            System.IO.StreamReader sr = new StreamReader(fs);
            originalLine.Clear();

            if (System.IO.Path.GetExtension(filename).ToLower() == ".shp")
            {
                fs.Position = 32;
                int tof = fs.ReadByte();
                if ((tof == 3))
                {
                    fs.Position = 104;
                    byte[] ba = new byte[4];
                    fs.Read(ba, 0, ba.Length);
                    if(BitConverter.IsLittleEndian) Array.Reverse(ba);
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
                                    originalLine.Add(ap);
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
                        originalLine.Add(new PointF(float.Parse(xyz[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(xyz[1], System.Globalization.CultureInfo.InvariantCulture)));
                    };
            };
            if (System.IO.Path.GetExtension(filename).ToLower() == ".gpx")
            {
                string file = sr.ReadToEnd();
                int si = 0;
                int ei = 0;
                // rtept
                {
                    si = file.IndexOf("<rtept", ei);
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
                        originalLine.Add(new PointF(float.Parse(lon, System.Globalization.CultureInfo.InvariantCulture), float.Parse(lat, System.Globalization.CultureInfo.InvariantCulture)));

                        si = file.IndexOf("<rtept", ei);
                        if (si > 0)
                            ei = file.IndexOf(">", si);
                    };
                };
                // trkpt
                {
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
                        originalLine.Add(new PointF(float.Parse(lon, System.Globalization.CultureInfo.InvariantCulture), float.Parse(lat, System.Globalization.CultureInfo.InvariantCulture)));

                        si = file.IndexOf("<trkpt", ei);
                        if (si > 0)
                            ei = file.IndexOf(">", si);
                    };
                };
            };
            sr.Close();
            fs.Close();
            
            DrawOriginalPoly();
        }

        public static void Save2Shape(string filename, PointF[] poly)
        {
            double xmin = double.MaxValue;
            double ymin = double.MinValue;
            double xmax = double.MaxValue;
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
            arr.AddRange(Convert(BitConverter.GetBytes((int)3), true)); // Polyline Type
            arr.AddRange(Convert(BitConverter.GetBytes((double)xmin), true));
            arr.AddRange(Convert(BitConverter.GetBytes((double)ymin), true));
            arr.AddRange(Convert(BitConverter.GetBytes((double)xmax), true));
            arr.AddRange(Convert(BitConverter.GetBytes((double)ymax), true));
            arr.AddRange(new byte[32]); // end of header

            arr.AddRange(Convert(BitConverter.GetBytes((int)1), false)); // rec number
            arr.AddRange(Convert(BitConverter.GetBytes((int)((48 + 16 * poly.Length) / 2)), false));// rec_length / 2
            arr.AddRange(Convert(BitConverter.GetBytes((int)3), true)); // rec type Polyline
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

        private void button2_Click(object sender, EventArgs e)
        {
            if (mapContent.ObjectsCount == 0) return;

            PointF[] src = mapContent[0].Points;
            if (src.Length < 3) return;
            src = PolyLineBuffer.PolyLineBufferCreator.Interpolate(src, (float)maVal.Value, PolyLineBuffer.PolyLineBufferCreator.GeographicDistFunc, usedMethod.SelectedIndex);
            DrawCustomPoly((float)maVal.Value, src);            
        }        

        private void iterations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mapInterpol.ObjectsCount > 0)
                for (int i = 0; i < mapInterpol.ObjectsCount; i++)
                    mapInterpol[i].Visible = false;

            if((iterations.SelectedIndices.Count > 0) && (iterations.SelectedIndices[0] > 0))
                mapInterpol[iterations.SelectedIndices[0] - 1].Visible = true;

            MView.DrawOnMapData();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            SaveBt.Enabled = (iterations.SelectedIndices.Count > 0) && (iterations.SelectedIndices[0] > 0);
        }

        private PointF[] sdcbres = null;
        public DialogResult ShowDialogCallBack(out PointF[] poly)
        {
            sdcbres = new PointF[0];
            DialogResult res = this.ShowDialog();
            poly = sdcbres;
            return res;
        }

        private void SaveBt_Click(object sender, EventArgs e)
        {
            if((iterations.SelectedIndices.Count == 0) || (iterations.SelectedIndices[0] == 0)) return;

            PointF[] finalLine = mapInterpol[iterations.SelectedIndices[0] - 1].Points;
            if (finalLine == null) return;
            if (finalLine.Length < 2) return;

            if (sdcbres != null)
            {
                sdcbres = finalLine;
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            };

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".kml";
            sfd.Filter = "KML files (*.kml)|*.kml|ESRI Shape files (*.shp)|*.shp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (sfd.FilterIndex == 1)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document>");
                    sw.WriteLine("<name>MyWay</name><createdby>KMZ Rebuilder</createdby><Folder><name>MyWay</name><Placemark><name>MyWay</name><styleUrl>#styleLOI0</styleUrl><description></description>");
                    sw.Write("<LineString><extrude>1</extrude><coordinates>");
                    foreach (PointF p in finalLine)
                        sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", p.X, p.Y));
                    sw.WriteLine("</coordinates></LineString></Placemark></Folder>");
                    sw.WriteLine("<Style id=\"styleLOI0\"><LineStyle><color>ffff0000</color><width>3</width></LineStyle></Style>");
                    sw.WriteLine("</Document></kml>");
                    sw.Close();
                    fs.Close();
                };
                if (sfd.FilterIndex == 2)
                    Save2Shape(sfd.FileName, finalLine);
            };
            sfd.Dispose();
        }

        private void InterLessForm_Load(object sender, EventArgs e)
        {
            MView.DrawMap = true;    
        }

        private void âûáðàòüÏàïêóÊýøàSASPlanetToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void toolStripDropDownButton1_DropDownOpening(object sender, EventArgs e)
        {
            spcl.Enabled = mru1.Count > 0;
        }

        private void InterLessForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            state = new State();
            state.MapID = iStorages.SelectedIndex;
            state.SASDir = SASPlanetCacheDir;
            state.URL = UserDefindedUrl;
            state.X = MView.CenterDegreesX;
            state.Y = MView.CenterDegreesY;
            state.Z = MView.ZoomID;
            string fn = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.stt";
            State.Save(fn, state);
        }
    }

    [Serializable]
    public class MapStore
    {
        public string Name;
        public string Url;
        public string CacheDir;
        public NaviMapNet.NaviMapNetViewer.MapServices Service = NaviMapNet.NaviMapNetViewer.MapServices.Custom_UserDefined;
        public NaviMapNet.NaviMapNetViewer.ImageSourceTypes Source = NaviMapNet.NaviMapNetViewer.ImageSourceTypes.tiles;
        public NaviMapNet.NaviMapNetViewer.ImageSourceProjections Projection = NaviMapNet.NaviMapNetViewer.ImageSourceProjections.EPSG3857;

        public override string ToString()
        {
            return Name;
        }

        public MapStore() { }
        public MapStore(string Name) { this.Name = Name; }
        public MapStore(string Name, string Url, string Cache)
        {
            this.Name = Name;
            this.Url = Url;
            this.CacheDir = Cache;
        }
    }
}