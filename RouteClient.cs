using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace nmsRouteClient
{
    public static class RouteClient
    {
        public static Route XMLToObject(string xml)
        {
            if (String.IsNullOrEmpty(xml)) return null;
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(Route));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] bb = System.Text.Encoding.UTF8.GetBytes(xml);
            ms.Write(bb, 0, bb.Length);
            ms.Flush();
            ms.Position = 0;
            System.IO.StreamReader reader = new System.IO.StreamReader(ms);
            Route c = (Route)xs.Deserialize(reader);
            reader.Close();
            return c;
        }
    }

    public class nmsRouteClientStopList
    {
        private List<string> nam = new List<string>();
        private List<double> lat = new List<double>();
        private List<double> lon = new List<double>();

        public nmsRouteClientStopList() { }

        public int Count { get { return nam.Count; } }

        public void AddStop(string nam, double lat, double lon)
        {
            if (nam.Length > 200) throw new Exception("Stop name must be less 200 symbols length");
            this.nam.Add(nam);
            this.lat.Add(lat);
            this.lon.Add(lon);            
        }

        public string[] GetStopNames() { return nam.ToArray(); }
        public double[] GetLatt() { return lat.ToArray(); }
        public double[] GetLonn() { return lon.ToArray(); }
    }

    public class Stop
    {
        /// <summary>
        ///     Имя
        /// </summary>
        [XmlText]
        public string name = "";
        /// <summary>
        ///     Широта
        /// </summary>
        [XmlAttribute()]
        public double lat = 0;
        /// <summary>
        ///     Долгота
        /// </summary>
        [XmlAttribute()]
        public double lon = 0;

        public Stop() { }

        public Stop(string name, double lat, double lon)
        {
            this.name = name;
            this.lat = lat;
            this.lon = lon;
        }
    }

    public class XYPoint
    {
        /// <summary>
        ///     Долгота
        /// </summary>
        [XmlAttribute()]
        public double x = 0;
        /// <summary>
        ///     Широта
        /// </summary>
        [XmlAttribute()]
        public double y = 0;

        public XYPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public XYPoint() { }
    }

    public class RoutePoint
    {
        public RoutePoint() { }

        public RoutePoint(int no, double x, double y, double segmentLength, double segmentTime, double totalLength, DateTime totalTime)
        {
            this.no = no;
            this.x = x;
            this.y = y;
            this.sLen = segmentLength;
            this.sTime = segmentTime;
            this.tLen = totalLength;
            this.tTime = totalTime;
        }

        /// <summary>
        ///     Нумрация с 1
        /// </summary>
        [XmlAttribute()]
        public int no = 0;

        /// <summary>
        ///     Инструкция1
        /// </summary>
        public string iToDo = "";
        /// <summary>
        ///     Инструкция2
        /// </summary>
        public string iToGo = "";
        /// <summary>
        ///     Инструкция1
        /// </summary>
        public string iStreet = "";

        /// <summary>
        ///     Долгота
        /// </summary>
        [XmlAttribute()]
        public double x = 0;
        /// <summary>
        ///     Широта
        /// </summary>
        [XmlAttribute()]
        public double y = 0;

        /// <summary>
        ///     Время текущего сегмента в мин
        /// </summary>
        [XmlAttribute()]
        public double sTime = 0;

        /// <summary>
        ///     Длина текущего сегмента в км
        /// </summary>
        [XmlAttribute()]
        public double sLen = 0;

        /// <summary>
        ///     Время прибытия в начало сегмента
        /// </summary>
        [XmlAttribute()]
        public DateTime tTime = DateTime.Now;

        /// <summary>
        ///     Длина от начала маршрута до сегмента
        /// </summary>
        [XmlAttribute()]
        public double tLen = 0;
    }

    public class Route
    {
        public Route() { }

        /// <summary>
        ///     Длина маршрута в км
        /// </summary>
        public double driveLength = 0;
        /// <summary>
        ///      расстояние между промежуточными точками маршрута
        /// </summary>
        [XmlArrayItem("dls")]
        public double[] driveLengthSegments = new double[0];
        /// <summary>
        ///     Время в пути в мин
        /// </summary>
        public double driveTime = 0;
        /// <summary>
        ///      время между промежуточными точками маршрута
        /// </summary>
        [XmlArrayItem("dts")]
        public double[] driveTimeSegments = new double[0];
        /// <summary>
        ///     Время выезда
        /// </summary>
        public DateTime startTime = DateTime.Now;
        /// <summary>
        ///     Время прибытия
        /// </summary>
        public DateTime finishTime = DateTime.Now;
        /// <summary>
        ///     Маршрутные точки
        /// </summary>
        [XmlArrayItem("stop")]
        public Stop[] stops = new Stop[0];
        /// <summary>
        ///     полилиния маршрута
        /// </summary>
        [XmlArrayItem("p")]
        public XYPoint[] polyline = new XYPoint[0];
        /// <summary>
        ///      индекс, указывающий на элемент массива polyline для каждого
        ///      участка между промежуточными точками маршрута
        /// </summary>
        [XmlArrayItem("ps")]
        public int[] polylineSegments = new int[0];
        /// <summary>
        ///     инструкции
        /// </summary>
        [XmlArrayItem("i")]
        public RoutePoint[] instructions = new RoutePoint[0];
        /// <summary>
        ///      индекс, указывающий на элемент массива instructions для каждого
        ///      участка между промежуточными точками маршрута
        /// </summary>
        [XmlArrayItem("is")]
        public int[] instructionsSegments = new int[0];

        /// <summary>
        ///     Ошибка, если есть
        /// </summary>
        public string LastError = String.Empty;
    }
}
