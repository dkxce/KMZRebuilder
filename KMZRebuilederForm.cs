using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Windows.Forms;
using System.Drawing.Imaging;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace KMZRebuilder
{
    public partial class KMZRebuilederForm : Form
    {
        static KMZRebuilederForm()
        {
            try
            {
                if (IntPtr.Size == 4) File.Copy(KMZRebuilederForm.CurrentDirectory() + @"\SQLite.Interop.x86.dll", KMZRebuilederForm.CurrentDirectory() + @"\SQLite.Interop.dll", true);
                else File.Copy(KMZRebuilederForm.CurrentDirectory() + @"\SQLite.Interop.x64.dll", KMZRebuilederForm.CurrentDirectory() + @"\SQLite.Interop.dll", true);
            }
            catch (Exception ex) { };
        }

        // P/Invoke constants
        private const int WM_SYSCOMMAND = 0x112;
        private const int MF_STRING = 0x0;
        private const int MF_SEPARATOR = 0x800;

        // KMZ Viewer Commands
        private const int XP_OPENFILE = 0xA801;
        private const int XP_OPENLAYER = 0xA802;
        private const int XP_OPENLAYERS = 0xA803;

        // P/Invoke declarations
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);

        private int SYSMENU_ABOUT_ID = 0x1;
        private int SYSMENU_WGSFormX = 0x2;
        private int SYSMENU_DefSize  = 0x3;
        private int SYSMENU_MinSize  = 0x4;
        private int SYSMENU_NEW_INST = 0x5;

        public string[] args;
        public MruList MyMruList;
        public static WaitingBoxForm waitBox;
        public MapIcons mapIcons;

        private MemFile.MemoryFile memFile = null;

        public Preferences Properties = Preferences.Load();

        public static PointInRegionUtils PIRU = new PointInRegionUtils();

        public static string CurrentDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
            // return Application.StartupPath;
            // return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // return System.IO.Directory.GetCurrentDirectory();
            // return Environment.CurrentDirectory;
            // return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            // return System.IO.Path.GetDirectory(Application.ExecutablePath);
        }

        public static string TempDirectory()
        {
            string dir = CurrentDirectory();
            if (!dir.EndsWith(@"\")) dir += @"\";
            dir += @"TMP\";
            return dir;
        }

        public KMZRebuilederForm(string[] args)
        {
            this.args = args;
            
            InitializeComponent();            
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Text += " " + fvi.FileVersion + " by " + fvi.CompanyName;

            RegisterFileAsses();
            prepareTranslit();
            LoadPreferences();
            MapIcons.InitZip(CurrentDirectory() + @"\mapicons\default.zip");
        }

        private void LoadPreferences()
        {
            GPIReader.LOCALE_LANGUAGE = Properties["gpi_localization"].ToUpper();
            GPIReader.SAVE_MEDIA = Properties.GetBoolValue("gpireader_save_media");
        }

        private void RegisterFileAsses()
        {
            FileAss.SetFileAssociation("kmz", "KMZFile", "Open in KMZRebuilder", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileAssociation("kml", "KMLFile", "Open in KMZRebuilder", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileAssociation("wpt", "WPTFile", "Open in KMZRebuilder", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileAssociation("gpx", "GPXFile", "Open in KMZRebuilder", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileAssociation("gpi", "KMZFile", "Open in KMZRebuilder", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileAssociation("rpp", "RPPFile", "Open in KMZRebuilder", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("dat", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("gdb", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("txt", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("csv", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("osm", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("db3", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("poi", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("map", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("fit", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("rpp", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.SetFileOpenWith("dxml", CurrentDirectory() + @"\KMZRebuilder.exe");
            FileAss.UpdateExplorer();
        }

        private void FormKMZ_Load(object sender, EventArgs e)
        {
            //foreach (PointF loc in GoogleGeometry.GooglePolylineConverter.DecodePE("wkwzHohvz@KYOWSUIEGEu@k@o@k@_@g@GIEKK[ESCQAMDWDWDW@]Bk@TeDBODQBGDIFKJGd@YJMJMFOBQ@Q?SCYa@aCAa@Aq@@}@D]Pu@Hc@B_@?a@A]g@gD]aC_@eCq@gFGg@GeAG}AG{BIqGEcB?aBFqAJqA^eC^mBZmAZaAX{@h@yA`AgBfAsBXs@\\mA`@cClAgKjCsUj@kEPkAJg@~@iD`@{Af@_CXoBPqBPoBD_@PcBj@uCb@oBDU\\cCTaB\\cDt@iGv@gFn@mDxAeHj@mC`@aBb@qA^y@t@mAvCmEt@qA~CmIlAaD~AeE^_Av@wA~@iAdAy@jAc@^OFCj@Wf@a@jCwCp@{@L_@F_@F{ADi@h@cDLa@JSNGNCb@@`B@d@ETKZ]FSNe@dAwDLa@~C{N`BoHfAoE|AoGjBuGjCwJtAaGnAkGlC}NJe@r@yD`@qBVqAn@mCVkAbAwDDQ`@sA|AiElBuEzAuCdCsDpBmCfG{GzF{FfBaBrAiAtAaAhBq@pCq@HAJC`Ac@j@_@x@s@v@}@fBeClCuDb@o@Rc@N]DOBO@M@O?O?[Aq@?Y"))
            //    MessageBox.Show(loc.ToString());

            MyMruList = new MruList(CurrentDirectory() + @"\KMZRebuilder.mru", MRU, 10);
            MyMruList.FileSelected += new MruList.FileSelectedEventHandler(MyMruList_FileSelected);
            waitBox = new WaitingBoxForm(this);

            this.AllowDrop = true;
            this.DragDrop += bgFiles_DragDrop;
            this.DragEnter += bgFiles_DragEnter;

            if(Directory.Exists(TempDirectory())) System.IO.Directory.Delete(TempDirectory(),true);
            System.IO.Directory.CreateDirectory(TempDirectory());

            LoadFiles(null, null);
            PreLoadPlugins(openPluginsToolStripMenuItem);
        }

        private void LoadFiles(object sender, EventArgs e)
        {
            if ((args != null) && (args.Length > 0))
            {
                List<string> files = new List<string>();
                foreach (string arg in args)
                    if (File.Exists(arg))
                        files.Add(arg);
                if (files.Count > 0)
                    LoadFiles(files.ToArray());
            };
        }

        private void MyMruList_FileSelected(string file_name)
        {
            LoadFiles(new string[] { file_name });
        }

        private void bgFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        private void bgFiles_DragDrop(object sender, DragEventArgs e)
        {
            LoadFiles((string[])e.Data.GetData(DataFormats.FileDrop));
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Main Supported Files|*.kml;*.kmz;*.gpx;*.dat;*.wpt;*.db3;*.osm;*.gdb;*.fit;*.zip;*.rpp;*.dxml;*.gpi|KML Format (*.kml)|*.kml|KMZ Format (*.kmz)|*.kmz|GPX Exchange Format (*.gpx)|*.gpx|ProGorod Favorites.dat (*.dat)|*.dat|OziExplorer Waypoint File (*.wpt)|*.wpt|SASPlanet SQLite (*.db3)|*.db3|OSM Export File (*.osm)|*.osm|Navitel Waypoints (*.gdb)|*.gdb|Garmin Ant Fit (*.fit)|*.fit|Garmin POI (*.gpi)|*.gpi";
            ofd.DefaultExt = ".kmz";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK) LoadFiles(ofd.FileNames);
            ofd.Dispose();
        }

        private void LoadFiles(string[] files)
        {
            foreach (string file in files)
                if (Path.GetExtension(file).ToLower() == ".zip")
                {
                    List<string> f2l = new List<string>();
                    string tmp_file_dir = KMZRebuilederForm.TempDirectory() + "IF" + DateTime.UtcNow.Ticks.ToString() + @"\";
                    ZipFile zf = new ZipFile(file);
                    foreach (ZipEntry zipEntry in zf)
                    {
                        if (!zipEntry.IsFile) continue;
                        String entryFileName = zipEntry.Name;
                        byte[] buffer = new byte[4096];     // 4K is optimum
                        Stream zipStream = zf.GetInputStream(zipEntry);
                        String fullZipToPath = Path.Combine(tmp_file_dir, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);
                        using (FileStream streamWriter = File.Create(fullZipToPath))
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        f2l.Add(fullZipToPath);
                    };
                    zf.Close();
                    if (f2l.Count > 0)
                        LoadFiles(f2l.ToArray());
                };

            int c = kmzFiles.Items.Count;
            if ((files.Length == 1) && ((Path.GetExtension(files[0]).ToLower() == ".rpp")))
            {
                waitBox.Show("Wait", "Loading map...");
                TrackSplitter pc = new TrackSplitter(this, waitBox);
                pc.OpenProject(files[0], true);
                pc.SetPlannerPage();
                waitBox.Hide();
                pc.ShowDialog();
                pc.Dispose();
                return;
            };
            if ((files.Length == 1) && ((Path.GetExtension(files[0]).ToLower() == ".dxml")))
            {
                waitBox.Show("Wait", "Loading map...");
                TrackSplitter pc = new TrackSplitter(this, waitBox);
                pc.SetPlannerPage();
                pc.Payways = AvtodorTRWeb.PayWays.Load(files[0]);
                waitBox.Hide();
                pc.ShowDialog();
                pc.Dispose();
                return;
            };
            if ((files.Length == 1) && ((Path.GetExtension(files[0]).ToLower() == ".dbf")))
            {
                ImportFromDBF(files[0]);
                return;
            };
            if ((files.Length == 1) && ((Path.GetExtension(files[0]).ToLower() == ".shp")))
            {
                ImportFromSHP(files[0]);
            };
            if ((files.Length == 1) && ((Path.GetExtension(files[0]).ToLower() == ".poi")))
            {
                ImportPOI(files[0]);
                return;
            };
            if ((files.Length == 1) && ((Path.GetExtension(files[0]).ToLower() == ".map")))
            {
                ImportMapsForgeMap(files[0]);
                return;
            };
            if((files.Length == 1) && ((Path.GetExtension(files[0]).ToLower() == ".csv") || (Path.GetExtension(files[0]).ToLower() == ".txt")))
            {
                ImportFromText(new FileInfo(files[0]));
                return;
            };
            if ((files.Length == 1) && (Path.GetExtension(files[0]).ToLower() == ".db3"))
            {
                ImportDB3(files[0]);
                return;
            };
            
            foreach (string file in files)
            {
                waitBox.Show("Loading", "Wait, loading file `" + Path.GetFileName(file) + "` ...");
                bool skip = false;
                foreach (object oj in kmzFiles.Items)
                {
                    KMFile f = (KMFile)oj;
                    if (f.src_file_pth == file) skip = true;
                };

                if (!KMFile.ValidForDragDropAuto(file)) skip = true;

                if (skip)
                    continue;
                else
                {
                    if (Path.GetExtension(file).ToLower() == ".osm")
                    {
                        FileInfo fi = new FileInfo(file);
                        if(file.Length > (1024*1024*300))
                        {
                            MessageBox.Show("File size is too big!", "Import .osm file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            waitBox.Hide();
                            return;
                        };
                    };
                    KMFile f = new KMFile(file);
                    if (!f.Valid) continue;
                    kmzFiles.Items.Add(f, f.isCheck);
                    MyMruList.AddFile(file);
                    if ((outName.Text == String.Empty) ||(kmzFiles.Items.Count == 1)) outName.Text = f.kmldocName;
                    if (f.parseError)
                        MessageBox.Show("File `" + Path.GetFileName(file) + "` loaded with errors!\r\nCheck your data and save it to normal file first!", "Loading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                };
            };

            if (c != kmzFiles.Items.Count) ReloadListboxLayers(true);
            waitBox.Hide();
        }

        private void ReloadListboxLayers(bool fullUpdate)
        {
            if (fullUpdate)
            {
                kmzLayers.Visible = false;
                kmzLayers.Items.Clear();                
            };

            int p = 0;
            int c = 0;
            int e = 0;
            int s = 0;
            if (kmzFiles.Items.Count == 0)
            {
                STT0.Text = "Layers";
                STT1.Text = "";
                STT2.Text = "";
                STT3.Text = "";
                STT4.Text = "";
                STT5.Text = "";
                STT6.Text = "";
                if (fullUpdate)
                    kmzLayers.Visible = true;
                return;
            };
            Dictionary<string, List<Point>> ATB = new Dictionary<string, List<Point>>();            
            for(int i=0;i<kmzFiles.Items.Count;i++)
            {
                KMFile f = (KMFile)kmzFiles.Items[i];
                if (!f.isCheck) continue;

                int r = 0;
                foreach (KMLayer layer in f.kmLayers)
                {
                    try
                    {
                        layer.ATB = -1;
                        if (!ATB.ContainsKey(layer.name))
                            ATB.Add(layer.name, new List<Point>(new Point[] { new Point(i, r) }));
                        else
                            ATB[layer.name].Add(new Point(i, r));
                    }
                    catch { };

                    if (fullUpdate)
                        kmzLayers.Items.Add(layer, layer.ischeck);
                    p += layer.placemarks;
                    if (layer.ischeck)
                    {
                        c++;
                        s += layer.placemarks;
                    };
                    if (layer.placemarks == 0) e++;
                    r++;
                };
            };

            try
            {
                int next_char = 0;
                foreach (KeyValuePair<string, List<Point>> atv in ATB)
                    if (atv.Value.Count > 1)
                    {
                        foreach (Point fl in atv.Value)
                            ((KMFile)kmzFiles.Items[fl.X]).kmLayers[fl.Y].ATB = next_char;
                        next_char++;
                    };
            }
            catch { };

            STT0.Text = String.Format("{0}",p);
            STT1.Text = "placemark(s) in";
            STT2.Text = String.Format("{0}", kmzLayers.Items.Count);
            STT3.Text = "layers(s)";
            STT4.Text = String.Format("{0} placemark(s) in {1} checked layer(s)", s, c);
            STT5.Text = String.Format("{0} layers unchecked", kmzLayers.Items.Count - c);
            STT6.Text = String.Format("{0} layers is empty", e);

            if (fullUpdate)
                kmzLayers.Visible = true;            
        }        

        private void ReloadXMLOnly_NoUpdateLayers()
        {
            if (kmzFiles.Items.Count == 0) return;
            for (int i = 0; i < kmzFiles.Items.Count; i++)
            {
                KMFile f = (KMFile)kmzFiles.Items[i];
                f.LoadKML(false);
            };
        }	

        private void kmzLayers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            KMLayer l = (KMLayer)kmzLayers.Items[e.Index];
            l.ischeck = e.NewValue == CheckState.Checked;
            ReloadListboxLayers(false);
        }

        private void kmzFiles_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            KMFile f = (KMFile)kmzFiles.Items[e.Index];
            f.isCheck = e.NewValue == CheckState.Checked;
            ReloadListboxLayers(true);
        }

        private void DeleteSelected_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedItems.Count == 0) return;
            for (int i = kmzFiles.SelectedIndices.Count - 1; i >= 0; i--)
            {
                KMFile f = (KMFile)kmzFiles.Items[kmzFiles.SelectedIndices[i]];
                System.IO.Directory.Delete(f.tmp_file_dir,true);
                kmzFiles.Items.RemoveAt(kmzFiles.SelectedIndices[i]);
            };
            ReloadListboxLayers(true);
        }

        private void DeleteAll_Click(object sender, EventArgs e)
        {
            kmzFiles.Items.Clear();
            try
            {
                if (Directory.Exists(TempDirectory())) System.IO.Directory.Delete(TempDirectory(), true);
            }
            catch { };
            ReloadListboxLayers(true);
        }

        private void DeleteChecked_Click(object sender, EventArgs e)
        {
            if (kmzFiles.CheckedItems.Count == 0) return;
            for (int i = kmzFiles.CheckedItems.Count - 1; i >= 0; i--)
            {
                KMFile f = (KMFile)kmzFiles.CheckedItems[i];
                System.IO.Directory.Delete(f.tmp_file_dir,true);
                kmzFiles.Items.Remove(kmzFiles.CheckedItems[i]);
            };
            ReloadListboxLayers(true);
        }        

        private void Save2KML(string filename, KMLayer kml)
        {
            log.Text = "";
            AddToLog("Creating single layer KML file for layer: `" + kml.name + "`...");

            waitBox.Show("Saving", "Wait, saving file...");

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
            sw.WriteLine("<kml xmlns='http://www.opengis.net/kml/2.2'><Document>");
            sw.WriteLine("<name>" + kml.name + "</name>");
            sw.WriteLine("<createdby>" + this.Text + "</createdby>");

            AddToLog(String.Format("Saving data to selected file: {0}", filename));

            List<KMStyle> styles = new List<KMStyle>();
            // collect all styles for layer
            {
                XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
                XmlNodeList xns = xn.SelectNodes("Placemark/styleUrl");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        string stname = xns[x].ChildNodes[0].Value;
                        if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
                        KMStyle kms = new KMStyle(stname, kml.file, "");
                        bool skip = false;
                        foreach (KMStyle get in styles) if (get.ToString() == kms.ToString()) { skip = true; };
                        if (!skip) styles.Add(kms);
                    };
                sw.WriteLine(xn.OuterXml); //Write As Is
            };            

            // select all styles for layer
            string style_history = "";
            foreach (KMStyle kms in styles)
            {
                // HISTORY
                {
                    string was_name = kms.name;
                    XmlNode xh = kml.file.kmlDoc.SelectSingleNode("kml/Document/style_history");
                    if (xh != null)
                    {
                        string sh = xh.InnerText;
                        if (!String.IsNullOrEmpty(sh))
                        {
                            string[] history = sh.Split(new char[] { ';' });
                            foreach (string h in history)
                            {
                                string[] was = h.Split(new char[] { '=' });
                                if (kms.name == was[0]) was_name = was[1];
                            };
                        };
                    };
                    if (style_history.Length > 0) style_history += ";";
                    style_history += kms.newname + "=" + was_name;
                };
                List<KMStyle> tmps = new List<KMStyle>();
                XmlNode xn = kms.file.kmlDoc.SelectSingleNode("kml/Document/StyleMap[@id='" + kms.name + "']");

                if (xn == null)
                    tmps.Add(kms);
                else
                {
                    foreach (XmlNode xn2 in xn.SelectNodes("Pair/styleUrl"))
                    {
                        string su = xn2.ChildNodes[0].Value;
                        if (su.IndexOf("#") == 0) su = su.Remove(0, 1);
                        tmps.Add(new KMStyle(su, kms.file, ""));
                    };
                    sw.WriteLine(xn.OuterXml); //Write As Is
                };

                foreach (KMStyle k2 in tmps)
                {
                    xn = k2.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + k2.name + "']");
                    if(xn != null) sw.WriteLine(xn.OuterXml); //Write As Is
                };
            };
            sw.WriteLine("<style_history>" + style_history + "</style_history>");

            sw.WriteLine("</Document></kml>");
            sw.Close();
            fs.Close();

            waitBox.Hide();
            AddToLog("Done");
        }

        private void Save2GPX(string filename, KMLayer kml, bool writeWPT, bool writeRTE)
        {
            log.Text = "";
            AddToLog("Creating GPX file for layer: `" + kml.name + "`...");

            waitBox.Show("Saving", "Wait, savining file...");

            XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
            XmlNodeList xnp = xn.SelectNodes("Placemark/Point/coordinates");
            XmlNodeList xnl = xn.SelectNodes("Placemark/LineString/coordinates");
            if ((xnp.Count > 0) || (xnl.Count > 0))
            {
                System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" creator=\"" + this.Text + "\" version=\"1.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd\">");
                if (xnp.Count > 0) 
                {
                    if (writeWPT)
                    {
                        AddToLog("Writing points...");
                        for (int x = 0; x < xnp.Count; x++)
                        {
                            string[] llz = xnp[x].ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            string name = xnp[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value;
                            string description = "";
                            try { description = xnp[x].ParentNode.ParentNode.SelectSingleNode("description").ChildNodes[0].Value; }
                            catch { };
                            sw.WriteLine("\t<wpt lat=\"" + llz[1] + "\" lon=\"" + llz[0] + "\">");
                            sw.WriteLine("\t\t<name>" + name + "</name>");
                            sw.WriteLine("\t\t<desc><![CDATA[" + description + "]]></desc>");
                            sw.WriteLine("\t</wpt>");
                        };
                    };
                }
                if (xnl.Count > 0)
                {
                    if (writeRTE)
                    {
                        AddToLog("Writing lines...");
                        for (int x = 0; x < xnl.Count; x++)
                        {
                            string[] llza = xnl[x].ChildNodes[0].Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            string name = xnl[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value;
                            string description = "";
                            try { description = xnl[x].ParentNode.ParentNode.SelectSingleNode("description").ChildNodes[0].Value; }
                            catch { };
                            sw.WriteLine("\t<rte>");
                            sw.WriteLine("\t\t<name>" + name + "</name>");
                            sw.WriteLine("\t\t<desc><![CDATA[" + description + "]]></desc>");
                            foreach (string llzix in llza)
                            {
                                string llzi = llzix.Trim('\r').Trim('\n');
                                if (String.IsNullOrEmpty(llzi)) continue;
                                string[] llz = llzi.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                sw.WriteLine("\t\t<rtept lat=\"" + llz[1] + "\" lon=\"" + llz[0] + "\"/>");
                            };
                            sw.WriteLine("\t</rte>");
                        };
                    }
                };
                if(writeRTE && writeWPT)
                    AddToLog("Saved " + xnp.Count.ToString() + " points and " + xnl.Count.ToString() + " lines");
                else if (writeWPT)
                    AddToLog("Saved " + xnp.Count.ToString() + " points");
                else
                    AddToLog("Saved " + xnl.Count.ToString() + " lines");
                AddToLog(String.Format("Saving data to selected file: {0}", filename));
                sw.WriteLine("</gpx>");
                sw.Close();
                fs.Close();

                waitBox.Hide();
                AddToLog("Done");
                return;
            };

            waitBox.Hide();
            AddToLog("File not created: Layer has no placemarks to save in gpx format!");
            MessageBox.Show("Layer has no placemarks to save in gpx format!", "File not created", MessageBoxButtons.OK, MessageBoxIcon.Information);            
        }

        private void Save2WPT(string filename, KMLayer kml)
        {
            log.Text = "";
            AddToLog("Creating WPT file for layer: `" + kml.name + "`...");

            waitBox.Show("Saving", "Wait, savining file...");
            
            
            XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
            XmlNodeList xns = xn.SelectNodes("Placemark/Point/coordinates");
            if (xns.Count > 0)
            {
                AddToLog("Writing points...");
                List<string> styles = new List<string>();                
                List<WPTPOI> poi = new List<WPTPOI>();
                for (int x = 0; x < xns.Count; x++)
                {
                    string[] llz = xns[x].ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    string name = xns[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value.Replace(",", ";");
                    int icon = 0;
                    XmlNode stn = xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                    if ((stn != null) && (stn.ChildNodes.Count > 0))
                    {
                        string style = stn.ChildNodes[0].Value;
                        if (styles.IndexOf(style) < 0) styles.Add(style);
                        icon = styles.IndexOf(style);
                    };
                    string desc = "";
                    XmlNode std = xns[x].ParentNode.ParentNode.SelectSingleNode("description");
                    if ((std != null) && (std.ChildNodes.Count > 0))
                        desc = std.ChildNodes[0].Value;

                    bool toTop = false;
                    if (!String.IsNullOrEmpty(desc))
                    {
                        string dtl = desc.ToLower();
                        if (dtl.IndexOf("progorod_dat_home=yes") >= 0) toTop = true;
                        if (dtl.IndexOf("progorod_dat_home=1") >= 0) toTop = true;
                        if (dtl.IndexOf("progorod_dat_home=true") >= 0) toTop = true;
                        if (dtl.IndexOf("progorod_dat_office=yes") >= 0) toTop = true;
                        if (dtl.IndexOf("progorod_dat_office=1") >= 0) toTop = true;
                        if (dtl.IndexOf("progorod_dat_office=true") >= 0) toTop = true;
                        dtl = (new Regex(@"[\w]+=[\S\s][^\r\n]+")).Replace(dtl, ""); // Remove TAGS
                    };
                    
                    WPTPOI p = new WPTPOI();
                    p.Name = name;
                    p.Description = desc;
                    p.Latitude = double.Parse(llz[1].Replace("\r", "").Replace("\n", "").Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture);
                    p.Longitude = double.Parse(llz[0].Replace("\r", "").Replace("\n", "").Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture);
                    p.Symbol = icon;
                    if (toTop)
                        poi.Insert(0, p);
                    else
                        poi.Add(p);
                };
                WPTPOI.WriteFile(filename, poi.ToArray(), this.Text);

                AddToLog("Saved " + xns.Count.ToString() + " points");
                AddToLog(String.Format("Saving data to selected file: {0}", filename));

                waitBox.Hide();
                AddToLog("Done");
                return;
            };

            waitBox.Hide();
            AddToLog("File not created: Layer has no placemarks to save in wpt format!");
            MessageBox.Show("Layer has no placemarks to save in wpt format!", "File not created", MessageBoxButtons.OK, MessageBoxIcon.Information);            
        }

        public override void Refresh()
        {
            ReloadListboxLayers(false);
            try
            {
                base.Refresh();
            }
            catch { };
        }

        private void Save2ReportCSV(string filename, IDictionary<string,string[]> fields)
        {
            log.Text = "";

            waitBox.Show("Saving", "Wait, saving report...");

            AddToLog("Create CSV Report: " +Path.GetFileName(filename));

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine(outName.Text);
            sw.WriteLine("Created by " + this.Text);
            foreach (KeyValuePair<string, string[]> h in fields)
                sw.Write(h.Key.Replace("\t", " ") + "\t");
            sw.WriteLine();

            int ttlpm = 0;
            int layers = 0;
            for (int i = 0; i < kmzLayers.CheckedItems.Count; i++)
            {
                KMLayer kml = (KMLayer)kmzLayers.CheckedItems[i];
                XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
                XmlNodeList xns = xn.SelectNodes("Placemark/Point/coordinates");
                if (xns.Count > 0)
                {
                    layers++;
                    for (int x = 0; x < xns.Count; x++)
                    {
                        ttlpm++;
                        string[] llz = xns[x].ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        string name = xns[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value.Replace(",", ";");
                        string desc = "";
                        XmlNode std = xns[x].ParentNode.ParentNode.SelectSingleNode("description");
                        if ((std != null) && (std.ChildNodes.Count > 0))
                            desc = std.ChildNodes[0].Value;
                        string lat = llz[1].Replace("\r", "").Replace("\n", "").Replace(" ", "");
                        string lon = llz[0].Replace("\r", "").Replace("\n", "").Replace(" ", "");

                        foreach (KeyValuePair<string, string[]> h in fields)
                        {
                            string value = h.Value[0].Replace("{layer}", kml.name).Replace("{name}", name).Replace("{latitude}", lat).Replace("{longitude}", lon).Replace("{description}", desc);
                            if (!String.IsNullOrEmpty(h.Value[1]))
                            {
                                Regex rx = new Regex(h.Value[1]);
                                Match mc = rx.Match(value);
                                if (mc.Success)
                                    value = mc.Groups[1].Value;
                                else
                                    value = "";
                            };
                            sw.Write(value.Replace("\t", " ") + "\t");
                        };
                        sw.WriteLine();
                    };                    
                };                
            };
            sw.WriteLine(String.Format("Report {1} placemarks in {0} layer(s)", layers, ttlpm));
            sw.Close();
            fs.Close();

            AddToLog(String.Format("Report {1} placemarks in {0} layer(s)", layers, ttlpm));
            AddToLog("Done");
            waitBox.Hide();
        }

        private void Save2ReportHTML(string filename, IDictionary<string, string[]> fields)
        {
            log.Text = "";

            waitBox.Show("Saving", "Wait, saving report...");

            AddToLog("Create HTML Report: " + Path.GetFileName(filename));

            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<html>");
            sw.WriteLine("<head>");
            sw.WriteLine("<title>"+outName.Text+"</title>");
            sw.WriteLine("</head><body>");
            sw.WriteLine("<h1>" + outName.Text + "</h1>");            
            sw.WriteLine("<table border=\"1\" cellpadding=\"2\" cellspacing=\"1\">");
            sw.WriteLine("<tr>");
            foreach (KeyValuePair<string, string[]> h in fields)
                sw.WriteLine("<td><b>" + h.Key + "</b></td>");
            sw.WriteLine("</tr>");

            int ttlpm = 0;
            int layers = 0;
            for (int i = 0; i < kmzLayers.CheckedItems.Count; i++)
            {
                KMLayer kml = (KMLayer)kmzLayers.CheckedItems[i];
                XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
                XmlNodeList xns = xn.SelectNodes("Placemark/Point/coordinates");
                if (xns.Count > 0)
                {
                    layers++;
                    for (int x = 0; x < xns.Count; x++)
                    {
                        ttlpm++;
                        string[] llz = xns[x].ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        string name = xns[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value.Replace(",", ";");
                        string desc = "";
                        XmlNode std = xns[x].ParentNode.ParentNode.SelectSingleNode("description");
                        if ((std != null) && (std.ChildNodes.Count > 0))
                            desc = std.ChildNodes[0].Value;
                        string lat = llz[1].Replace("\r", "").Replace("\n", "").Replace(" ", "");
                        string lon = llz[0].Replace("\r", "").Replace("\n", "").Replace(" ", "");

                        sw.WriteLine("<tr>");
                        foreach (KeyValuePair<string, string[]> h in fields)
                        {
                            sw.Write("<td>");
                            string value = h.Value[0].Replace("{layer}", kml.name).Replace("{name}", name).Replace("{latitude}", lat).Replace("{longitude}", lon).Replace("{description}", desc);
                            if (!String.IsNullOrEmpty(h.Value[1]))
                            {
                                Regex rx = new Regex(h.Value[1]);
                                Match mc = rx.Match(value);
                                if (mc.Success)
                                    value = mc.Groups[1].Value;
                                else
                                    value = "";
                            };
                            sw.Write(value);
                            sw.Write("</td>");
                        };
                        sw.WriteLine("</tr>");
                    };
                };
            };
            sw.WriteLine("</table>");
            sw.WriteLine("<div>" + String.Format("Report {1} placemarks in {0} layer(s)", layers, ttlpm) + "</div>");
            sw.WriteLine("<div>Created by " + this.Text + "</div>");
            sw.WriteLine("</body></html>");

            AddToLog(String.Format("Report {1} placemarks in {0} layer(s)", layers, ttlpm));
            AddToLog("Done");
            waitBox.Hide();
        }

        private string Save2KMZ(string filename, bool multilayers)
        {
            return Save2KMZ(filename, multilayers, true);
        }

        private string Save2KMZ(string filename, bool multilayers, bool createArchive)
        {
            log.Text = "";           

            waitBox.Show("Saving", "Wait, saving file...");

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random random = (new Random());
            string pref = new String(new char[] { chars[random.Next(chars.Length)], chars[random.Next(chars.Length)], chars[random.Next(chars.Length)] });            

            string zdir = KMZRebuilederForm.TempDirectory() + "OF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            System.IO.Directory.CreateDirectory(zdir);
            System.IO.Directory.CreateDirectory(zdir + @"\images\");
            AddToLog("Creating "+(multilayers ? "multi" : "single")+" layer KMZ file for selected layers...");
            AddToLog("Create Temp Folder: " + zdir);
            AddToLog("Create KML File: " + zdir + "doc.kml");

            System.IO.FileStream fs = new System.IO.FileStream(zdir + "doc.kml", System.IO.FileMode.Create, System.IO.FileAccess.Write);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
            sw.WriteLine("<kml xmlns='http://www.opengis.net/kml/2.2'><Document>");
            sw.WriteLine("<name>" + outName.Text + "</name>");
            sw.WriteLine("<createdby>" + this.Text + "</createdby>");

            List<KMStyle> styles = new List<KMStyle>();
            List<KMIcon> icons = new List<KMIcon>();

            int layers_found = 0;
            if(!multilayers)
                sw.WriteLine("<Folder><name>" + outName.Text + "</name>");

            int ttlpm = 0;
            for (int i = 0; i < kmzLayers.CheckedItems.Count; i++)
            {
                KMLayer kml = (KMLayer)kmzLayers.CheckedItems[i];
                CopySounds(kml.file, zdir);
                XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];

                ttlpm += kml.placemarks;

                // remove names
                if (!multilayers)
                {
                    XmlNode nn = xn.SelectSingleNode("name");
                    if (nn != null) xn.RemoveChild(nn);
                    nn = xn.SelectSingleNode("description");
                    if (nn != null) xn.RemoveChild(nn);
                };                

                // styles
                {
                    XmlNodeList xns = xn.SelectNodes("Placemark/styleUrl");
                    if (xns.Count > 0)
                        for (int x = 0; x < xns.Count; x++)
                        {
                            string stname = xns[x].ChildNodes[0].Value;
                            if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
                            KMStyle kms = new KMStyle(stname, kml.file, "style" + pref + styles.Count.ToString());
                            bool skip = false;
                            foreach (KMStyle get in styles) if (get.ToString() == kms.ToString()) { skip = true; kms.newname = get.newname; };
                            if (!skip) styles.Add(kms);
                            xns[x].ChildNodes[0].Value = "#" + kms.newname;
                        };
                };

                if (multilayers)
                    sw.WriteLine(xn.OuterXml);
                else
                    sw.WriteLine(xn.InnerXml);
                layers_found++;
            };
            if (!multilayers)
                sw.WriteLine("</Folder>");

            AddToLog(String.Format("Found {2} placemarks with {0} original styles in {1} layers", styles.Count, layers_found, ttlpm));
            Refresh();

            //styles
            int style_found = 0;
            int style_maps_found = 0;
            int icons_found = 0;
            int icons_added = 0;
            int icons_passed = 0;
            int icons_asURL = 0;
            string style_history = "";
            foreach (KMStyle kms in styles)
            {                
                // HISTORY
                {                    
                    string was_name = kms.name;                    
                    KMLayer kml = (KMLayer)kmzLayers.CheckedItems[0];
                    XmlNode xh = kml.file.kmlDoc.SelectSingleNode("kml/Document/style_history");
                    if (xh != null)
                    {
                        string sh = xh.InnerText;
                        if (!String.IsNullOrEmpty(sh))
                        {
                            string[] history = sh.Split(new char[] { ';' });
                            foreach (string h in history)
                            {
                                string[] was = h.Split(new char[] { '=' });
                                if (kms.name == was[0]) was_name = was[1];
                            };
                        };
                    };
                    if (style_history.Length > 0) style_history += ";";
                    style_history += kms.newname + "=" + was_name;
                };
                List<KMStyle> tmps = new List<KMStyle>();
                XmlNode xn = kms.file.kmlDoc.SelectSingleNode("kml/Document/StyleMap[@id='" + kms.name + "']");

                if (xn == null)
                    tmps.Add(kms);
                else
                {
                    xn.Attributes["id"].Value = kms.newname;
                    int cnc = 0;
                    foreach (XmlNode xn2 in xn.SelectNodes("Pair/styleUrl"))
                    {
                        string su = xn2.ChildNodes[0].Value;
                        if (su.IndexOf("#") == 0) su = su.Remove(0, 1);
                        string sunn = kms.newname + "-" + cnc.ToString();
                        xn2.ChildNodes[0].Value = "#" + sunn;
                        tmps.Add(new KMStyle(su, kms.file, sunn));
                        cnc++;
                    };
                    sw.WriteLine(xn.OuterXml);
                    style_maps_found++;
                };

                foreach (KMStyle k2 in tmps)
                {
                    xn = k2.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + k2.name + "']");
                    if (xn != null)
                    {
                        xn.Attributes["id"].Value = k2.newname;
                        XmlNode xn2 = xn.SelectSingleNode("IconStyle/Icon/href");
                        if (xn2 != null)
                        {
                            string href = xn2.ChildNodes[0].Value;
                            string ext = System.IO.Path.GetExtension(href).ToLower();
                            string newhref = "images/" + k2.newname + ext;
                            bool isurl = Uri.IsWellFormedUriString(href, UriKind.Absolute);
                            if (isurl) newhref = href;
                            KMIcon ki = new KMIcon(href, k2.file, newhref);
                            bool skip = false;
                            foreach (KMIcon get in icons) if (get.ToString() == ki.ToString()) { skip = true; ki.newhref = get.newhref; };
                            if (!skip)
                            {
                                icons_found++;
                                if (!isurl)
                                {
                                    if (File.Exists(k2.file.tmp_file_dir + ki.href))
                                    {
                                        System.IO.File.Copy(k2.file.tmp_file_dir + ki.href, zdir + ki.newhref);
                                        icons_added++;
                                    }
                                    else
                                    {
                                        AddToLog("Error: File not found: " + k2.file.tmp_file_dir + ki.href);
                                        icons_passed++;
                                    };
                                }
                                else
                                {
                                    icons_asURL++;
                                    if (saveURLIcons.Checked)
                                    {
                                        newhref = "images/" + k2.newname + ext;
                                        ki.newhref = newhref;
                                        try
                                        {
                                            GrabImage(href, zdir + ki.newhref);
                                            icons_added++;
                                        }
                                        catch (Exception ex)
                                        {
                                            AddToLog("Error downloading " + k2.newname + " icon at `" + href + "` - " + ex.Message);
                                            icons_passed++;
                                        };
                                    };
                                };
                                icons.Add(ki);
                            };
                            xn2.ChildNodes[0].Value = ki.newhref;
                        };
                        xn2 = xn.SelectSingleNode("BalloonStyle");
                        if (xn2 != null) xn.RemoveChild(xn2);
                        sw.WriteLine(xn.OuterXml);
                        style_found++;
                    };
                };
            };
            sw.WriteLine("<style_history>" + style_history + "</style_history>");

            AddToLog(String.Format("Found {0} icons, {1} saved, {2} passed, {3} in URLS", icons_found, icons_added, icons_passed, icons_asURL));
            AddToLog(String.Format("Saved {3} placemarks, {0} styles and {1} style maps with {4} icons in {2} layer(s)", style_found, style_maps_found, multilayers ? layers_found : 1, ttlpm, icons_added));
            
            sw.WriteLine("</Document></kml>");
            sw.Close();
            fs.Close();

            if (createArchive)
            {
                AddToLog(String.Format("Creating file: {0}", filename));
                CreateZIP(filename, zdir);
            };
            waitBox.Hide();

            return zdir;
        }

        private void CopySounds(KMFile kmf, string zdir)
        {
            // SOUNDS
            {
                string pth = Path.Combine(kmf.tmp_file_dir, "sounds");
                if (Directory.Exists(pth))
                {
                    System.IO.Directory.CreateDirectory(zdir + @"\sounds\");
                    string[] files = Directory.GetFiles(pth, "*.*", SearchOption.AllDirectories);
                    foreach (string ff in files)
                    {
                        string pto = Path.Combine(zdir + @"\sounds\", GetRelativePath(ff, pth));
                        Directory.CreateDirectory(Path.GetDirectoryName(pto));
                        File.Copy(ff, pto, true);
                    };
                };
            };
            // MEDIA
            {
                string pth = Path.Combine(kmf.tmp_file_dir, "media");
                if (Directory.Exists(pth))
                {
                    System.IO.Directory.CreateDirectory(zdir + @"\media\");
                    string[] files = Directory.GetFiles(pth, "*.*", SearchOption.AllDirectories);
                    foreach (string ff in files)
                    {
                        string pto = Path.Combine(zdir + @"\media\", GetRelativePath(ff, pth));
                        Directory.CreateDirectory(Path.GetDirectoryName(pto));
                        File.Copy(ff, pto, true);
                    };
                };
            };
        }       

        private string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
                folder += Path.DirectorySeparatorChar;
            Uri folderUri = new Uri(folder);
            return folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar);
        }

        private void Save2SplittedIcons(string filename, KMLayer layer)
        {
            log.Text = "";

            waitBox.Show("Saving", "Wait, splitting layer...");

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random random = (new Random());
            string pref = new String(new char[] { chars[random.Next(chars.Length)], chars[random.Next(chars.Length)], chars[random.Next(chars.Length)] });

            string zdir = KMZRebuilederForm.TempDirectory() + "OF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            System.IO.Directory.CreateDirectory(zdir);
            System.IO.Directory.CreateDirectory(zdir + @"\images\");
            AddToLog("Creating multi layer KMZ file for layer: `"+layer.name+"`...");
            AddToLog("Create Temp Folder: " + zdir);
            AddToLog("Creating KML File: " + zdir + "doc.kml");

            System.IO.FileStream fs = new System.IO.FileStream(zdir + "doc.kml", System.IO.FileMode.Create, System.IO.FileAccess.Write);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
            sw.WriteLine("<kml xmlns='http://www.opengis.net/kml/2.2'><Document>");
            sw.WriteLine("<name>" + layer.name + "</name>");
            sw.WriteLine("<createdby>" + this.Text + "</createdby>");

            List<KMIcon> icons = new List<KMIcon>();            
            int icons_added = 0;
            int icons_passed = 0;
            int icons_asURL = 0;
            
            // collect all icons in kml // no changes
            waitBox.Text = "Collecting all icons in kml...";
            {
                KMLayer kml = layer;
                XmlNodeList xns = kml.file.kmlDoc.SelectNodes("kml/Document/Style/IconStyle/Icon/href");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        XmlNode xn2 = xns[x];
                        string style = xn2.ParentNode.ParentNode.ParentNode.Attributes["id"].Value;
                        string href = xn2.ChildNodes[0].Value;
                        
                        bool isurl = Uri.IsWellFormedUriString(href, UriKind.Absolute);
                        KMIcon ki = new KMIcon(href, layer.file, href, style);
                        Image img = null;
                        
                        if (!isurl)
                        {
                            try
                            {
                                img = Image.FromFile(layer.file.tmp_file_dir + href);
                                ki.image = (Image)new Bitmap(img);
                            }
                            catch { };
                        }
                        else
                        {
                            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(ki.href);
                            try
                            {
                                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
                                using (Stream stream = response.GetResponseStream())
                                    img = Bitmap.FromStream(stream);
                                ki.image = (Image)new Bitmap(img);
                            }
                            catch
                            {
                            };
                        };                        
                        if(img != null) img.Dispose();

                        bool skip = false;
                        foreach (KMIcon get in icons)
                        {
                            if (get.ToString() == ki.ToString())
                            {
                                skip = true;
                                get.styles.Add(style);
                                break;
                            };
                            
                            if((ki.image != null) && (get.image != null))
                                if (CompareMemCmp((Bitmap)ki.image, (Bitmap)get.image))
                                {
                                    skip = true;
                                    get.styles.Add(style);
                                    break;
                                };
                        };
                        if (!skip) icons.Add(ki);                            
                    };
            };

            // collect all style maps in kml // no changes
            waitBox.Text = "Collecting all style maps in kml...";
            {
                KMLayer kml = layer;
                XmlNodeList xns = layer.file.kmlDoc.SelectNodes("kml/Document/StyleMap");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        string style = xns[x].Attributes["id"].Value;
                        foreach (XmlNode xn2 in xns[x].SelectNodes("Pair/styleUrl"))
                        {
                            string su = xn2.ChildNodes[0].Value;
                            if (su.IndexOf("#") == 0) su = su.Remove(0, 1);
                            foreach (KMIcon ki in icons)
                                if (ki.styles.IndexOf(su) >= 0)
                                    ki.styles.Add(style);
                        };
                    };
            };

            
            // collect all placemarks name and styles in layer // no changes
            List<XmlNode> nostyle = new List<XmlNode>();
            int ttl_objs = 0;
            int objs_w_icons = 0;
            waitBox.Text = "Collecting all placemarks name and styles in layer kml...";
            {
                KMLayer kml = layer;
                XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
                XmlNodeList xns = xn.SelectNodes("Placemark");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        ttl_objs++;
                        string nam = xns[x].SelectSingleNode("name").ChildNodes[0].Value;
                        XmlNode nsm = xns[x].SelectSingleNode("styleUrl");
                        if (nsm != null)
                        {
                            string stname = nsm.ChildNodes[0].Value;
                            if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
                            bool ex = false;
                            foreach (KMIcon ic in icons)
                                if (ic.styles.IndexOf(stname) >= 0)
                                {
                                    objs_w_icons++;
                                    ex = true;
                                    ic.placemarks++;
                                    if (ic.lcs == null)
                                        ic.lcs = nam;
                                    else
                                        ic.lcs = LCS(ic.lcs, nam);
                                };
                            if (!ex) nostyle.Add(xns[x]);
                        }
                        else nostyle.Add(xns[x]);
                    };
            };

            // delete empty styles // no changes
            waitBox.Text = "Copying layer icons...";
            for (int i = icons.Count - 1; i >= 0; i--)
                if (icons[i].placemarks == 0)
                    icons.RemoveAt(i);
                else
                {
                    KMIcon ki = icons[i];
                    bool isurl = Uri.IsWellFormedUriString(ki.href, UriKind.Absolute);
                    if (!isurl)
                    {
                        if (File.Exists(layer.file.tmp_file_dir + ki.href))
                        {
                            System.IO.File.Copy(layer.file.tmp_file_dir + ki.href, zdir + ki.href);                            
                            icons_added++;
                        }
                        else
                        {
                            AddToLog("Error: File not found: " + layer.file.tmp_file_dir + ki.href);
                            icons_passed++;
                        };
                    }
                    else
                    {
                        icons_asURL++;
                    };

                };


            AddToLog(String.Format("Found {5} placemarks, {0} icons for {4} placemarks, {6} placemarks with no icons, {1} icons saved, {2} passed, {3} in URLS",
                icons.Count, icons_added, icons_passed, icons_asURL, objs_w_icons, ttl_objs, nostyle.Count));
            Refresh();

            if (nostyle.Count > 0)
            {
                KMIcon kmi = new KMIcon("***", layer.file, "***");
                kmi.placemarks = nostyle.Count;
                kmi.lcs = "No icons";
                icons.Insert(0, kmi);
            };

            // rename layers window
            LayersRenamerForm sl = new LayersRenamerForm();
            for (int i = 0; i < icons.Count; i++)
            {
                if (icons[i].image == null)
                    sl.images.Images.Add(new Bitmap(16, 16));
                else
                    sl.images.Images.Add(icons[i].image);
                icons[i].lcs = icons[i].lcs.Trim();
                if (icons[i].lcs == String.Empty) icons[i].lcs = layer.name + ", Noname " + i.ToString();
                icons[i].lcs += " [" + icons[i].placemarks.ToString() + "]";
                sl.layers.Items.Add(i.ToString() + ": " + icons[i].lcs, i);
            };
            waitBox.Hide();
            if (sl.ShowDialog() == DialogResult.OK)
                for (int i = 0; i < icons.Count; i++)
                {
                    string txti = i.ToString()+": ";
                    icons[i].lcs = sl.layers.Items[i].Text.Remove(0,txti.Length);
                };
            sl.Dispose();
            waitBox.Show("Saving", "Wait, saving file...");


            // write layers to file
            waitBox.Text = "Writing layers to file...";            
            if (nostyle.Count > 0)
            {
                sw.WriteLine("<Folder><name>" + icons[0].lcs + "</name>");
                for (int i = 0; i < nostyle.Count; i++)
                    sw.WriteLine(nostyle[i].OuterXml);
                sw.WriteLine("</Folder>");
            };
            int splCo = 0;
            if (icons.Count > 0)
                for (int i = (nostyle.Count > 0 ? 1 : 0); i < icons.Count; i++)
                {
                    sw.WriteLine("<Folder><name>" + icons[i].lcs + "</name>");
                    KMLayer kml = layer;
                    XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
                    XmlNodeList xns = xn.SelectNodes("Placemark");
                    if (xns.Count > 0)
                        for (int x = 0; x < xns.Count; x++)
                        {
                            string nam = xns[x].SelectSingleNode("name").ChildNodes[0].Value;
                            XmlNode nsm = xns[x].SelectSingleNode("styleUrl");
                            if (nsm != null)
                            {
                                string stname = nsm.ChildNodes[0].Value;
                                if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
                                if (icons[i].styles.IndexOf(stname) >= 0)
                                {
                                    nsm.ChildNodes[0].Value = "#" + icons[i].styles[0].Replace("-","A");
                                    sw.WriteLine(xns[x].OuterXml);
                                    splCo++;
                                };
                            };
                        };
                    sw.WriteLine("</Folder>");
                };
            
            // write styles
            waitBox.Text = "Writing styles to file...";
            for (int i = (nostyle.Count > 0 ? 1 : 0); i < icons.Count; i++)
            {
                KMLayer kml = layer;
                XmlNode xn = layer.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='"+icons[i].styles[0]+"']");
                if (xn == null) xn = layer.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + icons[i].styles[1] + "']");
                xn.Attributes["id"].Value = icons[i].styles[0].Replace("-", "A");
                sw.WriteLine(xn.OuterXml);
            };            

            AddToLog(String.Format("Saved {0} placemarks in {1} layers", splCo + nostyle.Count, icons.Count));
            Refresh();

            sw.WriteLine("</Document></kml>");
            sw.Close();
            fs.Close();

            AddToLog(String.Format("Saving data to selected file: {0}", filename));
            waitBox.Text = "Saving output file...";
            CreateZIP(filename, zdir);
            waitBox.Hide();
            AddToLog("Done");
        }

        private void Save2SplittedNames(string filename, KMLayer layer, byte mode, string[] findText)
        {
            log.Text = "";

            waitBox.Show("Saving", "Wait, splitting layer...");

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random random = (new Random());
            string pref = new String(new char[] { chars[random.Next(chars.Length)], chars[random.Next(chars.Length)], chars[random.Next(chars.Length)] });

            string zdir = KMZRebuilederForm.TempDirectory() + "OF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            System.IO.Directory.CreateDirectory(zdir);
            System.IO.Directory.CreateDirectory(zdir + @"\images\");
            AddToLog("Creating multi layer KMZ file for layer: `" + layer.name + "`...");
            AddToLog("Create Temp Folder: " + zdir);
            AddToLog("Creating KML File: " + zdir + "doc.kml");

            System.IO.FileStream fs = new System.IO.FileStream(zdir + "doc.kml", System.IO.FileMode.Create, System.IO.FileAccess.Write);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
            sw.WriteLine("<kml xmlns='http://www.opengis.net/kml/2.2'><Document>");
            sw.WriteLine("<name>" + layer.name + "</name>");
            sw.WriteLine("<createdby>" + this.Text + "</createdby>");

            List<KMIcon> icons = new List<KMIcon>();
            int icons_added = 0;
            int icons_passed = 0;
            int icons_asURL = 0;

            // collect all icons in kml // no changes
            waitBox.Text = "Collecting all icons in kml...";
            {
                KMLayer kml = layer;
                XmlNodeList xns = kml.file.kmlDoc.SelectNodes("kml/Document/Style/IconStyle/Icon/href");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        XmlNode xn2 = xns[x];
                        string style = xn2.ParentNode.ParentNode.ParentNode.Attributes["id"].Value;
                        string href = xn2.ChildNodes[0].Value;

                        bool isurl = Uri.IsWellFormedUriString(href, UriKind.Absolute);
                        KMIcon ki = new KMIcon(href, layer.file, href, style);
                        Image img = null;

                        if (!isurl)
                        {
                            try
                            {
                                img = Image.FromFile(layer.file.tmp_file_dir + href);
                                ki.image = (Image)new Bitmap(img);
                            }
                            catch { };
                        }
                        else
                        {
                            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(ki.href);
                            try
                            {
                                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
                                using (Stream stream = response.GetResponseStream())
                                    img = Bitmap.FromStream(stream);
                                ki.image = (Image)new Bitmap(img);
                            }
                            catch
                            {
                            };
                        };
                        if (img != null) img.Dispose();

                        bool skip = false;
                        foreach (KMIcon get in icons)
                        {
                            if (get.ToString() == ki.ToString())
                            {
                                skip = true;
                                get.styles.Add(style);
                                break;
                            };

                            if ((ki.image != null) && (get.image != null))
                                if (CompareMemCmp((Bitmap)ki.image, (Bitmap)get.image))
                                {
                                    skip = true;
                                    get.styles.Add(style);
                                    break;
                                };
                        };
                        if (!skip) icons.Add(ki);
                    };
            };

            // collect all style maps in kml // no changes
            waitBox.Text = "Collecting all style maps in kml...";
            {
                KMLayer kml = layer;
                XmlNodeList xns = layer.file.kmlDoc.SelectNodes("kml/Document/StyleMap");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        string style = xns[x].Attributes["id"].Value;
                        foreach (XmlNode xn2 in xns[x].SelectNodes("Pair/styleUrl"))
                        {
                            string su = xn2.ChildNodes[0].Value;
                            if (su.IndexOf("#") == 0) su = su.Remove(0, 1);
                            foreach (KMIcon ki in icons)
                                if (ki.styles.IndexOf(su) >= 0)
                                    ki.styles.Add(style);
                        };
                    };
            };

            List<string> names = new List<string>();

            // collect all placemarks name and styles in layer // no changes
            List<XmlNode> nostyle = new List<XmlNode>();
            int ttl_objs = 0;
            int objs_w_icons = 0;
            waitBox.Text = "Collecting all placemarks name and styles in layer kml...";
            {
                KMLayer kml = layer;
                XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
                XmlNodeList xns = xn.SelectNodes("Placemark");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        ttl_objs++;
                        string nam = xns[x].SelectSingleNode("name").ChildNodes[0].Value;
                        names.Add(nam);
                        XmlNode nsm = xns[x].SelectSingleNode("styleUrl");
                        if (nsm != null)
                        {
                            string stname = nsm.ChildNodes[0].Value;
                            if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
                            bool ex = false;
                            foreach (KMIcon ic in icons)
                                if (ic.styles.IndexOf(stname) >= 0)
                                {
                                    objs_w_icons++;
                                    ex = true;
                                    ic.placemarks++;
                                    if (ic.lcs == null)
                                        ic.lcs = nam;
                                    else
                                        ic.lcs = LCS(ic.lcs, nam);
                                };
                            if (!ex) nostyle.Add(xns[x]);
                        }
                        else nostyle.Add(xns[x]);
                    };
            };

            // FIND
            List<DictionaryEntry> result = new List<DictionaryEntry>();
            if ((findText == null) || (findText.Length == 0))
            {
                if (names.Count > 0)
                {
                    Hashtable maxIn = new Hashtable();
                    foreach (string wts in names)
                    {
                        int[] counts = new int[wts.Length + 1];
                        for (int l = wts.Length; l > 0; l--)
                        {
                            string st = wts.Substring(0, l).ToLower();
                            foreach (string nm2 in names)
                            {
                                if (mode == 0)
                                {
                                    if (nm2.ToLower().IndexOf(st) == 0)
                                        counts[l]++;
                                }
                                else
                                {
                                    if (nm2.ToLower().Contains(st))
                                        counts[l]++;
                                };
                            };

                            if (counts[l] > 1)
                            {
                                bool skip = false;
                                foreach (DictionaryEntry entry in maxIn)
                                {
                                    string en = (string)entry.Key;
                                    if (mode == 0)
                                    {
                                        if ((en.ToLower().IndexOf(st) == 0) && ((int)entry.Value == counts[l]))
                                            skip = true;
                                    }
                                    else
                                    {
                                        if ((en.ToLower().Contains(st)) && ((int)entry.Value == counts[l]))
                                            skip = true;
                                    };
                                };
                                if (!skip)
                                {
                                    if (maxIn[st] != null)
                                    {
                                        int saved = (int)maxIn[st];
                                        if (counts[l] > saved) counts[l] = saved;
                                    };
                                    maxIn[st] = counts[l];
                                };
                            };
                        };
                    };
                    foreach (DictionaryEntry entry in maxIn)
                        result.Add(entry);
                };
            }
            else
            {
                if (names.Count > 0)
                    foreach (string wts in findText)
                    {
                        int counts = 0;
                        foreach (string nm2 in names)
                        {
                            if (mode == 0)
                            {
                                if (nm2.ToLower().IndexOf(wts.ToLower()) == 0)
                                    counts++;
                            }
                            else
                            {
                                if (nm2.ToLower().Contains(wts.ToLower()))
                                    counts++;
                            };
                        };
                        result.Add(new DictionaryEntry(wts, counts));
                    };
            };
            result.Sort(new SMLT());  

            // delete empty styles // no changes
            waitBox.Text = "Copying layer icons...";
            for (int i = icons.Count - 1; i >= 0; i--)
                if (icons[i].placemarks == 0)
                    icons.RemoveAt(i);
                else
                {
                    KMIcon ki = icons[i];
                    bool isurl = Uri.IsWellFormedUriString(ki.href, UriKind.Absolute);
                    if (!isurl)
                    {
                        if (File.Exists(layer.file.tmp_file_dir + ki.href))
                        {
                            System.IO.File.Copy(layer.file.tmp_file_dir + ki.href, zdir + ki.href);
                            icons_added++;
                        }
                        else
                        {
                            AddToLog("Error: File not found: " + layer.file.tmp_file_dir + ki.href);
                            icons_passed++;
                        };
                    }
                    else
                    {
                        icons_asURL++;
                    };

                };


            AddToLog(String.Format("Found {5} placemarks, {0} icons for {4} placemarks, {6} placemarks with no icons, {1} icons saved, {2} passed, {3} in URLS",
                icons.Count, icons_added, icons_passed, icons_asURL, objs_w_icons, ttl_objs, nostyle.Count));
            Refresh();

            if (nostyle.Count > 0)
            {
                KMIcon kmi = new KMIcon("***", layer.file, "***");
                kmi.placemarks = nostyle.Count;
                kmi.lcs = "No icons";
                icons.Insert(0, kmi);
            };

            // rename layers window
            List<string> resultNames = new List<string>();
            LBNRenamerForm sl = new LBNRenamerForm(ttl_objs, result);
            waitBox.Hide();
            DialogResult dr = sl.ShowDialog();
            resultNames = sl.entriesNames;
            if (dr == DialogResult.OK)
                for (int i = sl.layers.Items.Count - 1; i >= 0; i--)
                    if (!sl.layers.CheckedIndices.Contains(i))
                    {
                        sl.layers.Items.RemoveAt(i);
                        result.RemoveAt(i);
                        resultNames.RemoveAt(i);
                    };
            sl.Dispose();
            waitBox.Show("Saving", "Wait, saving file...");


            // write layers to file
            int splCo = 0;
            if (result.Count > 0)
                for (int j = 0; j < result.Count; j++)
                {
                    string nc = result[j].Key.ToString().ToLower();

                    sw.WriteLine("<Folder><name>" + resultNames[j] + "</name>");
                    KMLayer kml = layer;
                    XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
                    XmlNodeList xns = xn.SelectNodes("Placemark");
                    if (xns.Count > 0)
                        for (int x = 0; x < xns.Count; x++)
                        {
                            string nam = xns[x].SelectSingleNode("name").ChildNodes[0].Value.ToLower();
                            bool inside = false;
                            if (mode == 0)
                            {
                                inside = nam.IndexOf(nc) == 0;
                            }
                            else
                            {
                                inside = nam.Contains(nc);
                            };
                            if (inside)
                            {
                                XmlNode nsm = xns[x].SelectSingleNode("styleUrl");
                                if (nsm != null)
                                {
                                    string stname = nsm.ChildNodes[0].Value;
                                    if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
                                    if (icons.Count > 0)
                                        for (int i = 0; i < icons.Count; i++)
                                            if (icons[i].styles.IndexOf(stname) >= 0)
                                            {
                                                nsm.ChildNodes[0].Value = "#" + icons[i].styles[0].Replace("-", "A");
                                                sw.WriteLine(xns[x].OuterXml);
                                                splCo++;
                                            };
                                };
                                xns[x].ParentNode.RemoveChild(xns[x]);
                            };                            
                        };
                    sw.WriteLine("</Folder>");
                };

            // Write All other
            if(splCo < ttl_objs)
            {
                sw.WriteLine("<Folder><name>All other</name>");
                KMLayer kml = layer;
                XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
                XmlNodeList xns = xn.SelectNodes("Placemark");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        string nam = xns[x].SelectSingleNode("name").ChildNodes[0].Value;                        
                        XmlNode nsm = xns[x].SelectSingleNode("styleUrl");
                        if (nsm != null)
                        {
                            string stname = nsm.ChildNodes[0].Value;
                            if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
                            if (icons.Count > 0)
                                for (int i = 0; i < icons.Count; i++)
                                    if (icons[i].styles.IndexOf(stname) >= 0)
                                    {
                                        nsm.ChildNodes[0].Value = "#" + icons[i].styles[0].Replace("-", "A");
                                        sw.WriteLine(xns[x].OuterXml);
                                        splCo++;
                                    };
                        };
                    };
                sw.WriteLine("</Folder>");
            };

            // write styles
            waitBox.Text = "Writing styles to file...";
            for (int i = (nostyle.Count > 0 ? 1 : 0); i < icons.Count; i++)
            {
                KMLayer kml = layer;
                XmlNode xn = layer.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + icons[i].styles[0] + "']");
                if (xn == null) xn = layer.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + icons[i].styles[1] + "']");
                xn.Attributes["id"].Value = icons[i].styles[0].Replace("-", "A");
                sw.WriteLine(xn.OuterXml);
            };

            AddToLog(String.Format("Saved {0} placemarks", splCo + nostyle.Count));
            Refresh();

            sw.WriteLine("</Document></kml>");
            sw.Close();
            fs.Close();

            AddToLog(String.Format("Saving data to selected file: {0}", filename));
            waitBox.Text = "Saving output file...";
            CreateZIP(filename, zdir);
            waitBox.Hide();
            AddToLog("Done");
        }

        public class SMLT: IComparer<DictionaryEntry>
        {
            public int Compare(DictionaryEntry a, DictionaryEntry b)
            {
                string ta = (string)a.Key;
                string tb = (string)b.Key;
                int ia = ta.Length;
                int ib = tb.Length;
                return -1 * ia.CompareTo(ib);
            }
        }

        private void Save2GPIInt(string gpifile, KMFile kmfile)
        {
            string proj_name = kmfile.kmldocName;
            if (proj_name == "") proj_name = outName.Text;
            proj_name = proj_name.Trim();
            if (proj_name == "") proj_name = "KMZRebuilder Data";

            AddToLog("Creating Garmin POI file...");
            GPIWriter gw = new GPIWriter(kmfile.src_file_pth);
            gw.Name = proj_name;
            gw.DataSource = proj_name;
            gw.StoreDescriptions = Properties.GetBoolValue("gpiwriter_set_descriptions");
            gw.StoreAlerts = Properties.GetBoolValue("gpiwriter_set_alerts");
            gw.DefaultAlertIsOn = Properties.GetBoolValue("gpiwriter_default_alert_ison");
            gw.DefaultAlertType = Properties["gpiwriter_default_alert_type"];
            gw.DefaultAlertSound = Properties["gpiwriter_default_alert_sound"];
            
            //POI
            AddToLog("Saving POI...");
            waitBox.Show("Export to GPI", "Wait, saving POIs...");
            int poi_added = 0;
            {
                XmlNodeList nl = kmfile.kmlDoc.SelectNodes("kml/Document/Folder/Placemark/Point/coordinates");
                foreach (XmlNode n in nl)
                {
                    waitBox.Show("Export to GPI", String.Format("Wait, saving POIs {0}/{1} ...", poi_added + 1, nl.Count));

                    string fnam = "";
                    try { fnam = n.ParentNode.ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value; }
                    catch { };

                    string poi = n.ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value;
                    string desc = "";
                    try { desc = n.ParentNode.ParentNode.SelectSingleNode("description").ChildNodes[0].Value; }
                    catch { };
                    string[] ll = n.ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.None);

                    string styleUrl = "";
                    if (n.ParentNode.ParentNode.SelectSingleNode("styleUrl") != null) styleUrl = n.ParentNode.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                    if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);

                    double lat = double.Parse(ll[1].Replace("\r", "").Replace("\n", "").Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture);
                    double lon = double.Parse(ll[0].Replace("\r", "").Replace("\n", "").Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture);
                    gw.AddPOI(fnam, poi, desc, styleUrl, lat, lon);
                    poi_added++;
                };
            };

            AddToLog("Collecting Styles...");
            waitBox.Show("Export to GPI", "Wait, collecting styles...");
            List<KMIcon> icons = new List<KMIcon>();
            int sty_added = 0;
            {
                XmlNodeList xns = kmfile.kmlDoc.SelectNodes("kml/Document/Style/IconStyle/Icon/href");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        waitBox.Show("Export to GPI", String.Format("Wait, collecting styles {0}/{1}...", x + 1, xns.Count));

                        XmlNode xn2 = xns[x];
                        string style = xn2.ParentNode.ParentNode.ParentNode.Attributes["id"].Value;
                        string href = xn2.ChildNodes[0].Value;

                        bool isurl = Uri.IsWellFormedUriString(href, UriKind.Absolute);
                        KMIcon ki = new KMIcon(href, kmfile, href, style);
                        Image img = null;

                        if (!isurl)
                        {
                            try
                            {
                                img = Image.FromFile(kmfile.tmp_file_dir + href);
                                ki.image = (Image)new Bitmap(img);
                            }
                            catch { };
                        }
                        else
                        {
                            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(ki.href);
                            try
                            {
                                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
                                using (Stream stream = response.GetResponseStream())
                                    img = Bitmap.FromStream(stream);
                                ki.image = (Image)new Bitmap(img);
                            }
                            catch
                            {
                            };
                        };
                        if (img != null) img.Dispose();

                        bool skip = false;
                        foreach (KMIcon get in icons)
                        {
                            if (get.ToString() == ki.ToString())
                            {
                                skip = true;
                                get.styles.Add(style); sty_added++;
                                break;
                            };

                            if ((ki.image != null) && (get.image != null))
                                if (CompareMemCmp((Bitmap)ki.image, (Bitmap)get.image))
                                {
                                    skip = true;
                                    get.styles.Add(style); sty_added++;
                                    break;
                                };
                        };
                        if (!skip) { icons.Add(ki); sty_added++; };
                    };
            };

            // STYLE MAPS
            AddToLog("Collecting Style Maps...");
            waitBox.Show("Export to GPI", "Wait, collecting style maps...");
            {
                XmlNodeList xns = kmfile.kmlDoc.SelectNodes("kml/Document/StyleMap");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        waitBox.Show("Export to GPI", String.Format("Wait, collecting style maps {0}/{1}...", x + 1, xns.Count));

                        string style = xns[x].Attributes["id"].Value;
                        foreach (XmlNode xn2 in xns[x].SelectNodes("Pair/styleUrl"))
                        {
                            string su = xn2.ChildNodes[0].Value;
                            if (su.IndexOf("#") == 0) su = su.Remove(0, 1);
                            foreach (KMIcon ki in icons)
                                if (ki.styles.IndexOf(su) >= 0)
                                {
                                    ki.styles.Add(style);
                                    sty_added++;
                                };
                        };
                    };
            };

            // Saving Images
            AddToLog("Collecting Images...");
            waitBox.Show("Export to GPI", "Wait, collecting images...");
            int img_saved = 0;
            {
                for (int i = 0; i < icons.Count; i++)
                {
                    for (int x = 0; x < icons[i].styles.Count; x++)
                    {
                        waitBox.Show("Export to GPI", String.Format("Wait, collecting images {0}/{1}...", img_saved + 1, sty_added));
                        string sty = icons[i].styles[x];
                        gw.AddImage(sty, icons[i].image);
                        img_saved++;
                    };
                };
            };
            AddToLog("Saving...");
            waitBox.Show("Export to GPI", "Wait, saving gpi...");
            gw.Save(gpifile);
            waitBox.Hide();
            AddToLog("Done");
        }

        private void Save2GPI(string gpifile, KMFile kmfile)
        {
            string proj_name = kmfile.kmldocName;
            if (proj_name == "") proj_name = outName.Text;
            proj_name = proj_name.Trim(Path.GetInvalidPathChars());
            if (InputBox("Creating Garmin XML Project", "Enter GPI Project name:", ref proj_name) != DialogResult.OK)
                    return;
            proj_name = proj_name.Trim(Path.GetInvalidPathChars()).Trim();
            if (proj_name == "") return;            
            
            int icons_total = Directory.GetFiles(kmfile.tmp_file_dir+@"\images","*.*").Length;

            waitBox.Show("Saving", "Wait, converting files...");

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = (new Random());
            string group_id = new String(new char[] { chars[random.Next(chars.Length)], chars[random.Next(chars.Length)], chars[random.Next(chars.Length)], chars[random.Next(chars.Length)], chars[random.Next(chars.Length)], chars[random.Next(chars.Length)] });

            string zdir = KMZRebuilederForm.TempDirectory() + "OF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            string gdir = zdir + @"\" + proj_name + @"\";
            System.IO.Directory.CreateDirectory(zdir);
            System.IO.Directory.CreateDirectory(gdir);
            AddToLog("Creating GPX files for GPIGen...");            

            List<KMIcon> icons = new List<KMIcon>();
            Dictionary<string, string> gpx_files = new Dictionary<string, string>();
            
            // collect all icons in kml // no changes
            waitBox.Text = "Collecting icons...";
            {
                XmlNodeList xns = kmfile.kmlDoc.SelectNodes("kml/Document/Style/IconStyle/Icon/href");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        XmlNode xn2 = xns[x];
                        string style = xn2.ParentNode.ParentNode.ParentNode.Attributes["id"].Value;
                        string href = xn2.ChildNodes[0].Value;

                        bool isurl = Uri.IsWellFormedUriString(href, UriKind.Absolute);
                        KMIcon ki = new KMIcon(href, kmfile, href, style);
                        Image img = null;

                        if (!isurl)
                        {
                            try
                            {
                                img = Image.FromFile(kmfile.tmp_file_dir + href);
                                ki.image = (Image)new Bitmap(img);                                
                            }
                            catch { };
                        }
                        else
                        {
                            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(ki.href);
                            try
                            {
                                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
                                using (Stream stream = response.GetResponseStream())
                                    img = Bitmap.FromStream(stream);
                                ki.image = (Image)new Bitmap(img);
                            }
                            catch
                            {
                            };
                        };
                        if (img != null) img.Dispose();

                        bool skip = false;
                        foreach (KMIcon get in icons)
                        {
                            if (get.ToString() == ki.ToString())
                            {
                                skip = true;
                                get.styles.Add(style);
                                break;
                            };

                            if ((ki.image != null) && (get.image != null))
                                if (CompareMemCmp((Bitmap)ki.image, (Bitmap)get.image))
                                {
                                    skip = true;
                                    get.styles.Add(style);
                                    break;
                                };
                        };
                        if (!skip) icons.Add(ki);
                    };
            };

            // collect all style maps in kml // no changes
            waitBox.Text = "Collecting style maps...";
            {
                XmlNodeList xns = kmfile.kmlDoc.SelectNodes("kml/Document/StyleMap");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        string style = xns[x].Attributes["id"].Value;
                        foreach (XmlNode xn2 in xns[x].SelectNodes("Pair/styleUrl"))
                        {
                            string su = xn2.ChildNodes[0].Value;
                            if (su.IndexOf("#") == 0) su = su.Remove(0, 1);
                            foreach (KMIcon ki in icons)
                                if (ki.styles.IndexOf(su) >= 0)
                                    ki.styles.Add(style);
                        };
                    };
            };


            // collect all placemarks name and styles in layers // no changes
            int ttpm = 0;
            waitBox.Text = "Collecting placemarks and styles in layers...";
            {
                XmlNodeList fldrs = kmfile.kmlDoc.SelectNodes("kml/Document/Folder");
                for (int i = 0; i < fldrs.Count; i++)
                {
                    XmlNodeList xns = fldrs[i].SelectNodes("Placemark");
                    if (xns.Count > 0)
                        for (int x = 0; x < xns.Count; x++)
                        {
                            ttpm++;
                            string nam = xns[x].SelectSingleNode("name").ChildNodes[0].Value;
                            XmlNode nsm = xns[x].SelectSingleNode("styleUrl");
                            if (nsm != null)
                            {
                                string stname = nsm.ChildNodes[0].Value;
                                if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
                                foreach (KMIcon ic in icons)
                                    if (ic.styles.IndexOf(stname) >= 0)
                                    {
                                        ic.placemarks++;
                                        if (ic.lcs == null)
                                            ic.lcs = nam;
                                        else
                                            ic.lcs = LCS(ic.lcs, nam);
                                    };
                            };
                        };
                };
            };

            // delete empty styles                       
            for (int i = icons.Count - 1; i >= 0; i--)
                if (icons[i].placemarks == 0)
                    icons.RemoveAt(i);

            AddToLog("Preparing bitmaps...");
            waitBox.Text = "Preparing bitmaps...";
            for (int i = 0; i < icons.Count; i++)
            {
                waitBox.Text = String.Format("Preparing {0:00}% bitmap...", ((double)(i + 1)) / ((double)icons.Count) * 100);

                string bmpHref = i.ToString() + ".bmp";
                bool isurl = Uri.IsWellFormedUriString(icons[i].href, UriKind.Absolute);
                if (isurl)
                {
                    GrabImage(icons[i].href, zdir + "tmp.png");
                    ConvertImageToBmp8bppIndexed(zdir + "tmp.png", zdir + bmpHref);
                    File.Delete(zdir + "tmp.png");
                }
                else
                {
                    if (!File.Exists(zdir + bmpHref))
                        ConvertImageToBmp8bppIndexed(kmfile.tmp_file_dir + icons[i].href, zdir + bmpHref);
                };
            };

            Regex rxgpisn = new Regex(@"gpi_subname_(?<crc>[^=]+)\s*=(?<name>[\S\s][^\r\n]+)", RegexOptions.IgnoreCase);
            Regex rxgpinn = new Regex(@"gpi_name_(?<crc>[^=]+)\s*=(?<name>[\S\s][^\r\n]+)", RegexOptions.IgnoreCase);
            AddToLog("Prepare Categories...");
            waitBox.Text = "Prepare Categories...";
            int cat_created = 0;
            {
                XmlNodeList nl = kmfile.kmlDoc.SelectNodes("kml/Document/Folder");
                int layNo = 0;
                int ni = 0;
                foreach (XmlNode n in nl)
                {
                    waitBox.Text = String.Format("Preparing {0:00}% Category...", ((double)(++ni)) / ((double)nl.Count) * 100);

                    string nam = "";
                    try { nam = n.SelectSingleNode("name").ChildNodes[0].Value; }
                    catch { };
                    string desc = "";
                    try { desc = n.SelectSingleNode("description").ChildNodes[0].Value; }
                    catch { };
                    if (nam != String.Empty)
                    {
                        int sublay = 0;
                        string wasname = null;
                        foreach (XmlNode n2 in n.SelectNodes("Placemark/styleUrl"))
                        {
                            string styleUrl = n2.ChildNodes[0].Value;
                            if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);
                            if (styleUrl != String.Empty)
                            {
                                if (icons.Count > 0)
                                    for (int ics = 0; ics < icons.Count; ics++)
                                        if (icons[ics].styles.IndexOf(styleUrl) >= 0)
                                        {
                                            KMIcon kmi = icons[ics];
                                            string currname = nam;
                                            MatchCollection mc;
                                            if (!String.IsNullOrEmpty(desc) && ((mc = rxgpinn.Matches(desc)).Count > 0))
                                            {
                                                CRC32 crc = new CRC32();
                                                string cc = crc.CRC32Num(kmfile.tmp_file_dir + kmi.href).ToString();
                                                foreach (Match mx in mc)
                                                    if (mx.Groups["crc"].Value == cc)
                                                        currname = mx.Groups["name"].Value;
                                            };
                                            if (!String.IsNullOrEmpty(desc) && ((mc = rxgpisn.Matches(desc)).Count > 0))
                                            {
                                                CRC32 crc = new CRC32();
                                                string cc = crc.CRC32Num(kmfile.tmp_file_dir + kmi.href).ToString();
                                                foreach (Match mx in mc)
                                                    if (mx.Groups["crc"].Value == cc)
                                                        currname += " " + mx.Groups["name"].Value;
                                            };
                                            string cat = group_id + "L" + layNo.ToString() + "S" + ics.ToString();
                                            if (!gpx_files.ContainsKey(cat))
                                            {
                                                if (wasname == null) wasname = currname;
                                                string tnm = currname.Trim(Path.GetInvalidFileNameChars()).Trim();
                                                string gpx = gdir + cat + ".gpx";
                                                string bmp = gdir + cat + ".bmp";
                                                try
                                                {
                                                    FileStream fs = new FileStream(gpx, FileMode.Create, FileAccess.Write);
                                                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                                                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                                                    sw.WriteLine("<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" creator=\"" + this.Text + "\" version=\"1.1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd\">");
                                                    sw.WriteLine("\t<name><![CDATA[" + currname + "]]></name>");
                                                    sw.Close();
                                                    fs.Close();
                                                }
                                                catch { };
                                                try { File.Copy(zdir + ics.ToString() + ".bmp", bmp); }
                                                catch { };
                                                if (sublay == 0)
                                                    gpx_files.Add(cat, tnm);
                                                else
                                                {
                                                    List<string> keys = new List<string>(gpx_files.Keys);
                                                    for (int kvp = 0; kvp < keys.Count; kvp++)
                                                        if (gpx_files[keys[kvp]] == currname) gpx_files[keys[kvp]] = currname + " 1";
                                                    gpx_files.Add(cat, wasname == currname ? tnm + " " + (sublay + 1).ToString() : tnm);
                                                };
                                                sublay++;
                                                cat_created++;
                                                wasname = currname;
                                            };
                                        };
                            };
                        };
                    };
                    layNo++;
                };
            };            

            Regex rx = new Regex("&(?!amp;)");

            //POI
            AddToLog("Saving POI...");
            waitBox.Show();
            waitBox.Text = "Saving POI...";            
            int poi_added = 0;
            {
                XmlNodeList nl = kmfile.kmlDoc.SelectNodes("kml/Document/Folder");
                int layNo = 0;
                foreach (XmlNode n in nl)
                {
                    string nam = "";
                    try { nam = n.SelectSingleNode("name").ChildNodes[0].Value; }
                    catch { };

                    foreach (XmlNode n2 in n.SelectNodes("Placemark/Point/coordinates"))
                    {
                        string poi = n2.ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value;
                        string desc = "";
                        try { desc = n2.ParentNode.ParentNode.SelectSingleNode("description").ChildNodes[0].Value; }
                        catch { };
                        string[] ll = n2.ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.None);

                        // poi = rx.Replace(poi, "&amp;");
                        // desc = rx.Replace(desc, "&amp;");

                        string styleUrl = "";
                        if (n2.ParentNode.ParentNode.SelectSingleNode("styleUrl") != null) styleUrl = n2.ParentNode.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                        if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);

                        if (icons.Count > 0)
                            for (int ics = 0; ics < icons.Count; ics++)
                                if (icons[ics].styles.IndexOf(styleUrl) >= 0)
                                {
                                    waitBox.Text = String.Format("Saving {0:00}% POI...", ((double)(poi_added + 1) / ((double)ttpm) * 100));

                                    string cat = group_id + "L" + layNo.ToString() + "S" + ics.ToString();
                                    FileStream fs = new FileStream(gdir + cat + ".gpx", FileMode.Open, FileAccess.Write);
                                    fs.Position = fs.Length;
                                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                                    sw.WriteLine("\t<wpt lat=\"" + ll[1] + "\" lon=\"" + ll[0] + "\">");
                                    sw.WriteLine("\t\t<name><![CDATA[" + poi + "]]></name>");
                                    sw.WriteLine("\t\t<desc><![CDATA[" + System.Security.SecurityElement.Escape(desc) + "]]></desc>");
                                    sw.WriteLine("\t</wpt>");
                                    sw.Close();
                                    fs.Close();
                                    
                                    poi_added++;
                                };
                    };
                    layNo++;
                };
            };

            foreach (KeyValuePair<string, string> kvp in gpx_files)
            {
                FileStream fs = new FileStream(gdir + kvp.Key + ".gpx", FileMode.Open, FileAccess.Write);
                fs.Position = fs.Length;
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("</gpx>");
                sw.Close();
                fs.Close();
            };

            AddToLog(String.Format("Used {0}/{6} icons in {1} layers, created {2} POIs from {4} placemarks in {3} categories, {5} placemarks skipped", icons.Count, kmfile.kmLayers.Count, poi_added, cat_created, ttpm, ttpm - poi_added, icons_total));
            Refresh();

            waitBox.Hide();
            GMLayRenamerForm gml = GMLayRenamerForm.FromGPIGPX(gpx_files, gdir);
            gml.ShowDialog();
            {
                foreach (Category cat in gml.categories)
                {
                    try
                    {
                        File.Move(gdir + cat.ID + ".gpx", gdir + cat.name + ".gpx");
                        File.Move(gdir + cat.ID + ".bmp", gdir + cat.name + ".bmp");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error:\r\n"+ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        waitBox.Hide();
                        AddToLog("Error");
                        break;
                    };
                };
            };
            gml.Dispose();            

            waitBox.Text = "Creating GPI File...";
            AddToLog("Creating GPI File...");

            string gpigen = CurrentDirectory() + @"\GPIGen\gpigen.exe";
            //if(File.Exists(gpifile)) File.Delete(gpifile);
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(gpigen, String.Format("\"{0}\" \"{1}\"", zdir, gpifile));
            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi);
            proc.WaitForExit();

            waitBox.Hide();
            AddToLog("Done");
        }

        private void Save2GML(string zipfile, KMFile kmfile)
        {
            string proj_name = kmfile.kmldocName;
            if(proj_name == "") 
                proj_name = outName.Text;
            if (InputBox("Creating Garmin XML Project", "Enter GPI Project name:", ref proj_name) != DialogResult.OK)
                return;

            int icons_total = Directory.GetFiles(kmfile.tmp_file_dir + @"\images", "*.*").Length;

            waitBox.Show("Saving", "Wait, converting files...");

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = (new Random());
            string group_id = new String(new char[] { chars[random.Next(chars.Length)],  chars[random.Next(chars.Length)],  chars[random.Next(chars.Length)], chars[random.Next(chars.Length)], chars[random.Next(chars.Length)], chars[random.Next(chars.Length)] });

            string zdir = KMZRebuilederForm.TempDirectory() + "OF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            System.IO.Directory.CreateDirectory(zdir);            
            System.IO.Directory.CreateDirectory(zdir + @"\images\");
            AddToLog("Creating Garmin XML Project for Garmin GPI Creator...");
            AddToLog("Create Temp Folder: " + zdir);
            AddToLog("Creating XML File: " + zdir + "xml_input_file.xml");
                        
            FileStream fs = new FileStream(zdir + "xml_input_file.xml", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sw.WriteLine("<GPI xmlns=\"http://www.garmin.com\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.garmin.com GPI_XML.xsd\">");
            sw.WriteLine("\t<Group>");
            sw.WriteLine("\t\t<ID>" + group_id + "</ID>");
            sw.WriteLine("\t\t<Name>");
            sw.WriteLine("\t\t\t<LString lang=\"EN\">" + GetTranslit(proj_name) + "</LString>");
            sw.WriteLine("\t\t\t<LString lang=\"RU\">" + proj_name + "</LString>");
            sw.WriteLine("\t\t</Name>");

            List<KMIcon> icons = new List<KMIcon>();
            
            // collect all icons in kml // no changes
            waitBox.Text = "Collecting all icons in kml...";
            {
                XmlNodeList xns = kmfile.kmlDoc.SelectNodes("kml/Document/Style/IconStyle/Icon/href");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        XmlNode xn2 = xns[x];
                        string style = xn2.ParentNode.ParentNode.ParentNode.Attributes["id"].Value;
                        string href = xn2.ChildNodes[0].Value;

                        bool isurl = Uri.IsWellFormedUriString(href, UriKind.Absolute);
                        KMIcon ki = new KMIcon(href, kmfile, href, style);
                        Image img = null;

                        if (!isurl)
                        {
                            try
                            {
                                img = Image.FromFile(kmfile.tmp_file_dir + href);
                                ki.image = (Image)new Bitmap(img);
                            }
                            catch { };
                        }
                        else
                        {
                            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(ki.href);
                            try
                            {
                                using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
                                using (Stream stream = response.GetResponseStream())
                                    img = Bitmap.FromStream(stream);
                                ki.image = (Image)new Bitmap(img);
                            }
                            catch
                            {
                            };
                        };
                        if (img != null) img.Dispose();

                        bool skip = false;
                        foreach (KMIcon get in icons)
                        {
                            if (get.ToString() == ki.ToString())
                            {
                                skip = true;
                                get.styles.Add(style);
                                break;
                            };

                            if ((ki.image != null) && (get.image != null))
                                if (CompareMemCmp((Bitmap)ki.image, (Bitmap)get.image))
                                {
                                    skip = true;
                                    get.styles.Add(style);
                                    break;
                                };
                        };
                        if (!skip) icons.Add(ki);
                    };
            };

            // collect all style maps in kml // no changes
            waitBox.Text = "Collecting all style maps in kml...";
            {
                XmlNodeList xns = kmfile.kmlDoc.SelectNodes("kml/Document/StyleMap");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        string style = xns[x].Attributes["id"].Value;
                        foreach (XmlNode xn2 in xns[x].SelectNodes("Pair/styleUrl"))
                        {
                            string su = xn2.ChildNodes[0].Value;
                            if (su.IndexOf("#") == 0) su = su.Remove(0, 1);
                            foreach (KMIcon ki in icons)
                                if (ki.styles.IndexOf(su) >= 0)
                                    ki.styles.Add(style);
                        };
                    };
            };


            // collect all placemarks name and styles in layers // no changes
            int ttpm = 0;
            waitBox.Text = "Collecting all placemarks name and styles in kml layers...";
            {
                XmlNodeList fldrs = kmfile.kmlDoc.SelectNodes("kml/Document/Folder");
                for (int i = 0; i < fldrs.Count; i++)
                {
                    XmlNodeList xns = fldrs[i].SelectNodes("Placemark");
                    if (xns.Count > 0)
                        for (int x = 0; x < xns.Count; x++)
                        {
                            ttpm++;
                            string nam = xns[x].SelectSingleNode("name").ChildNodes[0].Value;
                            XmlNode nsm = xns[x].SelectSingleNode("styleUrl");
                            if (nsm != null)
                            {
                                string stname = nsm.ChildNodes[0].Value;
                                if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
                                foreach (KMIcon ic in icons)
                                    if (ic.styles.IndexOf(stname) >= 0)
                                    {
                                        ic.placemarks++;
                                        if (ic.lcs == null)
                                            ic.lcs = nam;
                                        else
                                            ic.lcs = LCS(ic.lcs, nam);
                                    };
                            };
                        };
                };
            };

            // delete empty styles                       
            for (int i = icons.Count - 1; i >= 0; i--)
                if (icons[i].placemarks == 0)
                    icons.RemoveAt(i);

            AddToLog("Preparing bitmaps...");
            sw.WriteLine("\t\t<SymbolList>");
            waitBox.Text = "Preparing bitmaps and save into SymbolList...";
            for (int i = 0; i < icons.Count; i++)
            {
                waitBox.Text = String.Format("Preparing {0:00}% bitmap and save into SymbolList...", ((double)(i + 1))/((double)icons.Count)*100);

                string style_r = icons[i].styles[0].Replace("-", "A");
                sw.WriteLine("\t\t\t<Symbol>");
                sw.WriteLine("\t\t\t\t<ID>" + style_r + "</ID>");
                string bmpHref = @"images\" + i.ToString() + ".bmp";
                sw.WriteLine("\t\t\t\t<File useTransparency=\"true\">" + bmpHref + "</File>");
                bool isurl = Uri.IsWellFormedUriString(icons[i].href, UriKind.Absolute);
                if (isurl)
                {
                    GrabImage(icons[i].href, zdir + "tmp.png");
                    ConvertImageToBmp8bppIndexed(zdir + "tmp.png", zdir + bmpHref);
                    File.Delete(zdir + "tmp.png");
                }
                else
                {
                    if (!File.Exists(zdir + bmpHref))
                        ConvertImageToBmp8bppIndexed(kmfile.tmp_file_dir + icons[i].href, zdir + bmpHref);
                };
                sw.WriteLine("\t\t\t</Symbol>");
            };
            sw.WriteLine("\t\t</SymbolList>");

            Regex rxgpisn = new Regex(@"gpi_subname_(?<crc>[^=]+)\s*=(?<name>[\S\s][^\r\n]+)", RegexOptions.IgnoreCase);
            Regex rxgpinn = new Regex(@"gpi_name_(?<crc>[^=]+)\s*=(?<name>[\S\s][^\r\n]+)", RegexOptions.IgnoreCase);
            AddToLog("Saving Symbols, Categories and POIs...");
            waitBox.Text = "Saving CategoryList...";
            int cat_created = 0;
            sw.WriteLine("\t\t<CategoryList>");
            {
                List<string> categories_added = new List<string>();
                XmlNodeList nl = kmfile.kmlDoc.SelectNodes("kml/Document/Folder");
                int layNo = 0;
                foreach (XmlNode n in nl)
                {
                    string nam = "";
                    try { nam = n.SelectSingleNode("name").ChildNodes[0].Value; }
                    catch { };
                    string desc = "";
                    try { desc = n.SelectSingleNode("description").ChildNodes[0].Value; }
                    catch { };
                    if (nam != String.Empty)
                    {
                        int sublay = 0;
                        string wasname = null;
                        foreach (XmlNode n2 in n.SelectNodes("Placemark/styleUrl"))
                        {
                            string styleUrl = n2.ChildNodes[0].Value;
                            if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);
                            if (styleUrl != String.Empty)
                            {
                                if (icons.Count > 0)
                                    for (int ics = 0; ics < icons.Count; ics++)
                                        if (icons[ics].styles.IndexOf(styleUrl) >= 0)
                                        {
                                            KMIcon kmi = icons[ics];
                                            string currname = nam;
                                            MatchCollection mc;
                                            if (!String.IsNullOrEmpty(desc) && ((mc = rxgpinn.Matches(desc)).Count > 0))
                                            {
                                                CRC32 crc = new CRC32();
                                                string cc = crc.CRC32Num(kmfile.tmp_file_dir + kmi.href).ToString();
                                                foreach (Match mx in mc)
                                                    if (mx.Groups["crc"].Value == cc)
                                                        currname = mx.Groups["name"].Value;
                                            };
                                            if (!String.IsNullOrEmpty(desc) && ((mc = rxgpisn.Matches(desc)).Count > 0))
                                            {
                                                CRC32 crc = new CRC32();
                                                string cc = crc.CRC32Num(kmfile.tmp_file_dir + kmi.href).ToString();
                                                foreach (Match mx in mc)
                                                    if (mx.Groups["crc"].Value == cc)
                                                        currname += " " + mx.Groups["name"].Value;
                                            };
                                            string cat = group_id + "L" + layNo.ToString() + "S" + ics.ToString();
                                            if (categories_added.IndexOf(cat) < 0)
                                            {
                                                if (wasname == null) wasname = currname;
                                                sw.WriteLine("\t\t\t<Category>");
                                                sw.WriteLine("\t\t\t\t<ID>" + cat + "</ID>");
                                                sw.WriteLine("\t\t\t\t<Name>");
                                                sw.WriteLine("\t\t\t\t\t<LString lang=\"EN\">" + GetTranslit(currname) + ((sublay > 0) && (currname == wasname) ? " " + sublay.ToString() : "") + "</LString>");
                                                sw.WriteLine("\t\t\t\t\t<LString lang=\"RU\">" + currname + ((sublay > 0) && (currname == wasname) ? " " + sublay.ToString() : "") + "</LString>");
                                                sw.WriteLine("\t\t\t\t</Name>");
                                                // set style to first in
                                                sw.WriteLine("\t\t\t\t<CustomSymbol>" + icons[ics].styles[0].Replace("-", "A") + "</CustomSymbol>");
                                                sw.WriteLine("\t\t\t</Category>");
                                                categories_added.Add(cat);
                                                sublay++;
                                                cat_created++;
                                                wasname = currname;
                                            };
                                        };
                            };
                        };
                    };
                    layNo++;
                };
            }
            sw.WriteLine("\t\t</CategoryList>");

            Regex rx = new Regex("&(?!amp;)");

            //POI
            waitBox.Text = "Saving POI...";
            int poi_added = 0;
            {
                XmlNodeList nl = kmfile.kmlDoc.SelectNodes("kml/Document/Folder");
                int layNo = 0;
                foreach (XmlNode n in nl)
                {
                    string nam = "";
                    try { nam = n.SelectSingleNode("name").ChildNodes[0].Value; }
                    catch { };

                    foreach (XmlNode n2 in n.SelectNodes("Placemark/Point/coordinates"))
                    {
                        string poi = n2.ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value;
                        string desc = "";
                        try { desc = n2.ParentNode.ParentNode.SelectSingleNode("description").ChildNodes[0].Value; }
                        catch { };
                        string[] ll = n2.ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.None);

                        poi = rx.Replace(poi, "&amp;");
                        desc = rx.Replace(desc, "&amp;");

                        string styleUrl = "";
                        if (n2.ParentNode.ParentNode.SelectSingleNode("styleUrl") != null) styleUrl = n2.ParentNode.ParentNode.SelectSingleNode("styleUrl").ChildNodes[0].Value;
                        if (styleUrl.IndexOf("#") == 0) styleUrl = styleUrl.Remove(0, 1);

                        if (icons.Count > 0)
                            for (int ics = 0; ics < icons.Count; ics++)
                                if (icons[ics].styles.IndexOf(styleUrl) >= 0)
                                {
                                    waitBox.Text = String.Format("Saving {0:00}% POI...", ((double)(poi_added + 1) / ((double)ttpm) * 100));

                                    sw.WriteLine("\t\t<POI>");
                                    sw.WriteLine("\t\t\t<Name>");
                                    sw.WriteLine("\t\t\t\t<LString lang=\"EN\">" + GetTranslit(poi) + "</LString>");
                                    sw.WriteLine("\t\t\t\t<LString lang=\"RU\">" + poi + "</LString>");
                                    sw.WriteLine("\t\t\t</Name>");
                                    sw.WriteLine("\t\t\t<Geo>");
                                    sw.WriteLine("\t\t\t\t<Lat>" + ll[1] + "</Lat>");
                                    sw.WriteLine("\t\t\t\t<Lon>" + ll[0] + "</Lon>");
                                    sw.WriteLine("\t\t\t</Geo>");
                                    sw.WriteLine("\t\t\t<CategoryID>" + group_id + "L" + layNo.ToString() + "S" + ics.ToString() + "</CategoryID>");
                                    if (desc != String.Empty)
                                        sw.WriteLine("\t\t\t\t<GeneralText><![CDATA[" + System.Security.SecurityElement.Escape(desc) + "]]></GeneralText>");
                                    sw.WriteLine("\t\t\t</POI>");
                                    poi_added++;                                    
                                };
                    };
                    layNo++;
                };
            }

            AddToLog(String.Format("Used {0}/{6} icons in {1} layers, created {2} POIs from {4} placemarks in {3} categories, {5} placemarks skipped", icons.Count, kmfile.kmLayers.Count, poi_added, cat_created, ttpm, ttpm - poi_added, icons_total));
            Refresh();
            ////////////////////////

            sw.WriteLine("</Group>");
            sw.WriteLine("</GPI>");
            sw.Close();
            fs.Close();

            waitBox.Text = "Saving product_info.pdi...";
            fs = new FileStream(zdir + @"product_info.pdi", FileMode.Create, FileAccess.Write);
            sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sw.WriteLine("<ProductInfo xmlns=\"http://www.garmin.com\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.garmin.com ..\\PDI_XML.xsd\">");
            sw.WriteLine("\t<DataName>");
            sw.WriteLine("\t\t<LString lang=\"EN\">" + GetTranslit(proj_name) + "</LString>");
            sw.WriteLine("\t\t<LString lang=\"RU\">" + proj_name + "</LString>");
            sw.WriteLine("\t</DataName>");
            sw.WriteLine("\t<Copyright>");
            sw.WriteLine("\t\t<LString lang=\"EN\">Copyrights " + DateTime.Now.ToString("yyyy") + "</LString>");
            sw.WriteLine("\t\t<LString lang=\"RU\">Copyrights " + DateTime.Now.ToString("yyyy") + "</LString>");
            sw.WriteLine("\t</Copyright>");
            sw.WriteLine("\t<DefaultLang>EN</DefaultLang>");
            sw.WriteLine("\t<Expiration>20199-01-01</Expiration>");
            sw.WriteLine("\t<UTF-8>true</UTF-8>");
            sw.WriteLine("\t<Units>Meters and KPH</Units>");
            sw.WriteLine("\t<AlertTrigger>Proximity</AlertTrigger>");
            sw.WriteLine("</ProductInfo>");
            sw.Close();
            fs.Close();

            waitBox.Text = "Saving license.gpil...";
            try { File.Copy(CurrentDirectory() + @"KMZRebuilder.gpil", zdir + @"license.gpil"); } catch { };

            waitBox.Text = "Saving configuration.config...";
            fs = new FileStream(zdir + @"configuration.config", FileMode.Create, FileAccess.Write);
            sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("gpi_file = ready.gpi");
            sw.WriteLine("input_files = xml_input_file.xml");
            sw.WriteLine("license_file = license.gpil");
            sw.WriteLine("locked = false");
            sw.WriteLine("name = " + GetTranslit(proj_name));
            sw.WriteLine("pdi_file = product_info.pdi");
            sw.Close();
            fs.Close();

            waitBox.Hide();

            GMLayRenamerForm ren = new GMLayRenamerForm(zdir + "xml_input_file.xml");
            ren.layers.FullRowSelect = true;
            ren.ShowDialog();
            ren.Dispose();

            waitBox.Show("Saving", "Wait, saving file...");

            AddToLog("Saving data to selected file: " + zipfile);
            CreateZIP(zipfile, zdir);
            waitBox.Hide();
            AddToLog("Done");
        }

        private void GrabImage(string url, string file)
        {
            
            System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
            wr.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
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

        private static void AddToLogS(string txt)
        {
            KMZRebuilder.Program.mainForm.AddToLog(txt);
        }

        public void ClearLog()
        {
            log.Text = "";
        }

        public void AddToLog(string txt)
        {
            if (!panel1.Visible)
                panel1.Visible = true;

            log.Text += String.Format("{0}\r\n", txt);
            log.SelectionStart = log.TextLength;
            log.ScrollToCaret();
            Refresh();
        }

        // https://github.com/icsharpcode/SharpZipLib/wiki/Zip-Samples#Create_a_Zip_fromto_a_memory_stream_or_byte_array_1
        private void CreateZIP(string filename, string folder)
        {
            FileStream fsOut = File.Create(filename);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);
            string comment_add = "";
            zipStream.SetComment("Google KMZ file For OruxMaps\r\n\r\n" + "Created at " + DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy") + "\r\nby " + this.Text + "\r\n\r\nUse OruxMaps for Android or KMZViewer for Windows to Explore file POI" + comment_add);
            zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
            // zipStream.Password = password;  // optional. Null is the same as not setting. Required if using AES.
            CompressFolder(folder, zipStream, folder.Length);
            zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
            zipStream.Close();
        }

        // Recurses down the folder structure
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

        private void FilesMenu_Opening(object sender, CancelEventArgs e)
        {
            Point point = kmzFiles.PointToClient(Cursor.Position);
            int index = kmzFiles.IndexFromPoint(point);
            kmzFiles.SelectedIndex = index;            

            MRU.Enabled = MyMruList.Count > 0;

            deleteAllToolStripMenuItem.Enabled = kmzFiles.Items.Count > 0;
            deleteToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            deleteCheckedToolStripMenuItem.Enabled = kmzFiles.CheckedItems.Count > 0;
            editKMLFileToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            removeDescriptionsToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            removeDescriptionOSMTagsTagToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            regexReplaceToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            changeStyleIconsToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            setNameSubnamesForGPIByIconsToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            findSimilarStyleIconsToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            reloadXMLToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            viewInKMLViewerToolStripMenuItem.Enabled = 
                reloadOriginalFileToolStripMenuItem.Enabled =
                    (kmzFiles.SelectedIndices.Count > 0) && ((KMFile)kmzFiles.SelectedItems[0]).AllowReloadOriginal;
            exportHTMLMapwithIconsToolStripMenuItem.Enabled = saveAsKMLToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            viewInWebBrowserToolStripMenuItem1.Enabled = applyFilterSelectionToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;            
            saveBtnGML.Enabled = kmzFiles.SelectedIndices.Count > 0;
            saveBtnGPI.Enabled = kmzFiles.SelectedIndices.Count > 0;
            saveBtnGPIN.Enabled = kmzFiles.SelectedIndices.Count > 0;
            setAsOutputKmzFileDocuemntNameToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            openSourceFileDirectoryToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            openToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;

            reloadAsColorSpeedTrackToolStripMenuItem.Enabled = gpxexToolStripMenuItem.Enabled = viewHTMLCSMap.Enabled = false;
                        

            CFPBF.Enabled = (File.Exists(CurrentDirectory() + @"\KMZPOIfromOSM.exe"));

            // NO ADD AFTER //
            reloadOriginalFileToolStripMenuItem.Text = "Reload Original file";
            if (kmzFiles.SelectedIndices.Count == 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            reloadOriginalFileToolStripMenuItem.Text = String.Format("Reload Original (`{0}`) file",f.src_file_nme);
            reloadAsColorSpeedTrackToolStripMenuItem.Enabled = gpxexToolStripMenuItem.Enabled = viewHTMLCSMap.Enabled = f.src_file_ext.ToLower() == ".gpx";            
        }

        private void LayersMenu_Opening(object sender, CancelEventArgs e)
        {
            Point point = kmzLayers.PointToClient(Cursor.Position);
            int index = kmzLayers.IndexFromPoint(point);
            kmzLayers.SelectedIndex = index;

            saveURLIcons.Enabled = false;

            viewCheckedInKMZViewerToolStripMenuItem.Enabled = kmzLayers.SelectedItems.Count > 0;
            
            fRemoveOSMTagsFromDescriptionToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            allLayersFRemoveDescriptionsToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            allLayersFBatchReplaceToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;

            addEmptyLayerToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            deleteLayerToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            removeEmptyLayersToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            renameLayerToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            changeLayerDescriptionToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            moveUpToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            moveDownToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveAsKMLToolStripMenuItem1.Enabled = kmzLayers.SelectedIndices.Count > 0;
            applyFilterSelectionToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveLayerToGPXwptrteFilewithNoIconsToolStripMenuItem.Enabled = gPX2ToolStripMenuItem.Enabled = gPX3ToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveLayerToWPTFileToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveLayerToWPTFilesetSymbolsToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveLayerToDATfavoritesdatFileForPROGORODToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveLayerToGDBNavitelgdbFileForNavitelToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;            
            saveLayerToOtherFormatsToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveLayerToToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            viewInKMLViewerToolStripMenuItem.Enabled = viewInKMLViewerToolStripMenuItem.Enabled = viewInWebBrowserToolStripMenuItem.Enabled = viewContentToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            getLayerCRCGPIToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            compareLayersToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            checkLayersWithSameNameButLessObjectsCountToolStripMenuItem.Enabled = kmzLayers.Items.Count > 1;
            checkLayersWithSameNameButMoreObjectsCountToolStripMenuItem.Enabled = kmzLayers.Items.Count > 1;
            unckeckLayersWithSameNamesToolStripMenuItem.Enabled = kmzLayers.Items.Count > 1;
            checkLayersWithSameNamesToolStripMenuItem.Enabled = kmzLayers.Items.Count > 1;
            checkLayersWithSameNamesButToolStripMenuItem.Enabled = (kmzLayers.Items.Count > 1) && (kmzFiles.Items.Count > 1);
            checkLayersWithSameNamesOnlyInFileToolStripMenuItem.Enabled = (kmzLayers.Items.Count > 1) && (kmzFiles.Items.Count > 1);
            checkLayersWithSameNamesButNotInFileToolStripMenuItem.Enabled = (kmzLayers.Items.Count > 1) && (kmzFiles.Items.Count > 1);
            uncheckLayersWithSameNameOnlyInFileToolStripMenuItem.Enabled = (kmzLayers.Items.Count > 1) && (kmzFiles.Items.Count > 1);
            selectAllToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            selectNoneToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            invertSelectionToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            checkOnlyWithPlacemarksToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            slb3.Enabled = slb2.Enabled = slbn.Enabled = asf2.Enabled = splitLayerByIconsToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;

            viewInKMZViewerToolStripMenuItem.Enabled = 
                (kmzLayers.SelectedIndices.Count > 0) && KMFile.ValidForDragDropAuto(((KMLayer)kmzLayers.Items[kmzLayers.SelectedIndices[0]]).file.src_file_nme);

            findLayerToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
        }

        private void RenameLayer_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;

            KMLayer l = (KMLayer)kmzLayers.SelectedItem;
            string name = l.name;
            if ((InputBox("Change layer name", "Name:", ref name) == DialogResult.OK) && (name != l.name))
            {
                l.name = name;
                l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id].SelectSingleNode("name").ChildNodes[0].Value = name;
                l.file.SaveKML();
                Refresh();
                //kmzLayers.Refresh();
            };
        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            return InputBox(title, promptText, ref value, null);
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
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            if(picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            value = textBox.Text;
            return dialogResult;
        }

        private void SortLayers(sbyte sortBy)
        {
            kmzLayers.Visible = false;
            panel1.Visible = false;
            waitBox.Show("Reording layers", "Wait, reording layers...");

            kmzLayers.Sorted = false;
            kmzLayers.SortBy = sortBy >= 0 ? (byte)sortBy : (byte)0;
            kmzLayers.Sorted = sortBy > 0;
            if (sortBy == 0) ReloadListboxLayers(true);
            sortByAddingToolStripMenuItem.Checked = sortBy == 0;
            sortASCToolStripMenuItem.Checked = sortBy == 1;
            sortByObjectsCountToolStripMenuItem.Checked = sortBy == 2;
            sortByCheckedToolStripMenuItem.Checked = sortBy == 3;
            sortByO.Checked = sortBy == 4;
            sortByL.Checked = sortBy == 5;
            sortByA.Checked = sortBy == 6;            

            for (int i = 0; i < kmzLayers.Items.Count; i++)
                kmzLayers.SetItemChecked(i, ((KMLayer)kmzLayers.Items[i]).ischeck);
            waitBox.Hide();
            kmzLayers.Visible = true;
        }

        private void SortByMouse()
        {
            kmzLayers.Sorted = false;
            kmzLayers.SortBy = (byte)0;
            sortByAddingToolStripMenuItem.Checked = false;
            sortASCToolStripMenuItem.Checked = false;
            sortByObjectsCountToolStripMenuItem.Checked = false;
            sortByCheckedToolStripMenuItem.Checked = false;
            sortByO.Checked = false;
            sortByL.Checked = false;
            sortByA.Checked = false;        
        }

        private void MoveLayerUp_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndex < 1) return;

            SortLayers(-1);

            int i = kmzLayers.SelectedIndex;
            KMLayer l = (KMLayer)kmzLayers.Items[i];
            kmzLayers.Items.RemoveAt(i);
            kmzLayers.Items.Insert(--i, l);
            kmzLayers.SetItemChecked(i, l.ischeck);
            kmzLayers.SelectedIndex = i;            
        }

        private void MoveLayerDown_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndex >= (kmzLayers.Items.Count-1)) return;

            if (sortASCToolStripMenuItem.Checked)
            {
                MessageBox.Show("Change Sort Mode First!", this.Text, MessageBoxButtons.OK);
                return;
            };

            int i = kmzLayers.SelectedIndex;
            KMLayer l = (KMLayer)kmzLayers.Items[i];
            kmzLayers.Items.RemoveAt(i);
            kmzLayers.Items.Insert(++i, l);
            kmzLayers.SetItemChecked(i, l.ischeck);
            kmzLayers.SelectedIndex = i;      
        }

        private void SelectLayers_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < kmzLayers.Items.Count; i++)
                kmzLayers.SetItemChecked(i, true);
        }

        private void DeselectLayers_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < kmzLayers.Items.Count; i++)
                kmzLayers.SetItemChecked(i, false);
        }

        private void InvertLayers_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < kmzLayers.Items.Count; i++)
                kmzLayers.SetItemChecked(i, !kmzLayers.GetItemChecked(i));
        }

        private void FileKMLName2Output_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            outName.Text = f.kmldocName;
        }

        private void EditTempKMLFile_click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string path = f.tmp_file_dir + "doc.kml";
            bool ok = false;
            if(!ok)
                try
                {
                    System.Diagnostics.Process.Start("notepad++", path);
                    ok = true;
                }
                catch { };
            if(!ok)
                try
                {
                    System.Diagnostics.Process.Start(CurrentDirectory() + @"AkelPad.exe", path);
                    ok = true;
                }
                catch{};
            if(!ok)
                try
                {
                    System.Diagnostics.Process.Start("notepad", path);
                }
                catch { };
        }

        private void ReloadTempKMLFile_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            waitBox.Show("Reloading", "Wait, reloading edited `doc.kml` file...");
            f.LoadKML(true);
            ReloadListboxLayers(true);
            waitBox.Hide();
        }

        private void ReloadOriginalFile_click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            waitBox.Show("Reloading", "Wait, reloading original `" + f.src_file_nme + "` file...");
            f.CopySrcFileToTempDirAndLoad();
            ReloadListboxLayers(true);
            waitBox.Hide();
        }

        private void saveFileToKML_click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndices.Count == 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string path = f.tmp_file_dir + "doc.kml";

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "KML Files (*.kml)|*.kml";
            sfd.DefaultExt = ".kml";
            sfd.FileName = f.src_file_nme.Remove(f.src_file_nme.Length - 4) + ".kml";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                AddToLog("Creating KML copy for file: `" + f.src_file_nme + "`...");                
                waitBox.Show("Saving", "Wait, savining file...");
                AddToLog(String.Format("Saving data to selected file: {0}", sfd.FileName));
                File.Copy(path, sfd.FileName, true);
                waitBox.Hide();
                AddToLog("Done");
            };
            sfd.Dispose();
        }

        private void saveLayerToKML_click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;
            
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "KML Files (*.kml)|*.kml";
            sfd.DefaultExt = ".kml";
            try
            {
                sfd.FileName = l.name + ".kml";
            }
            catch { };
            if (sfd.ShowDialog() == DialogResult.OK)
                Save2KML(sfd.FileName, l);
            sfd.Dispose();
        }        

        private void FormKMZ_FormClosed(object sender, FormClosedEventArgs e)
        {
            waitBox.Hide();
            Properties.Save();
            try { if (memFile != null) memFile.Close(); } catch { };
            try { if (Directory.Exists(TempDirectory())) System.IO.Directory.Delete(TempDirectory(), true); } catch { };
        }

        private void OpenTempDir_click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            System.Diagnostics.Process.Start(f.tmp_file_dir);
        }

        private void checkWithPlacemarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < kmzLayers.Items.Count; i++)
            {
                KMLayer l = (KMLayer)kmzLayers.Items[i];
                if(l.placemarks > 0)
                    kmzLayers.SetItemChecked(i, true);
            };
        }

        #region TRANSLIT
        private static Dictionary<string, string> transliter = new Dictionary<string, string>();
        private static void prepareTranslit()
        {
            transliter.Add("а", "a");
            transliter.Add("б", "b");
            transliter.Add("в", "v");
            transliter.Add("г", "g");
            transliter.Add("д", "d");
            transliter.Add("е", "e");
            transliter.Add("ё", "yo");
            transliter.Add("ж", "zh");
            transliter.Add("з", "z");
            transliter.Add("и", "i");
            transliter.Add("й", "j");
            transliter.Add("к", "k");
            transliter.Add("л", "l");
            transliter.Add("м", "m");
            transliter.Add("н", "n");
            transliter.Add("о", "o");
            transliter.Add("п", "p");
            transliter.Add("р", "r");
            transliter.Add("с", "s");
            transliter.Add("т", "t");
            transliter.Add("у", "u");
            transliter.Add("ф", "f");
            transliter.Add("х", "h");
            transliter.Add("ц", "c");
            transliter.Add("ч", "ch");
            transliter.Add("ш", "sh");
            transliter.Add("щ", "sch");
            transliter.Add("ъ", "j");
            transliter.Add("ы", "i");
            transliter.Add("ь", "j");
            transliter.Add("э", "e");
            transliter.Add("ю", "yu");
            transliter.Add("я", "ya");
            transliter.Add("А", "A");
            transliter.Add("Б", "B");
            transliter.Add("В", "V");
            transliter.Add("Г", "G");
            transliter.Add("Д", "D");
            transliter.Add("Е", "E");
            transliter.Add("Ё", "Yo");
            transliter.Add("Ж", "Zh");
            transliter.Add("З", "Z");
            transliter.Add("И", "I");
            transliter.Add("Й", "J");
            transliter.Add("К", "K");
            transliter.Add("Л", "L");
            transliter.Add("М", "M");
            transliter.Add("Н", "N");
            transliter.Add("О", "O");
            transliter.Add("П", "P");
            transliter.Add("Р", "R");
            transliter.Add("С", "S");
            transliter.Add("Т", "T");
            transliter.Add("У", "U");
            transliter.Add("Ф", "F");
            transliter.Add("Х", "H");
            transliter.Add("Ц", "C");
            transliter.Add("Ч", "Ch");
            transliter.Add("Ш", "Sh");
            transliter.Add("Щ", "Sch");
            transliter.Add("Ъ", "J");
            transliter.Add("Ы", "I");
            transliter.Add("Ь", "J");
            transliter.Add("Э", "E");
            transliter.Add("Ю", "Yu");
            transliter.Add("Я", "Ya");
        }
        public static string GetTranslit(string sourceText)
        {
            StringBuilder ans = new StringBuilder();
            for (int i = 0; i < sourceText.Length; i++)
            {
                if (transliter.ContainsKey(sourceText[i].ToString()))
                    ans.Append(transliter[sourceText[i].ToString()]);
                else
                    ans.Append(sourceText[i].ToString());
            }
            return ans.ToString();
        }
        #endregion TRANSLIT

        public static void ConvertImageToBmp8bppIndexed(string png, string bmp)
        {
            int tries = 3;
            while (tries > 0)
            {
                try
                {
                    ImageMagick.MagickImage mi = new ImageMagick.MagickImage(png);
                    mi.Opaque(new ImageMagick.MagickColor(Color.Transparent), new ImageMagick.MagickColor(Color.Fuchsia));
                    mi.Resize(22, 22);
                    mi.ColorType = ImageMagick.ColorType.Palette;
                    mi.Depth = 8;
                    Bitmap res = mi.ToBitmap(ImageFormat.Gif);
                    res.Save(bmp, ImageFormat.Bmp);
                    res.Dispose();
                    tries = 0;
                }
                catch {tries--;};
            };
        }

        private void saveGPX_click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "GPX Files (*.gpx)|*.gpx";
            sfd.DefaultExt = ".gpx";
            try
            {
                sfd.FileName = l.name + ".gpx";
            }
            catch { };
            if (sfd.ShowDialog() == DialogResult.OK)
                Save2GPX(sfd.FileName, l, true, true);
            sfd.Dispose();
        }

        private void saveWPT_click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            Save2WPTNoIcons((KMLayer)kmzLayers.SelectedItem);
        }

        private void Save2WPTNoIcons(KMLayer l)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "OziExplorer Waypoint Files (*.wpt)|*.wpt";
            sfd.DefaultExt = ".wpt";
            try
            {
                sfd.FileName = l.name + ".wpt";
            }
            catch { };
            if (sfd.ShowDialog() == DialogResult.OK)
                Save2WPT(sfd.FileName, l);
            sfd.Dispose();
        }

        private void saveLayerToWPTFilesetSymbolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            Export2WPT((KMLayer)kmzLayers.SelectedItem);
        }

        private void Export2WPT(KMLayer kml)
        {
            XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
            XmlNodeList xns = xn.SelectNodes("Placemark/Point/coordinates");
            if (xns.Count > 0)
            {
                string filename = null;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "OziExplorer Waypoint Files (*.wpt)|*.wpt";
                sfd.DefaultExt = ".wpt";
                try
                {
                    sfd.FileName = kml.name + ".wpt";
                }
                catch { };
                if (sfd.ShowDialog() == DialogResult.OK)
                    filename = sfd.FileName;
                sfd.Dispose();
                if (String.IsNullOrEmpty(filename)) return;

                //////////////////////////////////////////

                List<string> styles = new List<string>();
                List<int> new_styles = new List<int>();
                ImageList existingStylesIcon = new ImageList();
                IMM imm = new IMM();
                CRC32 crc = new CRC32();

                for (int x = 0; x < xns.Count; x++)
                {
                    string style = "none";
                    XmlNode stn = xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                    if ((stn != null) && (stn.ChildNodes.Count > 0))
                    {
                        style = stn.ChildNodes[0].Value;
                        if (styles.IndexOf(style) < 0)
                        {
                            styles.Add(style);
                            new_styles.Add(3);
                            string im = style.Replace("#", "");
                            XmlNode him = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + im + "']/IconStyle/Icon/href");
                            if (him != null)
                            {
                                im = kml.file.tmp_file_dir + him.InnerText.Replace("/", @"\");
                                existingStylesIcon.Images.Add(Image.FromFile(im));
                                imm.Set(crc.CRC32Num(im),style);
                            }
                            else
                                existingStylesIcon.Images.Add(new Bitmap(16, 16));
                        };
                    };
                };

                //////////////////////////////////////////                

                // LIST STYLES //
                if (styles.Count > 0)
                {
                    existingStylesIcon.ImageSize = new Size(16, 16);
                    XmlNode sh = kml.file.kmlDoc.SelectSingleNode("kml/Document/style_history");
                    string sht = sh == null ? "" : sh.InnerText;
                    RenameDat rd = RenameDat.CreateForWPT(sht, kml.file.tmp_file_dir + @"images\");
                    rd.listView2.SmallImageList = existingStylesIcon;
                    rd.imm = imm;
                    for (int i = 0; i < styles.Count; i++)
                    {
                        ListViewItem lvi = new ListViewItem(styles[i]);
                        lvi.SubItems.Add(((WPTPOI.SymbolIcon)new_styles[i]).ToString());
                        rd.listView2.Items.Add(lvi);
                        lvi.ImageIndex = i;
                    };
                    rd.Autodetect();
                    if (rd.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < styles.Count; i++)
                            new_styles[i] = rd.nlTexts.IndexOf(rd.listView2.Items[i].SubItems[1].Text);
                        imm = rd.imm;
                    }
                    else
                    {
                        rd.Dispose();
                        sfd.Dispose();
                        return;
                    };
                    bool sort = rd.DoSort;
                    bool remd = rd.RemoveDescriptions;
                    rd.Dispose();

                    // PROCESS //
                    AddToLog("Saving points to WPT...");
                    List<WPTPOI> poi = new List<WPTPOI>();
                    for (int x = 0; x < xns.Count; x++)
                    {
                        string[] llz = xns[x].ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        string name = xns[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value.Replace(",", ";");
                        string style = "none";
                        XmlNode stn = xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                        if ((stn != null) && (stn.ChildNodes.Count > 0))
                            style = stn.ChildNodes[0].Value;
                        int icon = styles.IndexOf(style) < 0 ? 0 : new_styles[styles.IndexOf(style)];
                        string desc = "";
                        XmlNode std = xns[x].ParentNode.ParentNode.SelectSingleNode("description");
                        if ((std != null) && (std.ChildNodes.Count > 0))
                            desc = std.ChildNodes[0].Value;

                        bool toTop = false;
                        if (!String.IsNullOrEmpty(desc))
                        {
                            string dtl = desc.ToLower();
                            if (dtl.IndexOf("progorod_dat_home=yes") >= 0) toTop = true;
                            if (dtl.IndexOf("progorod_dat_home=1") >= 0) toTop = true;
                            if (dtl.IndexOf("progorod_dat_home=true") >= 0) toTop = true;
                            if (dtl.IndexOf("progorod_dat_office=yes") >= 0) toTop = true;
                            if (dtl.IndexOf("progorod_dat_office=1") >= 0) toTop = true;
                            if (dtl.IndexOf("progorod_dat_office=true") >= 0) toTop = true;
                            dtl = (new Regex(@"[\w]+=[\S\s][^\r\n]+")).Replace(dtl, ""); // Remove TAGS
                        };
                        if (remd) desc = "";

                        WPTPOI p = new WPTPOI();
                        p.Name = name;
                        p.Description = desc;
                        p.Latitude = double.Parse(llz[1].Replace("\r", "").Replace("\n", "").Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture);
                        p.Longitude = double.Parse(llz[0].Replace("\r", "").Replace("\n", "").Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture);
                        p.Symbol = icon;
                        p.__toTop = toTop;
                        if (toTop)
                            poi.Insert(0, p);
                        else
                            poi.Add(p);
                    };

                    if (sort)
                        poi.Sort(new WPTPOI.WPTPOISorter());
                    WPTPOI.WriteFile(filename, poi.ToArray(), this.Text);
                    if (imm.save2file) imm.Save(filename + ".imm");

                    AddToLog("Saved " + poi.Count.ToString() + " points");
                    AddToLog(String.Format("Saving data to selected file: {0}", filename));
                    AddToLog("Done");
                };
                //////////////////////////////////////////
                return;
            };

            AddToLog("File not created: Layer has no placemarks to save in wpt format!");
            MessageBox.Show("Layer has no placemarks to save in wpt format!", "File not created", MessageBoxButtons.OK, MessageBoxIcon.Information);                        
        }

        private void saveITNConverter_click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;
            string tmpfn = l.file.tmp_file_dir + "temp.kml";

            Save2KML(tmpfn, l);
            AddToLog("Loafing file: `" + tmpfn + "` in ITN Converter");
            try
            {
                System.Diagnostics.Process.Start(CurrentDirectory() + @"ITNConv.exe", tmpfn);
                AddToLog("Done");
            }
            catch 
            {
                AddToLog("Error");
            };            
        }

        private void viewContentToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            if (kmzLayers.SelectedIndices.Count == 0) return;

            waitBox.Show("Wait", "Loading placemarks...");
            ContentViewer cv = new ContentViewer(this, waitBox);
            for (int i = 0; i < kmzLayers.Items.Count; i++)
                cv.laySelect.Items.Add(kmzLayers.Items[i].ToString());
            cv.laySelect.SelectedIndex = kmzLayers.SelectedIndices[0];
            waitBox.Hide();
            cv.ShowDialog();
            cv.Dispose();
        }

        private void splitLayerByIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select KMZ file name";
            sfd.Filter = "KMZ Files (*.kmz)|*.kmz";
            sfd.DefaultExt = ".kmz";
            sfd.FileName = "noname.kmz";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Save2SplittedIcons(sfd.FileName, l);
                ReloadXMLOnly_NoUpdateLayers();                
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

        [DllImport("msvcrt.dll")]
        private static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        public static bool CompareMemCmp(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) != (b2 == null)) return false;
            if (b1.Size != b2.Size) return false;

            BitmapData bd1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData bd2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                IntPtr bd1scan0 = bd1.Scan0;
                IntPtr bd2scan0 = bd2.Scan0;

                int stride = bd1.Stride;
                int len = stride * b1.Height;

                return memcmp(bd1scan0, bd2scan0, len) == 0;
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);
            }
        }

        private void contextMenuStrip2_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            saveURLIcons.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            contextMenuStrip3.Show(button2, 0, 0);
        }

        private void saveCheckedLayersToKMZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedItems.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select KMZ file name";
            sfd.Filter = "KMZ Files (*.kmz)|*.kmz";
            sfd.DefaultExt = ".kmz";
            sfd.FileName = "noname.kmz";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                Save2KMZ(sfd.FileName, true);
                ReloadXMLOnly_NoUpdateLayers();
                AddToLog("Done");
            };
            sfd.Dispose();            
        }

        private void prepareGarminXMLForGPIBuilderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndices.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select ZIP file for GML project";
            sfd.Filter = "ZIP archives (*.zip)|*.zip";
            sfd.DefaultExt = ".zip";
            sfd.FileName = "nonameGML.zip";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                Save2GML(sfd.FileName, (KMFile)kmzFiles.SelectedItems[0]);
            };
            sfd.Dispose();
        }

        private void contextMenuStrip3_Opening(object sender, CancelEventArgs e)
        {
            saveToCSVHTMLReportToolStripMenuItem.Enabled = saveBTNG.Enabled = saveBtnKMZO.Enabled = saveBtnKMZM.Enabled = kmzLayers.CheckedItems.Count > 0;
            exportToWPTToolStripMenuItem.Enabled = kmzLayers.CheckedItems.Count > 0;
            export2DatToolStripMenuItem.Enabled = kmzLayers.CheckedIndices.Count > 0;
            export2GDBToolStripMenuItem.Enabled = kmzLayers.CheckedIndices.Count > 0;
            export2WPTnoIconsToolStripMenuItem.Enabled = kmzLayers.CheckedIndices.Count > 0;
            convertToGarminPointsOfInterestsFileGPIToolStripMenuItem.Enabled = saveBTNG.Enabled;
            c2DGPIToolStripMenuItem.Enabled = saveBTNG.Enabled;
        }

        private void saveBtnKMZO_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedItems.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select KMZ file name";
            sfd.Filter = "KMZ Files (*.kmz)|*.kmz";
            sfd.DefaultExt = ".kmz";
            sfd.FileName = "noname.kmz";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                Save2KMZ(sfd.FileName, false);
                ReloadXMLOnly_NoUpdateLayers();
                AddToLog("Done");
            };
            sfd.Dispose();            
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            panel1.Visible = false;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Get a handle to a copy of this form's system (window) menu
            IntPtr hSysMenu = GetSystemMenu(this.Handle, false);
            AppendMenu(hSysMenu, MF_SEPARATOR, 0, string.Empty);
            AppendMenu(hSysMenu, MF_STRING, SYSMENU_WGSFormX, "Lat && Lon Converter ...");
            AppendMenu(hSysMenu, MF_STRING, SYSMENU_NEW_INST, "Run New Instance ...");
            AppendMenu(hSysMenu, MF_SEPARATOR, 0, string.Empty);
            AppendMenu(hSysMenu, MF_STRING, SYSMENU_DefSize, "Default Window Size");
            AppendMenu(hSysMenu, MF_STRING, SYSMENU_MinSize, "Minimum Window Size");
            AppendMenu(hSysMenu, MF_SEPARATOR, 0, string.Empty);
            AppendMenu(hSysMenu, MF_STRING, SYSMENU_ABOUT_ID, "&About…");
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m); 

            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == SYSMENU_MinSize))
            {
                this.Width = 600;
                this.Height = 400;
            };
            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == SYSMENU_DefSize))
            {
                this.Width = 900;
                this.Height = 600;
            };
            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == SYSMENU_WGSFormX))
            {
                (new KMZRebuilder.WGSFormX()).Show(this);
            };
            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == SYSMENU_ABOUT_ID))
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                string text = fvi.ProductName + " " + fvi.FileVersion + " by " + fvi.CompanyName + "\r\n";
                text += fvi.LegalCopyright + "\r\n";
                text += "\r\n-- with GPI Direct Import/Export Support --";                
                text += "\r\n-- with dkxce Route Engine Support --";
                text += "\r\n-- with OSRM Engine Support --";
                text += "\r\n-- with MapsForge File Support --";
                text += "\r\n-- with OSM POI File Support --";
                text += "\r\n-- support Raster MBTiles --\r\n";
                try
                {
                    string[] dnst = DNS.DNSLookUp.Get_TXT("kmztools.dkxce.linkpc.net");
                    foreach (string dt in dnst)
                        if (dt.StartsWith("reb: about:"))
                            text += "\r\n" + dt.Substring(11).Trim();
                }
                catch (Exception ex) 
                { 
                };
                MessageBox.Show(text, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == SYSMENU_NEW_INST))
            {
                string fn = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                System.Diagnostics.Process.Start(fn);
            };
        }

        private void saveBTNG_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedItems.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select ZIP file for GML project";
            sfd.Filter = "ZIP archives (*.zip)|*.zip";
            sfd.DefaultExt = ".zip";
            sfd.FileName = "nonameGML.zip";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                string zdir = Save2KMZ(sfd.FileName, true);
                ReloadXMLOnly_NoUpdateLayers();
                KMFile kmf = KMFile.FromZDir(zdir);
                Save2GML(sfd.FileName, kmf);
            };
            sfd.Dispose();   
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            contextMenuStrip4.Show(pictureBox1, 0, 0);
        }

        private void openFileAndConvertItToGarminXMLProjectForGPICreatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fIn = "";
            string fOut = "";

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Input file";
            ofd.DefaultExt = ".kmz";
            ofd.Filter = "KML,KMZ,GPX Files (*.kml;*.kmz;*.gpx)|*.kml;*.kmz;*.gpx";
            if (ofd.ShowDialog() == DialogResult.OK) fIn = ofd.FileName;
            ofd.Dispose();

            if (fIn == "") return;

            KMFile kmf = null;
            try
            {
                kmf = new KMFile(fIn);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select ZIP file for GML project";
            sfd.Filter = "ZIP archives (*.zip)|*.zip";
            sfd.DefaultExt = ".zip";
            sfd.FileName = "nonameGML.zip";
            if (sfd.ShowDialog() == DialogResult.OK) fOut = sfd.FileName;
            sfd.Dispose();

            if (fOut == "") return;
            try
            {
                log.Text = "";
                Save2GML(fOut, kmf);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }; 
        }

        private void openGarminXMLFileAndRenameLayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Garmin XML file";
            ofd.DefaultExt = ".xml";
            ofd.Filter = "XML files (*.xml)|*.xml";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    GMLayRenamerForm ren = new GMLayRenamerForm(ofd.FileName);
                    ren.ShowDialog();
                    ren.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            ofd.Dispose();
        }

        private void grabRussianhighwaysruajaxgetpointsphpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string url = "http://russianhighways.ru/ajax/getpoints.php?sid=24";
            if (InputBox("Grab russianhishways.ru", "Url:", ref url) != DialogResult.OK) return;
            string file = RussianHighwaysAjaxImporter.ParsePageAndSave(url);
            //if (File.Exists(file))
            //{
            //    DialogResult dr = MessageBox.Show("Do you want to add saved file to file list?", "Grab russianhishways.ru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //    if (dr == DialogResult.No) return;
            //    LoadFiles(new string[] { file });
            //};
        }

        private void kmFlagNumeratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KMNumeratorForm kmfn = new KMNumeratorForm();
            kmfn.ShowDialog();
            kmfn.Dispose();
        }

        private void benzinPriceruAnalyzerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BenzinPriceAnalizer.BenzinPriceAnalizerForm bpaf = new BenzinPriceAnalizer.BenzinPriceAnalizerForm();
            bpaf.ShowDialog();
            bpaf.Dispose();
        }

        private void iTNConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if(File.Exists(CurrentDirectory() + @"\ITNConv.exe"))
                    System.Diagnostics.Process.Start(CurrentDirectory() + @"\ITNConv.exe");
            }
            catch { };
        }

        private void convertImageTo22X22BMP8BppIndexed256ColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Source File";
            ofd.DefaultExt = ".png";
            ofd.Filter = "Popular Image Types (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.tiff)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.tiff|All file types (*.*)|*.*";
            string of = "";
            if (ofd.ShowDialog() == DialogResult.OK) of = ofd.FileName;
            ofd.Dispose();
            if (!File.Exists(of)) return;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select Destination File";
            sfd.DefaultExt = ".bmp";
            sfd.Filter = "BMP (*.bmp)|*.bmp";
            string sf = "";
            if (sfd.ShowDialog() == DialogResult.OK) sf = sfd.FileName;
            sfd.Dispose();
            if (sf == "") return;
            ConvertImageToBmp8bppIndexed(of, sf);
        }

        private void grabMprussianhighwaysru10443esb10cxfpoiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string urlT = "https://mp.russianhighways.ru:10443//esb/1.0/cxf/poi/types?lang=EN";
            string urlP = "https://mp.russianhighways.ru:10443/esb/1.0/cxf/poi/objects?north=90&south=0&west=0&east=180";
            if (InputBox("Grab mp.russianhishways.ru", "Enter Url for POI Types:", ref urlT) != DialogResult.OK) return;
            if (InputBox("Grab mp.russianhishways.ru", "Enter Url for POIs:", ref urlP) != DialogResult.OK) return;
            string file = RussianHighwaysESBImporter.ParsePageAndSave(urlT, urlP);
            //if (File.Exists(file))
            //{
            //    DialogResult dr = MessageBox.Show("Do you want to add saved file to file list?", "Grab russianhishways.ru", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //    if (dr == DialogResult.No) return;
            //    LoadFiles(new string[] { file });
            //};
        }

        private void applyFilterSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            if (kmzFiles.SelectedIndices.Count == 0) return;                        
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            Selection_Filter sfw = new Selection_Filter(this, f, null);
            sfw.ShowDialog();
            sfw.Dispose();
        }

        internal void ReloadAfterFilter(KMFile kmlfile, KMLayer kmllayer)
        {
            kmlfile.SaveKML();
            if (kmllayer == null)
            {
                // reload all layers

                // variant 1 //
                //kmlfile.SaveKML();
                //kmlfile.LoadKML(true);

                // variant 2 //
                //foreach (KMLayer kl in kmlfile.kmLayers)
                //{
                //    XmlNodeList nl2 = kmlfile.kmlDoc.SelectNodes("kml/Document/Folder")[kl.id].SelectNodes("Placemark");
                //    kl.placemarks = (nl2 == null ? 0 : nl2.Count);
                //};

                // variant 3 //
                for (int i = 0; i < kmzLayers.Items.Count; i++)
                {
                    KMLayer ll = (KMLayer)kmzLayers.Items[i];
                    XmlNodeList nl2 = ll.file.kmlDoc.SelectNodes("kml/Document/Folder")[ll.id].SelectNodes("Placemark");
                    ll.placemarks = (nl2 == null ? 0 : nl2.Count);
                    ll.points = ll.file.kmlDoc.SelectNodes("kml/Document/Folder")[ll.id].SelectNodes("Placemark/Point").Count;
                    ll.lines = ll.file.kmlDoc.SelectNodes("kml/Document/Folder")[ll.id].SelectNodes("Placemark/LineString").Count;
                    ll.areas = ll.file.kmlDoc.SelectNodes("kml/Document/Folder")[ll.id].SelectNodes("Placemark/Polygon").Count;
                    if (ll.placemarks == 0)
                        kmzLayers.SetItemChecked(i, false);
                };

                ReloadListboxLayers(true);
                waitBox.Hide();
            }
            else
            {
                // reload selected layer
                XmlNodeList nl2 = kmllayer.file.kmlDoc.SelectNodes("kml/Document/Folder")[kmllayer.id].SelectNodes("Placemark");
                kmllayer.placemarks = (nl2 == null ? 0 : nl2.Count);
                kmllayer.points = kmllayer.file.kmlDoc.SelectNodes("kml/Document/Folder")[kmllayer.id].SelectNodes("Placemark/Point").Count;
                kmllayer.lines = kmllayer.file.kmlDoc.SelectNodes("kml/Document/Folder")[kmllayer.id].SelectNodes("Placemark/LineString").Count;
                kmllayer.areas = kmllayer.file.kmlDoc.SelectNodes("kml/Document/Folder")[kmllayer.id].SelectNodes("Placemark/Polygon").Count;
                ReloadListboxLayers(true);
                waitBox.Hide();
            };
        }

        private void mapPolygonCreatorYouCanUseItForToolStripMenuItem_Click(object sender, EventArgs e)
        {
            waitBox.Show("Wait", "Loading map...");                        
            PolyCreator pc = new PolyCreator(this, waitBox);
            waitBox.Hide();
            pc.ShowDialog();
            pc.Dispose();
            return;
        }

        private void asf2_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;

            Selection_Filter sfw = new Selection_Filter(this, l.file, l);
            sfw.ShowDialog();
            sfw.Dispose();
        }

        private void slbn_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select KMZ file name";
            sfd.Filter = "KMZ Files (*.kmz)|*.kmz";
            sfd.DefaultExt = ".kmz";
            sfd.FileName = "noname.kmz";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Save2SplittedNames(sfd.FileName, l, 0, null);
                ReloadXMLOnly_NoUpdateLayers();
            };
            sfd.Dispose();
        }

        private void slb2_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select KMZ file name";
            sfd.Filter = "KMZ Files (*.kmz)|*.kmz";
            sfd.DefaultExt = ".kmz";
            sfd.FileName = "noname.kmz";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Save2SplittedNames(sfd.FileName, l, 1, null);
                ReloadXMLOnly_NoUpdateLayers();
            };
            sfd.Dispose();
        }

        private void slb3_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;

            string ttf = "Petrol";
            KMZRebuilederForm.InputBox("Found Text", "Enter found text with `;` as delimiter:", ref ttf, null);
            string[] toFindT = ttf.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select KMZ file name";
            sfd.Filter = "KMZ Files (*.kmz)|*.kmz";
            sfd.DefaultExt = ".kmz";
            sfd.FileName = "noname.kmz";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Save2SplittedNames(sfd.FileName, l, 1, toFindT);
                ReloadXMLOnly_NoUpdateLayers();
            };
            sfd.Dispose();
        }

        private void akelPadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if(File.Exists(CurrentDirectory() + @"\AkelPad.exe"))
                    System.Diagnostics.Process.Start(CurrentDirectory() + @"\AkelPad.exe");
            }
            catch { };
        }

        private void saveJSON(KMLayer layer, KMFile file)
        {
            string path = file.tmp_file_dir + "doc.json";
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.Write("[");
            {
                bool first = true;
                List<KMLayer> lays = new List<KMLayer>();
                if (layer != null)
                    lays.Add(layer);
                else
                    lays.AddRange(file.kmLayers);
                foreach (KMLayer kml in lays)
                {
                    XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
                    XmlNodeList xns = xn.SelectNodes("Placemark");
                    if (xns.Count > 0)
                        for (int x = 0; x < xns.Count; x++)
                        {
                            if ((xns[x].SelectSingleNode("Point/coordinates") == null) && (xns[x].SelectSingleNode("LineString/coordinates") == null))
                                continue;

                            if (first)
                                first = false;
                            else
                                sw.Write(",");

                            sw.Write("{");
                            string nam = xns[x].SelectSingleNode("name").ChildNodes[0].Value.Replace("\r", "").Replace("\n", "").Trim().Replace("'", "'");
                            nam = System.Security.SecurityElement.Escape(nam);
                            sw.Write("name:\'" + nam + "\'");
                            sw.Write(",layer:\'" + System.Security.SecurityElement.Escape(kml.name) + "\'");
                            string xy = "";
                            if(xns[x].SelectSingleNode("Point/coordinates") != null)
                                xy = xns[x].SelectSingleNode("Point/coordinates").ChildNodes[0].Value.Replace("\r", "").Replace("\n", "").Trim();
                            else
                                xy = xns[x].SelectSingleNode("LineString/coordinates").ChildNodes[0].Value.Replace("\r", "").Replace("\n", "").Replace(" ",",").Trim();
                            sw.Write(",xy:\'" + xy + "\'");
                            XmlNode nsm = xns[x].SelectSingleNode("styleUrl");
                            if (nsm != null)
                            {
                                string stname = nsm.ChildNodes[0].Value;
                                if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);                                
                                if (xns[x].SelectSingleNode("Point/coordinates") != null)
                                {
                                    XmlNode sn;
                                    sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + stname + "']/IconStyle/Icon/href");
                                    if(sn != null)
                                        sw.Write(",icon:\'" + sn.InnerText + "\'");
                                    sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/StyleMap[@id='" + stname + "']/Pair/styleUrl");
                                    if (sn != null)
                                    {
                                        sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + sn.InnerText.Substring(1) + "']/IconStyle/Icon/href");
                                        if (sn != null)
                                            sw.Write(",icon:\'" + sn.InnerText + "\'");
                                    };
                                };
                                if (xns[x].SelectSingleNode("LineString/coordinates") != null)
                                {
                                    XmlNode sn;
                                    sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + stname + "']");
                                    if (sn == null)
                                    {
                                        sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/StyleMap[@id='" + stname + "']/Pair/styleUrl");
                                        if (sn != null)
                                            stname = sn.InnerText.Substring(1);
                                    };
                                    sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + stname + "']/LineStyle/color");
                                    if (sn != null)
                                    {
                                        sw.Write(",color:\'#" + sn.InnerText.Substring(6, 2) + sn.InnerText.Substring(4, 2) + sn.InnerText.Substring(2, 2) + "\'");
                                        sw.Write(",opacity:\'" + (((double)Convert.ToUInt32(sn.InnerText.Substring(0, 2),16))/256.0).ToString("0.00",System.Globalization.CultureInfo.InvariantCulture) + "\'");
                                    };
                                    sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + stname + "']/LineStyle/width");
                                    if (sn != null)
                                        sw.Write(",width:\'" + (int.Parse(sn.InnerText)+2).ToString() + "\'");
                                };
                            };
                            XmlNode desc = xns[x].SelectSingleNode("description");
                            if ((desc != null) && (desc.ChildNodes.Count > 0))
                            {
                                string d = desc.ChildNodes[0].Value.Replace("\r", " ").Replace("\n", " ").Trim().Replace("'", "'");
                                d = System.Security.SecurityElement.Escape(d);
                                sw.Write(",desc:\'" + d + "\'");
                            };
                            sw.Write("}");
                        };
                }
            };
            sw.Write("]");
            sw.Close();
            fs.Close();
        }

        private void viewInWebBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;
            string tmpf = l.file.tmp_file_dir.Substring(l.file.tmp_file_dir.LastIndexOf("\\IF")).Replace("\\", "");
            tmpf = "file:///" + CurrentDirectory() + @"viewonmap.html#" + tmpf;

            saveJSON(l, l.file);
            
            System.Diagnostics.Process p = null;
            try { p = System.Diagnostics.Process.Start("firefox", "\"" + tmpf + "\""); }
            catch { };
            if (p == null) try { p = System.Diagnostics.Process.Start("chrome", "\"" + tmpf + "\""); }
                catch { };
            if (p == null) try { p = System.Diagnostics.Process.Start("iexplore", "\"" + tmpf + "\""); }
                catch { };
            if (p == null) try { p = System.Diagnostics.Process.Start(tmpf); }
                catch { };
        }

        private void viewInWebBrowserToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndices.Count == 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string tmpf = f.tmp_file_dir.Substring(f.tmp_file_dir.LastIndexOf("\\IF")).Replace("\\", "");
            tmpf = "file:///" + CurrentDirectory() + @"viewonmap.html#" + tmpf;

            saveJSON(null, f);

            System.Diagnostics.Process p = null;
            try { p = System.Diagnostics.Process.Start("firefox", "\"" + tmpf + "\""); }
            catch { };
            if (p == null) try { p = System.Diagnostics.Process.Start("chrome", "\"" + tmpf + "\""); }
                catch { };
            if (p == null) try { p = System.Diagnostics.Process.Start("iexplore", "\"" + tmpf + "\""); }
                catch { };
            if (p == null) try { p = System.Diagnostics.Process.Start(tmpf); }
                catch { };
        }

        private void exportHTMLMapwithIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndices.Count == 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select ZIP file name";
            sfd.Filter = "ZIP Files (*.zip)|*.zip";
            sfd.DefaultExt = ".zip";
            sfd.FileName = "noname.zip";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                Export2HTML(f, sfd.FileName);
                ReloadXMLOnly_NoUpdateLayers();
                AddToLog("Done");
            };
            sfd.Dispose();       
            
        }

        private string Export2HTML(KMFile f, string filename)
        {
            waitBox.Show("Export", "Wait, exporing file...");            

            string zdir = KMZRebuilederForm.TempDirectory() + "OF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            AddToLog("Create Temp Folder: " + zdir);
            System.IO.Directory.CreateDirectory(zdir);
            System.IO.Directory.CreateDirectory(zdir + @"\images\");
            System.IO.Directory.CreateDirectory(zdir + @"\js\");
            AddToLog("Creating HTML file...");
            AddToLog("Create HTML File: " + zdir + "map.html");

            string objs = "";
            objs += ("[");
            {
                bool first = true;
                List<KMLayer> lays = new List<KMLayer>();
                lays.AddRange(f.kmLayers);
                foreach (KMLayer kml in lays)
                {
                    XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
                    XmlNodeList xns = xn.SelectNodes("Placemark");
                    if (xns.Count > 0)
                        for (int x = 0; x < xns.Count; x++)
                        {
                            if ((xns[x].SelectSingleNode("Point/coordinates") == null) && (xns[x].SelectSingleNode("LineString/coordinates") == null))
                                continue;

                            if (first)
                                first = false;
                            else
                                objs += (",");

                            objs += ("{");
                            string nam = xns[x].SelectSingleNode("name").ChildNodes[0].Value.Replace("\r", "").Replace("\n", "").Trim().Replace("'", "'");
                            nam = System.Security.SecurityElement.Escape(nam);
                            objs += ("name:\'" + nam + "\'");
                            objs += (",layer:\'" + System.Security.SecurityElement.Escape(kml.name) + "\'");
                            string xy = "";
                            if (xns[x].SelectSingleNode("Point/coordinates") != null)
                                xy = xns[x].SelectSingleNode("Point/coordinates").ChildNodes[0].Value.Replace("\r", "").Replace("\n", "").Trim();
                            else
                                xy = xns[x].SelectSingleNode("LineString/coordinates").ChildNodes[0].Value.Replace("\r", "").Replace("\n", "").Replace(" ", ",").Trim();
                            objs += (",xy:\'" + xy + "\'");
                            XmlNode nsm = xns[x].SelectSingleNode("styleUrl");
                            if (nsm != null)
                            {
                                string stname = nsm.ChildNodes[0].Value;
                                if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
                                if (xns[x].SelectSingleNode("Point/coordinates") != null)
                                {
                                    XmlNode sn;
                                    sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + stname + "']/IconStyle/Icon/href");
                                    if (sn != null)
                                        objs += (",icon:\'" + sn.InnerText + "\'");
                                    sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/StyleMap[@id='" + stname + "']/Pair/styleUrl");
                                    if (sn != null)
                                    {
                                        sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + sn.InnerText.Substring(1) + "']/IconStyle/Icon/href");
                                        if (sn != null)
                                            objs += (",icon:\'" + sn.InnerText + "\'");
                                    };
                                };
                                if (xns[x].SelectSingleNode("LineString/coordinates") != null)
                                {
                                    XmlNode sn;
                                    sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + stname + "']");
                                    if (sn == null)
                                    {
                                        sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/StyleMap[@id='" + stname + "']/Pair/styleUrl");
                                        if (sn != null)
                                            stname = sn.InnerText.Substring(1);
                                    };
                                    sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + stname + "']/LineStyle/color");
                                    if (sn != null)
                                    {
                                        objs += (",color:\'#" + sn.InnerText.Substring(6, 2) + sn.InnerText.Substring(4, 2) + sn.InnerText.Substring(2, 2) + "\'");
                                        objs += (",opacity:\'" + (((double)Convert.ToUInt32(sn.InnerText.Substring(0, 2), 16)) / 256.0).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "\'");
                                    };
                                    sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + stname + "']/LineStyle/width");
                                    if (sn != null)
                                        objs += (",width:\'" + (int.Parse(sn.InnerText) + 2).ToString() + "\'");
                                };
                            };
                            XmlNode desc = xns[x].SelectSingleNode("description");
                            if ((desc != null) && (desc.ChildNodes.Count > 0))
                            {
                                string d = desc.ChildNodes[0].Value.Replace("\r", " ").Replace("\n", " ").Trim().Replace("'", "'");
                                d = System.Security.SecurityElement.Escape(d);
                                objs += (",desc:\'" + d + "\'");
                            };
                            objs += ("}");
                        };
                }
            };
            objs += ("]");

            FileStream fs = new FileStream(CurrentDirectory() + @"\viewonmap.tml", FileMode.Open,FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string data = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            data = data.Replace("<title>KMZ Map</title>", "<title>" + System.Security.SecurityElement.Escape(f.kmldocName) + "</title>");
            data = data.Replace("GetObjects([]);", "GetObjects(" + objs + ");");
            fs = new FileStream(zdir + "map.html", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(data);
            sw.Close();
            sr.Close();

            AddToLog("Copying map files...");
            foreach (string dirPath in Directory.GetDirectories(CurrentDirectory() + @"\js", "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(CurrentDirectory() + @"\js", zdir + @"\js"));
            foreach (string newPath in Directory.GetFiles(CurrentDirectory() + @"\js", "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(CurrentDirectory() + @"\js", zdir + @"\js"), true);
            AddToLog("Copying icons...");
            foreach (string dirPath in Directory.GetDirectories(f.tmp_file_dir + @"\images", "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(f.tmp_file_dir + @"\images", zdir + @"\images"));
            foreach (string newPath in Directory.GetFiles(f.tmp_file_dir + @"\images", "*.*", SearchOption.AllDirectories))
            {
                string tpath = newPath.Replace(f.tmp_file_dir + @"\images", zdir + @"\images");
                File.Copy(newPath, tpath, true);
                ImageMagick.MagickImage im = new ImageMagick.MagickImage(tpath);
                if (im.Width > 16)
                {
                    im.Resize(16, 16);
                    im.Write(tpath);
                };
                im.Dispose();
            };
            
            AddToLog(String.Format("Creating file: {0}", filename));
            CreateZIP(filename, zdir);
            waitBox.Hide();

            return zdir;
        }

        private void gPX2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "GPX Files (*.gpx)|*.gpx";
            sfd.DefaultExt = ".gpx";
            try
            {
                sfd.FileName = l.name + ".gpx";
            }
            catch { };
            if (sfd.ShowDialog() == DialogResult.OK)
                Save2GPX(sfd.FileName, l, true, false);
            sfd.Dispose();
        }

        private void gPX3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "GPX Files (*.gpx)|*.gpx";
            sfd.DefaultExt = ".gpx";
            try
            {
                sfd.FileName = l.name + ".gpx";
            }
            catch { };
            if (sfd.ShowDialog() == DialogResult.OK)
                Save2GPX(sfd.FileName, l, false, true);
            sfd.Dispose();
        }

        private void shapeViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if(File.Exists(CurrentDirectory() + @"\ShapeViewer.exe"))
                    System.Diagnostics.Process.Start(CurrentDirectory() + @"\ShapeViewer.exe");
            }
            catch { };
        }

        private void interpolateWayToLessPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            waitBox.Show("Wait", "Loading map...");
            InterLessForm pc = new InterLessForm(this, waitBox);
            waitBox.Hide();
            pc.ShowDialog();
            pc.Dispose();
            return;
        }

        private void viewInKMLViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndices.Count == 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;

            IntPtr vh = IntPtr.Zero;
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\", true)
                    .CreateSubKey("KMZViewer"))
                    vh = (IntPtr)((int)key.GetValue("Handle"));
                if (vh != IntPtr.Zero)
                {
                    string wt = SASPlacemarkConnector.GetText(vh);
                    if ((!String.IsNullOrEmpty(wt)) && (wt.IndexOf("KMZ Viewer") == 0))
                    {
                        ProcDataExchange.SendData(vh, this.Handle, XP_OPENFILE, f.src_file_pth);
                        return;
                    };
                };
            }
            catch (Exception ex) 
            { 
            };

            if (File.Exists(CurrentDirectory() + @"KMZViewer.exe"))
                System.Diagnostics.Process.Start(CurrentDirectory() + @"KMZViewer.exe", "\"" + f.src_file_pth + "\"");
            else
                MessageBox.Show("KMZViewer Not Found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void viewHTMLCSMap_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndices.Count == 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            if (f.src_file_ext.ToLower() != ".gpx") return;

            waitBox.Show("Loading", "Wait, preparing file...");  

            string tmpf = CurrentDirectory() + @"viewspeedmap.html";
            GPXReader.GPX2WebSpeedMap(f.src_file_pth, tmpf);
            tmpf = "file:///" + tmpf;

            System.Diagnostics.Process p = null;
            try { p = System.Diagnostics.Process.Start("firefox", "\"" + tmpf + "\""); }
            catch { };
            if (p == null) try { p = System.Diagnostics.Process.Start("chrome", "\"" + tmpf + "\""); }
                catch { };
            if (p == null) try { p = System.Diagnostics.Process.Start("iexplore", "\"" + tmpf + "\""); }
                catch { };
            if (p == null) try { p = System.Diagnostics.Process.Start(tmpf); }
                catch { };
            waitBox.Hide();
        }

        private void gpxexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndices.Count == 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            if (f.src_file_ext.ToLower() != ".gpx") return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select ZIP file name";
            sfd.Filter = "ZIP Files (*.zip)|*.zip";
            sfd.DefaultExt = ".zip";
            sfd.FileName = "noname.zip";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                GPX2HTML(f, sfd.FileName);
                AddToLog("Done");
            };
            sfd.Dispose();       
        }

        private string GPX2HTML(KMFile f, string filename)
        {
            waitBox.Show("Export", "Wait, exporing file...");

            string zdir = KMZRebuilederForm.TempDirectory() + "OF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            AddToLog("Create Temp Folder: " + zdir);
            System.IO.Directory.CreateDirectory(zdir);
            System.IO.Directory.CreateDirectory(zdir + @"\js\");

            AddToLog("Creating HTML file...");
            AddToLog("Create HTML File: " + zdir + "speedmap.html");
            GPXReader.GPX2WebSpeedMap(f.src_file_pth, zdir + "speedmap.html");

            AddToLog("Copying map files...");
            foreach (string dirPath in Directory.GetDirectories(CurrentDirectory() + @"\js", "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(CurrentDirectory() + @"\js", zdir + @"\js"));
            foreach (string newPath in Directory.GetFiles(CurrentDirectory() + @"\js", "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(CurrentDirectory() + @"\js", zdir + @"\js"), true);
            
            AddToLog(String.Format("Creating file: {0}", filename));
            CreateZIP(filename, zdir);
            waitBox.Hide();

            return zdir;
        }

        private void gPXTachographloadGPXAndViewOnCircleGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GPX_Tacho.GPXTachograph taxo = new GPX_Tacho.GPXTachograph();
            taxo.ShowDialog();
            taxo.Dispose();
        }

        private void reloadAsColorSpeedTrackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndices.Count == 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            if (f.src_file_ext.ToLower() != ".gpx") return;
            string path = f.tmp_file_dir + "doc.kml";

            waitBox.Show("Reloading", "Wait, reloading colored track...");
            GPXReader.GPX2ColorKML(f.src_file_pth, path);
            f.LoadKML(true);
            f.DrawEvenSizeIsTooSmall = true;
            ReloadListboxLayers(true);
            waitBox.Hide();
        }

        private void pROGORODPOIEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DAT2SCV_AND_BACK();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "DAT <--> CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
        }
        private void DAT2SCV_AND_BACK()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select file";
            ofd.Filter = "CSV and DAT files (*.csv;*.dat)|*.csv;*.dat";
            ofd.DefaultExt = ".dat";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(ofd.FileName.ToLower()) == ".dat")
                {
                    ProGorodPOI.FavRecord[] recs = ProGorodPOI.ReadFile(ofd.FileName);
                    if ((recs != null) && (recs.Length > 0))
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.FileName = Path.GetFileNameWithoutExtension(ofd.FileName) + ".csv";
                        sfd.DefaultExt = ".csv";
                        sfd.Filter = "CSV files (*.csv)|*.csv";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                            sw.WriteLine("#CODEPAGE: Windows-1251");
                            sw.WriteLine("#MAINTYPES: " + ProGorodPOI.THomeOffice.None.ToString() + ", " + ProGorodPOI.THomeOffice.Home.ToString() + ", " + ProGorodPOI.THomeOffice.Office.ToString());
                            sw.Write("#ICONTYPES: " + ProGorodPOI.TType.None.ToString());
                            for (int i = 1; i < 20; i++) sw.Write(", " + i.ToString() + " - " + ((ProGorodPOI.TType)i).ToString());
                            sw.WriteLine();
                            sw.WriteLine("#TOTAL POI: " + recs.Length.ToString());
                            sw.WriteLine("ID;MAINTYPE;ICONTYPE;NAME;DESCRIPTION;PHONE;ADDRESS;LATITUDE;LONGITUDE");
                            for (int i = 0; i < recs.Length; i++)
                            {
                                sw.Write(i.ToString() + ";");
                                sw.Write(recs[i].HomeOffice.ToString() + ";");
                                sw.Write(recs[i].Icon.ToString() + ";");
                                sw.Write(recs[i].Name.Replace(";", ",").Replace("\r\n", " ") + ";");
                                sw.Write(recs[i].Desc.Replace(";", ",").Replace("\r\n", " ") + ";");
                                sw.Write(recs[i].Phone.Replace(";", ",").Replace("\r\n", " ") + ";");
                                sw.Write(recs[i].Address.Replace(";", ",").Replace("\r\n", " ") + ";");
                                sw.Write(recs[i].Lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + ";");
                                sw.Write(recs[i].Lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + ";");
                                sw.WriteLine();
                            };
                            sw.Close();
                            fs.Close();
                            MessageBox.Show("Saved " + recs.Length.ToString() + " POI to CSV","DAT --> CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        };
                        sfd.Dispose();
                    };
                }
                else if (Path.GetExtension(ofd.FileName.ToLower()) == ".csv")
                {
                    FileInfo fi = new FileInfo(ofd.FileName);
                    if (fi.Length > 70)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.FileName = Path.GetFileNameWithoutExtension(ofd.FileName) + ".dat";
                        sfd.DefaultExt = ".dat";
                        sfd.Filter = "DAT files (*.dat)|*.dat";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            List<ProGorodPOI.FavRecord> recs = new List<ProGorodPOI.FavRecord>();
                            FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
                            StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
                            bool firstline = true;
                            int[] si = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
                            char delimiter = ';';
                            while (!sr.EndOfStream)
                            {
                                string line = sr.ReadLine().Trim();
                                if (String.IsNullOrEmpty(line)) continue;
                                if (line.StartsWith("#")) continue;
                                if (firstline)
                                {
                                    firstline = false;
                                    if (line.IndexOf(',') > 0) delimiter = ',';
                                    if (line.IndexOf('\t') > 0) delimiter = '\t';
                                    if (line.IndexOf(';') > 0) delimiter = ';';
                                    string[] delimiters = new string[] { ";",",","TAB" };
                                    string del = delimiter.ToString(); if (delimiter == '\t') del = "TAB";
                                    if (System.Windows.Forms.InputBox.Show("CSV --> DAT", "Select delimiter:", delimiters, ref del) == DialogResult.OK)
                                    {
                                        if (del == "TAB") delimiter = '\t';
                                        else delimiter = del[0];

                                        List<string> dline = new List<string>(line.Split(new char[] { delimiter }, 10));
                                        if (dline.Count < 9)
                                        {
                                            MessageBox.Show("Invalid Columns Count\r\nMust Be >= 9", "CSV --> DAT", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            sr.Close();
                                            fs.Close();
                                            return;
                                        };
                                        si[0] = dline.IndexOf("ID");
                                        si[1] = dline.IndexOf("MAINTYPE");
                                        si[2] = dline.IndexOf("ICONTYPE");
                                        si[3] = dline.IndexOf("NAME");
                                        si[4] = dline.IndexOf("DESCRIPTION");
                                        si[5] = dline.IndexOf("PHONE");
                                        si[6] = dline.IndexOf("ADDRESS");
                                        si[7] = dline.IndexOf("LATITUDE");
                                        si[8] = dline.IndexOf("LONGITUDE");

                                        if (si[3] < 0) { MessageBox.Show("Field NAME not Found!", "CSV --> DAT", MessageBoxButtons.OK, MessageBoxIcon.Error); return; };
                                        if (si[7] < 0) { MessageBox.Show("Field LATITUDE not Found!", "CSV --> DAT", MessageBoxButtons.OK, MessageBoxIcon.Error); return; };
                                        if (si[8] < 0) { MessageBox.Show("Field LONGITUDE not Found!", "CSV --> DAT", MessageBoxButtons.OK, MessageBoxIcon.Error); return; };
                                        continue;
                                    };
                                    sr.Close();
                                    fs.Close();
                                    return;
                                };
                                string[] pline = line.Split(new char[] { delimiter }, 10);
                                ProGorodPOI.FavRecord rec = new ProGorodPOI.FavRecord();
                                rec.HomeOffice = ProGorodPOI.THomeOffice.None;
                                if (si[1] >= 0)
                                {
                                    if (pline[si[1]].Trim() == ProGorodPOI.THomeOffice.Home.ToString())
                                        rec.HomeOffice = ProGorodPOI.THomeOffice.Home;
                                    if (pline[si[1]].Trim() == ProGorodPOI.THomeOffice.Office.ToString())
                                        rec.HomeOffice = ProGorodPOI.THomeOffice.Office;
                                };
                                if(si[2] >= 0)
                                    for (int i = 0; i < 20; i++)
                                    {
                                        if ((pline[si[2]].Trim() == ((ProGorodPOI.TType)i).ToString()) || (pline[si[2]].Trim() == i.ToString()))
                                            rec.Icon = (ProGorodPOI.TType)i;
                                    };
                                if (si[3] >= 0)
                                {
                                    rec.Name = pline[si[3]].Trim();
                                    if (rec.Name.Length > 128) rec.Name = rec.Name.Remove(128);
                                };
                                if (si[4] >= 0)
                                {
                                    rec.Desc = pline[si[4]].Trim();
                                    if (rec.Desc.Length > 128) rec.Desc = rec.Desc.Remove(128);
                                };
                                if (si[5] >= 0)
                                {
                                    rec.Phone = pline[si[5]].Trim();
                                    if (rec.Phone.Length > 128) rec.Phone = rec.Phone.Remove(128);
                                };
                                if (si[6] >= 0)
                                {
                                    rec.Address = pline[si[6]].Trim();
                                    if (rec.Address.Length > 128) rec.Address = rec.Address.Remove(128);
                                };
                                if (si[7] >= 0)
                                    rec.Lat = double.Parse(pline[si[7]].Replace(",",".").Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                if (si[8] >= 0)
                                    rec.Lon = double.Parse(pline[si[8]].Replace(",", ".").Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                if((rec.Lat != 0) && (rec.Lon != 0) && (!String.IsNullOrEmpty(rec.Name)))
                                    recs.Add(rec);
                            };                            
                            sr.Close();
                            fs.Close();
                            if (recs.Count > 0)
                            {
                                ProGorodPOI.WriteFile(sfd.FileName, recs.ToArray());
                                MessageBox.Show("Saved " + recs.Count.ToString() + " POI to DAT", "CSV --> DAT", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            };
                        };
                        sfd.Dispose();
                    };
                }
                else
                    MessageBox.Show("Wrong File Type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            };
            ofd.Dispose();
        }

        private void addFromTXTOrCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".txt";
            ofd.Filter = "CSV and TXT files (*.csv;*.txt)|*.csv;*.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
                ImportFromText(new FileInfo(ofd.FileName));
            ofd.Dispose();
        }

        private void ImportFromText(FileInfo fi)
        {
            FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);            

            CSVTXTForm addf = new CSVTXTForm(fs);
            if (addf.ShowDialog() == DialogResult.OK)
                ImportFromText2Kml(fs, addf, fi.Name);
            addf.Dispose();                                    
            fs.Close();
        }

        private void ImportFromSHP(string fileName)
        {
            string dFN = fileName.Substring(0, fileName.Length - 4) + ".dbf";
            bool ExDBF = File.Exists(dFN);

            //Read Shape
            long fShpLength = 0;
            int fShpType = 0;
            byte[] fShpData = null;
            {
                FileStream fShp = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                fShpLength = fShp.Length;
                fShpData = new Byte[fShpLength];
                fShp.Read(fShpData, 0, (int)fShpLength);
                fShp.Close();
                fShpType = readIntLittle(fShpData, 32);
                if ((fShpType != 1) && (fShpType != 3) && (fShpType != 5)) { return; };
            };

            // DBF
            FileStream fDbf = null;
            System.Text.Encoding FileReadEncoding = System.Text.Encoding.GetEncoding(1251);
            int dbfCodePage = 1251; int dbfNameField = 0; int fDbfFieldsCount = 0;
            short fDbf_dataRecord_1st_Pos = 0; short fDbf_dataRecord_Length = 0;
            int[] Fields_Offset = null; string[] fDbf_Fields_Names = null;
            Hashtable fDbf_fieldsLength = new Hashtable(); // Массив размеров полей
            if (ExDBF)
            {
                DBF.CodePageList CodePages = new DBF.CodePageList();
                string[] options = new string[CodePages.Count];
                int idx = 43;
                for (int i = 0; i < options.Length; i++)
                {
                    options[i] = CodePages[i].ToString();
                    if (CodePages[i].codePage == 1251)
                        idx = i;
                };

                int dW = System.Windows.Forms.InputBox.defWidth;
                System.Windows.Forms.InputBox.defWidth = 500;
                if (System.Windows.Forms.InputBox.Show("Import Shape File Step 1/2", "Select Code Page:", options, ref idx) == DialogResult.OK)
                    dbfCodePage = CodePages[idx].codePage;
                else
                {
                    System.Windows.Forms.InputBox.defWidth = dW;
                    return;
                };
                System.Windows.Forms.InputBox.defWidth = dW;

                fDbf = new FileStream(dFN, FileMode.Open, FileAccess.Read);
                System.Text.Encoding.GetEncoding(dbfCodePage);

                // Read File Version
                fDbf.Position = 0;
                int ver = fDbf.ReadByte();

                // Read Records Count
                fDbf.Position = 04;
                byte[] bb = new byte[4];
                fDbf.Read(bb, 0, 4);
                int total = BitConverter.ToInt32(bb, 0);

                // Read DataRecord 1st Position  
                fDbf.Position = 8;
                bb = new byte[2];
                fDbf.Read(bb, 0, 2);
                fDbf_dataRecord_1st_Pos = BitConverter.ToInt16(bb, 0);
                fDbfFieldsCount = (((bb[0] + (bb[1] * 0x100)) - 1) / 32) - 1;

                // Read DataRecord Length
                fDbf.Position = 10;
                bb = new byte[2];
                fDbf.Read(bb, 0, 2);
                fDbf_dataRecord_Length = BitConverter.ToInt16(bb, 0);

                // Read Заголовки Полей
                fDbf.Position = 32;
                fDbf_Fields_Names = new string[fDbfFieldsCount]; // Массив названий полей                
                Hashtable fieldsType = new Hashtable();   // Массив типов полей
                byte[] Fields_Dig = new byte[fDbfFieldsCount];   // Массив размеров дробной части
                Fields_Offset = new int[fDbfFieldsCount];    // Массив отступов
                bb = new byte[32 * fDbfFieldsCount]; // Описание полей: 32 байтa * кол-во, начиная с 33-го            
                fDbf.Read(bb, 0, bb.Length);
                int FieldsLength = 0;
                for (int x = 0; x < fDbfFieldsCount; x++)
                {
                    fDbf_Fields_Names[x] = System.Text.Encoding.Default.GetString(bb, x * 32, 10).TrimEnd(new char[] { (char)0x00 }).ToUpper();
                    fieldsType.Add(fDbf_Fields_Names[x], "" + (char)bb[x * 32 + 11]);
                    fDbf_fieldsLength.Add(fDbf_Fields_Names[x], (int)bb[x * 32 + 16]);
                    Fields_Dig[x] = bb[x * 32 + 17];
                    Fields_Offset[x] = 1 + FieldsLength;
                    FieldsLength = FieldsLength + (int)fDbf_fieldsLength[fDbf_Fields_Names[x]];
                };

                // STEPS 2/2                
                for (int x = 0; x < fDbfFieldsCount; x++)
                {
                    string fup = fDbf_Fields_Names[x].ToUpper();
                    if (fup == "NAME") dbfNameField = x;
                    if (fup == "LABEL") dbfNameField = x;                    
                };
                if (System.Windows.Forms.InputBox.Show("Import Shape File Step 2/2", "Select Name Field:", fDbf_Fields_Names, ref dbfNameField) != DialogResult.OK) { fDbf.Close(); return; };                
            };

            //READ FILES
            KMFile f = KMFile.CreateEmpty();            

            waitBox.Show("Import Shape", "Wait, reading file ...");
            using (FileStream outFile = new FileStream(f.src_file_pth, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(outFile, Encoding.UTF8);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("  <kml>");
                sw.WriteLine("    <Document>");
                sw.WriteLine("      <name>" + Path.GetFileName(fileName) + "</name>");
                sw.WriteLine("      <createdby>KMZRebuilder</createdby>");
                sw.WriteLine("      <Folder>");
                sw.WriteLine("        <name>" + Path.GetFileName(fileName) + "</name>");

                int record_no = 0;
                int fShpPosition = 100;
                while (fShpPosition < fShpLength)
                {
                    int recordStart = fShpPosition;
                    int recordNumber = readIntBig(fShpData, recordStart);
                    int contentLength = readIntBig(fShpData, recordStart + 4);
                    int recordContentStart = recordStart + 8;

                    int recordShapeType = readIntLittle(fShpData, recordContentStart);

                    // DBF++
                    string addit = "";
                    string name = "Noname [" + (record_no + 1).ToString() + "]";
                    if (ExDBF)
                    {
                        string[] FieldValues = new string[fDbfFieldsCount];
                        for (int y = 0; y < FieldValues.Length; y++)
                        {
                            fDbf.Position = fDbf_dataRecord_1st_Pos + (fDbf_dataRecord_Length * record_no) + Fields_Offset[y];
                            byte[] bb = new byte[(int)fDbf_fieldsLength[fDbf_Fields_Names[y]]];
                            fDbf.Read(bb, 0, bb.Length);
                            FieldValues[y] = FileReadEncoding.GetString(bb).Trim().TrimEnd(new char[] { (char)0x00 });
                            addit += fDbf_Fields_Names[y] + "=" + FieldValues[y] + "\r\n";
                        };
                        name = FieldValues[dbfNameField];
                    };
                    // DBF--

                    if ((recordShapeType == 3) || (recordShapeType == 5))
                    {
                        int numParts = readIntLittle(fShpData, recordContentStart + 36);
                        int[] parts = new int[numParts];
                        int numPoints = readIntLittle(fShpData, recordContentStart + 40);
                        PointF[] points = new PointF[numPoints];
                        int partStart = recordContentStart + 44;
                        for (int i = 0; i < numParts; i++)
                            parts[i] = readIntLittle(fShpData, partStart + i * 4);
                        int pointStart = recordContentStart + 44 + 4 * numParts;

                        sw.WriteLine("        <Placemark><name><![CDATA[" + name + "]]></name>");
                        sw.WriteLine("          <description><![CDATA[" + addit + "]]></description>");
                        if (recordShapeType == 3)
                            sw.Write("          <LineString>");
                        else
                            sw.Write("          <Polygon><extrude>1</extrude><outerBoundaryIs><LinearRing>");
                        sw.Write("<coordinates>");

                        for (int i = 0; i < numPoints; i++)
                        {
                            points[i].X = (float)readDoubleLittle(fShpData, pointStart + (i * 16));
                            points[i].Y = (float)readDoubleLittle(fShpData, pointStart + (i * 16) + 8);
                            sw.WriteLine("{0},{1},0 ", points[i].X.ToString(System.Globalization.CultureInfo.InvariantCulture), points[i].Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        };

                        sw.WriteLine("</coordinates>");
                        if (recordShapeType == 3)
                            sw.Write("          </LineString>");
                        else
                            sw.Write("          </LinearRing></outerBoundaryIs></Polygon>");
                        sw.WriteLine("        </Placemark>");

                        fShpPosition = recordStart + (4 + contentLength) * 2;
                    }
                    else
                    {
                        PointF point = new PointF((float)readDoubleLittle(fShpData, recordContentStart + 4), (float)readDoubleLittle(fShpData, recordContentStart + 4 + 8));
                        fShpPosition = recordStart + 4 + 8 + 8;
                        
                        sw.WriteLine("        <Placemark><name><![CDATA[" + name + "]]></name>");
                        sw.WriteLine("          <description><![CDATA[" + addit + "]]></description>");
                        sw.WriteLine("          <Point><coordinates>{0},{1},0</coordinates></Point>", point.X.ToString(System.Globalization.CultureInfo.InvariantCulture), point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        sw.WriteLine("        </Placemark>"); 
                    };
                    record_no++;
                };
                if (ExDBF && (fDbf != null)) fDbf.Close();
                sw.WriteLine("      </Folder>");
                sw.WriteLine("</Document></kml>");
                sw.Flush();
            };
            waitBox.Hide();

            f.kmldocName = Path.GetFileName(fileName);
            f.src_file_pth = Path.GetDirectoryName(fileName); ;
            f.src_file_nme = System.IO.Path.GetFileName(fileName);
            f.src_file_ext = System.IO.Path.GetExtension(fileName).ToLower();
            f.LoadKML(true);            
            kmzFiles.Items.Add(f, f.isCheck);
            if (outName.Text == String.Empty) outName.Text = f.kmldocName;
        }

        private static int readIntBig(byte[] data, int pos)
        {
            byte[] bytes = new byte[4];
            bytes[0] = data[pos];
            bytes[1] = data[pos + 1];
            bytes[2] = data[pos + 2];
            bytes[3] = data[pos + 3];
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        private static int readIntLittle(byte[] data, int pos)
        {
            byte[] bytes = new byte[4];
            bytes[0] = data[pos];
            bytes[1] = data[pos + 1];
            bytes[2] = data[pos + 2];
            bytes[3] = data[pos + 3];
            return BitConverter.ToInt32(bytes, 0);
        }

        private static double readDoubleLittle(byte[] data, int pos)
        {
            byte[] bytes = new byte[8];
            bytes[0] = data[pos];
            bytes[1] = data[pos + 1];
            bytes[2] = data[pos + 2];
            bytes[3] = data[pos + 3];
            bytes[4] = data[pos + 4];
            bytes[5] = data[pos + 5];
            bytes[6] = data[pos + 6];
            bytes[7] = data[pos + 7];
            return BitConverter.ToDouble(bytes, 0);
        }
    
        private void ImportFromDBF(string fileName)
        {
            // STEP 1/4
            int dbfCodePage = 1251;
            {
                DBF.CodePageList CodePages = new DBF.CodePageList();
                string[] options = new string[CodePages.Count];
                int idx = 43;
                for (int i = 0; i < options.Length; i++)
                {
                    options[i] = CodePages[i].ToString();
                    if (CodePages[i].codePage == 1251)
                        idx = i;
                };

                int dW = System.Windows.Forms.InputBox.defWidth;
                System.Windows.Forms.InputBox.defWidth = 500;
                if (System.Windows.Forms.InputBox.Show("Import DBF Step 1/4", "Select Code Page:", options, ref idx) == DialogResult.OK)
                    dbfCodePage = CodePages[idx].codePage;
                else
                {
                    System.Windows.Forms.InputBox.defWidth = dW;
                    return;
                };
                System.Windows.Forms.InputBox.defWidth = dW;
            };

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            System.Text.Encoding FileReadEncoding = System.Text.Encoding.GetEncoding(dbfCodePage);

            // Read File Version
            fs.Position = 0;
            int ver = fs.ReadByte();

            // Read Records Count
            fs.Position = 04;
            byte[] bb = new byte[4];
            fs.Read(bb, 0, 4);
            int total = BitConverter.ToInt32(bb, 0);

            // Read DataRecord 1st Position  
            fs.Position = 8;
            bb = new byte[2];
            fs.Read(bb, 0, 2);
            short dataRecord_1st_Pos = BitConverter.ToInt16(bb, 0);
            int FieldsCount = (((bb[0] + (bb[1] * 0x100)) - 1) / 32) - 1;

            // Read DataRecord Length
            fs.Position = 10;
            bb = new byte[2];
            fs.Read(bb, 0, 2);
            short dataRecord_Length = BitConverter.ToInt16(bb, 0);

            // Read Заголовки Полей
            fs.Position = 32;
            string[] Fields_Name = new string[FieldsCount]; // Массив названий полей
            Hashtable fieldsLength = new Hashtable(); // Массив размеров полей
            Hashtable fieldsType = new Hashtable();   // Массив типов полей
            byte[] Fields_Dig = new byte[FieldsCount];   // Массив размеров дробной части
            int[] Fields_Offset = new int[FieldsCount];    // Массив отступов
            bb = new byte[32 * FieldsCount]; // Описание полей: 32 байтa * кол-во, начиная с 33-го            
            fs.Read(bb, 0, bb.Length);
            int FieldsLength = 0;
            for (int x = 0; x < FieldsCount; x++)
            {
                Fields_Name[x] = System.Text.Encoding.Default.GetString(bb, x * 32, 10).TrimEnd(new char[] { (char)0x00 }).ToUpper();
                fieldsType.Add(Fields_Name[x], "" + (char)bb[x * 32 + 11]);
                fieldsLength.Add(Fields_Name[x], (int)bb[x * 32 + 16]);
                Fields_Dig[x] = bb[x * 32 + 17];
                Fields_Offset[x] = 1 + FieldsLength;
                FieldsLength = FieldsLength + (int)fieldsLength[Fields_Name[x]];
            };            

            // STEPS 2,3,4/4
            int dbfNameField = 0; int dbfXField = 0; int dbfYField = 0;
            for (int x = 0; x < FieldsCount; x++)
            {
                string fup = Fields_Name[x].ToUpper();
                if (fup == "NAME") dbfNameField = x;
                if (fup == "LABEL") dbfNameField = x;
                if (fup == "X") dbfXField = x;
                if (fup == "LON") dbfXField = x;
                if (fup == "LONG") dbfXField = x;
                if (fup == "LONGITUDE") dbfXField = x;
                if (fup == "Y") dbfYField = x;
                if (fup == "LAT") dbfYField = x;
                if (fup == "LATITUDE") dbfYField = x;                
            };
            if (System.Windows.Forms.InputBox.Show("Import DBF Step 2/4", "Select Name Field:", Fields_Name, ref dbfNameField) != DialogResult.OK) { fs.Close(); return; };            
            if (System.Windows.Forms.InputBox.Show("Import DBF Step 3/4", "Select X/Longitude Field:", Fields_Name, ref dbfXField) != DialogResult.OK) { fs.Close(); return; };            
            if (System.Windows.Forms.InputBox.Show("Import DBF Step 4/4", "Select Y/Latitude Field:", Fields_Name, ref dbfYField) != DialogResult.OK) { fs.Close(); return; };

            KMFile f = KMFile.CreateEmpty();            

            waitBox.Show("Import DBF", "Wait, reading file ...");
            using (FileStream outFile = new FileStream(f.src_file_pth, FileMode.Create, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(outFile, Encoding.UTF8);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("  <kml>");
                sw.WriteLine("    <Document>");
                sw.WriteLine("      <name>"+Path.GetFileName(fileName)+"</name>");
                sw.WriteLine("      <createdby>KMZRebuilder</createdby>");
                sw.WriteLine("      <Folder>");
                sw.WriteLine("        <name>" + Path.GetFileName(fileName) + "</name>");

                for (int record_no = 0; record_no < total; record_no++)
                {
                    waitBox.Show("Import DBF", String.Format("Wait, reading {0}/{1} ...", record_no, total));

                    string[] FieldValues = new string[FieldsCount];
                    Hashtable record = new Hashtable();
                    string addit = "";
                    for (int y = 0; y < FieldValues.Length; y++)
                    {
                        fs.Position = dataRecord_1st_Pos + (dataRecord_Length * record_no) + Fields_Offset[y];
                        bb = new byte[(int)fieldsLength[Fields_Name[y]]];
                        fs.Read(bb, 0, bb.Length);
                        FieldValues[y] = FileReadEncoding.GetString(bb).Trim().TrimEnd(new char[] { (char)0x00 });
                        record.Add(Fields_Name[y], FieldValues[y]);
                        addit += Fields_Name[y] + "=" + FieldValues[y] + "\r\n";
                    };
                    
                    sw.WriteLine("        <Placemark><name><![CDATA[" + FieldValues[dbfNameField] + "]]></name>");
                    sw.WriteLine("          <description><![CDATA[" + addit + "]]></description>");
                    sw.WriteLine("          <Point><coordinates>{0},{1},0</coordinates></Point>", FieldValues[dbfXField], FieldValues[dbfYField]);
                    sw.WriteLine("        </Placemark>");                    
                };

                sw.WriteLine("      </Folder>");
                sw.WriteLine("</Document></kml>");
                sw.Flush();
            };
            waitBox.Hide();
            fs.Close();
            
            f.kmldocName = Path.GetFileName(fileName);
            f.src_file_pth = Path.GetDirectoryName(fileName); ;
            f.src_file_nme = System.IO.Path.GetFileName(fileName);
            f.src_file_ext = System.IO.Path.GetExtension(fileName).ToLower();
            f.LoadKML(true);            
            kmzFiles.Items.Add(f, f.isCheck);
            if (outName.Text == String.Empty) outName.Text = f.kmldocName;
        }


        private void ImportFromClipboard()
        {
            string txt = Clipboard.GetText();
            if (String.IsNullOrEmpty(txt)) return;
            if (Text.Length < 9) return;

            MemoryStream fs = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(txt));

            CSVTXTForm addf = new CSVTXTForm(fs);
            if (addf.ShowDialog() == DialogResult.OK)
                ImportFromText2Kml(fs, addf, "Clipboard_Text_Data.txt");
            addf.Dispose();
            fs.Close();
        }

        private void ImportFromText2Kml(Stream fs, CSVTXTForm form, string filename)
        {
            Encoding enc = form.CodePage.GetEncoding();
            string[] sep = form.skipsw.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            char cd = form.delimiter.Text[0];
            if (form.delimiter.Text == "TAB") cd = '\t';
            bool he = form.flh.Text == "YES";
            int fName = form.fName.SelectedIndex - 1;
            int fDesc = form.fDesc.SelectedIndex - 1;
            int fLat = form.fLat.SelectedIndex - 1;
            int fLon = form.fLon.SelectedIndex - 1;
            int fStyle = form.fStyle.SelectedIndex - 1;
            char dsep = form.separator.Text[0];
            if(form.separator.Text == "AUTO") dsep = '\0';

            KMFile f = KMFile.FromCSVTXT(fs, enc, filename, sep, he, cd, dsep, fName, fDesc, fLat, fLon, fStyle);
            kmzFiles.Items.Add(f, f.isCheck);
            if (outName.Text == String.Empty) outName.Text = f.kmldocName;
            if (f.parseError)
                MessageBox.Show("Data loaded with errors!\r\nCheck your data and save it to normal format first!", "Loading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void importFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportFromClipboard();            
        }

        private void changeStyleIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string path = f.tmp_file_dir;

            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(path, "*.png", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(path, "*.jpeg", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(path, "*.gif", SearchOption.AllDirectories));

            if (files.Count == 0)
            {
                MessageBox.Show("No Images Found", "Change Style Icons", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            };

            SwapIcons rd = new SwapIcons(files.ToArray(), this);
            if (rd.ShowDialog() == DialogResult.OK)
            {
                int new_size = rd.imSize.SelectedIndex * 2 + 16;
                if (rd.imSize.SelectedIndex == 9) new_size = 0;
                waitBox.Show("Loading", "Wait, resize and replace icons...");
                log.Text = "";
                AddToLog("Resizing and replacing KMZ Icons");
                int c = 0;
                for(int i=0;i<rd.rep_files.Length;i++)
                    if ((!String.IsNullOrEmpty(rd.rep_files[i])) && File.Exists(rd.rep_files[i]))
                    {                      
                        ImageMagick.MagickImage im = new ImageMagick.MagickImage(rd.rep_files[i]);  
                        if ((new_size > 0) && ((im.Width > new_size) || (im.Height > new_size)))
                        {                            
                            im.Resize(new_size, new_size);
                            MapIcons.SaveIcon(im, files[i]);                            
                        }
                        else
                            MapIcons.SaveIcon(rd.rep_files[i], files[i]);
                        im.Dispose();
                        c++;
                    }
                    else if (rd.rep_images[i] != null)
                    {
                        ImageMagick.MagickImage im = new ImageMagick.MagickImage((Bitmap)rd.rep_images[i]);
                        if (((new_size > 0)  && ((im.Width > new_size) || (im.Height > new_size))) || (rd.rep_imars[i] == null) || (rd.rep_imars[i].Length == 0))
                        {                            
                            if((new_size > 0)  && ((im.Width > new_size))) 
                                im.Resize(new_size, new_size);
                            if (!String.IsNullOrEmpty(rd.rep_files[i]))
                            {
                                try
                                {
                                    if (rd.rep_files[i].StartsWith("gdb_icons.zip"))
                                    {
                                        string gdb = rd.rep_files[i].Substring(13).Trim('\\').Replace(".png", "");
                                        int _gdb;                                        
                                        im.SetAttribute("gdb", gdb);
                                        if (int.TryParse(gdb, out _gdb))
                                            im.SetAttribute("gdb_name", NavitelRecord.IconList[_gdb]);                                                                            
                                    };
                                }
                                catch { };
                            };
                            MapIcons.SaveIcon(im, files[i]);                            
                        }
                        else
                            MapIcons.SaveIcon(rd.rep_imars[i], files[i]);
                        im.Dispose();
                        c++;
                    };
                waitBox.Hide();
                if (c > 0)
                {
                    MessageBox.Show("Replaces " + c.ToString() + " images", "Change Style Icons", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AddToLog("Replaces " + c.ToString() + " images");
                };
                AddToLog("Done");
            };
            rd.Dispose();
        }

        private void saveLayerToDATfavoritesdatFileForPROGORODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            Export2Dat((KMLayer)kmzLayers.SelectedItem);
        }

        private void Export2Dat(KMLayer kml)
        {
            XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
            XmlNodeList xns = xn.SelectNodes("Placemark/Point/coordinates");
            if (xns.Count > 0)
            {
                string filename = null;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save to";
                sfd.FileName = "favorites.dat";
                sfd.DefaultExt = ".dat";
                sfd.Filter = "Favorites.dat (*.dat)|*.dat";
                if (sfd.ShowDialog() == DialogResult.OK)
                    filename = sfd.FileName;                    
                sfd.Dispose();
                if (String.IsNullOrEmpty(filename)) return;
                
                //////////////////////////////////////////

                List<string> styles = new List<string>();
                List<int> new_styles = new List<int>();
                ImageList imlF = new ImageList();
                IMM imm = new IMM();
                CRC32 crc = new CRC32();

                for (int x = 0; x < xns.Count; x++)
                {
                    string style = "none";
                    XmlNode stn = xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                    if ((stn != null) && (stn.ChildNodes.Count > 0))
                    {
                        style = stn.ChildNodes[0].Value;
                        if (styles.IndexOf(style) < 0) 
                        {
                            styles.Add(style);
                            new_styles.Add(0);
                            string im = style.Replace("#","");
                            XmlNode him = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + im + "']/IconStyle/Icon/href");
                            if (him != null)
                            {
                                im = kml.file.tmp_file_dir + him.InnerText.Replace("/", @"\");
                                imlF.Images.Add(Image.FromFile(im));
                                imm.Set(crc.CRC32Num(im), style);
                            }
                            else
                                imlF.Images.Add(new Bitmap(16, 16));
                        };
                    };                    
                };

                //////////////////////////////////////////                

                // LIST STYLES //
                if (styles.Count > 0)
                {
                    imlF.ImageSize = new Size(16, 16);
                    XmlNode sh = kml.file.kmlDoc.SelectSingleNode("kml/Document/style_history");
                    string sht = sh == null ? "" : sh.InnerText;
                    RenameDat rd = RenameDat.CreateForDAT(sht, kml.file.tmp_file_dir + @"images\");
                    rd.datpanel.Visible = true;
                    rd.listView2.SmallImageList = imlF;
                    rd.imm = imm;
                    for (int i = 0; i < styles.Count; i++)
                    {
                        ListViewItem lvi = new ListViewItem(styles[i]);
                        lvi.SubItems.Add(((ProGorodPOI.TType)new_styles[i]).ToString());
                        rd.listView2.Items.Add(lvi);
                        lvi.ImageIndex = i;
                    };
                    rd.Autodetect();
                    if (rd.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < styles.Count; i++)
                            new_styles[i] = rd.nlTexts.IndexOf(rd.listView2.Items[i].SubItems[1].Text);
                        imm = rd.imm;
                    }
                    else
                    {
                        rd.Dispose();
                        sfd.Dispose();
                        return;
                    };
                    bool sort = rd.DoSort;
                    bool remd = rd.RemoveDescriptions;
                    rd.Dispose();

                    // PROCESS //
                    log.Text = "";
                    AddToLog("Saving points to PROGOROD...");
                    List<ProGorodPOI.FavRecord> recs = new List<ProGorodPOI.FavRecord>();
                    for (int x = 0; x < xns.Count; x++)
                    {
                        string[] llz = xns[x].ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        string name = xns[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value.Replace(",", ";");
                        string style = "none";
                        XmlNode stn = xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                        if ((stn != null) && (stn.ChildNodes.Count > 0))
                            style = stn.ChildNodes[0].Value;
                        int icon = styles.IndexOf(style) < 0 ? 0 : new_styles[styles.IndexOf(style)];
                        string desc = "";
                        XmlNode std = xns[x].ParentNode.ParentNode.SelectSingleNode("description");
                        if ((std != null) && (std.ChildNodes.Count > 0))
                            desc = std.ChildNodes[0].Value;

                        ProGorodPOI.FavRecord rec = new ProGorodPOI.FavRecord();
                        rec.Name = name;                        
                        rec.Lat = double.Parse(llz[1].Replace("\r", "").Replace("\n", "").Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture);
                        rec.Lon = double.Parse(llz[0].Replace("\r", "").Replace("\n", "").Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture);
                        rec.HomeOffice = ProGorodPOI.THomeOffice.None;
                        if (!String.IsNullOrEmpty(desc))
                        {
                            string dtl = desc.ToLower();
                            if (dtl.IndexOf("progorod_dat_home=yes") >= 0) 
                                rec.HomeOffice = ProGorodPOI.THomeOffice.Home;
                            if (dtl.IndexOf("progorod_dat_home=1") >= 0) rec.HomeOffice = ProGorodPOI.THomeOffice.Home;
                            if (dtl.IndexOf("progorod_dat_home=true") >= 0) rec.HomeOffice = ProGorodPOI.THomeOffice.Home;
                            if (dtl.IndexOf("progorod_dat_office=yes") >= 0) rec.HomeOffice = ProGorodPOI.THomeOffice.Office;
                            if (dtl.IndexOf("progorod_dat_office=1") >= 0) rec.HomeOffice = ProGorodPOI.THomeOffice.Office;
                            if (dtl.IndexOf("progorod_dat_office=true") >= 0) rec.HomeOffice = ProGorodPOI.THomeOffice.Office;
                            dtl = (new Regex(@"[\w]+=[\S\s][^\r\n]+")).Replace(dtl, ""); // Remove TAGS
                        };
                        if (remd) desc = "";
                        rec.Desc = desc;
                        rec.Icon = (ProGorodPOI.TType)icon;
                        if ((rec.HomeOffice == ProGorodPOI.THomeOffice.Home) || (rec.HomeOffice == ProGorodPOI.THomeOffice.Office))
                            recs.Insert(0, rec);
                        else
                            recs.Add(rec);                        
                    };
                    if (recs.Count > 0)
                    {
                        if (sort)
                            recs.Sort(new ProGorodPOI.FavRecordSorter());
                        ProGorodPOI.WriteFile(filename, recs.ToArray());
                        if (imm.save2file) imm.Save(filename + ".imm");
                    };
                    AddToLog("Saved " + recs.Count.ToString() + " points");
                    AddToLog(String.Format("Saving data to selected file: {0}", filename));
                    AddToLog("Done");
                };
                //////////////////////////////////////////
                return;
            };

            AddToLog("File not created: Layer has no placemarks to save in PROGOROD format!");
            MessageBox.Show("Layer has no placemarks to save in dat format!", "File not created", MessageBoxButtons.OK, MessageBoxIcon.Information);                        
        }

        private void addEmptyLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;

            KMLayer l = (KMLayer)kmzLayers.SelectedItem;            

            string name = "New Layer";
            if ((InputBox("Create layer name", "Name:", ref name) == DialogResult.OK) && (!String.IsNullOrEmpty(name)))
            {
                XmlNode xn = l.file.kmlDoc.SelectSingleNode("kml/Document");
                xn = xn.AppendChild(l.file.kmlDoc.CreateElement("Folder"));
                xn = xn.AppendChild(l.file.kmlDoc.CreateElement("name"));
                xn.AppendChild(l.file.kmlDoc.CreateTextNode(name));
                l.file.SaveKML();
                waitBox.Show("Reloading", "Wait, reloading file layers...");
                l.file.LoadKML(true);
                ReloadListboxLayers(true);
                waitBox.Hide();
            };
        }

        private void createEpmtyFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KMFile f = KMFile.CreateEmpty();
            kmzFiles.Items.Add(f, f.isCheck);
            if (outName.Text == String.Empty) outName.Text = f.kmldocName;
            if (f.parseError)
                MessageBox.Show("Data loaded with errors!\r\nCheck your data and save it to normal format first!", "Loading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void latLonConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new WGSFormX()).Show(this);
        }

        private void deleteLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;

            KMLayer l = (KMLayer)kmzLayers.SelectedItem;
            
            if (MessageBox.Show("Delete\r\n" + l.ToString() + "\r\nlayer?", "Delete layer", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No) return;

            try
            {
                XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                xn.ParentNode.RemoveChild(xn);
                l.file.SaveKML();
                l.file.LoadKML(true);
                ReloadListboxLayers(true);
            }
            catch { };

            kmzLayers_SelectedIndexChanged(sender, e);
        }

        private void viewInKMZViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;

            IntPtr vh = IntPtr.Zero;
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\", true)
                    .CreateSubKey("KMZViewer"))
                    vh = (IntPtr)((int)key.GetValue("Handle"));
                if (vh != IntPtr.Zero)
                {
                    string wt = SASPlacemarkConnector.GetText(vh);
                    if ((!String.IsNullOrEmpty(wt)) && (wt.IndexOf("KMZ Viewer") == 0))
                    {
                        List<byte> data = new List<byte>();
                        data.AddRange(BitConverter.GetBytes(l.id));
                        data.AddRange(System.Text.Encoding.UTF8.GetBytes(l.file.src_file_pth));
                        ProcDataExchange.SendData(vh, this.Handle, XP_OPENLAYER, data.ToArray());
                        SASPlacemarkConnector.SetForegroundWindow(vh);
                        SASPlacemarkConnector.SetActiveWindow(vh);
                        SASPlacemarkConnector.SetFocus(vh);
                        return;
                    };
                };
            }
            catch (Exception ex)
            {
            };

            if (File.Exists(CurrentDirectory() + @"KMZViewer.exe"))
                System.Diagnostics.Process.Start(CurrentDirectory() + @"KMZViewer.exe", "\"" + l.file.src_file_pth + "\"");
            else
                MessageBox.Show("KMZViewer Window Not Found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void importFromSASPlanetSQLiteDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // https://github.com/ErshKUS/osmCatalog
            ImportDB3(null);
        }

        private void ImportMapsForgeMap(string openfile)
        {
            MapsForgeFileReader.MapsForgeReader mfr = null;
            try
            {
                mfr = new MapsForgeFileReader.MapsForgeReader(openfile);
                if ((mfr.POI_TAGS.Count == 0) && (mfr.WAY_TAGS.Count == 0))
                {
                    mfr.Close();
                    MessageBox.Show("No any tags found!");
                    return;
                };

                List<int> grab_tags = new List<int>();
                List<int> grab_ways = new List<int>();

                if (mfr.POI_TAGS.Count > 0)
                {
                    GMLayRenamerForm sf = new GMLayRenamerForm();
                    sf.Text = "Import POIs from MapsForge Map";
                    sf.layers.MultiSelect = true;
                    List<KeyValuePair<string, string>> kvp = new List<KeyValuePair<string, string>>();
                    kvp.AddRange(mfr.POI_TAGS);
                    kvp.Sort(new TAGSorter());
                    foreach (KeyValuePair<string, string> kv in kvp)
                    {
                        ListViewItem lvi = new ListViewItem(kv.Key + " = " + kv.Value);
                        lvi.Checked = true;
                        if (kv.Key == "power") lvi.Checked = false;
                        if (kv.Key == "barrier") lvi.Checked = false;
                        sf.layers.Items.Add(lvi);
                    };
                    sf.label1.Text = "Select Tags to Import (Total " + mfr.POI_TAGS.Count + " tags):";
                    sf.layers.View = View.Details;
                    sf.layers.Columns[0].Name = "Layer";
                    sf.layers.Columns[0].Width = sf.layers.Width - 100;
                    sf.layers.Columns.Add("Objects");
                    sf.layers.Columns[1].Width = 70;
                    sf.layers.Sort();
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

                    int l_co = sf.layers.CheckedItems.Count;
                    for (int i = sf.layers.Items.Count - 1; i >= 0; i--)
                    {
                        if (sf.layers.Items[i].Checked)
                            for (int x = 0; x < mfr.POI_TAGS.Count; x++)
                                if ((kvp[i].Key == mfr.POI_TAGS[x].Key) && (kvp[i].Value == mfr.POI_TAGS[x].Value))
                                    grab_tags.Add(x);
                    };
                    sf.Dispose();
                };

                if (mfr.WAY_TAGS.Count > 0)
                {
                    GMLayRenamerForm sf = new GMLayRenamerForm();
                    sf.Text = "Import WAYs Information from MapsForge Map";
                    sf.layers.MultiSelect = true;
                    List<KeyValuePair<string, string>> kvp = new List<KeyValuePair<string, string>>();
                    kvp.AddRange(mfr.WAY_TAGS);
                    kvp.Sort(new TAGSorter());
                    foreach (KeyValuePair<string, string> kv in kvp)
                    {
                        ListViewItem lvi = new ListViewItem(kv.Key + " = " + kv.Value);
                        lvi.Checked = false;
                        if (kv.Key == "amenity") lvi.Checked = true;
                        if (kv.Key == "tourism") lvi.Checked = true;
                        if (kv.Key == "building") lvi.Checked = true;
                        if (kv.Key == "leisure") lvi.Checked = true;
                        if (kv.Key == "sport") lvi.Checked = true;
                        if (kv.Key == "railway") lvi.Checked = true;
                        sf.layers.Items.Add(lvi);
                    };
                    sf.label1.Text = "Select Tags to Import (Total " + mfr.POI_TAGS.Count + " tags):";
                    sf.layers.View = View.Details;
                    sf.layers.Columns[0].Name = "Layer";
                    sf.layers.Columns[0].Width = sf.layers.Width - 100;
                    sf.layers.Columns.Add("Objects");
                    sf.layers.Columns[1].Width = 70;
                    sf.layers.Sort();
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

                    int l_co = sf.layers.CheckedItems.Count;
                    for (int i = sf.layers.Items.Count - 1; i >= 0; i--)
                    {
                        if (sf.layers.Items[i].Checked)
                            for (int x = 0; x < mfr.WAY_TAGS.Count; x++)
                                if ((kvp[i].Key == mfr.WAY_TAGS[x].Key) && (kvp[i].Value == mfr.WAY_TAGS[x].Value))
                                    grab_ways.Add(x);
                    };
                    sf.Dispose();
                };
                if ((grab_tags.Count == 0) && (grab_ways.Count == 0)) return;

                if (MessageBox.Show("Get POI Data for " + (grab_tags.Count + grab_ways.Count).ToString() + " categories from MapsForge file?", "Import Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                KMFile f = KMFile.FromMapsForgeMapFile(openfile, mfr, grab_tags, grab_ways);
                kmzFiles.Items.Add(f, f.isCheck);
                if (outName.Text == String.Empty) outName.Text = f.kmldocName;
                if (f.parseError)
                    MessageBox.Show("Data loaded with errors!", "Loading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                if (mfr != null) mfr.Close();
            };
        }

        private void ImportPOI(string openfile)
        {            
            Dictionary<int, object[]> lcats = new Dictionary<int, object[]>();
            List<string[]> cats = new List<string[]>();            
            waitBox.Show("Loading POI", "Wait, loading `" + Path.GetFileName(openfile) + "`...");
            try
            {
                System.Data.SQLite.SQLiteConnection sqlc = new System.Data.SQLite.SQLiteConnection(@"Data Source=" + openfile + ";Version=3;");
                sqlc.Open();
                System.Data.SQLite.SQLiteCommand sc = new System.Data.SQLite.SQLiteCommand("", sqlc);

                sc.CommandText = "SELECT * FROM POI_CATEGORIES";
                System.Data.SQLite.SQLiteDataReader dr = sc.ExecuteReader();
                while (dr.Read())
                    lcats.Add(int.Parse(dr["ID"].ToString()), new object[] { dr["NAME"].ToString(), dr["PARENT"].ToString(), "" });
                dr.Close();

                foreach (KeyValuePair<int, object[]> c in lcats)
                {
                    int k = c.Key;
                    object[] v = c.Value;
                    string name = c.Value[0].ToString();
                    while (v[1].ToString() != "")
                    {
                        k = int.Parse(v[1].ToString());
                        v = lcats[k];
                        if (v[0].ToString() != "root")
                            name = v[0].ToString() + @" \ " + name;                        
                    };
                    sc.CommandText = "SELECT COUNT(DISTINCT ID) FROM  POI_CATEGORY_MAP WHERE CATEGORY = " + c.Key.ToString();
                    string cnt = sc.ExecuteScalar().ToString();
                    cats.Add(new string[] { c.Key.ToString(), name, "", cnt});
                };
                sqlc.Close();
            }
            catch (Exception ex)
            {
                waitBox.Hide();
                MessageBox.Show(ex.Message, "Open POI File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };            

            if (cats.Count == 0)
            {
                waitBox.Hide();
                MessageBox.Show("No Categories found", "Open POI File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            };

            cats.Sort(new DB3CatSorted());
            GMLayRenamerForm sf = new GMLayRenamerForm();
            sf.Text = "Import From POI DB";
            sf.layers.MultiSelect = true;
            foreach (string[] obj in cats)
            {
                ListViewItem lvi = new ListViewItem(obj[1] + " ["+ obj[3]+"]");
                if(obj[3] != "0")
                    lvi.Checked = true;
                lvi.SubItems.Add(obj[2]);
                lvi.SubItems.Add(obj[0]);
                sf.layers.Items.Add(lvi);
            };
            sf.label1.Text = "Select Categories to Import (Total: " + cats.Count.ToString() + "):";
            sf.layers.View = View.Details;
            sf.layers.Columns[0].Name = "Layer";
            sf.layers.Columns[0].Width = sf.layers.Width - 100;
            sf.layers.Columns.Add("Objects");
            sf.layers.Columns[1].Width = 70;
            sf.layers.Sort();
            sf.layers.FullRowSelect = true;
            sf.layers.CheckBoxes = true;
            sf.layers.LargeImageList = null;
            sf.layers.SmallImageList = null;
            sf.layers.StateImageList = null;
            sf.layers.MultiSelect = true;
            waitBox.Hide();
            if ((sf.ShowDialog() != DialogResult.OK) || (sf.layers.CheckedIndices.Count == 0))
            {
                sf.Dispose();
                return;
            };

            string in_list = "";
            int l_co = sf.layers.CheckedItems.Count;
            for (int i = sf.layers.Items.Count - 1; i >= 0; i-- )
            {
                if (sf.layers.Items[i].Checked)
                    in_list += (in_list.Length > 0 ? "," : "") + cats[i][0];
                else
                    cats.RemoveAt(i);
            };
            sf.Dispose();


            waitBox.Show("Loading POI", "Wait, reading POIs from `" + Path.GetFileName(openfile) + "`...");
            int marks_count = 0;
            try
            {
                System.Data.SQLite.SQLiteConnection sqlc = new System.Data.SQLite.SQLiteConnection(@"Data Source=" + openfile + ";Version=3;");
                sqlc.Open();
                System.Data.SQLite.SQLiteCommand sc = new System.Data.SQLite.SQLiteCommand("", sqlc);

                sc.CommandText = "SELECT COUNT(*) FROM POI_CATEGORY_MAP WHERE CATEGORY IN (" + in_list + ")";
                System.Data.SQLite.SQLiteDataReader dr = sc.ExecuteReader();
                if (dr.Read())
                    marks_count = Convert.ToInt32(dr[0]);
                dr.Close();

                sqlc.Close();
            }
            catch (Exception ex)
            {
                waitBox.Hide();
                MessageBox.Show(ex.Message, "Open POI File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            if (marks_count == 0)
            {
                waitBox.Hide();
                MessageBox.Show("No Any POI Found", "Open POI File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else
            {
                waitBox.Hide();
                if (MessageBox.Show("Import " + marks_count.ToString() + " objects in " + l_co.ToString() + " layers from POI file?", "Open POI File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) 
                    return;
            };

            waitBox.Show("Loading POI", "Wait, saving POIs from `" + Path.GetFileName(openfile) + "`...");
            try
            {
                KMFile f = KMFile.FromMapsForgePOIFile(openfile, cats);
                kmzFiles.Items.Add(f, f.isCheck);
                if (outName.Text == String.Empty) outName.Text = f.kmldocName;
                waitBox.Hide();
                if (f.parseError)
                    MessageBox.Show("Data loaded with errors!", "Loading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                waitBox.Hide();
                MessageBox.Show(ex.Message, "Open POI File", MessageBoxButtons.OK, MessageBoxIcon.Error);                
            };
        }

        private void ImportDB3(string filename)
        {
            string sasini = @"C:\Program Files\SASPlanet\SASPlanet.ini";
            string openfile = "";            
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                string ff = procs[0].MainModule.FileName;
                sasini = Path.GetDirectoryName(ff) + @"\SASPlanet.ini";
            };
            if (File.Exists(sasini))
            {
                FileStream fs = new FileStream(sasini, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
                string line = "";
                while ((!sr.EndOfStream) && ((line = sr.ReadLine()) != "[MarkSystemConfig]")) { };
                if (line == "[MarkSystemConfig]")
                {
                    while ((!sr.EndOfStream) && ((line = sr.ReadLine()).IndexOf("[") != 0))
                        if (line.IndexOf("Item1_FileName") == 0)
                            openfile = line.Split(new char[] { '=' }, 2)[1];
                };
                sr.Close();
                fs.Close();
            };

            if ((!String.IsNullOrEmpty(filename)) && (File.Exists(filename))) 
                openfile = filename;
            else
                if (System.Windows.Forms.InputBox.QueryFileBox("Open SASPlanet SQLite DB File", "Select file to import:", ref openfile, "SQLite (*.db3)|*.db3") != DialogResult.OK) return;
            if (String.IsNullOrEmpty(openfile)) return;
            if (!File.Exists(openfile)) return;

            List<string[]> cats = new List<string[]>();
            try
            {
                System.Data.SQLite.SQLiteConnection sqlc = new System.Data.SQLite.SQLiteConnection(@"Data Source=" + openfile + ";Version=3;");
                sqlc.Open();
                System.Data.SQLite.SQLiteCommand sc = new System.Data.SQLite.SQLiteCommand("", sqlc);

                sc.CommandText = "SELECT * FROM CATEGORY";
                System.Data.SQLite.SQLiteDataReader dr = sc.ExecuteReader();
                while (dr.Read())
                    cats.Add(new string[] { dr["ID"].ToString(), dr["cName"].ToString(), "" });
                dr.Close();

                if (cats.Count > 0)
                    for (int i = cats.Count - 1; i >= 0 ;i--)
                    {
                        sc.CommandText = "SELECT COUNT(*) FROM MARK where mGeoType in (1,2,3) and mCategory = " + cats[i][0];
                        dr = sc.ExecuteReader();
                        if (dr.Read())
                        {
                            int c = Convert.ToInt32(dr[0]);
                            if (c > 0)
                                cats[i][2] = c.ToString();
                            else
                                cats.RemoveAt(i);
                        }
                        else
                            cats.RemoveAt(i);
                        dr.Close();
                    };

                sqlc.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Open SASPlanet SQLite DB File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            if (cats.Count == 0)
            {
                MessageBox.Show("No Categories found", "Open SASPlanet SQLite DB File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            };

            cats.Sort(new DB3CatSorted());
            GMLayRenamerForm sf = new GMLayRenamerForm();
            sf.Text = "Import From SASPlanet SQLite DB";
            sf.layers.MultiSelect = true;
            foreach (string[] obj in cats)
            {
                ListViewItem lvi = new ListViewItem(obj[1]);
                //lvi.Checked = true;
                lvi.SubItems.Add(obj[2]);
                lvi.SubItems.Add(obj[0]);
                sf.layers.Items.Add(lvi);                
            };
            sf.label1.Text = "Select Categories to Import (Total: " + cats.Count.ToString() + "):";
            sf.layers.View = View.Details;
            sf.layers.Columns[0].Name = "Layer";
            sf.layers.Columns[0].Width = sf.layers.Width - 100;
            sf.layers.Columns.Add("Objects");
            sf.layers.Columns[1].Width = 70;
            sf.layers.Sort();
            sf.layers.FullRowSelect = true;
            sf.layers.CheckBoxes = true;
            sf.layers.LargeImageList = null;
            sf.layers.SmallImageList = null;
            sf.layers.StateImageList = null;
            sf.layers.MultiSelect = true;
            if ((sf.ShowDialog() != DialogResult.OK) || (sf.layers.CheckedIndices.Count == 0))
            {
                sf.Dispose();
                return;
            };

            List<int> catl = new List<int>();
            string in_list = "";
            int l_co = sf.layers.CheckedItems.Count;
            for (int i = 0; i < sf.layers.CheckedItems.Count; i++)
            {
                catl.Add(Convert.ToInt32(sf.layers.CheckedItems[i].SubItems[2].Text));
                in_list += (in_list.Length > 0 ? "," : "") + sf.layers.CheckedItems[i].SubItems[2].Text;
            };
            sf.Dispose();

            int marks_count = 0;
            try
            {
                System.Data.SQLite.SQLiteConnection sqlc = new System.Data.SQLite.SQLiteConnection(@"Data Source=" + openfile + ";Version=3;");
                sqlc.Open();
                System.Data.SQLite.SQLiteCommand sc = new System.Data.SQLite.SQLiteCommand("", sqlc);

                sc.CommandText = "select count(*) from mark where mGeoType in (1,2,3) and mCategory in (" + in_list + ")";
                System.Data.SQLite.SQLiteDataReader dr = sc.ExecuteReader();
                if (dr.Read())
                    marks_count = Convert.ToInt32(dr[0]);
                dr.Close();

                sqlc.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Open SASPlanet SQLite DB File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            if (marks_count == 0)
            {
                MessageBox.Show("No Any Placemarks Found", "Open SASPlanet SQLite DB File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else
                if (MessageBox.Show("Import " + marks_count.ToString() + " objects in " + l_co.ToString() + " layers from SASPlanet SQLite DB?", "Open SASPlanet SQLite DB File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            string icons_path = Path.GetDirectoryName(sasini).Trim('\0') + @"\MarksIcons\";
            if(!Directory.Exists(icons_path))
                if (System.Windows.Forms.InputBox.QueryDirectoryBox("Open SASPlanet SQLite DB File", "Browse folder with SAPlanet Marks images:", ref icons_path) != DialogResult.OK)
                    icons_path = Path.GetDirectoryName(sasini).Trim('\0') + @"\MarksIcons\";

            KMFile f = KMFile.FromDB3(openfile, catl.ToArray(), icons_path);
            kmzFiles.Items.Add(f, f.isCheck);
            if (outName.Text == String.Empty) outName.Text = f.kmldocName;
            if (f.parseError)
                MessageBox.Show("Data loaded with errors!", "Loading", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void importPOIFromOSMExportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".osm";
            ofd.Filter = "OSM Export File (*.osm)|*.osm";
            if (ofd.ShowDialog() == DialogResult.OK)
                LoadFiles(new string[] { ofd.FileName });
            ofd.Dispose();
        }

        private void findLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count == 0) return;
            
            FindReplaceDlg frd = new FindReplaceDlg();
            frd.Text = "Find Layer";
            frd.FindOnly = true;
            frd.onFind += new EventHandler(FindButton_Click);
            frd.onFindAll += new EventHandler(FindAll_Click);
            frd.onFocus += new EventHandler(FF_Focus);
            frd.Left = this.Left + 60;
            frd.Top = this.Top + 60;
            frd.Show(this);            
        }

        private void FF_Focus(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count == 0) return;
            if (sender == null) return;
            if (((Control)sender).Parent == null) return;
            if (!(((Control)sender).Parent is FindReplaceDlg)) return;

            FindReplaceDlg frd = (FindReplaceDlg)((Control)sender).Parent;

            if (kmzLayers.SelectedIndices.Count > 0)
                frd.currentIndex = kmzLayers.SelectedIndices[0];
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count == 0) return;
            if(sender == null) return;
            if(((Control)sender).Parent == null) return;
            if (!(((Control)sender).Parent is FindReplaceDlg)) return;

            FindReplaceDlg frd = (FindReplaceDlg)((Control)sender).Parent;
            if (frd.Find == "") return;

            int index = frd.currentIndex;
            string tts = frd.Find;
            if (frd.CaseIgnore) tts = tts.ToLower();

            if (frd.Down)
            {
                frd.Enabled = false;
                index++;
                if (index < (kmzLayers.Items.Count - 1))
                    for (int i = index; i < kmzLayers.Items.Count; i++)
                    {
                        if ((frd.CheckedOnly) && (!kmzLayers.GetItemChecked(i))) continue;
                        string text = kmzLayers.Items[i].ToString();
                        if (frd.CaseIgnore) text = text.ToLower();
                        if (text.Contains(tts))
                        {
                            kmzLayers.SetSelected(i, true);
                            frd.currentIndex = i;
                            frd.Enabled = true;
                            return;
                        };
                    };
                if (index >= 0)
                    for (int i = 0; (i <= index) && (i < kmzLayers.Items.Count); i++)
                    {
                        if ((frd.CheckedOnly) && (!kmzLayers.GetItemChecked(i))) continue;
                        string text = kmzLayers.Items[i].ToString();
                        if (frd.CaseIgnore) text = text.ToLower();
                        if (text.Contains(tts))
                        {
                            kmzLayers.SetSelected(i, true);
                            frd.currentIndex = i;
                            frd.Enabled = true;
                            return;
                        };
                    };
                frd.Enabled = true;
            }
            else
            {
                frd.Enabled = false;
                index--;
                if (index < 0) index = kmzLayers.Items.Count - 1;
                if (index >= 0)
                    for (int i = index; i >= 0; i--)
                    {
                        if ((frd.CheckedOnly) && (!kmzLayers.GetItemChecked(i))) continue;
                        string text = kmzLayers.Items[i].ToString();
                        if (frd.CaseIgnore) text = text.ToLower();
                        if (text.Contains(tts))
                        {
                            kmzLayers.SetSelected(i, true);
                            frd.currentIndex = i;
                            frd.Enabled = true;
                            return;
                        };
                    };
                if (index < (kmzLayers.Items.Count - 1))
                    for (int i = kmzLayers.Items.Count - 1; i >= index; i--)
                    {
                        if ((frd.CheckedOnly) && (!kmzLayers.GetItemChecked(i))) continue;
                        string text = kmzLayers.Items[i].ToString();
                        if (frd.CaseIgnore) text = text.ToLower();
                        if (text.Contains(tts))
                        {
                            kmzLayers.SetSelected(i, true);
                            frd.currentIndex = i;
                            frd.Enabled = true;
                            return;
                        };
                    };
                frd.Enabled = true;
            };
        }

        private void FindAll_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count == 0) return;
            if (sender == null) return;
            if (((Control)sender).Parent == null) return;
            if (!(((Control)sender).Parent is FindReplaceDlg)) return;

            FindReplaceDlg frd = (FindReplaceDlg)((Control)sender).Parent;
            if (frd.Find == "") return;

            int index = frd.currentIndex;
            string tts = frd.Find;
            if (frd.CaseIgnore) tts = tts.ToLower();

            frd.Enabled = false;
            int first_index = -1;
            for (int i = 0; i < kmzLayers.Items.Count; i++)
            {
                string text = kmzLayers.Items[i].ToString();
                if (frd.CaseIgnore) text = text.ToLower();
                if (text.Contains(tts))
                {
                    if (first_index < 0) first_index = i;
                    kmzLayers.SetItemChecked(i, true);
                };
            };
            if (first_index >= 0)
                kmzLayers.SetItemChecked(first_index, true);
            frd.Enabled = true;
        }

        private void akelPadAkelpadsourceforgeneToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string ts = (sender as ToolStripMenuItem).Text.Split(new char[] { ':' }, 2)[1].Trim();
            try
            {
                System.Diagnostics.Process.Start("http://" + ts);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Online Web Services and Resources", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
        }

        private void convertOSMpbdToxmlAndImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string osmconvert = CurrentDirectory()+@"\OSM\osmconvert.exe";
            if(!File.Exists(osmconvert))
            {
                MessageBox.Show("osmconvert.exe not found!", "Convert OSM .pbf to .osm and Import ...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".pbf";
            ofd.Filter = "OSM Export File (*.pbf;*.osm)|*.pbf;*.osm";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string toOpen = Path.GetDirectoryName(ofd.FileName).Trim('\\') + @"\" + Path.GetFileNameWithoutExtension(ofd.FileName) + "[converted].osm";
                System.IO.FileStream fs = new FileStream(CurrentDirectory() + @"\OSM\convert.cmd", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                sw.WriteLine(String.Format("osmconvert.exe \"{0}\" --drop-ways --drop-relations --out-osm -o=\"{1}\"", ofd.FileName, toOpen));
                sw.Close();
                fs.Close();

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(CurrentDirectory() + @"\OSM\convert.cmd");
                psi.WorkingDirectory = CurrentDirectory() + @"\OSM\";
                System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi);
                proc.WaitForExit();
                if(File.Exists(toOpen))
                    LoadFiles(new string[] { toOpen });
                else
                    MessageBox.Show("Result file not found!", "Convert OSM .pbf to .osm and Import ...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            ofd.Dispose();
        }

        public void ImportZoneFromOSM(double[] bounds)
        {
            string osmconvert = CurrentDirectory() + @"\OSM\osmconvert.exe";
            if (!File.Exists(osmconvert))
            {
                MessageBox.Show("osmconvert.exe not found!", "Convert OSM .pbf to .osm and Import ...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".pbf";
            ofd.Filter = "OSM Export File (*.pbf;*.osm)|*.pbf;*.osm";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string toOpen = Path.GetDirectoryName(ofd.FileName).Trim('\\') + @"\" + Path.GetFileNameWithoutExtension(ofd.FileName) + "[converted].osm";
                System.IO.FileStream fs = new FileStream(CurrentDirectory() + @"\OSM\convert.cmd", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                string btxt = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2},{3}", new object[] { bounds[0], bounds[1], bounds[2], bounds[3] });
                sw.WriteLine(String.Format("osmconvert.exe \"{0}\" --drop-ways --drop-relations -b=" + btxt + " --out-osm -o=\"{1}\"", ofd.FileName, toOpen));
                sw.Close();
                fs.Close();

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(CurrentDirectory() + @"\OSM\convert.cmd");
                psi.WorkingDirectory = CurrentDirectory() + @"\OSM\";
                System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi);
                proc.WaitForExit();
                if (File.Exists(toOpen))
                    LoadFiles(new string[] { toOpen });
                else
                    MessageBox.Show("Result file not found!", "Convert OSM .pbf to .osm and Import ...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            ofd.Dispose();
        }

        private void CFPBF_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(CurrentDirectory() + @"\KMZPOIfromOSM.exe"))
                    System.Diagnostics.Process.Start(CurrentDirectory() + @"\KMZPOIfromOSM.exe");
            }
            catch { };
        }

        private void sdfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(CurrentDirectory() + @"\KMZPOIfromOSM.exe"))
                    System.Diagnostics.Process.Start(CurrentDirectory() + @"\KMZPOIfromOSM.exe");
            }
            catch { };
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            if(File.Exists(f.src_file_pth))
            try
            {
                Rectangle rect = kmzFiles.GetItemRectangle(kmzFiles.SelectedIndex);
                GongSolutions.Shell.ShellItem si = new GongSolutions.Shell.ShellItem(f.src_file_pth);
                GongSolutions.Shell.ShellContextMenu scm = new GongSolutions.Shell.ShellContextMenu(si);
                scm.ShowContextMenu((Control)kmzFiles, new Point(rect.Left + 100, rect.Top + rect.Height - 2));
            }
            catch { };
        }

        private void convertToGarminPointsOfInterestsFileGPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedItems.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select GPI file";
            sfd.Filter = "Garmin Points of Interests File (*.gpi)|*.gpi";
            sfd.DefaultExt = ".gpi";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                string zdir = Save2KMZ(sfd.FileName, true);
                ReloadXMLOnly_NoUpdateLayers();
                KMFile kmf = KMFile.FromZDir(zdir);
                Save2GPI(sfd.FileName, kmf); 
            };
            sfd.Dispose(); 
        }

        private void gPIFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndices.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select GPI file";
            sfd.Filter = "Garmin Points of Interests File (*.gpi)|*.gpi";
            sfd.DefaultExt = ".gpi";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                Save2GPI(sfd.FileName, (KMFile)kmzFiles.SelectedItems[0]);
            };
            sfd.Dispose();
        }

        private void navitelgdbFavoritescsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                GDB2SCV_AND_BACK();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "GDB <--> CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
        }
        private void GDB2SCV_AND_BACK()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select file";
            ofd.Filter = "CSV and GDB files (*.csv;*.gdb)|*.csv;*.gdb";
            ofd.DefaultExt = ".dat";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(ofd.FileName.ToLower()) == ".gdb")
                {
                    NavitelRecord[] recs = NavitelGDB.ReadFile(ofd.FileName);
                    if ((recs != null) && (recs.Length > 0))
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.FileName = Path.GetFileNameWithoutExtension(ofd.FileName) + ".csv";
                        sfd.DefaultExt = ".csv";
                        sfd.Filter = "CSV files (*.csv)|*.csv";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                            sw.WriteLine("#CODEPAGE: Windows-1251");
                            sw.WriteLine("#ICONTYPES: 0..248");
                            sw.WriteLine("#TOTAL POI: " + recs.Length.ToString());
                            sw.WriteLine("ID;ICON;TYPE;NAME;DESCRIPTION;LATITUDE;LONGITUDE");
                            for (int i = 0; i < recs.Length; i++)
                            {
                                sw.Write(i.ToString() + ";");
                                sw.Write(recs[i].iconIndex.ToString() + ";");
                                sw.Write(recs[i].iconName + ";");
                                sw.Write(recs[i].name.Replace(";", ",").Replace("\r\n", " ") + ";");
                                sw.Write(recs[i].desc.Replace(";", ",").Replace("\r\n", " ") + ";");
                                sw.Write(recs[i].lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + ";");
                                sw.Write(recs[i].lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + ";");
                                sw.WriteLine();
                            };
                            sw.Close();
                            fs.Close();
                            MessageBox.Show("Saved " + recs.Length.ToString() + " POI to CSV", "GDB --> CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        };
                        sfd.Dispose();
                    };
                }
                else if (Path.GetExtension(ofd.FileName.ToLower()) == ".csv")
                {
                    FileInfo fi = new FileInfo(ofd.FileName);
                    if (fi.Length > 70)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.FileName = Path.GetFileNameWithoutExtension(ofd.FileName) + ".gdb";
                        sfd.DefaultExt = ".dat";
                        sfd.Filter = "GDB files (*.gdb)|*.gdb";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            List<NavitelRecord> recs = new List<NavitelRecord>();
                            FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
                            StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
                            bool firstline = true;
                            int[] si = new int[] { 0, 1, 2, 3, 4, 5, 6 };
                            char delimiter = ';';
                            while (!sr.EndOfStream)
                            {
                                string line = sr.ReadLine().Trim();
                                if (String.IsNullOrEmpty(line)) continue;
                                if (line.StartsWith("#")) continue;
                                if (firstline)
                                {
                                    firstline = false;
                                    if (line.IndexOf(',') > 0) delimiter = ',';
                                    if (line.IndexOf('\t') > 0) delimiter = '\t';
                                    if (line.IndexOf(';') > 0) delimiter = ';';
                                    string[] delimiters = new string[] { ";", ",", "TAB" };
                                    string del = delimiter.ToString(); if (delimiter == '\t') del = "TAB";
                                    if (System.Windows.Forms.InputBox.Show("GDB --> DAT", "Select delimiter:", delimiters, ref del) == DialogResult.OK)
                                    {
                                        if (del == "TAB") delimiter = '\t';
                                        else delimiter = del[0];

                                        List<string> dline = new List<string>(line.Split(new char[] { delimiter }, 10));
                                        if (dline.Count < 5)
                                        {
                                            MessageBox.Show("Invalid Columns Count\r\nMust Be >= 5", "GDB --> DAT", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            sr.Close();
                                            fs.Close();
                                            return;
                                        };

                                        si[0] = dline.IndexOf("ID");
                                        si[1] = dline.IndexOf("ICON");
                                        si[2] = dline.IndexOf("TYPE");
                                        si[3] = dline.IndexOf("NAME");
                                        si[4] = dline.IndexOf("DESCRIPTION");
                                        si[5] = dline.IndexOf("LATITUDE");
                                        si[6] = dline.IndexOf("LONGITUDE");

                                        if (si[3] < 0) { MessageBox.Show("Field NAME not Found!", "GDB --> DAT", MessageBoxButtons.OK, MessageBoxIcon.Error); return; };
                                        if (si[5] < 0) { MessageBox.Show("Field LATITUDE not Found!", "GDB --> DAT", MessageBoxButtons.OK, MessageBoxIcon.Error); return; };
                                        if (si[6] < 0) { MessageBox.Show("Field LONGITUDE not Found!", "GDB --> DAT", MessageBoxButtons.OK, MessageBoxIcon.Error); return; };
                                        continue;
                                    };
                                    sr.Close();
                                    fs.Close();
                                    return;
                                };
                                string[] pline = line.Split(new char[] { delimiter }, 10);
                                NavitelRecord rec = new NavitelRecord();
                                rec.iconIndex = 0;
                                if (si[1] >= 0)
                                {
                                    uint.TryParse(pline[si[1]], out rec.iconIndex);
                                };
                                if (si[3] >= 0)
                                {
                                    rec.name = pline[si[3]].Trim();
                                    if (rec.name.Length > 128) rec.name = rec.name.Remove(128);
                                };
                                if (si[4] >= 0)
                                {
                                    rec.desc = pline[si[4]].Trim();
                                    if (rec.desc.Length > 128) rec.desc = rec.desc.Remove(128);
                                };
                                if (si[5] >= 0)
                                    rec.lat = double.Parse(pline[si[5]].Replace(",", ".").Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                if (si[6] >= 0)
                                    rec.lon = double.Parse(pline[si[6]].Replace(",", ".").Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                if ((rec.lat != 0) && (rec.lon != 0) && (!String.IsNullOrEmpty(rec.name)))
                                    recs.Add(rec);
                            };
                            sr.Close();
                            fs.Close();
                            if (recs.Count > 0)
                            {
                                NavitelGDB.WriteFile(sfd.FileName, recs.ToArray());
                                MessageBox.Show("Saved " + recs.Count.ToString() + " POI to GDB", "GDB --> DAT", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            };
                        };
                        sfd.Dispose();
                    };
                }
                else
                    MessageBox.Show("Wrong File Type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            };
            ofd.Dispose();
        }

        private void saveLayerToGDBNavitelgdbFileForNavitelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            Export2GDB((KMLayer)kmzLayers.SelectedItem);
        }

        private void Export2GDB(KMLayer kml)
        {
            XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
            XmlNodeList xns = xn.SelectNodes("Placemark/Point/coordinates");
            if (xns.Count > 0)
            {
                string filename = null;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save to";
                sfd.DefaultExt = ".gdb";
                sfd.Filter = "Navitel Waypoints (*.gdb)|*.gdb";
                try
                {
                    sfd.FileName = kml.name + ".gdb";
                }
                catch { };
                if (sfd.ShowDialog() == DialogResult.OK)
                    filename = sfd.FileName;                    
                sfd.Dispose();
                if (String.IsNullOrEmpty(filename)) return;
                
                //////////////////////////////////////////

                List<string> styles = new List<string>();
                List<int> new_styles = new List<int>();
                ImageList imlF = new ImageList();
                IMM imm = new IMM();
                CRC32 crc = new CRC32();

                for (int x = 0; x < xns.Count; x++)
                {
                    string style = "none";
                    XmlNode stn = xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                    if ((stn != null) && (stn.ChildNodes.Count > 0))
                    {
                        style = stn.ChildNodes[0].Value;
                        if (styles.IndexOf(style) < 0) 
                        {
                            styles.Add(style);
                            new_styles.Add(0);
                            string im = style.Replace("#","");
                            XmlNode him = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + im + "']/IconStyle/Icon/href");
                            if (him != null)
                            {
                                im = kml.file.tmp_file_dir + him.InnerText.Replace("/", @"\");
                                imlF.Images.Add(Image.FromFile(im));
                                imm.Set(crc.CRC32Num(im), style);
                            }
                            else
                                imlF.Images.Add(new Bitmap(16, 16));
                        };
                    };                    
                };

                //////////////////////////////////////////                

                // LIST STYLES //
                if (styles.Count > 0)
                {
                    imlF.ImageSize = new Size(16, 16);
                    XmlNode sh = kml.file.kmlDoc.SelectSingleNode("kml/Document/style_history");
                    string sht = sh == null ? "" : sh.InnerText;
                    RenameDat rd = RenameDat.CreateForGDB(sht, kml.file.tmp_file_dir + @"images\");
                    rd.listView2.SmallImageList = imlF;
                    rd.imm = imm;
                    for (int i = 0; i < styles.Count; i++)
                    {
                        ListViewItem lvi = new ListViewItem(styles[i]);
                        lvi.SubItems.Add(new_styles[i].ToString("000")+" - "+NavitelRecord.IconList[new_styles[i]]);
                        rd.listView2.Items.Add(lvi);
                        lvi.ImageIndex = i;
                    };
                    rd.Autodetect();
                    if (rd.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < styles.Count; i++)
                            new_styles[i] = rd.nlTexts.IndexOf(rd.listView2.Items[i].SubItems[1].Text);
                        imm = rd.imm;
                    }
                    else
                    {
                        rd.Dispose();
                        sfd.Dispose();
                        return;
                    };
                    bool sort = rd.DoSort;
                    bool remd = rd.RemoveDescriptions;
                    rd.Dispose();

                    // PROCESS //
                    log.Text = "";
                    AddToLog("Saving points to Navitel GDB...");
                    List<NavitelRecord> recs = new List<NavitelRecord>();
                    for (int x = 0; x < xns.Count; x++)
                    {
                        string[] llz = xns[x].ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        string name = xns[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value.Replace(",", ";");
                        string style = "none";
                        XmlNode stn = xns[x].ParentNode.ParentNode.SelectSingleNode("styleUrl");
                        if ((stn != null) && (stn.ChildNodes.Count > 0))
                            style = stn.ChildNodes[0].Value;
                        int icon = styles.IndexOf(style) < 0 ? 0 : new_styles[styles.IndexOf(style)];
                        string desc = "";
                        XmlNode std = xns[x].ParentNode.ParentNode.SelectSingleNode("description");
                        if ((std != null) && (std.ChildNodes.Count > 0))
                            desc = std.ChildNodes[0].Value;

                        bool toTop = false;
                        if (!String.IsNullOrEmpty(desc))
                        {
                            string dtl = desc.ToLower();
                            if (dtl.IndexOf("progorod_dat_home=yes") >= 0) toTop = true;
                            if (dtl.IndexOf("progorod_dat_home=1") >= 0) toTop = true;
                            if (dtl.IndexOf("progorod_dat_home=true") >= 0) toTop = true;
                            if (dtl.IndexOf("progorod_dat_office=yes") >= 0) toTop = true;
                            if (dtl.IndexOf("progorod_dat_office=1") >= 0) toTop = true;
                            if (dtl.IndexOf("progorod_dat_office=true") >= 0) toTop = true;
                            dtl = (new Regex(@"[\w]+=[\S\s][^\r\n]+")).Replace(dtl, ""); // Remove TAGS
                        };
                        if (remd) desc = "";

                        NavitelRecord rec = new NavitelRecord();
                        rec.name = name;
                        rec.desc = desc;
                        rec.lat = double.Parse(llz[1].Replace("\r", "").Replace("\n", "").Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture);
                        rec.lon = double.Parse(llz[0].Replace("\r", "").Replace("\n", "").Replace(" ", ""), System.Globalization.CultureInfo.InvariantCulture);
                        rec.iconIndex = (uint)icon;
                        rec.__toTop = toTop;
                        if (toTop)
                            recs.Insert(0, rec);
                        else
                            recs.Add(rec);                        
                    };
                    if (recs.Count > 0)
                    {
                        if (sort)
                            recs.Sort(new NavitelGDB.NavitelRecordSorter());
                        NavitelGDB.WriteFile(filename, recs.ToArray());
                        if (imm.save2file) imm.Save(filename + ".imm");
                    };
                    AddToLog("Saved " + recs.Count.ToString() + " points");
                    AddToLog(String.Format("Saving data to selected file: {0}", filename));
                    AddToLog("Done");
                };
                //////////////////////////////////////////
                return;
            };

            AddToLog("File not created: Layer has no placemarks to save in GDB format!");
            MessageBox.Show("Layer has no placemarks to save in gdb format!", "File not created", MessageBoxButtons.OK, MessageBoxIcon.Information);                        
        }

        private void fRemoveOSMTagsFromDescriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count == 0) return;

            string egex = @"(tag_[\w\-_]+\=[^\r\n]*)|(name:[^(ru|en)]+\=[^\r\n]*)|((\r+\n){2,})";
            if (System.Windows.Forms.InputBox.QueryRegexBox("Remove OSM tags from Description", "Pattern to delete (regex):", "Test text here:", ref egex) != DialogResult.OK)
                return;
            waitBox.Show("Remove description", "Wait, applying changes ... ");

            int proc_layers = 0;
            int proc_items = 0;

            for (int il = 0; il < kmzLayers.Items.Count; il++)
            {
                bool proc_l = false;

                waitBox.Text = String.Format("Remove descriptions, layer {0}/{1}", il, kmzLayers.Items.Count);

                KMLayer l = (KMLayer)kmzLayers.Items[il];               
                XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                XmlNodeList xns = xn.SelectNodes("Placemark");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        waitBox.Text = String.Format("Remove descriptions, layer {0}/{1}, placemarks {2}/{3}", il, kmzLayers.Items.Count, x, xns.Count);

                        bool proc_i = false;

                        XmlNode xd = xns[x].SelectSingleNode("description");
                        string desc = "";
                        if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
                        if (!String.IsNullOrEmpty(desc))
                        {
                            Regex eg = new Regex(egex, RegexOptions.IgnoreCase);
                            MatchCollection mc = eg.Matches(desc);
                            if (mc.Count > 0)
                            {
                                proc_i = true;
                                proc_l = true;
                                for (int ii = mc.Count - 1; ii >= 0; ii--)
                                    desc = desc.Remove(mc[ii].Index, mc[ii].Length);
                            };
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
                            xns[x].AppendChild(xd);
                        };
                        xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                        if (proc_i) proc_items++;
                    };
                l.file.SaveKML();
                if (proc_l) proc_layers++;
            };            
            waitBox.Hide();

            MessageBox.Show("Processed " + proc_items.ToString() + " items in " + proc_layers.ToString() + " layers", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void allLayersFRemoveDescriptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count == 0) return;

            waitBox.Show("Remove description", "Wait, applying changes ... ");

            for (int il = 0; il < kmzLayers.Items.Count; il++)
            {
                waitBox.Text = String.Format("Remove descriptions, layer {0}/{1}", il, kmzLayers.Items.Count);

                KMLayer l = (KMLayer)kmzLayers.Items[il];
                XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                XmlNodeList xns = xn.SelectNodes("Placemark");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        waitBox.Text = String.Format("Remove descriptions, layer {0}/{1}, placemarks {2}/{3}", il, kmzLayers.Items.Count, x, xns.Count);

                        XmlNode xd = xns[x].SelectSingleNode("description");
                        string desc = "";
                        if (xd != null)
                            xd.RemoveAll();
                        else
                        {
                            xd = l.file.kmlDoc.CreateElement("description");
                            xns[x].AppendChild(xd);
                        };
                        xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));
                    };
                l.file.SaveKML();
            };
            waitBox.Hide();
        }

        private void allLayersFBatchReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count == 0) return;

            string w_th = "(&amp;)";
            string w_at = "&";
            if (System.Windows.Forms.InputBox.QueryReplaceBox("Batch Replace Text", "Search to replace (regex) in name and desc:", "Replace with:", ref w_th, ref w_at) != DialogResult.OK)
                return;
            waitBox.Show("Replace Text", "Wait, applying changes ... ");

            int proc_layers = 0;
            int proc_items = 0;

            for (int il = 0; il < kmzLayers.Items.Count; il++)
            {
                bool proc_l = false;
                waitBox.Text = String.Format("Replace Text, layer {0}/{1}", il, kmzLayers.Items.Count);

                KMLayer l = (KMLayer)kmzLayers.Items[il];
                XmlNode xn = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                XmlNodeList xns = xn.SelectNodes("Placemark");
                if (xns.Count > 0)
                    for (int x = 0; x < xns.Count; x++)
                    {
                        waitBox.Text = String.Format("Replace Text, layer {0}/{1}, placemarks {2}/{3}", il, kmzLayers.Items.Count, x, xns.Count);

                        bool proc_i = false;

                        string nam_ = "";
                        XmlNode xm = xns[x].SelectSingleNode("name");
                        if ((xm != null) && (xm.ChildNodes.Count > 0)) nam_ = xm.ChildNodes[0].Value;
                        if (!String.IsNullOrEmpty(nam_))
                        {
                            Regex eg = new Regex(w_th, RegexOptions.IgnoreCase);
                            MatchCollection mc = eg.Matches(nam_);
                            if (mc.Count > 0)
                            {
                                proc_i = true;
                                proc_l = true;
                                for (int ii = mc.Count - 1; ii >= 0; ii--)
                                {
                                    nam_ = nam_.Remove(mc[ii].Index, mc[ii].Length);
                                    nam_ = nam_.Insert(mc[ii].Index, w_at);
                                };
                            };
                            nam_ = nam_.Trim();
                        };
                        if (xm != null)
                            xm.RemoveAll();
                        else
                        {
                            xm = l.file.kmlDoc.CreateElement("name");
                            xns[x].AppendChild(xm);
                        };
                        xm.AppendChild(l.file.kmlDoc.CreateTextNode(nam_));

                        XmlNode xd = xns[x].SelectSingleNode("description");
                        string desc = "";
                        if ((xd != null) && (xd.ChildNodes.Count > 0)) desc = xd.ChildNodes[0].Value;
                        if (!String.IsNullOrEmpty(desc))
                        {
                            Regex eg = new Regex(w_th, RegexOptions.IgnoreCase);
                            MatchCollection mc = eg.Matches(desc);
                            if (mc.Count > 0)
                            {
                                proc_i = true;
                                proc_l = true;
                                for (int ii = mc.Count - 1; ii >= 0; ii--)
                                {
                                    desc = desc.Remove(mc[ii].Index, mc[ii].Length);
                                    desc = desc.Insert(mc[ii].Index, w_at);
                                };
                            };
                            desc = desc.Trim();
                        };
                        if (xd != null)
                            xd.RemoveAll();
                        else
                        {
                            xd = l.file.kmlDoc.CreateElement("description");
                            xns[x].AppendChild(xd);
                        };
                        xd.AppendChild(l.file.kmlDoc.CreateTextNode(desc));

                        if (proc_i) proc_items++;
                    };
                l.file.SaveKML();
                if (proc_l) proc_layers++;
            };            
            waitBox.Hide();

            MessageBox.Show("Processed " + proc_items.ToString() + " items in " + proc_layers.ToString() + " layers", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }                     

        private void SortAdd_Click(object sender, EventArgs e)
        {
            SortLayers(0);
        }

        private void SortAsc_Click(object sender, EventArgs e)
        {
            SortLayers(1);
        }   

        private void SortCount_Click(object sender, EventArgs e)
        {
            SortLayers(2);
        }

        private void sortByCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortLayers(3);
        }

        private void findSimilarStyleIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sv = 0;
            if (System.Windows.Forms.InputBox.Show("Find Similar Images:", "Compare method:", 
                new string[] { 
                    "File Size + CRC32", 
                    "Mean Absolute",
                    "Absolute",
                    "Mean Error per Pixel",
                    "Squared",
                    "Normalized Cross Correlation",
                    "Perceptual Hash"}, 
                ref sv) != DialogResult.OK)
                return;
            FindSimilarImagesInStyles(sv);
        }

        private struct OSNS
        {
            public string newstyle;
            public string filename;
            public OSNS(string newstyle, string filename) { this.newstyle = newstyle; this.filename = filename; }
            public override string ToString() { return newstyle; }
        }
        private void FindSimilarImagesInStyles(int method)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string path = f.tmp_file_dir;

            int files_del = 0, styles_removed = 0, styles_repl = 0;

            waitBox.Show("Find Similar Style Icons", "Wait...");
            AddToLog("Find Similar Style Icons ...");
            Dictionary<string, OSNS> replaceMap = new Dictionary<string, OSNS>();
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(path + @"\images\", "*.png"));
            // Step 1 -- remove not used files and similar styles
            {
                Dictionary<string, int> flsCnt = new Dictionary<string, int>();
                foreach (string file in files) flsCnt.Add(file.Substring(file.LastIndexOf(@"\") + 1), 0);

                waitBox.Show("Find Similar Style Icons", "Wait, analyzing Styles...");
                XmlNodeList nl = f.kmlDoc.SelectNodes("kml/Document/Style/IconStyle/Icon/href");
                foreach (XmlElement ne in nl)
                {
                    string imFile = path + @"\" + ne.ChildNodes[0].Value.Replace("/", @"\");
                    string stSrc = ne.ChildNodes[0].Value.Substring(ne.ChildNodes[0].Value.IndexOf("/") + 1);
                    if (flsCnt.ContainsKey(stSrc)) flsCnt[stSrc]++;
                    string stId = ne.ParentNode.ParentNode.ParentNode.Attributes["id"].Value;
                    string stNn = null;                    
                    foreach(KeyValuePair<string, OSNS> kv in replaceMap)
                        if (kv.Value.filename == stSrc)
                        {
                            stNn = kv.Key;
                            break;
                        };
                    if(!replaceMap.ContainsKey(stId))
                        replaceMap.Add(stId, new OSNS(stNn, stSrc));
                };

                waitBox.Show("Find Similar Style Icons", "Wait, removing files...");
                List<string> ex_files = new List<string>();
                foreach (KeyValuePair<string, int> ff in flsCnt)
                    if (ff.Value == 0)
                    {
                        AddToLog("Delete file [" + (files_del + 1).ToString() + "] " + ff.Key);
                        File.Delete(path + @"\images\" + ff.Key);
                        files_del++;
                    }
                    else
                        ex_files.Add(path + @"\images\" + ff.Key);
                files = ex_files;


                waitBox.Show("Find Similar Style Icons", "Applying similar styles...");
                Dictionary<string, OSNS> newrm = new Dictionary<string, OSNS>();
                foreach (KeyValuePair<string, OSNS> kv in replaceMap)
                    if (!String.IsNullOrEmpty(kv.Value.newstyle))
                    {
                        XmlNode nn = f.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + kv.Key + "']");
                        if (nn != null)
                        {
                            AddToLog("Remove style [" + (styles_removed + 1).ToString() + "] " + kv.Key);
                            nn.ParentNode.RemoveChild(nn);
                            styles_removed++;
                        };
                        XmlNodeList nX = f.kmlDoc.SelectNodes("kml/Document/Folder/Placemark/styleUrl[text()='#" + kv.Key + "']");
                        foreach (XmlElement nE in nX)
                        {
                            nE.ChildNodes[0].Value = "#" + kv.Value.newstyle;
                            styles_repl++;
                        };
                    }
                    else
                        newrm.Add(kv.Key, kv.Value);
                replaceMap = newrm;
            };
            if ((files != null) && (files.Count > 1))
            {
                int TTL = -1;
                for (int ia = 0; ia < files.Count; ia++)
                {
                    int LEFT = 0;
                    for (int ib = ia; ib < files.Count; ib++)
                        LEFT += files.Count - ib - 1;
                    if(TTL == -1) TTL = LEFT;
       
                    string keya = null;
                    string fA = files[ia].Substring(files[ia].LastIndexOf(@"\") + 1);
                    foreach (KeyValuePair<string, OSNS> kv in replaceMap)
                        if (kv.Value.filename == fA)
                            keya = kv.Key;
                    for (int ib = files.Count - 1; ib > ia; ib--)
                    {
                        waitBox.Show("Find Similar Style Icons", "Wait, comparing images ... " + ((TTL - LEFT) * 100 / TTL).ToString() + "% (" + LEFT.ToString() + " left)");
                        bool same = CompareImages(files[ia], files[ib], method);
                        if (same)
                        {
                            string fB = files[ib].Substring(files[ia].LastIndexOf(@"\") + 1);
                            File.Delete(path + @"\images\" + fB);
                            AddToLog("Delete file [" + (files_del + 1).ToString() + "] " + fB);
                            files_del++;
                            foreach (KeyValuePair<string, OSNS> kv in replaceMap)
                                if (kv.Value.filename == fB)
                                {
                                    XmlNode nn = f.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + kv.Key + "']");
                                    if (nn != null)
                                    {
                                        AddToLog("Remove style [" + (styles_removed + 1).ToString() + "] " + kv.Key);
                                        nn.ParentNode.RemoveChild(nn);
                                        styles_removed++;
                                    };
                                    XmlNodeList nX = f.kmlDoc.SelectNodes("kml/Document/Folder/Placemark/styleUrl[text()='#" + kv.Key + "']");
                                    foreach (XmlElement nE in nX)
                                    {
                                        nE.ChildNodes[0].Value = "#" + keya;
                                        styles_repl++;
                                    };
                                };
                            files.RemoveAt(ib);
                        };
                        LEFT--;
                    };
                };
            };
            AddToLog("Deleted " + files_del.ToString() + " files\r\nRemoved " + styles_removed + " styles\r\nReplaced " + styles_repl + " styles");
            AddToLog("Done");
            waitBox.Hide();
            MessageBox.Show("Comparation Complete!\r\n\r\nDeleted " + files_del.ToString() + " files\r\nRemoved " + styles_removed + " styles\r\nReplaced " + styles_repl + " styles", "Find Similar Style Icons", MessageBoxButtons.OK, MessageBoxIcon.Information);
            f.SaveKML();       
        }

        public static bool CompareImages(string fileA, string fileB, int method)
        {
            if (fileA == fileB)
                return true;
            FileInfo fia = new FileInfo(fileA);
            FileInfo fib = new FileInfo(fileB);
            if (fia.Length == fib.Length)
            {
                CRC32 crc = new CRC32();
                if (crc.CRC32Num(fileA) == crc.CRC32Num(fileB))
                    return true;
            };
            if (method == 0) return false;
            ImageMagick.MagickImage a = new ImageMagick.MagickImage(fileA);
            ImageMagick.MagickImage b = new ImageMagick.MagickImage(fileB);
            double d = double.MaxValue;
            if (method == 1)
                d = a.Compare(b, ImageMagick.ErrorMetric.MeanAbsolute);
            else if (method == 2)
                d = a.Compare(b, ImageMagick.ErrorMetric.Absolute);
            else if (method == 3)
                d = a.Compare(b, ImageMagick.ErrorMetric.MeanErrorPerPixel);
            else if (method == 4)
                d = a.Compare(b, ImageMagick.ErrorMetric.MeanSquared);
            else if (method == 5)
                d = a.Compare(b, ImageMagick.ErrorMetric.NormalizedCrossCorrelation);
            else if (method == 6)
                d = a.Compare(b, ImageMagick.ErrorMetric.PerceptualHash);
            a.Dispose();
            b.Dispose();
            return d < 0.0001;
        }
        
        private void KMZRebuilederForm_Resize(object sender, EventArgs e)
        {
            outName.Width = outPanel.Width - 250;
            pictureBox1.Left = kmzFiles.Width - 25;
            label7.Left = kmzFiles.Width - 25;
        }

        private void linkLabel4_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            iTNConverterToolStripMenuItem_Click(sender, e);
        }

        private void OpenSAS_click(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"C:\Program Files\SASPlanet\SASPlanet.exe"))
            {
                SASPlacemarkConnector sc = new SASPlacemarkConnector();
                if (!sc.SASisOpen)
                    System.Diagnostics.Process.Start(@"C:\Program Files\SASPlanet\SASPlanet.exe");
                else
                    sc.ShowSAS();
            }
            else
            {
                try
                {
                    System.Diagnostics.Process.Start("http://www.sasgis.org/sasplaneta/");
                }
                catch { };
            };
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.google.com/mymaps");
            }
            catch { };
        }

        private void kmzLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            int was_i = kmzFiles.SelectedByLayer;
            if (kmzLayers.SelectedIndex < 0)
                kmzFiles.SelectedByLayer = -1;
            else
            {
                KMLayer l = (KMLayer)kmzLayers.SelectedItem;
                for (int i = 0; i < kmzFiles.Items.Count; i++)
                {
                    KMFile f = (KMFile)kmzFiles.Items[i];
                    if (f == l.file)
                        kmzFiles.SelectedByLayer = i;
                };
            };
            if(was_i != kmzFiles.SelectedByLayer)
                kmzFiles.Refresh();
        }

        private void kmzFiles_Enter(object sender, EventArgs e)
        {
            kmzFiles.SelectedByLayer = -1;
        }

        private void sortByO_Click(object sender, EventArgs e)
        {
            SortLayers(4);
        }

        private void sortByL_Click(object sender, EventArgs e)
        {
            SortLayers(5);
        }

        private void sortByA_Click(object sender, EventArgs e)
        {
            SortLayers(6);
        }

        private void checkWithPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < kmzLayers.Items.Count; i++)
            {
                KMLayer l = (KMLayer)kmzLayers.Items[i];
                if (l.points > 0)
                    kmzLayers.SetItemChecked(i, true);
            };
        }

        private void checkWithLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < kmzLayers.Items.Count; i++)
            {
                KMLayer l = (KMLayer)kmzLayers.Items[i];
                if (l.lines > 0)
                    kmzLayers.SetItemChecked(i, true);
            };
        }

        private void checkWithPolygonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < kmzLayers.Items.Count; i++)
            {
                KMLayer l = (KMLayer)kmzLayers.Items[i];
                if (l.areas > 0)
                    kmzLayers.SetItemChecked(i, true);
            };
        }

        private void viewCheckedInKMZViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedItems.Count == 0) return;
            List<int> lays = new List<int>();
            KMFile f = ((KMLayer)kmzLayers.CheckedItems[0]).file;
            for (int i = 0; i < kmzLayers.CheckedItems.Count; i++)
            {
                if (f != ((KMLayer)kmzLayers.CheckedItems[i]).file) return;
                lays.Add(((KMLayer)kmzLayers.CheckedItems[i]).id);
            };

            IntPtr vh = IntPtr.Zero;
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\", true)
                    .CreateSubKey("KMZViewer"))
                    vh = (IntPtr)((int)key.GetValue("Handle"));
                if (vh != IntPtr.Zero)
                {
                    string wt = SASPlacemarkConnector.GetText(vh);
                    if ((!String.IsNullOrEmpty(wt)) && (wt.IndexOf("KMZ Viewer") == 0))
                    {
                        byte[] fnArr = System.Text.Encoding.UTF8.GetBytes(f.src_file_pth);
                        List<byte> data = new List<byte>();
                        data.AddRange(BitConverter.GetBytes(fnArr.Length));
                        data.AddRange(fnArr);
                        data.AddRange(BitConverter.GetBytes(lays.Count));
                        for(int i=0;i<lays.Count;i++)
                            data.AddRange(BitConverter.GetBytes(lays[i]));
                        ProcDataExchange.SendData(vh, this.Handle, XP_OPENLAYERS, data.ToArray());
                        SASPlacemarkConnector.SetForegroundWindow(vh);
                        SASPlacemarkConnector.SetActiveWindow(vh);
                        SASPlacemarkConnector.SetFocus(vh);
                        return;
                    };
                };
            }
            catch (Exception ex)
            {
            };

            if (File.Exists(CurrentDirectory() + @"KMZViewer.exe"))
                System.Diagnostics.Process.Start(CurrentDirectory() + @"KMZViewer.exe", "\"" + f.src_file_pth + "\"");
            else
                MessageBox.Show("KMZViewer Window Not Found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void saveToCSVHTMLReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedItems.Count == 0) return;
            KMLReport.ReportForm rf = new KMLReport.ReportForm();
            string rd = "";
            if (rf.ShowDialog() == DialogResult.OK)
                rd = rf.Repord.Text;
            rf.Dispose();
            if (String.IsNullOrEmpty(rd)) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select file name";
            sfd.Filter = "CSV files (*.csv)|*.csv|HTML files (*.htm)|*.htm";
            sfd.DefaultExt = ".csv";
            sfd.FileName = outName.Text + ".csv";
            if (sfd.ShowDialog() == DialogResult.OK)
            {                
                log.Text = "";
                Dictionary<string, string[]> rfd = null;
                try
                {
                    rfd = GetReportFields(rd);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in Report Template: \r\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    sfd.Dispose();
                    return;
                };
                if ((rfd != null) && (rfd.Count > 0))
                {
                    if (sfd.FilterIndex == 1)
                        Save2ReportCSV(sfd.FileName, rfd);
                    if (sfd.FilterIndex == 2)
                        Save2ReportHTML(sfd.FileName, rfd);
                };
            };
            sfd.Dispose();         
        }

        public Dictionary<string, string[]> GetReportFields(string text)
        {
            Dictionary<string, string[]> res = new Dictionary<string, string[]>();
            if(!String.IsNullOrEmpty(text))          
            {
                string[] lns = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lns.Length; i++)
                {                    
                    lns[i] = lns[i].Trim();
                    if (lns[i].Length == 0) continue;
                    if (lns[i][0] != '[') continue;
                    if (lns[i][lns[i].Length - 1] != ']') continue;
                    string fName = lns[i].Substring(1, lns[i].Length - 2);
                    string fText = lns[++i];
                    string fRegex = lns[++i].Substring(6);
                    res.Add(fName, new string[] { fText, fRegex });
                };
            };
            return res;
        }

        private Dictionary<string, string> plugins = new Dictionary<string, string>();
        private void PreLoadPlugins(ToolStripMenuItem topItem)
        {
            string pdir = CurrentDirectory() + @"\Plugins";
            if (!Directory.Exists(pdir)) return;
            string[] sdirs = Directory.GetDirectories(pdir);
            if ((sdirs == null) || (sdirs.Length == 0)) return;
            foreach (string dir in sdirs)
            {
                string name = Path.GetFileName(dir);
                string ddir = name;
                string nmf = dir + @"\name.txt";
                if (File.Exists(nmf))
                {
                    try
                    {
                        FileStream fs = new FileStream(nmf, FileMode.Open, FileAccess.Read);
                        StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                        name = sr.ReadToEnd();
                        sr.Close();
                        fs.Close();
                    }
                    catch { };
                };
                string[] fls = Directory.GetFiles(dir, "*.exe");                
                if ((fls != null) && (fls.Length != 0))
                {
                    Array.Sort(fls);
                    ToolStripMenuItem tsmi = new ToolStripMenuItem();
                    tsmi.Text = name;
                    try
                    {
                        topItem.DropDownItems.Add(tsmi);
                        tsmi.Click += new EventHandler(tsmi_Click);
                        plugins.Add(name, fls[0]);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Plugin Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    };
                };
            };
            topItem.Enabled = plugins.Count > 0;
        }

        private void tsmi_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                ToolStripMenuItem mi = (ToolStripMenuItem)sender;
                if (String.IsNullOrEmpty(mi.Text)) return;
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(plugins[mi.Text]);
                psi.EnvironmentVariables.Add("MAPLEFT", "-180.0");
                psi.EnvironmentVariables.Add("MAPBOTTOM", "-90.0");
                psi.EnvironmentVariables.Add("MAPRIGHT", "180.0");
                psi.EnvironmentVariables.Add("MAPTOP", "90.0");
                psi.UseShellExecute = false;
                psi.StandardOutputEncoding = Encoding.UTF8;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;
                string output = "";
                try
                {
                    KMZViewer.RunProcStdOutForm pf = new KMZViewer.RunProcStdOutForm(mi.Text, "Running file: " + Path.GetFileName(psi.FileName));
                    pf.WriteLine("Starting plugin ...");
                    if (pf.StartProcAndShowWhileRunning(psi) != DialogResult.OK)
                        return;
                    output = pf.StdText;
                    pf.Dispose();
                    if (!String.IsNullOrEmpty(output))
                    {
                        string[] lines = output.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        string fn = lines[lines.Length - 1];
                        if (File.Exists(fn))
                        {
                            LoadFiles(new string[] { fn });
                            return;
                        }
                        else 
                        {
                            fn = Path.GetDirectoryName(plugins[mi.Text]) + @"\" + fn;
                            if (File.Exists(fn))
                            {
                                LoadFiles(new string[] { fn });
                                return;
                            };
                        };
                    };
                    if (output.Length > 2000) output = output.Substring(0, 2000) + "\r\n...";
                    if (output.IndexOf("Error") > 0)
                        MessageBox.Show("Plugin " + mi.Name + " return ERROR\r\n" + output, mi.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    else
                        MessageBox.Show("Plugin " + mi.Name + " return nothing\r\n" + output, mi.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Coldn't run plugin " + mi.Name + "\r\nError: " + ex.Message.ToString(), mi.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
            };
        }

        private void removeDescriptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string xml = f.kmlDoc.OuterXml;

            Regex rx = new Regex(@"<description>((.|\n)*?)</description>", RegexOptions.IgnoreCase);
            rw_rw_txt = "<description></description>";
            rw_rw_cnt = 0;
            xml = rx.Replace(xml, new MatchEvaluator(OnRRR));
            MessageBox.Show("Removed " + rw_rw_cnt.ToString() + " description(s)", "Regex Replace");

            if (rw_rw_cnt == 0) return;

            waitBox.Show("Reloading", "Wait, reloading KML...");
            try
            {
                FileStream fs = new FileStream(f.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(xml);
                sw.Close();
                fs.Close();
            }
            catch { };
            f.LoadKML(true);
            ReloadListboxLayers(true);
            waitBox.Hide();
        }

        private void regexReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string xml = f.kmlDoc.OuterXml;

            FindReplaceDlg frd = new FindReplaceDlg();
            frd.Text = "Regex Replace KML";
            
            frd.label3.Text = "Use regexstorm.net to help";
            frd.UP.Visible = false;
            frd.DOWN.Visible = false;
            frd.OnlyChecked.Visible = false;

            frd.FindButton.Text = "Find First";
            frd.REPF.Text = "Find Next";
            frd.ReplaceButton.Text = "Find All";
            frd.ReplaceALL.Text = "Replace All";

            frd.FindText.Text = @"<description>((.|\n)*?)</description>";
            frd.ReplaceText.Text = "<description></description>";
            frd.xmlText = xml;

            frd.onFind += new EventHandler(OnRRFind);
            frd.onReplaceFind += new EventHandler(OnRRFindAll);
            frd.onReplace += new EventHandler(OnRRReplace);
            frd.onReplaceAll += new EventHandler(OnRRReplaceAll);

            frd.ShowDialog();
            xml = frd.xmlText;
            frd.Dispose();

            waitBox.Show("Reloading", "Wait, reloading KML...");
            try
            {
                FileStream fs = new FileStream(f.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(xml);
                sw.Close();
                fs.Close();
            }
            catch { };
            f.LoadKML(true);
            ReloadListboxLayers(true);
            waitBox.Hide();
        }

        private void OnRRFind(object sender, EventArgs e)
        {
            FindReplaceDlg dlg = (sender as Button).Parent as FindReplaceDlg;
            Regex rx = new Regex(dlg.FindText.Text, dlg.IgnoreCase.Checked ? RegexOptions.IgnoreCase : RegexOptions.None);

            MatchCollection mx = rx.Matches(dlg.xmlText);
            dlg.intIndex = 0;
            if (mx.Count > 0)
                MessageBox.Show(mx[dlg.intIndex].Value.Length > 1024 ? mx[dlg.intIndex].Value.Substring(0, 1024) : mx[dlg.intIndex].Value, "Regex Replace");                       
        }

        private void OnRRFindAll(object sender, EventArgs e)
        {
            FindReplaceDlg dlg = (sender as Button).Parent as FindReplaceDlg;
            Regex rx = new Regex(dlg.FindText.Text, dlg.IgnoreCase.Checked ? RegexOptions.IgnoreCase : RegexOptions.None);

            MatchCollection mx = rx.Matches(dlg.xmlText);
            dlg.intIndex++;
            if (mx.Count > dlg.intIndex)
                MessageBox.Show(mx[dlg.intIndex].Value.Length > 1024 ? mx[dlg.intIndex].Value.Substring(0, 1024) : mx[dlg.intIndex].Value, "Regex Replace");
            else if(mx.Count > 0)
            {
                dlg.intIndex = 0;
                MessageBox.Show(mx[dlg.intIndex].Value.Length > 1024 ? mx[dlg.intIndex].Value.Substring(0, 1024) : mx[dlg.intIndex].Value, "Regex Replace");
            };
        }

        private void OnRRReplace(object sender, EventArgs e)
        {
            FindReplaceDlg dlg = (sender as Button).Parent as FindReplaceDlg;
            Regex rx = new Regex(dlg.FindText.Text, dlg.IgnoreCase.Checked ? RegexOptions.IgnoreCase : RegexOptions.None);

            MatchCollection mx = rx.Matches(dlg.xmlText);
            MessageBox.Show("Found " + mx.Count.ToString() + " item(s)", "Regex Replace");
        }

        private void OnRRReplaceAll(object sender, EventArgs e)
        {
            FindReplaceDlg dlg = (sender as Button).Parent as FindReplaceDlg;
            Regex rx = new Regex(dlg.FindText.Text, dlg.IgnoreCase.Checked ? RegexOptions.IgnoreCase : RegexOptions.None);

            rw_rw_txt = dlg.ReplaceText.Text;
            rw_rw_cnt = 0;
            dlg.xmlText = rx.Replace(dlg.xmlText, new MatchEvaluator(OnRRR));
            MessageBox.Show("Replaced " + rw_rw_cnt.ToString() + " item(s)", "Regex Replace");
        }

        private string rw_rw_txt = null;
        private int rw_rw_cnt = 0;
        private string OnRRR(Match mx)
        {
            rw_rw_cnt++;
            return rw_rw_txt;
        }

        private void removeDescriptionOSMTagsTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string xml = f.kmlDoc.OuterXml;

            Regex rx = new Regex(@"(tag_[^=]*=[^\n\]<]*\n*)|((\r+\n){2,})", RegexOptions.IgnoreCase);
            rw_rw_txt = "";
            rw_rw_cnt = 0;
            xml = rx.Replace(xml, new MatchEvaluator(OnRRR));
            MessageBox.Show("Removed " + rw_rw_cnt.ToString() + " tag(s)", "Regex Replace");

            if (rw_rw_cnt == 0) return;

            waitBox.Show("Reloading", "Wait, reloading KML...");
            try
            {
                FileStream fs = new FileStream(f.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(xml);
                sw.Close();
                fs.Close();
            }
            catch { };
            f.LoadKML(true);
            ReloadListboxLayers(true);
            waitBox.Hide();
        }

        private void impToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "MapsForge POI (*.poi)|*.poi";
            ofd.DefaultExt = ".poi";
            string fn = null;
            if(ofd.ShowDialog() == DialogResult.OK)
                fn = ofd.FileName;
            ofd.Dispose();
            if(fn == null) return;
            ImportPOI(fn);
        }

        private void iMFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "MapsForge MAP (*.map)|*.map";
            ofd.DefaultExt = ".map";
            string fn = null;
            if (ofd.ShowDialog() == DialogResult.OK)
                fn = ofd.FileName;
            ofd.Dispose();
            if (fn == null) return;
            ImportMapsForgeMap(fn);
        }

        private void removeEmptyLayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count == 0) return;

            int lc = 0;
            for (int i = kmzLayers.Items.Count - 1; i >= 0; i--)
            {
                KMLayer l = (KMLayer)kmzLayers.Items[i];
                if (l.placemarks == 0) lc++;
            };
            if (lc == 0) return;

            if (MessageBox.Show("Do you want to delete "+lc.ToString()+" empty layers?", "Remove Empty Layers", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            Dictionary<KMFile, List<int>> cnt = new Dictionary<KMFile, List<int>>();
            for (int i = kmzLayers.Items.Count - 1; i >= 0; i--)
            {
                KMLayer l = (KMLayer)kmzLayers.Items[i];
                if (l.placemarks > 0) continue;
                if (!cnt.ContainsKey(l.file)) cnt.Add(l.file, new List<int>());
                cnt[l.file].Add(l.id);
            };
            foreach (KeyValuePair<KMFile, List<int>> kv in cnt)
            {
                kv.Value.Sort();
                for (int l = kv.Value.Count - 1; l >= 0; l--)
                {
                    try
                    {
                        XmlNode xn = kv.Key.kmlDoc.SelectNodes("kml/Document/Folder")[kv.Value[l]];
                        xn.ParentNode.RemoveChild(xn);                        
                    }
                    catch { };
                };
                kv.Key.SaveKML();
                kv.Key.LoadKML(true);
            };
            ReloadListboxLayers(true);

            kmzLayers_SelectedIndexChanged(sender, e);
        }

        private void trackSplitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            waitBox.Show("Wait", "Loading map...");
            TrackSplitter pc = new TrackSplitter(this, waitBox);
            waitBox.Hide();
            pc.ShowDialog();
            pc.Dispose();
            return;
        }

        private void changeLayerDescriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;

            KMLayer l = (KMLayer)kmzLayers.SelectedItem;
            XmlNode pmk = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
            string description = "";
            try { description = pmk.SelectSingleNode("description").ChildNodes[0].Value; }
            catch { };
            
            if (System.Windows.Forms.InputBox.QueryText("Change layer description", "Description:", ref description) == DialogResult.OK)
            {
                l.hasDesc = !String.IsNullOrEmpty(description);
                try {
                    XmlNode dnd = pmk.SelectSingleNode("description");
                    if (dnd != null) dnd.ParentNode.RemoveChild(dnd);
                    dnd = l.file.kmlDoc.CreateElement("description");
                    pmk.AppendChild(dnd);
                    dnd.AppendChild(l.file.kmlDoc.CreateTextNode(description));
                } catch { };
                l.file.SaveKML();
                kmzLayers.Refresh();
            };
        }

        private void ReloadOriginalFiles()
        {
            List<KMFile> ff = new List<KMFile>();
            for (int i = 0; i < kmzLayers.CheckedIndices.Count; i++)
            {
                KMFile f = ((KMLayer)kmzLayers.CheckedItems[i]).file;
                if (ff.IndexOf(f) < 0) ff.Add(f);
            };
            waitBox.Show("Reloading", "Wait, reloading original files...");
            foreach(KMFile f in ff)
                f.CopySrcFileToTempDirAndLoad();
            ReloadListboxLayers(true);
            waitBox.Hide();
        }

        private void exportToWPTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedIndices.Count == 0) return;
            string zdir = Save2KMZ(null, false, false);
            KMFile kf = KMFile.FromZDir(zdir);
            Export2WPT(kf.kmLayers[0]);
            ReloadOriginalFiles();
        }

        private void export2DatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedIndices.Count == 0) return;
            string zdir = Save2KMZ(null, false, false);
            KMFile kf = KMFile.FromZDir(zdir);
            Export2Dat(kf.kmLayers[0]);
            ReloadOriginalFiles();
        }

        private void export2GDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedIndices.Count == 0) return;
            string zdir = Save2KMZ(null, false, false);
            KMFile kf = KMFile.FromZDir(zdir);
            Export2GDB(kf.kmLayers[0]);
            ReloadOriginalFiles();
        }

        private void export2WPTnoIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedIndices.Count == 0) return;
            string zdir = Save2KMZ(null, false, false);
            KMFile kf = KMFile.FromZDir(zdir);
            Save2WPTNoIcons(kf.kmLayers[0]);
            ReloadOriginalFiles();
        }

        private void HelpDesc_Click(object sender, EventArgs e)
        {
            string help = "To Create GPI:\r\n";
            help += "  gpi_name_<IMAGECRC>=<FULLNAME>\r\n";
            help += "  gpi_subname_<IMAGECRC>=<SUBNAME>\r\n";
            help += "\r\nFor Route Planner\r\n";
            help += "  route_planner_skip=true\r\n";
            help += "  route_planner_skip=false\r\n";
            help += "  route_planner_delay=minutes\r\n";
            help += "  route_planner_doubled=true\r\n";
            help += "  route_planner_speed=kmph\r\n";
            help += "  route_planner_source=route\r\n";
            System.Windows.Forms.InputBox.QueryText("Mini Help", "Used tags in layers descriptions:", ref help);
        }

        private void setNameSubnamesForGPIByIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            LayerNameEditor4GPICRC(f, -1);
        }

        private void LayerNameEditor4GPICRC(KMFile f, int layerId)
        {
            string path = f.tmp_file_dir;
            List<string> files = new List<string>();
            Dictionary<string, string> d4crc_names = new Dictionary<string, string>();
            Dictionary<string, string> d4crc_subnames = new Dictionary<string, string>();
            Regex rxgpinn = new Regex(@"gpi_name_(?<crc>[^=]+)\s*=(?<name>[\S\s][^\r\n]+)", RegexOptions.IgnoreCase);
            Regex rxgpisn = new Regex(@"gpi_subname_(?<crc>[^=]+)\s*=(?<name>[\S\s][^\r\n]+)", RegexOptions.IgnoreCase);
            Regex rxgpiun = new Regex(@"gpi_(?:sub)?name_(?<crc>\d+)=", RegexOptions.IgnoreCase);

            if (layerId < 0)
            {
                files.AddRange(Directory.GetFiles(path, "*.png", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(path, "*.jpeg", SearchOption.AllDirectories));
                files.AddRange(Directory.GetFiles(path, "*.gif", SearchOption.AllDirectories));

                if (files.Count == 0)
                {
                    MessageBox.Show("No Images Found", "Set Names by CRC", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                };
                
                foreach (KMLayer l in f.kmLayers)
                {
                    XmlNode pmk = l.file.kmlDoc.SelectNodes("kml/Document/Folder")[l.id];
                    string desc = "";
                    try
                    {
                        desc = pmk.SelectSingleNode("description").ChildNodes[0].Value;
                        MatchCollection mc;
                        if (!String.IsNullOrEmpty(desc) && ((mc = rxgpisn.Matches(desc)).Count > 0))
                        {
                            foreach (Match mx in mc)
                                if (!d4crc_subnames.ContainsKey(mx.Groups["crc"].Value))
                                    d4crc_subnames.Add(mx.Groups["crc"].Value, mx.Groups["name"].Value);
                                else
                                    d4crc_subnames[mx.Groups["crc"].Value] = mx.Groups["name"].Value;
                        };
                        if (!String.IsNullOrEmpty(desc) && ((mc = rxgpinn.Matches(desc)).Count > 0))
                        {
                            foreach (Match mx in mc)
                                if (!d4crc_names.ContainsKey(mx.Groups["crc"].Value))
                                    d4crc_names.Add(mx.Groups["crc"].Value, mx.Groups["name"].Value);
                                else
                                    d4crc_names[mx.Groups["crc"].Value] = mx.Groups["name"].Value;
                        };
                    }
                    catch { };
                };                
            }
            else if (layerId < f.kmLayers.Count)
            {
                List<string> s2f = new List<string>();
                XmlNodeList nX = f.kmlDoc.SelectNodes("kml/Document/Folder")[layerId].SelectNodes("Placemark/styleUrl");
                foreach (XmlElement nE in nX)
                {
                    string val = nE.ChildNodes[0].Value;
                    if (val.StartsWith("#")) val = val.Remove(0, 1);
                    if (s2f.IndexOf(val) < 0) s2f.Add(val);
                };
                if (s2f.Count > 0)
                {
                    int s2fl = s2f.Count;
                    for (int i = 0; i < s2fl; i++)
                    {
                        XmlNodeList xns = f.kmlDoc.SelectNodes("kml/Document/StyleMap[@id='" + s2f[i] + "']");
                        if (xns.Count > 0)
                            for (int x = 0; x < xns.Count; x++)
                            {
                                string style = xns[x].Attributes["id"].Value;
                                foreach (XmlNode xn2 in xns[x].SelectNodes("Pair/styleUrl"))
                                {
                                    string su = xn2.ChildNodes[0].Value;
                                    if (su.IndexOf("#") == 0) su = su.Remove(0, 1);
                                    if (s2f.IndexOf(su) < 0) s2f.Add(su);
                                };
                            };
                    };
                };
                foreach (string s22 in s2f)
                {
                    XmlNode nn = f.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + s22 + "']");
                    if (nn != null) nn = nn.SelectSingleNode("IconStyle/Icon/href");
                    if (nn != null)
                    {
                        string href = nn.ChildNodes[0].Value;
                        string tmpf = f.tmp_file_dir + href;
                        if (!File.Exists(tmpf))
                            tmpf = f.tmp_file_dir + href.Replace("/", @"\");
                        if (File.Exists(tmpf))
                            if (files.IndexOf(tmpf) < 0)
                                files.Add(tmpf);
                    };
                };

                if (files.Count == 0)
                {
                    MessageBox.Show("No Images Found", "Set Names by CRC", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                };

                {
                    XmlNode pmk = f.kmlDoc.SelectNodes("kml/Document/Folder")[layerId];
                    string desc = "";
                    try
                    {
                        desc = pmk.SelectSingleNode("description").ChildNodes[0].Value;
                        MatchCollection mc;
                        if (!String.IsNullOrEmpty(desc) && ((mc = rxgpisn.Matches(desc)).Count > 0))
                        {
                            foreach (Match mx in mc)
                                if (!d4crc_subnames.ContainsKey(mx.Groups["crc"].Value))
                                    d4crc_subnames.Add(mx.Groups["crc"].Value, mx.Groups["name"].Value);
                                else
                                    d4crc_subnames[mx.Groups["crc"].Value] = mx.Groups["name"].Value;
                        };
                        if (!String.IsNullOrEmpty(desc) && ((mc = rxgpinn.Matches(desc)).Count > 0))
                        {
                            foreach (Match mx in mc)
                                if (!d4crc_names.ContainsKey(mx.Groups["crc"].Value))
                                    d4crc_names.Add(mx.Groups["crc"].Value, mx.Groups["name"].Value);
                                else
                                    d4crc_names[mx.Groups["crc"].Value] = mx.Groups["name"].Value;
                        };
                    }
                    catch { };
                };                
            };

            if (files.Count == 0) return;
            GMLayRenamerForm grf = GMLayRenamerForm.ForCRCCheckSum(files.ToArray(), d4crc_names, d4crc_subnames);
            if (grf.ShowDialog() == DialogResult.OK)
            {
                string caption = layerId < 0 ? "GPI Tags for Layer description:" : "Save GPI Tags to Layer description?";
                string txt = "";
                foreach (Category cat in grf.categories)
                    if (!String.IsNullOrEmpty(cat.name))
                    {
                        if(grf.IsSubName(cat.CustomSymbol))
                            txt += "gpi_subname_" + cat.ID + "=" + cat.name + "\r\n";
                        else
                            txt += "gpi_name_" + cat.ID + "=" + cat.name + "\r\n";
                    };
                if ((System.Windows.Forms.InputBox.QueryText("Set Names by CRC", caption, ref txt) == DialogResult.OK) && layerId >= 0)
                {
                    XmlNode pmk = f.kmlDoc.SelectNodes("kml/Document/Folder")[layerId];
                    string description = "";
                    try { description = pmk.SelectSingleNode("description").ChildNodes[0].Value; }
                    catch { };
                    if (String.IsNullOrEmpty(description))
                        description = txt;                        
                    else
                    {
                        List<string> gSave = new List<string>(txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                        List<string> gLoad = new List<string>(description.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                        foreach (string gs in gSave)
                        {
                            Match mgs = null;
                            if (!(mgs = rxgpiun.Match(gs)).Success) continue;
                            bool ex = false;
                            for (int ii = 0; ii < gLoad.Count; ii++)
                            {
                                string glt = gLoad[ii];
                                Match mgl = null;
                                if (((mgl = rxgpiun.Match(glt)).Success) && (mgs.Groups["crc"].Value == mgl.Groups["crc"].Value))
                                {
                                    ex = true;
                                    gLoad[ii] = gs;
                                    ii = gLoad.Count;
                                };
                            }
                            if (!ex)
                                gLoad.Add(gs);
                        };
                        description = "";
                        foreach (string gl in gLoad)
                            description += gl + "\r\n";
                    };
                    try
                    {
                        XmlNode dnd = pmk.SelectSingleNode("description");
                        if (dnd != null) dnd.ParentNode.RemoveChild(dnd);
                        dnd = f.kmlDoc.CreateElement("description");
                        pmk.AppendChild(dnd);
                        dnd.AppendChild(f.kmlDoc.CreateTextNode(description));
                    }
                    catch { };
                    f.SaveKML();
                    kmzLayers.Refresh();
                };
            };
            grf.Dispose();
        }

        private void getLayerCRCGPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItems[0];
            LayerNameEditor4GPICRC(l.file, l.id);
        }

        private void compareLayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedItems.Count == 0) return;
            if (kmzLayers.Items.Count < 2) return;

            KMLayer layA = (KMLayer)kmzLayers.SelectedItems[0];
            KMLayer layB = null;

            if (kmzLayers.Items.Count > 2)
            {
                List<KMLayer> lays = new List<KMLayer>();
                List<string> layers = new List<string>();
                for (int i = 0; i < kmzLayers.Items.Count; i++)
                {
                    if (i == kmzLayers.SelectedIndices[0]) continue;
                    KMLayer l = (KMLayer)kmzLayers.Items[i];
                    lays.Add(l);
                    layers.Add(l.ToString());
                };

                int new_ind = 0;
                if (System.Windows.Forms.InputBox.Show("Compare Layers", "Select second layer to comare:", layers.ToArray(), ref new_ind) != DialogResult.OK) return;
                layB = lays[new_ind];
            }
            else
                layB = (KMLayer)kmzLayers.Items[kmzLayers.SelectedIndices[0] == 0 ? 1 : 0];

            waitBox.Show("Compare Layers", "Wait...");
            XmlNodeList nla = layA.file.kmlDoc.SelectNodes("kml/Document/Folder")[layA.id].SelectNodes("Placemark");
            XmlNodeList nlb = layB.file.kmlDoc.SelectNodes("kml/Document/Folder")[layB.id].SelectNodes("Placemark");
            List<XmlNode> a_not_in_b = new List<XmlNode>();
            List<XmlNode> b_not_in_a = new List<XmlNode>();
            List<XmlNode> a_and_b = new List<XmlNode>();
            List<XmlNode> a_diff_b = new List<XmlNode>();
            List<int> b_skip = new List<int>();
            for (int a = 0; a < nla.Count;a++ )
            {
                bool isSame = false;
                XmlNode nna = nla[a].Clone();
                if (nna.SelectSingleNode("styleUrl") != null) nna.RemoveChild(nna.SelectSingleNode("styleUrl"));
                XmlNode nnad = nna.Clone();
                if (nnad.SelectSingleNode("description") != null) nnad.RemoveChild(nnad.SelectSingleNode("description"));
                uint crcA = CRCbyPlacemarkIcon(nla[a], layA);
                for (int b = 0; b < nlb.Count; b++)
                {
                    XmlNode nnb = nlb[b].Clone();
                    if (nnb.SelectSingleNode("styleUrl") != null) nnb.RemoveChild(nnb.SelectSingleNode("styleUrl"));
                    XmlNode nnbd = nnb.Clone();
                    if (nnbd.SelectSingleNode("description") != null) nnbd.RemoveChild(nnbd.SelectSingleNode("description"));
                    uint crcB = CRCbyPlacemarkIcon(nlb[b], layB);
                    if (nna.InnerXml == nnb.InnerXml)
                    {
                        isSame = true;
                        b_skip.Add(b);
                        if (crcA == crcB)
                            a_and_b.Add(nla[a]);
                        else
                            a_diff_b.Add(nla[a]);
                    }
                    else if(nnad.InnerXml == nnbd.InnerXml)
                    {
                        isSame = true;
                        b_skip.Add(b);
                        XmlNode addn = nla[a].Clone();
                        if (addn.SelectSingleNode("description") == null)
                        {
                            XmlNode xd = layA.file.kmlDoc.CreateElement("description");
                            addn.AppendChild(xd);
                            xd.AppendChild(layA.file.kmlDoc.CreateTextNode(nlb[b].SelectSingleNode("description").ChildNodes[0].Value));
                        }
                        else if (nlb[b].SelectSingleNode("description") != null)
                            addn.SelectSingleNode("description").ChildNodes[0].Value += "\r\n---------------\r\n" + nlb[b].SelectSingleNode("description").ChildNodes[0].Value;
                        a_diff_b.Add(addn);
                    };
                };
                if (!isSame)
                    a_not_in_b.Add(nla[a]);
            };
            for (int b = 0; b < nlb.Count; b++)
            {
                if (b_skip.IndexOf(b) >= 0) continue;
                bool isSame = false;
                XmlNode nnb = nlb[b].Clone();
                if (nnb.SelectSingleNode("styleUrl") != null) nnb.RemoveChild(nnb.SelectSingleNode("styleUrl"));
                XmlNode nnbd = nnb.Clone();
                if (nnbd.SelectSingleNode("description") != null) nnbd.RemoveChild(nnbd.SelectSingleNode("description"));
                uint crcB = CRCbyPlacemarkIcon(nlb[b], layB);
                for (int a = 0; a < nla.Count; a++)
                {
                    XmlNode nna = nla[a].Clone();
                    if (nna.SelectSingleNode("styleUrl") != null) nna.RemoveChild(nna.SelectSingleNode("styleUrl"));
                    XmlNode nnad = nna.Clone();
                    if (nnad.SelectSingleNode("description") != null) nnad.RemoveChild(nnad.SelectSingleNode("description"));
                    uint crcA = CRCbyPlacemarkIcon(nla[a], layA);
                    if (nna.InnerXml == nnb.InnerXml)
                    {
                        isSame = true;
                        if (crcA == crcB)
                            a_and_b.Add(nla[a]);
                        else
                            a_diff_b.Add(nla[a]);
                    }
                    else if (nnad.InnerXml == nnbd.InnerXml)
                    {
                        isSame = true;
                        XmlNode addn = nla[a].Clone();
                        if (addn.SelectSingleNode("description") == null)
                        {
                            XmlNode xd = layA.file.kmlDoc.CreateElement("description");
                            addn.AppendChild(xd);
                            xd.AppendChild(layA.file.kmlDoc.CreateTextNode(nlb[b].SelectSingleNode("description").ChildNodes[0].Value));
                        }
                        else if (nlb[b].SelectSingleNode("description") != null)
                            addn.SelectSingleNode("description").ChildNodes[0].Value += "\r\n---------------\r\n" + nlb[b].SelectSingleNode("description").ChildNodes[0].Value;
                        a_diff_b.Add(addn);
                    };
                };
                if (!isSame)
                    b_not_in_a.Add(nlb[b]);
            };
            waitBox.Hide();
            
            GMLayRenamerForm grf = new GMLayRenamerForm();
            grf.Text = "Compare two layers";
            grf.label1.Text = "Comparation completed, check layers to save:";
            grf.layers.CheckBoxes = true;
            grf.button1.Text = "Save";
            grf.layers.Items.Add("-- same data -- (" + a_and_b.Count.ToString() + " placemarks)");            
            grf.layers.Items.Add("-- difference -- (" + a_diff_b.Count.ToString() + " placemarks)");
            grf.layers.Items.Add("-- in -- " + layA.name + " (" + a_not_in_b.Count.ToString() + " placemarks)");
            grf.layers.Items.Add("-- in -- " + layB.name + " (" + b_not_in_a.Count.ToString() + " placemarks)");
            grf.layers.Items[0].Checked = a_and_b.Count > 0;
            grf.layers.Items[1].Checked = a_diff_b.Count > 0;
            grf.layers.Items[2].Checked = a_not_in_b.Count > 0;
            grf.layers.Items[3].Checked = b_not_in_a.Count > 0;
            if (grf.ShowDialog() != DialogResult.OK)
            {
                grf.Dispose();
                return;
            };
            if (!grf.layers.Items[0].Checked) a_and_b.Clear();
            if (!grf.layers.Items[1].Checked) a_diff_b.Clear();
            if (!grf.layers.Items[2].Checked) a_not_in_b.Clear();
            if (!grf.layers.Items[3].Checked) b_not_in_a.Clear();
            grf.Dispose();

            if (a_and_b.Count > 0)
            {
                KMFile f = layA.file;
                XmlNode xn = f.kmlDoc.SelectSingleNode("kml/Document");
                xn = xn.AppendChild(f.kmlDoc.CreateElement("Folder"));
                xn = xn.AppendChild(f.kmlDoc.CreateElement("name"));
                xn.AppendChild(f.kmlDoc.CreateTextNode("-- same data --"));
                string outerXML = "";
                foreach (XmlNode an in a_and_b)
                    outerXML += an.OuterXml;
                xn.ParentNode.InnerXml += outerXML;
            };
            if (a_diff_b.Count > 0)
            {
                KMFile f = layA.file;
                XmlNode xn = f.kmlDoc.SelectSingleNode("kml/Document");
                xn = xn.AppendChild(f.kmlDoc.CreateElement("Folder"));
                xn = xn.AppendChild(f.kmlDoc.CreateElement("name"));
                xn.AppendChild(f.kmlDoc.CreateTextNode("-- difference --"));
                string outerXML = "";
                foreach (XmlNode an in a_diff_b)
                    outerXML += an.OuterXml;
                xn.ParentNode.InnerXml += outerXML;
            };
            if (a_not_in_b.Count > 0)
            {
                KMFile f = layA.file;
                XmlNode xn = f.kmlDoc.SelectSingleNode("kml/Document");
                xn = xn.AppendChild(f.kmlDoc.CreateElement("Folder"));
                xn = xn.AppendChild(f.kmlDoc.CreateElement("name"));
                xn.AppendChild(f.kmlDoc.CreateTextNode("-- in -- " + layA.name + " --"));
                string outerXML = "";
                foreach (XmlNode an in a_not_in_b)
                    outerXML += an.OuterXml;
                xn.ParentNode.InnerXml += outerXML;
            };
            if (b_not_in_a.Count > 0)
            {
                KMFile f = layB.file;
                XmlNode xn = f.kmlDoc.SelectSingleNode("kml/Document");
                xn = xn.AppendChild(f.kmlDoc.CreateElement("Folder"));
                xn = xn.AppendChild(f.kmlDoc.CreateElement("name"));
                xn.AppendChild(f.kmlDoc.CreateTextNode("-- in -- " + layB.name + " --"));
                string outerXML = "";
                foreach (XmlNode an in b_not_in_a)
                    outerXML += an.OuterXml;
                xn.ParentNode.InnerXml += outerXML;
            };

            layA.file.SaveKML();
            layB.file.SaveKML();
            
            waitBox.Show("Reloading", "Wait, reloading file layers...");
            layA.file.LoadKML(true);
            layB.file.LoadKML(true);
            ReloadListboxLayers(true);
            waitBox.Hide();
        }

        private uint CRCbyPlacemarkIcon(XmlNode placemark, KMLayer kml)
        {
            if (placemark == null) return 0;
            if (placemark.Name != "Placemark") return 0;
            if (placemark.SelectSingleNode("Point") == null) return 0;
            XmlNode nsm = placemark.SelectSingleNode("styleUrl");
            if (nsm == null) return 0;
            string stname = nsm.ChildNodes[0].Value;
            if (stname.IndexOf("#") == 0) stname = stname.Remove(0, 1);
            string file = "";
            XmlNode sn; sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + stname + "']/IconStyle/Icon/href");
            if (sn != null)
                file = sn.InnerText;
            sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/StyleMap[@id='" + stname + "']/Pair/styleUrl");
            if (sn != null)
            {
                sn = kml.file.kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + sn.InnerText.Substring(1) + "']/IconStyle/Icon/href");
                if (sn != null)
                    file = sn.InnerText;
            };
            if (!String.IsNullOrEmpty(file))
            {
                CRC32 crc = new CRC32();
                try
                {
                    return crc.CRC32Num(kml.file.tmp_file_dir + file);
                }
                catch { };
            };
                return 1;
            return 0;
        }

        private void checkLayersWithSameNameButLessObjectsCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count < 2) return;
            for (int a = 0; a < kmzLayers.Items.Count - 1; a++)
            {
                KMLayer la = (KMLayer)kmzLayers.Items[a];
                for (int b = a + 1; b < kmzLayers.Items.Count; b++)
                {
                    KMLayer lb = (KMLayer)kmzLayers.Items[b];
                    if (la.name == lb.name)
                    {
                        kmzLayers.SetItemChecked(a, la.placemarks < lb.placemarks);
                        kmzLayers.SetItemChecked(b, la.placemarks >= lb.placemarks);
                    };
                }
            };
        }

        private void checkLayersWithSameNameButMoreObjectsCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count < 2) return;
            for (int a = 0; a < kmzLayers.Items.Count - 1; a++)
            {
                KMLayer la = (KMLayer)kmzLayers.Items[a];
                for (int b = a + 1; b < kmzLayers.Items.Count; b++)
                {
                    KMLayer lb = (KMLayer)kmzLayers.Items[b];
                    if (la.name == lb.name)
                    {
                        kmzLayers.SetItemChecked(a, la.placemarks > lb.placemarks);
                        kmzLayers.SetItemChecked(b, la.placemarks <= lb.placemarks);
                    };
                }
            };
        }

        private void unckeckLayersWithSameNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count < 2) return;
            for (int a = 0; a < kmzLayers.Items.Count - 1; a++)
            {
                KMLayer la = (KMLayer)kmzLayers.Items[a];
                for (int b = a + 1; b < kmzLayers.Items.Count; b++)
                {
                    KMLayer lb = (KMLayer)kmzLayers.Items[b];
                    if (la.name == lb.name)
                    {
                        kmzLayers.SetItemChecked(a, false);
                        kmzLayers.SetItemChecked(b, false);
                    };
                }
            };
        }

        private void checkLayersWithSameNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count < 2) return;
            for (int a = 0; a < kmzLayers.Items.Count - 1; a++)
            {
                KMLayer la = (KMLayer)kmzLayers.Items[a];
                for (int b = a + 1; b < kmzLayers.Items.Count; b++)
                {
                    KMLayer lb = (KMLayer)kmzLayers.Items[b];
                    if (la.name == lb.name)
                    {
                        kmzLayers.SetItemChecked(a, true);
                        kmzLayers.SetItemChecked(b, true);
                    };
                }
            };
        }

        private void checkLayersWithSameNamesButToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count < 2) return;
            if (kmzFiles.Items.Count < 2) return;

            List<string> fns = new List<string>();
            for (int i = 0; i < kmzFiles.Items.Count; i++)
            {
                KMFile f = (KMFile)kmzFiles.Items[i];
                fns.Add(f.ToString());
            };

            int ind = 0;
            if (System.Windows.Forms.InputBox.Show("Uncheck Layers", "Select in wich file layers will not be unchecked (keep state):", fns.ToArray(), ref ind) != DialogResult.OK) return;
            string fname = ((KMFile)kmzFiles.Items[ind]).ToString();
            
            for (int a = 0; a < kmzLayers.Items.Count - 1; a++)
            {
                KMLayer la = (KMLayer)kmzLayers.Items[a];
                for (int b = a + 1; b < kmzLayers.Items.Count; b++)
                {
                    KMLayer lb = (KMLayer)kmzLayers.Items[b];
                    if (la.name == lb.name)
                    {
                        if(la.file.ToString() != fname)
                            kmzLayers.SetItemChecked(a, false);
                        if (lb.file.ToString() != fname)
                            kmzLayers.SetItemChecked(b, false);
                    };
                }
            };
        }

        private void checkLayersWithSameNamesOnlyInFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count < 2) return;
            if (kmzFiles.Items.Count < 2) return;

            List<string> fns = new List<string>();
            for (int i = 0; i < kmzFiles.Items.Count; i++)
            {
                KMFile f = (KMFile)kmzFiles.Items[i];
                fns.Add(f.ToString());
            };

            int ind = 0;
            if (System.Windows.Forms.InputBox.Show("Check Layers", "Select in wich file layers will be checked (check in):", fns.ToArray(), ref ind) != DialogResult.OK) return;
            string fname = ((KMFile)kmzFiles.Items[ind]).ToString();

            for (int a = 0; a < kmzLayers.Items.Count - 1; a++)
            {
                KMLayer la = (KMLayer)kmzLayers.Items[a];
                for (int b = a + 1; b < kmzLayers.Items.Count; b++)
                {
                    KMLayer lb = (KMLayer)kmzLayers.Items[b];
                    if (la.name == lb.name)
                    {
                        if (la.file.ToString() == fname)
                            kmzLayers.SetItemChecked(a, true);
                        if (lb.file.ToString() == fname)
                            kmzLayers.SetItemChecked(b, true);
                    };
                }
            };
        }

        private void checkLayersWithSameNamesButNotInFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count < 2) return;
            if (kmzFiles.Items.Count < 2) return;

            List<string> fns = new List<string>();
            for (int i = 0; i < kmzFiles.Items.Count; i++)
            {
                KMFile f = (KMFile)kmzFiles.Items[i];
                fns.Add(f.ToString());
            };

            int ind = 0;
            if (System.Windows.Forms.InputBox.Show("Check Layers", "Select except wich file layers will be checked (skip in):", fns.ToArray(), ref ind) != DialogResult.OK) return;
            string fname = ((KMFile)kmzFiles.Items[ind]).ToString();

            for (int a = 0; a < kmzLayers.Items.Count - 1; a++)
            {
                KMLayer la = (KMLayer)kmzLayers.Items[a];
                for (int b = a + 1; b < kmzLayers.Items.Count; b++)
                {
                    KMLayer lb = (KMLayer)kmzLayers.Items[b];
                    if (la.name == lb.name)
                    {
                        if (la.file.ToString() != fname)
                            kmzLayers.SetItemChecked(a, true);
                        if (lb.file.ToString() != fname)
                            kmzLayers.SetItemChecked(b, true);
                    };
                }
            };
        }

        private void uncheckLayersWithSameNameOnlyInFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.Items.Count < 2) return;
            if (kmzFiles.Items.Count < 2) return;

            List<string> fns = new List<string>();
            for (int i = 0; i < kmzFiles.Items.Count; i++)
            {
                KMFile f = (KMFile)kmzFiles.Items[i];
                fns.Add(f.ToString());
            };

            int ind = 0;
            if (System.Windows.Forms.InputBox.Show("Uncheck Layers", "Select in wich file layers will be unchecked:", fns.ToArray(), ref ind) != DialogResult.OK) return;
            string fname = ((KMFile)kmzFiles.Items[ind]).ToString();

            for (int a = 0; a < kmzLayers.Items.Count - 1; a++)
            {
                KMLayer la = (KMLayer)kmzLayers.Items[a];
                for (int b = a + 1; b < kmzLayers.Items.Count; b++)
                {
                    KMLayer lb = (KMLayer)kmzLayers.Items[b];
                    if (la.name == lb.name)
                    {
                        if (la.file.ToString() == fname)
                            kmzLayers.SetItemChecked(a, false);
                        if (lb.file.ToString() == fname)
                            kmzLayers.SetItemChecked(b, false);
                    };
                }
            };
        }

        private void importFromDBFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".dbf";
            ofd.Filter = "DBF files (*.dbf)|*.dbf";
            if (ofd.ShowDialog() == DialogResult.OK)
                ImportFromDBF(ofd.FileName);
            ofd.Dispose();
        }

        private void importFromShapeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".shp";
            ofd.Filter = "Shape files (*.shp)|*.shp";
            if (ofd.ShowDialog() == DialogResult.OK)
                ImportFromSHP(ofd.FileName);
            ofd.Dispose();
        }

        private object kmzl_temp;
        private int kmzl_trackingItem;
        private bool kmzl_checked;

        private void kmzLayers_MouseUp(object sender, MouseEventArgs e)
        {            
            if (kmzLayers.SelectedIndex < 0) return;
            if (kmzl_temp != null)
            {                
                int tempInd = kmzLayers.SelectedIndex;
                if ((tempInd >= 0) && (kmzl_trackingItem != tempInd))
                {
                    kmzLayers.Items.RemoveAt(kmzl_trackingItem);
                    kmzLayers.Items.Insert(tempInd, kmzl_temp);                    
                    kmzLayers.SelectedIndex = tempInd;
                    kmzLayers.SetItemChecked(tempInd, kmzl_checked);
                };
                kmzLayers.Cursor = Cursors.Default;
                kmzl_temp = null;
                kmzLayers.MovingItem = null;                
            };
        }

        private void kmzLayers_MouseDown(object sender, MouseEventArgs e)
        {
            if (kmzLayers.SelectedIndex < 0) return;
            
            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {                
                kmzLayers.Cursor = Cursors.Hand;
                kmzl_trackingItem = kmzLayers.SelectedIndex;
                if (kmzl_trackingItem >= 0)
                {
                    SortByMouse();
                    kmzl_temp = kmzLayers.Items[kmzl_trackingItem];
                    kmzl_checked = kmzLayers.GetItemChecked(kmzl_trackingItem);
                    kmzLayers.MovingItem = kmzl_temp;
                };
            };
        }

        private void pREFERENCESToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.ShowChangeDialog();
            LoadPreferences();
        }

        private void c2DGPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (kmzLayers.CheckedItems.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Export to GPI";
            sfd.Filter = "Garmin Points of Interests File (*.gpi)|*.gpi";
            sfd.DefaultExt = ".gpi";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                string zdir = Save2KMZ(sfd.FileName, true);
                ReloadXMLOnly_NoUpdateLayers();
                KMFile kmf = KMFile.FromZDir(zdir);
                Save2GPIInt(sfd.FileName, kmf);
            };
            sfd.Dispose(); 
        }

        private void saveBtnGPIN_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndices.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Select GPI file";
            sfd.Filter = "Garmin Points of Interests File (*.gpi)|*.gpi";
            sfd.DefaultExt = ".gpi";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                log.Text = "";
                Save2GPIInt(sfd.FileName, (KMFile)kmzFiles.SelectedItems[0]);
            };
            sfd.Dispose();
        }

        private void gPIAlertsHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            bool ok = false;
            string fName = Properties["gpiwriter_alert_help_file"];
            if (!ok) try { System.Diagnostics.Process.Start("notepad++", fName); ok = true; } catch { };
            if (!ok) try { System.Diagnostics.Process.Start(CurrentDirectory() + @"AkelPad.exe", fName); ok = true; } catch { };
            if (!ok) try { System.Diagnostics.Process.Start("notepad", fName); } catch { };
        }

        private void sourcePathToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) 
            {
                vimd.Enabled = vsod.Enabled = vmed.Enabled = false;
                return;
            };
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            f.GetSubISMDirs();
            vimd.Enabled = f.HasImagesDir;
            vsod.Enabled = f.HasSoundsDir;
            vmed.Enabled = f.HasMediaDir;
        }

        private void vimd_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string path = Path.Combine(f.tmp_file_dir, @"images\");
            try { System.Diagnostics.Process.Start("explorer.exe", path); } catch { };
        }

        private void vsod_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string path = Path.Combine(f.tmp_file_dir, @"sounds\");
            try { System.Diagnostics.Process.Start("explorer.exe", path); }catch { };
        }

        private void vmed_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            string path = Path.Combine(f.tmp_file_dir, @"media\");
            try { System.Diagnostics.Process.Start("explorer.exe", path); }catch { };
        }
    }

    public class FilesListBox : CheckedListBox
    {
        public FilesListBox() : base() { }

        public int SelectedByLayer = -1;

        private int[] GetMaxNameFileLength(Font font, Graphics g)
        {
            int[] res = new int[] { 0, 0 };
            for (int i = 0; i < this.Items.Count; i++)
            {
                int mfl = GetFileLength(i, font, g);
                if (mfl > res[0]) res[0] = mfl;
                int mnl = GetNameLength(i, font, g);
                if (mnl > res[1]) res[1] = mnl;                
            };
            return res;
        }

        private int GetNameLength(int index, Font font, Graphics g)
        {
            if (String.IsNullOrEmpty(((KMFile)this.Items[index]).kmldocName))
                return 2;
            else
                return (int)g.MeasureString("["+((KMFile)this.Items[index]).kmldocName+"]", font, 0, StringFormat.GenericDefault).Width + 5;
        }

        private int GetFileLength(int index, Font font, Graphics g)
        {
            if (String.IsNullOrEmpty(((KMFile)this.Items[index]).src_file_nme))
                return 2;
            else
                return (int)g.MeasureString(((KMFile)this.Items[index]).src_file_nme, font, 0, StringFormat.GenericDefault).Width + 5;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Size checkSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
            int dx = (e.Bounds.Height - checkSize.Width) / 2;
            int[] mnfl = GetMaxNameFileLength(e.Font, e.Graphics);

            e.DrawBackground();

            if (e.Index >= 0)
            {
                try
                {
                    bool isChecked = GetItemChecked(e.Index);
                    bool isSelected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);
                    KMFile kmf = (KMFile)this.Items[e.Index];

                    if (isChecked)
                    {
                        if (SelectedByLayer == e.Index)
                            e.Graphics.FillRectangle(isSelected ? Brushes.Green : new SolidBrush(Color.FromArgb(180, 255, 180)), e.Bounds);
                        e.Graphics.DrawString("v", new Font(e.Font, FontStyle.Bold), isSelected ? SystemBrushes.HighlightText : SystemBrushes.WindowText, new PointF(dx + 1, e.Bounds.Top));
                    }
                    else
                        e.Graphics.FillRectangle(isSelected ? Brushes.Maroon : Brushes.Yellow, e.Bounds);

                    Pen myp = new Pen(new SolidBrush(Color.FromArgb(230, 230, 250)));
                    myp.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawLine(myp, new Point(e.Bounds.Left, e.Bounds.Bottom - 1), new Point(e.Bounds.Right, e.Bounds.Bottom - 1));
                    int offset = e.Bounds.Left + e.Font.Height + 2;
                    e.Graphics.DrawLine(myp, new Point(offset, e.Bounds.Top), new Point(offset, e.Bounds.Bottom - 1));
                    offset += 5;

                    int txtwi = 0;
                    e.Graphics.DrawString(kmf.src_file_nme, e.Font, isSelected ? SystemBrushes.HighlightText : (SelectedByLayer == e.Index ? Brushes.Maroon : SystemBrushes.WindowText), new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericDefault);
                    txtwi += GetFileLength(e.Index, e.Font, e.Graphics);
                    if (txtwi < mnfl[0]) txtwi = mnfl[0];

                    e.Graphics.DrawLine(myp, new Point(offset + txtwi, e.Bounds.Top), new Point(offset + txtwi, e.Bounds.Bottom - 1));
                    offset += 4;                    

                    e.Graphics.DrawString((kmf.kmldocName != String.Empty ? "[" + kmf.kmldocName + "]" : ""), e.Font, isSelected ? SystemBrushes.HighlightText : Brushes.Blue, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericDefault);
                    txtwi += GetNameLength(e.Index, e.Font, e.Graphics);
                    if (txtwi < (mnfl[0] + mnfl[1])) txtwi = (mnfl[0] + mnfl[1]);

                    e.Graphics.DrawLine(myp, new Point(offset + txtwi, e.Bounds.Top), new Point(offset + txtwi, e.Bounds.Bottom - 1));
                    offset += 4;

                    if (kmf.HasImagesDir)
                    {
                        string optS = "I";
                        e.Graphics.DrawString(optS, e.Font, isSelected ? SystemBrushes.HighlightText : Brushes.DeepSkyBlue, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                        txtwi += (int)e.Graphics.MeasureString(optS, e.Font, 0, StringFormat.GenericTypographic).Width;
                    };
                    if (kmf.HasSoundsDir)
                    {
                        string optS = "S";
                        e.Graphics.DrawString(optS, e.Font, isSelected ? SystemBrushes.HighlightText : Brushes.Violet, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                        txtwi += (int)e.Graphics.MeasureString(optS, e.Font, 0, StringFormat.GenericTypographic).Width;
                    };
                    if (kmf.HasMediaDir)
                    {
                        string optS = "M";
                        e.Graphics.DrawString(optS, e.Font, isSelected ? SystemBrushes.HighlightText : Brushes.DarkOrange, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                        txtwi += (int)e.Graphics.MeasureString(optS, e.Font, 0, StringFormat.GenericTypographic).Width;
                    };
                    

                    string errText = (kmf.parseError ? " - BAD!" : "");
                    e.Graphics.DrawString(errText, e.Font, isSelected ? SystemBrushes.HighlightText : Brushes.Red, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                    txtwi += (int)e.Graphics.MeasureString(errText,e.Font,0,StringFormat.GenericTypographic).Width;                    

                    if (SelectedByLayer == e.Index)
                    {
                        string sSel = " <- layer in this file";
                        e.Graphics.DrawString(sSel, e.Font, isSelected ? Brushes.Silver : Brushes.Green, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                        txtwi += (int)e.Graphics.MeasureString(sSel, e.Font, 0, StringFormat.GenericTypographic).Width;
                    };
                }
                catch 
                {                    
                };
            };
        }
    }

    public class LayersListBox : CheckedListBox
    {
        public LayersListBox() : base() { }

        public byte SortBy = 0;

        private int[] GetMaxNameFileLength(Font font, Graphics g)
        {
            int[] res = new int[] { 0, 0 };
            for (int i = 0; i < this.Items.Count; i++)
            {
                int mnl = GetNameLength(i, font, g);
                if (mnl > res[0]) res[0] = mnl;
                int mfl = GetFileLength(i, font, g);
                if (mfl > res[1]) res[1] = mfl;
            };
            return res;
        }

        private int GetNameLength(int index, Font font, Graphics g)
        {
            if (String.IsNullOrEmpty(((KMLayer)this.Items[index]).name))
                return 2;
            else
                return (int)g.MeasureString(((KMLayer)this.Items[index]).name, font, 0, StringFormat.GenericDefault).Width + 5;
        }

        private int GetFileLength(int index, Font font, Graphics g)
        {
            if (String.IsNullOrEmpty(((KMLayer)this.Items[index]).file.src_file_nme))
                return 2;
            else
                return (int)g.MeasureString(((KMLayer)this.Items[index]).file.src_file_nme, font, 0, StringFormat.GenericDefault).Width + 5;
        }

        protected override void Sort()
        {
            if (this.Items.Count > 1)
            {
                bool swapped;
                do
                {
                    int counter = this.Items.Count - 1;
                    swapped = false;

                    while (counter > 0)
                    {
                        bool swap = false;
                        if (this.SortBy == 2)
                        {
                            KMLayer Current = (KMLayer)this.Items[counter];
                            KMLayer Previous = (KMLayer)this.Items[counter - 1];
                            if (Previous.placemarks.CompareTo(Current.placemarks) == -1)
                                swap = true;
                        }
                        else if (this.SortBy == 4)
                        {
                            KMLayer Current = (KMLayer)this.Items[counter];
                            KMLayer Previous = (KMLayer)this.Items[counter - 1];
                            if (Previous.points.CompareTo(Current.points) == -1)
                                swap = true;
                        }
                        else if (this.SortBy == 5)
                        {
                            KMLayer Current = (KMLayer)this.Items[counter];
                            KMLayer Previous = (KMLayer)this.Items[counter - 1];
                            if (Previous.lines.CompareTo(Current.lines) == -1)
                                swap = true;
                        }
                        else if (this.SortBy == 6)
                        {
                            KMLayer Current = (KMLayer)this.Items[counter];
                            KMLayer Previous = (KMLayer)this.Items[counter - 1];
                            if (Previous.areas.CompareTo(Current.areas) == -1)
                                swap = true;
                        } 
                        else if (this.SortBy == 3)
                        {
                            KMLayer Current = (KMLayer)this.Items[counter];
                            KMLayer Previous = (KMLayer)this.Items[counter - 1];
                            if (Current.ischeck && (!Previous.ischeck))
                                swap = true;
                        }
                        else
                        {
                            if (this.Items[counter].ToString().CompareTo(this.Items[counter - 1].ToString()) == -1)
                                swap = true;
                        };

                        if (swap)
                        {
                            object temp = Items[counter];
                            this.Items[counter] = this.Items[counter - 1];
                            this.Items[counter - 1] = temp;
                            swapped = true;
                        };
                        counter -= 1;
                    };
                }
                while (swapped);
            };

            if (this.SortBy == 3)
            {
                for (int i = 0; i < this.Items.Count; i++)
                    this.SetItemChecked(i, ((KMLayer)this.Items[i]).ischeck);
            };
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Size checkSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
            int dx = (e.Bounds.Height - checkSize.Width) / 2;
            int[] mnfl = GetMaxNameFileLength(e.Font, e.Graphics);
            int n_W_f = mnfl[0] + (int)e.Graphics.MeasureString("000 in ", e.Font, 0, StringFormat.GenericTypographic).Width + mnfl[1] + 5;

            e.DrawBackground();

            if (e.Index >= 0)
            {
                try
                {
                    bool isChecked = GetItemChecked(e.Index);
                    bool isSelected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);
                    KMLayer kml = (KMLayer)this.Items[e.Index];                    

                    if (isChecked)
                    {
                        if (kml.placemarks == 0)
                            e.Graphics.FillRectangle(isSelected ? Brushes.Maroon : Brushes.LightPink, e.Bounds);                            
                        e.Graphics.DrawString("v", new Font(e.Font, FontStyle.Bold), isSelected ? SystemBrushes.HighlightText : SystemBrushes.WindowText, new PointF(dx + 1, e.Bounds.Top));
                    }
                    else
                    {
                        if (isSelected)
                            e.Graphics.FillRectangle(kml.placemarks > 0 ? Brushes.DarkSlateBlue : Brushes.RosyBrown, e.Bounds);
                        else
                            e.Graphics.FillRectangle(kml.placemarks > 0 ? Brushes.Yellow : Brushes.LightGoldenrodYellow, e.Bounds);                            
                    };

                    if ((MovingItem != null) && isSelected)
                    {
                        e.Graphics.FillRectangle(Brushes.Fuchsia, e.Bounds);
                        if(isChecked) e.Graphics.DrawString("v", new Font(e.Font, FontStyle.Bold), isSelected ? SystemBrushes.HighlightText : SystemBrushes.WindowText, new PointF(dx + 1, e.Bounds.Top));
                        kml = (KMLayer)MovingItem;
                    };

                    Pen myp = new Pen(new SolidBrush(Color.FromArgb(230,230,250)));
                    myp.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawLine(myp, new Point(e.Bounds.Left, e.Bounds.Bottom - 1), new Point(e.Bounds.Right, e.Bounds.Bottom - 1));
                    int offset = e.Bounds.Left + e.Font.Height + 2;
                    e.Graphics.DrawLine(myp, new Point(offset, e.Bounds.Top), new Point(offset, e.Bounds.Bottom - 1));
                    offset += 5;

                    int txtwi = 0;

                    // DRAW LAYER NAME
                    e.Graphics.DrawString(kml.name, e.Font, isSelected ? SystemBrushes.HighlightText : Brushes.Black, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericDefault);
                    txtwi += GetNameLength(e.Index, e.Font, e.Graphics);
                    if (txtwi < mnfl[0]) txtwi = mnfl[0];                    

                    // DRAW VERT
                    e.Graphics.DrawLine(myp, new Point(offset + txtwi, e.Bounds.Top), new Point(offset + txtwi, e.Bounds.Bottom - 1));
                    offset += 4;

                    // DRAW LAYER NO
                    string layNo = kml.id.ToString("000") + " in";
                    e.Graphics.DrawString(layNo, e.Font, isSelected ? SystemBrushes.HighlightText : Brushes.Silver, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                    txtwi += (int)e.Graphics.MeasureString(layNo + ".", e.Font, 0, StringFormat.GenericTypographic).Width;

                    // DRAW FILE NAME
                    e.Graphics.DrawString(kml.file.src_file_nme, e.Font, isSelected ? SystemBrushes.HighlightText : Brushes.Maroon, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericDefault);
                    txtwi += GetFileLength(e.Index, e.Font, e.Graphics);
                    if (txtwi < n_W_f) txtwi = n_W_f;

                    // DRAW VERT
                    e.Graphics.DrawLine(myp, new Point(offset + txtwi, e.Bounds.Top), new Point(offset + txtwi, e.Bounds.Bottom - 1));
                    offset += 4;

                    string copyText = "";
                    try
                    {
                        if (kml.ATB >= 0)
                            copyText = KMLayer.digits.Substring(kml.ATB % KMLayer.digits.Length, 1);
                    }
                    catch { };
                    string descText = kml.hasDesc ? " -D-" : "";
                    // DRAW OBJECTS COUNT
                    string prefCnt = "000000".Remove(0, kml.placemarks.ToString("").Length > 6 ? 0 : kml.placemarks.ToString("").Length);
                    string mainCnt = kml.placemarks.ToString("");
                    string[] after_text = new string[]{
                    "000000".Remove(1, kml.points.ToString("").Length > 6 ? 0 : kml.points.ToString("").Length),
                    kml.points.ToString(""),
                    "000000".Remove(1, kml.lines.ToString("").Length > 6 ? 0 : kml.lines.ToString("").Length),
                    kml.lines.ToString(""),
                    "000000".Remove(1, kml.areas.ToString("").Length > 6 ? 0 : kml.areas.ToString("").Length),
                    kml.areas.ToString(""),
                    copyText,
                    descText
                };
                    e.Graphics.DrawString(prefCnt, e.Font, isSelected ? Brushes.Black : Brushes.Silver, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                    txtwi += (int)e.Graphics.MeasureString(prefCnt, e.Font, 0, StringFormat.GenericTypographic).Width;
                    e.Graphics.DrawString(mainCnt, e.Font, isSelected ? SystemBrushes.HighlightText : (kml.placemarks == 0 ? Brushes.Red : Brushes.Blue), new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                    txtwi += (int)e.Graphics.MeasureString(mainCnt, e.Font, 0, StringFormat.GenericTypographic).Width;
                    offset += 4;
                    e.Graphics.DrawLine(myp, new Point(offset + txtwi, e.Bounds.Top), new Point(offset + txtwi, e.Bounds.Bottom - 1));
                    offset += 3;

                    for (int at = 0; at < 6; at++)
                    {
                        e.Graphics.DrawString(after_text[at], e.Font, isSelected ? ((at % 2 == 1) ? SystemBrushes.HighlightText : Brushes.Black) : ((at % 2 == 1) ? (after_text[at] == "0" ? Brushes.Silver : Brushes.BlueViolet) : Brushes.Silver), new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                        txtwi += (int)e.Graphics.MeasureString(after_text[at], e.Font, 0, StringFormat.GenericTypographic).Width;
                        if (at % 2 == 1)
                        {
                            offset += 4;
                            e.Graphics.DrawLine(myp, new Point(offset + txtwi, e.Bounds.Top), new Point(offset + txtwi, e.Bounds.Bottom - 1));
                            offset += 3;
                        };
                    };
                    for (int at = 6; at < after_text.Length; at++)
                    {
                        if (String.IsNullOrEmpty(after_text[at])) continue;
                        Font ft = e.Font;
                        Brush bt = isSelected ? SystemBrushes.HighlightText : Brushes.Gray;
                        if (at == 6)
                        {
                            ft = new Font(e.Font, FontStyle.Bold);
                            bt = isSelected ? SystemBrushes.HighlightText : Brushes.SlateGray;
                        };
                        if (at == 7)
                            bt = isSelected ? SystemBrushes.HighlightText : Brushes.Goldenrod;
                        e.Graphics.DrawString(after_text[at], ft, bt, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                        txtwi += (int)e.Graphics.MeasureString(after_text[at], ft, 0, StringFormat.GenericTypographic).Width;
                    };

                    // DRAW OBJECTS INFO
                    if (isSelected)
                    {
                        string obji = " <- ttl | points | lines | polygons";
                        e.Graphics.DrawString(obji, e.Font, Brushes.Silver, new Rectangle(offset + txtwi, e.Bounds.Top, e.Bounds.Width - offset - txtwi, e.Bounds.Height), StringFormat.GenericTypographic);
                        txtwi += (int)e.Graphics.MeasureString(obji, e.Font, 0, StringFormat.GenericTypographic).Width;
                    };
                }
                catch
                {
                };
            };
        }

        public object MovingItem = null;
    }

    public class KMFile
    {
        public class CSVTXTFile
        {
            public Encoding enc;
            public string[] skipLinesStartsWith;
            public bool firstLineIsHeader;
            public char columnDelimiter;
            public char floatSeparator;
            public int fNameNumber;
            public int fDescNumber;
            public int fLatNumber;
            public int fLonNumber;
            public int fStyleNumber;

            public CSVTXTFile(Encoding enc, string[] skipLinesStartsWith, bool firstLineIsHeader, char columnDelimiter, char floatSeparator, int fNameNumber, int fDescNumber, int fLatNumber, int fLonNumber, int fStyleNumber)
            {
                this.enc = enc;
                this.skipLinesStartsWith = skipLinesStartsWith;
                this.firstLineIsHeader = firstLineIsHeader;
                this.columnDelimiter = columnDelimiter;
                this.floatSeparator = floatSeparator;
                this.fNameNumber = fNameNumber;
                this.fDescNumber = fDescNumber;
                this.fLatNumber = fLatNumber;
                this.fLonNumber = fLonNumber;
                this.fStyleNumber = fStyleNumber;
            }
        }

        public bool parseError = false;

        public string src_file_pth;
        public string src_file_nme;
        public string src_file_ext;
        public CSVTXTFile src_file_csv;
        public bool src_file_bad = false;
        public string tmp_file_dir = "";

        public XmlDocument kmlDoc;
        public string kmldocName = "";
        public string kmldocDesc = "";        
        public List<KMLayer> kmLayers = new List<KMLayer>();
       
        public bool isCheck = true;

        public bool DrawEvenSizeIsTooSmall = false;
                
        public bool Valid
        {
            get
            {
                if (src_file_bad) return false;
                if (src_file_csv != null) return true;
                return (src_file_ext == ".kmz") || (src_file_ext == ".kml") ||
                    (src_file_ext == ".gpx") || (src_file_ext == ".dat") || (src_file_ext == ".wpt") || (src_file_ext == ".osm")
                    || (src_file_ext == ".db3") || (src_file_ext == ".poi") || (src_file_ext == ".map") || (src_file_ext == ".gdb")
                        || (src_file_ext == ".fit") || (src_file_ext == ".shp") || (src_file_ext == ".dbf") || (src_file_ext == ".gpi");
            }
        }


        private bool[] HasISMDirs = new bool[] { false, false, false };
        public bool HasImagesDir
        {
            get
            {
                return HasISMDirs[0];
            }
        }

        public bool HasSoundsDir
        {
            get
            {
                return HasISMDirs[1];
            }
        }

        public bool HasMediaDir
        {
            get
            {
                return HasISMDirs[2];
            }
        }

        public static bool ValidForDragDropAuto(string filename)
        {
            if (filename == null) return false;
            string src_file_ext = Path.GetExtension(filename).ToLower();
            return (src_file_ext == ".kmz") || (src_file_ext == ".kml") ||
                    (src_file_ext == ".gpx") || (src_file_ext == ".dat") || (src_file_ext == ".wpt")
                    || (src_file_ext == ".osm") || (src_file_ext == ".gdb") || (src_file_ext == ".fit")
                        || (src_file_ext == ".gpi");

        }

        public bool AllowReloadOriginal
        {
            get
            {
                if (src_file_bad) return false;
                if ((src_file_csv != null) && (src_file_nme != "Clipboard_Text_Data.txt")) return true;
                return (src_file_ext == ".kmz") || (src_file_ext == ".kml") ||
                    (src_file_ext == ".gpx") || (src_file_ext == ".dat") || (src_file_ext == ".wpt")
                     || (src_file_ext == ".gdb") || (src_file_ext == ".fit") || (src_file_ext == ".gpi");
            }
        }

        public static KMFile FromZDir(string path)
        {
            KMFile kmf = new KMFile("");
            string filename = path + "doc.kml";
            kmf.src_file_pth = filename;
            kmf.src_file_nme = System.IO.Path.GetFileName(filename);
            kmf.src_file_ext = System.IO.Path.GetExtension(filename).ToLower();
            kmf.tmp_file_dir = path;
            kmf.PrepareNormalKML();
            kmf.LoadKML(true);
            return kmf;
        }

        public KMFile(string filename)
        {
            this.src_file_pth = filename;
            this.src_file_nme = System.IO.Path.GetFileName(filename);
            this.src_file_ext = System.IO.Path.GetExtension(filename).ToLower();
            this.tmp_file_dir = KMZRebuilederForm.TempDirectory() + "IF" + DateTime.UtcNow.Ticks.ToString() + @"\";

            this.CopySrcFileToTempDirAndLoad();
        }

        public static KMFile CreateEmpty()
        {
            return new KMFile();
        }

        public static KMFile CreateEmpty(string name)
        {
            return new KMFile(name, 0);
        }

        public static KMFile CreateEmpty(string name, int layers)
        {
            return new KMFile(name, layers);
        }

        public static KMFile FromCSVTXT(Stream fileStream, Encoding enc, string fileName, string[] skipLinesStartsWith, bool firstLineIsHeader, char columnDelimiter, char floatSeparator, int fName, int fDesc, int fLat, int fLon, int fStyle)
        {
            return new KMFile(fileStream, fileName, new CSVTXTFile(enc, skipLinesStartsWith, firstLineIsHeader, columnDelimiter, floatSeparator, fName, fDesc, fLat, fLon, fStyle));
        }

        private KMFile()
        {
            string efn = "E-" + DateTime.Now.ToString("yyMMdd-HHmmss");

            this.tmp_file_dir = KMZRebuilederForm.TempDirectory() + "IF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            this.src_file_pth = this.tmp_file_dir + "doc.kml";
            this.src_file_nme = System.IO.Path.GetFileName(this.src_file_pth);
            this.src_file_ext = System.IO.Path.GetExtension(this.src_file_pth).ToLower();
            this.src_file_nme = efn + ".kml";

            if (!Directory.Exists(this.tmp_file_dir)) System.IO.Directory.CreateDirectory(this.tmp_file_dir);
            if (!Directory.Exists(this.tmp_file_dir + @"images\")) System.IO.Directory.CreateDirectory(this.tmp_file_dir + @"images\");

            FileStream fs = new FileStream(this.src_file_pth, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>\r\n<Document>\r\n<name>"+efn+"</name>\r\n<createdby>KMZRebuilder XP</createdby>\r\n<Folder>\r\n<name>Empty Layer</name>\r\n</Folder>\r\n</Document>\r\n</kml>\r\n");
            sw.Close();
            fs.Close();            

            this.LoadKML(true);
        }

        private KMFile(string fileName, int layersCount)
        {
            string efn = fileName;

            this.tmp_file_dir = KMZRebuilederForm.TempDirectory() + "IF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            this.src_file_pth = this.tmp_file_dir + "doc.kml";
            this.src_file_nme = System.IO.Path.GetFileName(this.src_file_pth);
            this.src_file_ext = System.IO.Path.GetExtension(this.src_file_pth).ToLower();
            this.src_file_nme = efn + ".kml";

            if (!Directory.Exists(this.tmp_file_dir)) System.IO.Directory.CreateDirectory(this.tmp_file_dir);
            if (!Directory.Exists(this.tmp_file_dir + @"images\")) System.IO.Directory.CreateDirectory(this.tmp_file_dir + @"images\");

            FileStream fs = new FileStream(this.src_file_pth, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>\r\n<Document>\r\n<name>" + efn + "</name>\r\n<createdby>KMZRebuilder XP</createdby>\r\n");
            for (int i = 0; i < layersCount; i++)
                sw.WriteLine("<Folder>{0}</Folder>", i + 1);
            sw.WriteLine("</Document>\r\n</kml>\r\n");
            sw.Close();
            fs.Close();

            this.LoadKML(true);
        }

        private KMFile(Stream filestream, string filename, CSVTXTFile parameters)
        {
            this.src_file_csv = parameters;                        
            this.tmp_file_dir = KMZRebuilederForm.TempDirectory() + "IF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            this.src_file_pth = this.tmp_file_dir + System.IO.Path.GetFileName(filename);
            this.src_file_nme = System.IO.Path.GetFileName(filename);
            this.src_file_ext = System.IO.Path.GetExtension(filename).ToLower();

            if (!Directory.Exists(this.tmp_file_dir)) System.IO.Directory.CreateDirectory(this.tmp_file_dir);
            if (!Directory.Exists(this.tmp_file_dir + @"images\")) System.IO.Directory.CreateDirectory(this.tmp_file_dir + @"images\");

            FileStream fs = new FileStream(this.src_file_pth, FileMode.Create, FileAccess.Write);
            filestream.Position = 0;
            byte[] buff = new byte[ushort.MaxValue];
            int count = 0;
            while ((count = filestream.Read(buff, 0, buff.Length)) > 0)
                fs.Write(buff, 0, count);
            fs.Close();
            filestream.Close();

            CSVTXT2KML(); 
            this.LoadKML(true);
        }

        public static KMFile FromDB3(string db3file, int[] cats, string icons_path)
        {
            return new KMFile(db3file, cats, icons_path);
        }

        public static KMFile FromMapsForgeMapFile(string fileName, MapsForgeFileReader.MapsForgeReader mfr, List<int> poi_tags, List<int> way_tags)
        {
            return new KMFile(fileName, mfr, poi_tags, way_tags);
        }

        public static KMFile FromMapsForgePOIFile(string poifile, List<string[]> cats)
        {
            return new KMFile(poifile, cats);
        }

        private KMFile(string fileName, MapsForgeFileReader.MapsForgeReader mfr, List<int> poi_tags, List<int> way_tags)
        {
            this.tmp_file_dir = KMZRebuilederForm.TempDirectory() + "IF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            this.src_file_pth = this.tmp_file_dir + System.IO.Path.GetFileName(fileName);
            this.src_file_nme = System.IO.Path.GetFileName(fileName);
            this.src_file_ext = System.IO.Path.GetExtension(fileName).ToLower();

            if (!Directory.Exists(this.tmp_file_dir)) System.IO.Directory.CreateDirectory(this.tmp_file_dir);
            if (!Directory.Exists(this.tmp_file_dir + @"images\")) System.IO.Directory.CreateDirectory(this.tmp_file_dir + @"images\");

            From_MapsForgeMapFile(fileName, mfr, poi_tags, way_tags);
            
            this.LoadKML(true);
        }

        private KMFile(string poifile, List<string[]> cats)
        {
            this.tmp_file_dir = KMZRebuilederForm.TempDirectory() + "IF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            this.src_file_pth = this.tmp_file_dir + System.IO.Path.GetFileName(poifile);
            this.src_file_nme = System.IO.Path.GetFileName(poifile);
            this.src_file_ext = System.IO.Path.GetExtension(poifile).ToLower();

            if (!Directory.Exists(this.tmp_file_dir)) System.IO.Directory.CreateDirectory(this.tmp_file_dir);
            if (!Directory.Exists(this.tmp_file_dir + @"images\")) System.IO.Directory.CreateDirectory(this.tmp_file_dir + @"images\");

            From_MapsForgePOIFile(poifile, cats);

            this.LoadKML(true);
        }

        private KMFile(string db3file, int[] cats, string icons_path)
        {
            this.tmp_file_dir = KMZRebuilederForm.TempDirectory() + "IF" + DateTime.UtcNow.Ticks.ToString() + @"\";
            this.src_file_pth = this.tmp_file_dir + System.IO.Path.GetFileName(db3file);
            this.src_file_nme = System.IO.Path.GetFileName(db3file);
            this.src_file_ext = System.IO.Path.GetExtension(db3file).ToLower();

            if (!Directory.Exists(this.tmp_file_dir)) System.IO.Directory.CreateDirectory(this.tmp_file_dir);
            if (!Directory.Exists(this.tmp_file_dir + @"images\")) System.IO.Directory.CreateDirectory(this.tmp_file_dir + @"images\");

            DB32KML(db3file, cats, icons_path);
            
            this.LoadKML(true);
        }

        public override string ToString()
        {
            string res = this.src_file_nme + (this.kmldocName != String.Empty ? " [" + this.kmldocName + "]" : "") + (this.parseError ? " - BAD!" : "");
            res += " - " + this.kmLayers.Count.ToString() + " layers";
            return res;
        }
        
        public void CopySrcFileToTempDirAndLoad()
        {
            if (!this.Valid) return;

            if (!Directory.Exists(this.tmp_file_dir)) System.IO.Directory.CreateDirectory(this.tmp_file_dir);
            if (!Directory.Exists(this.tmp_file_dir + @"images\")) System.IO.Directory.CreateDirectory(this.tmp_file_dir + @"images\");

            if (this.src_file_csv != null)
            {
                CSVTXT2KML();
            }
            else if (this.src_file_ext == ".kml")
            {
                if(this.src_file_pth != (this.tmp_file_dir + "doc.kml"))
                    System.IO.File.Copy(this.src_file_pth, this.tmp_file_dir + "doc.kml", true);
                this.PrepareNormalKML();
            }
            else if (this.src_file_ext == ".gpx")
            {
                System.IO.File.Copy(this.src_file_pth, this.tmp_file_dir + "src.gpx", true);
                GPX2KML("NoName");
            }
            else if (this.src_file_ext == ".dat")
            {
                System.IO.File.Copy(this.src_file_pth, this.tmp_file_dir + "src.dat", true);
                ProGorodDat2KML("NoName");
            }
            else if (this.src_file_ext == ".fit")
            {
                System.IO.File.Copy(this.src_file_pth, this.tmp_file_dir + "src.fit", true);
                GarminFit2KML("NoName");
            }
            else if (this.src_file_ext == ".gdb")
            {
                System.IO.File.Copy(this.src_file_pth, this.tmp_file_dir + "src.gdb", true);
                GarminGDB2KML("NoName");
            }
            else if (this.src_file_ext == ".wpt")
            {
                System.IO.File.Copy(this.src_file_pth, this.tmp_file_dir + "src.wpt", true);
                WPT2KML("NoName");
            }
            else if (this.src_file_ext == ".osm")
            {
                OSM2KML();
            }
            else if (this.src_file_ext == ".kmz")
            {
                UnZipKMZ(this.src_file_pth, this.tmp_file_dir);
                this.PrepareNormalKML();
            }
            else if (this.src_file_ext == ".gpi")
            {
                GPI2KML();
            }
            else
            {
                // return;
            };
            
            this.LoadKML(true);
        }

        public void CSVTXT2KML()
        {
            if (!this.Valid) return;

            FileStream filestream = new FileStream(this.src_file_pth, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(filestream, this.src_file_csv.enc);
            bool he = this.src_file_csv.firstLineIsHeader;

            FileStream fs = new FileStream(this.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>");
            sw.WriteLine("\t<Document>");
            sw.WriteLine("\t<name>" + this.src_file_nme + "</name>");
            sw.WriteLine("\t\t<Folder>");
            sw.WriteLine("\t\t<name>" + this.src_file_nme+ "</name>");
            /////////
            List<string> styles = new List<string>();
            /////////
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Trim();
                if (String.IsNullOrEmpty(line)) continue;
                bool skip = false;
                if ((this.src_file_csv.skipLinesStartsWith != null) && (this.src_file_csv.skipLinesStartsWith.Length > 0))
                    for (int i = 0; i < this.src_file_csv.skipLinesStartsWith.Length; i++)
                        if (line.StartsWith(this.src_file_csv.skipLinesStartsWith[i]))
                            skip = true;
                if (skip) continue;

                string[] cells = line.Split(new char[] { this.src_file_csv.columnDelimiter });
                if (he) // header
                {
                    he = false;
                    continue;
                };
                string nam = cells[this.src_file_csv.fNameNumber];
                string desc = "";
                if (this.src_file_csv.fDescNumber >= 0) desc = cells[this.src_file_csv.fDescNumber];
                double lat = 0;
                double lon = 0;
                if ((this.src_file_csv.floatSeparator == ',') || (this.src_file_csv.floatSeparator == '\0'))
                {
                    double.TryParse(cells[this.src_file_csv.fLatNumber].Replace(" ", "").Replace(",", "."), System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out lat);
                    double.TryParse(cells[this.src_file_csv.fLonNumber].Replace(" ", "").Replace(",", "."), System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out lon);
                };
                if (this.src_file_csv.floatSeparator == '.')
                {
                    double.TryParse(cells[this.src_file_csv.fLatNumber].Replace(" ", ""), System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out lat);
                    double.TryParse(cells[this.src_file_csv.fLonNumber].Replace(" ", ""), System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out lon);
                };
                string xyz = lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0";

                string style = "nostyle";
                if (this.src_file_csv.fStyleNumber >= 0)
                {
                    style = cells[this.src_file_csv.fStyleNumber];
                    if (!String.IsNullOrEmpty(style)) 
                        style = Regex.Replace(style,@"\W","_");                    
                };
                if (styles.IndexOf(style) < 0) styles.Add(style);

                sw.WriteLine("\t\t\t<Placemark>");
                sw.WriteLine("\t\t\t\t<name><![CDATA[" + nam + "]]></name>");
                sw.WriteLine("\t\t\t\t<description><![CDATA[" + desc + "]]></description>");
                sw.WriteLine("\t\t\t\t<styleUrl>#" + style + "</styleUrl>");
                sw.WriteLine("\t\t\t\t<Point><coordinates>" + xyz + "</coordinates></Point>");
                sw.WriteLine("\t\t\t</Placemark>");
            };
            /////////
            sw.WriteLine("\t\t</Folder>");
            int id = 0;
            foreach (string style in styles)
            {
                id++;
                sw.WriteLine("\t<Style id=\"" + style + "\"><IconStyle><Icon><href>images/" + style + ".png</href></Icon></IconStyle></Style>");
                Image im = new Bitmap(16, 16);
                Graphics g = Graphics.FromImage(im);
                g.FillEllipse(Brushes.Magenta, 0, 0, 16, 16);
                g.DrawString(id.ToString("00"), new Font("MS Sans Serif", 8), Brushes.Black, 0, 2);
                g.Dispose();
                im.Save(this.tmp_file_dir + @"images\" + style + ".png");
                //File.Copy(KMZRebuilederForm.CurrentDirectory() + @"KMZRebuilder.noi.png", this.tmp_file_dir + @"images\" + style + ".png", true);
            };
            sw.WriteLine("\t</Document>");
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();            

            sr.Close();
            filestream.Close();
        }

        public float getAvailableRAM()
        {
            System.Diagnostics.PerformanceCounter pc = new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes");
            return pc.NextValue();
        }


        public void From_MapsForgeMapFile(string filename, MapsForgeFileReader.MapsForgeReader mfr, List<int> poi_tags, List<int> way_tags)
        {
            if(((poi_tags == null) || (poi_tags.Count == 0)) && ((way_tags == null) || (way_tags.Count == 0)))
                throw new Exception("Tags List is Empty");
            if (!this.Valid) return;

            Dictionary<int, List<long[]>> in_lay_pois = new Dictionary<int, List<long[]>>();
            Dictionary<int, List<long[]>> in_lay_ways = new Dictionary<int, List<long[]>>();
            int ttlPOI = 0; int ttlWAY = 0; int svdPOI = 0; int svdWAY = 0; int ovaPOI = 0; int ovaWAY = 0; int pp = -1;

            KMZRebuilder.Program.mainForm.ClearLog();
            KMZRebuilder.Program.mainForm.AddToLog("Import From MapsForge ...");
            KMZRebuilder.Program.mainForm.AddToLog("POI categories: " + poi_tags.Count.ToString());
            KMZRebuilder.Program.mainForm.AddToLog("WAY categories: " + way_tags.Count.ToString());
            KMZRebuilder.Program.mainForm.AddToLog("Total categories: " + (poi_tags.Count + way_tags.Count).ToString());
            
            KMZRebuilder.Program.mainForm.AddToLog("Import From MapsForge ...");
            if (KMZRebuilederForm.waitBox != null)
                KMZRebuilederForm.waitBox.Show("Import From MapsForge", "Wait...");

            FileStream fs = new FileStream(this.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
            byte[] tbuff = Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<kml>\r\n\t<Document>\r\n");
            fs.Write(tbuff, 0, tbuff.Length);
            System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds( mfr.FILE_CREATED / 1000 ).ToLocalTime();
            tbuff = Encoding.UTF8.GetBytes("\t<name>" + Path.GetFileName(filename) + " " + dtDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "</name>\r\n");
            fs.Write(tbuff, 0, tbuff.Length);            

            //++TODO    
            Regex rx = new Regex(@"[_\w\s\.\,\-\№\#\:\/\\\!\?""" + @"'`\*\^\(\)\[\]\@\$\%\+\~\;\=\&\|\r\n\t]*");
            try
            {
                // TEMP FILE
                float amem = getAvailableRAM();
                Stream tFS;
                if ((mfr.mff.Length < 52428800) || ((amem > 500) && (amem > (mfr.mff.Length / 1048576 * 1.5))))
                    tFS = new MemoryStream(); // <50Mb of file -- in-memory; or > 500MB of RAM
                else
                    tFS = new FileStream(this.tmp_file_dir + "temp_tags.kml", FileMode.Create, FileAccess.ReadWrite);
                
                // DOING
                KMZRebuilder.Program.mainForm.AddToLog("Reading Map Data ...");
                if (KMZRebuilederForm.waitBox != null)
                    KMZRebuilederForm.waitBox.Show("Reading Map Data", "Wait...");
                                
                foreach (MapsForgeFileReader.MapsForgeReader.ZoomInterval zi in mfr.ZOOM_LEVELS)
                    for (int y = 0; y < zi.tilesHeigth; y++)
                        for (int x = 0; x < zi.tilesWidth; x++)
                        {
                            MapsForgeFileReader.MapsForgeReader.ZoomInterval.Tile tile = zi.ReadTileFromZero(x, y, poi_tags.Count > 0, way_tags.Count > 0, mfr);
                            // GET POIs
                            if ((tile != null) && (tile.POIs.Count > 0))
                            {
                                foreach (MapsForgeFileReader.MapsForgeReader.ZoomInterval.POI poi in tile.POIs)
                                {
                                    bool add = false;
                                    foreach (int tid in poi.tags)
                                        if (poi_tags.Contains(tid))
                                        {
                                            ovaPOI++;                                            
                                            //++ GETPOI
                                            string txt = "";
                                            txt += ("\t\t\t<Placemark>");
                                            string poi_name = poi.name;
                                            if (String.IsNullOrEmpty(poi_name)) poi_name = "Noname";
                                            poi_name = poi_name.Replace("\ren\b", " - ");
                                            poi_name = poi_name.Replace("en\b", "");
                                            poi_name = rx.Match(poi_name).Value;
                                            string comm = poi_name + "\r\n" + (String.IsNullOrEmpty(poi.addr) ? "" : rx.Match(poi.addr).Value);
                                            poi_name = poi_name.Split(new char[] { '\r', '\n' })[0];
                                            if (add) poi_name += " [c]";
                                            txt += ("\t\t\t\t<name><![CDATA[" + poi_name + "]]></name>");
                                            txt += ("\t\t\t\t<description><![CDATA[" + comm + "]]></description>");
                                            {
                                                txt += ("\t\t\t\t<styleUrl>#poistyle" + tid.ToString() + "</styleUrl>");
                                                string xyz = poi.lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + poi.lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0";
                                                txt += ("\t\t\t\t<Point><coordinates>" + xyz + "</coordinates></Point>");
                                            };
                                            txt += ("\t\t\t</Placemark>");
                                            byte[] btw = Encoding.UTF8.GetBytes(txt);
                                            //-- GETPOI
                                            if(!in_lay_pois.ContainsKey(tid)) in_lay_pois.Add(tid, new List<long[]>());
                                            in_lay_pois[tid].Add(new long[] { tFS.Position, btw.Length  });
                                            tFS.Write(btw, 0, btw.Length);
                                            add = true;
                                        };
                                    if (add) svdPOI++;
                                };
                                ttlPOI += tile.POIs.Count;
                            };
                            // Get WAYs
                            if ((tile != null) && (tile.WAYShorts.Count > 0))
                            {
                                foreach (MapsForgeFileReader.MapsForgeReader.ZoomInterval.WAYSHORT way in tile.WAYShorts)
                                {
                                    bool add = false;
                                    foreach (int tid in way.tags)
                                        if (way_tags.Contains(tid))
                                        {
                                            ovaWAY++;                                            
                                            //++ GETWAY
                                            string txt = "";
                                            txt += ("\t\t\t<Placemark>");
                                            string way_name = way.name;
                                            if (String.IsNullOrEmpty(way_name)) way_name = "Noname";
                                            way_name = way_name.Replace("\ren\b", " - ");
                                            way_name = way_name.Replace("en\b", "");
                                            way_name = rx.Match(way_name).Value;
                                            string comm = way_name + "\r\n" + (String.IsNullOrEmpty(way.addr) ? "" : way.addr);
                                            way_name = way_name.Split(new char[] { '\r', '\n' })[0];
                                            if (add) way_name += " [c]";
                                            txt += ("\t\t\t\t<name><![CDATA[" + way_name + "]]></name>");
                                            txt += ("\t\t\t\t<description><![CDATA[" + comm + "]]></description>");
                                            {
                                                txt += ("\t\t\t\t<styleUrl>#waystyle" + tid.ToString() + "</styleUrl>");
                                                string xyz = way.lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + way.lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0";
                                                txt += ("\t\t\t\t<Point><coordinates>" + xyz + "</coordinates></Point>");
                                            };
                                            txt += ("\t\t\t</Placemark>");
                                            byte[] btw = Encoding.UTF8.GetBytes(txt);
                                            //-- GETWAY
                                            if (!in_lay_ways.ContainsKey(tid)) in_lay_ways.Add(tid, new List<long[]>());
                                            in_lay_ways[tid].Add(new long[] { tFS.Position, btw.Length });
                                            tFS.Write(btw, 0, btw.Length);
                                            add = true;
                                        };
                                    if (add) svdWAY++;
                                };
                                ttlWAY += tile.WAYShorts.Count;
                            };
                                                                                    
                            int cp = (int)(((double)mfr.mff.Position / (double)mfr.mff.Length) * 100.0);
                            if (cp != pp)
                                if (KMZRebuilederForm.waitBox != null)
                                    KMZRebuilederForm.waitBox.Show("Reading Map Data ...", "Reading Map Data " + cp.ToString() + "%, " + ttlPOI.ToString() + " POIs " + ttlWAY.ToString() + " WAYs");
                            pp = cp;
                        };

                if (KMZRebuilederForm.waitBox != null)
                    KMZRebuilederForm.waitBox.Show("Reading Map Data ...", "Reading Map Data 100%");
                KMZRebuilder.Program.mainForm.AddToLog("Total POIs counted " + ttlPOI.ToString());
                KMZRebuilder.Program.mainForm.AddToLog("Total WAYs counted " + ttlWAY.ToString());
                KMZRebuilder.Program.mainForm.AddToLog("Saved " + ovaPOI.ToString() + " POIs and " + svdPOI.ToString() + " unical");
                KMZRebuilder.Program.mainForm.AddToLog("Saved " + ovaWAY.ToString() + " WAYs and " + svdWAY.ToString() + " unical");

                // RELEASE TEMP FILE //
                tFS.Flush();
                byte[] cbuff = new byte[4096];
                KMZRebuilder.Program.mainForm.AddToLog("Saving result ...");
                if (KMZRebuilederForm.waitBox != null)
                    KMZRebuilederForm.waitBox.Show("Saving result", "Wait...");
                foreach (int tid in poi_tags) // POIs TAGS
                {
                    if (!in_lay_pois.ContainsKey(tid)) continue; // no empty
                    tbuff = Encoding.UTF8.GetBytes("\t\t<Folder>\r\n\t\t<name><![CDATA[POI " + mfr.POI_TAGS[tid].Key + " = " + mfr.POI_TAGS[tid].Value + " [" + in_lay_pois[tid].Count.ToString() + "]]]></name>\r\n");
                    fs.Write(tbuff, 0, tbuff.Length);
                    foreach (long[] pl in in_lay_pois[tid])
                    {
                        tFS.Position = pl[0];
                        int len = (int)pl[1];
                        if (len > cbuff.Length)
                            cbuff = new byte[len];
                        tFS.Read(cbuff, 0, len);
                        fs.Write(cbuff, 0, len);
                    };
                    tbuff = Encoding.UTF8.GetBytes("\t\t</Folder>\r\n");
                    fs.Write(tbuff, 0, tbuff.Length);
                };
                foreach (int tid in way_tags) // WAYs TAGS
                {
                    if (!in_lay_ways.ContainsKey(tid)) continue; // no empty
                    tbuff = Encoding.UTF8.GetBytes("\t\t<Folder>\r\n\t\t<name><![CDATA[WAY " + mfr.WAY_TAGS[tid].Key + " = " + mfr.WAY_TAGS[tid].Value + " [" + in_lay_ways[tid].Count.ToString() + "]]]></name>\r\n");
                    fs.Write(tbuff, 0, tbuff.Length);
                    foreach (long[] pl in in_lay_ways[tid])
                    {
                        tFS.Position = pl[0];
                        int len = (int)pl[1];
                        if (len > cbuff.Length)
                            cbuff = new byte[len];
                        tFS.Read(cbuff, 0, len);
                        fs.Write(cbuff, 0, len);
                    };
                    tbuff = Encoding.UTF8.GetBytes("\t\t</Folder>\r\n");
                    fs.Write(tbuff, 0, tbuff.Length);                 
                };
                tFS.Close();
                File.Delete(this.tmp_file_dir + "temp_tags.kml");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Reading File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Hide();
                KMZRebuilder.Program.mainForm.AddToLog("Error");
                return;
            };
            //--TODO

            // ZIP IMAGES
            KMZRebuilder.Program.mainForm.AddToLog("Creating Styles ...");
            if (KMZRebuilederForm.waitBox != null)
                KMZRebuilederForm.waitBox.Show("Saving result", "Creating Styles...");

            ZipFile zf = null;
            try { zf = new ZipFile(File.OpenRead(KMZRebuilederForm.CurrentDirectory() + @"\mapicons\default.zip")); }
            catch { };
            string imgs_dir = this.tmp_file_dir + @"\images";
            foreach (int tid in poi_tags) // POI STYLES
            {
                if (!in_lay_pois.ContainsKey(tid)) continue; // no empty
                string name = "";
                string name1 = mfr.POI_TAGS[tid].Key + "_" + mfr.POI_TAGS[tid].Value.Replace(" ", "_") + ".png";
                string name2 = mfr.POI_TAGS[tid].Value.Replace(" ", "_") + ".png";
                if (zf != null)
                {
                    try
                    {
                        ZipEntry ze = zf.GetEntry(name = name1);
                        if (ze == null)
                            ze = zf.GetEntry(name = name2);
                        if (ze != null)
                        {
                            byte[] buffer = new byte[4096];
                            Stream zipStream = zf.GetInputStream(ze);
                            using (FileStream streamWriter = File.Create(imgs_dir + @"\" + name))
                                StreamUtils.Copy(zipStream, streamWriter, buffer);
                        };
                    }
                    catch { };
                };
                tbuff = Encoding.UTF8.GetBytes("\t<Style id=\"poistyle" + tid.ToString() + "\"><IconStyle><Icon><href>images/" + name + "</href></Icon></IconStyle></Style>\r\n");
                fs.Write(tbuff, 0, tbuff.Length);   
            };
            foreach (int tid in way_tags) // WAY styles
            {
                if (!in_lay_ways.ContainsKey(tid)) continue; // no empty
                string name = "";
                string name1 = mfr.WAY_TAGS[tid].Key + "_" + mfr.WAY_TAGS[tid].Value.Replace(" ", "_") + ".png";
                string name2 = mfr.WAY_TAGS[tid].Value.Replace(" ", "_") + ".png";
                if (zf != null)
                {
                    try
                    {
                        ZipEntry ze = zf.GetEntry(name = name1);
                        if (ze == null)
                            ze = zf.GetEntry(name = name2);
                        if (ze != null)
                        {
                            byte[] buffer = new byte[4096];
                            Stream zipStream = zf.GetInputStream(ze);
                            using (FileStream streamWriter = File.Create(imgs_dir + @"\" + name))
                                StreamUtils.Copy(zipStream, streamWriter, buffer);
                        };
                    }
                    catch { };
                };
                tbuff = Encoding.UTF8.GetBytes("\t<Style id=\"waystyle" + tid.ToString() + "\"><IconStyle><Icon><href>images/" + name + "</href></Icon></IconStyle></Style>\r\n");
                fs.Write(tbuff, 0, tbuff.Length);   
            };
            if (zf != null) { zf.IsStreamOwner = true; zf.Close(); };   

            tbuff = Encoding.UTF8.GetBytes("\t</Document>\r\n</kml>\r\n");
            fs.Write(tbuff, 0, tbuff.Length);   
            fs.Close();

            if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Hide();
            KMZRebuilder.Program.mainForm.AddToLog("Done");
        }

        public void From_MapsForgePOIFile(string filename, List<string[]> cats)
        {
            if ((cats == null) || (cats.Count == 0))
                throw new Exception("Categories List is Empty");
            if (!this.Valid) return;

            KMZRebuilder.Program.mainForm.ClearLog();
            KMZRebuilder.Program.mainForm.AddToLog("Preparing output file ...");

            FileStream fs = new FileStream(this.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>");
            sw.WriteLine("\t<Document>");
            sw.WriteLine("\t<name>MapsForge POI " + Path.GetFileName(filename) + "</name>");

            try
            {
                KMZRebuilder.Program.mainForm.AddToLog("Opening POI file ...");
                System.Data.SQLite.SQLiteConnection sqlc = new System.Data.SQLite.SQLiteConnection(@"Data Source=" + filename + ";Version=3;");
                sqlc.Open();
                System.Data.SQLite.SQLiteCommand sc = new System.Data.SQLite.SQLiteCommand("", sqlc);
                System.Data.SQLite.SQLiteDataReader dr;

                KMZRebuilder.Program.mainForm.AddToLog("Reading POI file ...");
                Regex rnrx = new Regex(@"name=(?<name>[^\r\n]*)|name:ru=(?<ru>[^\r\n]*)|name:en=(?<en>[^\r\n]*)|name:int=(?<int>[^\r\n]*)", RegexOptions.IgnoreCase);
                foreach (string[] c in cats)
                {
                    KMZRebuilder.Program.mainForm.AddToLog("Reading POI " + c[1] + " ...");
                    bool fldr_wrtd = false;

                    sc.CommandText = "SELECT POI_DATA.ID, POI_DATA.DATA AS TEXT, (poi_index.minlat+poi_index.maxlat)/2 as lat, (poi_index.minlon+poi_index.maxlon)/2 as lon FROM POI_DATA LEFT JOIN POI_INDEX ON POI_DATA.ID = POI_INDEX.ID  WHERE POI_DATA.ID IN (SELECT ID FROM POI_CATEGORY_MAP WHERE CATEGORY = " + c[0] + ")";
                    dr = sc.ExecuteReader();
                    while (dr.Read())
                    {
                        if (!fldr_wrtd)
                        {
                            sw.WriteLine("\t\t<Folder>");
                            sw.WriteLine("\t\t<name><![CDATA[" + c[1] + "]]></name>");
                            fldr_wrtd = true;
                        };

                        string name = dr["ID"].ToString();
                        string text = dr["Text"].ToString();
                        {
                            MatchCollection rnmx = rnrx.Matches(text);
                            if (rnmx.Count > 0)
                            {
                                bool next = true;
                                if(next)
                                    for (int i = 0; i < rnmx.Count; i++)
                                        if (rnmx[i].Groups["ru"].Value != "")
                                        {
                                            name = rnmx[i].Groups["ru"].Value;
                                            next = false;
                                        };
                                if (next)
                                    for (int i = 0; i < rnmx.Count; i++)
                                        if (rnmx[i].Groups["en"].Value != "")
                                        {
                                            name = rnmx[i].Groups["en"].Value;
                                            next = false;
                                        };
                                if (next)
                                    for (int i = 0; i < rnmx.Count; i++)
                                        if (rnmx[i].Groups["int"].Value != "")
                                        {
                                            name = rnmx[i].Groups["int"].Value;
                                            next = false;
                                        };
                                if (next)
                                    for (int i = 0; i < rnmx.Count; i++)
                                        if (rnmx[i].Groups["name"].Value != "")
                                        {
                                            name = rnmx[i].Groups["name"].Value;
                                            next = false;
                                        };
                            };
                            if (c[1].ToLower() == "address")
                            {
                                string on = name;
                                name = "";
                                Match mx = (new Regex(@"addr:city=([^\r\n]*)", RegexOptions.IgnoreCase)).Match(text);
                                if(mx.Success)
                                    name = mx.Groups[1].Value;
                                mx = (new Regex(@"addr:street=([^\r\n]*)", RegexOptions.IgnoreCase)).Match(text);
                                if (mx.Success)
                                    name += (name.Length > 0 ? ", " : "") + mx.Groups[1].Value;
                                mx = (new Regex(@"addr:housenumber=([^\r\n]*)", RegexOptions.IgnoreCase)).Match(text);
                                if (mx.Success)
                                {
                                    if (mx.Groups[1].Value == on) on = "";
                                    name += (name.Length > 0 ? ", " : "") + mx.Groups[1].Value;
                                };
                                if(on != "")
                                    name += " (" + on + ")";
                            };
                        };
                        string dlat = dr["LAT"].ToString().Replace(",",".");
                        string dlon = dr["LON"].ToString().Replace(",", ".");
                        sw.WriteLine("\t\t\t<Placemark>");
                        sw.WriteLine("\t\t\t\t<name><![CDATA[" + name + "]]></name>");
                        sw.WriteLine("\t\t\t\t<description><![CDATA[" + text + "]]></description>");
                        {
                            sw.WriteLine("\t\t\t\t<styleUrl>#catstyle" + c[0] + "</styleUrl>");
                            double lon = double.Parse(dlon, System.Globalization.CultureInfo.InvariantCulture);
                            double lat = double.Parse(dlat, System.Globalization.CultureInfo.InvariantCulture);
                            string xyz = lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0";
                            sw.WriteLine("\t\t\t\t<Point><coordinates>" + xyz + "</coordinates></Point>");
                        };
                        sw.WriteLine("\t\t\t</Placemark>");
                    };
                    dr.Close();
                    
                    if(fldr_wrtd)
                        sw.WriteLine("\t\t</Folder>");
                };
                sqlc.Close();

                KMZRebuilder.Program.mainForm.AddToLog("Creating icons and styles ...");
                string imgs_dir = this.tmp_file_dir + @"\images";
                ZipFile zf = null;
                try { zf = new ZipFile(File.OpenRead(KMZRebuilederForm.CurrentDirectory() + @"\mapicons\default.zip")); } catch {};
                foreach (string[] c in cats)
                {
                    string cname = c[1];
                    int lif = cname.LastIndexOf(@"\");
                    if (lif > 0) cname = cname.Substring(lif + 1).Trim();
                    cname = cname.Replace(" ", "_").ToLower();
                    if (cname == "address") cname = "house";
                    if (zf != null)
                    {
                        try
                        {
                            ZipEntry ze = zf.GetEntry(cname + ".png");
                            if ((ze == null) && (cname.EndsWith("s")))
                                ze = zf.GetEntry((cname = cname.Substring(0, cname.Length - 1)) + ".png");
                            if (ze != null)
                            {
                                byte[] buffer = new byte[4096];
                                Stream zipStream = zf.GetInputStream(ze);
                                using (FileStream streamWriter = File.Create(imgs_dir + @"\" + cname + ".png"))
                                    StreamUtils.Copy(zipStream, streamWriter, buffer);
                            };
                        }
                        catch { };
                    };
                    sw.WriteLine("\t<Style id=\"catstyle" + c[0] + "\"><IconStyle><Icon><href>images/" + cname + ".png</href></Icon></IconStyle></Style>");
                };
                if (zf != null) {  zf.IsStreamOwner = true;  zf.Close(); };                                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SASPlanet DB Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                KMZRebuilder.Program.mainForm.AddToLog("Error");
                return;
            };

            sw.WriteLine("\t</Document>");
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();

            KMZRebuilder.Program.mainForm.AddToLog("Done");
        }


        public void DB32KML(string filename, int[] categories, string icons_path)
        {
            if ((categories == null) || (categories.Length == 0))
                throw new Exception("Categories List is Empty");
            if (!this.Valid) return;

            string spmd = Path.GetDirectoryName(icons_path).Trim('\0')+@"\";
            bool spmi = Directory.Exists(spmd);

            FileStream fs = new FileStream(this.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>");
            sw.WriteLine("\t<Document>");            
            sw.WriteLine("\t<name>SASPlanet SQLite DB " + Path.GetFileName(filename) + "</name>");

            Dictionary<int, string> cats = new Dictionary<int, string>();
            List<int> imgs = new List<int>();
            List<int> polapp = new List<int>();
            try
            {
                System.Data.SQLite.SQLiteConnection sqlc = new System.Data.SQLite.SQLiteConnection(@"Data Source=" + filename + ";Version=3;");
                sqlc.Open();
                System.Data.SQLite.SQLiteCommand sc = new System.Data.SQLite.SQLiteCommand("", sqlc);

                string cats_in = "";
                for (int i = 0; i < categories.Length; i++)
                    cats_in += (cats_in.Length > 0 ? "," : "") + categories[i].ToString();
                sc.CommandText = "SELECT * FROM CATEGORY where ID in (" + cats_in + ")";
                System.Data.SQLite.SQLiteDataReader dr = sc.ExecuteReader();
                while (dr.Read())
                    cats.Add(Convert.ToInt32(dr["ID"]), dr["cName"].ToString());
                dr.Close();

                foreach (KeyValuePair<int, string> kvp in cats)
                {
                    sw.WriteLine("\t\t<Folder>");
                    sw.WriteLine("\t\t<name><![CDATA[" + kvp.Value + "]]></name>");

                    sc.CommandText = "select ID, mImage, mAppearance, mName, mDesc, mGeoType, mGeoCount, mGeoWKB  from mark where mGeoType in (1,2,3) and mCategory = " + kvp.Key.ToString();
                    dr = sc.ExecuteReader();
                    while (dr.Read())
                    {
                        string nam = dr["mName"].ToString();
                        string desc = dr["mDesc"].ToString();
                        int geoType = Convert.ToInt32(dr["mGeoType"].ToString());
                        //int geoCount = Convert.ToInt32(dr["mGeoCount"].ToString());                        
                        byte[] blob = (byte[])dr["mGEOWKB"];
                        sw.WriteLine("\t\t\t<Placemark>");
                        sw.WriteLine("\t\t\t\t<name><![CDATA[" + nam + "]]></name>");
                        sw.WriteLine("\t\t\t\t<description><![CDATA[" + desc + "]]></description>");
                        if (geoType == 1)
                        {
                            int mImage = Convert.ToInt32(dr["mImage"].ToString());
                            sw.WriteLine("\t\t\t\t<styleUrl>#sasstyle" + mImage.ToString() + "</styleUrl>");                                                        
                            double lon = BitConverter.ToDouble(blob, 5 + 0);
                            double lat = BitConverter.ToDouble(blob, 5 + 8);
                            string xyz = lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0";
                            sw.WriteLine("\t\t\t\t<Point><coordinates>" + xyz + "</coordinates></Point>");

                            if (imgs.IndexOf(mImage) < 0) imgs.Add(mImage);
                        };
                        if (geoType == 2)
                        {
                            int mAppearance = Convert.ToInt32(dr["mAppearance"].ToString());
                            sw.WriteLine("\t\t\t\t<styleUrl>#saspoly" + mAppearance.ToString() + "</styleUrl>");
                            sw.Write("\t\t\t\t<LineString><extrude>1</extrude><coordinates>");                                                            
                            int count = BitConverter.ToInt32(blob, 5);
                            for (int i = 0; i < count; i++)
                            {
                                double lon = BitConverter.ToDouble(blob, 9 + i * 16);
                                double lat = BitConverter.ToDouble(blob, 9 + i * 16 + 8);
                                sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", lon, lat));
                            };
                            sw.WriteLine("</coordinates></LineString>");

                            if (polapp.IndexOf(mAppearance) < 0) polapp.Add(mAppearance);
                        };
                        if (geoType == 3)
                        {
                            int mAppearance = Convert.ToInt32(dr["mAppearance"].ToString());
                            sw.WriteLine("\t\t\t\t<styleUrl>#saspoly" + mAppearance.ToString() + "</styleUrl>");
                            sw.Write("\t\t\t\t<Polygon><extrude>1</extrude><outerBoundaryIs><LinearRing>\r\n");
                            sw.Write("\t\t\t\t<coordinates>");
                            int outb = BitConverter.ToInt32(blob, 5);
                            int count = BitConverter.ToInt32(blob, 9);                            
                            for (int i = 0; i < count; i++)
                            {
                                double lon = BitConverter.ToDouble(blob, 13 + i * 16);
                                double lat = BitConverter.ToDouble(blob, 13 + i * 16 + 8);
                                sw.Write(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", lon, lat));
                            };
                            sw.WriteLine("</coordinates>\t\t\t\t</LinearRing></outerBoundaryIs></Polygon>");                                                        

                            if (polapp.IndexOf(mAppearance) < 0) polapp.Add(mAppearance);
                        };
                        sw.WriteLine("\t\t\t</Placemark>");
                    };
                    dr.Close();                    

                    sw.WriteLine("\t\t</Folder>");
                };

                if(polapp.Count > 0)
                    for (int i = 0; i < polapp.Count; i++)
                    {
                        sc.CommandText = "select * from MarkAppearance where ID = " + polapp[i].ToString();
                        dr = sc.ExecuteReader();
                        if (dr.Read())
                        {
                            Color col1 = DB3ColorToColor((uint)Convert.ToInt64(dr["maColor1"].ToString()));
                            Color col2 = DB3ColorToColor((uint)Convert.ToInt64(dr["maColor2"].ToString()));
                            string width = dr["maScale1"].ToString();
                            sw.WriteLine("\t<Style id=\"saspoly" + polapp[i].ToString() + "\"><LineStyle><color>" + LineAreaStyleForm.HexStyleConverter(col1) + "</color><width>" + width + "</width></LineStyle>" +
                                "<PolyStyle><color>" + AddTextAsPoly.HexStyleConverter(col2) + "</color><fill>" + (col2.A == 0 ? "0" : "1") + "</fill></PolyStyle></Style>");
                        };
                        dr.Close();
                    };

                if (imgs.Count > 0)
                    for (int i = 0; i < imgs.Count; i++)
                    {
                        bool ex = false;                        
                        {
                            string name = "none";
                            sc.CommandText = "select miName from MarkImage where ID = " + imgs[i].ToString();
                            dr = sc.ExecuteReader();
                            if (dr.Read()) name = dr[0].ToString();                                
                            dr.Close();
                            if (spmi)
                            {
                                string file = spmd + name;
                                if (File.Exists(file))
                                    try
                                    {
                                        File.Copy(file, this.tmp_file_dir + @"\images\sasstyle" + imgs[i].ToString() + ".png", true);
                                        ex = true;
                                    }
                                    catch { }
                            }
                            else
                            {
                                string spzf = KMZRebuilederForm.CurrentDirectory() + @"\MapIcons\sasplanet.zip";
                                if(File.Exists(spzf))
                                {
                                    try
                                    {
                                        MapIcons.SaveIcon(GetFileFromZip(spzf, name), this.tmp_file_dir + @"\images\sasstyle" + imgs[i].ToString() + ".png");
                                        ex = true;
                                    }
                                    catch { };
                                };
                            };
                        };
                        if (!ex)
                        {
                            Bitmap bmp = new Bitmap(32, 32);
                            Graphics g = Graphics.FromImage(bmp);
                            g.FillEllipse(Brushes.Yellow, 0, 0, 32, 32);
                            g.DrawEllipse(new Pen(Color.Red, 2), 0, 0, 31, 31);
                            SizeF ms = g.MeasureString(imgs[i].ToString(), new Font("Arial", 11, FontStyle.Bold));
                            g.DrawString(imgs[i].ToString(), new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 16 - ms.Width / 2, 16 - ms.Height / 2);
                            g.Dispose();
                            string fName = this.tmp_file_dir + @"\images\sasstyle" + imgs[i].ToString() + ".png";
                            try
                            {
                                bmp.Save(fName, ImageFormat.Png);
                            }
                            catch (Exception ex2)
                            {
                                try
                                {
                                    ImageMagick.MagickImage mi = new ImageMagick.MagickImage(bmp);
                                    FileStream sfs = new FileStream(fName, FileMode.Create, FileAccess.Write);
                                    mi.Write(sfs, ImageMagick.MagickFormat.Png);
                                    sfs.Close();
                                }
                                catch (Exception subex)
                                {

                                };
                            };
                            bmp.Dispose();                            
                        };
                        sw.WriteLine("\t<Style id=\"sasstyle" + imgs[i].ToString() + "\"><IconStyle><Icon><href>images/sasstyle" + imgs[i].ToString() + ".png</href></Icon></IconStyle></Style>");
                };

                sqlc.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SASPlanet DB Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            sw.WriteLine("\t</Document>");
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();
        }

        public static Color DB3ColorToColor(uint delphiColor)
        {                        
            int b = (int)((delphiColor >> 0) & 0xFF);
            int g = (int)((delphiColor >> 8) & 0xFF);
            int r = (int)((delphiColor >> 16) & 0xFF);
            int a = (int)((delphiColor >> 24) & 0xFF);
            return Color.FromArgb(a, r, g, b);

            //switch ((delphiColor >> 24) & 0xFF)
            //{
            //    case 0x01: // Indexed
            //    case 0xFF: // Error
            //        return Color.Transparent;

            //    case 0x80: // System
            //        return Color.FromKnownColor((KnownColor)(delphiColor & 0xFFFFFF));

            //    default:
            //        int r = (int)(delphiColor & 0xFF);
            //        int g = (int)((delphiColor >> 8) & 0xFF);
            //        int b = (int)((delphiColor >> 16) & 0xFF);
            //        return Color.FromArgb(r, g, b);
            //}
        }

        public static byte[] GetFileFromZip(string zipfile, string imagefile)
        {
            try
            {
                FileStream fs = File.OpenRead(zipfile);
                ZipFile zf = new ZipFile(fs);
                int index = 0;
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile) continue; // Ignore directories
                    if (zipEntry.Name.ToLower() != imagefile.ToLower()) continue;

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    try
                    {                        
                        Stream ms = new MemoryStream();
                        StreamUtils.Copy(zipStream, ms, buffer);
                        ms.Flush();
                        ms.Position = 0;
                        byte[] res = new byte[ms.Length];
                        ms.Read(res, 0, res.Length);
                        ms.Dispose();
                        zf.Close();
                        fs.Close();
                        return res;
                    }
                    catch
                    {
                    };
                };
                zf.Close();
                fs.Close();
            }
            catch { };
            return null;
        }

        public static Image GetImageFromZip(string zipfile, string imagefile)
        {
            try
            {
                FileStream fs = File.OpenRead(zipfile);
                ZipFile zf = new ZipFile(fs);
                int index = 0;
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile) continue; // Ignore directories
                    if (zipEntry.Name.ToLower() != imagefile.ToLower()) continue;
                    
                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    try
                    {
                        Stream ms = new MemoryStream();
                        StreamUtils.Copy(zipStream, ms, buffer);
                        ms.Position = 0;
                        Image im = new Bitmap(ms);
                        ms.Dispose();
                        zf.Close();
                        fs.Close();
                        return im;
                    }
                    catch
                    {
                    };
                };
                zf.Close();
                fs.Close();
            }
            catch { };
            return null;
        }

        public void WPT2KML(string origin_name)
        {
            WPTPOI[] recs = WPTPOI.ReadFile(this.tmp_file_dir + "src.wpt");

            FileStream fs = new FileStream(this.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>");
            sw.WriteLine("\t<Document>");
            sw.WriteLine("\t<name>OziExplorer Waypoint File</name>");
            sw.WriteLine("\t\t<Folder>");
            sw.WriteLine("\t\t<name>OziExplorer Waypoint File</name>");
            List<int> styles = new List<int>();
            foreach (WPTPOI rec in recs)
            {
                string nam = rec.Name;
                string desc = rec.Description;
                string xyz = rec.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + rec.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0";
                sw.WriteLine("\t\t\t<Placemark>");
                sw.WriteLine("\t\t\t\t<name><![CDATA[" + nam + "]]></name>");
                sw.WriteLine("\t\t\t\t<description><![CDATA[" + desc + "]]></description>");
                if (styles.IndexOf(rec.Symbol) < 0)
                    styles.Add(rec.Symbol);
                sw.WriteLine("\t\t\t\t<styleUrl>#wpt" + rec.Symbol.ToString("00") + "</styleUrl>");
                sw.WriteLine("\t\t\t\t<Point><coordinates>" + xyz + "</coordinates></Point>");
                sw.WriteLine("\t\t\t</Placemark>");
            };
            sw.WriteLine("\t\t</Folder>");
            int id = 0;

            string zipFile = KMZRebuilederForm.CurrentDirectory() + @"\gdbicons\wpt_icons.zip";
            foreach (int style in styles)
            {                
                id++;
                sw.WriteLine("\t<Style id=\"wpt" + style.ToString("00") + "\"><IconStyle><Icon><href>images/wpt_" + style.ToString("00") + ".png</href></Icon></IconStyle></Style>");
                byte[] img = null;
                if (File.Exists(zipFile))
                    img = KMFile.GetFileFromZip(zipFile, style.ToString("00") + ".png");
                if (img != null)
                    MapIcons.SaveIcon(img, this.tmp_file_dir + @"images\wpt_" + style.ToString("00") + ".png");
                else
                {
                    Image im = new Bitmap(16, 16);
                    Graphics g = Graphics.FromImage(im);
                    g.FillEllipse(Brushes.Magenta, 0, 0, 16, 16);
                    if ((style >= 0) && (style <= 99))
                        g.DrawString(style.ToString("00"), new Font("MS Sans Serif", 8), Brushes.Black, 0, 2);
                    else
                        g.DrawString(id.ToString("00"), new Font("MS Sans Serif", 8), Brushes.Black, 0, 2);
                    g.Dispose();
                    im.Save(this.tmp_file_dir + @"images\wpt_" + style.ToString() + ".png");
                };
            };
            sw.WriteLine("\t</Document>");
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();
            
        }

        public void ProGorodDat2KML(string origin_name)
        {
            ProGorodPOI.FavRecord[] recs = ProGorodPOI.ReadFile(this.tmp_file_dir + "src.dat");

            //List<string> types = new List<string>();
            //for (int i = 0; i < 20; i++) types.Add(((ProGorodPOI.TType)i).ToString());

            FileStream fs = new FileStream(this.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>");
            sw.WriteLine("\t<Document>");
            sw.WriteLine("\t<name>ProGorod Favorites</name>");
            sw.WriteLine("\t\t<Folder>");
            sw.WriteLine("\t\t<name>ProGorod Favorites</name>");
            foreach (ProGorodPOI.FavRecord rec in recs)
            {
                string nam = rec.Name;
                string desc = rec.Desc;
                string xyz = rec.Lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + rec.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0";
                sw.WriteLine("\t\t\t<Placemark>");
                sw.WriteLine("\t\t\t\t<name><![CDATA[" + nam + "]]></name>");
                sw.WriteLine("\t\t\t\t<description><![CDATA[" + desc + "]]></description>");
                sw.WriteLine("\t\t\t\t<styleUrl>#progorod" + ((int)rec.Icon).ToString("00") + "</styleUrl>");
                sw.WriteLine("\t\t\t\t<Point><coordinates>" + xyz + "</coordinates></Point>");
                sw.WriteLine("\t\t\t</Placemark>");
            };
            sw.WriteLine("\t\t</Folder>");
            string zipFile = KMZRebuilederForm.CurrentDirectory() + @"\gdbicons\progorod.zip";
            if (File.Exists(zipFile))
                for (int i = 0; i < 20; i++)
                {
                    MapIcons.SaveIcon(KMFile.GetFileFromZip(zipFile, "progorod" + i.ToString("00") + ".png"), this.tmp_file_dir + @"\images\progorod" + (i).ToString("00") + ".png");
                    sw.WriteLine("\t<Style id=\"progorod" + (i).ToString("00") + "\"><IconStyle><Icon><href>images/progorod" + (i).ToString("00") + ".png</href></Icon></IconStyle></Style>");
                };
            sw.WriteLine("\t</Document>");
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();
        }

        public void GarminFit2KML(string origin_name)
        {
            try
            {
                FitParser.FitConverter.Fit2KML(this.tmp_file_dir + "src.fit", this.tmp_file_dir + "doc.kml");
            }
            catch { };
        }

        public void GarminGDB2KML(string origin_name)
        {
            NavitelRecord[] recs = NavitelGDB.ReadFile(this.tmp_file_dir + "src.gdb");

            //List<string> types = new List<string>();
            //for (int i = 0; i < 20; i++) types.Add(((ProGorodPOI.TType)i).ToString());

            FileStream fs = new FileStream(this.tmp_file_dir + "doc ", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>");
            sw.WriteLine("\t<Document>");
            sw.WriteLine("\t<name>Navitel GDB</name>");
            sw.WriteLine("\t\t<Folder>");
            sw.WriteLine("\t\t<name>Navitel GDB</name>");
            List<uint> iconlist = new List<uint>();
            foreach (NavitelRecord rec in recs)
            {
                string nam = rec.name;
                string desc = rec.desc;
                string xyz = rec.lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + rec.lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0";
                sw.WriteLine("\t\t\t<Placemark>");
                sw.WriteLine("\t\t\t\t<name><![CDATA[" + nam + "]]></name>");
                sw.WriteLine("\t\t\t\t<description><![CDATA[" + desc + "]]></description>");
                sw.WriteLine("\t\t\t\t<styleUrl>#gdb" + (rec.iconIndex).ToString("000") + "</styleUrl>");
                sw.WriteLine("\t\t\t\t<Point><coordinates>" + xyz + "</coordinates></Point>");
                sw.WriteLine("\t\t\t</Placemark>");
                if (iconlist.IndexOf(rec.iconIndex) < 0) iconlist.Add(rec.iconIndex);
            };
            sw.WriteLine("\t\t</Folder>");
            string zipFile = KMZRebuilederForm.CurrentDirectory()+@"\gdbicons\gdb_icons.zip";
            for (int i = 0; i < iconlist.Count; i++)
            {
                if (File.Exists(zipFile))
                {
                    try
                    {
                        MapIcons.SaveIcon(GetFileFromZip(zipFile, (iconlist[i]).ToString("000") + ".png"), this.tmp_file_dir + @"\images\gdb" + (iconlist[i]).ToString("000") + ".png");
                    } 
                    catch { };
                };
                sw.WriteLine("\t<Style id=\"gdb" + (iconlist[i]).ToString("000") + "\"><IconStyle><Icon><href>images/gdb" + (iconlist[i]).ToString("000") + ".png</href></Icon></IconStyle></Style>");
            };
            sw.WriteLine("\t</Document>");
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();
        }

        public void GPX2KML(string origin_name)
        {
            FileStream fs = new FileStream(this.tmp_file_dir + "src.gpx", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            string xml = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            xml = RemoveXMLNamespaces(xml);
            XmlDocument gpx = new XmlDocument();
            gpx.LoadXml(xml);

            fs = new FileStream(this.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>");
            sw.WriteLine("\t<Document>");
            sw.WriteLine("\t<name>" + origin_name + "</name>");
            sw.WriteLine("\t\t<Folder>");
            sw.WriteLine("\t\t<name>" + origin_name + "</name>");
            foreach (XmlNode wpt in gpx.SelectNodes("gpx/wpt"))
            {
                string nam = "";
                string desc = "";
                string xyz = wpt.Attributes["lon"].Value + "," + wpt.Attributes["lat"].Value + ",0";
                try { nam = wpt.SelectSingleNode("name").ChildNodes[0].Value; } catch { };
                try { desc = wpt.SelectSingleNode("desc").ChildNodes[0].Value; } catch { };
                sw.WriteLine("\t\t\t<Placemark>");
                sw.WriteLine("\t\t\t\t<name><![CDATA[" + nam + "]]></name>");
                sw.WriteLine("\t\t\t\t<description><![CDATA[" + desc + "]]></description>");
                sw.WriteLine("\t\t\t\t<styleUrl>#noicon</styleUrl>");
                sw.WriteLine("\t\t\t\t<Point><coordinates>" + xyz + "</coordinates></Point>");                
                sw.WriteLine("\t\t\t</Placemark>");
            };
            foreach (XmlNode rte in gpx.SelectNodes("gpx/rte"))
            {
                string nam = "";
                string desc = "";
                try { nam = rte.SelectSingleNode("name").ChildNodes[0].Value; }
                catch { };
                try { desc = rte.SelectSingleNode("desc").ChildNodes[0].Value; }
                catch { };
                string xyz = "";
                foreach(XmlNode rtept in rte.SelectNodes("rtept"))
                    xyz += rtept.Attributes["lon"].Value + "," + rtept.Attributes["lat"].Value + ",0 ";
                sw.WriteLine("\t\t\t<Placemark>");
                sw.WriteLine("\t\t\t\t<name>" + nam + "</name>");
                sw.WriteLine("\t\t\t\t<description><![CDATA[" + desc + "]]></description>");
                sw.WriteLine("\t\t\t\t<LineString><coordinates>" + xyz + "</coordinates></LineString>");
                sw.WriteLine("\t\t\t</Placemark>");
            };
            foreach (XmlNode trk in gpx.SelectNodes("gpx/trk"))
            {
                string nam = "";
                string desc = "";
                try { nam = trk.SelectSingleNode("name").ChildNodes[0].Value; }
                catch { };
                try { desc = trk.SelectSingleNode("desc").ChildNodes[0].Value; }
                catch { };

                foreach (XmlNode trkseg in trk.SelectNodes("trkseg"))
                {
                    string xyz = "";
                    XmlNodeList nl = trkseg.SelectNodes("trkpt");
                    int cnt = 0; 
                    foreach (XmlNode trkpt in nl) // FOR DEBUG COUNTER
                    {                         
                        xyz += trkpt.Attributes["lon"].Value + "," + trkpt.Attributes["lat"].Value + ",0 ";
                        cnt++;
                    };
                    sw.WriteLine("\t\t\t<Placemark>");
                    sw.WriteLine("\t\t\t\t<name>" + nam + "</name>");
                    sw.WriteLine("\t\t\t\t<description><![CDATA[" + desc + "]]></description>");
                    sw.WriteLine("\t\t\t\t<LineString><coordinates>" + xyz + "</coordinates></LineString>");
                    sw.WriteLine("\t\t\t</Placemark>");
                };
            };         
            sw.WriteLine("\t\t</Folder>");
            sw.WriteLine("\t<Style id=\"noicon\"><IconStyle><Icon><href>images/noicon.png</href></Icon></IconStyle></Style>");
            sw.WriteLine("\t</Document>");                        
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();

            File.Copy(KMZRebuilederForm.CurrentDirectory() + @"KMZRebuilder.gpx.png", this.tmp_file_dir + @"images\noicon.png", true);
        }

        public void GPI2KML()
        {
            try
            {
                GPIReader gpi = new GPIReader(this.src_file_pth);
                gpi.SaveToKML(this.tmp_file_dir + "doc.kml");
            }
            catch (Exception ex)
            {
                return;
            };            
        }
        
        public void OSM2KML()
        {
            // https://github.com/ErshKUS/osmCatalog
            
            if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Hide();
            string dfile = KMZRebuilederForm.CurrentDirectory() + @"\OSM\dictionary.json";
            if (!File.Exists(dfile))
            {
                string[] files = Directory.GetFiles(KMZRebuilederForm.CurrentDirectory() + @"\OSM\","*.json");
                if (files.Length > 0)
                {
                    List<string> fls = new List<string>();
                    fls.Add("NONE");
                    int fIndex = 0;
                    foreach (string ff in files)
                    {
                        string f = Path.GetFileName(ff);
                        if (f == "dictionary.json") fIndex = fls.Count;
                        fls.Add(f);
                    };
                    System.Windows.Forms.InputBox.Show("Import POI from OSM", "Select Dictionary File:", fls.ToArray(), ref fIndex);
                    if (fIndex > 0) dfile = KMZRebuilederForm.CurrentDirectory() + @"\OSM\" + fls[fIndex];
                };
            };
            OSMDictionary osmd = new OSMDictionary();
            if (File.Exists(dfile)) try { osmd = OSMDictionary.ReadFromFile(dfile); }
                catch { };

            string cfile = KMZRebuilederForm.CurrentDirectory() + @"\OSM\catalog.json";            
            if (!File.Exists(cfile))
            {
                string[] files = Directory.GetFiles(KMZRebuilederForm.CurrentDirectory() + @"\OSM\", "*.json");
                if (files.Length > 0)
                {
                    List<string> fls = new List<string>();
                    fls.Add("NONE");
                    int fIndex = 0;
                    foreach (string ff in files)
                    {
                        string f = Path.GetFileName(ff);
                        if (f == "catalog.json") fIndex = fls.Count;
                        fls.Add(f);
                    };
                    System.Windows.Forms.InputBox.Show("Import POI from OSM", "Select Catalog File:", fls.ToArray(), ref fIndex);
                    if (fIndex > 0) cfile = KMZRebuilederForm.CurrentDirectory() + @"\OSM\" + fls[fIndex];
                };
            };
            OSMCatalog cat = new OSMCatalog();
            if (File.Exists(cfile)) try { cat = OSMCatalog.ReadFromFile(cfile); }
                catch { };

            string zipFile = KMZRebuilederForm.CurrentDirectory() + @"\MapIcons\poi_marker.zip";
            if(!File.Exists(zipFile))
            {
                string[] files = Directory.GetFiles(KMZRebuilederForm.CurrentDirectory() + @"\MapIcons\", "*.zip");
                if (files.Length > 0)
                {
                    List<string> fls = new List<string>();
                    fls.Add("NONE");
                    int fIndex = 0;
                    foreach (string ff in files)
                    {
                        string f = Path.GetFileName(ff);
                        if (f == "poi_marker.zip") fIndex = fls.Count;
                        fls.Add(f);
                    };
                    System.Windows.Forms.InputBox.Show("Import POI from OSM", "Select Icon File:", fls.ToArray(), ref fIndex);
                    if (fIndex > 0) zipFile = KMZRebuilederForm.CurrentDirectory() + @"\MapIcons\" + fls[fIndex];
                };
            };            

            int cat_type = 0;
            if (cat.Count > 0)
            {
                cat_type = 3;
                string[] spi = new string[] { "No split data into Layers", "Split into Layers by Tags Value", "Split into Layers by Tags Key", "Split into Layers by Categories", "Split into Layers by Parent Categories", "Split into Layers by Top Parent Categories" };                
                System.Windows.Forms.InputBox.Show("Import POI from OSM", "Select how to split data:", spi, ref cat_type);                
            };

            if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Show("Import From OSM", "Wait, loading xml...");
            FileStream fs;                        
            XmlDocument osm = new XmlDocument();
            osm.Load(this.src_file_pth);            
            
            fs = new FileStream(this.tmp_file_dir + "doc.kml", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml>");
            sw.WriteLine("\t<Document>");
            sw.WriteLine("\t<name>OSM DATA</name>");

            List<KeyValuePair<string, List<XMLNodeWithIcon>>> byCategory = new List<KeyValuePair<string, List<XMLNodeWithIcon>>>();
            byCategory.Add(new KeyValuePair<string, List<XMLNodeWithIcon>>(cat_type == 0 ? "OSM DATA" : "No Category", new List<XMLNodeWithIcon>()));
            List<string> byIcon = new List<string>();
            byIcon.Add("Unknown");

            if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Show("Import From OSM", "Wait, parsing data...");
            XmlNodeList nodeslist = osm.SelectNodes("osm/node/tag[@k='name']");
            int n_i = 0;
            foreach (XmlNode node in nodeslist)
            {
                if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Text = String.Format(System.Globalization.CultureInfo.InvariantCulture,"Wait, parsing data {0:P}", ((float)n_i) / ((float)nodeslist.Count));
                n_i++;
                string catName = cat_type == 0 ? "OSM DATA" : "No Category";
                string iconName = "Unknown";
                if (cat.Count > 0)
                    for (int i = 0; i < cat.Count; i++)
                    {
                        XmlNode tn = null;
                        foreach (KeyValuePair<string, string> kvp in cat[i].tags)
                        {
                            tn = node.ParentNode.SelectSingleNode("tag[@k='" + kvp.Key + "']");
                            if ((tn != null) && (tn.Attributes["v"].Value == kvp.Value))
                            {
                                iconName = cat[i].name;
                                if (cat_type == 1)
                                    catName = kvp.Key + ": " + kvp.Value;
                                if (cat_type == 2)
                                    catName = kvp.Key;
                                if (cat_type == 3)
                                    catName = osmd.Translate(cat[i].name);
                                if (cat_type == 4)
                                {
                                    catName = osmd.Translate(cat[i].name);
                                    if (cat[i].parent.Length > 0)
                                        catName = osmd.Translate(cat[i].parent[0]);
                                };
                                if (cat_type == 5)
                                {
                                    catName = osmd.Translate(cat[i].name);
                                    catName = osmd.Translate(cat.GetTopParentCategory(cat[i].name));
                                };
                            };
                        };
                    };
                if (iconName.Length == 0)
                    iconName = "Unknown";
                else
                    for (int l = 0; l < iconName.Length; l++)
                        if ((!char.IsLetter(iconName[l])) && (!char.IsNumber(iconName[l])) && (iconName[l] != '_'))
                            iconName = iconName.Replace(iconName[l], '_');

                XMLNodeWithIcon nodi = new XMLNodeWithIcon();
                nodi.node = node;
                nodi.icon = byIcon.IndexOf(iconName);
                if (nodi.icon < 0) { nodi.icon = byIcon.Count; byIcon.Add(iconName); };

                int nodt = -1;
                if (byCategory.Count > 0) for (int i = 0; i < byCategory.Count; i++) if (byCategory[i].Key == catName) nodt = i;
                if (nodt < 0)
                {
                    nodt = byCategory.Count;
                    byCategory.Add(new KeyValuePair<string, List<XMLNodeWithIcon>>(catName, new List<XMLNodeWithIcon>()));
                };
                byCategory[nodt].Value.Add(nodi);
            };

            if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Show("Import From OSM", "Wait, saving data...");

            n_i = 0;
            for (int i = 0; i < byCategory.Count; i++)
            {
                if (byCategory[i].Value.Count == 0) continue;
                sw.WriteLine("\t\t<Folder>");
                sw.WriteLine("\t\t<name><![CDATA[" + byCategory[i].Key + "]]></name>");
                foreach (XMLNodeWithIcon nodi in byCategory[i].Value)
                {
                    if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Text = String.Format(System.Globalization.CultureInfo.InvariantCulture, "Wait, saving data {0:P}", ((float)n_i) / ((float)nodeslist.Count));
                    n_i++;
                    XmlNode node = nodi.node;
                    string nam = node.Attributes["v"].Value;
                    string desc = "";
                    if (node.ParentNode.SelectSingleNode("tag[@k='description']") != null)
                        desc = node.ParentNode.SelectSingleNode("tag[@k='description']").Attributes["v"].Value;
                    string xyz = node.ParentNode.Attributes["lon"].Value + "," + node.ParentNode.Attributes["lat"].Value + ",0";
                    try { nam = node.SelectSingleNode("name").ChildNodes[0].Value; }
                    catch { };
                    if (desc.Length > 0) desc += "\r\n";
                    foreach (XmlNode tag in node.ParentNode.SelectNodes("tag"))
                        desc += tag.Attributes["k"].Value + "=" + tag.Attributes["v"].Value + "\r\n";
                    sw.WriteLine("\t\t\t<Placemark>");
                    sw.WriteLine("\t\t\t\t<name><![CDATA[" + nam + "]]></name>");
                    sw.WriteLine("\t\t\t\t<description><![CDATA[" + desc + "]]></description>");
                    sw.WriteLine("\t\t\t\t<styleUrl>#icon" + nodi.icon.ToString() + "</styleUrl>");
                    sw.WriteLine("\t\t\t\t<Point><coordinates>" + xyz + "</coordinates></Point>");
                    sw.WriteLine("\t\t\t</Placemark>");
                };
                sw.WriteLine("\t\t</Folder>");
            };

            if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Show("Import From OSM", "Wait, prepare icons...");

            for (int i = 0; i < byIcon.Count; i++)
            {
                if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Text = String.Format(System.Globalization.CultureInfo.InvariantCulture, "Wait, prepare icons {0:P}", ((float)i) / ((float)byIcon.Count));
                bool saved_from_zip = false;
                if (File.Exists(zipFile))
                {
                    try
                    {
                        MapIcons.SaveIcon(GetFileFromZip(zipFile, byIcon[i] + ".png"), this.tmp_file_dir + @"\images\" + byIcon[i] + ".png");
                        saved_from_zip = true;
                    }
                    catch { };
                };
                if (!saved_from_zip)
                {
                    Bitmap bmp = new Bitmap(32, 32);
                    Graphics g = Graphics.FromImage(bmp);
                    g.FillEllipse(Brushes.LightBlue, 0, 0, 32, 32);
                    g.DrawEllipse(new Pen(Color.Black, 2), 0, 0, 31, 31);
                    SizeF ms = g.MeasureString(i.ToString(), new Font("Arial", 11, FontStyle.Bold));
                    g.DrawString(i.ToString(), new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 16 - ms.Width / 2, 16 - ms.Height / 2);
                    g.Dispose();
                    string fName = this.tmp_file_dir + @"\images\" + byIcon[i] + ".png";
                    try
                    {
                        bmp.Save(fName, ImageFormat.Png);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            ImageMagick.MagickImage mi = new ImageMagick.MagickImage(bmp);
                            FileStream sfs = new FileStream(fName, FileMode.Create, FileAccess.Write);
                            mi.Write(sfs, ImageMagick.MagickFormat.Png);
                            sfs.Close();
                        }
                        catch (Exception subex)
                        {

                        };
                    };

                    bmp.Dispose();
                };
                sw.WriteLine("\t<Style id=\"icon" + i.ToString() + "\"><IconStyle><Icon><href>images/" + byIcon[i] + ".png</href></Icon></IconStyle></Style>");
            };

            //sw.WriteLine("\t<Style id=\"noicon\"><IconStyle><Icon><href>images/noicon.png</href></Icon></IconStyle></Style>");
            sw.WriteLine("\t</Document>");
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();

            File.Copy(KMZRebuilederForm.CurrentDirectory() + @"KMZRebuilder.noi.png", this.tmp_file_dir + @"images\noicon.png", true);
        }

        private struct XMLNodeWithIcon
        {
            public int icon;
            public System.Xml.XmlNode node;
        }

        public void LoadKML(bool updateLayers)
        {
            if (!this.Valid) return;

            this.kmlDoc = new XmlDocument();
            try
            {
                this.kmlDoc.Load(this.tmp_file_dir + "doc.kml");
                src_file_bad = false;
            }
            catch (Exception ex)
            {
                src_file_bad = true;
                if (KMZRebuilederForm.waitBox != null) KMZRebuilederForm.waitBox.Hide();
                if (MessageBox.Show("Couldn't open XML File `"+this.src_file_nme+"`!\r\n\r\nError: " + ex.Message.ToString(), "Error open XML File", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                {
                    if (this.parseError)
                        return;
                    else
                    {
                        this.parseError = true;
                        this.src_file_bad = false;
                        this.PrepareNormalKML();
                        this.LoadKML(updateLayers);
                    };
                };
                return;
            };

            this.GetSubISMDirs();
            if (updateLayers) this.ParseLoadedKML();
        }

        public void SaveKML()
        {
            if (!this.Valid) return;
            this.kmlDoc.Save(this.tmp_file_dir + "doc.kml");
        }

        public void GetSubISMDirs()
        {
            string path;
            path = Path.Combine(this.tmp_file_dir, "images");
            HasISMDirs[0] = Directory.Exists(path);
            path = Path.Combine(this.tmp_file_dir, "sounds");
            HasISMDirs[1] = Directory.Exists(path);
            path = Path.Combine(this.tmp_file_dir, "media");
            HasISMDirs[2] = Directory.Exists(path);
        }

        private void ParseLoadedKML()
        {
            // get doc name
            {
                XmlNode xn;
                xn = kmlDoc.SelectSingleNode("kml/Document/name");
                try { if (xn != null) kmldocName = xn.ChildNodes[0].Value; }
                catch { };
                xn = kmlDoc.SelectSingleNode("kml/Document/description");
                try { if (xn != null) kmldocDesc = xn.ChildNodes[0].Value; }
                catch { };
            };

            // get layers
            this.kmLayers.Clear();
            {
                int l = 0;
                foreach (XmlNode xn in kmlDoc.SelectNodes("kml/Document/Folder"))
                {
                    XmlNode xn2 = xn.SelectSingleNode("name");
                    string lname = "";
                    try { if (xn2 != null) lname = xn2.ChildNodes[0].Value; }
                    catch { };
                    XmlNodeList nl2 = xn.SelectNodes("Placemark");
                    int p = (nl2 == null ? 0 : nl2.Count);
                    int pp = xn.SelectNodes("Placemark/Point").Count;
                    int pl = xn.SelectNodes("Placemark/LineString").Count;
                    int pa = xn.SelectNodes("Placemark/Polygon").Count;

                    bool hd = false;
                    try { hd = !String.IsNullOrEmpty(xn.SelectSingleNode("description").ChildNodes[0].Value); }
                    catch { };                    

                    this.kmLayers.Add(new KMLayer(l++, lname, p, pp, pl, pa, hd, this));
                };
            };            
        }

        public class XMLException : Exception
        {

        }

        private void PrepareNormalKML()
        {
            FileStream fs = new FileStream(this.tmp_file_dir + "doc.kml", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            string xml = sr.ReadToEnd();
            Regex rx = new Regex("&(?!amp;)");
            xml = rx.Replace(xml, "&amp;");
            sr.Close();
            fs.Close();

            xml = RemoveXMLNamespaces(xml);
            if (this.parseError) // Try to reopen bad file
            {
                for (int i = xml.Length - 1; i >= 0; i--)
                    if (((int)xml[i]) < 0x20)
                    {
                        char c = xml[i];
                        if (c == '\r') continue;
                        if (c == '\n') continue;
                        if (c == '\t') continue;
                        xml = xml.Remove(i, 1).Insert(i, " ");
                    };
            };
            this.kmlDoc = new XmlDocument();
            try
            {
                this.kmlDoc.LoadXml(xml);
            }
            catch (Exception ex)
            {
                return;
            };

            // 

            // Вытягиваем все вложеные папки (Folder) в документ (kml/Document)
            {
                XmlNode doc = kmlDoc.SelectSingleNode("kml/Document");
                XmlNode lp = null;
                XmlNode ln = null;
                XmlNode n;
                while ((n = kmlDoc.SelectSingleNode("kml/Document/Folder/Folder")) != null)
                {
                    XmlNode p = n.ParentNode;
                    p.RemoveChild(n);

                    if (lp != p)
                        doc.InsertAfter(n, p);
                    else
                        doc.InsertAfter(n, ln);

                    lp = p;
                    ln = n;
                };
            };

            // if no folders in kml
            {
                XmlNode doc = kmlDoc.SelectSingleNode("kml/Document/Folder");
                if (doc == null)
                {
                    doc = kmlDoc.SelectSingleNode("kml/Document"); // No Folders
                    if (doc == null)
                    {
                        doc = kmlDoc.SelectSingleNode("kml"); // No Document
                        if (doc != null)
                        {
                            doc.InnerXml = "<Document>" + doc.InnerXml + "</Document>";
                            doc = kmlDoc.SelectSingleNode("kml/Document/Folder");
                            if (doc == null)
                                doc = kmlDoc.SelectSingleNode("kml/Document");
                        };
                    }
                    else
                    {
                        string txt = doc.InnerXml;
                        XmlNode ns = kmlDoc.CreateElement("Folder");
                        ns.InnerXml = txt;
                        doc.AppendChild(ns);
                    };
                };
            };

            //вытягиваем все объекты без папки в папку
            {
                XmlNode doc = kmlDoc.SelectSingleNode("kml/Document");
                XmlNodeList nl = kmlDoc.SelectNodes("kml/Document/Placemark");
                if (nl.Count > 0)
                {
                    XmlNode ns = kmlDoc.CreateElement("Folder");
                    XmlNode nn = kmlDoc.CreateElement("name");
                    nn.AppendChild(kmlDoc.CreateTextNode("No in folder [" + nl.Count.ToString() + "]"));
                    ns.AppendChild(nn);
                    doc.AppendChild(ns);
                    foreach (XmlNode n in nl)
                    {
                        doc.RemoveChild(n);
                        ns.AppendChild(n);
                    };
                };
            }

            // move "Placemark/MultiGeometry/LineString" -> "Placemark/LineString"
            {
                foreach (XmlNode xn in kmlDoc.SelectNodes("kml/Document/Folder/Placemark/MultiGeometry/LineString"))
                {
                    XmlNode xnp = xn.ParentNode.ParentNode;
                    XmlNode xnm = xn.ParentNode;
                    xnm.RemoveChild(xn);
                    xnp.RemoveChild(xnm);
                    xnp.AppendChild(xn);
                };
            };

            // move styles from kml/Document/Folder to kml/Document/styleUrl
            // move styleUrls from kml/Document/Folder/styleUrl to kml/Document/styleUrl
            {
                //styles
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                Random random = (new Random());
                string pref = new String(new char[] { chars[random.Next(chars.Length)], chars[random.Next(chars.Length)], chars[random.Next(chars.Length)] });

                XmlNodeList nl;
                List<string> sl = new List<string>();
                nl = kmlDoc.SelectNodes("kml/Document/Folder/Placemark/Style");
                foreach (XmlNode n in nl)
                {
                    string ort = n.InnerXml;
                    int index = sl.IndexOf(ort);
                    if (index < 0)
                    {
                        sl.Add(ort);
                        index = sl.Count - 1;
                        XmlNode ns = kmlDoc.CreateElement("Style");
                        ns.Attributes.Append(kmlDoc.CreateAttribute("id"));
                        ns.Attributes["id"].Value = "style" + pref + index.ToString();
                        ns.InnerXml = ort;
                        n.ParentNode.ParentNode.ParentNode.AppendChild(ns);
                    };
                    XmlNode n2 = kmlDoc.CreateElement("styleUrl");
                    n2.AppendChild(kmlDoc.CreateTextNode("#style" + pref + index.ToString()));
                    n.ParentNode.ReplaceChild(n2, n);
                };

                //Style
                sl.Clear();
                nl = kmlDoc.SelectNodes("kml/Document/Folder/Style");
                foreach (XmlNode n in nl)
                {
                    XmlNode p = n.ParentNode.ParentNode;
                    n.ParentNode.RemoveChild(n);
                    string ort = n.OuterXml;
                    if (sl.IndexOf(ort) < 0)
                    {
                        sl.Add(ort);
                        p.AppendChild(n);
                    };
                };

                //styleMaps
                sl.Clear();
                nl = kmlDoc.SelectNodes("kml/Document/Folder/StyleMap");
                foreach (XmlNode n in nl)
                {
                    XmlNode p = n.ParentNode.ParentNode;
                    n.ParentNode.RemoveChild(n);
                    string ort = n.OuterXml;
                    if (sl.IndexOf(ort) < 0)
                    {
                        sl.Add(ort);
                        p.AppendChild(n);
                    };
                };
            };

            // remove not used styles
            {
                XmlNodeList sex = kmlDoc.SelectNodes("kml/Document/Folder/Placemark/styleUrl");
                XmlNodeList sml = kmlDoc.SelectNodes("kml/Document/StyleMap");
                XmlNodeList ssl = kmlDoc.SelectNodes("kml/Document/Style");

                List<string> sl = new List<string>();
                foreach (XmlNode n in sml)
                {
                    string sn = "#" + n.Attributes["id"].Value;
                    bool ex = false;
                    foreach (XmlNode scu in sex)
                        if (scu.ChildNodes[0].Value == sn)
                            ex = true;
                    if (!ex)
                    {
                        n.ParentNode.RemoveChild(n);
                        foreach (XmlNode xn2 in n.SelectNodes("Pair/styleUrl"))
                        {
                            string su = xn2.ChildNodes[0].Value;
                            if (su.IndexOf("#") == 0) su = su.Remove(0, 1);
                            sl.Add(su);
                        };
                    };
                };
                foreach (XmlNode n in ssl)
                {
                    if (n.Attributes["id"] == null)
                        n.ParentNode.RemoveChild(n);
                    else
                    {
                        string sn = n.Attributes["id"].Value;
                        bool ex = false;
                        foreach (XmlNode scu in sml)
                            if (n.Attributes["id"].Value == sn)
                                ex = true;
                        string snh = "#" + sn;
                        if (!ex)
                            foreach (XmlNode scu in sex)
                                if (scu.ChildNodes[0].Value == snh)
                                    ex = true;
                        if (!ex)
                            sl.Add(sn);
                    };
                };
                if (sl.Count > 0)
                    foreach (string ss in sl)
                    {
                        XmlNode n = kmlDoc.SelectSingleNode("kml/Document/Style[@id='" + ss + "']");
                        n.ParentNode.RemoveChild(n);
                    };
            };
            
            // get no icons and no name placemarks
            int noicons = 0;
            this.kmLayers.Clear();
            {
                foreach (XmlNode pm in kmlDoc.SelectNodes("kml/Document/Folder/Placemark"))
                {
                    if (pm.SelectSingleNode("name") == null)
                    {
                        XmlNode nn = kmlDoc.CreateElement("name");
                        nn.AppendChild(kmlDoc.CreateTextNode("NoName"));
                        pm.AppendChild(nn);
                    }
                    else if (pm.SelectSingleNode("name").ChildNodes.Count == 0)
                    {
                        XmlNode nn = pm.SelectSingleNode("name");
                        nn.AppendChild(kmlDoc.CreateTextNode("NoName"));
                        pm.AppendChild(nn);
                    };

                    if (pm.SelectSingleNode("Point") == null) continue;

                    XmlNode su = pm.SelectSingleNode("styleUrl");
                    if (su != null)
                    {
                        string sut = su.ChildNodes[0].Value.ToLower();
                        if (sut.IndexOf("root:") == 0)
                        {
                            su.ParentNode.RemoveChild(su);
                            su = null;
                        };
                    };

                    if ((pm.SelectSingleNode("Style") == null) && (su == null))
                    {
                        su = kmlDoc.CreateElement("styleUrl");
                        su.AppendChild(kmlDoc.CreateTextNode("#noicon"));
                        XmlNode ncn = pm.SelectSingleNode("descrption");
                        if (ncn == null) ncn = pm.SelectSingleNode("name");
                        if (ncn == null) ncn = pm.ChildNodes[0];
                        pm.InsertAfter(su, ncn);
                        noicons++;
                    };
                };

                if ((noicons > 0) && (kmlDoc.SelectSingleNode("kml/Document/Style[@id='noicon']") == null))
                {
                    XmlNode su = kmlDoc.CreateElement("Style");
                    su.Attributes.Append(kmlDoc.CreateAttribute("id"));
                    su.Attributes["id"].Value = "noicon";
                    su.InnerXml = "<IconStyle><Icon><href>images/noicon.png</href></Icon></IconStyle>";
                    kmlDoc.SelectSingleNode("kml/Document").AppendChild(su);
                };
            };

            // delete multi names
            {
                XmlNodeList nl = kmlDoc.SelectNodes("kml/Document/Folder");
                foreach (XmlNode n in nl)
                {
                    XmlNodeList subl = n.SelectNodes("name");
                    if (subl.Count > 1)
                        for (int i = subl.Count - 1; i > 0; i--)
                            n.RemoveChild(subl[i]);

                    subl = n.SelectNodes("Placemark");
                    foreach (XmlNode nn in subl)
                    {
                        XmlNodeList ssl = nn.SelectNodes("name");
                        if (ssl.Count > 1)
                            for (int i = ssl.Count - 1; i > 0; i--)
                                nn.RemoveChild(ssl[i]);                       
                    };
                };
            };

            // reorder placemark child nodes for OruxMaps
            {
                foreach (XmlNode pm in kmlDoc.SelectNodes("kml/Document/Folder"))
                {
                    XmlNode rn = null;
                    foreach (string nodeName in (new string[] { "description", "name" })) // name, description
                        if ((rn = pm.SelectSingleNode(nodeName)) != null)
                            pm.InsertAfter(pm.RemoveChild(rn), null);
                };
                foreach (XmlNode pm in kmlDoc.SelectNodes("kml/Document/Folder/Placemark"))
                {
                    XmlNode rn = null;
                    foreach (string nodeName in (new string[] { "Style", "styleUrl", "description", "name" })) // name, description, style, coordinates
                        if ((rn = pm.SelectSingleNode(nodeName)) != null)
                            pm.InsertAfter(pm.RemoveChild(rn), null);
                };
            };

            // set empty icons
            {
                foreach (XmlNode xn in kmlDoc.SelectNodes("kml/Document/Style/IconStyle/Icon/href"))
                {
                    string href = xn.ChildNodes[0].Value;
                    if (!Uri.IsWellFormedUriString(href, UriKind.Absolute))
                        if (!File.Exists(this.tmp_file_dir + href))
                        {
                            href = href.Replace("/", @"\");
                            if (href.LastIndexOf(@"\") >= 0)
                                href = @"images\" + href.Substring(href.LastIndexOf(@"\"));
                            else
                                href = @"images\" + href;
                            while (href.IndexOf(@"\\") >= 0) href = href.Replace(@"\\", @"\");
                            xn.ChildNodes[0].Value = href;
                            File.Copy(KMZRebuilederForm.CurrentDirectory() + @"KMZRebuilder.noi.png", this.tmp_file_dir + href);
                        };
                }
            };


            kmlDoc.Save(this.tmp_file_dir + "doc.kml");
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
            {
                string xmlnsPattern = "<gpx[^>]*?>";
                MatchCollection matchCol = Regex.Matches(outerXml, xmlnsPattern);
                foreach (Match match in matchCol)
                    outerXml = outerXml.Replace(match.ToString(), "<gpx>");
            };
            return outerXml;
        }        

        private static void UnZipKMZ(string archiveFilenameIn, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile) continue; // Ignore directories
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    };
                };
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }                       
    }

    public class KMLayer
    {
        public static string digits = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz123456789";

        public int id;
        public string name;
        public KMFile file;        
        public bool ischeck = true;
        public int placemarks = 0;
        public int points = 0;
        public int lines = 0;
        public int areas = 0;
        public bool hasDesc = false;
        public int ATB = -1;

        public KMLayer(int id, string name, int placemarks, int points, int lines, int areas, bool hasDesc, KMFile file)
        {
            this.id = id;
            this.name = name;
            this.placemarks = placemarks;
            this.points = points;
            this.lines = lines;
            this.areas = areas;
            this.ischeck = placemarks > 0;
            this.file = file;
            this.hasDesc = hasDesc;
        }

        public override string ToString()
        {
            return "`" + this.name + "` at " + id.ToString() + " in (" + file.src_file_nme + "); objects found: " + placemarks.ToString() + "(" + points.ToString() + "+" + lines.ToString() + "+" + areas.ToString() + ") placemarks";
        }        
    }

    public class KMStyle
    {
        public string name;
        public KMFile file;
        public string newname;

        public string lcs = "";
        public int lcsIn = 0;
        public Image defaultIcon = null;

        public KMStyle(string name, KMFile file, string newname)
        {
            this.name = name;
            this.file = file;
            this.newname = newname;
        }

        public override string ToString()
        {
            return this.file.tmp_file_dir + @"\" + this.name;
        }
    }

    public class KMIcon
    {
        public string href; 
        public KMFile file;
        public string newhref;

        public List<string> styles = new List<string>();
        public Image image = null;
        public int placemarks = 0;
        public string lcs = null;

        public KMIcon(string href, KMFile file, string newname)
        {
            this.href = href;
            this.file = file;
            this.newhref = newname;
        }

        public KMIcon(string href, KMFile file, string newname, string style)
        {
            this.href = href;
            this.file = file;
            this.newhref = newname;
            this.styles.Add(style);
        }

        public override string ToString()
        {
            return this.file.tmp_file_dir + @"\" + this.href;
        }
    }

    public class DB3CatSorted : IComparer<string[]>
    {
        public int Compare(string[] a, string[] b)
        {
            return a[1].CompareTo(b[1]);
        }
    }

    public class TAGSorter : IComparer<KeyValuePair<string,string>>
    {
        public int Compare(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
        {
            string _a = a.Key + "=" + a.Value;
            string _b = b.Key + "=" + b.Value;
            return _a.CompareTo(_b);
        }  
    }
}