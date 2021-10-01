using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FitParser.Core
{
    public sealed class MessageDefinition
    {
        private readonly byte _header;
        private readonly byte _architecture;

        public byte LocalMessageNumber
        {
            get { return (byte)(_header & 0xF); }
        }

        public List<FieldDefinition> FieldDefinitions;

        public ushort GlobalMessageNumber;

        public int Size;

        public int MessageDefinitionSize
        {
            get { return FieldDefinitions.Count * 3 + 5; }
        }

        public bool IsLittleEndian
        {
            get { return _architecture == 0; }
        }

        public MessageDefinition(byte header, BinaryReader reader)
        {
            _header = header;

            byte reserved = reader.ReadByte();

            // 0 == Definition and Data messages are little-endian
            // 1 == Definition and Data messages are big-endian

            // TODO: do something here to deal with endian-ness on subsequent parsing
            _architecture = reader.ReadByte();

            // Range of message numbers 0:65535
            // TODO: understand what this field really means
            GlobalMessageNumber = reader.ReadUInt16();

            // Parse the all of the fields in the definition message
            byte fieldCount = reader.ReadByte();
            int currentOffset = 0;
            FieldDefinitions = new List<FieldDefinition>();
            for (int i = 0; i < fieldCount; i++)
            {
                FieldDefinition fieldDefinition = new FieldDefinition(reader, ref currentOffset);
                FieldDefinitions.Add(fieldDefinition);
            }

            Size = currentOffset;
        }
    }

    public sealed class FieldDefinition
    {
        public int FieldDefinitionNumber;

        public int FieldOffset;

        public int FieldType;

        public FieldDefinition(BinaryReader reader, ref int currentOffset)
        {
            FieldDefinitionNumber = reader.ReadByte();
            int fieldSize = reader.ReadByte();
            FieldOffset = currentOffset;
            FieldType = reader.ReadByte();
            currentOffset += fieldSize;
        }
    }

    public sealed class FileHeader
    {
        public byte FITHeaderSize;
        public byte ProtocolVersion;
        public ushort ProfileVersion;
        public uint FITDataSize;
        public ushort CRC;
        private bool _validHeader = false;

        public FileHeader(BinaryReader reader)
        {
            FITHeaderSize = reader.ReadByte();
            ProtocolVersion = reader.ReadByte();
            ProfileVersion = reader.ReadUInt16();
            FITDataSize = reader.ReadUInt32();

            // .fit signature
            byte[] sig = reader.ReadBytes(4);
            _validHeader = Encoding.ASCII.GetString(sig) == ".FIT";
            
            // CRC is optional
            if (FITHeaderSize > 12)
            {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                ushort CRC_CALC = Crc16.ComputeCrc(reader, (int)FITHeaderSize - 2);
                CRC = reader.ReadUInt16();                
                _validHeader = CRC == CRC_CALC;
            };
        }

        public bool IsValidHeader
        {
            get
            {
                return _validHeader;
            }
        }
    }

    public sealed class Message
    {
        private readonly byte _header;
        private readonly MessageDefinition _messageDefinition;
        private readonly byte[] _messageData;

        private bool _isInitialized;
        private BinaryReader _binaryReader;

        public Message(byte header, MessageDefinition messageDefinition, BinaryReader reader)
        {
            _header = header;
            _messageDefinition = messageDefinition;
            _messageData = reader.ReadBytes(_messageDefinition.Size);
        }

        // Linear search through a Message's FieldDefinitions 
        // If found, will also guarantee that the internal BinaryReader
        // over the Message is initialized, and pointing at the start
        // of the field.
        // Returns null if not found.
        private FieldDefinition GetFieldDefinition(byte fieldNumber)
        {
            foreach (FieldDefinition fieldDefinition in _messageDefinition.FieldDefinitions)
            {
                if (fieldDefinition.FieldDefinitionNumber == (byte)fieldNumber)
                {
                    if (!_isInitialized)
                    {
                        MemoryStream memoryStream = new MemoryStream(_messageData);
                        _binaryReader = new BinaryReader(memoryStream);
                        _isInitialized = true;
                    }
                    _binaryReader.BaseStream.Seek(fieldDefinition.FieldOffset, SeekOrigin.Begin);
                    return fieldDefinition;
                }
            }
            return null;
        }

        public bool TryGetField(FieldDecl fieldDecl, out double value)
        {
            value = 0;
            FieldDefinition fieldDefinition = GetFieldDefinition(fieldDecl);
            if (fieldDefinition == null)
            {
                return false;
            }
            else
            {
                // We will return false if we encounter an invalid value in the raw data.
                // The caller needs to interpret invalid values the same as missing values.
                if (fieldDefinition.FieldType == 0x01)
                {
                    sbyte raw = _binaryReader.ReadSByte();
                    if (raw == 0x7f)
                    {
                        return false;
                    }
                    value = Convert.ToDouble(raw);
                }
                else if (fieldDefinition.FieldType == 0x02 || fieldDefinition.FieldType == 0x0A)
                {
                    byte raw = _binaryReader.ReadByte();
                    if (raw == 0xff)
                    {
                        return false; 
                    }
                    value = Convert.ToDouble(raw);
                }
                else if (fieldDefinition.FieldType == 0x83)
                {
                    Int16 raw = _binaryReader.ReadInt16();
                    if (raw == 0x7fff)
                    {
                        return false;
                    }
                    value = Convert.ToDouble(raw);
                }
                else if (fieldDefinition.FieldType == 0x84 || fieldDefinition.FieldType == 0x8B)
                {
                    UInt16 raw = _binaryReader.ReadUInt16();
                    if (raw == 0xffff)
                    {
                        return false;
                    }
                    value = Convert.ToDouble(raw);
                }
                else if (fieldDefinition.FieldType == 0x85)
                {
                    Int32 raw = _binaryReader.ReadInt32();
                    if (raw == 0x7fffffff)
                    {
                        return false;
                    }
                    value = Convert.ToDouble(raw);
                }
                else if (fieldDefinition.FieldType == 0x86 || fieldDefinition.FieldType == 0x8C)
                {
                    UInt32 raw = _binaryReader.ReadUInt32();
                    if (raw == 0xffffffff)
                    {
                        return false;
                    }
                    value = Convert.ToDouble(raw);
                }
                else if (fieldDefinition.FieldType == 0x88)
                {
                    // TODO: don't know how to handle floating point invalid values.
                    // I think I need to peek the raw bits rather than try to interpret 
                    value = Convert.ToDouble(_binaryReader.ReadSingle());
                }
                else if (fieldDefinition.FieldType == 0x89)
                {
                    // TODO: don't know how to handle floating point invalid values
                    // I think I need to peek the raw bits rather than try to interpret 
                    value = Convert.ToDouble(_binaryReader.ReadDouble());
                }
                else
                {
                    value = 0;
                    return false;
                }
                return true;
            }
        }

        private readonly System.DateTime _dateTimeOffset = new System.DateTime(1989, 12, 31, 0, 0, 0, System.DateTimeKind.Utc);

        public bool TryGetField(FieldDecl fieldDecl, out System.DateTime value)
        {
            FieldDefinition fieldDefinition = GetFieldDefinition(fieldDecl);
            if (fieldDefinition != null && fieldDefinition.FieldType == 0x86)
            {
                UInt32 timeStamp = _binaryReader.ReadUInt32();
                if (timeStamp < 0x10000000)
                {
                    throw new InvalidOperationException("timeStampValue > 0x10000000 I don't know how to compute this.");
                }
                value = new System.DateTime(timeStamp * 10000000L + _dateTimeOffset.Ticks, DateTimeKind.Utc);
                return true;
            }
            else
            {
                value = DateTime.MaxValue;
                return false;
            }
        }

        public bool TryGetField(FieldDecl fieldDecl, out string value)
        {
            FieldDefinition fieldDefinition = GetFieldDefinition(fieldDecl);
            if (fieldDefinition != null && fieldDefinition.FieldType == 0x07)
            {
                value = _binaryReader.ReadString();
                return true;
            }
            else
            {
                value = String.Empty;
                return false;
            }
        }

        // Read an enum type
        public bool TryGetField(FieldDecl fieldDecl, out byte value)
        {
            FieldDefinition fieldDefinition = GetFieldDefinition(fieldDecl);
            if (fieldDefinition != null && fieldDefinition.FieldType == 0x00)
            {
                value = _binaryReader.ReadByte();
                if (value == 0xff)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                value = 0xff;
                return false;
            }
        }

        // TODO: Read an array type

        public ushort GlobalMessageNumber
        {
            get { return _messageDefinition.GlobalMessageNumber; }
        }

        public MessageDefinition MessageDefinition
        {
            get { return _messageDefinition; }
        }
    }

    public sealed class FastParser : IDisposable
    {
        private BinaryReader _reader;
        public FileHeader Header;

        private MessageDefinition[] _localMessageDefinitions = new MessageDefinition[16];

        public FastParser(Stream stream)
        {
            _reader = new BinaryReader(stream);
            Header = new FileHeader(_reader);
        }

        public bool IsFileValid
        {
            get
            {
                // Compute the CRC and then reset position to start of file
                long startOfMessages = _reader.BaseStream.Position;

                _reader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Use new high-speed CRC calculator
                ushort crc = Crc16.ComputeCrc(_reader, (int)Header.FITDataSize + Header.FITHeaderSize);

                int fileCrc = _reader.ReadUInt16();
                bool result = (fileCrc == crc);

                // Reset position to the start of the messages
                _reader.BaseStream.Seek(startOfMessages, SeekOrigin.Begin);
                return result;
            }
        }

        public IEnumerable<Message> GetMessages()
        {
            _reader.BaseStream.Seek(Header.FITHeaderSize, SeekOrigin.Begin);
            uint bytesToRead = Header.FITDataSize;
            uint bytesRead = 0;

            while (bytesRead < bytesToRead)
            {
                byte header = _reader.ReadByte();

                // Normal header (vs. timestamp offset header is indicated by bit 7)
                // Message type is indicated by bit 6 
                //   1 == definition
                //   0 == record
                byte localMessageNumber = (byte)(header & 0xf);

                // Message definitions are parsed internally by the parser and not exposed to
                // the caller.
                if ((header & 0x80) == 0 && (header & 0x40) == 0x40)
                {
                    // Parse the message definition and store the definition in our array
                    MessageDefinition messageDefinition = new MessageDefinition(header, _reader);
                    _localMessageDefinitions[localMessageNumber] = messageDefinition;
                    bytesRead += (uint)(messageDefinition.MessageDefinitionSize + 1);
                }
                else if ((header & 0x80) == 0 && (header & 0x40) == 0)
                {
                    MessageDefinition currentMessageDefinition = _localMessageDefinitions[localMessageNumber];
                    Debug.Assert(currentMessageDefinition != null);

                    // This design reads the current message into an in-memory byte array.
                    // An alternate design would involve passing in the current binary reader
                    // and allowing the caller of Message to read fields using the binary
                    // reader directly instead of creating a MemoryStream over the byte array
                    // and using a different BinaryReader in the Message. I have done 
                    // exactly this and measured the performance, and it is actually SLOWER
                    // than this approach. I haven't root caused why, but would assume that
                    // Seek-ing arbitrarily using the BinaryReader over the FileStream is 
                    // slow vs. Seek-ing over a BinaryReader over a MemoryStream.

                    Message message = new Message(header, currentMessageDefinition, _reader);
                    yield return message;

                    bytesRead += (uint)(currentMessageDefinition.Size + 1);
                }
            }
        }

        public void Dispose()
        {
            _reader.BaseStream.Dispose();
        }
    }
}

namespace FitParser
{
    public sealed class FitConverter
    {
        public static List<KeyValuePair<System.Drawing.PointF, List<KeyValuePair<string, string>>>> ReadFile(string fileName)
        {
            List<KeyValuePair<System.Drawing.PointF, List<KeyValuePair<string, string>>>> result = new List<KeyValuePair<System.Drawing.PointF, List<KeyValuePair<string, string>>>>();

            double SEMICIRCLES_TO_DEGREES = (180 / Math.Pow(2, 31));
            using (FileStream stream = System.IO.File.OpenRead(fileName))
            {                
                FitParser.Core.FastParser fastParser = new FitParser.Core.FastParser(stream);
                bool hv = fastParser.Header.IsValidHeader;
                bool fv = fastParser.IsFileValid;
                if ((!hv) || (!fv)) return result;

                IEnumerable<FitParser.Core.Message> msgs = fastParser.GetMessages();
                foreach (FitParser.Core.Message dataRecord in msgs)
                {
                    if (dataRecord.GlobalMessageNumber == FitParser.Core.GlobalMessageDecls.Record)
                    {
                        System.DateTime timeStamp;
                        double lat, lon, val;
                        System.Drawing.PointF point = new System.Drawing.PointF();

                        List<KeyValuePair<string, string>> pars = new List<KeyValuePair<string, string>>();
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.TimeStamp, out timeStamp))
                            pars.Add(new KeyValuePair<string, string>("TimeStamp", timeStamp.ToString("HH:mm:ss dd.MM.yyyy")));
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.PositionLat, out lat))
                            point.Y = (float)(lat * SEMICIRCLES_TO_DEGREES);
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.PositionLong, out lon))
                            point.X = (float)(lon * SEMICIRCLES_TO_DEGREES);
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.Altitude, out val))
                            pars.Add(new KeyValuePair<string, string>("Altitude", (val / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture)));
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.HeartRate, out val))
                            pars.Add(new KeyValuePair<string, string>("HeartRate", val.ToString()));
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.Cadence, out val))
                            pars.Add(new KeyValuePair<string, string>("Cadence", val.ToString()));
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.Power, out val))
                            pars.Add(new KeyValuePair<string, string>("Power", val.ToString()));
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.Distance, out val))
                            pars.Add(new KeyValuePair<string, string>("Distance", (val / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture)));
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.Speed, out val))
                            pars.Add(new KeyValuePair<string, string>("Speed", (val / 1000.0 * 3.6).ToString(System.Globalization.CultureInfo.InvariantCulture)));
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.Temperature, out val))
                            pars.Add(new KeyValuePair<string, string>("Temperature", val.ToString()));
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.GpsAccuracy, out val))
                            pars.Add(new KeyValuePair<string, string>("GpsAccuracy", val.ToString()));
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.VerticalSpeed, out val))
                            pars.Add(new KeyValuePair<string, string>("VerticalSpeed", (val / 1000.0).ToString()));
                        if (dataRecord.TryGetField(FitParser.Core.RecordDef.Calories, out val))
                            pars.Add(new KeyValuePair<string, string>("Calories", val.ToString()));
                        result.Add(new KeyValuePair<System.Drawing.PointF, List<KeyValuePair<string, string>>>(point, pars));
                    };
                };
            };
            return result;
        }

        public static void Fit2GPX(string fitFileName, string gpxFileName)
        {
            List<KeyValuePair<System.Drawing.PointF, List<KeyValuePair<string, string>>>> result = new List<KeyValuePair<System.Drawing.PointF, List<KeyValuePair<string, string>>>>();

            double SEMICIRCLES_TO_DEGREES = (180 / Math.Pow(2, 31));
            using (FileStream inFile = System.IO.File.OpenRead(fitFileName))
            {
                FitParser.Core.FastParser fastParser = new FitParser.Core.FastParser(inFile);
                bool hv = fastParser.Header.IsValidHeader;
                bool fv = fastParser.IsFileValid;
                if (hv && fv)
                {
                    using (FileStream outFile = new FileStream(gpxFileName, FileMode.Create, FileAccess.Write))
                    {
                        StreamWriter sw = new StreamWriter(outFile, Encoding.UTF8);
                        sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                        sw.WriteLine("<gpx version=\"1.0\">");
                        sw.WriteLine("<name>"+Path.GetFileNameWithoutExtension(fitFileName)+"</name>");
                        sw.WriteLine("<trk><name>" + Path.GetFileNameWithoutExtension(fitFileName) + "</name><trkseg>");

                        IEnumerable<FitParser.Core.Message> msgs = fastParser.GetMessages();
                        foreach (FitParser.Core.Message dataRecord in msgs)
                        {
                            if (dataRecord.GlobalMessageNumber == FitParser.Core.GlobalMessageDecls.Record)
                            {
                                System.DateTime timeStamp;
                                bool hla = false;
                                bool hlo = false;
                                double lat, lon, val;
                                System.Drawing.PointF point = new System.Drawing.PointF();
                                string addit = "";
                                
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.TimeStamp, out timeStamp))
                                    addit += "<time>" + timeStamp.ToString("yyyy-MM-ddTHH:mm:ssZ") + "</time>";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.PositionLat, out lat))
                                {
                                    hla = true;
                                    point.Y = (float)(lat * SEMICIRCLES_TO_DEGREES);
                                };
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.PositionLong, out lon))
                                {
                                    hlo = true;
                                    point.X = (float)(lon * SEMICIRCLES_TO_DEGREES);
                                };
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Altitude, out val))
                                    addit += "<ele>" + (val / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "</ele>";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.HeartRate, out val))
                                    addit += "<HeartRate>" + val.ToString() + "</HeartRate>";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Cadence, out val))
                                    addit += "<Cadence>" + val.ToString() + "</Cadence>";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Power, out val))
                                    addit += "<Power>" + val.ToString() + "</Power>";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Distance, out val))
                                    addit += "<Distance>" + (val / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "</Distance>";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Speed, out val))
                                    addit += "<Speed>" + (val / 1000.0 * 3.6).ToString(System.Globalization.CultureInfo.InvariantCulture) + "</Speed>";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Temperature, out val))
                                    addit += "<Temperature>" + val.ToString() + "</Temperature>";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.GpsAccuracy, out val))
                                    addit += "<GpsAccuracy>" + val.ToString() + "</GpsAccuracy>";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.VerticalSpeed, out val))
                                    addit += "<VerticalSpeed>" + (val / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "</VerticalSpeed>";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Calories, out val))
                                    addit += "<Calories>" + val.ToString() + "</Calories>";
                                if (hla && hlo)
                                {                                    
                                    sw.WriteLine("  <trkpt lat=\"{0}\" lon=\"{1}\">{2}</trkpt>", 
                                        point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                        point.X.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                        addit);
                                };
                            };
                        };

                        sw.WriteLine("</trkseg></trk>");
                        sw.WriteLine("</gpx>");
                        sw.Flush();
                    };
                };
            };
        }

        public static void Fit2KML(string fitFileName, string kmlFileName)
        {
            List<KeyValuePair<System.Drawing.PointF, List<KeyValuePair<string, string>>>> result = new List<KeyValuePair<System.Drawing.PointF, List<KeyValuePair<string, string>>>>();

            double SEMICIRCLES_TO_DEGREES = (180 / Math.Pow(2, 31));
            using (FileStream inFile = System.IO.File.OpenRead(fitFileName))
            {
                FitParser.Core.FastParser fastParser = new FitParser.Core.FastParser(inFile);
                bool hv = fastParser.Header.IsValidHeader;
                bool fv = fastParser.IsFileValid;
                if (hv && fv)
                {
                    using (FileStream outFile = new FileStream(kmlFileName, FileMode.Create, FileAccess.Write))
                    {
                        StreamWriter sw = new StreamWriter(outFile, Encoding.UTF8);
                        sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                        sw.WriteLine("  <kml>");
                        sw.WriteLine("    <Document>");
                        sw.WriteLine("      <name>From Fit file</name>");
                        sw.WriteLine("      <createdby>KMZRebuilder FitConverter</createdby>");
                        sw.WriteLine("      <Folder>");
                        sw.WriteLine("        <name>Track Points</name>");                        

                        IEnumerable<FitParser.Core.Message> msgs = fastParser.GetMessages();
                        int pCo = 0;
                        string geoline = "";
                        foreach (FitParser.Core.Message dataRecord in msgs)
                        {
                            if (dataRecord.GlobalMessageNumber == FitParser.Core.GlobalMessageDecls.Record)
                            {
                                System.DateTime timeStamp;
                                bool hla = false;
                                bool hlo = false;
                                double lat, lon, val;
                                System.Drawing.PointF point = new System.Drawing.PointF();
                                string addit = "";
                                string pName = "";

                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.TimeStamp, out timeStamp))
                                {
                                    addit += "TimeStamp=" + timeStamp.ToString("yyyy-MM-ddTHH:mm:ssZ") + "\r\n";
                                    pName = timeStamp.ToString("HH:mm:ss dd.MM.yyyy");
                                };
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.PositionLat, out lat))
                                {
                                    hla = true;
                                    point.Y = (float)(lat * SEMICIRCLES_TO_DEGREES);
                                };
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.PositionLong, out lon))
                                {
                                    hlo = true;
                                    point.X = (float)(lon * SEMICIRCLES_TO_DEGREES);
                                };
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Altitude, out val))
                                    addit += "Altitude=" + (val / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "\r\n";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.HeartRate, out val))
                                    addit += "HeartRate=" + val.ToString() + "\r\n";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Cadence, out val))
                                    addit += "Cadence=" + val.ToString() + "\r\n";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Power, out val))
                                    addit += "Power=" + val.ToString() + "\r\n";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Distance, out val))
                                    addit += "Distance=" + (val / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "\r\n";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Speed, out val))
                                    addit += "Speed=" + (val / 1000.0 * 3.6).ToString(System.Globalization.CultureInfo.InvariantCulture) + "\r\n";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Temperature, out val))
                                    addit += "Temperature=" + val.ToString() + "\r\n";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.GpsAccuracy, out val))
                                    addit += "GpsAccuracy=" + val.ToString() + "\r\n";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.VerticalSpeed, out val))
                                    addit += "VerticalSpeed=" + (val / 1000.0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "\r\n";
                                if (dataRecord.TryGetField(FitParser.Core.RecordDef.Calories, out val))
                                    addit += "Calories=" + val.ToString() + "\r\n";
                                if (hla && hlo)
                                {
                                    geoline += String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1},0 ", point.X, point.Y);
                                    if (String.IsNullOrEmpty(pName))
                                        pName = pCo.ToString();
                                    else
                                        pName = pCo.ToString("0000") + " - " + pName;
                                    sw.WriteLine("        <Placemark><name>" + pName + "</name>");
                                    sw.WriteLine("          <description><![CDATA[" + addit + "]]></description>");
                                    sw.WriteLine("          <Point><coordinates>{0},{1},0</coordinates></Point>",
                                        point.X.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                        point.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                    sw.WriteLine("        </Placemark>");
                                    pCo++;
                                };
                            };
                        };

                        sw.WriteLine("      </Folder>");
                        if (!String.IsNullOrEmpty(geoline))
                        {
                            sw.WriteLine("      <Folder>");
                            sw.WriteLine("        <name>Track Line</name>");
                            sw.WriteLine("        <Placemark>");
                            sw.WriteLine("          <name>Track Line</name>");
                            sw.WriteLine("          <description><![CDATA[]]></description>");
                            sw.Write("          <LineString><coordinates>" + geoline);
                            sw.WriteLine("          </coordinates></LineString>");
                            sw.WriteLine("        </Placemark>");
                            sw.WriteLine("      </Folder>");
                        };
                        sw.WriteLine("</Document></kml>");
                        sw.Flush();
                    };
                };
            };
        }
    }
}