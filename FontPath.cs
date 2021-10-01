using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Text;

namespace KMZRebuilder
{
    public static class FontPath
    {
        public static PointD[][] SymbolToPath(FontFamily font, char symbol, float size, PointF startPoint, PathOffset offset, bool flipvertical, bool fliphorizontal, float angle)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new GraphicsPath();
            if(font.IsStyleAvailable(FontStyle.Regular))
                path.AddString(new string(new char[] { symbol }), font, (int)FontStyle.Regular, size, new PointF(0, 0), StringFormat.GenericDefault);
            else if (font.IsStyleAvailable(FontStyle.Bold))
                path.AddString(new string(new char[] { symbol }), font, (int)FontStyle.Bold, size, new PointF(0, 0), StringFormat.GenericDefault);
            else
                path.AddString(new string(new char[] { symbol }), font, (int)FontStyle.Italic, size, new PointF(0, 0), StringFormat.GenericDefault);
            if (flipvertical)// if flip
            {
                Matrix m = new Matrix(1f, 0f, 0f, -1f, 0f, 0f); // flip Y
                //m = new Matrix(-1, 0, 0, 1, 0, 0); // flip X                    
                path.Transform(m);
                ////////////////////////
                m = new Matrix();
                m.Translate(0, size);
                path.Transform(m);
            };
            if (fliphorizontal)// if flip
            {
                Matrix m = new Matrix(1f, 0f, 0f, -1f, 0f, 0f); // flip Y
                m = new Matrix(-1, 0, 0, 1, 0, 0); // flip X                    
                path.Transform(m);
                ////////////////////////
                m = new Matrix();
                m.Translate(size, 0);
                path.Transform(m);
            };
            if (angle != 0)
            {
                Matrix m = new Matrix();
                m.RotateAt(angle, new PointF(startPoint.X + size / 2.0f, startPoint.Y + size / 2.0f));
                path.Transform(m);
            };
            if (true) // move
            {
                Matrix m = new Matrix();
                RectangleF rect = GetBounds(path.PathPoints);
                switch (offset)
                {
                    case PathOffset.Center: m.Translate(startPoint.X - rect.Left - rect.Width / 2, startPoint.Y - rect.Top - rect.Height / 2); break;
                    case PathOffset.TopMiddle: m.Translate(startPoint.X - rect.Left - rect.Width / 2, startPoint.Y - rect.Top); break;
                    case PathOffset.TopRight: m.Translate(startPoint.X - rect.Left - rect.Width, startPoint.Y - rect.Top); break;
                    case PathOffset.RightMiddle: m.Translate(startPoint.X - rect.Left - rect.Width, startPoint.Y - rect.Top - rect.Height / 2); break;
                    case PathOffset.BottomRight: m.Translate(startPoint.X - rect.Left - rect.Width, startPoint.Y - rect.Top - rect.Height); break;
                    case PathOffset.BottomMiddle: m.Translate(startPoint.X - rect.Left - rect.Width / 2, startPoint.Y - rect.Top - rect.Height); break;
                    case PathOffset.BottomLeft: m.Translate(startPoint.X - rect.Left, startPoint.Y - rect.Top - rect.Height); break;
                    case PathOffset.LeftMiddle: m.Translate(startPoint.X - rect.Left, startPoint.Y - rect.Top - rect.Height / 2); break;
                    case PathOffset.TopLeft: m.Translate(startPoint.X - rect.Left, startPoint.Y - rect.Top); break;
                    case PathOffset.None: m.Translate(startPoint.X, startPoint.Y); break;
                };
                path.Transform(m);
            };
            List<PointD[]> pathXX = new List<PointD[]>();
            List<PointD> pathX = new List<PointD>();
            for (int i = 0; i < path.PathPoints.Length; i++)
            {
                if ((path.PathTypes[i] == 0) && (pathX.Count > 0))
                {
                    pathXX.Add(pathX.ToArray());
                    pathX.Clear();
                };
                pathX.Add(new PointD(path.PathPoints[i], path.PathTypes[i]));
            };
            if (pathX.Count > 0) pathXX.Add(pathX.ToArray());
            return pathXX.ToArray();
        }

        public static PointD[][] StringToPath(FontFamily font, string text, float size, PointF startPoint, PathOffset offset, bool flipvertical, bool fliphorizontal, float angle)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new GraphicsPath();
            if(font.IsStyleAvailable(FontStyle.Regular))
                path.AddString(text, font, (int)FontStyle.Regular, size, new PointF(0, 0), StringFormat.GenericDefault);
            else if (font.IsStyleAvailable(FontStyle.Bold))
                path.AddString(text, font, (int)FontStyle.Bold, size, new PointF(0, 0), StringFormat.GenericDefault);
            else
                path.AddString(text, font, (int)FontStyle.Italic, size, new PointF(0, 0), StringFormat.GenericDefault);
            if (flipvertical)// if flip
            {
                Matrix m = new Matrix(1f, 0f, 0f, -1f, 0f, 0f); // flip Y
                //m = new Matrix(-1, 0, 0, 1, 0, 0); // flip X                    
                path.Transform(m);
                ////////////////////////
                m = new Matrix();
                m.Translate(0, size);
                path.Transform(m);
            };
            if (fliphorizontal)// if flip
            {
                Matrix m = new Matrix(1f, 0f, 0f, -1f, 0f, 0f); // flip Y
                m = new Matrix(-1, 0, 0, 1, 0, 0); // flip X                    
                path.Transform(m);
                ////////////////////////
                m = new Matrix();
                m.Translate(size, 0);
                path.Transform(m);
            };
            if (angle != 0)
            {
                Matrix m = new Matrix();
                m.RotateAt(angle, new PointF(startPoint.X + size / 2.0f, startPoint.Y + size / 2.0f));
                path.Transform(m);
            };
            if (true) // move
            {
                Matrix m = new Matrix();
                RectangleF rect = GetBounds(path.PathPoints);
                switch (offset)
                {
                    case PathOffset.Center: m.Translate(startPoint.X - rect.Left - rect.Width / 2, startPoint.Y - rect.Top - rect.Height / 2); break;
                    case PathOffset.TopMiddle: m.Translate(startPoint.X - rect.Left - rect.Width / 2, startPoint.Y - rect.Top); break;
                    case PathOffset.TopRight: m.Translate(startPoint.X - rect.Left - rect.Width, startPoint.Y - rect.Top); break;
                    case PathOffset.RightMiddle: m.Translate(startPoint.X - rect.Left - rect.Width, startPoint.Y - rect.Top - rect.Height / 2); break;
                    case PathOffset.BottomRight: m.Translate(startPoint.X - rect.Left - rect.Width, startPoint.Y - rect.Top - rect.Height); break;
                    case PathOffset.BottomMiddle: m.Translate(startPoint.X - rect.Left - rect.Width / 2, startPoint.Y - rect.Top - rect.Height); break;
                    case PathOffset.BottomLeft: m.Translate(startPoint.X - rect.Left, startPoint.Y - rect.Top - rect.Height); break;
                    case PathOffset.LeftMiddle: m.Translate(startPoint.X - rect.Left, startPoint.Y - rect.Top - rect.Height / 2); break;
                    case PathOffset.TopLeft: m.Translate(startPoint.X - rect.Left, startPoint.Y - rect.Top); break;
                    case PathOffset.None: m.Translate(startPoint.X, startPoint.Y); break;
                };
                path.Transform(m);
            };
            List<PointD[]> pathXX = new List<PointD[]>();
            List<PointD> pathX = new List<PointD>();
            for (int i = 0; i < path.PathPoints.Length; i++)
            {
                if ((path.PathTypes[i] == 0) && (pathX.Count > 0))
                {
                    pathXX.Add(pathX.ToArray());
                    pathX.Clear();
                };
                pathX.Add(new PointD(path.PathPoints[i], path.PathTypes[i]));
            };
            if (pathX.Count > 0) pathXX.Add(pathX.ToArray());
            return pathXX.ToArray();
        }

        public static PointD[][] StringToPath(FontFamily font, string text, float size, float spacing, PointF startPoint, PathOffset offset, bool flipvertical, bool fliphorizontal, float angle)
        {
            bool flipvert = flipvertical;
            if (fliphorizontal)
                flipvert = !flipvert;
            PointF rotatePoint = new PointF(startPoint.X + size / 2, startPoint.Y + size / 2);

            text = text.Replace("\r", " ").Replace("\n", " ").Replace("\t", "  ");
            char[] chars = text.ToCharArray();
            RectangleF bounds = new RectangleF(0, 0, 0, 0);
            System.Drawing.Drawing2D.GraphicsPath PPS = new GraphicsPath();
            PointF start = new PointF(0, 0);
            foreach (char c in chars)
            {
                if (bounds.Width > 0)
                    start = new PointF(start.X + bounds.Width + spacing, start.Y);
                if (c == ' ')
                    continue;
                if ((c == '.') || (c == ',') || (c == ':') || (c == ';') || (c == '!') || (c == '?'))
                {
                    start = new PointF(start.X - spacing * 0.7f, start.Y);
                };
                PointD[][] path = SymbolToPath(font, c, size, start, FontPath.PathOffset.None, flipvert, false, 0);
                bounds = GetBounds(path);
                foreach (PointD[] pol in path)
                    PPS.AddPolygon(PointD.ToPointF(pol));
            };
            if (fliphorizontal)
            {
                Matrix m = new Matrix();
                m.RotateAt(180, rotatePoint);
                PPS.Transform(m);
                m = new Matrix();
                RectangleF rect = GetBounds(PPS.PathPoints);
                m.Translate(rect.Width - size / 2, 0);
                PPS.Transform(m);
            };
            if (angle != 0)
            {
                Matrix m = new Matrix();
                m.RotateAt(angle, rotatePoint);
                PPS.Transform(m);
            };
            if (true)// move
            {
                Matrix m = new Matrix();
                RectangleF rect = GetBounds(PPS.PathPoints);
                switch (offset)
                {
                    case PathOffset.Center: m.Translate(startPoint.X - rect.Left - rect.Width / 2, startPoint.Y - rect.Top - rect.Height / 2); break;
                    case PathOffset.TopMiddle: m.Translate(startPoint.X - rect.Left - rect.Width / 2, startPoint.Y - rect.Top); break;
                    case PathOffset.TopRight: m.Translate(startPoint.X - rect.Left - rect.Width, startPoint.Y - rect.Top); break;
                    case PathOffset.RightMiddle: m.Translate(startPoint.X - rect.Left - rect.Width, startPoint.Y - rect.Top - rect.Height / 2); break;
                    case PathOffset.BottomRight: m.Translate(startPoint.X - rect.Left - rect.Width, startPoint.Y - rect.Top - rect.Height); break;
                    case PathOffset.BottomMiddle: m.Translate(startPoint.X - rect.Left - rect.Width / 2, startPoint.Y - rect.Top - rect.Height); break;
                    case PathOffset.BottomLeft: m.Translate(startPoint.X - rect.Left, startPoint.Y - rect.Top - rect.Height); break;
                    case PathOffset.LeftMiddle: m.Translate(startPoint.X - rect.Left, startPoint.Y - rect.Top - rect.Height / 2); break;
                    case PathOffset.TopLeft: m.Translate(startPoint.X - rect.Left, startPoint.Y - rect.Top); break;
                };
                PPS.Transform(m);
            };
            List<PointD[]> pathXX = new List<PointD[]>();
            List<PointD> pathX = new List<PointD>();
            for (int i = 0; i < PPS.PathPoints.Length; i++)
            {
                if ((PPS.PathTypes[i] == 0) && (pathX.Count > 0))
                {
                    pathXX.Add(pathX.ToArray());
                    pathX.Clear();
                };
                pathX.Add(new PointD(PPS.PathPoints[i], PPS.PathTypes[i]));
            };
            if (pathX.Count > 0) pathXX.Add(pathX.ToArray());
            return pathXX.ToArray();
        }

        public static RectangleF GetBounds(PointF[][] poitns)
        {
            float xmin = float.MaxValue;
            float xmax = float.MinValue;
            float ymin = float.MaxValue;
            float ymax = float.MinValue;
            foreach (PointF[] pp in poitns)
                foreach (PointF p in pp)
                {
                    if (p.X < xmin) xmin = p.X;
                    if (p.X > xmax) xmax = p.X;
                    if (p.Y < ymin) ymin = p.Y;
                    if (p.Y > ymax) ymax = p.Y;
                };
            return new RectangleF(xmin, ymin, Math.Abs(xmax - xmin), Math.Abs(ymax - ymin));
        }

        public static RectangleF GetBounds(PointD[][] poitns)
        {
            float xmin = float.MaxValue;
            float xmax = float.MinValue;
            float ymin = float.MaxValue;
            float ymax = float.MinValue;
            foreach (PointD[] pp in poitns)
                foreach (PointD p in pp)
                {
                    if (p.X < xmin) xmin = (float)p.X;
                    if (p.X > xmax) xmax = (float)p.X;
                    if (p.Y < ymin) ymin = (float)p.Y;
                    if (p.Y > ymax) ymax = (float)p.Y;
                };
            return new RectangleF(xmin, ymin, Math.Abs(xmax - xmin), Math.Abs(ymax - ymin));
        }

        public static RectangleF GetBounds(PointF[] poitns)
        {
            float xmin = float.MaxValue;
            float xmax = float.MinValue;
            float ymin = float.MaxValue;
            float ymax = float.MinValue;
            foreach (PointF p in poitns)
            {
                if (p.X < xmin) xmin = p.X;
                if (p.X > xmax) xmax = p.X;
                if (p.Y < ymin) ymin = p.Y;
                if (p.Y > ymax) ymax = p.Y;
            };
            return new RectangleF(xmin, ymin, Math.Abs(xmax - xmin), Math.Abs(ymax - ymin));
        }

        public static RectangleF GetBounds(PointD[] poitns)
        {
            float xmin = float.MaxValue;
            float xmax = float.MinValue;
            float ymin = float.MaxValue;
            float ymax = float.MinValue;
            foreach (PointD p in poitns)
            {
                if (p.X < xmin) xmin = (float)p.X;
                if (p.X > xmax) xmax = (float)p.X;
                if (p.Y < ymin) ymin = (float)p.Y;
                if (p.Y > ymax) ymax = (float)p.Y;
            };
            return new RectangleF(xmin, ymin, Math.Abs(xmax - xmin), Math.Abs(ymax - ymin));
        }

        public static PointF[][] FlipVetrical(PointF[][] points)
        {
            RectangleF rect = GetBounds(points);
            for (int f = 0; f < points.Length; f++)
                for (int s = 0; s < points[f].Length; s++)
                    points[f][s].Y = rect.Top - points[f][s].Y + rect.Bottom;
            return points;
        }

        public static PointD[][] FlipVetrical(PointD[][] points)
        {
            RectangleF rect = GetBounds(points);
            for (int f = 0; f < points.Length; f++)
                for (int s = 0; s < points[f].Length; s++)
                    points[f][s].Y = rect.Top - points[f][s].Y + rect.Bottom;
            return points;
        }

        public static PointF[] FlipVetrical(PointF[] points)
        {
            RectangleF rect = GetBounds(points);
            for (int f = 0; f < points.Length; f++)
                points[f].Y = rect.Top - points[f].Y + rect.Bottom;
            return points;
        }

        public static PointD[] FlipVetrical(PointD[] points)
        {
            RectangleF rect = GetBounds(points);
            for (int f = 0; f < points.Length; f++)
                points[f].Y = rect.Top - points[f].Y + rect.Bottom;
            return points;
        }

        public static PointF[][] FlipHorizontal(PointF[][] points)
        {
            RectangleF rect = GetBounds(points);
            for (int f = 0; f < points.Length; f++)
                for (int s = 0; s < points[f].Length; s++)
                    points[f][s].X = rect.Right - points[f][s].X + rect.Left;
            return points;
        }

        public static PointD[][] FlipHorizontal(PointD[][] points)
        {
            RectangleF rect = GetBounds(points);
            for (int f = 0; f < points.Length; f++)
                for (int s = 0; s < points[f].Length; s++)
                    points[f][s].X = rect.Right - points[f][s].X + rect.Left;
            return points;
        }

        public static PointF[] FlipHorizontal(PointF[] points)
        {
            RectangleF rect = GetBounds(points);
            for (int f = 0; f < points.Length; f++)
                points[f].X = rect.Right - points[f].X + rect.Left;
            return points;
        }

        public static PointD[] FlipHorizontal(PointD[] points)
        {
            RectangleF rect = GetBounds(points);
            for (int f = 0; f < points.Length; f++)
                points[f].X = rect.Right - points[f].X + rect.Left;
            return points;
        }

        public static string[] PathToLineString(PointF[][] points)
        {
            List<string> result = new List<string>();
            foreach (PointF[] path in points)
                result.Add(PathToLineString(path));
            return result.ToArray();
        }

        public static string[] PathToLineString(PointD[][] points)
        {
            List<string> result = new List<string>();
            foreach (PointD[] path in points)
                result.Add(PathToLineString(path));
            return result.ToArray();
        }

        public static string PathToLineString(PointF[] points)
        {
            string result = "";
            foreach (PointF p in points)
            {
                if (result.Length > 0) result += " ";
                result += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", p.X, p.Y, 0);
            };
            result += String.Format(System.Globalization.CultureInfo.InvariantCulture, " {0},{1},{2}", points[0].X, points[0].Y, 0);
            return result;
        }

        public static string PathToLineString(PointD[] points)
        {
            string result = "";
            foreach (PointD p in points)
            {
                if (result.Length > 0) result += " ";
                result += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", p.X, p.Y, 0);
            };
            result += String.Format(System.Globalization.CultureInfo.InvariantCulture, " {0},{1},{2}", points[0].X, points[0].Y, 0);
            return result;
        }

        public static string PathToPolygonString(PointF[] points)
        {
            string result = "";
            foreach (PointF p in points)
            {
                if (result.Length > 0) result += " ";
                result += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", p.X, p.Y, 0);
            };
            return result;
        }

        public static string PathToPolygonString(PointD[] points)
        {
            string result = "";
            foreach (PointD p in points)
            {
                if (result.Length > 0) result += " ";
                result += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},{2}", p.X, p.Y, 0);
            };
            return result;
        }

        public static string[] PathToPolygonString(PointF[][] points)
        {
            List<string> result = new List<string>();
            foreach (PointF[] path in points)
                result.Add(PathToPolygonString(path));
            return result.ToArray();
        }

        public static string[] PathToPolygonString(PointD[][] points)
        {
            List<string> result = new List<string>();
            foreach (PointD[] path in points)
                result.Add(PathToPolygonString(path));
            return result.ToArray();
        }

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

        public enum PathOffset : byte
        {
            Center = 0,
            TopMiddle = 1,
            TopRight = 2,
            RightMiddle = 3,
            BottomRight = 4,
            BottomMiddle = 5,
            BottomLeft = 6,
            LeftMiddle = 7,
            TopLeft = 8,
            None = 255
        }
    }

    public static class FontPathSample
    {
        public static Image Sample()
        {
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(@"Fonts\PTZ55F.ttf");

            PointF startPoint = new PointF(100f, 100f);
            List<FontPath.PointD[]> pathes = new List<FontPath.PointD[]>();
            pathes.AddRange((FontPath.StringToPath(pfc.Families[0], "¡‰˚˘¸!", 75f, startPoint, FontPath.PathOffset.TopLeft, false, false, 0f)));
            //pathes.AddRange((FontPath.StringToPath(pfc.Families[0], "AıÚÛÌ„!!!", 0.01f, 0.01f * 0.12f, startPoint, FontPath.PathOffset.BottomLeft, false, false, 0f)));
            //pathes.AddRange((FontPath.SymbolToPath(pfc.Families[0], 'G', 150, startPoint, FontPath.PathOffset.None, false, false, -0f)));
            string[] LineStrings = FontPath.PathToLineString(pathes.ToArray());

            Image im = new Bitmap(600, 600);
            using (Graphics g = Graphics.FromImage(im))
            {
                Pen pen = new Pen(Brushes.Black, 2);
                Pen pen2 = new Pen(Brushes.Red, 1);
                g.DrawLine(pen2, new PointF(0, startPoint.Y), new PointF(im.Width, startPoint.Y));
                g.DrawLine(pen2, new PointF(startPoint.X, 0), new PointF(startPoint.X, im.Height));
                foreach (FontPath.PointD[] path in pathes)
                {
                    PointF[] p = FontPath.PointD.ToPointF(path);

                    //g.FillPolygon(Brushes.Red, p);

                    g.DrawPolygon(pen, p);

                    //g.DrawLines(pen, p);
                    //g.DrawLine(pen, p[p.Length-1], p[0]);
                };
            };
            return im;
        }
    }
}
