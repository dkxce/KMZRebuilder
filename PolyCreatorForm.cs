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
    public partial class PolyCreator : Form
    {
        private WaitingBoxForm wbf = null;
        public NaviMapNet.MapLayer mapContent = null;
        public ToolTip mapTootTip = new ToolTip();
        private KMZRebuilederForm parent = null;

        private string SASPlanetCacheDir = @"C:\Program Files\SASPlanet\cache\osmmapMapnik\";
        private string UserDefindedUrl = @"http://tile.openstreetmap.org/{z}/{x}/{y}.png";

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

        private void Init()
        {
            InitializeComponent();
            mapTootTip.ShowAlways = true;            

            mapContent = new NaviMapNet.MapLayer("mapContent");
            MView.MapLayers.Add(mapContent);            

            MView.NotFoundTileColor = Color.LightYellow;
            MView.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.OSM_Mapnik;
            MView.WebRequestTimeout = 3000;
            MView.ZoomID = 10;
            MView.OnMapUpdate = new NaviMapNet.NaviMapNetViewer.MapEvent(MapUpdate);
            //MapViewer.UserDefinedGetTileUrl = new NaviMapNet.NaviMapNetViewer.GetTilePathCall(this.GetTilePath);
            MView.DrawMap = true;

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

            MView.UserDefinedGetTileUrl += new NaviMapNet.NaviMapNetViewer.GetTilePathCall(UserDefinedGetTileUrl);
        }

        private void iStorages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (iStorages.SelectedIndex == 0)
            {
                MView.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Custom_UserDefined;
                MView.ImageSourceType = NaviMapNet.NaviMapNetViewer.ImageSourceTypes.tiles;
                MView.ImageSourceUrl = "";
            };
            if (iStorages.SelectedIndex == 1)
                MView.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.OSM_Mapnik;
            if (iStorages.SelectedIndex == 2)
                MView.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.OSM_Openvkarte;
            if (iStorages.SelectedIndex == 3)
                MView.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.OSM_Wikimapia;
            if (iStorages.SelectedIndex > 3)
            {
                MView.ImageSourceService = NaviMapNet.NaviMapNetViewer.MapServices.Custom_UserDefined;
                MView.ImageSourceType = NaviMapNet.NaviMapNetViewer.ImageSourceTypes.tiles;
                MView.ImageSourceProjection = NaviMapNet.NaviMapNetViewer.ImageSourceProjections.EPSG3857;
            };
            if (iStorages.SelectedIndex == 4)
                MView.ImageSourceUrl = "http://a.tile.opentopomap.org/{z}/{x}/{y}.png";
            if (iStorages.SelectedIndex == 5)
                MView.ImageSourceUrl = "http://tiles.maps.sputnik.ru/{z}/{x}/{y}.png";
            if (iStorages.SelectedIndex == 6)
                MView.ImageSourceUrl = "http://tile.digimap.ru/rumap/{z}/{x}/{y}.png";
            if (iStorages.SelectedIndex == 7)
                MView.ImageSourceUrl = "https://tile1.maps.2gis.com/tiles?x={x}&y={y}&z={z}&v=1.1";
            if (iStorages.SelectedIndex == 8)
                MView.ImageSourceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{z}/{y}/{x}.png";
            if (iStorages.SelectedIndex == 9)
                MView.ImageSourceUrl = "http://maptile.mapplayer1.maps.svc.ovi.com/maptiler/maptile/newest/normal.day/{z}/{x}/{y}/256/png8";
            if (iStorages.SelectedIndex == 10)
                MView.ImageSourceUrl = "http://1.maptile.lbs.ovi.com/maptiler/v2/maptile/newest/normal.day/{z}/{x}/{y}/256/png8?lg=RUS&token=fee2f2a877fd4a429f17207a57658582&appId=nokiaMaps";
            if (iStorages.SelectedIndex == 11)
                MView.ImageSourceUrl = "http://1.maptile.lbs.ovi.com/maptiler/v2/maptile/newest/satellite.day/{z}/{x}/{y}/256/png8?lg=RUS&token=fee2f2a877fd4a429f17207a57658582&appId=nokiaMaps";
            if (iStorages.SelectedIndex == 12)
                MView.ImageSourceUrl = "http://1.maptile.lbs.ovi.com/maptiler/v2/maptile/newest/hybrid.day/{z}/{x}/{y}/256/png8?lg=RUS&token=fee2f2a877fd4a429f17207a57658582&appId=nokiaMaps";
            if (iStorages.SelectedIndex == 13)
                MView.ImageSourceUrl = "http://1.maptile.lbs.ovi.com/maptiler/v2/maptile/newest/terrain.day/{z}/{x}/{y}/256/png8?lg=RUS&token=fee2f2a877fd4a429f17207a57658582&appId=nokiaMaps";
            if (iStorages.SelectedIndex == 14)
                MView.ImageSourceUrl = "http://maps.kosmosnimki.ru/TileService.ashx?Request=gettile&LayerName=04C9E7CE82C34172910ACDBF8F1DF49A&apikey=7BDJ6RRTHH&crs=epsg:3857&z={z}&x={x}&y={y}";
            if (iStorages.SelectedIndex == 15)
                MView.ImageSourceUrl = "http://maps.kosmosnimki.ru/TileService.ashx?Request=gettile&LayerName=04C9E7CE82C34172910ACDBF8F1DF49A&apikey=7BDJ6RRTHH&crs=epsg:3857&z={z}&x={x}&y={y}";
            if (iStorages.SelectedIndex == 16)
                MView.ImageSourceUrl = "http://irs.gis-lab.info/?layers=irs&request=GetTile&z={z}&x={x}&y={y}";
            if (iStorages.SelectedIndex == 17)
                MView.ImageSourceUrl = "http://mts0.google.com/vt/lyrs=m@177000000&hl=ru&src=app&x={x}&s=&y={y}&z={z}&s=Ga";
            if (iStorages.SelectedIndex == 18)
                MView.ImageSourceUrl = "http://mts0.google.com/vt/lyrs=h@177000000&hl=ru&src=app&x={x}&s=&y={y}&z={z}&s=G";
            if (iStorages.SelectedIndex == 20)
                MView.ImageSourceUrl = UserDefindedUrl;

            MView.ReloadMap();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            MView.DrawTilesBorder = !MView.DrawTilesBorder;
            toolStripMenuItem4.Checked = MView.DrawTilesBorder;
            MView.ReloadMap();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            MView.DrawTilesXYZ = !MView.DrawTilesXYZ;
            toolStripMenuItem5.Checked = MView.DrawTilesXYZ;
            MView.ReloadMap();
        }

        private void MapUpdate()
        {
            toolStripStatusLabel1.Text = "Last Requested File: " + MView.LastRequestedFile;
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
                    MView.ImageSourceUrl = UserDefindedUrl;
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

            if (inMapPoly.Count == 0)
            {
                MView.DrawOnMapData();
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
                }
                else
                {
                    NaviMapNet.MapPolyLine ml = new NaviMapNet.MapPolyLine(inMapPoly.ToArray());
                    ml.Width = 2;
                    ml.Name = "MyLine";
                    ml.Color = Color.FromArgb(125, Color.Maroon);
                    ml.BorderColor = Color.Maroon;                    
                    PolyLineBuffer.PolyLineBufferCreator.PolyResult pr = PolyLineBuffer.PolyLineBufferCreator.GetLineBufferPolygon(inMapPoly.ToArray(), (int)cD.Value, cR.Checked, cL.Checked, DistFunc);
                    NaviMapNet.MapPolygon mp = new NaviMapNet.MapPolygon(pr.polygon);
                    mp.Width = 2;
                    mp.Name = "MyPoly";
                    mp.Color = Color.FromArgb(125, Color.Red);
                    mp.BorderColor = Color.Red;
                    mapContent.Add(mp);
                    mapContent.Add(ml);
                    finalpoly = mp.Points;
                };
            };
            MView.DrawOnMapData();
        }


        public static float DistFunc(PointF a, PointF b)
        {
            return GetLengthMetersC(a.Y, a.X, b.Y, b.X, false);
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
            ofd.Filter = "KML & GPX files (*.kml;*.gpx)|*.kml;*.gpx";
            ofd.DefaultExt = "*.kml,*.gpx";            
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                loadroute(ofd.FileName);
            };
            ofd.Dispose();
        }

        private void loadroute(string filename)
        {
            System.IO.FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            System.IO.StreamReader sr = new StreamReader(fs);
            inMapPoly.Clear();

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
                    inMapPoly.Add(new PointF(float.Parse(lon, System.Globalization.CultureInfo.InvariantCulture), float.Parse(lat, System.Globalization.CultureInfo.InvariantCulture)));

                    si = file.IndexOf("<rtept", ei);
                    if (si > 0)
                        ei = file.IndexOf(">", si);
                };
            };
            sr.Close();
            fs.Close();

            if (inMapPoly.Count > 0)
            {
                cLR.Checked = false;
                NaviMapNet.MapPolygon mp = new NaviMapNet.MapPolygon(inMapPoly.ToArray());
                MView.CenterDegrees = mp.Center;
            };
            RedrawPoly();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (finalpoly == null) return;
            if (finalpoly.Length < 2) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".kml";
            sfd.Filter = "KML files (*.kml)|*.kml";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);                
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\"><Document>");
                sw.WriteLine("<Placemark><name>My Polygon</name>");
                sw.WriteLine("<Polygon><extrude>1</extrude><outerBoundaryIs><LinearRing><coordinates>");
                foreach (PointF p in finalpoly)
                    sw.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0", p.X, p.Y));
                sw.WriteLine("</coordinates></LinearRing></outerBoundaryIs></Polygon></Placemark>");
                sw.WriteLine("</Document>");
                sw.WriteLine("</kml>");
                sw.Close();
                fs.Close();
            };
            sfd.Dispose();
        }
    }
}