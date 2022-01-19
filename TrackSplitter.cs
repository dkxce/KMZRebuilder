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
using System.Xml.Serialization;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace KMZRebuilder
{
    public partial class TrackSplitter : Form
    {
        private WaitingBoxForm wbf = null;
        public NaviMapNet.MapLayer mapContent = null;
        public NaviMapNet.MapLayer mapSplitted = null;
        public NaviMapNet.MapLayer mapPlanned = null;
        public NaviMapNet.MapLayer mapSelect = null;
        public int selectedIndex = -1;
        public ToolTip mapTootTip = new ToolTip();
        private KMZRebuilederForm parent = null;

        private string SASPlanetCacheDir = @"C:\Program Files\SASPlanet\cache\osmmapMapnik\";
        private string UserDefindedUrl = @"http://tile.openstreetmap.org/{z}/{x}/{y}.png";
        private string UserDefindedFile = @"C:\nofile.mbtiles";

        private List<PointF> originalLine = new List<PointF>();
        private List<PointF[]> customLines = new List<PointF[]>();
        private List<SBS> segmentsList = new List<SBS>();
        public AvtodorTRWeb.PayWays Payways = null;
        private bool segCalculated = false;
        private double segmOriginalLineDist = 0;

        public TrackSplitter(KMZRebuilederForm parent)
        {
            this.parent = parent;
            Init();
        }

        public TrackSplitter(KMZRebuilederForm parent, WaitingBoxForm waitBox)
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

            mapContent = new NaviMapNet.MapLayer("mapContent");
            MView.MapLayers.Add(mapContent);
            mapSplitted = new NaviMapNet.MapLayer("mapSplitted");
            MView.MapLayers.Add(mapSplitted);
            mapPlanned = new NaviMapNet.MapLayer("mapPlanned");
            MView.MapLayers.Add(mapPlanned);
            mapSelect = new NaviMapNet.MapLayer("mapSelect");
            MView.MapLayers.Add(mapSelect);

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

            MView.SelectingMapArea += new EventHandler(OnSelectionBox);  
        }

        public void loadroute(PointF[] route)
        {
            if (route == null) return;
            if (route.Length < 3) return;

            originalLine.Clear();
            originalLine.AddRange(route);
            LoadPointsAndResetDefaults();

            DrawPolys();
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

        private void ËÁÏÂÌËÚ¸ToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void ËÁÏÂÌËÚ¸UserDefinedUrlToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void DrawPolys()
        {
            DrawOriginalPoly(customLines.Count > 0);
            DrawCustomPoly();
            MView.DrawOnMapData();
        }

        private void LoadPointsAndResetDefaults()
        {
            mapSelect.Clear();
            mapContent.Clear();
            mapSplitted.Clear();
            mapPlanned.Clear();
            customLines.Clear();
            svbtn.Enabled = false;
            PreloadNewTrack();

            plist.Items.Clear();
            if (originalLine.Count > 0)
            {
                MView.CenterDegrees = originalLine[0];

                double dist = 0;
                for (int i = 0; i < originalLine.Count; i++)
                {
                    if (i > 0)
                        dist += KMZRebuilder.Utils.GetLengthMeters(originalLine[i - 1].Y, originalLine[i - 1].X, originalLine[i].Y, originalLine[i].X, false);
                    ListViewItem lvi = plist.Items.Add(i.ToString());
                    lvi.SubItems.Add("--");
                    lvi.SubItems.Add(dist < 1000 ? dist.ToString("0") + " m" : (dist / 1000.0).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km");
                };
            };
        }

        private void DrawOriginalPoly(bool grayed)
        {
            mapContent.Clear();            
            if (originalLine.Count > 0)
            {
                NaviMapNet.MapPolyLine mp = new NaviMapNet.MapPolyLine(originalLine.ToArray());
                mp.Color = grayed ? Color.Gray : Color.Navy;
                mp.Width = 3;                
                mapContent.Add(mp);                
            };            
        }

        private void CalculateCustomPolys()
        {
            customLines.Clear();            
            if (originalLine.Count > 0)
            {
                List<PointF> curr = new List<PointF>();
                for (int i = 0; i < plist.Items.Count; i++)
                {
                    string it = plist.Items[i].SubItems[1].Text;
                    if ((it == "F") || (it == "--") || (it == "S")) curr.Add(originalLine[i]);
                    if ((it == "D") || (it == "S") || (it == "R"))
                    {
                        if (curr.Count > 1) customLines.Add(curr.ToArray());
                        curr.Clear();
                        if ((it == "S") || (it == "R")) curr.Add(originalLine[i]);
                    };
                };
                if (curr.Count > 1) customLines.Add(curr.ToArray());
                if ((customLines.Count == 1) && (customLines[0].Length == originalLine.Count)) customLines.Clear();
            };
            svbtn.Enabled = customLines.Count > 0;
            if (customLines.Count == 0)
                ttllbl.Text = String.Format("Total {0} points in {1} segment", 0, originalLine.Count, originalLine.Count > 0 ? 1 : 0);
            else
            {
                int ttl = 0;
                for (int i = 0; i < customLines.Count; i++) ttl += customLines[i].Length;
                ttllbl.Text = String.Format("Total {0} points in {1} segment(s)", ttl, customLines.Count);
            };
            DrawPolys();
        }

        private Color[] CArr = new Color[] { Color.DarkViolet, Color.DarkGreen, Color.DarkOrange, Color.DeepPink };
        private void DrawCustomPoly()
        {
            mapSplitted.Clear();

            if ((customLines.Count > 0))
            {
                for (int p = 0; p < customLines.Count; p++)
                {
                    if (customLines[p] == null) continue;
                    if (customLines[p].Length < 0) continue;

                    NaviMapNet.MapPolyLine mp = new NaviMapNet.MapPolyLine(customLines[p]);
                    mp.Color = CArr[p % 4];
                    mp.Width = 5;
                    mapSplitted.Add(mp);
                };
            };                                   
        }

        private void MView_MouseMove(object sender, MouseEventArgs e)
        {
            mapobjs_locate = false;
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
                loadroute(ofd.FileName);
            ofd.Dispose();
        }

        private void loadroute(string filename)
        {
            System.IO.FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            System.IO.StreamReader sr = new StreamReader(fs);
            originalLine.Clear();
            customLines.Clear();

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

            LoadPointsAndResetDefaults();
            DrawPolys();
        }

        public static byte[] Convert(byte[] ba, bool bigEndian)
        {
            if (BitConverter.IsLittleEndian != bigEndian) Array.Reverse(ba);
            return ba;
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }        

        private void iterations_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void SelectOnTrack(int id, int[] ids, bool byMap)
        {            
            if(originalLine.Count == 0) return;
            if ((this.selectedIndex == id) && ((ids == null) || (ids.Length == 1))) return;
            this.selectedIndex = id;
            if (this.selectedIndex < 0) this.selectedIndex = -1;
            if (this.selectedIndex >= originalLine.Count) this.selectedIndex = originalLine.Count - 1;

            mapSelect.Clear();
            if (this.selectedIndex >= 0)
            {
                if ((ids != null) && (ids.Length > 1))
                {
                    for (int i = 1; i < ids.Length; i++)
                        if (ids[i] == (ids[i - 1] + 1))
                        {
                            NaviMapNet.MapLine ml = new NaviMapNet.MapLine(originalLine[ids[i - 1]], originalLine[ids[i]]);
                            ml.Color = Color.Yellow;
                            ml.Width = 3;
                            mapSelect.Add(ml);
                        };
                };

                NaviMapNet.MapPoint ms = new NaviMapNet.MapPoint(originalLine[this.selectedIndex]);
                ms.Name = "Selected";
                ms.SizePixels = new Size(12, 12);
                ms.Squared = false;
                ms.Color = Color.Red;
                mapSelect.Add(ms);
                if (!byMap)
                {
                    double[] mm = MView.MapBoundsMinMaxDegrees;
                    if ((ms.Points[0].X <= mm[0]) || (ms.Points[0].Y <= mm[1]) || (ms.Points[0].X >= mm[2]) || (ms.Points[0].Y >= mm[3]))
                        MView.CenterDegrees = ms.Points[0];
                };

                if ((ids != null) && (ids.Length > 1))
                {
                    for (int i = 1; i < ids.Length - 1; i++)
                    {
                        NaviMapNet.MapPoint mb = new NaviMapNet.MapPoint(originalLine[ids[i]]);
                        mb.Name = "Selected";
                        mb.SizePixels = new Size(8, 8);
                        mb.Squared = false;
                        mb.Color = Color.Maroon;
                        mapSelect.Add(mb);
                    };

                    NaviMapNet.MapPoint me = new NaviMapNet.MapPoint(originalLine[ids[ids.Length - 1]]);
                    me.Name = "Selected";
                    me.SizePixels = new Size(12, 12);
                    me.Squared = false;
                    me.Color = Color.Green;
                    mapSelect.Add(me);
                };
            };
            MView.DrawOnMapData();
            SelectOnList(ids == null ? new int[]{id} : ids);
        }

        private void SelectOnList(int[] ids)
        {
            if (ids == null) return;
            if (ids.Length == 0) return;
            List<int> idxs = new List<int>();
            idxs.AddRange(ids);
            if (plist.Items.Count == 0) return;
            for (int i = 0; i < plist.Items.Count; i++)
            {
                if(idxs.IndexOf(i) >= 0)
                    plist.Items[i].BackColor = Color.SkyBlue;
                else
                {
                    if (plist.Items[i].SubItems[1].Text == "--") plist.Items[i].BackColor = plist.BackColor;
                    if (plist.Items[i].SubItems[1].Text == "F") plist.Items[i].BackColor = Color.Yellow;
                    if (plist.Items[i].SubItems[1].Text == "H") plist.Items[i].BackColor = Color.LightGray;
                    if (plist.Items[i].SubItems[1].Text == "D") plist.Items[i].BackColor = Color.LightPink;
                    if (plist.Items[i].SubItems[1].Text == "S") plist.Items[i].BackColor = Color.LightGreen;
                    if (plist.Items[i].SubItems[1].Text == "R") plist.Items[i].BackColor = Color.Orange;
                }
            };
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            exportKMFlagsToKMLToolStripMenuItem.Enabled = 
                inverseTrackToolStripMenuItem.Enabled =
                    originalLine.Count > 0;

            flagToolStripMenuItem.Enabled = 
                hidePointToolStripMenuItem.Enabled =
                    deletePointToolStripMenuItem.Enabled =
                        splitPointToolStripMenuItem.Enabled =
                            removeStepToolStripMenuItem.Enabled =
                                plist.SelectedIndices.Count > 0;            
        }

        private void SaveResults()
        {
            if (customLines.Count == 0) return;
            
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".kml";
            sfd.Filter = "KML files (*.kml)|*.kml";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string track_name = "MyWay";
                string ctn = track_name;
                if (InputBox.Show("Track Name", "Enter track name", ref track_name) == DialogResult.OK)
                    ctn = track_name;

                FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document>");
                sw.WriteLine("<name><![CDATA[" + ctn + "]]></name><createdby>KMZ Rebuilder</createdby>");
                sw.WriteLine("<Folder><name><![CDATA[" + ctn + "]]></name>");
                for (int i = 0; i < customLines.Count; i++)
                {
                    double d = GetDistInMeters(customLines[i], false);
                    string nm = ctn + " " + (i.ToString() + "/" + customLines.Count.ToString()) + " - " + (d < 1000 ? d.ToString("0") + " m" : (d / 1000.0).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km");
                    sw.Write("<Placemark><name><![CDATA[" + nm + "]]></name>");
                    sw.Write("<styleUrl>#styleLXA" + (i % 4).ToString() + "</styleUrl><description></description>");
                    sw.Write("<LineString><extrude>1</extrude><coordinates>");
                    foreach (PointF p in customLines[i])
                        sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", p.X, p.Y));
                    sw.WriteLine("</coordinates></LineString></Placemark>");
                };
                sw.WriteLine("</Folder>");
                for (int i = 0; i < 4; i++)
                {
                    Color c = CArr[i % 4];
                    sw.WriteLine("<Style id=\"styleLXA" + (i % 4).ToString() + "\"><LineStyle><color>" + String.Format("{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.B, c.G, c.R) + "</color><width>3</width></LineStyle></Style>");
                };
                sw.WriteLine("</Document></kml>");
                sw.Close();
                fs.Close();
            };
            sfd.Dispose();
        }

        public static uint GetDistInMeters(PointF[] polyline, bool polygon)
        {
            if (polyline == null) return 0;
            if (polyline.Length < 2) return 0;
            uint res = 0;
            for (int i = 1; i < polyline.Length; i++)
                res += (uint)KMZRebuilder.Utils.GetLengthMeters(polyline[i - 1].Y, polyline[i - 1].X, polyline[i].Y, polyline[i].X, false);
            if (polygon)
                res += (uint)KMZRebuilder.Utils.GetLengthMeters(polyline[polyline.Length - 1].Y, polyline[polyline.Length - 1].X, polyline[0].Y, polyline[0].X, false);
            return res;
        }

        private void InterLessForm_Load(object sender, EventArgs e)
        {
            MView.DrawMap = true;    
        }

        private void ‚˚·‡Ú¸œ‡ÔÍÛ ˝¯‡SASPlanetToolStripMenuItem_Click(object sender, EventArgs e)
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
            state.FILE = UserDefindedFile;
            state.X = MView.CenterDegreesX;
            state.Y = MView.CenterDegreesY;
            state.Z = MView.ZoomID;
            string fn = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.stt";
            State.Save(fn, state);
        }

        private void plist_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (plist.SelectedIndices.Count == 0) return;
            if (e.KeyChar == 'h') doHide();
            if (e.KeyChar == 'd') doDelete();
            if (e.KeyChar == 's') doSplit();
            if (e.KeyChar == 'r') doRemove();
            if (e.KeyChar == 'f') doFlag();
            if (e.KeyChar == '\r') doSelect();
        }

        private void doSelect()
        {
            if (plist.Items.Count == 0) return;
            if (plist.SelectedIndices.Count == 0) return;

            int[] ss = new int[plist.SelectedIndices.Count];
            for (int i = 0; i < plist.SelectedIndices.Count; i++) ss[i] = plist.SelectedIndices[i];
            SelectOnTrack(plist.SelectedIndices[0], ss, false);
        }

        private void doFlag()
        {
            int fc = 0;
            if (plist.SelectedIndices.Count == 0) return;
            for (int i = 0; i < plist.SelectedIndices.Count; i++)
            {
                int si = plist.SelectedIndices[i];
                if (plist.Items[si].SubItems[1].Text != "F")
                {
                    plist.Items[si].SubItems[1].Text = "F";
                    fc++;
                    plist.Items[si].BackColor = Color.Yellow;
                }
                else
                {
                    plist.Items[si].SubItems[1].Text = "--";
                    plist.Items[si].BackColor = plist.BackColor;
                };
            };
            Flags.Text = "Flags: " + fc.ToString();
        }


        private void doHide()
        {
            if (plist.SelectedIndices.Count == 0) return;
            for (int i = 0; i < plist.SelectedIndices.Count; i++)
            {
                int si = plist.SelectedIndices[i];
                if (plist.Items[si].SubItems[1].Text != "H")
                {
                    plist.Items[si].SubItems[1].Text = "H";
                    plist.Items[si].BackColor = Color.LightGray;
                }
                else
                {
                    plist.Items[si].SubItems[1].Text = "--";
                    plist.Items[si].BackColor = plist.BackColor;
                };
            };
            CalculateCustomPolys();
        }

        private void doDelete()
        {
            if (plist.SelectedIndices.Count == 0) return;
            for (int i = 0; i < plist.SelectedIndices.Count; i++)
            {
                int si = plist.SelectedIndices[i];
                if (plist.Items[si].SubItems[1].Text != "D")
                {
                    plist.Items[si].SubItems[1].Text = "D";
                    plist.Items[si].BackColor = Color.LightPink;
                }
                else
                {
                    plist.Items[si].SubItems[1].Text = "--";
                    plist.Items[si].BackColor = plist.BackColor;
                };
            };
            CalculateCustomPolys();
        }

        private void doSplit()
        {
            if (plist.SelectedIndices.Count == 0) return;
            for (int i = 0; i < plist.SelectedIndices.Count; i++)
            {
                int si = plist.SelectedIndices[i];
                if (plist.Items[si].SubItems[1].Text != "S")
                {
                    plist.Items[si].SubItems[1].Text = "S";
                    plist.Items[si].BackColor = Color.LightGreen;
                }
                else
                {
                    plist.Items[si].SubItems[1].Text = "--";
                    plist.Items[si].BackColor = plist.BackColor;
                };
            };
            CalculateCustomPolys();
        }

        private void doRemove()
        {
            if (plist.SelectedIndices.Count == 0) return;
            for (int i = 0; i < plist.SelectedIndices.Count; i++)
            {
                int si = plist.SelectedIndices[i];
                if (plist.Items[si].SubItems[1].Text != "R")
                {
                    plist.Items[si].SubItems[1].Text = "R";
                    plist.Items[si].BackColor = Color.Orange;
                }
                else
                {
                    plist.Items[si].SubItems[1].Text = "--";
                    plist.Items[si].BackColor = plist.BackColor;
                };
            };
            CalculateCustomPolys();
        }

        private void hidePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doHide();            
        }

        private void deletePointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doDelete();
        }

        private void splitPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doSplit();
        }

        private void removeStepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doRemove();
        }

        private void MView_MouseClick(object sender, MouseEventArgs e)
        {
            if (!mapobjs_locate) return;
            if (mapContent.ObjectsCount == 0) return;
            if (originalLine.Count == 0) return;

            Point clicked = MView.MousePositionPixels;
            PointF sCenter = MView.PixelsToDegrees(clicked);
            PointF sFrom = MView.PixelsToDegrees(new Point(clicked.X - 5, clicked.Y + 5));
            PointF sTo = MView.PixelsToDegrees(new Point(clicked.X + 5, clicked.Y - 5));

            int pnt_i = -1;
            int lne_i = -1;
            double dpnt = double.MaxValue;
            double dlne = double.MaxValue;
            for (int i = 0; i < originalLine.Count; i++)
            {
                if (i > 1)
                {
                    PointF pol;
                    int side;
                    float dtl = PolyLineBuffer.PolyLineBufferCreator.DistanceFromPointToLine(sCenter, originalLine[i - 1], originalLine[i], PolyLineBuffer.PolyLineBufferCreator.GeographicDistFunc, out pol, out side);
                    float pxd = PixelsDistance(MView.DegreesToPixels(pol), clicked);
                    if((pxd < 10) && (dtl < dlne))
                    {
                        dlne = dtl;
                        lne_i = i;
                    };
                };
                PointF p = originalLine[i];
                if (p.X < sFrom.X) continue;
                if (p.Y < sFrom.Y) continue;
                if (p.X > sTo.X) continue;
                if (p.Y > sTo.Y) continue;
                float x = PolyLineBuffer.PolyLineBufferCreator.SampleDistFunc(sCenter, p);
                if (x < dpnt)
                {
                    dpnt = x;
                    pnt_i = i;
                };
            };
            if (pnt_i >= 0)
            {
                plist.SelectedIndices.Clear();
                plist.Select();
                plist.EnsureVisible(pnt_i);
                plist.Items[pnt_i].Selected = true;
                SelectOnTrack(pnt_i, null, true);
            }
            else if (lne_i >= 0)
            {
                plist.SelectedIndices.Clear();
                plist.Select();
                plist.EnsureVisible(lne_i);
                plist.Items[lne_i - 1].Selected = true;
                plist.Items[lne_i].Selected = true;
                SelectOnTrack(lne_i - 1, new int[] { lne_i - 1, lne_i }, true);
            };
            //else
              //  SelectOnTrack(-1, null, true);
        }

        public static float PixelsDistance(Point a, Point b)
        {
            return (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        private void svbtn_Click(object sender, EventArgs e)
        {
            if (customLines.Count == 0) return;
            SaveResults();
        }

        private void OnSelectionBox(object sender, EventArgs e)
        {
            if (originalLine.Count == 0) return;
            double[] mmd = MView.SelectionBoundsMinMaxDegrees;
            List<int> inrect = new List<int>();
            for (int i = 0; i < originalLine.Count; i++)
            {
                PointF p = originalLine[i];
                if (p.X < mmd[0]) continue;
                if (p.Y < mmd[1]) continue;
                if (p.X > mmd[2]) continue;
                if (p.Y > mmd[3]) continue;
                inrect.Add(i);
            };
            if (inrect.Count > 0)
            {
                plist.SelectedIndices.Clear();
                plist.EnsureVisible(inrect[0]);
                plist.Select();
                for (int i = 0; i < inrect.Count; i++)
                    plist.Items[inrect[i]].Selected = true;
                SelectOnTrack(inrect[0], inrect.ToArray(), true);                
            };
        }

        private void MView_MouseUp(object sender, MouseEventArgs e)
        {
        }

        private void MView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SelectOnTrack(-1, null, true);
        }

        private void selectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectOnTrack(-1, null, true);
        }

        private bool mapobjs_locate = true;
        private void MView_MouseDown(object sender, MouseEventArgs e)
        {
            mapobjs_locate = true;
        }

        private void flagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doFlag();
        }

        private void setFlagsByObjectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportPlannerSegments(false);
        }

        public struct SBS
        {
            public string name; // name of this point
            public PointF point; // xy of this point
            public PointF pointOnLine; // point on nearest line
            public string tip; // X - cross // W - first or last point of way (S - Start; E - end)
            public int index; // nearest point index in line
            public double toline; // distance to nearest line (for Flag)
            public double dist; // distance from start to point on nearest line
            public ulong unical; // using for identify line start and end
            public string tIndex; // Table Index (from route_planner_index = ... )
            public int delay; // Delay in minutes
            public int speed;

            public SBS(string name, PointF point, string tip, ulong unical)
            {
                this.name = name; // name of this point
                this.point = point; // xy of this point
                this.tip = tip; // X - cross // W - first or last point of way
                this.index = -1; // nearest point index in line
                this.toline = double.MaxValue; // distance to nearest line
                this.dist = double.MaxValue; // distance from start to point on nearest line
                this.pointOnLine = new PointF(); // point on nearest line
                this.unical = unical;
                this.tIndex = "";
                this.delay = 0;
                this.speed = 60;
            }

            public override string ToString()
            {
                return this.name + " [" + this.tip.ToString() + "]";
            }
        }

        public class SBSSorter : IComparer<SBS>
        {
            public int Compare(SBS a, SBS b)
            {
                return a.dist.CompareTo(b.dist);
            }

        }

        public struct RoutePlannerProject
        {
            public string header;
            public bool calculated;
            public PointF[] route;
            public SBS[] segments;
            public double length;
            public AvtodorTRWeb.PayWays payways;
        }

        private void PreloadNewTrack()
        {
            segCalculated = false;
            if (segmentsList.Count == 0) return;
            for (int i = 0; i < segmentsList.Count; i++)
            {
                SBS sbs = segmentsList[i];
                sbs.index = -1;
                segmentsList[i] = sbs;
            };
            Flags.Text = "No Flags";
        }

        private void LoadSegments2View()
        {
            segView.Items.Clear();
            if (segmentsList.Count == 0) return;
            for (int i = 0; i < segmentsList.Count; i++)
            {
                SBS sbs = segmentsList[i];
                CustomListViewItem lvi = new CustomListViewItem(new string[] { 
                    (i+1).ToString(), 
                    sbs.tip, 
                    sbs.dist == double.MaxValue ? "" : ((int)(sbs.dist / 1000)).ToString() + " km", 
                    sbs.name,
                    sbs.toline == double.MaxValue ? "" : ((int)(sbs.toline / 1000)).ToString() + " km",
                    sbs.delay.ToString(),
                    sbs.speed.ToString()
                });
                if (segmentsList[i].tip != "X")
                    lvi.BackColor = Color.LightSkyBlue;
                segView.Items.Add(lvi);
            };
        }

        private void UpdateSegmentOnView(SBS sbs, ListViewItem lview)
        {
            lview.SubItems[5].Text = sbs.delay.ToString();
            lview.SubItems[6].Text = sbs.speed.ToString();
        }

        private void ReloadSegmentIndexes()
        {
            if (segmentsList.Count == 0) return;
            for (int i = 0; i < segmentsList.Count; i++)
                segView.Items[i].SubItems[0].Text = (i + 1).ToString();
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            clearPaywaysToolStripMenuItem.Enabled = Payways != null;
            deletePointsToolStripMenuItem.Enabled = segView.SelectedIndices.Count > 0;
            splitTrackBySegmentsToolStripMenuItem.Enabled =
                (segmentsList.Count > 0) && (originalLine.Count > 0);
            swap2PointToolStripMenuItem.Enabled =
                segView.SelectedIndices.Count == 2;
            saveWaypointAndSegmentsToFileToolStripMenuItem.Enabled =
                segCalculated && (segmentsList.Count > 0);
            saveProjectToolStripMenuItem.Enabled =
                (segmentsList.Count > 0) && (originalLine.Count > 0);
            clearSegmentsListToolStripMenuItem.Enabled =
                segmentsList.Count > 0;
            sortASCToolStripMenuItem.Enabled = 
                exportProjectToKMLToolStripMenuItem.Enabled =
                    segCalculated && (segmentsList.Count > 0);

            delayToolStripMenuItem.Enabled = speedToolStripMenuItem.Enabled = segView.SelectedIndices.Count == 1;
            map1.Enabled = map2.Enabled = map3.Enabled = map4.Enabled = map5.Enabled = segView.Items.Count > 0;
        }

        private void deletePointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (segView.SelectedIndices.Count == 0) return;
            List<ulong> unicals = new List<ulong>();
            for (int i = segView.SelectedIndices.Count - 1; i >= 0; i--)
                unicals.Add(segmentsList[segView.SelectedIndices[i]].unical);
            for(int i = segmentsList.Count-1;i>=0;i--)
                if (unicals.Contains(segmentsList[i].unical))
                {
                    segView.Items.RemoveAt(i);
                    segmentsList.RemoveAt(i);
                    if (mapSelect.ObjectsCount > 0)
                    {
                        mapSelect.Clear();
                        MView.DrawOnMapData();
                    };
                };
            ReloadSegmentIndexes();
        }

        private void splitTrackBySegmentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlanBySegments();
        }

        private void PlanBySegments()
        {
            if (segmentsList.Count == 0) return;
            if (originalLine.Count == 0) return;

            if (wbf != null)
                wbf.Show("Track Planner", "Wait, calculating waypoints ...");
            // FIND NEAREST/SEGMENTS/WAYPOINTS            
            List<SBS> pts2spl = segmentsList;
            // Reset Defaults
            for (int si = 0; si < pts2spl.Count; si++)
            {
                SBS sbs = pts2spl[si];
                sbs.index = -1;
                sbs.dist = double.MaxValue;
                sbs.toline = double.MaxValue;
                sbs.pointOnLine = new PointF();
                pts2spl[si] = sbs;
            };
            segmOriginalLineDist = 0;
            for (int li = 1; li < originalLine.Count; li++)
            {
                for (int si = 0; si < pts2spl.Count; si++)
                {
                    PointF pol; int side;
                    float cdi = PolyLineBuffer.PolyLineBufferCreator.DistanceFromPointToLine(pts2spl[si].point, originalLine[li - 1], originalLine[li], PolyLineBuffer.PolyLineBufferCreator.GeographicDistFunc, out pol, out side);
                    if (cdi < pts2spl[si].toline)
                    {
                        SBS sbs = pts2spl[si];
                        sbs.toline = cdi;
                        sbs.index = li - 1;
                        sbs.dist = segmOriginalLineDist + KMZRebuilder.Utils.GetLengthMeters(originalLine[li - 1].Y, originalLine[li - 1].X, pol.Y, pol.X, false);
                        sbs.pointOnLine = pol;
                        pts2spl[si] = sbs;
                    };
                };
                segmOriginalLineDist += KMZRebuilder.Utils.GetLengthMeters(originalLine[li - 1].Y, originalLine[li - 1].X, originalLine[li].Y, originalLine[li].X, false);
            };

            // FLAG LIST
            for (int si = 0; si < pts2spl.Count; si++)
            {
                plist.Items[pts2spl[si].index].SubItems[1].Text = "F";
                plist.Items[pts2spl[si].index].BackColor = Color.Yellow;
            };
            int fc = 0;
            for (int i = 0; i < plist.Items.Count; i++)
                if (plist.Items[i].SubItems[1].Text == "F")
                    fc++;
            Flags.Text = "Flags: " + fc.ToString();            

            // SET ENTRY/START LEAVE/END to WAYS/LINES
            List<ulong> unicals = new List<ulong>();
            for (int i = 0; i < pts2spl.Count; i++)
                if (pts2spl[i].tip != "X")
                    if (!unicals.Contains(pts2spl[i].unical))
                        unicals.Add(pts2spl[i].unical);
            for (int ui = 0; ui < unicals.Count; ui++)
            {
                int ia = -1;
                int ib = -1;
                for (int i = 0; i < pts2spl.Count; i++)
                    if (pts2spl[i].unical == unicals[ui])
                        if (ia < 0) ia = i; else ib = i;
                SBS a = pts2spl[ia];
                SBS b = pts2spl[ib];
                if (b.dist >= a.dist)
                {
                    a.tip = "S";
                    b.tip = "E";
                }
                else
                {
                    a.tip = "E";
                    b.tip = "S";
                };
                pts2spl[ia] = a;
                pts2spl[ib] = b;
            };

            // SORT BY LENGTH
            pts2spl.Sort(new SBSSorter());

            if (wbf != null)
                wbf.Hide();

            if (MessageBox.Show("Get Segment Speed by Requesting Route?", "Calculate Track Waypoints by Segments", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (wbf != null) wbf.Show("Track Planner", String.Format("Requesting route {0}/{1} ...", 1, pts2spl.Count + 1));
                KMZRebuilder.GetRouter groute = GetRouter.Load();
                nmsRouteClient.Route rt;
                double speed = 60;
                try
                {
                    rt = groute.GetRoute(originalLine[0], pts2spl[0].pointOnLine);
                    if (rt.driveTime != 0) speed = (rt.driveLength / 1000.0) / (rt.driveTime / 60.0);
                    SBS sbs = pts2spl[0];
                    sbs.speed = (int)speed;
                    pts2spl[0] = sbs;
                }
                catch { };
                for (int i = 1; i < pts2spl.Count; i++)
                {
                    if (wbf != null) wbf.Show("Track Planner", String.Format("Requesting route {0}/{1} ...", i + 1, pts2spl.Count + 1));
                    speed = 60;
                    try
                    {
                        rt = groute.GetRoute(pts2spl[i - 1].pointOnLine, pts2spl[i].pointOnLine);
                        if (rt.driveTime != 0) speed = (rt.driveLength / 1000.0) / (rt.driveTime / 60.0);
                        SBS sbs = pts2spl[i];
                        sbs.speed = (int)speed;
                        pts2spl[i] = sbs;
                    }
                    catch { };
                };
                speed = 60;
                try
                {
                    if (wbf != null) wbf.Show("Track Planner", String.Format("Requesting route {0}/{1} ...", pts2spl.Count + 1, pts2spl.Count + 1));
                    rt = groute.GetRoute(pts2spl[pts2spl.Count - 1].pointOnLine, originalLine[originalLine.Count - 1]);
                    if (rt.driveTime != 0) speed = (rt.driveLength / 1000.0) / (rt.driveTime / 60.0);
                    SBS sbs = pts2spl[pts2spl.Count - 1];
                    sbs.speed = (int)speed;
                    pts2spl[pts2spl.Count - 1] = sbs;
                }
                catch { };
            };

            if (wbf != null)
                wbf.Show("Track Planner", "Wait, calculating waypoints ...");
            LoadSegments2View();
            segCalculated = true;
            if (wbf != null)
                wbf.Hide();
                                  
        }

        private void swap2PointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (segView.SelectedIndices.Count != 2) return;
            int a = segView.SelectedIndices[0];
            int b = segView.SelectedIndices[1];
            SBS sa = segmentsList[a];
            SBS sb = segmentsList[b];
            segmentsList[a] = sb;
            segmentsList[b] = sa;
            sa = segmentsList[a];
            sb = segmentsList[b];
            CustomListViewItem la = new CustomListViewItem(new string[] { (a + 1).ToString(), sa.tip, 
                sa.dist == double.MaxValue ? "" : ((int)(sa.dist / 1000)).ToString() + " km", 
                sa.name,
                sa.toline == double.MaxValue ? "" : ((int)(sa.toline / 1000)).ToString() + " km"});
            la.Selected = true;
            CustomListViewItem lb = new CustomListViewItem(new string[] { (b + 1).ToString(), sb.tip, 
                sb.dist == double.MaxValue ? "" : ((int)(sb.dist / 1000)).ToString() + " km", 
                sb.name,
                sb.toline == double.MaxValue ? "" : ((int)(sb.toline / 1000)).ToString() + " km"});
            lb.Selected = true;
            segView.Items[a] = la;
            segView.Items[b] = lb;
        }

        private void saveWaypointAndSegmentsToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveRoutePlan();
        }

        private void SaveRoutePlan()
        {
            if (segmentsList.Count == 0) return;
            if (!segCalculated) return;

            string fileName = null;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save to Waypoints and Segments Report File for Excel";
            sfd.FilterIndex = 2;
            sfd.DefaultExt = ".xml";
            sfd.Filter = "Text Files (*.txt)|*.txt|XML Files (*.xml)|*.xml";
            sfd.FileName = "Track_Segments_and_Waypoints_4_Excel.xml";
            if (sfd.ShowDialog() == DialogResult.OK)
                fileName = sfd.FileName;
            sfd.Dispose();
            if (fileName == null) return;
            if (Path.GetExtension(fileName).ToLower() == ".txt")
                SaveRoutePlanTxt(fileName);
            if (Path.GetExtension(fileName).ToLower() == ".xml")
                SaveRoutePlanXml(fileName);
        }

        private void SaveRoutePlanTxt(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.WriteLine("##;ODOMETER;KM;SEGMENT LENGHT;EVENT;NAME;ENTRY TIME;DELAY TIME;MOVE TIME;SEGMENT TIME;LEAVE TIME;AVG SPEED;DIST TO ROUTE;REGION;DN;DN;DOF;DOF;");
            string regNm = "";
            int idd = 0, row = 1;
            // START POINT
            {
                ++idd; ++row;
                //A - ##
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", idd));
                //B - ODOMETER
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", segmentsList[segmentsList.Count - 1].dist > segmentsList[0].dist ? 0 : (int)segmOriginalLineDist));
                //C - KM
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", segmentsList[segmentsList.Count - 1].dist > segmentsList[0].dist ? 0 : (int)(segmOriginalLineDist / 1000)));
                //D - LENGTH
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=C{1}-C{0};", row, row + 1));
                //R - EVENT
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", "START"));
                //F - NAME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", "START"));
                //G - ENTRY TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", DateTime.Now.ToString("dd.MM.yy HH:mm")));
                //H - DELAY TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", "00:00"));
                //I - MOVE TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=D{0}/L{0}/24;", row));
                //J - SEGMENT TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=H{0}+I{0};", row));
                //K - LEAVE TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=G{0}+J{0};", row));
                //L - AVG SPEED
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", "60"));
                //M - DIST 2 ROUTE
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", "0"));
                //N - REGION 
                regNm = "";
                if (KMZRebuilederForm.PIRU != null)
                {
                    int regNo = KMZRebuilederForm.PIRU.PointInRegion(originalLine[0].Y, originalLine[0].X);
                    regNm = regNo > 0 ? KMZRebuilederForm.PIRU.GetRegionName(regNo) : "...";
                };
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", regNm));
                // O,P - Night/Day
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=≈—À»(◊¿—(G{0})<7.,'Õ'.,'ƒ');", row));
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=IF(HOUR(G{0})<7.,'N'.,'D');", row));
                // Q,R - DayOfWeek
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=ƒ≈Õ‹Õ≈ƒ(G{0});", row));
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=WEEKDAY(G{0});", row));
                //END
                sw.WriteLine();
            };
            // WAYPOINTS/SEGMENTS
            List<SBS> pts2spl = segmentsList;
            for (int si = 0; si < pts2spl.Count; si++)
            {
                ++idd; ++row;
                //A - ##
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", idd));
                //B - ODOMETER
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", (int)pts2spl[si].dist));
                //C - KM
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", (int)(pts2spl[si].dist / 1000)));
                //D - LENGTH
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=C{1}-C{0};", row, row + 1));
                //R - EVENT
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", pts2spl[si].tip));
                //F - NAME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", pts2spl[si].name));
                //G - ENTRY TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=K{0};", row - 1));
                //H - DELAY TIME
                TimeSpan ts = new TimeSpan(0, pts2spl[si].delay, 0);
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:00}:{1:00};", ts.Hours, ts.Minutes));
                //I - MOVE TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=D{0}/L{0}/24;", row));
                //J - SEGMENT TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=H{0}+I{0};", row));
                //K - LEAVE TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=G{0}+J{0};", row));
                //L - AVG SPEED
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", pts2spl[si].speed.ToString()));
                //M - DIST 2 ROUTE
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", (int)(pts2spl[si].toline / 1000)));
                //N - REGION 
                regNm = "";
                if (KMZRebuilederForm.PIRU != null)
                {
                    int regNo = KMZRebuilederForm.PIRU.PointInRegion(pts2spl[si].pointOnLine.Y, pts2spl[si].pointOnLine.X);
                    regNm = regNo > 0 ? KMZRebuilederForm.PIRU.GetRegionName(regNo) : "...";
                };
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", regNm));
                // O,P - Night/Day
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=≈—À»(◊¿—(G{0})<7.,'Õ'.,'ƒ');", row));
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=IF(HOUR(G{0})<7.,'N'.,'D');", row));
                // Q,R - DayOfWeek
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=ƒ≈Õ‹Õ≈ƒ(G{0});", row));
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=WEEKDAY(G{0});", row));
                //END LINE
                sw.WriteLine();
            };
            //FINISH/END POINT
            {
                ++idd; ++row;
                //A - ##
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", idd));
                //B - ODOMETER
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", segmentsList[segmentsList.Count - 1].dist > segmentsList[0].dist ? (int)segmOriginalLineDist : 0));
                //C - KM
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", segmentsList[segmentsList.Count - 1].dist > segmentsList[0].dist ? (int)(segmOriginalLineDist / 1000) : 0));
                //D - LENGTH
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", ""));
                //R - EVENT
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", "FINISH"));
                //F - NAME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", "FINISH"));
                //G - ENTRY TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=K{0};", row - 1));
                //H - DELAY TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", ""));
                //I - MOVE TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", ""));
                //J - SEGMENT TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", ""));
                //K - LEAVE TIME
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", ""));
                //L - AVG SPEED
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", ""));
                //M - DIST 2 ROUTE
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", ""));
                //N - REGION 
                regNm = "";
                if (KMZRebuilederForm.PIRU != null)
                {
                    int regNo = KMZRebuilederForm.PIRU.PointInRegion(originalLine[originalLine.Count - 1].Y, originalLine[originalLine.Count - 1].X);
                    regNm = regNo > 0 ? KMZRebuilederForm.PIRU.GetRegionName(regNo) : "...";
                };
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0};", regNm));
                // O,P - Night/Day
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=≈—À»(◊¿—(G{0})<7.,'Õ'.,'ƒ');", row));
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=IF(HOUR(G{0})<7.,'N'.,'D');", row));
                // Q,R - DayOfWeek
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=ƒ≈Õ‹Õ≈ƒ(G{0});", row));
                sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "=WEEKDAY(G{0});", row));
                //END
                sw.WriteLine();
            };
            sw.WriteLine();
            sw.WriteLine("Created by KMZ Rebuilder Route Planner");
            sw.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            sw.Close();
            fs.Close();
        }

        private void SaveRoutePlanXml(string fileName)
        {
            // https://novainfo.ru/article/158
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\"?>");
            sw.WriteLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sw.WriteLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            sw.WriteLine("<DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">\r\n  <Created>KMZRebuilder</Created>\r\n</DocumentProperties>");
            sw.WriteLine("<Styles>");
            sw.WriteLine("  <Style ss:ID=\"0\"><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"0t\"><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"0b\"><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"1\"><Alignment ss:Horizontal=\"Center\"/><Font ss:Bold=\"1\"/><Borders><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"2\"><Alignment ss:Horizontal=\"Center\"/><Font ss:Bold=\"1\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sKm\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"0&quot; km&quot;\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sDt\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"dd/mm\\ hh:mm\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sTm\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"hh:mm\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sSp\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"0&quot; km/h&quot;\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");            
            sw.WriteLine("  <Style ss:ID=\"dow\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"ddd\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sKmt\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"0&quot; km&quot;\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sDtt\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"dd/mm\\ hh:mm\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sTmt\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"hh:mm\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sSpt\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"0&quot; km/h&quot;\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"dowt\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"ddd\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sKmb\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"0&quot; km&quot;\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sDtb\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"dd/mm\\ hh:mm\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sTmb\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"hh:mm\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"sSpb\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"0&quot; km/h&quot;\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"dowb\"><Alignment ss:Horizontal=\"Center\"/><NumberFormat ss:Format=\"ddd\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Dot\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"dtf\"><NumberFormat ss:Format=\"dd/mm/yyyy\\ hh:mm\"/></Style>");
            if (Payways == null)
                sw.WriteLine("</Styles>");                
            else
                AvtodorTRWeb.PayWays.Export2ExcelXML(Payways, sw);
            sw.WriteLine("<Worksheet ss:Name=\"RoutePlan\">\r\n<Table>");
            sw.WriteLine("  <Column ss:Width=\"40\"/>");
            sw.WriteLine("  <Column ss:Width=\"40\"/>");
            sw.WriteLine("  <Column ss:Width=\"40\"/>");
            sw.WriteLine("  <Column ss:Width=\"40\"/>");
            sw.WriteLine("  <Column ss:Width=\"20\"/>");
            sw.WriteLine("  <Column ss:Width=\"40\"/>");
            sw.WriteLine("  <Column ss:Width=\"140\"/>");
            sw.WriteLine("  <Column ss:Width=\"65\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"65\"/>");
            sw.WriteLine("  <Column ss:Width=\"30\"/>");
            sw.WriteLine("  <Column ss:Width=\"40\"/>");
            sw.WriteLine("  <Column ss:Width=\"130\"/>");
            sw.WriteLine("  <Column ss:Width=\"30\"/>");
            sw.WriteLine("  <Column ss:Width=\"30\"/>");
            if (Payways != null)
            {
                sw.WriteLine("  <Column ss:Width=\"30\"/>");
                sw.WriteLine("  <Column ss:Width=\"30\"/>");
                sw.WriteLine("  <Column ss:Width=\"30\"/>");
                sw.WriteLine("  <Column ss:Width=\"30\"/>");
            };
            sw.WriteLine("  <Row>");
            sw.WriteLine("    <Cell ss:StyleID=\"1\"><Data ss:Type=\"String\">PNT</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"1\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\">DISTANCE</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"1\" ss:MergeAcross=\"3\"><Data ss:Type=\"String\">SEGMENT</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"1\" ss:MergeAcross=\"4\"><Data ss:Type=\"String\">TIME</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"1\"><Data ss:Type=\"String\">AVG</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"1\"><Data ss:Type=\"String\">DIST TO</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"1\"><Data ss:Type=\"String\">REGION</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"1\"><Data ss:Type=\"String\">DAY</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"1\"><Data ss:Type=\"String\">DAYOF</Data></Cell>");
            if (Payways != null)
            {
                sw.WriteLine("    <Cell ss:StyleID=\"1\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\">COST</Data></Cell>");
                sw.WriteLine("    <Cell><Data ss:Type=\"String\">SEGMENT</Data></Cell>");
                sw.WriteLine("    <Cell><Data ss:Type=\"String\">DOW</Data></Cell>");
            };
            sw.WriteLine("  </Row>");
            sw.WriteLine("  <Row>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">##</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">M</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">KM</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">LENGTH</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">ID</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">EVENT</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">NAME</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">ENTRY</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">DELAY</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">MOVE</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">SEGMENT</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">LEAVE</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">SPEED</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">ROUTE</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">REGION</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">NIGHT</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"2\"><Data ss:Type=\"String\">WEEK</Data></Cell>");
            if (Payways != null)
            {
                sw.WriteLine("    <Cell ss:StyleID=\"1\"><Data ss:Type=\"String\">Cash</Data></Cell>");
                sw.WriteLine("    <Cell ss:StyleID=\"1\"><Data ss:Type=\"String\">T-Pass</Data></Cell>");
                sw.WriteLine("    <Cell><Data ss:Type=\"String\">Row ID</Data></Cell>");
                sw.WriteLine("    <Cell><Data ss:Type=\"String\">Exists</Data></Cell>");
            };
            sw.WriteLine("  </Row>");
            string regNm = "";
            int idd = 0, row = 1;
            // START POINT
            {
                sw.WriteLine("  <Row>");
                ++idd; ++row;
                //A - ##
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"Number\">{0}</Data></Cell>", idd));
                //B - ODOMETER
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"Number\">{0}</Data></Cell>", segmentsList[segmentsList.Count - 1].dist > segmentsList[0].dist ? 0 : (int)segmOriginalLineDist));
                //C - KM
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sKm\"><Data ss:Type=\"Number\">{0}</Data></Cell>", segmentsList[segmentsList.Count - 1].dist > segmentsList[0].dist ? 0 : (int)(segmOriginalLineDist / 1000)));
                //D - LENGTH
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sKm\" ss:Formula=\"=R[1]C[-1]-R[0]C[-1]\"><Data ss:Type=\"Number\"></Data></Cell>", row, row + 1));
                //E - TINDEX
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", "SS"));
                //F - EVENT
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", "START"));
                //G - NAME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", "START"));
                //H - ENTRY TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sDt\"><Data ss:Type=\"DateTime\">{0}</Data></Cell>", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:00")));
                //I - DELAY TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sTm\"><Data ss:Type=\"String\">{0}</Data></Cell>", "00:00"));
                //K - MOVE TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sTm\" ss:Formula=\"=R[0]C[-6]/R[0]C[3]/24\"><Data ss:Type=\"DateTime\"></Data></Cell>", row));
                //K - SEGMENT TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sTm\" ss:Formula=\"=R[0]C[-2]+R[0]C[-1]\"><Data ss:Type=\"DateTime\"></Data></Cell>", row));
                //L - LEAVE TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sDt\" ss:Formula=\"=R[0]C[-4]+R[0]C[-1]\"><Data ss:Type=\"DateTime\"></Data></Cell>", row));
                //M - AVG SPEED
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sSp\"><Data ss:Type=\"Number\">{0}</Data></Cell>", "60"));
                //N - DIST 2 ROUTE
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sKm\"><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                //O - REGION 
                regNm = "";
                if (KMZRebuilederForm.PIRU != null)
                {
                    int regNo = KMZRebuilederForm.PIRU.PointInRegion(originalLine[0].Y, originalLine[0].X);
                    regNm = regNo > 0 ? KMZRebuilederForm.PIRU.GetRegionName(regNo) : "...";
                };
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", regNm));
                // P - Night/Day
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\" ss:Formula=\"=IF(HOUR(R[0]C[-8])&lt;7,&quot;N&quot;,&quot;D&quot;)\"><Data ss:Type=\"String\"></Data></Cell>", row));
                // Q - DayOfWeek
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"dow\" ss:Formula=\"=WEEKDAY(R[0]C[-9])\"><Data ss:Type=\"String\"></Data></Cell>", row));
                if (Payways != null)
                {
                    //R
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                    //S
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                    //T
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                    //U
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                }
                //END
                sw.WriteLine("  </Row>");
            };
            // WAYPOINTS/SEGMENTS
            List<SBS> pts2spl = segmentsList;
            for (int si = 0; si < pts2spl.Count; si++)
            {
                sw.WriteLine("  <Row>");
                ++idd; ++row;
                string tb = "";
                if (pts2spl[si].tip == "S") tb = "t";
                if (pts2spl[si].tip == "E") tb = "b";
                //A - ##
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0{1}\"><Data ss:Type=\"Number\">{0}</Data></Cell>", idd, tb));
                //B - ODOMETER
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0{1}\"><Data ss:Type=\"Number\">{0}</Data></Cell>", (int)pts2spl[si].dist, tb));
                //C - KM
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sKm{1}\"><Data ss:Type=\"Number\">{0}</Data></Cell>", (int)(pts2spl[si].dist / 1000), tb));
                //D - LENGTH
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sKm{2}\" ss:Formula=\"=R[1]C[-1]-R[0]C[-1]\"><Data ss:Type=\"Number\"></Data></Cell>", row, row + 1, tb));
                //E - TINDEX
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0{1}\"><Data ss:Type=\"String\">{0}</Data></Cell>", String.IsNullOrEmpty(pts2spl[si].tIndex) ? "--" : pts2spl[si].tIndex, tb));
                //F - EVENT 
                string toWr = pts2spl[si].tip;
                if ((toWr == "X") && (!String.IsNullOrEmpty(pts2spl[si].name)) && (pts2spl[si].name.StartsWith("œ¬œÚ") || (pts2spl[si].name.StartsWith("œ¬œÒ")))) toWr = pts2spl[si].name.Substring(0, 4);
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0{1}\"><Data ss:Type=\"String\">{0}</Data></Cell>", toWr, tb));
                //G - NAME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0{1}\"><Data ss:Type=\"String\">{0}</Data></Cell>", pts2spl[si].name, tb));
                //H - ENTRY TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sDt{1}\" ss:Formula=\"=R[-1]C[4]\"><Data ss:Type=\"DateTime\"></Data></Cell>", row - 1, tb));
                //I - DELAY TIME
                TimeSpan ts = new TimeSpan(0, pts2spl[si].delay, 0);
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sTm{1}\"><Data ss:Type=\"String\">{0}</Data></Cell>", String.Format("{0:00}:{1:00}", ts.Hours, ts.Minutes), tb));
                //J - MOVE TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sTm{1}\" ss:Formula=\"=R[0]C[-6]/R[0]C[3]/24\"><Data ss:Type=\"DateTime\"></Data></Cell>", row, tb));
                //K - SEGMENT TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sTm{1}\" ss:Formula=\"=R[0]C[-2]+R[0]C[-1]\"><Data ss:Type=\"DateTime\"></Data></Cell>", row, tb));
                //L - LEAVE TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sDt{1}\" ss:Formula=\"=R[0]C[-4]+R[0]C[-1]\"><Data ss:Type=\"DateTime\"></Data></Cell>", row, tb));
                //M - AVG SPEED
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sSp{1}\"><Data ss:Type=\"Number\">{0}</Data></Cell>", pts2spl[si].speed.ToString(), tb));
                //N - DIST 2 ROUTE
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sKm{1}\"><Data ss:Type=\"Number\">{0}</Data></Cell>", (int)(pts2spl[si].toline / 1000), tb));
                //O - REGION 
                regNm = "";
                if (KMZRebuilederForm.PIRU != null)
                {
                    int regNo = KMZRebuilederForm.PIRU.PointInRegion(pts2spl[si].pointOnLine.Y, pts2spl[si].pointOnLine.X);
                    regNm = regNo > 0 ? KMZRebuilederForm.PIRU.GetRegionName(regNo) : "...";
                };
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0{1}\"><Data ss:Type=\"String\">{0}</Data></Cell>", regNm, tb));
                // P - Night/Day
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0{1}\" ss:Formula=\"=IF(HOUR(R[0]C[-8])&lt;7,&quot;N&quot;,&quot;D&quot;)\"><Data ss:Type=\"String\"></Data></Cell>", row, tb));
                // Q - DayOfWeek
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"dow{1}\" ss:Formula=\"=WEEKDAY(R[0]C[-9])\"><Data ss:Type=\"String\"></Data></Cell>", row, tb));
                if (Payways != null)
                {
                    //R
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0{1}\" ss:Formula=\"=IF(MID(RC[-11],1,4) = &quot;œ¬œÚ&quot;,INDEX('Costs'!C1:C25,RC[2] + IF(RC[-2]=&quot;N&quot;,0,1)+IF(RC[3],0,2),15+IF(RC[-2]=&quot;N&quot;,0,1)), &quot;&quot;)\"><Data ss:Type=\"Number\">{0}</Data></Cell>", "0", tb));
                    //S
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0{1}\" ss:Formula=\"=IF(MID(RC[-12],1,4) = &quot;œ¬œÚ&quot;,INDEX('Costs'!C1:C25,RC[1] + IF(RC[-3]=&quot;N&quot;,0,1)+IF(RC[2],0,2),17+IF(RC[-3]=&quot;N&quot;,0,1)), &quot;&quot;)\"><Data ss:Type=\"Number\">{0}</Data></Cell>", "0", tb));
                    //T
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:Formula=\"=IFERROR(MATCH(RC[-15]&amp;&quot;&quot;,'Costs'!C1,0),&quot;&quot;)\"><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                    //U
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:Formula=\"=IFERROR(FIND(RC[-4],INDEX('Costs'!C13:C13,RC[-1] + IF(RC[-5]=&quot;N&quot;,0,1)+0,1)),IFERROR(FIND(RC[-4],INDEX('Costs'!C13:C13,RC[-1] + IF(RC[-5]=&quot;N&quot;,0,1)+2,1)),&quot;&quot;))\"><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                }
                //END
                sw.WriteLine("  </Row>");
            };
            //FINISH/END POINT
            {
                sw.WriteLine("  <Row>");
                ++idd; ++row;
                //A - ##
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"Number\">{0}</Data></Cell>", idd));
                //B - ODOMETER
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"Number\">{0}</Data></Cell>", segmentsList[segmentsList.Count - 1].dist > segmentsList[0].dist ? (int)segmOriginalLineDist : 0));
                //C - KM
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sKm\"><Data ss:Type=\"Number\">{0}</Data></Cell>", segmentsList[segmentsList.Count - 1].dist > segmentsList[0].dist ? (int)(segmOriginalLineDist / 1000) : 0));
                //D - LENGTH
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                //E - TINDEX
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", "FF"));
                //F - EVENT
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", "FINISH"));
                //G - NAME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", "FINISH"));
                //H - ENTRY TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"sDt\" ss:Formula=\"=R[-1]C[4]\"><Data ss:Type=\"DateTime\"></Data></Cell>", row - 1));
                //I - DELAY TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                //J - MOVE TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                //K - SEGMENT TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                //L - LEAVE TIME
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                //M - AVG SPEED
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                //N - DIST 2 ROUTE
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                //O - REGION 
                regNm = "";
                if (KMZRebuilederForm.PIRU != null)
                {
                    int regNo = KMZRebuilederForm.PIRU.PointInRegion(originalLine[originalLine.Count - 1].Y, originalLine[originalLine.Count - 1].X);
                    regNm = regNo > 0 ? KMZRebuilederForm.PIRU.GetRegionName(regNo) : "...";
                };
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"String\">{0}</Data></Cell>", regNm));
                // P - Night/Day
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\" ss:Formula=\"=IF(HOUR(R[0]C[-8])&lt;7,&quot;N&quot;,&quot;D&quot;)\"><Data ss:Type=\"String\"></Data></Cell>", row));
                // Q - DayOfWeek
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"dow\" ss:Formula=\"=WEEKDAY(R[0]C[-9])\"><Data ss:Type=\"String\"></Data></Cell>", row));
                //END
                if (Payways != null)
                {
                    //R
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                    //S
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"0\"><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                    //T
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                    //U
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"Number\">{0}</Data></Cell>", "0"));
                }
                sw.WriteLine("  </Row>");
            };
            // WHO
            {
                sw.WriteLine("  <Row>");
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                sw.WriteLine("  </Row>");
                sw.WriteLine("  <Row>");
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeAcross=\"5\"><Data ss:Type=\"String\">{0}</Data></Cell>", "Created by KMZ Rebuilder Route Planner"));
                sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"dtf\"><Data ss:Type=\"DateTime\">{0}</Data></Cell>", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")));
                sw.WriteLine("  </Row>"); 
            };
            sw.WriteLine("</Table>");
            sw.WriteLine("<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">");
            sw.WriteLine("  <FreezePanes/>");
            sw.WriteLine("  <FrozenNoSplit/>");
            sw.WriteLine("  <SplitHorizontal>2</SplitHorizontal>");
            sw.WriteLine("  <TopRowBottomPane>2</TopRowBottomPane>");
            sw.WriteLine("  <SplitVertical>1</SplitVertical>");
            sw.WriteLine("  <LeftColumnRightPane>1</LeftColumnRightPane>");
            sw.WriteLine("  <ActivePane>0</ActivePane>");
            sw.WriteLine("</WorksheetOptions>");
            sw.WriteLine("</Worksheet>\r\n</Workbook>");
            sw.Close();
            fs.Close();
        }


        private void segView_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetWayLoop();
            if (segmentsList.Count == 0) return;
            
            if (segView.SelectedIndices.Count != 1) return;
            int ia = segView.SelectedIndices[0];
            int ib = -1;
            SBS sbs = segmentsList[ia];
            if (sbs.tip == "X") return;

            for (int i = 0; i < segmentsList.Count; i++)
                if (i != ia)
                    if (segmentsList[i].unical == sbs.unical)
                        ib = i;
            SetWayLoop(ia, ib);
        }

        private void SetWayLoop(int ia, int ib)
        {
            wla = ia;
            wlb = ib;
            if ((wla >= 0) && (wla < segView.Items.Count)) segView.Items[wla].BackColor = Color.Red;
            if ((wlb >= 0) && (wlb < segView.Items.Count)) segView.Items[wlb].BackColor = Color.Yellow;
            if ((wla >= 0) && (wlb >= 0))
            {
                for (int i = Math.Min(wla, wlb) + 1; i < Math.Max(wla, wlb); i++)
                {
                    SBS sbs = segmentsList[i];
                    if (sbs.tip != "X")
                    {
                        segView.Items[i].ForeColor = Color.White;
                        segView.Items[i].BackColor = Color.Salmon;
                    };
                };
            };
        }

        private int wla = -1;
        private int wlb = -1;
        private void ResetWayLoop()
        {
            if ((wla >= 0) && (wla < segView.Items.Count)) segView.Items[wla].BackColor = Color.LightSkyBlue;
            if ((wlb >= 0) && (wlb < segView.Items.Count)) segView.Items[wlb].BackColor = Color.LightSkyBlue;
            if ((wla >= 0) && (wlb >= 0))
                for (int i = Math.Min(wla, wlb) + 1; i < Math.Max(wla, wlb); i++)
                {
                    segView.Items[i].ForeColor = segView.ForeColor;
                    segView.Items[i].BackColor = segView.BackColor;
                };
        }

        private void segView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (segView.SelectedIndices.Count == 0) return;            
            SelectPlanned(segView.SelectedIndices[0]);
        }

        private void SelectPlanned(int id)
        {
            if (segmentsList.Count == 0) return;            

            this.selectedIndex = -1;            
            mapSelect.Clear();
            if (id < 0) return;
            if (id >= segmentsList.Count) return;

            SBS sbs = segmentsList[id];
            
            if ((sbs.toline < double.MaxValue) && (sbs.toline > 500))
            {
                NaviMapNet.MapLine ml = new NaviMapNet.MapLine(sbs.point, sbs.pointOnLine);
                ml.Name = "Way";
                ml.Width = 3;
                ml.Color = Color.White;
                mapSelect.Add(ml);

                NaviMapNet.MapPoint mx = new NaviMapNet.MapPoint(sbs.point);
                mx.Name = "WayFrom";
                mx.SizePixels = new Size(12, 12);
                mx.Color = Color.Black;
                mapSelect.Add(mx);
            };

            NaviMapNet.MapPoint ms = new NaviMapNet.MapPoint(sbs.toline < double.MaxValue ? sbs.pointOnLine : sbs.point);
            ms.Name = "Waypoint";
            ms.SizePixels = new Size(12, 12);
            ms.Squared = false;
            ms.Color = Color.Red;
            mapSelect.Add(ms);
            double[] mm = MView.MapBoundsMinMaxDegrees;
            if ((ms.Points[0].X <= mm[0]) || (ms.Points[0].Y <= mm[1]) || (ms.Points[0].X >= mm[2]) || (ms.Points[0].Y >= mm[3]))
                MView.CenterDegrees = ms.Points[0];            

            if (sbs.tip != "X")
            {
                int ib = -1;
                for (int i = 0; i < segmentsList.Count; i++)
                    if (i != id)
                        if (segmentsList[i].unical == sbs.unical)
                            ib = i;
                if (ib >= 0)
                {
                    if ((segmentsList[ib].toline < double.MaxValue) && (segmentsList[ib].toline > 500))
                    {
                        NaviMapNet.MapPoint mx = new NaviMapNet.MapPoint(segmentsList[ib].point);
                        mx.Name = "Way2From";
                        mx.SizePixels = new Size(12, 12);
                        mx.Color = Color.Black;
                        mapSelect.Add(mx);

                        NaviMapNet.MapLine ml = new NaviMapNet.MapLine(segmentsList[ib].point, segmentsList[ib].pointOnLine);
                        ml.Name = "Way2";
                        ml.Width = 3;
                        ml.Color = Color.White;
                        mapSelect.Add(ml);
                    };
                    ms = new NaviMapNet.MapPoint(segmentsList[ib].toline < double.MaxValue ? segmentsList[ib].pointOnLine : segmentsList[ib].point);
                    ms.Name = "Waypoint 2";
                    ms.SizePixels = new Size(12, 12);
                    ms.Squared = false;
                    ms.Color = Color.Yellow;
                    mapSelect.Add(ms);
                };
            };

            MView.DrawOnMapData();
        }

        private void segView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                segView_MouseDoubleClick(sender, null);
        }

        private void plist_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            doSelect();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void Flagalize()
        {
            for (int si = 0; si < segmentsList.Count; si++)
            {
                plist.Items[segmentsList[si].index].SubItems[1].Text = "F";
                plist.Items[segmentsList[si].index].BackColor = Color.Yellow;
            };
            int fc = 0;
            for (int i = 0; i < plist.Items.Count; i++)
                if (plist.Items[i].SubItems[1].Text == "F")
                    fc++;
            Flags.Text = "Flags: " + fc.ToString();            
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Route Planner Project (*.rpp)|*.rpp";
            ofd.DefaultExt = ".rpp";
            if (ofd.ShowDialog() == DialogResult.OK)
                OpenProject(ofd.FileName, true);
            ofd.Dispose();
        }

        public void OpenProject(string fileName, bool loadPayways)
        {
            string json = String.Empty;
            FileStream fs = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                ZipFile zf = new ZipFile(fs);
                ZipEntry ze = zf.GetEntry("data");
                if (ze != null)
                    using (StreamReader sr = new StreamReader(zf.GetInputStream(ze), Encoding.UTF8))
                        json = sr.ReadToEnd();
                zf.Close();
                fs.Close();
            }
            catch { }
            finally { if ((fs != null) && (fs.CanSeek)) fs.Dispose(); };
            if (String.IsNullOrEmpty(json))
            {
                try
                {
                    fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                    json = sr.ReadToEnd();
                    sr.Close();
                    fs.Close();
                }
                catch { }
                finally { if ((fs != null) && (fs.CanSeek)) fs.Dispose(); };
            };

            RoutePlannerProject rpp;
            try
            {
                rpp = Newtonsoft.Json.JsonConvert.DeserializeObject<RoutePlannerProject>(json);
                if (rpp.header == "ROUTE PLANNER PROJECT")
                {                    
                    loadroute(rpp.route);
                    segmentsList.Clear();
                    segmentsList.AddRange(rpp.segments);
                    LoadSegments2View();
                    Flagalize();
                    segCalculated = rpp.calculated;
                    segmOriginalLineDist = rpp.length;
                    if (loadPayways && (rpp.payways != null))
                        Payways = rpp.payways;
                };
            }
            catch { };
        }

        public void SetSplitterPage()
        {
            tabControl1.SelectedIndex = 0;
        }

        public void SetPlannerPage()
        {
            tabControl1.SelectedIndex = 1;
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (segmentsList.Count == 0) return;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Route Planner Project (*.rpp)|*.rpp";
            sfd.DefaultExt = ".rpp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                RoutePlannerProject rpp = new RoutePlannerProject();                
                rpp.header = "ROUTE PLANNER PROJECT";
                rpp.payways = Payways;
                rpp.route = originalLine.ToArray();
                rpp.segments = segmentsList.ToArray();
                rpp.calculated = segCalculated;
                rpp.length = segmOriginalLineDist;
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(rpp);
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                
                // No ZIP
                //StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                //sw.Write(json);
                //sw.Close();

                // ZIP
                ZipOutputStream zipStream = new ZipOutputStream(fs);
                zipStream.SetLevel(6); //0-9, 9 being the highest level of compression
                ZipEntry newEntry = new ZipEntry("data");
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                newEntry.Size = buffer.Length;
                zipStream.PutNextEntry(newEntry);
                zipStream.Write(buffer,0,buffer.Length);
                zipStream.CloseEntry();
                zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                zipStream.Close();

                fs.Close();
            }
            sfd.Dispose();            
        }

        private void selectNoneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mapSelect.Clear();
            MView.DrawOnMapData();
        }

        private void importSegmentsFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportPlannerSegments(true);
        }

        private void ImportPlannerSegments(bool keep_old)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Objects for Segmentation";
            ofd.Filter = "KML, KMZ files (*.kml;*.kmz)|*.kml;*.kmz";
            ofd.DefaultExt = ".kml";
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                ofd.Dispose();
                return;
            };

            // OPEN KML/KMZ
            KMFile kmf = new KMFile(ofd.FileName);
            ofd.Dispose();
            if (kmf.kmLayers.Count == 0) return;

            Regex rx = new Regex(@"route_planner_(?<name>[^=]+)\s*=(?<value>[\S\s][^\r\n]+)", RegexOptions.IgnoreCase);

            // SELECT LAYERS
            GMLayRenamerForm sf = new GMLayRenamerForm();
            sf.subtext.Visible = true;
            sf.subtext.Text = "Additional tags in description:\r\n";
            sf.subtext.Text += "  route_planner_skip=true -- skip point\r\n";
            sf.subtext.Text += "  route_planner_skip=false -- check layer on import\r\n";
            sf.subtext.Text += "  route_planner_delay=value -- delay in minutes\r\n";
            sf.subtext.Text += "  route_planner_source=route -- source route";
            sf.Text = "Flag Route by Objects from Layers";
            sf.layers.MultiSelect = true;
            int objc = 0;
            foreach (KMLayer lay in kmf.kmLayers)
            {
                objc += lay.placemarks;
                CustomListViewItem lvi = new CustomListViewItem(lay.name + "[" + lay.placemarks.ToString() + "]");
                XmlNode pmk = lay.file.kmlDoc.SelectNodes("kml/Document/Folder")[lay.id];
                string description = "";
                try
                {
                    description = pmk.SelectSingleNode("description").ChildNodes[0].Value;
                    MatchCollection mc = rx.Matches(description);
                    if (mc.Count > 0)
                        foreach (Match mx in mc)
                        {
                            if ((mx.Groups["name"].Value.ToLower() == "skip") && (mx.Groups["value"].Value.ToLower() == "false")) lvi.Checked = true;
                            if ((mx.Groups["name"].Value.ToLower() == "skip") && (mx.Groups["value"].Value.ToLower() == "0")) lvi.Checked = true;
                            if ((mx.Groups["name"].Value.ToLower() == "skip") && (mx.Groups["value"].Value.ToLower() == "no")) lvi.Checked = true;
                        };
                }
                catch { };
                sf.layers.Items.Add(lvi);
            };
            sf.label1.Text = "Select Objects from Layers (Total " + objc.ToString() + " objects in " + kmf.kmLayers.Count.ToString() + " layers):";
            sf.layers.View = View.Details;
            sf.layers.Columns[0].Name = "Layer";
            sf.layers.Columns[0].Width = sf.layers.Width - 100;
            sf.layers.Columns.Add("Objects");
            sf.layers.Columns[1].Width = 70;
            sf.layers.FullRowSelect = true;
            sf.layers.CheckBoxes = true;
            sf.layers.LargeImageList = null;
            sf.layers.SmallImageList = null;
            sf.layers.StateImageList = null;
            sf.layers.MultiSelect = true;
            if (sf.ShowDialog() != DialogResult.OK)
            {
                sf.Dispose();
                return;
            };
            List<int> indicies = new List<int>();
            for (int i = 0; i < sf.layers.CheckedIndices.Count; i++)
                indicies.Add(sf.layers.CheckedIndices[i]);
            sf.Dispose();
            if (indicies.Count == 0) return;
            ulong unical = 0;
            Dictionary<ulong, int> unics = new Dictionary<ulong, int>();
            if (keep_old)
                for (int i = 0; i < segmentsList.Count; i++)
                    if (segmentsList[i].unical > unical)
                        unical = segmentsList[i].unical;

            if (wbf != null)
                wbf.Show("Track Planner", "Wait, loading segments ...");

            Regex rindex = new Regex(@"route_planner_index=(?<id>[^\r\n]*)", RegexOptions.IgnoreCase);

            PointF[] source_points = null;
            // LOAD POINTS and LINES
            List<SBS> pts2spl = new List<SBS>();
            for (int i = 0; i < indicies.Count; i++)
            {
                KMLayer l = kmf.kmLayers[indicies[i]];
                XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                XmlNodeList xnf = xn.SelectNodes("Placemark");
                if (xnf.Count == 0) continue;

                int el_line = 0;
                int el_polygon = 0;
                int el_point = 0;
                for (int el = 0; el < xnf.Count; el++)
                    if (xnf[el].ChildNodes.Count > 0)
                    {
                        bool todo = true;
                        bool source = false;
                        string name = "NoName";
                        XmlNode xnn = null;
                        try { name = xnf[el].SelectSingleNode("name").ChildNodes[0].Value; }
                        catch { };
                        string description = "";
                        string idex = "";
                        string json = "";
                        int delay = 0;
                        int speed = 60;
                        bool doubled = false;
                        try
                        {
                            description = xnf[el].SelectSingleNode("description").ChildNodes[0].Value;
                            MatchCollection mc = rx.Matches(description);
                            if (mc.Count > 0)
                                foreach (Match mx in mc)
                                {
                                    if ((mx.Groups["name"].Value.ToLower() == "skip") && (mx.Groups["value"].Value.ToLower() == "true")) todo = false;
                                    if ((mx.Groups["name"].Value.ToLower() == "skip") && (mx.Groups["value"].Value.ToLower() == "1")) todo = false;
                                    if ((mx.Groups["name"].Value.ToLower() == "skip") && (mx.Groups["value"].Value.ToLower() == "yes")) todo = false;
                                    if ((mx.Groups["name"].Value.ToLower() == "delay") && (mx.Groups["value"].Value.ToLower() != "0")) int.TryParse(mx.Groups["value"].Value.ToLower(), out delay);
                                    if ((mx.Groups["name"].Value.ToLower() == "speed") && (mx.Groups["value"].Value.ToLower() != "0")) int.TryParse(mx.Groups["value"].Value.ToLower(), out speed);
                                    if (mx.Groups["name"].Value.ToLower() == "doubled") bool.TryParse(mx.Groups["value"].Value.ToLower(), out doubled);
                                    if (mx.Groups["name"].Value.ToLower() == "json") json = mx.Groups["value"].Value;
                                    if ((mx.Groups["name"].Value.ToLower() == "source") && (mx.Groups["value"].Value.ToLower() == "route")) { todo = false; source = true; };
                                };
                            Match mj = rindex.Match(description);
                            if (mj.Success)
                                idex = mj.Groups["id"].Value;
                        }
                        catch { };
                        if (xnf[el].SelectNodes("LineString").Count > 0) // ++LINE
                        {
                            el_line++;
                            xnn = xnf[el].SelectNodes("LineString/coordinates")[0];
                        };
                        if (xnf[el].SelectNodes("Polygon").Count > 0) // ++Polygon
                        {
                            el_polygon++;
                        };
                        if (xnf[el].SelectNodes("Point").Count > 0) // ++Point
                        {
                            el_point++;
                            xnn = xnf[el].SelectNodes("Point/coordinates")[0];
                        };
                        string[] xyz = null;
                        if (xnn != null) xyz = xnn.ChildNodes[0].Value.Trim('\n').Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (!String.IsNullOrEmpty(json))
                        {
                            try
                            {
                                SBS sbs = Newtonsoft.Json.JsonConvert.DeserializeObject<SBS>(json);                                
                                if (unics.ContainsKey(sbs.unical))
                                    sbs.unical = (ulong)unics[sbs.unical];
                                else
                                {
                                    unics.Add(sbs.unical, (int)++unical);
                                    sbs.unical = (ulong)unical;
                                };
                                pts2spl.Add(sbs);
                                todo = false;
                            }
                            catch 
                            {
                            };
                        };
                        if (todo && (xyz != null) && (xyz.Length != 0))
                        {
                            ++unical;
                            string[] cc = xyz[0].Split(new char[] { ',' });
                            SBS sbs = new SBS(name, new PointF((float)double.Parse(cc[0], System.Globalization.CultureInfo.InvariantCulture), (float)double.Parse(cc[1], System.Globalization.CultureInfo.InvariantCulture)), xyz.Length == 1 ? "X" : "W", unical);
                            sbs.tIndex = idex;
                            sbs.delay = delay;
                            sbs.speed = speed;
                            pts2spl.Add(sbs);
                            if (xyz.Length > 1)
                            {
                                cc = xyz[xyz.Length - 1].Split(new char[] { ',' });
                                sbs = new SBS(name, new PointF((float)double.Parse(cc[0], System.Globalization.CultureInfo.InvariantCulture), (float)double.Parse(cc[1], System.Globalization.CultureInfo.InvariantCulture)), "W", unical);
                                sbs.tIndex = idex;
                                pts2spl.Add(sbs);
                            }
                            else if (doubled)
                            {
                                sbs.unical = ++unical;
                                sbs.delay = 0;
                                pts2spl.Add(sbs);
                            };
                        };
                        if (source && (xyz != null) && (xyz.Length > 1))
                        {
                            source_points = new PointF[xyz.Length];
                            for (int sip = 0; sip < xyz.Length; sip++)
                            {
                                string[] cc = xyz[sip].Split(new char[] { ',' });
                                source_points[sip] = new PointF((float)double.Parse(cc[0], System.Globalization.CultureInfo.InvariantCulture), (float)double.Parse(cc[1], System.Globalization.CultureInfo.InvariantCulture));
                            };                            
                        };
                    };
            };
            if (pts2spl.Count == 0) return;
            if (keep_old)
                segmentsList.AddRange(pts2spl);
            else
                segmentsList = pts2spl;
            LoadSegments2View();
            segCalculated = false;
            if(source_points != null)
                loadroute(source_points);
            if (wbf != null)
                wbf.Hide();
        }

        private void clearSegmentsListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            segmentsList.Clear();
            segView.Items.Clear();
            segCalculated = false;
        }

        private void exportProjectToKMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (segmentsList.Count == 0) return;
            if (!segCalculated) return;

            string fileName = null;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Export";
            sfd.DefaultExt = ".kml";
            sfd.Filter = "KML Files (*.kml)|*.kml";
            if (sfd.ShowDialog() == DialogResult.OK)
                fileName = sfd.FileName;
            sfd.Dispose();
            if (fileName == null) return;

            string kml_name = "Route Planner Data";
            string ctn = kml_name;
            if (InputBox.Show("Export to KML", "Enter Layer Name:", ref kml_name) == DialogResult.OK)
                ctn = kml_name;

            try
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document>");
                sw.WriteLine("<name><![CDATA[" + ctn + "]]></name><createdby>KMZ Rebuilder Route Planner</createdby>");
                sw.WriteLine("<Folder><name><![CDATA[" + ctn + "]]></name>");
                sw.WriteLine("<description><![CDATA[route_planner_skip=false]]></description>");
                for (int i = 0; i < segmentsList.Count; i++)
                {
                    SBS sbs = segmentsList[i];
                    sw.Write("<Placemark><name><![CDATA[" + sbs.name + "]]></name>");
                    string desc = "route_planner_skip=false\r\n";
                    desc += "route_planner_delay="+sbs.delay.ToString()+"\r\n";
                    desc += "route_planner_speed=" + sbs.speed.ToString() + "\r\n";
                    desc += "route_planner_json=" + Newtonsoft.Json.JsonConvert.SerializeObject(sbs);
                    sw.WriteLine("<description><![CDATA[" + desc + "]]></description>");
                    sw.Write("<Point><coordinates>");
                    sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", sbs.pointOnLine.X, sbs.pointOnLine.Y));
                    sw.WriteLine("</coordinates></Point></Placemark>");
                };
                sw.WriteLine("</Folder>");
                int km_iter = 5;
                if ((originalLine.Count > 1) && InputBox.Show("Save km flags to KML file", "Place each flag next km:", ref km_iter, 1, 250, null) == DialogResult.OK)
                {

                    sw.WriteLine("<Folder><name>Each " + km_iter.ToString() + " km flags</name>");
                    sw.WriteLine("<description><![CDATA[route_planner_skip=true]]></description>");

                    sw.Write("<Placemark>");
                    sw.WriteLine("<name><![CDATA[0 km]]></name>");
                    sw.Write("<Point><coordinates>");
                    sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", originalLine[0].X, originalLine[0].Y));
                    sw.WriteLine("</coordinates></Point></Placemark>");

                    float kmError = 0.15f;
                    float total_dist = 0;
                    int currFlag = 1;

                    List<KM> kmFlags = new List<KM>();
                    float prevLat = originalLine[0].Y;
                    float prevLon = originalLine[0].X;

                    for (int i = 1; i < originalLine.Count; i++)
                    {
                        float currLat = originalLine[i].Y;
                        float currLon = originalLine[i].X;
                        float dist_prev_curr = Utils.GetLengthMeters((double)prevLat, (double)prevLon, (double)currLat, (double)currLon, false) / 1000f;
                        total_dist += dist_prev_curr;
                        bool flag = true;
                        while (flag)
                        {
                            float walked = total_dist - (currFlag);
                            if (walked >= 0)
                            {
                                if (walked <= kmError)
                                {
                                    if ((currFlag % km_iter) == 0)
                                    {
                                        sw.Write("<Placemark>");
                                        sw.WriteLine("<name><![CDATA[" + currFlag.ToString() + " km]]></name>");
                                        sw.Write("<Point><coordinates>");
                                        sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", currLon, currLat));
                                        sw.WriteLine("</coordinates></Point></Placemark>");
                                    };
                                    currFlag++;
                                }
                                else
                                {
                                    float walkback = dist_prev_curr - walked;
                                    float btwLat = prevLat + (((currLat - prevLat) / dist_prev_curr) * walkback);
                                    float btwLon = prevLon + (((currLon - prevLon) / dist_prev_curr) * walkback);
                                    if ((currFlag % km_iter) == 0)
                                    {
                                        sw.Write("<Placemark>");
                                        sw.WriteLine("<name><![CDATA[" + currFlag.ToString() + " km]]></name>");
                                        sw.Write("<Point><coordinates>");
                                        sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", btwLon, btwLat));
                                        sw.WriteLine("</coordinates></Point></Placemark>");
                                    };
                                    currFlag++;
                                }
                            }
                            else
                            {
                                flag = false;
                            };
                        }
                        prevLat = currLat;
                        prevLon = currLon;
                    }

                    sw.WriteLine("</Folder>");
                };
                sw.WriteLine("</Document></kml>");
                sw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error saving KML", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
        }

        private void inverseTrackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalLine.Count == 0) return;
            originalLine.Reverse();
            LoadPointsAndResetDefaults();
            DrawPolys();
        }

        private void sortASCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (segmentsList.Count == 0) return;
            if (!segCalculated) return;

            segmentsList.Reverse();
            LoadSegments2View();
        }

        private void segView_KeyDown(object sender, KeyEventArgs e)
        {
            if (segmentsList.Count == 0) return;
            if (segView.SelectedIndices.Count != 1) return;
            int si = segView.SelectedIndices[0];                        
            SBS sbs = segmentsList[si];
            if (e.KeyValue == 37) // left
            {
                if (si == 0) return;
                if (sbs.tip == "X")
                {
                    segView.Items[si].Selected = false;
                    segView.Items[si-1].Selected = true;
                    segView.Items[si-1].Focused = true;
                    segView.EnsureVisible(si - 1);
                    e.Handled = true;
                    return;
                };
                int ib = -1;
                for (int i = 0; i < segmentsList.Count; i++)
                    if (i != si)
                        if (segmentsList[i].unical == sbs.unical)
                            ib = i;
                if (ib < si)
                {
                    segView.Items[si].Selected = false;
                    segView.Items[ib].Selected = true;
                    segView.Items[ib].Focused = true;
                    segView.EnsureVisible(ib);
                    e.Handled = true;
                    return;
                }
                else
                {
                    segView.Items[si].Selected = false;
                    segView.Items[si - 1].Selected = true;
                    segView.Items[si - 1].Focused = true;
                    segView.EnsureVisible(si - 1);
                    e.Handled = true;
                    return;
                };
                
            };
            if (e.KeyValue == 39) // right
            {
                if (si == (segmentsList.Count - 1)) return;
                if (sbs.tip == "X")
                {
                    segView.Items[si].Selected = false;
                    segView.Items[si+1].Selected = true;
                    segView.Items[si+1].Focused = true;
                    segView.EnsureVisible(si + 1);
                    e.Handled = true;
                    return;
                };
                int ib = -1;
                for (int i = 0; i < segmentsList.Count; i++)
                    if (i != si)
                        if (segmentsList[i].unical == sbs.unical)
                            ib = i;
                if (ib > si)
                {
                    segView.Items[si].Selected = false;
                    segView.Items[ib].Selected = true;
                    segView.Items[ib].Focused = true;
                    segView.EnsureVisible(ib);
                    e.Handled = true;
                    return;
                }
                else
                {
                    segView.Items[si].Selected = false;
                    segView.Items[si + 1].Selected = true;
                    segView.Items[si + 1].Focused = true;
                    segView.EnsureVisible(si + 1);
                    e.Handled = true;
                    return;
                };
            };
        }

        private void exportKMFlagsToKMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalLine.Count < 2) return;
            
            string fileName = null;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Export";
            sfd.DefaultExt = ".kml";
            sfd.Filter = "KML Files (*.kml)|*.kml";
            if (sfd.ShowDialog() == DialogResult.OK)
                fileName = sfd.FileName;
            sfd.Dispose();
            if (fileName == null) return;

            int km_iter = 5;
            if (InputBox.Show("Save km flags to KML file", "Place each flag next km:", ref km_iter, 1, 250, null) != DialogResult.OK)
                return;

            try
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document>");
                sw.WriteLine("<name>Each " + km_iter.ToString() + " km flags</name><createdby>KMZ Rebuilder Route Planner</createdby>");
                sw.WriteLine("<Folder><name>Each " + km_iter.ToString() + " km flags</name>");
                sw.WriteLine("<description><![CDATA[route_planner_skip=true]]></description>");

                sw.Write("<Placemark>");
                sw.WriteLine("<name><![CDATA[0 km]]></name>");
                sw.Write("<Point><coordinates>");
                sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", originalLine[0].X, originalLine[0].Y));
                sw.WriteLine("</coordinates></Point></Placemark>");

                float kmError = 0.15f;
                float total_dist = 0;
                int currFlag = 1;

                List<KM> kmFlags = new List<KM>();
                float prevLat = originalLine[0].Y;
                float prevLon = originalLine[0].X;

                for (int i = 1; i < originalLine.Count; i++)
                {
                    float currLat = originalLine[i].Y;
                    float currLon = originalLine[i].X;
                    float dist_prev_curr = Utils.GetLengthMeters((double)prevLat, (double)prevLon, (double)currLat, (double)currLon, false) / 1000f;
                    total_dist += dist_prev_curr;
                    bool flag = true;
                    while (flag)
                    {
                        float walked = total_dist - (currFlag);
                        if (walked >= 0)
                        {
                            if (walked <= kmError)
                            {
                                if ((currFlag % km_iter) == 0)
                                {
                                    sw.Write("<Placemark>");
                                    sw.WriteLine("<name><![CDATA[" + currFlag.ToString() + " km]]></name>");
                                    sw.Write("<Point><coordinates>");
                                    sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", currLon, currLat));
                                    sw.WriteLine("</coordinates></Point></Placemark>");
                                };
                                currFlag++;
                            }
                            else
                            {
                                float walkback = dist_prev_curr - walked;
                                float btwLat = prevLat + (((currLat - prevLat) / dist_prev_curr) * walkback);
                                float btwLon = prevLon + (((currLon - prevLon) / dist_prev_curr) * walkback);
                                if ((currFlag % km_iter) == 0)
                                {
                                    sw.Write("<Placemark>");
                                    sw.WriteLine("<name><![CDATA[" + currFlag.ToString() + " km]]></name>");
                                    sw.Write("<Point><coordinates>");
                                    sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", btwLon, btwLat));
                                    sw.WriteLine("</coordinates></Point></Placemark>");
                                };
                                currFlag++;
                            }
                        }
                        else
                        {
                            flag = false;
                        };
                    }
                    prevLat = currLat;
                    prevLon = currLon;
                }

                sw.WriteLine("</Folder>");
                sw.WriteLine("</Document></kml>");
                sw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error saving KML", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
        }

        private void loadPayWayTarrifsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Payway Tarrifs (*.dxml)|*.dxml";
            ofd.DefaultExt = ".dxml";
            if (ofd.ShowDialog() == DialogResult.OK)
                Payways = AvtodorTRWeb.PayWays.Load(ofd.FileName);
            ofd.Dispose();
        }

        private void clearPaywaysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Payways = null;
        }

        private void loadRoutePlannerProjectNoPaywaysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Route Planner Project (*.rpp)|*.rpp";
            ofd.DefaultExt = ".rpp";
            if (ofd.ShowDialog() == DialogResult.OK)
                OpenProject(ofd.FileName, false);
            ofd.Dispose();
        }

        private void selectMBTilesFilesToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void delayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModifySegment(true, false);
        }

        private void speedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModifySegment(false, true);
        }

        private void ModifySegment(bool del, bool sp)
        {
            if (segView.SelectedIndices.Count != 1) return;
            int si = segView.SelectedIndices[0];
            SBS sbs = segmentsList[si];
            if (del)
            {
                int delay = sbs.delay;
                if (InputBox.Show(sbs.name, "Delay (in minutes):", ref delay, 0, 2880) == DialogResult.OK)
                {
                    sbs.delay = delay;
                    segmentsList[si] = sbs;
                };
            };
            if (sp)
            {
                int speed = sbs.speed;
                if (InputBox.Show(sbs.name, "Speed (in kmph):", ref speed, 0, 150) == DialogResult.OK)
                {
                    sbs.speed = speed;
                    segmentsList[si] = sbs;
                };
            };
            UpdateSegmentOnView(sbs, segView.Items[si]);
        }

        private void map1_Click(object sender, EventArgs e)
        {
            if (segView.Items.Count == 0) return;
            int delay = 0;
            if (InputBox.Show("Set Delay to All", "Delay (in minutes):", ref delay, 0, 2880) == DialogResult.OK)
            {
                for (int i = 0; i < segmentsList.Count; i++)
                {
                    SBS sbs = segmentsList[i];
                    sbs.delay = delay;
                    segmentsList[i] = sbs;
                };
                LoadSegments2View();
            };
        }

        private void map2_Click(object sender, EventArgs e)
        {
            if (segView.Items.Count == 0) return;
            int speed = 0;
            if (InputBox.Show("Set Speed to All", "Speed (in kmph):", ref speed, 0, 150) == DialogResult.OK)
            {
                for (int i = 0; i < segmentsList.Count; i++)
                {
                    SBS sbs = segmentsList[i];
                    sbs.speed = speed;
                    segmentsList[i] = sbs;
                };
                LoadSegments2View();
            };
        }

        private void map3_Click(object sender, EventArgs e)
        {
            if (segView.Items.Count == 0) return;
            int delay = 0;
            if (InputBox.Show("Increase\\Decrease Delay to All", "Delay (in minutes +\\-):", ref delay, -2880, 2880) == DialogResult.OK)
            {
                for (int i = 0; i < segmentsList.Count; i++)
                {
                    SBS sbs = segmentsList[i];
                    sbs.delay += delay;
                    if (sbs.delay < 0) sbs.delay = 0;
                    segmentsList[i] = sbs;
                };
                LoadSegments2View();
            };
        }

        private void map4_Click(object sender, EventArgs e)
        {
            if (segView.Items.Count == 0) return;
            int speed = 0;
            if (InputBox.Show("Increase\\Decrease Speed to All", "Speed (in kmph +\\-):", ref speed, -150, 150) == DialogResult.OK)
            {
                for (int i = 0; i < segmentsList.Count; i++)
                {
                    SBS sbs = segmentsList[i];
                    sbs.speed += speed;
                    if (sbs.speed < 0) sbs.speed = 0;
                    segmentsList[i] = sbs;
                };
                LoadSegments2View();
            };
        }

        private void map5_Click(object sender, EventArgs e)
        {
            if (segView.Items.Count == 0) return;
            int dist = 3500;
            if (InputBox.Show("Remove where Distance to Route is less than", "Distance to Route (in meters):", ref dist, 0, 300000) == DialogResult.OK)
            {
                for (int i = segmentsList.Count - 1; i >= 0 ; i--)
                    if (segmentsList[i].toline > dist) 
                        segmentsList.RemoveAt(i);
                LoadSegments2View();
            };
        }
    }

    public class CustomListViewItem : ListViewItem
    {
        public CustomListViewItem(): base(){}
        public CustomListViewItem(string text) : base(text) { }
        public CustomListViewItem(string[] items) : base(items) { }
    }
}

namespace AvtodorTRWeb
{
    public class PayWays
    {
        [XmlElement("url")]
        public string url;
        [XmlElement("grabbed")]
        public DateTime grabbed;
        [XmlElement("createdBy")]
        public string createdBy = "Avtodor-Tr M4 Grabber by milokz@gmail.com";

        [XmlElement("payway")]
        public List<PayWay> Payways = new List<PayWay>();

        public void Add(PayWay payway) { this.Payways.Add(payway); }
        public void Sort() { this.Payways.Sort(new PayWaySorter()); }
        public List<PayWay>.Enumerator GetEnumerator() { return this.Payways.GetEnumerator(); }

        [XmlIgnore]
        public List<AvtodorTRWeb.Tarrif> MaxTarrif
        {
            get
            {
                if (this.Payways.Count == 0) return null;
                List<AvtodorTRWeb.Tarrif> res = new List<Tarrif>();
                int max = int.MinValue;
                for (int i = 0; i < this.Payways.Count; i++)
                {
                    if (this.Payways[i].Tarrifs.Count <= max) continue;
                    max = this.Payways[i].Tarrifs.Count;
                    res = this.Payways[i].Tarrifs;
                };
                return res;
            }
        }

        public static PayWays Load(string fname)
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(PayWays));
            System.IO.StreamReader reader = System.IO.File.OpenText(fname);
            PayWays c = (PayWays)xs.Deserialize(reader);
            reader.Close();
            return c;
        }

        public static void Export2ExcelXML(PayWays payways, StreamWriter sw)
        {
            sw.WriteLine("  <Style ss:ID=\"x0\"><Alignment ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"x1\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Font ss:Bold=\"1\"/><Borders><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"x2\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Font ss:Bold=\"1\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"2\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"col0\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"col1r\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Interior ss:Color=\"#FFB0FF\" ss:Pattern=\"Solid\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"col2r\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Interior ss:Color=\"#FFB066\" ss:Pattern=\"Solid\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"col1b\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Interior ss:Color=\"#FFCCFF\" ss:Pattern=\"Solid\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"col2b\"><Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/><Interior ss:Color=\"#FFCC66\" ss:Pattern=\"Solid\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"nor1r\"><Alignment ss:Vertical=\"Center\" ss:WrapText=\"1\"/><Interior ss:Color=\"#FFCCFF\" ss:Pattern=\"Solid\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("  <Style ss:ID=\"nor2r\"><Alignment ss:Vertical=\"Center\" ss:WrapText=\"1\"/><Interior ss:Color=\"#FFCC66\" ss:Pattern=\"Solid\"/><Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/><Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\"/></Borders></Style>");
            sw.WriteLine("</Styles>");
            sw.WriteLine("<Worksheet ss:Name=\"Costs\">\r\n<Table>");
            sw.WriteLine("  <Column ss:Width=\"30\"/>");
            sw.WriteLine("  <Column ss:Width=\"180\"/>");
            sw.WriteLine("  <Column ss:Width=\"64\"/>");
            sw.WriteLine("  <Column ss:Width=\"42\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"70\"/>");
            sw.WriteLine("  <Column ss:Width=\"20\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Column ss:Width=\"50\"/>");
            sw.WriteLine("  <Row>");
            sw.WriteLine("    <Cell ss:StyleID=\"x1\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\">œÎ‡ÚÌ˚È Û˜‡ÒÚÓÍ</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x1\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\">œ≈–»Œƒ</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x1\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\"> ¿“≈√Œ–»ﬂ 1</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x1\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\"> ¿“≈√Œ–»ﬂ 2</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x1\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\"> ¿“≈√Œ–»ﬂ 3</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x1\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\"> ¿“≈√Œ–»ﬂ 4</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x1\"><Data ss:Type=\"String\">ƒÕ»</Data></Cell>");
            sw.WriteLine("    <Cell><Data ss:Type=\"String\"></Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x1\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\">Õ¿À»◊Õ€≈</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x1\" ss:MergeAcross=\"1\"><Data ss:Type=\"String\">“–¿Õ—œŒÕƒ≈–</Data></Cell>");
            sw.WriteLine("  </Row>");
            sw.WriteLine("  <Row>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">##</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">Õ¿»Ã≈ÕŒ¬¿Õ»≈</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">¬–≈Ãﬂ</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">ƒ≈Õ‹</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">Õ¿À</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">“–¿Õ—œ</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">Õ¿À</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">“–¿Õ—œ</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">Õ¿À</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">“–¿Õ—œ</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">Õ¿À</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">“–¿Õ—œ</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">Õ≈ƒ≈À»</Data></Cell>");
            sw.WriteLine("    <Cell><Data ss:Type=\"String\"></Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">ÕŒ◊‹</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">ƒ≈Õ‹</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">ÕŒ◊‹</Data></Cell>");
            sw.WriteLine("    <Cell ss:StyleID=\"x2\"><Data ss:Type=\"String\">ƒ≈Õ‹</Data></Cell>");
            sw.WriteLine("  </Row>");
            int line = 2;
            List<string> payed = new List<string>();
            List<int> pOQ = new List<int>();
            List<int> pPR = new List<int>();
            foreach (AvtodorTRWeb.PayWay pay in payways)
            {
                bool isFirst = true;
                if (!(String.IsNullOrEmpty(pay.ID)) && (char.IsDigit(pay.ID[pay.ID.Length - 1])))
                    if (pay.Tarrifs.Count == 4)
                        pOQ.Add(line + (pay.Tarrifs[1].Costs[1] > pay.Tarrifs[3].Costs[1] ? 2 : 4));
                    else
                        pOQ.Add(line + 2);
                foreach (AvtodorTRWeb.Tarrif tar in pay.Tarrifs)
                {
                    ++line;
                    sw.WriteLine("  <Row>");
                    string col = "col2";
                    string nor = "nor2r";
                    try
                    {
                        col = ((char.IsDigit(pay.ID[pay.ID.Length - 1])) ? "col1" : "col2") + (((line % 2) == 0) ? "b" : "r");
                        nor = ((char.IsDigit(pay.ID[pay.ID.Length - 1])) ? "nor1r" : "nor2r");
                    }
                    catch { };
                    if (isFirst)
                    {
                        sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeDown=\"" + (pay.Tarrifs.Count - 1).ToString() + "\" ss:StyleID=\"" + col + "\"><Data ss:Type=\"String\">{0}</Data></Cell>", pay.ID));
                        sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeDown=\"" + (pay.Tarrifs.Count - 1).ToString() + "\" ss:StyleID=\"" + nor + "\"><Data ss:Type=\"String\"><B>{0}</B></Data></Cell>", pay.Name));
                        isFirst = false;
                    };

                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:Index=\"3\" ss:StyleID=\"" + col + "\"><Data ss:Type=\"String\">{0}</Data></Cell>", tar.Time));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"" + col + "\"><Data ss:Type=\"String\">{0}</Data></Cell>", tar.DaysRus));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"" + col + "\"><Data ss:Type=\"Number\">{0}</Data></Cell>", tar.GetCost(1, false)));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"" + col + "\"><Data ss:Type=\"Number\">{0}</Data></Cell>", tar.GetCost(1, true)));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"" + col + "\"><Data ss:Type=\"Number\">{0}</Data></Cell>", tar.GetCost(2, false)));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"" + col + "\"><Data ss:Type=\"Number\">{0}</Data></Cell>", tar.GetCost(2, true)));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"" + col + "\"><Data ss:Type=\"Number\">{0}</Data></Cell>", tar.GetCost(3, false)));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"" + col + "\"><Data ss:Type=\"Number\">{0}</Data></Cell>", tar.GetCost(3, true)));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"" + col + "\"><Data ss:Type=\"Number\">{0}</Data></Cell>", tar.GetCost(4, false)));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"" + col + "\"><Data ss:Type=\"Number\">{0}</Data></Cell>", tar.GetCost(4, true)));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"" + col + "\"><Data ss:Type=\"String\">{0}</Data></Cell>", tar.Days));
                    sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                    if ((line % 2) == 0) sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                    else sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"=R[0]C[-10]\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                    if ((line % 2) == 1) sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                    else sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"=R[0]C[-11]\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                    if ((line % 2) == 0) sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                    else sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"=R[0]C[-11]\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                    if ((line % 2) == 1) sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                    else sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"=R[0]C[-12]\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
                    sw.WriteLine("  </Row>");
                };
            };
            sw.WriteLine("  <Row>");
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine("  </Row>");

            line += 3;
            string sO = "0";
            foreach (int iv in pOQ) sO += "+R[" + (iv - line).ToString() + "]C";

            sw.WriteLine("  <Row>");
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeDown=\"3\" ss:StyleID=\"col1r\"><Data ss:Type=\"String\">{0}</Data></Cell>", "ALL"));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeDown=\"3\" ss:StyleID=\"col1r\"><Data ss:Type=\"String\">{0}</Data></Cell>", "¬ÒÂ Û˜‡ÒÚÍË Ú‡ÒÒ˚ M4"));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\"><Data ss:Type=\"String\">{0}</Data></Cell>", payways.MaxTarrif[0].TimeRus));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\"><Data ss:Type=\"String\">{0}</Data></Cell>", payways.MaxTarrif[0].DaysRus));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=R[0]C[10]\"><Data ss:Type=\"Number\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=R[0]C[11]\"><Data ss:Type=\"Number\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=RC[-3]-RC[-2]\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeAcross=\"4\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"={0}\"><Data ss:Type=\"String\"></Data></Cell>", sO));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"={0}\"><Data ss:Type=\"String\"></Data></Cell>", sO));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine("  </Row>");
            sw.WriteLine("  <Row>");
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:Index=\"3\" ss:StyleID=\"col1r\"><Data ss:Type=\"String\">{0}</Data></Cell>", payways.MaxTarrif[1].TimeRus));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\"><Data ss:Type=\"String\">{0}</Data></Cell>", payways.MaxTarrif[1].DaysRus));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=R[0]C[11]\"><Data ss:Type=\"Number\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=R[0]C[12]\"><Data ss:Type=\"Number\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=RC[-3]-RC[-2]\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeAcross=\"4\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"={0}\"><Data ss:Type=\"String\"></Data></Cell>", sO));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"={0}\"><Data ss:Type=\"String\"></Data></Cell>", sO));
            sw.WriteLine("  </Row>");
            sw.WriteLine("  <Row>");
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:Index=\"3\" ss:StyleID=\"col1r\"><Data ss:Type=\"String\">{0}</Data></Cell>", payways.MaxTarrif[2].TimeRus));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\"><Data ss:Type=\"String\">{0}</Data></Cell>", payways.MaxTarrif[2].DaysRus));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=R[0]C[10]\"><Data ss:Type=\"Number\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=R[0]C[11]\"><Data ss:Type=\"Number\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=RC[-3]-RC[-2]\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeAcross=\"4\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"={0}\"><Data ss:Type=\"String\"></Data></Cell>", sO));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"={0}\"><Data ss:Type=\"String\"></Data></Cell>", sO));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine("  </Row>");
            sw.WriteLine("  <Row>");
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:Index=\"3\" ss:StyleID=\"col1r\"><Data ss:Type=\"String\">{0}</Data></Cell>", payways.MaxTarrif[3].TimeRus));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\"><Data ss:Type=\"String\">{0}</Data></Cell>", payways.MaxTarrif[3].DaysRus));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=R[0]C[11]\"><Data ss:Type=\"Number\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=R[0]C[12]\"><Data ss:Type=\"Number\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col1r\" ss:Formula=\"=RC[-3]-RC[-2]\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeAcross=\"4\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"={0}\"><Data ss:Type=\"String\"></Data></Cell>", sO));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\"><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:StyleID=\"col0\" ss:Formula=\"={0}\"><Data ss:Type=\"String\"></Data></Cell>", sO));
            sw.WriteLine("  </Row>");

            sw.WriteLine("  <Row>");
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine("  </Row>");

            sw.WriteLine("  <Row>");
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeAcross=\"11\"><Data ss:Type=\"String\">Grab Url: {0}</Data></Cell>", payways.url));
            sw.WriteLine("  </Row>");
            sw.WriteLine("  <Row>");
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeAcross=\"11\"><Data ss:Type=\"String\">ƒ‡ÌÌ˚Â ÔÓÎÛ˜ÂÌ˚: {0}</Data></Cell>", payways.grabbed.ToString("dd.MM.yyyy HH:mm")));
            sw.WriteLine("  </Row>");
            sw.WriteLine("  <Row>");
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell><Data ss:Type=\"String\">{0}</Data></Cell>", ""));
            sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture, "    <Cell ss:MergeAcross=\"11\"><Data ss:Type=\"String\">Created by: {0}</Data></Cell>", "Avtodor-Tr M4 Grabber by milokz@gmail.com"));
            sw.WriteLine("  </Row>");

            sw.WriteLine("</Table>");
            sw.WriteLine("<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">");
            sw.WriteLine("  <FreezePanes/>");
            sw.WriteLine("  <FrozenNoSplit/>");
            sw.WriteLine("  <SplitHorizontal>2</SplitHorizontal>");
            sw.WriteLine("  <TopRowBottomPane>2</TopRowBottomPane>");
            sw.WriteLine("  <SplitVertical>1</SplitVertical>");
            sw.WriteLine("  <LeftColumnRightPane>1</LeftColumnRightPane>");
            sw.WriteLine("  <ActivePane>0</ActivePane>");
            sw.WriteLine("</WorksheetOptions>");
            sw.WriteLine("</Worksheet>");
        }

        public static void Export2ExcelXML(PayWays payways, string fname)
        {
            FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\"?>");
            sw.WriteLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sw.WriteLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            sw.WriteLine("<DocumentProperties>\r\n  <Created>Avtodor-Tr M4 Grabber by milokz@gmail.com</Created>\r\n</DocumentProperties>");
            sw.WriteLine("<Styles>");
            Export2ExcelXML(payways, sw);
            sw.WriteLine("</Workbook>");
            sw.Close();
            fs.Close();
        }
    }

    public class PayWay
    {
        [XmlAttribute("id")]
        public string ID = String.Empty;
        [XmlAttribute("name")]
        public string Name = String.Empty;
        [XmlElement("tarrifs")]
        public List<Tarrif> Tarrifs = new List<Tarrif>();
    }

    public class PayWaySorter : IComparer<PayWay>
    {
        public int Compare(PayWay a, PayWay b)
        {
            string va = a.ID;
            string vb = b.ID;
            Regex ex = new Regex(@"[\d]+");
            if (ex.Match(va).Success)
                va = int.Parse(ex.Match(va).Value).ToString("00000") + va.Replace(ex.Match(va).Value, "");
            if (ex.Match(va).Success)
                vb = int.Parse(ex.Match(vb).Value).ToString("00000") + vb.Replace(ex.Match(vb).Value, "");
            return va.CompareTo(vb);
        }
    }

    public class Tarrif
    {
        public static string[] DaysOfWeek = new string[] { "œÕ", "¬“", "—–", "◊“", "œ“", "—¡", "¬—" };

        [XmlAttribute("time")]
        public string Time = String.Empty;
        [XmlAttribute("cost")]
        public float[] Costs = new float[16];
        [XmlAttribute("days")]
        public string Days = "1,2,3,4,5,6,7";

        [XmlIgnore]
        public string TimeRus
        {
            get
            {
                if (Time == "NIGHT") return "ÕŒ◊‹";
                if (Time == "DAY") return "ƒ≈Õ‹";
                return "???";
            }
        }

        [XmlIgnore]
        public string DaysRus
        {
            get
            {
                if (String.IsNullOrEmpty(this.Days)) return "???";
                List<string> dd = new List<string>(new string[] { "¬—", "œÕ", "¬“", "—–", "◊“", "œ“", "—¡" });
                string[] d = this.Days.Split(new char[] { ',' }, 7);
                int pd = Convert.ToInt32(d[0]) - 1;
                string res = dd[pd];
                int lc = 0;
                int cd = pd;
                for (int i = 1; i < d.Length; i++)
                {
                    cd = Convert.ToInt32(d[i]) - 1;
                    if (((cd - pd) == 1) || ((cd == 0) && (pd == 6)))
                        lc++;
                    else
                    {
                        if (lc == 0) res += "," + dd[cd];
                        if (lc == 1) res += "," + dd[pd] + "," + dd[cd];
                        if (lc > 1) res += "-" + dd[pd] + "," + dd[cd];
                        lc = 0;
                    };
                    pd = cd;
                };
                if (lc == 1) res += "," + dd[cd];
                if (lc > 1) res += "-" + dd[cd];
                return res;
            }
        }

        public Tarrif Clone()
        {
            Tarrif res = new Tarrif();
            res.Time = this.Time;
            res.Costs = (new List<float>(this.Costs)).ToArray();
            return res;
        }

        public void AddCost(string txt, ref int catub)
        {
            if (String.IsNullOrEmpty(txt)) return;
            Regex rx = new Regex(@"(?<cost>\d+[.,\d]+)");
            Match mx = rx.Match(txt);
            if (mx.Success)
            {
                float val = 0;
                if (float.TryParse(mx.Groups["cost"].Value.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
                    this.Costs[catub] = val;
                catub++;
            };
        }

        public float GetCost(byte cat, bool transponder)
        {
            if (transponder)
                return Math.Min(this.Costs[cat * 2 - 2], this.Costs[cat * 2 - 1]);
            else
                return Math.Max(this.Costs[cat * 2 - 2], this.Costs[cat * 2 - 1]);
        }

        public bool HasDay(byte day)
        {
            return Days.IndexOf(day.ToString()) >= 0;
        }

        public bool HasDay(string day)
        {
            List<string> di = new List<string>(new string[] { "¬—", "œÕ", "¬“", "—–", "◊“", "œ“", "—¡" });
            int dir = Days.IndexOf(di.IndexOf(day.ToUpper()).ToString());
            List<string> ei = new List<string>(new string[] { "SU", "MO", "TU", "WE", "TH", "FR", "SA" });
            int eir = Days.IndexOf(di.IndexOf(day.ToUpper()).ToString());
            return Math.Max(dir, eir) >= 0;
        }

        public bool HasTime(DateTime time)
        {
            if (String.IsNullOrEmpty(this.Time)) return true;
            string dd = this.Time.Replace(" ", "").ToUpper().Trim();
            Regex rx = new Regex(@"(?<from>\w{1,2}:\w{1,2})-(?<to>\w{1,2}:\w{1,2})");
            Match mc = rx.Match(dd);
            if (!mc.Success) return true;
            string tf = mc.Groups["from"].Value;
            string tt = mc.Groups["to"].Value;
            DateTime dtf = DateTime.ParseExact(tf, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            DateTime dtt = DateTime.ParseExact(tt, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            time = new DateTime(dtf.Year, dtf.Month, dtf.Day, time.Hour, time.Minute, time.Second);
            List<DateTime[]> intervals = new List<DateTime[]>();
            if (dtt > dtf)
                intervals.Add(new DateTime[] { dtf, dtt });
            else
            {
                intervals.Add(new DateTime[] { dtf, dtf.Date.AddDays(1) });
                intervals.Add(new DateTime[] { dtt.Date, dtt });
            };
            foreach (DateTime[] dti in intervals)
                if ((dti[0] <= time) && (time <= dti[1]))
                    return true;
            return false;
        }

        public bool HasTime(double time)
        {
            return HasTime(DateTime.FromOADate(time));
        }
    }

    public class TarifSorter : IComparer<Tarrif>
    {
        //public int Compare(Tarrif a, Tarrif b)
        //{
        //    return (a.Day + a.Time).CompareTo(b.Day + b.Time);
        //}
        public int Compare(Tarrif a, Tarrif b)
        {
            return -1 * (a.Days + " - " + a.Time).CompareTo(b.Days + " - " + b.Time);
            //return -1 * (a.Time + " - " + a.Days).CompareTo(b.Time + " - " + b.Days);
        }
    }
}