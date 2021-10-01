using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KMZRebuilder
{
    public partial class WGSFormX : Form
    {
        public WGSFormX()
        {
            InitializeComponent();
            if (dsep.Items.IndexOf(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) < 0)
                dsep.Items.Add(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);                        
            dsep.SelectedIndex = 0;            
        }

        private void WGSFormX_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        private void LatN_Validating(object sender, CancelEventArgs e)
        {
            if (sender == null) return;
            TextBox box = (sender as TextBox);
            switch (box.Name)
            {
                case "LatN":
                case "LatD":
                case "LatM":
                case "LatS":
                    UpdateAll(box.Text, true, false);
                    break;
                case "LonN":
                case "LonD":
                case "LonM":
                case "LonS":
                    UpdateAll(box.Text, false, true);
                    break;
                default:
                    UpdateAll(box.Text, true, true);
                    break;
            };
        }

        private void UpdateAll(string text, bool lat, bool lon)
        {
            PointD parsed = new PointD();
            if (Separator != ".")
                text = text.Replace(Separator, ".");
            if (lat && lon)
            {
                parsed = LatLonParser.Parse(text);
            }
            else if (lat)
            {
                parsed.Y = LatLonParser.Parse(text, true);
                string txt2 = LonN.Text;
                if (Separator != ".")
                    txt2 = txt2.Replace(Separator, ".");
                parsed.X = LatLonParser.Parse(txt2, false);
            }
            else
            {
                parsed.X = LatLonParser.Parse(text, false);
                string txt2 = LatN.Text;
                if (Separator != ".")
                    txt2 = txt2.Replace(Separator, ".");
                parsed.Y = LatLonParser.Parse(txt2, true);
            };

            if (Separator == ".")
            {
                LatN.Text = parsed.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
                LatD.Text = LatLonParser.GetLinePrefix(parsed.Y, LatLonParser.DFormat.ENG_NS) + LatLonParser.ToString(parsed.Y, LatLonParser.FFormat.DDDDDD);
                LatM.Text = LatLonParser.GetLinePrefix(parsed.Y, LatLonParser.DFormat.ENG_NS) + LatLonParser.ToString(parsed.Y, LatLonParser.FFormat.DDMMMM);
                LatS.Text = LatLonParser.GetLinePrefix(parsed.Y, LatLonParser.DFormat.ENG_NS) + LatLonParser.ToString(parsed.Y, LatLonParser.FFormat.DDMMSS);
                LonN.Text = parsed.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
                LonD.Text = LatLonParser.GetLinePrefix(parsed.X, LatLonParser.DFormat.ENG_EW) + LatLonParser.ToString(parsed.X, LatLonParser.FFormat.DDDDDD);
                LonM.Text = LatLonParser.GetLinePrefix(parsed.X, LatLonParser.DFormat.ENG_EW) + LatLonParser.ToString(parsed.X, LatLonParser.FFormat.DDMMMM);
                LonS.Text = LatLonParser.GetLinePrefix(parsed.X, LatLonParser.DFormat.ENG_EW) + LatLonParser.ToString(parsed.X, LatLonParser.FFormat.DDMMSS);
                BothN.Text = LatLonParser.ToString(parsed);
                BothD.Text = LatLonParser.ToString(parsed, LatLonParser.FFormat.DDDDDD);
                BothM.Text = LatLonParser.ToString(parsed, LatLonParser.FFormat.DDMMMM);
                BothS.Text = LatLonParser.ToString(parsed, LatLonParser.FFormat.DDMMSS);
                Multi.Text =
                    LatLonParser.GetLinePrefix(parsed.Y, LatLonParser.DFormat.ENG_NS) +
                    LatLonParser.ToString(parsed.Y, LatLonParser.FFormat.DDMMSS) +
                    "\r\n" +
                    LatLonParser.GetLinePrefix(parsed.X, LatLonParser.DFormat.ENG_EW) +
                    LatLonParser.ToString(parsed.X, LatLonParser.FFormat.DDMMSS)
                    ;
            }
            else
            {
                LatN.Text = parsed.Y.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(".", Separator);
                LatD.Text = LatLonParser.GetLinePrefix(parsed.Y, LatLonParser.DFormat.ENG_NS) + LatLonParser.ToString(parsed.Y, LatLonParser.FFormat.DDDDDD).Replace(".", Separator);
                LatM.Text = LatLonParser.GetLinePrefix(parsed.Y, LatLonParser.DFormat.ENG_NS) + LatLonParser.ToString(parsed.Y, LatLonParser.FFormat.DDMMMM).Replace(".", Separator);
                LatS.Text = LatLonParser.GetLinePrefix(parsed.Y, LatLonParser.DFormat.ENG_NS) + LatLonParser.ToString(parsed.Y, LatLonParser.FFormat.DDMMSS).Replace(".", Separator);
                LonN.Text = parsed.X.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(".", Separator);
                LonD.Text = LatLonParser.GetLinePrefix(parsed.X, LatLonParser.DFormat.ENG_EW) + LatLonParser.ToString(parsed.X, LatLonParser.FFormat.DDDDDD).Replace(".", Separator);
                LonM.Text = LatLonParser.GetLinePrefix(parsed.X, LatLonParser.DFormat.ENG_EW) + LatLonParser.ToString(parsed.X, LatLonParser.FFormat.DDMMMM).Replace(".", Separator);
                LonS.Text = LatLonParser.GetLinePrefix(parsed.X, LatLonParser.DFormat.ENG_EW) + LatLonParser.ToString(parsed.X, LatLonParser.FFormat.DDMMSS).Replace(".", Separator);
                BothN.Text = LatLonParser.ToString(parsed).Replace(",",";").Replace(".",",");
                BothD.Text = LatLonParser.ToString(parsed, LatLonParser.FFormat.DDDDDD).Replace(".", Separator);
                BothM.Text = LatLonParser.ToString(parsed, LatLonParser.FFormat.DDMMMM).Replace(".", Separator);
                BothS.Text = LatLonParser.ToString(parsed, LatLonParser.FFormat.DDMMSS).Replace(".", Separator);
                Multi.Text =
                    LatLonParser.GetLinePrefix(parsed.Y, LatLonParser.DFormat.ENG_NS) +
                    LatLonParser.ToString(parsed.Y, LatLonParser.FFormat.DDMMSS).Replace(".", Separator) +
                    "\r\n" +
                    LatLonParser.GetLinePrefix(parsed.X, LatLonParser.DFormat.ENG_EW) +
                    LatLonParser.ToString(parsed.X, LatLonParser.FFormat.DDMMSS).Replace(".", Separator)
                    ;
            };
        }

        private void X()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];

                IntPtr hSysMenu = KMZRebuilederForm.GetSystemMenu(p.MainWindowHandle, false);
                KMZRebuilederForm.AppendMenu(hSysMenu, 0x800, 0, string.Empty);
                KMZRebuilederForm.AppendMenu(hSysMenu, 0x0, 0x1, "KMZRebuilder ...");

                IntPtr tp0 = IntPtr.Zero;
                while ((tp0 = SASPlacemarkConnector.FindWindowEx(p.MainWindowHandle, tp0, "TImage32", null)) != IntPtr.Zero)
                {
                    SASPlacemarkConnector.TRect rect;
                    SASPlacemarkConnector.GetClientRect(tp0, out rect);
                    IntPtr ImDC = SASPlacemarkConnector.GetDC(tp0);
                    Graphics g = Graphics.FromHdc(ImDC);

                    Image im = new Bitmap(rect.Width, rect.Height);
                    Graphics g2 = Graphics.FromHwnd(this.Handle);
                    SASPlacemarkConnector.BitBlt(g2.GetHdc(), 0, 0, im.Width, im.Height, ImDC, 0, 0, SASPlacemarkConnector.TernaryRasterOperations.SRCCOPY);
                    g2.Dispose();
                };
            };
        }

        private void dsep_SelectedIndexChanged(object sender, EventArgs e)
        {
            LatN.Text = LatN.Text.Replace(",", ".");
            LonN.Text = LonN.Text.Replace(",",".");
            UpdateAll(LatN.Text, true, false);
        }

        private string Separator
        {
            get
            {
                if (String.IsNullOrEmpty(dsep.SelectedItem.ToString()))
                    return ".";
                return dsep.SelectedItem.ToString();
            }
        }
    }
}