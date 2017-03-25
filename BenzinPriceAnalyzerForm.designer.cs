namespace BenzinPriceAnalizer
{
    partial class BenzinPriceAnalizerForm
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
            this.bgFiles = new System.Windows.Forms.ListBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.‰Ó·‡‚ËÚ¸ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.delone = new System.Windows.Forms.ToolStripMenuItem();
            this.delall = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.marksFilter = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.routeFileBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.status = new System.Windows.Forms.TextBox();
            this.inrad = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.byImgs = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inrad)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Drop files here:";
            // 
            // bgFiles
            // 
            this.bgFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bgFiles.ContextMenuStrip = this.contextMenuStrip1;
            this.bgFiles.FormattingEnabled = true;
            this.bgFiles.Location = new System.Drawing.Point(15, 25);
            this.bgFiles.Name = "bgFiles";
            this.bgFiles.Size = new System.Drawing.Size(453, 132);
            this.bgFiles.TabIndex = 1;
            this.bgFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.bpFiles_DragDrop);
            this.bgFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.bpFiles_DragEnter);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.‰Ó·‡‚ËÚ¸ToolStripMenuItem,
            this.delone,
            this.delall});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(150, 70);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // ‰Ó·‡‚ËÚ¸ToolStripMenuItem
            // 
            this.‰Ó·‡‚ËÚ¸ToolStripMenuItem.Name = "‰Ó·‡‚ËÚ¸ToolStripMenuItem";
            this.‰Ó·‡‚ËÚ¸ToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.‰Ó·‡‚ËÚ¸ToolStripMenuItem.Text = "ƒÓ·‡‚ËÚ¸...";
            this.‰Ó·‡‚ËÚ¸ToolStripMenuItem.Click += new System.EventHandler(this.addmnu_click);
            // 
            // delone
            // 
            this.delone.Name = "delone";
            this.delone.Size = new System.Drawing.Size(149, 22);
            this.delone.Text = "”‰‡ÎËÚ¸";
            this.delone.Click += new System.EventHandler(this.delmnu_click);
            // 
            // delall
            // 
            this.delall.Name = "delall";
            this.delall.Size = new System.Drawing.Size(149, 22);
            this.delall.Text = "”‰‡ÎËÚ¸ ‚ÒÂ";
            this.delall.Click += new System.EventHandler(this.delall_click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 160);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Selection:";
            // 
            // marksFilter
            // 
            this.marksFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.marksFilter.Enabled = false;
            this.marksFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.marksFilter.FormattingEnabled = true;
            this.marksFilter.Items.AddRange(new object[] {
            "All Points",
            "[LR] All Placemarks in Route Buffer (Left & Right)",
            "[R] Placemarks by Right side only",
            "[L] Placemarks by Left side only"});
            this.marksFilter.Location = new System.Drawing.Point(12, 176);
            this.marksFilter.Name = "marksFilter";
            this.marksFilter.Size = new System.Drawing.Size(350, 21);
            this.marksFilter.TabIndex = 3;
            this.marksFilter.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 200);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Route (Path):";
            // 
            // routeFileBox
            // 
            this.routeFileBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.routeFileBox.Location = new System.Drawing.Point(12, 216);
            this.routeFileBox.Name = "routeFileBox";
            this.routeFileBox.Size = new System.Drawing.Size(370, 20);
            this.routeFileBox.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(388, 216);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 20);
            this.button1.TabIndex = 6;
            this.button1.Text = "Select...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 242);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(224, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Filter Points by Selection and Path";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // status
            // 
            this.status.Location = new System.Drawing.Point(12, 288);
            this.status.Multiline = true;
            this.status.Name = "status";
            this.status.ReadOnly = true;
            this.status.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.status.Size = new System.Drawing.Size(456, 262);
            this.status.TabIndex = 8;
            this.status.WordWrap = false;
            // 
            // inrad
            // 
            this.inrad.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.inrad.Enabled = false;
            this.inrad.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.inrad.Location = new System.Drawing.Point(368, 176);
            this.inrad.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.inrad.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.inrad.Name = "inrad";
            this.inrad.Size = new System.Drawing.Size(79, 20);
            this.inrad.TabIndex = 9;
            this.inrad.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(453, 184);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "m";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(242, 242);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(226, 23);
            this.button3.TabIndex = 11;
            this.button3.Text = "Save Filtered Points...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // byImgs
            // 
            this.byImgs.AutoSize = true;
            this.byImgs.Checked = true;
            this.byImgs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.byImgs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.byImgs.Location = new System.Drawing.Point(242, 265);
            this.byImgs.Name = "byImgs";
            this.byImgs.Size = new System.Drawing.Size(193, 17);
            this.byImgs.TabIndex = 12;
            this.byImgs.Text = "Create multi layer KMZ file (by icons)";
            this.byImgs.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 559);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(453, 26);
            this.label5.TabIndex = 13;
            this.label5.Text = "You must grab data from benzine-price.ru map (http://www.benzin-price.ru/map_xml_" +
                "data.php)\r\nand save it to text file, then drop saved file on form. (P.S: You mus" +
                "t be registered user on site!)\r\n";
            // 
            // BenzinPriceAnalizerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 596);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.byImgs);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.inrad);
            this.Controls.Add(this.status);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.routeFileBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.marksFilter);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bgFiles);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "BenzinPriceAnalizerForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Benzin-Price.ru Analyzer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.inrad)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox bgFiles;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ‰Ó·‡‚ËÚ¸ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem delone;
        private System.Windows.Forms.ToolStripMenuItem delall;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox marksFilter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox routeFileBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox status;
        private System.Windows.Forms.NumericUpDown inrad;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.CheckBox byImgs;
        private System.Windows.Forms.Label label5;
    }
}

