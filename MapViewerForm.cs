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
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows;

namespace KMZRebuilder
{
    public partial class ContentViewer : Form
    {
        private GetRouter groute = null;
        private WaitingBoxForm wbf = null;        
        public NaviMapNet.MapLayer mapContent = null;
        public NaviMapNet.MapLayer mapSelect = null;
        public ToolTip mapTootTip = new ToolTip();
        private KMZRebuilederForm parent = null;
        private bool firstboot = true;

        public NaviMapNet.MapLayer mapRoute = null;
        public NaviMapNet.MapPoint mapRStart = null;
        public NaviMapNet.MapPoint mapRFinish = null;
        public NaviMapNet.MapPolyLine mapRVector = null;
        private MultiPointRouteForm mapRMulti = null;

        private string SASPlanetCacheDir = @"C:\Program Files\SASPlanet\cache\osmmapMapnik";
        private string UserDefindedUrl = @"http://tile.openstreetmap.org/{z}/{x}/{y}.png";
        private string UserDefindedFile = @"C:\nofile.mbtiles";

        MruList mru1;
        State state;

        public ContentViewer(KMZRebuilederForm parent)
        {
            this.parent = parent;
            Init();
            PastInit();
            LoadXUN();
        } 

        public ContentViewer(KMZRebuilederForm parent, WaitingBoxForm waitBox)
        {
            this.parent = parent;
            this.wbf = waitBox;
            Init();
            PastInit();
            LoadXUN();
        }

        private void PastInit()
        {
            ToolStripMenuItem mi = new ToolStripMenuItem("Select None");
            mi.Click += new EventHandler(selectNoneToolStripMenuItem_Click);
            mi.ShortcutKeys = Keys.N | Keys.Control;
            MapViewer.AddItemToDefaultMenu(mi);
            MapViewer.AddItemToDefaultMenu(new ToolStripSeparator());
            mi = new ToolStripMenuItem("Switch to Constructor Mode");
            mi.Click += new EventHandler(mcme_Click);
            mi.ShortcutKeys = Keys.F3;
            MapViewer.AddItemToDefaultMenu(mi);
        }

        private List<XUN> xuns = new List<XUN>();
        private void LoadXUN()
        {
            string fn = KMZRebuilederForm.CurrentDirectory() + @"\Map_Places.txt";
            if (!File.Exists(fn)) return;
            FileStream fs = new FileStream(fn, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.StartsWith("#")) continue;
                if (line.StartsWith("@")) continue;
                if (line.Length < 5) continue;
                string[] xyn = line.Split(new char[] { ' ' }, 3);
                try
                {
                    double la = double.Parse(xyn[0], System.Globalization.CultureInfo.InvariantCulture);
                    double lo = double.Parse(xyn[1], System.Globalization.CultureInfo.InvariantCulture);
                    xuns.Add(new XUN(xyn[2], la, lo));
                }
                catch { };
            };
            sr.Close();
            fs.Close();
        }

        public class XUN
        {
            public double lat;
            public double lon;
            public string nam;

            public XUN(string nam, double lat, double lon)
            {
                this.lat = lat;
                this.lon = lon;
                this.nam = nam;
            }

            public override string ToString()
            {
                return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} ({1:0.000000} {2:0.000000})", nam, lat, lon);
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
            public MapStore(string Name, string Url, NaviMapNet.NaviMapNetViewer.MapServices Service)
            {
                this.Name = Name;
                this.Url = Url;
                this.Service = Service;
            }
        }

        private void Init()
        {
            InitializeComponent();

            string fn = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.stt";
            if (File.Exists(fn)) state = State.Load(fn);

            mru1 = new MruList(KMZRebuilederForm.CurrentDirectory()+@"\KMZRebuilder.drs", spcl, 10);
            mru1.FileSelected += new MruList.FileSelectedEventHandler(mru1_FileSelected);

            mapTootTip.ShowAlways = true;

            mapRoute = new NaviMapNet.MapLayer("mapRoute");
            MapViewer.MapLayers.Add(mapRoute);
            mapSelect = new NaviMapNet.MapLayer("mapSelect");
            MapViewer.MapLayers.Add(mapSelect);
            mapContent = new NaviMapNet.MapLayer("mapContent");
            MapViewer.MapLayers.Add(mapContent);

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

            MapViewer.NotFoundTileColor = Color.LightYellow;
            MapViewer.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Custom_LocalFiles;
            MapViewer.ImageSourceUrl = @"C:\Program Files\SASPlanet\cache\osmmapMapnik\";
            MapViewer.WebRequestTimeout = 10000;
            MapViewer.ZoomID = 10;
            MapViewer.OnMapUpdate = new NaviMapNet.NaviMapNetViewer.MapEvent(MapUpdate);

            MapViewer.UserDefinedGetTileUrl = new NaviMapNet.NaviMapNetViewer.GetTilePathCall(UserDefinedGetTileUrl);    
            
            //MapViewer.DrawMap = true;
            //MapViewer.ReloadMap();

            //iStorages.SelectedIndex = iStorages.Items.Count - 2;    

            if (state != null)
            {
                SASPlanetCacheDir = state.SASDir;
                UserDefindedUrl = state.URL;
                UserDefindedFile = state.FILE;

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

        private string UserDefinedGetTileUrl(int x, int y, int z)
        {
            if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
                return SASPlanetCache(x, y, z + 1);
            return "";
        }

        private void iStorages_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapStore iS = (MapStore)iStorages.SelectedItem;

            MapViewer.ImageSourceService = iS.Service;
            MapViewer.ImageSourceType = iS.Source;
            MapViewer.ImageSourceProjection = iS.Projection;

            if (iStorages.SelectedIndex < (iStorages.Items.Count - 1))
            {
                MapViewer.UseDiskCache = true;
                MapViewer.UserDefinedMapName = iS.CacheDir;

                if (iStorages.SelectedIndex == (iStorages.Items.Count - 2))
                    MapViewer.ImageSourceUrl = UserDefindedUrl;
                else if (iStorages.SelectedIndex == (iStorages.Items.Count - 3))
                {
                    MapViewer.UseDiskCache = false;
                    MapViewer.ImageSourceUrl = UserDefindedFile;
                }
                else
                    MapViewer.ImageSourceUrl = iS.Url;
            };

            if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
            {
                MapViewer.UseDiskCache = false;
                MapViewer.UserDefinedMapName = iS.CacheDir = @"LOCAL\" + SASPlanetCacheDir.Substring(SASPlanetCacheDir.LastIndexOf(@"\") + 1);
                MapViewer.ImageSourceUrl = SASPlanetCacheDir;
            };

            iStorages.Refresh();
            MapViewer.ReloadMap();
        }

        private void MapUpdate()
        {
            string lreq = MapViewer.LastRequestedFile;
            if (lreq.Length > 70) lreq = "... " + lreq.Substring(lreq.Length - 70);            

            toolStripStatusLabel1.Text = "Last Requested File: " + lreq;
            toolStripStatusLabel2.Text = MapViewer.CenterDegreesLat.ToString().Replace(",", ".");
            toolStripStatusLabel3.Text = MapViewer.CenterDegreesLon.ToString().Replace(",", ".");

            string regNm = "...";
            if (MapViewer.ZoomID > 7)
            {
                int regNo = KMZRebuilederForm.PIRU.PointInRegion(MapViewer.CenterDegreesY, MapViewer.CenterDegreesX);
                regNm = regNo > 0 ? KMZRebuilederForm.PIRU.GetRegionName(regNo) : "...";
            };
            RegName.Text = regNm;
        }

        private Timer mmTimer = null;
        private bool locate = false;
        private void MapViewer_MouseMove(object sender, MouseEventArgs e)
        {
            locate = false;
            if (e.Button != MouseButtons.None) mapTootTip.Hide(this);

            if (mmTimer != null)
                mmTimer.Enabled = false;
            else
            {
                mmTimer = new Timer();
                mmTimer.Interval = 800;
                mmTimer.Tick += new EventHandler(mmTimer_Tick);
            };
            mmTimer.Start();
            
            PointF m = MapViewer.MousePositionDegrees;
            toolStripStatusLabel4.Text = m.Y.ToString().Replace(",", ".");
            toolStripStatusLabel5.Text = m.X.ToString().Replace(",", ".");
        }

        private void mmTimer_Tick(object sender, EventArgs e)
        {
            mmTimer.Enabled = false;
            if (mapContent.ObjectsCount == 0) return;

            try
            {
                Point f = this.PointToScreen(new Point(0, 0));
                Point p = Cursor.Position;
                Point s = new Point(p.X - f.X, p.Y - f.Y);

                Point current = MapViewer.MousePositionPixels;
                PointF sCenter = MapViewer.PixelsToDegrees(current);
                PointF sFrom = MapViewer.PixelsToDegrees(new Point(current.X - 5, current.Y + 5));
                PointF sTo = MapViewer.PixelsToDegrees(new Point(current.X + 5, current.Y - 5));
                NaviMapNet.MapObject[] objs = mapContent.Select(new RectangleF(sFrom, new SizeF(sTo.X - sFrom.X, sTo.Y - sTo.X)));
                if ((objs != null) && (objs.Length > 0))
                {
                    uint len = uint.MaxValue;
                    int ind = 0;
                    for (int i = 0; i < objs.Length; i++)
                    {
                        uint tl = GetLengthMetersC(sCenter.Y, sCenter.X, objs[i].Center.Y, objs[i].Center.X, false);
                        if (tl < len) { len = tl; ind = i; };
                    };

                    mapTootTip.Show(objs[ind].Name, this, s.X, s.Y, 5000);
                }
                else
                    mapTootTip.Hide(this);
            }
            catch { };
        }

        private void objects_DoubleClick(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            
            NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
            if ((mo is NaviMapNet.MapPolyLine) || (mo is NaviMapNet.MapPolygon))
            {
                if (mo is NaviMapNet.MapPolyLine)
                {
                    if ((mo.Bounds.Width > MapViewer.MapBoundsRectDegrees.Width) || (mo.Bounds.Height > MapViewer.MapBoundsRectDegrees.Height))
                        MapViewer.CenterDegrees = mo.Points[0];
                    else
                        MapViewer.ZoomByArea((mo as NaviMapNet.MapPolyLine).Bounds, MapViewer.ZoomID);
                }
                else
                {
                    byte nextZoom = MapViewer.ZoomID;
                    if ((mo.Bounds.Width > MapViewer.MapBoundsRectDegrees.Width) || (mo.Bounds.Height > MapViewer.MapBoundsRectDegrees.Height))
                    {
                        int pow = (int)Math.Round(Math.Max(mo.Bounds.Width / MapViewer.MapBoundsRectDegrees.Width, mo.Bounds.Height / MapViewer.MapBoundsRectDegrees.Height));
                        nextZoom = (byte)(nextZoom - pow);
                        if (nextZoom < 2) nextZoom = 2;
                        if (nextZoom > 20) nextZoom = 2;
                    };
                    MapViewer.ZoomByArea((mo as NaviMapNet.MapPolygon).Bounds, nextZoom);
                };
            }
            else
            {
                double[] b = MapViewer.MapBoundsMinMaxDegrees;
                if((mo.Points[0].X < b[0]) || (mo.Points[0].Y < b[1]) || (mo.Points[0].X > b[2]) || (mo.Points[0].Y > b[3]))
                    MapViewer.CenterDegrees = mo.Points[0];
            };
            SelectOnMap(objects.SelectedIndices[0]);
        }

        Dictionary<string, string> style2image = new Dictionary<string, string>();
        private int prev_selected = -1;
        private void laySelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            List<int> selected_to_del = new List<int>();
            if(prev_selected == laySelect.SelectedIndex)
            {
                if (objects.Items.Count > 0)
                    for (int i = 0; i < objects.Items.Count; i++)
                        if (objects.Items[i].SubItems[6].Text == "Yes")
                            selected_to_del.Add(i);
            }
            else
                mapSelect.Clear();
            prev_selected = laySelect.SelectedIndex;

            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            images.Images.Clear();
            objects.Items.Clear();
            mapContent.Clear();            
            

            Hashtable imList = new Hashtable();

            if (true)
            {
                KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
                XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

                int el_line = 0;
                int el_polygon = 0;
                int el_point = 0;
                XmlNodeList xnf = xn.SelectNodes("Placemark");
                if (xnf.Count > 0)
                    for (int el = 0; el < xnf.Count; el++)
                    {
                        if (el % 100 == 0)
                        {
                            toolStripStatusLabel1.Text = String.Format("Loading {0} of {1} placemarks", el, xnf.Count);
                            statusStrip2.Refresh();
                        };
                        if ((wbf != null) && (el % 100 == 0)) wbf.Text = String.Format("Loading {0} of {1} placemarks", el, xnf.Count);

                        if (xnf[el].ChildNodes.Count == 0) continue;

                        if (xnf[el].SelectNodes("LineString").Count > 0) // ++LINE
                        {
                            XmlNode xnn = xnf[el].SelectNodes("LineString/coordinates")[0];

                            string[] llza = xnn.ChildNodes[0].Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            string name = "NoName";
                            try { name = xnn.ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value; }
                            catch { };
                            string description = "";
                            try { description = xnn.ParentNode.ParentNode.SelectSingleNode("description").ChildNodes[0].Value; }
                            catch { };

                            string styleUrl = "";
                            if (xnn.ParentNode.ParentNode.SelectSingleNode("styleUrl") != null) styleUrl = xnn.ParentNode.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                            if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);

                            Color lineColor = Color.FromArgb(255, Color.Blue);
                            int lineWidth = 3;

                            XmlNode sn = null;
                            if (styleUrl != "")
                            {
                                string firstsid = styleUrl;
                                XmlNodeList pk = l.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']/Pair/key");
                                if (pk.Count > 0)
                                    for (int n = 0; n < pk.Count; n++)
                                    {
                                        XmlNode cn = pk[n];
                                        if ((cn.ChildNodes[0].Value != "normal") && (n > 0)) continue;
                                        if (cn.ParentNode.SelectSingleNode("styleUrl") == null) continue;
                                        firstsid = cn.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                                        if (firstsid.IndexOf("#") == 0) firstsid = firstsid.Remove(0, 1);
                                    };
                                try
                                {
                                    sn = l.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']/LineStyle");
                                }
                                catch { };
                            }
                            else
                                sn = xnn.ParentNode.ParentNode.SelectSingleNode("Style/LineStyle");
                            if (sn != null)
                            {
                                string colval = sn.SelectSingleNode("color").ChildNodes[0].Value;
                                try
                                {
                                    lineColor = Color.FromName(colval);
                                    if (colval.Length == 8)
                                    {
                                        lineColor = Color.FromArgb(
                                            Convert.ToInt32(colval.Substring(0, 2), 16),
                                            Convert.ToInt32(colval.Substring(6, 2), 16),
                                            Convert.ToInt32(colval.Substring(4, 2), 16),
                                            Convert.ToInt32(colval.Substring(2, 2), 16)
                                            );
                                    };
                                }
                                catch { };
                                string widval = sn.SelectSingleNode("width").ChildNodes[0].Value;
                                try
                                {
                                    lineWidth = (int)double.Parse(widval, ni);
                                    if (lineWidth < 3) lineWidth = 3;
                                }
                                catch { };
                            };

                            List<PointF> xy = new List<PointF>();
                            foreach (string llzix in llza)
                            {
                                string llzi = llzix.Trim('\r').Trim('\n');
                                if (String.IsNullOrEmpty(llzi)) continue;
                                string[] llz = llzi.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                xy.Add(new PointF(float.Parse(llz[0], ni), float.Parse(llz[1], ni)));
                            };

                            NaviMapNet.MapPolyLine ml = new NaviMapNet.MapPolyLine(xy.ToArray());
                            ml.Name = name;
                            ml.UserData = description;
                            ml.Color = lineColor;
                            ml.Width = lineWidth;

                            Image im = new Bitmap(16, 16);
                            Graphics g = Graphics.FromImage(im);
                            g.FillRectangle(new SolidBrush(lineColor), 0, 0, 16, 16);
                            g.DrawString("L", new Font("Terminal", 11, FontStyle.Bold), new SolidBrush(Color.FromArgb(255 - lineColor.R, 255 - lineColor.G, 255 - lineColor.B)), 1, -1);
                            g.Dispose();
                            images.Images.Add(im);

                            if (l.file.DrawEvenSizeIsTooSmall) ml.DrawEvenSizeIsTooSmall = true;
                            mapContent.Add(ml);
                            ListViewItem lvi = objects.Items.Add(ml.Name, images.Images.Count - 1);
                            lvi.SubItems.Add("Line (" + ml.PointsCount.ToString() + " points)");
                            lvi.SubItems.Add(ml.Points[0].Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            lvi.SubItems.Add(ml.Points[0].X.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add("Placemark/LineString/coordinates[" + el_line.ToString() + "]");
                            if (((el_point + el_polygon + el_line) == 0) && firstboot) MapViewer.CenterDegrees = ml.Points[0];

                            if (selected_to_del.IndexOf(lvi.Index) >= 0)
                            {
                                lvi.SubItems[6].Text = "Yes";
                                lvi.Font = new Font(lvi.Font, FontStyle.Strikeout);
                                mapContent[lvi.Index].Visible = false;
                            };

                            el_line++;
                        }; // --LINE

                        if (xnf[el].SelectNodes("Polygon").Count > 0) // ++Polygon
                        {
                            XmlNode xnn = xnf[el].SelectNodes("Polygon/outerBoundaryIs/LinearRing/coordinates")[0];

                            string[] llza = xnn.ChildNodes[0].Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            string name = "NoName";
                            try { name = xnn.ParentNode.ParentNode.ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value; }
                            catch { };
                            string description = "";
                            try { description = xnn.ParentNode.ParentNode.ParentNode.ParentNode.SelectSingleNode("description").ChildNodes[0].Value; }
                            catch { };

                            string styleUrl = "";
                            if (xnn.ParentNode.ParentNode.ParentNode.ParentNode.SelectSingleNode("styleUrl") != null) styleUrl = xnn.ParentNode.ParentNode.ParentNode.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                            if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);

                            Color lineColor = Color.FromArgb(255, Color.Blue);
                            int lineWidth = 3;
                            Color fillColor = Color.FromArgb(255, Color.Blue);
                            int fill = 1;

                            XmlNode sl = null;
                            XmlNode sf = null;
                            if (styleUrl != "")
                            {
                                string firstsid = styleUrl;
                                XmlNodeList pk = l.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']/Pair/key");
                                if (pk.Count > 0)
                                    for (int n = 0; n < pk.Count; n++)
                                    {
                                        XmlNode cn = pk[n];
                                        if ((cn.ChildNodes[0].Value != "normal") && (n > 0)) continue;
                                        if (cn.ParentNode.SelectSingleNode("styleUrl") == null) continue;
                                        firstsid = cn.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                                        if (firstsid.IndexOf("#") == 0) firstsid = firstsid.Remove(0, 1);
                                    };
                                try
                                {
                                    sl = l.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']/LineStyle");
                                }
                                catch { };
                                try
                                {
                                    sf = l.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']/PolyStyle");
                                }
                                catch { };
                            }
                            else
                            {
                                sl = xnn.ParentNode.ParentNode.SelectSingleNode("Style/LineStyle");
                                sf = xnn.ParentNode.ParentNode.SelectSingleNode("Style/PolyStyle");
                            };
                            if (sl != null)
                            {
                                string colval = sl.SelectSingleNode("color").ChildNodes[0].Value;
                                try
                                {
                                    lineColor = Color.FromName(colval);
                                    if (colval.Length == 8)
                                    {
                                        lineColor = Color.FromArgb(
                                            Convert.ToInt32(colval.Substring(0, 2), 16),
                                            Convert.ToInt32(colval.Substring(6, 2), 16),
                                            Convert.ToInt32(colval.Substring(4, 2), 16),
                                            Convert.ToInt32(colval.Substring(2, 2), 16)
                                            );
                                    };
                                }
                                catch { };
                                string widval = sl.SelectSingleNode("width").ChildNodes[0].Value;
                                try
                                {
                                    lineWidth = (int)double.Parse(widval, ni);
                                    if (lineWidth < 2)
                                        lineWidth = 2;
                                }
                                catch { };
                            };
                            if (sf != null)
                            {
                                string colval = sf.SelectSingleNode("color").ChildNodes[0].Value;
                                try
                                {
                                    fillColor = Color.FromName(colval);
                                    if (colval.Length == 8)
                                    {
                                        fillColor = Color.FromArgb(
                                            Convert.ToInt32(colval.Substring(0, 2), 16),
                                            Convert.ToInt32(colval.Substring(6, 2), 16),
                                            Convert.ToInt32(colval.Substring(4, 2), 16),
                                            Convert.ToInt32(colval.Substring(2, 2), 16)
                                            );
                                    };
                                }
                                catch { };
                                string fillval = sf.SelectSingleNode("fill").ChildNodes[0].Value;
                                try
                                {
                                    fill = int.Parse(fillval, ni);
                                }
                                catch { };
                            };

                            List<PointF> xy = new List<PointF>();
                            foreach (string llzix in llza)
                            {
                                string llzi = llzix.Trim('\r').Trim('\n');
                                if (String.IsNullOrEmpty(llzi)) continue;
                                string[] llz = llzi.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                xy.Add(new PointF(float.Parse(llz[0], ni), float.Parse(llz[1], ni)));
                            };

                            NaviMapNet.MapPolygon mp = new NaviMapNet.MapPolygon(xy.ToArray());
                            mp.Name = name;
                            mp.UserData = description;
                            mp.BorderColor = lineColor;
                            mp.Width = lineWidth;
                            mp.BodyColor = Color.FromArgb(0, fillColor);
                            if (fill != 0)
                                mp.BodyColor = fillColor;

                            Image im = new Bitmap(16, 16);
                            Graphics g = Graphics.FromImage(im);
                            g.FillRectangle(new SolidBrush(fillColor), 0, 0, 16, 16);
                            g.DrawRectangle(new Pen(new SolidBrush(lineColor), 2), 0, 0, 16, 16);
                            g.DrawString("A", new Font("Terminal", 11, FontStyle.Bold), new SolidBrush(Color.FromArgb(255 - fillColor.R, 255 - fillColor.G, 255 - fillColor.B)), 1, -1);
                            g.Dispose();
                            images.Images.Add(im);

                            if (l.file.DrawEvenSizeIsTooSmall) mp.DrawEvenSizeIsTooSmall = true;
                            mapContent.Add(mp);
                            ListViewItem lvi = objects.Items.Add(mp.Name, images.Images.Count - 1);
                            lvi.SubItems.Add("Polygon (" + mp.PointsCount.ToString() + " points)");
                            lvi.SubItems.Add(mp.Center.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            lvi.SubItems.Add(mp.Center.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add("Placemark/Polygon/outerBoundaryIs/LinearRing/coordinates[" + el_polygon.ToString() + "]");
                            if (((el_point + el_polygon + el_line) == 0) && firstboot) MapViewer.CenterDegrees = mp.Center;

                            if (selected_to_del.IndexOf(lvi.Index) >= 0)
                            {
                                lvi.SubItems[6].Text = "Yes";
                                lvi.Font = new Font(lvi.Font, FontStyle.Strikeout);
                                mapContent[lvi.Index].Visible = false;
                            };

                            el_polygon++;
                        }; // --Polygon

                        if (xnf[el].SelectNodes("Point").Count > 0) // ++Point
                        {
                            XmlNode xnn = xnf[el].SelectNodes("Point/coordinates")[0];

                            string[] llz = xnn.ChildNodes[0].Value.Replace("\r", "").Replace("\n", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            string name = "NoName";
                            try { name = xnn.ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value; }
                            catch { };
                            string description = "";
                            try { description = xnn.ParentNode.ParentNode.SelectSingleNode("description").ChildNodes[0].Value; }
                            catch { };

                            string styleUrl = "";
                            string href = "";
                            try
                            {
                                if (xnn.ParentNode.ParentNode.SelectSingleNode("styleUrl") != null) styleUrl = xnn.ParentNode.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                                if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);
                            }
                            catch { };

                            if (styleUrl != "")
                            {
                                string firstsid = styleUrl;
                                XmlNodeList pk = l.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']/Pair/key");
                                if (pk.Count > 0)
                                    for (int n = 0; n < pk.Count; n++)
                                    {
                                        XmlNode cn = pk[n];
                                        if ((cn.ChildNodes[0].Value != "normal") && (n > 0)) continue;
                                        if (cn.ParentNode.SelectSingleNode("styleUrl") == null) continue;
                                        firstsid = cn.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                                        if (firstsid.IndexOf("#") == 0) firstsid = firstsid.Remove(0, 1);
                                    };
                                try
                                {
                                    XmlNode nts = l.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']/IconStyle/Icon/href");
                                    href = nts.ChildNodes[0].Value;
                                    if (!style2image.ContainsKey("#" + firstsid))
                                        style2image.Add("#" + firstsid, href);
                                }
                                catch { };
                            };

                            NaviMapNet.MapPoint mp = new NaviMapNet.MapPoint(double.Parse(llz[1], ni), double.Parse(llz[0], ni));
                            mp.Name = name;
                            mp.UserData = description;
                            mp.SizePixels = new Size(16, 16);
                            int ii = -1;
                            if (imList.ContainsKey(href))
                                ii = (int)imList[href];
                            else
                            {
                                if (href == "")
                                    imList.Add(href, -1);
                                else
                                {
                                    Image im = null;
                                    if (Uri.IsWellFormedUriString(href, UriKind.Absolute))
                                    {
                                        System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(href);

                                        try
                                        {
                                            using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
                                            using (Stream stream = response.GetResponseStream())
                                                im = Bitmap.FromStream(stream);
                                        }
                                        catch
                                        { im = null; };
                                    }
                                    else
                                    {
                                        try { im = Image.FromFile(l.file.tmp_file_dir + href); }
                                        catch { im = null; };
                                    };

                                    if (im != null)
                                    {
                                        images.Images.Add(href, (Image)new Bitmap(im));
                                        im.Dispose();
                                        imList.Add(href, ii = images.Images.Count - 1);
                                    }
                                    else
                                        imList.Add(href, ii = -1);
                                };
                            };
                            if (ii >= 0)
                            {
                                mp.Color = Color.Transparent;
                                mp.Squared = true;
                                mp.Img = images.Images[ii];
                            }
                            else
                            {
                                mp.Color = Color.Purple;
                                mp.Squared = false;
                            };

                            mapContent.Add(mp);
                            ListViewItem lvi = objects.Items.Add(String.Format("{0}", mp.Name, mp.Center.Y.ToString(System.Globalization.CultureInfo.InvariantCulture), mp.Center.X.ToString(System.Globalization.CultureInfo.InvariantCulture)), ii);
                            lvi.SubItems.Add("Point");
                            lvi.SubItems.Add(mp.Center.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            lvi.SubItems.Add(mp.Center.X.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add("");
                            lvi.SubItems.Add("Placemark/Point/coordinates[" + el_point.ToString() + "]");
                            if (((el_point + el_polygon + el_line) == 0) && firstboot) MapViewer.CenterDegrees = mp.Center;

                            if (selected_to_del.IndexOf(lvi.Index) >= 0)
                            {
                                lvi.SubItems[6].Text = "Yes";
                                lvi.Font = new Font(lvi.Font, FontStyle.Strikeout);
                                mapContent[lvi.Index].Visible = false;
                            };

                            el_point++;
                        }; // --Point
                    };
            };

            toolStripStatusLabel1.Text = "All placemarks loaded";
            statusStrip2.Refresh();

            NPB.Enabled = false;
            NNB.Enabled = false;

            laySelect.Enabled = selected_to_del.Count == 0;
            MapViewer.DrawOnMapData();
            firstboot = false;
            UpdateCheckedAndMarked(true);            
        }

        private void FindCopies(int toIndex, bool xy4, bool nm5, Single distanceInMeters)
        {
            if (objects.Items.Count == 0) return;

            Color[] colors = new Color[] { 
                Color.LightBlue, Color.LightCoral, Color.LightCyan, Color.LightGray, Color.LightGreen, 
                Color.LightPink, Color.LightSalmon, Color.LightSeaGreen, Color.LightSkyBlue, Color.LightSteelBlue, 
                Color.LightYellow, Color.Lime, Color.LimeGreen, Color.Orange, Color.OrangeRed, 
                Color.Pink, Color.RoyalBlue, Color.SeaGreen, Color.SeaShell, Color.SkyBlue, 
                Color.Tan, Color.YellowGreen};

            int simIndex = 0;

            Dictionary<string,int[]> copies = new Dictionary<string,int[]>(); // all combinations store

            for (int i = 0; i < objects.Items.Count; i++)
            {

                objects.Items[i].BackColor = objects.BackColor;
                objects.Items[i].SubItems[4].Text = "";
                objects.Items[i].SubItems[5].Text = "";
            };

            int iFrom = 0;
            int iTo = mapContent.ObjectsCount - 1;
            if (toIndex >= 0) { iFrom = toIndex; iTo = toIndex; };
            for (int i = iFrom; i <= iTo; i++)
                for(int j=0;j<mapContent.ObjectsCount;j++)
                    if (i != j)
                    {
                        NaviMapNet.MapObject a = mapContent[i];
                        NaviMapNet.MapObject b = mapContent[j];
                        bool same = (!nm5) || (a.Name.Trim().ToLower() == b.Name.Trim().ToLower());
                        if (xy4 && same)
                        {
                            same = false;
                            if (a.PointsCount == b.PointsCount)
                            {
                                same = true;
                                for (int n = 0; n < a.PointsCount; n++)
                                {
                                    if (distanceInMeters <= 0)
                                    {
                                        if (a.Points[n].X != b.Points[n].X) { same = false; break; };
                                        if (a.Points[n].Y != b.Points[n].Y) { same = false; break; }
                                    }
                                    else
                                    {
                                        float dist = Utils.GetLengthMeters(a.Points[n].Y, a.Points[n].X, b.Points[n].Y, b.Points[n].X, false);
                                        if (dist > distanceInMeters) 
                                        { same = false; break; };
                                    };
                                };                                
                            };
                        };
                        string key = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", a.Center.X, a.Center.Y);
                        if (nm5) key = a.Name.Trim().ToLower();
                        if (same)
                        {
                            int fex = -1;
                            int sex = -1;
                            if (copies.Count > 0)
                                foreach (KeyValuePair<string, int[]> kpv in copies)
                                {
                                    if (Array.IndexOf<int>(kpv.Value, i) >= 0) fex = kpv.Value[0];
                                    if (Array.IndexOf<int>(kpv.Value, j) >= 0) sex = kpv.Value[0];
                                };
                            if ((fex >= 0) && (sex >= 0) && (fex == sex))
                                continue; // combination exists

                            if (!copies.ContainsKey(key)) copies.Add(key, new int[] { simIndex++ }); // create new combination or add to existing
                            List<int> val = new List<int>();
                            val.AddRange(copies[key]);
                            if (val.IndexOf(i, 1) < 0) val.Add(i);
                            if (val.IndexOf(j, 1) < 0) val.Add(j);
                            copies[key] = val.ToArray();

                            int colIndex = val[0] % colors.Length;
                            objects.Items[i].BackColor = colors[colIndex];
                            objects.Items[j].BackColor = colors[colIndex];

                            if (nm5)
                            {
                                objects.Items[i].SubItems[5].Text = val[0].ToString();
                                objects.Items[j].SubItems[5].Text = val[0].ToString();
                            };
                            if (xy4)
                            {
                                if(objects.Items[i].SubItems[4].Text == "")
                                  objects.Items[i].SubItems[4].Text = val[0].ToString();
                                if(objects.Items[j].SubItems[4].Text == "")
                                  objects.Items[j].SubItems[4].Text = val[0].ToString();
                            };
                        };
                    };

            status.Text = "";
            int ttl = 0;
            if (copies.Count > 0)
            {
                NPB.Enabled = xy4;
                NNB.Enabled = nm5;

                foreach (KeyValuePair<string, int[]> kpv in copies)
                    ttl += (kpv.Value.Length - 2);
                if (xy4 && nm5)
                    status.Text += "Found " + ttl.ToString() + " combinations for " + copies.Count.ToString() + " placemarks\r\n";
                else if (xy4)
                    status.Text += "Found " + ttl.ToString() + " combinations for " + copies.Count.ToString() + " placemarks by coordinates\r\n";
                else if (nm5)
                    status.Text += "Found " + ttl.ToString() + " combinations for " + copies.Count.ToString() + " placemarks by name\r\n";
                if (ttl > objects.Items.Count)
                    status.Text += "Too many combinations! You must use less search radius!\r\n";
            }
            else
            {
                status.Text = "No copies found";
                NPB.Enabled = false;
                NNB.Enabled = false;
            };
            status.SelectionStart = status.TextLength;
            status.ScrollToCaret();
        }
        
        private void MapViewer_MouseClick(object sender, MouseEventArgs e)
        {            
            if (!locate) 
                return;

            Point clicked = MapViewer.MousePositionPixels;
            PointF sCenter = MapViewer.PixelsToDegrees(clicked);

            if (mapContent.ObjectsCount == 0)
            {
                SubClick(sCenter, null);
                return;
            };
            PointF sFrom = MapViewer.PixelsToDegrees(new Point(clicked.X - 5, clicked.Y + 5));
            PointF sTo = MapViewer.PixelsToDegrees(new Point(clicked.X + 5, clicked.Y - 5));
            NaviMapNet.MapObject[] objs = mapContent.Select(new RectangleF(sFrom, new SizeF(sTo.X - sFrom.X, sTo.Y - sFrom.Y)), NaviMapNet.MapObjectType.mEllipse | NaviMapNet.MapObjectType.mLine | NaviMapNet.MapObjectType.mPoint | NaviMapNet.MapObjectType.mPolygon | NaviMapNet.MapObjectType.mPolyline, true, false);
            if ((objs != null) && (objs.Length > 0))
            {
                uint len = uint.MaxValue;
                int ind = 0;
                for (int i = 0; i < objs.Length; i++)
                {
                    uint tl = GetLengthMetersC(sCenter.Y, sCenter.X, objs[i].Center.Y, objs[i].Center.X, false);
                    if (tl < len) { len = tl; ind = i; };
                };

                if ((objects.SelectedIndices.Count == 0) || (objects.SelectedIndices[0] != objs[ind].Index))
                {
                    objects.Items[objs[ind].Index].Selected = true;
                    objects.Items[objs[ind].Index].Focused = true;
                };

                SelectOnMap(objs[ind].Index);
                if(objs[ind].PointsCount == 1)
                    SubClick(new PointF(objs[ind].Center.X, objs[ind].Center.Y),objs[ind].Name);
                else
                    SubClick(sCenter, null);
            }
            else
            {
                SubClick(sCenter, null);
            };
        }        

        private static uint GetLengthMetersC(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
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

        private Color prevSIC = Color.White;
        private ListViewItem prevSII = null;
        private void objects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (prevSII != null)
            {
                prevSII.BackColor = prevSIC;
                prevSII = null;
            };
            if (mapSelect.ObjectsCount > 0)
            {
                mapSelect.Clear();
                MapViewer.DrawOnMapData();
            };
            if (objects.SelectedItems.Count == 0)
            {
                persquare.Visible = false;
                return;
            };
            persquare.Visible = false;

            objects.EnsureVisible(objects.SelectedIndices[0]);
            prevSII = objects.SelectedItems[0];
            prevSIC = objects.SelectedItems[0].BackColor;
            objects.SelectedItems[0].BackColor = Color.Red;

            NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
            textBox1.Text = mo.UserData.ToString().Replace("<br/>", "\r\n").Replace("<br>", "\r\n");
            if (mo is NaviMapNet.MapPolyLine)
            {
                uint len = PolyLineBuffer.PolyLineBufferCreator.GetDistInMeters(mo.Points, false);
                persquare.Text = "Length: " + (len < 1000 ? len.ToString() + " m" : ((double)len / 1000.0).ToString("0.00" + " km"));
                persquare.Visible = true;
            };
            if (mo is NaviMapNet.MapPolygon)
            {
                uint len = PolyLineBuffer.PolyLineBufferCreator.GetDistInMeters(mo.Points, true);
                double square = PolyLineBuffer.PolyLineBufferCreator.GetSquareInMeters(mo.Points);
                persquare.Text = "Perimeter: " + (len < 1000 ? len.ToString() + " m" : ((double)len / 1000.0).ToString("0.00" + " km"))
                    +
                    " / Square: " + square.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km2";
                persquare.Visible = true;
            };
        }

        private void SelectOnMap(int id)
        {
            if (id < 0) return;

            mapSelect.Clear();
            if (mapContent[id].ObjectType == NaviMapNet.MapObjectType.mPolyline)
            {
                NaviMapNet.MapPolyLine mp = new NaviMapNet.MapPolyLine(mapContent[id].Points);
                mp.Name = "Selected";
                mp.Color = Color.FromArgb(100, Color.Blue);
                mp.Width = (mapContent[id] as NaviMapNet.MapPolyLine).Width + 4;
                mapSelect.Add(mp);
                MapViewer.DrawOnMapData();
            };
            if (mapContent[id].ObjectType == NaviMapNet.MapObjectType.mPolygon)
            {
                NaviMapNet.MapPolygon mp = new NaviMapNet.MapPolygon(mapContent[id].Points);
                mp.Name = "Selected";
                mp.Color = Color.FromArgb(100, Color.Blue);
                mp.Width = (mapContent[id] as NaviMapNet.MapPolygon).Width + 4;
                mapSelect.Add(mp);
                MapViewer.DrawOnMapData();
            };
            if (mapContent[id].ObjectType == NaviMapNet.MapObjectType.mPoint)
            {
                NaviMapNet.MapPoint mp = new NaviMapNet.MapPoint(mapContent[id].Center);
                mp.Name = "Selected";
                mp.SizePixels = new Size(22, 22);
                mp.Squared = false;
                mp.Color = Color.Fuchsia;
                mapSelect.Add(mp);
                MapViewer.DrawOnMapData();
            };
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            SASPlacemarkConnector sc = new SASPlacemarkConnector();
            exportToSASPlanetToolStripMenuItem.Enabled =
                (objects.SelectedItems.Count > 0) && (sc.SASisOpen);
            importFromSASPlanetToolStripMenuItem.Enabled =
                sc.SASisOpen && sc.Visible;

            allItem.Enabled = objects.Items.Count > 0;
            addCopyOfPointToolStripMenuItem.Enabled =
                (objects.SelectedItems.Count > 0) && (objects.SelectedItems[0].SubItems[1].Text == "Point");

            renameToolStripMenuItem.Enabled = objects.SelectedItems.Count > 0;
            setNewIconToAllTheSameToolStripMenuItem.Enabled = objects.SelectedItems.Count > 0;
            changeIconToolStripMenuItem.Enabled = objects.SelectedItems.Count > 0;
            setNewIconFromListToolStripMenuItem.Enabled = objects.SelectedItems.Count > 0;
            setNewIconFromListToAllWithSameImagesToolStripMenuItem.Enabled = objects.SelectedItems.Count > 0;
            
            markAsSkipWhenSaveToolStripMenuItem.Enabled = objects.SelectedItems.Count > 0;
            if(objects.SelectedItems.Count > 0)
                markAsSkipWhenSaveToolStripMenuItem.Checked = objects.SelectedItems[0].SubItems[6].Text == "Yes";

            exPOlToolStripMenuItem.Enabled = false;
            savePolyToolStripMenuItem.Enabled = false;
            interpolateToolStripMenuItem.Enabled = false;
            openInTrackSplitterToolStripMenuItem.Enabled = false;
            inverseLineDirectionToolStripMenuItem.Enabled = false;
            getCRCOfImageToolStripMenuItem.Enabled = false;
            if (objects.SelectedItems.Count > 0)
            {
                //if (objects.SelectedItems[0].SubItems[1].Text.StartsWith("Polygon"))

                if (objects.SelectedItems[0].SubItems[1].Text != "Point")
                {
                    setNewIconFromListToolStripMenuItem.Enabled = false;
                    setNewIconFromListToAllWithSameImagesToolStripMenuItem.Enabled = false;
                    changeIconToolStripMenuItem.Enabled = false;
                    setNewIconToAllTheSameToolStripMenuItem.Enabled = false;
                    addCopyOfPointToolStripMenuItem.Enabled = false;
                    LPSB.Enabled = true;
                    exPOlToolStripMenuItem.Enabled = true;
                    savePolyToolStripMenuItem.Enabled = true;
                    interpolateToolStripMenuItem.Enabled = true;
                    if (objects.SelectedItems[0].SubItems[1].Text.StartsWith("Polygon"))
                        savePolyToolStripMenuItem.Text = "Save Polygon to File ...";
                    if (objects.SelectedItems[0].SubItems[1].Text.StartsWith("Line"))
                    {
                        openInTrackSplitterToolStripMenuItem.Enabled = true;
                        savePolyToolStripMenuItem.Text = "Convert to Polygon and Save to File ...";
                        inverseLineDirectionToolStripMenuItem.Enabled = true;
                    };
                }
                else
                {
                    getCRCOfImageToolStripMenuItem.Enabled = true;
                    LPSB.Enabled = false;
                };
            };
            
            removeOSMSpecifiedTagsFromDescriptionToolStripMenuItem.Enabled =
            removeDescriptionToolStripMenuItem.Enabled =
            cHBN.Enabled =
                cHBNIP.Enabled =
                    cHBNIL.Enabled =
                        cHBNIF.Enabled =
                            cHBDC.Enabled =
                                cHB3.Enabled = 
                                    cHB2.Enabled =
                                        cHB0.Enabled = 
                                            objects.CheckedItems.Count > 0;
            cHB3A.Enabled = 
                cHB2A.Enabled =
                    cHB0A.Enabled =
                        objects.CheckedItems.Count < objects.Items.Count;

            cHBA.Enabled  = (objects.CheckedItems.Count < objects.Items.Count);
            cHB1.Enabled  = (objects.CheckedItems.Count > 0) && (laySelect.Items.Count > 1);
            cHBD.Enabled = (objects.CheckedItems.Count > 0) && (laySelect.Items.Count > 1);
            cHBDS.Enabled = (objects.CheckedItems.Count > 0) && sc.SASisOpen;
            cHBDM.Enabled = prev_m > 0;

            NBC4.Enabled = NBC2.Enabled = NBC0.Enabled = prev_m > 0;
            NBC5.Enabled = NBC3.Enabled = NBC1.Enabled = prev_m < objects.Items.Count;

            markAllItemsAsDeletedToolStripMenuItem.Enabled = prev_m != objects.Items.Count;
            markAllAsNotDeletedToolStripMenuItem.Enabled = prev_m > 0;

            sba.Enabled = objects.Items.Count > 1;
            sbi.Enabled = objects.Items.Count > 1;
            sbd.Enabled = (objects.Items.Count > 1) && (objects.SelectedItems.Count > 0) && (objects.SelectedItems[0].SubItems[1].Text == "Point");
            sbl.Enabled = (objects.Items.Count > 1) && (objects.SelectedItems.Count > 0) && (objects.SelectedItems[0].SubItems[1].Text.StartsWith("Line"));
            sbls.Enabled = (objects.Items.Count > 1) && (objects.SelectedItems.Count > 0) && (objects.SelectedItems[0].SubItems[1].Text.StartsWith("Line"));
            sortByRouteLengthToThisPointToolStripMenuItem.Enabled = (objects.Items.Count > 1) && (objects.SelectedItems.Count > 0) && (objects.SelectedItems[0].SubItems[1].Text == "Point");
            sbrn.Enabled = objects.Items.Count > 1;
            sbrs.Enabled = objects.Items.Count > 1;
        }

        private void findSimilarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            int dist = 2;
            if (System.Windows.Forms.InputBox.Show("Distance", "Max distance in meters:", ref dist, 0, 9999) == DialogResult.OK)
                    FindCopies(objects.SelectedIndices[0], true, false, dist);
        }

        private void markAsSkipWhenSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;

            objects.SelectedItems[0].SubItems[6].Text = (objects.SelectedItems[0].SubItems[6].Text == "Yes") ? "" : "Yes";
            if(objects.SelectedItems[0].SubItems[6].Text == "Yes")
                objects.SelectedItems[0].Font = new Font(objects.SelectedItems[0].Font, FontStyle.Strikeout);
            else
                objects.SelectedItems[0].Font = new Font(objects.SelectedItems[0].Font, FontStyle.Regular);

            CheckMarked();
        }

        private void markAllAsNotDeletedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            for (int i = 0; i < objects.Items.Count; i++)
            {
                objects.Items[i].SubItems[6].Text = "";
                objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Regular);
            };
            status.Text = "";
            CheckMarked();
        }

        private void markAllCopiesAsDeletedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            Hashtable htXY = new Hashtable();
            Hashtable htNM = new Hashtable();

            for (int i = 0; i < objects.Items.Count; i++)
            {
                string simByXY = objects.Items[i].SubItems[4].Text;
                string simByNM = objects.Items[i].SubItems[5].Text;

                if (simByXY != "")
                {
                    object oXY = htXY[simByXY];
                    if (oXY == null) htXY.Add(simByXY, oXY = new List<int>());
                    List<int> list = (List<int>)oXY;
                    list.Add(i);
                };

                if (simByNM != "")
                {
                    object oXY = htNM[simByNM];
                    if (oXY == null) htNM.Add(simByNM, oXY = new List<int>());
                    List<int> list = (List<int>)oXY;
                    list.Add(i);
                };
            };
            
            int sxy = 0;
            foreach (string key in htXY.Keys)
            {
                List<int> list = (List<int>)htXY[key];
                if (list.Count < 2) continue;
                for (int i = 1; i < list.Count; i++)
                {
                    objects.Items[list[i]].SubItems[6].Text = "Yes";
                    objects.Items[list[i]].Font = new Font(objects.Items[list[i]].Font, FontStyle.Strikeout);
                    sxy++;
                };
            };

            int snm = 0;
            foreach (string key in htNM.Keys)
            {
                List<int> list = (List<int>)htNM[key];
                if (list.Count < 2) continue;
                for (int i = 1; i < list.Count; i++)
                {
                    objects.Items[list[i]].SubItems[6].Text = "Yes";
                    objects.Items[list[i]].Font = new Font(objects.Items[list[i]].Font, FontStyle.Strikeout);
                    snm++;
                };
            };

            if (sxy > 0) status.Text += "Marked " + sxy.ToString() + " copies as Deleted by Coordinates\r\n";
            if (snm > 0) status.Text += "Marked " + snm.ToString() + " copies as Deleted by Name\r\n";
            status.SelectionStart = status.TextLength;
            status.ScrollToCaret();

            CheckMarked();
        }

        private void findCopiesForToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int dist = 2;
            if (System.Windows.Forms.InputBox.Show("Distance", "Max distance in meters:", ref dist, 0, 9999) == DialogResult.OK)
                FindCopies(-1, true, false, dist);
        }

        private void markAllItemsAsDeletedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            for (int i = 0; i < objects.Items.Count; i++)
            {
                objects.Items[i].SubItems[6].Text = "Yes";
                objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Strikeout);
            };
            status.Text = "";
            CheckMarked();
        }

        private void invertDeletedMarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (objects.Items[i].SubItems[6].Text == "Yes")
                {
                    objects.Items[i].SubItems[6].Text = "";
                    objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Regular);
                }
                else
                {
                    objects.Items[i].SubItems[6].Text = "Yes";
                    objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Strikeout);
                };
            };
            status.Text = "";
            CheckMarked();
        }

        private void findCopiesForSelectedItemByNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            FindCopies(objects.SelectedIndices[0], false, true, 0);
        }

        private void findCopiesForEachItemByNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindCopies(-1, false, true, 0);
        }        

        private void markCopiesAsNotDeletedToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            Hashtable htXY = new Hashtable();
            Hashtable htNM = new Hashtable();

            for (int i = 0; i < objects.Items.Count; i++)
            {
                string simByXY = objects.Items[i].SubItems[4].Text;
                string simByNM = objects.Items[i].SubItems[5].Text;

                if (simByXY != "")
                {
                    object oXY = htXY[simByXY];
                    if (oXY == null) htXY.Add(simByXY, oXY = new List<int>());
                    List<int> list = (List<int>)oXY;
                    list.Add(i);
                };

                if (simByNM != "")
                {
                    object oXY = htNM[simByNM];
                    if (oXY == null) htNM.Add(simByNM, oXY = new List<int>());
                    List<int> list = (List<int>)oXY;
                    list.Add(i);
                };
            };

            int sxy = 0;
            foreach (string key in htXY.Keys)
            {
                List<int> list = (List<int>)htXY[key];
                if (list.Count < 2) continue;
                for (int i = 1; i < list.Count; i++)
                {
                    objects.Items[list[i]].SubItems[6].Text = "";
                    objects.Items[list[i]].Font = new Font(objects.Items[list[i]].Font, FontStyle.Regular);
                    sxy++;
                };
            };

            int snm = 0;
            foreach (string key in htNM.Keys)
            {
                List<int> list = (List<int>)htNM[key];
                if (list.Count < 2) continue;
                for (int i = 1; i < list.Count; i++)
                {
                    objects.Items[list[i]].SubItems[6].Text = "";
                    objects.Items[list[i]].Font = new Font(objects.Items[list[i]].Font, FontStyle.Regular);
                    snm++;
                };
            };

            if (sxy > 0) status.Text += "Marked " + sxy.ToString() + " copies as Not Deleted by Coordinates\r\n";
            if (snm > 0) status.Text += "Marked " + snm.ToString() + " copies as Not Deleted by Name\r\n";
            status.SelectionStart = status.TextLength;
            status.ScrollToCaret();

            CheckMarked();
        }

        private void ContentViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            state = new State();
            state.MapID = iStorages.SelectedIndex;
            state.SASDir = SASPlanetCacheDir;
            state.URL = UserDefindedUrl;
            state.FILE = UserDefindedFile;
            string fn = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.stt";
            State.Save(fn, state);

            if (groute != null) groute.Save();

            if (objects.Items.Count == 0) return;

            int marked = 0;
            for (int i = 0; i < objects.Items.Count; i++)
                if (objects.Items[i].SubItems[6].Text == "Yes")
                    marked++;

            if (marked == 0) return;

            if (marked > 0)
            {
                DialogResult dr = MessageBox.Show(String.Format("{0} placemark(s) marked as Deleted!\r\nDo you want to delete them in source layer?", marked), "Saving...", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                };
                if (dr == DialogResult.No) return;
            };

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            int pDel = 0;
            int[] pla = new int[3];
            for (int i = objects.Items.Count - 1; i >= 0; i--)
                if (objects.Items[i].SubItems[6].Text == "Yes")
                {
                    string XPath = objects.Items[i].SubItems[7].Text; 
                    string indx = XPath.Substring(XPath.IndexOf("["));
                    XPath = XPath.Remove(XPath.IndexOf("["));
                    int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                    XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                    xn = xn.SelectNodes(XPath)[ind].ParentNode.ParentNode;
                    if (xn.Name == "outerBoundaryIs") // polygon
                    {
                        xn = xn.ParentNode.ParentNode;
                        pla[2]++;
                    }
                    else
                    {
                        if (xn.SelectSingleNode("Point") != null) pla[0]++;
                        if (xn.SelectSingleNode("LineString") != null) pla[1]++;
                    };
                    xn = xn.ParentNode.RemoveChild(xn);
                    pDel++;
                };
            l.file.SaveKML();
            l.placemarks -= pDel;
            l.points -= pla[0];
            l.lines -= pla[1];
            l.areas -= pla[2];
            parent.Refresh();
        }

        private int prev_m = 0;
        private void CheckMarked()
        {
            if (objects.Items.Count == 0) return;

            int marked = 0;
            for (int i = 0; i < objects.Items.Count; i++)
            {
                bool mrkd = objects.Items[i].SubItems[6].Text == "Yes";
                if (mrkd) marked++;
                mapContent[i].Visible = !mrkd;
            };

            laySelect.Enabled = marked == 0;
            MapViewer.DrawOnMapData();

            if (prev_m != marked)
            {
                prev_m = marked;
                UpdateCheckedAndMarked(true);                
            };
            
        }

        private void objects_KeyPress(object sender, KeyPressEventArgs e)
        {            
            if (objects.SelectedIndices.Count == 0) return;
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
                objects_DoubleClick(sender, e);
            if (e.KeyChar == ' ')
            {
                if (!objects.CheckBoxes)
                {
                    objects.SelectedItems[0].Checked = true;
                    objects.CheckBoxes = true;
                } 
                else if(objects.CheckedItems.Count == 0)
                    objects.CheckBoxes = false;
            };
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedIndices.Count == 0) return;

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            string XPath = objects.SelectedItems[0].SubItems[7].Text;
            string indx = XPath.Substring(XPath.IndexOf("["));
            XPath = XPath.Remove(XPath.IndexOf("["));
            int ind = int.Parse(indx.Substring(1, indx.Length - 2));
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            XmlNode xy = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("Point/coordinates");
            XmlNode xn = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("name");
            if(xn == null)
                xn = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.ParentNode.ParentNode.SelectSingleNode("name");
            XmlNode xd = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("description");
            if(xd == null)
                xd = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.ParentNode.ParentNode.SelectSingleNode("description");

            string style = "";
            XmlNode st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
            if ((st != null) && (st.ChildNodes.Count > 0))
                style = st.ChildNodes[0].Value;
            string nam = "NoName";
            try { nam = xn.ChildNodes[0].Value; } catch { };
            string xyt = xy == null ? ",," : xy.ChildNodes[0].Value;
            string[] xya = xyt.Split(new string[] { "," }, StringSplitOptions.None);
            string x = xya[0];
            string y = xya[1];
            string desc = "";
            if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
            string dw = desc;
            string styleN = style;

            int xcx = images.Images.Count;
            if (InputXY(objects.SelectedItems[0].SubItems[1].Text == "Point", l.file.tmp_file_dir, ref nam, ref y, ref x, ref desc, ref styleN) == DialogResult.OK)
            {
                bool ch = false;
                bool chxy = false;
                nam = nam.Trim();
                desc = desc.Trim();
                try { if (nam != xn.ChildNodes[0].Value) ch = true; }
                catch { ch = true; };
                if (desc != dw) ch = true;                
                x = x.Trim().Replace(",", ".");
                y = y.Trim().Replace(",", ".");
                if (styleN != style) { ch = true; chxy = true; };
                if (x != xya[0]) chxy = true;
                if (y != xya[1]) chxy = true;
                if (ch)
                {
                    objects.SelectedItems[0].Text = nam;
                    if(xn.ChildNodes.Count > 0)
                        xn.ChildNodes[0].Value = nam;
                    else
                        xn.AppendChild(l.file.kmlDoc.CreateTextNode(nam));
                    NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
                    mo.Name = nam;

                    if (xd != null)
                        xd.RemoveAll();
                    else
                    {
                        xd = l.file.kmlDoc.CreateElement("description");
                        xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.AppendChild(xd);
                    };
                    xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                    if (st != null)
                        st.RemoveAll();
                    else
                    {
                        st = l.file.kmlDoc.CreateElement("styleUrl");
                        xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.AppendChild(st);
                    };
                    st.AppendChild(l.file.kmlDoc.CreateTextNode(styleN));
                    if (style2image.ContainsKey(styleN))
                        if (images.Images.ContainsKey(style2image[styleN]))
                        {
                            mo.Img = images.Images[style2image[styleN]];
                            objects.SelectedItems[0].ImageIndex = images.Images.IndexOfKey(style2image[styleN]);
                        };
                    mo.UserData = desc;
                    textBox1.Text = desc; 
                };
                if (chxy)
                {
                    xy.ChildNodes[0].Value = String.Format("{0},{1},0", x, y);
                    objects.SelectedItems[0].SubItems[2].Text = y;
                    objects.SelectedItems[0].SubItems[3].Text = x;
                    NaviMapNet.MapPoint mp = (NaviMapNet.MapPoint)mapContent[objects.SelectedIndices[0]];
                    System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
                    System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
                    ni.NumberDecimalSeparator = ".";
                    mp.Points[0] = new PointF(float.Parse(x, ni), float.Parse(y, ni));                    
                };
                if (ch || chxy)
                {                    
                    l.file.SaveKML();
                    MapViewer.DrawOnMapData();
                };
            };
        }

        public class XYTextBox : TextBox 
        { 
            public TextBox xBox; 
            public TextBox yBox;
            public XYTextBox()
            {
                this.Validating += new CancelEventHandler(XYTextBox_Validating);
            }

            private void XYTextBox_Validating(object sender, CancelEventArgs e)
            {
                XYTextBox tb = (sender as XYTextBox);

                PointD pd = LatLonParser.Parse(tb.Text.Trim());
                if ((pd != null) && (pd.X != pd.Y) && (pd.X != 0) && (pd.Y != 0) && (tb.xBox != null) && (tb.yBox != null))
                {
                    tb.xBox.Text = LatLonParser.DoubleToStringMax(pd.X, 8);
                    tb.yBox.Text = LatLonParser.DoubleToStringMax(pd.Y, 8);
                    tb.SelectAll();
                };

                double d = 0.0;
                if (double.TryParse(tb.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) return;

                string wast = tb.Text.Trim();
                string text = tb.Text.Replace(",", ".").Trim();
                if (tb.Name == "xBox")
                    try { d = LatLonParser.Parse(text, false); }
                    catch { };
                if (tb.Name == "yBox")
                    try { d = LatLonParser.Parse(text, true); }
                    catch { };
                tb.Text = LatLonParser.DoubleToStringMax(d, 8);
                if (wast != tb.Text)
                    e.Cancel = true;
                tb.SelectAll();
            }
        }

        public DialogResult InputXY(bool changeXY, string file, ref string value, ref string lat, ref string lon, ref string desc, ref string style)
        {
            Form form = new Form();
            form.ShowInTaskbar = false;
            Label nameText = new Label();
            Label xText = new Label();
            Label yText = new Label();
            Label dText = new Label();
            PictureBox pBox = new PictureBox();
            ComboBox cBox = new ComboBox(); cBox.DropDownStyle = ComboBoxStyle.DropDownList; cBox.FlatStyle = FlatStyle.Flat;
            TextBox nameBox = new TextBox(); nameBox.BorderStyle = BorderStyle.FixedSingle;
            XYTextBox xBox = new XYTextBox(); xBox.BorderStyle = BorderStyle.FixedSingle;
            xBox.Name = "xBox";
            XYTextBox yBox = new XYTextBox(); yBox.BorderStyle = BorderStyle.FixedSingle;
            yBox.Name = "yBox";
            TextBox dBox = new TextBox(); dBox.BorderStyle = BorderStyle.FixedSingle;
            dBox.Multiline = true;
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            Button buttonPlay = new Button();

            if (changeXY)
            {
                foreach (KeyValuePair<string, string> kvp in style2image)
                    cBox.Items.Add(kvp.Key);
                cBox.SelectedIndexChanged += new EventHandler(cBox_SelectedIndexChanged);
            }
            else
            {
                cBox.Visible = false;
                pBox.Visible = false;
            };

            form.Text = "Change placemark";
            nameText.Text = "Name:";
            nameBox.Text = value.Replace("\r\n", "").Trim();
            xText.Text = "Longitude (D/DM/DMS):";
            xBox.Text = lon.Replace("\r\n", "").Trim();
            yText.Text = "Latitude (D/DM/DMS):";
            yBox.Text = lat.Replace("\r\n","").Trim();
            dText.Text = "Description:";
            dBox.Text = desc;
            dBox.ScrollBars = ScrollBars.Vertical;

            if (!changeXY) xBox.Enabled = yBox.Enabled = false;

            xBox.xBox = xBox; xBox.yBox = yBox;
            yBox.xBox = xBox; yBox.yBox = yBox;
            
            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            buttonPlay.Text = "Play Sound";
            buttonPlay.FlatStyle = FlatStyle.Flat;
            buttonPlay.Click += (delegate(object sender, EventArgs e)
            {
                Regex rx = new Regex(@"alert_sound=(?<sound>.+)", RegexOptions.None);
                Match mx = rx.Match(dBox.Text);
                if (mx.Success)
                {

                    try
                    {
                        string fName = mx.Groups["sound"].Value.Trim(new char[] { '\r', '\n' });
                        FileInfo fi;
                        if (Path.IsPathRooted(fName))
                            fi = new FileInfo(fName);
                        else if (!String.IsNullOrEmpty(file))
                            fi = new FileInfo(Path.Combine(Path.GetDirectoryName(file), fName));
                        else
                            fi = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fName));
                        if (fi.Exists)
                            System.Diagnostics.Process.Start(fi.FullName);
                    }
                    catch (Exception ex) { };
                };
            });

            dBox.TextChanged += (delegate(object sender, EventArgs e) { buttonPlay.Visible = dBox.Text.Contains("alert_sound="); });
            
            nameText.SetBounds(9, 20, 472, 13);
            nameBox.SetBounds(12, 36, 472, 20);
            yText.SetBounds(9, 60, 472, 13);
            yBox.SetBounds(12, 76, 472, 20);
            xText.SetBounds(9, 100, 472, 13);
            xBox.SetBounds(12, 116, 472, 20);
            dText.SetBounds(9, 140, 472, 13);
            dBox.SetBounds(12, 156, 472, 180);

            buttonOk.SetBounds(328, 347, 75, 23);
            buttonCancel.SetBounds(409, 347, 75, 23);
            buttonPlay.SetBounds(12, 347, 90, 23);
            cBox.SetBounds(298, 6, 90, 23);
            pBox.SetBounds(274, 7, 22, 22);            
            
            nameText.AutoSize = true;
            nameBox.Anchor = nameBox.Anchor | AnchorStyles.Right;
            yBox.Anchor = yBox.Anchor | AnchorStyles.Right;
            xBox.Anchor = xBox.Anchor | AnchorStyles.Right;
            dBox.Anchor = dBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonPlay.Visible = dBox.Text.Contains("alert_sound=");

            form.ClientSize = new Size(496, 380);
            form.Controls.AddRange(new Control[] { nameText, nameBox, yText, yBox, xText, xBox, dText, dBox, cBox, buttonPlay, buttonOk, buttonCancel, pBox });
            form.ClientSize = new Size(Math.Max(400, nameText.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            if(cBox.Items.IndexOf(style) >= 0)
                cBox.SelectedIndex = cBox.Items.IndexOf(style);
            else
                if(cBox.Items.Count > 0)
                    cBox.SelectedIndex = 0;

            DialogResult dialogResult = form.ShowDialog();
            form.Dispose();
            if(dialogResult == DialogResult.OK)
            value = nameBox.Text;            
            desc = dBox.Text;
            if (changeXY)
            {
                lat = yBox.Text;
                lon = xBox.Text;
                style = cBox.Text;
            };
            return dialogResult;
        }

        private void cBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cBox = sender as ComboBox;
            PictureBox pBox = (PictureBox)(cBox.Parent.Controls[cBox.Parent.Controls.Count - 1]);
            if (style2image.ContainsKey(cBox.Text))
                pBox.Image = images.Images[style2image[cBox.Text]];
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
                        yy = ydir + @"\y" + y.ToString() + ".jpg";
                        if (File.Exists(yy))
                            return yy;
                        yy = ydir + @"\y" + y.ToString() + ".gif";
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
            if (InputBox.QueryDirectoryBox("SAS Planet Cache", "Enter Cache Path Here:", ref spcd) == DialogResult.OK)
                SASPlanetCacheDir = ClearLastSlash(spcd);
            else
                return;

            if(Directory.Exists(SASPlanetCacheDir))
                mru1.AddFile(SASPlanetCacheDir);

            if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
                iStorages_SelectedIndexChanged(sender, e);
            else
                iStorages.SelectedIndex = iStorages.Items.Count - 1;
        }

        public string ClearLastSlash(string file_name)
        {
            if (file_name.Substring(file_name.Length - 1) == @"\")
                return file_name.Remove(file_name.Length - 1);
            return file_name;
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

        private void MapViewer_MouseDown(object sender, MouseEventArgs e)
        {
            locate = true;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (Char)13) return;
            e.Handled = true;            

            if (objects.Items.Count == 0) return;
            string st = textBox2.Text.ToLower();
            List<int> foundAll = new List<int>();
            for (int i = 0; i < objects.Items.Count; i++)
                if (objects.Items[i].SubItems[0].Text.ToLower().Contains(st))
                    foundAll.Add(i);

            if (foundAll.Count == 0) return;

            int si = -1;
            if (objects.SelectedIndices.Count > 0)
                si = foundAll.IndexOf(objects.SelectedIndices[0]);
            if ((si >= 0) && (si < (foundAll.Count - 1)))
            {
                objects.Items[foundAll[si + 1]].Selected = true;
                objects.Items[foundAll[si + 1]].EnsureVisible();
            }
            else
            {
                objects.Items[foundAll[0]].Selected = true;
                objects.Items[foundAll[0]].EnsureVisible();
            };
        }

        private void selectMarkDeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".cscript";
            ofd.InitialDirectory = KMZRebuilederForm.CurrentDirectory() + @"\Scripts";
            ofd.Filter = "CS Scripts (*.cscript)|*.cscript";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string script = "";
                System.IO.StreamReader sr = new StreamReader(ofd.FileName, System.Text.Encoding.GetEncoding(1251));
                script = sr.ReadToEnd();
                sr.Close();
                CallSctript(script);
            };
            ofd.Dispose();
        }

        private void CallSctript(string script)
        {            
            System.Reflection.Assembly ScriptAsm = null;
            try
            {
                ScriptAsm = CSScriptLibrary.CSScript.LoadCode(
                     "using System;\r\n " +
                     "using System.Text;\r\n " +
                     "using System.Collections.Generic;\r\n " +                     
                     "using System.Collections;\r\n " +
                     "using System.Drawing;\r\n " +
                     "using System.Windows.Forms;\r\n" +
                     "public class Script {\r\n" +
                     script +
                     "}", null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Script Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            ScriptHelper sh = new ScriptHelper(ScriptAsm);
                        
            int simIndex = 0;

            Hashtable simByXY = new Hashtable();
            Hashtable simByNm = new Hashtable();
            
            int mad = 0;
            for (int i = 0; i < mapContent.ObjectsCount - 1; i++)
            {
                NaviMapNet.MapObject a = mapContent[i];
                if(sh.MarkAsDeleted((byte) a.ObjectType, a.Name, a.X, a.Y, a.Points, a.UserData))
                {
                    mad++;
                    objects.Items[i].SubItems[6].Text = "Yes";
                    objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Strikeout);                    
                };
            };

            status.Text = "";
            if (simByXY.Count > 0) status.Text += "Found " + simByXY.Count.ToString() + " similar placemarks by coordinates\r\n";
            if (simByNm.Count > 0) status.Text += "Found " + simByNm.Count.ToString() + " similar placemarks by name\r\n";
            if (status.Text == "") status.Text = String.Format("{0} objects marked as deleted", mad);
            status.SelectionStart = status.TextLength;
            status.ScrollToCaret();
        }

        public class ScriptHelper : CSScriptLibrary.AsmHelper
        {
            public ScriptHelper(System.Reflection.Assembly asm)
                : base(asm)
            {

            }

            public bool MarkAsDeleted(byte ObjectType, string Name, double X, double Y, PointF[] points, object UsersData)
            {
                return (bool)this.Invoke("Script.MarkAsDeleted", new object[] { ObjectType, Name, X, Y, points, UsersData });
            }
        }

        private void toolStripDropDownButton1_DropDownOpening(object sender, EventArgs e)
        {
            spcl.Enabled = mru1.Count > 0;
            MCDT.Enabled = File.Exists(KMZRebuilederForm.CurrentDirectory() + @"\Map_Cache_Dirs.txt");
            CDSC.Enabled = Directory.Exists(state.SASCacheDir);
        }

        private void applySelectionFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (laySelect.SelectedIndex < 0) return;
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];

            Selection_Filter sfw = new Selection_Filter(this, l.file, l);
            sfw.APPD.Visible = true;
            sfw.APPS.Visible = true;
            sfw.ShowDialog();
            sfw.Dispose();
        }

        public void ApplyFilter(int[] toKeep, int[] toDel, bool checkbox)
        {
            if (laySelect.SelectedIndex < 0) return;
            if (toDel == null) return;
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];

            if (checkbox)
                objects.CheckBoxes = true;

            XmlNodeList placemarks = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id].SelectNodes("Placemark");
            for (int i = 0; i < toDel.Length; i++)
            {
                if (checkbox)
                    objects.Items[toDel[i]].Checked = true;
                else
                {
                    objects.Items[toDel[i]].SubItems[6].Text = "Yes";
                    objects.Items[toDel[i]].Font = new Font(objects.Items[toDel[i]].Font, FontStyle.Strikeout);
                };
            };

            CheckMarked();
        }

        private void ContentViewer_Load(object sender, EventArgs e)
        {
            MapViewer.DrawMap = true;
        }

        private void âûáðàòüÏàïêóÊýøàSASPlanetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string mcd = KMZRebuilederForm.CurrentDirectory() + @"\Map_Cache_Dirs.txt";
            if (File.Exists(mcd))
            {
                List<string> pathes = new List<string>();
                List<string> names = new List<string>();
                try
                {
                    FileStream fs = new FileStream(mcd, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (String.IsNullOrEmpty(line)) continue;
                        if (line.StartsWith("#")) continue;
                        if (line.StartsWith("@")) continue;
                        line = line.Replace("%CD%", KMZRebuilederForm.CurrentDirectory());
                        string[] LP = line.Split(new char[] { '=' }, 2);
                        string prefix = "";
                        if (LP.Length > 1)
                        {
                            line = ClearLastSlash(LP[1]);
                            prefix = LP[0].Trim();
                        }
                        else
                            line = ClearLastSlash(LP[0]);
                        if (Directory.Exists(line))
                        {
                            pathes.Add(line);
                            names.Add((String.IsNullOrEmpty(prefix) ? "" : (prefix + ": ")) + Path.GetFileName(line) + " ... " + line);
                        };
                    };
                    sr.Close();
                    fs.Close();
                }
                catch
                { };
                if (pathes.Count > 0)
                {
                    int sel = -1;
                    for (int i = 0; i < pathes.Count; i++)
                        if (pathes[i] == SASPlanetCacheDir)
                            sel = i;
                    if (InputBox.Show("SAS Planet Cache", "Select Path:", names.ToArray(), ref sel) == DialogResult.OK)
                    {
                        SASPlanetCacheDir = ClearLastSlash(pathes[sel]);
                        if(Directory.Exists(SASPlanetCacheDir))
                            mru1.AddFile(SASPlanetCacheDir);

                        if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
                            iStorages_SelectedIndexChanged(sender, e);
                        else
                            iStorages.SelectedIndex = iStorages.Items.Count - 1;
                    };
                };
            };       
        }

        private void spvl_Click(object sender, EventArgs e)
        {
            spvl.Checked = !(splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed);
        }

        private void iStorages_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1) return;

            string text = ((ComboBox)sender).Items[e.Index].ToString();
            int lastIndex = ((ComboBox)sender).Items.Count - 1;

            Color itemForegroundColor = new Color();
            itemForegroundColor = Color.Black;
            bool selected = e.BackColor != ((ComboBox)sender).BackColor;

            if (e.Index == 0)
                itemForegroundColor = Color.Silver;
            else if (e.Index < (lastIndex - 2))
                itemForegroundColor = Color.Black;
            else if (e.Index == (lastIndex - 2))
            {
                itemForegroundColor = Color.Crimson;
                if (selected && ((ComboBox)sender).DroppedDown)
                    text = UserDefindedFile;
                else
                {
                    string txt = UserDefindedFile;
                    SizeF sf = e.Graphics.MeasureString("FILE: .. " + txt, e.Font);
                    while (sf.Width > e.Bounds.Width)
                    {
                        txt = txt.Remove(0, 1);
                        sf = e.Graphics.MeasureString("FILE: .. " + txt, e.Font);
                    };
                    text = "FILE: .. " + txt;
                };
            }
            else if (e.Index == lastIndex)
            {
                itemForegroundColor = Color.DarkViolet;
                if (selected && ((ComboBox)sender).DroppedDown)
                    text = SASPlanetCacheDir;
                else
                {
                    string txt = SASPlanetCacheDir;
                    SizeF sf = e.Graphics.MeasureString("PATH: .. " + txt, e.Font);
                    while (sf.Width > e.Bounds.Width)
                    {
                        txt = txt.Remove(0, 1);
                        sf = e.Graphics.MeasureString("PATH: .. " + txt, e.Font);
                    };
                    text = "PATH: .. " + txt;
                };
            }
            else
            {
                itemForegroundColor = Color.Green;
                if (selected && ((ComboBox)sender).DroppedDown)
                    text = UserDefindedUrl;
                else
                {
                    string txt = UserDefindedUrl;
                    SizeF sf = e.Graphics.MeasureString("URL: .. " + txt, e.Font);
                    while (sf.Width > e.Bounds.Width)
                    {
                        txt = txt.Remove(0, 1);
                        sf = e.Graphics.MeasureString("URL: .. " + txt, e.Font);
                    };
                    text = "URL: .. " + txt;
                };
            };

            e.DrawBackground();
            e.Graphics.FillRectangle(new SolidBrush(selected ? itemForegroundColor : ((ComboBox)sender).BackColor), e.Bounds);
            e.Graphics.DrawString(text, e.Font, new SolidBrush(selected ? ((ComboBox)sender).BackColor : itemForegroundColor), e.Bounds);
            e.DrawFocusRectangle();
        }

        private void BTNMORE_Click(object sender, EventArgs e)
        {
            toolStripDropDownButton1.ShowDropDown();
        }

        private void âûáðàòüÏàïêóÑÊýøåìToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void CDSC_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(state.SASCacheDir)) return;

            string[] dirs = Directory.GetDirectories(state.SASCacheDir, "*.*");
            if (dirs == null) return;
            if (dirs.Length == 0) return;

            int sel = -1;
            string[] names = new string[dirs.Length];
            for (int i = 0; i < dirs.Length; i++)
            {
                names[i] = Path.GetFileName(dirs[i]);
                if (dirs[i] == SASPlanetCacheDir) sel = i;
            };

            if (InputBox.Show("SAS Planet Cache", "Select Path:", names, ref sel) == DialogResult.OK)
            {
                SASPlanetCacheDir = ClearLastSlash(dirs[sel]);
                if (Directory.Exists(SASPlanetCacheDir))
                    mru1.AddFile(SASPlanetCacheDir);

                if (iStorages.SelectedIndex == (iStorages.Items.Count - 1))
                    iStorages_SelectedIndexChanged(sender, e);
                else
                    iStorages.SelectedIndex = iStorages.Items.Count - 1;
            };
        }        

        private void addCopyOfPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedIndices.Count == 0) return;
            if (objects.SelectedItems[0].SubItems[1].Text != "Point") return;

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "#none";
            
            string XPath = objects.SelectedItems[0].SubItems[7].Text;
            string indx = XPath.Substring(XPath.IndexOf("["));
            XPath = XPath.Remove(XPath.IndexOf("["));
            int ind = int.Parse(indx.Substring(1, indx.Length - 2));
            XmlNode st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
            if ((st != null) && (st.ChildNodes.Count > 0))
                style = st.ChildNodes[0].Value;
            st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode;
            
            XmlNode el = l.file.kmlDoc.CreateElement("Placemark");            
            el.AppendChild(l.file.kmlDoc.CreateElement("name"));            
            el.AppendChild(l.file.kmlDoc.CreateElement("styleUrl").AppendChild(l.file.kmlDoc.CreateTextNode(style)).ParentNode);
            el.AppendChild(l.file.kmlDoc.CreateElement("Point").AppendChild(l.file.kmlDoc.CreateElement("coordinates")).ParentNode);
            el.AppendChild(l.file.kmlDoc.CreateElement("description"));
            XmlNode xy = el.SelectSingleNode("Point/coordinates");
            XmlNode xn = el.SelectSingleNode("name");
            XmlNode xd = el.SelectSingleNode("description");

            string nam = st.SelectSingleNode("name").ChildNodes[0].Value+" (Copy)";
            string xyt = xy == null ? ",," : st.SelectSingleNode("Point/coordinates").ChildNodes[0].Value;
            string[] xya = xyt.Split(new string[] { "," }, StringSplitOptions.None);
            string x = xya[0];
            string y = xya[1];
            string desc = "";
            if ((st.SelectSingleNode("description") != null) && (st.SelectSingleNode("description").ChildNodes.Count > 0)) desc = st.SelectSingleNode("description").ChildNodes[0].Value;
            string dw = desc;
            st = st.SelectSingleNode("styleUrl");

            if (InputXY(objects.SelectedItems[0].SubItems[1].Text == "Point", l.file.tmp_file_dir, ref nam, ref y, ref x, ref desc, ref style) == DialogResult.OK)
            {
                bool ch = false;
                if (!String.IsNullOrEmpty(nam)) ch = true;
                if (!String.IsNullOrEmpty(desc)) ch = true;
                x = x.Trim().Replace(",", ".");
                y = y.Trim().Replace(",", ".");
                if (!String.IsNullOrEmpty(x)) ch = true;
                if (!String.IsNullOrEmpty(y)) ch = true;
                if (ch)
                {
                    xn.AppendChild(l.file.kmlDoc.CreateTextNode(nam));
                    xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                    xy.AppendChild(l.file.kmlDoc.CreateTextNode(String.Format("{0},{1},0", x, y)));
                    st.ChildNodes[0].Value = style;
                    xf.AppendChild(el);
                    l.file.SaveKML();
                    l.placemarks++;
                    l.points++;
                    parent.Refresh();
                    laySelect.Items[laySelect.SelectedIndex] = l.ToString();
                    if (objects.Items.Count > 0)
                        objects.Items[objects.Items.Count - 1].Selected = true;
                };
            };
        }

        private void mcmn_Click(object sender, EventArgs e)
        {
            mcm_Change(0);
        }

        private void mcme_Click(object sender, EventArgs e)
        {
            mcm_Change(1);
        }

        private void mcm_Change(byte mode)
        {
            if (mode == 0)
            {
                mcmn.Checked = true;
                mcme.Checked = false;
                MapViewer.UseDefaultContextMenu = true;
                MapViewer.ContextMenuStrip = null;
                OMM.Text = "N";
                OMM.ForeColor = Color.Green;
            };
            if (mode == 1)
            {
                mcmn.Checked = false;
                mcme.Checked = true;
                MapViewer.UseDefaultContextMenu = false;
                MapViewer.ContextMenuStrip = contextMenuStrip2;
                OMM.Text = "C";
                OMM.ForeColor = Color.Red;
            };
        }

        private void addNewPointToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "#none";
            XmlNode st = null;
            if (objects.SelectedIndices.Count > 0)
            {
                string XPath = objects.SelectedItems[0].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                if ((st != null) && (st.ChildNodes.Count > 0))
                    style = st.ChildNodes[0].Value;
            };

            XmlNode el = l.file.kmlDoc.CreateElement("Placemark");            
            el.AppendChild(l.file.kmlDoc.CreateElement("name"));            
            el.AppendChild(l.file.kmlDoc.CreateElement("styleUrl").AppendChild(l.file.kmlDoc.CreateTextNode(style)).ParentNode);
            el.AppendChild(l.file.kmlDoc.CreateElement("Point").AppendChild(l.file.kmlDoc.CreateElement("coordinates")).ParentNode);
            el.AppendChild(l.file.kmlDoc.CreateElement("description"));
            st = el.SelectSingleNode("styleUrl");
            XmlNode xy = el.SelectSingleNode("Point/coordinates");
            XmlNode xn = el.SelectSingleNode("name");
            XmlNode xd = el.SelectSingleNode("description");

            string nam = "Point "+DateTime.Now.ToString("yyyyMMddHHmmss");
            string x = MapViewer.MouseDownDegrees.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = MapViewer.MouseDownDegrees.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string desc = "";
            if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
            string dw = desc;

            if (InputXY(true, l.file.tmp_file_dir, ref nam, ref y, ref x, ref desc, ref style) == DialogResult.OK)
            {
                bool ch = false;
                if (!String.IsNullOrEmpty(nam)) ch = true;
                if (!String.IsNullOrEmpty(desc)) ch = true;
                x = x.Trim().Replace(",", ".");
                y = y.Trim().Replace(",", ".");
                if (!String.IsNullOrEmpty(x)) ch = true;
                if (!String.IsNullOrEmpty(y)) ch = true;
                if (ch)
                {
                    xn.AppendChild(l.file.kmlDoc.CreateTextNode(nam));
                    xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                    xy.AppendChild(l.file.kmlDoc.CreateTextNode(String.Format("{0},{1},0", x, y)));
                    st.ChildNodes[0].Value = style;
                    xf.AppendChild(el);

                    l.file.SaveKML();
                    l.placemarks++;
                    l.points++;
                    parent.Refresh();
                    laySelect.Items[laySelect.SelectedIndex] = l.ToString();
                    if (objects.Items.Count > 0)
                        objects.Items[objects.Items.Count - 1].Selected = true;
                };
            };
        }

        private bool addNewPointByNXY(string name, string desc, PointF point)
        {
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "#none";
            XmlNode st = null;
            if (objects.SelectedIndices.Count > 0)
            {
                string XPath = objects.SelectedItems[0].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                if ((st != null) && (st.ChildNodes.Count > 0))
                    style = st.ChildNodes[0].Value;
            };

            XmlNode el = l.file.kmlDoc.CreateElement("Placemark");
            el.AppendChild(l.file.kmlDoc.CreateElement("name"));
            el.AppendChild(l.file.kmlDoc.CreateElement("styleUrl").AppendChild(l.file.kmlDoc.CreateTextNode(style)).ParentNode);
            el.AppendChild(l.file.kmlDoc.CreateElement("Point").AppendChild(l.file.kmlDoc.CreateElement("coordinates")).ParentNode);
            el.AppendChild(l.file.kmlDoc.CreateElement("description"));
            st = el.SelectSingleNode("styleUrl");
            XmlNode xy = el.SelectSingleNode("Point/coordinates");
            XmlNode xn = el.SelectSingleNode("name");
            XmlNode xd = el.SelectSingleNode("description");

            string nam = name;
            string x = point.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
            string dw = desc;

            if (InputXY(true, l.file.tmp_file_dir, ref nam, ref y, ref x, ref desc, ref style) == DialogResult.OK)
            {
                bool ch = false;
                if (!String.IsNullOrEmpty(nam)) ch = true;
                if (!String.IsNullOrEmpty(desc)) ch = true;
                x = x.Trim().Replace(",", ".");
                y = y.Trim().Replace(",", ".");
                if (!String.IsNullOrEmpty(x)) ch = true;
                if (!String.IsNullOrEmpty(y)) ch = true;
                if (ch)
                {
                    xn.AppendChild(l.file.kmlDoc.CreateTextNode(nam));
                    xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                    xy.AppendChild(l.file.kmlDoc.CreateTextNode(String.Format("{0},{1},0", x, y)));
                    st.ChildNodes[0].Value = style;
                    xf.AppendChild(el);

                    l.file.SaveKML();
                    l.placemarks++;
                    l.points++;
                    parent.Refresh();
                    laySelect.Items[laySelect.SelectedIndex] = l.ToString();
                    if (objects.Items.Count > 0)
                        objects.Items[objects.Items.Count - 1].Selected = true;
                };
                return true;
            }
            else
                return false;
        }

        private void addNewPointWithNewStyleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "new_"+DateTime.UtcNow.ToString("HHmmss");
            string href = "images/"+style+".png";
            style2image.Add("#"+style, href);
            File.Copy(KMZRebuilederForm.CurrentDirectory() + @"KMZRebuilder.noi.png", l.file.tmp_file_dir + href.Replace("/",@"\"));
            images.Images.Add("images/" + style + ".png", Image.FromFile(l.file.tmp_file_dir + href.Replace("/",@"\")));
            XmlNode ssd = l.file.kmlDoc.SelectSingleNode("kml/Document"); //Style[@id='" + firstsid + "']/IconStyle/Icon/href");
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Style"));
            XmlAttribute attr = l.file.kmlDoc.CreateAttribute("id");
            attr.Value = style;
            ssd.Attributes.Append(attr);
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("IconStyle"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Icon"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("href"));
            ssd.AppendChild(l.file.kmlDoc.CreateTextNode(href));
            style = "#" + style;            

            XmlNode el = l.file.kmlDoc.CreateElement("Placemark");
            el.AppendChild(l.file.kmlDoc.CreateElement("name"));
            el.AppendChild(l.file.kmlDoc.CreateElement("styleUrl").AppendChild(l.file.kmlDoc.CreateTextNode(style)).ParentNode);
            el.AppendChild(l.file.kmlDoc.CreateElement("Point").AppendChild(l.file.kmlDoc.CreateElement("coordinates")).ParentNode);            
            el.AppendChild(l.file.kmlDoc.CreateElement("description"));            
            XmlNode st = el.SelectSingleNode("styleUrl");
            XmlNode xy = el.SelectSingleNode("Point/coordinates");
            XmlNode xn = el.SelectSingleNode("name");
            XmlNode xd = el.SelectSingleNode("description");

            string nam = "Point " + DateTime.Now.ToString("yyyyMMddHHmmss");
            string x = MapViewer.MouseDownDegrees.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = MapViewer.MouseDownDegrees.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string desc = "";
            if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
            string dw = desc;

            if (InputXY(true, l.file.tmp_file_dir, ref nam, ref y, ref x, ref desc, ref style) == DialogResult.OK)
            {
                bool ch = false;
                if (!String.IsNullOrEmpty(nam)) ch = true;
                if (!String.IsNullOrEmpty(desc)) ch = true;
                x = x.Trim().Replace(",", ".");
                y = y.Trim().Replace(",", ".");
                if (!String.IsNullOrEmpty(x)) ch = true;
                if (!String.IsNullOrEmpty(y)) ch = true;
                if (ch)
                {
                    xn.AppendChild(l.file.kmlDoc.CreateTextNode(nam));
                    xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                    xy.AppendChild(l.file.kmlDoc.CreateTextNode(String.Format("{0},{1},0", x, y)));
                    st.ChildNodes[0].Value = style;
                    xf.AppendChild(el);

                    l.file.SaveKML();
                    l.placemarks++;
                    l.points++;
                    parent.Refresh();
                    laySelect.Items[laySelect.SelectedIndex] = l.ToString();
                    if (objects.Items.Count > 0)
                        objects.Items[objects.Items.Count - 1].Selected = true;
                };
            };
        }

        private void addNewPointWithNewIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addNewPointWithNewIconFromFile(MapViewer.MouseDownDegrees);
        }

        private void addNewPointWithNewIconFromFile(PointF click)
        {
            PointF where = click;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Image";
            ofd.DefaultExt = ".png";
            ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif";
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                ofd.Dispose();
                return;
            };            

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "new_" + DateTime.UtcNow.ToString("HHmmss");
            string href = "images/" + style + ".png";
            style2image.Add("#" + style, href);

            ImageMagick.MagickImage im = new ImageMagick.MagickImage(ofd.FileName);
            ofd.Dispose();
            if ((im.Width > 32) || (im.Height > 32))
            {
                im.Resize(32, 32);
                MapIcons.SaveIcon(im, l.file.tmp_file_dir + href.Replace("/", @"\"));
            }
            else
                MapIcons.SaveIcon(ofd.FileName, l.file.tmp_file_dir + href.Replace("/", @"\"));
            images.Images.Add("images/" + style + ".png", im.ToBitmap());
            im.Dispose();
            XmlNode ssd = l.file.kmlDoc.SelectSingleNode("kml/Document"); //Style[@id='" + firstsid + "']/IconStyle/Icon/href");
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Style"));
            XmlAttribute attr = l.file.kmlDoc.CreateAttribute("id");
            attr.Value = style;
            ssd.Attributes.Append(attr);
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("IconStyle"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Icon"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("href"));
            ssd.AppendChild(l.file.kmlDoc.CreateTextNode(href));
            style = "#" + style;

            XmlNode el = l.file.kmlDoc.CreateElement("Placemark");
            el.AppendChild(l.file.kmlDoc.CreateElement("name"));
            el.AppendChild(l.file.kmlDoc.CreateElement("styleUrl").AppendChild(l.file.kmlDoc.CreateTextNode(style)).ParentNode);
            el.AppendChild(l.file.kmlDoc.CreateElement("Point").AppendChild(l.file.kmlDoc.CreateElement("coordinates")).ParentNode);            
            el.AppendChild(l.file.kmlDoc.CreateElement("description"));            
            XmlNode st = el.SelectSingleNode("styleUrl");
            XmlNode xy = el.SelectSingleNode("Point/coordinates");
            XmlNode xn = el.SelectSingleNode("name");
            XmlNode xd = el.SelectSingleNode("description");

            string nam = "Point " + DateTime.Now.ToString("yyyyMMddHHmmss");
            string x = where.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = where.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string desc = "";
            if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
            string dw = desc;

            if (InputXY(true, l.file.tmp_file_dir, ref nam, ref y, ref x, ref desc, ref style) == DialogResult.OK)
            {
                bool ch = false;
                if (!String.IsNullOrEmpty(nam)) ch = true;
                if (!String.IsNullOrEmpty(desc)) ch = true;
                x = x.Trim().Replace(",", ".");
                y = y.Trim().Replace(",", ".");
                if (!String.IsNullOrEmpty(x)) ch = true;
                if (!String.IsNullOrEmpty(y)) ch = true;
                if (ch)
                {
                    xn.AppendChild(l.file.kmlDoc.CreateTextNode(nam));
                    xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                    xy.AppendChild(l.file.kmlDoc.CreateTextNode(String.Format("{0},{1},0", x, y)));
                    st.ChildNodes[0].Value = style;
                    xf.AppendChild(el);

                    l.file.SaveKML();
                    l.placemarks++;
                    l.points++;
                    parent.Refresh();
                    laySelect.Items[laySelect.SelectedIndex] = l.ToString();
                    if (objects.Items.Count > 0)
                        objects.Items[objects.Items.Count - 1].Selected = true;
                };
            };
        }

        private void changeIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedIndices.Count == 0) return;

            /////////////

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Image";
            ofd.DefaultExt = ".png";
            ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif";
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                ofd.Dispose();
                return;
            };            

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "new_" + DateTime.UtcNow.ToString("HHmmss");
            string href = "images/" + style + ".png";
            style2image.Add("#" + style, href);

            ImageMagick.MagickImage im = new ImageMagick.MagickImage(ofd.FileName);
            ofd.Dispose();
            if ((im.Width > 32) || (im.Height > 32))
            {
                im.Resize(32, 32);
                MapIcons.SaveIcon(im, l.file.tmp_file_dir + href.Replace("/", @"\"));
            }
            else
                MapIcons.SaveIcon(ofd.FileName, l.file.tmp_file_dir + href.Replace("/", @"\"));
            images.Images.Add("images/" + style + ".png", im.ToBitmap());
            im.Dispose();
            XmlNode ssd = l.file.kmlDoc.SelectSingleNode("kml/Document"); //Style[@id='" + firstsid + "']/IconStyle/Icon/href");
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Style"));
            XmlAttribute attr = l.file.kmlDoc.CreateAttribute("id");
            attr.Value = style;
            ssd.Attributes.Append(attr);
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("IconStyle"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Icon"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("href"));
            ssd.AppendChild(l.file.kmlDoc.CreateTextNode(href));
            style = "#" + style;

            /////////////

            string XPath = objects.SelectedItems[0].SubItems[7].Text;
            string indx = XPath.Substring(XPath.IndexOf("["));
            XPath = XPath.Remove(XPath.IndexOf("["));
            int ind = int.Parse(indx.Substring(1, indx.Length - 2));
            XmlNode st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
            if ((st != null) && (st.ChildNodes.Count > 0))
                st.ChildNodes[0].Value = style;
            else
            {
                if(st == null)
                    st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.AppendChild(l.file.kmlDoc.CreateElement("styleUrl"));
                st.AppendChild(l.file.kmlDoc.CreateTextNode(style));
            };
            NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
            mo.Img = images.Images[style2image[style]];
            objects.SelectedItems[0].ImageIndex = images.Images.IndexOfKey(style2image[style]);
            l.file.SaveKML();
            MapViewer.DrawOnMapData();
        }

        private void setNewIconToAllTheSameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedIndices.Count == 0) return;

            /////////////

            string style = "";
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            string XPath = objects.SelectedItems[0].SubItems[7].Text;
            string indx = XPath.Substring(XPath.IndexOf("["));
            XPath = XPath.Remove(XPath.IndexOf("["));
            int ind = int.Parse(indx.Substring(1, indx.Length - 2));
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];            
            XmlNode st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
            if ((st != null) && (st.ChildNodes.Count > 0))
                style = st.ChildNodes[0].Value;
            if (String.IsNullOrEmpty(style)) return;

            style = style.Replace("#", "");
            XmlNode nts = l.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + style + "']/IconStyle/Icon/href");
            string href = null;
            if((nts != null) && (nts.ChildNodes.Count > 0))
                href = nts.ChildNodes[0].Value;
            else
                return;
            string file_name = l.file.tmp_file_dir + href.Replace("/", @"\");

            /////////////

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Image";
            ofd.DefaultExt = ".png";
            ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif";
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                ofd.Dispose();
                return;
            };

            ImageMagick.MagickImage im = new ImageMagick.MagickImage(ofd.FileName);
            ofd.Dispose();
            if ((im.Width > 32) || (im.Height > 32))
            {
                im.Resize(32, 32);
                MapIcons.SaveIcon(im, file_name);
            }
            else
                MapIcons.SaveIcon(ofd.FileName, file_name);
            im.Dispose();

            /////////////

            int si = objects.SelectedIndices[0];
            laySelect_SelectedIndexChanged(sender, e);
            objects.Items[si].Selected = true;
        }

        private void sWAPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mcmn.Checked)
                mcme_Click(sender, e);
            else
                mcmn_Click(sender, e);
        }

        private void switchToNavigationModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mcmn_Click(sender, e);
        }

        private void addNewPointWithNewIconFromListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PointF where = MapViewer.MouseDownDegrees;

            if (parent.mapIcons == null)
                parent.mapIcons = new MapIcons();
            DialogResult dr = parent.mapIcons.ShowDialog();
            if (dr == DialogResult.Ignore)
                addNewPointWithNewIconFromFile(where);
            if (dr != DialogResult.OK) return;            

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "new_" + DateTime.UtcNow.ToString("HHmmss");
            string href = "images/" + style + ".png";
            style2image.Add("#" + style, href);
            
            ImageMagick.MagickImage im = new ImageMagick.MagickImage((Bitmap)parent.mapIcons.SelectedImage);
            if ((im.Width > 32) || (im.Height > 32))
            {
                im.Resize(32, 32);
                MapIcons.SaveIcon(im, l.file.tmp_file_dir + href.Replace("/", @"\"));
            }
            else
                MapIcons.SaveIcon(parent.mapIcons.SelectedImageArr,l.file.tmp_file_dir + href.Replace("/", @"\"));
            images.Images.Add("images/" + style + ".png", im.ToBitmap());
            im.Dispose();
            XmlNode ssd = l.file.kmlDoc.SelectSingleNode("kml/Document"); //Style[@id='" + firstsid + "']/IconStyle/Icon/href");
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Style"));
            XmlAttribute attr = l.file.kmlDoc.CreateAttribute("id");
            attr.Value = style;
            ssd.Attributes.Append(attr);
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("IconStyle"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Icon"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("href"));
            ssd.AppendChild(l.file.kmlDoc.CreateTextNode(href));
            style = "#" + style;

            XmlNode el = l.file.kmlDoc.CreateElement("Placemark");
            el.AppendChild(l.file.kmlDoc.CreateElement("name"));
            el.AppendChild(l.file.kmlDoc.CreateElement("styleUrl").AppendChild(l.file.kmlDoc.CreateTextNode(style)).ParentNode);
            el.AppendChild(l.file.kmlDoc.CreateElement("Point").AppendChild(l.file.kmlDoc.CreateElement("coordinates")).ParentNode);            
            el.AppendChild(l.file.kmlDoc.CreateElement("description"));            
            XmlNode st = el.SelectSingleNode("styleUrl");
            XmlNode xy = el.SelectSingleNode("Point/coordinates");
            XmlNode xn = el.SelectSingleNode("name");
            XmlNode xd = el.SelectSingleNode("description");

            string nam = "Point " + DateTime.Now.ToString("yyyyMMddHHmmss");
            string x = where.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = where.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string desc = "";
            if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
            string dw = desc;

            if (InputXY(true, l.file.tmp_file_dir, ref nam, ref y, ref x, ref desc, ref style) == DialogResult.OK)
            {
                bool ch = false;
                if (!String.IsNullOrEmpty(nam)) ch = true;
                if (!String.IsNullOrEmpty(desc)) ch = true;
                x = x.Trim().Replace(",", ".");
                y = y.Trim().Replace(",", ".");
                if (!String.IsNullOrEmpty(x)) ch = true;
                if (!String.IsNullOrEmpty(y)) ch = true;
                if (ch)
                {
                    xn.AppendChild(l.file.kmlDoc.CreateTextNode(nam));
                    xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                    xy.AppendChild(l.file.kmlDoc.CreateTextNode(String.Format("{0},{1},0", x, y)));
                    st.ChildNodes[0].Value = style;
                    xf.AppendChild(el);

                    l.file.SaveKML();
                    l.placemarks++;
                    l.points++;
                    parent.Refresh();
                    laySelect.Items[laySelect.SelectedIndex] = l.ToString();
                    if (objects.Items.Count > 0)
                        objects.Items[objects.Items.Count - 1].Selected = true;
                };
            };
        }

        private void setNewIconFromListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedIndices.Count == 0) return;

            /////////////

            if (parent.mapIcons == null)
                parent.mapIcons = new MapIcons();
            DialogResult dr = parent.mapIcons.ShowDialog();
            if (dr == DialogResult.Ignore)
                changeIconToolStripMenuItem_Click(sender, e);
            if (dr != DialogResult.OK) return;                                    

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "new_" + DateTime.UtcNow.ToString("HHmmss");
            string href = "images/" + style + ".png";
            style2image.Add("#" + style, href);

            ImageMagick.MagickImage im = new ImageMagick.MagickImage((Bitmap)parent.mapIcons.SelectedImage);
            if ((im.Width > 32) || (im.Height > 32))
            {
                im.Resize(32, 32);
                MapIcons.SaveIcon(im, l.file.tmp_file_dir + href.Replace("/", @"\"));
            }
            else
                MapIcons.SaveIcon(parent.mapIcons.SelectedImageArr, l.file.tmp_file_dir + href.Replace("/", @"\"));
            images.Images.Add("images/" + style + ".png", im.ToBitmap());
            im.Dispose();
            XmlNode ssd = l.file.kmlDoc.SelectSingleNode("kml/Document"); //Style[@id='" + firstsid + "']/IconStyle/Icon/href");
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Style"));
            XmlAttribute attr = l.file.kmlDoc.CreateAttribute("id");
            attr.Value = style;
            ssd.Attributes.Append(attr);
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("IconStyle"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Icon"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("href"));
            ssd.AppendChild(l.file.kmlDoc.CreateTextNode(href));
            style = "#" + style;

            /////////////

            string XPath = objects.SelectedItems[0].SubItems[7].Text;
            string indx = XPath.Substring(XPath.IndexOf("["));
            XPath = XPath.Remove(XPath.IndexOf("["));
            int ind = int.Parse(indx.Substring(1, indx.Length - 2));
            XmlNode st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
            if ((st != null) && (st.ChildNodes.Count > 0))
                st.ChildNodes[0].Value = style;
            else
            {
                if (st == null)
                    st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.AppendChild(l.file.kmlDoc.CreateElement("styleUrl"));
                st.AppendChild(l.file.kmlDoc.CreateTextNode(style));
            };
            NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
            mo.Img = images.Images[style2image[style]];
            objects.SelectedItems[0].ImageIndex = images.Images.IndexOfKey(style2image[style]);
            l.file.SaveKML();
            MapViewer.DrawOnMapData();
        }

        private void setNewIconFromListToAllWithSameImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedIndices.Count == 0) return;

            /////////////

            string style = "";
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            string XPath = objects.SelectedItems[0].SubItems[7].Text;
            string indx = XPath.Substring(XPath.IndexOf("["));
            XPath = XPath.Remove(XPath.IndexOf("["));
            int ind = int.Parse(indx.Substring(1, indx.Length - 2));
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            XmlNode st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
            if ((st != null) && (st.ChildNodes.Count > 0))
                style = st.ChildNodes[0].Value;
            if (String.IsNullOrEmpty(style)) return;

            style = style.Replace("#", "");
            XmlNode nts = l.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + style + "']/IconStyle/Icon/href");
            string href = null;
            if ((nts != null) && (nts.ChildNodes.Count > 0))
                href = nts.ChildNodes[0].Value;
            else
                return;
            string file_name = l.file.tmp_file_dir + href.Replace("/", @"\");

            /////////////

            if (parent.mapIcons == null)
                parent.mapIcons = new MapIcons();
            DialogResult dr = parent.mapIcons.ShowDialog();
            if (dr == DialogResult.Ignore)
                setNewIconToAllTheSameToolStripMenuItem_Click(sender, e);
            if (dr != DialogResult.OK) return;

            ImageMagick.MagickImage im = new ImageMagick.MagickImage((Bitmap)parent.mapIcons.SelectedImage);
            if ((im.Width > 32) || (im.Height > 32))
            {
                im.Resize(32, 32);
                MapIcons.SaveIcon(im, file_name);
            }
            else
                MapIcons.SaveIcon(parent.mapIcons.SelectedImageArr, file_name);
            im.Dispose();

            /////////////

            int si = objects.SelectedIndices[0];
            laySelect_SelectedIndexChanged(sender, e);
            objects.Items[si].Selected = true;
        }

        private void addTextAsPolygonHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTextAsPolyF(true);
        }

        private void addTextAsPolylineHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTextAsPolyF(false);
        }

        private void AddTextAsPolyF(bool ispolygon)
        {
            PointF where = MapViewer.MouseDownDegrees;
            uint lm = GetLengthMetersC(where.Y - 0.005, where.X, where.Y + 0.005, where.X, false);
            float scale_d = 0.01f / (float)lm;

            AddTextAsPoly atap = new AddTextAsPoly(KMZRebuilederForm.CurrentDirectory()+@"\Fonts\");
            if (ispolygon)
            {
                atap.LBColor.BackColor = Color.FromArgb(255, Color.Red);
                atap.LColor.Text = LineAreaStyleForm.HexConverter(Color.Red);
                atap.ABColor.BackColor = Color.FromArgb(255, Color.Lime);
                atap.AColor.Text = LineAreaStyleForm.HexConverter(Color.Lime);
                atap.Text = "Add Text as Polygone";
            }
            else
            {
                atap.LBColor.BackColor = Color.FromArgb(255, Color.Blue);
                atap.LColor.Text = LineAreaStyleForm.HexConverter(Color.Blue);
                atap.Text = "Add Text as Polyline";
                atap.AColor.Enabled = false;
                atap.ABColor.Enabled = false;
                atap.AOpacity.Enabled = false;
                atap.AFill.Enabled = false;
            };
            if ((atap.ShowDialog() != DialogResult.OK) || (String.IsNullOrEmpty(atap.TextOut.Text.Trim())))
            {
                atap.Dispose();
                return;
            };

            FontFamily font = null;
            if (atap.fontCustom.Checked && (atap.fontCustomList.SelectedIndex >= 0))
                font = ((AddTextAsPoly.FontRec)atap.fontCustomList.Items[atap.fontCustomList.SelectedIndex]).font;
            if (atap.fontSystem.Checked && (atap.fontSysList.SelectedIndex >= 0))
                font = ((AddTextAsPoly.FontRec)atap.fontSysList.Items[atap.fontSysList.SelectedIndex]).font;
            if (font == null)
            {
                atap.Dispose();
                return;
            };
            float scale = scale_d * (float)atap.TextSize.Value;
            string text = atap.TextOut.Text.Trim();
            float angle = 90 - (float)atap.TextAzimuth.Value;
            FontPath.PathOffset offset = (FontPath.PathOffset)atap.TextAlign.SelectedIndex;
            if (offset == FontPath.PathOffset.TopLeft) // Flip vertical align
                offset = FontPath.PathOffset.BottomLeft;
            else if (offset == FontPath.PathOffset.TopMiddle)
                offset = FontPath.PathOffset.BottomMiddle;
            else if (offset == FontPath.PathOffset.TopRight)
                offset = FontPath.PathOffset.BottomRight;
            else if (offset == FontPath.PathOffset.BottomLeft)
                offset = FontPath.PathOffset.TopLeft;
            else if (offset == FontPath.PathOffset.BottomMiddle)
                offset = FontPath.PathOffset.TopMiddle;
            else if (offset == FontPath.PathOffset.BottomRight)
                offset = FontPath.PathOffset.TopRight;
            Color lineColor = Color.FromArgb((int)((float)atap.LOpacity.Value / 100f * 255f), atap.LBColor.BackColor);
            int lineWidth = (int)atap.LWidth.Value;
            Color fillColor = Color.FromArgb((int)((float)atap.AOpacity.Value / 100f * 255f), atap.ABColor.BackColor);
            int fill = atap.AFill.SelectedIndex;
            atap.Dispose();            
            PointF startPoint = new PointF(where.X,where.Y);

            Application.DoEvents();
            wbf.Show("Add Text as Shape", "Wait, calculating points...");
            FontPath.PointD[][] pathes = null;
            try
            {
                pathes = FontPath.StringToPath(font, text, scale, startPoint, offset, true, false, angle);
            }
            catch (Exception ex)
            {
                wbf.Hide();
                Application.DoEvents();
                MessageBox.Show("Text Error: " + ex.Message, "Add Text as Shape", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            string[] PolygonStrings = null;
            if(ispolygon)
                PolygonStrings = FontPath.PathToPolygonString(pathes);
            else
                PolygonStrings = FontPath.PathToLineString(pathes);
            string XML = "";
            int count = 0;
            string stylen = "newstyle" + DateTime.UtcNow.ToString("HHmmss");
            foreach (string coords in PolygonStrings)
            {
                count++;
                XML += "<Placemark>\r\n";
                XML += "<name><![CDATA[" + String.Format("{0} {1}/{2}", text, count, PolygonStrings.Length) + "]]></name>\r\n";
                XML += "<styleUrl>#" + stylen + "</styleUrl>\r\n";
                XML += "<description><![CDATA[" + String.Format("Part {1}/{2} of {0}", text, count, PolygonStrings.Length) + "]]></description>\r\n";
                if (ispolygon)
                {                                        
                    XML += "<Polygon><extrude>1</extrude><outerBoundaryIs><LinearRing>\r\n";
                    XML += "<coordinates>" + coords + "</coordinates>\r\n";
                    XML += "</LinearRing></outerBoundaryIs></Polygon>\r\n";
                }
                else
                {
                    XML += "<LineString>\r\n";
                    XML += "<coordinates>" + coords + "</coordinates>\r\n";
                    XML += "</LineString>\r\n";
                };
                XML += "</Placemark>\r\n";
            };
            wbf.Hide();
            
            string style = "<Style id=\"" + stylen + "\"><LineStyle><color>" + AddTextAsPoly.HexStyleConverter(lineColor) + "</color><width>" + lineWidth.ToString() + "</width></LineStyle>";
            if(ispolygon)
                style += "<PolyStyle><color>" + AddTextAsPoly.HexStyleConverter(fillColor) + "</color><fill>" + fill.ToString() + "</fill></PolyStyle>";
            style += "</Style>\r\n";

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            xf.InnerXml += XML;
            l.file.kmlDoc.SelectNodes("kml/Document")[0].InnerXml += style;            

            l.file.SaveKML();
            l.placemarks += PolygonStrings.Length;
            if(ispolygon)
                l.areas += PolygonStrings.Length;
            else
                l.lines += PolygonStrings.Length;
            parent.Refresh();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString();            
        }        

        private void LPSB_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (objects.SelectedItems[0].SubItems[1].Text == "Point") return;
            bool ispolygon = false;

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            string XPath = objects.SelectedItems[0].SubItems[7].Text;
            string indx = XPath.Substring(XPath.IndexOf("["));
            XPath = XPath.Remove(XPath.IndexOf("["));
            int ind = int.Parse(indx.Substring(1, indx.Length - 2));
            XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            string styleUrl = "";
            XmlNode x_placemark = x_folder.SelectNodes(XPath)[ind].ParentNode.ParentNode;
            if (x_placemark.Name == "outerBoundaryIs")
            {
                x_placemark = x_placemark.ParentNode.ParentNode;
                ispolygon = true;
            };
            XmlNode x_style = x_placemark.SelectSingleNode("styleUrl");
            if ((x_style != null) && (x_style.ChildNodes.Count > 0))
                styleUrl = x_style.ChildNodes[0].Value;
            XmlNode x_linestyle = null;
            XmlNode x_areastyle = null;

            if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);

            Color lineColor = Color.FromArgb(255, Color.Blue);
            int lineWidth = 3;
            Color fillColor = Color.FromArgb(255, Color.Blue);
            int fill = 1;
            
            if (styleUrl != "")
            {
                string firstsid = styleUrl;
                XmlNodeList pk = l.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']/Pair/key");
                if (pk.Count > 0)
                    for (int n = 0; n < pk.Count; n++)
                    {
                        XmlNode cn = pk[n];
                        if ((cn.ChildNodes[0].Value != "normal") && (n > 0)) continue;
                        if (cn.ParentNode.SelectSingleNode("styleUrl") == null) continue;
                        firstsid = cn.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                        if (firstsid.IndexOf("#") == 0) firstsid = firstsid.Remove(0, 1);
                    };
                try
                {
                    x_linestyle = l.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']/LineStyle");
                }
                catch { };
                try
                {
                    x_areastyle = l.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']/PolyStyle");
                }
                catch { };
            }
            else
            {
                x_linestyle = x_placemark.SelectSingleNode("Style/LineStyle");
                x_areastyle = x_placemark.SelectSingleNode("Style/PolyStyle");
            };
            if (x_linestyle != null)
            {
                string colval = x_linestyle.SelectSingleNode("color").ChildNodes[0].Value;
                try
                {
                    lineColor = Color.FromName(colval);
                    if (colval.Length == 8)
                    {
                        lineColor = Color.FromArgb(
                            Convert.ToInt32(colval.Substring(0, 2), 16),
                            Convert.ToInt32(colval.Substring(6, 2), 16),
                            Convert.ToInt32(colval.Substring(4, 2), 16),
                            Convert.ToInt32(colval.Substring(2, 2), 16)
                            );
                    };
                }
                catch { };
                string widval = x_linestyle.SelectSingleNode("width").ChildNodes[0].Value;
                try
                {
                    lineWidth = (int)double.Parse(widval, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch { };
            };
            if (x_areastyle != null)
            {
                string colval = x_areastyle.SelectSingleNode("color").ChildNodes[0].Value;
                try
                {
                    fillColor = Color.FromName(colval);
                    if (colval.Length == 8)
                    {
                        fillColor = Color.FromArgb(
                            Convert.ToInt32(colval.Substring(0, 2), 16),
                            Convert.ToInt32(colval.Substring(6, 2), 16),
                            Convert.ToInt32(colval.Substring(4, 2), 16),
                            Convert.ToInt32(colval.Substring(2, 2), 16)
                            );
                    };
                }
                catch { };
                string fillval = x_areastyle.SelectSingleNode("fill").ChildNodes[0].Value;
                try
                {
                   fill = int.Parse(fillval, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch { };
            };

            LineAreaStyleForm lasf = new LineAreaStyleForm();
            lasf.LBColor.BackColor = Color.FromArgb(255, lineColor);
            lasf.LColor.Text = LineAreaStyleForm.HexConverter(lineColor);
            lasf.LOpacity.Value = (int)((float)lineColor.A / 255f * 100f);
            lasf.LWidth.Value = lineWidth;
            lasf.ApplyTo.SelectedIndex = 1;
            if (ispolygon)
            {
                lasf.ABColor.BackColor = Color.FromArgb(255, fillColor);
                lasf.AColor.Text = LineAreaStyleForm.HexConverter(fillColor);
                lasf.AOpacity.Value = (int)((float)fillColor.A / 255f * 100f);
                lasf.AFill.SelectedIndex = fill == 1 ? 1 : 0;
            }
            else
            {
                lasf.AColor.Enabled = false;
                lasf.ABColor.Enabled = false;
                lasf.AOpacity.Enabled = false;
                lasf.AFill.Enabled = false;
            };
            if (lasf.ShowDialog() != DialogResult.OK)
            {
                lasf.Dispose();
                return;
            };

            lineColor = Color.FromArgb((int)((float)lasf.LOpacity.Value / 100f * 255f),lasf.LBColor.BackColor);
            lineWidth = (int)lasf.LWidth.Value;
            fillColor = Color.FromArgb((int)((float)lasf.AOpacity.Value / 100f * 255f), lasf.ABColor.BackColor);
            fill = lasf.AFill.SelectedIndex;
            if(true) // todo // if (lasf.ApplyTo.SelectedIndex == 1)
            {
                NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];

                foreach (XmlNode n in x_placemark.SelectNodes("styleUrl"))
                    n.ParentNode.RemoveChild(n);
                foreach (XmlNode n in x_placemark.SelectNodes("Style"))
                    n.ParentNode.RemoveChild(n);
                if ((lasf.ApplyTo.SelectedIndex == 0) || ((lasf.ApplyTo.SelectedIndex == 1) && String.IsNullOrEmpty(styleUrl)))
                        styleUrl = "newStyle" + DateTime.UtcNow.ToString("HHmmss");

                x_style = l.file.kmlDoc.CreateElement("styleUrl");
                x_style.AppendChild(l.file.kmlDoc.CreateTextNode("#" + styleUrl));
                x_placemark.AppendChild(x_style);

                if (lasf.ApplyTo.SelectedIndex == 1)
                {
                    foreach (XmlNode n in l.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']"))
                        n.ParentNode.RemoveChild(n);
                    if (x_linestyle != null)
                        x_linestyle.ParentNode.ParentNode.RemoveChild(x_linestyle.ParentNode);
                    if (x_areastyle != null)
                        if(x_areastyle.ParentNode.ParentNode != null)
                            x_areastyle.ParentNode.ParentNode.RemoveChild(x_areastyle.ParentNode);
                };

                string style = "";
                if (ispolygon)
                {
                    (mo as NaviMapNet.MapPolygon).BorderColor = lineColor;
                    (mo as NaviMapNet.MapPolygon).Width = lineWidth;
                    (mo as NaviMapNet.MapPolygon).BodyColor = fillColor;
                    style = "<Style id=\"" + styleUrl + "\"><LineStyle><color>" + LineAreaStyleForm.HexStyleConverter(lineColor) + "</color><width>" + lineWidth.ToString() + "</width></LineStyle><PolyStyle><color>" + LineAreaStyleForm.HexStyleConverter(fillColor) + "</color><fill>" + fill.ToString() + "</fill></PolyStyle></Style>\r\n";
                }
                else
                {
                    (mo as NaviMapNet.MapPolyLine).Color = lineColor;
                    (mo as NaviMapNet.MapPolyLine).Width = lineWidth;
                    style = "<Style id=\"" + styleUrl + "\"><LineStyle><color>" + LineAreaStyleForm.HexStyleConverter(lineColor) + "</color><width>" + lineWidth.ToString() + "</width></LineStyle></Style>\r\n";
                };

                l.file.kmlDoc.SelectSingleNode("kml/Document").InnerXml += style;
                l.file.SaveKML();
                if (lasf.ApplyTo.SelectedIndex == 0)
                {
                    MapViewer.DrawOnMapData();
                    int index = objects.SelectedIndices[0];

                    Image im = new Bitmap(16, 16);
                    Graphics g = Graphics.FromImage(im);
                    if (ispolygon)
                    {
                        g.FillRectangle(new SolidBrush(fillColor), 0, 0, 16, 16);
                        g.DrawRectangle(new Pen(new SolidBrush(lineColor), 2), 0, 0, 16, 16);
                        g.DrawString("A", new Font("Terminal", 11, FontStyle.Bold), new SolidBrush(Color.FromArgb(255 - fillColor.R, 255 - fillColor.G, 255 - fillColor.B)), 1, -1);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(lineColor), 0, 0, 16, 16);
                        g.DrawString("L", new Font("Terminal", 11, FontStyle.Bold), new SolidBrush(Color.FromArgb(255 - lineColor.R, 255 - lineColor.G, 255 - lineColor.B)), 1, -1);
                    };
                    g.Dispose();
                    images.Images[index] = im;
                }
                else
                {
                    int index = objects.SelectedIndices[0];
                    laySelect_SelectedIndexChanged(sender, e);
                    objects.Items[index].Selected = true;
                };
            };

            lasf.Dispose();
        }

        private void selectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            objects.SelectedItems.Clear();
            //if (objects.CheckedIndices.Count > 0)
            //    for (int i = objects.CheckedIndices.Count - 1; i >= 0; i--)
            //        objects.CheckedItems[i].Checked = false;
            //objects.CheckBoxes = false;
            mapSelect.Clear();
            MapViewer.DrawOnMapData();
        }

        private void selectNoneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            objects.SelectedItems.Clear();
            //if (objects.CheckedIndices.Count > 0)
            //    for (int i = objects.CheckedIndices.Count - 1; i >= 0; i--)
            //        objects.CheckedItems[i].Checked = false;
            //objects.CheckBoxes = false;
            mapSelect.Clear();
            MapViewer.DrawOnMapData();
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            navigateToToolStripMenuItem.Enabled = xuns.Count > 0;
            SASPlacemarkConnector sc = new SASPlacemarkConnector();
            addNewPointToSASPlanetToolStripMenuItem.Enabled = sc.SASisOpen;
            importDataFromOSMInsideSelectionToolStripMenuItem.Enabled = MapViewer.SelectionBoxIsVisible;
            selectAllInAreaToolStripMenuItem.Enabled = MapViewer.SelectionBoxIsVisible && (objects.Items.Count > 0);
            findAllPlacemarksOutsideAreaToolStripMenuItem.Enabled = MapViewer.SelectionBoxIsVisible && (objects.Items.Count > 0);
            clearSelectionBoxToolStripMenuItem.Enabled = MapViewer.SelectionBoxIsVisible;
        }

        private void navigateToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sel = -1;
            string[] xunn = new string[xuns.Count];
            for (int i = 0; i < xuns.Count; i++) xunn[i] = xuns[i].ToString();
            if (System.Windows.Forms.InputBox.Show("Navigate", "Select preset:", xunn, ref sel) == DialogResult.OK)
                MapViewer.CenterDegrees = new PointF((float)xuns[sel].lon, (float)xuns[sel].lat);
        }

        private void cHB0_Click(object sender, EventArgs e)
        {
            if (!objects.CheckBoxes) return;
            if (objects.CheckedItems.Count == 0) return;

            for (int i = 0; i < objects.CheckedItems.Count; i++)
            {
                objects.CheckedItems[i].SubItems[6].Text = "Yes";
                objects.CheckedItems[i].Font = new Font(objects.CheckedItems[i].Font, FontStyle.Strikeout);
                //objects.SelectedItems[0].SubItems[6].Text = (objects.SelectedItems[0].SubItems[6].Text == "Yes") ? "" : "Yes";
                //if (objects.SelectedItems[0].SubItems[6].Text == "Yes")
                //    objects.SelectedItems[0].Font = new Font(objects.SelectedItems[0].Font, FontStyle.Strikeout);
                //else
                //    objects.SelectedItems[0].Font = new Font(objects.SelectedItems[0].Font, FontStyle.Regular);
            };

            CheckMarked();
        }

        private void cHB1_Click(object sender, EventArgs e)
        {
            if (!objects.CheckBoxes) return;
            if (objects.CheckedItems.Count == 0) return;
            if (laySelect.Items.Count < 2) return;

            string[] layers = new string[laySelect.Items.Count];
            for (int i = 0; i < laySelect.Items.Count; i++)
                layers[i] = laySelect.Items[i].ToString();

            int new_ind = laySelect.SelectedIndex;
            if (System.Windows.Forms.InputBox.Show("Move Placemarks", "Select Layer:", layers, ref new_ind) != DialogResult.OK) return;
            if (new_ind == laySelect.SelectedIndex) return;

            if (MessageBox.Show("Move " + objects.CheckedItems.Count.ToString() + " placemark(s) to layer:\r\n" + parent.kmzLayers.Items[new_ind].ToString(), "Move placemarks", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            int[] pla = new int[3];
            KMLayer l_old = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            KMLayer l_new = (KMLayer)parent.kmzLayers.Items[new_ind];
            for (int i = objects.CheckedItems.Count - 1; i >= 0 ; i--)
            {
                string XPath = objects.CheckedItems[i].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                XmlNode xn = l_old.file.kmlDoc.SelectNodes("kml/Document/Folder")[l_old.id];
                xn = xn.SelectNodes(XPath)[ind].ParentNode.ParentNode;
                if (xn.Name == "outerBoundaryIs") // polygon
                {
                    xn = xn.ParentNode.ParentNode;
                    pla[2]++;
                }
                else
                {
                    if (xn.SelectSingleNode("Point") != null) pla[0]++;
                    if (xn.SelectSingleNode("LineString") != null) pla[1]++;
                };
                xn = xn.ParentNode.RemoveChild(xn);

                if (l_old.file == l_new.file)
                    l_new.file.kmlDoc.SelectNodes("kml/Document/Folder")[l_new.id].AppendChild(xn);
                else
                {
                    // copy styles //
                    string styleUrl = "";
                    if (xn.SelectSingleNode("styleUrl") != null) styleUrl = xn.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                    if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);

                    if (styleUrl != "")
                    {
                        string firstsid = styleUrl;
                        XmlNodeList pk = l_old.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']");
                        if (pk.Count > 0) // copy style map
                        {
                            if (l_new.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']").Count == 0)
                                l_new.file.kmlDoc.SelectSingleNode("kml/Document").InnerXml += pk[0].OuterXml;
                        };
                        pk = l_old.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']/Pair/key");
                        if (pk.Count > 0)
                            for (int n = 0; n < pk.Count; n++)
                            {
                                XmlNode cn = pk[n];
                                if ((cn.ChildNodes[0].Value != "normal") && (n > 0)) continue;
                                if (cn.ParentNode.SelectSingleNode("styleUrl") == null) continue;
                                firstsid = cn.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                                if (firstsid.IndexOf("#") == 0) firstsid = firstsid.Remove(0, 1);                                
                            };
                        try // copy style
                        {                            
                            XmlNode nts = l_old.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']");
                            if (l_new.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + firstsid + "']").Count == 0)
                                l_new.file.kmlDoc.SelectSingleNode("kml/Document").InnerXml += nts.OuterXml;
                        }
                        catch { };
                        try // copy icons
                        {
                            XmlNode nts = l_old.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']/IconStyle/Icon/href");
                            string href = nts.ChildNodes[0].Value;
                            if (!String.IsNullOrEmpty(href))
                            {
                                href = href.Replace("/", @"\");
                                System.IO.File.Copy(l_old.file.tmp_file_dir + href, l_new.file.tmp_file_dir + href, false);
                            };
                        }
                        catch { };
                    };
                    /////////////////
                    l_new.file.kmlDoc.SelectNodes("kml/Document/Folder")[l_new.id].InnerXml += xn.OuterXml;                    
                };
            };
            l_old.file.SaveKML();
            if (l_old.file != l_new.file)
                l_new.file.SaveKML();            
            laySelect.Items[new_ind] = l_new.ToString();
            laySelect.Items[laySelect.SelectedIndex] = l_old.ToString();
            l_new.placemarks += pla[0] + pla[1] + pla[2];
            l_new.points += pla[0];
            l_new.lines += pla[1];
            l_new.areas += pla[2];
            l_old.placemarks -= pla[0] + pla[1] + pla[2];
            l_old.points -= pla[0];
            l_old.lines -= pla[1];
            l_old.areas -= pla[2];            

            laySelect.Items[laySelect.SelectedIndex] = l_old.ToString();
            laySelect.Items[new_ind] = l_new.ToString();
            parent.Refresh();
        }

        private void cHB2_Click(object sender, EventArgs e)
        {
            if (!objects.CheckBoxes) return;
            if (objects.CheckedItems.Count == 0) return;

            for (int i = 0; i < objects.CheckedItems.Count; i++)
            {
                objects.CheckedItems[i].SubItems[6].Text = "";
                objects.CheckedItems[i].Font = new Font(objects.CheckedItems[i].Font, FontStyle.Regular);
                //objects.SelectedItems[0].SubItems[6].Text = (objects.SelectedItems[0].SubItems[6].Text == "Yes") ? "" : "Yes";
                //if (objects.SelectedItems[0].SubItems[6].Text == "Yes")
                //    objects.SelectedItems[0].Font = new Font(objects.SelectedItems[0].Font, FontStyle.Strikeout);
                //else
                //    objects.SelectedItems[0].Font = new Font(objects.SelectedItems[0].Font, FontStyle.Regular);
            };

            CheckMarked();
        }

        private void cHB3_Click(object sender, EventArgs e)
        {
            if (!objects.CheckBoxes) return;
            if (objects.CheckedItems.Count == 0) return;

            for (int i = 0; i < objects.CheckedItems.Count; i++)
            {
                objects.CheckedItems[i].SubItems[6].Text = (objects.CheckedItems[i].SubItems[6].Text == "Yes") ? "" : "Yes";
                if (objects.CheckedItems[i].SubItems[6].Text == "Yes")
                    objects.CheckedItems[i].Font = new Font(objects.CheckedItems[i].Font, FontStyle.Strikeout);
                else
                    objects.CheckedItems[i].Font = new Font(objects.CheckedItems[i].Font, FontStyle.Regular);
            };

            CheckMarked();
        }

        private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            objects.CheckBoxes = true;
            for (int i = 0; i < objects.Items.Count; i++)
                objects.Items[i].Checked = true;
        }

        private void checkNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            objects.CheckBoxes = false;
            for (int i = 0; i < objects.Items.Count; i++)
                objects.Items[i].Checked = false;
        }

        private void inverseCheckedAndNotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            objects.CheckBoxes = true;
            for (int i = 0; i < objects.Items.Count; i++)
                objects.Items[i].Checked = !objects.Items[i].Checked;
            if (objects.CheckedIndices.Count == 0)
                objects.CheckBoxes = false;
        }

        private void cHBDM_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            int marked = 0;
            for (int i = 0; i < objects.Items.Count; i++)
                if (objects.Items[i].SubItems[6].Text == "Yes")
                    marked++;

            if (marked == 0) return;

            if (marked > 0)
            {
                if(MessageBox.Show(String.Format("{0} placemark(s) marked as Deleted!\r\nDo you want to delete them in source layer?", marked), "Delete placemarks", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)!=DialogResult.Yes)
                    return;
            };

            int[] pla = new int[3];
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            int pDel = 0;
            for (int i = objects.Items.Count - 1; i >= 0; i--)
                if (objects.Items[i].SubItems[6].Text == "Yes")
                {
                    string XPath = objects.Items[i].SubItems[7].Text;
                    string indx = XPath.Substring(XPath.IndexOf("["));
                    XPath = XPath.Remove(XPath.IndexOf("["));
                    int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                    XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                    xn = xn.SelectNodes(XPath)[ind].ParentNode.ParentNode;
                    if (xn.Name == "outerBoundaryIs") // polygon
                    {
                        xn = xn.ParentNode.ParentNode;
                        pla[2]++;
                    }
                    else
                    {
                        if (xn.SelectSingleNode("Point") != null) pla[0]++;
                        if (xn.SelectSingleNode("LineString") != null) pla[1]++;
                    };
                    xn = xn.ParentNode.RemoveChild(xn);
                    pDel++;
                };
            l.file.SaveKML();
            l.placemarks -= pDel;
            l.points -= pla[0];
            l.lines -= pla[1];
            l.areas -= pla[2];
            objects.Items.Clear();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString();
            parent.Refresh();
        }

        private void cHBDC_Click(object sender, EventArgs e)
        {
            if (!objects.CheckBoxes) return;
            if (objects.CheckedItems.Count == 0) return;

            if (MessageBox.Show(String.Format("{0} placemark(s) marked as Deleted!\r\nDo you want to delete them in source layer?", objects.CheckedItems.Count), "Delete placemarks", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int[] pla = new int[3];
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            for (int i = objects.CheckedItems.Count - 1; i >= 0; i--)
            {
                string XPath = objects.CheckedItems[i].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                xn = xn.SelectNodes(XPath)[ind].ParentNode.ParentNode;
                if (xn.Name == "outerBoundaryIs") // polygon
                {
                    xn = xn.ParentNode.ParentNode;
                    pla[2]++;
                }
                else
                {
                    if (xn.SelectSingleNode("Point") != null) pla[0]++;
                    if (xn.SelectSingleNode("LineString") != null) pla[1]++;
                };
                xn = xn.ParentNode.RemoveChild(xn);
            };
            l.file.SaveKML();
            l.placemarks -= pla[0] + pla[1] + pla[2];
            l.points -= pla[0];
            l.lines -= pla[1];
            l.areas -= pla[2];
            objects.Items.Clear();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString();
            parent.Refresh();
        }

        private void RenameObject(int i, string text, bool saveKML)
        {
            if (i < 0) return;
            if (i >= objects.Items.Count) return;

            objects.Items[i].SubItems[0].Text = text;

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            string XPath = objects.Items[i].SubItems[7].Text;
            string indx = XPath.Substring(XPath.IndexOf("["));
            XPath = XPath.Remove(XPath.IndexOf("["));
            int ind = int.Parse(indx.Substring(1, indx.Length - 2));
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            XmlNode xn = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("name");
            if (xn == null)
                xn = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.ParentNode.ParentNode.SelectSingleNode("name");

            objects.Items[i].SubItems[0].Text = text;
            xn.ChildNodes[0].Value = text;
            NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
            mo.Name = text;
            if(saveKML)
                l.file.SaveKML();
        }

        private FindReplaceDlg frd;
        private void ReplaceALL_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            if (frd.Find == "") return;
            if (frd.Replace == "") return;
            if ((objects.CheckedItems.Count == 0) && frd.CheckedOnly) return;

            string tts = frd.Find;
            if (frd.CaseIgnore) tts = tts.ToLower();

            int c = 0;
            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (frd.CheckedOnly && (!objects.Items[i].Checked)) continue;
                string text = objects.Items[i].Text;
                if (frd.CaseIgnore) text = text.ToLower();
                if (text.Contains(tts))
                {
                    int index = -1;
                    while ((index = text.IndexOf(tts)) >= 0)
                    {
                        text = text.Remove(index, tts.Length);
                        text = text.Insert(index, frd.Replace);
                    };
                    RenameObject(i, text, false);
                    c++;
                };
            };
            if (c > 0) ((KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex]).file.SaveKML();
            MessageBox.Show(c.ToString() + " placemarks replaced", "Find & Replace", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ReplaceButton_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            if (frd.Find == "") return;
            if (frd.Replace == "") return;
            if ((objects.CheckedItems.Count == 0) && frd.CheckedOnly) return;
            if (frd.currentIndex < 0) return;
            if (frd.currentIndex >= objects.Items.Count) return;
            
            string tts = frd.Find;
            if (frd.CaseIgnore) tts = tts.ToLower();

            int i = frd.currentIndex;
            if (frd.CheckedOnly && (!objects.Items[i].Checked)) return;

            string text = objects.Items[i].Text;
            if (frd.CaseIgnore) text = text.ToLower();
            if (text.Contains(tts))
            {
                int index = -1;
                while((index = text.IndexOf(tts)) >= 0)
                {
                    text = text.Remove(index, tts.Length);
                    text = text.Insert(index, frd.Replace);
                };
                RenameObject(i, text, true);
            };
        }

        private void ReplaceFind_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            if (frd.Find == "") return;
            if (frd.Replace == "") return;
            if ((objects.CheckedItems.Count == 0) && frd.CheckedOnly) return;
            if (frd.currentIndex < 0) return;
            if (frd.currentIndex >= objects.Items.Count) return;

            ReplaceButton_Click(sender, e);
            FindButton_Click(sender, e);
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            if (frd.Find == "") return;
            if((objects.CheckedItems.Count == 0) && frd.CheckedOnly) return;

            int index = frd.currentIndex;
            string tts = frd.Find;
            if (frd.CaseIgnore) tts = tts.ToLower();

            if (frd.Down)
            {
                frd.Enabled = false;
                index++;
                
                if (index < (objects.Items.Count - 1))
                    for (int i = index; i < objects.Items.Count; i++)
                    {
                        if (frd.CheckedOnly && (!objects.Items[i].Checked)) continue;
                        string text = objects.Items[i].Text;
                        if(((byte)frd.CustomData) == 2)
                            if (mapContent[i].UserData != null)
                                text = mapContent[i].UserData.ToString();
                        if (((byte)frd.CustomData) == 1)
                            if (mapContent[i].UserData != null)
                                text += mapContent[i].UserData.ToString();
                        if (frd.CaseIgnore) text = text.ToLower();
                        if (text.Contains(tts))
                        {
                            objects.Items[i].Selected = true;
                            objects.Items[i].EnsureVisible();
                            frd.currentIndex = i;
                            frd.Enabled = true;
                            return;
                        };
                    };
                if(index >= 0)
                    for (int i = 0; (i <= index) && (i < objects.Items.Count); i++)
                    {
                        if (frd.CheckedOnly && (!objects.Items[i].Checked)) continue;
                        string text = objects.Items[i].Text;
                        if (((byte)frd.CustomData) == 2)
                            if (mapContent[i].UserData != null)
                                text = mapContent[i].UserData.ToString();
                        if (((byte)frd.CustomData) == 1)
                            if (mapContent[i].UserData != null)
                                text += mapContent[i].UserData.ToString();
                        if (frd.CaseIgnore) text = text.ToLower();
                        if (text.Contains(tts))
                        {
                            objects.Items[i].Selected = true;
                            objects.Items[i].EnsureVisible();
                            frd.currentIndex = i;
                            frd.Enabled = true;
                            return;
                        };
                    };
                frd.Enabled = true;
            };
            if (frd.Up)
            {
                frd.Enabled = false;
                index--;
                if (index < 0) index = objects.Items.Count - 1;
                if (index >= 0)
                    for (int i = index; i >= 0; i--)
                    {
                        if (frd.CheckedOnly && (!objects.Items[i].Checked)) continue;
                        string text = objects.Items[i].Text;
                        if (((byte)frd.CustomData) == 2)
                            if (mapContent[i].UserData != null)
                                text = mapContent[i].UserData.ToString();
                        if (((byte)frd.CustomData) == 1)
                            if (mapContent[i].UserData != null)
                                text += mapContent[i].UserData.ToString();
                        if (frd.CaseIgnore) text = text.ToLower();
                        if (text.Contains(tts))
                        {
                            objects.Items[i].Selected = true;
                            objects.Items[i].EnsureVisible();
                            frd.currentIndex = i;
                            frd.Enabled = true;
                            return;
                        };
                    };
                if (index < (objects.Items.Count - 1))
                    for (int i = objects.Items.Count - 1; i >= index; i--)
                    {
                        if (frd.CheckedOnly && (!objects.Items[i].Checked)) continue;
                        string text = objects.Items[i].Text;
                        if (((byte)frd.CustomData) == 2)
                            if (mapContent[i].UserData != null)
                                text = mapContent[i].UserData.ToString();
                        if (((byte)frd.CustomData) == 1)
                            if (mapContent[i].UserData != null)
                                text += mapContent[i].UserData.ToString();
                        if (frd.CaseIgnore) text = text.ToLower();
                        if (text.Contains(tts))
                        {
                            objects.Items[i].Selected = true;
                            objects.Items[i].EnsureVisible();
                            frd.currentIndex = i;
                            frd.Enabled = true;
                            return;
                        };
                    };
                frd.Enabled = true;
            };
        }

        private void FF_Focus(object sender, EventArgs e)
        {
            if (objects.SelectedIndices.Count > 0)
                frd.currentIndex = objects.SelectedIndices[0];
        }

        private void FRB_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            if (frd != null)
            {
                frd.Dispose();
                frd = null;
            };
            if (frd == null)
            {
                frd = new FindReplaceDlg();
                frd.FindOnly = false;
                frd.CustomData = (byte)3; // replace
                frd.Text = "Find & Replace Placemark Name";
                frd.onFind += new EventHandler(FindButton_Click);                
                frd.onReplace += new EventHandler(ReplaceButton_Click);
                frd.onReplaceFind += new EventHandler(ReplaceFind_Click);
                frd.onReplaceAll += new EventHandler(ReplaceALL_Click);
                frd.onFocus += new EventHandler(FF_Focus);
            };
            frd.currentIndex = -1;
            if (objects.SelectedIndices.Count > 0)
                frd.currentIndex = objects.SelectedIndices[0];
            frd.Left = this.Left + 100;
            frd.Top = this.Top + 100;
            frd.Show(this);            
        }

        private void FRBD_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            if (frd != null)
            {
                frd.Dispose();
                frd = null;
            };
            if (frd == null)
            {
                frd = new FindReplaceDlg();
                frd.FindOnly = true;
                frd.CustomData = (byte)2; // find in desc
                frd.Text = "Find Text in Placemark Description";
                frd.onFind += new EventHandler(FindButton_Click);
                frd.onReplace += new EventHandler(ReplaceButton_Click);
                frd.onReplaceFind += new EventHandler(ReplaceFind_Click);
                frd.onReplaceAll += new EventHandler(ReplaceALL_Click);
                frd.onFindAll += new EventHandler(FindAll_Click);
                frd.onFocus += new EventHandler(FF_Focus);
            };
            frd.currentIndex = -1;
            if (objects.SelectedIndices.Count > 0)
                frd.currentIndex = objects.SelectedIndices[0];
            frd.Left = this.Left + 100;
            frd.Top = this.Top + 100;
            frd.Show(this);            
        }

        private void FRBN_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            if (frd != null)
            {
                frd.Dispose();
                frd = null;
            };
            if (frd == null)
            {
                frd = new FindReplaceDlg();
                frd.FindOnly = true;
                frd.CustomData = (byte)0; // find in name
                frd.Text = "Find Text in Placemark Name";
                frd.onFind += new EventHandler(FindButton_Click);
                frd.onReplace += new EventHandler(ReplaceButton_Click);
                frd.onReplaceFind += new EventHandler(ReplaceFind_Click);
                frd.onReplaceAll += new EventHandler(ReplaceALL_Click);
                frd.onFindAll += new EventHandler(FindAll_Click);
                frd.onFocus += new EventHandler(FF_Focus);
            };
            frd.currentIndex = -1;
            if (objects.SelectedIndices.Count > 0)
                frd.currentIndex = objects.SelectedIndices[0];
            frd.Left = this.Left + 100;
            frd.Top = this.Top + 100;
            frd.Show(this);            
        }

        private void FRBA_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            if (frd != null)
            {
                frd.Dispose();
                frd = null;
            };
            if (frd == null)
            {
                frd = new FindReplaceDlg();
                frd.FindOnly = true;
                frd.CustomData = (byte)1; // find in all
                frd.Text = "Find Text in Placemark Name & Description";
                frd.onFind += new EventHandler(FindButton_Click);
                frd.onReplace += new EventHandler(ReplaceButton_Click);
                frd.onReplaceFind += new EventHandler(ReplaceFind_Click);
                frd.onReplaceAll += new EventHandler(ReplaceALL_Click);
                frd.onFindAll += new EventHandler(FindAll_Click);
                frd.onFocus += new EventHandler(FF_Focus);
            };
            frd.currentIndex = -1;
            if (objects.SelectedIndices.Count > 0)
                frd.currentIndex = objects.SelectedIndices[0];
            frd.Left = this.Left + 100;
            frd.Top = this.Top + 100;
            frd.Show(this);            
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FRBN_Click(sender, e);
        }

        private void exportToSASPlanetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            SASPlacemarkConnector sc = new SASPlacemarkConnector();
            if (!sc.SASisOpen) return;
            NaviMapNet.MapPoint mp = (NaviMapNet.MapPoint)mapContent[objects.SelectedIndices[0]];
            if(sc.Visible)
                sc.SetPlacemark(mp.Name, mp.Points[0].Y, mp.Points[0].X);
            else
                sc.AddPlacemark(mp.Name, mp.Points[0].Y, mp.Points[0].X);
        }

        private void importFromSASPlanetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SASPlacemarkConnector sc = new SASPlacemarkConnector();
            if (!sc.SASisOpen) return;
            if (!sc.Visible) return;
            PointD ll = sc.LatLon;
            if (ll.IsEmpty) return;

            string nam = sc.Name;
            if (nam == null) return;

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "#none";
            XmlNode st = null;
            if (objects.SelectedIndices.Count > 0)
            {
                string XPath = objects.SelectedItems[0].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                if ((st != null) && (st.ChildNodes.Count > 0))
                    style = st.ChildNodes[0].Value;
            };

            XmlNode el = l.file.kmlDoc.CreateElement("Placemark");
            el.AppendChild(l.file.kmlDoc.CreateElement("name"));
            el.AppendChild(l.file.kmlDoc.CreateElement("styleUrl").AppendChild(l.file.kmlDoc.CreateTextNode(style)).ParentNode);
            el.AppendChild(l.file.kmlDoc.CreateElement("Point").AppendChild(l.file.kmlDoc.CreateElement("coordinates")).ParentNode);            
            el.AppendChild(l.file.kmlDoc.CreateElement("description"));            
            st = el.SelectSingleNode("styleUrl");
            XmlNode xy = el.SelectSingleNode("Point/coordinates");
            XmlNode xn = el.SelectSingleNode("name");
            XmlNode xd = el.SelectSingleNode("description");

            string x = ll.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = ll.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string desc = "";
            if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
            string dw = desc;

            if (InputXY(true, l.file.tmp_file_dir, ref nam, ref y, ref x, ref desc, ref style) == DialogResult.OK)
            {
                bool ch = false;
                if (!String.IsNullOrEmpty(nam)) ch = true;
                if (!String.IsNullOrEmpty(desc)) ch = true;
                x = x.Trim().Replace(",", ".");
                y = y.Trim().Replace(",", ".");
                if (!String.IsNullOrEmpty(x)) ch = true;
                if (!String.IsNullOrEmpty(y)) ch = true;
                if (ch)
                {
                    xn.AppendChild(l.file.kmlDoc.CreateTextNode(nam));
                    xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                    xy.AppendChild(l.file.kmlDoc.CreateTextNode(String.Format("{0},{1},0", x, y)));
                    st.ChildNodes[0].Value = style;
                    xf.AppendChild(el);

                    l.file.SaveKML();
                    l.placemarks++;
                    l.points++;
                    parent.Refresh();
                    laySelect.Items[laySelect.SelectedIndex] = l.ToString();
                    if (objects.Items.Count > 0)
                        objects.Items[objects.Items.Count - 1].Selected = true;
                };
            };
        }

        private void addNewPointToSASPlanetToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            SASPlacemarkConnector sc = new SASPlacemarkConnector();
            PointF click = MapViewer.MouseDownDegrees;
            if (!sc.SASisOpen) return;
            if (sc.Visible)
                sc.SetPlacemark("Map Click", click.Y, click.X);
            else
                sc.AddPlacemark("Map Click", click.Y, click.X);
        }

        private void cHBDS_Click(object sender, EventArgs e)
        {
            if (objects.CheckedItems.Count == 0) return;
            SASPlacemarkConnector sc = new SASPlacemarkConnector();
            if (!sc.SASisOpen) return;

            if (MessageBox.Show("Copy " + objects.CheckedItems.Count.ToString() + " placemark(s) to SASPlanet?", "Copy placemarks", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            int p = 0;
            for (int i = 0; i < objects.CheckedIndices.Count; i++)                
            {
                NaviMapNet.MapPoint mp = (NaviMapNet.MapPoint)mapContent[objects.CheckedIndices[i]];
                if(!(mp is NaviMapNet.MapPoint)) continue;
                p++;
                if (sc.Visible)
                {
                    sc.SetPlacemark(mp.Name, mp.Points[0].Y, mp.Points[0].X);
                    System.Threading.Thread.Sleep(100);
                    sc.ClickOk();
                }
                else
                {
                    sc.AddPlacemark(mp.Name, mp.Points[0].Y, mp.Points[0].X);
                    System.Threading.Thread.Sleep(100);
                    sc.ClickOk();
                };
                System.Threading.Thread.Sleep(200);
            };
            MessageBox.Show("Copied " + p.ToString() + " points to SASPlanet", "Copy placemarks", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void selectAllInAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!MapViewer.SelectionBoxIsVisible) return;
            if (objects.Items.Count == 0) return;

            NaviMapNet.MapObject[] objs = mapContent.Select(MapViewer.SelectionBoundsRectDegrees);
            if((objs != null) && (objs.Length > 0))
            {
                objects.CheckBoxes = true;
                for (int i = 0; i < objs.Length; i++)
                    objects.Items[objs[i].Index].Checked = true;
            };
        }

        private void findAllPlacemarksOutsideAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!MapViewer.SelectionBoxIsVisible) return;
            if (objects.Items.Count == 0) return;            

            NaviMapNet.MapObject[] objs = mapContent.Select(MapViewer.SelectionBoundsRectDegrees);
            List<int> ids = new List<int>();
            if ((objs != null) && (objs.Length > 0))
            {
                objects.CheckBoxes = true;
                for (int i = 0; i < objs.Length; i++)
                    ids.Add(objs[i].Index);                
            };
            for (int i = 0; i < objects.Items.Count; i++)
                if (ids.IndexOf(i) < 0)
                    objects.Items[i].Checked = true;
        }

        private void clearSelectionBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapViewer.ClearSelectionBox();
        }

        private int prev_c = 0;
        private void UpdateCheckedAndMarked(bool update)
        {
            bool up = update;

            if (prev_c != objects.CheckedIndices.Count)
            {
                prev_c = objects.CheckedIndices.Count;
                up = true;
            };

            if (!up) return;
            NCB.Enabled = prev_c > 0;
            NDB.Enabled = prev_m > 0;

            if ((prev_m == 0) && (prev_c == 0))
                status.Text = "";
            else
                status.Text = String.Format("Marked to delete {0}/{2} objects\r\nChecked {1}/{2} objects\r\n", prev_m, prev_c, objects.Items.Count);

            status.SelectionStart = status.TextLength;
            status.ScrollToCaret();
        }

        private void objects_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            UpdateCheckedAndMarked(false);
        }

        private void NCB_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            
            int si = -1;
            if (objects.SelectedIndices.Count > 0)
                si = objects.SelectedIndices[0];

            si++;

            if(si < objects.Items.Count)
                for(int i = si; i<objects.Items.Count;i++)
                    if (objects.Items[i].Checked)
                    {
                        objects.Items[i].Selected = true;
                        objects.Items[i].EnsureVisible();
                        return;
                    };

            for (int i = 0; i <= si; i++)
            {
                if (objects.Items[i].Checked)
                {
                    objects.Items[i].Selected = true;
                    objects.Items[i].EnsureVisible();
                    return;
                };
            };
        }

        private void NDB_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            int si = -1;
            if (objects.SelectedIndices.Count > 0)
                si = objects.SelectedIndices[0];

            si++;

            if (si < objects.Items.Count)
                for (int i = si; i < objects.Items.Count; i++)
                    if (objects.Items[i].SubItems[6].Text == "Yes")
                    {
                        objects.Items[i].Selected = true;
                        objects.Items[i].EnsureVisible();
                        return;
                    };

            for (int i = 0; i <= si; i++)
            {
                if (objects.Items[i].SubItems[6].Text == "Yes")
                {
                    objects.Items[i].Selected = true;
                    objects.Items[i].EnsureVisible();
                    return;
                };
            };
        }

        private void FindAll_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            if (frd.Find == "") return;

            int index = frd.currentIndex;
            string tts = frd.Find;
            if (frd.CaseIgnore) tts = tts.ToLower();

            frd.Enabled = false;
            int first_index = -1;
            for (int i = 0; i < objects.Items.Count; i++)
            {                
                string text = objects.Items[i].Text;
                if (((byte)frd.CustomData) == 2)
                    if (mapContent[i].UserData != null)
                        text = mapContent[i].UserData.ToString();
                if (((byte)frd.CustomData) == 1)
                    if (mapContent[i].UserData != null)
                        text += mapContent[i].UserData.ToString();
                if (frd.CaseIgnore) text = text.ToLower();
                if (text.Contains(tts))
                {
                    if (first_index < 0) first_index = i;
                    objects.Items[i].Checked = true;
                    objects.CheckBoxes = true;
                };
            };
            if (first_index >= 0)
            {
                objects.Items[first_index].Selected = true;
                objects.Items[first_index].EnsureVisible();
            };
            frd.Enabled = true;                     
        }

        private void cHBNIL_Click(object sender, EventArgs e)
        {
            if (objects.CheckedIndices.Count == 0) return;

            /////////////

            if (parent.mapIcons == null)
                parent.mapIcons = new MapIcons();
            DialogResult dr = parent.mapIcons.ShowDialog();
            if (dr == DialogResult.Ignore)
                changeIconToolStripMenuItem_Click(sender, e);
            if (dr != DialogResult.OK) return;            

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "new_" + DateTime.UtcNow.ToString("HHmmss");
            string href = "images/" + style + ".png";
            style2image.Add("#" + style, href);

            ImageMagick.MagickImage im = new ImageMagick.MagickImage((Bitmap)parent.mapIcons.SelectedImage);
            if ((im.Width > 32) || (im.Height > 32))
            {
                im.Resize(32, 32);
                MapIcons.SaveIcon(im, l.file.tmp_file_dir + href.Replace("/", @"\"));
            }
            else
                MapIcons.SaveIcon(parent.mapIcons.SelectedImageArr, l.file.tmp_file_dir + href.Replace("/", @"\"));
            images.Images.Add("images/" + style + ".png", im.ToBitmap());
            im.Dispose();
            XmlNode ssd = l.file.kmlDoc.SelectSingleNode("kml/Document"); //Style[@id='" + firstsid + "']/IconStyle/Icon/href");
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Style"));
            XmlAttribute attr = l.file.kmlDoc.CreateAttribute("id");
            attr.Value = style;
            ssd.Attributes.Append(attr);
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("IconStyle"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Icon"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("href"));
            ssd.AppendChild(l.file.kmlDoc.CreateTextNode(href));
            style = "#" + style;

            /////////////
            wbf.Show("Change Style", "Wait, applying changes ... ");
            for (int i = 0; i < objects.CheckedIndices.Count; i++)
            {
                NaviMapNet.MapObject mo = mapContent[objects.CheckedIndices[i]];
                if (!(mo is NaviMapNet.MapPoint)) continue;

                string XPath = objects.CheckedItems[i].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                XmlNode st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                if ((st != null) && (st.ChildNodes.Count > 0))
                    st.ChildNodes[0].Value = style;
                else
                {
                    if (st == null)
                        st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.AppendChild(l.file.kmlDoc.CreateElement("styleUrl"));
                    st.AppendChild(l.file.kmlDoc.CreateTextNode(style));
                };                
                mo.Img = images.Images[style2image[style]];
                objects.CheckedItems[i].ImageIndex = images.Images.IndexOfKey(style2image[style]);                
            };
            wbf.Hide();
            l.file.SaveKML();
            MapViewer.DrawOnMapData();
        }

        private void cHBNIF_Click(object sender, EventArgs e)
        {
            if (objects.CheckedIndices.Count == 0) return;

            /////////////

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Image";
            ofd.DefaultExt = ".png";
            ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif";
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                ofd.Dispose();
                return;
            };            

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            string style = "new_" + DateTime.UtcNow.ToString("HHmmss");
            string href = "images/" + style + ".png";
            style2image.Add("#" + style, href);

            ImageMagick.MagickImage im = new ImageMagick.MagickImage(ofd.FileName);
            ofd.Dispose();
            if ((im.Width > 32) || (im.Height > 32))
            {
                im.Resize(32, 32);
                MapIcons.SaveIcon(im, l.file.tmp_file_dir + href.Replace("/", @"\"));
            }
            else
                MapIcons.SaveIcon(ofd.FileName, l.file.tmp_file_dir + href.Replace("/", @"\"));
            images.Images.Add("images/" + style + ".png", im.ToBitmap());
            im.Dispose();
            XmlNode ssd = l.file.kmlDoc.SelectSingleNode("kml/Document"); //Style[@id='" + firstsid + "']/IconStyle/Icon/href");
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Style"));
            XmlAttribute attr = l.file.kmlDoc.CreateAttribute("id");
            attr.Value = style;
            ssd.Attributes.Append(attr);
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("IconStyle"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("Icon"));
            ssd = ssd.AppendChild(l.file.kmlDoc.CreateElement("href"));
            ssd.AppendChild(l.file.kmlDoc.CreateTextNode(href));
            style = "#" + style;

            /////////////
            /////////////
            wbf.Show("Change Style", "Wait, applying changes ... ");
            for (int i = 0; i < objects.CheckedIndices.Count; i++)
            {
                NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
                if (!(mo is NaviMapNet.MapPoint)) continue;

                string XPath = objects.CheckedItems[i].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                XmlNode st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                if ((st != null) && (st.ChildNodes.Count > 0))
                    st.ChildNodes[0].Value = style;
                else
                {
                    if (st == null)
                        st = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.AppendChild(l.file.kmlDoc.CreateElement("styleUrl"));
                    st.AppendChild(l.file.kmlDoc.CreateTextNode(style));
                };                
                mo.Img = images.Images[style2image[style]];
                objects.CheckedItems[i].ImageIndex = images.Images.IndexOfKey(style2image[style]);                
            };
            wbf.Hide();
            l.file.SaveKML();
            MapViewer.DrawOnMapData();
        }

        private void cHBNIP_Click(object sender, EventArgs e)
        {
            if (objects.CheckedIndices.Count == 0) return;

            LineAreaStyleForm lasf = new LineAreaStyleForm();
            lasf.LColor.Text = LineAreaStyleForm.HexConverter(lasf.LBColor.BackColor = Color.Blue);
            lasf.AColor.Text = LineAreaStyleForm.HexConverter(lasf.ABColor.BackColor = Color.Red);
            lasf.AFill.SelectedIndex = 0;
            lasf.ApplyTo.SelectedIndex = 1;
            lasf.ApplyTo.Enabled = false;
            if (lasf.ShowDialog() != DialogResult.OK)
            {
                lasf.Dispose();
                return;
            };

            /////////////
            wbf.Show("Change Style", "Wait, applying changes ... ");

            Color lineColor = Color.FromArgb((int)((float)lasf.LOpacity.Value / 100f * 255f), lasf.LBColor.BackColor);
            int lineWidth = (int)lasf.LWidth.Value;
            Color fillColor = Color.FromArgb((int)((float)lasf.AOpacity.Value / 100f * 255f), lasf.ABColor.BackColor);
            int fill = lasf.AFill.SelectedIndex;
            lasf.Dispose();

            Image iml = new Bitmap(16, 16);
            Image ima = new Bitmap(16, 16);
            {
                Graphics g = Graphics.FromImage(iml);
                g.FillRectangle(new SolidBrush(lineColor), 0, 0, 16, 16);
                g.DrawString("L", new Font("Terminal", 11, FontStyle.Bold), new SolidBrush(Color.FromArgb(255 - lineColor.R, 255 - lineColor.G, 255 - lineColor.B)), 1, -1);
                g.Dispose();
            };
            {
                Graphics g = Graphics.FromImage(ima);
                g.FillRectangle(new SolidBrush(fillColor), 0, 0, 16, 16);
                g.DrawRectangle(new Pen(new SolidBrush(lineColor), 2), 0, 0, 16, 16);
                g.DrawString("A", new Font("Terminal", 11, FontStyle.Bold), new SolidBrush(Color.FromArgb(255 - fillColor.R, 255 - fillColor.G, 255 - fillColor.B)), 1, -1);
                g.Dispose();
            };
            images.Images.Add(iml);
            images.Images.Add(ima);

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            string styleUrl = "newStyle" + DateTime.UtcNow.ToString("HHmmss");
            string style = "<Style id=\"" + styleUrl + "\"><LineStyle><color>" + LineAreaStyleForm.HexStyleConverter(lineColor) + "</color><width>" + lineWidth.ToString() + "</width></LineStyle><PolyStyle><color>" + LineAreaStyleForm.HexStyleConverter(fillColor) + "</color><fill>" + fill.ToString() + "</fill></PolyStyle></Style>\r\n";
            l.file.kmlDoc.SelectSingleNode("kml/Document").InnerXml += style;

            for (int i = 0; i < objects.CheckedIndices.Count; i++)
            {
                if (objects.CheckedItems[i].SubItems[1].Text == "Point") continue;

                bool ispolygon = false;

                string XPath = objects.CheckedItems[i].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

                XmlNode x_placemark = x_folder.SelectNodes(XPath)[ind].ParentNode.ParentNode;
                if (x_placemark.Name == "outerBoundaryIs")
                {
                    x_placemark = x_placemark.ParentNode.ParentNode;
                    ispolygon = true;
                };

                foreach (XmlNode n in x_placemark.SelectNodes("styleUrl"))
                    n.ParentNode.RemoveChild(n);
                foreach (XmlNode n in x_placemark.SelectNodes("Style"))
                    n.ParentNode.RemoveChild(n);

                XmlNode x_style = l.file.kmlDoc.CreateElement("styleUrl");
                x_style.AppendChild(l.file.kmlDoc.CreateTextNode("#" + styleUrl));
                x_placemark.AppendChild(x_style);

                NaviMapNet.MapObject mo = mapContent[objects.CheckedIndices[i]];
                if (ispolygon)
                {
                    objects.CheckedItems[i].ImageIndex = images.Images.Count - 1;
                    (mo as NaviMapNet.MapPolygon).BorderColor = lineColor;
                    (mo as NaviMapNet.MapPolygon).Width = lineWidth;
                    (mo as NaviMapNet.MapPolygon).BodyColor = fillColor;
                }
                else
                {
                    objects.CheckedItems[i].ImageIndex = images.Images.Count - 2;
                    (mo as NaviMapNet.MapPolyLine).Color = lineColor;
                    (mo as NaviMapNet.MapPolyLine).Width = lineWidth;
                };
            };
            wbf.Hide();
            l.file.SaveKML();
            //laySelect_SelectedIndexChanged(sender, e);
            MapViewer.DrawOnMapData();
        }

        private void cHB0A_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (objects.Items[i].Checked) continue;
                objects.Items[i].SubItems[6].Text = "Yes";
                objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Strikeout);
            };

            CheckMarked();
        }

        private void cHB2A_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (objects.Items[i].Checked) continue;
                objects.Items[i].SubItems[6].Text = "";
                objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Regular);
            };

            CheckMarked();
        }

        private void cHB3A_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (objects.Items[i].Checked) continue;
                objects.Items[i].SubItems[6].Text = (objects.Items[i].SubItems[6].Text == "Yes") ? "" : "Yes";
                if (objects.Items[i].SubItems[6].Text == "Yes")
                    objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Strikeout);
                else
                    objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Regular);
            };

            CheckMarked();
        }

        private void NBC0_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (objects.Items[i].SubItems[6].Text == "Yes")
                {
                    objects.CheckBoxes = true;
                    objects.Items[i].Checked = true;
                };
            };
        }

        private void NBC1_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (objects.Items[i].SubItems[6].Text != "Yes")
                {
                    objects.CheckBoxes = true;
                    objects.Items[i].Checked = true;
                };
            };
        }

        private void NBC2_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (objects.Items[i].SubItems[6].Text == "Yes")
                    objects.Items[i].Checked = false;
            };
            if (objects.CheckedIndices.Count == 0)
                objects.CheckBoxes = false;
        }

        private void NBC3_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (objects.Items[i].SubItems[6].Text != "Yes")
                    objects.Items[i].Checked = false;
            };
            if (objects.CheckedIndices.Count == 0)
                objects.CheckBoxes = false;
        }

        private void NBC4_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (objects.Items[i].SubItems[6].Text == "Yes")
                {
                    if (objects.Items[i].Checked)
                        objects.Items[i].Checked = false;
                    else
                    {
                        objects.CheckBoxes = true;
                        objects.Items[i].Checked = true;
                    };
                };
            };
            if (objects.CheckedIndices.Count == 0)
                objects.CheckBoxes = false;
        }

        private void NBC5_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            for (int i = 0; i < objects.Items.Count; i++)
            {
                if (objects.Items[i].SubItems[6].Text != "Yes")
                {
                    if (objects.Items[i].Checked)
                        objects.Items[i].Checked = false;
                    else
                    {
                        objects.CheckBoxes = true;
                        objects.Items[i].Checked = true;
                    };
                };
            };
            if (objects.CheckedIndices.Count == 0)
                objects.CheckBoxes = false;
        }

        private void checkCopiesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            Hashtable htXY = new Hashtable();
            Hashtable htNM = new Hashtable();

            for (int i = 0; i < objects.Items.Count; i++)
            {
                string simByXY = objects.Items[i].SubItems[4].Text;
                string simByNM = objects.Items[i].SubItems[5].Text;

                if (simByXY != "")
                {
                    object oXY = htXY[simByXY];
                    if (oXY == null) htXY.Add(simByXY, oXY = new List<int>());
                    List<int> list = (List<int>)oXY;
                    list.Add(i);
                };

                if (simByNM != "")
                {
                    object oXY = htNM[simByNM];
                    if (oXY == null) htNM.Add(simByNM, oXY = new List<int>());
                    List<int> list = (List<int>)oXY;
                    list.Add(i);
                };
            };

            int sxy = 0;
            foreach (string key in htXY.Keys)
            {
                List<int> list = (List<int>)htXY[key];
                if (list.Count < 2) continue;
                for (int i = 1; i < list.Count; i++)
                {
                    objects.CheckBoxes = true;
                    objects.Items[list[i]].Checked = true;
                    sxy++;
                };
            };

            int snm = 0;
            foreach (string key in htNM.Keys)
            {
                List<int> list = (List<int>)htNM[key];
                if (list.Count < 2) continue;
                for (int i = 1; i < list.Count; i++)
                {
                    objects.CheckBoxes = true;
                    objects.Items[list[i]].Checked = true;
                    snm++;
                };
            };

            if (sxy > 0) status.Text += "Checked " + sxy.ToString() + " copies by Coordinates\r\n";
            if (snm > 0) status.Text += "Checked " + snm.ToString() + " copies by Name\r\n";
            status.SelectionStart = status.TextLength;
            status.ScrollToCaret();
        }

        private void uncheckCopiesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;

            Hashtable htXY = new Hashtable();
            Hashtable htNM = new Hashtable();

            for (int i = 0; i < objects.Items.Count; i++)
            {
                string simByXY = objects.Items[i].SubItems[4].Text;
                string simByNM = objects.Items[i].SubItems[5].Text;

                if (simByXY != "")
                {
                    object oXY = htXY[simByXY];
                    if (oXY == null) htXY.Add(simByXY, oXY = new List<int>());
                    List<int> list = (List<int>)oXY;
                    list.Add(i);
                };

                if (simByNM != "")
                {
                    object oXY = htNM[simByNM];
                    if (oXY == null) htNM.Add(simByNM, oXY = new List<int>());
                    List<int> list = (List<int>)oXY;
                    list.Add(i);
                };
            };

            int sxy = 0;
            foreach (string key in htXY.Keys)
            {
                List<int> list = (List<int>)htXY[key];
                if (list.Count < 2) continue;
                for (int i = 1; i < list.Count; i++)
                {
                    objects.Items[list[i]].Checked = false;
                    sxy++;
                };
            };

            int snm = 0;
            foreach (string key in htNM.Keys)
            {
                List<int> list = (List<int>)htNM[key];
                if (list.Count < 2) continue;
                for (int i = 1; i < list.Count; i++)
                {
                    objects.Items[list[i]].Checked = false;
                    snm++;
                };
            };

            if (sxy > 0) status.Text += "Uncheck " + sxy.ToString() + " copies by Coordinates\r\n";
            if (snm > 0) status.Text += "Uncheck " + snm.ToString() + " copies by Name\r\n";
            status.SelectionStart = status.TextLength;
            status.ScrollToCaret();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            // 4 - XY
            // 5 - Name

            if (objects.Items.Count == 0) return;

            int si = -1;
            string search_text = "";
            if (objects.SelectedIndices.Count > 0)
            {
                if (objects.SelectedIndices.Count > 0)
                {
                    if (!String.IsNullOrEmpty(objects.SelectedItems[0].SubItems[4].Text))
                    {
                        search_text = objects.SelectedItems[0].SubItems[4].Text;
                        si = objects.SelectedIndices[0] + 1;
                    };                    
                };
            };
            if(si < 0)
            {
                for (int i = 0; i < objects.Items.Count; i++)
                    if (!String.IsNullOrEmpty(objects.Items[i].SubItems[4].Text))
                    {
                        si = i;
                        search_text = objects.Items[i].SubItems[4].Text;
                        break;
                    };
                if (si < 0) return;
            };                

            if (si < objects.Items.Count)
                for (int i = si; i < objects.Items.Count; i++)
                    if (objects.Items[i].SubItems[4].Text == search_text)
                    {
                        objects.Items[i].Selected = true;
                        objects.Items[i].EnsureVisible();
                        return;
                    };

            for (int i = 0; i <= si; i++)
            {
                if (objects.Items[i].SubItems[4].Text == search_text)
                {
                    objects.Items[i].Selected = true;
                    objects.Items[i].EnsureVisible();
                    return;
                }
            };
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            // 4 - XY
            // 5 - Name

            if (objects.Items.Count == 0) return;

            int si = -1;
            string search_text = "";
            if (objects.SelectedIndices.Count > 0)
            {
                if (!String.IsNullOrEmpty(objects.SelectedItems[0].SubItems[5].Text))
                {
                    search_text = objects.SelectedItems[0].SubItems[5].Text;
                    si = objects.SelectedIndices[0] + 1;
                };
            };
            if (si < 0)
            {
                for (int i = 0; i < objects.Items.Count; i++)
                    if (!String.IsNullOrEmpty(objects.Items[i].SubItems[5].Text))
                    {
                        si = i;
                        search_text = objects.Items[i].SubItems[5].Text;
                        break;
                    };
                if (si < 0) return;
            };

            if (si < objects.Items.Count)
                for (int i = si; i < objects.Items.Count; i++)
                    if (objects.Items[i].SubItems[5].Text == search_text)
                    {
                        objects.Items[i].Selected = true;
                        objects.Items[i].EnsureVisible();
                        return;
                    };

            for (int i = 0; i <= si; i++)
            {
                if (objects.Items[i].SubItems[5].Text == search_text)
                {
                    objects.Items[i].Selected = true;
                    objects.Items[i].EnsureVisible();
                    return;
                }
            };
        }

        private void byNameAndCoordinatesforSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            int dist = 2;
            if (System.Windows.Forms.InputBox.Show("Distance", "Max distance in meters:", ref dist, 0, 9999) == DialogResult.OK)
                FindCopies(objects.SelectedIndices[0], true, true, dist);
        }

        private void byNameAndCoordinatesforAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int dist = 2;
            if (System.Windows.Forms.InputBox.Show("Distance", "Max distance in meters:", ref dist, 0, 9999) == DialogResult.OK)
                FindCopies(-1, true, true, dist);
        }

        private void importDataFromOSMInsideSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[] minmax = MapViewer.SelectionBoundsMinMaxDegrees;
            this.Close();
            parent.ImportZoneFromOSM(minmax);
        }

        private void removeDescriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.CheckedIndices.Count == 0) return;

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            wbf.Show("Remove description", "Wait, applying changes ... ");
            for (int i = 0; i < objects.CheckedIndices.Count; i++)
            {
                string XPath = objects.CheckedItems[i].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));                
                XmlNode xd = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("description");
                if (xd == null)
                    xd = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.ParentNode.ParentNode.SelectSingleNode("description");
                string desc = "";
                if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
                desc = "";
                if (xd != null)
                    xd.RemoveAll();
                else
                {
                    xd = l.file.kmlDoc.CreateElement("description");
                    xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.AppendChild(xd);
                };
                xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                NaviMapNet.MapObject mo = mapContent[objects.CheckedIndices[i]];
                mo.UserData = desc;
            };
            textBox1.Text = "";
            wbf.Hide();
            l.file.SaveKML();
        }

        private void removeOSMSpecifiedTagsFromDescriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.CheckedIndices.Count == 0) return;

            string egex = @"(tag_[\w\-_]+\=[^\r\n]*)|(name:[^(ru|en)]+\=[^\r\n]*)";
            if(InputBox.QueryRegexBox("Remove OSM tags from Description", "Pattern to delete:", "Test text here:", ref egex)!=DialogResult.OK)
                return;

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];

            wbf.Show("Remove description", "Wait, applying changes ... ");
            for (int i = 0; i < objects.CheckedIndices.Count; i++)
            {
                string XPath = objects.CheckedItems[i].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                XmlNode xd = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("description");
                if (xd == null)
                    xd = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.ParentNode.ParentNode.SelectSingleNode("description");
                string desc = "";
                if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
                if (!String.IsNullOrEmpty(desc))
                {
                    Regex eg = new Regex(egex, RegexOptions.IgnoreCase);
                    MatchCollection mc = eg.Matches(desc);
                    if (mc.Count > 0)
                        for (int ii = mc.Count - 1; ii >= 0; ii--)
                            desc = desc.Remove(mc[ii].Index, mc[ii].Length);
                    while (desc.IndexOf("\r\n\r\n") >= 0) desc = desc.Replace("\r\n\r\n", "\r\n");
                    while (desc.IndexOf("\r\r") >= 0) desc = desc.Replace("\r\r", "\r");
                    while (desc.IndexOf("\n\n") >= 0) desc = desc.Replace("\n\n", "\n");
                    if (desc.IndexOf("\r\n") == 0) desc = desc.Remove(0, 2);
                    if (desc.IndexOf("\r") == 0) desc = desc.Remove(0, 1);
                    if (desc.IndexOf("\n") == 0) desc = desc.Remove(0, 1);
                    desc = desc.Trim();
                };
                if (xd != null)
                    xd.RemoveAll();
                else
                {
                    xd = l.file.kmlDoc.CreateElement("description");
                    xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.AppendChild(xd);
                };
                xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                NaviMapNet.MapObject mo = mapContent[objects.CheckedIndices[i]];
                mo.UserData = desc;
            };
            textBox1.Text = "";
            wbf.Hide();
            l.file.SaveKML();
        }

        private void checkPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            objects.CheckBoxes = true;
            for (int i = 0; i < objects.Items.Count; i++)
                if(objects.Items[i].SubItems[1].Text.StartsWith("Point"))
                    objects.Items[i].Checked = true;
        }

        private void checkLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            objects.CheckBoxes = true;
            for (int i = 0; i < objects.Items.Count; i++)
                if (objects.Items[i].SubItems[1].Text.StartsWith("Line"))
                    objects.Items[i].Checked = true;
        }

        private void checkPolygonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) return;
            objects.CheckBoxes = true;
            for (int i = 0; i < objects.Items.Count; i++)
                if (objects.Items[i].SubItems[1].Text.StartsWith("Polygon"))
                    objects.Items[i].Checked = true;
        }

        private void exPOlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (objects.SelectedItems[0].SubItems[1].Text == "Point") return;
            if (objects.SelectedItems[0].SubItems[1].Text.StartsWith("Line"))
            {
                NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
                if (!(mo is NaviMapNet.MapPolyLine)) return;
                KMZRebuilederForm.waitBox.Show("Wait", "Loading map...");
                PolyCreator pc = new PolyCreator(this.parent, this.wbf, mo.Points, false);
                KMZRebuilder.KMZRebuilederForm.waitBox.Hide();
                this.Hide();
                pc.ShowDialog();
                this.Show();
                pc.Dispose();
                return;
            };
            if (objects.SelectedItems[0].SubItems[1].Text.StartsWith("Polygon"))
            {
                NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
                if (!(mo is NaviMapNet.MapPolygon)) return;                
                KMZRebuilederForm.waitBox.Show("Wait", "Loading map...");
                PolyCreator pc = new PolyCreator(this.parent, this.wbf, mo.Points, true);
                KMZRebuilder.KMZRebuilederForm.waitBox.Hide();
                this.Hide();
                pc.ShowDialog();
                this.Show();
                pc.Dispose();
                return;
            };
        }

        private void savePolyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (objects.SelectedItems[0].SubItems[1].Text == ("Point")) return;
            if (objects.SelectedItems[0].SubItems[1].Text.StartsWith("Line"))
            {
                NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
                if (!(mo is NaviMapNet.MapPolyLine)) return;
                int in_dist = 2500;
                if(InputBox.Show("Convert Line to Polygon", "Enter buffer size in meters:", ref in_dist, 100, 10000) != DialogResult.OK) return;
                PolyLineBuffer.PolyLineBufferCreator.PolyResult pr = PolyLineBuffer.PolyLineBufferCreator.GetLineBufferPolygon(mo.Points, 2500, true, true, PolyLineBuffer.PolyLineBufferCreator.GeographicDistFunc, 0);
                PolyCreator.SavePoly2File(pr.polygon);
            };
            if (objects.SelectedItems[0].SubItems[1].Text.StartsWith("Polygon"))
            {
                NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
                if (!(mo is NaviMapNet.MapPolygon)) return;
                PolyCreator.SavePoly2File(mo.Points);
            };
        }

        private void interpolateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (objects.SelectedItems[0].SubItems[1].Text == ("Point")) return;
            NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
            if (mo.Points.Length < 3) return;
            this.wbf.Show("Wait", "Loading map...");
            InterLessForm pc = new InterLessForm(this.parent, this.wbf);
            pc.loadroute(mo.Points);
            pc.loadbutton.Enabled = false;
            this.wbf.Hide();
            this.Hide();
            PointF[] res;
            pc.ShowDialogCallBack(out res);
            pc.Dispose();
            this.Show();
            if ((res != null) && (res.Length > 1))
            {
                mo.Points = res;
                KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
                string XPath = objects.SelectedItems[0].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                XmlNode x_placemark = x_folder.SelectNodes(XPath)[ind].ParentNode.ParentNode;
                XmlNode x_coord = null;
                if ((mo is NaviMapNet.MapPolyLine))
                    x_coord = x_placemark.SelectNodes("LineString/coordinates")[0];
                if ((mo is NaviMapNet.MapPolygon))
                {
                    x_coord = x_placemark.SelectNodes("outerBoundaryIs/LinearRing/coordinates")[0];
                    if(x_coord == null)
                        x_coord = x_placemark.SelectNodes("LinearRing/coordinates")[0];
                };
                if (x_coord != null)
                {
                    string txc = "";
                    foreach (PointF p in res)
                    {
                        if (txc.Length > 0) txc += " ";
                        txc += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0", p.X, p.Y);
                    };
                    x_coord.ChildNodes[0].Value = txc;
                    l.file.SaveKML();
                    if (mapSelect.ObjectsCount > 0)
                    {
                        mapSelect.Clear();
                        SelectOnMap(mo.Index);
                    };
                    if ((mo is NaviMapNet.MapPolyLine))
                        objects.SelectedItems[0].SubItems[1].Text = "Line (" + res.Length.ToString() + " points)";
                    if ((mo is NaviMapNet.MapPolygon))
                        objects.SelectedItems[0].SubItems[1].Text = "Polygon (" + res.Length.ToString() + " points)";
                    MapViewer.DrawOnMapData();                    
                };
            };
        }

        private void openInTrackSplitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (!objects.SelectedItems[0].SubItems[1].Text.StartsWith("Line")) return;
            NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
            if (mo.Points.Length < 3) return;
            this.wbf.Show("Wait", "Loading map...");
            TrackSplitter pc = new TrackSplitter(this.parent, this.wbf);
            pc.loadroute(mo.Points);
            this.wbf.Hide();
            this.Hide();
            pc.ShowDialog();
            pc.Dispose();
            this.Show();
        }

        private void inverseLineDirectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (!objects.SelectedItems[0].SubItems[1].Text.StartsWith("Line")) return;
            
            NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
            if (mo.Points.Length < 3) return;

            if (MessageBox.Show("Do you want to Invertse `" + mo.Name + "`", "Inverse Line Start/Stop", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            List<PointF> points = new List<PointF>();
            points.AddRange(mo.Points);
            points.Reverse();
            mo.Points = points.ToArray();

            //SAVE
            {
                KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
                string XPath = objects.SelectedItems[0].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                XmlNode x_placemark = x_folder.SelectNodes(XPath)[ind].ParentNode.ParentNode;
                XmlNode x_coord = null;
                if ((mo is NaviMapNet.MapPolyLine))
                {
                    x_coord = x_placemark.SelectNodes("LineString/coordinates")[0];
                    string txc = "";
                    foreach (PointF p in points)
                    {
                        if (txc.Length > 0) txc += " ";
                        txc += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0", p.X, p.Y);
                    };
                    x_coord.ChildNodes[0].Value = txc;
                    l.file.SaveKML();
                };
            };
        }

        private void getCRCOfImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (objects.SelectedItems[0].SubItems[1].Text != "Point") return;

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            string XPath = objects.SelectedItems[0].SubItems[7].Text;
            string indx = XPath.Substring(XPath.IndexOf("["));
            XPath = XPath.Remove(XPath.IndexOf("["));
            int ind = int.Parse(indx.Substring(1, indx.Length - 2));
            XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            XmlNode x_placemark = x_folder.SelectNodes(XPath)[ind].ParentNode.ParentNode;

            string styleUrl = "";
            if (x_placemark.SelectSingleNode("styleUrl") != null) styleUrl = x_placemark.SelectSingleNode("styleUrl").ChildNodes[0].Value;
            if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);

            XmlNode sn = null;
            if (styleUrl != "")
            {
                string firstsid = styleUrl;
                XmlNodeList pk = l.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']/Pair/key");
                if (pk.Count > 0)
                    for (int n = 0; n < pk.Count; n++)
                    {
                        XmlNode cn = pk[n];
                        if ((cn.ChildNodes[0].Value != "normal") && (n > 0)) continue;
                        if (cn.ParentNode.SelectSingleNode("styleUrl") == null) continue;
                        firstsid = cn.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                        if (firstsid.IndexOf("#") == 0) firstsid = firstsid.Remove(0, 1);
                    };
                try
                {
                    XmlNode nts = l.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']/IconStyle/Icon/href");
                    string href = nts.ChildNodes[0].Value;
                    CRC32 crc = new CRC32();
                    string cc = crc.CRC32Num(l.file.tmp_file_dir + href).ToString();
                    InputBox.Show("CRC of Image", objects.SelectedItems[0].SubItems[0].Text + ":", cc);
                }
                catch 
                {
                    MessageBox.Show("Could not calculate CRC", "CRC of Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
            };
        }

        private void cHBD_Click(object sender, EventArgs e)
        {
            if (!objects.CheckBoxes) return;
            if (objects.CheckedItems.Count == 0) return;
            if (laySelect.Items.Count < 2) return;

            string[] layers = new string[laySelect.Items.Count];
            for (int i = 0; i < laySelect.Items.Count; i++)
                layers[i] = laySelect.Items[i].ToString();

            int new_ind = laySelect.SelectedIndex;
            if (System.Windows.Forms.InputBox.Show("Copy Placemarks", "Select Layer:", layers, ref new_ind) != DialogResult.OK) return;
            if (new_ind == laySelect.SelectedIndex) return;

            if (MessageBox.Show("Copy " + objects.CheckedItems.Count.ToString() + " placemark(s) to layer:\r\n" + parent.kmzLayers.Items[new_ind].ToString(), "Move placemarks", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            int[] pla = new int[3];
            KMLayer l_old = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            KMLayer l_new = (KMLayer)parent.kmzLayers.Items[new_ind];
            for (int i = objects.CheckedItems.Count - 1; i >= 0; i--)
            {
                string XPath = objects.CheckedItems[i].SubItems[7].Text;
                string indx = XPath.Substring(XPath.IndexOf("["));
                XPath = XPath.Remove(XPath.IndexOf("["));
                int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                XmlNode xn = l_old.file.kmlDoc.SelectNodes("kml/Document/Folder")[l_old.id];
                xn = xn.SelectNodes(XPath)[ind].ParentNode.ParentNode;
                if (xn.Name == "outerBoundaryIs") // polygon
                {
                    xn = xn.ParentNode.ParentNode;
                    pla[2]++;
                }
                else
                {
                    if (xn.SelectSingleNode("Point") != null) pla[0]++;
                    if (xn.SelectSingleNode("LineString") != null) pla[1]++;
                };
                xn = xn.Clone();

                if (l_old.file == l_new.file)
                    l_new.file.kmlDoc.SelectNodes("kml/Document/Folder")[l_new.id].AppendChild(xn);
                else
                {
                    // copy styles //
                    string styleUrl = "";
                    if (xn.SelectSingleNode("styleUrl") != null) styleUrl = xn.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                    if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);

                    if (styleUrl != "")
                    {
                        string firstsid = styleUrl;
                        XmlNodeList pk = l_old.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']");
                        if (pk.Count > 0) // copy style map
                        {
                            if (l_new.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']").Count == 0)
                                l_new.file.kmlDoc.SelectSingleNode("kml/Document").InnerXml += pk[0].OuterXml;
                        };
                        pk = l_old.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + styleUrl + "']/Pair/key");
                        if (pk.Count > 0)
                            for (int n = 0; n < pk.Count; n++)
                            {
                                XmlNode cn = pk[n];
                                if ((cn.ChildNodes[0].Value != "normal") && (n > 0)) continue;
                                if (cn.ParentNode.SelectSingleNode("styleUrl") == null) continue;
                                firstsid = cn.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                                if (firstsid.IndexOf("#") == 0) firstsid = firstsid.Remove(0, 1);
                            };
                        try // copy style
                        {
                            XmlNode nts = l_old.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']");
                            if (l_new.file.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + firstsid + "']").Count == 0)
                                l_new.file.kmlDoc.SelectSingleNode("kml/Document").InnerXml += nts.OuterXml;
                        }
                        catch { };
                        try // copy icons
                        {
                            XmlNode nts = l_old.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + firstsid + "']/IconStyle/Icon/href");
                            string href = nts.ChildNodes[0].Value;
                            if (!String.IsNullOrEmpty(href))
                            {
                                href = href.Replace("/", @"\");
                                System.IO.File.Copy(l_old.file.tmp_file_dir + href, l_new.file.tmp_file_dir + href, false);
                            };
                        }
                        catch { };
                    };
                    /////////////////
                    l_new.file.kmlDoc.SelectNodes("kml/Document/Folder")[l_new.id].InnerXml += xn.OuterXml;
                };
            };
            l_old.file.SaveKML();
            if (l_old.file != l_new.file)
                l_new.file.SaveKML();
            laySelect.Items[new_ind] = l_new.ToString();
            laySelect.Items[laySelect.SelectedIndex] = l_old.ToString();
            l_new.placemarks += pla[0] + pla[1] + pla[2];
            l_new.points += pla[0];
            l_new.lines += pla[1];
            l_new.areas += pla[2];
            
            laySelect.Items[laySelect.SelectedIndex] = l_old.ToString();
            laySelect.Items[new_ind] = l_new.ToString();
            parent.Refresh();
        }

        private void sba_Click(object sender, EventArgs e)
        {
            if (this.objects.Items.Count == 0) return;
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            List<KeyValuePair<XmlNode, double>> placemarks = new List<KeyValuePair<XmlNode, double>>();
            XmlNodeList nl = x_folder.SelectNodes("Placemark");
            foreach (XmlNode n in nl)
            {
                placemarks.Add(new KeyValuePair<XmlNode, double>(n, 0));
                x_folder.RemoveChild(n);
            };
            placemarks.Sort(PlaceMarkAtFolderSorter.Alpabetically);
            foreach (KeyValuePair<XmlNode, double> n in placemarks)
                x_folder.AppendChild(n.Key);
            
            l.file.SaveKML();            
            objects.Items.Clear();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString();
            parent.Refresh();
        }

        private void sbi_Click(object sender, EventArgs e)
        {
            if (this.objects.Items.Count == 0) return;
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            List<KeyValuePair<XmlNode, double>> placemarks = new List<KeyValuePair<XmlNode, double>>();
            XmlNodeList nl = x_folder.SelectNodes("Placemark");
            foreach (XmlNode n in nl)
            {
                placemarks.Add(new KeyValuePair<XmlNode, double>(n, 0));
                x_folder.RemoveChild(n);
            };
            placemarks.Sort(PlaceMarkAtFolderSorter.Alpabetically);
            placemarks.Reverse();
            foreach (KeyValuePair<XmlNode, double> n in placemarks)
                x_folder.AppendChild(n.Key);

            l.file.SaveKML();
            objects.Items.Clear();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString();
            parent.Refresh();
        }

        private void sbd_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (objects.Items.Count < 2) return;
            if(!(mapContent[objects.SelectedIndices[0]] is NaviMapNet.MapPoint)) return;
            NaviMapNet.MapPoint mp = (NaviMapNet.MapPoint)mapContent[objects.SelectedIndices[0]];

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            List<KeyValuePair<XmlNode, double>> placemarks = new List<KeyValuePair<XmlNode, double>>();
            XmlNodeList nl = x_folder.SelectNodes("Placemark");
            foreach (XmlNode n in nl)
            {
                double dist = double.MaxValue;
                XmlNode xn = n.SelectNodes("*/coordinates")[0];
                string[] xy = xn.ChildNodes[0].Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ln in xy)
                {
                    try
                    {
                        string[] xyz = ln.Split(new char[]{','}, StringSplitOptions.None);
                        PointF cp = new PointF((float)double.Parse(xyz[0],System.Globalization.CultureInfo.InvariantCulture),(float)double.Parse(xyz[1],System.Globalization.CultureInfo.InvariantCulture));
                        float d = PolyLineBuffer.PolyLineBufferCreator.GeographicDistFunc(mp.Points[0], cp);
                        if (d < dist) dist = d;
                    }
                    catch { };
                };
                placemarks.Add(new KeyValuePair<XmlNode, double>(n, dist));
                x_folder.RemoveChild(n);
            };
            placemarks.Sort(PlaceMarkAtFolderSorter.ByDistance);
            foreach (KeyValuePair<XmlNode, double> n in placemarks)
                x_folder.AppendChild(n.Key);

            l.file.SaveKML();    
            objects.Items.Clear();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString();
            parent.Refresh();
        }

        private void sbls_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (objects.Items.Count < 2) return;
            if (!(mapContent[objects.SelectedIndices[0]] is NaviMapNet.MapPolyLine)) return;
            NaviMapNet.MapPolyLine ml = (NaviMapNet.MapPolyLine)mapContent[objects.SelectedIndices[0]];
            SortByRoute(ml.Points, false);
        }   

        private void sbl_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (objects.Items.Count < 2) return;
            if (!(mapContent[objects.SelectedIndices[0]] is NaviMapNet.MapPolyLine)) return;
            NaviMapNet.MapPolyLine ml = (NaviMapNet.MapPolyLine)mapContent[objects.SelectedIndices[0]];
            SortByRoute(ml.Points, true);
        }

        private PointF[] loadroute()
        {
            string filename = null;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "KML, GPX & Shape files (*.kml;*.gpx;*.shp)|*.kml;*.gpx;*.shp";
            ofd.DefaultExt = "*.kml,*.gpx";
            if (ofd.ShowDialog() == DialogResult.OK)
                filename = ofd.FileName;
            ofd.Dispose();

            if (String.IsNullOrEmpty(filename)) return null;
            if (!File.Exists(filename)) return null;

            System.IO.FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            System.IO.StreamReader sr = new StreamReader(fs);
            List<PointF> res = new List<PointF>();

            if (System.IO.Path.GetExtension(filename).ToLower() == ".shp")
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
                                    res.Add(ap);
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
                        res.Add(new PointF(float.Parse(xyz[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(xyz[1], System.Globalization.CultureInfo.InvariantCulture)));
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
                        res.Add(new PointF(float.Parse(lon, System.Globalization.CultureInfo.InvariantCulture), float.Parse(lat, System.Globalization.CultureInfo.InvariantCulture)));

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
                        res.Add(new PointF(float.Parse(lon, System.Globalization.CultureInfo.InvariantCulture), float.Parse(lat, System.Globalization.CultureInfo.InvariantCulture)));

                        si = file.IndexOf("<trkpt", ei);
                        if (si > 0)
                            ei = file.IndexOf(">", si);
                    };
                };
            };
            sr.Close();
            fs.Close();

            return res.ToArray();
        }

        private void sbrn_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count < 2) return;
            SortByRoute(loadroute(), false);
        }

        private void sbrs_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count < 2) return;
            SortByRoute(loadroute(), true);
        }    

        private void SortByRoute(PointF[] route, bool fromStart)
        {
            if (route == null) return;
            if (route.Length < 2) return;
            if (objects.Items.Count < 2) return;
            if (objects.SelectedItems.Count == 0) return;

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            List<KeyValuePair<XmlNode, double>> placemarks = new List<KeyValuePair<XmlNode, double>>();
            XmlNodeList nl = x_folder.SelectNodes("Placemark");
            foreach (XmlNode n in nl)
            {
                double dist = double.MaxValue;
                XmlNode xn = n.SelectNodes("*/coordinates")[0];
                string[] xy = xn.ChildNodes[0].Value.Trim('\n').Trim('\r').Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                try
                {
                    string[] xyz = xy[0].Split(new char[] { ',' }, StringSplitOptions.None);
                    PointF cp = new PointF((float)double.Parse(xyz[0], System.Globalization.CultureInfo.InvariantCulture), (float)double.Parse(xyz[1], System.Globalization.CultureInfo.InvariantCulture));
                    float dfs;
                    float d2l = PolyLineBuffer.PolyLineBufferCreator.DistanceFromPointToRoute(cp, route, PolyLineBuffer.PolyLineBufferCreator.GeographicDistFunc, out dfs);
                    if(!fromStart) dfs = d2l;
                    if (dfs < dist) dist = dfs;
                }
                catch { };
                placemarks.Add(new KeyValuePair<XmlNode, double>(n, dist));
                x_folder.RemoveChild(n);
            };
            placemarks.Sort(PlaceMarkAtFolderSorter.ByDistance);
            foreach (KeyValuePair<XmlNode, double> n in placemarks)
                x_folder.AppendChild(n.Key);

            l.file.SaveKML();
            objects.Items.Clear();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString();
            parent.Refresh();
        }

        public class PlaceMarkAtFolderSorter : IComparer<KeyValuePair<XmlNode, double>>
        {
            private byte sType = 0; // 0 - Asc, 1 - Dist, 2 - Dist Between
            private PlaceMarkAtFolderSorter() { }
            public static PlaceMarkAtFolderSorter Alpabetically 
            {
                get
                {
                    PlaceMarkAtFolderSorter res = new PlaceMarkAtFolderSorter();
                    res.sType = 0;
                    return res;
                }
            }
            public static PlaceMarkAtFolderSorter ByDistance
            {
                get
                {
                    PlaceMarkAtFolderSorter res = new PlaceMarkAtFolderSorter();
                    res.sType = 1;
                    return res;
                }
            }

            public int Compare(KeyValuePair<XmlNode, double> A, KeyValuePair<XmlNode, double> B)
            {
                if((A.Key == null) && (B.Key == null)) return 0;
                if (B.Key == null) return -1;
                if (A.Key == null) return 1;                

                if (sType == 0)
                {
                    string nameA = String.Empty;
                    string nameB = String.Empty;
                    try { nameA = A.Key.SelectSingleNode("name").ChildNodes[0].Value; }
                    catch { };
                    try { nameB = B.Key.SelectSingleNode("name").ChildNodes[0].Value; }
                    catch { };
                    return nameA.CompareTo(nameB);
                }
                else if (sType == 2)
                {
                    double dist = double.MaxValue;
                    PointF pa = PointF.Empty;
                    PointF pb = PointF.Empty;
                    try
                    {
                        XmlNode xn = A.Key.SelectNodes("*/coordinates")[0];
                        string[] xy = xn.ChildNodes[0].Value.Trim('\n').Trim('\r').Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        string[] xyz = xy[0].Split(new char[] { ',' }, StringSplitOptions.None);
                        pa = new PointF((float)double.Parse(xyz[0], System.Globalization.CultureInfo.InvariantCulture), (float)double.Parse(xyz[1], System.Globalization.CultureInfo.InvariantCulture));
                    }
                    catch { };
                    try
                    {
                        XmlNode xn = B.Key.SelectNodes("*/coordinates")[0];
                        string[] xy = xn.ChildNodes[0].Value.Trim('\n').Trim('\r').Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        string[] xyz = xy[0].Split(new char[] { ',' }, StringSplitOptions.None);
                        pb = new PointF((float)double.Parse(xyz[0], System.Globalization.CultureInfo.InvariantCulture), (float)double.Parse(xyz[1], System.Globalization.CultureInfo.InvariantCulture));
                    }
                    catch { };
                    try
                    {
                        dist = PolyLineBuffer.PolyLineBufferCreator.GeographicDistFunc(pa, pb);
                    }
                    catch { };
                    return (int)dist;
                }
                else
                {
                    return A.Value.CompareTo(B.Value);
                };
            }
        }

        private void showRouteBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showRouteBarToolStripMenuItem.Checked = !showRouteBarToolStripMenuItem.Checked;
            routeBar.Visible = showRouteBarToolStripMenuItem.Checked;
            if (routeBar.Visible && (groute == null))
            {
                InputBox.defWidth = 600;
                groute = GetRouter.Load();
                rbSet.Text = String.Format("Set ({0})", groute.ServiceIndex);
                rbGet.Text = String.Format("Get ({0})", groute.ServiceIndex);
                if (groute.mode == 0) rbDN_Click(sender, e);
                if (groute.mode == 1) rbStFi_Click(sender, e);
                if (groute.mode == 2) rbSt_Click(sender, e);
                if (groute.mode == 3) rbFi_Click(sender, e);
                if (groute.mode == 4) rbMi_Click(sender, e);
                if (groute.getroute) rbGR_Click(sender, e);
                if (groute.saveroute) rbSL_Click(sender, e);
            };
            if (routeBar.Visible && (groute.mode == 0)) rbStFi_Click(sender, e);
        }

        private void rbGR_Click(object sender, EventArgs e)
        {
            rbGR.Checked = !rbGR.Checked;
            groute.getroute = rbGR.Checked;
        }

        private void rbSL_Click(object sender, EventArgs e)
        {
            rbSL.Checked = !rbSL.Checked;
            groute.saveroute = rbSL.Checked;
        }

        private void rbDN_Click(object sender, EventArgs e)
        {
            groute.mode = 0;
            rbDN.Checked = true;
            rbStFi.Checked = false;
            rbSt.Checked = false;            
            rbFi.Checked = false;
            rbMi.Checked = false;
            oncb.Text = rbDN.Text;
        }

        private void rbStFi_Click(object sender, EventArgs e)
        {
            groute.mode = 1;
            rbDN.Checked = false;
            rbStFi.Checked = true;
            rbSt.Checked = false;
            rbFi.Checked = false;
            rbMi.Checked = false;
            oncb.Text = rbStFi.Text;
        }

        private void rbSt_Click(object sender, EventArgs e)
        {
            groute.mode = 2;
            rbDN.Checked = false;
            rbStFi.Checked = false;
            rbSt.Checked = true;
            rbFi.Checked = false;
            rbMi.Checked = false;
            oncb.Text = rbSt.Text;
        }

        private void rbFi_Click(object sender, EventArgs e)
        {
            groute.mode = 3;
            rbDN.Checked = false;
            rbStFi.Checked = false;
            rbSt.Checked = false;
            rbFi.Checked = true;
            rbMi.Checked = false;
            oncb.Text = rbFi.Text;
        }

        private void setURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (groute == null) return;
            if (groute.service < 0)
            {
                string url = groute.ServiceURL;
                List<string> urls = new List<string>();
                urls.Add("http://localhost:8080/nms/");
                if (!urls.Contains(url)) urls.Insert(0, url);
                if (InputBox.Show("Web Route Engine", "Enter HTTP Url to " + groute.ServiceName + ":", urls.ToArray(), ref url, true) != DialogResult.OK) return;
                url = url.Trim();
                int si = -1;
                for (int i = 0; i < groute.url_dkxce.Count; i++)
                    if (groute.url_dkxce[i].url == url)
                        si = i;
                if (si >= 0)
                {
                    groute.service = (si + 1) * -1;
                    rbSet.Text = String.Format("Set ({0})", groute.ServiceIndex);
                    rbGet.Text = String.Format("Get ({0})", groute.ServiceIndex);
                }
                else
                {
                    GetRouter.DRSParams p = new GetRouter.DRSParams();
                    p.url = url;
                    Uri uri = new Uri(url);
                    p.name = uri.Host + ":" + uri.Port + " # "+DateTime.Now.ToString("HHmmssMMddyy");
                    groute.url_dkxce.Add(p);
                    groute.service = (groute.url_dkxce.Count) * -1;
                    rbSet.Text = String.Format("Set ({0})", groute.ServiceIndex);
                    rbGet.Text = String.Format("Get ({0})", groute.ServiceIndex);
                };
            }
            else
            {
                string url = groute.ServiceURL;
                if (InputBox.Show("Web Route Engine", "Enter HTTP Url to " + groute.ServiceName + ":", ref url) != DialogResult.OK) return;
                url = url.Trim();
                int si = -1;
                for (int i = 0; i < groute.url_osrm.Count; i++)
                    if (groute.url_osrm[i].url == url)
                        si = i;
                if (si >= 0)
                {
                    groute.service = (si + 1);
                    rbSet.Text = String.Format("Set ({0})", groute.ServiceIndex);
                    rbGet.Text = String.Format("Get ({0})", groute.ServiceIndex);
                }
                else
                {
                    GetRouter.OSRMParams p = new GetRouter.OSRMParams();
                    p.url = url;
                    Uri uri = new Uri(url);
                    p.name = uri.Host + ":" + uri.Port + " # " + DateTime.Now.ToString("HHmmssMMddyy");
                    groute.url_osrm.Add(p);
                    groute.service = (groute.url_osrm.Count);
                    rbSet.Text = String.Format("Set ({0})", groute.ServiceIndex);
                    rbGet.Text = String.Format("Get ({0})", groute.ServiceIndex);
                };
            };
        }

        private void setKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (groute.service >= 0) return;
            int ki = groute.service * -1 - 1;

            string key = groute.url_dkxce[ki].key;
            List<string> keys = new List<string>();
            keys.Add("TEST");
            if (!keys.Contains(key)) keys.Insert(0, key);
            if (InputBox.Show("Web Route Engine", "Enter Key to " + groute.ServiceName + ":", keys.ToArray(), ref key, true) != DialogResult.OK) return;
            groute.url_dkxce[ki].key = key;
        }

        private void rbSet_ButtonClick(object sender, EventArgs e)
        {
            setURLToolStripMenuItem_Click(sender, e);
        }

        private void rbClear_ButtonClick(object sender, EventArgs e)
        {
            if (groute != null)
            {
                groute.start = null;
                groute.finish = null;
                groute.counter = 0;
            };
            rtStart.Text = "START";
            rtFinish.Text = "FINISH";
            mapRoute.Clear();
            mapRStart = null;
            mapRFinish = null;
            mapRVector = null;
            rbSave.Enabled = false;
            MapViewer.DrawOnMapData();
        }

        private void SubClick(PointF click, string name)
        {
            if (!showRouteBarToolStripMenuItem.Checked) return;
            if (groute == null) return;
            if (groute.mode == 0) return;

            groute.counter++;
            if (groute.mode == 1)
            {
                if ((groute.counter % 2) > 0)
                {
                    if (mapRStart == null)
                    {
                        mapRStart = new NaviMapNet.MapPoint();
                        mapRStart.Squared = true;
                        mapRStart.SizePixels = new Size(12, 12);
                        mapRStart.Img = global::KMZRebuilder.Properties.Resources.rStart;
                        mapRoute.Add(mapRStart);
                    };
                    mapRStart.Points = new PointF []{click};
                    groute.start = new object[] { name, click.X, click.Y };
                    rtStart.Text = String.IsNullOrEmpty(name) ? String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", click.Y, click.X) : name;
                }
                else
                {
                    if (mapRFinish == null)
                    {
                        mapRFinish = new NaviMapNet.MapPoint();
                        mapRFinish.Squared = true;
                        mapRFinish.SizePixels = new Size(12, 12);
                        mapRFinish.Img = global::KMZRebuilder.Properties.Resources.rFinish;
                        mapRoute.Add(mapRFinish);
                    };
                    mapRFinish.Points = new PointF[] { click };
                    groute.finish = new object[] { name, click.X, click.Y };
                    rtFinish.Text = String.IsNullOrEmpty(name) ? String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", click.Y, click.X) : name;
                };
            }
            else if (groute.mode == 2)
            {
                if (mapRStart == null) 
                {
                    mapRStart = new NaviMapNet.MapPoint();
                    mapRStart.Squared = true;
                    mapRStart.SizePixels = new Size(12, 12);
                    mapRStart.Img = global::KMZRebuilder.Properties.Resources.rStart;
                    mapRoute.Add(mapRStart);
                };
                mapRStart.Points = new PointF[] { click };
                groute.start = new object[] { name, click.X, click.Y };
                rtStart.Text = String.IsNullOrEmpty(name) ? String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", click.Y, click.X) : name;
            }
            else if (groute.mode == 3)
            {
                if (mapRFinish == null) 
                {
                    mapRFinish = new NaviMapNet.MapPoint();
                    mapRFinish.Squared = true;
                    mapRFinish.SizePixels = new Size(12, 12);
                    mapRFinish.Img = global::KMZRebuilder.Properties.Resources.rFinish;
                    mapRoute.Add(mapRFinish);
                };
                mapRFinish.Points = new PointF[] { click };
                groute.finish = new object[] { name, click.X, click.Y };
                rtFinish.Text = String.IsNullOrEmpty(name) ? String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", click.Y, click.X) : name;
            }
            else if (groute.mode == 4)
            {
                MultiPointFormShow(null);
                if (String.IsNullOrEmpty(name)) name = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", click.Y, click.X);                                
                NaviMapNet.MapPoint mapRPoint = new NaviMapNet.MapPoint();
                mapRPoint.Squared = true;
                mapRPoint.SizePixels = new Size(16, 16);
                mapRPoint.Img = GetRouter.ImageFromNumber(mapRMulti.Count + 1);
                mapRPoint.Points = new PointF[] { click };                
                mapRPoint.Text = name;                
                mapRoute.Add(mapRPoint);
                mapRMulti.AddPoint(new KeyValuePair<string, PointF>(name, click), mapRPoint);
                MapViewer.DrawOnMapData();
                return;
            };
            AfterClick();
        }

        private void MultiPointFormShow(List<KeyValuePair<string, PointF>> pArr)
        {
            if (mapRMulti == null)
            {                
                mapRMulti = new MultiPointRouteForm();
                mapRMulti.StartPosition = FormStartPosition.Manual;
                mapRMulti.Left = this.Left + this.Width - objects.Width;
                mapRMulti.Top = this.Top + panel1.Height;
                mapRMulti.buttonOk.Click += new EventHandler(buttonOk_Click);
                mapRMulti.buttonCancel.Click += new EventHandler(buttonCancel_Click);
                mapRMulti.FormClosed += new FormClosedEventHandler(mapRMulti_FormClosed);
                multirouteFromCheckedToolStripMenuItem.Enabled = false;
                mapRMulti.TopMost = true;
                mapRMulti.Show();
            };            
            if (pArr != null) mapRMulti.Points = pArr;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {            
            mapRMulti.Clear();
            mapRMulti.Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {            
            mapRMulti.Close();
        }

        private void mapRMulti_FormClosed(object sender, FormClosedEventArgs e)
        {
            bool hasMarkers = false;
            for (int i = 0; i < mapRMulti.OnMapPoints.Count; i++)
                try { hasMarkers = true; mapRoute.Remove(mapRMulti.OnMapPoints[i]); } catch { };

            List<KeyValuePair<string, PointF>> pArr = mapRMulti.Points;
            multirouteFromCheckedToolStripMenuItem.Enabled = true;
            mapRMulti.Dispose();
            mapRMulti = null;

            if (pArr.Count < 2) 
            {
                if(hasMarkers) MapViewer.DrawOnMapData();
                try
                {
                    MapViewer.Focus();
                    MapViewer.Select();
                }
                catch { };
                return;
            };
            List<PointF> pVector = new List<PointF>();
            for (int i = 0; i < pArr.Count; i++) pVector.Add(pArr[i].Value);
            wbf.Show("Get Route: Multipoints", "Wait, requesting route of " + pVector.Count.ToString() + " points");
            PointF[] vector = null;
            nmsRouteClient.Route route = null;
            rtStatus.Text = "Request route...";
            Application.DoEvents();
            double rLength = groute.GetRoute(pVector.ToArray(), wbf, out vector, out route);

            wbf.Hide();
            if (mapRVector != null) mapRoute.Remove(mapRVector);
            Application.DoEvents();
            if ((rLength == double.MaxValue) || (vector == null))
            {
                rtStatus.Text = "No route found";
                MapViewer.DrawOnMapData();
                try
                {
                    MapViewer.Focus();
                    MapViewer.Select();
                }
                catch { };
                return;
            };

            rtStatus.Text = String.Format(System.Globalization.CultureInfo.InvariantCulture, "Route length: {0:0.00} km", rLength / 1000.0);

            mapRVector = new NaviMapNet.MapPolyLine(vector);
            mapRVector.Color = groute.color;
            mapRVector.Width = groute.width;
            mapRVector.UserData = route;
            mapRoute.Add(mapRVector);

            MapViewer.DrawOnMapData();
            rbSave.Enabled = true;
            if (rbSL.Checked) SaveResult(mapRVector, false);
            try
            {
                MapViewer.Focus();
                MapViewer.Select();
            }
            catch { };

            //string XML = "";
            //string style = "newstyle" + DateTime.UtcNow.ToString("HHmmssfff");
            //{                
            //    string polys = "";
            //    string comms = "";

            //    if (!String.IsNullOrEmpty(route.LastError))
            //    {
            //        double ttld = 0;
            //        for (int j = 0; j < pVector.Count; j++)
            //        {
            //            polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", pVector[j].X, pVector[j].Y, 0);
            //            polys += " ";
            //            if (j > 0)
            //                ttld += GetLengthMetersC(pVector[j - 1].Y, pVector[j - 1].X, pVector[j].Y, pVector[j].X, false);
            //        };
            //        comms = String.Format("Length: {0:0.0}\r\nError: {1}", ttld / 1024.0, route.LastError);
            //    }
            //    else
            //    {
            //        comms = String.Format(System.Globalization.CultureInfo.InvariantCulture, "Route: {0} - {1}\r\nLength: {2:0.0} km\r\nTime: {3:0.0} min\r\nFrom: {4}\r\nSName: {5}Engine: {6}", rtStart.Text, points[i].Name, route.driveLength / 1024.0, route.driveTime, groute.ServiceURL, groute.ServiceName, groute.ServiceEngine);
            //        for (int j = 0; j < route.polyline.Length; j++)
            //        {
            //            if (polys.Length > 0) polys += " ";
            //            polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", route.polyline[j].x, route.polyline[j].y, 0);
            //        };
            //    };

            //    XML += "<Placemark>\r\n";
            //    XML += "<name><![CDATA[Multiroute]]></name>\r\n";
            //    XML += "<styleUrl>#" + style + "</styleUrl>\r\n";
            //    XML += "<description><![CDATA[" + comms + "]]></description>\r\n";
            //    XML += "<LineString>\r\n";
            //    XML += "<coordinates>" + polys + "</coordinates>\r\n";
            //    XML += "</LineString>\r\n";
            //    XML += "</Placemark>\r\n";
            //};            
            //wbf.Hide();

            //KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            //XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            //xf.InnerXml += XML;
            //l.file.kmlDoc.SelectNodes("kml/Document")[0].InnerXml += style;

            //l.file.SaveKML();
            //l.placemarks += points.Length;
            //l.lines += points.Length;
            //parent.Refresh();
            //laySelect.Items[laySelect.SelectedIndex] = l.ToString(); 
        }

        private void AfterClick()
        {
            MapViewer.DrawOnMapData();
            if (rbGR.Checked)
            {
                bool ok = rbSave.Enabled = GetRoute();
                if (rbSL.Checked && ok) SaveResult(mapRVector, false);
            };
        }

        private bool GetRoute()
        {
            if (groute == null) return false;
            if ((groute.start == null) || (groute.finish == null)) return false;            
            if (mapRVector != null) mapRoute.Remove(mapRVector);
            //
            PointF a = new PointF((float)groute.start[1], (float)groute.start[2]);
            PointF b = new PointF((float)groute.finish[1], (float)groute.finish[2]);
            PointF[] vector = null;
            nmsRouteClient.Route route = null;
            rtStatus.Text = "Request route...";
            Application.DoEvents();            
            double rLength = groute.GetRoute(a, b, wbf, out vector, out route);
            wbf.Hide();
            Application.DoEvents();
            if ((rLength == double.MaxValue) || (vector == null))
            {
                rtStatus.Text = "No route found";
                MapViewer.DrawOnMapData();
                try
                {
                    MapViewer.Focus();
                    MapViewer.Select();
                }
                catch { };
                return false;
            };
            
            rtStatus.Text = String.Format(System.Globalization.CultureInfo.InvariantCulture, "Route length: {0:0.00} km", rLength / 1000.0);
            
            mapRVector = new NaviMapNet.MapPolyLine(vector);
            mapRVector.Color = groute.color;
            mapRVector.Width = groute.width;
            mapRVector.UserData = route;
            mapRoute.Add(mapRVector);

            MapViewer.DrawOnMapData();
            try
            {
                MapViewer.Focus();
                MapViewer.Select();
            }
            catch { };
            return true;
        }

        private bool SaveResult(NaviMapNet.MapPolyLine polyline, bool askName)
        {
            if (polyline == null) return false;
            if (polyline.PointsCount < 2) return false;


            if(polyline.PointsCount > 100)
                wbf.Show("Get Route", "Wait, saving to layer...");
            string poly = "";
            foreach (PointF p in polyline.Points)
            {
                if (poly.Length > 0) poly += " ";
                poly += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", p.X, p.Y, 0);
            };
            string XML = "";
            string stylen = "newstyle" + DateTime.UtcNow.ToString("HHmmss");
            {
                string NAME = String.Format("{0} - {1}", rtStart.Text, rtFinish.Text);
                if (askName)
                {
                    wbf.Hide();
                    if(InputBox.Show("Save route to leayer", "Enter route name:", ref NAME) != DialogResult.OK)
                        NAME = String.Format("{0} - {1}", rtStart.Text, rtFinish.Text);
                    wbf.Show();
                };

                XML += "<Placemark>\r\n";
                XML += "<name><![CDATA[" + NAME + "]]></name>\r\n";
                XML += "<styleUrl>#" + stylen + "</styleUrl>\r\n";
                XML += "<description><![CDATA[" + String.Format(System.Globalization.CultureInfo.InvariantCulture, "Route: {0} - {1}\r\nLength: {2:0.0} km\r\nTime: {3:0.0} min\r\nFrom: {4}\r\nSName: {5}\r\nEngine: {6}", rtStart.Text, rtFinish.Text, ((nmsRouteClient.Route)polyline.UserData).driveLength / 1024.0, ((nmsRouteClient.Route)polyline.UserData).driveTime, groute.ServiceURL, groute.ServiceName, groute.ServiceEngine) + "]]></description>\r\n";
                XML += "<LineString>\r\n";
                XML += "<coordinates>" + poly+ "</coordinates>\r\n";
                XML += "</LineString>\r\n";
                XML += "</Placemark>\r\n";
            };
            
            string style = "<Style id=\"" + stylen + "\"><LineStyle><color>" + AddTextAsPoly.HexStyleConverter(GetRandomColor()) + "</color><width>" + (5).ToString() + "</width></LineStyle>";
            style += "</Style>\r\n";

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            xf.InnerXml += XML;
            l.file.kmlDoc.SelectNodes("kml/Document")[0].InnerXml += style;

            l.file.SaveKML();
            l.placemarks++;
            l.lines++;
            parent.Refresh();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString();
            wbf.Hide();

            return true;
        }

        private void rbGet_ButtonClick(object sender, EventArgs e)
        {
            rbSave.Enabled = GetRoute();
        }

        private void rbSave_ButtonClick(object sender, EventArgs e)
        {
            SaveResult(mapRVector, false);
        }

        private void setToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (groute.service >= 0) return;
            int ki = groute.service * -1 - 1;

            string ra = groute.url_dkxce[ki].ra;
            List<string> ras = new List<string>();
            ras.Add("00000000000000000000000000000000");
            if (!ras.Contains(ra)) ras.Insert(0, ra);
            if (InputBox.Show("Web Route Engine", "Enter RA (Route Attributes) to " + groute.ServiceName + ":", ras.ToArray(), ref ra, true) != DialogResult.OK) return;
            groute.url_dkxce[ki].ra = ra;
        }

        private void rbSw_ButtonClick(object sender, EventArgs e)
        {
            if (groute.start == null) return;
            if (groute.finish == null) return;

            object[] tmp = groute.start;
            groute.start = groute.finish;
            groute.finish = tmp;

            string txt = rtStart.Text;
            rtStart.Text = rtFinish.Text;
            rtFinish.Text = txt;

            PointF[] tmv = mapRStart.Points;
            mapRStart.Points = mapRFinish.Points;
            mapRFinish.Points = tmv;

            AfterClick();
        }

        private void setColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color c = groute.color;
            if (InputBox.QueryColorBox("Web Route Engine", "Select color for Route:", ref c) != DialogResult.OK) return;
            groute.color = c;
        }

        private void setWidthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int w = groute.width;
            if (InputBox.Show("Web Route Engine", "Select width for Route:", ref w, 2, 20) != DialogResult.OK) return;
            groute.width = w;
        }

        private void rbGet_DropDownOpening(object sender, EventArgs e)
        {
            
        }

        private NaviMapNet.MapPoint[] GetCheckedPoints()
        {
            List<NaviMapNet.MapPoint> res = new List<NaviMapNet.MapPoint>();
            for (int i = 0; i < objects.CheckedItems.Count; i++)
            {
                string t = objects.CheckedItems[i].SubItems[1].Text;
                if (t != "Point") continue;
                int id = objects.CheckedItems[i].Index;
                if (mapContent[id].ObjectType != NaviMapNet.MapObjectType.mPoint) continue;
                res.Add((NaviMapNet.MapPoint)mapContent[id]);
            };
            return res.ToArray();
        }

        private NaviMapNet.MapPolyLine[] GetCheckedLines()
        {
            List<NaviMapNet.MapPolyLine> res = new List<NaviMapNet.MapPolyLine>();
            for (int i = 0; i < objects.CheckedItems.Count; i++)
            {
                string t = objects.CheckedItems[i].SubItems[1].Text;
                if (!t.StartsWith("Line")) continue;
                int id = objects.CheckedItems[i].Index;
                if (mapContent[id].ObjectType != NaviMapNet.MapObjectType.mPolyline) continue;
                res.Add((NaviMapNet.MapPolyLine)mapContent[id]);
            };
            return res.ToArray();
        }

        private void rtt1_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) { MessageBox.Show("No layer objects found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            if (objects.CheckedItems.Count == 0) { MessageBox.Show("No checked objects found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            if (groute == null) { MessageBox.Show("Route layer not initialized!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            if (groute.start == null) { MessageBox.Show("Route start point is not set!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };

            NaviMapNet.MapPoint[] points = GetCheckedPoints();
            if ((points == null) || (points.Length == 0)) { MessageBox.Show("No checked points found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };

            string XML = "";
            List<string> styles = new List<string>();
            for (int i = 0; i < points.Length; i++)
            {
                wbf.Show("Get Route: Start to Checked", "Wait, requesting route " + (i+1).ToString() + "/" + points.Length.ToString());

                string captn = String.Format("{0} - {1}", rtStart.Text, points[i].Name);                
                PointF a = new PointF((float)groute.start[1], (float)groute.start[2]);
                nmsRouteClient.Route route = groute.GetRoute(a, points[i].Center);
                
                string polys = "";
                string comms = "";                
                
                if (!String.IsNullOrEmpty(route.LastError))
                {
                    polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", a.X, a.Y, 0);
                    polys += " ";
                    polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", points[i].Center.X, points[i].Center.Y, 0);
                    comms = String.Format("Length: {0:0.0}\r\nError: {1}", GetLengthMetersC(a.Y, a.X, points[i].Y, points[i].X, false) / 1024.0, route.LastError);
                }
                else
                {
                    comms = String.Format(System.Globalization.CultureInfo.InvariantCulture, "Route: {0} - {1}\r\nLength: {2:0.0} km\r\nTime: {3:0.0} min\r\nFrom: {4}\r\nSName: {5}Engine: {6}", rtStart.Text, points[i].Name, route.driveLength / 1024.0, route.driveTime, groute.ServiceURL, groute.ServiceName, groute.ServiceEngine);
                    for (int j = 0; j < route.polyline.Length; j++)
                    {
                        if (polys.Length > 0) polys += " ";
                        polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", route.polyline[j].x, route.polyline[j].y, 0);
                    };                    
                };

                string stylen = "newstyle" + DateTime.UtcNow.ToString("HHmmssfff");
                styles.Add(stylen);

                XML += "<Placemark>\r\n";
                XML += "<name><![CDATA[" + captn + "]]></name>\r\n";
                XML += "<styleUrl>#" + stylen + "</styleUrl>\r\n";
                XML += "<description><![CDATA[" + comms + "]]></description>\r\n";
                XML += "<LineString>\r\n";
                XML += "<coordinates>" + polys + "</coordinates>\r\n";
                XML += "</LineString>\r\n";
                XML += "</Placemark>\r\n";
            };

            string style = "";
            foreach (string st in styles)
            {
                style += "<Style id=\"" + st + "\"><LineStyle><color>" + AddTextAsPoly.HexStyleConverter(GetRandomColor()) + "</color><width>" + (5).ToString() + "</width></LineStyle>";
                style += "</Style>\r\n";
            }

            wbf.Hide();

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            xf.InnerXml += XML;
            l.file.kmlDoc.SelectNodes("kml/Document")[0].InnerXml += style;

            l.file.SaveKML();
            l.placemarks += points.Length;
            l.lines += points.Length;
            parent.Refresh();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString(); 
        }

        private void rtt2_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) { MessageBox.Show("No layer objects found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            if (objects.CheckedItems.Count == 0) { MessageBox.Show("No checked objects found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            if (groute == null) { MessageBox.Show("Route layer not initialized!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            if (groute.finish == null) { MessageBox.Show("Route finish point is not set!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };

            NaviMapNet.MapPoint[] points = GetCheckedPoints();
            if ((points == null) || (points.Length == 0)) { MessageBox.Show("No checked points found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };

            string XML = "";
            List<string> styles = new List<string>();
            for (int i = 0; i < points.Length; i++)
            {
                wbf.Show("Get Route: Checked to Finish", "Wait, requesting route " + (i + 1).ToString() + "/" + points.Length.ToString());

                string captn = String.Format("{0} - {1}", points[i].Name, rtFinish.Text);
                PointF b = new PointF((float)groute.finish[1], (float)groute.finish[2]);
                nmsRouteClient.Route route = groute.GetRoute(points[i].Center, b);

                string polys = "";
                string comms = "";

                if (!String.IsNullOrEmpty(route.LastError))
                {
                    polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", points[i].Center.X, points[i].Center.Y, 0);
                    polys += " ";
                    polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", b.X, b.Y, 0);
                    comms = String.Format("Length: {0:0.0}\r\nError: {1}", GetLengthMetersC(points[i].Y, points[i].X, b.Y, b.X, false) / 1024.0, route.LastError);
                }
                else
                {
                    comms = String.Format(System.Globalization.CultureInfo.InvariantCulture, "Route: {0} - {1}\r\nLength: {2:0.0} km\r\nTime: {3:0.0} min\r\nFrom: {4}\r\nSName: {5}Engine: {6}", points[i].Name, rtFinish.Text, route.driveLength / 1024.0, route.driveTime, groute.ServiceURL, groute.ServiceName, groute.ServiceEngine);
                    for (int j = 0; j < route.polyline.Length; j++)
                    {
                        if (polys.Length > 0) polys += " ";
                        polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", route.polyline[j].x, route.polyline[j].y, 0);
                    };
                };

                string stylen = "newstyle" + DateTime.UtcNow.ToString("HHmmssfff");
                styles.Add(stylen);

                XML += "<Placemark>\r\n";
                XML += "<name><![CDATA[" + captn + "]]></name>\r\n";
                XML += "<styleUrl>#" + stylen + "</styleUrl>\r\n";
                XML += "<description><![CDATA[" + comms + "]]></description>\r\n";
                XML += "<LineString>\r\n";
                XML += "<coordinates>" + polys + "</coordinates>\r\n";
                XML += "</LineString>\r\n";
                XML += "</Placemark>\r\n";
            };

            string style = "";
            foreach (string st in styles)
            {
                style += "<Style id=\"" + st + "\"><LineStyle><color>" + AddTextAsPoly.HexStyleConverter(GetRandomColor()) + "</color><width>" + (5).ToString() + "</width></LineStyle>";
                style += "</Style>\r\n";
            }

            wbf.Hide();

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            xf.InnerXml += XML;
            l.file.kmlDoc.SelectNodes("kml/Document")[0].InnerXml += style;

            l.file.SaveKML();
            l.placemarks += points.Length;
            l.lines += points.Length;
            parent.Refresh();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString(); 
        }

        private void rtt3_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) { MessageBox.Show("No layer objects found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            if (objects.CheckedItems.Count == 0) { MessageBox.Show("No checked objects found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            if (groute == null) { MessageBox.Show("Route layer not initialized!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            
            NaviMapNet.MapPoint[] points = GetCheckedPoints();
            if ((points == null) || (points.Length == 0)) { MessageBox.Show("No checked points found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            NaviMapNet.MapPolyLine[] lines = GetCheckedLines();
            if ((lines == null) || (lines.Length == 0)) { MessageBox.Show("No checked lines found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };

            int sel = lines.Length == 1 ? 0 : SelectLineDialog(lines, "Get Route: Line to Checked");
            if (sel < 0) return;

            wbf.Show("Get Route: Line to Checked", "Wait, calculating line...");
            PointF[] nearest = new PointF[points.Length];
            double[] mindist = new double[points.Length];
            for (int j = 0; j < points.Length; j++) mindist[j] = double.MaxValue;
            for (int i = 0; i < lines[sel].PointsCount; i++)
            {
                for (int j = 0; j < points.Length; j++)
                {
                    double dist = GetLengthMetersC(points[j].Y, points[j].X, lines[sel].Points[i].Y, lines[sel].Points[i].X, false);
                    if (dist < mindist[j])
                    {                        
                        mindist[j] = dist;
                        nearest[j] = lines[sel].Points[i];
                    };
                };
            };

            string XML = "";
            List<string> styles = new List<string>();
            for (int i = 0; i < points.Length; i++)
            {
                wbf.Show("Get Route: Line to Checked", "Wait, requesting route " + (i + 1).ToString() + "/" + points.Length.ToString());

                string captn = String.Format("{0} - {1}", lines[sel].Name, points[i].Name);
                nmsRouteClient.Route route = groute.GetRoute(nearest[i], points[i].Center);

                string polys = "";
                string comms = "";

                if (!String.IsNullOrEmpty(route.LastError))
                {
                    polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", nearest[i].X, nearest[i].Y, 0);
                    polys += " ";
                    polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", points[i].Center.X, points[i].Center.Y, 0);
                    comms = String.Format("Length: {0:0.0}\r\nError: {1}", GetLengthMetersC(nearest[i].Y, nearest[i].X, points[i].Y, points[i].X, false) / 1024.0, route.LastError);
                }
                else
                {
                    comms = String.Format(System.Globalization.CultureInfo.InvariantCulture, "Route: {0} - {1}\r\nLength: {2:0.0} km\r\nTime: {3:0.0} min\r\nFrom: {4}\r\nSName: {5}Engine: {6}", lines[sel].Name, points[i].Name, route.driveLength / 1024.0, route.driveTime, groute.ServiceURL, groute.ServiceName, groute.ServiceEngine);
                    for (int j = 0; j < route.polyline.Length; j++)
                    {
                        if (polys.Length > 0) polys += " ";
                        polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", route.polyline[j].x, route.polyline[j].y, 0);
                    };
                };

                string stylen = "newstyle" + DateTime.UtcNow.ToString("HHmmssfff");
                styles.Add(stylen);

                XML += "<Placemark>\r\n";
                XML += "<name><![CDATA[" + captn + "]]></name>\r\n";
                XML += "<styleUrl>#" + stylen + "</styleUrl>\r\n";
                XML += "<description><![CDATA[" + comms + "]]></description>\r\n";
                XML += "<LineString>\r\n";
                XML += "<coordinates>" + polys + "</coordinates>\r\n";
                XML += "</LineString>\r\n";
                XML += "</Placemark>\r\n";
            };

            string style = "";
            foreach (string st in styles)
            {
                style += "<Style id=\"" + st + "\"><LineStyle><color>" + AddTextAsPoly.HexStyleConverter(GetRandomColor()) + "</color><width>" + (5).ToString() + "</width></LineStyle>";
                style += "</Style>\r\n";
            }

            wbf.Hide();

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            xf.InnerXml += XML;
            l.file.kmlDoc.SelectNodes("kml/Document")[0].InnerXml += style;

            l.file.SaveKML();
            l.placemarks += points.Length;
            l.lines += points.Length;
            parent.Refresh();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString(); 
        }

        private void rtt4_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) { MessageBox.Show("No layer objects found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; }
            if (objects.CheckedItems.Count == 0) { MessageBox.Show("No checked objects found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; }
            if (groute == null) { MessageBox.Show("Route layer not initialized!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; }

            NaviMapNet.MapPoint[] points = GetCheckedPoints();
            if ((points == null) || (points.Length == 0)) { MessageBox.Show("No checked points found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            NaviMapNet.MapPolyLine[] lines = GetCheckedLines();
            if ((lines == null) || (lines.Length == 0)) { MessageBox.Show("No checked lines found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };

            int sel = lines.Length == 1 ? 0 : SelectLineDialog(lines, "Get Route: Checked to Line");
            if (sel < 0) return;

            wbf.Show("Get Route: Checked to Line", "Wait, calculating line...");
            PointF[] nearest = new PointF[points.Length];
            double[] mindist = new double[points.Length];
            for (int j = 0; j < points.Length; j++) mindist[j] = double.MaxValue;
            for (int i = 0; i < lines[sel].PointsCount; i++)
            {
                for (int j = 0; j < points.Length; j++)
                {
                    double dist = GetLengthMetersC(points[j].Y, points[j].X, lines[sel].Points[i].Y, lines[sel].Points[i].X, false);
                    if (dist < mindist[j])
                    {
                        mindist[j] = dist;
                        nearest[j] = lines[sel].Points[i];
                    };
                };
            };

            string XML = "";
            List<string> styles = new List<string>();
            for (int i = 0; i < points.Length; i++)
            {
                wbf.Show("Get Route: Checked to Line", "Wait, requesting route " + (i + 1).ToString() + "/" + points.Length.ToString());

                string captn = String.Format("{0} - {1}", points[i].Name, lines[sel].Name);
                nmsRouteClient.Route route = groute.GetRoute(points[i].Center, nearest[i]);

                string polys = "";
                string comms = "";

                if (!String.IsNullOrEmpty(route.LastError))
                {
                    polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", points[i].Center.X, points[i].Center.Y, 0);
                    polys += " ";
                    polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", nearest[i].X, nearest[i].Y, 0);
                    comms = String.Format("Length: {0:0.0}\r\nError: {1}", GetLengthMetersC(points[i].Y, points[i].X, nearest[i].Y, nearest[i].X, false) / 1024.0, route.LastError);
                }
                else
                {
                    comms = String.Format(System.Globalization.CultureInfo.InvariantCulture, "Route: {0} - {1}\r\nLength: {2:0.0} km\r\nTime: {3:0.0} min\r\nFrom: {4}\r\nSName: {5}Engine: {6}", points[i].Name, lines[sel].Name, route.driveLength / 1024.0, route.driveTime, groute.ServiceURL, groute.ServiceName, groute.ServiceEngine);
                    for (int j = 0; j < route.polyline.Length; j++)
                    {
                        if (polys.Length > 0) polys += " ";
                        polys += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", route.polyline[j].x, route.polyline[j].y, 0);
                    };
                };

                string stylen = "newstyle" + DateTime.UtcNow.ToString("HHmmssfff");
                styles.Add(stylen);

                XML += "<Placemark>\r\n";
                XML += "<name><![CDATA[" + captn + "]]></name>\r\n";
                XML += "<styleUrl>#" + stylen + "</styleUrl>\r\n";
                XML += "<description><![CDATA[" + comms + "]]></description>\r\n";
                XML += "<LineString>\r\n";
                XML += "<coordinates>" + polys + "</coordinates>\r\n";
                XML += "</LineString>\r\n";
                XML += "</Placemark>\r\n";
            };

            string style = "";
            foreach (string st in styles)
            {
                style += "<Style id=\"" + st + "\"><LineStyle><color>" + AddTextAsPoly.HexStyleConverter(GetRandomColor()) + "</color><width>" + (5).ToString() + "</width></LineStyle>";
                style += "</Style>\r\n";
            }

            wbf.Hide();

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xf = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            xf.InnerXml += XML;
            l.file.kmlDoc.SelectNodes("kml/Document")[0].InnerXml += style;

            l.file.SaveKML();
            l.placemarks += points.Length;
            l.lines += points.Length;
            parent.Refresh();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString(); 
        }

        private int SelectLineDialog(NaviMapNet.MapPolyLine[] lines, string caption)
        {
            int res = 0;
            string[] lns = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++) lns[i] = lines[i].Name;
            if (InputBox.QueryListBox(caption, "Select Line:", lns, ref res) != DialogResult.OK) return -1;
            return res;
        }

        private Random rndc = new Random();
        public Color GetRandomColor()
        {
            Color[] rColors = new Color[] { Color.Fuchsia, Color.OrangeRed, Color.Brown, Color.Coral, Color.DarkCyan, Color.DarkKhaki, Color.DarkOrange, Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkViolet, Color.DeepPink, Color.Firebrick, Color.ForestGreen, Color.IndianRed, Color.Indigo, Color.Lavender, Color.Magenta, Color.Maroon, Color.MediumVioletRed, Color.Navy, Color.PaleVioletRed, Color.Plum, Color.RosyBrown, Color.SlateBlue, Color.Tomato, Color.Violet };
            int col = rndc.Next(rColors.Length - 1);
            return rColors[col];
        }

        private void clearWayOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(mapRVector != null)
                mapRoute.Remove(mapRVector);
            mapRVector = null;
            rbSave.Enabled = false;
            MapViewer.DrawOnMapData();
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rbClear_ButtonClick(sender, e);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string atext = "This is a tool for dkxce Route Engine and OSRM Route Engine\r\n\r\nMore info:\r\n   https://github.com/dkxce/\r\nBy:\r\n   " + fvi.CompanyName;
            MessageBox.Show(atext, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void selectMBTilesFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fName = null;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select MBTiles File";
            ofd.DefaultExt = ".mbtiles";
            ofd.Filter = "All supported files|*.mbtiles;*.sqlite;*.db;*.db3|All Types (*.*)|*.*";
            try { ofd.FileName = UserDefindedFile; } catch { };
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

        private void timeoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int w = groute.timeout;
            if (System.Windows.Forms.InputBox.Show("Web Route Engine", "Select timeout for Route request:", ref w, 10, 180) != DialogResult.OK) return;
            groute.timeout = w;
        }

        private void routeEngineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void rbSet_DropDownOpening(object sender, EventArgs e)
        {            
            setKeyToolStripMenuItem.Enabled = (groute != null) && (groute.service < 0);
            setToolStripMenuItem.Enabled = (groute != null) && (groute.service < 0);
            if (groute == null) return;
            if(groute.service > 0)
                routeEngineToolStripMenuItem.Text = String.Format("{0} - OSRM Engine [{1}]", groute.ServiceIndex, groute.ServiceName);
            else
                routeEngineToolStripMenuItem.Text = String.Format("{0} - dkxce Engine [{1}]", groute.ServiceIndex, groute.ServiceName);
        }

        private void selectRouteServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (groute == null) return;

            int si = 0;
            List<string> svcs = new List<string>();
            List<int> svci = new List<int>();
            if (groute.url_dkxce.Count > 0)
            {
                for (int i = 0; i < groute.url_dkxce.Count; i++)
                {
                    svcs.Add(String.Format("D{0}: {1}", i + 1, groute.url_dkxce[i].name));
                    int sid = -1 * i - 1;
                    svci.Add(sid);
                    if (sid == groute.service) si = svci.Count - 1;
                };
            };
            if (groute.url_osrm.Count > 0)
            {
                for (int i = 0; i < groute.url_osrm.Count; i++)
                {
                    svcs.Add(String.Format("O{0}: {1}", i + 1, groute.url_osrm[i].name));
                    int sid = i + 1;
                    svci.Add(sid);
                    if (sid == groute.service) si = svci.Count - 1;
                };
            };

            if (InputBox.Show("Select Route Engine", "Select Web Route Service (D - dkxce Engine, O - OSRM Engine):", svcs.ToArray(), ref si) != DialogResult.OK) return;
            groute.service = svci[si];
            rbSet.Text = String.Format("Set ({0})", groute.ServiceIndex);
            rbGet.Text = String.Format("Get ({0})", groute.ServiceIndex);
        }

        private void multirouteFromCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.Items.Count == 0) { MessageBox.Show("No layer objects found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            if (objects.CheckedItems.Count == 0) { MessageBox.Show("No checked objects found!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            if (groute == null) { MessageBox.Show("Route layer not initialized!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };
            
            NaviMapNet.MapPoint[] points = GetCheckedPoints();
            if (((points == null ? 0 : points.Length) + (groute.start == null ? 0 : 1) + (groute.finish == null ? 0 : 1)) < 3) { MessageBox.Show("Not enough points for route!", "GetRoute", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); return; };

            List<KeyValuePair<string, PointF>> pArr = new List<KeyValuePair<string, PointF>>();
            if (groute.start != null) pArr.Add(new KeyValuePair<string,PointF>((groute.start[0] == null ? "START POINT" : (string)groute.start[0]), new PointF((float)groute.start[1], (float)groute.start[2])));
            foreach (NaviMapNet.MapPoint p in points) pArr.Add(new KeyValuePair<string,PointF>(p.Name, p.Center));
            if (groute.finish != null) pArr.Add(new KeyValuePair<string, PointF>((groute.finish[0] == null ? "FINISH POINT" : (string)groute.finish[0]), new PointF((float)groute.finish[1], (float)groute.finish[2])));

            MultiPointFormShow(pArr);
        }

        private void fromStartToFinishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rbSave.Enabled = GetRoute();
        }

        private void saveRouteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!rbSave.Enabled) return;
            SaveResult(mapRVector, false);
        }

        private void saveRouteAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!rbSave.Enabled) return;
            SaveResult(mapRVector, true);
        }

        private void rbMi_Click(object sender, EventArgs e)
        {
            groute.mode = 4;
            rbDN.Checked = false;
            rbStFi.Checked = false;
            rbSt.Checked = false;
            rbFi.Checked = false;
            rbMi.Checked = true;
            oncb.Text = rbMi.Text;
        }

        private ListViewItem objects_temp;
        
        private void objects_MouseDown(object sender, MouseEventArgs e)
        {
            if (objects.Items.Count == 0) return;

            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {                
                objects_temp = GetItemFromPoint(objects, Cursor.Position);
                objects.MovingItem = objects_temp;
                if (objects_temp != null) objects.Cursor = Cursors.Hand;                
            };
        }

        private void objects_MouseUp(object sender, MouseEventArgs e)
        {
            if (objects.Items.Count == 0) return;
            if (objects_temp != null)
            {                
                ListViewItem toItem = GetItemFromPoint(objects, Cursor.Position);
                if ((toItem != null) && (objects_temp.Index != toItem.Index))
                {
                    int fi = objects_temp.Index;
                    int ti = toItem.Index;
                    objects.Items.RemoveAt(fi);
                    objects.Items.Insert(ti, objects_temp);
                    ReorderLayer(fi, ti, false);
                    mapSelect.Clear();
                };
                objects.Cursor = Cursors.Default;
                objects_temp = null;
                objects.MovingItem = null;
            };
        }

        private ListViewItem GetItemFromPoint(ListView listView, Point mousePosition)
        {
            Point localPoint = listView.PointToClient(mousePosition);
            return listView.GetItemAt(localPoint.X, localPoint.Y);
        }

        private void ReorderLayer(int fi, int ti, bool reload)
        {
            if (this.objects.Items.Count == 0) return;
            
            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            List<KeyValuePair<XmlNode, double>> placemarks = new List<KeyValuePair<XmlNode, double>>();
            XmlNodeList nl = x_folder.SelectNodes("Placemark");
            foreach (XmlNode n in nl)
            {
                placemarks.Add(new KeyValuePair<XmlNode, double>(n, 0));
                x_folder.RemoveChild(n);
            };

            KeyValuePair<XmlNode, double> kvt = placemarks[fi];
            placemarks.RemoveAt(fi);
            placemarks.Insert(ti, kvt);
            
            foreach (KeyValuePair<XmlNode, double> n in placemarks)
                x_folder.AppendChild(n.Key);

            l.file.SaveKML();

            if (reload)
            {
                objects.Items.Clear();
                laySelect.Items[laySelect.SelectedIndex] = l.ToString();
                parent.Refresh();
            }
            else
            {
                int el_list = 0;                
                int el_line = 0;
                int el_polygon = 0;
                int el_point = 0;
                XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                XmlNodeList xnf = xn.SelectNodes("Placemark");
                if (xnf.Count > 0)
                    for (int el = 0; el < xnf.Count; el++)
                    {
                        if (el_list >= objects.Items.Count) break;
                        if (xnf[el].ChildNodes.Count == 0) continue;

                        if (xnf[el].SelectNodes("LineString").Count > 0)
                            objects.Items[el_list++].SubItems[7].Text = ("Placemark/LineString/coordinates[" + (el_line++).ToString() + "]");
						if (xnf[el].SelectNodes("Polygon").Count > 0)
                            objects.Items[el_list++].SubItems[7].Text = ("Placemark/Polygon/outerBoundaryIs/LinearRing/coordinates[" + (el_polygon++).ToString() + "]");
                        if (xnf[el].SelectNodes("Point").Count > 0)
                            objects.Items[el_list++].SubItems[7].Text = ("Placemark/Point/coordinates[" + (el_point++).ToString() + "]");
                    };

                NaviMapNet.MapObject tempo = mapContent[fi];
                mapContent.Remove(fi);
                mapContent.Add(tempo, ti);
                MapViewer.DrawOnMapData();
            };
        }

        private void sortByRouteLengthToThisPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            if (objects.Items.Count < 2) return;
            if (!(mapContent[objects.SelectedIndices[0]] is NaviMapNet.MapPoint)) return;

            if (groute == null) groute = GetRouter.Load();
            wbf.Show("Sorting by route", "Wait, loading...");

            NaviMapNet.MapPoint mp = (NaviMapNet.MapPoint)mapContent[objects.SelectedIndices[0]];

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode x_folder = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            List<KeyValuePair<XmlNode, double>> placemarks = new List<KeyValuePair<XmlNode, double>>();
            XmlNodeList nl = x_folder.SelectNodes("Placemark");
            int nrc = 0;
            foreach (XmlNode n in nl)
            {
                wbf.Show("Sorting by route", String.Format("Wait, getting route {0}/{1}...", ++nrc, nl.Count));
                double dist = double.MaxValue;
                XmlNode xn = n.SelectNodes("*/coordinates")[0];
                string[] xy = xn.ChildNodes[0].Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ln in xy)
                {
                    try
                    {
                        string[] xyz = ln.Split(new char[] { ',' }, StringSplitOptions.None);
                        PointF cp = new PointF((float)double.Parse(xyz[0], System.Globalization.CultureInfo.InvariantCulture), (float)double.Parse(xyz[1], System.Globalization.CultureInfo.InvariantCulture));
                        nmsRouteClient.Route route = groute.GetRoute(mp.Points[0], cp);
                        if (route.driveLength < dist) dist = route.driveLength;                        
                    }
                    catch { };
                };
                placemarks.Add(new KeyValuePair<XmlNode, double>(n, dist));
                x_folder.RemoveChild(n);
            };
            wbf.Show("Sorting by route", "Wait, sorting...");
            placemarks.Sort(PlaceMarkAtFolderSorter.ByDistance);
            foreach (KeyValuePair<XmlNode, double> n in placemarks)
                x_folder.AppendChild(n.Key);
            wbf.Hide();

            l.file.SaveKML();
            objects.Items.Clear();
            laySelect.Items[laySelect.SelectedIndex] = l.ToString();
            parent.Refresh();
        }

        private void osmf_Click(object sender, EventArgs e)
        {
            string text = textBox2.Text.Trim();
            if (String.IsNullOrEmpty(text)) return;
            {
                wbf.Show("OSM Search", "Wait, getting OSM data...");
                GEOOSMJSON result = null;
                try
                {
                    // &bbox=33.75%2C52.221069523572794%2C48.768310546875%2C55.986091533808384
                    string bbox = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}%2C{1}%2C{2}%2C{3}", MapViewer.MapBoundsRectOversizeDegrees.Left, MapViewer.MapBoundsRectOversizeDegrees.Bottom, MapViewer.MapBoundsRectOversizeDegrees.Right, MapViewer.MapBoundsRectOversizeDegrees.Top);
                    string slat = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", MapViewer.CenterDegreesLat);
                    string slon = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", MapViewer.CenterDegreesLon);
                    System.Net.HttpWebRequest wq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(@"http://openstreetmap.ru/api/search?q=" + System.Security.SecurityElement.Escape(text) + "&bbox=" + bbox + "&lat=" + slat + "&lon=" + slon);
                    System.Net.HttpWebResponse wr = (System.Net.HttpWebResponse)wq.GetResponse();
                    StreamReader sr = new StreamReader(wr.GetResponseStream(), System.Text.Encoding.ASCII);
                    string response = sr.ReadToEnd();
                    result = (GEOOSMJSON)Newtonsoft.Json.JsonConvert.DeserializeObject(response, typeof(GEOOSMJSON));
                    sr.Close();
                    wr.Close();                    
                }
                catch (Exception ex)
                {
                    wbf.Hide();
                    MessageBox.Show(ex.Message, "Ãåîïîèñê OSM", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                };                
                if ((result == null) || (result.matches == null) || (result.matches.Length == 0))
                {
                    wbf.Hide();
                    MessageBox.Show("Íè÷åãî íå íàéäåíî", "Ãåîïîèñê OSM", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                };
                result.Sort(MapViewer.CenterDegrees);
                wbf.Hide();
                
                PointF mapCWas = MapViewer.CenterDegrees;
                byte mapZWas = MapViewer.ZoomID;
                MultiPointRouteForm mprf = new MultiPointRouteForm();
                mprf.SetListOfSearchResults();
                mprf.StartPosition = FormStartPosition.Manual;
                mprf.Left = this.Left + this.Width - mprf.Width;
                if (mprf.Left < 0) mprf.Left = this.Left;
                mprf.Top = this.Top + panel1.Height;
                mprf.TopMost = true;
                mprf.buttonOk.Click += (delegate(object subsender, EventArgs sube) { mprf.Close(); });
                mprf.buttonCancel.Click += (delegate(object subsender, EventArgs sube) { mprf.Close(); MapViewer.CenterDegrees = mapCWas; MapViewer.ZoomID = mapZWas; });
                mprf.onItemDoubleClick += (delegate(object subsender, MouseEventArgs sube) { if (mprf.SelectedIndex >= 0) MapViewer.CenterDegrees = mprf.OnMapPoints[mprf.SelectedIndex].Center; });
                mprf.onSaveOnMap += (delegate(object subsender, EventArgs sube)
                {
                    if (mprf.SelectedIndex >= 0)
                    {
                        mprf.TopMost = false;
                        try
                        {
                            bool svd = addNewPointByNXY(result.matches[mprf.SelectedIndex].name, result.matches[mprf.SelectedIndex].display_name, mprf.OnMapPoints[mprf.SelectedIndex].Center);
                            if (svd) mprf.buttonCancel.Visible = false;
                        }
                        catch { };
                        mprf.TopMost = true;
                    };
                });
                mprf.FormClosed += (delegate(object subsender, FormClosedEventArgs sube) 
                {
                    for (int i = 0; i < mprf.OnMapPoints.Count; i++) try { mapRoute.Remove(mprf.OnMapPoints[i]); } catch { };
                    try { MapViewer.DrawOnMapData(); MapViewer.Focus(); MapViewer.Select(); }catch { };
                    mprf.Dispose();
                    osmf.Enabled = true;
                });                                
                PointF center = new PointF();                
                for (int i = 0; i < result.matches.Length; i++)
                {
                    string nm = result.matches[i].display_name;
                    PointD pd = new PointD(result.matches[i].lon, result.matches[i].lat);
                    if (i == 0) center = pd.PointF;
                    NaviMapNet.MapPoint mapRPoint = new NaviMapNet.MapPoint();
                    mapRPoint.Squared = true;
                    mapRPoint.SizePixels = new Size(16, 16);
                    mapRPoint.Img = GetRouter.ImageFromNumber(i + 1);
                    mapRPoint.Points = new PointF[] { pd.PointF };
                    mapRPoint.Text = nm;
                    mapRoute.Add(mapRPoint);
                    mprf.AddPoint(new KeyValuePair<string, PointF>(nm, pd.PointF), mapRPoint);
                };
                MapViewer.DrawOnMapData();
                mprf.Show();
                osmf.Enabled = false;                
            };
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyValue == 13) && (osmf.Enabled)) osmf_Click(sender, e);
        }
    }

    public class ToolStripSpringTextBox : ToolStripTextBox
    {
        public bool Spring = true;

        public override Size GetPreferredSize(Size constrainingSize)
        {
            if (!Spring)
                return base.GetPreferredSize(constrainingSize);                
            
            // Use the default size if the text box is on the overflow menu
            // or is on a vertical ToolStrip.
            if (IsOnOverflow || Owner.Orientation == Orientation.Vertical)
            {
                return DefaultSize;
            }

            // Declare a variable to store the total available width as 
            // it is calculated, starting with the display width of the 
            // owning ToolStrip.
            Int32 width = Owner.DisplayRectangle.Width;

            // Subtract the width of the overflow button if it is displayed. 
            if (Owner.OverflowButton.Visible)
            {
                width = width - Owner.OverflowButton.Width -
                    Owner.OverflowButton.Margin.Horizontal;
            }

            // Declare a variable to maintain a count of ToolStripSpringTextBox 
            // items currently displayed in the owning ToolStrip. 
            Int32 springBoxCount = 0;

            foreach (ToolStripItem item in Owner.Items)
            {
                // Ignore items on the overflow menu.
                if (item.IsOnOverflow) continue;

                if (item is ToolStripSpringTextBox)
                {
                    // For ToolStripSpringTextBox items, increment the count and 
                    // subtract the margin width from the total available width.
                    springBoxCount++;
                    width -= item.Margin.Horizontal;
                }
                else
                {
                    // For all other items, subtract the full width from the total
                    // available width.
                    width = width - item.Width - item.Margin.Horizontal;
                }
            }

            // If there are multiple ToolStripSpringTextBox items in the owning
            // ToolStrip, divide the total available width between them. 
            if (springBoxCount > 1) width /= springBoxCount;

            // If the available width is less than the default width, use the
            // default width, forcing one or more items onto the overflow menu.
            if (width < DefaultSize.Width) width = DefaultSize.Width;

            // Retrieve the preferred size from the base class, but change the
            // width to the calculated width. 
            Size size = base.GetPreferredSize(constrainingSize);
            size.Width = width;
            return size;
        }
    }

    public class ObjectsListView : ListView
    {
        private ListViewItem prItem;
        public ListViewItem MovingItem = null;

        public ObjectsListView()
        {
            this.OwnerDraw = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
            this.MouseMove += new MouseEventHandler(ObjectsListView_MouseMove);
            this.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(ObjectsListView_DrawColumnHeader);
        }

        private void ObjectsListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void ObjectsListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.Items.Count <= 0) return;            
            if (prItem != null) this.RedrawItems(prItem.Index, prItem.Index, true);

            prItem = null;
            if (this.MovingItem == null) return;

            prItem = this.GetItemAt(e.X, e.Y);            
            if (prItem != null) this.RedrawItems(prItem.Index, prItem.Index, true);                            
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawSubItem(e);
            if ((e.ColumnIndex > 0) || (MovingItem == null) || (MovingItem.Index == e.ItemIndex) || (prItem == null) || (prItem.Index != e.ItemIndex))
            {
                
            }
            else
            {
                e.DrawDefault = false;
                int offset = 19;
                string name = MovingItem.SubItems[0].Text;
                e.Graphics.FillRectangle(Brushes.Fuchsia, e.Bounds);
                e.Graphics.DrawString(name, e.Item.Font, Brushes.White, new Rectangle(offset, e.Bounds.Top + 2, e.Bounds.Width - offset, e.Bounds.Height), StringFormat.GenericDefault);
            };
        }
    }

    [Serializable]
    public class GetRouter
    {       
        public int mode = 0;
        public int service = -1; // < 0 - dkxce; > 0 - OSRM      
        public bool getroute = false;
        public bool saveroute = false;
        public int routecolor { get { return color.ToArgb(); } set { color = Color.FromArgb(value); } }
        public int width = 5;
        public int timeout = 30;

        [XmlIgnore]
        public object[] start = null;
        [XmlIgnore]
        public object[] finish = null;
        [XmlIgnore]
        public ulong counter = 0;
        [XmlIgnore]
        public Color color = Color.OrangeRed;    

        [XmlArray(ElementName = "dkxce.Route.Service")]
        [XmlArrayItem(ElementName = "url")]
        public List<DRSParams> url_dkxce = new List<DRSParams>();
        
        [XmlArray(ElementName = "OSRMaps.Route")]
        [XmlArrayItem(ElementName = "url")] // http://project-osrm.org/docs/v5.24.0/api/#
        public List<OSRMParams> url_osrm = new List<OSRMParams>();                
        
        [XmlIgnore]
        public string ServiceURL 
        {
            get { if (service >= 0) return url_osrm[service - 1].url; else return url_dkxce[-1 * service - 1].url; }
            set { if (service >= 0) url_osrm[service - 1].url = value; else url_dkxce[-1 * service - 1].url = value; }
        }
        [XmlIgnore]
        public string ServiceName
        {
            get { if (service >= 0) return url_osrm[service - 1].name; else return url_dkxce[-1 * service - 1].name; }
            set { if (service >= 0) url_osrm[service - 1].name = value; else url_dkxce[-1 * service - 1].name = value; }
        }
        [XmlIgnore]
        public string ServiceIndex
        {
            get { if (service >= 0) return String.Format("O{0}", service); else return String.Format("D{0}", -1 * service ); }
        }
        [XmlIgnore]
        public string ServiceEngine { get { if (service > 0) return "OSRM"; else return "dkxce"; } }

        public double GetRoute(PointF[] points, WaitingBoxForm wbf, out PointF[] vector, out nmsRouteClient.Route route)
        {
            vector = null;
            route = null;
            if (service >= 0) return GetRouteOSRM(points, wbf, out vector, out route, url_osrm[service - 1]);
            return GetRouteDKXCE(points, wbf, out vector, out route, url_dkxce[-1 * service - 1]);
            return 0;
        }

        public nmsRouteClient.Route GetRoute(PointF a, PointF b)
        {
            if (service >= 0) return GetRouteOSRM(a, b, url_osrm[service - 1]);
            return GetRouteDKXCE(a, b, url_dkxce[- 1 * service - 1]);
        }

        public nmsRouteClient.Route GetRouteDKXCE(PointF a, PointF b, DRSParams param)
        {
            nmsRouteClient.Route res = new nmsRouteClient.Route();
            res.LastError = "Couldn't request route";

            string furl = param.url;
            {
                int iu = furl.ToUpper().IndexOf("/NMS");
                if (iu > 0) furl = furl.Substring(0, iu + 4) + "/";
                string xx = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", a.X, b.X);
                string yy = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", a.Y, b.Y);
                furl += String.Format("route?k={0}&f=2&p=1&i=0&minby=time&x={1}&y={2}&ra={3}&n=start,dest", param.key.Trim(), xx, yy, param.ra.Trim());
            };

            try
            {
                System.Net.HttpWebRequest wReq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(furl);
                wReq.Timeout = this.timeout * 1000;
                System.Net.HttpWebResponse wRes = (System.Net.HttpWebResponse)wReq.GetResponse();
                StreamReader sr = new StreamReader(wRes.GetResponseStream());
                string xml = sr.ReadToEnd();
                sr.Close();
                wRes.Close();

                if (String.IsNullOrEmpty(xml)) { res.LastError = "No valid route XML"; return res; };

                return nmsRouteClient.RouteClient.XMLToObject(xml);                
            }
            catch (Exception ex)
            {
                res.LastError = ex.Message;
                return res;
            };
        }

        public nmsRouteClient.Route GetRouteOSRM(PointF a, PointF b, OSRMParams param)
        {
            nmsRouteClient.Route route = new nmsRouteClient.Route();
            route.LastError = "Couldn't request route";

            string furl = param.url;
            {
                int iu = furl.LastIndexOf("/");
                if (iu > 0) furl = furl.Substring(0, iu + 1);
                string xyxy = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1};{2},{3}", a.X, a.Y, b.X, b.Y);
                furl += String.Format("{0}?overview=full&geometries=polyline", xyxy);
            };
            
            try
            {
                System.Net.HttpWebRequest wReq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(furl);
                wReq.Timeout = this.timeout * 1000;
                System.Net.HttpWebResponse wRes = (System.Net.HttpWebResponse)wReq.GetResponse();
                StreamReader sr = new StreamReader(wRes.GetResponseStream());
                string json = sr.ReadToEnd();
                sr.Close();
                wRes.Close();

                if (String.IsNullOrEmpty(json)) throw new Exception("No valid route JSON");

                OSMRResponse osmr = OSMRResponse.FromText(json);
                if ((!String.IsNullOrEmpty(osmr.code)) && (osmr.code.ToLower() != "ok")) throw new Exception(osmr.code);
                if ((osmr.routes == null) || (osmr.routes.Length == 0)) { route.LastError = osmr.code; return route; };

                PointF[] vector = osmr.routes[0].points;
                route = new nmsRouteClient.Route();
                route.driveLength = osmr.routes[0].distance;
                route.driveTime = osmr.routes[0].duration / 60.0;
                route.polyline = new nmsRouteClient.XYPoint[vector.Length];
                for (int i = 0; i < vector.Length; i++)
                    route.polyline[i] = new nmsRouteClient.XYPoint(vector[i].X, vector[i].Y);
            }
            catch (Exception ex)
            {
                route.LastError = ex.Message;
                return route;
            };
            return route;
        }

        public double GetRoute(PointF a, PointF b, WaitingBoxForm wbf, out PointF[] vector, out nmsRouteClient.Route route)
        {
            if (service >= 0) return GetRouteOSRM(a, b, wbf, out vector, out route, url_osrm[service - 1]);
            return GetRouteDKXCE(a, b, wbf, out vector, out route, url_dkxce[- 1 * service - 1]);
        }

        public double GetRouteDKXCE(PointF a, PointF b, WaitingBoxForm wbf, out PointF[] vector, out nmsRouteClient.Route route, DRSParams param)
        {
            vector = null;
            route = null;
            double res = double.MaxValue;

            string furl = param.url;
            {
                int iu = furl.ToUpper().IndexOf("/NMS");
                if (iu > 0) furl = furl.Substring(0, iu + 4) + "/";
                string xx = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", a.X, b.X);
                string yy = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", a.Y, b.Y);
                furl += String.Format("route?k={0}&f=2&p=1&i=0&minby=time&x={1}&y={2}&ra={3}&n=start,dest", param.key.Trim(), xx, yy, param.ra.Trim());
            };
            wbf.Show("Request route", param.url);
            try
            {
                System.Net.HttpWebRequest wReq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(furl);
                wReq.Timeout = this.timeout * 1000;
                System.Net.HttpWebResponse wRes = (System.Net.HttpWebResponse)wReq.GetResponse();
                StreamReader sr = new StreamReader(wRes.GetResponseStream());
                string xml = sr.ReadToEnd();
                sr.Close();
                wRes.Close();

                if (String.IsNullOrEmpty(xml)) throw new Exception("No valid route XML");

                route = nmsRouteClient.RouteClient.XMLToObject(xml);
                if (!String.IsNullOrEmpty(route.LastError)) throw new Exception(route.LastError);
                res = route.driveLength;
                vector = new PointF[route.polyline.Length];
                for (int i = 0; i < vector.Length; i++)
                    vector[i] = new PointF((float)route.polyline[i].x, (float)route.polyline[i].y);
            }
            catch (Exception ex)
            {
                wbf.Hide();
                MessageBox.Show("Get route failed\r\nError: " + ex.Message, "Get Route", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return double.MaxValue;
            };
            wbf.Hide();
            return res;
        }

        public double GetRouteDKXCE(PointF[] points, WaitingBoxForm wbf, out PointF[] vector, out nmsRouteClient.Route route, DRSParams param)
        {
            vector = null;
            route = null;
            double res = double.MaxValue;
            if ((points == null) || (points.Length < 2)) return res;

            string furl = param.url;
            {
                int iu = furl.ToUpper().IndexOf("/NMS");
                if (iu > 0) furl = furl.Substring(0, iu + 4) + "/";
                string xx = "";
                string yy = "";
                for (int i = 0; i < points.Length; i++)
                {
                    if (xx.Length > 0) xx += ",";
                    if (yy.Length > 0) yy += ",";
                    xx += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", points[i].X);
                    yy += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", points[i].Y);
                };
                furl += String.Format("route?k={0}&f=2&p=1&i=0&minby=time&x={1}&y={2}&ra={3}&n=start,dest", param.key.Trim(), xx, yy, param.ra.Trim());
            };
            wbf.Show("Request route", param.url);
            try
            {
                System.Net.HttpWebRequest wReq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(furl);
                wReq.Timeout = this.timeout * 1000;
                System.Net.HttpWebResponse wRes = (System.Net.HttpWebResponse)wReq.GetResponse();
                StreamReader sr = new StreamReader(wRes.GetResponseStream());
                string xml = sr.ReadToEnd();
                sr.Close();
                wRes.Close();

                if (String.IsNullOrEmpty(xml)) throw new Exception("No valid route XML");

                route = nmsRouteClient.RouteClient.XMLToObject(xml);
                if (!String.IsNullOrEmpty(route.LastError)) throw new Exception(route.LastError);
                res = route.driveLength;
                vector = new PointF[route.polyline.Length];
                for (int i = 0; i < vector.Length; i++)
                    vector[i] = new PointF((float)route.polyline[i].x, (float)route.polyline[i].y);
            }
            catch (Exception ex)
            {
                wbf.Hide();
                MessageBox.Show("Get route failed\r\nError: " + ex.Message, "Get Route", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return double.MaxValue;
            };
            wbf.Hide();
            return res;
        }

        public double GetRouteOSRM(PointF a, PointF b, WaitingBoxForm wbf, out PointF[] vector, out nmsRouteClient.Route route, OSRMParams param)
        {
            vector = null;
            route = null;
            double res = double.MaxValue;

            string furl = param.url;
            {
                int iu = furl.LastIndexOf("/");
                if (iu > 0) furl = furl.Substring(0, iu + 1);
                string xyxy = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1};{2},{3}", a.X, a.Y, b.X, b.Y);
                furl += String.Format("{0}?overview=full&geometries=polyline", xyxy);
            };
            wbf.Show("Request route", param.url);
            try
            {
                System.Net.HttpWebRequest wReq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(furl);
                wReq.Timeout = this.timeout * 1000;
                System.Net.HttpWebResponse wRes = (System.Net.HttpWebResponse)wReq.GetResponse();
                StreamReader sr = new StreamReader(wRes.GetResponseStream());
                string json = sr.ReadToEnd();
                sr.Close();
                wRes.Close();

                if (String.IsNullOrEmpty(json)) throw new Exception("No valid route JSON");
                
                OSMRResponse osmr = OSMRResponse.FromText(json);
                if ((!String.IsNullOrEmpty(osmr.code)) && (osmr.code.ToLower() != "ok")) throw new Exception(osmr.code);
                if ((osmr.routes == null) || (osmr.routes.Length == 0)) return double.MaxValue;
                res = osmr.routes[0].distance;
                vector = osmr.routes[0].points;

                route = new nmsRouteClient.Route();
                route.driveLength = osmr.routes[0].distance;
                route.driveTime = osmr.routes[0].duration / 60.0;
                route.polyline = new nmsRouteClient.XYPoint[vector.Length];
                for (int i = 0; i < vector.Length; i++)
                    route.polyline[i] = new nmsRouteClient.XYPoint(vector[i].X, vector[i].Y);
            }
            catch (Exception ex)
            {
                wbf.Hide();
                MessageBox.Show("Get route failed\r\nError: " + ex.Message, "Get Route", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return double.MaxValue;
            };
            wbf.Hide();
            return res;
        }

        public double GetRouteOSRM(PointF[] points, WaitingBoxForm wbf, out PointF[] vector, out nmsRouteClient.Route route, OSRMParams param)
        {
            vector = null;
            route = null;
            double res = double.MaxValue;

            if ((points == null) || (points.Length < 2)) return res;

            string furl = param.url;
            {
                int iu = furl.LastIndexOf("/");
                if (iu > 0) furl = furl.Substring(0, iu + 1);
                string xyxy = "";
                for (int i = 0; i < points.Length; i++)
                {
                    if (xyxy.Length > 0) xyxy += ";";
                    xyxy += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", points[i].X, points[i].Y);
                };
                furl += String.Format("{0}?overview=full&geometries=polyline", xyxy);
            };
            wbf.Show("Request route", param.url);
            try
            {
                System.Net.HttpWebRequest wReq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(furl);
                wReq.Timeout = this.timeout * 1000;
                System.Net.HttpWebResponse wRes = (System.Net.HttpWebResponse)wReq.GetResponse();
                StreamReader sr = new StreamReader(wRes.GetResponseStream());
                string json = sr.ReadToEnd();
                sr.Close();
                wRes.Close();

                if (String.IsNullOrEmpty(json)) throw new Exception("No valid route JSON");

                OSMRResponse osmr = OSMRResponse.FromText(json);
                if ((!String.IsNullOrEmpty(osmr.code)) && (osmr.code.ToLower() != "ok")) throw new Exception(osmr.code);
                if ((osmr.routes == null) || (osmr.routes.Length == 0)) return double.MaxValue;
                res = osmr.routes[0].distance;
                vector = osmr.routes[0].points;

                route = new nmsRouteClient.Route();
                route.driveLength = osmr.routes[0].distance;
                route.driveTime = osmr.routes[0].duration / 60.0;
                route.polyline = new nmsRouteClient.XYPoint[vector.Length];
                for (int i = 0; i < vector.Length; i++)
                    route.polyline[i] = new nmsRouteClient.XYPoint(vector[i].X, vector[i].Y);
            }
            catch (Exception ex)
            {
                wbf.Hide();
                MessageBox.Show("Get route failed\r\nError: " + ex.Message, "Get Route", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return double.MaxValue;
            };
            wbf.Hide();
            return res;
        }

        public static GetRouter Load()
        {
            string fName = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.rtc";
            if (File.Exists(fName))
            {
                try
                {
                    GetRouter res = XMLSaved<GetRouter>.Load(fName);
                    if (res.service == 0) res.service = -1;
                    if (res.service > 0)
                    {
                        int ind = res.service - 1;
                        if (ind >= res.url_osrm.Count) res.service = -1;
                    };
                    if (res.service < 0)
                    {
                        int ind = res.service * -1 - 1;
                        if (ind >= res.url_dkxce.Count) res.service = -1;
                    };
                    if((res.url_dkxce == null) || (res.url_dkxce.Count == 0)) res.url_dkxce = new List<DRSParams>(new DRSParams[] { new DRSParams() });
                    if ((res.url_osrm == null) || (res.url_osrm.Count == 0)) res.url_osrm = new List<OSRMParams>(new OSRMParams[] { new OSRMParams("map.project-osrm.org", "http://router.project-osrm.org/route/v1/driving/"), new OSRMParams("maps.openrouteservice.org", "http://routing.openstreetmap.de/routed-car/route/v1/driving/") });
                    return res;
                }
                catch { };
            };
            {
                GetRouter res = new GetRouter();
                res.url_dkxce = new List<DRSParams>(new DRSParams[] { new DRSParams() });
                res.url_osrm = new List<OSRMParams>(new OSRMParams[] { new OSRMParams("map.project-osrm.org", "http://router.project-osrm.org/route/v1/driving/"), new OSRMParams("maps.openrouteservice.org", "http://routing.openstreetmap.de/routed-car/route/v1/driving/") });
                return res;
            };
        }

        public void Save()
        {
            try
            {
                string fName = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.rtc";
                XMLSaved<GetRouter>.Save(fName, this);
            }
            catch { };
        }

        public class DRSParams
        {
            [XmlText]
            public string url = "http://localhost:8080/nms/";
            [XmlAttribute]
            public string key = "TEST";
            [XmlAttribute]
            public string ra = "00000000000000000000000000000000";
            [XmlAttribute]
            public string name = "localhost:8080";
        }

        public class OSRMParams
        {
            [XmlText]
            public string url;
            [XmlAttribute]
            public string name;
            public OSRMParams() { }
            public OSRMParams(string name, string url) { this.name = name; this.url = url; }
        }

        public class OSMRResponse
        {
            public class OSMRRoute
            {
                public string geometry;
                public string weight_name;
                public double weight;
                public double distance;
                public double duration;
                public PointF[] points { get { return DecodeA(this.geometry); } }

                private static IEnumerable<PointF> Decode(string polylineString)
                {
                    char[] polylineChars = polylineString.ToCharArray();
                    int index = 0;

                    double currentLat = 0;
                    double currentLng = 0;

                    while (index < polylineChars.Length)
                    {
                        // Next lat
                        int sum = 0;
                        int shifter = 0;
                        int nextFiveBits;
                        do
                        {
                            nextFiveBits = polylineChars[index++] - 63;
                            sum |= (nextFiveBits & 31) << shifter;
                            shifter += 5;
                        } while (nextFiveBits >= 32 && index < polylineChars.Length);

                        if (index >= polylineChars.Length)
                            break;

                        currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                        // Next lng
                        sum = 0;
                        shifter = 0;
                        do
                        {
                            nextFiveBits = polylineChars[index++] - 63;
                            sum |= (nextFiveBits & 31) << shifter;
                            shifter += 5;
                        } while (nextFiveBits >= 32 && index < polylineChars.Length);

                        if (index >= polylineChars.Length && nextFiveBits >= 32)
                            break;

                        currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                        yield return new PointF((float)(Convert.ToDouble(currentLng) / 1.0E+5), (float)(Convert.ToDouble(currentLat) / 1.0E+5));
                    };
                }

                private static PointF[] DecodeA(string polylineString)
                {
                    List<PointF> res = new List<PointF>();
                    foreach (PointF pnt in Decode(polylineString)) res.Add(pnt);
                    return res.ToArray();
                }
            }
            
            public string code;
            public OSMRRoute[] routes;

            public static OSMRResponse FromText(string text)
            {
                OSMRResponse result = new OSMRResponse();
                List<OSMRRoute> resrts = new List<OSMRRoute>();

                Newtonsoft.Json.Linq.JToken osmd = (Newtonsoft.Json.Linq.JContainer)Newtonsoft.Json.JsonConvert.DeserializeObject(text);
                foreach (Newtonsoft.Json.Linq.JProperty suntoken in osmd)
                {
                    if (suntoken.Name == "code") result.code = suntoken.Value.ToString();
                    if (suntoken.Name == "routes")
                    {
                        foreach (Newtonsoft.Json.Linq.JObject rt in suntoken.Value)
                        {
                            OSMRRoute rres = new OSMRRoute();
                            foreach (Newtonsoft.Json.Linq.JProperty trp in (Newtonsoft.Json.Linq.JContainer)rt)
                            {
                                if (trp.Name == "geometry") rres.geometry = trp.Value.ToString();
                                if (trp.Name == "weight_name") rres.weight_name = trp.Value.ToString();
                                if (trp.Name == "weight") double.TryParse(trp.Value.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out rres.weight);
                                if (trp.Name == "distance") double.TryParse(trp.Value.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out rres.distance);
                                if (trp.Name == "duration") double.TryParse(trp.Value.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out rres.duration);
                            };
                            resrts.Add(rres);
                        };
                    };                    
                };
                result.routes = resrts.ToArray();
                return result;
            }
        }

        public static Image ImageFromNumber(int num)
        {
            Bitmap im = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(im);          
            g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, 16, 16));
            g.DrawString(String.Format("{0:00}",num), new Font("Tahoma", 8, FontStyle.Regular), new SolidBrush(Color.White), new PointF(0, 1));
            g.Dispose();
            return im;
        }
    }

    public class GEOOSMJSON
    {
        public GEOMATCH[] matches;
        public string search;
        public string ver;
        public bool find;

        public class GEOMATCH
        {
            public string display_name;
            public string addr_type;
            public string name;
            public string weight;
            public double lon;
            public double lat;
        }

        public void Sort(PointF center)
        {
            if (matches == null) return;
            if (matches.Length == 0) return;
            Array.Sort(matches, new MComparer(center));
        }

        private class MComparer: IComparer<GEOMATCH>
        {
            private PointF center;

            public MComparer(PointF center)
            {
                this.center = center;
            }
            public int Compare(GEOMATCH a, GEOMATCH b)
            {
                double la = GetLengthMetersC(center.Y, center.X, a.lat, a.lon, false);
                double lb = GetLengthMetersC(center.Y, center.X, b.lat, b.lon, false);
                return la.CompareTo(lb);
            }
            private double GetLengthMetersC(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
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

                return Math.Round(dDistance);
            }
        }
    }

    [Serializable]
    public class XMLSaved<T>
    {
        public static string ToUpper(string str)
        {
            if (String.IsNullOrEmpty(str)) return "";
            return str.ToUpper();
        }

        /// <summary>
        ///     Ñîõðàíåíèå ñòðóêòóðû â ôàéë
        /// </summary>
        /// <param name="file">Ïîëíûé ïóòü ê ôàéëó</param>
        /// <param name="obj">Ñòðóêòóðà</param>
        public static void Save(string file, T obj)
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
            System.IO.StreamWriter writer = System.IO.File.CreateText(file);
            xs.Serialize(writer, obj);
            writer.Flush();
            writer.Close();
        }

        public static string Save(T obj)
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
            System.IO.MemoryStream ms = new MemoryStream();
            System.IO.StreamWriter writer = new StreamWriter(ms);
            xs.Serialize(writer, obj);
            writer.Flush();
            ms.Position = 0;
            byte[] bb = new byte[ms.Length];
            ms.Read(bb, 0, bb.Length);
            writer.Close();
            return System.Text.Encoding.UTF8.GetString(bb); ;
        }

        /// <summary>
        ///     Ïîäêëþ÷åíèå ñòðóêòóðû èç ôàéëà
        /// </summary>
        /// <param name="file">Ïîëíûé ïóòü ê ôàéëó</param>
        /// <returns>Ñòðóêòóðà</returns>
        public static T Load(string file)
        {
            // if couldn't create file in temp - add credintals
            // http://support.microsoft.com/kb/908158/ru
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
            System.IO.StreamReader reader = System.IO.File.OpenText(file);
            T c = (T)xs.Deserialize(reader);
            reader.Close();
            return c;
        }
    }

    [Serializable]
    public class State : XMLSaved<State>
    {
        public string SASCacheDir = @"C:\Program Files\SASPlanet\cache";
        public int MapID = -1;
        public string SASDir = null;
        public string URL = null;
        public string FILE = null;
        public double X;
        public double Y;
        public byte Z;
    }
}