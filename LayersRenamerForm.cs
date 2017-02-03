using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KMZRebuilder
{
    public partial class LayersRenamerForm : Form
    {
        public LayersRenamerForm()
        {
            InitializeComponent();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            renameToolStripMenuItem.Enabled = layers.SelectedItems.Count > 0;
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (layers.SelectedItems.Count == 0) return;
            string txti = layers.SelectedIndices[0].ToString() + ": ";
            string name = layers.Items[layers.SelectedIndices[0]].Text.Remove(0, txti.Length);
            KMZRebuilederForm.InputBox("Layer name", "Change layer name:", ref name, (Bitmap)images.Images[layers.Items[layers.SelectedIndices[0]].ImageKey]);
            layers.Items[layers.SelectedIndices[0]].Text = txti + name;
        }

        private void layers_DoubleClick(object sender, EventArgs e)
        {
            renameToolStripMenuItem_Click(sender, e);
        }
    }
}