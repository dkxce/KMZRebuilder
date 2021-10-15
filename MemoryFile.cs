/**********************************************
 *                                            *
 *                                            *
 *           C#  Memory File Class            *
 *           (by milokz@gmail.com)            *
 *                                            *
 *  use for share data between applications   *
 *           and send notifications           *
 *                                            *
 *           use unsafe for build             *
 *                                            *
 *                                            *
 *********************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Windows;
using System.Windows.Forms;

namespace MemFile
{
    /// <summary>
    ///     Memory File
    /// </summary>
    public class MemoryFile
    {
        public static bool useWithSendMessage = true;

        /// <summary>
        ///     Notify Event Types
        /// </summary>
        public enum NotifyEvent : byte
        {
            /// <summary>
            ///     New connection to the file
            /// </summary>
            fConnected = 0,
            /// <summary>
            ///     Client disconnected from file
            /// </summary>
            fDisconnected = 1,
            /// <summary>
            ///     Data was readed from file
            /// </summary>
            fHandled = 2,
            /// <summary>
            ///     Data was writed to file
            /// </summary>
            fWrited = 3,
            /// <summary>
            ///     User Event was set
            /// </summary>
            fUserEvent = 4
        }

        /// <summary>
        ///     Notify Source
        /// </summary>
        public enum NotifySource : byte
        {
            nsUnknown = 0,
            nsThread = 1,
            nsSystem = 2
        }

        /// <summary>
        ///     File State 
        /// </summary>
        public enum FileState : byte
        {
            /// <summary>
            ///     File is Empty
            /// </summary>
            fsEmpty = 0,
            /// <summary>
            ///     File is Ready to read/write
            /// </summary>
            fsReady = 1,
            /// <summary>
            ///     File is Busy for read/write
            /// </summary>
            fsBusy = 2
        }

        /// <summary>
        ///     File Data Types
        /// </summary>
        public enum FileType : byte
        {
            /// <summary>
            ///     Unknown Type
            /// </summary>
            ftUnknown = 0,
            /// <summary>
            ///     Data placed by Binary Serializer
            /// </summary>
            ftBinSeriazable = 1,
            /// <summary>
            ///     Data placed by XML Serializer
            /// </summary>
            ftXmlSeriazable = 2,
            /// <summary>
            ///     Data placed by Key-Value Pairs
            /// </summary>
            ftKeyValues = 3,
            /// <summary>
            ///     Data placed as string
            /// </summary>
            ftText = 4,
            /// <summary>
            ///     Data placed by Binary Serializer as string
            /// </summary>
            ftString = 5,
            /// <summary>
            ///     Data placed by Binary Serializer as string[]
            /// </summary>
            ftStringArray = 6,
            /// <summary>
            ///     Data placed by Binary Serializer as int
            /// </summary>
            ftInteger = 7,
            /// <summary>
            ///     Data placed by Binary Serializer as int[]
            /// </summary>
            ftIntArray = 8,
            /// <summary>
            ///     Marshal Structure
            /// </summary>
            ftMarshalStructure = 9,
            /// <summary>
            ///     User-Defined Data
            /// </summary>
            ftUserDefined = 0xFF
        }

        /// <summary>
        ///     Notify Delegate
        /// </summary>
        /// <param name="notify">Notify Event Type</param>
        /// <param name="notifyParam">Notify param, for fUserEvent is userEventCode</param>
        public delegate void OnGetNotify(NotifyEvent notify, NotifySource source, byte notifyParam);

        private SafeFileMappingHandle fileHandle = null;
        private const int notify_timeout = 500; // ms timeout        
        private IntPtr ptrState = IntPtr.Zero;  // File Flags: 0 - fileState; 1 - fileClients; 2 - fileReaded; 3 - fileWrited; 4 - fileType; 5 - userEvent; 6 & 7 - Reserved
        private IntPtr ptrStart = IntPtr.Zero;  // File Data, Stream
        private System.IO.Stream _Stream;       // File Stream

        private string FullFileName = "Global\\NoName";
        private uint FileSize = 1048568; // 1 MB
        private uint FullFileSize = 1048576;

        private System.Threading.Thread nThread = null;
        private byte[] prevState = new byte[] { 0, 0, 0, 0 };
        private bool typeFileOrKernel = false; // false - file
        private bool connected = false;
        private bool _resetUserEvent2Zero = true;
        private IncomingMessagesWindow imw = null;
        private NotifySource _processNotifySources = NotifySource.nsThread;
        private Exception _lastEx = null;

        /// <summary>
        ///     On Notify Event
        /// </summary>
        public OnGetNotify onGetNotify = null;

        /// <summary>
        ///     Last Exception
        /// </summary>
        public Exception LastException { get { return _lastEx; } }

        /// <summary>
        ///     if false: onGetNotify will call on any change of userEvent and will no reset it to 0 (zero);
        ///     if true: onGetNotify will call on any change of userEvent and will reset it to 0 (zero);
        ///     (use true if you should detect userEvent with same code serveral times)
        /// </summary>
        public bool ResetUserEventToZero
        {
            get { return _resetUserEvent2Zero; }
            set { _resetUserEvent2Zero = value; }
        }

        /// <summary>
        ///     Process Notify from: nsUnknown - no process; nsSystem, nsFile or nsSystem | nsFile
        /// </summary>
        public NotifySource ProcessNotifySources
        {
            get
            {
                return _processNotifySources;
            }
            set
            {                
                _processNotifySources = value;
            }
        }

        /// <summary>
        ///     Linked to file in memory
        /// </summary>
        public bool Connected { get { return connected; } }

        /// <summary>
        ///     File Size (availabe to read/write operations)
        /// </summary>
        public uint Size { get { return FileSize; } }

        /// <summary>
        ///     File Size in Memory (file size + flag bytes)
        /// </summary>
        public uint MemorySize { get { return FullFileSize; } }

        /// <summary>
        ///     Create Memory File and Link to it
        /// </summary>
        /// <param name="fileName">unical file name</param>
        public MemoryFile(string fileName)
        {            
            FullFileName = String.Format("Global\\{0}", fileName);
            Connect();
        }

        /// <summary>
        ///     Create Memory File and Link to it
        /// </summary>
        /// <param name="fileName">unical file name</param>
        /// <param name="FileSize">file size</param>
        public MemoryFile(string fileName, uint FileSize)
        {            
            this.FileSize = FileSize;
            this.FullFileSize = this.FileSize + 8;
            FullFileName = String.Format("Global\\{0}", fileName);
            Connect();
        }

        /// <summary>
        ///     Set User Event Code for fUserEvent
        /// </summary>
        /// <param name="userEventCode"></param>
        public void SetNotifyUserEvent(byte userEventCode)
        {
            _userEvent = userEventCode;
            if(imw != null) imw.SendMessage((int)0xFF00 + (int)userEventCode);
        }

        /// <summary>
        ///     Pointer to first byte of file data
        /// </summary>
        public IntPtr Pointer
        {
            get
            {
                return ptrStart;
            }
        }

        /// <summary>
        ///     Connections count to the file
        /// </summary>
        public byte Connections
        {
            get
            {
                return _fileClients;
            }
        }

        /// <summary>
        ///     Type of File Data
        /// </summary>
        public FileType DataType
        {
            get
            {
                return (FileType)_fileType;
            }
            set
            {
                _fileType = (byte)value;
            }
        }

        /// <summary>
        ///     File State
        /// </summary>
        private FileState intState
        {
            get
            {
                unsafe
                {
                    byte* ist = (byte*)ptrState.ToPointer();
                    return (FileState)(*ist);
                };
            }
            set
            {
                unsafe
                {
                    byte* ist = (byte*)ptrState.ToPointer();
                    *ist = (byte)value;
                };
            }
        }

        /// <summary>
        ///     File Clients Connected
        /// </summary>
        private byte _fileClients
        {
            get
            {
                unsafe
                {
                    byte* ist = (byte*)((int)ptrState + 1);
                    return *ist;
                };
            }
            set
            {
                prevState[0] = value;
                unsafe
                {
                    byte* ist = (byte*)((int)ptrState+1);
                    *ist = (byte)value;
                };
            }
        }

        /// <summary>
        ///     File Readed Counter
        /// </summary>
        private byte _fileReaded
        {
            get
            {
                unsafe
                {
                    byte* ist = (byte*)((int)ptrState + 2);
                    return *ist;
                };
            }
            set
            {
                prevState[1] = value;
                unsafe
                {
                    byte* ist = (byte*)((int)ptrState + 2);
                    *ist = (byte)value;
                };
            }
        }

        /// <summary>
        ///     File Writed Counter
        /// </summary>
        private byte _fileWrited
        {
            get
            {
                unsafe
                {
                    byte* ist = (byte*)((int)ptrState + 3);
                    return *ist;
                };
            }
            set
            {
                prevState[2] = value;
                unsafe
                {
                    byte* ist = (byte*)((int)ptrState + 3);
                    *ist = (byte)value;
                };
            }
        }

        /// <summary>
        ///     File Type)
        /// </summary>
        private byte _fileType
        {
            get
            {
                unsafe
                {
                    byte* ist = (byte*)((int)ptrState + 4);
                    return *ist;
                };
            }
            set
            {
                unsafe
                {
                    byte* ist = (byte*)((int)ptrState + 4);
                    *ist = (byte)value;
                };
            }
        }

        /// <summary>
        ///     User Event
        /// </summary>
        private byte _userEvent
        {
            get
            {
                unsafe
                {
                    byte* ist = (byte*)((int)ptrState + 5);
                    return *ist;
                };
            }
            set
            {
                prevState[3] = value;
                unsafe
                {
                    byte* ist = (byte*)((int)ptrState + 5);
                    *ist = (byte)value;
                };
            }
        }

        /// <summary>
        ///     Clear File
        /// </summary>
        /// <param name="sendUpdate">send WM_APP_FileUpdated ?</param>
        public void Clear(bool sendUpdate)
        {
            while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
            this.intState = FileState.fsBusy;
            {
                byte[] b = new byte[FileSize];
                Stream.Position = 0;
                Stream.Write(b, 0, b.Length);
                Stream.Position = 0;
            };
            _fileType = (byte)FileType.ftUnknown;
            this.intState = FileState.fsReady;
            if (sendUpdate)
            {
                _fileWrited++;
                if (imw != null) imw.SendMessage((byte)NotifyEvent.fWrited);
            };
        }

        /// <summary>
        ///     Clear File with send WM_APP_FileUpdated
        /// </summary>
        public void Clear()
        {
            this.Clear(true);
        }

        /// <summary>
        ///     Create file
        /// </summary>
        private void Connect()
        {
            try
            {
                SECURITY_ATTRIBUTES sa = SECURITY_ATTRIBUTES.Empty;
                fileHandle = NativeMethod.CreateFileMapping(
                    INVALID_HANDLE_VALUE,
                    ref sa,
                    FileProtection.PAGE_READWRITE,
                    0,
                    FullFileSize,
                    FullFileName);

                if (fileHandle.IsInvalid) throw new Win32Exception();

                //IntPtr sidPtr = IntPtr.Zero;
                //SECURITY_INFORMATION sFlags = SECURITY_INFORMATION.Owner;
                //System.Security.Principal.NTAccount user = new System.Security.Principal.NTAccount("P1R4T3\\Harris");
                //System.Security.Principal.SecurityIdentifier sid = (System.Security.Principal.SecurityIdentifier)user.Translate(typeof(System.Security.Principal.SecurityIdentifier));
                //ConvertStringSidToSid(sid.ToString(), ref sidPtr);
                SetNamedSecurityInfoW(FullFileName, typeFileOrKernel ? SE_OBJECT_TYPE.SE_KERNEL_OBJECT : SE_OBJECT_TYPE.SE_FILE_OBJECT, SECURITY_INFORMATION.Dacl, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

                ptrState = NativeMethod.MapViewOfFile(fileHandle, FileMapAccess.FILE_MAP_ALL_ACCESS, 0, 0, FullFileSize);
                if (ptrState == IntPtr.Zero) throw new Win32Exception();
                ptrStart = (IntPtr)((int)ptrState + 8);
                
                connected = true;
                _fileClients++;

                try
                {
                    imw = new IncomingMessagesWindow(FullFileName, Process.GetCurrentProcess().Handle, GetNotify);
                }
                catch (Exception ex) { _lastEx = ex; };
                if (imw != null) imw.SendMessage((byte)NotifyEvent.fConnected);

                nThread = new System.Threading.Thread(NotifyThread);
                nThread.Start();                

                unsafe
                {
                    _Stream = new System.IO.UnmanagedMemoryStream((byte*)ptrStart.ToPointer(), FileSize, FileSize, System.IO.FileAccess.ReadWrite);
                };                
            }
            catch (Exception ex)
            {
                _lastEx = ex;
                throw ex;
            };
        }

        /// <summary>
        ///     Get File Stream;
        ///     If you are using Stream application will not send WM_APP_FileUpdated or WM_APP_FileHandled messages
        /// </summary>
        public System.IO.Stream Stream
        {
            get
            {
                return _Stream;
            }
        }

        /// <summary>
        ///         Read/Write byte to File;
        ///         No Send WM_APP_FileUpdated or WM_APP_FileHandled Messages
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte this[int index]
        {
            get
            {
                byte[] res = new byte[1];
                while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
                this.intState = FileState.fsBusy;
                {
                    Marshal.Copy((IntPtr)((int)ptrStart + index), res, 0, 1);
                };
                this.intState = FileState.fsReady;
                return res[0];
            }
            set
            {
                byte[] res = new byte[] { value };
                while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
                this.intState = FileState.fsBusy;
                {
                    Marshal.Copy(res, 0, (IntPtr)((int)ptrStart + index), 1);
                };
                this.intState = FileState.fsReady;
            }
        }

        /// <summary>
        ///     Read/Write bytes to File;
        ///     No Send WM_APP_FileUpdated or WM_APP_FileHandled Messages
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] this[int offset, int count]
        {
            get
            {
                byte[] res = new byte[count];
                while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
                this.intState = FileState.fsBusy;
                {
                    Marshal.Copy((IntPtr)((int)ptrStart + offset), res, 0, count);
                };
                this.intState = FileState.fsReady;
                return res;
            }
            set
            {
                while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
                this.intState = FileState.fsBusy;
                {
                    Marshal.Copy(value, 0, (IntPtr)((int)ptrStart + offset), count);
                };
                this.intState = FileState.fsReady;
            }
        }

        /// <summary>
        ///     File is Ready
        /// </summary>
        public bool IsReady
        {
            get
            {
                return (this.intState != FileState.fsBusy);
            }
        }

        /// <summary>
        ///     File is Empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.intState == FileState.fsEmpty;
            }
        }

        /// <summary>
        ///     File is Busy
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this.intState == FileState.fsBusy;
            }
        }
        
        /// <summary>
        ///     Set object to File (Bin Serializer)
        /// </summary>
        /// <param name="obj"></param>
        public void SetSeriazable(object obj)
        {
            Type tof = obj.GetType();
            this.Clear(false);
            while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
            this.intState = FileState.fsBusy;
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Stream.Position = 0;
                formatter.Serialize(Stream, obj);
                Stream.Position = 0;
            };
            _fileType = (byte)FileType.ftBinSeriazable;
            if (tof == typeof(string)) _fileType = (byte)FileType.ftString;
            if (tof == typeof(string[])) _fileType = (byte)FileType.ftStringArray;
            if (tof == typeof(int)) _fileType = (byte)FileType.ftInteger;
            if (tof == typeof(int[])) _fileType = (byte)FileType.ftIntArray;
            this.intState = FileState.fsReady;
            _fileWrited++;
            if (imw != null) imw.SendMessage((byte)NotifyEvent.fWrited);
        }

        /// <summary>
        ///     Get object from File (Bin Serializer)
        /// </summary>
        /// <returns></returns>
        public object GetSeriazable()
        {
            object res = null;
            while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
            this.intState = FileState.fsBusy;
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Stream.Position = 0;
                res = formatter.Deserialize(Stream);
                Stream.Position = 0;
            };
            this.intState = FileState.fsReady;
            _fileReaded++;
            if (imw != null) imw.SendMessage((byte)NotifyEvent.fHandled);
            return res;
        }

        /// <summary>
        ///     Set Object to File (Xml Serializer)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="T"></param>
        public void SetSeriazable(object obj, Type T)
        {
            this.Clear(false);
            while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
            this.intState = FileState.fsBusy;
            {
                Stream.Position = 0;
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(T);
                System.IO.StreamWriter writer = new System.IO.StreamWriter(Stream);
                xs.Serialize(writer, obj);
                Stream.Position = 0;
            };
            _fileType = (byte)FileType.ftXmlSeriazable;
            this.intState = FileState.fsReady;
            _fileWrited++;
            if (imw != null) imw.SendMessage((byte)NotifyEvent.fWrited);
        }

        /// <summary>
        ///     Get Object from File (Xml Serializer)
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public object GetSeriazable(Type T)
        {
            object res = null;
            while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
            this.intState = FileState.fsBusy;
            {
                Stream.Position = 0;
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(T);
                res = xs.Deserialize(Stream);
                Stream.Position = 0;
            };
            this.intState = FileState.fsReady;
            _fileReaded++;
            if (imw != null) imw.SendMessage((byte)NotifyEvent.fHandled);
            return res;
        }

        /// <summary>
        ///     Close Memory File
        /// </summary>
        public void Close()
        {
            connected = false;            
            if (nThread != null)
            {
                System.Threading.Thread.Sleep(notify_timeout);
                nThread.Abort();
                nThread = null;
            };
      
            if (fileHandle != null)
            {
                try { _Stream.Close(); } catch (Exception ex) { _lastEx = ex; };
                _fileClients--;
                if (imw != null)
                {
                    imw.SendMessage((byte)NotifyEvent.fDisconnected);
                    imw.Destroy();
                    imw = null;
                };

                if (ptrState != IntPtr.Zero)
                {
                    NativeMethod.UnmapViewOfFile(ptrState);
                    ptrState = IntPtr.Zero;
                };

                fileHandle.Close();
                fileHandle = null;
            };            
        }

        /// <summary>
        ///     Get/Set KeyValues Pairs to File
        /// </summary>
        public List<KeyValuePair<string, string>> Keys
        {
            get
            {
                List<KeyValuePair<string, string>> res = new List<KeyValuePair<string, string>>();
                {
                    int next_str_len = 0, offset = 0;
                    byte[] header = this[0, 8];
                    if (BitConverter.ToUInt64(header, 0) != 0x4b45595356414c53) return res;
                    offset += 8;
                    while ((next_str_len = BitConverter.ToInt32(this[offset, 4], 0)) > 0)
                    {
                        offset += 4;
                        string name = System.Text.Encoding.UTF8.GetString(this[offset, next_str_len]);
                        offset += next_str_len;
                        next_str_len = BitConverter.ToInt32(this[offset, 4], 0);
                        offset += 4;
                        string value = System.Text.Encoding.UTF8.GetString(this[offset, next_str_len]);
                        offset += next_str_len;
                        res.Add(new KeyValuePair<string, string>(name, value));
                    };
                };
                _fileReaded++;
                if (imw != null) imw.SendMessage((byte)NotifyEvent.fHandled);
                return res;
            }
            set
            {
                this.Clear(false);
                this.intState = FileState.fsReady;
                if ((value != null) && (value.Count > 0))
                {
                    int offset = 0;
                    byte[] header = BitConverter.GetBytes(0x4b45595356414c53);
                    this[offset, header.Length] = header; offset += header.Length;
                    foreach (KeyValuePair<string, string> kvp in value)
                    {
                        byte[] na = System.Text.Encoding.UTF8.GetBytes(kvp.Key);
                        byte[] nl = BitConverter.GetBytes(na.Length);
                        byte[] va = System.Text.Encoding.UTF8.GetBytes(kvp.Value);
                        byte[] vl = BitConverter.GetBytes(va.Length);
                        byte[] nb = BitConverter.GetBytes((int)99);
                        this[offset, nl.Length] = nl; offset += nl.Length;
                        this[offset, na.Length] = na; offset += na.Length;
                        this[offset, vl.Length] = vl; offset += vl.Length;
                        this[offset, va.Length] = va; offset += va.Length;
                    };
                };
                _fileType = (byte)FileType.ftKeyValues;
                _fileWrited++;
                if (imw != null) imw.SendMessage((byte)NotifyEvent.fWrited);
            }
        }

        /// <summary>
        ///     MemFile as TextFile
        /// </summary>
        public string AsString
        {
            get
            {
                byte[] res = new byte[FileSize];
                while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
                this.intState = FileState.fsBusy;
                {
                    Marshal.Copy(ptrStart, res, 0, res.Length);
                };
                _fileType = (byte)FileType.ftText;
                this.intState = FileState.fsReady;
                _fileReaded++;
                if (imw != null) imw.SendMessage((byte)NotifyEvent.fHandled);
                return System.Text.Encoding.UTF8.GetString(res).Trim('\0');
            }
            set
            {
                this.Clear(false);
                byte[] tocopy = System.Text.Encoding.UTF8.GetBytes(value);
                while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
                this.intState = FileState.fsBusy;
                {
                    Marshal.Copy(tocopy, 0, ptrStart, tocopy.Length < FileSize ? tocopy.Length : (int)FileSize);
                };
                this.intState = FileState.fsReady;
                _fileWrited++;
                if (imw != null) imw.SendMessage((byte)NotifyEvent.fWrited);
            }
        }

        /// <summary>
        ///     Save Memory File to Disk
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
            this.intState = FileState.fsBusy;
            {
                byte[] b = new byte[FileSize];
                Stream.Position = 0;
                Stream.Read(b, 0, b.Length);
                System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                fs.Write(b, 0, b.Length);
                fs.Close();
            };
            this.intState = FileState.fsReady;            
            _fileReaded++;
            if (imw != null) imw.SendMessage((byte)NotifyEvent.fHandled);
        }

        /// <summary>
        ///     Load Memory File From Disk
        /// </summary>
        /// <param name="fileName"></param>
        public void Load(string fileName)
        {
            while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
            this.intState = FileState.fsBusy;
            {
                byte[] b = new byte[FileSize];
                System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                fs.Read(b, 0, b.Length);
                fs.Close();
                Stream.Position = 0;
                Stream.Write(b, 0, b.Length);
            };
            this.intState = FileState.fsReady;
            _fileWrited++;
            if (imw != null) imw.SendMessage((byte)NotifyEvent.fWrited);
        }

        /// <summary>
        ///     Save Structure to the file (see: SampleClass4Marshal)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="str">Structure</param>
        public void Set<T>(T str)
        {
            while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
            this.intState = FileState.fsBusy;
            {
                Marshal.StructureToPtr(str, ptrStart, true);
            };
            _fileType = (byte)FileType.ftMarshalStructure;
            this.intState = FileState.fsReady;
            _fileWrited++;
            if (imw != null) imw.SendMessage((byte)NotifyEvent.fWrited);
        }

        /// <summary>
        ///     Read Structure from the file (see: SampleClass4Marshal)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Structure</returns>
        public T Get<T>()
        {
            T res = default(T);
            while (this.intState == FileState.fsBusy) System.Threading.Thread.Sleep(5);
            this.intState = FileState.fsBusy;
            {
                res = (T)Marshal.PtrToStructure(ptrStart, typeof(T));
            };
            this.intState = FileState.fsReady;
            _fileReaded++;
            if (imw != null) imw.SendMessage((byte)NotifyEvent.fHandled);
            return res;
        }

        /// <summary>
        ///     Link Memory File as Pointer (void*, char*, int*, byte* ...);
        ///     https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public unsafe void* LinkAsPointer(int offset)
        {
            return (void*)((int)ptrStart + offset);
        }

        /// <summary>
        ///     on detect changes
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="notifyParam"></param>
        private void GetNotify(NotifyEvent notify, NotifySource source, byte notifyParam)
        {            
            if ((source & _processNotifySources) <= 0) return;
            if ((notify == NotifyEvent.fUserEvent) && (_resetUserEvent2Zero) && (notifyParam > 0)) _userEvent = 0;

            if (onGetNotify == null)
                Console.WriteLine("Get Notify from {2}: {0}({1})", notify, notifyParam, source);
            else
                onGetNotify(notify, source, notifyParam);
        }

        /// <summary>
        ///     Detect Changes
        /// </summary>
        private void NotifyThread()
        {
            prevState[0] = _fileClients;
            prevState[1] = _fileReaded;
            prevState[2] = _fileWrited;
            prevState[3] = _userEvent;
            try
            {
                while (connected)
                {
                    if (prevState[0] < _fileClients) { GetNotify(NotifyEvent.fConnected, NotifySource.nsThread, _fileClients); };
                    if (prevState[0] > _fileClients) { GetNotify(NotifyEvent.fDisconnected, NotifySource.nsThread, _fileClients); };
                    if (prevState[1] != _fileReaded) { GetNotify(NotifyEvent.fHandled, NotifySource.nsThread, _fileReaded); };
                    if (prevState[2] != _fileWrited) { GetNotify(NotifyEvent.fWrited, NotifySource.nsThread, _fileWrited); };
                    if (prevState[3] != _userEvent) { GetNotify(NotifyEvent.fUserEvent, NotifySource.nsThread, _userEvent); };
                    prevState[0] = _fileClients;
                    prevState[1] = _fileReaded;
                    prevState[2] = _fileWrited;
                    prevState[3] = _userEvent;
                    System.Threading.Thread.Sleep(notify_timeout);
                };
            }
            catch (Exception ex) { _lastEx = ex; };            
        }        

        /// <summary>
        ///     Destroy
        /// </summary>
        ~MemoryFile() { Close(); }
        
        /// <summary>
        ///     Get Exe Path
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDir()
        {
            string fname = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.ToString();
            fname = fname.Replace("file:///", "");
            fname = fname.Replace("/", @"\");
            fname = fname.Substring(0, fname.LastIndexOf(@"\") + 1);
            return fname;
        }

        #region Native API Signatures and Types

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern uint SetNamedSecurityInfoW(String pObjectName, SE_OBJECT_TYPE ObjectType, SECURITY_INFORMATION SecurityInfo, IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl);

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern bool ConvertStringSidToSid(String StringSid, ref IntPtr Sid);

        private enum SE_OBJECT_TYPE
        {
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY,
            SE_LMSHARE,
            SE_KERNEL_OBJECT,
            SE_WINDOW_OBJECT,
            SE_DS_OBJECT,
            SE_DS_OBJECT_ALL,
            SE_PROVIDER_DEFINED_OBJECT,
            SE_WMIGUID_OBJECT,
            SE_REGISTRY_WOW64_32KEY
        }

        [Flags]
        private enum SECURITY_INFORMATION : uint
        {
            Owner = 0x00000001,
            Group = 0x00000002,
            Dacl = 0x00000004,
            Sacl = 0x00000008,
            ProtectedDacl = 0x80000000,
            ProtectedSacl = 0x40000000,
            UnprotectedDacl = 0x20000000,
            UnprotectedSacl = 0x10000000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;

            public static SECURITY_ATTRIBUTES Empty
            {
                get
                {
                    SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                    sa.nLength = sizeof(int) * 2 + IntPtr.Size;
                    sa.lpSecurityDescriptor = IntPtr.Zero;
                    sa.bInheritHandle = 0;
                    return sa;
                }
            }
        }

        /// <summary>
        /// Memory Protection Constants
        /// http://msdn.microsoft.com/en-us/library/aa366786.aspx
        /// </summary>
        [Flags]
        public enum FileProtection : uint
        {
            NONE = 0x00,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400,
            SEC_FILE = 0x800000,
            SEC_IMAGE = 0x1000000,
            SEC_RESERVE = 0x4000000,
            SEC_COMMIT = 0x8000000,
            SEC_NOCACHE = 0x10000000
        }

        /// <summary>
        /// Access rights for file mapping objects
        /// http://msdn.microsoft.com/en-us/library/aa366559.aspx
        /// </summary>
        [Flags]
        public enum FileMapAccess
        {
            FILE_MAP_COPY = 0x0001,
            FILE_MAP_WRITE = 0x0002,
            FILE_MAP_READ = 0x0004,
            FILE_MAP_ALL_ACCESS = 0x000F001F
        }

        /// <summary>
        /// Represents a wrapper class for a file mapping handle. 
        /// </summary>
        [SuppressUnmanagedCodeSecurity,
        HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
        internal sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            private SafeFileMappingHandle()
                : base(true)
            {
            }

            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            public SafeFileMappingHandle(IntPtr handle, bool ownsHandle)
                : base(ownsHandle)
            {
                base.SetHandle(handle);
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success),
            DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(base.handle);
            }
        }

        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        /// <summary>
        /// The class exposes Windows APIs used in this code sample.
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        internal class NativeMethod
        {
            /// <summary>
            /// Creates or opens a named or unnamed file mapping object for a 
            /// specified file.
            /// </summary>
            /// <param name="hFile">
            /// A handle to the file from which to create a file mapping object.
            /// </param>
            /// <param name="lpAttributes">
            /// A pointer to a SECURITY_ATTRIBUTES structure that determines 
            /// whether a returned handle can be inherited by child processes.
            /// </param>
            /// <param name="flProtect">
            /// Specifies the page protection of the file mapping object. All 
            /// mapped views of the object must be compatible with this 
            /// protection.
            /// </param>
            /// <param name="dwMaximumSizeHigh">
            /// The high-order DWORD of the maximum size of the file mapping 
            /// object.
            /// </param>
            /// <param name="dwMaximumSizeLow">
            /// The low-order DWORD of the maximum size of the file mapping 
            /// object.
            /// </param>
            /// <param name="lpName">
            /// The name of the file mapping object.
            /// </param>
            /// <returns>
            /// If the function succeeds, the return value is a handle to the 
            /// newly created file mapping object.
            /// </returns>
            [DllImport("Kernel32.dll", SetLastError = true)]
            public static extern SafeFileMappingHandle CreateFileMapping(
                IntPtr hFile,
                ref SECURITY_ATTRIBUTES lpAttributes,
                FileProtection flProtect,
                uint dwMaximumSizeHigh,
                uint dwMaximumSizeLow,
                string lpName);


            /// <summary>
            /// Maps a view of a file mapping into the address space of a calling
            /// process.
            /// </summary>
            /// <param name="hFileMappingObject">
            /// A handle to a file mapping object. The CreateFileMapping and 
            /// OpenFileMapping functions return this handle.
            /// </param>
            /// <param name="dwDesiredAccess">
            /// The type of access to a file mapping object, which determines the 
            /// protection of the pages.
            /// </param>
            /// <param name="dwFileOffsetHigh">
            /// A high-order DWORD of the file offset where the view begins.
            /// </param>
            /// <param name="dwFileOffsetLow">
            /// A low-order DWORD of the file offset where the view is to begin.
            /// </param>
            /// <param name="dwNumberOfBytesToMap">
            /// The number of bytes of a file mapping to map to the view. All bytes 
            /// must be within the maximum size specified by CreateFileMapping.
            /// </param>
            /// <returns>
            /// If the function succeeds, the return value is the starting address 
            /// of the mapped view.
            /// </returns>
            [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr MapViewOfFile(
                SafeFileMappingHandle hFileMappingObject,
                FileMapAccess dwDesiredAccess,
                uint dwFileOffsetHigh,
                uint dwFileOffsetLow,
                uint dwNumberOfBytesToMap);


            /// <summary>
            /// Unmaps a mapped view of a file from the calling process's address 
            /// space.
            /// </summary>
            /// <param name="lpBaseAddress">
            /// A pointer to the base address of the mapped view of a file that 
            /// is to be unmapped.
            /// </param>
            /// <returns></returns>
            [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
        }

        #endregion
    }

    /// <summary>
    ///     Sample Class For Serialization Objects
    /// </summary>
    [Serializable]
    public class SampleClass4Serialize
    {
        public int AAAA = -1;
        public string BBBB = "";
        public bool CCCC = false;
        public DateTime DDDD = DateTime.MinValue;

        public SampleClass4Serialize() { }
        public SampleClass4Serialize(int a, string b, bool c) { this.AAAA = a; this.BBBB = b; this.CCCC = c; this.DDDD = DateTime.UtcNow; }

        public override string ToString() { return String.Format("A = {0}, B = {1}, C = {2}, D = {3}", AAAA, BBBB, CCCC, DDDD); }
    }    

    /// <summary>
    ///     Sample Class For Marshal Objects
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public class SampleClass4Marshal
    {
        private const char splitter = /* RS */ (char) 30; // 5 - ENQ (Enquiry) // 24 - CAN (Cancel) // 28 - FS (File Separator) // 27 - ESC (Escape) // 29 - GS (GGroup Separator) // 30 - RS (Record Separator) // 31 - S (Unit Separator)

        public int x = 0;
        public int y = 0;
        public DateTime dt = DateTime.MinValue;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6144)]
        public string name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 202400)]
        private string values;

        public string[] VALUES
        {
            get
            {
                if (String.IsNullOrEmpty(values)) return null;
                return values.Split(new char[] { splitter });
            }
            set
            {
                if ((value == null) || (value.Length == 0)) values = null;
                values = string.Join(new string(new char[] { splitter }), value);
            }
        }

        public SampleClass4Marshal() { }
        public SampleClass4Marshal(int x, int y, string name, string[] vals) { this.x = x; this.y = y; this.name = name; this.dt = DateTime.Now; this.VALUES = vals; }
        public override string ToString() { return String.Format("{3} {0};{1} {2} {4}", x, y, name, dt, values); }
    }

    /// <summary>
    ///     Class to Receive System Messages (SendMessage, PostMessage, SendNotifyMessage)
    /// </summary>
    public class IncomingMessagesWindow : NativeWindow
    {
        private const int HWND_BROADCAST = 0xFFFF;
        private const int WM_CLOSE = 0x0010;
        private const int WM_USER = 0x0400;
        private const int WM_USER_MAX = 0x7FFF;
        private const int WM_APP = 0x8000;
        private const int WM_APP_MAX = 0xBFFF;        

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SendNotifyMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private int WM_USERNOTIFY = 0;
        private IntPtr _appHandle = IntPtr.Zero;
        private string _caption = "NoName";
        private MemoryFile.OnGetNotify _onGetNotify = null;

        public IncomingMessagesWindow(string caption, IntPtr applicationHandle, MemoryFile.OnGetNotify onGetNotify)
        {
            _caption = "MSG_" + caption;
            _appHandle = applicationHandle;
            WM_USERNOTIFY = RegisterWindowMessage(_caption);
            _caption += "_" + WM_USERNOTIFY.ToString();
            _onGetNotify = onGetNotify;

            CreateParams cp = new CreateParams();
            cp.Style = 0;
            cp.ExStyle = 0;
            cp.ClassStyle = 0;
            cp.Caption = _caption;
            cp.Parent = IntPtr.Zero;
            CreateHandle(cp);
        }

        private static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                StringBuilder builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            };
            return String.Empty;
        }

        private static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate(IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(wnd);
                };
                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);
            return windows;
        }

        public static IEnumerable<IntPtr> FindWindows(string titleText)
        {
            return FindWindows(delegate(IntPtr wnd, IntPtr param)
            {
                return GetWindowText(wnd).Contains(titleText);
            });
        } 

        public void SendMessage(int message)
        {
            try
            {
                // Broadcast
                SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_USERNOTIFY, message, _appHandle);

                //// No Broadcast
                //IEnumerable<IntPtr> apps = FindWindows(_caption);
                //if (apps == null) return;
                //foreach (IntPtr app in apps)
                //{
                //    try
                //    {
                //        SendNotifyMessage(app, WM_USERNOTIFY, message, _appHandle);
                //        //PostMessage(app, WM_USERNOTIFY, message, (int)_appHandle);
                //        //SendMessage(app, WM_USERNOTIFY, message, _appHandle);                    
                //    }
                //    catch (Exception ex)
                //    {
                //       _lastEx = ex;
                //    };
                //};
            }
            catch { }
        }

        public void Destroy()
        {
            SendMessage(this.Handle, WM_CLOSE, 0, IntPtr.Zero);
            ReleaseHandle();
        }

        protected override void WndProc(ref Message m)
        {
            // WM_USER //
            if ((m.Msg >= WM_USER) && (m.Msg <= WM_USER_MAX)) { };

            // WM_APP //
            if ((m.Msg == WM_APP) && (m.Msg <= WM_APP_MAX)) { };

            // Registered by RegisterWindowMessage //
            if (m.Msg == WM_USERNOTIFY) 
            {
                if (m.LParam != _appHandle)
                {
                    MemoryFile.NotifyEvent ne = (MemoryFile.NotifyEvent)m.WParam;
                    byte paramVal = 0;
                    if ((int)m.WParam >= 0xFF)
                    {
                        ne = MemoryFile.NotifyEvent.fUserEvent;
                        paramVal = (byte)m.WParam;
                    };
                    _onGetNotify(ne, MemoryFile.NotifySource.nsSystem, paramVal);
                };
            };

            base.WndProc(ref m);
        }
    }    
}
