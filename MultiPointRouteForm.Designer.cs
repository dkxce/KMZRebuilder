namespace KMZRebuilder
{
    partial class MultiPointRouteForm
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
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pbox = new KMZRebuilder.MCLB();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.checkAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inverseCheckedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteCheckedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteUncheckedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.clearAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.importRoutePointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportRoutePointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(341, 368);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(12, 368);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(342, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select points and order (Use Alt+Up/Ctrl+Down or Alt+Mouse to move):";
            // 
            // pbox
            // 
            this.pbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbox.ContextMenuStrip = this.contextMenuStrip1;
            this.pbox.FormattingEnabled = true;
            this.pbox.Location = new System.Drawing.Point(12, 25);
            this.pbox.Name = "pbox";
            this.pbox.Size = new System.Drawing.Size(404, 317);
            this.pbox.TabIndex = 3;
            this.pbox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbox_MouseUp);
            this.pbox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbox_MouseDown);
            this.pbox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.pbox_KeyUp);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllToolStripMenuItem,
            this.checkNoneToolStripMenuItem,
            this.inverseCheckedToolStripMenuItem,
            this.toolStripMenuItem1,
            this.deleteCheckedToolStripMenuItem,
            this.deleteUncheckedToolStripMenuItem,
            this.toolStripMenuItem2,
            this.clearAllToolStripMenuItem,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.importRoutePointsToolStripMenuItem,
            this.exportRoutePointsToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(197, 226);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // checkAllToolStripMenuItem
            // 
            this.checkAllToolStripMenuItem.Name = "checkAllToolStripMenuItem";
            this.checkAllToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.checkAllToolStripMenuItem.Text = "Check All";
            this.checkAllToolStripMenuItem.Click += new System.EventHandler(this.checkAllToolStripMenuItem_Click);
            // 
            // checkNoneToolStripMenuItem
            // 
            this.checkNoneToolStripMenuItem.Name = "checkNoneToolStripMenuItem";
            this.checkNoneToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.checkNoneToolStripMenuItem.Text = "Check None";
            this.checkNoneToolStripMenuItem.Click += new System.EventHandler(this.checkNoneToolStripMenuItem_Click);
            // 
            // inverseCheckedToolStripMenuItem
            // 
            this.inverseCheckedToolStripMenuItem.Name = "inverseCheckedToolStripMenuItem";
            this.inverseCheckedToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.inverseCheckedToolStripMenuItem.Text = "Inverse Checking";
            this.inverseCheckedToolStripMenuItem.Click += new System.EventHandler(this.inverseCheckedToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(193, 6);
            // 
            // deleteCheckedToolStripMenuItem
            // 
            this.deleteCheckedToolStripMenuItem.Name = "deleteCheckedToolStripMenuItem";
            this.deleteCheckedToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.deleteCheckedToolStripMenuItem.Text = "Delete Checked";
            this.deleteCheckedToolStripMenuItem.Click += new System.EventHandler(this.deleteCheckedToolStripMenuItem_Click);
            // 
            // deleteUncheckedToolStripMenuItem
            // 
            this.deleteUncheckedToolStripMenuItem.Name = "deleteUncheckedToolStripMenuItem";
            this.deleteUncheckedToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.deleteUncheckedToolStripMenuItem.Text = "Delete Unchecked";
            this.deleteUncheckedToolStripMenuItem.Click += new System.EventHandler(this.deleteUncheckedToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(193, 6);
            // 
            // clearAllToolStripMenuItem
            // 
            this.clearAllToolStripMenuItem.Name = "clearAllToolStripMenuItem";
            this.clearAllToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.clearAllToolStripMenuItem.Text = "Delete All";
            this.clearAllToolStripMenuItem.Click += new System.EventHandler(this.clearAllToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(193, 6);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(193, 6);
            // 
            // importRoutePointsToolStripMenuItem
            // 
            this.importRoutePointsToolStripMenuItem.Name = "importRoutePointsToolStripMenuItem";
            this.importRoutePointsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.importRoutePointsToolStripMenuItem.Text = "Import Route Points ...";
            this.importRoutePointsToolStripMenuItem.Click += new System.EventHandler(this.importRoutePointsToolStripMenuItem_Click);
            // 
            // exportRoutePointsToolStripMenuItem
            // 
            this.exportRoutePointsToolStripMenuItem.Name = "exportRoutePointsToolStripMenuItem";
            this.exportRoutePointsToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.exportRoutePointsToolStripMenuItem.Text = "Export Route Points ...";
            this.exportRoutePointsToolStripMenuItem.Click += new System.EventHandler(this.exportRoutePointsToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(267, 346);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(150, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Click on map to add new point";
            // 
            // MultiPointRouteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(428, 396);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pbox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MultiPointRouteForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Multipoint Route";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private MCLB pbox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem checkAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkNoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem inverseCheckedToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem deleteCheckedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteUncheckedToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem clearAllToolStripMenuItem;
        public System.Windows.Forms.Button buttonOk;
        public System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem importRoutePointsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportRoutePointsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
    }
}