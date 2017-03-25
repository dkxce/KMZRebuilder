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
    public partial class ContentViewer : Form
    {
        private WaitingBoxForm wbf = null;
        public NaviMapNet.MapLayer mapContent = null;
        public NaviMapNet.MapLayer mapSelect = null;
        public ToolTip mapTootTip = new ToolTip();
        private KMZRebuilederForm parent = null;
        private bool firstboot = true;

        private string SASPlanetCacheDir = @"C:\Program Files\SASPlanet\cache\osmmapMapnik\";
        private string UserDefindedUrl = @"http://tile.openstreetmap.org/{z}/{x}/{y}.png";

        public ContentViewer(KMZRebuilederForm parent)
        {
            this.parent = parent;
            Init();
        }

        public ContentViewer(KMZRebuilederForm parent, WaitingBoxForm waitBox)
        {
            this.parent = parent;
            this.wbf = waitBox;
            Init();
        }

        private void Init()
        {
            InitializeComponent();
            mapTootTip.ShowAlways = true;            

            mapSelect = new NaviMapNet.MapLayer("mapSelect");
            MapViewer.MapLayers.Add(mapSelect);
            mapContent = new NaviMapNet.MapLayer("mapContent");
            MapViewer.MapLayers.Add(mapContent);            

            MapViewer.NotFoundTileColor = Color.LightYellow;
            MapViewer.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.OSM_Mapnik;
            MapViewer.WebRequestTimeout = 3000;
            MapViewer.ZoomID = 10;
            MapViewer.OnMapUpdate = new NaviMapNet.NaviMapNetViewer.MapEvent(MapUpdate);
            //MapViewer.UserDefinedGetTileUrl = new NaviMapNet.NaviMapNetViewer.GetTilePathCall(this.GetTilePath);
            MapViewer.DrawMap = true;

            iStorages.Items.Add("No Map");

            iStorages.Items.Add("OSM Mapnik Render Tiles");
            iStorages.Items.Add("OSM OpenVkarte Render Tiles");
            iStorages.Items.Add("Wikimapia");

            iStorages.Items.Add("OpenTopoMaps");
            iStorages.Items.Add("Sputnik.ru");
            iStorages.Items.Add("RUMAP");
            iStorages.Items.Add("2GIS");
            iStorages.Items.Add("ArcGIS ESRI");

            iStorages.Items.Add("Nokia-Ovi");
            iStorages.Items.Add("OviMap");
            iStorages.Items.Add("OviMap Sputnik");
            iStorages.Items.Add("OviMap Relief");
            iStorages.Items.Add("OviMap Hybrid");

            iStorages.Items.Add("Kosmosnimki.ru ScanEx 1");
            iStorages.Items.Add("Kosmosnimki.ru ScanEx 2");
            iStorages.Items.Add("Kosmosnimki.ru IRS Sat");

            iStorages.Items.Add("Google Map");
            iStorages.Items.Add("Google Sat");

            iStorages.Items.Add("-- SAS Planet Cache --");
            iStorages.Items.Add("-- User-Defined Url --");

            iStorages.SelectedIndex = 1;

            MapViewer.UserDefinedGetTileUrl += new NaviMapNet.NaviMapNetViewer.GetTilePathCall(UserDefinedGetTileUrl);
        }

        private void iStorages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (iStorages.SelectedIndex == 0)
            {
                MapViewer.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Custom_UserDefined;
                MapViewer.ImageSourceType = NaviMapNet.NaviMapNetViewer.ImageSourceTypes.tiles;
                MapViewer.ImageSourceUrl = "";
            };
            if (iStorages.SelectedIndex == 1)
                MapViewer.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.OSM_Mapnik;
            if (iStorages.SelectedIndex == 2)
                MapViewer.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.OSM_Openvkarte;
            if (iStorages.SelectedIndex == 3)
                MapViewer.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.OSM_Wikimapia;
            if (iStorages.SelectedIndex > 3)
            {
                MapViewer.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Custom_UserDefined;
                MapViewer.ImageSourceType = NaviMapNet.NaviMapNetViewer.ImageSourceTypes.tiles;
                MapViewer.ImageSourceProjection = NaviMapNet.NaviMapNetViewer.ImageSourceProjections.EPSG3857;
            };
            if (iStorages.SelectedIndex == 4)
                MapViewer.ImageSourceUrl = "http://a.tile.opentopomap.org/{z}/{x}/{y}.png";
            if (iStorages.SelectedIndex == 5)
                MapViewer.ImageSourceUrl = "http://tiles.maps.sputnik.ru/{z}/{x}/{y}.png";
            if (iStorages.SelectedIndex == 6)
                MapViewer.ImageSourceUrl = "http://tile.digimap.ru/rumap/{z}/{x}/{y}.png";
            if (iStorages.SelectedIndex == 7)
                MapViewer.ImageSourceUrl = "https://tile1.maps.2gis.com/tiles?x={x}&y={y}&z={z}&v=1.1";
            if (iStorages.SelectedIndex == 8)
                MapViewer.ImageSourceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{z}/{y}/{x}.png";
            if (iStorages.SelectedIndex == 9)
                MapViewer.ImageSourceUrl = "http://maptile.mapplayer1.maps.svc.ovi.com/maptiler/maptile/newest/normal.day/{z}/{x}/{y}/256/png8";
            if (iStorages.SelectedIndex == 10)
                MapViewer.ImageSourceUrl = "http://1.maptile.lbs.ovi.com/maptiler/v2/maptile/newest/normal.day/{z}/{x}/{y}/256/png8?lg=RUS&token=fee2f2a877fd4a429f17207a57658582&appId=nokiaMaps";
            if (iStorages.SelectedIndex == 11)
                MapViewer.ImageSourceUrl = "http://1.maptile.lbs.ovi.com/maptiler/v2/maptile/newest/satellite.day/{z}/{x}/{y}/256/png8?lg=RUS&token=fee2f2a877fd4a429f17207a57658582&appId=nokiaMaps";
            if (iStorages.SelectedIndex == 12)
                MapViewer.ImageSourceUrl = "http://1.maptile.lbs.ovi.com/maptiler/v2/maptile/newest/hybrid.day/{z}/{x}/{y}/256/png8?lg=RUS&token=fee2f2a877fd4a429f17207a57658582&appId=nokiaMaps";
            if (iStorages.SelectedIndex == 13)
                MapViewer.ImageSourceUrl = "http://1.maptile.lbs.ovi.com/maptiler/v2/maptile/newest/terrain.day/{z}/{x}/{y}/256/png8?lg=RUS&token=fee2f2a877fd4a429f17207a57658582&appId=nokiaMaps";
            if (iStorages.SelectedIndex == 14)
                MapViewer.ImageSourceUrl = "http://maps.kosmosnimki.ru/TileService.ashx?Request=gettile&LayerName=04C9E7CE82C34172910ACDBF8F1DF49A&apikey=7BDJ6RRTHH&crs=epsg:3857&z={z}&x={x}&y={y}";
            if (iStorages.SelectedIndex == 15)
                MapViewer.ImageSourceUrl = "http://maps.kosmosnimki.ru/TileService.ashx?Request=gettile&LayerName=04C9E7CE82C34172910ACDBF8F1DF49A&apikey=7BDJ6RRTHH&crs=epsg:3857&z={z}&x={x}&y={y}";
            if (iStorages.SelectedIndex == 16)
                MapViewer.ImageSourceUrl = "http://irs.gis-lab.info/?layers=irs&request=GetTile&z={z}&x={x}&y={y}";
            if (iStorages.SelectedIndex == 17)
                MapViewer.ImageSourceUrl = "http://mts0.google.com/vt/lyrs=m@177000000&hl=ru&src=app&x={x}&s=&y={y}&z={z}&s=Ga";
            if (iStorages.SelectedIndex == 18)
                MapViewer.ImageSourceUrl = "http://mts0.google.com/vt/lyrs=h@177000000&hl=ru&src=app&x={x}&s=&y={y}&z={z}&s=G";
            if (iStorages.SelectedIndex == 20)
                MapViewer.ImageSourceUrl = UserDefindedUrl;

            MapViewer.ReloadMap();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            MapViewer.DrawTilesBorder = !MapViewer.DrawTilesBorder;
            toolStripMenuItem4.Checked = MapViewer.DrawTilesBorder;
            MapViewer.ReloadMap();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            MapViewer.DrawTilesXYZ = !MapViewer.DrawTilesXYZ;
            toolStripMenuItem5.Checked = MapViewer.DrawTilesXYZ;
            MapViewer.ReloadMap();
        }

        private void MapUpdate()
        {
            toolStripStatusLabel1.Text = "Last Requested File: " + MapViewer.LastRequestedFile;
            toolStripStatusLabel2.Text = MapViewer.CenterDegreesLat.ToString().Replace(",", ".");
            toolStripStatusLabel3.Text = MapViewer.CenterDegreesLon.ToString().Replace(",", ".");
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
            MapViewer.CenterDegrees = mo.Center;
            SelectOnMap(objects.SelectedIndices[0]);
        }

        private void laySelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            ni.NumberDecimalSeparator = ".";

            images.Images.Clear();
            objects.Items.Clear();
            mapContent.Clear();

            Hashtable imList = new Hashtable();            

            KMLayer l = (KMLayer)parent.kmzLayers.Items[laySelect.SelectedIndex];
            XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            XmlNodeList xns = xn.SelectNodes("Placemark/LineString/coordinates");            
            if (xns.Count > 0)
            {
                for (int x = 0; x < xns.Count; x++)
                {
                    toolStripStatusLabel1.Text = String.Format("Loading {0} of {1} lines",x,xns.Count);
                    statusStrip2.Refresh();
                    if (wbf != null) wbf.Text = String.Format("Loading {0} of {1} lines", x, xns.Count);
                    
                    string[] llza = xns[x].ChildNodes[0].Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    string name = xns[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value;
                    string description = "";
                    try { description = xns[x].ParentNode.ParentNode.SelectSingleNode("description").ChildNodes[0].Value; }
                    catch { };

                    string styleUrl = "";
                    if (xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl") != null) styleUrl = xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
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
                        sn = xns[x].ParentNode.ParentNode.SelectSingleNode("Style/LineStyle");
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
                                    Convert.ToInt32(colval.Substring(2, 2), 16),
                                    Convert.ToInt32(colval.Substring(4, 2), 16),
                                    Convert.ToInt32(colval.Substring(6, 2), 16)
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
                    foreach (string llzi in llza)
                    {
                        string[] llz = llzi.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        xy.Add(new PointF(float.Parse(llz[0], ni), float.Parse(llz[1], ni)));
                    };

                    NaviMapNet.MapPolyLine ml = new NaviMapNet.MapPolyLine(xy.ToArray());
                    ml.Name = name;
                    ml.UserData = description;
                    ml.Color = lineColor;
                    ml.Width = lineWidth;

                    mapContent.Add(ml);
                    ListViewItem lvi = objects.Items.Add(ml.Name, -1);
                    lvi.SubItems.Add("Line");
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("Placemark/LineString/coordinates["+x.ToString()+"]");
                };
            };

            xns = xn.SelectNodes("Placemark/Point/coordinates");
            if (xns.Count > 0)
            {
                for (int x = 0; x < xns.Count; x++)
                {
                    toolStripStatusLabel1.Text = String.Format("Loading {0} of {1} points", x, xns.Count);
                    statusStrip2.Refresh();
                    if (wbf != null) wbf.Text = String.Format("Loading {0} of {1} points", x, xns.Count);

                    string[] llz = xns[x].ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    string name = "";
                    try { name = xns[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value; } catch { };
                    string description = "";
                    try { description = xns[x].ParentNode.ParentNode.SelectSingleNode("description").ChildNodes[0].Value; }
                    catch { };

                    string styleUrl = "";
                    string href = "";
                    if (xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl") != null) styleUrl = xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                    if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);

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
                                try { im = Image.FromFile(l.file.tmp_file_dir + href); } catch { im = null; };
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
                    ListViewItem lvi = objects.Items.Add(String.Format("{0}", mp.Name, mp.Center.Y.ToString().Replace(",", "."), mp.Center.X.ToString().Replace(",", ".")), ii);
                    lvi.SubItems.Add("Point");
                    lvi.SubItems.Add(mp.Center.Y.ToString());
                    lvi.SubItems.Add(mp.Center.X.ToString());
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add("Placemark/Point/coordinates[" + x.ToString() + "]");
                    if ((x == 0) && firstboot) MapViewer.CenterDegrees = mp.Center;
                };
            };

            toolStripStatusLabel1.Text = "All placemarks loaded";
            statusStrip2.Refresh();

            MapViewer.DrawOnMapData();
            firstboot = false;
        }

        private void FindCopies(int toIndex, bool xy4, bool nm4, Single distanceInMeters)
        {
            if (objects.Items.Count == 0) return;

            Color[] colors = new Color[] { 
                Color.LightBlue, Color.LightCoral, Color.LightCyan, Color.LightGray, Color.LightGreen, 
                Color.LightPink, Color.LightSalmon, Color.LightSeaGreen, Color.LightSkyBlue, Color.LightSteelBlue, 
                Color.LightYellow, Color.Lime, Color.LimeGreen, Color.Orange, Color.OrangeRed, 
                Color.Pink, Color.RoyalBlue, Color.SeaGreen, Color.SeaShell, Color.SkyBlue, 
                Color.Tan, Color.YellowGreen};

            int simIndex = 0;

            Hashtable simByXY = new Hashtable();
            Hashtable simByNm = new Hashtable();

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
                        if (xy4)
                        {
                            if (a.PointsCount == b.PointsCount)
                            {
                                bool isSame = true;
                                for (int n = 0; n < a.PointsCount; n++)
                                {
                                    if (distanceInMeters <= 0)
                                    {
                                        if (a.Points[n].X != b.Points[n].X) { isSame = false; break; };
                                        if (a.Points[n].Y != b.Points[n].Y) { isSame = false; break; }
                                    }
                                    else
                                    {
                                        float dist = Utils.GetLengthMeters(a.Points[n].Y, a.Points[n].X, b.Points[n].Y, b.Points[n].X, false);
                                        if (dist > distanceInMeters) { isSame = false; break; };
                                    };
                                };
                                if (isSame)
                                {
                                    string key = a.Center.X.ToString() + "," + a.Center.Y.ToString();
                                    if (!simByXY.ContainsKey(key)) simByXY.Add(key, new int[] { simIndex++ });
                                    List<int> val = new List<int>();
                                    val.AddRange((int[])simByXY[key]);
                                    if (val.IndexOf(i, 1) < 0) val.Add(i);
                                    if (val.IndexOf(j, 1) < 0) val.Add(j);
                                    simByXY[key] = val.ToArray();

                                    int colIndex = val[0] % colors.Length;
                                    objects.Items[i].BackColor = colors[colIndex];
                                    objects.Items[j].BackColor = colors[colIndex];

                                    objects.Items[i].SubItems[4].Text = val[0].ToString();
                                    objects.Items[j].SubItems[4].Text = val[0].ToString();
                                };
                            };
                        };
                        if (nm4)
                        {
                            if (a.Name == b.Name)
                            {
                                string key = a.Name;
                                if (!simByNm.ContainsKey(key)) simByNm.Add(key, new int[] { simIndex++ });
                                List<int> val = new List<int>();
                                val.AddRange((int[])simByNm[key]);
                                if (val.IndexOf(i, 1) < 0) val.Add(i);
                                if (val.IndexOf(j, 1) < 0) val.Add(j);
                                simByNm[key] = val.ToArray();

                                int colIndex = val[0] % colors.Length;
                                objects.Items[i].BackColor = colors[colIndex];
                                objects.Items[j].BackColor = colors[colIndex];

                                objects.Items[i].SubItems[5].Text = val[0].ToString();
                                objects.Items[j].SubItems[5].Text = val[0].ToString();
                            };
                        };
                    };

            status.Text = "";
            if (simByXY.Count > 0) status.Text += "Found " + simByXY.Count.ToString() + " similar placemarks by coordinates\r\n";
            if (simByNm.Count > 0) status.Text += "Found " + simByNm.Count.ToString() + " similar placemarks by name\r\n";
            if (status.Text == "") status.Text = "No copies found";
            status.SelectionStart = status.TextLength;
            status.ScrollToCaret();
        }
        
        private void MapViewer_MouseClick(object sender, MouseEventArgs e)
        {
            if (!locate) return;
            if (mapContent.ObjectsCount == 0) return;
            Point clicked = MapViewer.MousePositionPixels;
            PointF sCenter = MapViewer.PixelsToDegrees(clicked);
            PointF sFrom = MapViewer.PixelsToDegrees(new Point(clicked.X - 5, clicked.Y + 5));
            PointF sTo = MapViewer.PixelsToDegrees(new Point(clicked.X + 5, clicked.Y - 5));
            NaviMapNet.MapObject[] objs = mapContent.Select(new RectangleF(sFrom, new SizeF(sTo.X - sFrom.X, sTo.Y - sFrom.Y)));
            if ((objs != null) && (objs.Length > 0))
            {
                uint len = uint.MaxValue;
                int ind = 0;
                for (int i = 0; i < objs.Length; i++)
                {
                    uint tl = GetLengthMetersC(sCenter.Y, sCenter.X, objs[i].Center.Y, objs[i].Center.X, false);
                    if (tl < len) { len = tl; ind = i; };
                };

                if((objects.SelectedIndices.Count == 0) || (objects.SelectedIndices[0] != objs[ind].Index))
                {                    
                    objects.Items[objs[ind].Index].Selected = true;
                    objects.Items[objs[ind].Index].Focused = true;
                };

                SelectOnMap(objs[ind].Index);
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
            if (objects.SelectedItems.Count == 0) return;

            objects.EnsureVisible(objects.SelectedIndices[0]);
            prevSII = objects.SelectedItems[0];
            prevSIC = objects.SelectedItems[0].BackColor;
            objects.SelectedItems[0].BackColor = Color.Red;

            NaviMapNet.MapObject mo = mapContent[objects.SelectedIndices[0]];
            textBox1.Text = mo.UserData.ToString().Replace("<br/>", "\r\n").Replace("<br>", "\r\n");
        }

        private void SelectOnMap(int id)
        {
            if (id < 0) return;

            mapSelect.Clear();
            if (mapContent[id].ObjectType == NaviMapNet.MapObjectType.mPoint)
            {
                NaviMapNet.MapPoint mp = new NaviMapNet.MapPoint(mapContent[id].Center);
                mp.Name = "Selected";
                mp.SizePixels = new Size(22, 22);
                mp.Squared = false;
                mp.Color = Color.Red;
                mapSelect.Add(mp);
                MapViewer.DrawOnMapData();
            };
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {   
            currItem.Enabled = objects.SelectedItems.Count > 0;
            allItem.Enabled = objects.Items.Count > 0;

            renameToolStripMenuItem.Enabled = objects.SelectedItems.Count > 0;

            markAsSkipWhenSaveToolStripMenuItem.Enabled = objects.SelectedItems.Count > 0;
            if(objects.SelectedItems.Count > 0)
                markAsSkipWhenSaveToolStripMenuItem.Checked = objects.SelectedItems[0].SubItems[6].Text == "Yes";            
        }

        private void findSimilarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            string dist = "0";
            if (KMZRebuilederForm.InputBox("Distance", "Max distance in meters:", ref dist) == DialogResult.OK)
            {
                int d = 0;
                if(int.TryParse(dist, out d))
                    FindCopies(objects.SelectedIndices[0], true, false, d);
            };            
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

        private void markSimilarAsDeletedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            string simByXY = objects.SelectedItems[0].SubItems[4].Text;
            string simByNM = objects.SelectedItems[0].SubItems[5].Text;
            
            int sxy = 0;
            if (simByXY != "")
                for (int i = 0; i < objects.Items.Count; i++)
                    if(i != objects.SelectedIndices[0])
                        if (objects.Items[i].SubItems[4].Text == simByXY)
                        {
                            objects.Items[i].SubItems[6].Text = "Yes";
                            objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Strikeout);
                            sxy++;
                        };

            int snm = 0;
            if (simByNM != "")
                for (int i = 0; i < objects.Items.Count; i++)
                    if (i != objects.SelectedIndices[0])
                        if (objects.Items[i].SubItems[5].Text == simByNM)
                        {
                            objects.Items[i].SubItems[6].Text = "Yes";
                            objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Strikeout);
                            snm++;
                        };

            if (sxy > 0) status.Text += "Marked " + sxy.ToString() + " copies as Deleted by Coordinates\r\n";
            if (snm > 0) status.Text += "Marked " + snm.ToString() + " copies as Deleted by Name\r\n";
            status.SelectionStart = status.TextLength;
            status.ScrollToCaret();

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
            string dist = "0";
            if (KMZRebuilederForm.InputBox("Distance", "Max distance in meters:", ref dist) == DialogResult.OK)
            {
                int d = 0;
                if (int.TryParse(dist, out d))
                    FindCopies(-1, true, false, d);
            };                    
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

        private void markCopiesAsNotDeletedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objects.SelectedItems.Count == 0) return;
            string simByXY = objects.SelectedItems[0].SubItems[4].Text;
            string simByNM = objects.SelectedItems[0].SubItems[5].Text;

            int sxy = 0;
            if (simByXY != "")
                for (int i = 0; i < objects.Items.Count; i++)
                    if (i != objects.SelectedIndices[0])
                        if (objects.Items[i].SubItems[4].Text == simByXY)
                        {
                            objects.Items[i].SubItems[6].Text = "";
                            objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Regular);
                            sxy++;
                        };

            int snm = 0;
            if (simByNM != "")
                for (int i = 0; i < objects.Items.Count; i++)
                    if (i != objects.SelectedIndices[0])
                        if (objects.Items[i].SubItems[5].Text == simByNM)
                        {
                            objects.Items[i].SubItems[6].Text = "";
                            objects.Items[i].Font = new Font(objects.Items[i].Font, FontStyle.Regular);
                            snm++;
                        };

            if (sxy > 0) status.Text += "Marked " + sxy.ToString() + " copies as Not Deleted by Coordinates\r\n";
            if (snm > 0) status.Text += "Marked " + snm.ToString() + " copies as Not Deleted by Name\r\n";
            status.SelectionStart = status.TextLength;
            status.ScrollToCaret();

            CheckMarked();
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
            for (int i = objects.Items.Count - 1; i >= 0; i--)
                if (objects.Items[i].SubItems[6].Text == "Yes")
                {
                    string XPath = objects.Items[i].SubItems[7].Text;
                    string indx = XPath.Substring(XPath.IndexOf("["));
                    XPath = XPath.Remove(XPath.IndexOf("["));
                    int ind = int.Parse(indx.Substring(1, indx.Length - 2));
                    XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                    xn = xn.SelectNodes(XPath)[ind].ParentNode.ParentNode;
                    xn = xn.ParentNode.RemoveChild(xn);
                    pDel++;
                };
            l.file.SaveKML();
            l.placemarks -= pDel;
            parent.Refresh();
        }

        private void CheckMarked()
        {
            if (objects.Items.Count == 0) return;

            int marked = 0;
            for (int i = 0; i < objects.Items.Count; i++)
                if (objects.Items[i].SubItems[6].Text == "Yes")
                    marked++;

            laySelect.Enabled = marked == 0;
            
        }

        private void objects_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != Convert.ToChar(Keys.Enter)) return;
            if (objects.SelectedIndices.Count == 0) return;            
            SelectOnMap(objects.SelectedIndices[0]);
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
            XmlNode xd = xf.SelectNodes(XPath)[ind].ParentNode.ParentNode.SelectSingleNode("description");

            string nam = xn.ChildNodes[0].Value;
            string xyt = xy == null ? ",," : xy.ChildNodes[0].Value;
            string[] xya = xyt.Split(new string[] { "," }, StringSplitOptions.None);
            string x = xya[0];
            string y = xya[1];
            string desc = "";
            if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
            string dw = desc;

            if (InputXY(objects.SelectedItems[0].SubItems[1].Text == "Point", ref nam, ref y, ref x, ref desc) == DialogResult.OK)
            {
                bool ch = false;
                bool chxy = false;
                nam = nam.Trim();
                desc = desc.Trim();
                if (nam != xn.ChildNodes[0].Value) ch = true;
                if (desc != dw) ch = true;
                x = x.Trim().Replace(",", ".");
                y = y.Trim().Replace(",", ".");
                if (x != xya[0]) chxy = true;
                if (y != xya[1]) chxy = true;
                if (ch)
                {
                    objects.SelectedItems[0].Text = nam;
                    xn.ChildNodes[0].Value = nam;
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
                    MapViewer.DrawOnMapData();
                };
                if (ch || chxy) l.file.SaveKML();
            };
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

        private void changeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private string UserDefinedGetTileUrl(int x, int y, int z)
        {
            if (iStorages.SelectedIndex == 19) return SASPlanetCache(x, y, z + 1);
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
                SASPlanetCacheDir = spcd;
        }

        private void èçìåíèòüUserDefinedUrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string udu = UserDefindedUrl;
            if (KMZRebuilederForm.InputBox("User-Defined Url", "Enter Url Here:", ref udu) == DialogResult.OK)
            {
                UserDefindedUrl = udu;
                if (iStorages.SelectedIndex == 20)
                    MapViewer.ImageSourceUrl = UserDefindedUrl;
            };
        }

        private void MapViewer_MouseDown(object sender, MouseEventArgs e)
        {
            locate = true;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (Char)13) return;
            if (objects.Items.Count == 0) return;
            string st = textBox2.Text.ToLower();
            for (int i = 0; i < objects.Items.Count; i++)
                if (objects.Items[i].SubItems[0].Text.ToLower().Contains(st))
                {
                    objects.Items[i].Selected = true;
                    objects.Items[i].EnsureVisible();
                    break;
                };
        }
    }
}