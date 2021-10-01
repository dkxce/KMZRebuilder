using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace KMZRebuilder
{
    public partial class AddTextAsPoly : Form
    {
        private string FontsDirectory;

        public AddTextAsPoly(string FontsDirectory)
        {
            InitializeComponent();

            TextAlign.SelectedIndex = 0;
            AFill.SelectedIndex = 0;
            DialogResult = DialogResult.Cancel;

            this.FontsDirectory = FontsDirectory;

            LoadFonts();            
        }

        public void LoadFonts()
        {
            using (InstalledFontCollection fontsCollection = new InstalledFontCollection())
            {
                FontFamily[] fontFamilies = fontsCollection.Families;
                List<string> fonts = new List<string>();
                foreach (FontFamily font in fontFamilies)
                {
                    fontSysList.Items.Add(new FontRec(font));
                    if (font.Name == "Arial")
                        fontSysList.SelectedIndex = fontSysList.Items.Count - 1;
                    if (font.Name == "Times New Roman")
                        fontSysList.SelectedIndex = fontSysList.Items.Count - 1;
                    if (font.Name == "MS Sans Serif")
                        fontSysList.SelectedIndex = fontSysList.Items.Count - 1;
                };
                if(fontSysList.Items.Count > 0)
                    if (fontSysList.SelectedIndex < 0)
                        fontSysList.SelectedIndex = 0;
            };
            PrivateFontCollection pfc = new PrivateFontCollection();            
            if ((!String.IsNullOrEmpty(FontsDirectory)) && Directory.Exists(FontsDirectory))
            {
                string[] files = Directory.GetFiles(FontsDirectory, "*.ttf");
                if(files.Length > 0)
                    for (int i = 0; i < files.Length; i++)
                        pfc.AddFontFile(files[i]);
            };
            if (pfc.Families.Length > 0)
                for (int i = 0; i < pfc.Families.Length; i++)
                {
                    fontCustomList.Items.Add(new FontRec(pfc.Families[i]));
                    if (pfc.Families[i].Name == "PT Serif Caption")
                        fontCustomList.SelectedIndex = fontCustomList.Items.Count - 1;
                };
            if (fontCustomList.Items.Count > 0)
                if (fontCustomList.SelectedIndex < 0)
                    fontCustomList.SelectedIndex = 0;
        }

        public class FontRec
        {
            public FontFamily font;
            public FontRec(FontFamily font) { this.font = font; }
            public override string ToString()
            {
                string styles = "";
                if (font.IsStyleAvailable(FontStyle.Regular))
                    styles += "Regular";
                if (font.IsStyleAvailable(FontStyle.Bold))
                {
                    if (styles.Length > 0) styles += ", ";
                    styles += "Bold";
                };
                if (font.IsStyleAvailable(FontStyle.Italic))
                {
                    if (styles.Length > 0) styles += ", ";
                    styles += "Italic";
                };
                if (styles.Length > 0) styles = " [" + styles + "]";
                return font.Name + styles;
            }
        }

        private void selFont_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Font";
            ofd.DefaultExt = ".ttf";
            ofd.Filter = "True Type Fonts (*.ttf)|*.ttf";
            if (!String.IsNullOrEmpty(FontsDirectory))
                if (Directory.Exists(FontsDirectory))
                    ofd.InitialDirectory = FontsDirectory;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                PrivateFontCollection pfc = new PrivateFontCollection();
                pfc.AddFontFile(ofd.FileName);
                fontCustomList.Items.Add(new FontRec(pfc.Families[0]));
                fontCustomList.SelectedIndex = fontCustomList.Items.Count - 1;
            };
            ofd.Dispose();
        }

        private void LColor_TextChanged(object sender, EventArgs e)
        {
            string hex = (sender as MaskedTextBox).Text;
            if (Regex.IsMatch(hex, @"^(#[\dA-Fa-f]{0,6})$"))
                LBColor.BackColor = RGBConverter(hex);
        }

        private void LColor_Validating(object sender, CancelEventArgs e)
        {
            string hex = (sender as MaskedTextBox).Text;
            e.Cancel = !Regex.IsMatch(hex, @"^(#[\dA-Fa-f]{0,6})$");
        }

        private void AColor_TextChanged(object sender, EventArgs e)
        {
            string hex = (sender as MaskedTextBox).Text;
            if (Regex.IsMatch(hex, @"^(#[\dA-Fa-f]{0,6})$"))
                ABColor.BackColor = RGBConverter(hex);
        }

        private void LBColor_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.FullOpen = true;
            cd.Color = RGBConverter(LColor.Text);
            if (cd.ShowDialog() == DialogResult.OK)
            {
                LColor.Text = HexConverter(Color.FromArgb(255, cd.Color));
                LBColor.BackColor = Color.FromArgb(255, cd.Color);
            };
            cd.Dispose();
        }

        private void ABColor_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.FullOpen = true;
            cd.Color = RGBConverter(AColor.Text);
            if (cd.ShowDialog() == DialogResult.OK)
            {
                AColor.Text = HexConverter(Color.FromArgb(255, cd.Color));
                ABColor.BackColor = Color.FromArgb(255, cd.Color);
            };
            cd.Dispose();
        }

        public static String HexConverter(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static String HexStyleConverter(Color c)
        {
            return c.A.ToString("X2") + c.B.ToString("X2") + c.G.ToString("X2") + c.R.ToString("X2");
        }

        public static Color RGBConverter(string hex)
        {
            Color rtn = Color.Black;
            try
            {
                return Color.FromArgb(
                    int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber));
            }
            catch (Exception ex)
            {
                //doing nothing
            }

            return rtn;
        }

        private void fontSysList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Visible)
                fontSystem.Checked = true;
        }

        private void fontCustomList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Visible)
                fontCustom.Checked = true;
        }

        private void TextAlign_SelectedIndexChanged(object sender, EventArgs e)
        {
            Image im = new Bitmap(21, 21);
            Graphics g = Graphics.FromImage(im);
            g.FillRectangle(new SolidBrush(this.BackColor), 0, 0, 21, 21);
            g.FillRectangle(Brushes.White, 0, 0, 20, 20);            
            g.DrawRectangle(Pens.Black, 0, 0, 20, 20);
            switch (TextAlign.SelectedIndex)
            {
                case 0: g.FillRectangle(Brushes.Black, 9, 9, 4, 4); break;
                case 1: g.FillRectangle(Brushes.Black, 9, 2, 4, 4); break;
                case 2: g.FillRectangle(Brushes.Black, 15, 2, 4, 4); break;
                case 3: g.FillRectangle(Brushes.Black, 15, 9, 4, 4); break;
                case 4: g.FillRectangle(Brushes.Black, 15, 15, 4, 4); break;
                case 5: g.FillRectangle(Brushes.Black, 9, 15, 4, 4); break;
                case 6: g.FillRectangle(Brushes.Black, 2, 15, 4, 4); break;
                case 7: g.FillRectangle(Brushes.Black, 2, 9, 4, 4); break;
                case 8: g.FillRectangle(Brushes.Black, 2, 2, 4, 4); break;
            };
            g.Dispose();
            pb.Image = im;
        }     
    }
}