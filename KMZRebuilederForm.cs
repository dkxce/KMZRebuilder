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
        // P/Invoke constants
        private const int WM_SYSCOMMAND = 0x112;
        private const int MF_STRING = 0x0;
        private const int MF_SEPARATOR = 0x800;

        // P/Invoke declarations
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);

        private int SYSMENU_ABOUT_ID = 0x1;

        public string[] args;
        public MruList MyMruList;
        public WaitingBoxForm waitBox;

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

            prepareTranslit();
        }

        private void FormKMZ_Load(object sender, EventArgs e)
        {
            MyMruList = new MruList(CurrentDirectory() + @"\KMZRebuilder.mru", MRU, 10);
            MyMruList.FileSelected += new MruList.FileSelectedEventHandler(MyMruList_FileSelected);
            waitBox = new WaitingBoxForm(this);

            kmzFiles.AllowDrop = true;
            kmzFiles.DragDrop += bgFiles_DragDrop;
            kmzFiles.DragEnter += bgFiles_DragEnter;

            kmzLayers.AllowDrop = true;
            kmzLayers.DragDrop += bgFiles_DragDrop;
            kmzLayers.DragEnter += bgFiles_DragEnter;

            if(Directory.Exists(TempDirectory())) System.IO.Directory.Delete(TempDirectory(),true);
            System.IO.Directory.CreateDirectory(TempDirectory());

            if ((args != null) && (args.Length > 0))
            {
                List<string> files = new List<string>();
                foreach(string arg in args)
                    if(File.Exists(arg))
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
            ofd.Filter = "KML,KMZ,GPX Files (*.kml;*.kmz;*.gpx)|*.kml;*.kmz;*.gpx";
            ofd.DefaultExt = ".kmz";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK) LoadFiles(ofd.FileNames);
            ofd.Dispose();
        }

        private void LoadFiles(string[] files)
        {
            int c = kmzFiles.Items.Count;
            waitBox.Show("Loading", "Wait, loading file(s)...");            
            foreach (string file in files)
            {
                bool skip = false;
                foreach (object oj in kmzFiles.Items)
                {
                    KMFile f = (KMFile)oj;
                    if (f.src_file_pth == file) skip = true;
                };

                if (skip) 
                    continue;
                else
                {
                    KMFile f = new KMFile(file);
                    if (!f.Valid) continue;
                    kmzFiles.Items.Add(f, f.isCheck);
                    MyMruList.AddFile(file);
                    if (outName.Text == String.Empty) outName.Text = f.kmldocName;
                };
            };

            if (c != kmzFiles.Items.Count) ReloadListboxLayers();
            waitBox.Hide();
        }        

        private void ReloadListboxLayers()
        {
            panel1.Visible = false;
            kmzLayers.Height = 347;

            kmzLayers.Items.Clear();

            int p = 0;
            if (kmzFiles.Items.Count == 0)
            {
                label2.Text = "Layers:";
                return;
            };
            for(int i=0;i<kmzFiles.Items.Count;i++)
            {
                KMFile f = (KMFile)kmzFiles.Items[i];
                if (!f.isCheck) continue;

                foreach (KMLayer layer in f.kmLayers)
                {
                    kmzLayers.Items.Add(layer, layer.ischeck);
                    p += layer.placemarks;
                };
            };

            label2.Text = String.Format("{0} placemark(s) in {1} layer(s):", p, kmzLayers.Items.Count);
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
        }

        private void kmzFiles_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            KMFile f = (KMFile)kmzFiles.Items[e.Index];
            f.isCheck = e.NewValue == CheckState.Checked;
            ReloadListboxLayers();
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
            ReloadListboxLayers();
        }

        private void DeleteAll_Click(object sender, EventArgs e)
        {
            kmzFiles.Items.Clear();
            if (Directory.Exists(TempDirectory())) System.IO.Directory.Delete(TempDirectory(), true);
            ReloadListboxLayers();
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
            ReloadListboxLayers();
        }        

        private void Save2KML(string filename, KMLayer kml)
        {
            kmzLayers.Height = 227;
            panel1.Visible = true;
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
            foreach (KMStyle kms in styles)
            {
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

            sw.WriteLine("</Document></kml>");
            sw.Close();
            fs.Close();

            waitBox.Hide();
            AddToLog("Done");
        }

        private void Save2GPX(string filename, KMLayer kml)
        {
            kmzLayers.Height = 227;
            panel1.Visible = true;
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
                }
                if (xnl.Count > 0)
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
                        foreach (string llze in llza)
                        {
                            string[] llz = llze.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            sw.WriteLine("\t\t<rtept lat=\"" + llz[1] + "\" lon=\"" + llz[0] + "\"/>");
                        };
                        sw.WriteLine("\t</rte>");
                    };
                };
                AddToLog("Saved " + xnp.Count.ToString() + " points and " + xnl.Count.ToString() + " lines");
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
            kmzLayers.Height = 227;
            panel1.Visible = true;
            log.Text = "";
            AddToLog("Creating WPT file for layer: `" + kml.name + "`...");

            waitBox.Show("Saving", "Wait, savining file...");

            XmlNode xn = kml.file.kmlDoc.SelectNodes("kml/Document/Folder")[kml.id];
            XmlNodeList xns = xn.SelectNodes("Placemark/Point/coordinates");
            if (xns.Count > 0)
            {
                AddToLog("Writing points...");
                System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                sw.Write("OziExplorer Waypoint File Version 1.0\r\nWGS 84\r\nCreated by " + this.Text + "\r\nReserved\r\n");
                for (int x = 0; x < xns.Count; x++)
                {
                    string[] llz = xns[x].ChildNodes[0].Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    string name = xns[x].ParentNode.ParentNode.SelectSingleNode("name").ChildNodes[0].Value.Replace(",", ";");
                    sw.WriteLine(String.Format("{0}, {1}, {2}, {3},, 18, 1, 3, 0, 65535,, 0, 0, 0, 0.00000", x + 1, name, "+" + llz[1], "+" + llz[0]));
                };
                sw.Close();
                fs.Close();

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
            try
            {
                base.Refresh();
            }
            catch { };
        }        

        private string Save2KMZ(string filename, bool multilayers)
        {
            kmzLayers.Height = 227;
            panel1.Visible = true;            

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
            foreach (KMStyle kms in styles)
            {
                List<KMStyle> tmps = new List<KMStyle>();
                XmlNode xn = kms.file.kmlDoc.SelectSingleNode("kml/Document/StyleMap[@id='"+kms.name+"']");

                if (xn == null)
                    tmps.Add(kms);
                else
                {
                    xn.Attributes["id"].Value = kms.newname;
                    int cnc = 0;
                    foreach (XmlNode xn2 in xn.SelectNodes("Pair/styleUrl"))
                    {
                        string su = xn2.ChildNodes[0].Value;
                        if(su.IndexOf("#") == 0) su = su.Remove(0,1);
                        string sunn = kms.newname + "-" + cnc.ToString();
                        xn2.ChildNodes[0].Value = "#" + sunn;
                        tmps.Add(new KMStyle(su, kms.file, sunn));
                        cnc++;
                    };
                    sw.WriteLine(xn.OuterXml);
                    style_maps_found++;
                };

                foreach(KMStyle k2 in tmps)
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
                            bool isurl = Uri.IsWellFormedUriString(href,UriKind.Absolute);
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
                                            AddToLog("Error downloading "+k2.newname+" icon at `"+href+"` - "+ex.Message);
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
            AddToLog(String.Format("Found {0} icons, {1} saved, {2} passed, {3} in URLS", icons_found, icons_added, icons_passed, icons_asURL));
            AddToLog(String.Format("Saved {3} placemarks, {0} styles and {1} style maps with {4} icons in {2} layer(s)", style_found, style_maps_found, multilayers ? layers_found : 1, ttlpm, icons_added));
            
            sw.WriteLine("</Document></kml>");
            sw.Close();
            fs.Close();

            AddToLog(String.Format("Creating file: {0}", filename));
            CreateZIP(filename, zdir);
            waitBox.Hide();

            return zdir;
        }

        private void Save2Splitted(string filename, KMLayer layer)
        {
            kmzLayers.Height = 227;
            panel1.Visible = true;
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

        private void Save2GML(string zipfile, KMFile kmfile)
        {
            string proj_name = kmfile.kmldocName;
            if(proj_name == "") 
                proj_name = outName.Text;
            if (InputBox("Creating Garmin XML Project", "Enter GPI Project name:", ref proj_name) != DialogResult.OK)
                return;

            kmzLayers.Height = 227;
            panel1.Visible = true;            

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
                    if (nam != String.Empty)
                    {
                        int sublay = 0;
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
                                            string cat = group_id + "L" + layNo.ToString() + "S" + ics.ToString();
                                            if (categories_added.IndexOf(cat) < 0)
                                            {
                                                sw.WriteLine("\t\t\t<Category>");
                                                sw.WriteLine("\t\t\t\t<ID>" + cat + "</ID>");
                                                sw.WriteLine("\t\t\t\t<Name>");
                                                sw.WriteLine("\t\t\t\t\t<LString lang=\"EN\">" + GetTranslit(nam) + " " + (sublay > 0 ? sublay.ToString() : "") + "</LString>");
                                                sw.WriteLine("\t\t\t\t\t<LString lang=\"RU\">" + nam + " " + (sublay > 0 ? sublay.ToString() : "") + "</LString>");
                                                sw.WriteLine("\t\t\t\t</Name>");
                                                // set style to first in
                                                sw.WriteLine("\t\t\t\t<CustomSymbol>" + icons[ics].styles[0].Replace("-", "A") + "</CustomSymbol>");
                                                sw.WriteLine("\t\t\t</Category>");
                                                categories_added.Add(cat);
                                                sublay++;
                                                cat_created++;
                                            };
                                        };
                            };
                        };
                    };
                    layNo++;
                };
            }
            sw.WriteLine("\t\t</CategoryList>");

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

            AddToLog(String.Format("Found {0} icons in {1} layers, created {2} POIs from {4} placemarks in {3} categories, {5} placemarks skipped", icons.Count, kmfile.kmLayers.Count, poi_added, cat_created, ttpm, ttpm - poi_added));
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
            File.Copy(CurrentDirectory() + @"KMZRebuilder.gpil", zdir + @"license.gpil");

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

        private void AddToLog(string txt)
        {
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
            reloadXMLToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            reloadOriginalFileToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            saveAsKMLToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            saveBtnGML.Enabled = kmzFiles.SelectedIndices.Count > 0;
            setAsOutputKmzFileDocuemntNameToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;
            openSourceFileDirectoryToolStripMenuItem.Enabled = kmzFiles.SelectedIndices.Count > 0;

            reloadOriginalFileToolStripMenuItem.Text = "Reload Original file";
            if (kmzFiles.SelectedIndices.Count == 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            reloadOriginalFileToolStripMenuItem.Text = String.Format("Reload Original (`{0}`) file",f.src_file_nme);
        }

        private void LayersMenu_Opening(object sender, CancelEventArgs e)
        {
            Point point = kmzLayers.PointToClient(Cursor.Position);
            int index = kmzLayers.IndexFromPoint(point);
            kmzLayers.SelectedIndex = index;

            saveURLIcons.Enabled = false;

            renameLayerToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            moveUpToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            moveDownToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveAsKMLToolStripMenuItem1.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveLayerToGPXFileToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveLayerToWPTFileToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            saveLayerToOtherFormatsToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            viewContentToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
            selectAllToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            selectNoneToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            invertSelectionToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            checkOnlyWithPlacemarksToolStripMenuItem.Enabled = kmzLayers.Items.Count > 0;
            splitLayerByIconsToolStripMenuItem.Enabled = kmzLayers.SelectedIndices.Count > 0;
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

        private void OpenMyMaps_Click(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.google.com/mymaps");
        }

        private void SortLayers_Click(object sender, EventArgs e)
        {
            SortLayers(sortByAddingToolStripMenuItem.Checked);
        }        

        private void SortLayers(bool ASC)
        {
            panel1.Visible = false;
            kmzLayers.Height = 347;

            if (ASC)
            {
                kmzLayers.Sorted = true;
                sortByAddingToolStripMenuItem.Checked = false;
                sortASCToolStripMenuItem.Checked = true;

                
            }
            else
            {
                kmzLayers.Sorted = false;
                sortByAddingToolStripMenuItem.Checked = true;
                sortASCToolStripMenuItem.Checked = false;

                ReloadListboxLayers();
            };
        }

        private void MoveLayerUp_Click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndex < 1) return;

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
            try
            {
                System.Diagnostics.Process.Start("notepad++", path);
            }
            catch
            {
                try
                {
                    System.Diagnostics.Process.Start("notepad", path);
                }
                catch { };
            };
        }

        private void ReloadTempKMLFile_Click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            waitBox.Show("Reloading", "Wait, reloading edited `doc.kml` file...");
            f.LoadKML(true);
            ReloadListboxLayers();
            waitBox.Hide();
        }

        private void ReloadOriginalFile_click(object sender, EventArgs e)
        {
            if (kmzFiles.SelectedIndex < 0) return;
            KMFile f = (KMFile)kmzFiles.SelectedItem;
            waitBox.Show("Reloading", "Wait, reloading original `" + f.src_file_nme + "` file...");
            f.CopySrcFileToTempDirAndLoad();
            ReloadListboxLayers();
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
                kmzLayers.Height = 227;
                panel1.Visible = true;
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
            sfd.FileName = l.name+".kml";
            if (sfd.ShowDialog() == DialogResult.OK)
                Save2KML(sfd.FileName, l);
            sfd.Dispose();
        }

        private void OpenSAS_click(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"C:\Program Files\SASPlanet\SASPlanet.exe"))
                System.Diagnostics.Process.Start(@"C:\Program Files\SASPlanet\SASPlanet.exe");
            else
                System.Diagnostics.Process.Start("http://www.sasgis.org/sasplaneta/");            
        }

        private void FormKMZ_FormClosed(object sender, FormClosedEventArgs e)
        {
            waitBox.Hide();
            if (Directory.Exists(TempDirectory())) System.IO.Directory.Delete(TempDirectory(), true);
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
            sfd.FileName = l.name + ".gpx";
            if (sfd.ShowDialog() == DialogResult.OK)
                Save2GPX(sfd.FileName, l);
            sfd.Dispose();
        }

        private void saveWPT_click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "OziExplorer Waypoint Files (*.wpt)|*.wpt";
            sfd.DefaultExt = ".wpt";
            sfd.FileName = l.name + ".wpt";
            if (sfd.ShowDialog() == DialogResult.OK)
                Save2WPT(sfd.FileName, l);
            sfd.Dispose();
        }

        private void saveITNConverter_click(object sender, EventArgs e)
        {
            if (kmzLayers.SelectedIndices.Count == 0) return;
            KMLayer l = (KMLayer)kmzLayers.SelectedItem;
            string tmpfn = l.file.tmp_file_dir + "temp.kml";

            Save2KML(tmpfn, l);
            AddToLog("Loafing file: `" + tmpfn + "` in ITN Converter");
            System.Diagnostics.Process.Start(CurrentDirectory() + @"ITNConv.exe", tmpfn);
            AddToLog("Done");
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
                Save2Splitted(sfd.FileName, l);
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
            saveBTNG.Enabled = saveBtnKMZO.Enabled = saveBtnKMZM.Enabled = kmzLayers.CheckedItems.Count > 0;
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
            kmzLayers.Height = 347;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Get a handle to a copy of this form's system (window) menu
            IntPtr hSysMenu = GetSystemMenu(this.Handle, false);
            AppendMenu(hSysMenu, MF_SEPARATOR, 0, string.Empty);
            AppendMenu(hSysMenu, MF_STRING, SYSMENU_ABOUT_ID, "&About…");
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == SYSMENU_ABOUT_ID))
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                string text = fvi.ProductName + " " + fvi.FileVersion + " by " + fvi.CompanyName + "\r\n";
                text += fvi.LegalCopyright;
                MessageBox.Show(text, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            System.Diagnostics.Process.Start(CurrentDirectory() + @"ITNConv.exe");
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
    }

    public class KMFile
    {
        public string src_file_pth;
        public string src_file_nme;
        public string src_file_ext;
        public string tmp_file_dir = "";        

        public XmlDocument kmlDoc;
        public string kmldocName = "";
        public string kmldocDesc = "";        
        public List<KMLayer> kmLayers = new List<KMLayer>();

        public bool isCheck = true;
                
        public bool Valid
        {
            get
            {
                return (src_file_ext == ".kmz") || (src_file_ext == ".kml") || (src_file_ext == ".gpx"); 
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

        public override string ToString()
        {
            return this.src_file_nme + (this.kmldocName != String.Empty ? " [" + this.kmldocName + "]" : "");
        }
        
        public void CopySrcFileToTempDirAndLoad()
        {
            if (!this.Valid) return;

            if (!Directory.Exists(this.tmp_file_dir)) System.IO.Directory.CreateDirectory(this.tmp_file_dir);
            if (!Directory.Exists(this.tmp_file_dir + @"images\")) System.IO.Directory.CreateDirectory(this.tmp_file_dir + @"images\");

            if (this.src_file_ext == ".kml")
            {
                System.IO.File.Copy(this.src_file_pth, this.tmp_file_dir + "doc.kml", true);
                this.PrepareNormalKML();
            }
            else if (this.src_file_ext == ".gpx")
            {
                System.IO.File.Copy(this.src_file_pth, this.tmp_file_dir + "src.gpx", true);
                GPX2KML("NoName");
            }
            else
            {
                UnZipKMZ(this.src_file_pth, this.tmp_file_dir);
                this.PrepareNormalKML();
            };
            
            this.LoadKML(true);
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
                sw.WriteLine("\t\t\t\t<name>"+nam+"</name>");
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
            sw.WriteLine("\t\t</Folder>");
            sw.WriteLine("\t<Style id=\"noicon\"><IconStyle><Icon><href>images/noicon.png</href></Icon></IconStyle></Style>");
            sw.WriteLine("\t</Document>");                        
            sw.WriteLine("</kml>");
            sw.Close();
            fs.Close();

            File.Copy(KMZRebuilederForm.CurrentDirectory() + @"KMZRebuilder.gpx.png", this.tmp_file_dir + @"images\noicon.png");
        }

        public void LoadKML(bool updateLayers)
        {
            if (!this.Valid) return;

            this.kmlDoc = new XmlDocument();
            this.kmlDoc.Load(this.tmp_file_dir + "doc.kml");

            if (updateLayers) this.ParseLoadedKML();
        }

        public void SaveKML()
        {
            if (!this.Valid) return;
            this.kmlDoc.Save(this.tmp_file_dir + "doc.kml");
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
                    this.kmLayers.Add(new KMLayer(l++, lname, p, this));
                };
            };            
        }

        private void PrepareNormalKML()
        {
            FileStream fs = new FileStream(this.tmp_file_dir + "doc.kml", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            string xml = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            xml = RemoveXMLNamespaces(xml);
            this.kmlDoc = new XmlDocument();
            this.kmlDoc.LoadXml(xml);

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
                    doc = kmlDoc.SelectSingleNode("kml/Document");
                    string txt = doc.InnerXml;
                    XmlNode ns = kmlDoc.CreateElement("Folder");
                    ns.InnerXml = txt;
                    doc.AppendChild(ns);
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
                    string sn = "#"+n.Attributes["id"].Value;
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

            // delete multi names, move style before coord
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

                        XmlNode scn = nn.SelectSingleNode("styleUrl");
                        XmlNode ncn = nn.SelectSingleNode("descrption");
                        if (ncn == null) ncn = nn.SelectSingleNode("name");
                        if (ncn == null) ncn = nn.ChildNodes[0];
                        if ((scn != null) && (ncn != scn))
                        {
                            nn.RemoveChild(scn);
                            nn.InsertAfter(scn, ncn);
                        };
                    };
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
        public int id;
        public string name;
        public KMFile file;        
        public bool ischeck = true;
        public int placemarks = 0;

        public KMLayer(int id, string name, int placemarks, KMFile file)
        {
            this.id = id;
            this.name = name;
            this.placemarks = placemarks;
            this.ischeck = placemarks > 0;
            this.file = file;
        }

        public override string ToString()
        {
            return "`"+this.name + "` at "+id.ToString()+" in (" + file.src_file_nme+"); objects found: "+placemarks.ToString()+" placemarks";
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
}