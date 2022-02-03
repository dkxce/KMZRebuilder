using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Threading;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KMZRebuilder
{
    static class Program
    {
        public static KMZRebuilederForm mainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {            
            //FormKMZ.ConvertImageToBmp8bppIndexed(@"C:\Downloads\CD-REC\KMZRebuilder[Sources]\bin\Debug\dot[red].png",
            //    @"C:\Downloads\CD-REC\KMZRebuilder[Sources]\bin\Debug\dot[red].bmp");
            //return;

            if ((args != null) && (args.Length > 2) && ((args[0].ToLower() == "/kmz2gpi") || (args[0].ToLower() == "/kml2gpi")))
            {
                WinConsoleApplication.Initialize(true, true, false);
                KMZRebuilederForm.ConvertKMZ2GPI(args[1], args[2]);
                WinConsoleApplication.DeInitialize();
                return;
            };

            if ((args != null) && (args.Length > 2) && ((args[0].ToLower() == "/gpi2kmz") || (args[0].ToLower() == "/gpi2kml")))
            {
                WinConsoleApplication.Initialize(true, true, false);
                KMZRebuilederForm.ConvertGPI2KMZ(args[1], args[2]);
                WinConsoleApplication.DeInitialize();
                return;
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if ((args != null) && (args.Length > 0) && (args[0] == "/bpa"))
            {
                Application.Run(new BenzinPriceAnalizer.BenzinPriceAnalizerForm());
                return;
            };
            if((args != null) && (args.Length > 0) && (args[0] == "/km"))
            {
                Application.Run(new KMNumeratorForm());
                return;
            };
            if((args != null) && (args.Length > 0) && (args[0] == "/ilp"))
            {
                Application.Run(new InterLessForm(null));
                return;
            };
            if ((args != null) && (args.Length > 0) && (args[0] == "/tsp"))
            {
                Application.Run(new TrackSplitter(null));
                return;
            };
            if((args != null) && (args.Length > 0) && (args[0] == "/mpc"))
            {
                Application.Run(new PolyCreator(null));
                return;
            };
            if ((args != null) && (args.Length > 0) && (args[0] == "/tax"))
            {
                Application.Run(new GPX_Tacho.GPXTachograph());
                return;
            };
            if ((args != null) && (args.Length > 0) && (args[0] == "/llc"))
            {
                Application.Run(new WGSFormX());
                return;
            };            

            Application.Run(mainForm = new KMZRebuilederForm(args));
        }

        //private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    MessageBox.Show("The method or operation is not implemented.");
        //}

        //private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}        
    } 
   
    public static class WinConsoleApplication
    {
        public static Encoding ConsoleEncoding = Encoding.GetEncoding(866);

        public static void Initialize(bool createNewConsole, bool allowWrite, bool allowRead)
        {
            bool consoleAttached = true;
            if (createNewConsole || ((AttachConsole(ATTACH_PARRENT) == 0) && (Marshal.GetLastWin32Error() != ERROR_ACCESS_DENIED)))
                consoleAttached = AllocConsole() != 0;
                        
            if (consoleAttached)
            {
                if (allowWrite) InitStdOut();
                if (allowRead) InitStdIn();
            };
            try
            {
                Console.OutputEncoding = ConsoleEncoding;
                Console.InputEncoding = ConsoleEncoding;
            }
            catch { };
        }

        public static void DeInitialize()
        {
            Console.Out.Close();
            Console.In.Close();
            FreeConsole();
        }

        private static void InitStdOut()
        {
            FileStream fs = CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write);
            if (fs != null)
            {
                StreamWriter writer = new StreamWriter(fs, ConsoleEncoding);
                writer.AutoFlush = true;                
                Console.SetOut(writer);
                Console.SetError(writer);                
            };
        }

        private static void InitStdIn()
        {
            FileStream fs = CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read);
            if (fs != null)
            {
                Console.SetIn(new StreamReader(fs, ConsoleEncoding));                
            };
        }

        private static FileStream CreateFileStream(string name, uint win32DesiredAccess, uint win32ShareMode, FileAccess dotNetFileAccess)
        {
            Microsoft.Win32.SafeHandles.SafeFileHandle file = new Microsoft.Win32.SafeHandles.SafeFileHandle(CreateFileW(name, win32DesiredAccess, win32ShareMode, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero), true);
            if (!file.IsInvalid)
            {
                FileStream fs = new FileStream(file, dotNetFileAccess);
                return fs;
            };
            return null;
        }

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        [DllImport("kernel32.dll", EntryPoint = "AttachConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern UInt32 AttachConsole(UInt32 dwProcessId);

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CreateFileW(string lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", EntryPoint = "FreeConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool FreeConsole();

        private const UInt32 GENERIC_WRITE = 0x40000000;
        private const UInt32 GENERIC_READ = 0x80000000;
        private const UInt32 FILE_SHARE_READ = 0x00000001;
        private const UInt32 FILE_SHARE_WRITE = 0x00000002;
        private const UInt32 OPEN_EXISTING = 0x00000003;
        private const UInt32 FILE_ATTRIBUTE_NORMAL = 0x80;
        private const UInt32 ERROR_ACCESS_DENIED = 5;
        private const UInt32 ATTACH_PARRENT = 0xFFFFFFFF;
    }   
}