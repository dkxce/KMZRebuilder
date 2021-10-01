using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace KMZRebuilder
{
    public partial class MapIcons : Form
    {
        private static string deffile;
        private static string zipfile;
        public static void SetZIP(string filename)
        {
            zipfile = filename;
        }

        public static void InitZip(string defaultIconsZip)
        {
            deffile = defaultIconsZip;
            if (!String.IsNullOrEmpty(defaultIconsZip))
                if (File.Exists(defaultIconsZip))
                    zipfile = defaultIconsZip;

            string other_zip = null;
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\", true)
                    .CreateSubKey("KMZRebuilder")
                    .CreateSubKey("MapIcons"))
                    other_zip = key.GetValue("LastFile").ToString();
            }
            catch { };
            if (File.Exists(other_zip))
                zipfile = other_zip;
        }

        private bool firstrun = true;
        public MapIcons()
        {
            InitializeComponent();            
        }

        private void LoadIcons(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
                zipfile = fileName;

            ImageList iml = new ImageList();
            iml.ImageSize = new Size(32, 32);
            iconView.Visible = false;
            panel1.Visible = false;
            panel2.Visible = false;
            iconView.Clear();

            WaitingBoxForm wbf = new WaitingBoxForm(this);
            wbf.Show("Select Map Icon", "Wait, loading icons...");

            try
            {
                FileStream fs = File.OpenRead(zipfile);
                ZipFile zf = new ZipFile(fs);
                int index = 0;
                foreach (ZipEntry zipEntry in zf)
                {
                    Application.DoEvents();
                    if (!zipEntry.IsFile) continue; // Ignore directories
                    String entryFileName = zipEntry.Name;
                    string ext = Path.GetExtension(entryFileName).ToLower();
                    if ((ext != ".png") && (ext != ".jpg") && (ext != ".jpeg") && (ext != ".gif")) continue;

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    try
                    {
                        Stream ms = new MemoryStream();
                        StreamUtils.Copy(zipStream, ms, buffer);
                        ms.Position = 0;
                        Image im = new Bitmap(ms);
                        ms.Dispose();

                        ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(entryFileName) + "\r\n" + index.ToString("00000"));
                        lvi.ImageIndex = iml.Images.Count;
                        iconView.Items.Add(lvi);
                        iml.Images.Add(im);
                    }
                    catch
                    {
                        ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(entryFileName) + "\r\n" + index.ToString("00000"));
                        lvi.ImageIndex = iml.Images.Count;
                        iconView.Items.Add(lvi);
                        iml.Images.Add(new Bitmap(32, 32));
                    };

                    index++;
                    if (index % 50 == 0)
                        wbf.Show("Select Map Icon", "Wait, loading " + index.ToString() + " icons...");
                };
                zf.Close();
                fs.Close();
                wbf.Show("Select Map Icon", "Wait, visualizing " + iconView.Items.Count.ToString() + " icons...");
                iconView.LargeImageList = iml;                
            }
            catch { };            
            TXTC.Text = "Total Icons: " + iconView.Items.Count.ToString();                        
            iconView.Visible = true;
            panel1.Visible = true;
            panel2.Visible = true;
            wbf.Hide();
            wbf = null;

            if (iconView.Items.Count > 0)
                SaveLast();
        }        

        public string ZipFile
        {
            get
            {
                return zipfile;
            }
        }

        private string search_by_number = "";
        private string search_by_text = "";
        private void iconView_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;

            if (e.KeyChar == (char)27)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            };
            if (e.KeyChar == '\r')
            {
                Select();
                return;
            };
            if (iconView.Items.Count > 0)
            {
                bool found = false;
                if (char.IsDigit(e.KeyChar) && (!found))
                {
                    resetTimer.Enabled = false;
                    found = false;
                    string str = search_by_number += new string(new char[] { e.KeyChar });
                    while (true)
                    {
                        for (int i = 0; i < iconView.Items.Count; i++)
                            if (iconView.Items[i].Text.EndsWith(search_by_number))
                            {
                                SelectIconById(i);
                                found = true;
                                break;
                            };
                        if (found) break;
                        if(search_by_number.Length > 1)
                            search_by_number = new string(new char[] { e.KeyChar });
                    };                    
                };
                if (char.IsLetterOrDigit(e.KeyChar) && (!found))
                {
                    resetTimer.Enabled = false;
                    found = false;
                    string str = search_by_text += new string(new char[] { e.KeyChar }).ToLower();
                    while (true)
                    {
                        for (int i = 0; i < iconView.Items.Count; i++)
                            if (iconView.Items[i].Text.ToLower().StartsWith(search_by_text))
                            {
                                SelectIconById(i);
                                found = true;
                                break;
                            };
                        if (found) break;
                        if (search_by_text.Length > 1)
                            search_by_text = new string(new char[] { e.KeyChar }).ToLower();
                        else
                            break;
                    };
                };
                resetTimer.Enabled = true;                
            };
        }

        private void SelectIconById(int id)
        {
            iconView.Items[id].Selected = true;
            iconView.Items[id].Focused = true;
            iconView.Items[id].EnsureVisible();
            ListViewHelper.SelectIndex(iconView, id);            
        }

        private void resetTimer_Tick(object sender, EventArgs e)
        {
            resetTimer.Enabled = false;
            search_by_number = "";
            search_by_text = "";
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)27)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            };
            if ((e.KeyChar == '\r') && (iconView.Items.Count > 0) && (!String.IsNullOrEmpty(textBox1.Text)))
            {
                string stext = textBox1.Text.ToLower();
                int start_from = 0;
                if (iconView.SelectedItems.Count > 0)
                {
                    start_from = iconView.SelectedIndices[0] + 1;
                    if (start_from >= iconView.Items.Count)
                        start_from = 0;
                };
                for (int i = start_from; i < iconView.Items.Count; i++)
                    if (iconView.Items[i].Text.ToLower().IndexOf(stext) >= 0)
                    {
                        SelectIconById(i);
                        return;
                    };
                if(start_from > 0)
                    for (int i = 0; i < start_from; i++)
                        if (iconView.Items[i].Text.ToLower().IndexOf(stext) >= 0)
                        {
                            SelectIconById(i);
                            return;
                        };
            };
        }

        private void iconView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (e.Item.BackColor != SystemColors.Window)
                e.Item.BackColor = Color.Lime;
            e.DrawDefault = false;

            Rectangle box = e.Bounds;
            if (e.Item.Selected)
                e.Graphics.FillRectangle(SystemBrushes.Highlight, box);                
            else
                e.Graphics.FillRectangle(SystemBrushes.Window, box);
            
            Image im = iconView.LargeImageList.Images[e.Item.ImageIndex];            
            e.Graphics.DrawImage(im, box.Left + box.Width / 2 - im.Width / 2, box.Top + 1);

            Rectangle labelBounds = e.Item.GetBounds(ItemBoundsPortion.Label);
            
            string text = e.Item.Text.Substring(0,e.Item.Text.IndexOf("\r\n"));
            string index = e.Item.Text.Substring(e.Item.Text.IndexOf("\r\n") + 2);

            SizeF sl = e.Graphics.MeasureString(text, e.Item.Font);
            SizeF si = e.Graphics.MeasureString(index, e.Item.Font);
            while (sl.Width > labelBounds.Width)
            {
                text = text.Substring(0, text.Length - 1);
                sl = e.Graphics.MeasureString(text, e.Item.Font);
            };
            if (e.Item.Selected)
            {
                e.Graphics.DrawString(text, e.Item.Font, SystemBrushes.HighlightText, labelBounds.Left + labelBounds.Width / 2 - sl.Width / 2, labelBounds.Top);
                e.Graphics.DrawString(index, e.Item.Font, SystemBrushes.HighlightText, labelBounds.Left + labelBounds.Width / 2 - si.Width / 2, labelBounds.Top + e.Item.Font.Height + 1);
            }
            else
            {
                e.Graphics.DrawString(text, e.Item.Font, SystemBrushes.WindowText, labelBounds.Left + labelBounds.Width / 2 - sl.Width / 2, labelBounds.Top);
                e.Graphics.DrawString(index, e.Item.Font, SystemBrushes.WindowText, labelBounds.Left + labelBounds.Width / 2 - si.Width / 2, labelBounds.Top + e.Item.Font.Height + 1);
            };
        }

        private void iconView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (iconView.SelectedIndices.Count == 0)
                SIT.Text = "[NONE]";
            else
                SIT.Text = iconView.SelectedItems[0].Text.Replace("\r\n", "   ");
        }

        private void iconView_DoubleClick(object sender, EventArgs e)
        {
            Select();
        }

        private void Select()
        {
            if (iconView.SelectedIndices.Count == 0) return;
            DialogResult = DialogResult.OK;
            // textBox1.Text = "";
            Close();
        }

        public string SelectedIcon
        {
            get
            {
                if (iconView.SelectedIndices.Count == 0) return null;
                return iconView.SelectedItems[0].Text.Substring(0, iconView.SelectedItems[0].Text.IndexOf("\r\n"));
            }
            set
            {
                if (iconView.Items.Count == 0) return;
                if(String.IsNullOrEmpty(value)) return;
                for(int i=0;i<iconView.Items.Count;i++)
                    if (iconView.Items[i].Text.Substring(0, iconView.Items[i].Text.IndexOf("\r\n")) == value)
                    {
                        if (iconView.SelectedIndices.Count > 0)
                            iconView.SelectedItems[0].Selected = false;
                        SelectIconById(i);                
                        return;
                    };
            }
        }

        public string GetFileById(int id)
        {
            if (id < 0) return "";
            if (iconView.Items.Count == 0) return "";
            if (id >= iconView.Items.Count) return "";
            return Path.GetFileName(zipfile) + @"\" + iconView.Items[id].Text.Substring(0, iconView.Items[id].Text.IndexOf("\r\n")) + ".png";
        }

        public int GetIdByFile(string file)
        {
            if (String.IsNullOrEmpty(zipfile)) return -1;
            if (String.IsNullOrEmpty(file)) return -1;
            if (file.IndexOf(@"\") <= 0) return -1;
            if (iconView.Items.Count == 0) return -1;

            string[] dd = file.Split(new char[] { '\\' }, 2);
            if (dd[0].ToLower() == Path.GetFileName(zipfile).ToLower())
            {
                string txt = Path.GetFileNameWithoutExtension(dd[1]);                
                for (int i = 0; i < iconView.Items.Count; i++)
                    if (iconView.Items[i].Text.Substring(0, iconView.Items[i].Text.IndexOf("\r\n")) == txt)
                        return i;
            };
            return -1;
        }

        public string SelectedFile
        {
            get
            {
                if (iconView.SelectedIndices.Count == 0) return null;
                return Path.GetFileName(zipfile) + @"\" + iconView.SelectedItems[0].Text.Substring(0, iconView.SelectedItems[0].Text.IndexOf("\r\n")) + ".png";
            }
            set
            {
                if (String.IsNullOrEmpty(zipfile)) return;
                if (String.IsNullOrEmpty(value)) return;
                if (value.IndexOf(@"\") <= 0) return;
                
                string[] dd = value.Split(new char[] { '\\' }, 2);
                if (dd[0].ToLower() == Path.GetFileName(zipfile).ToLower())
                    SelectedIcon = Path.GetFileNameWithoutExtension(dd[1]);
            }
        }

        public int SelectedIndex
        {
            get
            {
                if (iconView.SelectedIndices.Count == 0) return -1;
                return iconView.SelectedItems[0].Index;
            }
            set
            {
                if (value < 1)
                    if (iconView.SelectedIndices.Count > 0)
                        iconView.SelectedItems[0].Selected = false;
                if (value < iconView.Items.Count)
                {
                    if (iconView.SelectedIndices.Count > 0)
                        iconView.SelectedItems[0].Selected = false;
                    SelectIconById(value);
                };
            }
        }
        
        public Image SelectedImage
        {
            get
            {
                if (iconView.SelectedIndices.Count == 0) return null;
                Image im = GetImageByName(SelectedIcon);                
                return im;
            }
        }

        public byte[] SelectedImageArr
        {
            get
            {
                if (iconView.SelectedIndices.Count == 0) return null;
                return GetImageArrByName(SelectedIcon);
            }
        }

        public int Count
        {
            get
            {
                return iconView.Items.Count;
            }
        }

        public Image GetImageByName(string name)
        {            
            if (String.IsNullOrEmpty(name)) return null;
            Image result = null;

            FileStream fs = File.OpenRead(zipfile);
            ZipFile zf = new ZipFile(fs);
            int index = 0;
            foreach (ZipEntry zipEntry in zf)
            {
                if (!zipEntry.IsFile) continue; // Ignore directories
                String entryFileName = zipEntry.Name;
                string ext = Path.GetExtension(entryFileName).ToLower();
                if ((ext != ".png") && (ext != ".jpg") && (ext != ".jpeg") && (ext != ".gif")) continue;

                if (Path.GetFileNameWithoutExtension(entryFileName).ToLower() == name.ToLower())
                {
                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    try
                    {
                        Stream ms = new MemoryStream();
                        StreamUtils.Copy(zipStream, ms, buffer);
                        ms.Position = 0;
                        result = new Bitmap(ms);
                        ms.Dispose();
                    }
                    catch
                    {
                        // NOTHING
                    };
                };
                index++;
            };
            zf.Close();
            fs.Close();

            return result;
        }

        public byte[] GetImageArrByName(string name)
        {
            if (String.IsNullOrEmpty(name)) return null;
            byte[] result = null;

            FileStream fs = File.OpenRead(zipfile);
            ZipFile zf = new ZipFile(fs);
            int index = 0;
            foreach (ZipEntry zipEntry in zf)
            {
                if (!zipEntry.IsFile) continue; // Ignore directories
                String entryFileName = zipEntry.Name;
                string ext = Path.GetExtension(entryFileName).ToLower();
                if ((ext != ".png") && (ext != ".jpg") && (ext != ".jpeg") && (ext != ".gif")) continue;

                if (Path.GetFileNameWithoutExtension(entryFileName).ToLower() == name.ToLower())
                {
                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    try
                    {
                        Stream ms = new MemoryStream();
                        StreamUtils.Copy(zipStream, ms, buffer);
                        ms.Flush();
                        ms.Position = 0;
                        result = new byte[ms.Length];
                        ms.Read(result, 0, result.Length);
                        ms.Dispose();
                    }
                    catch
                    {
                        // NOTHING
                    };
                };
                index++;
            };
            zf.Close();
            fs.Close();

            return result;
        }

        public Image GetImageByID(int id)
        {
            if (id < 0) return null;
            Image result = null;

            FileStream fs = File.OpenRead(zipfile);
            ZipFile zf = new ZipFile(fs);
            int index = 0;
            foreach (ZipEntry zipEntry in zf)
            {
                if (!zipEntry.IsFile) continue; // Ignore directories
                String entryFileName = zipEntry.Name;
                string ext = Path.GetExtension(entryFileName).ToLower();
                if ((ext != ".png") && (ext != ".jpg") && (ext != ".jpeg") && (ext != ".gif")) continue;

                if (index == id)
                {
                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    try
                    {
                        Stream ms = new MemoryStream();
                        StreamUtils.Copy(zipStream, ms, buffer);
                        ms.Position = 0;
                        result = new Bitmap(ms);
                        ms.Dispose();
                    }
                    catch 
                    { 
                        // NOTHING
                    };
                };
                index++;
            };
            zf.Close();
            fs.Close();

            return result;
        }

        public byte[] GetImageArrByID(int id)
        {
            if (id < 0) return null;
            byte[] result = null;

            FileStream fs = File.OpenRead(zipfile);
            ZipFile zf = new ZipFile(fs);
            int index = 0;
            foreach (ZipEntry zipEntry in zf)
            {
                if (!zipEntry.IsFile) continue; // Ignore directories
                String entryFileName = zipEntry.Name;
                string ext = Path.GetExtension(entryFileName).ToLower();
                if ((ext != ".png") && (ext != ".jpg") && (ext != ".jpeg") && (ext != ".gif")) continue;

                if (index == id)
                {
                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    try
                    {
                        Stream ms = new MemoryStream();
                        StreamUtils.Copy(zipStream, ms, buffer);
                        ms.Flush();
                        ms.Position = 0;
                        result = new byte[ms.Length];
                        ms.Read(result, 0, result.Length);
                        ms.Dispose();
                    }
                    catch
                    {
                        // NOTHING
                    };
                };
                index++;
            };
            zf.Close();
            fs.Close();

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(button1, 0, 0);                        
        }

        private void MapIcons_Shown(object sender, EventArgs e)
        {
            if (firstrun)
            {
                if (!String.IsNullOrEmpty(deffile))
                {
                    string dir = Path.GetDirectoryName(deffile);
                    if (Directory.Exists(dir))
                    {
                        string[] zips = Directory.GetFiles(dir, "*.zip");
                        if(zips.Length > 0)
                            for (int i = 0; i < zips.Length; i++)
                            {
                                ToolStripItem mi = contextMenuStrip1.Items.Add(Path.GetFileName(zips[i]));
                                contextMenuStrip1.Items.Insert(contextMenuStrip1.Items.Count - 6, mi);
                                mi.Click += new EventHandler(mi_Click);
                            };
                    };
                };
                LoadIcons(null);
            };
            firstrun = false;
        }

        private void mi_Click(object sender, EventArgs e)
        {
            string file = (sender as ToolStripItem).Text;
            string dir = Path.GetDirectoryName(deffile);
            file = dir + @"\" + file;
            if (File.Exists(file))
                LoadIcons(file);
        }

        private void SaveLast()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\", true)
                    .CreateSubKey("KMZRebuilder")
                    .CreateSubKey("MapIcons"))
                {
                    if(!String.IsNullOrEmpty(deffile))
                        if(File.Exists(deffile))
                            key.SetValue("", deffile);
                    key.SetValue("LastFile", zipfile);
                };
            }
            catch { };
        }

        private void loadIconsFromDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadIcons(deffile);            
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            for (int i = 0; i < contextMenuStrip1.Items.Count; i++)
                contextMenuStrip1.Items[i].Font = new Font(contextMenuStrip1.Items[i].Font, FontStyle.Regular);

            if(String.IsNullOrEmpty(zipfile)) return;

            for (int i = 0; i < contextMenuStrip1.Items.Count; i++)
                if (contextMenuStrip1.Items[i].Text == Path.GetFileName(zipfile))
                    contextMenuStrip1.Items[i].Font = new Font(contextMenuStrip1.Items[i].Font, FontStyle.Bold);
        }


        private void loadIconFromImageFileToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            this.DialogResult = DialogResult.Ignore;
            Close();
        }

        private void loadIconsFromZIPFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "ZIP Archives (*.zip)|*.zip";
            if (!String.IsNullOrEmpty(zipfile))
                ofd.FileName = zipfile;
            ofd.FileName = "mapicons.zip";
            if (ofd.ShowDialog() == DialogResult.OK)
                LoadIcons(ofd.FileName);
            ofd.Dispose();
        }

        public static void SaveIcon(byte[] arr, string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            fs.Write(arr, 0, arr.Length);
            fs.Close();
        }

        public static void SaveIcon(string fromFile, string fileName)
        {
            File.Copy(fromFile, fileName, true);
        }

        public static void SaveIcon(ImageMagick.MagickImage im, string fileName)
        {
            im.Write(fileName);
        }
    }

    public static class ListViewHelper
    {
        private const int LVM_FIRST = 0x1000;
        private const int LVM_SETITEMSTATE = LVM_FIRST + 43;
        
        private const int LVIS_FOCUSED = 0x0001;
        private const int LVIS_SELECTED = 0x0002;
        private const int LVIF_STATE = 0x0008;        
            
        [StructLayout(LayoutKind.Sequential)]
        private struct LV_ITEM
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            public int pszText;
            public int cchTextMax;
            public int iImage;
            public int lParam;
            public int iIndent;
            public int iGroupId;
            public int cColumns;
            public int puColumns;
        }

        public static void SelectAll(ListView list, bool selected)
        {
            LV_ITEM lvItem = new LV_ITEM();

            lvItem.mask = LVIF_STATE;
            lvItem.state = selected ? 0xF : 0;
            lvItem.stateMask = LVIS_SELECTED;

            unsafe
            {
                SendMessage(list.Handle, LVM_SETITEMSTATE, -1, (int)&lvItem);
            }
        }

        public static void SelectIndex(ListView list, int index)
        {
            LV_ITEM lvItem = new LV_ITEM();
            lvItem.iItem = index;
            lvItem.mask = LVIF_STATE;
            lvItem.state = 0xF;
            lvItem.stateMask = LVIS_SELECTED;

            unsafe
            {
                SendMessage(list.Handle, LVM_SETITEMSTATE, index, (int)&lvItem);
            }

            lvItem = new LV_ITEM();
            lvItem.iItem = index;
            lvItem.mask = LVIF_STATE;
            lvItem.state = 0xF;
            lvItem.stateMask = LVIS_FOCUSED;

            unsafe
            {
                SendMessage(list.Handle, LVM_SETITEMSTATE, index, (int)&lvItem);
            }
        }

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
    }
}
