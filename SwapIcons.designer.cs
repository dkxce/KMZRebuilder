namespace KMZRebuilder
{
    partial class SwapIcons
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
            this.imlist = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.popup = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.sil = new System.Windows.Forms.ToolStripMenuItem();
            this.sif = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.autofillImageFromListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autofillImageByIDFromListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.imSize = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.getCRCOfImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.popup.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader9,
            this.columnHeader10});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(798, 523);
            this.listView1.TabIndex = 20;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Шир";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Дол";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Место";
            this.columnHeader10.Width = 190;
            // 
            // imlist
            // 
            this.imlist.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.imlist.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3});
            this.imlist.ContextMenuStrip = this.popup;
            this.imlist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imlist.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.imlist.FullRowSelect = true;
            this.imlist.GridLines = true;
            this.imlist.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.imlist.Location = new System.Drawing.Point(0, 0);
            this.imlist.Name = "imlist";
            this.imlist.OwnerDraw = true;
            this.imlist.Size = new System.Drawing.Size(798, 486);
            this.imlist.TabIndex = 21;
            this.imlist.UseCompatibleStateImageBehavior = false;
            this.imlist.View = System.Windows.Forms.View.Details;
            this.imlist.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listView2_DrawColumnHeader);
            this.imlist.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView2_DrawItem);
            this.imlist.DoubleClick += new System.EventHandler(this.listView2_DoubleClick);
            this.imlist.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listView2_KeyPress);
            this.imlist.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView2_KeyDown);
            this.imlist.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listView2_DrawSubItem);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Old Image";
            this.columnHeader1.Width = 270;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "New Image";
            this.columnHeader3.Width = 487;
            // 
            // popup
            // 
            this.popup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sil,
            this.sif,
            this.toolStripMenuItem1,
            this.getCRCOfImageToolStripMenuItem,
            this.toolStripMenuItem2,
            this.autofillImageFromListToolStripMenuItem,
            this.autofillImageByIDFromListToolStripMenuItem});
            this.popup.Name = "popup";
            this.popup.Size = new System.Drawing.Size(256, 126);
            this.popup.Opening += new System.ComponentModel.CancelEventHandler(this.popup_Opening);
            // 
            // sil
            // 
            this.sil.Name = "sil";
            this.sil.Size = new System.Drawing.Size(255, 22);
            this.sil.Text = "Select Image from List ... (Enter)";
            this.sil.Click += new System.EventHandler(this.sil_Click);
            // 
            // sif
            // 
            this.sif.Name = "sif";
            this.sif.Size = new System.Drawing.Size(255, 22);
            this.sif.Text = "Select Image from File ... (Space)";
            this.sif.Click += new System.EventHandler(this.sif_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(252, 6);
            // 
            // autofillImageFromListToolStripMenuItem
            // 
            this.autofillImageFromListToolStripMenuItem.Name = "autofillImageFromListToolStripMenuItem";
            this.autofillImageFromListToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.autofillImageFromListToolStripMenuItem.Text = "Autofill Image by Name from List ...";
            this.autofillImageFromListToolStripMenuItem.Click += new System.EventHandler(this.autofillImageFromListToolStripMenuItem_Click);
            // 
            // autofillImageByIDFromListToolStripMenuItem
            // 
            this.autofillImageByIDFromListToolStripMenuItem.Name = "autofillImageByIDFromListToolStripMenuItem";
            this.autofillImageByIDFromListToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.autofillImageByIDFromListToolStripMenuItem.Text = "Autofill Image by ID from List ...";
            this.autofillImageByIDFromListToolStripMenuItem.Click += new System.EventHandler(this.autofillImageByIDFromListToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.imSize);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 486);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(798, 37);
            this.panel1.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "New size:";
            // 
            // imSize
            // 
            this.imSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.imSize.FormattingEnabled = true;
            this.imSize.Items.AddRange(new object[] {
            "16 x 16",
            "18 x 18",
            "20 x 20",
            "22 x 22",
            "24 x 24",
            "26 x 26",
            "28 x 28",
            "30 x 30",
            "32 x 32",
            "No change"});
            this.imSize.Location = new System.Drawing.Point(71, 8);
            this.imSize.Name = "imSize";
            this.imSize.Size = new System.Drawing.Size(161, 21);
            this.imSize.TabIndex = 2;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(676, 7);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(595, 7);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(252, 6);
            // 
            // getCRCOfImageToolStripMenuItem
            // 
            this.getCRCOfImageToolStripMenuItem.Name = "getCRCOfImageToolStripMenuItem";
            this.getCRCOfImageToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.getCRCOfImageToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.getCRCOfImageToolStripMenuItem.Text = "Get CRC of Image ...";
            this.getCRCOfImageToolStripMenuItem.Click += new System.EventHandler(this.getCRCOfImageToolStripMenuItem_Click);
            // 
            // SwapIcons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(798, 523);
            this.Controls.Add(this.imlist);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.listView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SwapIcons";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Replace KML Style Icons";
            this.popup.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        public System.Windows.Forms.ListView imlist;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.ComboBox imSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip popup;
        private System.Windows.Forms.ToolStripMenuItem sil;
        private System.Windows.Forms.ToolStripMenuItem sif;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem autofillImageFromListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autofillImageByIDFromListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getCRCOfImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
    }
}