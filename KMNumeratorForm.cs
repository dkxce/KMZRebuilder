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

namespace KMZRebuilder
{
    public partial class KMNumeratorForm : Form
    {
        private XmlDocument document = null;
        private string fileName = "";
        private bool isKml = false;

        public KMNumeratorForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".gpx";
            dialog.Filter = "GPX and KML files(*.gpx;*.kml)|*.gpx;*.kml|KML files (*.kml)|*.kml|GPS Exchange Format (*.gpx)|*.gpx|All files (*.*)|*.*";
            dialog.FileName = this.fileName;
            if (dialog.ShowDialog() == DialogResult.OK)
                if (File.Exists(dialog.FileName))
                {
                    this.fileName = dialog.FileName;                    
                    AnalyseFile();
                    button2.Enabled = true;
                };
            dialog.Dispose();
            progressBar1.Value = 0;
        }

        private void AnalyseFile()
        {
            this.textBox1.Text = System.IO.Path.GetFileName(this.fileName);
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            document = new XmlDocument();
            document.Load(fileName);
            if (ext == ".kml") document.InnerXml = Regex.Replace(document.InnerXml, @"(<kml.*?>\s*)+", "<kml>", RegexOptions.Singleline);
            if (ext == ".gpx") document.InnerXml = Regex.Replace(document.InnerXml, @"(<gpx.*?>\s*)+", "<gpx>", RegexOptions.Singleline);
            isKml = ext == ".kml";
            string[] toFind = new string[]{
                "kml/Document/Placemark/MultiGeometry/LineString/coordinates",
                "kml/Document/MultiGeometry/LineString/coordinates",
                "kml/Document/Folder/Placemark/MultiGeometry/LineString/coordinates",                
                "kml/Document/Folder/MultiGeometry/LineString/coordinates",
                "kml/Document/Folder/Placemark/LineString/coordinates",
                "kml/Document/Folder/Placemark/Point/coordinates",                
                "gpx/trk/trkseg/trkpt",
                "gpx/rte/rtept"
            };
            string txt = "";
            comboBox1.Items.Clear();
            foreach (string toF in toFind)
            {
                XmlNodeList list = document.SelectNodes(toF);
                if (list.Count > 0)
                {
                    comboBox1.Items.Add(comboBox1.Text = toF);
                    txt += (txt.Length > 0 ? ", " :"") + list.Count.ToString();
                };
            };
            if (comboBox1.Items.Count > 1)
                comboBox1.SelectedIndex = 0;

            if (txt == "")
                txt = "Nothing found";
            else
                txt = "Found: " + txt + " objects";

            statusText.Text = txt;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            System.Globalization.NumberFormatInfo provider = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
            provider.NumberDecimalSeparator = ".";

            XmlNodeList nlist = document.SelectNodes(comboBox1.Text);
            if (nlist.Count == 0)
            {
                MessageBox.Show("No Data Found!");
                return;
            };

            // WORK
            {
                List<PointF> xy = new List<PointF>();
                foreach (XmlNode xn in nlist)
                {
                    if (!isKml)
                        xy.Add(new PointF(float.Parse(xn.Attributes["lon"].InnerText, provider), float.Parse(xn.Attributes["lat"].InnerText, provider)));
                    else
                    {
                        string[] xya = xn.ChildNodes[0].Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string xye in xya)
                        {
                            string[] xyz = xye.Split(new string[] { "," }, StringSplitOptions.None);
                            xy.Add(new PointF(float.Parse(xyz[0], provider), float.Parse(xyz[1], provider)));
                        };
                    };
                };

                if (xy.Count == 0)
                {
                    MessageBox.Show("No Data Found!");
                    return;
                };

                this.progressBar1.Maximum = xy.Count;
                this.statusText.Text = "Running...";

                float kmError = 0.15f;
                float total_dist = 0;                
                int currFlag = 1;

                List<KM> kmFlags = new List<KM>();
                float prevLat = xy[0].Y;
                float prevLon = xy[0].X;
                kmFlags.Add(new KM(0, prevLat, prevLon));

                for (int i = 1; i < xy.Count; i++)
                {
                    float currLat = xy[i].Y;
                    float currLon = xy[i].X;
                    float dist_prev_curr = Utils.GetLengthMeters((double)prevLat, (double)prevLon, (double)currLat, (double)currLon, false) / 1000f;
                    total_dist += dist_prev_curr;
                    bool flag = true;
                    while (flag)
                    {
                        float walked = total_dist - (currFlag);
                        if (walked >= 0)
                        {
                            if (walked <= kmError)
                            {
                                kmFlags.Add(new KM(currFlag, currLat, currLon));
                                currFlag++;
                            }
                            else
                            {
                                float walkback = dist_prev_curr - walked;
                                float btwLat = prevLat + (((currLat - prevLat) / dist_prev_curr) * walkback);
                                float btwLon = prevLon + (((currLon - prevLon) / dist_prev_curr) * walkback);
                                kmFlags.Add(new KM(currFlag, btwLat, btwLon));
                                currFlag++;
                            }
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                    prevLat = currLat;
                    prevLon = currLon;
                    this.progressBar1.Value = i - 1;
                    this.statusText.Text = string.Format("Analize points {0} of {1}, counted {2} kms of path", i - 1, xy.Count, kmFlags.Count);
                    this.statusText.Update();
                }

                this.statusText.Text = string.Format("Analized {1} point, counted {2} kms of path", 0, xy.Count, kmFlags.Count);
                this.statusText.Update();

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Select output KML file";
                sfd.DefaultExt = ".kml";
                sfd.FileName = this.fileName.Remove(this.fileName.Length - 4) + "[km].kml";
                sfd.Filter = "KML Files (*.kml)|*.kml";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    int incKm = (int)this.incVal.Value;
                    int startKm = (int)this.startVal.Value;
                    int skipKm = (int)this.skipVal.Value;

                    FileStream stream = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                    writer.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?><kml xmlns=\"http://earth.google.com/kml/2.2\"><Document xmlns=\"\"><name>Km Flags</name>\r\n");
                    writer.Write("<Folder><name>Each " + this.incVal.Text + " km</name>\r\n");
                    for (int i = skipKm; i < kmFlags.Count; i+=incKm)
                    {
                        KM km = kmFlags[i];
                        writer.Write("<Placemark><name>" + (km.km - skipKm + startKm).ToString() + " km</name><Point><coordinates>");
                        writer.Write(km.Lon.ToString().Replace(",", ".") + "," + km.Lat.ToString().Replace(",", ".") + ",0 ");
                        writer.Write("</coordinates></Point></Placemark>\r\n");
                    }
                    writer.Write("</Folder></Document></kml>");
                    writer.Flush();
                    writer.Close();                    
                };
                sfd.Dispose();                
            }

        }
    }

    public class KM
    {
        public int km;
        public float Lat;
        public float Lon;

        public KM(int km, float Lat, float Lon)
        {
            this.km = km;
            this.Lat = Lat;
            this.Lon = Lon;
        }
    }

    public class Utils
    {
        // Рассчет расстояния       
        #region LENGTH
        public static float GetLengthMeters(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            // use fastest
            float result = (float)GetLengthMetersD(StartLat, StartLong, EndLat, EndLong, radians);

            if (float.IsNaN(result))
            {
                result = (float)GetLengthMetersC(StartLat, StartLong, EndLat, EndLong, radians);
                if (float.IsNaN(result))
                {
                    result = (float)GetLengthMetersE(StartLat, StartLong, EndLat, EndLong, radians);
                    if (float.IsNaN(result))
                        result = 0;
                };
            };

            return result;
        }

        // Slower
        public static uint GetLengthMetersA(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double D2R = Math.PI / 180;     // Преобразование градусов в радианы

            double a = 6378137.0000;     // WGS-84 Equatorial Radius (a)
            double f = 1 / 298.257223563;  // WGS-84 Flattening (f)
            double b = (1 - f) * a;      // WGS-84 Polar Radius
            double e2 = (2 - f) * f;      // WGS-84 Квадрат эксцентричности эллипсоида  // 1-(b/a)^2

            // Переменные, используемые для вычисления смещения и расстояния
            double fPhimean;                           // Средняя широта
            double fdLambda;                           // Разница между двумя значениями долготы
            double fdPhi;                           // Разница между двумя значениями широты
            double fAlpha;                           // Смещение
            double fRho;                           // Меридианский радиус кривизны
            double fNu;                           // Поперечный радиус кривизны
            double fR;                           // Радиус сферы Земли
            double fz;                           // Угловое расстояние от центра сфероида
            double fTemp;                           // Временная переменная, использующаяся в вычислениях

            // Вычисляем разницу между двумя долготами и широтами и получаем среднюю широту
            // предположительно что расстояние между точками << радиуса земли
            if (!radians)
            {
                fdLambda = (StartLong - EndLong) * D2R;
                fdPhi = (StartLat - EndLat) * D2R;
                fPhimean = ((StartLat + EndLat) / 2) * D2R;
            }
            else
            {
                fdLambda = StartLong - EndLong;
                fdPhi = StartLat - EndLat;
                fPhimean = (StartLat + EndLat) / 2;
            };

            // Вычисляем меридианные и поперечные радиусы кривизны средней широты
            fTemp = 1 - e2 * (sqr(Math.Sin(fPhimean)));
            fRho = (a * (1 - e2)) / Math.Pow(fTemp, 1.5);
            fNu = a / (Math.Sqrt(1 - e2 * (Math.Sin(fPhimean) * Math.Sin(fPhimean))));

            // Вычисляем угловое расстояние
            if (!radians)
            {
                fz = Math.Sqrt(sqr(Math.Sin(fdPhi / 2.0)) + Math.Cos(EndLat * D2R) * Math.Cos(StartLat * D2R) * sqr(Math.Sin(fdLambda / 2.0)));
            }
            else
            {
                fz = Math.Sqrt(sqr(Math.Sin(fdPhi / 2.0)) + Math.Cos(EndLat) * Math.Cos(StartLat) * sqr(Math.Sin(fdLambda / 2.0)));
            };
            fz = 2 * Math.Asin(fz);

            // Вычисляем смещение
            if (!radians)
            {
                fAlpha = Math.Cos(EndLat * D2R) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            }
            else
            {
                fAlpha = Math.Cos(EndLat) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            };
            fAlpha = Math.Asin(fAlpha);

            // Вычисляем радиус Земли
            fR = (fRho * fNu) / (fRho * sqr(Math.Sin(fAlpha)) + fNu * sqr(Math.Cos(fAlpha)));
            // Получаем расстояние
            return (uint)Math.Round(Math.Abs(fz * fR));
        }
        // Slowest
        public static uint GetLengthMetersB(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double fPhimean, fdLambda, fdPhi, fAlpha, fRho, fNu, fR, fz, fTemp, Distance,
                D2R = Math.PI / 180,
                a = 6378137.0,
                e2 = 0.006739496742337;
            if (radians) D2R = 1;

            fdLambda = (StartLong - EndLong) * D2R;
            fdPhi = (StartLat - EndLat) * D2R;
            fPhimean = (StartLat + EndLat) / 2.0 * D2R;

            fTemp = 1 - e2 * Math.Pow(Math.Sin(fPhimean), 2);
            fRho = a * (1 - e2) / Math.Pow(fTemp, 1.5);
            fNu = a / Math.Sqrt(1 - e2 * Math.Sin(fPhimean) * Math.Sin(fPhimean));

            fz = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(fdPhi / 2.0), 2) +
              Math.Cos(EndLat * D2R) * Math.Cos(StartLat * D2R) * Math.Pow(Math.Sin(fdLambda / 2.0), 2)));
            fAlpha = Math.Asin(Math.Cos(EndLat * D2R) * Math.Sin(fdLambda) / Math.Sin(fz));
            fR = fRho * fNu / (fRho * Math.Pow(Math.Sin(fAlpha), 2) + fNu * Math.Pow(Math.Cos(fAlpha), 2));
            Distance = fz * fR;

            return (uint)Math.Round(Distance);
        }
        // Average
        public static uint GetLengthMetersC(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
        {
            double D2R = Math.PI / 180;
            if (radians) D2R = 1;
            double dDistance = Double.MinValue;
            double dLat1InRad = StartLat * D2R;
            double dLong1InRad = StartLong * D2R;
            double dLat2InRad = EndLat * D2R;
            double dLong2InRad = EndLong * D2R;

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            const double kEarthRadiusKms = 6378137.0000;
            dDistance = kEarthRadiusKms * c;

            return (uint)Math.Round(dDistance);
        }
        // Fastest
        public static double GetLengthMetersD(double sLat, double sLon, double eLat, double eLon, bool radians)
        {
            double EarthRadius = 6378137.0;

            double lon1 = radians ? sLon : DegToRad(sLon);
            double lon2 = radians ? eLon : DegToRad(eLon);
            double lat1 = radians ? sLat : DegToRad(sLat);
            double lat2 = radians ? eLat : DegToRad(eLat);

            return EarthRadius * (Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2)));
        }
        // Fastest
        public static double GetLengthMetersE(double sLat, double sLon, double eLat, double eLon, bool radians)
        {
            double EarthRadius = 6378137.0;

            double lon1 = radians ? sLon : DegToRad(sLon);
            double lon2 = radians ? eLon : DegToRad(eLon);
            double lat1 = radians ? sLat : DegToRad(sLat);
            double lat2 = radians ? eLat : DegToRad(eLat);

            /* This algorithm is called Sinnott's Formula */
            double dlon = (lon2) - (lon1);
            double dlat = (lat2) - (lat1);
            double a = Math.Pow(Math.Sin(dlat / 2), 2.0) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2.0);
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return EarthRadius * c;
        }
        private static double sqr(double val)
        {
            return val * val;
        }
        public static double DegToRad(double deg)
        {
            return (deg / 180.0 * Math.PI);
        }
        #endregion LENGTH


        /// <summary>
        /// Converts base data types to an array of bytes, and an array of bytes to base
        /// data types.
        /// All info taken from the meta data of System.BitConverter. This implementation
        /// allows for Endianness consideration.
        ///</summary>
        public class MyBitConverter
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            public MyBitConverter()
            {

            }

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="IsLittleEndian">Indicates the byte order ("endianess") in which data is stored in this computer architecture.</param>
            public MyBitConverter(bool IsLittleEndian)
            {
                this.isLittleEndian = IsLittleEndian;
            }

            /// <summary>
            ///     Indicates the byte order ("endianess") in which data is stored in this computer
            /// architecture.
            /// </summary>
            private bool isLittleEndian = true;

            /// <summary>
            /// Indicates the byte order ("endianess") in which data is stored in this computer
            /// architecture.
            ///</summary>
            public bool IsLittleEndian { get { return isLittleEndian; } set { isLittleEndian = value; } } // should default to false, which is what we want for Empire

            /// <summary>
            /// Converts the specified double-precision floating point number to a 64-bit
            /// signed integer.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// A 64-bit signed integer whose value is equivalent to value.
            ///</summary>
            public long DoubleToInt64Bits(double value) { throw new NotImplementedException(); }
            ///
            /// <summary>
            /// Returns the specified Boolean value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// A Boolean value.
            ///
            /// Returns:
            /// An array of bytes with length 1.
            ///</summary>
            public byte[] GetBytes(bool value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified Unicode character value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// A character to convert.
            ///
            /// Returns:
            /// An array of bytes with length 2.
            ///</summary>
            public byte[] GetBytes(char value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified double-precision floating point value as an array of
            /// bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 8.
            ///</summary>
            public byte[] GetBytes(double value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified single-precision floating point value as an array of
            /// bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 4.
            ///</summary>
            public byte[] GetBytes(float value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 32-bit signed integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 4.
            ///</summary>
            public byte[] GetBytes(int value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 64-bit signed integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 8.
            ///</summary>
            public byte[] GetBytes(long value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 16-bit signed integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 2.
            ///</summary>
            public byte[] GetBytes(short value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 32-bit unsigned integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 4.
            ///</summary>
            public byte[] GetBytes(uint value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 64-bit unsigned integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 8.
            ///</summary>
            public byte[] GetBytes(ulong value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 16-bit unsigned integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 2.
            ///</summary>
            public byte[] GetBytes(ushort value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Converts the specified 64-bit signed integer to a double-precision floating
            /// point number.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// A double-precision floating point number whose value is equivalent to value.
            ///</summary>
            public double Int64BitsToDouble(long value) { throw new NotImplementedException(); }
            ///
            /// <summary>
            /// Returns a Boolean value converted from one byte at a specified position in
            /// a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// true if the byte at startIndex in value is nonzero; otherwise, false.
            ///
            /// Exceptions:
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public bool ToBoolean(byte[] value, int startIndex) { throw new NotImplementedException(); }
            ///
            /// <summary>
            /// Returns a Unicode character converted from two bytes at a specified position
            /// in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A character formed by two bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex equals the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public char ToChar(byte[] value, int startIndex) { throw new NotImplementedException(); }
            ///
            /// <summary>
            /// Returns a double-precision floating point number converted from eight bytes
            /// at a specified position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A double precision floating point number formed by eight bytes beginning
            /// at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 7, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public double ToDouble(byte[] value, int startIndex) 
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToDouble(value, startIndex);
                }
                else
                {
                    byte[] res = new byte[8];
                    Array.Copy(value, startIndex, res, 0, 8);
                    Array.Reverse(res);
                    return System.BitConverter.ToDouble(res, 0);
                }
            }
            ///
            /// <summary>
            /// Returns a 16-bit signed integer converted from two bytes at a specified position
            /// in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 16-bit signed integer formed by two bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex equals the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public short ToInt16(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToInt16(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToInt16(res, value.Length - sizeof(Int16) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Returns a 32-bit signed integer converted from four bytes at a specified
            /// position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 32-bit signed integer formed by four bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 3, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public int ToInt32(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToInt32(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToInt32(res, value.Length - sizeof(Int32) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Returns a 64-bit signed integer converted from eight bytes at a specified
            /// position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 64-bit signed integer formed by eight bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 7, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public long ToInt64(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToInt64(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToInt64(res, value.Length - sizeof(Int64) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Returns a single-precision floating point number converted from four bytes
            /// at a specified position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A single-precision floating point number formed by four bytes beginning at
            /// startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 3, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public float ToSingle(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToSingle(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToSingle(res, value.Length - sizeof(Single) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Converts the numeric value of each element of a specified array of bytes
            /// to its equivalent hexadecimal string representation.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// Returns:
            /// A System.String of hexadecimal pairs separated by hyphens, where each pair
            /// represents the corresponding element in value; for example, "7F-2C-4A".
            ///
            /// Exceptions:
            /// System.ArgumentNullException:
            /// value is null.
            ///</summary>
            public string ToString(byte[] value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToString(value);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToString(res);
                }
            }
            ///
            /// <summary>
            /// Converts the numeric value of each element of a specified subarray of bytes
            /// to its equivalent hexadecimal string representation.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A System.String of hexadecimal pairs separated by hyphens, where each pair
            /// represents the corresponding element in a subarray of value; for example,
            /// "7F-2C-4A".
            ///
            /// Exceptions:
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public string ToString(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToString(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res, startIndex, value.Length - startIndex);
                    return System.BitConverter.ToString(res, startIndex);
                }
            }
            ///
            /// <summary>
            /// Converts the numeric value of each element of a specified subarray of bytes
            /// to its equivalent hexadecimal string representation.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// length:
            /// The number of array elements in value to convert.
            ///
            /// Returns:
            /// A System.String of hexadecimal pairs separated by hyphens, where each pair
            /// represents the corresponding element in a subarray of value; for example,
            /// "7F-2C-4A".
            ///
            /// Exceptions:
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex or length is less than zero. -or- startIndex is greater than
            /// zero and is greater than or equal to the length of value.
            ///
            /// System.ArgumentException:
            /// The combination of startIndex and length does not specify a position within
            /// value; that is, the startIndex parameter is greater than the length of value
            /// minus the length parameter.
            ///</summary>
            public string ToString(byte[] value, int startIndex, int length)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToString(value, startIndex, length);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res, startIndex, length);
                    return System.BitConverter.ToString(res, startIndex, length);
                }
            }
            ///
            /// <summary>
            /// Returns a 16-bit unsigned integer converted from two bytes at a specified
            /// position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// The array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 16-bit unsigned integer formed by two bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex equals the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public ushort ToUInt16(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToUInt16(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToUInt16(res, value.Length - sizeof(UInt16) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Returns a 32-bit unsigned integer converted from four bytes at a specified
            /// position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 32-bit unsigned integer formed by four bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 3, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public uint ToUInt32(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToUInt32(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToUInt32(res, value.Length - sizeof(UInt32) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Returns a 64-bit unsigned integer converted from eight bytes at a specified
            /// position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 64-bit unsigned integer formed by the eight bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 7, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public ulong ToUInt64(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToUInt64(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToUInt64(res, value.Length - sizeof(UInt64) - startIndex);
                }
            }
        }
    }

}