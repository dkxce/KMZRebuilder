using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KMZRebuilder
{
    public partial class LBNRenamerForm : Form
    {
        public int ttl_objs = 0;
        public List<DictionaryEntry> entries = null;
        public List<string> entriesNames = new List<string>();

        public LBNRenamerForm(int ttl_objs, List<DictionaryEntry> entries)
        {
            InitializeComponent();
            this.ttl_objs = ttl_objs;
            this.entries = entries;
            int id = 0;
            foreach (DictionaryEntry entry in this.entries)
            {
                layers.Items.Add(String.Format("{0}: {1}: {2}", id++, (string)entry.Key, ((int)entry.Value).ToString()));
                entriesNames.Add((string)entry.Key);
            };
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            renameToolStripMenuItem.Enabled = layers.SelectedItems.Count > 0;
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (layers.SelectedItems.Count == 0) return;
            string txti = layers.SelectedIndices[0].ToString() + ": ";
            string name = (layers.Items[layers.SelectedIndices[0]]).ToString().Remove(0, txti.Length);
            KMZRebuilederForm.InputBox("Layer name", "Change layer name:", ref name, null);
            layers.Items[layers.SelectedIndices[0]] = txti + name;
            entriesNames[layers.SelectedIndices[0]] = name;
        }

        private void layers_DoubleClick(object sender, EventArgs e)
        {
            renameToolStripMenuItem_Click(sender, e);
        }
    }
}