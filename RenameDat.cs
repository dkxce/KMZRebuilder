using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace KMZRebuilder
{
    public partial class RenameDat : Form
    {
        public byte wptTYPE = 0; // 0 - DAT; 1 - WPT; 2 - GDB
        public Image ImageNone = null;
        private ImageList nlIcons = new ImageList();
        public List<string> nlTexts = new List<string>();
        private string stylehistory = "";
        private string imagesdir = "";
        public IMM imm = new IMM();

        private RenameDat()
        {
            this.InitializeComponent();
            nlIcons.ImageSize = new Size(16, 16);
        }

        public static RenameDat CreateForDAT(string stylehistory, string imagesdir)
        {
            RenameDat res = new RenameDat();
            res.wptTYPE = (byte)0;
            res.stylehistory = stylehistory;
            res.imagesdir = imagesdir;
            res.ImageNone = res.Get16x16ImageByID(0);
            res.listView2.Columns[1].Text = "PROGOROD Type";
            for (int i = 0; i < 20; i++)
            {
                res.nlTexts.Add(((KMZRebuilder.ProGorodPOI.TType)i).ToString());
                res.nlIcons.Images.Add(res.Get16x16ImageByID(i));
            };
            return res;
        }

        public static RenameDat CreateForWPT(string stylehistory, string imagesdis)
        {
            RenameDat res = new RenameDat();
            res.wptTYPE = (byte)1;
            res.stylehistory = stylehistory;
            res.imagesdir = imagesdis;
            res.ImageNone = res.Get16x16ImageByID(3);
            res.listView2.Columns[1].Text = "WPT Type";
            for (int i = 0; i <= 22; i++)
            {
                res.nlTexts.Add(((WPTPOI.SymbolIcon)i).ToString());
                res.nlIcons.Images.Add(res.Get16x16ImageByID(i));
            };
            return res;
        }

        public static RenameDat CreateForGDB(string stylehistory, string imagesdir)
        {
            RenameDat res = new RenameDat();
            res.wptTYPE = (byte)2;
            res.stylehistory = stylehistory;
            res.imagesdir = imagesdir;
            res.ImageNone = res.Get16x16ImageByID(0);            
            res.listView2.Columns[1].Text = "Navital Type";
            for (int i = 0; i < NavitelRecord.IconList.Length; i++)
            {
                res.nlTexts.Add(NavitelRecord.IconText(i));
                res.nlIcons.Images.Add(res.Get16x16ImageByID(i));
            };
            return res;
        }

        public bool DoSort
        {
            get
            {
                return this.sortasc.Checked;
            }
            set
            {
                this.sortasc.Checked = value;
            }
        }

        public bool RemoveDescriptions
        {
            get
            {
                return rds.Checked;
            }
            set
            {
                rds.Checked = value;
            }
        }

        public bool SaveIMM
        {
            get
            {
                return svimm.Checked;
            }
            set
            {
                svimm.Checked = value;
            }
        }       

        private Image Get16x16ImageByID(int id)
        {
            if (wptTYPE == 0)
            {
                string zipFile = KMZRebuilederForm.CurrentDirectory() + @"\gdbicons\progorod.zip";
                if (File.Exists(zipFile))
                    return ResizeImage(KMFile.GetImageFromZip(zipFile, "progorod" + id.ToString("00") + ".png"), 16, 16);
            };
            if (wptTYPE == 1)
            {
                string zipFile = KMZRebuilederForm.CurrentDirectory() + @"\gdbicons\wpt_icons.zip";
                if (File.Exists(zipFile))
                    return ResizeImage(KMFile.GetImageFromZip(zipFile, id.ToString("00") + ".png"), 16, 16);
            };
            if(wptTYPE == 2)
            {
                string zipFile = KMZRebuilederForm.CurrentDirectory() + @"\gdbicons\gdb_icons.zip";
                if (File.Exists(zipFile))
                    return ResizeImage(KMFile.GetImageFromZip(zipFile, id.ToString("000") + ".png"), 16, 16);
            };
            // NoImage
            {
                Image im = new Bitmap(16, 16);
                Graphics g = Graphics.FromImage(im);
                g.FillRectangle(Brushes.Black, 0, 0, 16, 16);
                g.DrawString(id.ToString("00"), this.Font, Brushes.White, 0, 1);
                g.Dispose();
                return im;
            };
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                };
            }

            return destImage;
        }

        private void listView2_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = true;
            ListView listView = (ListView)sender;
            if(e.ColumnIndex == 1)
            {
                e.DrawDefault = false;

                Rectangle rowBounds = e.SubItem.Bounds;
                Rectangle labelBounds = e.Item.GetBounds(ItemBoundsPortion.Label);
                int leftMargin = labelBounds.Left - 1;
                Rectangle bounds = new Rectangle(rowBounds.Left + leftMargin, rowBounds.Top,rowBounds.Width - leftMargin, rowBounds.Height);

                int sel = -1;
                if (listView.SelectedItems.Count > 0) sel = listView.SelectedItems[0].Index;
                int imIndex = nlTexts.IndexOf(e.Item.SubItems[1].Text);
                if (e.Item.Index == sel)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, rowBounds);
                    if ((this.nlIcons != null) && (imIndex >= 0))
                        e.Graphics.DrawImage(this.nlIcons.Images[imIndex], rowBounds.Left + 1, rowBounds.Top);
                    else
                        e.Graphics.DrawImage(ImageNone, rowBounds.Left + 1, rowBounds.Top);
                    e.Graphics.DrawString(e.SubItem.Text, listView.Font, SystemBrushes.HighlightText, bounds);
                }
                else
                {
                    Brush brd = SystemBrushes.Window;
                    int bi = nlTexts.IndexOf(e.SubItem.Text);
                    if (bi >= 0)
                    {
                        Brush[] sb = new Brush[] { SystemBrushes.Window, Brushes.AliceBlue, Brushes.LightCoral, Brushes.LightCyan, Brushes.LightGreen, Brushes.LightPink, Brushes.LightSalmon, Brushes.LightSkyBlue, Brushes.LightYellow, Brushes.Lime, Brushes.MistyRose, Brushes.Orange, Brushes.Orchid, Brushes.Pink, Brushes.Red, Brushes.RoyalBlue, Brushes.Violet, Brushes.Tan, Brushes.YellowGreen, Brushes.Yellow, Brushes.AliceBlue, Brushes.LightCoral, Brushes.LightCyan, Brushes.LightGreen, Brushes.LightPink, Brushes.LightSalmon, Brushes.LightSkyBlue, Brushes.LightYellow, Brushes.Lime, Brushes.MistyRose, Brushes.Orange, Brushes.Orchid, Brushes.Pink, Brushes.Red, Brushes.RoyalBlue, Brushes.Violet, Brushes.Tan, Brushes.YellowGreen, Brushes.Yellow};
                        brd = sb[bi % sb.Length];
                    };
                    e.Graphics.FillRectangle(brd, rowBounds);
                    if ((this.nlIcons != null) && (imIndex >= 0))
                        e.Graphics.DrawImage(this.nlIcons.Images[imIndex], rowBounds.Left + 1, rowBounds.Top);
                    else
                        e.Graphics.DrawImage(ImageNone, rowBounds.Left + 1, rowBounds.Top);
                    e.Graphics.DrawString(e.SubItem.Text, listView.Font, SystemBrushes.WindowText, bounds);
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

        private void listView2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)1)
            {
                for (int i = 0; i < listView2.Items.Count; i++)
                    listView2.Items[i].Selected = true;
                return;
            };

            if (listView2.SelectedItems.Count == 0) return;

            if (char.IsDigit(e.KeyChar))
            {
                digit_input += new string(new char[] { e.KeyChar });

                if (String.IsNullOrEmpty(digit_input)) return;
                int dig = -1;
                if (!int.TryParse(digit_input, out dig)) return;
                if (dig < 0) { digit_input = ""; return; };
                if (dig >= nlTexts.Count) { dig = dig % 10; digit_input = dig.ToString(); };
                if (dig >= nlTexts.Count) { digit_input = ""; return; };

                if (wptTYPE == 0)
                {
                    if (dig >= 20) dig = dig % 20;
                    listView2.SelectedItems[0].SubItems[1].Text = ((KMZRebuilder.ProGorodPOI.TType)dig).ToString();
                }
                else if(wptTYPE == 1)
                {
                    if (dig > 22) dig = dig % 22;
                    listView2.SelectedItems[0].SubItems[1].Text =((KMZRebuilder.WPTPOI.SymbolIcon)dig).ToString();
                }
                else if (wptTYPE == 2)
                {
                    if (dig >= NavitelRecord.IconList.Length) dig = dig % NavitelRecord.IconList.Length;
                    listView2.SelectedItems[0].SubItems[1].Text = NavitelRecord.IconText(dig);
                };                
                upCurrentSimilar();
            }
            else
                digit_input = "";
        }

        private string digit_input = "";
        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue < 96) digit_input = "";
            if (e.KeyValue > 105) digit_input = "";
            
            if (listView2.SelectedItems.Count != 1) return;

            if (e.KeyValue == 13) listView2_DoubleClick(sender, e);
            if (e.KeyValue == 32) listView2_DoubleClick(sender, e);
            if (e.KeyValue == 46)
            {
                if (wptTYPE == 0)
                    listView2.SelectedItems[0].SubItems[1].Text = ((KMZRebuilder.ProGorodPOI.TType)0).ToString();
                else if (wptTYPE == 2)
                    listView2.SelectedItems[0].SubItems[1].Text = NavitelRecord.IconText(0);
                else
                    listView2.SelectedItems[0].SubItems[1].Text = ((KMZRebuilder.WPTPOI.SymbolIcon)0).ToString();
                upCurrentSimilar();
            };
            if ((e.KeyValue == 37) || (e.KeyValue == 39))
            {
                int index = nlTexts.IndexOf(listView2.SelectedItems[0].SubItems[1].Text);
                if (e.KeyValue == 37) index -= 1;
                if (e.KeyValue == 39) index += 1;
                if (wptTYPE == 0)
                {
                    if (index < 0) index = 19;
                    if (index > 19) index = 0;
                    listView2.SelectedItems[0].SubItems[1].Text = ((KMZRebuilder.ProGorodPOI.TType)index).ToString();
                }
                else if(wptTYPE == 1)
                {
                    if (index < 0) index = 22;
                    if (index > 22) index = 0;
                    listView2.SelectedItems[0].SubItems[1].Text = ((KMZRebuilder.WPTPOI.SymbolIcon)index).ToString();
                }
                else if (wptTYPE == 2)
                {
                    if (index < 0) index = NavitelRecord.IconList.Length - 1;
                    if (index >= NavitelRecord.IconList.Length) index = 0;
                    listView2.SelectedItems[0].SubItems[1].Text = NavitelRecord.IconText(index);
                };
                
                upCurrentSimilar();
            };
        }

        private void upCurrentSimilar()
        {
            if (listView2.SelectedItems.Count == 0) return;
            if (imm == null) return;
            if (imm.Count == 0) return;

            List<string> styles = new List<string>();
            int oldI = listView2.SelectedIndices[0];
            string oldS = listView2.SelectedItems[0].Text;
            foreach (KeyValuePair<uint, IMMR> kv in imm)
                if (kv.Value.styles.Contains(oldS))
                    styles.AddRange(kv.Value.styles);
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                if (oldI == i) continue;
                oldS = listView2.Items[i].Text;
                if (styles.Contains(oldS))
                    listView2.Items[i].SubItems[1].Text = listView2.SelectedItems[0].SubItems[1].Text;
            };
        }

        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            if (listView2.Items.Count == 0) return;
            if (listView2.SelectedItems.Count != 1) return;

            string txt = listView2.SelectedItems[0].SubItems[1].Text;
            string TTP = "";
            if (wptTYPE == 0) TTP = "PROGOROD";
            if (wptTYPE == 1) TTP = "WPT";
            if (wptTYPE == 2) TTP = "GDB";
            if (InputBox.Show("POI Type", "Enter " + TTP + " Type:", nlTexts.ToArray(), ref txt, this.nlIcons == null ? null : this.nlIcons) == DialogResult.OK)
                listView2.SelectedItems[0].SubItems[1].Text = txt;
            upCurrentSimilar();
        }

        private void ïðèìåíèòüÊîÂñåìToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.Items.Count == 0) return;

            string txt = "";
            if (listView2.SelectedItems.Count == 1) txt = listView2.SelectedItems[0].SubItems[1].Text;
            string TTP = "";
            if (wptTYPE == 0) TTP = "PROGOROD";
            if (wptTYPE == 1) TTP = "WPT";
            if (wptTYPE == 2) TTP = "GDB";
            if (InputBox.Show("POI Type", "Enter " + TTP + " Type:", nlTexts.ToArray(), ref txt, this.nlIcons == null ? null : this.nlIcons) == DialogResult.OK)
            {
                for(int i=0;i<listView2.Items.Count;i++)
                    listView2.Items[i].SubItems[1].Text = txt;
            };
        }

        private void autoFillByImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectByStyleHistory(false);
        }

        public void Autodetect()
        {
            DetectByImageTags(true);
            DetectByImageMapCRC(true);
            DetectByStyleHistory(true);
        }

        private void DetectByStyleHistory(bool auto)
        {            
            if (listView2.Items.Count == 0) return;
            if (String.IsNullOrEmpty(stylehistory)) return;

            string[] history = stylehistory.Split(new char[] { ';' });
            foreach (string h in history)
            {
                string[] was = h.Split(new char[] { '=' });
                for (int i = 0; i < listView2.Items.Count; i++)
                {
                    string st = listView2.Items[i].SubItems[0].Text.Substring(1);
                    if (st == was[0])
                    {
                        if (was[1].StartsWith("progorod"))
                        {
                            int id = 0;
                            int.TryParse(was[1].Substring(was[1].Length - 2), out id);
                            if(!auto)
                                listView2.Items[i].SubItems[1].Text = ((ProGorodPOI.TType)id).ToString();
                            else if(listView2.Items[i].SubItems[1].Text != ((ProGorodPOI.TType)0).ToString())
                                listView2.Items[i].SubItems[1].Text = ((ProGorodPOI.TType)id).ToString();
                        };
                        if (was[1].StartsWith("gdb"))
                        {
                            int id = 0;
                            int.TryParse(was[1].Substring(was[1].Length - 3), out id);
                            if (id < 0) id = 0;
                            if (!auto)
                                listView2.Items[i].SubItems[1].Text = NavitelRecord.IconText(id);
                            else if (listView2.Items[i].SubItems[1].Text != NavitelRecord.IconText(0))
                                listView2.Items[i].SubItems[1].Text = NavitelRecord.IconText(id);
                        };
                        if (was[1].StartsWith("wpt"))
                        {
                            int id = 0;
                            int.TryParse(was[1].Substring(was[1].Length - 2), out id);
                            if (id < 0) id = 3;
                            if (!auto)
                                listView2.Items[i].SubItems[1].Text = ((WPTPOI.SymbolIcon)id).ToString();
                            else if (listView2.Items[i].SubItems[1].Text != ((WPTPOI.SymbolIcon)3).ToString())
                                listView2.Items[i].SubItems[1].Text = ((WPTPOI.SymbolIcon)id).ToString();
                        };
                    };
                };
            };
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            autoFillByImageToolStripMenuItem.Visible = !String.IsNullOrEmpty(this.stylehistory);
            autodetectByImageToolStripMenuItem.Visible = !String.IsNullOrEmpty(this.imagesdir);
            autodetectByInternalImageMapCRCTableToolStripMenuItem.Visible = IMM.IsInternalFileExists;
            autodetectByImageComparationToolStripMenuItem.Visible = !String.IsNullOrEmpty(this.imagesdir);
        }

        private void autodetectByImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectByImageTags(false);
        }

        private void DetectByImageTags(bool auto)
        {
            if (listView2.Items.Count == 0) return;
            if (String.IsNullOrEmpty(this.imagesdir)) return;

            for (int i = 0; i < listView2.Items.Count; i++)
            {
                string st = listView2.Items[i].SubItems[0].Text.Substring(1);
                st = imagesdir + @"\" + st + ".png";
                if (System.IO.File.Exists(st))
                {
                    ImageMagick.MagickImage a = new ImageMagick.MagickImage(st);                    
                    double minerr = double.MaxValue;

                    if (wptTYPE == 0)
                    {
                        int id = 0;
                        string attr = null;
                        try { attr = a.GetAttribute("progorod"); } catch { };

                        if (!String.IsNullOrEmpty(attr) && (int.TryParse(attr, out id)))
                            listView2.Items[i].SubItems[1].Text = ((ProGorodPOI.TType)id).ToString();
                    };
                    if (wptTYPE == 1)
                    {
                        int id = 0;
                        string attr = null;
                        try { attr = a.GetAttribute("wpt"); }
                        catch { };

                        if (!String.IsNullOrEmpty(attr) && (int.TryParse(attr, out id)))
                            listView2.Items[i].SubItems[1].Text = ((WPTPOI.SymbolIcon)id).ToString();
                    };
                    if (wptTYPE == 2)
                    {
                        int id = 0;
                        string attr = null;
                        try { attr = a.GetAttribute("gdb"); }
                        catch { };

                        if (!String.IsNullOrEmpty(attr) && (int.TryParse(attr, out id)))
                            listView2.Items[i].SubItems[1].Text = NavitelRecord.IconText(id);                        
                    };
                    a.Dispose();                    
                };
            };
        }

        private void DetectByImageComparation()
        {
            if (listView2.Items.Count == 0) return;
            if (String.IsNullOrEmpty(this.imagesdir)) return;

            CRC32 crc = new CRC32();
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                string st = listView2.Items[i].SubItems[0].Text.Substring(1);
                st = imagesdir + @"\" + st + ".png";
                if (System.IO.File.Exists(st))
                {
                    uint crca = crc.CRC32Num(st); // CRC OF STYLED
                    long lena = (new FileInfo(st)).Length; // LEN OF STYLED
                    ImageMagick.MagickImage a = new ImageMagick.MagickImage(st);                                        
                    double minerr = double.MaxValue;

                    if (wptTYPE == 0)
                    {
                        int id = -1;
                        string zipFile = KMZRebuilederForm.CurrentDirectory() + @"\gdbicons\progorod.zip";
                        if (File.Exists(zipFile))
                            for (int c = 0; c < 20; c++)
                            {
                                byte[] data = KMFile.GetFileFromZip(zipFile, "progorod" + c.ToString("00") + ".png");
                                if ((lena == data.Length) && (crca == crc.CRC32Num(data))) // LEN OF ZIP_IMAGE & CRC OF ZIP_IMAGE
                                {
                                    minerr = 0;
                                    id = c;
                                    c = int.MaxValue - 1;
                                }
                                else
                                {
                                    ImageMagick.MagickImage b = new ImageMagick.MagickImage(data);
                                    double d = a.Compare(b, ImageMagick.ErrorMetric.MeanAbsolute);
                                    b.Dispose();
                                    if (d < minerr)
                                    {
                                        minerr = d;
                                        id = c;
                                    };
                                };
                            };
                        if (id >= 0)
                            listView2.Items[i].SubItems[1].Text = ((ProGorodPOI.TType)id).ToString();
                    };
                    if (wptTYPE == 1)
                    {
                        int id = -1;
                        string zipFile = KMZRebuilederForm.CurrentDirectory() + @"\gdbicons\wpt_icons.zip";
                        if (File.Exists(zipFile))
                            for (int c = 0; c <= 22; c++)
                            {
                                byte[] data = KMFile.GetFileFromZip(zipFile, c.ToString("00") + ".png");
                                if ((lena == data.Length) && (crca == crc.CRC32Num(data))) // LEN OF ZIP_IMAGE & CRC OF ZIP_IMAGE
                                {
                                    minerr = 0;
                                    id = c;
                                    c = int.MaxValue - 1;
                                }
                                else
                                {
                                    ImageMagick.MagickImage b = new ImageMagick.MagickImage(data);
                                    double d = a.Compare(b, ImageMagick.ErrorMetric.MeanAbsolute);
                                    b.Dispose();
                                    if (d < minerr)
                                    {
                                        minerr = d;
                                        id = c;
                                    };
                                };
                            };
                        if (id >= 0)
                            listView2.Items[i].SubItems[1].Text = ((WPTPOI.SymbolIcon)id).ToString();
                    };
                    if (wptTYPE == 2)
                    {
                        int id = -1;
                        string zipFile = KMZRebuilederForm.CurrentDirectory() + @"\gdbicons\gdb_icons.zip";
                        if (File.Exists(zipFile))
                            for (int c = 0; c < NavitelRecord.IconList.Length; c++)
                            {
                                byte[] data = KMFile.GetFileFromZip(zipFile, c.ToString("000") + ".png");
                                if ((lena == data.Length) && (crca == crc.CRC32Num(data))) // LEN OF ZIP_IMAGE & CRC OF ZIP_IMAGE
                                {
                                    minerr = 0;
                                    id = c;
                                    c = int.MaxValue - 1;
                                }
                                else
                                {
                                    ImageMagick.MagickImage b = new ImageMagick.MagickImage(data);
                                    double d = a.Compare(b, ImageMagick.ErrorMetric.MeanAbsolute);
                                    b.Dispose();
                                    if (d < minerr)
                                    {
                                        minerr = d;
                                        id = c;
                                    };
                                };
                            };
                        if (id >= 0)
                            listView2.Items[i].SubItems[1].Text = NavitelRecord.IconText(id);
                    };
                    a.Dispose();
                };
            };
        }

        private void RenameDat_Shown(object sender, EventArgs e)
        {
            if (wptTYPE != 0) return;
            if (listView2.Items.Count == 0) return;

            for(int i=0;i<listView2.Items.Count;i++)
                if (listView2.Items[i].SubItems[0].Text.StartsWith("#progorod"))
                {
                    int id = 0;
                    int.TryParse(listView2.Items[i].SubItems[0].Text.Substring(listView2.Items[i].SubItems[0].Text.Length - 2), out id);
                    listView2.Items[i].SubItems[1].Text = ((ProGorodPOI.TType)id).ToString();
                };            
        }

        private void svimm_CheckedChanged(object sender, EventArgs e)
        {
            this.imm.save2file = svimm.Checked;
        }

        // wptTYPE = 0; // 0 - DAT; 1 - WPT; 2 - GDB
        public void ApplyFromMap(bool auto)
        {
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                string oldS = listView2.Items[i].SubItems[0].Text;
                foreach (KeyValuePair<uint, IMMR> kv in imm)
                    if (kv.Value.styles.Contains(oldS))
                    {
                        if ((wptTYPE == 0) && (kv.Value.dat >= 0))
                        {
                            if ((!auto) || (listView2.Items[i].SubItems[1].Text == nlTexts[0])) // def = 0
                                listView2.Items[i].SubItems[1].Text = nlTexts[kv.Value.dat];
                        }
                        if ((wptTYPE == 1) && (kv.Value.wpt >= 0))
                        {
                            if ((!auto) || (listView2.Items[i].SubItems[1].Text == nlTexts[3])) // def = 3
                                listView2.Items[i].SubItems[1].Text = nlTexts[kv.Value.wpt];
                        }
                        if ((wptTYPE == 2) && (kv.Value.gdb >= 0))
                        {
                            if ((!auto) || (listView2.Items[i].SubItems[1].Text == nlTexts[0])) // def = 0
                                listView2.Items[i].SubItems[1].Text = nlTexts[kv.Value.gdb];
                        };
                    };
            };
        }

        // wptTYPE = 0; // 0 - DAT; 1 - WPT; 2 - GDB
        public void ApplyToMap()
        {
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                string oldS = listView2.Items[i].SubItems[0].Text;
                int newS = nlTexts.IndexOf(listView2.Items[i].SubItems[1].Text);
                foreach(KeyValuePair<uint,IMMR> kv in imm)
                    if (kv.Value.styles.Contains(oldS))
                    {
                        if (wptTYPE == 0) kv.Value.dat = newS;
                        if (wptTYPE == 1) kv.Value.wpt = newS;
                        if (wptTYPE == 2) kv.Value.gdb = newS;
                    };
            };
        }

        private void saveImageMapToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ApplyToMap();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Image Map";
            sfd.DefaultExt = ".imm";
            sfd.Filter = "Image Map File (*.imm)|*.imm";
            if (sfd.ShowDialog() == DialogResult.OK)
                imm.Save(sfd.FileName);
            sfd.Dispose();
        }

        private void loadImageMapFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load Image Map";
            ofd.DefaultExt = ".imm";
            ofd.Filter = "Image Map File (*.imm)|*.imm";
            if (ofd.ShowDialog() == DialogResult.OK)
                imm.UpdateFromFile(ofd.FileName);
            ofd.Dispose();
            ApplyFromMap(false);
        }

        private void RenameDat_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK) return;
            ApplyToMap();
            imm.Save2Internal();            
        }

        private void RenameDat_Load(object sender, EventArgs e)
        {
            
        }

        private void DetectByImageMapCRC(bool auto)
        {
            imm.UpdateFromInternal();
            ApplyFromMap(auto);
        }

        private void autodetectByInternalImageMapCRCTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectByImageMapCRC(false);
        }

        private void autodetectByImageComparationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetectByImageComparation();
        }
    }

    public class IMM : Dictionary<uint,IMMR>
    {
        public bool save2file = false;

        public void Save(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<imagemap>");
            foreach (KeyValuePair<uint, IMMR> kv in this)
                sw.WriteLine(String.Format(" <im crc=\"{0}\" dat=\"{1}\" gdb=\"{2}\" wpt=\"{3}\"/>", kv.Key, kv.Value.dat, kv.Value.gdb, kv.Value.wpt));
            sw.WriteLine("</imagemap>");
            sw.Close();
            fs.Close();
        }        
        public static IMM LoadFromFile(string fileName)
        {
            IMM imm = new IMM();
            XmlDocument xd = new XmlDocument();
            xd.Load(fileName);
            XmlNodeList nl = xd.SelectNodes("imagemap/im");
            foreach (XmlNode xn in nl)
                imm.Set(uint.Parse(xn.Attributes["crc"].Value), new IMMR(int.Parse(xn.Attributes["dat"].Value), int.Parse(xn.Attributes["wpt"].Value), int.Parse(xn.Attributes["gdb"].Value)));
            return imm;
        }
        public static IMM LoadFromInternal()
        {
            string fn = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.imm";
            if (File.Exists(fn))
                return IMM.LoadFromFile(fn);
            else
                return new IMM();
        }
        public void UpdateFromFile(string fileName)
        {
            IMM imm = IMM.LoadFromFile(fileName);
            foreach (KeyValuePair<uint, IMMR> kv in imm)
                this.Update(kv.Key, kv.Value);
        }
        public void UpdateFromInternal()
        {
            string fn = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.imm";
            if (File.Exists(fn))
                this.UpdateFromFile(fn);
        }
        public void Update(uint crc, IMMR map)
        {
            if (this.ContainsKey(crc))
            {
                foreach(string st in map.styles)
                    if (!map.styles.Contains(st)) this[crc].styles.Add(st);
                if (map.dat >= 0) this[crc].dat = map.dat;
                if (map.gdb >= 0) this[crc].gdb = map.gdb;
                if (map.wpt >= 0) this[crc].wpt = map.wpt;
            }
            else
                this.Add(crc, map);
        }
        public void Update(IMM imm)
        {
            foreach (KeyValuePair<uint, IMMR> kv in imm)
                this.Update(kv.Key, kv.Value);
        }
        public IMMR Get(uint crc)
        {
            if (this.ContainsKey(crc))
                return this[crc];
            else
                return null;
        }
        public void Set(uint crc, IMMR map)
        {
            if (this.ContainsKey(crc))
            {
                this[crc].dat = map.dat;
                this[crc].wpt = map.wpt;
                this[crc].gdb = map.gdb;
                foreach (string st in map.styles)
                    if (!this[crc].styles.Contains(st)) this[crc].styles.Add(st);
            }
            else
                this.Add(crc, map);
        }
        public void Set(uint crc, string style)
        {
            if (this.ContainsKey(crc))
            {
                if (!this[crc].styles.Contains(style)) this[crc].styles.Add(style);
            }
            else
                this.Add(crc, new IMMR(style));
        }
        public void Set(uint crc, int dat, int wpt, int gdb)
        {
            if (this.ContainsKey(crc))
            {
                this[crc].dat = dat;
                this[crc].wpt = wpt;
                this[crc].gdb = gdb;
            }
            else
                this.Add(crc, new IMMR(dat, wpt, gdb));
        }
        public void Set(uint crc, string style, int dat, int wpt, int gdb)
        {
            if (this.ContainsKey(crc))
            {
                this[crc].dat = dat;
                this[crc].wpt = wpt;
                this[crc].gdb = gdb;
                if (!this[crc].styles.Contains(style)) this[crc].styles.Add(style);
            }
            else
                this.Add(crc, new IMMR(style, dat, wpt, gdb));
        }

        public static bool IsInternalFileExists
        {
            get
            {
                string fn = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.imm";
                return File.Exists(fn);
            }
        }

        public void Save2Internal()
        {
            string fn = KMZRebuilederForm.CurrentDirectory() + @"\KMZRebuilder.imm";
            if (File.Exists(fn))
            {
                IMM cimm = IMM.LoadFromFile(fn);
                cimm.Update(this);
                cimm.Save(fn);
            }
            else
                this.Save(fn);
        }        
    }

    public class IMMR
    {
        public int dat = -1;
        public int wpt = -1;
        public int gdb = -1;
        public List<string> styles = new List<string>();

        public IMMR()
        {            
        }

        public IMMR(string style)
        {
            this.styles.Add(style);
        }

        public IMMR(int dat, int wpt, int gdb)
        {
            this.dat = dat;
            this.wpt = wpt;
            this.gdb = gdb;
        }

        public IMMR(string style, int dat, int wpt, int gdb)
        {
            this.styles.Add(style);
            this.dat = dat;
            this.wpt = wpt;
            this.gdb = gdb;
        }

        public override string ToString()
        {
            return String.Format("d[{0}] w[{1}] g[{2}]", dat, wpt, gdb);
        }
    }

    public class CRC32
    {
        private const uint poly = 0xEDB88320;
        private uint[] checksumTable;

        public CRC32()
        {
            checksumTable = new uint[256];
            for (uint index = 0; index < 256; index++)
            {
                uint el = index;
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((el & 1) != 0)
                        el = (poly ^ (el >> 1));
                    else
                        el = (el >> 1);
                };
                checksumTable[index] = el;
            };
        }

        public uint CRC32Num(byte[] data)
        {
            uint res = 0xFFFFFFFF;
            for (int i = 0; i < data.Length; i++)
                res = checksumTable[(res & 0xFF) ^ (byte)data[i]] ^ (res >> 8);
            return ~res;
        }

        public uint CRC32Num(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] arr = new byte[fs.Length];
            fs.Read(arr, 0, arr.Length);
            fs.Close();
            return CRC32Num(arr);
        }

        public byte[] CRC32Arr(byte[] data, bool isLittleEndian)
        {
            uint res = CRC32Num(data);
            byte[] hash = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (isLittleEndian)
                    hash[i] = (byte)((res >> (24 - i * 8)) & 0xFF);
                else
                    hash[i] = (byte)((res >> (i * 8)) & 0xFF);
            };
            return hash;
        }
    }  
}