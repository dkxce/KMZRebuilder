using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace KMZRebuilder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //FormKMZ.ConvertImageToBmp8bppIndexed(@"C:\Downloads\CD-REC\KMZRebuilder[Sources]\bin\Debug\dot[red].png",
            //    @"C:\Downloads\CD-REC\KMZRebuilder[Sources]\bin\Debug\dot[red].bmp");
            //return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new KMZRebuilederForm(args));
        }        
    }

    
}