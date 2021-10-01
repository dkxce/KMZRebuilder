namespace KMZRebuilder
{
    partial class RenameDat
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
            this.listView2 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ïðèìåíèòüÊîÂñåìToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.autodetectByImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autodetectByImageComparationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autodetectByInternalImageMapCRCTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoFillByImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.loadImageMapFromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImageMapToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.svimm = new System.Windows.Forms.CheckBox();
            this.rds = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.sortasc = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.datpanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.datpanel.SuspendLayout();
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
            this.listView1.Size = new System.Drawing.Size(403, 523);
            this.listView1.TabIndex = 20;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Øèð";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Äîë";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Ìåñòî";
            this.columnHeader10.Width = 190;
            // 
            // listView2
            // 
            this.listView2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3});
            this.listView2.ContextMenuStrip = this.contextMenuStrip1;
            this.listView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listView2.FullRowSelect = true;
            this.listView2.GridLines = true;
            this.listView2.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView2.Location = new System.Drawing.Point(0, 0);
            this.listView2.MultiSelect = false;
            this.listView2.Name = "listView2";
            this.listView2.OwnerDraw = true;
            this.listView2.Size = new System.Drawing.Size(403, 425);
            this.listView2.TabIndex = 21;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.View = System.Windows.Forms.View.Details;
            this.listView2.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listView2_DrawColumnHeader);
            this.listView2.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView2_DrawItem);
            this.listView2.DoubleClick += new System.EventHandler(this.listView2_DoubleClick);
            this.listView2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listView2_KeyPress);
            this.listView2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listView2_KeyDown);
            this.listView2.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listView2_DrawSubItem);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "KMZ Type";
            this.columnHeader1.Width = 170;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "PROGOROD Type";
            this.columnHeader3.Width = 190;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ïðèìåíèòüÊîÂñåìToolStripMenuItem,
            this.toolStripMenuItem1,
            this.autodetectByImageToolStripMenuItem,
            this.autodetectByInternalImageMapCRCTableToolStripMenuItem,
            this.autodetectByImageComparationToolStripMenuItem,
            this.autoFillByImageToolStripMenuItem,
            this.toolStripMenuItem3,
            this.loadImageMapFromFileToolStripMenuItem,
            this.saveImageMapToFileToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(346, 192);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // ïðèìåíèòüÊîÂñåìToolStripMenuItem
            // 
            this.ïðèìåíèòüÊîÂñåìToolStripMenuItem.Name = "ïðèìåíèòüÊîÂñåìToolStripMenuItem";
            this.ïðèìåíèòüÊîÂñåìToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            this.ïðèìåíèòüÊîÂñåìToolStripMenuItem.Text = "Apply to all ...";
            this.ïðèìåíèòüÊîÂñåìToolStripMenuItem.Click += new System.EventHandler(this.ïðèìåíèòüÊîÂñåìToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(342, 6);
            // 
            // autodetectByImageToolStripMenuItem
            // 
            this.autodetectByImageToolStripMenuItem.Name = "autodetectByImageToolStripMenuItem";
            this.autodetectByImageToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            this.autodetectByImageToolStripMenuItem.Text = "Autodetect by Image Tags";
            this.autodetectByImageToolStripMenuItem.Click += new System.EventHandler(this.autodetectByImageToolStripMenuItem_Click);
            // 
            // autodetectByImageComparationToolStripMenuItem
            // 
            this.autodetectByImageComparationToolStripMenuItem.Name = "autodetectByImageComparationToolStripMenuItem";
            this.autodetectByImageComparationToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            this.autodetectByImageComparationToolStripMenuItem.Text = "Autodetect by Image Comparation";
            this.autodetectByImageComparationToolStripMenuItem.Click += new System.EventHandler(this.autodetectByImageComparationToolStripMenuItem_Click);
            // 
            // autodetectByInternalImageMapCRCTableToolStripMenuItem
            // 
            this.autodetectByInternalImageMapCRCTableToolStripMenuItem.Name = "autodetectByInternalImageMapCRCTableToolStripMenuItem";
            this.autodetectByInternalImageMapCRCTableToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            this.autodetectByInternalImageMapCRCTableToolStripMenuItem.Text = "Autodetect by Internal ImageMap CRC Table (.imm)";
            this.autodetectByInternalImageMapCRCTableToolStripMenuItem.Click += new System.EventHandler(this.autodetectByInternalImageMapCRCTableToolStripMenuItem_Click);
            // 
            // autoFillByImageToolStripMenuItem
            // 
            this.autoFillByImageToolStripMenuItem.Name = "autoFillByImageToolStripMenuItem";
            this.autoFillByImageToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            this.autoFillByImageToolStripMenuItem.Text = "Autodetect by KML Style History";
            this.autoFillByImageToolStripMenuItem.Click += new System.EventHandler(this.autoFillByImageToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(342, 6);
            // 
            // loadImageMapFromFileToolStripMenuItem
            // 
            this.loadImageMapFromFileToolStripMenuItem.Name = "loadImageMapFromFileToolStripMenuItem";
            this.loadImageMapFromFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.loadImageMapFromFileToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            this.loadImageMapFromFileToolStripMenuItem.Text = "Load ImageMap CRC Table From File (.imm) ...";
            this.loadImageMapFromFileToolStripMenuItem.Click += new System.EventHandler(this.loadImageMapFromFileToolStripMenuItem_Click);
            // 
            // saveImageMapToFileToolStripMenuItem
            // 
            this.saveImageMapToFileToolStripMenuItem.Name = "saveImageMapToFileToolStripMenuItem";
            this.saveImageMapToFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveImageMapToFileToolStripMenuItem.Size = new System.Drawing.Size(345, 22);
            this.saveImageMapToFileToolStripMenuItem.Text = "Save ImageMap CRC Table To File (.imm) ...";
            this.saveImageMapToFileToolStripMenuItem.Click += new System.EventHandler(this.saveImageMapToFileToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.svimm);
            this.panel1.Controls.Add(this.rds);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.sortasc);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 474);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(403, 49);
            this.panel1.TabIndex = 22;
            // 
            // svimm
            // 
            this.svimm.AutoSize = true;
            this.svimm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.svimm.Location = new System.Drawing.Point(4, 30);
            this.svimm.Name = "svimm";
            this.svimm.Size = new System.Drawing.Size(201, 17);
            this.svimm.TabIndex = 3;
            this.svimm.Text = "Save ImageMap With Destination File";
            this.svimm.UseVisualStyleBackColor = true;
            this.svimm.CheckedChanged += new System.EventHandler(this.svimm_CheckedChanged);
            // 
            // rds
            // 
            this.rds.AutoSize = true;
            this.rds.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rds.Location = new System.Drawing.Point(4, 15);
            this.rds.Name = "rds";
            this.rds.Size = new System.Drawing.Size(124, 17);
            this.rds.TabIndex = 2;
            this.rds.Text = "Remove Descriptions";
            this.rds.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(311, 10);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // sortasc
            // 
            this.sortasc.AutoSize = true;
            this.sortasc.Checked = true;
            this.sortasc.CheckState = System.Windows.Forms.CheckState.Checked;
            this.sortasc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sortasc.Location = new System.Drawing.Point(4, 0);
            this.sortasc.Name = "sortasc";
            this.sortasc.Size = new System.Drawing.Size(163, 17);
            this.sortasc.TabIndex = 1;
            this.sortasc.Text = "Sort Waypoints Alphabetically";
            this.sortasc.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(232, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // datpanel
            // 
            this.datpanel.Controls.Add(this.label1);
            this.datpanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.datpanel.Location = new System.Drawing.Point(0, 425);
            this.datpanel.Name = "datpanel";
            this.datpanel.Size = new System.Drawing.Size(403, 49);
            this.datpanel.TabIndex = 24;
            this.datpanel.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(222, 39);
            this.label1.TabIndex = 0;
            this.label1.Text = "You can use next placemark description tags:\r\n  progorod_dat_home=yes/true\r\n  pro" +
                "gorod_dat_office=yes/true";
            // 
            // RenameDat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 523);
            this.Controls.Add(this.listView2);
            this.Controls.Add(this.datpanel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.listView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RenameDat";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set Icon/Symbol Type";
            this.Load += new System.EventHandler(this.RenameDat_Load);
            this.Shown += new System.EventHandler(this.RenameDat_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RenameDat_FormClosing);
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.datpanel.ResumeLayout(false);
            this.datpanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        public System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ïðèìåíèòüÊîÂñåìToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoFillByImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autodetectByImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem loadImageMapFromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveImageMapToFileToolStripMenuItem;
        public System.Windows.Forms.Panel datpanel;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.CheckBox sortasc;
        public System.Windows.Forms.CheckBox rds;
        private System.Windows.Forms.ToolStripMenuItem autodetectByInternalImageMapCRCTableToolStripMenuItem;
        private System.Windows.Forms.CheckBox svimm;
        private System.Windows.Forms.ToolStripMenuItem autodetectByImageComparationToolStripMenuItem;
    }
}