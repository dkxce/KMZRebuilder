using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MapsForgeFileReader
{
    // Format: 
    // https://github.com/mapsforge/mapsforge/blob/master/docs/Specification-Binary-Map-File.md
    //
    public class MapsForgeReader
    {
        public FileStream mff = null;
        public KMZRebuilder.Utils.MyBitConverter mbc = new KMZRebuilder.Utils.MyBitConverter(false);

        public string FILE_TOP_HEADER = "";
        public int    FILE_HEADER_SIZE = 0;
        public int    FILE_VERSION = 0;
        public ulong  FILE_SIZE = 0;
        public ulong  FILE_CREATED = 0;
        public double BOUNDING_BOX_minLat = -90;
        public double BOUNDING_BOX_minLon = -180;
        public double BOUNDING_BOX_maxLat = 90;
        public double BOUNDING_BOX_maxLon = -180;
        public ushort TILE_SIZE = 0;
        public string FILE_PROJECTION = "";
        public byte   FILE_FLAGS = 0;
        public double FILE_START_POS_LAT = 0;
        public double FILE_START_POS_LON = 0;
        public byte   FILE_START_ZOOM_LVL = 0;
        public string FILE_LANGUAGE = "unknown";
        public string FILE_COMMENT = "";
        public string FILE_CREATED_BY = "";
        public List<KeyValuePair<string,string>> POI_TAGS = new List<KeyValuePair<string,string>>();
        public List<KeyValuePair<string, string>> WAY_TAGS = new List<KeyValuePair<string, string>>();
        public List<ZoomInterval> ZOOM_LEVELS = new List<ZoomInterval>();

        public MapsForgeReader(string file)
        {
            mff = new FileStream(file, FileMode.Open, FileAccess.Read);
            ReadHeader();
            CheckZoomIntervals();            
        }

        private void ReadHeader()
        {
            byte[] buff = new byte[20];
            mff.Read(buff, 0, buff.Length);
            FILE_TOP_HEADER = Encoding.ASCII.GetString(buff);
            if (FILE_TOP_HEADER != "mapsforge binary OSM")
                throw new IOException("Invalid File Format");

            buff = new byte[24];
            mff.Read(buff, 0, buff.Length);
            FILE_HEADER_SIZE = mbc.ToInt32(buff, 0);
            FILE_VERSION = mbc.ToInt32(buff, 4);
            FILE_SIZE = mbc.ToUInt64(buff, 8);
            FILE_CREATED = mbc.ToUInt64(buff, 16);

            buff = new byte[18];
            mff.Read(buff, 0, buff.Length);
            BOUNDING_BOX_minLat = mbc.ToInt32(buff, 0) / 1000000.0;
            BOUNDING_BOX_minLon = mbc.ToInt32(buff, 4) / 1000000.0;
            BOUNDING_BOX_maxLat = mbc.ToInt32(buff, 8) / 1000000.0;
            BOUNDING_BOX_maxLon = mbc.ToInt32(buff, 12) / 1000000.0;
            TILE_SIZE = mbc.ToUInt16(buff, 16);

            FILE_PROJECTION = ReadString();
            FILE_FLAGS = (byte)mff.ReadByte();
            if ((FILE_FLAGS & 0x80) > 0) // DEBUG
            {

            };
            if ((FILE_FLAGS & 0x40) > 0)
            {
                buff = new byte[8];
                mff.Read(buff, 0, buff.Length);
                FILE_START_POS_LAT = mbc.ToInt32(buff, 0) / 1000000.0;
                FILE_START_POS_LON = mbc.ToInt32(buff, 4) / 1000000.0;
            };
            if ((FILE_FLAGS & 0x20) > 0)
                FILE_START_ZOOM_LVL = (byte)mff.ReadByte();

            if ((FILE_FLAGS & 0x10) > 0)
                FILE_LANGUAGE = ReadString();

            if ((FILE_FLAGS & 0x08) > 0)
                FILE_COMMENT = ReadString();

            if ((FILE_FLAGS & 0x04) > 0)
                FILE_CREATED_BY = ReadString();
            
            buff = new byte[2];
            mff.Read(buff,0,buff.Length);
            int tags_count = mbc.ToUInt16(buff, 0);
            for(int i=0;i<tags_count;i++)
            {
                string[] tag = ReadString().Split(new char[] { '=' }, 2);
                POI_TAGS.Add(new KeyValuePair<string,string>(tag[0],tag[1]));
            };
            buff = new byte[2];
            mff.Read(buff, 0, buff.Length);
            tags_count = mbc.ToUInt16(buff, 0);
            for (int i = 0; i < tags_count; i++)
            {
                string[] tag = ReadString().Split(new char[] { '=' }, 2);
                WAY_TAGS.Add(new KeyValuePair<string, string>(tag[0], tag[1]));
            };
            int zLvl = (byte)mff.ReadByte();
            buff = new byte[19];
            for (int i = 0; i < zLvl; i++)
            {
                mff.Read(buff, 0, buff.Length);
                ZoomInterval zi = new ZoomInterval();
                zi.lvl = buff[0];
                zi.min = buff[1];
                zi.max = buff[2];
                zi.indexPos = zi.startPos = mbc.ToUInt64(buff, 3);
                if ((FILE_FLAGS & 0x80) > 0) zi.indexPos += 16;
                zi.subSize = mbc.ToUInt64(buff, 11);
                // method 1
                zi.minY = lat_2_tileY(BOUNDING_BOX_maxLat, zi.lvl);
                zi.maxY = lat_2_tileY(BOUNDING_BOX_minLat, zi.lvl);
                zi.maxX = lon_2_tileX(BOUNDING_BOX_maxLon, zi.lvl);
                zi.minX = lon_2_tileX(BOUNDING_BOX_minLon, zi.lvl);

                // method 2                
                //int[] txy = location2tile(BOUNDING_BOX_maxLat, BOUNDING_BOX_minLon, zi.lvl); // top,left
                //int[] dxy = location2tile(BOUNDING_BOX_minLat, BOUNDING_BOX_maxLon, zi.lvl); // bottom, right
                //zi.minY = txy[1];
                //zi.maxY = dxy[1];
                //zi.minX = txy[0];
                //zi.maxX = dxy[0];

                // method 3
                //GetTileXYFromLatLon(BOUNDING_BOX_maxLat, BOUNDING_BOX_minLon, zi.lvl, out zi.minX, out zi.minY); // top, left
                //GetTileXYFromLatLon(BOUNDING_BOX_minLat, BOUNDING_BOX_maxLon, zi.lvl, out zi.maxX, out zi.maxY); // bottom, right

                ZOOM_LEVELS.Add(zi);
            };
        }

        private void CheckZoomIntervals()
        {
            byte[] buff;
            foreach (ZoomInterval zi in ZOOM_LEVELS)
            {
                int _additH = 0;
                buff = new byte[16];
                mff.Position = (long)zi.startPos;
                if ((FILE_FLAGS & 0x80) > 0) // DEBUG
                {
                    _additH = 16;
                    mff.Read(buff, 0, buff.Length);
                    zi.IndexHeader = Encoding.ASCII.GetString(buff);
                };
                byte ief = (byte)mff.ReadByte();
                long add = ((byte)ief & 0x7F) << 32;
                buff = new byte[4];
                mff.Read(buff, 0, buff.Length);
                long first_tile_offset = mbc.ToUInt32(buff, 0) + add;
                if (first_tile_offset != (_additH + 5 * zi.tilesCount))
                    throw new Exception("Wrong tiles count at zoom " + zi.lvl.ToString());
            };
        }

        public void ReadZooms()
        {
            Console.WriteLine("0%");
            int ttlPOI = 0;
            int ttlWAY = 0;
            int pp = 0;
            foreach (ZoomInterval zi in ZOOM_LEVELS)
                for (int y = 0; y < zi.tilesHeigth; y++)
                    for (int x = 0; x < zi.tilesWidth; x++)
                    {
                        ZoomInterval.Tile tile = zi.ReadTileFromZero(x, y, true, true, this);
                        if (tile == null) continue;
                        ttlPOI += tile.POIs.Count;
                        ttlWAY += tile.WAYShorts.Count;
                        int cp = (int)(((double)mff.Position / (double)mff.Length) * 100.0);
                        if (cp != pp)
                        {
                            Console.CursorTop = Console.CursorTop - 1;
                            Console.WriteLine(pp + "%");
                        };
                        pp = cp;
                    };
            Console.CursorTop = Console.CursorTop - 1;
            Console.WriteLine("100%");
            Console.WriteLine("Total POIs: " + ttlPOI.ToString());
            Console.WriteLine("Total WAYs: " + ttlWAY.ToString());
            Console.ReadLine();
        }


        public ulong ReadUnsigned()
        {
            ulong res = 0;
            int rb = 0;
            int rc = 0;
            do
            {
                rb = mff.ReadByte();
                if (rb == -1) break;
                res += (ulong)((byte)(rb & 0x7F) << 7 * rc);
                rc++;
                if (rc == 8) break;
            }
            while (((byte)rb & 0x80) > 0);
            return res;
        }

        public long ReadSigned()
        {
            long res = 0;
            int rb = 0;
            int rc = 0;
            bool rn;
            do
            {
                rb = mff.ReadByte();
                if (rb == -1) break;
                rn = ((byte)rb & 0x80) > 0;
                if (rn)
                    res += (long)((byte)(rb & 0x7F) << 7 * rc);
                else
                {
                    res += (long)((byte)(rb & 0x3F) << 7 * rc);
                    if ((rb & 0x40) > 0) res *= -1;
                };
                rc++;
                if (rc == 8) break;
            }
            while (rn);
            return res;
        }

        public string ReadString()
        {
            ulong _textSize = ReadUnsigned();
            byte[] buff = new byte[_textSize];
            if ((mff.Position + (long)_textSize) > mff.Length)
                return null;
            mff.Read(buff, 0, buff.Length);
            return Encoding.UTF8.GetString(buff);
        }

        public string ReadString(long maxPos)
        {
            ulong _textSize = ReadUnsigned();
            if (_textSize > 10240)
                return null;
            if ((mff.Position + (long)_textSize) > (maxPos + 1))
                return null;
            if ((mff.Position + (long)_textSize) > mff.Length)
                return null;
            byte[] buff = new byte[_textSize];
            mff.Read(buff, 0, buff.Length);
            return Encoding.UTF8.GetString(buff);
        }

        public void Close()
        {
            if(mff != null)
                mff.Close();
            mff = null;
        }

        public static int lon_2_tileX(double lon, int z)
        {
            return (int)(Math.Floor((lon + 180.0) / 360.0 * (1 << z)));
        }

        public static int lat_2_tileY(double lat, int z)
        {
            double res = Math.Floor((1 - Math.Log(Math.Tan(ToRadians(lat)) + 1 / Math.Cos(ToRadians(lat))) / Math.PI) / 2 * (1 << z));
            return (int)res;
        }

        public static double tileX_2_lon(int x, int z)
        {
            return x / (double)(1 << z) * 360.0 - 180;
        }

        public static double tileY_2_lat(int y, int z)
        {
            double n = Math.PI - 2.0 * Math.PI * y / (double)(1 << z);
            return 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
        }

        public static double[] tileXY_to_LonLat(double x, double y, int zoom)
        {
            double Lng = ((x * 256) - (256 * Math.Pow(2, zoom - 1))) / ((256 * Math.Pow(2, zoom)) / 360);
            while (Lng > 180) Lng -= 360;
            while (Lng < -180) Lng += 360;

            double exp = ((y * 256) - (256 * Math.Pow(2, zoom - 1))) / ((-256 * Math.Pow(2, zoom)) / (2 * Math.PI));
            double Lat = ((2 * Math.Atan(Math.Exp(exp))) - (Math.PI / 2)) / (Math.PI / 180);
            if (Lat < -90) Lat = -90;
            if (Lat > 90) Lat = 90;

            return new double[] { Lng, Lat };
        }

        public static int[] LatLon_2_tileXY(double lat, double lon, int zoom)
        {
            if (System.Math.Abs(lat) > 85.0511287798066) return new int[] { 0, 0 };

            double sin_phi = System.Math.Sin(lat * System.Math.PI / 180);
            double norm_x = lon / 180;
            double norm_y = (0.5 * System.Math.Log((1 + sin_phi) / (1 - sin_phi))) / System.Math.PI;
            return new int[] { (int)(System.Math.Pow(2, zoom) * ((norm_x + 1) / 2)), (int)(System.Math.Pow(2, zoom) * ((1 - norm_y) / 2)) };
        }

        public void GetTileXYFromLatLon(double lat, double lon, int zoom, out int x, out int y)
        {
            x = (int)((lon + 180.0) / 360.0 * Math.Pow(2.0, zoom));
            y = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * Math.Pow(2.0, zoom));
        }

        // top left
        public static void GetLatLonFromTileXY(int x, int y, int zoom, out double lat, out double lon)
        {
            lon = ((x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
            double n = Math.PI - ((2.0 * Math.PI * y) / Math.Pow(2.0, zoom));
            lat = (180.0 / Math.PI * Math.Atan(Math.Sinh(n)));
        }

        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public class ZoomInterval
        {
            public class POI
            {
                public double lat;
                public double lon;
                public byte layer;
                public string name;
                public string addr;
                public int elev;
                public List<int> tags = new List<int>();
            }

            public class WAYSHORT
            {
                public double lat;
                public double lon;
                public byte layer;
                public string name;
                public string addr;
                public string refer;
                public List<int> tags = new List<int>();
            }

            public class ZoomTable
            {
                public byte zoom;
                public int pois;
                public int ways;
            }

            public class Tile
            {
                public long indexOffset = 0;
                public long tileOffset = 0;
                public long waysOffset = 0;

                public bool coveredByWater = false;
                public ZoomTable[] zoomTable = null;
                public int totalPOIs = 0;
                public List<POI> POIs = new List<POI>();
                public int totalWAYs = 0;
                public List<WAYSHORT> WAYShorts = new List<WAYSHORT>();
            }

            public byte lvl;
            public byte min;
            public byte max;
            public ulong startPos;
            public ulong indexPos;
            public ulong subSize;

            public int minX;
            public int minY;
            public int maxX;
            public int maxY;

            public int tilesWidth { get { return (int)Math.Abs(maxX - minX) + 1; }}
            public int tilesHeigth { get { return (int)Math.Abs(maxY - minY) + 1; } }
            public int tilesCount { get { return tilesWidth * tilesHeigth; } }

            public string IndexHeader;

            public Tile ReadTileFromXY(int x, int y, bool readPOIs, bool readWAYs, MapsForgeReader reader)
            {
                return ReadTileFromZero(x - minX, y - minY, readPOIs, readWAYs, reader);
            }

            public Tile ReadTileFromZero(int x, int y, bool readPOIs, bool readWAYs, MapsForgeReader reader)
            {                
                double tile_lat = MapsForgeReader.tileY_2_lat(minY + y, lvl);
                double tile_lon = MapsForgeReader.tileX_2_lon(minX + x, lvl);
                //double[] lola = tileXY_to_LonLat(minX + x, minY + y, lvl);
                //GetLatLonFromTileXY(minX + x, minY + y, lvl, out tile_lat, out tile_lon);

                Tile tile = new Tile();                
                tile.indexOffset = (long)indexPos + (long)(tilesWidth * 5 * y + 5 * x);
                reader.mff.Position = tile.indexOffset;

                if (reader.mff.Length == reader.mff.Position)
                    return null; // CHECK OVERFLOW (no more tiles exists)

                byte ief = (byte)reader.mff.ReadByte();
                tile.coveredByWater = ((byte)ief & 0x80) > 0;
                long add = ((byte)ief & 0x7F) << 32;
                byte[] buff = new byte[4];
                reader.mff.Read(buff, 0, buff.Length);
                KMZRebuilder.Utils.MyBitConverter mbc = new KMZRebuilder.Utils.MyBitConverter(false);
                long rto = mbc.ToUInt32(buff, 0) + add;
                tile.tileOffset = (long)this.startPos + rto;
                reader.mff.Position = tile.tileOffset;                

                if ((reader.FILE_FLAGS & 0x80) > 0) // DEBUG
                    reader.mff.Position += 32;

                if (reader.mff.Length == reader.mff.Position)
                    return null; // CHECK OVERFLOW (no more tiles exists)

                tile.zoomTable = new ZoomTable[this.max - this.min + 1];                
                for (int z = 0; z < tile.zoomTable.Length; z++)
                {
                    tile.zoomTable[z] = new ZoomTable();
                    tile.zoomTable[z].zoom = (byte)(this.min + z);                    
                    tile.totalPOIs += tile.zoomTable[z].pois = (int)reader.ReadUnsigned();
                    tile.totalWAYs += tile.zoomTable[z].ways = (int)reader.ReadUnsigned();
                };

                if (reader.mff.Length == reader.mff.Position)
                    throw new Exception("No Tile POIs and WAYs");

                tile.waysOffset = (long)reader.ReadUnsigned() + reader.mff.Position; // first_WAY_OFFSET
                //++ READ POIs ++//
                if (readPOIs)
                {
                    if (reader.mff.Length == reader.mff.Position)
                        throw new Exception("No POIs found for Tile");
                    for (int i = 0; i < tile.totalPOIs; i++)
                    {
                        if ((reader.FILE_FLAGS & 0x80) > 0) // DEBUG
                            reader.mff.Position += 32;

                        POI p = new POI();

                        double lat_diff = reader.ReadSigned() / 1000000.0;
                        double lon_diff = reader.ReadSigned() / 1000000.0;
                        p.lat = tile_lat + lat_diff;
                        p.lon = tile_lon + lon_diff;
                        byte special = (byte)reader.mff.ReadByte();
                        p.layer = (byte)(((special & 0xF0) >> 4) - 5);
                        byte tags = (byte)(special & 0x0F);
                        for (int t = 0; t < tags; t++)
                            p.tags.Add((int)reader.ReadUnsigned());
                        byte flags = (byte)reader.mff.ReadByte();
                        if ((flags & 0x80) > 0) p.name = reader.ReadString();
                        if ((flags & 0x40) > 0) p.addr = reader.ReadString();
                        if ((flags & 0x20) > 0) p.elev = (int)reader.ReadSigned();
                        tile.POIs.Add(p);
                    };
                };
                //-- READ POIs --//
                //++ READ WAYs ++//
                if (readWAYs)
                {
                    if (reader.mff.Position != tile.waysOffset) reader.mff.Position = tile.waysOffset;
                    if (reader.mff.Length == reader.mff.Position)
                        throw new Exception("No WAYs found for Tile");
                    for (int i = 0; i < tile.totalWAYs; i++)
                    {
                        if ((reader.FILE_FLAGS & 0x80) > 0) // DEBUG
                            reader.mff.Position += 32;

                        WAYSHORT ws = new WAYSHORT();

                        ulong way_data_size = reader.ReadUnsigned();
                        long _nextpos_ = reader.mff.Position + (long)way_data_size;

                        buff = new byte[3];
                        reader.mff.Read(buff, 0, buff.Length);
                        ushort sub_tile_bitmap = mbc.ToUInt16(buff, 0);
                        byte special = buff[2];

                        byte way_layer = (byte)(((special & 0xF0) >> 4) - 5);
                        byte tags = (byte)(special & 0x0F);
                        for (int t = 0; t < tags; t++)
                            ws.tags.Add((int)reader.ReadUnsigned());
                        byte flags = (byte)reader.mff.ReadByte();

                        if ((flags & 0x80) > 0)
                        {
                            ws.name = reader.ReadString(_nextpos_);
                            if (ws.name == null)
                            {
                                reader.mff.Position = _nextpos_;
                                continue;
                            };
                        };
                        if ((flags & 0x40) > 0)
                        {
                            ws.addr = reader.ReadString(_nextpos_);
                            if (ws.addr == null)
                            {
                                reader.mff.Position = _nextpos_;
                                continue;
                            };
                        };
                        if ((flags & 0x20) > 0)
                        {
                            ws.refer = reader.ReadString(_nextpos_);
                            if (ws.refer == null)
                            {
                                reader.mff.Position = _nextpos_;
                                continue;
                            };
                        };
                        if ((flags & 0x10) > 0)
                        {
                            ws.lat = reader.ReadSigned() / 1000000.0;
                            ws.lon = reader.ReadSigned() / 1000000.0;                            
                        };
                        int number_of_blocks = ((flags & 0x08) > 0) ? (int)reader.ReadUnsigned() : 1;
                        int wcb_enc = ((flags & 0x04) > 0) ? 1 : 0;
                        //++  WAY DATA ++//     
                        if (((flags & 0x10) > 0) && (ws.name != null))
                        {
                            ulong number_of_way_coordinate_blocks = reader.ReadUnsigned();
                            if (number_of_way_coordinate_blocks > 0)
                            {
                                ulong amount_of_way_nodes = reader.ReadUnsigned();
                                if (amount_of_way_nodes > 0)
                                {
                                    ws.lat = (tile_lat + reader.ReadSigned() / 1000000.0) + ws.lat;
                                    ws.lon = (tile_lon + reader.ReadSigned() / 1000000.0) + ws.lon;
                                    tile.WAYShorts.Add(ws);
                                };
                            };
                        };
                        //-- WAY DATA --//
                        reader.mff.Position = _nextpos_;
                    };
                };
                //-- READ WAYs --//
                return tile;
            }
        }
    }
}
