namespace KMZRebuilder
{
    partial class GMLayRenamerForm
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
            this.layers = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeBitmapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.loadNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.images = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.selttl = new System.Windows.Forms.Label();
            this.subtext = new System.Windows.Forms.Label();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.switchToSubnameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // layers
            // 
            this.layers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.layers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.layers.ContextMenuStrip = this.contextMenuStrip1;
            this.layers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.layers.LargeImageList = this.images;
            this.layers.Location = new System.Drawing.Point(12, 25);
            this.layers.MultiSelect = false;
            this.layers.Name = "layers";
            this.layers.ShowGroups = false;
            this.layers.Size = new System.Drawing.Size(581, 396);
            this.layers.SmallImageList = this.images;
            this.layers.TabIndex = 0;
            this.layers.UseCompatibleStateImageBehavior = false;
            this.layers.View = System.Windows.Forms.View.Details;
            this.layers.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.layers_ItemChecked);
            this.layers.SelectedIndexChanged += new System.EventHandler(this.layers_SelectedIndexChanged);
            this.layers.DoubleClick += new System.EventHandler(this.layers_DoubleClick);
            this.layers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.layers_KeyDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 550;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renameToolStripMenuItem,
            this.changeBitmapToolStripMenuItem,
            this.toolStripMenuItem1,
            this.saveListToolStripMenuItem,
            this.saveImagesToolStripMenuItem,
            this.toolStripMenuItem2,
            this.loadNamesToolStripMenuItem,
            this.loadImagesToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(282, 148);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(281, 22);
            this.renameToolStripMenuItem.Text = "Rename layer...";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // changeBitmapToolStripMenuItem
            // 
            this.changeBitmapToolStripMenuItem.Name = "changeBitmapToolStripMenuItem";
            this.changeBitmapToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F4;
            this.changeBitmapToolStripMenuItem.Size = new System.Drawing.Size(281, 22);
            this.changeBitmapToolStripMenuItem.Text = "Change bitmap...";
            this.changeBitmapToolStripMenuItem.Click += new System.EventHandler(this.changeBitmapToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(278, 6);
            // 
            // saveListToolStripMenuItem
            // 
            this.saveListToolStripMenuItem.Name = "saveListToolStripMenuItem";
            this.saveListToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveListToolStripMenuItem.Size = new System.Drawing.Size(281, 22);
            this.saveListToolStripMenuItem.Text = "Save names...";
            this.saveListToolStripMenuItem.Click += new System.EventHandler(this.saveListToolStripMenuItem_Click);
            // 
            // saveImagesToolStripMenuItem
            // 
            this.saveImagesToolStripMenuItem.Name = "saveImagesToolStripMenuItem";
            this.saveImagesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.S)));
            this.saveImagesToolStripMenuItem.Size = new System.Drawing.Size(281, 22);
            this.saveImagesToolStripMenuItem.Text = "Save names and images...";
            this.saveImagesToolStripMenuItem.Click += new System.EventHandler(this.saveImagesToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(278, 6);
            // 
            // loadNamesToolStripMenuItem
            // 
            this.loadNamesToolStripMenuItem.Name = "loadNamesToolStripMenuItem";
            this.loadNamesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.loadNamesToolStripMenuItem.Size = new System.Drawing.Size(281, 22);
            this.loadNamesToolStripMenuItem.Text = "Load names...";
            this.loadNamesToolStripMenuItem.Click += new System.EventHandler(this.loadNamesToolStripMenuItem_Click);
            // 
            // loadImagesToolStripMenuItem
            // 
            this.loadImagesToolStripMenuItem.Name = "loadImagesToolStripMenuItem";
            this.loadImagesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.O)));
            this.loadImagesToolStripMenuItem.Size = new System.Drawing.Size(281, 22);
            this.loadImagesToolStripMenuItem.Text = "Load names and images...";
            this.loadImagesToolStripMenuItem.Click += new System.EventHandler(this.loadImagesToolStripMenuItem_Click);
            // 
            // images
            // 
            this.images.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.images.ImageSize = new System.Drawing.Size(22, 22);
            this.images.TransparentColor = System.Drawing.Color.Fuchsia;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(255, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "You can enter new names for layers in XML file here:";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(267, 465);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(518, 1);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "select all";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Location = new System.Drawing.Point(444, 1);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "select none";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // selttl
            // 
            this.selttl.Location = new System.Drawing.Point(363, 434);
            this.selttl.Name = "selttl";
            this.selttl.Size = new System.Drawing.Size(230, 15);
            this.selttl.TabIndex = 5;
            this.selttl.Text = "0 / 0";
            this.selttl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // subtext
            // 
            this.subtext.AutoSize = true;
            this.subtext.Location = new System.Drawing.Point(12, 424);
            this.subtext.Name = "subtext";
            this.subtext.Size = new System.Drawing.Size(290, 39);
            this.subtext.TabIndex = 6;
            this.subtext.Text = "For automatic naming you can use tags in layers description:\r\n    gpi_name_<IMAGE" +
                "CRC>=<FULLNAME>\r\n    gpi_subname_<IMAGECRC>=<SUBNAME>";
            this.subtext.Visible = false;
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.switchToSubnameToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(215, 26);
            this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
            // 
            // switchToSubnameToolStripMenuItem
            // 
            this.switchToSubnameToolStripMenuItem.Name = "switchToSubnameToolStripMenuItem";
            this.switchToSubnameToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.switchToSubnameToolStripMenuItem.Size = new System.Drawing.Size(214, 22);
            this.switchToSubnameToolStripMenuItem.Text = "Switch to Subname";
            this.switchToSubnameToolStripMenuItem.Click += new System.EventHandler(this.switchToSubnameToolStripMenuItem_Click);
            // 
            // GMLayRenamerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 497);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.subtext);
            this.Controls.Add(this.selttl);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.layers);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GMLayRenamerForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Garmin XML Layers";
            this.Shown += new System.EventHandler(this.GMLayRenamerForm_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GMLayRenamer_FormClosing);
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.ImageList images;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeBitmapToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveImagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadImagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        public System.Windows.Forms.ListView layers;
        public System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label selttl;
        public System.Windows.Forms.Label subtext;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem switchToSubnameToolStripMenuItem;
        public System.Windows.Forms.Button button1;
    }
}