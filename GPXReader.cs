using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace KMZRebuilder
{
    public class Tachograph
    {
        /// <summary>
        ///     Track Point
        /// </summary>
        public class trkpt
        {
            /// <summary>
            ///     Latitude
            /// </summary>
            public double lat = 0;
            /// <summary>
            ///     Longitude
            /// </summary>
            public double lon = 0;
            /// <summary>
            ///     Elevation
            /// </summary>
            public double ele = 0;
            /// <summary>
            ///     Date & Time
            /// </summary>
            public DateTime time = new DateTime(1900, 1, 1);
            /// <summary>
            ///     Horizontal Speed (km/h)
            /// </summary>
            public double hspeed = 0;
            /// <summary>
            ///     Vertical Speed (m/s)
            /// </summary>
            public double vspeed = 0;
            /// <summary>
            ///     Horizontal segment length (km)
            /// </summary>
            public double seg_h_length = 0;
            /// <summary>
            ///     Vertical segment length (m)
            /// </summary>
            public double seg_v_length = 0;
            /// <summary>
            ///     Segment time
            /// </summary>
            public TimeSpan seg_time = new TimeSpan();
            /// <summary>
            ///     Time lapse from start
            /// </summary>
            public TimeSpan timeLapse = new TimeSpan();
            /// <summary>
            ///     Distance from start (km)
            /// </summary>
            public double distance = 0;

            /// <summary>
            ///     Track Point
            /// </summary>
            /// <param name="lat">Latitude</param>
            /// <param name="lon">Longitude</param>
            /// <param name="ele">Elevation</param>
            /// <param name="time">Date & Time</param>
            public trkpt(double lat, double lon, double ele, DateTime time)
            {
                this.lat = lat;
                this.lon = lon;
                this.ele = ele;
                this.time = time;
            }

            public override string ToString()
            {
                return timeLapse.Hours + ":" + timeLapse.Minutes + ":" + timeLapse.Seconds + " " + distance + "km " + hspeed + "km/h " + lat + " " + lon;
            }
        }

        /// <summary>
        ///     Track Segment
        /// </summary>
        public class trkseg
        {
            /// <summary>
            ///     Track Points
            /// </summary>
            public trkpt[] points = new trkpt[0];
            /// <summary>
            ///     Minimum Latitude
            /// </summary>
            public double MinLat = double.MaxValue;
            /// <summary>
            ///     Maximum Latitude
            /// </summary>
            public double MaxLat = double.MinValue;
            /// <summary>
            ///     Minimum Longitude
            /// </summary>
            public double MinLon = double.MaxValue;
            /// <summary>
            ///     Maximum Longitude
            /// </summary>
            public double MaxLon = double.MinValue;
            /// <summary>
            ///     Minimum Elevation
            /// </summary>
            public double MinEle = double.MaxValue;
            /// <summary>
            ///     Maximum Elevation
            /// </summary>
            public double MaxEle = double.MinValue;
            /// <summary>
            ///     Minimum Horizontal Speed (km/h)
            /// </summary>
            public double MinSpeed = double.MaxValue;
            /// <summary>
            ///     Maximum Horizontal Speed (km/h)
            /// </summary>
            public double MaxSpeed = double.MinValue;
            /// <summary>
            ///     Average Horizontal Speed (km/h)
            /// </summary>
            public double AvgSpeed = 0;
            /// <summary>
            ///     Total Length (km)
            /// </summary>
            public double TotalLength = 0;
            /// <summary>
            ///     Total Time
            /// </summary>
            public TimeSpan TotalTime = new TimeSpan();
            /// <summary>
            ///     Start Time
            /// </summary>
            public DateTime MinTime = new DateTime(1900, 1, 1);
            /// <summary>
            ///     Finish Time
            /// </summary>
            public DateTime MaxTime = new DateTime(8000, 1, 1);

            public trkseg() { }
        }

        /// <summary>
        ///     Track Segments
        /// </summary>
        private List<trkseg> track = new List<trkseg>();
        /// <summary>
        ///     Track Start Time
        /// </summary>
        private DateTime trackStartTime = new DateTime(1900, 1, 1);
        /// <summary>
        ///     Track End Time
        /// </summary>
        private DateTime trackEndTime = new DateTime(8000, 1, 1);

        /// <summary>
        ///     Graph Image
        /// </summary>
        private Bitmap bmp;
        /// <summary>
        ///     Graph Image
        /// </summary>
        private Graphics g;
        /// <summary>
        ///     Graph Width
        /// </summary>
        private int width = 600;
        /// <summary>
        ///     Graph Height
        /// </summary>
        private int height = 600;
        /// <summary>
        ///     Center of the Graph
        /// </summary>
        private Point center;

        /// <summary>
        ///     Brush for km/h labels
        /// </summary>
        private Brush kmFontBrush = new SolidBrush(Color.FromArgb(80, 80, 80));
        /// <summary>
        ///     Brush for hours labels
        /// </summary>
        private Brush HoursFontBrush = Brushes.Black;
        /// <summary>
        ///     Pen for hour lines
        /// </summary>
        private Pen HoursDelimPen = new Pen(new SolidBrush(Color.FromArgb(230, 230, 230)), 2);
        /// <summary>
        ///     Pen for main km/h lines
        /// </summary>
        private Pen kmMainLinesPen = new Pen(new SolidBrush(Color.FromArgb(20, 20, 20)), 1);
        /// <summary>
        ///     Pen for misc km lines (110,90)
        /// </summarykm/h
        private Pen kmMiscLinesPen = new Pen(new SolidBrush(Color.FromArgb(100, 200, 120, 120)), 1);
        /// <summary>
        ///     Fill pen for misc km/h lines (110,90)
        /// </summary>
        private Pen kmMiscLinesFill = new Pen(new SolidBrush(Color.FromArgb(100, 180, 180, 180)), 2);
        /// <summary>
        ///     Bold pen for avtivity zone
        /// </summary>
        private Pen activityPenBold = new Pen(Brushes.Navy, 2);
        /// <summary>
        ///     Pen for avtivity zone
        /// </summary>
        private Pen activityPen = new Pen(Brushes.Black, 1);

        /// <summary>
        ///     Background color
        /// </summary>
        private Brush bFillBackground = Brushes.White;
        /// <summary>
        ///     Night zone color
        /// </summary>
        private Brush bFillNight = new SolidBrush(Color.FromArgb(220, 220, 255));
        /// <summary>
        ///     Hours font
        /// </summary>
        private Font hoursFont = new Font("Arial", 12, FontStyle.Bold);
        /// <summary>
        ///     Hours small font
        /// </summary>
        private Font hoursSmallFont = new Font("Arial", 10, FontStyle.Bold);
        /// <summary>
        ///     km/h font
        /// </summary>
        private Font kmphFont = new Font("MS Sans Serif", 9);
        /// <summary>
        ///     Text zone font
        /// </summary>
        private Font textFont = new Font("Arial", 11, FontStyle.Bold);
        /// <summary>
        ///     Text zone main color
        /// </summary>
        private Brush textBrush1 = Brushes.Black;
        /// <summary>
        ///     Text zone 2 color
        /// </summary>
        private Brush textBrush2 = Brushes.Maroon;
        /// <summary>
        ///     Text zone 3 color
        /// </summary>
        private Brush textBrush3 = Brushes.Navy;

        /// <summary>
        ///     Множитель радиуса для 125 км/ч
        /// </summary>
        const double sc125kmph = 1.02;
        /// <summary>
        ///     Множитель радиуса для 120 км/ч
        /// </summary>
        const double sc120kmph = 1;
        /// <summary>
        ///     Множитель радиуса для 110 км/ч
        /// </summary>
        const double sc110kmph = 0.96;
        /// <summary>
        ///     Множитель радиуса для 100 км/ч
        /// </summary>
        const double sc100kmph = 0.92;
        /// <summary>
        ///     Множитель радиуса для 90 км/ч
        /// </summary>
        const double sc90kmph = 0.88;
        /// <summary>
        ///     Множитель радиуса для 80 км/ч
        /// </summary>
        const double sc80kmph = 0.84;
        /// <summary>
        ///     Множитель радиуса для 60 км/ч
        /// </summary>
        const double sc60kmph = 0.76;
        /// <summary>
        ///     Множитель радиуса для 40 км/ч
        /// </summary>
        const double sc40kmph = 0.68;
        /// <summary>
        ///     Множитель радиуса для 20 км/ч
        /// </summary>
        const double sc20kmph = 0.60;
        /// <summary>
        ///     Множитель радиуса для 0 км/ч
        /// </summary>
        const double sc0kmph = 0.52;
        /// <summary>
        ///     Множитель сдвига зоны активности от смежных зон
        /// </summary>
        const double scActivityOffset = 0.02;
        /// <summary>
        ///     Множитель расстояния между зонами активности
        /// </summary>
        const double scActivityScale = 0.04;
        /// <summary>
        ///     Множитель внутреннего радиуса зоны активности
        /// </summary>
        const double scActivityInside = 0.32;
        /// <summary>
        ///     Отступ от внешнего радиуса диска
        /// </summary>
        const double otstup = 15;
        /// <summary>
        ///     Максимальный радиус скоростной зоны и зоны активности
        /// </summary>
        double maxRadiusLineScale = 300 - otstup;
        /// <summary>
        ///     Масштабность для скорость в 100 км/ч
        /// </summary>
        const double sc0_100kmph = (sc100kmph - sc0kmph) / 100;

        /// <summary>
        ///     driver text
        /// </summary>
        private string _driverText = "";
        /// <summary>
        ///     Start Point
        /// </summary>
        private string _startFrom = "";
        /// <summary>
        ///     End Point
        /// </summary>
        private string _endTo = "";
        /// <summary>
        ///     Start Date
        /// </summary>
        private string _startDT = "";
        /// <summary>
        ///     End Date
        /// </summary>
        private string _endDT = "";
        /// <summary>
        ///  Vehicle number
        /// </summary>
        private string _vehicle = "";
        /// <summary>
        ///     Start odometer
        /// </summary>
        private string _startKM = "";
        /// <summary>
        ///     End odometer
        /// </summary>
        private string _endKM = "";
        /// <summary>
        ///     Text in text zone align by center
        /// </summary>
        private bool _altCenter = false;
        /// <summary>
        ///     Sunrise time
        /// </summary>
        private DateTime _sunrise = DateTime.Now.Date.AddHours(5).AddMinutes(37);
        /// <summary>
        ///     Sunset time
        /// </summary>
        private DateTime _sunset = DateTime.Now.Date.AddHours(21).AddMinutes(23);
        /// <summary>
        ///     Sunset visible
        /// </summary>
        private bool _vSS = false;
        /// <summary>
        ///     Sunrise visible
        /// </summary>
        private bool _vSR = false;
        /// <summary>
        ///     custom start odometer text
        /// </summary>
        private bool customStartOdometerText = false;
        /// <summary>
        ///     custom end odometer text
        /// </summary>
        private bool customFinishOdometerText = false;
        /// <summary>
        ///     custom start/finish date text
        /// </summary>
        private bool customStartFinishDateText = false;
        /// <summary>
        ///     custom start/finish point text
        /// </summary>
        private bool customStartFinishPointText = false;

        /// <summary>
        ///     Driver Name Text
        /// </summary>
        public string DriverText { get { return _driverText; } set { _driverText = value; } }
        /// <summary>
        ///     Vehicle Number Text
        /// </summary>
        public string VehicleText { get { return _vehicle; } set { _vehicle = value; } }

        /// <summary>
        ///     Start From Point Text
        /// </summary>
        public string StartPointText { get { return _startFrom; } set { customStartFinishPointText = true; _startFrom = value; } }
        /// <summary>
        ///     Finish To Point Text
        /// </summary>
        public string FinishPointText { get { return _endTo; } set { customStartFinishPointText = true; _endTo = value; } }
        /// <summary>
        ///     Start From Date Text
        /// </summary>
        public string StartDateText { get { return _startDT; } set { customStartFinishDateText = true; _startDT = value; } }
        /// <summary>
        ///     Finish To Date Text
        /// </summary>
        public string FinishDateText { get { return _endDT; } set { customStartFinishDateText = true; _endDT = value; } }
        /// <summary>
        ///     Start From Odometer Value
        /// </summary>
        public string StartKMText { get { return _startKM; } set { customStartOdometerText = true; _startKM = value; RecalculateTrackOutputOptions(false); } }
        /// <summary>
        ///     Finish To Odometer Value
        /// </summary>
        public string FinishKMText { get { return _endKM; } set { customFinishOdometerText = true; _endKM = value; RecalculateTrackOutputOptions(false); } }

        /// <summary>
        ///     Text in text zone align by center   
        /// </summary>
        public bool AlignTextToCenter { get { return _altCenter; } set { _altCenter = value; } }
        /// <summary>
        ///     Text font in text zone
        /// </summary>
        public Font TextFont { get { return textFont; } set { textFont = value; } }
        /// <summary>
        ///     Sunset Time
        /// </summary>
        public DateTime SunsetTime { get { return _sunset; } set { _sunset = value; } }
        /// <summary>
        ///     Sunrise time
        /// </summary>
        public DateTime SunriseTime { get { return _sunrise; } set { _sunrise = value; } }
        /// <summary>
        ///     Sunset Visible
        /// </summary>
        public bool SunsetVisible { get { return _vSS; } set { _vSS = value; } }
        /// <summary>
        ///     Sunrise Visible
        /// </summary>
        public bool SunriseVisible { get { return _vSR; } set { _vSR = value; } }
        /// <summary>
        ///     Graph begins at TrackStartTime    
        /// </summary>
        public DateTime TrackStartTime { get { return trackStartTime; } set { trackStartTime = value; RecalculateTrackOutputOptions(false); } }
        /// <summary>
        ///     Graph ends at TrackFinishTime
        /// </summary>
        public DateTime TrackFinishTime { get { return trackEndTime; } set { trackEndTime = value; RecalculateTrackOutputOptions(false); } }
        /// <summary>
        ///     Custom Start Odometer Text
        /// </summary>
        public bool CustomStartOdometerText { get { return customStartOdometerText; } set { customStartOdometerText = value; RecalculateTrackOutputOptions(false); } }
        /// <summary>
        ///     Custom Finish Odometer Text
        /// </summary>
        public bool CustomFinishOdometerText { get { return customFinishOdometerText; } set { customFinishOdometerText = value; RecalculateTrackOutputOptions(false); } }
        /// <summary>
        ///     Custom Start/Finish Date Text
        /// </summary>
        public bool CustomStartFinishDateText { get { return customStartFinishDateText; } set { customStartFinishDateText = value; } }
        /// <summary>
        ///     Custom Start/Finish Point Text
        /// </summary>
        public bool CustomStartFinishPointText { get { return customStartFinishPointText; } set { customStartFinishPointText = value; } }

        System.Globalization.NumberFormatInfo nfi;

        /// <summary>
        ///     Create Tachogramm
        /// </summary>
        public Tachograph()
        {
            nfi = (System.Globalization.NumberFormatInfo)
            System.Globalization.CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ".";

            Init();
        }

        /// <summary>
        ///     Create Tachogramm
        /// </summary>
        /// <param name="width">graph width</param>
        /// <param name="height">graph height</param>
        public Tachograph(int width, int height)
        {
            nfi = (System.Globalization.NumberFormatInfo)
            System.Globalization.CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ".";

            this.width = width;
            this.height = height;
            Init();
        }

        /// <summary>
        ///     Resize graph
        /// </summary>
        /// <param name="width">graph width</param>
        /// <param name="height">graph height</param>
        public void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            Init();
        }

        private void RecalculateTrackOutputOptions()
        {
            RecalculateTrackOutputOptions(true);
        }

        private void RecalculateTrackOutputOptions(bool nullDates)
        {
            if (nullDates)
            {
                trackStartTime = new DateTime(8000, 1, 1);
                trackEndTime = new DateTime(1900, 1, 1);
                foreach (trkseg ts in track)
                {
                    if (trackStartTime > ts.MinTime) trackStartTime = ts.MinTime;
                    if (trackEndTime < ts.MaxTime) trackEndTime = ts.MaxTime;
                };
            };

            double skipped_dist = 0;
            double dist = 0;
            string sfTxt = ""; DateTime minT = new DateTime(8000, 1, 1);
            string etTxt = ""; DateTime maxT = new DateTime(1900, 1, 1);
            foreach (trkseg ts in this.track)
            {
                for (int i = 0; i < ts.points.Length; i++)
                {
                    trkpt curr = ts.points[i];
                    if (curr.time < TrackStartTime) { skipped_dist += curr.seg_h_length; continue; };
                    if (curr.time > TrackFinishTime) continue;

                    dist += curr.seg_h_length;

                    if (minT > curr.time)
                    {
                        minT = curr.time;
                        sfTxt = curr.lat.ToString("0.000000", nfi) + "° " + curr.lon.ToString("0.000000", nfi) + "°";
                    };
                    if (maxT < curr.time)
                    {
                        maxT = curr.time;
                        etTxt = curr.lat.ToString("0.000000", nfi) + "° " + curr.lon.ToString("0.000000", nfi) + "°";
                    };
                }
            };

            if (!customStartOdometerText)
            {
                _startKM = skipped_dist.ToString("0.00", nfi);
                if (!customFinishOdometerText)
                    _endKM = (skipped_dist + dist).ToString("0.00", nfi);
            };
            if (!customFinishOdometerText)
            {
                if (!customStartOdometerText)
                    _endKM = (skipped_dist + dist).ToString("0.00", nfi);
                else
                {
                    double d = 0;
                    double.TryParse(_startKM, out d);
                    _endKM = (d + dist).ToString("0.00", nfi);
                };
            };

            if (!customStartFinishPointText)
            {
                _startFrom = sfTxt; // track[0].points[0].lat.ToString("0.000000", nfi) + "° " + track[0].points[0].lon.ToString("0.000000", nfi) + "°";
                _endTo = etTxt; // track[track.Count - 1].points[track[track.Count - 1].points.Length - 1].lat.ToString("0.000000", nfi) + "° " + track[track.Count - 1].points[track[track.Count - 1].points.Length - 1].lon.ToString("0.000000", nfi) + "°";
            };

            if (!customStartFinishDateText)
            {
                _startDT = trackStartTime.ToString("ddd dd.MM.yyyy HH:mm:ss");
                _endDT = trackEndTime.ToString("ddd dd.MM.yyyy HH:mm:ss");
            };
        }

        /// <summary>
        ///     Set or reset to new track for graph
        /// </summary>
        /// <param name="track">track segments</param>
        public void SetTrack(trkseg[] track)
        {
            this.track.Clear();
            this.track.AddRange(track);
            RecalculateTrackOutputOptions();
        }

        /// <summary>
        ///     Add segement to track for graph
        /// </summary>
        /// <param name="segment">track segment</param>
        public void AddSegment(trkseg segment)
        {
            this.track.Add(segment);
            RecalculateTrackOutputOptions();
        }

        /// <summary>
        ///     Remove track (clear graph)
        /// </summary>
        public void RemoveTrack()
        {
            this.track.Clear();
        }

        /// <summary>
        ///     Init graph
        /// </summary>
        private void Init()
        {
            if (width > height) width = height;
            if (height > width) height = width;
            center = new Point(width / 2, height / 2);

            bmp = new Bitmap(width, height);
            g = Graphics.FromImage(bmp);

            maxRadiusLineScale = this.width / 2 - otstup;
        }

        /// <summary>
        ///     Draw empty tachogramm disk
        /// </summary>
        private void DrawTemplate()
        {
            double sunriseAngle = 15 * _sunrise.Hour + 0.25 * _sunrise.Minute;
            double sunsetAngle = 15 * _sunset.Hour + 0.25 * _sunset.Minute;

            // CIRCLE
            Rectangle fRect = new Rectangle(0, 0, width - 1, height - 1);
            g.FillEllipse(bFillBackground, fRect);
            g.DrawPie(new Pen(Color.Brown, 3), fRect, 0, 180);
            g.DrawLine(new Pen(Color.White, 5), new Point(0, center.Y), new Point(width, center.Y));
            g.DrawEllipse(new Pen(Color.Brown, 2), fRect);

            // NIGHT
            if (_vSR && _vSS)
                g.FillPie(bFillNight, fRect, (float)sunsetAngle + 180, (float)(sunriseAngle + 360 - sunsetAngle));

            // km/h LINES & Activity Lines            
            g.DrawEllipse(kmMainLinesPen, (int)(center.X - maxRadiusLineScale * sc120kmph), (int)(center.Y - maxRadiusLineScale * sc120kmph), (int)(maxRadiusLineScale * sc120kmph) * 2, (int)(maxRadiusLineScale * sc120kmph) * 2);
            g.DrawEllipse(kmMainLinesPen, (int)(center.X - maxRadiusLineScale * sc100kmph), (int)(center.Y - maxRadiusLineScale * sc100kmph), (int)(maxRadiusLineScale * sc100kmph) * 2, (int)(maxRadiusLineScale * sc100kmph) * 2);
            g.DrawEllipse(kmMainLinesPen, (int)(center.X - maxRadiusLineScale * sc80kmph), (int)(center.Y - maxRadiusLineScale * sc80kmph), (int)(maxRadiusLineScale * sc80kmph) * 2, (int)(maxRadiusLineScale * sc80kmph) * 2);
            g.DrawEllipse(kmMiscLinesFill, (int)(center.X - maxRadiusLineScale * sc60kmph), (int)(center.Y - maxRadiusLineScale * sc60kmph), (int)(maxRadiusLineScale * sc60kmph) * 2, (int)(maxRadiusLineScale * sc60kmph) * 2);
            g.DrawEllipse(kmMainLinesPen, (int)(center.X - maxRadiusLineScale * sc60kmph), (int)(center.Y - maxRadiusLineScale * sc60kmph), (int)(maxRadiusLineScale * sc60kmph) * 2, (int)(maxRadiusLineScale * sc60kmph) * 2);
            g.DrawEllipse(kmMainLinesPen, (int)(center.X - maxRadiusLineScale * sc40kmph), (int)(center.Y - maxRadiusLineScale * sc40kmph), (int)(maxRadiusLineScale * sc40kmph) * 2, (int)(maxRadiusLineScale * sc40kmph) * 2);
            g.DrawEllipse(kmMainLinesPen, (int)(center.X - maxRadiusLineScale * sc20kmph), (int)(center.Y - maxRadiusLineScale * sc20kmph), (int)(maxRadiusLineScale * sc20kmph) * 2, (int)(maxRadiusLineScale * sc20kmph) * 2);
            for (double angle = 0; angle < 360; angle += 15 / 4)
            {
                bool fillNight = _vSR && _vSS && (!(angle > (sunriseAngle) && (angle < (sunsetAngle))));
                Pen p = new Pen(fillNight ? bFillNight : bFillBackground, 5);
                double x2 = center.X + (width / 2 - ((angle - 90) % 3.75 > 0 ? 3 : 6 + ((angle - 90) % 5 > 0 ? 0 : 4))) * Math.Sin((angle - 90) * Math.PI / 180);
                double y2 = center.Y - (height / 2 - ((angle - 90) % 3.75 > 0 ? 3 : 6 + ((angle - 90) % 5 > 0 ? 0 : 4))) * Math.Cos((angle - 90) * Math.PI / 180);
                g.DrawLine(p, new Point((int)x2, (int)y2), new Point(center.X, center.Y));
            };
            g.DrawEllipse(kmMiscLinesFill, (int)(center.X - maxRadiusLineScale * sc110kmph), (int)(center.Y - maxRadiusLineScale * sc110kmph), (int)(maxRadiusLineScale * sc110kmph) * 2, (int)(maxRadiusLineScale * sc110kmph) * 2);
            g.DrawEllipse(kmMiscLinesPen, (int)(center.X - maxRadiusLineScale * sc110kmph), (int)(center.Y - maxRadiusLineScale * sc110kmph), (int)(maxRadiusLineScale * sc110kmph) * 2, (int)(maxRadiusLineScale * sc110kmph) * 2);
            g.DrawEllipse(kmMiscLinesFill, (int)(center.X - maxRadiusLineScale * sc90kmph), (int)(center.Y - maxRadiusLineScale * sc90kmph), (int)(maxRadiusLineScale * sc90kmph) * 2, (int)(maxRadiusLineScale * sc90kmph) * 2);
            g.DrawEllipse(kmMiscLinesPen, (int)(center.X - maxRadiusLineScale * sc90kmph), (int)(center.Y - maxRadiusLineScale * sc90kmph), (int)(maxRadiusLineScale * sc90kmph) * 2, (int)(maxRadiusLineScale * sc90kmph) * 2);
            for (double angle = 0; angle < 360; angle += 15)
            {
                bool fillNight = _vSR && _vSS && (!(angle > (sunriseAngle) && (angle < (sunsetAngle))));
                Pen p = new Pen(fillNight ? bFillNight : bFillBackground, 7);
                double x2 = center.X + (maxRadiusLineScale * sc125kmph - 1) * Math.Sin((angle - 90) * Math.PI / 180);
                double y2 = center.Y - (maxRadiusLineScale * sc125kmph - 1) * Math.Cos((angle - 90) * Math.PI / 180);
                g.DrawLine(p, new Point((int)x2, (int)y2), new Point(center.X, center.Y));
                double x1 = center.X + (maxRadiusLineScale * sc0kmph - 1) * Math.Sin((angle - 90) * Math.PI / 180);
                double y1 = center.Y - (maxRadiusLineScale * sc0kmph - 1) * Math.Cos((angle - 90) * Math.PI / 180);
                g.DrawLine(HoursDelimPen, new Point((int)x2, (int)y2), new Point((int)x1, (int)y1));
            };
            g.DrawEllipse(new Pen(Color.Brown, 3), (int)(center.X - maxRadiusLineScale * sc0kmph), (int)(center.Y - maxRadiusLineScale * sc0kmph), (int)(maxRadiusLineScale * sc0kmph) * 2, (int)(maxRadiusLineScale * sc0kmph) * 2);
            for (int i = 0; i < 4; i++)
            {
                drawRotatedBgText(g, (int)(center.X + (maxRadiusLineScale - 1) * sc20kmph * Math.Sin((i * 90 - 67.5) * Math.PI / 180)), (int)(center.Y - (maxRadiusLineScale - 1) * sc20kmph * Math.Cos((i * 90 - 67.5) * Math.PI / 180)), (float)(i * 90 - 67.5), "20км/ч", kmphFont, kmFontBrush, _vSR && _vSS && (!((i * 90 + 90 - 67.5) > (sunriseAngle) && ((i * 90 + 90 - 67.5) < (sunsetAngle)))) ? bFillNight : bFillBackground);
                drawRotatedBgText(g, (int)(center.X + (maxRadiusLineScale - 1) * sc40kmph * Math.Sin((i * 90 - 52.5) * Math.PI / 180)), (int)(center.Y - (maxRadiusLineScale - 1) * sc40kmph * Math.Cos((i * 90 - 52.5) * Math.PI / 180)), (float)(i * 90 - 52.5), "40км/ч", kmphFont, kmFontBrush, _vSR && _vSS && (!((i * 90 + 90 - 52.5) > (sunriseAngle) && ((i * 90 + 90 - 52.5) < (sunsetAngle)))) ? bFillNight : bFillBackground);
                drawRotatedBgText(g, (int)(center.X + (maxRadiusLineScale - 1) * sc60kmph * Math.Sin((i * 90 - 37.5) * Math.PI / 180)), (int)(center.Y - (maxRadiusLineScale - 1) * sc60kmph * Math.Cos((i * 90 - 37.5) * Math.PI / 180)), (float)(i * 90 - 37.5), "60км/ч", kmphFont, kmFontBrush, _vSR && _vSS && (!((i * 90 + 90 - 37.5) > (sunriseAngle) && ((i * 90 + 90 - 37.5) < (sunsetAngle)))) ? bFillNight : bFillBackground);
                drawRotatedBgText(g, (int)(center.X + (maxRadiusLineScale - 1) * sc80kmph * Math.Sin((i * 90 - 22.5) * Math.PI / 180)), (int)(center.Y - (maxRadiusLineScale - 1) * sc80kmph * Math.Cos((i * 90 - 22.5) * Math.PI / 180)), (float)(i * 90 - 22.5), "80км/ч", kmphFont, kmFontBrush, _vSR && _vSS && (!((i * 90 + 90 - 22.5) > (sunriseAngle) && ((i * 90 + 90 - 22.5) < (sunsetAngle)))) ? bFillNight : bFillBackground);
                drawRotatedBgText(g, (int)(center.X + (maxRadiusLineScale - 1) * sc100kmph * Math.Sin((i * 90 - 7.5) * Math.PI / 180)), (int)(center.Y - (maxRadiusLineScale - 1) * sc100kmph * Math.Cos((i * 90 - 7.5) * Math.PI / 180)), (float)(i * 90 - 7.5), "100км/ч", kmphFont, kmFontBrush, _vSR && _vSS && (!((i * 90 + 90 - 7.5) > (sunriseAngle) && ((i * 90 + 90 - 7.5) < (sunsetAngle)))) ? bFillNight : bFillBackground);
                drawRotatedBgText(g, (int)(center.X + (maxRadiusLineScale - 1) * sc120kmph * Math.Sin((i * 90 + 7.5) * Math.PI / 180)), (int)(center.Y - (maxRadiusLineScale - 1) * sc120kmph * Math.Cos((i * 90 + 7.5) * Math.PI / 180)), (float)(i * 90 + 7.5), "120км/ч", kmphFont, kmFontBrush, _vSR && _vSS && (!((i * 90 + 90 + 7.5) > (sunriseAngle) && ((i * 90 + 90 + 7.5) < (sunsetAngle)))) ? bFillNight : bFillBackground);
            };

            // MINUTES
            for (double angle = 0; angle < 360; angle += 1.25)
            {
                Pen p = new Pen(Brushes.Brown, 1);
                double x1 = center.X + (width / 2) * Math.Sin(angle * Math.PI / 180);
                double y1 = center.Y - (height / 2) * Math.Cos(angle * Math.PI / 180);
                double x2 = center.X + (width / 2 - (angle % 3.75 > 0 ? 3 : 6 + (angle % 5 > 0 ? 0 : 4))) * Math.Sin(angle * Math.PI / 180);
                double y2 = center.Y - (height / 2 - (angle % 3.75 > 0 ? 3 : 6 + (angle % 5 > 0 ? 0 : 4))) * Math.Cos(angle * Math.PI / 180);
                g.DrawLine(p, new Point((int)x1, (int)y1), new Point((int)x2, (int)y2));
                x1 = center.X + (maxRadiusLineScale * sc0kmph + (angle % 3.75 > 0 ? 3 : 6 + (angle % 5 > 0 ? 0 : 4))) * Math.Sin((angle - 180) * Math.PI / 180);
                y1 = center.Y - (maxRadiusLineScale * sc0kmph + (angle % 3.75 > 0 ? 3 : 6 + (angle % 5 > 0 ? 0 : 4))) * Math.Cos((angle - 180) * Math.PI / 180);
                x2 = center.X + (maxRadiusLineScale * sc0kmph) * Math.Sin((angle - 180) * Math.PI / 180);
                y2 = center.Y - (maxRadiusLineScale * sc0kmph) * Math.Cos((angle - 180) * Math.PI / 180);
                g.DrawLine(p, new Point((int)x1, (int)y1), new Point((int)x2, (int)y2));
            };

            // HOURS            
            for (int hour = 1; hour < 25; hour += 1)
            {
                int angle = 90 + 15 * hour;
                string txt = hour.ToString();
                SizeF tSize = g.MeasureString(txt, hoursFont);
                double dx = (width / 2 - tSize.Width / 2 - 4) * Math.Sin((angle - 180) * Math.PI / 180);
                double dy = (height / 2 - tSize.Height / 2 - 4) * Math.Cos((angle - 180) * Math.PI / 180);
                int x = (int)(center.X + dx);
                int y = (int)(center.Y - dy);
                drawRotatedText(g, x, y, 180 + angle, txt, hoursFont, HoursFontBrush);
                tSize = g.MeasureString(txt, hoursSmallFont);
                double x1 = center.X + (maxRadiusLineScale * sc0kmph + 4) * Math.Sin((angle - 180) * Math.PI / 180);
                double y1 = center.Y - (maxRadiusLineScale * sc0kmph + 4) * Math.Cos((angle - 180) * Math.PI / 180);
                drawRotatedText(g, (int)x1, (int)y1, 180 + angle, txt, hoursSmallFont, HoursFontBrush);
            };

            g.FillEllipse(bFillBackground, new Rectangle((int)(center.X - maxRadiusLineScale * sc0kmph) + 3, (int)(center.Y - maxRadiusLineScale * sc0kmph) + 3, (int)(maxRadiusLineScale * sc0kmph) * 2 - 6, (int)(maxRadiusLineScale * sc0kmph) * 2 - 6));

            // Activity
            g.DrawEllipse(activityPen, (int)(center.X - maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 0))), (int)(center.Y - maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 0))), (int)(maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 0))) * 2, (int)(maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 0))) * 2);
            g.DrawEllipse(activityPen, (int)(center.X - maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 1))), (int)(center.Y - maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 1))), (int)(maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 1))) * 2, (int)(maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 1))) * 2);
            g.DrawEllipse(activityPen, (int)(center.X - maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 2))), (int)(center.Y - maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 2))), (int)(maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 2))) * 2, (int)(maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 2))) * 2);
            g.DrawEllipse(activityPen, (int)(center.X - maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 3))), (int)(center.Y - maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 3))), (int)(maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 3))) * 2, (int)(maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 3))) * 2);
            g.DrawEllipse(activityPen, (int)(center.X - maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 4))), (int)(center.Y - maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 4))), (int)(maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 4))) * 2, (int)(maxRadiusLineScale * (sc0kmph - scActivityOffset - (scActivityScale * 4))) * 2);
            for (double angle = 0; angle < 360; angle += 15 / 4)
            {
                Pen p = new Pen(bFillBackground, 4);
                double x1 = center.X + maxRadiusLineScale * (sc0kmph - scActivityOffset) * Math.Sin(angle * Math.PI / 180);
                double y1 = center.Y - maxRadiusLineScale * (sc0kmph - scActivityOffset) * Math.Cos(angle * Math.PI / 180);
                double x2 = center.X + maxRadiusLineScale * (scActivityInside) * Math.Sin(angle * Math.PI / 180);
                double y2 = center.Y - maxRadiusLineScale * (scActivityInside) * Math.Cos(angle * Math.PI / 180);
                g.DrawLine(p, new Point((int)x1, (int)y1), new Point((int)x2, (int)y2));
                if (angle % 15 == 0)
                    g.DrawLine(HoursDelimPen, new Point((int)x2, (int)y2), new Point((int)x1, (int)y1));
            };
            g.DrawEllipse(activityPenBold, (int)(center.X - maxRadiusLineScale * scActivityInside), (int)(center.Y - maxRadiusLineScale * scActivityInside), (int)(maxRadiusLineScale * scActivityInside) * 2, (int)(maxRadiusLineScale * scActivityInside) * 2);
            g.DrawEllipse(activityPenBold, (int)(center.X - maxRadiusLineScale * (scActivityInside - scActivityOffset)), (int)(center.Y - maxRadiusLineScale * (scActivityInside - scActivityOffset)), (int)(maxRadiusLineScale * (scActivityInside - scActivityOffset)) * 2, (int)(maxRadiusLineScale * (scActivityInside - scActivityOffset)) * 2);
            for (int i = 0; i < 4; i++)
            {
                drawRotatedImage(g, (int)(center.X + (maxRadiusLineScale * (scActivityInside + scActivityOffset + scActivityScale * 3.4)) * Math.Sin((i * 90 + 22.5) * Math.PI / 180)), (int)(center.Y - (maxRadiusLineScale * (scActivityInside + scActivityOffset + scActivityScale * 3.4)) * Math.Cos((i * 90 + 22.5) * Math.PI / 180)), (float)(i * 90 + 22.5), global::KMZRebuilder.Properties.Resources._01DRIVE);
                drawRotatedImage(g, (int)(center.X + (maxRadiusLineScale * (scActivityInside + scActivityOffset + scActivityScale * 2.4)) * Math.Sin((i * 90 + 7.5) * Math.PI / 180)), (int)(center.Y - (maxRadiusLineScale * (scActivityInside + scActivityOffset + scActivityScale * 2.4)) * Math.Cos((i * 90 + 7.5) * Math.PI / 180)), (float)(i * 90 + 7.5), global::KMZRebuilder.Properties.Resources._02WORK);
                drawRotatedImage(g, (int)(center.X + (maxRadiusLineScale * (scActivityInside + scActivityOffset + scActivityScale * 1.4)) * Math.Sin((i * 90 - 7.5) * Math.PI / 180)), (int)(center.Y - (maxRadiusLineScale * (scActivityInside + scActivityOffset + scActivityScale * 1.4)) * Math.Cos((i * 90 - 7.5) * Math.PI / 180)), (float)(i * 90 - 15), global::KMZRebuilder.Properties.Resources._04PERIODS);
                drawRotatedImage(g, (int)(center.X + (maxRadiusLineScale * (scActivityInside + scActivityOffset + scActivityScale * 0.4)) * Math.Sin((i * 90 - 22.5) * Math.PI / 180)), (int)(center.Y - (maxRadiusLineScale * (scActivityInside + scActivityOffset + scActivityScale * 0.4)) * Math.Cos((i * 90 - 22.5) * Math.PI / 180)), (float)(i * 90 - 22.5), global::KMZRebuilder.Properties.Resources._03REST);
            };

            // 24:00
            {
                Pen p = new Pen(bFillBackground, 4);
                double x1 = center.X + maxRadiusLineScale * (sc0kmph) * Math.Sin(-90 * Math.PI / 180);
                double y1 = center.Y - maxRadiusLineScale * (sc0kmph) * Math.Cos(-90 * Math.PI / 180);
                double x2 = center.X + maxRadiusLineScale * (scActivityInside) * Math.Sin(-90 * Math.PI / 180);
                double y2 = center.Y - maxRadiusLineScale * (scActivityInside) * Math.Cos(-90 * Math.PI / 180);
                g.DrawLine(Pens.Navy, new Point(0, center.Y), new Point((int)x1, (int)y1));
                g.DrawLine(Pens.Maroon, new Point((int)x1, (int)y1), new Point((int)x2, (int)y2));
            }

            // SUN PERIODS
            if (_vSR || _vSS)
            {
                if (_vSR)
                {
                    double x1 = center.X + this.width / 2 * Math.Sin((sunriseAngle - 90) * Math.PI / 180);
                    double y1 = center.Y - this.width / 2 * Math.Cos((sunriseAngle - 90) * Math.PI / 180);
                    double x2 = center.X + maxRadiusLineScale * (sc0kmph) * Math.Sin((sunriseAngle - 90) * Math.PI / 180);
                    double y2 = center.Y - maxRadiusLineScale * (sc0kmph) * Math.Cos((sunriseAngle - 90) * Math.PI / 180);
                    g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(120, Color.DarkGoldenrod)), 3), new Point((int)x1, (int)y1), new Point((int)x2, (int)y2));
                };
                if (_vSS)
                {
                    double x1 = center.X + this.width / 2 * Math.Sin((sunsetAngle - 90) * Math.PI / 180);
                    double y1 = center.Y - this.width / 2 * Math.Cos((sunsetAngle - 90) * Math.PI / 180);
                    double x2 = center.X + maxRadiusLineScale * (sc0kmph) * Math.Sin((sunsetAngle - 90) * Math.PI / 180);
                    double y2 = center.Y - maxRadiusLineScale * (sc0kmph) * Math.Cos((sunsetAngle - 90) * Math.PI / 180);
                    g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(120, Color.DarkRed)), 3), new Point((int)x1, (int)y1), new Point((int)x2, (int)y2));
                };
            };

            // LOGO
            {
                string tLogo = "GPX Tacho";
                Font logoF = new Font("Arial", 10, FontStyle.Bold);
                SizeF mLogo = g.MeasureString(tLogo, logoF);
                g.DrawString(tLogo, logoF, new SolidBrush(Color.FromArgb(200, 200, 200)), new PointF(center.X - mLogo.Width / 2, (float)(center.Y - maxRadiusLineScale * (scActivityInside - scActivityOffset) + mLogo.Height / 2)));
            };

            // LINES AND SYMBOLS
            {
                //DRIVER
                double x1 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-55 * Math.PI / 180);
                double y1 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-55 * Math.PI / 180);
                double x2 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(55 * Math.PI / 180);
                double y2 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(55 * Math.PI / 180);
                g.DrawLine(kmMiscLinesFill, (float)x1, (float)y2, (float)x2, (float)y2);
                double xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-55 * Math.PI / 180);
                double yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-55 * Math.PI / 180);
                g.DrawImage(global::KMZRebuilder.Properties.Resources._05DRIVER, (float)xi, (float)yi - 17);
                //START POINT
                x1 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-67 * Math.PI / 180);
                y1 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-67 * Math.PI / 180);
                x2 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(67 * Math.PI / 180);
                y2 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(67 * Math.PI / 180);
                g.DrawLine(kmMiscLinesFill, (float)x1, (float)y2, (float)x2, (float)y2);
                xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-67 * Math.PI / 180);
                yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-67 * Math.PI / 180);
                g.DrawImage(global::KMZRebuilder.Properties.Resources._06START, (float)xi, (float)yi - 17);
                //END POINT
                x1 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-79 * Math.PI / 180);
                y1 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-79 * Math.PI / 180);
                x2 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(79 * Math.PI / 180);
                y2 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(79 * Math.PI / 180);
                g.DrawLine(kmMiscLinesFill, (float)x1, (float)y2, (float)x2, (float)y2);
                g.DrawLine(kmMiscLinesFill, (float)x1, (float)y2, (float)x2, (float)y2);
                xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-79 * Math.PI / 180);
                yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-79 * Math.PI / 180);
                g.DrawImage(global::KMZRebuilder.Properties.Resources._07FINISH, (float)xi, (float)yi - 17);
                //TACHO INSERTED
                x1 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-91 * Math.PI / 180);
                y1 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-91 * Math.PI / 180);
                x2 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(91 * Math.PI / 180);
                y2 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(91 * Math.PI / 180);
                g.DrawLine(kmMiscLinesFill, (float)x1, (float)y2, (float)x2, (float)y2);
                xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-91 * Math.PI / 180);
                yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-91 * Math.PI / 180);
                g.DrawImage(global::KMZRebuilder.Properties.Resources._10STARTD, (float)xi, (float)yi - 17);
                //TACHO REMOVED
                x1 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-103 * Math.PI / 180);
                y1 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-103 * Math.PI / 180);
                x2 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(103 * Math.PI / 180);
                y2 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(103 * Math.PI / 180);
                g.DrawLine(kmMiscLinesFill, (float)x1, (float)y2, (float)x2, (float)y2);
                xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-103 * Math.PI / 180);
                yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-103 * Math.PI / 180);
                g.DrawImage(global::KMZRebuilder.Properties.Resources._11FINISHD, (float)xi, (float)yi - 17);
                //VEHICLE
                x1 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-115 * Math.PI / 180);
                y1 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-115 * Math.PI / 180);
                x2 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(115 * Math.PI / 180);
                y2 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(115 * Math.PI / 180);
                g.DrawLine(kmMiscLinesFill, (float)x1, (float)y2, (float)x2, (float)y2);
                xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-115 * Math.PI / 180);
                yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-115 * Math.PI / 180);
                g.DrawImage(global::KMZRebuilder.Properties.Resources._12VEHICLE, (float)xi, (float)yi - 17);
                //ODO START
                x1 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-127 * Math.PI / 180);
                y1 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-127 * Math.PI / 180);
                x2 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(127 * Math.PI / 180);
                y2 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(127 * Math.PI / 180);
                g.DrawLine(kmMiscLinesFill, (float)x1, (float)y2, (float)x2, (float)y2);
                xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-127 * Math.PI / 180);
                yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-127 * Math.PI / 180);
                g.DrawImage(global::KMZRebuilder.Properties.Resources._08KMSTART, (float)xi, (float)yi - 17);
                //ODO END
                x1 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-139 * Math.PI / 180);
                y1 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-139 * Math.PI / 180);
                x2 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(139 * Math.PI / 180);
                y2 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(139 * Math.PI / 180);
                xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-139 * Math.PI / 180);
                yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-139 * Math.PI / 180);
                g.DrawImage(global::KMZRebuilder.Properties.Resources._09KMFINISH, (float)xi, (float)yi - 17);
                g.DrawLine(kmMiscLinesFill, (float)x1, (float)y2, (float)x2, (float)y2);
                //
                x1 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-155 * Math.PI / 180);
                y1 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-155 * Math.PI / 180);
                x2 = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(155 * Math.PI / 180);
                y2 = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(155 * Math.PI / 180);
                g.DrawLine(kmMiscLinesFill, (float)x1, (float)y2, (float)x2, (float)y2);
            };

            g.FillEllipse(bFillBackground, new Rectangle((int)(center.X - maxRadiusLineScale * 0.09), (int)(center.Y - maxRadiusLineScale * 0.09), (int)(maxRadiusLineScale * 0.09) * 2, (int)(maxRadiusLineScale * 0.09) * 2));
            g.DrawEllipse(Pens.Silver, new Rectangle((int)(center.X - maxRadiusLineScale * 0.09), (int)(center.Y - maxRadiusLineScale * 0.09), (int)(maxRadiusLineScale * 0.09) * 2, (int)(maxRadiusLineScale * 0.09) * 2));
        }

        /// <summary>
        ///     Draw Rotated Text
        /// </summary>
        /// <param name="g">graph</param>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="angle">rotate anle</param>
        /// <param name="text">text</param>
        /// <param name="font">font</param>
        /// <param name="brush">color</param>
        private void drawRotatedText(Graphics g, int x, int y, float angle, string text, Font font, Brush brush)
        {
            g.TranslateTransform(x, y); // Set rotation point
            g.RotateTransform(angle); // Rotate text
            g.TranslateTransform(-x, -y); // Reset translate transform
            SizeF size = g.MeasureString(text, font); // Get size of rotated text (bounding box)
            g.DrawString(text, font, brush, new PointF(x - size.Width / 2.0f, y - size.Height / 2.0f)); // Draw string centered in x, y
            g.ResetTransform(); // Only needed if you reuse the Graphics object for multiple calls to DrawString
        }

        /// <summary>
        ///     Draw Rotated Text with background    
        /// </summary>
        /// <param name="g">graph</param>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="angle">rotate anle</param>
        /// <param name="text">text</param>
        /// <param name="font">font</param>
        /// <param name="brush">color</param>
        /// <param name="Fill">background</param>
        private void drawRotatedBgText(Graphics g, int x, int y, float angle, string text, Font font, Brush brush, Brush Fill)
        {
            g.TranslateTransform(x, y); // Set rotation point
            g.RotateTransform(angle); // Rotate text
            g.TranslateTransform(-x, -y); // Reset translate transform
            SizeF size = g.MeasureString(text, font); // Get size of rotated text (bounding box)
            g.FillRectangle(Fill, new Rectangle((int)(x - size.Width / 2.0f - 3), (int)(y - size.Height / 2.0f - 3), (int)size.Width + 6, (int)size.Height + 6));
            g.DrawString(text, font, brush, new PointF(x - size.Width / 2.0f, y - size.Height / 2.0f)); // Draw string centered in x, y
            g.ResetTransform(); // Only needed if you reuse the Graphics object for multiple calls to DrawString
        }

        /// <summary>
        ///     Draw Rotated Image    
        /// </summary>
        /// <param name="g">graph</param>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="angle">rotate anle</param>
        /// <param name="image">image</param>
        private void drawRotatedImage(Graphics g, int x, int y, float angle, Bitmap image)
        {
            g.TranslateTransform(x, y); // Set rotation point
            g.RotateTransform(angle); // Rotate text
            g.TranslateTransform(-x, -y); // Reset translate transform
            g.DrawImage(image, new Point((int)(x - image.Width / 2.0), (int)(y - image.Height / 2.0)));
            g.ResetTransform(); // Only needed if you reuse the Graphics object for multiple calls to DrawString
        }

        /// <summary>
        ///     Draw text zone data
        /// </summary>
        private void DrawTexts()
        {
            //DRIVER
            double xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-55 * Math.PI / 180);
            double yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-55 * Math.PI / 180);
            SizeF sz = g.MeasureString(_driverText, textFont);
            g.DrawString(_driverText, textFont, textBrush2, AlignTextToCenter ? center.X - sz.Width / 2 : (float)xi + 16, (float)yi - 15); ;

            //START POINT
            xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-67 * Math.PI / 180);
            yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-67 * Math.PI / 180);
            sz = g.MeasureString(_startFrom, textFont);
            g.DrawString(_startFrom, textFont, textBrush1, AlignTextToCenter ? center.X - sz.Width / 2 : (float)xi + 16, (float)yi - 15); ;

            //END POINT
            xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-79 * Math.PI / 180);
            yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-79 * Math.PI / 180);
            sz = g.MeasureString(_endTo, textFont);
            g.DrawString(_endTo, textFont, textBrush1, AlignTextToCenter ? center.X - sz.Width / 2 : (float)xi + 16, (float)yi - 15); ;

            //TACHO INSERTED
            xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-91 * Math.PI / 180);
            yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-91 * Math.PI / 180);
            sz = g.MeasureString(_startDT, textFont);
            g.DrawString(_startDT, textFont, textBrush2, AlignTextToCenter ? center.X - sz.Width / 2 : (float)xi + 16, (float)yi - 15); ;

            //TACHO REMOVED
            xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-103 * Math.PI / 180);
            yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-103 * Math.PI / 180);
            sz = g.MeasureString(_endDT, textFont);
            g.DrawString(_endDT, textFont, textBrush2, AlignTextToCenter ? center.X - sz.Width / 2 : (float)xi + 16, (float)yi - 15); ;

            //VEHICLE
            xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-115 * Math.PI / 180);
            yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-115 * Math.PI / 180);
            sz = g.MeasureString(_vehicle, textFont);
            g.DrawString(_vehicle, textFont, textBrush3, AlignTextToCenter ? center.X - sz.Width / 2 : (float)xi + 16, (float)yi - 15); ;

            //ODO START
            xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-127 * Math.PI / 180);
            yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-127 * Math.PI / 180);
            sz = g.MeasureString(_startKM, textFont);
            g.DrawString(_startKM, textFont, textBrush3, AlignTextToCenter ? center.X - sz.Width / 2 : (float)xi + 16, (float)yi - 15); ;

            //ODO END
            xi = center.X + (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Sin(-139 * Math.PI / 180);
            yi = center.Y - (maxRadiusLineScale * (scActivityInside - scActivityOffset * 2)) * Math.Cos(-139 * Math.PI / 180);
            sz = g.MeasureString(_endKM, textFont);
            g.DrawString(_endKM, textFont, textBrush3, AlignTextToCenter ? center.X - sz.Width / 2 : (float)xi + 16, (float)yi - 15);

            //CIRCLES
            g.DrawEllipse(activityPenBold, (int)(center.X - maxRadiusLineScale * scActivityInside), (int)(center.Y - maxRadiusLineScale * scActivityInside), (int)(maxRadiusLineScale * scActivityInside) * 2, (int)(maxRadiusLineScale * scActivityInside) * 2);
            g.DrawEllipse(activityPenBold, (int)(center.X - maxRadiusLineScale * (scActivityInside - scActivityOffset)), (int)(center.Y - maxRadiusLineScale * (scActivityInside - scActivityOffset)), (int)(maxRadiusLineScale * (scActivityInside - scActivityOffset)) * 2, (int)(maxRadiusLineScale * (scActivityInside - scActivityOffset)) * 2);
        }

        /// <summary>
        ///     Plot graphic data
        /// </summary>
        private void DrawData()
        {
            if (this.track == null) return;
            if (this.track.Count == 0) return;
            foreach (trkseg ts in this.track)
            {
                double prev_x = 0;
                double prev_y = 0;
                bool first = true;
                for (int i = 0; i < ts.points.Length; i++)
                {
                    trkpt curr = ts.points[i];
                    if (curr.time < TrackStartTime) continue;
                    if (curr.time > TrackFinishTime) continue;
                    //
                    double tm_val = curr.time.Subtract(curr.time.Date).TotalMinutes;
                    double tm_ang = 0.25 * tm_val;
                    double sp_val = (curr.hspeed > 130 ? 130 : curr.hspeed) * sc0_100kmph + sc0kmph;

                    // activity
                    if (curr.hspeed >= 5)
                    {
                        double x2 = center.X + (maxRadiusLineScale * (sc0kmph - scActivityOffset - scActivityScale * 0)) * Math.Sin((tm_ang - 90) * Math.PI / 180);
                        double y2 = center.Y - (maxRadiusLineScale * (sc0kmph - scActivityOffset - scActivityScale * 0)) * Math.Cos((tm_ang - 90) * Math.PI / 180);
                        double x3 = center.X + (maxRadiusLineScale * (sc0kmph - scActivityOffset - scActivityScale * 1)) * Math.Sin((tm_ang - 90) * Math.PI / 180);
                        double y3 = center.Y - (maxRadiusLineScale * (sc0kmph - scActivityOffset - scActivityScale * 1)) * Math.Cos((tm_ang - 90) * Math.PI / 180);
                        g.DrawLine(Pens.Navy, new Point((int)x3, (int)y3), new Point((int)x2, (int)(y2)));
                    };
                    if (curr.hspeed < 5)
                    {
                        double x2 = center.X + (maxRadiusLineScale * (sc0kmph - scActivityOffset - scActivityScale * 1)) * Math.Sin((tm_ang - 90) * Math.PI / 180);
                        double y2 = center.Y - (maxRadiusLineScale * (sc0kmph - scActivityOffset - scActivityScale * 1)) * Math.Cos((tm_ang - 90) * Math.PI / 180);
                        double x3 = center.X + (maxRadiusLineScale * (sc0kmph - scActivityOffset - scActivityScale * 2)) * Math.Sin((tm_ang - 90) * Math.PI / 180);
                        double y3 = center.Y - (maxRadiusLineScale * (sc0kmph - scActivityOffset - scActivityScale * 2)) * Math.Cos((tm_ang - 90) * Math.PI / 180);
                        g.DrawLine(Pens.DarkGreen, new Point((int)x3, (int)y3), new Point((int)x2, (int)(y2)));
                    };

                    // speed
                    double x1 = center.X + (maxRadiusLineScale * sp_val) * Math.Sin((tm_ang - 90) * Math.PI / 180);
                    double y1 = center.Y - (maxRadiusLineScale * sp_val) * Math.Cos((tm_ang - 90) * Math.PI / 180);
                    //
                    if (first)
                    {
                        prev_x = center.X + (maxRadiusLineScale * sc0kmph) * Math.Sin((tm_ang - 90) * Math.PI / 180);
                        prev_y = center.Y - (maxRadiusLineScale * sc0kmph) * Math.Cos((tm_ang - 90) * Math.PI / 180);
                        first = false;
                    };
                    Pen p = new Pen(new SolidBrush(Color.FromArgb(220, 0, 0, 0)), 1);
                    if (curr.hspeed > 110) p = new Pen(new SolidBrush(Color.Maroon), 1);
                    g.DrawLine(p, new Point((int)prev_x, (int)(prev_y)), new Point((int)x1, (int)(y1)));
                    prev_x = x1;
                    prev_y = y1;
                    //
                };
            };
        }

        /// <summary>
        ///     Width of graph
        /// </summary>
        public int Width
        {
            set
            {
                this.width = value;
                Init();
            }
            get
            {
                return width;
            }
        }

        /// <summary>
        ///     Height of graph
        /// </summary>
        public int Height
        {
            set
            {
                this.height = value;
                Init();
            }
            get
            {
                return height;
            }
        }

        /// <summary>
        ///     Get Graphic
        /// </summary>
        public Bitmap Graph
        {
            get
            {
                DrawTemplate();
                DrawTexts();
                DrawData();
                Bitmap ret = (Bitmap)bmp.Clone();
                //ret.SetResolution(150, 150);
                return ret;
            }
        }

        ~Tachograph()
        {
            g.Dispose();
            bmp.Dispose();
        }
    }


    public class GPXReader
    {
        public XmlDocument xd = new XmlDocument();
        public XmlNode trk_Node;
        public string TrackName = "";
        public string TrackDesctiprion = "";
        public string TrackType = "";
        public List<Tachograph.trkseg> TrackSegments = new List<Tachograph.trkseg>();

        public DateTime MinTime = new DateTime(8000, 1, 1);
        public DateTime MaxTime = new DateTime(1900, 1, 1);

        public GPXReader(string fileName)
        {
            System.Globalization.NumberFormatInfo nfi = (System.Globalization.NumberFormatInfo)
            System.Globalization.CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ".";

            xd.Load(fileName);
            xd = StripNamespace(xd);
            trk_Node = xd.SelectSingleNode("/gpx/trk");
            if (trk_Node == null) return;
            XmlNode curr = null;
            curr = trk_Node.SelectSingleNode("name");
            if ((curr != null) && curr.HasChildNodes) TrackName = curr.ChildNodes[0].Value;
            curr = trk_Node.SelectSingleNode("desc");
            if ((curr != null) && curr.HasChildNodes) TrackDesctiprion = curr.ChildNodes[0].Value;
            curr = trk_Node.SelectSingleNode("type");
            if ((curr != null) && curr.HasChildNodes) TrackType = curr.ChildNodes[0].Value;
            XmlNodeList trkseg = xd.SelectNodes("/gpx/trk/trkseg");
            if (trkseg == null) return;
            foreach (XmlNode xn in trkseg)
            {
                Tachograph.trkpt last = null;
                int t = TrackSegments.Count;
                XmlNodeList points = xn.SelectNodes("trkpt");
                TrackSegments.Add(new Tachograph.trkseg());
                TrackSegments[t].points = new Tachograph.trkpt[points.Count];
                for (int i = 0; i < points.Count; i++)
                {
                    Tachograph.trkseg cs = TrackSegments[t];
                    Tachograph.trkpt cp = null;
                    string lat = points[i].SelectSingleNode("@lat") != null ? points[i].SelectSingleNode("@lat").Value : "0";
                    string lon = points[i].SelectSingleNode("@lon") != null ? points[i].SelectSingleNode("@lon").Value : "0";
                    string ele = points[i].SelectSingleNode("ele") != null ? points[i].SelectSingleNode("ele").ChildNodes[0].Value : "0";
                    string time = points[i].SelectSingleNode("time") != null ? points[i].SelectSingleNode("time").ChildNodes[0].Value : DateTime.Now.Date.ToString();
                    cs.points[i] = cp = new Tachograph.trkpt(double.Parse(lat, nfi), double.Parse(lon, nfi), double.Parse(ele, nfi), DateTime.Parse(time));
                    if (i == 0)
                    {
                        cs.MinTime = cp.time;
                        if (this.MinTime > cs.MinTime) this.MinTime = cs.MinTime;
                    };
                    if (i == (points.Count - 1))
                    {
                        cs.MaxTime = cp.time;
                        if (this.MaxTime < cs.MaxTime) this.MaxTime = cs.MaxTime;
                    };
                    {
                        if (cs.MaxEle < cp.ele) cs.MaxEle = cp.ele;
                        if (cs.MinEle > cp.ele) cs.MinEle = cp.ele;
                        if (cs.MaxLat < cp.lat) cs.MaxLat = cp.lat;
                        if (cs.MinLat > cp.lat) cs.MinLat = cp.lat;
                        if (cs.MaxLon < cp.lon) cs.MaxLon = cp.lon;
                        if (cs.MinLon > cp.lon) cs.MinLon = cp.lon;
                    };
                    if (last != null)
                    {
                        cp.seg_time = cp.time.Subtract(last.time);
                        cp.timeLapse = cp.time.Subtract(cs.points[0].time);
                        cp.seg_v_length = cp.ele - last.ele;
                        cp.seg_h_length = Utils.GetLengthMeters(last.lat, last.lon, cp.lat, cp.lon, false) / 1000;
                        cp.distance = last.distance + cp.seg_h_length;
                        cp.vspeed = cp.seg_time.TotalSeconds == 0 ? 0 : cp.seg_v_length / cp.seg_time.TotalSeconds;
                        cp.hspeed = cp.seg_time.TotalHours == 0 ? 0 : cp.seg_h_length / cp.seg_time.TotalHours;

                        if (cs.MaxSpeed < cp.hspeed) cs.MaxSpeed = cp.hspeed;
                        if (cs.MinSpeed > cp.hspeed) cs.MinSpeed = cp.hspeed;
                        cs.TotalLength += cp.seg_h_length;
                        cs.TotalTime += cp.seg_time;
                    };
                    last = cp;
                };
                TrackSegments[0].AvgSpeed = TrackSegments[0].TotalLength / TrackSegments[0].TotalTime.TotalHours;
            };
        }

        private XmlDocument StripNamespace(XmlDocument doc)
        {
            if (doc.DocumentElement.NamespaceURI.Length > 0)
            {
                doc.DocumentElement.SetAttribute("xmlns", "");
                XmlDocument newDoc = new XmlDocument();
                newDoc.LoadXml(doc.OuterXml);
                return newDoc;
            }
            else
            {
                return doc;
            }
        }

        private static void Write2WebSpeedMap(StreamWriter sw, GPXReader gpx)
        {
            sw.Write("{name:\"" + gpx.TrackName.Replace("\"", "`") + "\", segments: ");
            sw.Write("[");
            bool fseg = true;
            Bitmap bmp = global::KMZRebuilder.Properties.Resources._00COLORS;
            double spf = (double)(bmp.Width - 1) / 130.0;
            foreach (Tachograph.trkseg ts in gpx.TrackSegments)
            {
                if (!fseg) sw.Write(",");
                fseg = false;
                sw.Write("[");
                int writed = 0;

                int fPoint = 1;
                for (int cPoint = 2; cPoint < ts.points.Length; cPoint++) // ts.points.Length
                {
                    Tachograph.trkpt fp = ts.points[fPoint];
                    Tachograph.trkpt pp = ts.points[cPoint - 1];
                    Tachograph.trkpt cp = ts.points[cPoint];
                    double fps = (fp.hspeed > 130 ? 130 : (int)fp.hspeed);
                    double cps = (cp.hspeed > 130 ? 130 : (int)cp.hspeed);
                    if (Math.Abs(cps - fps) < 3) continue;

                    double stt = fp.time.Subtract(fp.time.Date).TotalMinutes;
                    double ent = pp.time.Subtract(pp.time.Date).TotalMinutes;

                    if (writed > 0) sw.Write(",");
                    sw.Write("{");
                    sw.Write("speed:" + fp.hspeed.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + ",");
                    sw.Write("slen:" + fp.distance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ",");
                    sw.Write("elen:" + pp.distance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ",");
                    sw.Write("sdt:\"" + fp.time.ToString("HH:mm:ss dd.MM.yy") + "\",");
                    sw.Write("edt:\"" + pp.time.ToString("HH:mm:ss dd.MM.yy") + "\",");
                    sw.Write("stime:" + stt.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ",");
                    sw.Write("etime:" + ent.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ",");
                    Color c = bmp.GetPixel((int)(spf * fps), 0);
                    sw.Write("color:\"#" + String.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B) + "\",");
                    sw.Write("ll:");
                    string ll = "";
                    for (int i = fPoint - 1; i < cPoint; i++)
                        ll += ",[" + ts.points[i].lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + ts.points[i].lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";
                    sw.Write("[" + ll.Substring(1) + "]");
                    sw.Write("}");
                    writed++;

                    fPoint = cPoint;
                };
                // last
                {
                    Tachograph.trkpt fp = ts.points[fPoint];
                    Tachograph.trkpt pp = ts.points[ts.points.Length - 1];
                    double fps = (fp.hspeed > 130 ? 130 : (int)fp.hspeed);

                    double stt = fp.time.Subtract(fp.time.Date).TotalMinutes;
                    double ent = pp.time.Subtract(pp.time.Date).TotalMinutes;

                    if (writed > 0) sw.Write(",");
                    sw.Write("{");
                    sw.Write("speed:" + fp.hspeed.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + ",");
                    sw.Write("slen:" + fp.distance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ",");
                    sw.Write("elen:" + pp.distance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ",");
                    sw.Write("sdt:\"" + fp.time.ToString("HH:mm:ss dd.MM.yy") + "\",");
                    sw.Write("edt:\"" + pp.time.ToString("HH:mm:ss dd.MM.yy") + "\",");
                    sw.Write("stime:" + stt.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ",");
                    sw.Write("etime:" + ent.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ",");
                    Color c = bmp.GetPixel((int)(spf * fps), 0);
                    sw.Write("color:\"#" + String.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B) + "\",");
                    sw.Write("ll:");
                    string ll = "";
                    for (int i = fPoint - 1; i < ts.points.Length; i++)
                        ll += ",[" + ts.points[i].lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + ts.points[i].lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";
                    sw.Write("[" + ll.Substring(1) + "]");
                    sw.Write("}");
                    writed++;
                };
                sw.Write("]");
            };
            sw.Write("]};");
        }

        private static void Write2KmlSpeedMap(StreamWriter sw, GPXReader gpx)
        {
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml><Document>");
            sw.WriteLine("<name>" + gpx.TrackName.Replace("\"", "`") + "</name><createdby>KMZ Rebuilder</createdby>");

            Bitmap bmp = global::KMZRebuilder.Properties.Resources._00COLORS;
            double spf = (double)(bmp.Width - 1) / 130.0;
            int segNo = 0;
            foreach (Tachograph.trkseg ts in gpx.TrackSegments)
            {
                segNo++;
                sw.WriteLine("<Folder><name>Track Segment "+segNo.ToString()+"</name>");

                int writed = 0;

                int fPoint = 1;
                for (int cPoint = 2; cPoint < ts.points.Length; cPoint++) // ts.points.Length
                {
                    Tachograph.trkpt fp = ts.points[fPoint];
                    Tachograph.trkpt pp = ts.points[cPoint - 1];
                    Tachograph.trkpt cp = ts.points[cPoint];
                    double fps = (fp.hspeed > 130 ? 130 : (int)fp.hspeed);
                    double cps = (cp.hspeed > 130 ? 130 : (int)cp.hspeed);
                    if (Math.Abs(cps - fps) < 3) continue;

                    double stt = fp.time.Subtract(fp.time.Date).TotalMinutes;
                    double ent = pp.time.Subtract(pp.time.Date).TotalMinutes;

                    string desc = "";
                    desc += "Speed:" + fp.hspeed.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " km/h, ";
                    desc += (pp.distance - fp.distance).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km, ";
                    desc += (ent - stt).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " min \r\n";
                    desc += (fp.time.ToString("HH:mm:ss dd.MM.yyyy") + " (" + fp.distance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km) \r\n");
                    desc += (pp.time.ToString("HH:mm:ss dd.MM.yyyy") + " (" + pp.distance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km) \r\n");

                    string nm = (fp.distance).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " (+" + (pp.distance - fp.distance).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ") km, ";
                    nm += fp.time.ToString("HH:mm:ss") + " (+" + (ent - stt).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " min), ";
                    nm += fp.hspeed.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " km/h";
                    sw.Write("<Placemark><name>"+nm+"</name><styleUrl>#speed" + ((int)fps).ToString() + "</styleUrl><description>" + desc + "</description>");
                    sw.Write("<LineString><extrude>1</extrude><coordinates>");
                    for (int i = fPoint - 1; i < cPoint; i++)
                        sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", ts.points[i].lon, ts.points[i].lat));
                    sw.WriteLine("</coordinates></LineString></Placemark>");
                                                           
                    sw.WriteLine();
                    writed++;

                    fPoint = cPoint;
                };
                // last
                {
                    Tachograph.trkpt fp = ts.points[fPoint];
                    Tachograph.trkpt pp = ts.points[ts.points.Length - 1];
                    double fps = (fp.hspeed > 130 ? 130 : (int)fp.hspeed);

                    double stt = fp.time.Subtract(fp.time.Date).TotalMinutes;
                    double ent = pp.time.Subtract(pp.time.Date).TotalMinutes;

                    string desc = "";
                    desc += "Speed:" + fp.hspeed.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " km/h, ";
                    desc += (pp.distance - fp.distance).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km, ";
                    desc += (ent - stt).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " min \r\n";
                    desc += (fp.time.ToString("HH:mm:ss dd.MM.yyyy") + " (" + fp.distance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km) \r\n");
                    desc += (pp.time.ToString("HH:mm:ss dd.MM.yyyy") + " (" + pp.distance.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " km) \r\n");

                    string nm = (fp.distance).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " (+" + (pp.distance - fp.distance).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ") km, ";
                    nm += fp.time.ToString("HH:mm:ss") + " (+" + (ent - stt).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " min), ";
                    nm += fp.hspeed.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " km/h";
                    sw.Write("<Placemark><name>"+nm+"</name><styleUrl>#speed" + ((int)fps).ToString() + "</styleUrl><description>" + desc + "</description>");
                    sw.Write("<LineString><extrude>1</extrude><coordinates>");
                    for (int i = fPoint - 1; i < ts.points.Length; i++)
                        sw.Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", ts.points[i].lon, ts.points[i].lat));
                    sw.WriteLine("</coordinates></LineString></Placemark>");

                    sw.WriteLine();

                    writed++;
                };

                sw.WriteLine("</Folder>");
            };            

            for (int i = 0; i <= 130; i++)
            {
                Color c = bmp.GetPixel((int)(spf * i), 0);
                sw.WriteLine("<Style id=\"speed" + i.ToString() + "\"><LineStyle><color>ff" + String.Format("{0:X2}{1:X2}{2:X2}", c.B, c.G, c.R) + "</color><width>5</width></LineStyle></Style>");
            };
            sw.WriteLine("</Document></kml>");
        }

        public static void GPX2WebSpeedMap(string gpxFile, string htmlFile)
        {
            GPXReader gpx;
            try
            {
                gpx = new GPXReader(gpxFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Couldn't load GPX file `{0}`!\r\nError: {1}", System.IO.Path.GetFileName(gpxFile), ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            if (gpx.TrackSegments.Count == 0)
            {
                MessageBox.Show("No track found in GPX File", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            };

            string xtml = GPXReader.GetCurrentDir() + @"\viewspeedmap.tml";
            FileStream rfs = new FileStream(xtml, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(rfs, System.Text.Encoding.UTF8);
            string fileData = sr.ReadToEnd();
            sr.Close();
            rfs.Close();
            int fdPos = fileData.IndexOf("var track = {};");

            FileStream fs = new FileStream(htmlFile, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.Write(fileData.Substring(0, fdPos + 12));
            Write2WebSpeedMap(sw, gpx);
            sw.Write(fileData.Substring(fdPos + 15));
            sw.Close();
            fs.Close();
        }

        public static void GPX2ColorKML(string gpxFile, string kmlFile)
        {
            GPXReader gpx;
            try
            {
                gpx = new GPXReader(gpxFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Couldn't load GPX file `{0}`!\r\nError: {1}", System.IO.Path.GetFileName(gpxFile), ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            if (gpx.TrackSegments.Count == 0)
            {
                MessageBox.Show("No track found in GPX File", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            };

            FileStream fs = new FileStream(kmlFile, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            Write2KmlSpeedMap(sw, gpx);
            sw.Close();
            fs.Close();
        }


        public static string GetCurrentDir()
        {
            string fname = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            fname = fname.Replace("file:///", "");
            fname = fname.Replace("/", @"\");
            fname = fname.Substring(0, fname.LastIndexOf(@"\") + 1);
            return fname;
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
}
