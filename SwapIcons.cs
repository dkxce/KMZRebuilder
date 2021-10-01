using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace KMZRebuilder
{
    public partial class SwapIcons : Form
    {
        public List<string> files = new List<string>();
        public ImageList images = new ImageList();
        public string[] rep_files = new string[0];
        public Image[] rep_images = new Image[0];
        public byte[][] rep_imars = new byte[0][];
        private KMZRebuilederForm parent;

        public SwapIcons(string[] files, KMZRebuilederForm parent)
        {
            this.parent = parent;
            this.files.AddRange(files);            
            rep_files = new string[this.files.Count];
            rep_images = new Image[this.files.Count];
            rep_imars = new byte[this.files.Count][];

            InitializeComponent();
            this.Text += " [" + this.files.Count.ToString() + "]";

            imlist.Columns[0].Width = 230;
            imlist.Columns[1].Width = 480;

            for (int i = 0; i < this.files.Count; i++)
            {
                images.Images.Add(Image.FromFile(this.files[i]));
                ListViewItem lvi = new ListViewItem(System.IO.Path.GetFileNameWithoutExtension(this.files[i]));
                lvi.SubItems.Add("");
                lvi.SubItems.Add("");
                lvi.ImageIndex = i;
                lvi.UseItemStyleForSubItems = true;
                imlist.Items.Add(lvi);
            };
            images.ImageSize = new Size(32, 32);
            imlist.StateImageList = images;
            imlist.SmallImageList = images;

            imSize.SelectedIndex = imSize.Items.Count - 2;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            ImageMagick.MagickImage im = new ImageMagick.MagickImage((Bitmap)image);
            im.Resize(width, height);
            return im.ToBitmap();

            //Rectangle destRect = new Rectangle(0, 0, width, height);
            //Bitmap destImage = new Bitmap(width, height);

            //destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //using (Graphics graphics = Graphics.FromImage(destImage))
            //{
            //    graphics.CompositingMode = CompositingMode.SourceCopy;
            //    graphics.CompositingQuality = CompositingQuality.HighQuality;
            //    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //    graphics.SmoothingMode = SmoothingMode.HighQuality;
            //    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            //    using (ImageAttributes wrapMode = new ImageAttributes())
            //    {
            //        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            //        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            //    }
            //}

            //return destImage;
        }

        private void listView2_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
            ListView listView = (ListView)sender;            
            if(e.ColumnIndex == 1)
            {
                e.DrawDefault = false;

                Rectangle rowBounds = e.SubItem.Bounds;
                int leftMargin = 20 + 34;
                Rectangle bounds = new Rectangle(rowBounds.Left + leftMargin, rowBounds.Top + 5, rowBounds.Width - leftMargin, rowBounds.Height);

                int sel = -1;
                if (listView.SelectedItems.Count > 0) sel = listView.SelectedItems[0].Index;
                if (e.Item.Index == sel)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, rowBounds);
                    {
                        if (!String.IsNullOrEmpty(rep_files[e.ItemIndex]))
                            e.Graphics.DrawImage(rep_images[e.ItemIndex], rowBounds.Left + 20, rowBounds.Top + rowBounds.Height / 2 - rep_images[e.ItemIndex].Height / 2);
                    };
                    e.Graphics.DrawString(e.SubItem.Text, listView.Font, SystemBrushes.HighlightText, bounds);
                }
                else
                {
                    e.Graphics.FillRectangle(SystemBrushes.Window, rowBounds);
                    {
                        if (!String.IsNullOrEmpty(rep_files[e.ItemIndex]))
                            e.Graphics.DrawImage(rep_images[e.ItemIndex], rowBounds.Left + 20, rowBounds.Top + rowBounds.Height / 2 - rep_images[e.ItemIndex].Height / 2);
                    };
                    e.Graphics.DrawString(e.SubItem.Text, listView.Font, SystemBrushes.WindowText, bounds);
                };

                string txt2 = e.Item.SubItems[2].Text;
                if (!String.IsNullOrEmpty(txt2))
                {
                    SizeF ts = e.Graphics.MeasureString(txt2, e.Item.Font);
                    e.Graphics.DrawString(txt2, e.Item.Font, (e.Item.Index == sel) ? Brushes.Silver : Brushes.Maroon, e.Bounds.Right - ts.Width - 2, e.Bounds.Top + e.Bounds.Height / 2 - ts.Height / 2);
                };
            };
        }

        private void listView2_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView2_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;
        }

        private int copied = -1;
        private bool copiex = false;
        private void listView2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (imlist.Items.Count == 0) return;

            if (e.KeyChar == (char)1)
            {
                for (int i = 0; i < imlist.Items.Count; i++)
                    imlist.Items[i].Selected = true;
                return;
            };

            if (imlist.SelectedItems.Count == 0) return;

            if (char.IsDigit(e.KeyChar))
            {
                digit_input += new string(new char[] { e.KeyChar });

                if (parent == null) return;
                if (parent.mapIcons == null) return;
                if (parent.mapIcons.Count == 0) return;
                if (String.IsNullOrEmpty(digit_input)) return;
                int dig = -1;
                if (!int.TryParse(digit_input, out dig)) return;
                if (dig < 0) return;
                if (dig >= parent.mapIcons.Count) return;

                rep_files[imlist.SelectedIndices[0]] = parent.mapIcons.GetFileById(dig);
                rep_images[imlist.SelectedIndices[0]] = ResizeImage(parent.mapIcons.GetImageByID(dig), 32, 32);
                rep_imars[imlist.SelectedIndices[0]] = parent.mapIcons.GetImageArrByID(dig);
                imlist.SelectedItems[0].SubItems[1].Text = parent.mapIcons.GetFileById(dig);
                imlist.SelectedItems[0].SubItems[2].Text = dig.ToString();
            }
            else
                digit_input = "";

            if (e.KeyChar == '\r') listView2_DoubleClick(sender, e);
            if (e.KeyChar == ' ') sif_Click(sender, e);   
         
            if ((e.KeyChar == (char)3) || (e.KeyChar == (char)99))
            {
                copied = imlist.SelectedIndices[0];
                copiex = false;
            };
            if (((e.KeyChar == (char)22) || (e.KeyChar == (char)118)) && (copied >= 0))
            {
                for (int i = 0; i < imlist.SelectedItems.Count; i++)
                {
                    rep_files[imlist.SelectedIndices[i]] = rep_files[copied];
                    rep_images[imlist.SelectedIndices[i]] = rep_images[copied];
                    rep_imars[imlist.SelectedIndices[i]] = rep_imars[copied];
                    imlist.SelectedItems[i].SubItems[1].Text = imlist.Items[copied].SubItems[1].Text;
                    imlist.SelectedItems[i].SubItems[2].Text = imlist.Items[copied].SubItems[2].Text;
                };

                if (copiex)
                {
                    rep_files[copied] = null;
                    rep_images[copied] = null;
                    rep_imars[copied] = null;
                    imlist.Items[copied].SubItems[1].Text = "";
                    imlist.Items[copied].SubItems[2].Text = "";
                    copied = -1;
                };
            };
            if ((e.KeyChar == (char)24) || (e.KeyChar == (char)120))
            {
                copied = imlist.SelectedIndices[0];
                copiex = true;
            };
        }

        private string digit_input = "";
        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyValue < 96)  digit_input = "";
            if (e.KeyValue > 105) digit_input = "";

            if (imlist.Items.Count == 0) return;
            if (imlist.SelectedItems.Count == 0) return;
            
            if (e.KeyValue == 46)
            {
                for (int i = 0; i < imlist.SelectedItems.Count; i++)
                {
                    rep_files[imlist.SelectedIndices[i]] = null;
                    rep_images[imlist.SelectedIndices[i]] = null;
                    rep_imars[imlist.SelectedIndices[i]] = null;
                    imlist.SelectedItems[i].SubItems[1].Text = "";
                    imlist.SelectedItems[i].SubItems[2].Text = "";
                };
            };
            if ((e.KeyValue == 37) || (e.KeyValue == 39))
            {
                if (parent == null) return;
                if ((parent.mapIcons == null) || (parent.mapIcons.Count == 0))
                {
                    listView2_DoubleClick(sender, e);
                    return;
                };
                string txts = imlist.SelectedItems[0].SubItems[1].Text;
                int index = parent.mapIcons.SelectedIndex;
                if (!String.IsNullOrEmpty(txts))
                {
                    index = parent.mapIcons.GetIdByFile(txts);
                    if (index < 0)
                        return;
                    if (e.KeyValue == 37) index--;
                    if (e.KeyValue == 39) index++;
                }
                else 
                    if (index < 0) index = 0;
                
                if (index < 0) index = parent.mapIcons.Count - 1;
                if (index >= parent.mapIcons.Count) index = 0;
                for (int i = 0; i < imlist.SelectedItems.Count; i++)
                {
                    rep_files[imlist.SelectedIndices[i]] = parent.mapIcons.GetFileById(index);
                    rep_images[imlist.SelectedIndices[i]] = ResizeImage(parent.mapIcons.GetImageByID(index), 32, 32);
                    rep_imars[imlist.SelectedIndices[i]] = parent.mapIcons.GetImageArrByID(index);
                    imlist.SelectedItems[i].SubItems[1].Text = parent.mapIcons.GetFileById(index);
                    imlist.SelectedItems[i].SubItems[2].Text = index.ToString();
                };
            };            
        }

        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            if (imlist.Items.Count == 0) return;
            if (imlist.SelectedItems.Count == 0) return;

            if (parent.mapIcons == null)
                parent.mapIcons = new MapIcons();

            parent.mapIcons.SelectedFile = imlist.SelectedItems[0].SubItems[1].Text;
            DialogResult dr = parent.mapIcons.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Image im = parent.mapIcons.SelectedImage;
                byte[] ia = parent.mapIcons.SelectedImageArr;
                string filename = parent.mapIcons.SelectedFile;
                int id = parent.mapIcons.SelectedIndex;
                for (int i = 0; i < imlist.SelectedItems.Count; i++)
                {
                    rep_files[imlist.SelectedIndices[i]] = filename;
                    rep_images[imlist.SelectedIndices[i]] = ResizeImage(im, 32, 32);
                    rep_imars[imlist.SelectedIndices[i]] = ia;
                    imlist.SelectedItems[i].SubItems[1].Text = filename;
                    imlist.SelectedItems[i].SubItems[2].Text = id.ToString();
                };
            }
            else if (dr == DialogResult.Ignore)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Select Image";
                ofd.DefaultExt = ".png";
                ofd.Filter = "Image files (*.png;*.jpg;*.gif)|*.png;*.jpg;*.gif";
                if (!String.IsNullOrEmpty(rep_files[imlist.SelectedIndices[0]]))
                    ofd.FileName = rep_files[imlist.SelectedIndices[0]];
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Image im = Image.FromFile(ofd.FileName);
                    System.IO.FileStream fs = new System.IO.FileStream(ofd.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    byte[] ia = new byte[fs.Length];
                    fs.Read(ia, 0, ia.Length);
                    fs.Close();
                    for (int i = 0; i < imlist.SelectedItems.Count; i++)
                    {
                        rep_files[imlist.SelectedIndices[i]] = ofd.FileName;
                        rep_images[imlist.SelectedIndices[i]] = ResizeImage(im, 32, 32);
                        rep_imars[imlist.SelectedIndices[i]] = ia;
                        imlist.SelectedItems[i].SubItems[1].Text = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
                        imlist.SelectedItems[i].SubItems[2].Text = "";
                    };
                };
                ofd.Dispose();
            };
        }

        private void ïðèìåíèòüÊîÂñåìToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imlist.Items.Count == 0) return;            
        }

        private void popup_Opening(object sender, CancelEventArgs e)
        {
            sil.Enabled =
                sif.Enabled =
                    imlist.SelectedIndices.Count > 0;

            getCRCOfImageToolStripMenuItem.Enabled =
                imlist.SelectedIndices.Count > 0;

            autofillImageByIDFromListToolStripMenuItem.Enabled =
            autofillImageFromListToolStripMenuItem.Enabled =
                (imlist.Items.Count > 0) &&
                (parent != null) &&
                (parent.mapIcons != null) &&
                (parent.mapIcons.Count > 0) &&
                (!String.IsNullOrEmpty(parent.mapIcons.ZipFile)) &&
                (System.IO.File.Exists(parent.mapIcons.ZipFile));
        }

        private void sil_Click(object sender, EventArgs e)
        {
            listView2_DoubleClick(sender, e);
        }

        private void sif_Click(object sender, EventArgs e)
        {
            if (imlist.Items.Count == 0) return;
            if (imlist.SelectedItems.Count == 0) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Image";
            ofd.DefaultExt = ".png";
            ofd.Filter = "Image files (*.png;*.jpg;*.gif)|*.png;*.jpg;*.gif";
            if (!String.IsNullOrEmpty(rep_files[imlist.SelectedIndices[0]]))
                ofd.FileName = rep_files[imlist.SelectedIndices[0]];
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Image im = Image.FromFile(ofd.FileName);
                System.IO.FileStream fs = new System.IO.FileStream(ofd.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                byte[] ia = new byte[fs.Length];
                fs.Read(ia, 0, ia.Length);
                fs.Close();
                for (int i = 0; i < imlist.SelectedItems.Count; i++)
                {
                    rep_files[imlist.SelectedIndices[i]] = ofd.FileName;
                    rep_images[imlist.SelectedIndices[i]] = ResizeImage(im, 32, 32);
                    rep_imars[imlist.SelectedIndices[i]] = ia;
                    imlist.SelectedItems[i].SubItems[1].Text = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName);
                    imlist.SelectedItems[i].SubItems[2].Text = "";
                };
            };
            ofd.Dispose();
        }

        private void autofillImageFromListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imlist.Items.Count == 0) return;
            if (parent == null) return;
            if (parent.mapIcons == null) return;
            if (parent.mapIcons.Count == 0) return;
            if (String.IsNullOrEmpty(parent.mapIcons.ZipFile)) return;
            if (!System.IO.File.Exists(parent.mapIcons.ZipFile)) return;
            string zipfile = System.IO.Path.GetFileName(parent.mapIcons.ZipFile);

            int repall = 0;
            string[] eoa = new string[] { "Only Empty Images", "All Images" };
            if (System.Windows.Forms.InputBox.Show("Autofill Image by Name from List", "Select Items to Set:", eoa, ref repall) != DialogResult.OK) return;

            string prefix = zipfile + @"\{ImageName}.png";
            if (System.Windows.Forms.InputBox.Show("Autofill Image by Name from List", "Select Image Prefix and Suffix:", ref prefix) != DialogResult.OK) return;

            int replaced = 0;
            for (int i = 0; i < imlist.Items.Count; i++)
            {
                if ((repall == 0) && (imlist.Items[i].SubItems[1].Text != "")) continue;

                int index = parent.mapIcons.GetIdByFile(prefix.Replace("{ImageName}", imlist.Items[i].Text));
                if (index < 0) continue;

                rep_files[i] = parent.mapIcons.GetFileById(index);
                rep_images[i] = ResizeImage(parent.mapIcons.GetImageByID(index), 32, 32);
                rep_imars[i] = parent.mapIcons.GetImageArrByID(index);
                imlist.Items[i].SubItems[1].Text = parent.mapIcons.GetFileById(index);
                imlist.Items[i].SubItems[2].Text = index.ToString();
                replaced++;
            };

            MessageBox.Show("Found and set " + replaced.ToString() + "/" + imlist.Items.Count.ToString() + " images", "Autofill Image by Name from List", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void autofillImageByIDFromListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imlist.Items.Count == 0) return;
            if (parent == null) return;
            if (parent.mapIcons == null) return;
            if (parent.mapIcons.Count == 0) return;
            if (String.IsNullOrEmpty(parent.mapIcons.ZipFile)) return;
            if (!System.IO.File.Exists(parent.mapIcons.ZipFile)) return;
            string zipfile = System.IO.Path.GetFileName(parent.mapIcons.ZipFile);

            int repall = 0;
            string[] eoa = new string[] { "Only Empty Images", "All Images" };
            if (System.Windows.Forms.InputBox.Show("Autofill Image by ID from List", "Select Items to Set:", eoa, ref repall) != DialogResult.OK) return;

            int index_offset = parent.mapIcons.GetIdByFile(zipfile + @"\number_0.png");
            if (index_offset < 0) index_offset = 0;

            if (System.Windows.Forms.InputBox.Show("Autofill Image by ID from List","Select Image Number Offset:", ref index_offset) != DialogResult.OK) return;

            int replaced = 0;
            for (int i = 0; i < imlist.Items.Count; i++)
            {
                if ((repall == 0) && (imlist.Items[i].SubItems[1].Text != "")) continue;

                int index = index_offset + i;
                if (index < 0) continue;
                if (index >= parent.mapIcons.Count) continue; 

                rep_files[i] = parent.mapIcons.GetFileById(index);
                rep_images[i] = ResizeImage(parent.mapIcons.GetImageByID(index), 32, 32);
                rep_imars[i] = parent.mapIcons.GetImageArrByID(index);
                imlist.Items[i].SubItems[1].Text = parent.mapIcons.GetFileById(index);
                imlist.Items[i].SubItems[2].Text = index.ToString();
                replaced++;
            };
            MessageBox.Show("Found and set " + replaced.ToString() + "/" + imlist.Items.Count.ToString() + " images", "Autofill Image by ID from List", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void getCRCOfImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imlist.SelectedItems.Count == 0) return;
            CRC32 crc = new CRC32();
            uint cc = crc.CRC32Num(this.files[imlist.SelectedIndices[0]]);
            InputBox.Show("CRC Checksum", imlist.SelectedItems[0].Text + ":", cc.ToString());
        }              
    }
}