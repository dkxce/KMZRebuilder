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
    }

}