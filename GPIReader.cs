/***************************************************/
/*                                                 */
/*            C# Garmin POI File Reader            */
/*              (by milokz@gmail.com)              */
/*                                                 */
/*         GPIReader by milokz@gmail.com           */
/*          Part of KMZRebuilder Project           */
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

namespace KMZRebuilder
{
    #region RECTYPES
    public enum RecType: ushort
    {
        Header0 = 0,
        Header1 = 1,
        Waypoint = 2,
        Alert = 3,
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
        Unknown15 = 15,
        Unknown16 = 16,
        Copyright = 17,
        Media = 18,
        SpeedCamera = 19,
        Index = 20,
        Unknown23 = 23,
        Unknown24 = 24,
        Unknown25 = 25,
        Unknown27 = 27,
        End = 0xFFFF
    }

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

    public class Record
    {
        public Record Parent = null;
        public List<Record> Childs = new List<Record>();
        public bool IsRoot { get { return Parent == null; } }
        public int RootLevel { get { return Parent == null ? 0 : Parent.RootLevel + 1; } }
        public string Ierarchy { get { return Parent == null ? @"\Root" : Parent.Ierarchy + @"\" + RecordType.ToString(); } }
        public Exception ReadError = null;

        public ushort RType = 0;
        public RecType RecordType { get { return (RecType)RType; } }
        public ushort RFlags = 0;
        public bool HasExtra { get { return (RFlags & 0x08) == 0x08; } }

        public uint MainLength = 0;
        public uint ExtraLength = 0;
        public uint TotalLength = 0;
        public byte[] MainData = new byte[0];
        public byte[] ExtraData = new byte[0];

        public Record(Record parent)
        {
            this.Parent = parent;
            if (parent != null) parent.Childs.Add(this);
        }

        public static Record Create(Record parent, ushort RecordType)
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
            if (RecordType == 17) res = new RecCopyright(parent);
            if (RecordType == 18) res = new RecMedia(parent);
            if (RecordType == 19) res = new RecSpeedCamera(parent);
            if (RecordType == 0xFFFF) res = new RecEnd(parent);
            if (res == null) res = new Record(parent);
            res.RType = RecordType;
            return res;            
        }

        public override string ToString()
        {
            return String.Format("{1}[{2}]{3}", RecordType, RType, RootLevel, Ierarchy);
        }
    }

    // 0
    public class RecHeader0 : Record
    {
        public RecHeader0(Record parent) : base(parent) { }
        public string Header = null;
        public string Version = null;
        public DateTime Created = DateTime.MinValue;
        public string Name = null;
    }

    // 1
    public class RecHeader1 : Record
    {
        public RecHeader1(Record parent) : base(parent) { }
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
    public class RecWaypoint : Record
    {
        public RecWaypoint(Record parent) : base(parent) { }
        public int cLat;
        public int cLon;
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
    public class RecAlert : Record
    {
        public RecAlert(Record parent) : base(parent) { }
        public ushort Proximity;
        public ushort cSpeed;
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
    }

    // 4
    public class RecBitmapReference : Record
    {
        public RecBitmapReference(Record parent) : base(parent) { }
        public ushort BitmapID;
    }

    // 5
    public class RecBitmap : Record
    {
        public RecBitmap(Record parent) : base(parent) { }
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
    public class RecCategoryReference : Record
    {
        public RecCategoryReference(Record parent) : base(parent) { }
        public ushort CategoryID;
    }

    // 7
    public class RecCategory : Record
    {
        public RecCategory(Record parent) : base(parent) { }
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
    public class RecArea : Record
    {
        public RecArea(Record parent) : base(parent) { }
        public int cMaxLat;
        public int cMaxLon;
        public int cMinLat;
        public int cMinLon;
        public double MaxLat { get { return (double)cMaxLat * 360.0 / Math.Pow(2, 32); } }
        public double MaxLon { get { return (double)cMaxLon * 360.0 / Math.Pow(2, 32); } }
        public double MinLat { get { return (double)cMinLat * 360.0 / Math.Pow(2, 32); } }
        public double MinLon { get { return (double)cMinLon * 360.0 / Math.Pow(2, 32); } }        
    }

    // 9
    public class RecPOIGroup : Record
    {
        public RecPOIGroup(Record parent) : base(parent) { }
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
    public class RecComment : Record
    {
        public RecComment(Record parent) : base(parent) { }
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
    public class RecAddress : Record
    {
        public RecAddress(Record parent) : base(parent) { }
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
    public class RecContact : Record
    {
        public RecContact(Record parent) : base(parent) { }
        public ushort Flags;
        public List<KeyValuePair<string, string>> cPhone = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> cPhone2 = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> cFax = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> cEmail = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> cWeb = new List<KeyValuePair<string, string>>();

        public string Phone
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in cPhone)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cPhone)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cPhone)
                    return kvp.Value;
                return null;
            }
        }

        public string Phone2
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in cPhone2)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cPhone2)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cPhone2)
                    return kvp.Value;
                return null;
            }
        }

        public string Fax
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in cFax)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cFax)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cFax)
                    return kvp.Value;
                return null;
            }
        }

        public string Email
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in cEmail)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cEmail)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cEmail)
                    return kvp.Value;
                return null;
            }
        }

        public string Web
        {
            get
            {
                foreach (KeyValuePair<string, string> kvp in cWeb)
                    if (kvp.Key == GPIReader.LOCALE_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cWeb)
                    if (kvp.Key == GPIReader.DEFAULT_LANGUAGE)
                        return kvp.Value;
                foreach (KeyValuePair<string, string> kvp in cWeb)
                    return kvp.Value;
                return null;
            }
        }
    }

    // 13
    public class RecImage : Record
    {
        public RecImage(Record parent) : base(parent) { }
        public uint Length;
        public byte[] ImageData;
    }

    // 14
    public class RecDescription : Record
    {
        public RecDescription(Record parent) : base(parent) { }
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

    // 17
    public class RecCopyright : Record
    {
        public RecCopyright(Record parent) : base(parent) { }
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
    public class RecMedia : Record
    {
        public RecMedia(Record parent) : base(parent) { }
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
    public class RecSpeedCamera : Record
    {
        public RecSpeedCamera(Record parent) : base(parent) { }
        public int cMaxLat;
        public int cMaxLon;
        public int cMinLat;
        public int cMinLon;
        public double MaxLat { get { return (double)cMaxLat * 360.0 / Math.Pow(2, 24); } }
        public double MaxLon { get { return (double)cMaxLon * 360.0 / Math.Pow(2, 24); } }
        public double MinLat { get { return (double)cMinLat * 360.0 / Math.Pow(2, 24); } }
        public double MinLon { get { return (double)cMinLon * 360.0 / Math.Pow(2, 24); } }
        public byte Flags;
        public int cLat;
        public int cLon;
        public double Lat { get { return (double)cLat * 360.0 / Math.Pow(2, 24); } }
        public double Lon { get { return (double)cLon * 360.0 / Math.Pow(2, 24); } }
    }

    // 0xFFFF
    public class RecEnd : Record
    {
        public RecEnd(Record parent) : base(parent) { }
    }
    #endregion RECTYPES

    /// <summary>
    ///     GPI Reader
    /// </summary>
    public class GPIReader
    {
        public static string LOCALE_LANGUAGE  = "EN"; // 2-SYMBOLS
        public static string DEFAULT_LANGUAGE = "EN"; // 2-SYMBOLS

        private string fileName;
        public string FileName { get { return fileName; } }

        public Record RootElement = new Record(null);

        public string Content = null;
        public ushort CodePage = 0xFDE9;
        public Encoding Encoding = Encoding.Unicode;
        public string Header = null;
        public string Version = null;
        public DateTime Created = DateTime.MinValue;
        public string Name = null;
        public List<KeyValuePair<string, string>> cDataSource = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> cCopyrights = new List<KeyValuePair<string, string>>();

        public Dictionary<ushort, RecCategory> Categories = new Dictionary<ushort, RecCategory>();
        public Dictionary<ushort, RecBitmap> Bitmaps = new Dictionary<ushort, RecBitmap>();

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

        public GPIReader(string fileName)
        {
            this.fileName = fileName;
            this.Read();
            this.LoopRecords(this.RootElement.Childs);
        }

        public void SaveToKML(string fileName)
        {
            string images_file_dir = Path.GetDirectoryName(fileName) + @"\images\";
            Directory.CreateDirectory(images_file_dir);

            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<kml><Document>");
            string caption = (String.IsNullOrEmpty(this.Name) ? "GPI Has No Name" : this.Name);
            sw.WriteLine("<name><![CDATA[" + caption +  "]]></name><createdby>KMZ Rebuilder GPI Reader</createdby>");
            string desc = "Created: " + this.Created.ToString() + "\r\n";
            foreach (KeyValuePair<string, string> langval in this.cDataSource)
                desc += String.Format("data_source:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
            foreach (KeyValuePair<string, string> langval in this.cCopyrights)
                desc += String.Format("copyrights:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
            sw.WriteLine("<description><![CDATA[" + desc + "]]></description>");
            List<string> simstyles = new List<string>();
            foreach (KeyValuePair<ushort, RecCategory> kCat in this.Categories)
            {
                if (kCat.Value.Waypoints.Count == 0) continue;

                string style = "catid" + kCat.Value.CategoryID.ToString();
                if (kCat.Value.Bitmap != null) style = "imgid" + kCat.Value.Bitmap.BitmapID.ToString();
                
                sw.WriteLine("<Folder><name><![CDATA[" + kCat.Value.Name + "]]></name>");
                desc = "CategoryID: " + kCat.Value.CategoryID.ToString() + "\r\n";
                desc += "Objects: " + kCat.Value.Waypoints.Count.ToString() + "\r\n";
                foreach (KeyValuePair<string, string> langval in kCat.Value.Category)
                    desc += String.Format("name:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                if (kCat.Value.Description != null)
                    foreach (KeyValuePair<string, string> langval in kCat.Value.Description.Description)
                        desc += String.Format("desc:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                if (kCat.Value.Comment != null)
                    foreach (KeyValuePair<string, string> langval in kCat.Value.Comment.Comment)
                        desc += String.Format("comm:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                if (kCat.Value.Contact != null)
                {
                    foreach (KeyValuePair<string, string> langval in kCat.Value.Contact.cPhone)
                        desc += String.Format("contact_phone:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                    foreach (KeyValuePair<string, string> langval in kCat.Value.Contact.cPhone2)
                        desc += String.Format("contact_phone2:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                    foreach (KeyValuePair<string, string> langval in kCat.Value.Contact.cFax)
                        desc += String.Format("contact_fax:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                    if (!String.IsNullOrEmpty(kCat.Value.Contact.Email))
                        desc += String.Format("contact_email={0}\r\n", kCat.Value.Contact.Email);
                    if (!String.IsNullOrEmpty(kCat.Value.Contact.Web))
                        desc += String.Format("contact_web={0}\r\n", kCat.Value.Contact.Web);
                };
                sw.WriteLine("<description><![CDATA[" + desc + "]]></description>");
                foreach (RecWaypoint wp in kCat.Value.Waypoints)
                {
                    sw.WriteLine("<Placemark>");
                    sw.WriteLine("<name><![CDATA[" + wp.Name + "]]></name>");
                    string text = "";
                    foreach (KeyValuePair<string, string> langval in wp.ShortName)
                        text += String.Format("name:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                    if (wp.Description != null)
                        foreach (KeyValuePair<string, string> langval in wp.Description.Description)
                            text += String.Format("desc:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                    if (wp.Comment != null)
                        foreach (KeyValuePair<string, string> langval in wp.Comment.Comment)
                            text += String.Format("comm:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                    if (wp.Contact != null)
                    {
                        foreach (KeyValuePair<string, string> langval in wp.Contact.cPhone)
                            text += String.Format("contact_phone:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                        foreach (KeyValuePair<string, string> langval in wp.Contact.cPhone2)
                            text += String.Format("contact_phone2:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
                        foreach (KeyValuePair<string, string> langval in wp.Contact.cFax)
                            text += String.Format("contact_fax:{0}={1}\r\n", langval.Key.ToLower(), langval.Value);
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
                            text += String.Format("addr_house={0}\r\n", wp.Address.Postal);
                    };
                    if (wp.Alert != null)
                    {
                        text += String.Format("alert_proximity={0}\r\n", wp.Alert.Proximity);
                        text += String.Format("alert_speed={0}\r\n", wp.Alert.Speed);
                        text += String.Format("alert_ison={0}\r\n", wp.Alert.Alert);
                        text += String.Format("alert_type={0}\r\n", wp.Alert.IsType);
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
                            style = simid;
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
                }
                catch (Exception ex) {};
            }
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
            sw.WriteLine("</Document></kml>");
            sw.Close();
            fs.Close();
        }        

        private void LoopRecords(List<Record> records)
        {
            if ((records == null) || (records.Count == 0)) return;
            foreach (Record r in records)
            {
                GetReferences(r);
                LoopRecords(r.Childs);
            };
        }

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

        private void Read()
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] fileData = new byte[fs.Length];
            fs.Read(fileData, 0, fileData.Length);
            fs.Close();

            if (fileData.Length != 0) 
                ReadData(ref fileData, RootElement);
        }

        private void ReadData(ref byte[] fileData, Record parent)
        {
            int offset = 0;
            while (offset < fileData.Length)
            {
                int blockLength = ReadRecordBlock(ref fileData, parent, offset);
                offset += blockLength;
            };
        }

        private int ReadRecordBlock(ref byte[] data, Record parent, int offset)
        {
            int start_offset = offset;            
            Record rec = Record.Create(parent, BitConverter.ToUInt16(data, offset)); offset += 2;
            rec.RFlags = BitConverter.ToUInt16(data, offset); offset += 2;
            rec.TotalLength = BitConverter.ToUInt32(data, offset); offset += 4;
            rec.MainLength = rec.TotalLength;
            try
            {
                if (rec.HasExtra)
                {
                    rec.MainLength = BitConverter.ToUInt32(data, offset); offset += 4;
                    rec.ExtraLength = rec.TotalLength - rec.MainLength;
                };
                if (RecEnum<RecType>.IsDefined((RecType)rec.RType)) // only specified
                {
                    rec.MainData = new byte[rec.MainLength];
                    Array.Copy(data, offset, rec.MainData, 0, rec.MainData.Length);
                    if (rec.HasExtra)
                    {
                        rec.ExtraData = new byte[rec.ExtraLength];
                        Array.Copy(data, offset + rec.MainLength, rec.ExtraData, 0, rec.ExtraData.Length);
                    };
                    ReadMainBlock(rec);
                    if (rec.HasExtra) ReadData(ref rec.ExtraData, rec);
                };
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
            int ttlbl = (int)(offset - start_offset + rec.TotalLength);
            return ttlbl;
        }

        private void ReadMainBlock(Record rec)
        {
            if ((rec.RType == 0) && (rec is RecHeader0)) ReadHeader1((RecHeader0)rec);
            if ((rec.RType == 1) && (rec is RecHeader1)) ReadHeader2((RecHeader1)rec);
            if ((rec.RType == 2) && (rec is RecWaypoint)) ReadWaypoint((RecWaypoint)rec);
            if ((rec.RType == 3) && (rec is RecAlert)) ReadAlert((RecAlert)rec);
            if ((rec.RType == 4) && (rec is RecBitmapReference)) ReadBitmapReference((RecBitmapReference)rec);
            if ((rec.RType == 5) && (rec is RecBitmap)) ReadBitmap((RecBitmap)rec);
            if ((rec.RType == 6) && (rec is RecCategoryReference)) ReadCategoryReference((RecCategoryReference)rec);
            if ((rec.RType == 7) && (rec is RecCategory)) ReadCategory((RecCategory)rec);
            if ((rec.RType == 8) && (rec is RecArea)) ReadArea((RecArea)rec);
            if ((rec.RType == 9) && (rec is RecPOIGroup)) ReadPOIGroup((RecPOIGroup)rec);
            if ((rec.RType == 10) && (rec is RecComment)) ReadComment((RecComment)rec);
            if ((rec.RType == 11) && (rec is RecAddress)) ReadAddress((RecAddress)rec);
            if ((rec.RType == 12) && (rec is RecContact)) ReadContact((RecContact)rec);
            if ((rec.RType == 13) && (rec is RecImage)) ReadImage((RecImage)rec);
            if ((rec.RType == 14) && (rec is RecDescription)) ReadDecription((RecDescription)rec);
            if ((rec.RType == 17) && (rec is RecCopyright)) ReadCopyright((RecCopyright)rec);
            if ((rec.RType == 18) && (rec is RecMedia)) ReadMedia((RecMedia)rec);
            if ((rec.RType == 19) && (rec is RecSpeedCamera)) ReadSpeedCamera((RecSpeedCamera)rec);
        }

        private void ReadHeader1(RecHeader0 rec) // 0
        {
            byte[] sub = new byte[6];
            Array.Copy(rec.MainData, 0, sub, 0, 6);
            rec.Header = Header = Encoding.ASCII.GetString(sub);
            sub = new byte[2];
            Array.Copy(rec.MainData, 6, sub, 0, 2);
            rec.Version = Version = Encoding.ASCII.GetString(sub);
            uint time = BitConverter.ToUInt32(rec.MainData, 8);
            if (time != 0xFFFFFFFF)
                rec.Created = Created = (new DateTime(1990, 1, 1)).AddSeconds(time);
            ushort slen = BitConverter.ToUInt16(rec.MainData, 14);
            rec.Name = Name = Encoding.ASCII.GetString(rec.MainData, 16, slen);
        }

        private void ReadHeader2(RecHeader1 rec) // 1
        {
            int bLen = 0;
            while(rec.MainData[bLen] != 0) bLen++;
            rec.Content = this.Content = Encoding.ASCII.GetString(rec.MainData,0, bLen++);            
            rec.CodePage = this.CodePage = BitConverter.ToUInt16(rec.MainData, bLen + 4);
            this.Encoding = rec.Encoding;
        }

        private void ReadWaypoint(RecWaypoint rec) // 2
        {
            rec.cLat = BitConverter.ToInt32(rec.MainData, 0);
            rec.cLon = BitConverter.ToInt32(rec.MainData, 4);
            int offset = 11;
            uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
            int readed = 0;
            while (readed < len)
            {
                string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                rec.ShortName.Add(new KeyValuePair<string, string>(lang, text));
            };
        }

        private void ReadAlert(RecAlert rec) // 3
        {
            try
            {
                rec.Proximity = BitConverter.ToUInt16(rec.MainData, 0);
                rec.cSpeed = BitConverter.ToUInt16(rec.MainData, 2);
                rec.Alert = rec.MainData[8];
                rec.AlertType = rec.MainData[9];
                rec.SoundNumber = rec.MainData[10];
                rec.AudioAlert = rec.MainData[11];
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint)) ((RecWaypoint)rec.Parent).Alert = rec;
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
        }

        public void ReadBitmapReference(RecBitmapReference rec) // 4
        {
            rec.BitmapID = BitConverter.ToUInt16(rec.MainData, 0);
        }

        public void ReadBitmap(RecBitmap rec) // 5
        {
            try
            {
                int offset = 0;
                rec.BitmapID = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
                rec.Height = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
                rec.Width = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
                rec.LineSize = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
                rec.BitsPerPixel = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
                rec.Reserved9 = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
                rec.ImageSize = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                rec.Reserved10 = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                rec.Palette = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                rec.TransparentColor = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                rec.Flags = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                rec.Reserved11 = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                rec.Pixels = new byte[rec.ImageSize];
                Array.Copy(rec.MainData, offset, rec.Pixels, 0, rec.ImageSize); offset += (int)rec.ImageSize;
                rec.Colors = new uint[rec.Palette];
                for (int i = 0; i < rec.Colors.Length; i++) { rec.Colors[i] = BitConverter.ToUInt32(rec.MainData, offset); offset += 4; };
                this.Bitmaps.Add(rec.BitmapID, rec);
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
        }

        private void ReadCategoryReference(RecCategoryReference rec) // 6
        {
            rec.CategoryID = BitConverter.ToUInt16(rec.MainData, 0);                
        }

        private void ReadCategory(RecCategory rec) // 7
        {
            rec.CategoryID = BitConverter.ToUInt16(rec.MainData, 0);
            int offset = 2;
            uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
            int readed = 0;
            while (readed < len)
            {
                string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                rec.Category.Add(new KeyValuePair<string, string>(lang, text));
            };
            this.Categories.Add(rec.CategoryID, rec);
        }

        private void ReadArea(RecArea rec) // 8
        {
            rec.cMaxLat = BitConverter.ToInt32(rec.MainData, 0);
            rec.cMaxLon = BitConverter.ToInt32(rec.MainData, 4);
            rec.cMinLat = BitConverter.ToInt32(rec.MainData, 8);
            rec.cMinLon = BitConverter.ToInt32(rec.MainData, 12);
        }

        private void ReadPOIGroup(RecPOIGroup rec) // 9
        {
            int offset = 0;
            uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
            int readed = 0;
            while (readed < len)
            {
                string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                rec.DataSource.Add(new KeyValuePair<string, string>(lang, text));
            };

            byte[] areas = new byte[rec.MainLength - offset];
            Array.Copy(rec.MainData, offset, areas, 0, areas.Length);
            ReadData(ref areas, rec);
        }

        private void ReadComment(RecComment rec) // 10
        {
            try
            {
                int offset = 0;
                uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                int readed = 0;
                while (readed < len)
                {
                    string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                    ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                    string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                    rec.Comment.Add(new KeyValuePair<string, string>(lang, text));
                };
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint)) ((RecWaypoint)rec.Parent).Comment = rec;
                if ((rec.Parent != null) && (rec.Parent is RecCategory)) ((RecCategory)rec.Parent).Comment = rec;
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
        }

        private void ReadAddress(RecAddress rec) // 11
        {
            int offset = 0;
            rec.Flags = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
            try
            {                
                if (this.Version == "01")
                {
                    if ((rec.Flags & 0x0001) == 0x0001)
                    {
                        uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                        int readed = 0;
                        while (readed < len)
                        {
                            string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                            ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                            string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                            rec.aCity.Add(new KeyValuePair<string, string>(lang, text));
                        };
                    };
                    if ((rec.Flags & 0x0002) == 0x0002)
                    {
                        uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                        int readed = 0;
                        while (readed < len)
                        {
                            string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                            ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                            string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                            rec.aCountry.Add(new KeyValuePair<string, string>(lang, text));
                        };
                    };
                    if ((rec.Flags & 0x0004) == 0x0004)
                    {
                        uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                        int readed = 0;
                        while (readed < len)
                        {
                            string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                            ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                            string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                            rec.aState.Add(new KeyValuePair<string, string>(lang, text));
                        };
                    };
                    if ((rec.Flags & 0x0008) == 0x0008)
                    {
                        ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
                        string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen;
                        rec.Postal = text;
                    };
                    if ((rec.Flags & 0x0010) == 0x0010)
                    {
                        uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                        int readed = 0;
                        while (readed < len)
                        {
                            string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                            ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                            string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                            rec.aStreet.Add(new KeyValuePair<string, string>(lang, text));
                        };
                    };
                    if ((rec.Flags & 0x0020) == 0x0020)
                    {
                        ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
                        string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen;
                        rec.House = text;
                    };
                };
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint)) ((RecWaypoint)rec.Parent).Address = rec;
                
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
        }

        private void ReadContact(RecContact rec) // 12
        {
            int offset = 0;
            rec.Flags = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
            try
            {                
                if (this.Version == "01")
                {
                    if ((rec.Flags & 0x0001) == 0x0001)
                    {
                        uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                        int readed = 0;
                        while (readed < len)
                        {
                            string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                            ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                            string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                            rec.cPhone.Add(new KeyValuePair<string, string>(lang, text));
                        };
                    };
                    if ((rec.Flags & 0x0002) == 0x0002)
                    {
                        uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                        int readed = 0;
                        while (readed < len)
                        {
                            string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                            ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                            string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                            rec.cPhone2.Add(new KeyValuePair<string, string>(lang, text));
                        };
                    };
                    if ((rec.Flags & 0x0004) == 0x0004)
                    {
                        uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                        int readed = 0;
                        while (readed < len)
                        {
                            string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                            ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                            string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                            rec.cFax.Add(new KeyValuePair<string, string>(lang, text));
                        };
                    };
                    if ((rec.Flags & 0x0008) == 0x0008)
                    {
                        uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                        int readed = 0;
                        while (readed < len)
                        {
                            string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                            ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                            string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                            rec.cEmail.Add(new KeyValuePair<string, string>(lang, text));
                        };
                    };
                    if ((rec.Flags & 0x0010) == 0x0010)
                    {
                        uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                        int readed = 0;
                        while (readed < len)
                        {
                            string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                            ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                            string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                            rec.cWeb.Add(new KeyValuePair<string, string>(lang, text));
                        };
                    };     
                };
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint)) ((RecWaypoint)rec.Parent).Contact = rec;
                if ((rec.Parent != null) && (rec.Parent is RecCategory)) ((RecCategory)rec.Parent).Contact = rec;
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
        }

        private void ReadImage(RecImage rec) // 13
        {
            try
            {
                rec.Length = BitConverter.ToUInt32(rec.MainData, 1);
                rec.ImageData = new byte[rec.Length];
                if (rec.Length > 0)
                    Array.Copy(rec.MainData, 5, rec.ImageData, 0, rec.Length);
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
        }

        private void ReadDecription(RecDescription rec) // 14
        {
            try
            {
                int offset = 1;
                uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                int readed = 0;
                while (readed < len)
                {
                    string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                    ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                    string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                    rec.Description.Add(new KeyValuePair<string, string>(lang, text));
                };
                if ((rec.Parent != null) && (rec.Parent is RecWaypoint)) ((RecWaypoint)rec.Parent).Description = rec;
                if ((rec.Parent != null) && (rec.Parent is RecCategory)) ((RecCategory)rec.Parent).Description = rec;
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
        }

        private void ReadCopyright(RecCopyright rec) // 17
        {
            try
            {
                int offset = 0;
                rec.Flags1 = BitConverter.ToUInt16(rec.MainData, offset); offset += 2;
                rec.Flags2 = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; offset += 4;
                uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                int readed = 0;
                while (readed < len)
                {
                    string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                    ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                    string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                    rec.cDataSource.Add(new KeyValuePair<string, string>(lang, text));
                };
                len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                readed = 0;
                while (readed < len)
                {
                    string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                    ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                    string text = this.Encoding.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                    rec.cCopyrights.Add(new KeyValuePair<string, string>(lang, text));
                };
                this.cDataSource = rec.cDataSource;
                this.cCopyrights = rec.cCopyrights;
                if ((rec.Flags1 & 0x0400) == 0x0400)
                {
                    ushort tlen = BitConverter.ToUInt16(rec.MainData, offset); offset += 2; readed += 2;
                    string text = Encoding.ASCII.GetString(rec.MainData, offset, tlen); offset += tlen; readed += tlen;
                    rec.DeviceModel = text;
                };
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
        }

        private void ReadMedia(RecMedia rec) // 18
        {
            rec.MediaID = BitConverter.ToUInt16(rec.MainData, 0);
            rec.Format = rec.MainData[2];
            try
            {
                int offset = 3;
                int readed = 0;
                uint len = BitConverter.ToUInt32(rec.MainData, offset); offset += 4;
                while (readed < len)
                {
                    string lang = Encoding.ASCII.GetString(rec.MainData, offset, 2); offset += 2; readed += 2;
                    uint mlen = BitConverter.ToUInt32(rec.MainData, offset); offset += 4; readed += 4;
                    byte[] media = new byte[mlen];
                    Array.Copy(rec.MainData, offset, media, 0, mlen);
                    rec.Content.Add(new KeyValuePair<string, byte[]>(lang, media));
                };
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
        }

        private void ReadSpeedCamera(RecSpeedCamera rec) // 19
        {
            try
            {
                int offset = 0;
                byte[] buff = new byte[4];
                Array.Copy(rec.MainData, offset, buff, 0, 3); offset += 3;
                rec.cMaxLat = BitConverter.ToInt32(buff, 0);
                Array.Copy(rec.MainData, offset, buff, 0, 3); offset += 3;
                rec.cMaxLon = BitConverter.ToInt32(buff, 0);
                Array.Copy(rec.MainData, offset, buff, 0, 3); offset += 3;
                rec.cMinLat = BitConverter.ToInt32(buff, 0);
                Array.Copy(rec.MainData, offset, buff, 0, 3); offset += 3;
                rec.cMinLon = BitConverter.ToInt32(buff, 0);
                rec.Flags = rec.MainData[offset]; offset++;
                if (rec.Flags == 0x81) offset += 11;
                if ((rec.Flags == 0x80) || (rec.Flags > 0x81)) offset++;
                byte f10v = rec.MainData[offset]; offset++;
                if (rec.Flags == 0x81) offset++;
                offset += 1 + f10v;
                Array.Copy(rec.MainData, offset, buff, 0, 3); offset += 3;
                rec.cLat = BitConverter.ToInt32(buff, 0);
                Array.Copy(rec.MainData, offset, buff, 0, 3); offset += 3;
                rec.cLon = BitConverter.ToInt32(buff, 0);
            }
            catch (Exception ex)
            {
                rec.ReadError = ex;
            };
        }

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
        private Translitter ml = new Translitter();

        public string Name = "Exported Data";
        public string DataSource = "KMZRebuilder";

        private double MinLat = -90;
        private double MaxLat = 90;
        private double MinLon = -180;
        private double MaxLon = 180;
        private Color TransColor = Color.FromArgb(0xFE,0xFE,0xFE); // Almost white

        private List<string> Categories = new List<string>();
        private List<string> Styles = new List<string>();
        private Dictionary<uint, List<POI>> POIs = new Dictionary<uint, List<POI>>();
        private Dictionary<string, Image> Images = new Dictionary<string, Image>();

        public void AddPOI(string category, string name, string description, string style, double lat, double lon)
        {
            // Rebound
            if (Categories.Count == 0) { MinLat = 90; MaxLat = -90; MinLon = 180; MaxLon = -180; };

            POI poi = new POI(name, description, lat, lon);

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
        }

        public void AddImage(string style, Image im)
        {
            Bitmap nim = new Bitmap(im.Width, im.Height);
            Graphics g = Graphics.FromImage(nim);
            g.Clear(TransColor);
            g.DrawImage(im, new Point(0, 0));
            g.Dispose();
            if (Images.ContainsKey(style))
                Images[style] = nim;
            else
                Images.Add(style, nim);
        }

        private uint LatLonToZone(double lat, double lon)
        {
            short la = (short)lat;
            short lo = (short)lon;
            List<byte> lalo = new List<byte>();
            lalo.AddRange(BitConverter.GetBytes(la));
            lalo.AddRange(BitConverter.GetBytes(lo));
            return BitConverter.ToUInt32(lalo.ToArray(), 0);
        }

        private void ZoneToLatLon(uint zone, out double lat, out double lon)
        {
            byte[] lalo = BitConverter.GetBytes(zone);
            lat = BitConverter.ToInt16(lalo, 0);
            lon = BitConverter.ToInt16(lalo, 2);
        }

        public void Save(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            byte[] block;
            // Header0
            { 
                block = GetHeader0Block().Data;
                fs.Write(block, 0, block.Length);
            };
            // Header1
            { 
                block = GetHeader1Block().Data;
                fs.Write(block, 0, block.Length);
            };
            // POIs
            { 
                block = GetPOIGroupBlock().Data;
                fs.Write(block, 0, block.Length);
            };
            // Footer
            { 
                block = GetFooter().Data;
                fs.Write(block, 0, block.Length);
            };
            fs.Close();
        }

        private class POI
        {
            public string name;
            public string description;
            public double lat;
            public double lon;
            public int cat;
            public int sty;

            public POI() { }
            public POI(string name, double lat, double lon) { this.name = name; this.description = null; this.lat = lat; this.lon = lon; }
            public POI(string name, string description, double lat, double lon) { this.name = name; this.description = description; this.lat = lat; this.lon = lon; }
        }

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

        private FileBlock GetHeader0Block()
        {
            FileBlock fb = new FileBlock();
            fb.bType = 0x00;
            // MAIN
            {
                fb.MainData.AddRange(Encoding.ASCII.GetBytes("GRMREC01")); // Header Text
                TimeSpan tsec = DateTime.Now.Subtract(new DateTime(1990, 1, 1));
                uint sec = (uint)tsec.TotalSeconds;
                fb.MainData.AddRange(BitConverter.GetBytes(((uint)(sec)))); // Time
                fb.MainData.AddRange(new byte[] { 1, 0 }); // Must Have
                fb.MainData.AddRange(ToPString(String.IsNullOrEmpty(this.Name) ? "Exported Data" : this.Name, true)); // File Name
            };
            // EXTRA
            {
                FileBlock b15 = new FileBlock();
                b15.bType = 15;
                b15.MainData.AddRange(new byte[] { 1, 7, 9, 0, 0 }); // Must Have
                fb.ExtraData.AddRange(b15.Data);
            };
            return fb;
        }

        private FileBlock GetHeader1Block()
        {
            FileBlock fb = new FileBlock();
            fb.bType = 0x01;
            //Main
            {
                fb.MainData.AddRange(Encoding.ASCII.GetBytes("POI\0")); // Header Text
                fb.MainData.AddRange(new byte[] { 0, 0 }); // Reserved
                fb.MainData.AddRange(Encoding.ASCII.GetBytes("01")); // Version
                fb.MainData.AddRange(BitConverter.GetBytes(((ushort)(0xFDE9)))); //UTF-8 Encoding
                fb.MainData.AddRange(BitConverter.GetBytes(((ushort)(17)))); // Copyrights Exists
            };
            // Extra
            {
                FileBlock b17 = new FileBlock(); // COPYRIGHTS
                b17.bType = 17;
                b17.MainData.AddRange(new byte[] { 20, 0, 0, 0, 0, 0, 0, 0 }); // Must Have Data
                b17.MainData.AddRange(ToLString(String.IsNullOrEmpty(this.DataSource) ? "KMZRebuilder" : this.DataSource)); // Data Source
                b17.MainData.AddRange(ToLString("Created with KMZRebuilder")); // Copyrights
                b17.MainData.AddRange(new byte[] { 0x01, 0x01, 0xE7, 0x4E }); // Must Have
                fb.ExtraData.AddRange(b17.Data);
            };
            return fb;
        }

        private FileBlock GetPOIGroupBlock()
        {
            FileBlock fb = new FileBlock();
            fb.bType = 9;
            // Main
            {
                fb.MainData.AddRange(ToLString(String.IsNullOrEmpty(this.DataSource) ? "KMZRebuilder" : this.DataSource)); // POI Group Name
                fb.MainData.AddRange(GetMainAreaBlock().Data); // Main Area Block
            };
            // Extra
            {
                // List Categories
                for (int i = 0; i < Categories.Count; i++) fb.ExtraData.AddRange(GetCatBlock(i).Data);
                // List Bitmaps
                for (int i = 0; i < Styles.Count; i++) fb.ExtraData.AddRange(GetBmpBlock(i).Data);
            };
            return fb;
        }

        private FileBlock GetMainAreaBlock()
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
                    fb.ExtraData.AddRange(GetSubAreaBlock(zn.Key).Data);
            };
            return fb;
        }

        private FileBlock GetSubAreaBlock(uint zone)
        {
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
                    f08.ExtraData.AddRange(GetPOIBlock(poi).Data);
            };
            return f08;
        }

        private FileBlock GetPOIBlock(POI poi)
        {
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
                f02.ExtraData.AddRange(GetCatIDBlock(poi.cat).Data);
                // STYLE
                f02.ExtraData.AddRange(GetBmpIDBlock(poi.sty).Data);
                // Alert
                /* NO Alert */
                // Comment
                /* NO Comment */
                // Address
                /* NO Address */
                // Contact
                /* NO Contact */
                // Image
                /* NO Image */
                // DESC
                if (!String.IsNullOrEmpty(poi.description)) 
                    f02.ExtraData.AddRange(GetDescBlock(poi.description).Data);
            };
            return f02;
        }

        private FileBlock GetCatBlock(int cat)
        {
            FileBlock f07 = new FileBlock();
            f07.bType = 7;
            f07.MainData.AddRange(BitConverter.GetBytes((ushort)cat)); // ID
            f07.MainData.AddRange(ToLString(Categories[cat])); // Name
            return f07;
        }

        private FileBlock GetCatIDBlock(int cat)
        {
            FileBlock f06 = new FileBlock();
            f06.bType = 6;
            f06.MainData.AddRange(BitConverter.GetBytes((ushort)cat)); // ID
            return f06;
        }

        private FileBlock GetBmpBlock(int sty)
        {
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

        private FileBlock GetBmpIDBlock(int sty)
        {
            FileBlock f04 = new FileBlock();
            f04.bType = 4;
            f04.MainData.AddRange(BitConverter.GetBytes((ushort)sty)); // ID
            return f04;
        }

        private FileBlock GetDescBlock(string desc)
        {
            FileBlock f14 = new FileBlock();
            f14.bType = 14;
            f14.MainData.Add(1); // Reserved
            f14.MainData.AddRange(ToLString(desc)); // Text
            return f14;
        }

        private FileBlock GetFooter()
        {
            FileBlock fb = new FileBlock();
            fb.bType = 0xFFFF;
            return fb;
        }

        private Bitmap GetBitmapFromStyle(int sty, out List<Color> palette)
        {
            Bitmap im;
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
            ImageMagick.MagickImage mi = new ImageMagick.MagickImage(im);
            if ((im.Width > 32) || (im.Height > 32)) // im = ResizeImage(im, 32, 32);
                mi.Resize(32, 32);
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

        private byte[] ToPString(string value, bool translit) // Pascal-like String
        {
            List<byte> res = new List<byte>();
            byte[] tnArr = Encoding.UTF8.GetBytes(translit ? ml.Translit(value) : value);
            res.AddRange(BitConverter.GetBytes((ushort)tnArr.Length));
            res.AddRange(tnArr);
            return res.ToArray();
        }

        private byte[] ToLString(string value) // Multilang String
        {
            List<byte> res = new List<byte>();
            res.AddRange(Encoding.ASCII.GetBytes("EN"));
            res.AddRange(ToPString(value, true));
            res.AddRange(Encoding.ASCII.GetBytes("RU"));
            res.AddRange(ToPString(value, false));
            res.InsertRange(0, BitConverter.GetBytes((uint)res.Count));
            return res.ToArray();
        }

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

        private static uint ColorToUint(Color c)
        {
            uint res = (uint)(c.R + (c.G << 8) + (c.B << 16));
            return res;
        }
    }

    /// <summary>
    ///     Lang Translit (RU -> EN)
    /// </summary>
    public class Translitter
    {
        private Dictionary<string, string> words = new Dictionary<string, string>();

        public Translitter() { InitDict(); }

        private void InitDict()
        {
            words.Add("à", "a");
            words.Add("á", "b");
            words.Add("â", "v");
            words.Add("ã", "g");
            words.Add("ä", "d");
            words.Add("å", "e");
            words.Add("¸", "yo");
            words.Add("æ", "zh");
            words.Add("ç", "z");
            words.Add("è", "i");
            words.Add("é", "j");
            words.Add("ê", "k");
            words.Add("ë", "l");
            words.Add("ì", "m");
            words.Add("í", "n");
            words.Add("î", "o");
            words.Add("ï", "p");
            words.Add("ð", "r");
            words.Add("ñ", "s");
            words.Add("ò", "t");
            words.Add("ó", "u");
            words.Add("ô", "f");
            words.Add("õ", "h");
            words.Add("ö", "c");
            words.Add("÷", "ch");
            words.Add("ø", "sh");
            words.Add("ù", "sch");
            words.Add("ú", "j");
            words.Add("û", "i");
            words.Add("ü", "j");
            words.Add("ý", "e");
            words.Add("þ", "yu");
            words.Add("ÿ", "ya");
            words.Add("À", "A");
            words.Add("Á", "B");
            words.Add("Â", "V");
            words.Add("Ã", "G");
            words.Add("Ä", "D");
            words.Add("Å", "E");
            words.Add("¨", "Yo");
            words.Add("Æ", "Zh");
            words.Add("Ç", "Z");
            words.Add("È", "I");
            words.Add("É", "J");
            words.Add("Ê", "K");
            words.Add("Ë", "L");
            words.Add("Ì", "M");
            words.Add("Í", "N");
            words.Add("Î", "O");
            words.Add("Ï", "P");
            words.Add("Ð", "R");
            words.Add("Ñ", "S");
            words.Add("Ò", "T");
            words.Add("Ó", "U");
            words.Add("Ô", "F");
            words.Add("Õ", "H");
            words.Add("Ö", "C");
            words.Add("×", "Ch");
            words.Add("Ø", "Sh");
            words.Add("Ù", "Sch");
            words.Add("Ú", "J");
            words.Add("Û", "I");
            words.Add("Ü", "J");
            words.Add("Ý", "E");
            words.Add("Þ", "Yu");
            words.Add("ß", "Ya");

        }

        public string Translit(string text)
        {
            string res = text;
            foreach (KeyValuePair<string, string> pair in words) res = res.Replace(pair.Key, pair.Value);
            return res;
        }
    }    
}
