//#define gpc
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

#if gpc
using GpcWrapper;
#endif


namespace PolyLineBuffer
{
    using ClipperLib;

    public class PolyLineBufferCreator
    {
        public class PolyResult
        {
            public PointF[] polygon;
            public List<PointF[]> segments;

            public PolyResult()
            {
                this.polygon = new PointF[0];
                this.segments = new List<PointF[]>();
            }

            public PolyResult(PointF[] polygon, List<PointF[]> segments)
            {
                this.polygon = polygon;
                this.segments = segments;
            }

            public bool PointIn(PointF point)
            {
                if (segments.Count == 0) return false;
                for (int i = 0; i < segments.Count; i++)
                    if (PointInPolygon(point, segments[i]))
                        return true;
                return false;
            }

            private static bool PointInPolygon(PointF point, PointF[] polygon)
            {
                if (polygon == null) return false;
                if (polygon.Length < 2) return false;

                int i, j, nvert = polygon.Length;
                bool c = false;

                for (i = 0, j = nvert - 1; i < nvert; j = i++)
                {
                    if (((polygon[i].Y >= point.Y) != (polygon[j].Y >= point.Y)) &&
                        (point.X <= (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                      )
                        c = !c;
                }

                return c;
            }
        }

        /// <summary>
        ///     Calc distance in custom units
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        /// <returns>distance in custom units</returns>
        public delegate float DistanceFunction(PointF a, PointF b);

        /// <summary>
        ///     return (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float SampleDistFunc(PointF a, PointF b)
        {
            return (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        /// <summary>
        ///     return distance in meters between 2 points
        /// </summary>
        /// <param name="a">Point A</param>
        /// <param name="b">Point B</param>
        /// <returns>distance in meters</returns>
        public static float GeographicDistFunc(PointF a, PointF b)
        {
            return GetGeoLengthInMetersC(a.Y, a.X, b.Y, b.X, false);
        }

        /// <summary>
        ///     Return total length of polyline in meters
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns>in meters</returns>
        public static uint GetDistInMeters(PointF[] polyline, bool polygon)
        {
            if (polyline == null) return 0;
            if (polyline.Length < 2) return 0;
            uint res = 0;
            for (int i = 1; i < polyline.Length; i++)
                res += GetGeoLengthInMetersC(polyline[i - 1].Y, polyline[i - 1].X, polyline[i].Y, polyline[i].X, false);
            if(polygon)
                res += GetGeoLengthInMetersC(polyline[polyline.Length - 1].Y, polyline[polyline.Length - 1].X, polyline[0].Y, polyline[0].X, false);
            return res;
        }

        private static double GetDeterminant(double x1, double y1, double x2, double y2)
        {
            return x1 * y2 - x2 * y1;
        }

        /// <summary>
        ///     Calculate Square of Geographic Polygon By Simplify Method
        ///     (faster)
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static double GetSquareInMetersA(PointF[] poly)
        {
            if (poly == null) return 0;
            if (poly.Length < 3) return 0;
            PointF st = new PointF(float.MaxValue, float.MaxValue);
            for (int i = 0; i < poly.Length; i++)
            {
                if (poly[i].X < st.X) st.X = poly[i].X;
                if (poly[i].Y < st.Y) st.Y = poly[i].Y;
            };
            PointF[] polygon = new PointF[poly.Length];
            for (int i = 0; i < polygon.Length; i++)
                polygon[i] = new PointF(GetGeoLengthInMetersC(st.Y, st.X, st.Y, poly[i].X, false), GetGeoLengthInMetersC(st.Y, st.X, poly[i].Y, st.X, false));

            double area = GetDeterminant(polygon[polygon.Length - 1].X, polygon[polygon.Length - 1].Y, polygon[0].X, polygon[0].Y);
            for (int i = 1; i < polygon.Length; i++)
                area += GetDeterminant(polygon[i - 1].X, polygon[i - 1].Y, polygon[i].X, polygon[i].Y);

            return Math.Abs(area / 2.0 / 1000000.0);
        }

        /// <summary>
        ///     Calculate Square of Geographic Polygon By Triangulation Method
        ///     (better but slower)
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static double GetSquareInMetersT(PointF[] polygon)
        {
            if (polygon == null) return 0;
            if (polygon.Length < 3) return 0;
            double square = 0;

            // To Trinagles //
            int nVertices = polygon.Length;
            GeometryUtility.CPoint2D[] vertices = new GeometryUtility.CPoint2D[nVertices];
            for (int i = 0; i < nVertices; i++)
                vertices[i] = new GeometryUtility.CPoint2D(polygon[i].X, polygon[i].Y);
            PolygonCuttingEar.CPolygonShape cutPolygon = new PolygonCuttingEar.CPolygonShape(vertices);
            cutPolygon.CutEar();
            for (int i = 0; i < cutPolygon.NumberOfPolygons; i++)
            {
                int nPoints = cutPolygon.Polygons(i).Length;
                PointF[] triangle = new PointF[nPoints];
                for (int j = 0; j < nPoints; j++)
                {
                    triangle[j].X = (float)cutPolygon.Polygons(i)[j].X;
                    triangle[j].Y = (float)cutPolygon.Polygons(i)[j].Y;
                };

                double a = GeographicDistFunc(triangle[0], triangle[1]);
                double b = GeographicDistFunc(triangle[1], triangle[2]);
                double c = GeographicDistFunc(triangle[2], triangle[0]);
                double p = (a + b + c) / 2.0;
                double s = Math.Sqrt(p * (p - a) * (p - b) * (p - c)); //  formula Gerona
                square += s;
            };

            return square / 1000000.0;
        }

        /// <summary>
        ///     Calculate Square of Geographic Polygon
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static double GetSquareInMeters(PointF[] poly)
        {
            return GetSquareInMetersA(poly);
        }

        /// <summary>
        ///     Geographic Get Distance Between 2 points
        /// </summary>
        /// <param name="StartLat">A Lat</param>
        /// <param name="StartLong">A Lon</param>
        /// <param name="EndLat">B Lat</param>
        /// <param name="EndLong">B Lon</param>
        /// <param name="radians">radians or degrees</param>
        /// <returns>length in meters</returns>
        public static uint GetGeoLengthInMetersC(double StartLat, double StartLong, double EndLat, double EndLong, bool radians)
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

        /// <summary>
        ///     Get Buffer Polygon for polyline
        /// </summary>
        /// <param name="line">polyline</param>
        /// <param name="distance">Buffer radius size in custom units (meters)</param>
        /// <param name="right">include right side</param>
        /// <param name="left">include left side</param>
        /// <param name="DistanceFunc">distance between point function</param>
        /// <returns>polygon</returns>
        public static PolyResult GetLineBufferPolygon(PointF[] line, float distance, bool right, bool left, DistanceFunction DistanceFunc)
        {
            return GetLineBufferPolygon(line, distance, right, left, DistanceFunc, 0);
        }

        /// <summary>
        ///     Get Buffer Polygon for polyline
        /// </summary>
        /// <param name="line">polyline</param>
        /// <param name="distance">Buffer radius size in custom units (meters)</param>
        /// <param name="right">include right side</param>
        /// <param name="left">include left side</param>
        /// <returns>polygon</returns>
        public static PolyResult GetLineBufferPolygon(PointF[] line, float distance, bool right, bool left)
        {
            return GetLineBufferPolygon(line, distance, right, left, GeographicDistFunc, 0);
        }

        /// <summary>
        ///     Get Buffer Polygon for polyline
        /// </summary>
        /// <param name="line">polyline</param>
        /// <param name="distance">Buffer radius size in custom units (meters)</param>
        /// <param name="right">include right side</param>
        /// <param name="left">include left side</param>
        /// <param name="method">calc method</param>
        /// <returns>polygon</returns>
        public static PolyResult GetLineBufferPolygon(PointF[] line, float distance, bool right, bool left, int method)
        {
            return GetLineBufferPolygon(line, distance, right, left, GeographicDistFunc, method);
        }

        /// <summary>
        ///     Get Buffer Polygon for polyline
        /// </summary>
        /// <param name="line">polyline</param>
        /// <param name="distance">Buffer radius size in custom units (meters)</param>
        /// <param name="right">include right side</param>
        /// <param name="left">include left side</param>
        /// <param name="DistanceFunc">distance between point function</param>
        /// <param name="method">calc method (0 - merge, 1 - Union NH, 2 - Union WH, 3 - BBox Union NH, 4 - BBox Union WH, 5 - BRound Bezier)</param>
        /// <returns>polygon</returns>
        public static PolyResult GetLineBufferPolygon(PointF[] line, float distance, bool right, bool left, DistanceFunction DistanceFunc, int method)
        {
            float step = 1;

            // Empty
            if (line == null) return new PolyResult();
            if (line.Length == 0) return new PolyResult();
            if ((!left) && (!right)) return new PolyResult();

            // Point
            if (line.Length == 1)
            {
                PointF s = line[0];
                float d = DistanceFunc == null ? SampleDistFunc(s, new PointF(s.X + step, s.Y)) : DistanceFunc(s, new PointF(s.X + step, s.Y));
                float r = distance / d * step;
                PointF[] res = GetEllipse(s, r, 0, 360);
                List<PointF[]> segments = new List<PointF[]>();
                segments.Add(res);
                return new PolyResult(res, segments);
            };

            // 0 - Merge Method
            if((line.Length == 2) || (method == 0) || ((line.Length > 2000) && (method > 0) && (method < 5)))
            {
                List<PointF[]> poliSegments = new List<PointF[]>();

                List<PointF> p = new List<PointF>();
                List<PointF> v = new List<PointF>();
                v.AddRange(line);
                #if gpc                
                p.AddRange(glbp(v.ToArray(), distance, poliSegments, DistanceFunc));
                #else
                if (right)
                {
                    p.AddRange(glbp(v.ToArray(), distance, poliSegments, DistanceFunc));
                    if (!left)
                    {
                        v.Reverse();
                        p.AddRange(v);
                    };
                };
                if (left)
                {
                    if (!right) p.AddRange(v);
                    v.Reverse();
                    if (right) p.RemoveAt(p.Count - 1);
                    p.AddRange(glbp(v.ToArray(), distance, poliSegments, DistanceFunc));
                    if (right) p.RemoveAt(p.Count - 1);
                };
                #endif
                return new PolyResult(p.ToArray(), poliSegments);
            };            

            // BRound Bezier Method
            if (method == 5)
            {                
                List<PointF> sr = new List<PointF>();
                PointF s = line[0];
                PointF e = line[1];
                float prevA = 0;
                // first
                {
                    float angle = (float)((Math.Atan((e.Y - s.Y) / (e.X - s.X)) * 180 / Math.PI));
                    float d = DistanceFunc == null ? SampleDistFunc(s, new PointF(s.X + step, s.Y)) : DistanceFunc(s, new PointF(s.X + step, s.Y));
                    float r = distance / d * step;
                    PointF[] el = GetEllipse(s, r, s.X > e.X ? angle - 90 : angle + 90, s.X > e.X ? angle + 90 : angle - 90);
                    Array.Reverse(el);
                    if (left && right)
                        sr.AddRange(el);
                    else if (left)
                    {
                        sr.Add(s);
                        for (int i = el.Length / 2; i < el.Length; i++) sr.Add(el[i]);
                    }
                    else if (right)
                    {
                        for (int i = 0; i <= el.Length / 2; i++) sr.Add(el[i]);
                        sr.Add(s);
                    };
                    prevA = angle;
                };

                for (int cpi = 2; cpi < line.Length; cpi++)
                {
                    PointF npL = new PointF(sr[sr.Count - 1].X + e.X - s.X, sr[sr.Count - 1].Y + e.Y - s.Y);
                    PointF npR = new PointF(sr[0].X + e.X - s.X, sr[0].Y + e.Y - s.Y);
                    if (left) sr.Add(npL);
                    if (right) sr.Insert(0, npR);

                    s = line[cpi - 1];
                    e = line[cpi];
                    float angle = (float)((Math.Atan((e.Y - s.Y) / (e.X - s.X)) * 180 / Math.PI));
                    float dA = angle - prevA;
                    float d = DistanceFunc == null ? SampleDistFunc(s, new PointF(s.X + step, s.Y)) : DistanceFunc(s, new PointF(s.X + step, s.Y));
                    float r = distance / d * step;
                    PointF[] el = GetEllipse(s, r, s.X > e.X ? angle - 90 : angle + 90, s.X > e.X ? angle + 90 : angle - 90);
                    Array.Reverse(el);
                    if (left && right)
                    {
                        sr.Insert(0, el[0]);
                        sr.Add(el[el.Length - 1]);
                    }
                    else if (left)
                    {
                        sr.Insert(0, s);
                        sr.Add(el[el.Length - 1]);
                    }
                    else if (right)
                    {
                        sr.Insert(0, el[0]);
                        sr.Add(s);
                    };
                    prevA = angle;
                };

                // last
                {
                    PointF npL = new PointF(sr[sr.Count - 1].X + e.X - s.X, sr[sr.Count - 1].Y + e.Y - s.Y);
                    PointF npR = new PointF(sr[0].X + e.X - s.X, sr[0].Y + e.Y - s.Y);
                    if (left) sr.Add(npL);
                    if (right) sr.Insert(0, npR);

                    if (left)
                    {
                        List<PointF> interLeft = new List<PointF>();
                        for (int dp = sr.Count - (line.Length - 2) * 2; dp < sr.Count; dp += 2)
                        {
                            PointF ix = LineIntersection(sr[dp - 2], sr[dp - 1], sr[dp], sr[dp + 1]);
                            if ((IsInsideLine(sr[dp - 2], sr[dp - 1], ix)))
                                interLeft.Add(ix);
                            else
                            {
                                float dixa = DistanceFunc == null ? SampleDistFunc(sr[dp - 1], ix) : DistanceFunc(sr[dp - 1], ix);
                                float dixb = DistanceFunc == null ? SampleDistFunc(ix, sr[dp]) : DistanceFunc(ix, sr[dp]);
                                if ((dixa > (1.2 * distance)) || (dixb > (1.2 * distance)))
                                {
                                    PointF ixA = new PointF(sr[dp - 1].X + (float)1.5 * distance * (ix.X - sr[dp - 1].X) / dixa, sr[dp - 1].Y + (float)1.5 * distance * (ix.Y - sr[dp - 1].Y) / dixa);
                                    PointF ixB = new PointF(sr[dp].X + (float)1.5 * distance * (ix.X - sr[dp].X) / dixb, sr[dp].Y + (float)1.5 * distance * (ix.Y - sr[dp].Y) / dixb);
                                    interLeft.AddRange(Bezier(new PointF[] { sr[dp - 1], ixA, ixB, sr[dp] }, 7));
                                }
                                else
                                    interLeft.AddRange(Bezier(new PointF[] { sr[dp - 1], ix, sr[dp] }, 7));
                            };
                        };
                        for (int dp = 0; dp < (line.Length - 1) * 2; dp++) sr.RemoveAt(sr.Count - 1);
                        sr.AddRange(interLeft);
                    };
                    if (right)
                    {
                        List<PointF> interRight = new List<PointF>();
                        for (int dp = 2; dp < (line.Length - 1) * 2; dp += 2)
                        {
                            PointF ix = LineIntersection(sr[dp - 2], sr[dp - 1], sr[dp], sr[dp + 1]);
                            if ((IsInsideLine(sr[dp - 2], sr[dp - 1], ix)))
                                interRight.Add(ix);
                            else
                            {
                                float dixa = DistanceFunc == null ? SampleDistFunc(sr[dp - 1], ix) : DistanceFunc(sr[dp - 1], ix);
                                float dixb = DistanceFunc == null ? SampleDistFunc(ix, sr[dp]) : DistanceFunc(ix, sr[dp]);
                                if ((dixa > (1.2 * distance)) || (dixb > (1.2 * distance)))
                                {
                                    PointF ixA = new PointF(sr[dp - 1].X + (float)1.5 * distance * (ix.X - sr[dp - 1].X) / dixa, sr[dp - 1].Y + (float)1.5 * distance * (ix.Y - sr[dp - 1].Y) / dixa);
                                    PointF ixB = new PointF(sr[dp].X + (float)1.5 * distance * (ix.X - sr[dp].X) / dixb, sr[dp].Y + (float)1.5 * distance * (ix.Y - sr[dp].Y) / dixb);
                                    interRight.AddRange(Bezier(new PointF[] { sr[dp - 1], ixA, ixB, sr[dp] }, 7));
                                }
                                else
                                    interRight.AddRange(Bezier(new PointF[] { sr[dp - 1], ix, sr[dp] }, 7));
                            };
                        };
                        for (int dp = 0; dp < (line.Length - 1) * 2; dp++) sr.RemoveAt(0);
                        interRight.Reverse();
                        for (int dp = 0; dp < interRight.Count; dp++)
                            sr.Insert(0, interRight[dp]);
                    };

                    float angle = (float)((Math.Atan((e.Y - s.Y) / (e.X - s.X)) * 180 / Math.PI));
                    float d = DistanceFunc == null ? SampleDistFunc(e, new PointF(e.X + step, e.Y)) : DistanceFunc(e, new PointF(e.X + step, e.Y));
                    float r = distance / d * step;
                    PointF[] el = GetEllipse(e, r, s.X < e.X ? angle - 90 : angle + 90, s.X < e.X ? angle + 90 : angle - 90);
                    Array.Reverse(el);
                    if (left && right)
                        sr.AddRange(el);
                    else if (left)
                    {
                        for (int i = 0; i <= el.Length / 2; i++) sr.Add(el[i]);
                        sr.Add(e);
                    }
                    else if (right)
                    {
                        sr.Add(e);
                        for (int i = el.Length / 2; i < el.Length; i++) sr.Add(el[i]);
                    };
                };

                return new PolyResult(sr.ToArray(), null);
            };

            // RUnion+BUnion NH/WH
            if (line.Length > 2) // || method == 1 || method == 2 || method == 3 || method == 4
            {                
                List<List<ClipperLib.IntPoint>> base_poly = new List<List<ClipperLib.IntPoint>>();
                bool overdot = true;
                for (int cpi = 1; cpi < line.Length; cpi++)
                {
                    List<PointF> p = new List<PointF>();
                    
                    List<PointF[]> poliSegments = new List<PointF[]>();
                    if((method == 1) || (method == 2)) // BRound // method 1,2
                    {
                        List<PointF> v = new List<PointF>();
                        v.Add(line[cpi - 1]);
                        v.Add(line[cpi]);
                        #if gpc                
                        p.AddRange(glbp(v.ToArray(), distance, poliSegments, DistanceFunc));
                        #else
                        if (right)
                        {
                            try
                            {
                                p.AddRange(glbp(v.ToArray(), distance, poliSegments, DistanceFunc));
                            }
                            catch { };
                            if (!left)
                            {
                                v.Reverse();
                                p.AddRange(v);
                            };
                        };
                        if (left)
                        {
                            if (!right) p.AddRange(v);
                            v.Reverse();
                            if (right && (p.Count > 0)) p.RemoveAt(p.Count - 1);
                            try
                            {
                                p.AddRange(glbp(v.ToArray(), distance, poliSegments, DistanceFunc));
                            }
                            catch { };
                            if (right && (p.Count > 0)) p.RemoveAt(p.Count - 1);
                        };
                        #endif
                    }
                    else // BUnion // method = 3 || method = 4
                    {
                        PointF s = line[cpi - 1];
                        PointF e = line[cpi];
                        float dix = DistanceFunc == null ? SampleDistFunc(s, e) : DistanceFunc(s, e);
                        float angle = (float)((Math.Atan((e.Y - s.Y) / (e.X - s.X)) * 180 / Math.PI));
                        float d = DistanceFunc == null ? SampleDistFunc(s, new PointF(s.X + step, s.Y)) : DistanceFunc(s, new PointF(s.X + step, s.Y));
                        float r = distance / d * step;
                        if (left && right)
                        {
                            PointF ap1 = GetAngledPoint(s, r, s.X > e.X ? angle - 90 : angle + 90);
                            PointF ap2 = GetAngledPoint(e, r, s.X > e.X ? angle - 90 : angle + 90);
                            PointF ad1 = new PointF(ap1.X - distance * (ap2.X - ap1.X) / dix, ap1.Y - distance * (ap2.Y - ap1.Y) / dix);
                            PointF ad2 = new PointF(ap2.X + distance * (ap2.X - ap1.X) / dix, ap2.Y + distance * (ap2.Y - ap1.Y) / dix);
                            p.Add(ad1);
                            p.Add(ad2);
                            ap1 = GetAngledPoint(s, r, s.X > e.X ? angle + 90 : angle - 90);
                            ap2 = GetAngledPoint(e, r, s.X > e.X ? angle + 90 : angle - 90);
                            ad1 = new PointF(ap1.X - distance * (ap2.X - ap1.X) / dix, ap1.Y - distance * (ap2.Y - ap1.Y) / dix);
                            ad2 = new PointF(ap2.X + distance * (ap2.X - ap1.X) / dix, ap2.Y + distance * (ap2.Y - ap1.Y) / dix);
                            p.Insert(0, ad1);
                            p.Insert(0, ad2);
                        }
                        else if (left)
                        {
                            if (overdot)
                                p.Add(new PointF(s.X - distance * (e.X - s.X) / dix, s.Y - distance * (e.Y - s.Y) / dix));
                            else
                                p.Add(s);
                            PointF ap1 = GetAngledPoint(s, r, s.X > e.X ? angle - 90 : angle + 90);
                            PointF ap2 = GetAngledPoint(e, r, s.X > e.X ? angle - 90 : angle + 90);
                            PointF ad1 = new PointF(ap1.X - distance * (ap2.X - ap1.X) / dix, ap1.Y - distance * (ap2.Y - ap1.Y) / dix);
                            PointF ad2 = new PointF(ap2.X + distance * (ap2.X - ap1.X) / dix, ap2.Y + distance * (ap2.Y - ap1.Y) / dix);
                            if (overdot)
                                p.Add(ad1);
                            else
                                p.Add(LineIntersection(line[cpi - 2], s, ap1, ap2));
                            overdot = true;
                            if (cpi < (line.Length - 1))
                            {
                                double na = AngleFrom3PointsInDegrees3(s, e, line[cpi + 1]);
                                overdot = ((na >= 0) && (na <= 180)) || (na <= -180);
                            };
                            if (overdot)
                                p.Add(ad2);
                            else
                                p.Add(LineIntersection(ap1, ap2, e, line[cpi + 1]));
                            if (overdot)
                                p.Add(new PointF(e.X + distance * (e.X - s.X) / dix, e.Y + distance * (e.Y - s.Y) / dix));
                            else
                                p.Add(e);
                        }
                        else if (right)
                        {
                            if (overdot)
                                p.Insert(0, new PointF(s.X - distance * (e.X - s.X) / dix, s.Y - distance * (e.Y - s.Y) / dix));
                            else
                                p.Insert(0, s);
                            PointF ap1 = GetAngledPoint(s, r, s.X > e.X ? angle + 90 : angle - 90);
                            PointF ap2 = GetAngledPoint(e, r, s.X > e.X ? angle + 90 : angle - 90);
                            PointF ad1 = new PointF(ap1.X - distance * (ap2.X - ap1.X) / dix, ap1.Y - distance * (ap2.Y - ap1.Y) / dix);
                            PointF ad2 = new PointF(ap2.X + distance * (ap2.X - ap1.X) / dix, ap2.Y + distance * (ap2.Y - ap1.Y) / dix);
                            if (overdot)
                                p.Insert(0, ap1);
                            else
                                p.Insert(0, LineIntersection(line[cpi - 2], s, ap1, ap2));
                            overdot = true;
                            if (cpi < (line.Length - 1))
                            {
                                double na = AngleFrom3PointsInDegrees3(s, e, line[cpi + 1]);
                                overdot = !(((na >= 0) && (na <= 180)) || (na <= -180));
                            };
                            if (overdot)
                                p.Insert(0, ad2);
                            else
                                p.Insert(0, LineIntersection(ap1, ap2, e, line[cpi + 1]));
                            if (overdot)
                                p.Insert(0, new PointF(e.X + distance * (e.X - s.X) / dix, e.Y + distance * (e.Y - s.Y) / dix));
                            else
                                p.Insert(0, e);
                        };
                    };                    

                    List<ClipperLib.IntPoint> cpoly = new List<ClipperLib.IntPoint>();
                    foreach (PointF ptf in p)
                        cpoly.Add(new ClipperLib.IntPoint(ptf.X * 10000000, ptf.Y * 10000000));
                    cpoly.Add(new ClipperLib.IntPoint(p[0].X * 10000000, p[0].Y * 10000000));
                    List<List<ClipperLib.IntPoint>> curr_poly = new List<List<ClipperLib.IntPoint>>();
                    curr_poly.Add(cpoly);

                    if (cpi == 1)
                        base_poly = curr_poly;
                    else
                    {
                        ClipperLib.Clipper clipper = new ClipperLib.Clipper(0);
                        clipper.AddPaths(base_poly, ClipperLib.PolyType.ptSubject, true);
                        clipper.AddPaths(curr_poly, ClipperLib.PolyType.ptClip, true);

                        ClipperLib.PolyTree ptree = new ClipperLib.PolyTree();
                        if (clipper.Execute(ClipperLib.ClipType.ctUnion, ptree, ClipperLib.PolyFillType.pftEvenOdd))
                        {
                            base_poly.Clear();
                            int tol = ptree.m_AllPolys.Count > 0 ? 1 : 0;
                            if ((method == 2) || ((method == 4))) tol = ptree.m_AllPolys.Count;
                            for (int ci = 0; ci < tol; ci++)
                                base_poly.Add(ptree.m_AllPolys[ci].Contour);                            
                        };
                    };
                };
                List<PointF> ppf = new List<PointF>();
                if(base_poly.Count > 0)
                    foreach (ClipperLib.IntPoint ipp in base_poly[0])
                        ppf.Add(new PointF((float)(ipp.X / 10000000.0), (float)(ipp.Y / 10000000.0)));
                if (base_poly.Count > 1)
                {
                    List<InBoundInsert> inb = new List<InBoundInsert>();
                    for (int ip = 1; ip < base_poly.Count; ip++)
                    {
                        PointF[] inner = new PointF[base_poly[ip].Count];
                        for (int ia = 0; ia < inner.Length; ia++)
                            inner[ia] = new PointF((float)(base_poly[ip][ia].X / 10000000.0), (float)(base_poly[ip][ia].Y / 10000000.0));
                        int indA, indB;
                        FindNearestDots(ppf, inner, out indA, out indB);
                        inb.Add(new InBoundInsert(inner, indA, indB));               
                    };
                    inb.Sort(new InBoundInsertComparer());
                    for (int ip = inb.Count - 1; ip >= 0; ip--)
                        InsertHole(ppf, inb[ip].poly, inb[ip].outboundIndex, inb[ip].inboundIndex);
                };
                return new PolyResult(ppf.ToArray(), null);
            };

            return new PolyResult();
        }

        private static void FindNearestDots(List<PointF> polyA, PointF[] polyB, out int indA, out int indB)
        {
            indA = -1; indB = -1;
            double minDist = double.MaxValue;
            double d = 0;
            for(int a = 0; a < polyA.Count; a++)
                for (int b = 0; b < polyB.Length; b++)
                    if ((d = Math.Sqrt(Math.Pow(polyB[b].X - polyA[a].X, 2) + Math.Pow(polyB[b].Y - polyA[a].Y, 2))) < minDist)
                    {
                        minDist = d;
                        indA = a;
                        indB = b;
                    };
        }

        private static void InsertHole(List<PointF> outbound, PointF[] inbound, int indA, int indB)
        {
            int ins = indA;
            PointF shortway = outbound[ins++];
            for (int i = indB; i < inbound.Length; i++)
                outbound.Insert(ins++, inbound[i]);
            for (int i = 0; i <= indB; i++)
                outbound.Insert(ins++, inbound[i]);
            outbound.Insert(ins++, shortway);
        }

        public static PointF LineIntersection(PointF A, PointF B, PointF C, PointF D)
        {
            // Line AB represented as a1x + b1y = c1  
            double a1 = B.Y - A.Y;
            double b1 = A.X - B.X;
            double c1 = a1 * (A.X) + b1 * (A.Y);

            // Line CD represented as a2x + b2y = c2  
            double a2 = D.Y - C.Y;
            double b2 = C.X - D.X;
            double c2 = a2 * (C.X) + b2 * (C.Y);

            double determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                // The lines are parallel. This is simplified  
                // by returning a pair of FLT_MAX  
                return new PointF((float)double.MaxValue, (float)double.MaxValue);
            }
            else
            {
                double x = (b2 * c1 - b1 * c2) / determinant;
                double y = (a1 * c2 - a2 * c1) / determinant;
                return new PointF((float)x, (float)y);
            }
        }

        private static bool IsInsideLine(PointF[] line, double x, double y)
        {
            return (x >= line[0].X && x <= line[1].X
                        || x >= line[1].X && x <= line[0].X)
                   && (y >= line[0].Y && y <= line[1].Y
                        || y >= line[1].Y && y <= line[0].Y);
        }

        private static bool IsInsideLine(PointF[] line, PointF point)
        {
            return IsInsideLine(line, point.X, point.Y);
        }

        private static bool IsInsideLine(PointF lineA , PointF lineB, PointF point)
        {
            return IsInsideLine(new PointF[] { lineA, lineB }, point.X, point.Y);
        }

        /// <summary>
        ///     Get Buffer Polygon for polyline
        /// </summary>
        /// <param name="line">polyline</param>
        /// <param name="distance">Buffer radius size in custom units (meters)</param>
        /// <param name="DistanceFunc">distance between point function</param>
        /// <returns>polygon</returns>
        public static PolyResult GetLineBufferPolygon(PointF[] line, float distance, DistanceFunction DistanceFunc)
        {
            return GetLineBufferPolygon(line, distance, true, true, DistanceFunc, 0);
        }

        /// <summary>
        ///     Get Buffer Polygon for polyline
        /// </summary>
        /// <param name="line">polyline</param>
        /// <param name="distance">Buffer radius size in custom units (meters)</param>
        /// <returns>polygon</returns>
        public static PolyResult GetLineBufferPolygon(PointF[] line, float distance)
        {
            return GetLineBufferPolygon(line, distance, true, true, GeographicDistFunc, 0);
        }

        /// <summary>
        ///     Get Buffer Polygon for polyline
        /// </summary>
        /// <param name="line">polyline</param>
        /// <param name="distance">Buffer radius size in custom units (meters)</param>
        /// <param name="method">calc method</param>
        /// <returns>polygon</returns>
        public static PolyResult GetLineBufferPolygon(PointF[] line, float distance, int method)
        {
            return GetLineBufferPolygon(line, distance, true, true, GeographicDistFunc, method);
        }

        /// <summary>
        ///     Interpolate polyline/polygon to less points
        /// </summary>
        /// <param name="poly">points</param>
        /// <param name="interpolateLevel">max angle</param>
        /// <param name="distFunc">distance function</param>
        /// <returns>interpolated polyline</returns>
        public static PointF[] Interpolate(PointF[] poly, float interpolateLevel, DistanceFunction distFunc)
        {
            return Interpolate(poly, interpolateLevel, distFunc, 0);
        }

        /// <summary>
        ///     Interpolate polyline/polygon to less points
        /// </summary>
        /// <param name="poly">points</param>
        /// <param name="interpolateLevel">max angle</param>
        /// <param name="distFunc">distance function</param>
        /// <param name="method">method used (0 - multi pass normal, 1 - multi pass triangle, 2 - sinlge pass normal, 3 - single pass triangle)</param>
        /// <returns>interpolated polyline</returns>
        public static PointF[] Interpolate(PointF[] poly, float interpolateLevel, DistanceFunction distFunc, int method)
        {
            int was;
            do
            {
                was = poly.Length;
                List<PointF> pts = new List<PointF>();
                pts.Add(poly[0]);
                for (int i = 1; i < (poly.Length - 1); i++)
                {
                    if (distFunc != null)
                    {
                        float dA = distFunc(poly[i - 1], poly[i]);
                        float dB = distFunc(poly[i - 1], poly[i + 1]);
                        if ((dA == dB) || (dB == 2 * dA)) continue; // single line
                        if ((method == 1) || (method == 3))
                        {
                            double cos = Math.Abs(180 - Math.Abs(AngleFrom3PointsInDegrees3(poly[i + 1], poly[i - 1], poly[i])));
                            if (cos < 1) continue;
                            if ((dB > dA) && (dB / dA < 2) && (cos < interpolateLevel)) continue;
                        };                        
                    };

                    double c = AngleFrom3PointsInDegrees3(poly[i - 1], poly[i], poly[i + 1]);
                    if (Math.Abs(c) < interpolateLevel) continue;
                    else pts.Add(poly[i]);
                };
                pts.Add(poly[poly.Length - 1]);
                poly = pts.ToArray();
                if ((method == 2) || (method == 3)) was = 0;
            }
            while (poly.Length < was);
            return poly;
        }

        private static PointF Lin1(PointF p1, PointF p2, double t)
        {
            PointF q = new PointF();
            q.X = Convert.ToSingle(p2.X * t + p1.X * (1 - t));
            q.Y = Convert.ToSingle(p2.Y * t + p1.Y * (1 - t));
            return q;
        }

        // метод де  астельжо (с рекурсией)
        //  p Ч массив исходных точек, t Ч параметр (distance), n Ч номер уровн€ (0, 1, 2, 3), m Ч номер точки на этом уровне
        private static PointF CastR(PointF[] p, double t, int n, int m)
        {
            if (n == 0)
                return p[m];
            else
                return Lin1(CastR(p, t, n - 1, m), CastR(p, t, n - 1, m + 1), t);
        }

        public static PointF[] Bezier(PointF[] dots, int steps)
        {
            if (dots == null) return null;
            if (dots.Length < 3) return dots;
            if (dots.Length > 4)
            {
                PointF[] dtmp = new PointF[4];
                dtmp[0] = dots[0];
                dtmp[1] = dots[1 * dots.Length / 3];
                dtmp[2] = dots[2 * dots.Length / 3];
                dtmp[3] = dots[dots.Length - 1];
                dots = dtmp;
            };

            PointF[] res = new PointF[steps];
            double d = 1.0 / (res.Length - 1);
            for (int i = 0; i < res.Length; i++)
                res[i] = (CastR(dots, d * i, dots.Length == 3 ? 2 : 3, 0));
            return res;
        }

        private static double AngleFrom3PointsInDegrees1(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double a = x2 - x1;
            double b = y2 - y1;
            double c = x3 - x2;
            double d = y3 - y2;

            double atanA = Math.Atan2(a, b);
            double atanB = Math.Atan2(c, d);

            return (atanA - atanB) * (-180 / Math.PI);
        }

        private static double AngleFrom3PointsInDegrees3(PointF prev, PointF curr, PointF next)
        {
            double angle1 = Math.Atan2(prev.Y - curr.Y, prev.X - curr.X);
            double angle2 = Math.Atan2(curr.Y - next.Y, curr.X - next.X);
            return (angle1 - angle2) * 180.0 / Math.PI;
        }

        private static PointF[] glbp(PointF[] line, float distance, List<PointF[]> poliSegments, DistanceFunction DistanceFunc)
        {
            float step = 1;
            List<PointF> p = new List<PointF>();

#if gpc
            for (int n = 1; n < line.Length; n++)
            {
                List<PointF> polse = new List<PointF>();
                PointF s = line[n - 1];
                PointF e = line[n];

                float angle = (float)((Math.Atan((e.Y - s.Y) / (e.X - s.X)) * 180 / Math.PI));
                if (e.X < s.X) angle = 180 + angle;
                if (angle < 0) angle += 360;

                float c = (float)(e.Y - Math.Tan(angle * Math.PI / 180) * e.X);
                float d = DistFunc(s, new PointF(s.X + step, s.Y));
                float r = distance / d * step;
                float cCR = (float)(c - r / Math.Cos(angle * Math.PI / 180));

                PointF[] els = GetEllipse(s, r, angle - 270, angle - 90);
                PointF[] ele = GetEllipse(e, r, angle - 90, angle + 90);
                List<PointF> segments = new List<PointF>();
                segments.AddRange(els);
                segments.AddRange(ele);
                poliSegments.Add(segments.ToArray());
                p = segments;
            };
            if (poliSegments.Count != 0)
            {
                Polygon was = null;
                for (int i = 0; i < poliSegments.Count; i++)
                {
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddPolygon(poliSegments[i]);
                    Polygon cur = new Polygon(gp);
                    if (i > 0) cur = cur.Clip(GpcOperation.Union, was);
                    was = cur;
                };
                if ((was != null) && (was.Contour != null) && (was.Contour.Length > 0))
                {
                    p.Clear();
                    for (int i = 0; i < was.Contour[0].Vertex.Length; i++)
                        p.Add(new PointF((float)was.Contour[0].Vertex[i].X, (float)was.Contour[0].Vertex[i].Y));
                };
            };
                
#else

            float pAn = 0;
            float pCR = 0;            

            for (int n = 1; n < line.Length; n++)
            {
                // y = f(x)
                // y = tan(angle) * x + c;

                PointF befpP = n < 2 ? new PointF(0, 0) : line[n - 2];
                PointF prevP = line[n - 1];
                PointF currP = line[n];
                float angle = (float)((Math.Atan((currP.Y - prevP.Y) / (currP.X - prevP.X)) * 180 / Math.PI));
                if (currP.X < prevP.X) angle = 180 + angle;
                if (angle < 0) angle += 360;                

                if(Math.Abs(Math.Tan(angle * Math.PI / 180)) > 750) angle -= (float)0.05;


                float c = (float)(currP.Y - Math.Tan(angle * Math.PI / 180) * currP.X);
                float d = DistanceFunc == null ? SampleDistFunc(prevP, new PointF(prevP.X + step, prevP.Y)) : DistanceFunc(prevP, new PointF(prevP.X + step, prevP.Y));
                float r = distance / d * step;
                float cCR = (float)(c - r / Math.Cos(angle * Math.PI / 180));

                //if (poliSegments != null)
                //{
                //    PointF[] els = GetEllipse(s, r, angle - 270, angle - 90);
                //    PointF[] ele = GetEllipse(e, r, angle - 90, angle + 90);
                //    List<PointF> segments = new List<PointF>();
                //    segments.AddRange(els);
                //    segments.AddRange(ele);
                //    poliSegments.Add(segments.ToArray());
                //};

                // first point
                if (n == 1)
                {
                    PointF[] el = GetEllipse(prevP, r, angle - 180, angle - 90);
                    p.AddRange(el);
                    List<PointF> tmps = new List<PointF>();
                    tmps.AddRange(el); tmps.Add(prevP);
                    if (poliSegments != null) poliSegments.Add(tmps.ToArray());
                };

                // lines
                {
                    // yr = f(x)
                    // yr = tan(angle) * x + c - r;

                    if (n == 1)
                    {
                        pAn = angle;
                        pCR = c - r;
                    };

                    if ((angle == pAn) && (n != 1)) // no turn
                    {
                        float xr = (float)(prevP.X + Math.Cos((angle - 90) * Math.PI / 180) * r);
                        float yr = (float)(prevP.Y + Math.Sin((angle - 90) * Math.PI / 180) * r);
                        p.Add(new PointF(xr, yr));
                        List<PointF> tmps = new List<PointF>();
                        tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 1]);
                        tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 2]);
                        tmps.Add(new PointF(xr, yr));
                        tmps.Add(currP);
                        if (poliSegments != null) poliSegments.Add(tmps.ToArray());
                    };                    

                    // turn to left/right
                    if (n > 1)
                    {
                        float dA = (pAn - angle);
                        if (dA < -180) dA += 360;
                        if (dA > 180) dA = dA - 360;

                        if ((dA > 0) && (dA < 180)) // turn left
                        {
                            // tan(angle)*x+(c-r) = tan(pAn)*x+pCR
                            // x = (pCR-(c-r))/tan(angle)-tan(pAn)                            

                            float xu = (float)((pCR - cCR) / (Math.Tan(angle * Math.PI / 180) - Math.Tan(pAn * Math.PI / 180)));
                            float yu = (float)(Math.Tan(pAn * Math.PI / 180) * xu + pCR);

                            bool add = true;
                            if (befpP.X != 0)
                            {
                                float dsum = DistanceFunc == null ? SampleDistFunc(currP, befpP) : DistanceFunc(currP, befpP);
                                if (dsum < (distance * 2))
                                    add = false;
                                else
                                {
                                    float dprv = DistanceFunc == null ? SampleDistFunc(prevP, currP) : DistanceFunc(prevP, currP);
                                    float dcur = DistanceFunc == null ? SampleDistFunc(prevP, befpP) : DistanceFunc(prevP, befpP);
                                    float dadd = DistanceFunc == null ? SampleDistFunc(prevP, new PointF(xu, yu)) : DistanceFunc(prevP, new PointF(xu, yu));
                                    if ((dadd > dcur) && (dadd > dprv)) 
                                        add = false;
                                };                               
                            };


                            if (add)
                            {
                                float da = pAn - dA / 2 - 90;
                                p.Add(new PointF(xu, yu));

                                if (poliSegments != null)
                                {
                                    List<PointF> tmps = new List<PointF>();
                                    tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 1]);
                                    tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 2]);
                                    tmps.Add(new PointF(xu, yu));
                                    tmps.Add(prevP);
                                    poliSegments.Add(tmps.ToArray());
                                };
                            };
                        }
                        else // turn right
                        {
                            PointF[] el = GetEllipse(prevP, r, pAn - 90, angle - 90);
                            p.AddRange(el);
                            
                            if (poliSegments != null)
                            {
                                List<PointF> tmps = new List<PointF>();
                                tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 1]);
                                tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 2]);
                                tmps.AddRange(el);
                                tmps.Add(prevP);
                                poliSegments.Add(tmps.ToArray());
                            };
                        };
                    };
                    pAn = angle;
                    pCR = cCR;
                };

                // last point
                if (n == (line.Length - 1))
                {
                    PointF[] el = GetEllipse(currP, r, angle - 90, angle);
                    p.AddRange(el);
                    if (poliSegments != null)
                    {
                        List<PointF> tmps = new List<PointF>();
                        tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 1]);
                        tmps.Add(poliSegments[poliSegments.Count - 1][poliSegments[poliSegments.Count - 1].Length - 2]);
                        if((el != null) && (el.Length > 0))
                            tmps.Add(el[0]);
                        tmps.Add(currP);
                        poliSegments.Add(tmps.ToArray());
                    };
                    {
                        List<PointF> tmps = new List<PointF>();
                        tmps.Add(currP); tmps.AddRange(el);
                        if (poliSegments != null) poliSegments.Add(tmps.ToArray());
                    };
                };
            };
#endif
            if ((p[0].X == p[p.Count - 1].X) && (p[0].Y == p[p.Count - 1].Y)) p.RemoveAt(p.Count - 1);
            for (int i = p.Count - 2, j = p.Count - 1; i >= 0; i--, j--)
                if ((p[i].X == p[j].X) && (p[i].Y == p[j].Y)) p.RemoveAt(j);

            // optimize
            //for (int pn = p.Count - 1; pn >= 0; pn--)
            //{
            //    float maxd = float.MaxValue;
            //    for (int n = 1; n < line.Length; n++)
            //    {
            //        PointF prevP = line[n - 1];
            //        PointF currP = line[n];                    

            //        PointF pol;
            //        int side;
            //        float curd = DistanceFromPointToLine(p[pn], prevP, currP, DistanceFunc, out pol, out side);
            //        if (curd < maxd) maxd = curd;                    
            //    };
            //    if (maxd < (distance * 0.75)) p.RemoveAt(pn);
            //};

            return p.ToArray();
        }

        /// <summary>
        ///     Create Circle Zone for point
        /// </summary>
        /// <param name="center">Point</param>
        /// <param name="radius">Radius</param>
        /// <param name="angle_from">Angle From</param>
        /// <param name="angle_to">Angle To</param>
        /// <returns>polygon</returns>
        public static PointF[] GetEllipse(PointF center, float radius, float angle_from, float angle_to)
        {
            List<PointF> p = new List<PointF>();
            if (angle_to < angle_from) angle_to += 360;
            for (float an = angle_from; an <= angle_to; an += 90 / 5)
            {
                float x = (float)(center.X + Math.Cos(an * Math.PI / 180) * radius);
                float y = (float)(center.Y + Math.Sin(an * Math.PI / 180) * radius);
                p.Add(new PointF(x, y));
            };
            return p.ToArray();
        }

        public static PointF GetAngledPoint(PointF center, float radius, float angle)
        {
            float x = (float)(center.X + Math.Cos(angle * Math.PI / 180) * radius);
            float y = (float)(center.Y + Math.Sin(angle * Math.PI / 180) * radius);
            return new PointF(x, y);
        }
        
        /// <summary>
        ///     Distance from specified point to line
        /// </summary>
        /// <param name="pt">Specified point</param>
        /// <param name="lineStart">Line Start</param>
        /// <param name="lineEnd">Line End</param>
        /// <param name="DistanceFunc">Get Distance Function</param>
        /// <param name="pointOnLine">Nearest point on line</param>
        /// <param name="side">side of</param>
        /// <returns>distance</returns>
        public static float DistanceFromPointToLine(PointF pt, PointF lineStart, PointF lineEnd, DistanceFunction DistanceFunc, out PointF pointOnLine, out int side)
        {
            float dx = lineEnd.X - lineStart.X;
            float dy = lineEnd.Y - lineStart.Y;

            if ((dx == 0) && (dy == 0))
            {
                // line is a point
                // лини€ может быть с нулевой длиной после анализа TRA
                pointOnLine = lineStart;
                side = 0;
                //dx = pt.X - lineStart.X;
                //dy = pt.Y - lineStart.Y;                
                //return Math.Sqrt(dx * dx + dy * dy);
                float dist = DistanceFunc == null ? SampleDistFunc(pt, pointOnLine) : DistanceFunc(pt, pointOnLine);
                return dist;
            };

            side = Math.Sign((lineEnd.X - lineStart.X) * (pt.Y - lineStart.Y) - (lineEnd.Y - lineStart.Y) * (pt.X - lineStart.X));

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - lineStart.X) * dx + (pt.Y - lineStart.Y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                pointOnLine = new PointF(lineStart.X, lineStart.Y);
                dx = pt.X - lineStart.X;
                dy = pt.Y - lineStart.Y;
            }
            else if (t > 1)
            {
                pointOnLine = new PointF(lineEnd.X, lineEnd.Y);
                dx = pt.X - lineEnd.X;
                dy = pt.Y - lineEnd.Y;
            }
            else
            {
                pointOnLine = new PointF(lineStart.X + t * dx, lineStart.Y + t * dy);
                dx = pt.X - pointOnLine.X;
                dy = pt.Y - pointOnLine.Y;
            };

            float d = DistanceFunc == null ? SampleDistFunc(pt, pointOnLine) : DistanceFunc(pt, pointOnLine);
            return d;
        }

        /// <summary>
        ///     Get Distance by Route
        /// </summary>
        /// <param name="pt">Point</param>
        /// <param name="route">Route</param>
        /// <param name="DistanceFunc">Dist Func</param>
        /// <param name="distance_from_start">Distance from Start of the Route to Point</param>
        /// <returns>Distance from Point to Route</returns>
        public static float DistanceFromPointToRoute(PointF pt, PointF[] route, DistanceFunction DistanceFunc, out float distance_from_start)
        {
            float route_dist = 0;
            float min_dist = float.MaxValue;
            distance_from_start = float.MaxValue;
            for (int i = 1; i < route.Length; i++)
            {
                PointF pointOnLine = PointF.Empty; int side = 0;
                float dist2line = DistanceFromPointToLine(pt, route[i - 1], route[i], DistanceFunc, out pointOnLine, out side);
                if (dist2line < min_dist)
                {
                    min_dist = dist2line;
                    float dist2turn = DistanceFunc == null ? SampleDistFunc(route[i - 1], pointOnLine) : DistanceFunc(route[i - 1], pointOnLine);
                    distance_from_start = route_dist + dist2turn + dist2line;
                };
                route_dist += (DistanceFunc == null ? SampleDistFunc(route[i - 1], route[i]) : DistanceFunc(route[i - 1], route[i]));
            };
            return min_dist;
        }

        public struct InBoundInsert
        {
            public PointF[] poly;
            public int outboundIndex;
            public int inboundIndex;

            public InBoundInsert(PointF[] poly, int outboundIndex, int inboundIndex)
            {
                this.poly = poly;
                this.outboundIndex = outboundIndex;
                this.inboundIndex = inboundIndex;
            }
        }

        public class InBoundInsertComparer : IComparer<InBoundInsert>
        {
            public int Compare(InBoundInsert a, InBoundInsert b)
            {
                return a.outboundIndex.CompareTo(b.outboundIndex);
            }
        }
    }
}
