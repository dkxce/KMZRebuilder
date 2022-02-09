using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Windows.Forms;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace KMZRebuilder
{
    public partial class GMLayRenamerForm : Form
    {
        private bool noGML = false;
        public bool noGPI = true;
        public bool isCRC = false;
        private string filename = "";
        private string xml = "";
        private XmlDocument xd = null;
        public List<Category> categories = new List<Category>();
        private List<Symbol> symbols = new List<Symbol>();
        
        public GMLayRenamerForm(string filename)
        {            
            InitializeComponent();
            this.button2.Visible = false;
            this.button3.Visible = false;
            this.subtext.Visible = true;

            this.filename = filename;
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            this.xml = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            this.xml = KMFile.RemoveXMLNamespaces(this.xml);
            this.xd = new XmlDocument();
            this.xd.LoadXml(xml);
                        
            int no = 0;
            foreach (XmlNode xn in xd.SelectNodes("GPI/Group/CategoryList/Category"))
            {
                string id = xn.SelectSingleNode("ID").ChildNodes[0].Value;
                string CustomSymbol = xn.SelectSingleNode("CustomSymbol").ChildNodes[0].Value;
                string Name = xn.SelectSingleNode("Name/LString[@lang='RU']").ChildNodes[0].Value;
                categories.Add(new Category(id, CustomSymbol, Name));
                layers.Items.Add(String.Format("{0}: {1}", no++, Name), CustomSymbol);
            };

            string path = System.IO.Path.GetDirectoryName(filename) + @"\";
            foreach (XmlNode xn in xd.SelectNodes("GPI/Group/SymbolList/Symbol"))
            {
                string id = xn.SelectSingleNode("ID").ChildNodes[0].Value;
                string file = path + xn.SelectSingleNode("File").ChildNodes[0].Value;
                symbols.Add(new Symbol(id, file));
                Image im = Image.FromFile(file);
                images.Images.Add(id, (Image)(new Bitmap(im)));
                im.Dispose();
            };
        }

        public GMLayRenamerForm()
        {
            noGML = true;
            InitializeComponent();
            layers.ContextMenuStrip = null;            
        }

        public static GMLayRenamerForm FromGPIGPX(Dictionary<string,string> files, string dir)
        {
            GMLayRenamerForm res = new GMLayRenamerForm();
            res.layers.ContextMenuStrip = res.contextMenuStrip1;
            res.loadImagesToolStripMenuItem.Visible = false;
            res.saveImagesToolStripMenuItem.Visible = false;
            res.layers.FullRowSelect = true;
            res.button2.Visible = false;
            res.button3.Visible = false;
            res.subtext.Visible = true;
            int no = 0;
            foreach (KeyValuePair<string, string> kvp in files)
            {
                res.categories.Add(new Category(kvp.Key, kvp.Key, kvp.Value));
                res.layers.Items.Add(String.Format("{0}: {1}", no++, kvp.Value), kvp.Key);
                res.symbols.Add(new Symbol(kvp.Key, kvp.Value));

                Image im = Image.FromFile(dir + kvp.Key + ".bmp");
                res.images.Images.Add(kvp.Key, (Image)(new Bitmap(im)));
                im.Dispose();
            };
            res.noGPI = false;
            return res;
        }

        public static GMLayRenamerForm ForCRCCheckSum(string[] files, Dictionary<string, string> d4crc_names, Dictionary<string, string> d4crc_subnames)
        {
            GMLayRenamerForm res = new GMLayRenamerForm();
            res.layers.Columns[0].Width = 450;
            if (res.layers.Columns.Count == 1)
                res.layers.Columns.Add(new ColumnHeader());
            res.layers.Columns[1].Width = 100;
            res.layers.FullRowSelect = true;
            res.Text = "GPI Layers Names Editor by Images CRC";
            res.label1.Text = "You can enter names for images styles here (gpi_name_*):";
            res.button2.Text = "Save to ...";
            res.button3.Text = "Load from ...";
            res.subtext.Visible = true;
            res.layers.ContextMenuStrip = res.contextMenuStrip2;
            CRC32 crc = new CRC32();
            foreach (string file in files)
            {
                string sn = Path.GetFileNameWithoutExtension(file);
                string cc = crc.CRC32Num(file).ToString();

                bool skip = false;
                foreach (Category cat in res.categories)
                    if (cat.ID == cc) skip = true;
                if (skip)
                    continue;

                bool chd = false;
                string nm = "";
                if ((d4crc_names != null) && (d4crc_names.ContainsKey(cc)))
                {
                    nm = d4crc_names[cc];
                    chd = false;
                };
                if ((d4crc_subnames != null) && (d4crc_subnames.ContainsKey(cc)))
                {
                    nm = d4crc_subnames[cc];
                    chd = true;
                };

                res.categories.Add(new Category(cc, sn, nm));
                ListViewItem lvi = new ListViewItem(new string[] { nm, chd ? "subname" : "name" }, sn);
                lvi.SubItems[0].BackColor = chd ? Color.LightPink : res.layers.BackColor;
                res.layers.Items.Add(lvi);
                res.symbols.Add(new Symbol(sn, sn));

                Image im = Image.FromFile(file);
                res.images.Images.Add(sn, (Image)(new Bitmap(im)));
                im.Dispose();
            };
            res.isCRC = true;
            return res;
        }

        private void GMLayRenamer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (noGML) return;

            images.Dispose();
            if (this.DialogResult == DialogResult.OK)
            {
                XmlNodeList nl = xd.SelectNodes("GPI/Group/CategoryList/Category");
                for (int i = 0; i < nl.Count;i++)
                {
                    XmlNode xn = nl[i];
                    xn.SelectSingleNode("Name/LString[@lang='RU']").ChildNodes[0].Value = categories[i].name;
                    xn.SelectSingleNode("Name/LString[@lang='EN']").ChildNodes[0].Value = KMZRebuilederForm.GetTranslit(categories[i].name);
                };
                XmlNode n = xd.SelectSingleNode("GPI");
                FileStream fs = new FileStream(this.filename, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sw.WriteLine("<GPI xmlns=\"http://www.garmin.com\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.garmin.com GPI_XML.xsd\">");
                sw.WriteLine(n.InnerXml);
                sw.WriteLine("</GPI>");
                sw.Close();
                fs.Close();
            };
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            changeBitmapToolStripMenuItem.Enabled = renameToolStripMenuItem.Enabled = layers.SelectedIndices.Count > 0;
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isCRC)
            {
                if (layers.SelectedIndices.Count == 0) return;
                string nme = categories[layers.SelectedIndices[0]].name;
                if (KMZRebuilederForm.InputBox("Layer name", "Change layer name:", ref nme, (Bitmap)images.Images[layers.Items[layers.SelectedIndices[0]].ImageKey]) != DialogResult.OK)
                    return;
                categories[layers.SelectedIndices[0]].name = nme;
                layers.Items[layers.SelectedIndices[0]].Text = nme;
            };

            if (noGML && noGPI) return;

            if (layers.SelectedIndices.Count == 0) return;
            string name = categories[layers.SelectedIndices[0]].name;
            if (KMZRebuilederForm.InputBox("Layer name", "Change layer name:", ref name, (Bitmap)images.Images[layers.Items[layers.SelectedIndices[0]].ImageKey]) != DialogResult.OK)
                return;
            if (!noGPI)
            {
                name = name.Trim(Path.GetInvalidFileNameChars()).Trim();
                for (int i = 0; i < categories.Count; i++)
                {
                    if (i == layers.SelectedIndices[0]) continue;
                    if (categories[i].name == name)
                    {
                        MessageBox.Show("Categoty with name `" + name + "` already exists!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    };
                };                
            };
            categories[layers.SelectedIndices[0]].name = name;
            layers.Items[layers.SelectedIndices[0]].Text = String.Format("{0}: {1}", layers.SelectedIndices[0], name);
        }

        private void layers_DoubleClick(object sender, EventArgs e)
        {
            renameToolStripMenuItem_Click(sender, e);
        }

        private void changeBitmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (layers.SelectedIndices.Count == 0) return;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Bitmap files (*.bmp)|*.bmp";
            ofd.DefaultExt = ".bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = new Bitmap(ofd.FileName);
                if ((bmp.Height != 22) || (bmp.Width != 22) || (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed))
                    MessageBox.Show("Error", "Bitmap must be 22x22 pixels 8bpp format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    string img = layers.Items[layers.SelectedIndices[0]].ImageKey;
                    images.Images.RemoveByKey(img);
                    images.Images.Add(img, (Image)new Bitmap(bmp));
                    Symbol sym = null;
                    foreach (Symbol sm in symbols)
                        if (categories[layers.SelectedIndices[0]].CustomSymbol == sm.ID)
                            sym = sm;
                    File.Copy(ofd.FileName, sym.file, true);
                };
                bmp.Dispose();
            };
            ofd.Dispose();
        }

        private void saveListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text files (*.txt)|*.txt";
            sfd.DefaultExt = ".txt";
            sfd.FileName = "layernames.txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                for (int i = 0; i < categories.Count; i++)
                    sw.WriteLine(categories[i].name);
                sw.Close();
                fs.Close();
                MessageBox.Show(String.Format("{0} layers names saved", categories.Count), "Save layers names", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            sfd.Dispose();
        }

        private void loadNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text files (*.txt)|*.txt";
            ofd.DefaultExt = ".txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
                List<string> ltmp = new List<string>();
                while (!sr.EndOfStream) ltmp.Add(sr.ReadLine());
                sr.Close();
                fs.Close();

                if (ltmp.Count != categories.Count)
                {
                    MessageBox.Show("In text file count of lines must be " + layers.Items.Count.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    for (int i = 0; i < categories.Count; i++)
                    {
                        categories[i].name = ltmp[i];
                        layers.Items[i].Text = String.Format("{0}: {1}", i, ltmp[i]);
                    };
                    MessageBox.Show(categories.Count.ToString() + " layer names updated!","Load layers names",MessageBoxButtons.OK,MessageBoxIcon.Information);
                };
            };
            ofd.Dispose();
        }

        private void saveImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Zip archives (*.zip)|*.zip";
            sfd.DefaultExt = ".zip";
            sfd.FileName = "layersprofile.zip";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                FileStream fsOut = File.Create(sfd.FileName);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                //names
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms, System.Text.Encoding.GetEncoding(1251));
                for (int i = 0; i < categories.Count; i++)
                    sw.WriteLine(categories[i].name);
                sw.Flush();
                ms.Flush();
                ms.Position = 0;
                ZipEntry newEntry = new ZipEntry("names.txt");
                newEntry.DateTime = DateTime.Now;
                newEntry.Size = ms.Length;
                zipStream.PutNextEntry(newEntry);
                byte[] buffer = new byte[4096];
                StreamUtils.Copy(ms, zipStream, buffer);
                zipStream.CloseEntry();
                sw.Close();
                ms.Close();

                // images
                try
                {
                    for (int i = 0; i < symbols.Count; i++)
                        ZipFile(symbols[i].file, zipStream);
                }
                catch { };
                
                zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                zipStream.Close();

                MessageBox.Show(String.Format("{0} layers names and {1} images saved", categories.Count, symbols.Count), "Save layers names and images", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            sfd.Dispose();
        }

        private void ZipFile(string filename, ZipOutputStream zipStream)
        {
            FileInfo fi = new FileInfo(filename);
            string entryName = fi.Name;
            ZipEntry newEntry = new ZipEntry(entryName);
            newEntry.DateTime = fi.LastWriteTime;
            newEntry.Size = fi.Length;
            zipStream.PutNextEntry(newEntry);
            byte[] buffer = new byte[4096];
            using (FileStream streamReader = File.OpenRead(filename))
                StreamUtils.Copy(streamReader, zipStream, buffer);
            zipStream.CloseEntry();
        }

        private void loadImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Zip archives (*.zip)|*.zip";
            ofd.DefaultExt = ".zip";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ExtractZipFile(ofd.FileName);                
            };
            ofd.Dispose();
        }

        public void ExtractZipFile(string archiveFilenameIn)
        {
            ZipFile zf = null;
            string txtT = "";
            string txtI = "";
            try
            {
                int imgsupd = 0;
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                        continue;           // Ignore directories

                    String entryFileName = zipEntry.Name;
                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);
                    int file_id = 0;
                    if (entryFileName.ToLower() == "names.txt")
                    {
                        DialogResult dr = MessageBox.Show("Do you want to load names from selected file?", "Layer names", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (dr == DialogResult.Cancel) break;
                        if (dr == DialogResult.Yes)
                        {
                            MemoryStream ms = new MemoryStream();
                            StreamUtils.Copy(zipStream, ms, buffer);
                            ms.Position = 0;
                            StreamReader sr = new StreamReader(ms, System.Text.Encoding.GetEncoding(1251));
                            List<string> ltmp = new List<string>();
                            while (!sr.EndOfStream) ltmp.Add(sr.ReadLine());
                            sr.Close();
                            ms.Close();

                            if (ltmp.Count != categories.Count)
                            {
                                txtT += "No layer names updated!";
                                MessageBox.Show("In text file count of lines must be " + layers.Items.Count.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {
                                for (int i = 0; i < categories.Count; i++)
                                {
                                    categories[i].name = ltmp[i];
                                    layers.Items[i].Text = String.Format("{0}: {1}", i, ltmp[i]);
                                };
                                txtT += categories.Count.ToString() + " layer names updated!";
                            };                            
                        };
                    }
                    else
                    {
                        if (!int.TryParse(System.IO.Path.GetFileNameWithoutExtension(zipEntry.Name), out file_id)) file_id = -1;
                        if ((file_id < 0) || (file_id > (symbols.Count - 1))) continue;

                        if (imgsupd == 0)
                        {
                            DialogResult dr = MessageBox.Show("Do you want to update images from selected file?", "Layer images", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                            if (dr == DialogResult.Cancel) break;
                            if (dr == DialogResult.No) break;
                        };

                        Symbol sym = symbols[file_id];                    
                        using (FileStream streamWriter = File.Create(sym.file))
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        images.Images.RemoveByKey(sym.ID);
                        Bitmap bmp = new Bitmap(sym.file);
                        images.Images.Add(sym.ID, (Image)new Bitmap(bmp));
                        bmp.Dispose();
                        imgsupd++;
                    };                    
                };
                if (imgsupd > 0)
                {                    
                    txtI = imgsupd.ToString() + " layer images updated!";
                };
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            };
            string txt = txtT + ((txtT == "" ? "" : "\r\n")) + txtI;
            if (txt != "")
                MessageBox.Show(txt, "Update layers", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void layers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 113) layers_DoubleClick(sender, e);
            if (e.KeyValue == 13) layers_DoubleClick(sender, e);
            // if ((layers.CheckBoxes == false) && (e.KeyValue == 32)) layers_DoubleClick(sender, e);
            if (e.KeyValue == 32) switchsub();
            if (e.KeyValue == 37) switchsub();
            if (e.KeyValue == 39) switchsub();
        }

        public bool IsSubName(string imageIndex)
        {
            if (layers.Items.Count == 0) return false;
            if (String.IsNullOrEmpty(imageIndex)) return false;
            for (int ii = 0; ii < layers.Items.Count; ii++)
                if (layers.Items[ii].ImageKey == imageIndex)
                    if(layers.Items[ii].SubItems.Count > 1)
                        if(layers.Items[ii].SubItems[1].Text == "subname")
                            return true;
            return false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (layers.Items.Count == 0) return;

            if (isCRC)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Load Names From File";
                ofd.DefaultExt = ".txt";
                ofd.Filter = "Text Files (*.txt)|*.txt";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Regex rxgpinn = new Regex(@"gpi_name_(?<crc>[^=]+)\s*=(?<name>[^\r\n]+)", RegexOptions.IgnoreCase);
                    Regex rxgpisn = new Regex(@"gpi_subname_(?<crc>[^=]+)\s*=(?<name>[^\r\n]+)", RegexOptions.IgnoreCase);
                    FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs, true);
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if(line.StartsWith("#")) continue;
                        MatchCollection mc;
                        if ((mc = rxgpisn.Matches(line)).Count > 0)
                        {
                            string cc = mc[0].Groups["crc"].Value;
                            string nn = mc[0].Groups["name"].Value;
                            for (int i = 0; i < this.categories.Count; i++)
                                if (this.categories[i].ID == cc)
                                {
                                    this.categories[i].name = nn;
                                    this.layers.Items[i].SubItems[0].Text = nn;
                                    this.layers.Items[i].SubItems[1].Text = "subname";
                                    this.layers.Items[i].SubItems[0].BackColor = Color.LightPink;
                                };
                        };
                        if ((mc = rxgpinn.Matches(line)).Count > 0)
                        {
                            string cc = mc[0].Groups["crc"].Value;
                            string nn = mc[0].Groups["name"].Value;
                            for (int i = 0; i < this.categories.Count; i++)
                                if (this.categories[i].ID == cc)
                                {
                                    this.categories[i].name = nn;
                                    this.layers.Items[i].SubItems[0].Text = nn;
                                    this.layers.Items[i].SubItems[1].Text = "name";
                                    this.layers.Items[i].SubItems[0].BackColor = this.layers.BackColor;
                                };
                        };
                    };
                    sr.Close();
                    fs.Close();
                }
                ofd.Dispose();
                return;
            };

            for (int i = 0; i < layers.Items.Count; i++)
                layers.Items[i].Checked = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (layers.Items.Count == 0) return;

            if (isCRC)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save Names To File";
                sfd.DefaultExt = ".txt";
                sfd.Filter = "Text Files (*.txt)|*.txt";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    List<string> gSave = new List<string>();
                    List<string> gLoad = new List<string>();
                    foreach (Category cat in categories)
                        if (!String.IsNullOrEmpty(cat.name))
                        {
                            if (this.IsSubName(cat.CustomSymbol))
                                gSave.Add("gpi_subname_" + cat.ID + "=" + cat.name);
                            else
                                gSave.Add("gpi_name_" + cat.ID + "=" + cat.name);
                        };
                    FileStream fs;
                    if(File.Exists(sfd.FileName))
                    {
                        fs = new FileStream(sfd.FileName, FileMode.Open, FileAccess.Read);
                        StreamReader sr = new StreamReader(fs, true);
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            if (String.IsNullOrEmpty(line)) continue;
                            gLoad.Add(line);
                        };
                        sr.Close();
                        fs.Close();
                    };
                    Regex rxgpiun = new Regex(@"gpi_(?:sub)?name_(?<crc>\d+)=", RegexOptions.IgnoreCase);
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
                    fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                    foreach (string gl in gLoad) sw.WriteLine(gl);
                    sw.Close();
                    fs.Close();
                }
                sfd.Dispose();
                return;
            };

            for (int i = 0; i < layers.Items.Count; i++)
                layers.Items[i].Checked = true;
        }

        private void layers_SelectedIndexChanged(object sender, EventArgs e)
        {
            selttl.Text = layers.CheckedIndices.Count.ToString() + " / " + layers.Items.Count.ToString();
        }

        private void GMLayRenamerForm_Shown(object sender, EventArgs e)
        {
            layers_SelectedIndexChanged(this, e);
        }

        private void layers_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            layers_SelectedIndexChanged(this, e);
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            switchToSubnameToolStripMenuItem.Enabled = (layers.SelectedItems.Count > 0) && (layers.SelectedItems[0].SubItems.Count > 1);
            switchToSubnameToolStripMenuItem.Text = "Switch to " + (layers.SelectedItems[0].SubItems[1].Text == "name" ? "SubName" : "Name");
        }

        private void switchToSubnameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switchsub();
        }

        private void switchsub()
        {
            if ((layers.SelectedItems.Count == 0) || (layers.SelectedItems[0].SubItems.Count < 2)) return;
            layers.SelectedItems[0].SubItems[1].Text = (layers.SelectedItems[0].SubItems[1].Text == "name" ? "subname" : "name");
            layers.SelectedItems[0].SubItems[0].BackColor = (layers.SelectedItems[0].SubItems[1].Text == "name" ? layers.BackColor : Color.LightPink);
        }
    }

    public class Category
    {
        public string ID;
        public string CustomSymbol;
        public string name;

        public Category(string ID, string CustomSymbol, string name)
        {
            this.ID = ID;
            this.CustomSymbol = CustomSymbol;
            this.name = name;
        }
    }

    public class Symbol
    {
        public string ID;
        public string file;

        public Symbol(string ID, string file)
        {
            this.ID = ID;
            this.file = file;
        }
    }
}