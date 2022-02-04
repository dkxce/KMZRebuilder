/***************************************************/
/*                                                 */
/*            C# Garmin POI File Reader            */
/*              (by milokz@gmail.com)              */
/*                        &                        */
/*            C# Garmin POI File Writer            */
/*              (by milokz@gmail.com)              */
/*                                                 */
/*         GPIReader by milokz@gmail.com           */
/*     Part of KMZRebuilder & KMZViewer Project    */
/*         GPIWrited by milokz@gmail.com           */
/*          Part of KMZRebuilder Project           */
/*                                                 */
/*             by reverse engineering              */
/*                                                 */
/***************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace KMZRebuilder
{
    #region RECTYPES
    /// <summary>
    ///     GPI File Record Types
    /// </summary>
    public enum RecType: ushort
    {
        Header0  = 0,
        Header1  = 1,
        Waypoint = 2,
        Alert    = 3,
        BitmapReference = 4,
        Bitmap = 5,
        CategoryReference = 6,
        Category = 7,
        Area = 8,
        POIGroup = 9,
        Comment = 10,
        Address = 11,
        Contact = 12,
        Image = 13,
        Description = 14,
        ProductInfo = 15,
        AlertCircle = 16,
        Copyright = 17,
        Media = 18,
        SpeedCamera = 19,
        AlertTriggerOptions = 27,
        End = 0xFFFF
    }

    /// <summary>
    ///     Type Utils
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class RecEnum<T>
    {
        public static bool IsDefined(string name)
        {
            return Enum.IsDefined(typeof(T), name);
        }

        public static bool IsDefined(T value)
        {
            return Enum.IsDefined(typeof(T), value);
        }       
    }

    /// <summary>
    ///     GPI File Record Block
    /// </summary>
    public class Record
    {
        /// <summary>
        ///     Parent Record
        /// </summary>
        public Record Parent = null;
        /// <summary>
        ///     Child Records
        /// </summary>
        public List<Record> Childs = new List<Record>();
        
        /// <summary>
        ///     Record is on top
        /// </summary>
        public bool RootIsTop { get { return Parent == null; } }
        /// <summary>
        ///     Record Nesting Index
        /// </summary>
        public int RootLevel { get { return Parent == null ? 0 : Parent.RootLevel + 1; } }        
        /// <summary>
        ///     Record Nesting Ierarchy
        /// </summary>
        internal string RootIerarchy { get { return Parent == null ? @"\Root" : Parent.RootIerarchy + @"\" + RecordType.ToString(); } }

        /// <summary>
        ///     Has Extra Block Data
        /// </summary>
        public bool RecHasExtra { get { return (RecFlags & 0x08) == 0x08; } }
        /// <summary>
        ///     GPI Record Type
        /// </summary>
        public RecType RecordType { get { return (RecType)RecType; } }
        /// <summary>
        ///     GPI Record Type
        /// </summary>
        internal ushort RecType = 0;
        /// <summary>
        ///     GPI Record Flags
        /// </summary>
        internal ushort RecFlags = 0;        

        /// <summary>
        ///     Offset of Record Block
        /// </summary>
        internal uint OffsetBlock = 0;
        /// <summary>
        ///     Offset of Record Main Data Block
        /// </summary>
        internal uint OffsetMain = 0;
        /// <summary>
        ///     Offset of Record Extra Data Block
        /// </summary>
        internal uint OffsetExtra = 0;

        /// <summary>
        ///     Record Block Length
        /// </summary>
        internal uint LengthBlock = 0;
        /// <summary>
        ///     Record Block Main Data Length
        /// </summary>
        internal uint LengthMain = 0;
        /// <summary>
        ///     Record Block Extra Data Length
        /// </summary>
        internal uint LengthExtra = 0;
        /// <summary>
        ///     Record Block Main & Extra Data Length
        /// </summary>
        internal uint LengthTotal = 0;        

        /// <summary>
        ///     Source Block Without Any Offsets
        /// </summary>
        internal byte[] DataBlock;
        /// <summary>
        ///     Record Main Data
        /// </summary>
        public byte[] DataMain
        {
            get
            {
                if (DataBlock == null) return null;
                byte[] res = new byte[LengthMain];
                Array.Copy(DataBlock, OffsetMain, res, 0, LengthMain);
                return res;
            }
        }
        /// <summary>
        ///     Record Extra Data
        /// </summary>
        public byte[] DataExtra
        {
            get
            {
                if (DataBlock == null) return null;
                byte[] res = new byte[LengthExtra];
                Array.Copy(DataBlock, OffsetExtra, res, 0, LengthExtra);
                return res;
            }
        }
        /// <summary>
        ///     Record Main & Extra Data
        /// </summary>
        public byte[] DataTotal
        {
            get
            {
                if (DataBlock == null) return null;
                byte[] res = new byte[LengthTotal];
                Array.Copy(DataBlock, OffsetMain, res, 0, LengthTotal);
                return res;
            }
        }
        
        /// <summary>
        ///     Last Read Record Block Error
        /// </summary>
        internal Exception ReadError = null;

        /// <summary>
        ///     Create with Parent
        /// </summary>
        /// <param name="parent"></param>
        protected Record(Record parent)
        {
            this.Parent = parent;
            if (parent != null) parent.Childs.Add(this);
        }

        /// <summary>
        ///     Create No Parent (File Root)
        /// </summary>
        public static Record ROOT
        {
            get
            {
                return new Record(null);
            }             
        }

        public static Record Create(Record parent, uint offset, ref byte[] sourceData, ushort RecordType)
        {
            Record res = null;            
            if (RecordType == 0) res = new RecHeader0(parent);
            if (RecordType == 1) res = new RecHeader1(parent);            
            if (RecordType == 2) res = new RecWaypoint(parent);
            if (RecordType == 3) res = new RecAlert(parent);
            if (RecordType == 4) res = new RecBitmapReference(parent);
            if (RecordType == 5) res = new RecBitmap(parent);
            if (RecordType == 6) res = new RecCategoryReference(parent);
            if (RecordType == 7) res = new RecCategory(parent);
            if (RecordType == 8) res = new RecArea(parent);
            if (RecordType == 9) res = new RecPOIGroup(parent);
            if (RecordType == 10) res = new RecComment(parent);
            if (RecordType == 11) res = new RecAddress(parent);
            if (RecordType == 12) res = new RecContact(parent);
            if (RecordType == 13) res = new RecImage(parent);
            if (RecordType == 14) res = new RecDescription(parent);
            if (RecordType == 15) res = new RecProductInfo(parent);
            if (RecordType == 16) res = new RecAlertCircle(parent);
            if (RecordType == 17) res = new RecCopyright(parent);
            if (RecordType == 18) res = new RecMedia(parent);
            if (RecordType == 19) res = new RecSpeedCamera(parent);
            if (RecordType == 27) res = new RecAlertTriggerOptions(parent);
            if (RecordType == 0xFFFF) res = new RecEnd(parent);
            if (res == null) res = new Record(parent);
            res.RecType = RecordType;
            res.OffsetBlock = offset;
            res.DataBlock = sourceData;
            return res;            
        }

        public override string ToString()
        {
            return String.Format("{1}[{2}]{3}", RecordType, RecType, RootLevel, RootIerarchy);
        }
    }

    // 0
    public sealed class RecHeader0 : Record
    {
        internal RecHeader0(Record parent) : base(parent) { }
        public string Header = null;
        public string Version = null;
        public DateTime Created = DateTime.MinValue;
        public string Name = null;
    }

    // 1
    public sealed class RecHeader1 : Record
    {
        internal RecHeader1(Record parent) : base(parent) { }
        public string Content = null;
        public ushort CodePage = 0xFDE9;
        public Encoding Encoding
        {
            get
            {
                try { return Encoding.GetEncoding(CodePage); } catch { };
                return Encoding.Unicode;
            }
        }
    }

    // 2
    public sealed class RecWaypoint : Record
    {
        internal RecWaypoint(Record parent) : base(parent) { }
        internal int cLat;
        internal int cLon;
        public double Lat { get { return (double)cLat * 360.0 / Math.Pow(2, 32); } }
        public double Lon { get { return (double)cLon * 360.0 / Math.Pow(2, 32); } }
        public List<KeyValuePair<string, string>> ShortName = new List<KeyValuePair<string, string>>();

        public string Name
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in ShortName)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in ShortName)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in ShortName)
                    return kvp.Value;
                return null;
            }
        }

        public RecAlert Alert;
        public RecBitmap Bitmap;
        public RecImage Image;        

        public RecDescription Description;
        public RecComment Comment;
        public RecContact Contact;
        public RecAddress Address;      
    }

    // 3
    public sealed class RecAlert : Record
    {
        internal RecAlert(Record parent) : base(parent) { }
        public ushort Proximity;
        internal ushort cSpeed;
        public int Speed { get { return (int)Math.Round((double)cSpeed / 100.0 * 3.6); } }
        public byte Alert;
        public byte AlertType;
        public byte SoundNumber;
        public byte AudioAlert;
        public bool IsOn { get { return Alert == 1; } }
        public string IsType { get {
            if (AlertType == 0) return "proximity";
            if (AlertType == 1) return "along_road";
            if (AlertType == 2) return "toure_guide";
            return AlertType.ToString();
        } }
        public RecAlertCircle AlertCircles;
        public RecAlertTriggerOptions AlertTriggerOptions;
    }

    // 4
    public sealed class RecBitmapReference : Record
    {
        internal RecBitmapReference(Record parent) : base(parent) { }
        public ushort BitmapID;
    }

    // 5
    public sealed class RecBitmap : Record
    {
        internal RecBitmap(Record parent) : base(parent) { }
        public ushort BitmapID;
        public ushort Height;
        public ushort Width;
        public ushort LineSize;
        public ushort BitsPerPixel;
        public ushort Reserved9;
        public uint ImageSize; // LineSize * Height
        public uint Reserved10;
        public uint Palette;
        public uint TransparentColor;
        public uint Flags;
        public uint Reserved11;
        public byte[] Pixels;
        public uint[] Colors;
    }

    // 6
    public sealed class RecCategoryReference : Record
    {
        internal RecCategoryReference(Record parent) : base(parent) { }
        public ushort CategoryID;
    }

    // 7
    public sealed class RecCategory : Record
    {
        internal RecCategory(Record parent) : base(parent) { }
        public ushort CategoryID;
        public List<KeyValuePair<string, string>> Category = new List<KeyValuePair<string, string>>();        
        public string Name
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in Category)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in Category)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in Category)
                    return kvp.Value;
                return null;
            }
        }

        public List<RecWaypoint> Waypoints = new List<RecWaypoint>();
        public RecBitmap Bitmap = null;

        public RecDescription Description;
        public RecComment Comment;
        public RecContact Contact;
    }

    // 8
    public sealed class RecArea : Record
    {
        internal RecArea(Record parent) : base(parent) { }
        internal int cMaxLat;
        internal int cMaxLon;
        internal int cMinLat;
        internal int cMinLon;
        public double MaxLat { get { return (double)cMaxLat * 360.0 / Math.Pow(2, 32); } }
        public double MaxLon { get { return (double)cMaxLon * 360.0 / Math.Pow(2, 32); } }
        public double MinLat { get { return (double)cMinLat * 360.0 / Math.Pow(2, 32); } }
        public double MinLon { get { return (double)cMinLon * 360.0 / Math.Pow(2, 32); } }        
    }

    // 9
    public sealed class RecPOIGroup : Record
    {
        internal RecPOIGroup(Record parent) : base(parent) { }
        public List<KeyValuePair<string, string>> DataSource = new List<KeyValuePair<string, string>>();

        public string Name
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in DataSource)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in DataSource)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in DataSource)
                    return kvp.Value;
                return null;
            }
        }
    }

    // 10
    public sealed class RecComment : Record
    {
        internal RecComment(Record parent) : base(parent) { }
        public List<KeyValuePair<string, string>> Comment = new List<KeyValuePair<string, string>>();

        public string Text
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in Comment)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in Comment)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in Comment)
                    return kvp.Value;
                return null;
            }
        }
    }

    // 11
    public sealed class RecAddress : Record
    {
        internal RecAddress(Record parent) : base(parent) { }
        public ushort Flags;
        public List<KeyValuePair<string, string>> aCity = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> aCountry = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> aState = new List<KeyValuePair<string, string>>();
        public string Postal;
        public List<KeyValuePair<string, string>> aStreet = new List<KeyValuePair<string, string>>();
        public string House;

        public string City
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in aCity)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in aCity)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in aCity)
                    return kvp.Value;
                return null;
            }
        }

        public string Country
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in aCountry)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in aCountry)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in aCountry)
                    return kvp.Value;
                return null;
            }
        }

        public string State
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in aState)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in aState)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in aState)
                    return kvp.Value;
                return null;
            }
        }

        public string Street
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in aStreet)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in aStreet)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in aStreet)
                    return kvp.Value;
                return null;
            }
        }
    }

    // 12
    public sealed class RecContact : Record
    {
        internal RecContact(Record parent) : base(parent) { }
        public ushort Flags;
        public string Phone;
        public string Phone2;
        public string Fax;
        public string Email;
        public string Web;       
    }

    // 13
    public sealed class RecImage : Record
    {
        internal RecImage(Record parent) : base(parent) { }
        public uint Length;
        public byte[] ImageData;
    }

    // 14
    public sealed class RecDescription : Record
    {
        internal RecDescription(Record parent) : base(parent) { }
        public List<KeyValuePair<string, string>> Description = new List<KeyValuePair<string, string>>();

        public string Text
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in Description)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in Description)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in Description)
                    return kvp.Value;
                return null;
            }
        }
    }

    // 15
    public sealed class RecProductInfo : Record
    {
        internal RecProductInfo(Record parent) : base(parent) { }
        public ushort FactoryID;
        public byte ProductID;
        public byte RegionID;
        public byte VendorID;
    }

    // 16
    public sealed class RecAlertCircle : Record
    {
        internal RecAlertCircle(Record parent) : base(parent) { }
        public ushort Count;
        public double[] lat;
        public double[] lon;
        public uint[] radius;
    }

    // 17
    public sealed class RecCopyright : Record
    {
        internal RecCopyright(Record parent) : base(parent) { }
        public ushort Flags1 = 0;
        public ushort Flags2 = 0;
        public List<KeyValuePair<string, string>> cDataSource = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> cCopyrights = new List<KeyValuePair<string, string>>();
        public string DeviceModel = null;

        public string DataSource
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in cDataSource)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cDataSource)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cDataSource)
                    return kvp.Value;
                return null;
            }
        }

        public string Copyrights
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in cCopyrights)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cCopyrights)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cCopyrights)
                    return kvp.Value;
                return null;
            }
        }
    }

    // 18
    public sealed class RecMedia : Record
    {
        internal RecMedia(Record parent) : base(parent) { }
        public ushort MediaID;
        public byte Format;
        public bool IsWav { get { return Format == 0; } }
        public bool IsMP3 { get { return Format == 1; } }
        public List<KeyValuePair<string, byte[]>> Content = new List<KeyValuePair<string, byte[]>>();

        public byte[] Media
        {
            get
            {
                foreach (KeyValuePair<string, byte[]> kvp in Content)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, byte[]> kvp in Content)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                return null;
            }
        }
    }

    // 19
    public sealed class RecSpeedCamera : Record
    {
        internal RecSpeedCamera(Record parent) : base(parent) { }
        internal int cMaxLat;
        internal int cMaxLon;
        internal int cMinLat;
        internal int cMinLon;
        public double MaxLat { get { return (double)cMaxLat * 360.0 / Math.Pow(2, 24); } }
        public double MaxLon { get { return (double)cMaxLon * 360.0 / Math.Pow(2, 24); } }
        public double MinLat { get { return (double)cMinLat * 360.0 / Math.Pow(2, 24); } }
        public double MinLon { get { return (double)cMinLon * 360.0 / Math.Pow(2, 24); } }
        public byte Flags;
        internal int cLat;
        internal int cLon;
        public double Lat { get { return (double)cLat * 360.0 / Math.Pow(2, 24); } }
        public double Lon { get { return (double)cLon * 360.0 / Math.Pow(2, 24); } }
    }

    // 27 
    public sealed class RecAlertTriggerOptions : Record
    {
        internal RecAlertTriggerOptions(Record parent) : base(parent) { }
        public byte BearingCount = 0;
        public ushort[] BearingAngle;
        public ushort[] BearingWide;
        public bool[] BearingBiDir;
        public byte[] DateTimeBlock;
        public List<string> DateTimeList = new List<string>();
    }

    [Serializable]
    public class MarkerBlock
    {
        public MarkerBlock() { }

        public static MarkerBlock FromBytes(byte[] data)
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(MarkerBlock));
            MemoryStream ms = new MemoryStream(data);
            System.IO.StreamReader reader = new System.IO.StreamReader(ms, System.Text.Encoding.UTF8);
            MarkerBlock c = (MarkerBlock)xs.Deserialize(reader);
            ms.Close();
            return c;
        }

        public byte[] ToBytes()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            this.Creator = "KMZRebuilder v" + fvi.FileVersion;
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            settings.OmitXmlDeclaration = true;
            settings.NewLineHandling = NewLineHandling.None;
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(MarkerBlock));
            System.IO.MemoryStream ms = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(ms, settings);
            xs.Serialize(writer, this, ns);
            writer.Flush();
            ms.Position = 0;
            byte[] bb = new byte[ms.Length];
            ms.Read(bb, 0, bb.Length);
            writer.Close();
            ms.Close();
            return bb;
        }

        [XmlArray("Bounds"),XmlArrayItem("B")]
        public double[] Bounds = null;
        public string Creator = "KMZRebuilder";
        [XmlElement("DT")]
        public DateTime Created = DateTime.MinValue;
        public string Description = null;
    }

    // 0xFFFF
    public sealed class RecEnd : Record
    {
        internal RecEnd(Record parent) : base(parent) { }
    }
    #endregion RECTYPES

    /// <summary>
    ///     GPI Reader
    /// </summary>
    public class GPIReader
    {
        public delegate void Add2LogProc(string text);

        /// <summary>
        ///     Current Locale Language ISO-639
        /// </summary>
        public static string LOCALE_LANGUAGE  = "EN"; // 2-SYMBOLS
        /// <summary>
        ///     Default Language ISO-639
        /// </summary>
        public static string DEFAULT_LANGUAGE = "EN"; // 2-SYMBOLS
        /// <summary>
        ///     Save Media Content to disk
        /// </summary>
        public static bool SAVE_MEDIA = false;
        /// <summary>
        ///     Create Images for categories without images
        /// </summary>
        public static bool CREATE_CATEGORY_IMAGES_IFNO = false;
        /// <summary>
        ///     Set kmz poi image from jpeg (not bitmap); false - from bitmap; true - from image (if specified)
        /// </summary>
        public static bool POI_IMAGE_FROM_JPEG = false; // bitmap o
        /// <summary>
        ///     Save Multilanguage Names in Description
        /// </summary>
        public static bool SAVE_MULTINAMES = true;

        /// <summary>
        ///     Source File Name
        /// </summary>
        public string FileName { get { return fileName; } }
        private string fileName;        

        /// <summary>
        ///     Public GPI Root Element
        /// </summary>
        public Record RootElement = Record.ROOT;

        /// <summary>
        ///     GPI File Document Name
        /// </summary>
        public string Content = null;
        /// <summary>
        ///     GPI Text CodePage
        /// </summary>
        public ushort CodePage = 0xFDE9;
        /// <summary>
        ///     GPI Text Encoding
        /// </summary>
        public Encoding Encoding = Encoding.Unicode;
        /// <summary>
        ///     GPI File Header Text
        /// </summary>
        public string Header = null;
        /// <summary>
        ///     GPI File Version
        /// </summary>
        public string Version = null;
        /// <summary>
        ///     GPI File DateTime Created
        /// </summary>
        public DateTime Created = DateTime.MinValue;
        /// <summary>
        ///     GPI File Name
        /// </summary>
        public string Name = null;
        /// <summary>
        ///     Multilang Content Data Sources
        /// </summary>
        public List<KeyValuePair<string, string>> cDataSource = new List<KeyValuePair<string, string>>();
        /// <summary>
        ///     Multilang Content Copyrights
        /// </summary>
        public List<KeyValuePair<string, string>> cCopyrights = new List<KeyValuePair<string, string>>();

        /// <summary>
        ///     List Of POI Categories in file
        /// </summary>
        public Dictionary<ushort, RecCategory> Categories = new Dictionary<ushort, RecCategory>();

        /// <summary>
        ///     List of Bitmaps in file
        /// </summary>
        public Dictionary<ushort, RecBitmap> Bitmaps = new Dictionary<ushort, RecBitmap>();

        /// <summary>
        ///     List of Media in file
        /// </summary>
        public Dictionary<ushort, RecMedia> Medias = new Dictionary<ushort, RecMedia>();

        private Add2LogProc Add2Log;
        private uint readNotifier;

        /// <summary>
        ///     File Content Data Source (local language)
        /// </summary>
        public string DataSource
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in cDataSource)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cDataSource)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cDataSource)
                    return kvp.Value;
                return null;
            }
        }

        /// <summary>
        ///     File Content Copyrights (local language)
        /// </summary>
        public string Copyrights
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in cCopyrights)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cCopyrights)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cCopyrights)
                    return kvp.Value;
                return null;
            }
        }

        /// <summary>
        ///     Marker Block
        /// </summary>
        public MarkerBlock MarkerData = null;

        /// <summary>
        ///     Constructor (GPI File Reader)
        /// </summary>
        /// <param name="fileName"></param>
        public GPIReader(string fileName)
        {
            this.fileName = fileName;
            this.Read();
            this.LoopRecords(this.RootElement.Childs);
        }

        /// <summary>
        ///     Constructor (GPI File Reader)
        /// </summary>
        /// <param name="fileName"></param>
        public GPIReader(string fileName, Add2LogProc Add2Log)
        {
            this.Add2Log = Add2Log;
            this.fileName = fileName;
            this.Read();
            if (Add2Log != null) Add2Log(String.Format("POI File, version {0}",this.Version));
            if (Add2Log != null) Add2Log("Reading References...");
            this.LoopRecords(this.RootElement.Childs);
            if (Add2Log != null) Add2Log("Reading Done");
        }

        /// <summary>
        ///     Save File Content to KML file
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveToKML(string fileName)
        {
            SaveToKML(fileName, null);
        }

        /// <summary>
        ///     Save File Content to KML file
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveToKML(string fileName, Add2LogProc Add2Log)
        {
            if(Add2Log != null) this.Add2Log = Add2Log;
            string images_file_dir = Path.GetDirectoryName(fileName) + @"\images\";
            Directory.CreateDirectory(images_file_dir);

            if (this.Add2Log != null) this.Add2Log("Saving to kml...");
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml><Document>");
            string caption = (String.IsNullOrEmpty(this.Name) ? "GPI Has No Name" : this.Name);
            if (!String.IsNullOrEmpty(this.DataSource)) caption = this.DataSource;
            sw.WriteLine("<name><![CDATA[" + caption +  "]]></name><createdby>KMZ Rebuilder GPI Reader</createdby>");
            string desc = "Created: " + this.Created.ToString() + "\r\n";
            foreach (KeyValuePair<string, string> langval in this.cDataSource)
                desc += String.Format("data_source:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
            foreach (KeyValuePair<string, string> langval in this.cCopyrights)
                desc += String.Format("copyrights:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
            sw.WriteLine("<description><![CDATA[" + desc + "]]></description>");
            List<string> simstyles = new List<string>();
            int ccount = 0;
            foreach (KeyValuePair<ushort, RecCategory> kCat in this.Categories)
            {
                ccount++;
                if (kCat.Value.Waypoints.Count == 0) continue;
                if (this.Add2Log != null) this.Add2Log(String.Format("Saving {2} POIs of {0}/{1} Category...", ccount, this.Categories.Count, kCat.Value.Waypoints.Count));

                string style = "catid" + kCat.Value.CategoryID.ToString();
                if (kCat.Value.Bitmap != null) style = "imgid" + kCat.Value.Bitmap.BitmapID.ToString();
                
                sw.WriteLine("<Folder><name><![CDATA[" + kCat.Value.Name + "]]></name>");
                desc = "CategoryID: " + kCat.Value.CategoryID.ToString() + "\r\n";
                desc += "Objects: " + kCat.Value.Waypoints.Count.ToString() + "\r\n";
                if(GPIReader.SAVE_MULTINAMES)
                    foreach (KeyValuePair<string, string> langval in kCat.Value.Category)
                        desc += String.Format("name:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);                
                if (kCat.Value.Comment != null)
                    foreach (KeyValuePair<string, string> langval in kCat.Value.Comment.Comment)
                        desc += String.Format("comm:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                if (kCat.Value.Contact != null)
                {
                    if(!String.IsNullOrEmpty(kCat.Value.Contact.Phone))
                        desc += String.Format("contact_phone={0}\r\n", kCat.Value.Contact.Phone);
                    if (!String.IsNullOrEmpty(kCat.Value.Contact.Phone2))
                        desc += String.Format("contact_phone2={0}\r\n", kCat.Value.Contact.Phone2);
                    if (!String.IsNullOrEmpty(kCat.Value.Contact.Fax))
                        desc += String.Format("contact_fax={0}\r\n", kCat.Value.Contact.Fax);
                    if (!String.IsNullOrEmpty(kCat.Value.Contact.Email))
                        desc += String.Format("contact_email={0}\r\n", kCat.Value.Contact.Email);
                    if (!String.IsNullOrEmpty(kCat.Value.Contact.Web))
                        desc += String.Format("contact_web={0}\r\n", kCat.Value.Contact.Web);
                };
                if ((kCat.Value.Description != null) && (kCat.Value.Description.Description.Count > 0))
                {
                    if(desc.Length > 0) desc += "\r\n";
                    foreach (KeyValuePair<string, string> langval in kCat.Value.Description.Description)
                        desc += String.Format("desc:{0}={1}\r\n\r\n", langval.Key.ToLower(), TrimDesc(langval.Value));
                };
                sw.WriteLine("<description><![CDATA[" + desc + "]]></description>");
                foreach (RecWaypoint wp in kCat.Value.Waypoints)
                {
                    sw.WriteLine("<Placemark>");
                    sw.WriteLine("<name><![CDATA[" + wp.Name + "]]></name>");
                    string text = "";
                    if (GPIReader.SAVE_MULTINAMES)
                        foreach (KeyValuePair<string, string> langval in wp.ShortName)
                            text += String.Format("name:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);                    
                    if (wp.Comment != null)
                        foreach (KeyValuePair<string, string> langval in wp.Comment.Comment)
                            text += String.Format("comm:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                    if (wp.Contact != null)
                    {
                        if (!String.IsNullOrEmpty(wp.Contact.Phone))
                            text += String.Format("contact_phone={0}\r\n", wp.Contact.Phone);
                        if (!String.IsNullOrEmpty(wp.Contact.Phone2))
                            text += String.Format("contact_phone2={0}\r\n", wp.Contact.Phone2);
                        if (!String.IsNullOrEmpty(wp.Contact.Fax))
                            text += String.Format("contact_fax={0}\r\n", wp.Contact.Fax);
                        if (!String.IsNullOrEmpty(wp.Contact.Email))
                            text += String.Format("contact_email={0}\r\n", wp.Contact.Email);
                        if (!String.IsNullOrEmpty(wp.Contact.Web))
                            text += String.Format("contact_web={0}\r\n", wp.Contact.Web);
                    };
                    if (wp.Address != null)
                    {
                        foreach (KeyValuePair<string, string> langval in wp.Address.aCountry)
                            text += String.Format("addr_country:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                        if (!String.IsNullOrEmpty(wp.Address.Postal))
                            text += String.Format("addr_postal={0}\r\n", wp.Address.Postal);
                        foreach (KeyValuePair<string, string> langval in wp.Address.aState)
                            text += String.Format("addr_state:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                        foreach (KeyValuePair<string, string> langval in wp.Address.aCity)
                            text += String.Format("addr_city:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                        foreach (KeyValuePair<string, string> langval in wp.Address.aStreet)
                            text += String.Format("addr_street:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                        if (!String.IsNullOrEmpty(wp.Address.House))
                            text += String.Format("addr_house={0}\r\n", wp.Address.House);
                    };
                    if (wp.Alert != null)
                    {                        
                        text += String.Format("alert_proximity={0}\r\n", wp.Alert.Proximity);
                        text += String.Format("alert_speed={0}\r\n", wp.Alert.Speed);
                        text += String.Format("alert_ison={0}\r\n", wp.Alert.Alert);
                        text += String.Format("alert_type={0}\r\n", wp.Alert.IsType);                        
                        if (SAVE_MEDIA)
                        {
                            ushort sn = (ushort)(wp.Alert.SoundNumber + (wp.Alert.AudioAlert << 8));
                            if (Medias.ContainsKey(sn))
                            {
                                string ext = "bin";
                                if (Medias[sn].Format == 0) ext = "wav";
                                if (Medias[sn].Format == 1) ext = "mp3";
                                string fName = String.Format("{0}-{1}.{2}", Medias[sn].MediaID, Medias[sn].Content[0].Key, ext);
                                text += String.Format("alert_sound=media/{0}\r\n", fName);
                            };
                        };
                        if (wp.Alert.AlertCircles != null)
                        {
                            for (int z = 0; z < wp.Alert.AlertCircles.Count; z++)
                            {
                                double clat = wp.Alert.AlertCircles.lat[z];
                                double clon = wp.Alert.AlertCircles.lon[z];
                                uint crad = wp.Alert.AlertCircles.radius[z];
                                if ((clat == wp.Lat) && (clon == wp.Lon))
                                    text += String.Format("alert_circle={0}\r\n", crad);
                                else
                                    text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "alert_circle={0},{1:0.000000},{2:0.000000}\r\n", crad, clat, clon);
                            };
                        };
                        if (wp.Alert.AlertTriggerOptions != null)
                        {
                            if(wp.Alert.AlertTriggerOptions.BearingCount > 0)
                                for (int z = 0; z < wp.Alert.AlertTriggerOptions.BearingCount; z++)
                                    text += String.Format(System.Globalization.CultureInfo.InvariantCulture, "alert_bearing={0},{1},{2}\r\n", wp.Alert.AlertTriggerOptions.BearingAngle[z], wp.Alert.AlertTriggerOptions.BearingWide[z], wp.Alert.AlertTriggerOptions.BearingBiDir[z] ? "bidir" : "onedir");
                            if ((wp.Alert.AlertTriggerOptions.DateTimeList != null) && (wp.Alert.AlertTriggerOptions.DateTimeList.Count > 0))
                                for (int z = 0; z < wp.Alert.AlertTriggerOptions.DateTimeList.Count; z++)
                                    text += String.Format("alert_datetime={0}\r\n", wp.Alert.AlertTriggerOptions.DateTimeList[z]);
                        };
                    };
                    if ((wp.Description != null) && (wp.Description.Description.Count > 0))
                    {
                        if (text.Length > 0) text += "\r\n";
                        foreach (KeyValuePair<string, string> langval in wp.Description.Description)
                            text += String.Format("desc:{0}={1}\r\n\r\n", langval.Key.ToLower(), TrimDesc(langval.Value));
                    };
                    if (wp.Bitmap != null) style = "imgid" + wp.Bitmap.BitmapID.ToString();
                    if ((wp.Image != null) && (wp.Image.Length > 0))
                    {
                        try
                        {
                            string simid = "simid" + simstyles.Count.ToString();
                            FileStream fsimid = new FileStream(images_file_dir + simid + ".jpg", FileMode.Create, FileAccess.Write);
                            fsimid.Write(wp.Image.ImageData, 0, wp.Image.ImageData.Length);
                            fsimid.Close();
                            simstyles.Add(simid);
                            if(POI_IMAGE_FROM_JPEG) style = simid;
                        }
                        catch (Exception ex) { };
                    };
                    sw.WriteLine("<description><![CDATA[" + text + "]]></description>");
                    sw.WriteLine("<styleUrl>#" + style + "</styleUrl>");
                    string xyz = wp.Lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + wp.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",0";
                    sw.WriteLine("<Point><coordinates>" + xyz + "</coordinates></Point>");
                    sw.WriteLine("</Placemark>");
                };
                sw.WriteLine("</Folder>");
            };
            foreach (string simid in simstyles)
            {
                sw.WriteLine("\t<Style id=\"" + simid + "\"><IconStyle><Icon><href>images/" + simid + ".jpg</href></Icon></IconStyle></Style>");
            };
            if (CREATE_CATEGORY_IMAGES_IFNO)
            {
                if (this.Add2Log != null)
                    this.Add2Log(String.Format("Saving Images for {0} Categories...", this.Categories.Count));
                int imsvd = 0;
                foreach (KeyValuePair<ushort, RecCategory> kCat in this.Categories)
                {
                    if (kCat.Value.Bitmap != null) continue;
                    string catID = "catid" + kCat.Value.CategoryID.ToString();
                    sw.WriteLine("\t<Style id=\"" + catID + "\"><IconStyle><Icon><href>images/" + catID + ".png</href></Icon></IconStyle></Style>");
                    try
                    {
                        Image im = new Bitmap(16, 16);
                        Graphics g = Graphics.FromImage(im);
                        g.FillEllipse(Brushes.Magenta, 0, 0, 16, 16);
                        string ttd = kCat.Value.CategoryID.ToString();
                        while (ttd.Length < 2) ttd = "0" + ttd;
                        g.DrawString(ttd, new Font("MS Sans Serif", 8), Brushes.Black, 0, 2);
                        g.Dispose();
                        im.Save(images_file_dir + catID + ".png");
                        imsvd++;
                    }
                    catch (Exception ex) { };
                };
                if (this.Add2Log != null) this.Add2Log(String.Format("Saved {0} Images", imsvd));
            };
            if (this.Add2Log != null) this.Add2Log(String.Format("Saving {0} Bitmaps...", this.Bitmaps.Count));
            foreach (KeyValuePair<ushort, RecBitmap> bitmaps in this.Bitmaps)
            {
                string imgID = "imgid" + bitmaps.Value.BitmapID.ToString();
                sw.WriteLine("\t<Style id=\"" + imgID + "\"><IconStyle><Icon><href>images/" + imgID + ".png</href></Icon></IconStyle></Style>");
                RecBitmap br = bitmaps.Value;
                if ((br.Pixels != null) && (br.Pixels.Length > 0))
                {
                    try
                    {
                        int wi = br.Width;
                        byte[] sub = new byte[4];
                        int pixelsize = 1;
                        if (br.Palette == 0) pixelsize = br.LineSize / br.Width;
                        if (br.Palette == 16) wi = br.Width / 2;

                        Bitmap bmp = new Bitmap(br.Width, br.Height);
                        Graphics g = Graphics.FromImage(bmp);
                        g.Clear(Color.Transparent);
                        g.Dispose();
                        for (int h = 0; h < br.Height; h++)
                        {
                            int voffset = br.LineSize * h;
                            for (int w = 0; w < wi; w++)
                            {
                                int hoffset = voffset + w * pixelsize;
                                Array.Copy(br.Pixels, hoffset, sub, 0, pixelsize);
                                uint color = BitConverter.ToUInt32(sub, 0);
                                Color c = Color.Transparent;
                                if (br.Palette == 0)
                                {
                                    bmp.SetPixel(w, h, Color.Transparent);
                                    if (color == br.TransparentColor) continue;
                                    c = ColorFromUint(color);
                                    bmp.SetPixel(w, h, c);
                                }
                                else if (br.Palette > 16)
                                {
                                    bmp.SetPixel(w, h, Color.Transparent);
                                    color = br.Colors[color];
                                    if (color == br.TransparentColor) continue;
                                    c = ColorFromUint(color);
                                    bmp.SetPixel(w, h, c);
                                }
                                else
                                {
                                    bmp.SetPixel(2 * w, h, Color.Transparent);
                                    bmp.SetPixel(2 * w + 1, h, Color.Transparent);
                                    int low = (int)br.Colors[(color) & 0x0F];
                                    int hi = (int)br.Colors[((color) & 0xF0) >> 4];
                                    if (low != br.TransparentColor) bmp.SetPixel(2 * w, h, Color.FromArgb(low));
                                    if (hi != br.TransparentColor) bmp.SetPixel(2 * w + 1, h, Color.FromArgb(hi));
                                };
                            };
                        };
                        bmp.Save(images_file_dir + imgID + ".png");
                        bmp.Dispose();                        
                    }
                    catch (Exception ex)
                    {

                    };
                }; 
            };            
            if (SAVE_MEDIA && ((this.MarkerData == null ? this.Medias.Count : this.Medias.Count - 1) > 0))
            {
                if (this.Add2Log != null) 
                    this.Add2Log(String.Format("Saving {0} Medias...", this.MarkerData == null ? this.Medias.Count : this.Medias.Count - 1));
                string medias_file_dir = Path.GetDirectoryName(fileName) + @"\media\";
                Directory.CreateDirectory(medias_file_dir);
                foreach (KeyValuePair<ushort, RecMedia> rm in Medias)
                {
                    for (int i = 0; i < rm.Value.Content.Count; i++)
                    {
                        if (rm.Value.Format != 0x77)
                        {
                            string ext = "bin";
                            if (rm.Value.Format == 0) ext = "wav";
                            if (rm.Value.Format == 1) ext = "mp3";
                            string fName = String.Format("{0}{1}-{2}.{3}", medias_file_dir, rm.Value.MediaID, rm.Value.Content[i].Key, ext);
                            try
                            {
                                FileStream fsw = new FileStream(fName, FileMode.Create, FileAccess.Write);
                                fsw.Write(rm.Value.Content[i].Value, 0, rm.Value.Content[i].Value.Length);
                                fsw.Close();
                            }
                            catch (Exception ex) { };
                        };
                    };
                };
            };
            sw.WriteLine("</Document></kml>");
            sw.Close();
            fs.Close();            
            if (this.Add2Log != null) this.Add2Log("All data saved");
            if ((this.MarkerData != null) && (this.Add2Log != null))
            {
                this.Add2Log(String.Format("Creator: {0}", this.MarkerData.Creator));
                this.Add2Log(String.Format("Created: {0}", this.MarkerData.Created));
            };
        }

        /// <summary>
        ///     Trim Text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string TrimDesc(string text)
        {
            while (text.IndexOf("\r\n\r\n") >= 0) text = text.Replace("\r\n\r\n", "\r\n");
            text = text.Trim(new char[] { '\r','\n' });
            return text;
        }

        /// <summary>
        ///     Loop Records References
        /// </summary>
        /// <param name="records"></param>
        private void LoopRecords(List<Record> records)
        {
            if ((records == null) || (records.Count == 0)) return;
            foreach (Record r in records)
            {
                GetReferences(r);
                LoopRecords(r.Childs);
            };
        }

        /// <summary>
        ///     Get Record References
        /// </summary>
        /// <param name="r"></param>
        private void GetReferences(Record r)
        {
            if (r is RecBitmapReference)
            {
                RecBitmapReference rec = (RecBitmapReference)r;
                if ((rec.Parent != null) && (rec.Parent is RecCategory))
                    try { ((RecCategory)rec.Parent).Bitmap = this.Bitmaps[rec.BitmapID]; } catch { /* No Bitmap */ };
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint))
                    try { ((RecWaypoint)rec.Parent).Bitmap = this.Bitmaps[rec.BitmapID]; } catch { /* No Bitmap */ };
            };
            if (r is RecCategoryReference)
            {
                RecCategoryReference rec = (RecCategoryReference)r;
                if((rec.Parent != null) && (rec.Parent is RecWaypoint))
                {
                    RecWaypoint rw = (RecWaypoint)rec.Parent;
                    try { this.Categories[rec.CategoryID].Waypoints.Add(rw); } catch { /* No Category */ };
                };
            };            
        }

        /// <summary>
        ///     Read Source File Data
        /// </summary>
        private void Read()
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] fileData = new byte[fs.Length];
            fs.Read(fileData, 0, fileData.Length);
            fs.Close();

            if (fileData.Length != 0) 
                ReadData(ref fileData, 0, (uint)fileData.Length, RootElement);
        }

        /// <summary>
        ///     Read Block Data
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="parent"></param>
        private void ReadData(ref byte[] blockData, uint blockOffset, uint blockLength, Record parent)
        {
            uint currOffset = blockOffset;
            while (currOffset < (blockOffset + blockLength))
            {
                if (this.Add2Log != null)
                {
                    if (currOffset >= readNotifier)
                    {
                        this.Add2Log(String.Format("Reading {0}/{1} Data...", currOffset, blockData.Length));
                        readNotifier += 256000; // 256kb
                    };
                };
                uint readedLength = ReadRecordBlock(ref blockData, parent, currOffset);
                currOffset += readedLength;
            };
        }

        /// <summary>
        ///     Read Block Record Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parent"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private uint ReadRecordBlock(ref byte[] data, Record parent, uint offset)
        {
            Record rec = Record.Create(parent, offset, ref data, BitConverter.ToUInt16(data, (int)offset)); offset += 2;
            rec.RecFlags = BitConverter.ToUInt16(data, (int)offset); offset += 2;
            rec.LengthTotal = BitConverter.ToUInt32(data, (int)offset); offset += 4;
            rec.LengthMain = rec.LengthTotal;
            rec.LengthBlock = rec.LengthTotal + (uint)(rec.RecHasExtra ? 12 : 8);
            try
            {
                if (rec.RecHasExtra)
                {
                    rec.LengthMain = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                    rec.LengthExtra = rec.LengthTotal - rec.LengthMain;
                };
                rec.OffsetMain = offset; 
                rec.OffsetExtra = rec.OffsetMain + rec.LengthMain; //
                if (RecEnum<RecType>.IsDefined((RecType)rec.RecType)) // only if specified
                {
                    bool processExtras = ReadMainBlock(ref data, rec);
                    if (processExtras && rec.RecHasExtra) ReadData(ref data, rec.OffsetExtra, rec.LengthExtra, rec);
                };
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return rec.LengthBlock;
        }

        /// <summary>
        ///     Read Block Record Main Data
        /// </summary>
        /// <param name="rec"></param>
        private bool ReadMainBlock(ref byte[] data, Record rec)
        {
            if ((rec.RecType == 0) && (rec is RecHeader0)) return Read00Header1(ref data, (RecHeader0)rec);
            if ((rec.RecType == 1) && (rec is RecHeader1)) return Read01Header2(ref data, (RecHeader1)rec);
            if ((rec.RecType == 2) && (rec is RecWaypoint)) return Read02Waypoint(ref data, (RecWaypoint)rec);
            if ((rec.RecType == 3) && (rec is RecAlert)) return Read03Alert(ref data, (RecAlert)rec);                        
            if ((rec.RecType == 4) && (rec is RecBitmapReference)) return Read04BitmapReference(ref data, (RecBitmapReference)rec);
            if ((rec.RecType == 5) && (rec is RecBitmap)) return Read05Bitmap(ref data, (RecBitmap)rec);                        
            if ((rec.RecType == 6) && (rec is RecCategoryReference)) return Read06CategoryReference(ref data, (RecCategoryReference)rec);
            if ((rec.RecType == 7) && (rec is RecCategory)) return Read07Category(ref data, (RecCategory)rec);            
            if ((rec.RecType == 8) && (rec is RecArea)) return Read08Area(ref data, (RecArea)rec);            
            if ((rec.RecType == 9) && (rec is RecPOIGroup)) return Read09POIGroup(ref data, (RecPOIGroup)rec);
            if ((rec.RecType == 10) && (rec is RecComment)) return Read10Comment(ref data, (RecComment)rec);
            if ((rec.RecType == 11) && (rec is RecAddress)) return Read11Address(ref data, (RecAddress)rec);
            if ((rec.RecType == 12) && (rec is RecContact)) return Read12Contact(ref data, (RecContact)rec);
            if ((rec.RecType == 13) && (rec is RecImage)) return Read13Image(ref data, (RecImage)rec);
            if ((rec.RecType == 14) && (rec is RecDescription)) return Read14Decription(ref data, (RecDescription)rec);   
            if ((rec.RecType == 15) && (rec is RecProductInfo)) return Read15ProductInfo(ref data, (RecProductInfo)rec);
            if ((rec.RecType == 16) && (rec is RecAlertCircle)) return Read16AlertCircle(ref data, (RecAlertCircle)rec);            
            if ((rec.RecType == 17) && (rec is RecCopyright)) return Read17Copyright(ref data, (RecCopyright)rec);
            if ((rec.RecType == 18) && (rec is RecMedia)) return Read18Media(ref data, (RecMedia)rec);
            if ((rec.RecType == 19) && (rec is RecSpeedCamera)) return Read19SpeedCamera(ref data, (RecSpeedCamera)rec);
            if ((rec.RecType == 27) && (rec is RecAlertTriggerOptions)) return Read27AlertTriggerOptions(ref data, (RecAlertTriggerOptions)rec);
            return true;
        }

        private bool Read00Header1(ref byte[] data, RecHeader0 rec) // 0
        {
            uint offset = rec.OffsetMain;
            byte[] sub = new byte[6];
            Array.Copy(data, offset, sub, 0, 6);
            rec.Header = Header = Encoding.ASCII.GetString(sub);
            sub = new byte[2];
            Array.Copy(data, offset + 6, sub, 0, 2);
            rec.Version = Version = Encoding.ASCII.GetString(sub);
            uint time = BitConverter.ToUInt32(data, (int)offset + 8);
            if (time != 0xFFFFFFFF)
                rec.Created = Created = (new DateTime(1990, 1, 1)).AddSeconds(time);
            ushort slen = BitConverter.ToUInt16(data, (int)offset + 14);
            rec.Name = Name = Encoding.ASCII.GetString(data, (int)offset + 16, slen);
            return true;
        }

        private bool Read01Header2(ref byte[] data, RecHeader1 rec) // 1
        {
            uint offset = rec.OffsetMain;
            int bLen = 0;
            while (data[offset + bLen] != 0) bLen++;
            rec.Content = this.Content = Encoding.ASCII.GetString(data, (int)offset, bLen++);
            rec.CodePage = this.CodePage = BitConverter.ToUInt16(data, (int)offset + bLen + 4);
            this.Encoding = rec.Encoding;
            return true;
        }

        private bool Read02Waypoint(ref byte[] data, RecWaypoint rec) // 2
        {
            uint offset = rec.OffsetMain;
            rec.cLat = BitConverter.ToInt32(data, (int)offset); offset += 4;
            rec.cLon = BitConverter.ToInt32(data, (int)offset); offset += 4;
            offset += 3;
            uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
            int readed = 0;
            while (readed < len)
            {
                string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                {
                    string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                    rec.ShortName.Add(new KeyValuePair<string, string>(lang, text));
                }
                else
                    offset += tlen;
            };
            return true;
        }

        private bool Read03Alert(ref byte[] data, RecAlert rec) // 3
        {
            try
            {
                uint offset = rec.OffsetMain;
                rec.Proximity = BitConverter.ToUInt16(data, (int)offset);
                rec.cSpeed = BitConverter.ToUInt16(data, (int)offset + 2);
                rec.Alert = data[(int)offset + 8];
                rec.AlertType = data[(int)offset + 9];
                rec.SoundNumber = data[(int)offset + 10];
                rec.AudioAlert = data[(int)offset + 11];
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint)) ((RecWaypoint)rec.Parent).Alert = rec;
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return true;
        }      

        public bool Read04BitmapReference(ref byte[] data, RecBitmapReference rec) // 4
        {
            rec.BitmapID = BitConverter.ToUInt16(data, (int)rec.OffsetMain);
            return false;
        }

        public bool Read05Bitmap(ref byte[] data, RecBitmap rec) // 5
        {
            try
            {
                uint offset = rec.OffsetMain;
                rec.BitmapID = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                rec.Height = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                rec.Width = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                rec.LineSize = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                rec.BitsPerPixel = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                rec.Reserved9 = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                rec.ImageSize = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                rec.Reserved10 = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                rec.Palette = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                rec.TransparentColor = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                rec.Flags = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                rec.Reserved11 = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                rec.Pixels = new byte[rec.ImageSize];
                Array.Copy(data, offset, rec.Pixels, 0, rec.ImageSize); offset += rec.ImageSize;
                rec.Colors = new uint[rec.Palette];
                for (int i = 0; i < rec.Colors.Length; i++) { rec.Colors[i] = BitConverter.ToUInt32(data, (int)offset); offset += 4; };
                this.Bitmaps.Add(rec.BitmapID, rec);
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }            

        private bool Read06CategoryReference(ref byte[] data, RecCategoryReference rec) // 6
        {
            rec.CategoryID = BitConverter.ToUInt16(data, (int)rec.OffsetMain);
            return false;
        }

        private bool Read07Category(ref byte[] data, RecCategory rec) // 7
        {
            uint offset = rec.OffsetMain;
            rec.CategoryID = BitConverter.ToUInt16(data, (int)offset); offset += 2;
            uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
            int readed = 0;
            while (readed < len)
            {
                string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                {
                    string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                    rec.Category.Add(new KeyValuePair<string, string>(lang, text));
                }
                else
                    offset += tlen;
            };
            this.Categories.Add(rec.CategoryID, rec);
            return true;
        }          

        private bool Read08Area(ref byte[] data, RecArea rec) // 8
        {
            uint offset = rec.OffsetMain;
            rec.cMaxLat = BitConverter.ToInt32(data, (int)offset);
            rec.cMaxLon = BitConverter.ToInt32(data, (int)offset + 4);
            rec.cMinLat = BitConverter.ToInt32(data, (int)offset + 8);
            rec.cMinLon = BitConverter.ToInt32(data, (int)offset + 12);            
            return true;
        }      

        private bool Read09POIGroup(ref byte[] data, RecPOIGroup rec) // 9
        {
            uint offset = rec.OffsetMain;
            uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
            int readed = 0;
            while (readed < len)
            {
                string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                {
                    string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                    rec.DataSource.Add(new KeyValuePair<string, string>(lang, text));
                }
                else
                    offset += tlen;
            };

            ReadData(ref data, (uint)offset, rec.LengthMain - (uint)readed, rec);
            return true;
        }

        private bool Read10Comment(ref byte[] data, RecComment rec) // 10
        {
            try
            {
                uint offset = rec.OffsetMain;
                uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                int readed = 0;
                while (readed < len)
                {
                    string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                    if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                    {
                        string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                        rec.Comment.Add(new KeyValuePair<string, string>(lang, text));
                    }
                    else
                        offset += tlen;
                };
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint)) ((RecWaypoint)rec.Parent).Comment = rec;
                if ((rec.Parent != null) && (rec.Parent is RecCategory)) ((RecCategory)rec.Parent).Comment = rec;
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }

        private bool Read11Address(ref byte[] data, RecAddress rec) // 11
        {
            uint offset = rec.OffsetMain;
            rec.Flags = BitConverter.ToUInt16(data, (int)offset); offset += 2;
            try
            {
                if ((rec.Flags & 0x0001) == 0x0001)
                {
                    uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                    int readed = 0;
                    while (readed < len)
                    {
                        string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                        ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                        if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                        {
                            string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                            rec.aCity.Add(new KeyValuePair<string, string>(lang, text));
                        }
                        else
                            offset += tlen;
                    };
                };
                if ((rec.Flags & 0x0002) == 0x0002)
                {
                    uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                    int readed = 0;
                    while (readed < len)
                    {
                        string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                        ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                        if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                        {
                            string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                            rec.aCountry.Add(new KeyValuePair<string, string>(lang, text));
                        }
                        else
                            offset += tlen;
                    };
                };
                if ((rec.Flags & 0x0004) == 0x0004)
                {
                    uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                    int readed = 0;
                    while (readed < len)
                    {
                        string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                        ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                        if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                        {
                            string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                            rec.aState.Add(new KeyValuePair<string, string>(lang, text));
                        }
                        else
                            offset += tlen;
                    };
                };
                if ((rec.Flags & 0x0008) == 0x0008)
                {
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                    string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen;
                    rec.Postal = text;
                };
                if ((rec.Flags & 0x0010) == 0x0010)
                {
                    uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                    int readed = 0;
                    while (readed < len)
                    {
                        string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                        ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                        if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                        {
                            string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                            rec.aStreet.Add(new KeyValuePair<string, string>(lang, text));
                        }
                        else
                            offset += tlen;
                    };
                };
                if ((rec.Flags & 0x0020) == 0x0020)
                {
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                    string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen;
                    rec.House = text;
                };
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint)) ((RecWaypoint)rec.Parent).Address = rec;
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }

        private bool Read12Contact(ref byte[] data, RecContact rec) // 12
        {
            uint offset = rec.OffsetMain;
            rec.Flags = BitConverter.ToUInt16(data, (int)offset); offset += 2;
            try
            {
                if ((rec.Flags & 0x0001) == 0x0001)
                {
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                    string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen;
                    rec.Phone = text;
                };
                if ((rec.Flags & 0x0002) == 0x0002)
                {
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                    string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen;
                    rec.Phone2 = text;
                };
                if ((rec.Flags & 0x0004) == 0x0004)
                {
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                    string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen;
                    rec.Fax = text;
                };
                if ((rec.Flags & 0x0008) == 0x0008)
                {
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                    string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen;
                    rec.Email = text;
                };
                if ((rec.Flags & 0x0010) == 0x0010)
                {
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                    string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen;
                    rec.Web = text;
                };
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint)) ((RecWaypoint)rec.Parent).Contact = rec;
                if ((rec.Parent != null) && (rec.Parent is RecCategory)) ((RecCategory)rec.Parent).Contact = rec;
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }

        private bool Read13Image(ref byte[] data, RecImage rec) // 13
        {
            try
            {
                uint offset = rec.OffsetMain;
                rec.Length = BitConverter.ToUInt32(data, (int)offset + 1);
                rec.ImageData = new byte[rec.Length];
                if (rec.Length > 0)
                {
                    Array.Copy(data, (int)offset + 5, rec.ImageData, 0, rec.Length);
                    if ((rec.Parent != null) && (rec.Parent is RecWaypoint))
                        ((RecWaypoint)rec.Parent).Image = rec;
                };
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }

        private bool Read14Decription(ref byte[] data, RecDescription rec) // 14
        {
            try
            {
                uint offset = rec.OffsetMain + 1;
                uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                int readed = 0;
                while (readed < len)
                {
                    string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                    if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                    {
                        string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                        rec.Description.Add(new KeyValuePair<string, string>(lang, text));
                    }
                    else
                        offset += tlen;
                };
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint)) ((RecWaypoint)rec.Parent).Description = rec;
                if ((rec.Parent != null) && (rec.Parent is RecCategory)) ((RecCategory)rec.Parent).Description = rec;
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }

        private bool Read15ProductInfo(ref byte[] data, RecProductInfo rec) // 15
        {
            uint offset = rec.OffsetMain;
            rec.FactoryID = BitConverter.ToUInt16(data, (int)offset);
            rec.ProductID = data[(int)offset + 2];
            rec.RegionID = data[(int)offset + 3];
            rec.VendorID = data[(int)offset + 4];
            return false;
        }

        private bool Read16AlertCircle(ref byte[] data, RecAlertCircle rec) // 16
        {
            try
            {
                uint offset = rec.OffsetMain;
                rec.Count = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                rec.lat = new double[rec.Count];
                rec.lon = new double[rec.Count];
                rec.radius = new uint[rec.Count];
                for (int i = 0; i < rec.Count; i++)
                {
                    rec.lat[i] = (double)BitConverter.ToUInt32(data, (int)offset + i * 12) * 360.0 / Math.Pow(2, 32);
                    rec.lon[i] = (double)BitConverter.ToUInt32(data, (int)offset + i * 12 + 4) * 360.0 / Math.Pow(2, 32);
                    rec.radius[i] = BitConverter.ToUInt32(data, (int)offset + i * 12 + 8);
                };
                if ((rec.Parent != null) && (rec.Parent is RecAlert)) ((RecAlert)rec.Parent).AlertCircles = rec;
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }

        private bool Read17Copyright(ref byte[] data, RecCopyright rec) // 17
        {
            try
            {
                uint offset = rec.OffsetMain;
                rec.Flags1 = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                rec.Flags2 = BitConverter.ToUInt16(data, (int)offset); offset += 2; offset += 4;
                uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                int readed = 0;
                while (readed < len)
                {
                    string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                    if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                    {
                        string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                        rec.cDataSource.Add(new KeyValuePair<string, string>(lang, text));
                    }
                    else
                        offset += tlen;
                };
                len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                readed = 0;
                while (readed < len)
                {
                    string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                    if ((tlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                    {
                        string text = this.Encoding.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                        rec.cCopyrights.Add(new KeyValuePair<string, string>(lang, text));
                    }
                    else
                        offset += tlen;
                };
                this.cDataSource = rec.cDataSource;
                this.cCopyrights = rec.cCopyrights;
                if ((rec.Flags1 & 0x0400) == 0x0400)
                {
                    ushort tlen = BitConverter.ToUInt16(data, (int)offset); offset += 2; readed += 2;
                    string text = Encoding.ASCII.GetString(data, (int)offset, tlen); offset += tlen; readed += tlen;
                    rec.DeviceModel = text;
                };
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }

        private bool Read18Media(ref byte[] data, RecMedia rec) // 18
        {
            uint offset = rec.OffsetMain;
            rec.MediaID = BitConverter.ToUInt16(data, (int)offset); offset += 2;
            rec.Format = data[(int)offset];
            try
            {
                offset = rec.OffsetExtra;
                int readed = 0;
                uint len = BitConverter.ToUInt32(data, (int)offset); offset += 4;
                while (readed < len)
                {
                    string lang = Encoding.ASCII.GetString(data, (int)offset, 2); offset += 2; readed += 2;
                    uint mlen = BitConverter.ToUInt32(data, (int)offset); offset += 4; readed += 4;
                    if ((mlen > 0) && char.IsLetter(lang[0]) && char.IsLetter(lang[1]))
                    {
                        byte[] media = new byte[mlen];
                        Array.Copy(data, offset, media, 0, mlen); offset += mlen; readed += (int)mlen;
                        if ((rec.MediaID == 0x7777) && (rec.Format == 0x77))
                            try { this.MarkerData = MarkerBlock.FromBytes(media); } catch { };
                        rec.Content.Add(new KeyValuePair<string, byte[]>(lang, media));
                    }
                    else
                        offset += mlen;
                };
                Medias.Add(rec.MediaID, rec);
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }

        private bool Read19SpeedCamera(ref byte[] data, RecSpeedCamera rec) // 19
        {
            try
            {
                uint offset = rec.OffsetMain;
                byte[] buff = new byte[4];
                Array.Copy(data, (int)offset, buff, 0, 3); offset += 3;
                rec.cMaxLat = BitConverter.ToInt32(buff, 0);
                Array.Copy(data, (int)offset, buff, 0, 3); offset += 3;
                rec.cMaxLon = BitConverter.ToInt32(buff, 0);
                Array.Copy(data, (int)offset, buff, 0, 3); offset += 3;
                rec.cMinLat = BitConverter.ToInt32(buff, 0);
                Array.Copy(data, (int)offset, buff, 0, 3); offset += 3;
                rec.cMinLon = BitConverter.ToInt32(buff, 0);
                rec.Flags = data[(int)offset]; offset++;
                if (rec.Flags == 0x81) offset += 11;
                if ((rec.Flags == 0x80) || (rec.Flags > 0x81)) offset++;
                byte f10v = data[(int)offset]; offset++;
                if (rec.Flags == 0x81) offset++;
                offset += (uint)(1 + f10v);
                Array.Copy(data, (int)offset, buff, 0, 3); offset += 3;
                rec.cLat = BitConverter.ToInt32(buff, 0);
                Array.Copy(data, (int)offset, buff, 0, 3); offset += 3;
                rec.cLon = BitConverter.ToInt32(buff, 0);
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }

        private bool Read27AlertTriggerOptions(ref byte[] data, RecAlertTriggerOptions rec) // 27
        {
            try
            {
                uint offset = rec.OffsetMain;
                ushort keyv = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                if (keyv < 16)
                {
                    rec.BearingCount = (byte)(keyv & 0x07);
                    bool hasDTL = (keyv & 0x08) == 0x08;
                    if (rec.BearingCount > 0)
                    {
                        rec.BearingAngle = new ushort[rec.BearingCount];
                        rec.BearingWide = new ushort[rec.BearingCount];
                        rec.BearingBiDir = new bool[rec.BearingCount];
                        for (int i = 0; i < rec.BearingCount; i++)
                        {
                            ushort br = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                            rec.BearingAngle[i] = (ushort)(br & 0x01FF);
                            rec.BearingWide[i] = (ushort)(((br >> 9) & 0x0F) * 5);
                            rec.BearingBiDir[i] = (br & 0x2000) == 0x2000;
                        };
                    };
                    if (hasDTL) // DateTime List Block
                    {
                        ushort len = BitConverter.ToUInt16(data, (int)offset); offset += 2;
                        if (len > 0)
                        {
                            rec.DateTimeBlock = new byte[len];
                            Array.Copy(data, offset, rec.DateTimeBlock, 0, len); offset += len;
                            Read27DateTimeListBlock(rec);
                        };                        
                    };
                    if ((rec.Parent != null) && (rec.Parent is RecAlert)) ((RecAlert)rec.Parent).AlertTriggerOptions = rec;
                };
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            return false;
        }

        private static void Read27DateTime00Block(RecAlertTriggerOptions rec, byte b8Flag, ref int offset) // By Month
        {
            bool has_month_from = (b8Flag & 0x01) == 0x01;
            bool has_month_till = (b8Flag & 0x02) == 0x02;
            //bool has_time_start = (b8Flag & 0x04) == 0x04;
            //bool has_time_end   = (b8Flag & 0x08) == 0x08;
            //bool has_dof_end    = (b8Flag & 0x10) == 0x10;

            byte tfromh = 0; byte tfromm = 0; byte ttillh = 24; byte ttillm = 0; byte bydw = 0x7F;
            string line = "";

            byte mfrom = 1; byte mtill = 12;   
            if(has_month_from) mfrom = rec.DateTimeBlock[offset++];
            if(has_month_till) mtill = rec.DateTimeBlock[offset++];
            
            if ((mfrom != 1) || (mtill != 12))
                line += String.Format("on_month:{0:00}~{1:00},", mfrom, mtill);

            Read27DateTimeBlockTDOFPart(rec, ref offset, b8Flag, ref tfromh, ref tfromm, ref ttillh, ref ttillm, ref bydw);
            line += DateTimeDOFToLine(tfromh, tfromm, ttillh, ttillm, bydw);

            rec.DateTimeList.Add(line);
        }

        private static void Read27DateTime20Block(RecAlertTriggerOptions rec, byte b8Flag, ref int offset) // Dates
        {
            bool has_no_year    = (b8Flag & 0x01) == 0x01;
            bool has_day_month  = (b8Flag & 0x02) == 0x02;
            //bool has_time_start = (b8Flag & 0x04) == 0x04;
            //bool has_time_end   = (b8Flag & 0x08) == 0x08;
            //bool has_dof_end    = (b8Flag & 0x10) == 0x10;

            byte tfromh = 0; byte tfromm = 0; byte ttillh = 24; byte ttillm = 0; byte bydw = 0x7F;
            string line = "on_day:";
            
            if (has_no_year)
            {
                int mmddf = rec.DateTimeBlock[offset++] + (rec.DateTimeBlock[offset++] << 8);
                int mmddt = rec.DateTimeBlock[offset++] + (rec.DateTimeBlock[offset++] << 8);
                line += String.Format("{0:00}.{1:00}-", (mmddf >> 4) & 0x1F, mmddf & 0x0F);
                line += String.Format("{0:00}.{1:00},", (mmddt >> 4) & 0x1F, mmddt & 0x0F);
            }
            else
            {
                int ddf = rec.DateTimeBlock[offset++];
                int mmyyf = rec.DateTimeBlock[offset++] + (rec.DateTimeBlock[offset++] << 8);
                int ddt = rec.DateTimeBlock[offset++];
                int mmyyt = rec.DateTimeBlock[offset++] + (rec.DateTimeBlock[offset++] << 8);
                line += String.Format("{0:00}.{1:00}.{2:0000}-", ddf, mmyyf & 0x0F, (mmyyf >> 4) & 0x0FFF);
                line += String.Format("{0:00}.{1:00}.{2:0000},", ddt, mmyyt & 0x0F, (mmyyt >> 4) & 0x0FFF);
            };

            Read27DateTimeBlockTDOFPart(rec, ref offset, b8Flag, ref tfromh, ref tfromm, ref ttillh, ref ttillm, ref bydw);
            line += DateTimeDOFToLine(tfromh, tfromm, ttillh, ttillm, bydw);
            
            rec.DateTimeList.Add(line);
        }

        private static void Read27DateTime40Block(RecAlertTriggerOptions rec, byte b8Flag, ref int offset) // Day of year by week
        {
            bool has_dof_start  = (b8Flag & 0x01) == 0x01;
            bool has_day_oyear  = (b8Flag & 0x02) == 0x02;
            //bool has_time_start = (b8Flag & 0x04) == 0x04;
            //bool has_time_end   = (b8Flag & 0x08) == 0x08;
            //bool has_dof_end   = (b8Flag & 0x10) == 0x10;

            byte tfromh = 0; byte tfromm = 0; byte ttillh = 24; byte ttillm = 0; byte bydw = 0x7F;
            string line = "";
            
            if (has_day_oyear)
            {
                line = "on_day:";
                ushort ddf = (ushort)(rec.DateTimeBlock[offset++] + (rec.DateTimeBlock[offset++] << 8));
                ushort ddt = (ushort)(rec.DateTimeBlock[offset++] + (rec.DateTimeBlock[offset++] << 8));
                line += String.Format("{0:000}~{1:000},", ddf, ddt);
            };

            if (has_dof_start)  bydw = rec.DateTimeBlock[offset++];

            Read27DateTimeBlockTDOFPart(rec, ref offset, b8Flag, ref tfromh, ref tfromm, ref ttillh, ref ttillm, ref bydw);
            line += DateTimeDOFToLine(tfromh, tfromm, ttillh, ttillm, bydw);
            
            rec.DateTimeList.Add(line);
        }

        private static void Read27DateTime60Block(RecAlertTriggerOptions rec, byte b8Flag, ref int offset) // day of month
        {
            bool unset_flag     = (b8Flag & 0x01) == 0x01;
            bool has_day_omonth = (b8Flag & 0x02) == 0x02;
            //bool has_time_start = (b8Flag & 0x04) == 0x04;
            //bool has_time_end   = (b8Flag & 0x08) == 0x08;
            //bool has_dof_end    = (b8Flag & 0x10) == 0x10;

            byte tfromh = 0; byte tfromm = 0; byte ttillh = 24; byte ttillm = 0; byte bydw = 0x7F;
            string line = "on_day:";
            
            if (has_day_omonth)
                line += String.Format("{0:00}-{1:00},", rec.DateTimeBlock[offset++], rec.DateTimeBlock[offset++]);
            
            Read27DateTimeBlockTDOFPart(rec, ref offset, b8Flag, ref tfromh, ref tfromm, ref ttillh, ref ttillm, ref bydw);
            line += DateTimeDOFToLine(tfromh, tfromm, ttillh, ttillm, bydw);
            
            rec.DateTimeList.Add(line);
        }

        private static void Read27DateTimeC0Block(RecAlertTriggerOptions rec, byte b8Flag, ref int offset) // week of month
        {
            bool unset_flag      = (b8Flag & 0x01) == 0x01;
            bool has_week_omonth = (b8Flag & 0x02) == 0x02;
            //bool has_time_start  = (b8Flag & 0x04) == 0x04;
            //bool has_time_end    = (b8Flag & 0x08) == 0x08;
            //bool has_dof_end     = (b8Flag & 0x10) == 0x10;

            byte tfromh = 0; byte tfromm = 0; byte ttillh = 24; byte ttillm = 0; byte bydw = 0x7F;
            string line = "on_week:";
                        
            if (has_week_omonth)
                line += String.Format("{0:0}-{1:0},", rec.DateTimeBlock[offset++], rec.DateTimeBlock[offset++]);
            
            Read27DateTimeBlockTDOFPart(rec, ref offset, b8Flag, ref tfromh, ref tfromm, ref ttillh, ref ttillm, ref bydw);
            line += DateTimeDOFToLine(tfromh, tfromm, ttillh, ttillm, bydw);
            
            rec.DateTimeList.Add(line);
        }

        private static void Read27DateTimeE0Block(RecAlertTriggerOptions rec, byte b8Flag, ref int offset) // week of year
        {
            bool unset_flag     = (b8Flag & 0x01) == 0x01;
            bool has_week_oyear = (b8Flag & 0x02) == 0x02;
            //bool has_time_start = (b8Flag & 0x04) == 0x04;
            //bool has_time_end   = (b8Flag & 0x08) == 0x08;
            //bool has_dof_end    = (b8Flag & 0x10) == 0x10;

            byte tfromh = 0; byte tfromm = 0; byte ttillh = 24; byte ttillm = 0; byte bydw = 0x7F;
            string line = "on_week:";            
            
            if (has_week_oyear)
                line += String.Format("{0:00}~{1:00},", rec.DateTimeBlock[offset++], rec.DateTimeBlock[offset++]);
            
            Read27DateTimeBlockTDOFPart(rec, ref offset, b8Flag, ref tfromh, ref tfromm, ref ttillh, ref ttillm, ref bydw);
            line += DateTimeDOFToLine(tfromh, tfromm, ttillh, ttillm, bydw);
            
            rec.DateTimeList.Add(line);
        }

        /// <summary>
        ///     Read Time & Day of Week Part of DateTime Block
        /// </summary>
        /// <param name="rec">Record</param>
        /// <param name="offset">offset</param>
        /// <param name="b8Flag">block08 flags</param>
        /// <param name="tfromh">Hour from</param>
        /// <param name="tfromm">Minutes from</param>
        /// <param name="ttillh">Hour till</param>
        /// <param name="ttillm">Minutes till</param>
        /// <param name="bydw">Day of week masked</param>
        private static void Read27DateTimeBlockTDOFPart(RecAlertTriggerOptions rec, ref int offset, byte b8Flag, ref byte tfromh, ref byte tfromm, ref byte ttillh, ref byte ttillm, ref byte bydw)
        {
            bool has_time_start = (b8Flag & 0x04) == 0x04;
            bool has_time_end   = (b8Flag & 0x08) == 0x08;
            bool has_dof_end    = (b8Flag & 0x10) == 0x10;

            if (has_time_start)
            {
                tfromh = rec.DateTimeBlock[offset++];
                if ((tfromh & 0x80) == 0x80) 
                { 
                    tfromh = (byte)(tfromh & 0x7F); 
                    tfromm = rec.DateTimeBlock[offset++]; 
                };
            };

            if (has_time_end)
            {
                ttillh = rec.DateTimeBlock[offset++];
                if ((ttillh & 0x80) == 0x80) 
                { 
                    ttillh = (byte)(ttillh & 0x7F);
                    ttillm = rec.DateTimeBlock[offset++]; 
                };
            };

            if (has_dof_end) 
                bydw = rec.DateTimeBlock[offset++];
        }

        /// <summary>
        ///     Time & Day of Week to text
        /// </summary>
        /// <param name="tfromh">Hour from</param>
        /// <param name="tfromm">Minutes from</param>
        /// <param name="ttillh">Hour till</param>
        /// <param name="ttillm">Minutes till</param>
        /// <param name="bydw">Day of week masked</param>
        /// <returns></returns>
        private static string DateTimeDOFToLine(byte tfromh, byte tfromm, byte ttillh, byte ttillm, byte bydw)
        {
            string line = "";

            line += String.Format("{0:00}:{1:00}..", tfromh, tfromm);
            line += String.Format("{0:00}:{1:00}", ttillh, ttillm);

            if (bydw != 0x7F)
                for (int z = 0; z < 7; z++)
                {
                    int vz = (int)Math.Pow(2, z);
                    if ((bydw & vz) == vz)
                    {
                        if (LOCALE_LANGUAGE == "RU")
                            line += String.Format(",{0}", (new string[] { "сб", "пт", "чт", "ср", "вт", "по", "вс" })[z]);
                        else
                            line += String.Format(",{0}", (new string[] { "sa", "fr", "th", "we", "tu", "mo", "su" })[z]);
                    };
                };
            return line;
        }

        /// <summary>
        ///     Part of 27 Record Type (RecAlertTriggerOptions)
        /// </summary>
        /// <param name="rec"></param>
        private static void Read27DateTimeListBlock(RecAlertTriggerOptions rec)
        {
            int offset = 0;
            while (true)
            {
                try
                {
                    byte b8Type = rec.DateTimeBlock[offset++];
                    byte b8Flag = rec.DateTimeBlock[offset++];
                    bool is_last_entry = (b8Flag & 0x80) == 0x80;
                    if (b8Type == 0x00) Read27DateTime00Block(rec, b8Flag, ref offset); // By Month
                    if (b8Type == 0x20) Read27DateTime20Block(rec, b8Flag, ref offset); // Dates                    
                    if (b8Type == 0x40) Read27DateTime40Block(rec, b8Flag, ref offset); // Day of year by week
                    if (b8Type == 0x60) Read27DateTime60Block(rec, b8Flag, ref offset); // day of month
                    if (b8Type == 0xC0) Read27DateTimeC0Block(rec, b8Flag, ref offset); // week of month
                    if (b8Type == 0xE0) Read27DateTimeE0Block(rec, b8Flag, ref offset); // week of year
                    if (is_last_entry || (offset >= rec.DateTimeBlock.Length)) break;
                }
                catch (Exception ex)
                {
                    rec.ReadError = ex;
                    break;
                };
            };
        } // part of 27

        /// <summary>
        ///     Get Color from number
        /// </summary>
        /// <param name="value">number</param>
        /// <returns></returns>
        private static Color ColorFromUint(uint value)
        {
            return Color.FromArgb((int)((value >> 0) & 0xFF), (int)((value >> 8) & 0xFF), (int)((value >> 16) & 0xFF));
        }
    }

    /// <summary>
    ///     GPI Writer
    /// </summary>
    public class GPIWriter
    {        
        /// <summary>
        ///     Transliteration unit
        /// </summary>
        private Translitter ml = new Translitter();
        /// <summary>
        ///     Skip Alert Block In Description
        /// </summary>
        private static string rxskip = @"(?:alert_[\w]+=.+)|(?:name\:\w\w=.+)";
        /// <summary>
        ///     Description in Description Block
        /// </summary>
        private static string rxdesc = @"(?<desc>desc\:(?<lang>\w\w)=(?<value>[\w\W]+?)\r\n\r\n)";
        /// <summary>
        ///     Comment in Description Block
        /// </summary>
        private static string rxcomm = @"(?:comment=(?<comment>.+))";
        /// <summary>
        ///     Comment by lang in Description Block
        /// </summary>
        private static string rxcmln = @"(?:comm\:(?<lang>\w\w)=(?<comment>.+))";

        /// <summary>
        ///     Address in Description Block
        /// </summary>
        private static string[] rxaddr = new string[] {
                @"(?:addr_city=(?<value>.+))",
                @"(?:addr_country=(?<value>.+))",
                @"(?:addr_state=(?<value>.+))",
                @"(?:addr_postal=(?<value>.+))",
                @"(?:addr_street=(?<value>.+))",
                @"(?:addr_house=(?<value>.+))"};
        /// <summary>
        ///     Address by lang in Description Block
        /// </summary>
        private static string[] rxadln = new string[] {
                @"(?:addr_city\:(?<lang>\w\w)=(?<value>.+))",
                @"(?:addr_country\:(?<lang>\w\w)=(?<value>.+))",
                @"(?:addr_state\:(?<lang>\w\w)=(?<value>.+))",
                @"(?:addr_postal\:(?<lang>\w\w)=(?<value>.+))",
                @"(?:addr_street\:(?<lang>\w\w)=(?<value>.+))",
                @"(?:addr_house\:(?<lang>\w\w)=(?<value>.+))"};
        /// <summary>
        ///     Contacts in Description Block
        /// </summary>
        private static string[] rxcont = new string[] {
                @"(?:contact_phone=(?<value>.+))",
                @"(?:contact_phone2=(?<value>.+))",
                @"(?:contact_fax=(?<value>.+))",
                @"(?:contact_email=(?<value>.+))",
                @"(?:contact_web=(?<value>.+))"};

        /// <summary>
        ///     GPI File Name
        /// </summary>
        public string Name = "Exported Data";
        /// <summary>
        ///     GPI Content Data Source
        /// </summary>
        public string DataSource = "KMZRebuilder"; 
        /// <summary>
        ///     Store POI Description Text
        /// </summary>
        public bool StoreDescriptions = true; // true is default
        /// <summary>
        ///     Store POI Alert (gets from description)
        /// </summary>
        public bool StoreAlerts = false; // false is default
        /// <summary>
        ///     Store POI Images as is (in JPEG format, no resize)
        /// </summary>
        public bool StoreImagesAsIs = false; // false is default
        /// <summary>
        ///     Save text only in local language
        /// </summary>
        public bool StoreOnlyLocalLang = false;
        /// <summary>
        ///     Save Descriptions only in local language
        /// </summary>
        public bool StoreOnlyLocalLangDescriptions = false;
        /// <summary>
        ///     Save Comments only in local language
        /// </summary>
        public bool StoreOnlyLocalLangComments = false;
        /// <summary>
        ///     Save Address only in local language
        /// </summary>
        public bool StoreOnlyLocalLangAddress = false;
        /// <summary>
        ///     By Default Alert in POI is on (if not specified)
        /// </summary>
        public bool DefaultAlertIsOn = true; // try is default
        /// <summary>
        ///     By Default Alert in POI is proxinity (if not specified)
        /// </summary>
        public string DefaultAlertType = "proximity"; // proximity is default
        /// <summary>
        ///     By Default Alert in POI is plugn sound (if not specified)
        /// </summary>
        public string DefaultAlertSound = "4"; // 4 is default
        /// <summary>
        ///     Max Alert DateTime Triggers Count (1..32)
        /// </summary>
        public byte MaxAlertDateTimeCount = 16;
        /// <summary>
        ///     Source KML file (for relative image & sounds paths)
        /// </summary>
        public string SourceKMLfile = null;
        /// <summary>
        ///     Max Image Width/Height (22 is def, 16..48)
        /// </summary>
        public byte MaxImageSide = 22; // 22 is default, 16 min, 48 max
        /// <summary>
        ///     Bitmap Transparent Color (#FEFEFE or #FF00FF)
        /// </summary>
        public Color TransColor = Color.FromArgb(0xFE, 0xFE, 0xFE); // Color.FromArgb(0xFE,0xFE,0xFE); // Almost white
        /// <summary>
        ///     Analyze OSM Tags in Description
        /// </summary>
        public bool AnalyzeOSMTags = true;

        private byte formatVer = 1;
        public byte FormatVersion { get { return formatVer; } set { formatVer = value; if (formatVer > 1) formatVer = 1; } }

        /// <summary>
        ///     POI Bounds: Min Lat
        /// </summary>
        private double MinLat = -90;
        /// <summary>
        ///     POI Bounds: Max Lat
        /// </summary>
        private double MaxLat = 90;
        /// <summary>
        ///     POI Bounds: Min Lon
        /// </summary>
        private double MinLon = -180;
        /// <summary>
        ///     POI Bounds: Max Lon
        /// </summary>
        private double MaxLon = 180;

        /// <summary>
        ///     Total POIs count
        /// </summary>
        public uint TotalPOIs { get { return TotalPoints; } }
        /// <summary>
        ///     Total POIs count
        /// </summary>
        private uint TotalPoints = 0;

        #region SavedCounters
        private uint savedAddresses = 0;
        public uint SavedAddresses { get { return savedAddresses; } }
        private uint savedAlerts = 0;
        public uint SavedAlerts { get { return savedAlerts; } }
        private uint savedAreas = 0;
        public uint SavedAreas { get { return savedAreas; } }
        private uint savedBearings = 0;
        public uint SavedBearings { get { return savedBearings; } }
        private uint savedBearingBlocks = 0;
        public uint SavedBearingBlocks { get { return savedBearingBlocks; } }
        private uint savedBitmaps = 0;
        public uint SavedBitmaps { get { return savedBitmaps; } }
        private uint savedCategories = 0;
        public uint SavedCategories { get { return savedCategories; } }
        private uint savedCircleBlocks = 0;
        public uint SavedCircleBlocks { get { return savedCircleBlocks; } }
        private uint savedCircles = 0;
        public uint SavedCircles { get { return savedCircles; } }
        private uint savedContacts = 0;
        public uint SavedContacts { get { return savedContacts; } }
        private uint savedComments = 0;
        public uint SavedComments { get { return savedComments; } }
        private uint savedDescriptions = 0;
        public uint SavedDescriptions { get { return savedDescriptions; } }
        private uint savedImages = 0;
        public uint SavedImages { get { return savedImages; } }
        private uint savedMedias = 0;
        public uint SavedMedias { get { return savedMedias; } }
        private uint savedPoints = 0;
        public uint SavedPoints { get { return savedPoints; } }
        private uint savedTriggerBlocks = 0;
        public uint SavedTriggerBlocks { get { return savedTriggerBlocks; } }
        private uint savedTriggerDTBlocks = 0;
        public uint SavedTriggerDTBlocks { get { return savedTriggerDTBlocks; } }
        private uint savedTriggerDTs = 0;
        public uint SavedTriggerDTs { get { return savedTriggerDTs; } }
        #endregion SavedCounters

        /// <summary>
        ///     List of POI Categories (to store)
        /// </summary>
        private List<string> Categories = new List<string>();
        /// <summary>
        ///     List of POI Styles (to store bitmaps)
        /// </summary>
        private List<string> Styles = new List<string>();
        /// <summary>
        ///     List of POI Sounds (to store media)
        /// </summary>
        private List<string> MP3s = new List<string>();
        /// <summary>
        ///     POI List splitted by areas (each degree is an area)
        /// </summary>
        private Dictionary<uint, List<POI>> POIs = new Dictionary<uint, List<POI>>();
        /// <summary>
        ///     POI Bitmap/Image from POI style (by kml style ref)
        /// </summary>
        private Dictionary<string, Image> Images = new Dictionary<string, Image>();

        public GPIWriter() { }
        public GPIWriter(string source_kml_file) { SourceKMLfile = source_kml_file; }

        /// <summary>
        ///     Origin File Description
        /// </summary>
        public string Description;

        /// <summary>
        ///     Clear All Added Data
        /// </summary>
        public void Clear()
        {
            TotalPoints = 0;
            ResetCounters();

            Categories.Clear();
            Styles.Clear();
            MP3s.Clear();
            POIs.Clear();
            Images.Clear();            
        }

        /// <summary>
        ///     Add POI
        /// </summary>
        /// <param name="category">Category/Layer</param>
        /// <param name="name">POI Name</param>
        /// <param name="description">POI Description (could contains alert, comment, address, contact)</param>
        /// <param name="style">POI style for (specified for image)</param>
        /// <param name="lat">Latitude</param>
        /// <param name="lon">Longitude</param>
        public void AddPOI(string category, string name, string description, string style, double lat, double lon)
        {            
            // Rebound
            if (Categories.Count == 0) { MinLat = 90; MaxLat = -90; MinLon = 180; MaxLon = -180; };

            POI poi = new POI(name, lat, lon);
            if (!String.IsNullOrEmpty(description))
            {
                if (this.AnalyzeOSMTags) AnalyzeDescription.OSM(ref description);

                { // GET COMMENT
                    { // full comment
                        Match mx = ((new Regex(rxcomm)).Match(description));
                        if (mx.Success)
                        {
                            description = description.Replace(mx.Value, "");
                            poi.comment = TrimDesc(mx.Groups["comment"].Value);
                        };
                    };
                    { // by lang comment
                        MatchCollection mc = (new Regex(rxcmln)).Matches(description);
                        foreach (Match mx in mc)
                        {
                            description = description.Replace(mx.Value, "");
                            if (mx.Groups["lang"].Value == GPIReader.LOCALE_LANGUAGE.ToLower())
                                poi.comment = TrimDesc(mx.Groups["comment"].Value);
                        };
                    };
                };
                { // GET ADDRESS
                    bool hasad = false; string[] addr = new string[6];
                    { // full address
                        Match mx;
                        for (int i = 0; i < 6; i++)
                        {
                            mx = (new Regex(rxaddr[i])).Match(description);
                            if (mx.Success)
                            {
                                description = description.Replace(mx.Value, "");
                                addr[i] = TrimDesc(mx.Groups["value"].Value);
                                hasad = true;
                            };
                        };
                    };
                    { // by lang address
                        MatchCollection mc;
                        for (int i = 0; i < 6; i++)
                        {
                            mc = (new Regex(rxadln[i])).Matches(description);
                            foreach (Match mx in mc)
                            {
                                description = description.Replace(mx.Value, "");
                                if (mx.Groups["lang"].Value == GPIReader.LOCALE_LANGUAGE.ToLower())
                                {
                                    addr[i] = TrimDesc(mx.Groups["value"].Value);
                                    hasad = true;
                                };
                            };
                        };
                    };
                    if (hasad) poi.addr = addr;
                };
                { // GET CONTACTS
                    bool hascon = false; string[] conts = new string[5];
                    Match mx;
                    for (int i = 0; i < 5; i++)
                    {
                        mx = (new Regex(rxcont[i])).Match(description);
                        if (mx.Success)
                        {
                            description = description.Replace(mx.Value, "");
                            conts[i] = TrimDesc(mx.Groups["value"].Value);
                            hascon = true;
                        };
                    };
                    if (hascon)
                        poi.contacts = conts;

                };
                description = description.TrimStart(new char[] { '\r', '\n' });
            };
            if (StoreAlerts && (Regex.IsMatch(description, @"alert_[\w]+=.+", RegexOptions.None)))
            {
                poi.alert = description;
                if (!String.IsNullOrEmpty(poi.alert))
                {
                    Regex rx = new Regex(@"alert_sound=(?<sound>.+)", RegexOptions.None);
                    Match mx = rx.Match(poi.alert);
                    if (mx.Success)
                    {
                        string fName = mx.Groups["sound"].Value.Trim('\r');
                        if (!MP3s.Contains(fName)) MP3s.Add(fName);
                    };
                };
            };  
            if (StoreDescriptions && (!String.IsNullOrEmpty(description)))
            {
                poi.description = (new Regex(rxskip, RegexOptions.None)).Replace(description, "").TrimStart(new char[] { '\r', '\n' });
                if (!String.IsNullOrEmpty(poi.description))
                {
                    MatchCollection mc = (new Regex(rxdesc)).Matches(poi.description);
                    foreach (Match mx in mc)
                        if (mx.Groups["lang"].Value != GPIReader.LOCALE_LANGUAGE.ToLower())
                            poi.description = poi.description.Replace(mx.Value, "");
                        else
                            poi.description = poi.description.Replace(mx.Value, mx.Groups["value"].Value);
                };
                poi.description = TrimDesc(poi.description);
            };                 

            // Get Category Index
            poi.cat = Categories.IndexOf(category);
            if (poi.cat < 0) { poi.cat = Categories.Count; Categories.Add(category); };

            // Get Style Index
            poi.sty = Styles.IndexOf(style);
            if (poi.sty < 0) { poi.sty = Styles.Count; Styles.Add(style); };

            // Bounds
            if (lat < MinLat) MinLat = lat;
            if (lat > MaxLat) MaxLat = lat;
            if (lon < MinLon) MinLon = lon;
            if (lon > MaxLon) MaxLon = lon;

            // Zone
            uint zone = LatLonToZone(lat, lon);
            if (!POIs.ContainsKey(zone)) POIs.Add(zone, new List<POI>());
            POIs[zone].Add(poi);
            TotalPoints++;
        }

        /// <summary>
        ///     Add Image
        /// </summary>
        /// <param name="style">POI style (specified for image)</param>
        /// <param name="im">Preloaded Image</param>
        public void AddImage(string style, Image im)
        {            
            if (Images.ContainsKey(style))
                Images[style] = im;
            else
                Images.Add(style, im);
        }

        /// <summary>
        ///     Trim Text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string TrimDesc(string text)
        {
            while (text.IndexOf("\r\n\r\n") >= 0) text = text.Replace("\r\n\r\n", "\r\n");
            text = text.Trim(new char[] { '\r', '\n' });
            return text;
        }

        /// <summary>
        ///     Convert Lat & Lon to Area
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        private uint LatLonToZone(double lat, double lon)
        {
            short la = (short)lat;
            short lo = (short)lon;
            List<byte> lalo = new List<byte>();
            lalo.AddRange(BitConverter.GetBytes(la));
            lalo.AddRange(BitConverter.GetBytes(lo));
            return BitConverter.ToUInt32(lalo.ToArray(), 0);
        }

        /// <summary>
        ///     Convert Area to Lat & Lon
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        private void ZoneToLatLon(uint zone, out double lat, out double lon)
        {
            byte[] lalo = BitConverter.GetBytes(zone);
            lat = BitConverter.ToInt16(lalo, 0);
            lon = BitConverter.ToInt16(lalo, 2);
        }

        private void ResetCounters()
        {
            this.savedAddresses = 0;
            this.savedAlerts = 0;
            this.savedAreas = 0;
            this.savedBearings = 0;
            this.savedBearingBlocks = 0;
            this.savedBitmaps = 0;
            this.savedCategories = 0;
            this.savedCircleBlocks = 0;
            this.savedCircles = 0;
            this.savedContacts = 0;
            this.savedComments = 0;
            this.savedDescriptions = 0;
            this.savedImages = 0;
            this.savedMedias = 0;
            this.savedPoints = 0;
            this.savedTriggerBlocks = 0;
            this.savedTriggerDTBlocks = 0;
            this.savedTriggerDTs = 0;
        }

        /// <summary>
        ///     Save to GPI file
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <param name="Add2Log">Add To Log Procedure</param>
        public void Save(string fileName)
        {
            Save(fileName, null);
        }
        
        /// <summary>
        ///     Save to GPI file
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <param name="Add2Log">Add To Log Procedure</param>
        public void Save(string fileName, GPIReader.Add2LogProc Add2Log)
        {
            ResetCounters();

            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            byte[] block;
            if(Add2Log != null)
                Add2Log(String.Format("Writing POI file, version {0:00}", this.formatVer));
            // Header0
            { 
                block = Get00Header0Block().Data;
                fs.Write(block, 0, block.Length);
            };
            // Header1
            { 
                block = Get01Header1Block().Data;
                fs.Write(block, 0, block.Length);
            };
            // POIs
            {                
                block = Get09POIGroupBlock().Data;
                fs.Write(block, 0, block.Length);
                if (Add2Log != null)
                {
                    Add2Log(String.Format("Saved {0}/{1} POIs with {2} Images", this.savedPoints, this.TotalPoints, this.savedImages));
                    Add2Log(String.Format(".. in {0} Cats & {1} Areas", this.savedCategories, this.savedAreas));
                    Add2Log(String.Format(".. of {0} Bmps & {1} Medias", this.savedBitmaps, this.savedMedias));
                    Add2Log(String.Format(".. w/ {0} addr, {1} cont, {2} comms, {3} descs", this.savedAddresses, this.savedContacts, this.savedComments, this.savedDescriptions));
                    Add2Log(String.Format(".. w/ {0} alerts & {1} circles in {2} POIs", this.savedAlerts, this.savedCircles, this.savedCircleBlocks));
                    Add2Log(String.Format(".. w/ {0} triggers:", this.savedTriggerBlocks));
                    Add2Log(String.Format("....  - {0} bearings in {1} POIs", this.savedBearings, this.savedBearingBlocks));
                    Add2Log(String.Format("....  - {0} dtimes in {1} POIs", this.savedTriggerDTs, this.savedTriggerDTBlocks));
                };
            };
            // Footer
            { 
                block = GetFooter().Data;
                fs.Write(block, 0, block.Length);
            };
            fs.Close();
            if (Add2Log != null) Add2Log(String.Format("{0} writed",Path.GetFileName(fileName)));
        }

        /// <summary>
        ///     POI OBJECT
        /// </summary>
        private class POI
        {
            public string name;
            public string description;
            public string comment; // predefined from desc
            public double lat;
            public double lon;
            public int cat;
            public int sty;
            public string alert; // predefined from desc
            public string[] addr; // predefined from desc;
            public string[] contacts; // predefined from desc

            public POI() { }
            public POI(string name, double lat, double lon) { this.name = name; this.description = null; this.lat = lat; this.lon = lon; }
            public POI(string name, string description, double lat, double lon) { this.name = name; this.description = description; this.lat = lat; this.lon = lon; }
        }

        /// <summary>
        ///     GPI Record Block to Write
        /// </summary>
        private class FileBlock
        {
            public ushort bType = 0;
            public List<byte> MainData = new List<byte>();
            public List<byte> ExtraData = new List<byte>();
            public byte[] Data
            {
                get
                {
                    List<byte> res = new List<byte>();
                    res.AddRange(BitConverter.GetBytes(bType));
                    if(ExtraData.Count > 0)
                        res.AddRange(BitConverter.GetBytes((ushort)0x08)); // Has Extra
                    else
                        res.AddRange(BitConverter.GetBytes((ushort)0x00)); // No Extra
                    res.AddRange(BitConverter.GetBytes(((uint)(MainData.Count + ExtraData.Count))));
                    if(ExtraData.Count > 0)
                        res.AddRange(BitConverter.GetBytes(((uint)(MainData.Count))));
                    res.AddRange(MainData);
                    if (ExtraData.Count > 0)
                        res.AddRange(ExtraData);
                    return res.ToArray();
                }
            }            
        }        
        
        /// <summary>
        ///     Fill 27 Block (AlertTriggerOptions) of Bearing List
        /// </summary>
        /// <param name="poi"></param>
        /// <param name="fb27"></param>
        /// <param name="count"></param>
        private void FillTriggerBearings(POI poi, FileBlock fb27, ref ushort count)
        {
            MatchCollection mc = (new Regex(@"(?:alert_bearing=(?<value>.+))", RegexOptions.None)).Matches(poi.alert);
            foreach (Match mx in mc)
            {
                string bal = mx.Groups["value"].Value.Trim('\r');
                if (String.IsNullOrEmpty(bal)) continue;
                string[] baa = bal.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (String.IsNullOrEmpty(baa[0])) continue;

                ushort bearing = 0;
                if (!ushort.TryParse(baa[0], out bearing)) continue;

                byte wide5 = 5; // wide 0..75 devided by 5 (def: 25 degrees)
                bool bidir = false; // def: onedir
                if (baa.Length > 1)
                {
                    for (int z = 1; z < Math.Min(3, baa.Length); z++)
                    {
                        byte val;
                        if ((char.IsDigit(baa[z][0])) && (byte.TryParse(baa[z], out val)))
                        {
                            if (val > 75) val = 75;
                            wide5 = (byte)((val / 5) & 0x0F);
                        };
                        if (baa[z].ToLower().Substring(0,1) == "b")
                            bidir = true;
                    };
                };
                bearing += (ushort)(wide5 << 9);
                if (bidir) bearing += 0x2000;
                fb27.MainData.AddRange(BitConverter.GetBytes(((ushort)(bearing))));
                count++;

                if (count == 7) break;
            };

            if (count > 0)
            {
                this.savedBearingBlocks++;
                this.savedBearings += (uint)count;
            };
        } // Part of trigger block

        /// <summary>
        ///     Get DateTime OnDay Block - part of 27 Block (AlertTriggerOptions)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static byte[] GetDateTimeBlockOnDay(string text) // DateTime List Entry
        {
            List<byte> curr = new List<byte>();

            int rFrom, rTill; byte[] tFrom, tTill; byte dof; int[] dFrom, dTill;            
            bool hr = HasRange(text, 1, 366, out rFrom, out rTill);
            bool hg = HasDates(text, 1, 31, out dFrom, out dTill);
            bool ht = HasTime(text, out tFrom, out tTill);
            bool hd = HasDof(text, out dof);
            
            if (hr && hg) return null;

            //// returned as day of year
            //if ((!hr) && (!hg) && (!hd))
            //{
            //    if (!ht) return null;
            //    curr.Add(0); // By Month
            //    curr.Add((byte)(/*has_time_start*/(ht ? 0x04 : 0) + /*has_time_end*/0x08 + /*has_dof_end*/(hd ? 0x10 : 0))); // Flags
            //    curr.AddRange(tFrom);
            //    curr.AddRange(tTill);
            //    return curr.ToArray();
            //};

            if (hr || ((!hr) && (!hg)))
            {
                curr.Add(0x40); // day of year
                if (!hr)
                {
                    curr.Add((byte)(/*has_dof_start*/0x01 + /*has_time_start*/(ht ? 0x04 : 0) + /*has_time_end*/0x08)); // Flags
                    curr.Add(dof);
                    if (ht) curr.AddRange(tFrom);
                    curr.AddRange(tTill);
                }
                else
                {
                    curr.Add((byte)(/*has_day_oyear*/0x02 + /*has_time_start*/(ht ? 0x04 : 0) + /*has_time_end*/0x08 + /*has_dof_end*/(hd ? 0x10 : 0))); // Flags
                    curr.AddRange(BitConverter.GetBytes((ushort)rFrom));
                    curr.AddRange(BitConverter.GetBytes((ushort)rTill));
                    if (ht) curr.AddRange(tFrom);
                    curr.AddRange(tTill);
                    if (hd) curr.Add(dof);
                };
                return curr.ToArray();
            };
            if (hg)
            {
                if ((dFrom[1] == 0) || (dTill[1] == 0))
                {
                    curr.Add(0x60); // day of month
                    curr.Add((byte)(/*has_day_omonth*/0x02 + /*has_time_start*/(ht ? 0x04 : 0) + /*has_time_end*/0x08 + /*has_dof_end*/(hd ? 0x10 : 0)));
                    curr.Add((byte)dFrom[0]);
                    curr.Add((byte)dTill[0]);
                    if (ht) curr.AddRange(tFrom);
                    curr.AddRange(tTill);
                    if (hd) curr.Add(dof);
                    return curr.ToArray();
                }
                else
                {
                    curr.Add(0x20); // Dates
                    curr.Add((byte)(/*has_no_year*/((dFrom[2] == 0) || (dTill[2] == 0) ? 0x01 : 0) + /*has_day_month*/0x02 + /*has_time_start*/(ht ? 0x04 : 0) + /*has_time_end*/0x08 + /*has_dof_end*/(hd ? 0x10 : 0)));
                    if ((dFrom[2] == 0) || (dTill[2] == 0))
                    {
                        ushort mmddf = (ushort)(dFrom[1] + (dFrom[0] << 4));
                        ushort mmddt = (ushort)(dTill[1] + (dTill[0] << 4));
                        curr.AddRange(BitConverter.GetBytes(mmddf));
                        curr.AddRange(BitConverter.GetBytes(mmddt));
                    }
                    else
                    {
                        ushort mmyyf = (ushort)(dFrom[1] + (dFrom[2] << 4));
                        curr.Add((byte)dFrom[0]);
                        curr.AddRange(BitConverter.GetBytes(mmyyf));
                        ushort mmyyt = (ushort)(dTill[1] + (dTill[2] << 4));
                        curr.Add((byte)dTill[0]);
                        curr.AddRange(BitConverter.GetBytes(mmyyt));
                    };
                    if (ht) curr.AddRange(tFrom);
                    curr.AddRange(tTill);
                    if (hd) curr.Add(dof);
                    return curr.ToArray();
                };
            };

            return null;
        }

        /// <summary>
        ///     Get DateTime onWeek Block - part of 27 Block (AlertTriggerOptions)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static byte[] GetDateTimeBlockOnWeek(string text) // DateTime List Entry
        {
            List<byte> curr = new List<byte>();

            int rFrom, rTill; byte[] tFrom, tTill; byte dof; int[] dFrom, dTill;
            bool hr = HasRange(text, 1, 52, out rFrom, out rTill);
            bool hg = HasDates(text, 1, 5, out dFrom, out dTill);
            bool ht = HasTime(text, out tFrom, out tTill);
            bool hd = HasDof(text, out dof);
            
            if ((!hr) && (!hg)) return null;
            if (hr == hg) return null;
                                    
            if (hr) curr.Add(0xE0); // week of year
            if (hg) curr.Add(0xC0); // week of month
            curr.Add((byte)(/*has_week*/0x02 + /*has_time_start*/(ht ? 0x04 : 0) + /*has_time_end*/0x08 + /*has_dof_end*/(hd ? 0x10 : 0))); // Flags
            if (hr) { curr.Add((byte)rFrom); curr.Add((byte)rTill); };
            if (hg) { curr.Add((byte)dFrom[0]); curr.Add((byte)dTill[0]); };
            if (ht) curr.AddRange(tFrom);
            curr.AddRange(tTill);
            if (hd) curr.Add(dof);
            return curr.ToArray();
        }

        /// <summary>
        ///     Get DateTime OnMonth Block - part of 27 Block (AlertTriggerOptions)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static byte[] GetDateTimeBlockOnMonth(string text) // DateTime List Entry
        {
            List<byte> curr = new List<byte>();

            int rFrom, rTill; byte[] tFrom, tTill; byte dof; int[] dFrom, dTill;           
            bool hr = HasRange(text, 1, 12, out rFrom, out rTill);
            bool hg = HasDates(text, 1, 12, out dFrom, out dTill);
            bool ht = HasTime(text, out tFrom, out tTill);
            bool hd = HasDof(text, out dof);
            
            if ((!hr) && (!hg)) return null;
            if (hr == hg) return null;

            curr.Add(0); // By Month
            curr.Add((byte)(/*has_month_from*/0x01 + /*has_month_till*/0x02 + /*has_time_start*/(ht ? 0x04 : 0) + /*has_time_end*/0x08 + /*has_dof_end*/(hd ? 0x10 : 0))); // Flags
            if (hr) { curr.Add((byte)rFrom); curr.Add((byte)rTill); };
            if (hg) { curr.Add((byte)dFrom[0]); curr.Add((byte)dTill[0]); };
            if (ht) curr.AddRange(tFrom);
            curr.AddRange(tTill);
            if (hd) curr.Add(dof);
            return curr.ToArray();
        }

        /// <summary>
        ///     Fill 27 Block (AlertTriggerOptions) of DateTime List
        /// </summary>
        /// <param name="poi"></param>
        /// <param name="fb27"></param>
        /// <param name="count"></param>
        private void FillTriggerDateTime(POI poi, FileBlock fb27, ref ushort count) // Part of trigger block
        {
            MatchCollection mc = (new Regex(@"(?:alert_datetime=(?<value>.+))", RegexOptions.None)).Matches(poi.alert);
            List<byte[]> intDTBlocks = new List<byte[]>();
            ushort intDTLength = 0;
            foreach (Match mx in mc)
            {
                string bal = mx.Groups["value"].Value.Replace(" ", "").Replace("\t", "").Trim('\r').ToLower();
                if (String.IsNullOrEmpty(bal)) continue;

                if ((bal.StartsWith("on_day:") && (bal.Length > 7)) || ((!bal.StartsWith("o")) && (bal.Length > 1)))
                {
                    if (bal.StartsWith("on_day:")) bal = bal.Substring(7).Trim();
                    byte[] toAdd = GetDateTimeBlockOnDay(bal);
                    if ((toAdd != null) && (toAdd.Length > 0))
                    {
                        intDTBlocks.Add(toAdd);
                        intDTLength += (ushort)toAdd.Length;
                    };                    
                };
                if (bal.StartsWith("on_week:") && (bal.Length > 8))
                {
                    bal = bal.Substring(8).Trim();
                    byte[] toAdd = GetDateTimeBlockOnWeek(bal);
                    if ((toAdd != null) && (toAdd.Length > 0))
                    {
                        intDTBlocks.Add(toAdd);
                        intDTLength += (ushort)toAdd.Length;
                    };                       
                };
                if (bal.StartsWith("on_month:") && (bal.Length > 9))
                {
                    bal = bal.Substring(9).Trim();
                    byte[] toAdd = GetDateTimeBlockOnMonth(bal);
                    if ((toAdd != null) && (toAdd.Length > 0))
                    {
                        intDTBlocks.Add(toAdd);
                        intDTLength += (ushort)toAdd.Length;
                    };                          
                };

                if (intDTBlocks.Count >= this.MaxAlertDateTimeCount) break;
            };
            if (intDTBlocks.Count > 0)
            {
                count += 8;
                intDTBlocks[intDTBlocks.Count - 1][1] += 0x80; // last record
                fb27.MainData.AddRange(BitConverter.GetBytes(intDTLength));
                foreach (byte[] d in intDTBlocks) fb27.MainData.AddRange(d);

                this.savedTriggerDTBlocks++;
                this.savedTriggerDTs += (uint)intDTBlocks.Count;
            };
        }

        /// <summary>
        ///     is `alert_datetime=...` has dates range
        /// </summary>
        /// <param name="value">text</param>
        /// <param name="min">min day</param>
        /// <param name="max">max day</param>
        /// <param name="dFrom">date from dd,MM,yy</param>
        /// <param name="dTill">date till dd,MM,yy</param>
        /// <returns></returns>
        private static bool HasDates(string value, int min, int max, out int[] dFrom, out int[] dTill)
        {
            dFrom = new int[3] { min, 0, 0 };
            dTill = new int[3] { max, 0, 0 };
            if (String.IsNullOrEmpty(value)) return false;
            Regex rxDates = new Regex(@"(?<dates>(?<df>\d{1,2})(?:\.(?<mf>\d{1,2}))?(?:\.(?<yf>\d{1,4}))?-(?<dt>\d{1,2})(?:\.(?<mt>\d{1,2}))?(?:\.(?<yt>\d{1,4}))?)", RegexOptions.IgnoreCase);
            Match mx = rxDates.Match(value);
            if (!mx.Success) return false;
            if (!int.TryParse(mx.Groups["df"].Value, out dFrom[0])) return false;
            if (!int.TryParse(mx.Groups["dt"].Value, out dTill[0])) return false;
            if (!String.IsNullOrEmpty(mx.Groups["mf"].Value)) int.TryParse(mx.Groups["mf"].Value, out dFrom[1]);
            if (!String.IsNullOrEmpty(mx.Groups["mt"].Value)) int.TryParse(mx.Groups["mt"].Value, out dTill[1]);
            if (!String.IsNullOrEmpty(mx.Groups["yf"].Value)) int.TryParse(mx.Groups["yf"].Value, out dFrom[2]);
            if (!String.IsNullOrEmpty(mx.Groups["yt"].Value)) int.TryParse(mx.Groups["yt"].Value, out dTill[2]);
            if (dFrom[0] < min) dFrom[0] = min;
            if (dFrom[0] > max) dFrom[0] = max;
            if (dTill[0] < min) dTill[0] = min;
            if (dTill[0] > max) dTill[0] = max;
            if (dFrom[1] < 0) dFrom[1] = 0;
            if (dFrom[1] > 12) dFrom[1] = 12;
            if (dFrom[2] < 0) dFrom[2] = 0;
            if (dFrom[2] > 2199) dFrom[2] = 2199;
            return true;
        }

        /// <summary>
        ///     is `alert_datetime=...` has days of week set
        /// </summary>
        /// <param name="value">text</param>
        /// <param name="dof">days of week by mask</param>
        /// <returns></returns>
        private static bool HasDof(string value, out byte dof)
        {
            dof = 0x7F;
            if (String.IsNullOrEmpty(value)) return false;
            bool ex = false;
            Regex rxDof = new Regex(@"\b(?<dof>(?:\p{L}){2,3})\b", RegexOptions.IgnoreCase);
            MatchCollection mc = rxDof.Matches(value);
            if (mc.Count > 0) dof = 0;
            foreach (Match mx in mc)
            {
                ex = true;
                string val = mx.Groups["dof"].Value.ToLower().Substring(0, 2);
                for (int z = 0; z < 7; z++)
                {
                    byte bf = (byte)Math.Pow(2, z);
                    if (val == (new string[] { "sa", "fr", "th", "we", "tu", "mo", "su" })[z]) // EN
                        dof += bf;
                    else if (val == (new string[] { "сб", "пт", "чт", "ср", "вт", "по", "вс" })[z]) // RU (2-symbols)
                        dof += bf;
                    else if (val == (new string[] { "су", "пя", "че", "ср", "вт", "по", "во" })[z]) // RU (3-symbols)
                        dof += bf;
                };
            };
            return dof != 0x7F;
        }

        /// <summary>
        ///     is `alert_datetime=...` has ~ range
        /// </summary>
        /// <param name="value">text</param>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        /// <param name="rFrom">from value</param>
        /// <param name="rTill">till value</param>
        /// <returns></returns>
        private static bool HasRange(string value, int min, int max, out int rFrom, out int rTill)
        {
            rFrom = 0;
            rTill = 0;
            if(String.IsNullOrEmpty(value)) return false;
            Regex rxRange = new Regex(@"(?<range>(?<from>\d{1,3})~(?<till>\d{1,3}))", RegexOptions.IgnoreCase);
            Match mx = rxRange.Match(value);
            if (!mx.Success) return false;
            if (!int.TryParse(mx.Groups["from"].Value, out rFrom)) return false;
            if (!int.TryParse(mx.Groups["till"].Value, out rTill)) return false;
            if (rFrom < min) rFrom = min;
            if (rFrom > max) rFrom = max;
            if (rTill < min) rTill = min;
            if (rTill > max) rTill = max;
            if (rTill < rFrom) return false;
            return true;
        }

        /// <summary>
        ///     is `alert_datetime=...` has time range
        /// </summary>
        /// <param name="value">text</param>
        /// <param name="tFrom">time from hh,mm</param>
        /// <param name="tTill">time till hh,mm</param>
        /// <returns></returns>
        private static bool HasTime(string value, out byte[] tFrom, out byte[] tTill)
        {
            tFrom = new byte[0];
            tTill = new byte[1] { 24 };

            int[] vFrom = new int[2] { 0, 0 };
            int[] vTill = new int[2] { 24, 0 };
            if (String.IsNullOrEmpty(value)) return false;
            Regex rxTime = new Regex(@"(?<time>(?<hf>\d{1,2}):(?<mf>\d{1,2})\.\.(?<ht>\d{1,2}):(?<mt>\d{1,2}))", RegexOptions.IgnoreCase);
            Match mx = rxTime.Match(value);
            if (!mx.Success) return false;
            if (!int.TryParse(mx.Groups["hf"].Value, out vFrom[0])) return false;
            if (!int.TryParse(mx.Groups["mf"].Value, out vFrom[1])) return false;
            if (!int.TryParse(mx.Groups["ht"].Value, out vTill[0])) return false;
            if (!int.TryParse(mx.Groups["mt"].Value, out vTill[1])) return false;
            if ((vFrom[0] == 0) && (vFrom[1] == 0) && (vTill[0] == 24)) return false;
            if (vFrom[0] < 0) vFrom[0] = 0;
            if (vFrom[0] > 23) vFrom[0] = 23;
            if (vFrom[1] < 0) vFrom[1] = 0;
            if (vFrom[1] > 59) vFrom[1] = 59;
            if (vTill[0] < 0) vTill[0] = 0;
            if (vTill[0] > 24) vTill[0] = 24;
            if (vTill[1] < 0) vTill[1] = 0;
            if (vTill[1] > 59) vTill[1] = 59;
            if (vTill[0] < vFrom[0]) return false;
            if ((vFrom[0] != 0) || (vFrom[1] != 0))
            {
                if (vFrom[1] > 0) tFrom = new byte[2] { (byte)(vFrom[0] + 0x80), (byte)(vFrom[1]) }; else tFrom = new byte[1] { (byte)(vFrom[0]) };
            };
            if (vTill[1] > 0) tTill = new byte[2] { (byte)(vTill[0] + 0x80), (byte)(vTill[1]) }; else tTill = new byte[1] { (byte)(vTill[0]) };
            return true;
        }

        private FileBlock Get00Header0Block() // 0
        {
            FileBlock fb = new FileBlock();
            fb.bType = 0x00;
            // MAIN
            {
                if (formatVer == 0)
                    fb.MainData.AddRange(Encoding.ASCII.GetBytes("GRMREC00")); // Header Text
                else
                    fb.MainData.AddRange(Encoding.ASCII.GetBytes("GRMREC01")); // Header Text
                TimeSpan tsec = DateTime.Now.Subtract(new DateTime(1990, 1, 1));
                uint sec = (uint)tsec.TotalSeconds;
                fb.MainData.AddRange(BitConverter.GetBytes(((uint)(sec)))); // Time
                fb.MainData.AddRange(new byte[] { 1, 0 }); // Must Have
                fb.MainData.AddRange(ToPString(String.IsNullOrEmpty(this.Name) ? "Exported Data" : this.Name, true)); // File Name
            };
            // EXTRA
            if (formatVer == 1)
            {
                fb.ExtraData.AddRange(Get15ProductBlock().Data);
            };
            return fb;
        }

        private FileBlock Get01Header1Block() // 1
        {
            FileBlock fb = new FileBlock();
            fb.bType = 0x01;
            //Main
            {
                fb.MainData.AddRange(Encoding.ASCII.GetBytes("POI\0")); // Header Text
                fb.MainData.AddRange(new byte[] { 0, 0 }); // Reserved
                if (formatVer == 0)
                    fb.MainData.AddRange(Encoding.ASCII.GetBytes("00")); // Version
                else
                    fb.MainData.AddRange(Encoding.ASCII.GetBytes("01")); // Version
                fb.MainData.AddRange(BitConverter.GetBytes(((ushort)(0xFDE9)))); //UTF-8 Encoding
                fb.MainData.AddRange(BitConverter.GetBytes(((ushort)(17)))); // Copyrights Exists
            };
            // Extra
            if (formatVer == 1)
            {
                fb.ExtraData.AddRange(Get17CopyrightsBlock().Data);
            };
            return fb;
        }

        private FileBlock Get02POIBlock(POI poi) // 2
        {
            this.savedPoints++;

            FileBlock f02 = new FileBlock();
            f02.bType = 2;
            // Main
            {
                f02.MainData.AddRange(BitConverter.GetBytes(((uint)(poi.lat * Math.Pow(2, 32) / 360.0))));
                f02.MainData.AddRange(BitConverter.GetBytes(((uint)(poi.lon * Math.Pow(2, 32) / 360.0))));
                f02.MainData.AddRange(new byte[] { 1, 0, 0 }); // Reserved
                f02.MainData.AddRange(ToLString(String.IsNullOrEmpty(poi.name) ? "Unknown" : poi.name)); // POI Name
            };
            // Extra
            {
                // CAT
                f02.ExtraData.AddRange(Get06CatIDBlock(poi.cat).Data);
                // STYLE
                f02.ExtraData.AddRange(Get04BmpIDBlock(poi.sty).Data);
                // Alert
                if (StoreAlerts && (!String.IsNullOrEmpty(poi.alert))) f02.ExtraData.AddRange(Get03AlertBlock(poi).Data);
                // Comment
                if (!String.IsNullOrEmpty(poi.comment)) f02.ExtraData.AddRange(Get10CommentBlock(poi.comment).Data);
                // Address
                if ((poi.addr != null) && (poi.addr.Length == 6)) f02.ExtraData.AddRange(Get11AddressBlock(poi.addr).Data);
                // Contact
                if ((poi.contacts != null) && (poi.contacts.Length == 5)) f02.ExtraData.AddRange(Get12ContactsBlock(poi.contacts).Data);
                // Image
                if (StoreImagesAsIs) f02.ExtraData.AddRange(Get13ImageBlock(GetImageFromStyle(poi.sty)).Data);
                // DESC
                if (!String.IsNullOrEmpty(poi.description)) f02.ExtraData.AddRange(Get14DescBlock(poi.description).Data);
            };
            return f02;
        }

        private FileBlock Get03AlertBlock(POI poi) // 3
        {
            this.savedAlerts++;

            FileBlock f03 = new FileBlock();
            f03.bType = 3;
            // Main
            {
                // LENGHT is 12 bytes
                Regex rx; Match mx;
                ushort proximity = 300;
                rx = new Regex(@"alert_proximity=(?<proximity>\d+)", RegexOptions.None); mx = rx.Match(poi.alert);
                if (mx.Success) ushort.TryParse(mx.Groups["proximity"].Value.Trim(), out proximity);
                ushort speed = 00;
                rx = new Regex(@"alert_speed=(?<speed>\d+)", RegexOptions.None); mx = rx.Match(poi.alert);
                if (mx.Success) ushort.TryParse(mx.Groups["speed"].Value.Trim(), out speed);
                byte ison = DefaultAlertIsOn ? (byte)1 : (byte)0;
                rx = new Regex(@"alert_ison=(?<ison>\d+)", RegexOptions.None); mx = rx.Match(poi.alert);
                if (mx.Success) byte.TryParse(mx.Groups["ison"].Value.Trim(), out ison);
                byte atype = 0;
                if (!String.IsNullOrEmpty(DefaultAlertType))
                {
                    if (DefaultAlertType == "proximity") atype = 0;
                    if (DefaultAlertType == "along_road") atype = 1;
                    if (DefaultAlertType == "toure_guide") atype = 2;
                };
                rx = new Regex(@"alert_type=(?<type>\d+)", RegexOptions.None); mx = rx.Match(poi.alert);
                if (mx.Success)
                {
                    string typeval = mx.Groups["type"].Value.Trim().ToLower();
                    if (typeval == "proximity") atype = 0;
                    if (typeval == "along_road") atype = 1;
                    if (typeval == "toure_guide") atype = 2;
                };
                byte sound_number = 4;
                if (!String.IsNullOrEmpty(DefaultAlertSound)) byte.TryParse(DefaultAlertSound, out sound_number);
                f03.MainData.AddRange(BitConverter.GetBytes((ushort)(proximity))); //Proximity in meters
                f03.MainData.AddRange(BitConverter.GetBytes((ushort)(((double)speed) / 3.6 * 100.0))); //Speed in 100*mps
                f03.MainData.AddRange(BitConverter.GetBytes((ushort)(0))); //Reserved
                f03.MainData.AddRange(BitConverter.GetBytes((ushort)(0))); //Reserved
                f03.MainData.Add(ison);          // is on
                f03.MainData.Add(atype);         // Type of alert
                bool defaudio = true;
                rx = new Regex(@"alert_sound=(?<sound>.+)", RegexOptions.None); mx = rx.Match(poi.alert);
                if (mx.Success)
                {
                    string fName = mx.Groups["sound"].Value.Trim('\r');
                    int medid = MP3s.IndexOf(fName);
                    if (medid >= 0)
                    {
                        f03.MainData.AddRange(BitConverter.GetBytes((ushort)(medid + 1)));
                        defaudio = false;
                    };
                }
                if (defaudio)
                {
                    f03.MainData.Add(sound_number);  // Sound number // if predefined: 0 - beep, 1 - tone, 2 - three beeps, 3 - silence, 4-plung, 5-plungplung
                    f03.MainData.Add(0x10);          // Audio Alert // 0x10 - predefined, 0x00 in media record                
                };
            };
            if (formatVer == 1)
            { // EXTRA 
                // AlertCircles
                if (poi.alert.Contains("alert_circle=")) f03.ExtraData.AddRange(Get16CirclesBlock(poi).Data);
                // AlertTriggerOptions
                if (poi.alert.Contains("alert_bearing=") || poi.alert.Contains("alert_datetime=")) f03.ExtraData.AddRange(Get27AlertTriggerBlock(poi).Data);
            };
            return f03;
        }

        private FileBlock Get04BmpIDBlock(int sty) // 4
        {
            FileBlock f04 = new FileBlock();
            f04.bType = 4;
            f04.MainData.AddRange(BitConverter.GetBytes((ushort)sty)); // ID
            return f04;
        }

        private FileBlock Get05BmpBlock(int sty) // 5
        {
            this.savedBitmaps++;

            FileBlock f05 = new FileBlock();
            f05.bType = 5;

            List<Color> palette;
            Bitmap im = GetBitmapFromStyle(sty, out palette);

            int lsize = im.Width + 2;
            f05.MainData.AddRange(BitConverter.GetBytes((ushort)(sty))); // BitmapID
            f05.MainData.AddRange(BitConverter.GetBytes((ushort)(im.Height))); // Height
            f05.MainData.AddRange(BitConverter.GetBytes((ushort)(im.Width))); // Width
            f05.MainData.AddRange(BitConverter.GetBytes((ushort)(lsize))); // LineSize
            f05.MainData.AddRange(BitConverter.GetBytes((ushort)(8))); // BitsPerPixel
            f05.MainData.AddRange(BitConverter.GetBytes((ushort)(0))); // Reserved9
            f05.MainData.AddRange(BitConverter.GetBytes((uint)(lsize * im.Height))); // ImageSize
            f05.MainData.AddRange(BitConverter.GetBytes((uint)(44)));  // Reserved10
            f05.MainData.AddRange(BitConverter.GetBytes((uint)(256))); // Palette
            f05.MainData.AddRange(BitConverter.GetBytes((uint)ColorToUint(TransColor))); // TransparentColor
            f05.MainData.AddRange(BitConverter.GetBytes((uint)(1))); // Flags
            f05.MainData.AddRange(BitConverter.GetBytes((uint)(lsize * im.Height + 44))); // Reserved11
            for (int y = 0; y < im.Height; y++) // Pixels
            {
                for (int x = 0; x < im.Width; x++)
                {
                    Color c = im.GetPixel(x, y);
                    int ci = palette.IndexOf(c);
                    f05.MainData.Add((byte)ci);
                };
                f05.MainData.Add(0); f05.MainData.Add(0);
            };
            for (int i = 0; i < 256; i++) // Colors
            {
                if (i < palette.Count)
                {
                    Color c = palette[i];
                    uint cu = ColorToUint(c);
                    f05.MainData.AddRange(BitConverter.GetBytes(cu));
                }
                else f05.MainData.AddRange(BitConverter.GetBytes((uint)(0))); // NO COLORS
            };
            return f05;
        }

        private FileBlock Get06CatIDBlock(int cat) // 6
        {
            FileBlock f06 = new FileBlock();
            f06.bType = 6;
            f06.MainData.AddRange(BitConverter.GetBytes((ushort)cat)); // ID
            return f06;
        }

        private FileBlock Get07CatBlock(int cat) // 7
        {
            this.savedCategories++;

            FileBlock f07 = new FileBlock();
            f07.bType = 7;
            f07.MainData.AddRange(BitConverter.GetBytes((ushort)cat)); // ID
            f07.MainData.AddRange(ToLString(Categories[cat])); // Name
            return f07;
        }

        private FileBlock Get08MainAreaBlock() // 8
        {
            FileBlock fb = new FileBlock();
            fb.bType = 8;
            // Main
            {
                fb.MainData.AddRange(BitConverter.GetBytes(((uint)(MaxLat * Math.Pow(2, 32) / 360.0))));
                fb.MainData.AddRange(BitConverter.GetBytes(((uint)(MaxLon * Math.Pow(2, 32) / 360.0))));
                fb.MainData.AddRange(BitConverter.GetBytes(((uint)(MinLat * Math.Pow(2, 32) / 360.0))));
                fb.MainData.AddRange(BitConverter.GetBytes(((uint)(MinLon * Math.Pow(2, 32) / 360.0))));
                fb.MainData.AddRange(new byte[] { 0, 0, 0, 0, 1, 0, 2 }); // Must Have
            };
            // Extra
            {
                // SubAreas
                foreach (KeyValuePair<uint, List<POI>> zn in POIs)
                    fb.ExtraData.AddRange(Get08SubAreaBlock(zn.Key).Data);
            };
            return fb;
        }

        private FileBlock Get08SubAreaBlock(uint zone) // 8
        {
            this.savedAreas++;

            double lat, lon;
            ZoneToLatLon(zone, out lat, out lon);
            FileBlock f08 = new FileBlock();
            f08.bType = 8;
            // Main
            {
                f08.MainData.AddRange(BitConverter.GetBytes(((uint)((lat + 1) * Math.Pow(2, 32) / 360.0))));
                f08.MainData.AddRange(BitConverter.GetBytes(((uint)((lon + 1) * Math.Pow(2, 32) / 360.0))));
                f08.MainData.AddRange(BitConverter.GetBytes(((uint)(lat * Math.Pow(2, 32) / 360.0))));
                f08.MainData.AddRange(BitConverter.GetBytes(((uint)(lon * Math.Pow(2, 32) / 360.0))));
                f08.MainData.AddRange(new byte[] { 0, 0, 0, 0, 1, 0, 2 }); // Must Have
            };
            // Extra
            {
                // POIs
                foreach (POI poi in POIs[zone])
                    f08.ExtraData.AddRange(Get02POIBlock(poi).Data);
            };
            return f08;
        }

        private FileBlock Get09POIGroupBlock() // 9
        {
            FileBlock fb = new FileBlock();
            fb.bType = 9;
            // Main
            {
                fb.MainData.AddRange(ToLString(String.IsNullOrEmpty(this.DataSource) ? "KMZRebuilder" : this.DataSource)); // POI Group Name
                fb.MainData.AddRange(Get08MainAreaBlock().Data); // Main Area Block
            };
            // Extra
            {
                // List Categories
                for (int i = 0; i < Categories.Count; i++) fb.ExtraData.AddRange(Get07CatBlock(i).Data);
                // List Bitmaps
                for (int i = 0; i < Styles.Count; i++) fb.ExtraData.AddRange(Get05BmpBlock(i).Data);
                // List Media
                for (int i = 0; i < MP3s.Count; i++) fb.ExtraData.AddRange(Get18MediaBlock(i).Data);
                // Marker
                fb.ExtraData.AddRange(Get18MarkerBlock().Data);
            };
            return fb;
        }

        private FileBlock Get10CommentBlock(string comment) // 10
        {
            this.savedComments++;

            FileBlock f10 = new FileBlock();
            f10.bType = 10;
            f10.MainData.AddRange(ToLString(comment, StoreOnlyLocalLangComments)); // Text
            return f10;
        }

        private FileBlock Get11AddressBlock(string[] addr) // 11
        {
            this.savedAddresses++;

            FileBlock f11 = new FileBlock();
            f11.bType = 11;
            int flags = 0;
            for (int i = 0; i < 6; i++) if (!String.IsNullOrEmpty(addr[i])) flags += (int)Math.Pow(2, i);
            f11.MainData.AddRange(BitConverter.GetBytes((ushort)flags));
            for (int i = 0; i < 6; i++)
                if (!String.IsNullOrEmpty(addr[i]))
                {
                    if ((i == 3) || (i == 5))
                    {
                        if (formatVer == 0)
                            f11.ExtraData.AddRange(ToPString(addr[i], false));
                        else
                            f11.MainData.AddRange(ToPString(addr[i], false));
                    }
                    else
                    {
                        if (formatVer == 0)
                            f11.ExtraData.AddRange(ToLString(addr[i], StoreOnlyLocalLangAddress));
                        else
                            f11.MainData.AddRange(ToLString(addr[i], StoreOnlyLocalLangAddress));
                    };
                };
            return f11;
        }

        private FileBlock Get12ContactsBlock(string[] conts) // 12
        {
            this.savedContacts++;

            FileBlock f12 = new FileBlock();
            f12.bType = 12;
            int flags = 0;
            for (int i = 0; i < 5; i++) if (!String.IsNullOrEmpty(conts[i])) flags += (int)Math.Pow(2, i);
            f12.MainData.AddRange(BitConverter.GetBytes((ushort)flags));
            for (int i = 0; i < 5; i++)
                if (!String.IsNullOrEmpty(conts[i]))
                {
                    if (formatVer == 0)
                        f12.ExtraData.AddRange(ToPString(conts[i], false));
                    else
                        f12.MainData.AddRange(ToPString(conts[i], false));
                };
            return f12;
        }

        private FileBlock Get13ImageBlock(Image im) // 13
        {
            this.savedImages++;

            FileBlock f13 = new FileBlock();
            f13.bType = 13;
            f13.MainData.Add(0); // Reserved
            MemoryStream ms = new MemoryStream();
            im.Save(ms, ImageFormat.Jpeg);
            byte[] arr = ms.ToArray();
            ms.Close();
            f13.MainData.AddRange(BitConverter.GetBytes((uint)arr.Length));
            f13.MainData.AddRange(arr); // ID
            return f13;
        }                

        private FileBlock Get14DescBlock(string desc) // 14
        {
            this.savedDescriptions++;

            FileBlock f14 = new FileBlock();
            f14.bType = 14;
            f14.MainData.Add(1); // Reserved
            f14.MainData.AddRange(ToLString(desc, StoreOnlyLocalLangDescriptions)); // Text
            return f14;
        }

        private FileBlock Get15ProductBlock() // 15
        {
            FileBlock b15 = new FileBlock(); // PRODUCT VERSION DATA
            b15.bType = 15;
            //b15.MainData.AddRange(new byte[] { 1, 7, 9, 0, 0 }); // Must Have; 1, 7 - FID (0x0701), 9 - PID, 0 - RgnID, 0 - VendorID
            b15.MainData.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0, 0 }); // Must Have; UNLOCKED
            return b15;
        }

        private FileBlock Get16CirclesBlock(POI poi) // 16
        {
            this.savedCircleBlocks++;

            FileBlock fb16 = new FileBlock();
            fb16.bType = 16;
            MatchCollection mc = (new Regex(@"(?:alert_circle=(?<value>.+))", RegexOptions.None)).Matches(poi.alert);
            ushort count = 0;
            foreach (Match mx in mc)
            {
                double lat = poi.lat;
                double lon = poi.lon;
                string val = mx.Groups["value"].Value.Trim('\r');
                if (String.IsNullOrEmpty(val)) continue;
                string[] ryx = val.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (String.IsNullOrEmpty(ryx[0])) continue;
                uint rad = 300;
                uint.TryParse(ryx[0], out rad);
                if (ryx.Length == 3)
                {
                    if (!double.TryParse(ryx[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lat)) lat = poi.lat;
                    if (!double.TryParse(ryx[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out lon)) lat = poi.lon;
                };
                fb16.MainData.AddRange(BitConverter.GetBytes(((uint)(lat * Math.Pow(2, 32) / 360.0))));
                fb16.MainData.AddRange(BitConverter.GetBytes(((uint)(lon * Math.Pow(2, 32) / 360.0))));
                fb16.MainData.AddRange(BitConverter.GetBytes((uint)rad)); // radius in meters
                count++;
            };
            fb16.MainData.InsertRange(0, BitConverter.GetBytes((ushort)count)); // count of circles
            this.savedCircles += (uint)count;
            return fb16;
        }

        private FileBlock Get17CopyrightsBlock() // 17
        {
            FileBlock b17 = new FileBlock(); // COPYRIGHTS
            b17.bType = 17;
            b17.MainData.AddRange(new byte[] { 20, 0, 0, 0, 0, 0, 0, 0 }); // Must Have Data
            b17.MainData.AddRange(ToLString(String.IsNullOrEmpty(this.DataSource) ? "KMZRebuilder" : this.DataSource)); // Data Source
            b17.MainData.AddRange(ToLString("Created with KMZRebuilder")); // Copyrights
            b17.MainData.AddRange(new byte[] { 0x01, 0x01, 0xE7, 0x4E }); // Must Have
            return b17;
        }

        private FileBlock Get18MediaBlock(int medid) // 18
        {
            this.savedMedias++;

            FileBlock f18 = new FileBlock();
            f18.bType = 18;

            try
            {
                string fName = MP3s[medid].Trim(new char[] { '\r', '\n' });
                FileInfo fi;
                if (Path.IsPathRooted(fName))
                    fi = new FileInfo(fName);
                else if (!String.IsNullOrEmpty(SourceKMLfile))
                    fi = new FileInfo(Path.Combine(Path.GetDirectoryName(SourceKMLfile), fName));
                else
                    fi = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fName));
                if (fi.Exists)
                {
                    { // MAIN DATA: 3 bytes
                        f18.MainData.AddRange(BitConverter.GetBytes((ushort)(medid + 1)));
                        f18.MainData.Add(fi.Extension.ToLower() == ".wav" ? (byte)0 : (byte)1); // 0 - WAV, 1 - MP3
                    };
                    { // EXTRA DATA
                        long ttl_len = fi.Length + 4 + 2;
                        f18.ExtraData.AddRange(BitConverter.GetBytes((uint)ttl_len)); // TOTAL LENGTH
                        f18.ExtraData.AddRange(Encoding.ASCII.GetBytes(GPIReader.LOCALE_LANGUAGE.Substring(0, 2)));
                        f18.ExtraData.AddRange(BitConverter.GetBytes((uint)fi.Length));
                        byte[] fileData = new byte[fi.Length];
                        FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);
                        fs.Read(fileData, 0, fileData.Length);
                        fs.Close();
                        f18.ExtraData.AddRange(fileData);
                    };
                };
            }
            catch (Exception ex) { };
            return f18;
        }

        private FileBlock Get18MarkerBlock() // 18
        {
            FileBlock f18 = new FileBlock();
            f18.bType = 18;
            { // MAIN DATA
                f18.MainData.AddRange(BitConverter.GetBytes((ushort)(0x7777)));
                f18.MainData.Add(0x77); // 0 - WAV, 1 - MP3
            };            
            { // EXTRA DATA

                MarkerBlock mb = new MarkerBlock();
                mb.Created = DateTime.Now;
                mb.Description = this.Description;
                mb.Bounds = new double[4] { MinLat, MaxLat, MinLon, MaxLon };
                byte[] data = mb.ToBytes();
                long ttl_len = data.Length + 4 + 2;
                f18.ExtraData.AddRange(BitConverter.GetBytes((uint)ttl_len)); // TOTAL LENGTH
                f18.ExtraData.AddRange(Encoding.ASCII.GetBytes("RU"));
                f18.ExtraData.AddRange(BitConverter.GetBytes((uint)data.Length));
                f18.ExtraData.AddRange(data);
            };
            return f18;
        }

        private FileBlock Get27AlertTriggerBlock(POI poi) // 27
        {
            this.savedTriggerBlocks++;

            FileBlock fb27 = new FileBlock();
            fb27.bType = 27;
            ushort count = 0;
            // Bearing List Entries
            FillTriggerBearings(poi, fb27, ref count);
            // DateTime List Entries
            FillTriggerDateTime(poi, fb27, ref count);
            fb27.MainData.InsertRange(0, BitConverter.GetBytes((ushort)count)); // count of circles
            return fb27;
        }
       
        private FileBlock GetFooter() // end
        {
            FileBlock fb = new FileBlock();
            fb.bType = 0xFFFF;
            return fb;
        }

        /// <summary>
        ///     Get Image from style to store in Image Block in POI
        /// </summary>
        /// <param name="sty">style name from image list</param>
        /// <returns></returns>
        private Image GetImageFromStyle(int sty)
        {
            Image im;
            if (Images.ContainsKey(Styles[sty]))
                im = (Bitmap)Images[Styles[sty]];
            else
            {
                im = new Bitmap(16, 16);
                Graphics g = Graphics.FromImage(im);
                g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, 16, 16));
                g.FillRectangle(new SolidBrush(Color.White), new Rectangle(1, 1, 14, 14));
                string ttd = sty.ToString();
                while (ttd.Length < 2) ttd = "0" + ttd;
                g.DrawString(ttd, new Font("MS Sans Serif", 8), Brushes.Black, 0, 1);
                g.Dispose();
            };
            return im;
        }

        /// <summary>
        ///     GEt Image from style to store in Bmp Block
        /// </summary>
        /// <param name="sty"></param>
        /// <param name="palette"></param>
        /// <returns></returns>
        private Bitmap GetBitmapFromStyle(int sty, out List<Color> palette)
        {
            Bitmap im;
            if (Images.ContainsKey(Styles[sty]))
            {
                Bitmap bim = (Bitmap)Images[Styles[sty]];
                im = new Bitmap(bim.Width, bim.Height);
                Graphics g = Graphics.FromImage(im);
                g.Clear(TransColor);
                g.DrawImage(bim, new Point(0, 0));
                g.Dispose();
            }
            else
            {
                im = new Bitmap(16, 16);
                Graphics g = Graphics.FromImage(im);
                g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(0, 0, 16, 16));
                g.FillRectangle(new SolidBrush(Color.White), new Rectangle(1, 1, 14, 14));
                string ttd = sty.ToString();
                while (ttd.Length < 2) ttd = "0" + ttd;
                g.DrawString(ttd, new Font("MS Sans Serif", 8), Brushes.Black, 0, 1);
                g.Dispose();
            };
            int maximside = MaxImageSide;
            if (maximside < 16) maximside = 16;
            if (maximside > 48) maximside = 48;
            ImageMagick.MagickImage mi = new ImageMagick.MagickImage(im);
            if ((im.Width > maximside) || (im.Height > maximside)) // im = ResizeImage(im, 32, 32);
                mi.Resize(maximside, maximside);            
            ImageMagick.QuantizeSettings qs = new ImageMagick.QuantizeSettings();
            qs.Colors = 256;
            mi.Quantize(qs);
            int index = 0;
            palette = new List<Color>();
            ImageMagick.MagickColor col;
            while (true)
            {
                col = mi.GetColormap(index++);
                if (col == null) break;
                else palette.Add(col.ToColor());
            };            
            return mi.ToBitmap();
        }        

        /// <summary>
        ///     Text to Pascal-like string
        /// </summary>
        /// <param name="value">text</param>
        /// <param name="translit">translit?</param>
        /// <returns></returns>
        private byte[] ToPString(string value, bool translit) // Pascal-like String
        {
            List<byte> res = new List<byte>();
            byte[] tnArr = Encoding.UTF8.GetBytes(translit ? ml.Translit(value) : value);
            res.AddRange(BitConverter.GetBytes((ushort)tnArr.Length));
            res.AddRange(tnArr);
            return res.ToArray();
        }

        /// <summary>
        ///     Text to Multilanguage string
        /// </summary>
        /// <param name="value">text</param>
        /// <returns></returns>
        private byte[] ToLString(string value) // Multilang String
        {
            return ToLString(value, false);
        }

        /// <summary>
        ///     Text to Multilanguage string
        /// </summary>
        /// <param name="value">text</param>
        /// <returns></returns>
        private byte[] ToLString(string value, bool onlyLocal) // Multilang String
        {
            List<byte> res = new List<byte>();
            if (!onlyLocal)
            {
                // EN if not (first of all)
                if ((!StoreOnlyLocalLang) && (GPIReader.LOCALE_LANGUAGE != GPIReader.DEFAULT_LANGUAGE) && (GPIReader.LOCALE_LANGUAGE != "EN") && (GPIReader.DEFAULT_LANGUAGE != "EN"))
                {
                    res.AddRange(Encoding.ASCII.GetBytes("EN"));
                    res.AddRange(ToPString(value, true));
                };
                // Default if is set
                if ((!StoreOnlyLocalLang) && (GPIReader.LOCALE_LANGUAGE != GPIReader.DEFAULT_LANGUAGE))
                {
                    res.AddRange(Encoding.ASCII.GetBytes(GPIReader.DEFAULT_LANGUAGE.Substring(0, 2)));
                    res.AddRange(ToPString(value, true));
                };
            };
            // Local if is set
            res.AddRange(Encoding.ASCII.GetBytes(GPIReader.LOCALE_LANGUAGE.Substring(0,2)));
            res.AddRange(ToPString(value, false));            
            res.InsertRange(0, BitConverter.GetBytes((uint)res.Count));
            return res.ToArray();
        }

        /// <summary>
        ///     Resize image to smaller
        /// </summary>
        /// <param name="imgPhoto">image</param>
        /// <param name="Width">width</param>
        /// <param name="Height">height</param>
        /// <returns></returns>
        private static Bitmap ResizeImage(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Red);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        /// <summary>
        ///     Color to number
        /// </summary>
        /// <param name="c">Color</param>
        /// <returns></returns>
        private static uint ColorToUint(Color c)
        {
            uint res = (uint)(c.R + (c.G << 8) + (c.B << 16));
            return res;
        }
    }

    /// <summary>
    ///     Lang Translit (now is: RU -> EN)
    /// </summary>
    public class Translitter
    {
        private Dictionary<string, string> words = new Dictionary<string, string>();

        public Translitter() { InitDict(); }

        private void InitDict()
        {
            words.Add("а", "a");
            words.Add("б", "b");
            words.Add("в", "v");
            words.Add("г", "g");
            words.Add("д", "d");
            words.Add("е", "e");
            words.Add("ё", "yo");
            words.Add("ж", "zh");
            words.Add("з", "z");
            words.Add("и", "i");
            words.Add("й", "j");
            words.Add("к", "k");
            words.Add("л", "l");
            words.Add("м", "m");
            words.Add("н", "n");
            words.Add("о", "o");
            words.Add("п", "p");
            words.Add("р", "r");
            words.Add("с", "s");
            words.Add("т", "t");
            words.Add("у", "u");
            words.Add("ф", "f");
            words.Add("х", "h");
            words.Add("ц", "c");
            words.Add("ч", "ch");
            words.Add("ш", "sh");
            words.Add("щ", "sch");
            words.Add("ъ", "j");
            words.Add("ы", "i");
            words.Add("ь", "j");
            words.Add("э", "e");
            words.Add("ю", "yu");
            words.Add("я", "ya");
            words.Add("А", "A");
            words.Add("Б", "B");
            words.Add("В", "V");
            words.Add("Г", "G");
            words.Add("Д", "D");
            words.Add("Е", "E");
            words.Add("Ё", "Yo");
            words.Add("Ж", "Zh");
            words.Add("З", "Z");
            words.Add("И", "I");
            words.Add("Й", "J");
            words.Add("К", "K");
            words.Add("Л", "L");
            words.Add("М", "M");
            words.Add("Н", "N");
            words.Add("О", "O");
            words.Add("П", "P");
            words.Add("Р", "R");
            words.Add("С", "S");
            words.Add("Т", "T");
            words.Add("У", "U");
            words.Add("Ф", "F");
            words.Add("Х", "H");
            words.Add("Ц", "C");
            words.Add("Ч", "Ch");
            words.Add("Ш", "Sh");
            words.Add("Щ", "Sch");
            words.Add("Ъ", "J");
            words.Add("Ы", "I");
            words.Add("Ь", "J");
            words.Add("Э", "E");
            words.Add("Ю", "Yu");
            words.Add("Я", "Ya");

        }

        public string Translit(string text)
        {
            string res = text;
            foreach (KeyValuePair<string, string> pair in words) res = res.Replace(pair.Key, pair.Value);
            return res;
        }
    }

    public static class AnalyzeDescription
    {
        public static bool OSM(ref string desc)
        {
            string origin = desc;
            bool changed = false;
            if (String.IsNullOrEmpty(desc)) return changed;

            Match mx;
            // Addresses
            if ((mx = (new Regex(@"(?:addr\:postcode=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("addr_postal={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:addr\:country=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("addr_country={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:addr\:state=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("addr_state={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            // addr:region, addr:district, addr:subdistrict
            {
                string state = "";
                Match mx1 = (new Regex(@"(?:addr\:region=(?<value>.+)\n?)")).Match(desc);
                if (mx1.Success) { state += (state.Length == 0 ? "" : ", ") + mx1.Groups["value"].Value.Trim('\r'); desc = desc.Replace(mx1.Value, ""); };
                Match mx2 = (new Regex(@"(?:addr\:district=(?<value>.+)\n?)")).Match(desc);
                if (mx2.Success) { state += (state.Length == 0 ? "" : ", ") + mx2.Groups["value"].Value.Trim('\r'); desc = desc.Replace(mx2.Value, ""); };
                Match mx3 = (new Regex(@"(?:addr\:subdistrict=(?<value>.+)\n?)")).Match(desc);
                if (mx3.Success) { state += (state.Length == 0 ? "" : ", ") + mx3.Groups["value"].Value.Trim('\r'); desc = desc.Replace(mx3.Value, ""); };                
                if (!String.IsNullOrEmpty(state))
                {
                    if (!desc.EndsWith("\r\n")) desc += "\r\n";
                    desc += String.Format("addr_state={0}", state);
                    changed = true;
                };
            };
            // addr:city, addr:suburb, addr:place
            {
                string city = "";
                Match mx1 = (new Regex(@"(?:addr\:city=(?<value>.+)\n?)")).Match(desc);
                if (mx1.Success) { city += (city.Length == 0 ? "" : ", ") + mx1.Groups["value"].Value.Trim('\r'); desc = desc.Replace(mx1.Value, ""); };
                Match mx2 = (new Regex(@"(?:addr\:suburb=(?<value>.+)\n?)")).Match(desc);
                if (mx2.Success) { city += (city.Length == 0 ? "" : ", ") + mx2.Groups["value"].Value.Trim('\r'); desc = desc.Replace(mx2.Value, ""); };
                Match mx3 = (new Regex(@"(?:addr\:place=(?<value>.+)\n?)")).Match(desc);
                if (mx3.Success) { city += (city.Length == 0 ? "" : ", ") + mx3.Groups["value"].Value.Trim('\r'); desc = desc.Replace(mx3.Value, ""); };
                if (!String.IsNullOrEmpty(city))
                {
                    if (!desc.EndsWith("\r\n")) desc += "\r\n";
                    desc += String.Format("addr_city={0}", city);
                    changed = true;
                };
            }
            if ((mx = (new Regex(@"(?:addr\:street=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("addr_street={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:addr\:housename=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("addr_house={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:addr\:housenumber=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("addr_house={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            // Contacts            
            if ((mx = (new Regex(@"(?:(?:contact\:)?phone=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("contact_phone={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:(?:contact\:)?phone2=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("contact_phone2={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:(?:contact\:)?email=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("contact_email={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:(?:contact\:)?fax=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("contact_fax={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:(?:contact\:)?ok=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("contact_web={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:(?:contact\:)?facebook=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("contact_web={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:(?:contact\:)?vk=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("contact_web={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:(?:contact\:)?instagram=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("contact_web={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            if ((mx = (new Regex(@"(?:(?:contact\:)?website=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("contact_web={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            // Comment-Description
            if ((mx = (new Regex(@"(?:desc(?:ription)?=(?<value>.+))")).Match(desc)).Success) { desc = desc.Replace(mx.Value.Trim('\r'), String.Format("comment={0}", mx.Groups["value"].Value.Trim('\r'))); changed = true; };
            // No Name
            if ((mx = (new Regex(@"(?:name=(?<value>.+)\n?)")).Match(desc)).Success) { desc = desc.Replace(mx.Value,""); changed = true; };
            // NO TAG ID
            if ((mx = (new Regex(@"(?:tag(?:id)?=(?<value>.+)\n?)")).Match(desc)).Success) { desc = desc.Replace(mx.Value, ""); changed = true; };
            if ((mx = (new Regex(@"(?:ref(?:id)?=(?<value>.+)\n?)")).Match(desc)).Success) { desc = desc.Replace(mx.Value, ""); changed = true; };

            return changed;
        }
    }
}
