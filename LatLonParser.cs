using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace KMZRebuilder
{
    public class PointD
    {
        public double X;
        public double Y;
        public byte Type;

        public PointD() { }

        public PointD(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public PointD(int X, int Y, byte Type)
        {
            this.X = X;
            this.Y = Y;
            this.Type = Type;
        }

        public PointD(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public PointD(float X, float Y, byte Type)
        {
            this.X = X;
            this.Y = Y;
            this.Type = Type;
        }

        public PointD(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public PointD(double X, double Y, byte Type)
        {
            this.X = X;
            this.Y = Y;
            this.Type = Type;
        }

        public PointD(Point point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public PointD(Point point, byte Type)
        {
            this.X = point.X;
            this.Y = point.Y;
            this.Type = Type;
        }

        public PointD(PointF point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        public PointD(PointF point, byte Type)
        {
            this.X = point.X;
            this.Y = point.Y;
            this.Type = Type;
        }

        public PointF PointF
        {
            get
            {
                return new PointF((float)X, (float)Y);
            }
            set
            {
                this.X = value.X;
                this.Y = value.Y;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (X == 0) && (Y == 0);
            }
        }

        public static PointF ToPointF(PointD point)
        {
            return point.PointF;
        }

        public static PointF[] ToPointF(PointD[] points)
        {
            PointF[] result = new PointF[points.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = points[i].PointF;
            return result;
        }
    }

    public class LatLonParser
    {
        public enum FFormat : byte
        {
            None = 0,
            DDDDDD = 1,
            DDMMMM = 2,
            DDMMSS = 3
        }

        public enum DFormat : byte
        {
            ENG_NS = 0,
            ENG_EW = 1,
            RUS_NS = 2,
            RUS_EW = 3,
            MINUS = 4,
            DEFAULT = 4,
            NONE = 4
        }

        public static double ToLat(string line_in)
        {
            return Parse(line_in, true);
        }

        public static double ToLon(string line_in)
        {
            return Parse(line_in, false);
        }

        public static double Parse(string line_in, bool true_lat_false_lon)
        {
            int nn = 1;
            string full = GetCorrectString(line_in, true_lat_false_lon, out nn);
            if (String.IsNullOrEmpty(full)) return 0f;
            string mm = "0";
            string ss = "0";
            string dd = "0";
            if (full.IndexOf("°") > 0)
            {
                int dms = 0;
                int from = 0;
                int next = 0;
                dd = full.Substring(from, (next = full.IndexOf("°", from)) - from);
                from = next + 1;
                if (full.IndexOf("'") > 0)
                {
                    dms = 1;
                    mm = full.Substring(from, (next = full.IndexOf("'", from)) - from);
                    from = next + 1;
                };
                if (full.IndexOf("\"") > 0)
                {
                    dms = 2;
                    ss = full.Substring(from, (next = full.IndexOf("\"", from)) - from);
                    from = next + 1;
                };
                if (from < full.Length)
                {
                    if (dms == 1)
                        ss = full.Substring(from);
                    else if (dms == 0)
                        mm = full.Substring(from);
                };
            }
            else
            {
                bool loop = true;
                double num3 = 0.0;
                int num4 = 1;
                if (full[0] == '-') num4++;
                while (loop)
                {
                    try
                    {
                        num3 = Convert.ToDouble(full.Substring(0, num4++), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        loop = false;
                    };
                    if (num4 > full.Length)
                    {
                        loop = false;
                    };
                }
                dd = num3.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            double d = ((Convert.ToDouble(dd, System.Globalization.CultureInfo.InvariantCulture) + Convert.ToDouble(mm, System.Globalization.CultureInfo.InvariantCulture) / 60.0 + Convert.ToDouble(ss, System.Globalization.CultureInfo.InvariantCulture) / 60.0 / 60.0) * (double)nn);
            return d;
        }

        public static PointD Parse(string line_in)
        {
            return new PointD(ToLon(line_in), ToLat(line_in));
        }

        private static string GetCorrectString(string str, bool lat, out int digit)
        {
            digit = 1;
            if (String.IsNullOrEmpty(str)) return null;

            string text = str.Trim();
            if (String.IsNullOrEmpty(text)) return null;

            text = text.ToLower().Replace("``", "\"").Replace("`", "'").Replace("%20", " ").Trim();
            while (text.IndexOf("  ") >= 0) text = text.Replace("  ", " ");
            text = text.Replace("° ", "°").Replace("' ", "'").Replace("\" ", "\"");
            if (String.IsNullOrEmpty(text)) return null;

            bool hasDigits = false;
            bool noletters = true;
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                    hasDigits = true;
                if (char.IsLetter(text[i]))
                    noletters = false;
            };
            if (!hasDigits) return null;

            if (noletters)
            {
                string[] lalo = text.Split(new char[] { '+', ' ', '=', ';', ',', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (lalo.Length == 0)
                    return null;
                if (lalo.Length == 2)
                {
                    if (lat)
                        text = lalo[0];
                    else
                        text = lalo[1];
                };
            };

            text = text.Replace("+", "").Replace(" ", "").Replace("=", "").Replace(";", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();
            if (String.IsNullOrEmpty(text)) return null;

            double d;
            if (double.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d))
            {
                if (d < 0) digit = -1;
                return text.Replace("-","");
            };

            int copyl = text.Length;
            int find = 0;
            int start = 0;
            bool endsWithLetter = (char.IsLetter(text[text.Length - 1]));

            if (lat)
            {
                if ((find = text.IndexOf("lat")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("latitude")) >= 0) start = find + (endsWithLetter ? 0 : 8);
                if ((find = text.IndexOf("ш")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("шир")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("широта")) >= 0) start = find + (endsWithLetter ? 0 : 6);
                if ((find = text.IndexOf("n")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("с")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("сш")) >= 0) start = find + (endsWithLetter ? 0 : 2);
                if ((find = text.IndexOf("с.ш")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("с.ш.")) >= 0) start = find + (endsWithLetter ? 0 : 4);
                if ((find = text.IndexOf("s")) >= 0) { start = find + (endsWithLetter ? 0 : 1); digit = -1; };
                if ((find = text.IndexOf("ю")) >= 0) { start = find + (endsWithLetter ? 0 : 1); digit = -1; };
                if ((find = text.IndexOf("юш")) >= 0) { start = find + (endsWithLetter ? 0 : 2); digit = -1; };
                if ((find = text.IndexOf("ю.ш")) >= 0) { start = find + (endsWithLetter ? 0 : 3); digit = -1; };
                if ((find = text.IndexOf("ю.ш.")) >= 0) { start = find + (endsWithLetter ? 0 : 4); digit = -1; };
            }
            else
            {
                if ((find = text.IndexOf("lon")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("longitude")) >= 0) start = find + (endsWithLetter ? 0 : 9);
                if ((find = text.IndexOf("д")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("дол")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("долгота")) >= 0) start = find + (endsWithLetter ? 0 : 7);
                if ((find = text.IndexOf("e")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("в")) >= 0) start = find + (endsWithLetter ? 0 : 1);
                if ((find = text.IndexOf("вд")) >= 0) start = find + (endsWithLetter ? 0 : 2);
                if ((find = text.IndexOf("в.д")) >= 0) start = find + (endsWithLetter ? 0 : 3);
                if ((find = text.IndexOf("в.д.")) >= 0) start = find + (endsWithLetter ? 0 : 4);
                if ((find = text.IndexOf("w")) >= 0) { start = find + (endsWithLetter ? 0 : 1); digit = -1; };
                if ((find = text.IndexOf("з")) >= 0) { start = find + (endsWithLetter ? 0 : 1); digit = -1; };
                if ((find = text.IndexOf("зд")) >= 0) { start = find + (endsWithLetter ? 0 : 2); digit = -1; };
                if ((find = text.IndexOf("з.д")) >= 0) { start = find + (endsWithLetter ? 0 : 3); digit = -1; };
                if ((find = text.IndexOf("з.д.")) >= 0) { start = find + (endsWithLetter ? 0 : 4); digit = -1; };
            };

            if (endsWithLetter)
            {
                copyl = start;
                start = 0;
                for (int i = copyl - 1; i >= start; i--)
                    if (char.IsLetter(text[i]))
                        copyl = copyl - (start = i + 1);
            }
            else
            {
                for (int i = start; i < copyl; i++)
                    if (char.IsLetter(text[i]))
                        copyl = i - start;
            };

            if (copyl > (text.Length - start)) copyl -= start;

            text = text.Substring(start, copyl);
            text = text.Replace(",", ".");
            return text;
        }

        public static string ToString(double fvalue)
        {
            return DoubleToString(fvalue, -1);
        }

        public static string ToString(double fvalue, int digitsAfterDelimiter)
        {
            return DoubleToString(fvalue, digitsAfterDelimiter);
        }

        public static string ToString(double lat, double lon)
        {
            return String.Format("{0},{1}", ToString(lat), ToString(lon));
        }

        public static string ToString(double lat, double lon, int digitsAfterDelimiter)
        {
            return String.Format("{0},{1}", DoubleToString(lat, digitsAfterDelimiter), DoubleToString(lon, digitsAfterDelimiter));
        }

        public static string ToString(double lat, double lon, FFormat fformat)
        {
            if (fformat == FFormat.None)
                return String.Format("{0},{1}", ToString(lat, fformat), ToString(lon, fformat));
            else
                return String.Format("{0} {1} {2} {3}", new string[] { GetLinePrefix(lat, DFormat.ENG_NS), ToString(lat, fformat), GetLinePrefix(lat, DFormat.ENG_EW), ToString(lon, fformat) });
        }

        public static string ToString(double lat, double lon, FFormat fformat, int digitsAfterDelimiter)
        {
            if (fformat == FFormat.None)
                return String.Format("{0},{1}", ToString(lat, fformat, digitsAfterDelimiter), ToString(lon, fformat, digitsAfterDelimiter));
            else
                return String.Format("{0} {1} {2} {3}", new string[] { GetLinePrefix(lat, DFormat.ENG_NS), ToString(lat, fformat, digitsAfterDelimiter), GetLinePrefix(lat, DFormat.ENG_EW), ToString(lon, fformat, digitsAfterDelimiter) });
        }

        public static string ToString(PointD latlon)
        {
            return String.Format("{0},{1}", ToString(latlon.Y), ToString(latlon.X));
        }

        public static string ToString(PointD latlon, int digitsAfterDelimiter)
        {
            return String.Format("{0},{1}", DoubleToString(latlon.Y, digitsAfterDelimiter), DoubleToString(latlon.X, digitsAfterDelimiter));
        }

        public static string ToString(PointD latlon, FFormat fformat)
        {
            if (fformat == FFormat.None)
                return String.Format("{0},{1}", ToString(latlon.Y, fformat), ToString(latlon.X, fformat));
            else
                return String.Format("{0} {1} {2} {3}", new string[] { GetLinePrefix(latlon.Y, DFormat.ENG_NS), ToString(latlon.Y, fformat), GetLinePrefix(latlon.X, DFormat.ENG_EW), ToString(latlon.X, fformat) });
        }

        public static string ToString(PointD latlon, FFormat fformat, int digitsAfterDelimiter)
        {
            if (fformat == FFormat.None)
                return String.Format("{0},{1}", ToString(latlon.Y, fformat, digitsAfterDelimiter), ToString(latlon.X, fformat, digitsAfterDelimiter));
            else
                return String.Format("{0} {1} {2} {3}", new string[] { GetLinePrefix(latlon.Y, DFormat.ENG_NS), ToString(latlon.Y, fformat, digitsAfterDelimiter), GetLinePrefix(latlon.X, DFormat.ENG_EW), ToString(latlon.X, fformat, digitsAfterDelimiter) });
        }

        public static string ToString(double fvalue, FFormat format)
        {
            return ToString(fvalue, format, 6);
        }

        public static string ToString(double fvalue, FFormat format, int digitsAfterDelimiter)
        {
            double num = Math.Abs(fvalue);
            string result;
            if (format == FFormat.None)
            {
                result = DoubleToString(num, digitsAfterDelimiter);
            }
            else if (format == FFormat.DDDDDD)
            {
                result = DoubleToString(num, digitsAfterDelimiter) + "°";
            }
            else
            {
                string text = "";
                text = text + DoubleToString(Math.Truncate(num), 0) + "° ";
                double num2 = (num - Math.Truncate(num)) * 60.0;
                if (format == FFormat.DDMMMM)
                {
                    text = text + DoubleToString(num2, 4) + "'";
                }
                else
                {
                    text = text + string.Format("{0}", (int)Math.Truncate(num2)) + "' ";
                    num2 = (num2 - Math.Truncate(num2)) * 60.0;
                    text = text + DoubleToString(num2, 3) + "\"";
                }
                result = text;
            }
            return result;
        }

        public static string GetLinePrefix(double fvalue, DFormat format)
        {
            string result;
            switch ((byte)format)
            {
                case 0:
                    result = ((fvalue >= 0.0) ? "N" : "S");
                    break;
                case 1:
                    result = ((fvalue >= 0.0) ? "E" : "W");
                    break;
                case 2:
                    result = ((fvalue >= 0.0) ? "С" : "Ю");
                    break;
                case 3:
                    result = ((fvalue >= 0.0) ? "В" : "З");
                    break;
                default:
                    result = ((fvalue >= 0.0) ? "" : "-");
                    break;
            }
            return result;
        }

        public static string DoubleToString(double val, int digitsAfterDelimiter)
        {
            if (digitsAfterDelimiter < 0)
                return val.ToString(System.Globalization.CultureInfo.InvariantCulture);

            string daf = "";
            for (int i = 0; i < digitsAfterDelimiter; i++) daf += "0";
            return val.ToString("0." + daf, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string DoubleToStringMax(double val, int maxDigitsAfterDelimiter)
        {
            string res = val.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (maxDigitsAfterDelimiter < 0) return res;
            if (res.IndexOf(".") < 0) return res;
            if ((res.Length - res.IndexOf(".")) <= maxDigitsAfterDelimiter) return res;

            string daf = "";
            for (int i = 0; i < maxDigitsAfterDelimiter; i++) daf += "0";
            return val.ToString("0." + daf, System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public class SASPlacemarkConnector
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct TRect
        {
            public int Left, Top, Right, Bottom;

            public TRect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public TRect(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(TRect r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator TRect(System.Drawing.Rectangle r)
            {
                return new TRect(r);
            }

            public static bool operator ==(TRect r1, TRect r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(TRect r1, TRect r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(TRect r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is TRect)
                    return Equals((TRect)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new TRect((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }

        public enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }

        private delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out TRect lpRect);

        [DllImport("gdi32.dll")]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
            int nWidthDest, int nHeightDest,
            IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, 
            int nWidth, int nHeight, 
            IntPtr hdcSrc, int nXSrc, int nYSrc,
            TernaryRasterOperations dwRop);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);


        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(HandleRef hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(HandleRef hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(int hWnd, int Msg, int wparam, int lparam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wparam, IntPtr lparam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wparam, int lparam);

        //Sets window attributes
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //Gets window attributes
        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetMenu(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll")]
        static extern bool DrawMenuBar(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        private const int SW_HIDE = 0x0000;
        private const int SW_SHOWNORMAL = 0x0001;
        public const int SW_SHOW = 0x0005;
        private const int WM_SETTEXT = 0x000C;
        private const int WM_GETTEXT = 0x000D;
        private const int WM_GETTEXTLENGTH = 0x000E;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        public const uint WM_CHAR = 0x102;
        public const uint WM_LBUTTONDOWN = 0x201;
        public const uint WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;

        private const int MK_LBUTTON = 0x01;
        private const int VK_RETURN = 0x0d;
        private const int VK_ESCAPE = 0x1b;
        private const int VK_TAB = 0x09;
        private const int VK_LEFT = 0x25;
        private const int VK_UP = 0x26;
        private const int VK_RIGHT = 0x27;
        private const int VK_DOWN = 0x28;
        private const int VK_F5 = 0x74;
        private const int VK_F6 = 0x75;
        private const int VK_F7 = 0x76;

        private static uint MF_BYPOSITION = 0x400;
        private static uint MF_REMOVE = 0x1000;
        private static int GWL_STYLE = -16;
        private static int WS_CHILD = 0x40000000; //child window
        private static int WS_BORDER = 0x00800000; //window with border
        private static int WS_DLGFRAME = 0x00400000; //window with double border but no title
        private static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar 
        private static int WS_SYSMENU = 0x00080000; //window menu             

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
                return false;
            list.Add(handle);
            return true;
        }

        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                Win32Callback childProc = new Win32Callback(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        public static string GetWinClass(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return null;
            StringBuilder classname = new StringBuilder(100);
            IntPtr result = GetClassName(hwnd, classname, classname.Capacity);
            if (result != IntPtr.Zero)
                return classname.ToString();
            return null;
        }

        public static IEnumerable<IntPtr> EnumAllWindows(IntPtr hwnd, string childClassName)
        {
            List<IntPtr> children = GetChildWindows(hwnd);
            if (children == null)
                yield break;
            foreach (IntPtr child in children)
            {
                if (GetWinClass(child) == childClassName)
                    yield return child;
                foreach (IntPtr childchild in EnumAllWindows(child, childClassName))
                    yield return childchild;
            }
        }

        public static string GetText(IntPtr hWnd)
        {
            IntPtr Handle = Marshal.AllocHGlobal(250);
            int NumText = (int)SendMessage(hWnd, WM_GETTEXT, 250, Handle);
            byte[] res = new byte[250];
            Marshal.Copy(Handle, res, 0, res.Length);
            Marshal.FreeHGlobal(Handle);
            string Text = System.Text.Encoding.GetEncoding(1251).GetString(res, 0, NumText);
            return Text;
        }

        public static string GetText(IntPtr hWnd, out byte[] res)
        {
            IntPtr Handle = Marshal.AllocHGlobal(250);
            int NumText = (int)SendMessage(hWnd, WM_GETTEXT, 250, Handle);
            res = new byte[250];
            Marshal.Copy(Handle, res, 0, res.Length);
            Marshal.FreeHGlobal(Handle);
            string Text = System.Text.Encoding.GetEncoding(1251).GetString(res, 0, NumText);
            return Text;
        }

        public static bool SetText(IntPtr hWnd, string text)
        {
            IntPtr Handle = Marshal.AllocHGlobal(250);
            byte[] txt = System.Text.Encoding.GetEncoding(1251).GetBytes(text + "\0");
            Marshal.Copy(txt, 0, Handle, txt.Length);
            int NumText = (int)SendMessage(hWnd, WM_SETTEXT, txt.Length, Handle);
            Marshal.FreeHGlobal(Handle);
            return NumText == 1;
        }

        public void Show()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                    ShowWindow(MEP, SW_SHOW);
            };
        }

        public bool SASisOpen
        {
            get
            {
                System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
                if (procs.Length > 0)
                    return true;
                return false;
            }
        }

        public void ShowSAS()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length == 0) return;
            System.Diagnostics.Process p = procs[0];
            ShowWindow(p.MainWindowHandle, SW_SHOW);
            SetForegroundWindow(p.MainWindowHandle);
            SetActiveWindow(p.MainWindowHandle);
            SetFocus(p.MainWindowHandle);
        }

        private bool IsVisible()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                    return IsWindowVisible(MEP);
            };
            return false;
        }

        public bool Visible
        {
            get
            {
                return IsVisible();
            }
            set
            {
                if (value == true)
                    Show();
                else
                    Hide();
            }
        }

        public void Hide()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                    ShowWindow(MEP, SW_HIDE);
            };
        }

        public bool ClickOk()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                {
                    IntPtr tp0 = IntPtr.Zero;
                    while ((tp0 = FindWindowEx(MEP, tp0, "TPanel", null)) != IntPtr.Zero)
                    {
                        IntPtr tp1 = IntPtr.Zero;
                        while ((tp1 = FindWindowEx(tp0, tp1, "TButton", null)) != IntPtr.Zero)
                        {
                            string txt = GetText(tp1);
                            if (txt == "Ok")
                            {
                                SendMessage(tp1, (int)WM_LBUTTONDOWN, (int)10, (int)10);
                                SendMessage(tp1, (int)WM_LBUTTONUP, (int)10, (int)10);
                            };
                        };
                    };
                };
            };
            return false;
        }

        public bool ClickCancel()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                {
                    IntPtr tp0 = IntPtr.Zero;
                    while ((tp0 = FindWindowEx(MEP, tp0, "TPanel", null)) != IntPtr.Zero)
                    {
                        IntPtr tp1 = IntPtr.Zero;
                        while ((tp1 = FindWindowEx(tp0, tp1, "TButton", null)) != IntPtr.Zero)
                        {
                            string txt = GetText(tp1);
                            if ((txt != "Ok") && (txt == "~"))
                            {
                                SendMessage(tp1, (int)WM_LBUTTONDOWN, (int)10, (int)10);
                                SendMessage(tp1, (int)WM_LBUTTONUP, (int)10, (int)10);
                            };
                        };
                    };
                };
            };
            return false;
        }

        public bool AddPlacemark(string name, double lat, double lon)
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);
                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if ((MEP == IntPtr.Zero) || (!IsWindowVisible(MEP)))
                {
                    ShowWindow(p.MainWindowHandle, SW_SHOW);
                    SetForegroundWindow(p.MainWindowHandle);
                    SetActiveWindow(p.MainWindowHandle);
                    SetFocus(p.MainWindowHandle);

                    IntPtr tp0 = IntPtr.Zero;
                    while ((tp0 = FindWindowEx(p.MainWindowHandle, tp0, "TImage32", null)) != IntPtr.Zero)
                    {

                        System.Threading.Thread thr = new System.Threading.Thread(RClickSend);
                        thr.Start(tp0);
                        IntPtr popup = IntPtr.Zero;
                        int maxwait = 200;
                        while ((maxwait >= 0) && (popup == IntPtr.Zero))
                        {
                            popup = FindWindow("TTBXPopupWindowS", null);
                            System.Threading.Thread.Sleep(10);
                            maxwait--;
                        };
                        if (popup != IntPtr.Zero)
                        {
                            SendInputData.SendKeyDown(SendInputData.KeyCode.DOWN);
                            SendInputData.SendKeyDown(SendInputData.KeyCode.ENTER);
                        };
                        maxwait = 200;
                        while ((maxwait >= 0) && (MEP == IntPtr.Zero))
                        {
                            MEP = FindWindow("TfrmMarkEditPoint", null);
                            System.Threading.Thread.Sleep(10);
                            maxwait--;
                        };
                        if (MEP != IntPtr.Zero)
                        {
                            System.Threading.Thread.Sleep(200);
                            bool ok = true;
                            ok = SetPOIName(name) && ok;
                            ok = SetLat(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}°", lat >= 0 ? "N" : "S", lat)) && ok;
                            ok = SetLon(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}°", lon >= 0 ? "E" : "W", lon)) && ok;
                            return ok;
                        };
                    };
                };
            };
            return false;
        }

        public bool SetPlacemark(string name, double lat, double lon)
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);
                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if ((MEP != IntPtr.Zero) && IsWindowVisible(MEP))
                {
                    ShowWindow(MEP, SW_SHOW);
                    SetForegroundWindow(MEP);
                    SetActiveWindow(MEP);
                    SetFocus(MEP);

                    SetPOIName(name);
                    SetLat(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}°", lat >= 0 ? "N" : "S", lat));
                    SetLon(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}°", lon >= 0 ? "E" : "W", lon));
                };
            };
            return false;
        }

        private static void RClickSend(object handle)
        {
            if (handle == null) return;
            try
            {
                SendMessage((IntPtr)handle, WM_RBUTTONDOWN, 100, 100);
                SendMessage((IntPtr)handle, WM_RBUTTONUP, 100, 100);
            }
            catch { };
        }

        private PointD GetLatLon()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                {
                    IntPtr tp0 = IntPtr.Zero;
                    while ((tp0 = FindWindowEx(MEP, tp0, "TPanel", null)) != IntPtr.Zero)
                    {
                        IntPtr tp1 = IntPtr.Zero;
                        while ((tp1 = FindWindowEx(tp0, tp1, "TfrLonLat", null)) != IntPtr.Zero)
                        {
                            IntPtr tp2 = IntPtr.Zero;
                            while ((tp2 = FindWindowEx(tp1, tp2, "TGridPanel", null)) != IntPtr.Zero)
                            {
                                IntPtr te0 = IntPtr.Zero;
                                PointD result = new PointD();
                                int ctr = 0;
                                while ((te0 = FindWindowEx(tp2, te0, "TEdit", null)) != IntPtr.Zero)
                                {
                                    ctr++;
                                    if (ctr == 1)
                                    {
                                        string txt = GetText(te0);
                                        if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                                            txt = txt.Replace(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
                                        result.X = LatLonParser.ToLon(txt);
                                    };
                                    if (ctr == 2)
                                    {
                                        string txt = GetText(te0);
                                        if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                                            txt = txt.Replace(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
                                        result.Y = LatLonParser.ToLat(txt);
                                    };
                                };
                                return result;
                            };
                        };
                    };
                };
            };
            return new PointD();
        }

        private bool SetLatLon(PointD latlon)
        {
            bool ok = true;
            ok = SetLat(LatLonParser.GetLinePrefix(latlon.Y, LatLonParser.DFormat.ENG_NS) + LatLonParser.ToString(latlon.Y, LatLonParser.FFormat.DDDDDD)) && ok;
            ok = SetLon(LatLonParser.GetLinePrefix(latlon.X, LatLonParser.DFormat.ENG_EW) + LatLonParser.ToString(latlon.X, LatLonParser.FFormat.DDDDDD)) && ok;
            return ok;
        }

        public PointD LatLon
        {
            get
            {
                return GetLatLon();
            }
            set
            {
                SetLatLon(value);
            }
        }

        private string GetLat()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                {
                    IntPtr tp0 = IntPtr.Zero;
                    while ((tp0 = FindWindowEx(MEP, tp0, "TPanel", null)) != IntPtr.Zero)
                    {
                        IntPtr tp1 = IntPtr.Zero;
                        while ((tp1 = FindWindowEx(tp0, tp1, "TfrLonLat", null)) != IntPtr.Zero)
                        {
                            IntPtr tp2 = IntPtr.Zero;
                            while ((tp2 = FindWindowEx(tp1, tp2, "TGridPanel", null)) != IntPtr.Zero)
                            {
                                IntPtr te0 = IntPtr.Zero;
                                int ctr = 0;
                                while ((te0 = FindWindowEx(tp2, te0, "TEdit", null)) != IntPtr.Zero)
                                {
                                    ctr++;
                                    if (ctr == 2)
                                    {
                                        string txt = GetText(te0);
                                        if(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                                            txt = txt.Replace(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
                                        return txt;
                                    };
                                };
                            };
                        };
                    };
                };
            };
            return null;
        }

        private bool SetLat(string lat)
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                {
                    IntPtr tp0 = IntPtr.Zero;
                    while ((tp0 = FindWindowEx(MEP, tp0, "TPanel", null)) != IntPtr.Zero)
                    {
                        IntPtr tp1 = IntPtr.Zero;
                        while ((tp1 = FindWindowEx(tp0, tp1, "TfrLonLat", null)) != IntPtr.Zero)
                        {
                            IntPtr tp2 = IntPtr.Zero;
                            while ((tp2 = FindWindowEx(tp1, tp2, "TGridPanel", null)) != IntPtr.Zero)
                            {
                                IntPtr te0 = IntPtr.Zero;
                                int ctr = 0;
                                while ((te0 = FindWindowEx(tp2, te0, "TEdit", null)) != IntPtr.Zero)
                                {
                                    ctr++;
                                    if (ctr == 2)
                                    {
                                        if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                                            lat = lat.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                                        return SetText(te0, lat);
                                    };
                                };
                            };
                        };
                    };
                };
            };
            return false;
        }

        public string Lat
        {
            get
            {
                return GetLat();
            }
            set
            {
                SetLat(value);
            }
        }

        private string GetLon()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                {
                    IntPtr tp0 = IntPtr.Zero;
                    while ((tp0 = FindWindowEx(MEP, tp0, "TPanel", null)) != IntPtr.Zero)
                    {
                        IntPtr tp1 = IntPtr.Zero;
                        while ((tp1 = FindWindowEx(tp0, tp1, "TfrLonLat", null)) != IntPtr.Zero)
                        {
                            IntPtr tp2 = IntPtr.Zero;
                            while ((tp2 = FindWindowEx(tp1, tp2, "TGridPanel", null)) != IntPtr.Zero)
                            {
                                IntPtr te0 = IntPtr.Zero;
                                int ctr = 0;
                                while ((te0 = FindWindowEx(tp2, te0, "TEdit", null)) != IntPtr.Zero)
                                {
                                    ctr++;
                                    if (ctr == 1)
                                    {
                                        string txt = GetText(te0);
                                        if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                                            txt = txt.Replace(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
                                        return txt;
                                    };
                                };
                            };
                        };
                    };
                };
            };
            return null;
        }

        private bool SetLon(string lon)
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                {
                    IntPtr tp0 = IntPtr.Zero;
                    while ((tp0 = FindWindowEx(MEP, tp0, "TPanel", null)) != IntPtr.Zero)
                    {
                        IntPtr tp1 = IntPtr.Zero;
                        while ((tp1 = FindWindowEx(tp0, tp1, "TfrLonLat", null)) != IntPtr.Zero)
                        {
                            IntPtr tp2 = IntPtr.Zero;
                            while ((tp2 = FindWindowEx(tp1, tp2, "TGridPanel", null)) != IntPtr.Zero)
                            {
                                IntPtr te0 = IntPtr.Zero;
                                int ctr = 0;
                                while ((te0 = FindWindowEx(tp2, te0, "TEdit", null)) != IntPtr.Zero)
                                {
                                    ctr++;
                                    if (ctr == 1)
                                    {
                                        if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                                            lon = lon.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                                        return SetText(te0, lon);
                                    };
                                };
                            };
                        };
                    };
                };
            };
            return false;
        }

        public string Lon
        {
            get
            {
                return GetLon();
            }
            set
            {
                SetLon(value);
            }
        }

        bool SetPOIName(string text)
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                {
                    IntPtr tp0 = IntPtr.Zero;
                    while ((tp0 = FindWindowEx(MEP, tp0, "TPanel", null)) != IntPtr.Zero)
                    {
                        IntPtr tp1 = IntPtr.Zero;
                        while ((tp1 = FindWindowEx(tp0, tp1, "TPanel", null)) != IntPtr.Zero)
                        {
                            IntPtr tp2 = IntPtr.Zero;
                            while ((tp2 = FindWindowEx(tp1, tp2, "TPanel", null)) != IntPtr.Zero)
                            {
                                IntPtr te0 = IntPtr.Zero;
                                while ((te0 = FindWindowEx(tp2, te0, "TEdit", null)) != IntPtr.Zero)
                                {
                                    return SetText(te0, text);
                                };
                            };
                        };
                    };
                };
            };
            return false;
        }

        private string GetPOIName()
        {
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName("SASPlanet");
            if (procs.Length > 0)
            {
                System.Diagnostics.Process p = procs[0];
                // List<IntPtr> chw = GetChildWindows(p.MainWindowHandle);

                IntPtr MEP = FindWindow("TfrmMarkEditPoint", null);
                if (MEP != IntPtr.Zero)
                {
                    IntPtr tp0 = IntPtr.Zero;
                    while ((tp0 = FindWindowEx(MEP, tp0, "TPanel", null)) != IntPtr.Zero)
                    {
                        IntPtr tp1 = IntPtr.Zero;
                        while ((tp1 = FindWindowEx(tp0, tp1, "TPanel", null)) != IntPtr.Zero)
                        {
                            IntPtr tp2 = IntPtr.Zero;
                            while ((tp2 = FindWindowEx(tp1, tp2, "TPanel", null)) != IntPtr.Zero)
                            {
                                IntPtr te0 = IntPtr.Zero;
                                while ((te0 = FindWindowEx(tp2, te0, "TEdit", null)) != IntPtr.Zero)
                                {
                                    return GetText(te0);
                                };
                            };
                        };
                    };
                };
            };
            return null;
        }

        public string Name
        {
            get
            {
                return GetPOIName();
            }
            set
            {
                SetPOIName(value);
            }
        }

        public class SendInputData
        {
            [DllImport("user32.dll", SetLastError = true)]
            private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

            /// <summary>
            /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private struct INPUT
            {
                public uint Type;
                public MOUSEKEYBDHARDWAREINPUT Data;
            }

            /// <summary>
            /// http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/f0e82d6e-4999-4d22-b3d3-32b25f61fb2a
            /// </summary>
            [StructLayout(LayoutKind.Explicit)]
            private struct MOUSEKEYBDHARDWAREINPUT
            {
                [FieldOffset(0)]
                public HARDWAREINPUT Hardware;
                [FieldOffset(0)]
                public KEYBDINPUT Keyboard;
                [FieldOffset(0)]
                public MOUSEINPUT Mouse;
            }

            /// <summary>
            /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private struct HARDWAREINPUT
            {
                public uint Msg;
                public ushort ParamL;
                public ushort ParamH;
            }

            /// <summary>
            /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private struct KEYBDINPUT
            {
                public ushort Vk;
                public ushort Scan;
                public uint Flags;
                public uint Time;
                public IntPtr ExtraInfo;
            }

            /// <summary>
            /// http://social.msdn.microsoft.com/forums/en-US/netfxbcl/thread/2abc6be8-c593-4686-93d2-89785232dacd
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private struct MOUSEINPUT
            {
                public int X;
                public int Y;
                public uint MouseData;
                public uint Flags;
                public uint Time;
                public IntPtr ExtraInfo;
            }

            public enum KeyCode : ushort
            {
                #region Media

                /// <summary>
                /// Next track if a song is playing
                /// </summary>
                MEDIA_NEXT_TRACK = 0xb0,

                /// <summary>
                /// Play pause
                /// </summary>
                MEDIA_PLAY_PAUSE = 0xb3,

                /// <summary>
                /// Previous track
                /// </summary>
                MEDIA_PREV_TRACK = 0xb1,

                /// <summary>
                /// Stop
                /// </summary>
                MEDIA_STOP = 0xb2,

                #endregion

                #region math

                /// <summary>Key "+"</summary>
                ADD = 0x6b,
                /// <summary>
                /// "*" key
                /// </summary>
                MULTIPLY = 0x6a,

                /// <summary>
                /// "/" key
                /// </summary>
                DIVIDE = 0x6f,

                /// <summary>
                /// Subtract key "-"
                /// </summary>
                SUBTRACT = 0x6d,

                #endregion

                #region Browser
                /// <summary>
                /// Go Back
                /// </summary>
                BROWSER_BACK = 0xa6,
                /// <summary>
                /// Favorites
                /// </summary>
                BROWSER_FAVORITES = 0xab,
                /// <summary>
                /// Forward
                /// </summary>
                BROWSER_FORWARD = 0xa7,
                /// <summary>
                /// Home
                /// </summary>
                BROWSER_HOME = 0xac,
                /// <summary>
                /// Refresh
                /// </summary>
                BROWSER_REFRESH = 0xa8,
                /// <summary>
                /// browser search
                /// </summary>
                BROWSER_SEARCH = 170,
                /// <summary>
                /// Stop
                /// </summary>
                BROWSER_STOP = 0xa9,
                #endregion

                #region Numpad numbers
                /// <summary>
                /// 
                /// </summary>
                NUMPAD0 = 0x60,
                /// <summary>
                /// 
                /// </summary>
                NUMPAD1 = 0x61,
                /// <summary>
                /// 
                /// </summary>
                NUMPAD2 = 0x62,
                /// <summary>
                /// 
                /// </summary>
                NUMPAD3 = 0x63,
                /// <summary>
                /// 
                /// </summary>
                NUMPAD4 = 100,
                /// <summary>
                /// 
                /// </summary>
                NUMPAD5 = 0x65,
                /// <summary>
                /// 
                /// </summary>
                NUMPAD6 = 0x66,
                /// <summary>
                /// 
                /// </summary>
                NUMPAD7 = 0x67,
                /// <summary>
                /// 
                /// </summary>
                NUMPAD8 = 0x68,
                /// <summary>
                /// 
                /// </summary>
                NUMPAD9 = 0x69,

                #endregion

                #region Fkeys
                /// <summary>
                /// F1
                /// </summary>
                F1 = 0x70,
                /// <summary>
                /// F10
                /// </summary>
                F10 = 0x79,
                /// <summary>
                /// 
                /// </summary>
                F11 = 0x7a,
                /// <summary>
                /// 
                /// </summary>
                F12 = 0x7b,
                /// <summary>
                /// 
                /// </summary>
                F13 = 0x7c,
                /// <summary>
                /// 
                /// </summary>
                F14 = 0x7d,
                /// <summary>
                /// 
                /// </summary>
                F15 = 0x7e,
                /// <summary>
                /// 
                /// </summary>
                F16 = 0x7f,
                /// <summary>
                /// 
                /// </summary>
                F17 = 0x80,
                /// <summary>
                /// 
                /// </summary>
                F18 = 0x81,
                /// <summary>
                /// 
                /// </summary>
                F19 = 130,
                /// <summary>
                /// 
                /// </summary>
                F2 = 0x71,
                /// <summary>
                /// 
                /// </summary>
                F20 = 0x83,
                /// <summary>
                /// 
                /// </summary>
                F21 = 0x84,
                /// <summary>
                /// 
                /// </summary>
                F22 = 0x85,
                /// <summary>
                /// 
                /// </summary>
                F23 = 0x86,
                /// <summary>
                /// 
                /// </summary>
                F24 = 0x87,
                /// <summary>
                /// 
                /// </summary>
                F3 = 0x72,
                /// <summary>
                /// 
                /// </summary>
                F4 = 0x73,
                /// <summary>
                /// 
                /// </summary>
                F5 = 0x74,
                /// <summary>
                /// 
                /// </summary>
                F6 = 0x75,
                /// <summary>
                /// 
                /// </summary>
                F7 = 0x76,
                /// <summary>
                /// 
                /// </summary>
                F8 = 0x77,
                /// <summary>
                /// 
                /// </summary>
                F9 = 120,

                #endregion

                #region Other
                /// <summary>
                /// 
                /// </summary>
                OEM_1 = 0xba,
                /// <summary>
                /// 
                /// </summary>
                OEM_102 = 0xe2,
                /// <summary>
                /// 
                /// </summary>
                OEM_2 = 0xbf,
                /// <summary>
                /// 
                /// </summary>
                OEM_3 = 0xc0,
                /// <summary>
                /// 
                /// </summary>
                OEM_4 = 0xdb,
                /// <summary>
                /// 
                /// </summary>
                OEM_5 = 220,
                /// <summary>
                /// 
                /// </summary>
                OEM_6 = 0xdd,
                /// <summary>
                /// 
                /// </summary>
                OEM_7 = 0xde,
                /// <summary>
                /// 
                /// </summary>
                OEM_8 = 0xdf,
                /// <summary>
                /// 
                /// </summary>
                OEM_CLEAR = 0xfe,
                /// <summary>
                /// 
                /// </summary>
                OEM_COMMA = 0xbc,
                /// <summary>
                /// 
                /// </summary>
                OEM_MINUS = 0xbd,
                /// <summary>
                /// 
                /// </summary>
                OEM_PERIOD = 190,
                /// <summary>
                /// 
                /// </summary>
                OEM_PLUS = 0xbb,

                #endregion

                #region KEYS

                /// <summary>
                /// 
                /// </summary>
                KEY_0 = 0x30,
                /// <summary>
                /// 
                /// </summary>
                KEY_1 = 0x31,
                /// <summary>
                /// 
                /// </summary>
                KEY_2 = 50,
                /// <summary>
                /// 
                /// </summary>
                KEY_3 = 0x33,
                /// <summary>
                /// 
                /// </summary>
                KEY_4 = 0x34,
                /// <summary>
                /// 
                /// </summary>
                KEY_5 = 0x35,
                /// <summary>
                /// 
                /// </summary>
                KEY_6 = 0x36,
                /// <summary>
                /// 
                /// </summary>
                KEY_7 = 0x37,
                /// <summary>
                /// 
                /// </summary>
                KEY_8 = 0x38,
                /// <summary>
                /// 
                /// </summary>
                KEY_9 = 0x39,
                /// <summary>
                /// 
                /// </summary>
                KEY_A = 0x41,
                /// <summary>
                /// 
                /// </summary>
                KEY_B = 0x42,
                /// <summary>
                /// 
                /// </summary>
                KEY_C = 0x43,
                /// <summary>
                /// 
                /// </summary>
                KEY_D = 0x44,
                /// <summary>
                /// 
                /// </summary>
                KEY_E = 0x45,
                /// <summary>
                /// 
                /// </summary>
                KEY_F = 70,
                /// <summary>
                /// 
                /// </summary>
                KEY_G = 0x47,
                /// <summary>
                /// 
                /// </summary>
                KEY_H = 0x48,
                /// <summary>
                /// 
                /// </summary>
                KEY_I = 0x49,
                /// <summary>
                /// 
                /// </summary>
                KEY_J = 0x4a,
                /// <summary>
                /// 
                /// </summary>
                KEY_K = 0x4b,
                /// <summary>
                /// 
                /// </summary>
                KEY_L = 0x4c,
                /// <summary>
                /// 
                /// </summary>
                KEY_M = 0x4d,
                /// <summary>
                /// 
                /// </summary>
                KEY_N = 0x4e,
                /// <summary>
                /// 
                /// </summary>
                KEY_O = 0x4f,
                /// <summary>
                /// 
                /// </summary>
                KEY_P = 80,
                /// <summary>
                /// 
                /// </summary>
                KEY_Q = 0x51,
                /// <summary>
                /// 
                /// </summary>
                KEY_R = 0x52,
                /// <summary>
                /// 
                /// </summary>
                KEY_S = 0x53,
                /// <summary>
                /// 
                /// </summary>
                KEY_T = 0x54,
                /// <summary>
                /// 
                /// </summary>
                KEY_U = 0x55,
                /// <summary>
                /// 
                /// </summary>
                KEY_V = 0x56,
                /// <summary>
                /// 
                /// </summary>
                KEY_W = 0x57,
                /// <summary>
                /// 
                /// </summary>
                KEY_X = 0x58,
                /// <summary>
                /// 
                /// </summary>
                KEY_Y = 0x59,
                /// <summary>
                /// 
                /// </summary>
                KEY_Z = 90,

                #endregion

                #region volume
                /// <summary>
                /// Decrese volume
                /// </summary>
                VOLUME_DOWN = 0xae,

                /// <summary>
                /// Mute volume
                /// </summary>
                VOLUME_MUTE = 0xad,

                /// <summary>
                /// Increase volue
                /// </summary>
                VOLUME_UP = 0xaf,

                #endregion


                /// <summary>
                /// Take snapshot of the screen and place it on the clipboard
                /// </summary>
                SNAPSHOT = 0x2c,

                /// <summary>Send right click from keyboard "key that is 2 keys to the right of space bar"</summary>
                RightClick = 0x5d,

                /// <summary>
                /// Go Back or delete
                /// </summary>
                BACKSPACE = 8,

                /// <summary>
                /// Control + Break "When debuging if you step into an infinite loop this will stop debug"
                /// </summary>
                CANCEL = 3,
                /// <summary>
                /// Caps lock key to send cappital letters
                /// </summary>
                CAPS_LOCK = 20,
                /// <summary>
                /// Ctlr key
                /// </summary>
                CONTROL = 0x11,

                /// <summary>
                /// Alt key
                /// </summary>
                ALT = 18,

                /// <summary>
                /// "." key
                /// </summary>
                DECIMAL = 110,

                /// <summary>
                /// Delete Key
                /// </summary>
                DELETE = 0x2e,


                /// <summary>
                /// Arrow down key
                /// </summary>
                DOWN = 40,

                /// <summary>
                /// End key
                /// </summary>
                END = 0x23,

                /// <summary>
                /// Escape key
                /// </summary>
                ESC = 0x1b,

                /// <summary>
                /// Home key
                /// </summary>
                HOME = 0x24,

                /// <summary>
                /// Insert key
                /// </summary>
                INSERT = 0x2d,

                /// <summary>
                /// Open my computer
                /// </summary>
                LAUNCH_APP1 = 0xb6,
                /// <summary>
                /// Open calculator
                /// </summary>
                LAUNCH_APP2 = 0xb7,

                /// <summary>
                /// Open default email in my case outlook
                /// </summary>
                LAUNCH_MAIL = 180,

                /// <summary>
                /// Opend default media player (itunes, winmediaplayer, etc)
                /// </summary>
                LAUNCH_MEDIA_SELECT = 0xb5,

                /// <summary>
                /// Left control
                /// </summary>
                LCONTROL = 0xa2,

                /// <summary>
                /// Left arrow
                /// </summary>
                LEFT = 0x25,

                /// <summary>
                /// Left shift
                /// </summary>
                LSHIFT = 160,

                /// <summary>
                /// left windows key
                /// </summary>
                LWIN = 0x5b,


                /// <summary>
                /// Next "page down"
                /// </summary>
                PAGEDOWN = 0x22,

                /// <summary>
                /// Num lock to enable typing numbers
                /// </summary>
                NUMLOCK = 0x90,

                /// <summary>
                /// Page up key
                /// </summary>
                PAGE_UP = 0x21,

                /// <summary>
                /// Right control
                /// </summary>
                RCONTROL = 0xa3,

                /// <summary>
                /// Return key
                /// </summary>
                ENTER = 13,

                /// <summary>
                /// Right arrow key
                /// </summary>
                RIGHT = 0x27,

                /// <summary>
                /// Right shift
                /// </summary>
                RSHIFT = 0xa1,

                /// <summary>
                /// Right windows key
                /// </summary>
                RWIN = 0x5c,

                /// <summary>
                /// Shift key
                /// </summary>
                SHIFT = 0x10,

                /// <summary>
                /// Space back key
                /// </summary>
                SPACE_BAR = 0x20,

                /// <summary>
                /// Tab key
                /// </summary>
                TAB = 9,

                /// <summary>
                /// Up arrow key
                /// </summary>
                UP = 0x26,

            }

            public static bool SendKeyPress(KeyCode keyCode)
            {
                INPUT input = new INPUT();
                input.Type = 1;
                input.Data = new MOUSEKEYBDHARDWAREINPUT();
                input.Data.Keyboard = new KEYBDINPUT();
                input.Data.Keyboard.Vk = (ushort)keyCode;
                input.Data.Keyboard.Scan = 0;
                input.Data.Keyboard.Flags = 0;
                input.Data.Keyboard.Time = 0;
                input.Data.Keyboard.ExtraInfo = IntPtr.Zero;

                INPUT input2 = new INPUT();
                input2.Type = 1;
                input2.Data = new MOUSEKEYBDHARDWAREINPUT();
                input2.Data.Keyboard = new KEYBDINPUT();
                input2.Data.Keyboard.Vk = (ushort)keyCode;
                input2.Data.Keyboard.Scan = 0;
                input2.Data.Keyboard.Flags = 2;
                input2.Data.Keyboard.Time = 0;
                input2.Data.Keyboard.ExtraInfo = IntPtr.Zero;

                INPUT[] inputs = new INPUT[] { input, input2 };
                return (SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT))) != 0);
            }

            public static bool SendKeyDown(KeyCode keyCode)
            {
                INPUT input = new INPUT();
                input.Type = 1;
                input.Data = new MOUSEKEYBDHARDWAREINPUT();
                input.Data.Keyboard = new KEYBDINPUT();
                input.Data.Keyboard.Vk = (ushort)keyCode;
                input.Data.Keyboard.Scan = 0;
                input.Data.Keyboard.Flags = 0;
                input.Data.Keyboard.Time = 0;
                input.Data.Keyboard.ExtraInfo = IntPtr.Zero;

                INPUT[] inputs = new INPUT[] { input };
                return (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) != 0);
            }

            public static bool SendKeyUp(KeyCode keyCode)
            {
                INPUT input = new INPUT();
                input.Type = 1;
                input.Data = new MOUSEKEYBDHARDWAREINPUT();
                input.Data.Keyboard = new KEYBDINPUT();
                input.Data.Keyboard.Vk = (ushort)keyCode;
                input.Data.Keyboard.Scan = 0;
                input.Data.Keyboard.Flags = 2;
                input.Data.Keyboard.Time = 0;
                input.Data.Keyboard.ExtraInfo = IntPtr.Zero;

                INPUT[] inputs = new INPUT[] { input };
                return (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) != 0);
            }
        }
    }

}
