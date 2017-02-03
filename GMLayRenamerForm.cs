using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace KMZRebuilder
{
    public partial class GMLayRenamerForm : Form
    {
        private string filename = "";
        private string xml = "";
        private XmlDocument xd = null;
        private List<Category> categories = new List<Category>();
        private List<Symbol> symbols = new List<Symbol>();
        
        public GMLayRenamerForm(string filename)
        {            
            InitializeComponent();

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

        private void GMLayRenamer_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            if (layers.SelectedIndices.Count == 0) return;
            string name = categories[layers.SelectedIndices[0]].name;
            KMZRebuilederForm.InputBox("Layer name", "Change layer name:", ref name, (Bitmap)images.Images[layers.Items[layers.SelectedIndices[0]].ImageKey]);
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
                for (int i = 0; i < symbols.Count; i++)
                    ZipFile(symbols[i].file, zipStream);
                
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