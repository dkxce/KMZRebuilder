using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KMZRebuilder
{
    public class NavitelGDB
    {
        private static byte[] gdb_header = new byte[] { 77, 115, 82, 99, 102, 0, 2, 0, 0, 0, 68, 109, 0, 26, 0, 0, 0, 65, 88, 2, 75, 71, 0, 65, 112, 114, 32, 32, 53, 32, 50, 48, 49, 57, 0, 49, 57, 58, 48, 51, 58, 51, 57, 0, 77, 97, 112, 83, 111, 117, 114, 99, 101, 0 };

        public static void WriteFile(string fileName, NavitelRecord[] records)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            fs.Write(gdb_header, 0, gdb_header.Length);
            for (int i = 0; i < records.Length; i++)
            {
                byte[] rec = records[i].ToRecord();
                fs.Write(BitConverter.GetBytes((int)rec.Length), 0, 4);
                fs.WriteByte((byte)'W');
                fs.Write(rec, 0, rec.Length);
            };
            {
                fs.Write(BitConverter.GetBytes((int)2), 0, 4);
                fs.WriteByte((byte)'V');
                fs.WriteByte(0);
                fs.WriteByte(1);
            };
            fs.Close();
        }

        public static NavitelRecord[] ReadFile(string fileName)
        {
            List<NavitelRecord> nrecords = new List<NavitelRecord>();

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            // SIGNATURE
            byte[] buffer = new byte[4];
            fs.Read(buffer, 0, 4); // 00 .. 03
            string str = System.Text.Encoding.ASCII.GetString(buffer);

            // Primary File Format
            buffer = new byte[2];
            fs.Read(buffer, 0, 2); // 04 .. 05
            ushort version = BitConverter.ToUInt16(buffer, 0); // 0x0066 GDB

            // RECORD D
            buffer = new byte[5];
            fs.Read(buffer, 0, 5); // 06 .. 10
            uint r_length = BitConverter.ToUInt32(buffer, 0);
            char r_type = (char)buffer[4];
            buffer = new byte[r_length];
            fs.Read(buffer, 0, buffer.Length);
            ushort d_version = BitConverter.ToUInt16(buffer, 0); // 0x06D MapSource (*.gdb) 6.12.2-6.16.3 “Ver 3” // 0109 // File Version 1.9

            // Record A
            buffer = new byte[5];
            fs.Read(buffer, 0, 5); // 06 .. 10
            r_length = BitConverter.ToUInt32(buffer, 0);
            r_type = (char)buffer[4];
            buffer = new byte[r_length];
            fs.Read(buffer, 0, buffer.Length);
            ushort a_version = BitConverter.ToUInt16(buffer, 0); //
            string a_info = System.Text.Encoding.ASCII.GetString(buffer, 2, buffer.Length - 2);

            // Application Field
            string app_fld = "";
            int rb = -1;
            while ((rb = fs.ReadByte()) > 0)
                app_fld += (char)rb;

            // RECORDS
            while (fs.Position != fs.Length)
            {
                buffer = new byte[5];
                fs.Read(buffer, 0, 5); // 06 .. 10
                r_length = BitConverter.ToUInt32(buffer, 0);
                r_type = (char)buffer[4];
                byte[] record_data = new byte[r_length];
                fs.Read(record_data, 0, record_data.Length);

                if (r_type == 'W') // WayPoint
                {
                    int frm = 0;
                    int el = 0;
                    while (record_data[el] != 0) el++;
                    string wpt_name = System.Text.Encoding.UTF8.GetString(record_data, 0, el - frm); el++;

                    uint wpt_class = BitConverter.ToUInt32(record_data, el); el += 4;

                    el += 23;
                    // 20 20 20 20 20 20 20 20 20 ff ff ff ff ff ff ff ff 20 20 ff ff ff ff  

                    double lat = BitConverter.ToInt32(record_data, el) * 360.0 / Math.Pow(2, 32); el += 4;
                    double lon = BitConverter.ToInt32(record_data, el) * 360.0 / Math.Pow(2, 32); el += 4;

                    byte alt_f = record_data[el]; el++;
                    double alt = 0;
                    if (alt_f == 1)
                        alt = BitConverter.ToDouble(record_data, el); el += 8;

                    frm = el;
                    while (record_data[el] != 0) el++;
                    string wpt_comment = System.Text.Encoding.UTF8.GetString(record_data, frm, el - frm); el++;

                    byte prox_f = record_data[el]; el++;
                    double prox = 0;
                    if (prox_f == 1)
                        prox = BitConverter.ToDouble(record_data, el); el += 8;

                    uint wpt_display = BitConverter.ToUInt32(record_data, el); el += 4;
                    uint wpt_color = BitConverter.ToUInt32(record_data, el); el += 4;
                    uint wpt_icon = BitConverter.ToUInt32(record_data, el); el += 4;

                    if (wpt_icon == 0)
                    {
                        byte[] _bw = new byte[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 130, 173, 161, 93, 0, 0, 0, 0, 0, 0 };
                        el += _bw.Length;
                    }
                    else
                    {
                        byte[] _bw = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 130, 173, 161, 93, 0, 0, 0, 0, 0, 0 };
                        el += _bw.Length;
                    };

                    nrecords.Add(new NavitelRecord(lat, lon, wpt_icon, wpt_name, wpt_comment));
                }
                else if (r_type == 'V') // Map Set Name
                {
                    int frm = 0;
                    int el = 0;
                    while (record_data[el] != 0) el++;
                    string wpt_name = System.Text.Encoding.UTF8.GetString(record_data, 0, el - frm); el++;
                    byte auto_name = record_data[el]; // == 1
                };

            };

            fs.Close();

            return nrecords.ToArray();
        }

        public class NavitelRecordSorter : IComparer<NavitelRecord>
        {
            public int Compare(NavitelRecord a, NavitelRecord b)
            {
                if (a.__toTop && (!b.__toTop)) return -1;
                if (b.__toTop && (!a.__toTop)) return 1;
                return a.name.CompareTo(b.name);
            }
        }
    }

    public class NavitelRecord
    {
        public static string[] IconList = new string[] { "Flag", "Bell", "DiamondGrn", "DiamondRed", "Dive1", "Dive2", "Dollar", "Fish", "Fuel", "Horn", "House", "Knife", "Light", "Mug", "Skull", "SquareGrn", "SquareRed", "Wuoy", "WptDot", "Wreck", "Mob", "BuoyAmbr", "BuoyBlck", "BuoyBlue", "BuoyGrn", "BuoyGrnRed", "BuoyGrnWh", "BuoyOrng", "BuoyRed", "BuoyRedGrn", "BuoyRedWh", "BuoyViolet", "BuoyWht", "BuoyWhtGrn", "BuoyWhtRe", "Dot", "Rbcn", "BoatRamp", "Camp", "Restrooms", "Showers", "DrinkingW", "Phone", "1stAid", "Info", "Parking", "Park", "Picnic", "Senic", "Skiing", "Swimming", "Dam", "Controlled", "Danger", "Restricted", "Ball", "Car", "Deer", "ShpngCar", "Lodging", "Mine", "TrailHead", "TruckStop", "UserExit", "Flg", "CircleX", "MiMrkr", "Trcbck", "Golf", "SmlCty", "MedCty", "LrgCty", "CapCty", "AmusePk", "Bowling", "CarRental", "CarRepair", "Fastfood", "Fitness", "Movie", "Museu", "Pharmacy", "Pizza", "PostOfc", "RvPark", "School", "Stadium", "Store", "Zoo", "GasPlus", "Faces", "WeighSttn", "TollBooth", "Bridge", "Building", "Cemetery", "Church", "Civil", "Crossing", "HistTown", "Levee", "Military", "OilField", "Tunnel", "Beach", "Forest", "Summit", "Airport", "Heliport", "Private", "SoftFld", "TallTower", "ShotTower", "Glider", "Ultralight", "Parachute", "Seaplane", "Geocache", "GeocacheFoun", "ContactAfro", "ContactAlien", "ContactBalCap", "ContactBigEar", "ContactBiker", "ContactBug", "ContactCat", "ContactDog", "ContactDreads", "ContactFem1", "ContactFem2", "ContactFem3", "ContactGoatee", "ContactKungFu", "ContactPig", "ContactPiate", "ContactRanger", "ContactSmiley", "ContactSpike", "ContactSumo", "WaterHydrant", "FlagRed", "FlagBlue", "FlagGreen", "PinRed", "PinBlue", "PinGreen", "BlockRed", "BlockBlue", "BlockGreen", "BikeTrail", "FHSFacility", "PoliceStation", "SkiResort", "IceSkating", "Wrecker", "NoAnchor", "Beacon", "CoastGuard", "Reef", "WeedBed", "DropOff", "Dock", "Marina", "BaitTackle", "Stump", "CircleRed", "CircleGreen", "CircleBlue", "DiamondBlue", "OvalRed", "OvalGreen", "OvalBlue", "RectRed", "RectGreen", "RectBlue", "SquareBlue", "LetterARed", "LetterAGreen", "LetterABlue", "LetterBRed", "LetterBGreen", "LetterBBlue", "LetterCRed", "LetterCGreen", "LetterCBlue", "LtterDRed", "LetterDGreen", "LetterDBlue", "Number0Red", "Number0Green", "Number0Blue", "Number1Red", "Number1Green", "Number1Blue", "Number2Red", "Number2Green", "Nmber2Blue", "Number3Red", "Number3Green", "Number3Blue", "Number4Red", "Number4Green", "Number4Blue", "Number5Red", "Number5Green", "Number5Blue", "Number6Red", "Nuber6Green", "Number6Blue", "Number7Red", "Number7Green", "Number7Blue", "Number8Red", "Number8Green", "Number8Blue", "Number9Red", "Number9Green", "Number9Blue", "TriangleBlue", "TriangleGreen", "TrangleRed", "ContactBlonde", "ContactClown", "ContactGlasses", "ContactPanda", "MultiCache", "LetterboxCache", "PuzzleCache", "Library", "BusStation", "CityHall", "Winry", "ATV", "BigGame", "Blind", "BloodTrail", "Cover", "Covey", "FoodSource", "Furbearer", "Lodge", "SmallGame", "AnimalTracks", "TreedQuarry", "TreeStand", "Truck", "UplndGame", "Waterfowl", "WaterSource" };

        public double lat = 0;
        public double lon = 0;
        public string name = "";
        public string desc = null;
        public uint iconIndex = 0;
        public bool __toTop = false;

        public NavitelRecord() { }

        public NavitelRecord(double lat, double lon, uint icon, string name, string desc)
        {
            this.lat = lat; this.lon = lon; this.name = name; this.desc = desc; this.iconIndex = icon;
            if ((this.name != null) && (this.name.StartsWith(" "))) this.name = this.name.Substring(1);
            if ((this.desc != null) && (this.desc.StartsWith(" "))) this.desc = this.desc.Substring(1);
        }

        public override string ToString()
        {
            return name + " " + lat.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + lon.ToString(System.Globalization.CultureInfo.InvariantCulture) + " [" + iconName + "]";
        }

        public byte[] ToRecord()
        {
            List<byte> result = new List<byte>();
            if (!String.IsNullOrEmpty(name))
                result.AddRange(System.Text.Encoding.UTF8.GetBytes(" " + name)); // name
            else
                result.AddRange(System.Text.Encoding.UTF8.GetBytes(" ")); // name
            result.Add(0);
            result.AddRange(new byte[] { 0, 0, 0, 0 }); // class
            result.AddRange(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0, 0x0, 0xff, 0xff, 0xff, 0xff });
            result.AddRange(BitConverter.GetBytes((int)(lat * Math.Pow(2, 32) / 360.0))); // lat
            result.AddRange(BitConverter.GetBytes((int)(lon * Math.Pow(2, 32) / 360.0))); // lon
            result.Add(1); // alt_f
            result.AddRange(BitConverter.GetBytes((double)0)); // alt        
            if (!String.IsNullOrEmpty(desc))
                result.AddRange(System.Text.Encoding.UTF8.GetBytes(" " + desc)); // name
            result.Add(0);

            result.Add(1); // prox_f
                result.AddRange(BitConverter.GetBytes((double)-1.0)); // prox

            if (iconIndex == 0)
                result.AddRange(BitConverter.GetBytes((uint)18)); // wpt_display
            else
                result.AddRange(BitConverter.GetBytes((uint)1)); // wpt_display
            result.AddRange(BitConverter.GetBytes((uint)0)); // wpt_color
            result.AddRange(BitConverter.GetBytes((uint)iconIndex)); // wpt_icon

            if (iconIndex == 0)
            {
                byte[] _bw = new byte[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 130, 173, 161, 93, 0, 0, 0, 0, 0, 0 };
                result.AddRange(_bw);
            }
            else
            {
                byte[] _bw = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 130, 173, 161, 93, 0, 0, 0, 0, 0, 0 };
                result.AddRange(_bw);
            };

            return result.ToArray();
        }

        public string iconName
        {
            get
            {
                if (iconIndex < IconList.Length)
                    return IconList[iconIndex];
                else
                    return null;
            }
            set
            {
                int ic = ((new List<string>(IconList)).IndexOf(value));
                if (ic >= 0) iconIndex = (uint)ic;
            }
        }

        public static string IconText(int index)
        {
            try
            {
                return index.ToString("000") + " - " + NavitelRecord.IconList[index];
            }
            catch
            {
                return null;
            };
        }
    }
}
