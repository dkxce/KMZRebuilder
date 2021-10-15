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
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public static KMZRebuilederForm mainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool onlyOneInstance = true;

            //FormKMZ.ConvertImageToBmp8bppIndexed(@"C:\Downloads\CD-REC\KMZRebuilder[Sources]\bin\Debug\dot[red].png",
            //    @"C:\Downloads\CD-REC\KMZRebuilder[Sources]\bin\Debug\dot[red].bmp");
            //return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if ((args != null) && (args.Length > 0) && (args[0] == "/m")) onlyOneInstance = false;
            if ((args != null) && (args.Length > 0) && (args[0] == "/multi")) onlyOneInstance = false;
            if ((args != null) && (args.Length > 0) && (args[0] == "/multiple")) onlyOneInstance = false;
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

            Process rp = RunningProcess();
            if ((onlyOneInstance) && (rp != null))
            {
                try { SetForegroundWindow(rp.MainWindowHandle); } catch { };
                if ((args != null) && (args.Length > 0))
                {
                    MemFile.MemoryFile mf = null;
                    try
                    {
                        mf = new MemFile.MemoryFile("KMZRebuilder");
                        mf.SetSeriazable(args);
                        mf.SetNotifyUserEvent(1);
                    }
                    catch { };
                    try { if(mf != null) mf.Close(); } catch { };                                        
                };
                Application.Exit();
                return;
            };
            Application.Run(mainForm = new KMZRebuilederForm(args, rp == null ? "" : "[" + (new Random()).Next(10,99).ToString() + "] "));
        }

        public static Process RunningProcess()
        {
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                if (process.Id != current.Id)
                    return process;
            foreach (Process process in Process.GetProcessesByName("KMZRebuilder.vshost.exe"))
                if (process.Id != current.Id)
                    return process;
            return null;
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
}