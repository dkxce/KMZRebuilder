using System;
using System.IO;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace KMZRebuilder
{
    public class FileAss
    {
        public static void SetFileAssociation(string Extension, string Class, string Command, string ExePath)
        {
            try
            {
                Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\", true)
                    .CreateSubKey("." + Extension)
                    .CreateSubKey("OpenWithList")
                    .CreateSubKey(Path.GetFileName(ExePath))
                    .SetValue("", "\"" + ExePath + "\"" + " \"%1\"");

                using (RegistryKey User_Classes = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\", true))
                using (RegistryKey User_Ext = User_Classes.CreateSubKey("." + Extension))
                using (RegistryKey User_FileClass = User_Classes.CreateSubKey(Class))
                using (RegistryKey User_Command = User_FileClass.CreateSubKey("shell").CreateSubKey(Command).CreateSubKey("command"))
                {
                    User_Ext.SetValue("", Class, RegistryValueKind.String);
                    User_Classes.SetValue("", Class, RegistryValueKind.String);
                    User_Command.SetValue("", "\"" + ExePath + "\"" + " \"%1\"");
                };                         
            }
            catch
            {
                //Your code here
            }
        }

        public static void SetFileOpenWith(string Extension, string ExePath)
        {
            try
            {
                Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\", true)
                    .CreateSubKey("." + Extension)
                    .CreateSubKey("OpenWithList")
                    .CreateSubKey(Path.GetFileName(ExePath))
                    .SetValue("", "\"" + ExePath + "\"" + " \"%1\"");
                
            }
            catch (Exception excpt)
            {
                //Your code here
            }
        }

        public static void UpdateExplorer()
        {
            try
            {
                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            }
            catch { };
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
