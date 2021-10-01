namespace KMZRebuilder
{
    partial class CSVTXTForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.codepage = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.delimiter = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.skipsw = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.flh = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.separator = new System.Windows.Forms.ComboBox();
            this.SD = new System.Windows.Forms.ListView();
            this.label6 = new System.Windows.Forms.Label();
            this.fName = new System.Windows.Forms.ComboBox();
            this.fLat = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.fLon = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.fDesc = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.OK = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.s1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.s2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.s3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.s4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label10 = new System.Windows.Forms.Label();
            this.fStyle = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File Encoding:";
            // 
            // codepage
            // 
            this.codepage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.codepage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.codepage.FormattingEnabled = true;
            this.codepage.Location = new System.Drawing.Point(12, 19);
            this.codepage.Name = "codepage";
            this.codepage.Size = new System.Drawing.Size(391, 21);
            this.codepage.TabIndex = 1;
            this.codepage.SelectedIndexChanged += new System.EventHandler(this.codepage_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(701, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Cell Delimiter:";
            // 
            // delimiter
            // 
            this.delimiter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.delimiter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.delimiter.FormattingEnabled = true;
            this.delimiter.Items.AddRange(new object[] {
            ";",
            ",",
            "@",
            "#",
            "$",
            "|",
            ":",
            "!",
            "&",
            "*",
            "~",
            "`",
            "^",
            "TAB"});
            this.delimiter.Location = new System.Drawing.Point(705, 19);
            this.delimiter.Name = "delimiter";
            this.delimiter.Size = new System.Drawing.Size(67, 21);
            this.delimiter.TabIndex = 4;
            this.delimiter.SelectedIndexChanged += new System.EventHandler(this.codepage_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(491, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Skip line starts with:";
            // 
            // skipsw
            // 
            this.skipsw.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.skipsw.Location = new System.Drawing.Point(494, 20);
            this.skipsw.Name = "skipsw";
            this.skipsw.Size = new System.Drawing.Size(114, 20);
            this.skipsw.TabIndex = 2;
            this.skipsw.Text = "# -- /* //";
            this.skipsw.TextChanged += new System.EventHandler(this.skipsw_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(613, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "1st line Header:";
            // 
            // flh
            // 
            this.flh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.flh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.flh.FormattingEnabled = true;
            this.flh.Items.AddRange(new object[] {
            "YES",
            "NO"});
            this.flh.Location = new System.Drawing.Point(616, 19);
            this.flh.Name = "flh";
            this.flh.Size = new System.Drawing.Size(81, 21);
            this.flh.TabIndex = 3;
            this.flh.SelectedIndexChanged += new System.EventHandler(this.codepage_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(777, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Deciamal separator:";
            // 
            // separator
            // 
            this.separator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.separator.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.separator.FormattingEnabled = true;
            this.separator.Items.AddRange(new object[] {
            "AUTO",
            ".",
            ","});
            this.separator.Location = new System.Drawing.Point(780, 19);
            this.separator.Name = "separator";
            this.separator.Size = new System.Drawing.Size(98, 21);
            this.separator.TabIndex = 5;
            // 
            // SD
            // 
            this.SD.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SD.GridLines = true;
            this.SD.Location = new System.Drawing.Point(12, 46);
            this.SD.MultiSelect = false;
            this.SD.Name = "SD";
            this.SD.Size = new System.Drawing.Size(866, 394);
            this.SD.TabIndex = 6;
            this.SD.UseCompatibleStateImageBehavior = false;
            this.SD.View = System.Windows.Forms.View.Details;
            this.SD.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SD_MouseClick);
            this.SD.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.SD_ColumnClick);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 442);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Name Field:";
            // 
            // fName
            // 
            this.fName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fName.FormattingEnabled = true;
            this.fName.Location = new System.Drawing.Point(12, 458);
            this.fName.Name = "fName";
            this.fName.Size = new System.Drawing.Size(210, 21);
            this.fName.TabIndex = 10;
            this.fName.SelectedIndexChanged += new System.EventHandler(this.fName_SelectedIndexChanged);
            // 
            // fLat
            // 
            this.fLat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fLat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fLat.FormattingEnabled = true;
            this.fLat.Location = new System.Drawing.Point(449, 458);
            this.fLat.Name = "fLat";
            this.fLat.Size = new System.Drawing.Size(210, 21);
            this.fLat.TabIndex = 12;
            this.fLat.SelectedIndexChanged += new System.EventHandler(this.fName_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(446, 442);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Latitude:";
            // 
            // fLon
            // 
            this.fLon.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fLon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fLon.FormattingEnabled = true;
            this.fLon.Location = new System.Drawing.Point(668, 458);
            this.fLon.Name = "fLon";
            this.fLon.Size = new System.Drawing.Size(210, 21);
            this.fLon.TabIndex = 13;
            this.fLon.SelectedIndexChanged += new System.EventHandler(this.fName_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(665, 442);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(57, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Longitude:";
            // 
            // fDesc
            // 
            this.fDesc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fDesc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fDesc.FormattingEnabled = true;
            this.fDesc.Location = new System.Drawing.Point(230, 458);
            this.fDesc.Name = "fDesc";
            this.fDesc.Size = new System.Drawing.Size(210, 21);
            this.fDesc.TabIndex = 11;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(227, 442);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 13);
            this.label9.TabIndex = 13;
            this.label9.Text = "Description:";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(230, 507);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(429, 3);
            this.panel1.TabIndex = 16;
            // 
            // OK
            // 
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Enabled = false;
            this.OK.Location = new System.Drawing.Point(802, 496);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 17;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(721, 496);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 18;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.s1ToolStripMenuItem,
            this.s2ToolStripMenuItem,
            this.s3ToolStripMenuItem,
            this.s4ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(235, 92);
            // 
            // s1ToolStripMenuItem
            // 
            this.s1ToolStripMenuItem.Name = "s1ToolStripMenuItem";
            this.s1ToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.s1ToolStripMenuItem.Text = "Set Column as Field Name";
            this.s1ToolStripMenuItem.Click += new System.EventHandler(this.s1ToolStripMenuItem_Click);
            // 
            // s2ToolStripMenuItem
            // 
            this.s2ToolStripMenuItem.Name = "s2ToolStripMenuItem";
            this.s2ToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.s2ToolStripMenuItem.Text = "Set Column as Field Description";
            this.s2ToolStripMenuItem.Click += new System.EventHandler(this.s2ToolStripMenuItem_Click);
            // 
            // s3ToolStripMenuItem
            // 
            this.s3ToolStripMenuItem.Name = "s3ToolStripMenuItem";
            this.s3ToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.s3ToolStripMenuItem.Text = "Set Column as Field Latitude";
            this.s3ToolStripMenuItem.Click += new System.EventHandler(this.s3ToolStripMenuItem_Click);
            // 
            // s4ToolStripMenuItem
            // 
            this.s4ToolStripMenuItem.Name = "s4ToolStripMenuItem";
            this.s4ToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            this.s4ToolStripMenuItem.Text = "Set Column as Field Longitude";
            this.s4ToolStripMenuItem.Click += new System.EventHandler(this.s4ToolStripMenuItem_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.SystemColors.Window;
            this.label10.Location = new System.Drawing.Point(681, 404);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(161, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "Table shows only first 20 records";
            // 
            // fStyle
            // 
            this.fStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fStyle.FormattingEnabled = true;
            this.fStyle.Location = new System.Drawing.Point(12, 498);
            this.fStyle.Name = "fStyle";
            this.fStyle.Size = new System.Drawing.Size(210, 21);
            this.fStyle.TabIndex = 14;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 482);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(103, 13);
            this.label11.TabIndex = 21;
            this.label11.Text = "Type / Style / Icon: ";
            // 
            // CSVTXTForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 527);
            this.Controls.Add(this.fStyle);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.fLon);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.fDesc);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.fLat);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.fName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.SD);
            this.Controls.Add(this.separator);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.flh);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.skipsw);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.delimiter);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.codepage);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CSVTXTForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Importing Text File ...";
            this.Load += new System.EventHandler(this.CSVTXTForm_Load);
            this.Shown += new System.EventHandler(this.CSVTXTForm_Shown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ComboBox codepage;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox delimiter;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox skipsw;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.ComboBox flh;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.ComboBox separator;
        public System.Windows.Forms.ListView SD;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.ComboBox fName;
        public System.Windows.Forms.ComboBox fLat;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.ComboBox fLon;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.ComboBox fDesc;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem s1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem s2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem s3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem s4ToolStripMenuItem;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.ComboBox fStyle;
        private System.Windows.Forms.Label label11;
    }
}