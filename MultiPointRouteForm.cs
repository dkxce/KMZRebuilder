using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KMZRebuilder
{
    public partial class MultiPointRouteForm : Form
    {
        object temp;
        int trackingItem;

        public MultiPointRouteForm()
        {
            InitializeComponent();
        }

        public List<KeyValuePair<string, PointF>> Points
        {
            get
            {
                List<KeyValuePair<string, PointF>> cpts = new List<KeyValuePair<string, PointF>>();
                for (int i = 0; i < pbox.CheckedItems.Count; i++)
                {
                    KeyValuePair<string, PointF> p = (KeyValuePair<string, PointF>)pbox.CheckedItems[i];
                    cpts.Add(p);
                };
                return cpts;
            }
            set
            {
                InitPoints(value);
            }
        }

        private void InitPoints(List<KeyValuePair<string, PointF>> points)
        {
            pbox.Items.Clear();
            if ((points == null) || (points.Count == 0)) return;
            for (int i = 0; i < points.Count; i++)
                pbox.Items.Add(points[i], true);
        }

        public void MoveUp()
        {
            MoveItem(-1);
        }

        public void MoveDown()
        {
            MoveItem(1);
        }

        public void MoveItem(int direction)
        {
            if (pbox.SelectedItem == null || pbox.SelectedIndex < 0) return;
            int newIndex = pbox.SelectedIndex + direction;
            if (newIndex < 0 || newIndex >= pbox.Items.Count) return;
            object selected = pbox.SelectedItem;
            pbox.Items.Remove(selected);
            pbox.Items.Insert(newIndex, selected);
            pbox.SelectedIndex = newIndex;
            pbox.SetItemChecked(newIndex, true);
        }

        private void pbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Alt)
            {
                if (e.KeyValue == 38) { MoveUp(); e.Handled = false; };
                if (e.KeyValue == 40) { MoveDown(); e.Handled = false; };
            };
        }

        private void pbox_MouseDown(object sender, MouseEventArgs e)
        {
            if (pbox.SelectedIndex < 0) return;

            base.OnMouseDown(e);
            pbox.Cursor = Cursors.Hand;
            trackingItem = pbox.SelectedIndex;
            if (trackingItem >= 0)
            { temp = pbox.Items[pbox.SelectedIndex]; }
        }

        private void pbox_MouseUp(object sender, MouseEventArgs e)
        {
            if (pbox.SelectedIndex < 0) return;

            base.OnMouseUp(e);
            int tempInd = pbox.SelectedIndex;
            if ((tempInd >= 0) && (trackingItem != tempInd))
            {
                pbox.Items.RemoveAt(trackingItem);
                pbox.Items.Insert(tempInd, temp);
                pbox.SelectedIndex = tempInd;
                pbox.SetItemChecked(tempInd, true);
            };            
            pbox.Cursor = Cursors.Default;
        }

        private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < pbox.CheckedItems.Count; i++)
                pbox.SetItemChecked(i, true);
        }

        private void checkNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < pbox.CheckedItems.Count; i++)
                pbox.SetItemChecked(i, false);
        }

        private void inverseCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < pbox.CheckedItems.Count; i++)
                pbox.SetItemChecked(i, !pbox.GetItemChecked(i));
        }

        private void deleteCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = pbox.CheckedItems.Count - 1; i >= 0; i--)
                if(pbox.GetItemChecked(i))
                    pbox.Items.RemoveAt(i);
        }

        private void deleteUncheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = pbox.CheckedItems.Count - 1; i >= 0; i--)
                if (!pbox.GetItemChecked(i))
                    pbox.Items.RemoveAt(i);
        }
    }

}