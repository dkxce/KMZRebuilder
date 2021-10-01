using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.IO;

namespace KMZRebuilder
{
    public class MruList
    {
        private string MRUListSavedFileName;
        private int MRUFilesCount;
        private List<FileInfo> MRUFilesInfos;
        private ToolStripMenuItem MyMenu;

        private bool UseSeparator = false;
        private ToolStripSeparator Separator = null;
        private ToolStripMenuItem[] MenuItems;

        // Raised when the user selects a file from the MRU list.
        public delegate void FileSelectedEventHandler(string file_name);
        public event FileSelectedEventHandler FileSelected;

        public int Count { get { return MRUFilesInfos.Count; } }

        // Constructor.
        public MruList(string MRUFileName, ToolStripMenuItem menu, int num_files)
        {
            this.MRUListSavedFileName = MRUFileName;
            MyMenu = menu;
            MRUFilesCount = num_files;
            MRUFilesInfos = new List<FileInfo>();

            // Make a separator
            Separator = new ToolStripSeparator();
            Separator.Visible = false;
            if (UseSeparator) MyMenu.DropDownItems.Add(Separator);

            // Make the menu items we may later need.
            MenuItems = new ToolStripMenuItem[MRUFilesCount + 1];
            for (int i = 0; i < MRUFilesCount; i++)
            {
                MenuItems[i] = new ToolStripMenuItem();
                MenuItems[i].Visible = false;
                MyMenu.DropDownItems.Add(MenuItems[i]);
            };

            // Reload items from the registry.
            LoadFiles();

            // Display the items.
            ShowFiles();
        }

        private void LoadFiles()
        {
            string filemru = this.MRUListSavedFileName;
            if (!File.Exists(filemru)) return; 

            FileStream fs = new FileStream(filemru, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
            while (!sr.EndOfStream)
            {
                string filename = sr.ReadLine();
                if (File.Exists(filename))
                    MRUFilesInfos.Add(new FileInfo(filename));
                else if (Directory.Exists(filename))
                        MRUFilesInfos.Add(new FileInfo(filename));
                        
            };
            sr.Close();
            fs.Close();
        }

        // Save the current items in the Registry.
        private void SaveFiles()
        {            
            string filemru = this.MRUListSavedFileName;
            if (filemru == null) return;
            FileStream fs = new FileStream(filemru, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
            foreach (FileInfo file_info in MRUFilesInfos)
                sw.WriteLine(file_info.FullName);
            sw.Close();
            fs.Close();            
        }

        // Remove a file's info from the list.
        private void RemoveFileInfo(string file_name)
        {
            // Remove occurrences of the file's information from the list.
            for (int i = MRUFilesInfos.Count - 1; i >= 0; i--)
            {
                if (MRUFilesInfos[i].FullName == file_name) MRUFilesInfos.RemoveAt(i);
            }
        }

        // Add a file to the list, rearranging if necessary.
        public void AddFile(string file_name)
        {
            // Remove the file from the list.
            RemoveFileInfo(file_name);

            // Add the file to the beginning of the list.
            MRUFilesInfos.Insert(0, new FileInfo(file_name));

            // If we have too many items, remove the last one.
            if (MRUFilesInfos.Count > MRUFilesCount) MRUFilesInfos.RemoveAt(MRUFilesCount);

            // Display the files.
            ShowFiles();

            // Update the Registry.
            SaveFiles();
        }

        // Remove a file from the list, rearranging if necessary.
        public void RemoveFile(string file_name)
        {
            // Remove the file from the list.
            RemoveFileInfo(file_name);

            // Display the files.
            ShowFiles();

            // Update the Registry.
            SaveFiles();
        }

        // Display the files in the menu items.
        private void ShowFiles()
        {
            Separator.Visible = (MRUFilesInfos.Count > 0);
            for (int i = 0; i < MRUFilesInfos.Count; i++)
            {
                string name = "`"+MRUFilesInfos[i].Name + "` at .. " + MRUFilesInfos[i].FullName.Remove(MRUFilesInfos[i].FullName.Length-MRUFilesInfos[i].Name.Length);
                while (name.Length > 90) name = name.Remove(name.IndexOf("` at .. ") + 8, 1);
                MenuItems[i].Text = string.Format("&{0} {1}", i + 1, name);
                MenuItems[i].Visible = true;
                MenuItems[i].Tag = MRUFilesInfos[i];
                MenuItems[i].Click -= File_Click;
                MenuItems[i].Click += File_Click;
            }
            for (int i = MRUFilesInfos.Count; i < MRUFilesCount; i++)
            {
                MenuItems[i].Visible = false;
                MenuItems[i].Click -= File_Click;
            }
        }

        // The user selected a file from the menu.
        private void File_Click(object sender, EventArgs e)
        {
            // Don't bother if no one wants to catch the event.
            if (FileSelected != null)
            {
                // Get the corresponding FileInfo object.
                ToolStripMenuItem menu_item = sender as ToolStripMenuItem;
                FileInfo file_info = menu_item.Tag as FileInfo;

                // Raise the event.
                FileSelected(file_info.FullName);
            }
        }
    }
}
