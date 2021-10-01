namespace KMZRebuilder
{
    partial class MapIcons
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapIcons));
            this.iconView = new System.Windows.Forms.ListView();
            this.resetTimer = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.loadIconsFromDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadIconsFromZIPFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.loadIconFromImageFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.SIT = new System.Windows.Forms.Label();
            this.TXTC = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // iconView
            // 
            this.iconView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iconView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iconView.FullRowSelect = true;
            this.iconView.GridLines = true;
            this.iconView.HideSelection = false;
            this.iconView.Location = new System.Drawing.Point(0, 22);
            this.iconView.MultiSelect = false;
            this.iconView.Name = "iconView";
            this.iconView.OwnerDraw = true;
            this.iconView.Size = new System.Drawing.Size(756, 492);
            this.iconView.TabIndex = 0;
            this.iconView.UseCompatibleStateImageBehavior = false;
            this.iconView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.iconView_DrawItem);
            this.iconView.SelectedIndexChanged += new System.EventHandler(this.iconView_SelectedIndexChanged);
            this.iconView.DoubleClick += new System.EventHandler(this.iconView_DoubleClick);
            this.iconView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.iconView_KeyPress);
            // 
            // resetTimer
            // 
            this.resetTimer.Interval = 1000;
            this.resetTimer.Tick += new System.EventHandler(this.resetTimer_Tick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(756, 22);
            this.panel1.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(50, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(650, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.WordWrap = false;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // button1
            // 
            this.button1.ContextMenuStrip = this.contextMenuStrip1;
            this.button1.Dock = System.Windows.Forms.DockStyle.Right;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(700, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 22);
            this.button1.TabIndex = 2;
            this.button1.Text = "MENU";
            this.button1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.loadIconsFromDefaultToolStripMenuItem,
            this.loadIconsFromZIPFileToolStripMenuItem,
            this.toolStripMenuItem2,
            this.loadIconFromImageFileToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(256, 82);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(252, 6);
            // 
            // loadIconsFromDefaultToolStripMenuItem
            // 
            this.loadIconsFromDefaultToolStripMenuItem.Name = "loadIconsFromDefaultToolStripMenuItem";
            this.loadIconsFromDefaultToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.loadIconsFromDefaultToolStripMenuItem.Text = "Load Default Icons";
            this.loadIconsFromDefaultToolStripMenuItem.Click += new System.EventHandler(this.loadIconsFromDefaultToolStripMenuItem_Click);
            // 
            // loadIconsFromZIPFileToolStripMenuItem
            // 
            this.loadIconsFromZIPFileToolStripMenuItem.Name = "loadIconsFromZIPFileToolStripMenuItem";
            this.loadIconsFromZIPFileToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.loadIconsFromZIPFileToolStripMenuItem.Text = "Load Icons From ZIP File ...";
            this.loadIconsFromZIPFileToolStripMenuItem.Click += new System.EventHandler(this.loadIconsFromZIPFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(252, 6);
            // 
            // loadIconFromImageFileToolStripMenuItem
            // 
            this.loadIconFromImageFileToolStripMenuItem.Name = "loadIconFromImageFileToolStripMenuItem";
            this.loadIconFromImageFileToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.loadIconFromImageFileToolStripMenuItem.Text = "Select Map Icon From Image File ...";
            this.loadIconFromImageFileToolStripMenuItem.Click += new System.EventHandler(this.loadIconFromImageFileToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 22);
            this.label1.TabIndex = 1;
            this.label1.Text = "Locate: ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.SIT);
            this.panel2.Controls.Add(this.TXTC);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 514);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(756, 18);
            this.panel2.TabIndex = 2;
            // 
            // SIT
            // 
            this.SIT.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SIT.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SIT.Location = new System.Drawing.Point(85, 0);
            this.SIT.Name = "SIT";
            this.SIT.Size = new System.Drawing.Size(586, 18);
            this.SIT.TabIndex = 3;
            this.SIT.Text = "[NONE]";
            this.SIT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TXTC
            // 
            this.TXTC.Dock = System.Windows.Forms.DockStyle.Right;
            this.TXTC.Location = new System.Drawing.Point(671, 0);
            this.TXTC.Name = "TXTC";
            this.TXTC.Size = new System.Drawing.Size(85, 18);
            this.TXTC.TabIndex = 4;
            this.TXTC.Text = "Total Icons: 0";
            this.TXTC.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "Selected Image: ";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MapIcons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(756, 532);
            this.Controls.Add(this.iconView);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MapIcons";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Map Icon";
            this.Shown += new System.EventHandler(this.MapIcons_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView iconView;
        private System.Windows.Forms.Timer resetTimer;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label SIT;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label TXTC;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem loadIconsFromDefaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem loadIconFromImageFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadIconsFromZIPFileToolStripMenuItem;
    }
}