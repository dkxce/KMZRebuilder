using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KMZRebuilder
{
    public partial class FindReplaceDlg : Form
    {
        public int intIndex = 0;
        public string xmlText = "";

        public int currentIndex = -1;
        private bool findOnly = false;
        public object CustomData = null;

        public FindReplaceDlg()
        {
            InitializeComponent();
        }

        private void FindText_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void FindText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                if (onFind != null)
                    onFind(sender, e);

            if (e.KeyChar == (char)27)
                Close();
        }

        public EventHandler onFind;
        public EventHandler onFindAll;
        public EventHandler onReplace;
        public EventHandler onReplaceFind;
        public EventHandler onReplaceAll;
        public EventHandler onFocus;

        public bool CheckedOnly
        {
            get
            {
                return this.OnlyChecked.Checked;
            }
            set
            {
                this.OnlyChecked.Checked = value;
            }
        }

        public string Find
        {
            get
            {
                return this.FindText.Text.Trim();
            }
            set
            {
                this.FindText.Text = value;
            }
        }

        public string Replace
        {
            get
            {
                return this.ReplaceText.Text.Trim();
            }
            set
            {
                this.ReplaceText.Text = value;
            }
        }

        public bool Up
        {
            get
            {
                return this.UP.Checked;
            }
            set
            {
                this.UP.Checked = value;
            }
        }

        public bool Down
        {
            get
            {
                return this.DOWN.Checked;
            }
            set
            {
                this.DOWN.Checked = value;
            }
        }

        public bool CaseIgnore
        {
            get
            {
                return this.IgnoreCase.Checked;
            }
            set
            {
                this.IgnoreCase.Checked = value;
            }
        }

        public bool FindOnly
        {
            get
            {
                return findOnly;
            }
            set
            {
                if (value)
                    REPF.Text = "Find All";
                else
                    REPF.Text = "Replace && Find";
                SetFindOnly(value);
            }
        }

        private void SetFindOnly(bool hideReplace)
        {
            this.findOnly = hideReplace;
            //REPF.Enabled = !this.findOnly;
            ReplaceButton.Enabled = !this.findOnly;
            ReplaceALL.Enabled = !this.findOnly;
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            if (onFind != null) onFind(sender, e);
        }

        private void ReplaceButton_Click(object sender, EventArgs e)
        {
            if (onReplace != null) onReplace(sender, e);
        }

        private void ReplaceText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)27)
                Close();

            if (this.findOnly) return;

            if (e.KeyChar == '\r')
                if (onReplace != null) 
                    onReplace(sender, e);            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.findOnly)
            {
                if (onFindAll != null) onFindAll(sender, e);
            }
            else
            {
                if (onReplaceFind != null) onReplaceFind(sender, e);
            };
        }

        private void ReplaceALL_Click(object sender, EventArgs e)
        {
            if (onReplaceAll != null)
                onReplaceAll(sender, e);
        }

        private void FindReplaceDlg_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)27)
                Close();
        }

        private void FindReplaceDlg_Activated(object sender, EventArgs e)
        {
            if (onFocus != null)
                onFocus(sender, e);
        }

        private void FindReplaceDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
        }

        private void OnlyChecked_CheckedChanged(object sender, EventArgs e)
        {
            REPF.Enabled = !(this.findOnly && OnlyChecked.Checked);
        }
    }
}