using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace KMZRebuilder
{
    public partial class LineAreaStyleForm : Form
    {
        public LineAreaStyleForm()
        {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }

        private void LColor_Validating(object sender, CancelEventArgs e)
        {
            string hex = (sender as MaskedTextBox).Text;
            e.Cancel = !Regex.IsMatch(hex, @"^(#[\dA-Fa-f]{0,6})$");
        }   

        private void LColor_TextChanged(object sender, EventArgs e)
        {
            string hex = (sender as MaskedTextBox).Text;
            if (Regex.IsMatch(hex, @"^(#[\dA-Fa-f]{0,6})$"))
                LBColor.BackColor = RGBConverter(hex);
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
    }
}