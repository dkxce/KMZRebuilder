using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KMZRebuilder
{
    public class ProGorodPOI
    {
        public static FavRecord[] ReadFile(string file)
        {
            List<FavRecord> records = new List<FavRecord>();
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            fs.Position = 2;
            byte[] data = new byte[4];
            fs.Read(data, 0, data.Length);
            int count = BitConverter.ToInt32(data, 0);
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(0x414);
            for (int i = 0; i < count; i++)
            {
                data = new byte[0x414];
                fs.Read(data, 0, data.Length);
                Marshal.Copy(data, 0, unmanagedPointer, data.Length);
                FavRecord rec = (FavRecord)Marshal.PtrToStructure(unmanagedPointer, typeof(FavRecord));
                records.Add(rec);
            };
            Marshal.FreeHGlobal(unmanagedPointer);
            fs.Close();
            return records.ToArray();
        }

        public static void WriteFile(string file, FavRecord[] POI)
        {
            if (POI == null) return;

            FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write);
            //header
            fs.WriteByte(1); fs.WriteByte(0);
            // write count
            byte[] data = BitConverter.GetBytes((int)POI.Length);
            fs.Write(data, 0, data.Length);
            if (POI.Length > 0)
            {
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(0x414);
                for (int i = 0; i < POI.Length; i++)
                {
                    data = new byte[0x414];
                    Marshal.StructureToPtr(POI[i], unmanagedPointer, true);
                    Marshal.Copy(unmanagedPointer, data, 0, data.Length);
                    fs.Write(data, 0, data.Length);
                };
                Marshal.FreeHGlobal(unmanagedPointer);
            };
            fs.Close();
        }

        public enum THomeOffice : uint
        {
            None = 0,
            Home = 1,
            Office = 2
        }

        public enum TType : uint
        {
            None = 0,
            Flower = 1,
            Home = 2,
            Note = 3,
            Building = 4,
            Parking = 5,
            Food = 6,
            Key = 7,
            Instrument = 8,
            Ok = 9,
            Point = 10,
            Coffee = 11,
            RestroomM = 12,
            RestroomF = 13,
            Shirt = 14,
            Car = 15,
            Plane = 16,
            Hand = 17,
            Book = 18,
            Shop = 19

        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 0x414)]
        public struct FavRecord
        {
            [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
            private THomeOffice pntType; //home-office, 1 - home, 2 - office, 4 байта
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            private byte[] head;
            [MarshalAs(UnmanagedType.I4, SizeConst = 4)]
            private int lon;
            [MarshalAs(UnmanagedType.I4, SizeConst = 4)]
            private int lat;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            private byte[] address;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            private byte[] name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            private byte[] phone;
            [MarshalAs(UnmanagedType.U4, SizeConst = 4)]
            private TType icon;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            private byte[] desc;

            public THomeOffice HomeOffice
            {
                get
                {
                    return pntType;
                }
                set
                {
                    pntType = value;
                }
            }
            public TType Icon
            {
                get
                { return icon; }
                set
                { icon = value; }
            }

            public double Lat
            {
                get
                {
                    return lat * .00001;
                }
                set
                {
                    lat = (int)(value / .00001);
                }
            }

            public double Lon
            {
                get
                {
                    return lon * .00001;
                }
                set
                {
                    lon = (int)(value / .00001);
                }
            }

            public string Address
            {
                get
                {
                    return System.Text.Encoding.Unicode.GetString(address).Trim('\0').Trim();
                }
                set
                {
                    byte[] arr = System.Text.Encoding.Unicode.GetBytes(value.Trim());
                    address = new byte[256];
                    Array.Copy(arr, address, arr.Length > 128 ? 128 : arr.Length);
                }
            }
            public string Name
            {
                get
                {
                    return System.Text.Encoding.Unicode.GetString(name).Trim('\0').Trim();
                }
                set
                {
                    byte[] arr = System.Text.Encoding.Unicode.GetBytes(value.Trim());
                    name = new byte[256];
                    Array.Copy(arr, name, arr.Length > 128 ? 128 : arr.Length);
                }
            }
            public string Phone
            {
                get
                {
                    return System.Text.Encoding.Unicode.GetString(phone).Trim('\0').Trim();
                }
                set
                {
                    byte[] arr = System.Text.Encoding.Unicode.GetBytes(value.Trim());
                    phone = new byte[256];
                    Array.Copy(arr, phone, arr.Length > 128 ? 128 : arr.Length);
                }
            }
            public string Desc
            {
                get
                {
                    return System.Text.Encoding.Unicode.GetString(desc).Trim('\0').Trim();
                }
                set
                {
                    byte[] arr = System.Text.Encoding.Unicode.GetBytes(value.Trim());
                    desc = new byte[256];
                    Array.Copy(arr, desc, arr.Length > 128 ? 128 : arr.Length);
                }
            }

            public override string ToString()
            {
                return ((uint)icon).ToString("00") + " " + Name + (String.IsNullOrEmpty(Desc) ? "" : " (" + Desc + ")");
            }
        }

        public class FavRecordSorter : IComparer<FavRecord>
        {
            public int Compare(FavRecord a, FavRecord b)
            {
                if ((a.HomeOffice == THomeOffice.Home) && (b.HomeOffice != THomeOffice.Home)) return -1;
                if ((b.HomeOffice == THomeOffice.Home) && (a.HomeOffice != THomeOffice.Home)) return 1;
                if ((a.HomeOffice == THomeOffice.Office) && (b.HomeOffice != THomeOffice.Office)) return -1;
                if ((b.HomeOffice == THomeOffice.Office) && (a.HomeOffice != THomeOffice.Office)) return 1;
                return a.Name.CompareTo(b.Name);
            }
        }
    }

    public class WPTPOI
    {
        public enum SymbolIcon : byte
        {
            x_00_Romb = 0,
            x_01_Deli = 1,
            x_02_Doll = 2,
            x_03_Dot  = 3,
            x_04_Skull = 4,
            x_05_Fish = 5,
            x_06_Fishes = 6,
            x_07_Kit = 7,
            x_08_Anchor = 8,
            x_09_Boat = 9,
            x_10_Home = 10,
            x_11_GasStation = 11,
            x_12_ManTree = 12,
            x_13_Stairs = 13,
            x_14_Kaktuz = 14,
            x_15_ArrUp = 15,
            x_16_SquaredS = 16,
            x_17_SquaredD = 17,
            x_18_Loading = 18,
            x_19_SquaredAnchor = 19,
            x_20_SquaredN = 20,
            x_21_Lamp = 21,
            x_22_Point = 22
        }

        public static WPTPOI FromLine(string line)
        {
            if (String.IsNullOrEmpty(line)) return null;
            string[] lines = line.Split(new char[] { ',' });
            WPTPOI poi = new WPTPOI();
            int l = lines.Length;
            if (l > poi.FIELDS.Length) l = poi.FIELDS.Length;
            for (int i = 0; i < l; i++)
                poi.FIELDS[i] = lines[i];
            return poi;
        }

        public static WPTPOI[] ReadFile(string file)
        {
            List<WPTPOI> res = new List<WPTPOI>();
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
            string line = sr.ReadLine();
            if (line.IndexOf("OziExplorer") >= 0)
            {
                sr.ReadLine(); sr.ReadLine(); sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (!String.IsNullOrEmpty(line))
                    {
                        WPTPOI wpt = WPTPOI.FromLine(line);
                        res.Add(wpt);
                    };
                };
            }
            sr.Close();
            fs.Close();
            return res.ToArray();
        }

        public static void WriteFile(string file, WPTPOI[] POI, string creator)
        {
            if (POI == null) return;

            FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
            //header
            sw.WriteLine("OziExplorer Waypoint File Version 1.0");
            sw.WriteLine("WGS 84");
            if(String.IsNullOrEmpty(creator))
                sw.WriteLine("Reserved 2");
            else
                sw.WriteLine("Created by "+creator);
            sw.WriteLine("Reserved 3");
            // write count
            if (POI.Length > 0)
            {
                for (int i = 0; i < POI.Length; i++)
                {
                    sw.Write((i + 1).ToString() + ", ");
                    for(int c=1;c<POI[i].FIELDS.Length;c++)
                        sw.Write(POI[i].FIELDS[c] + ", ");
                    sw.WriteLine();
                };                
            };
            sw.Close();
            fs.Close();
        }

        public enum FIELD: byte
        {
            Number = 0,
            Name = 1,
            Latitude = 2,
            Longitude = 3,
            Date = 4,
            Symbol = 5,
            Status = 6,
            MapDisplayFormat = 7,
            ForegroundColor = 8,
            BackgroundColor = 9,
            Description = 10,
            PointerDirection = 11,
            GarminDisplayFormat = 12,
            ProximityDistance = 13,
            Altitude = 14
        }

        public string[] FIELDS = new string[15] { "0", "", "0", "0", "", "0", "3", "0", "65535", "0", "", "0", "0", "0", "0" };

        public int Symbol
        {
            get
            {
                string s = FIELDS[(byte)FIELD.Symbol];
                if (String.IsNullOrEmpty(s)) return 0;
                int res = 0;
                int.TryParse(s, out res);
                return res;
            }
            set
            {
                FIELDS[(byte)FIELD.Symbol] = value.ToString();
            }
        }

        public int Number
        {
            get
            {
                if (String.IsNullOrEmpty(FIELDS[(byte)FIELD.Number])) return 0;
                return int.Parse(FIELDS[(byte)FIELD.Number].Trim());
            }
            set
            {
                FIELDS[(byte)FIELD.Number] = value.ToString();
            }
        }

        public string Name
        {
            get
            {
                return FIELDS[(byte)FIELD.Name];
            }
            set
            {
                FIELDS[(byte)FIELD.Name] = value.Trim().Replace(",", ";").Replace("\r", " ").Replace("\n", " ");
            }
        }

        public string Description
        {
            get
            {
                return FIELDS[(byte)FIELD.Description];
            }
            set
            {
                FIELDS[(byte)FIELD.Description] = value.Trim().Replace(",", ";").Replace("\r", " ").Replace("\n", " ");
                if (FIELDS[(byte)FIELD.Description].Length > 40)
                    FIELDS[(byte)FIELD.Description] = FIELDS[(byte)FIELD.Description].Remove(40);
            }
        }

        public double Latitude
        {
            get
            {
                if (String.IsNullOrEmpty(FIELDS[(byte)FIELD.Latitude])) return 0;
                return double.Parse(FIELDS[(byte)FIELD.Latitude].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            }
            set
            {
                FIELDS[(byte)FIELD.Latitude] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public double Longitude
        {
            get
            {
                if (String.IsNullOrEmpty(FIELDS[(byte)FIELD.Longitude])) return 0;
                return double.Parse(FIELDS[(byte)FIELD.Longitude].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            }
            set
            {
                FIELDS[(byte)FIELD.Longitude] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public double Altitude
        {
            get
            {
                if (String.IsNullOrEmpty(FIELDS[(byte)FIELD.Altitude])) return 0;
                return double.Parse(FIELDS[(byte)FIELD.Altitude].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            }
            set
            {
                FIELDS[(byte)FIELD.Altitude] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public bool __toTop = false;

        public class WPTPOISorter: IComparer<WPTPOI>
        {
            public int Compare(WPTPOI a, WPTPOI b)
            {
                if (a.__toTop && (!b.__toTop)) return -1;
                if (b.__toTop && (!a.__toTop)) return 1;
                return a.Name.CompareTo(b.Name);
            }
        }
    }
}
