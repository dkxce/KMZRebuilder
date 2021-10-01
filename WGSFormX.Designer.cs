namespace KMZRebuilder
{
    partial class WGSFormX
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
            this.LatD = new System.Windows.Forms.TextBox();
            this.LatM = new System.Windows.Forms.TextBox();
            this.LatS = new System.Windows.Forms.TextBox();
            this.LatN = new System.Windows.Forms.TextBox();
            this.LonN = new System.Windows.Forms.TextBox();
            this.LonS = new System.Windows.Forms.TextBox();
            this.LonM = new System.Windows.Forms.TextBox();
            this.LonD = new System.Windows.Forms.TextBox();
            this.BothN = new System.Windows.Forms.TextBox();
            this.BothD = new System.Windows.Forms.TextBox();
            this.BothM = new System.Windows.Forms.TextBox();
            this.BothS = new System.Windows.Forms.TextBox();
            this.Multi = new System.Windows.Forms.TextBox();
            this.dsep = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // LatD
            // 
            this.LatD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(255)))), ((int)(((byte)(210)))));
            this.LatD.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LatD.ForeColor = System.Drawing.SystemColors.WindowText;
            this.LatD.Location = new System.Drawing.Point(136, 22);
            this.LatD.Name = "LatD";
            this.LatD.Size = new System.Drawing.Size(120, 20);
            this.LatD.TabIndex = 3;
            this.LatD.Text = "0";
            this.LatD.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // LatM
            // 
            this.LatM.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(255)))));
            this.LatM.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LatM.Location = new System.Drawing.Point(262, 22);
            this.LatM.Name = "LatM";
            this.LatM.Size = new System.Drawing.Size(120, 20);
            this.LatM.TabIndex = 5;
            this.LatM.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // LatS
            // 
            this.LatS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.LatS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LatS.Location = new System.Drawing.Point(388, 22);
            this.LatS.Name = "LatS";
            this.LatS.Size = new System.Drawing.Size(120, 20);
            this.LatS.TabIndex = 7;
            this.LatS.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // LatN
            // 
            this.LatN.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LatN.Location = new System.Drawing.Point(10, 22);
            this.LatN.Name = "LatN";
            this.LatN.Size = new System.Drawing.Size(120, 20);
            this.LatN.TabIndex = 1;
            this.LatN.Text = "0";
            this.LatN.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // LonN
            // 
            this.LonN.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LonN.Location = new System.Drawing.Point(10, 61);
            this.LonN.Name = "LonN";
            this.LonN.Size = new System.Drawing.Size(120, 20);
            this.LonN.TabIndex = 2;
            this.LonN.Text = "0";
            this.LonN.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // LonS
            // 
            this.LonS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.LonS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LonS.Location = new System.Drawing.Point(388, 61);
            this.LonS.Name = "LonS";
            this.LonS.Size = new System.Drawing.Size(120, 20);
            this.LonS.TabIndex = 8;
            this.LonS.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // LonM
            // 
            this.LonM.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(255)))));
            this.LonM.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LonM.Location = new System.Drawing.Point(262, 61);
            this.LonM.Name = "LonM";
            this.LonM.Size = new System.Drawing.Size(120, 20);
            this.LonM.TabIndex = 6;
            this.LonM.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // LonD
            // 
            this.LonD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(255)))), ((int)(((byte)(210)))));
            this.LonD.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LonD.ForeColor = System.Drawing.SystemColors.WindowText;
            this.LonD.Location = new System.Drawing.Point(136, 61);
            this.LonD.Name = "LonD";
            this.LonD.Size = new System.Drawing.Size(120, 20);
            this.LonD.TabIndex = 4;
            this.LonD.Text = "0";
            this.LonD.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // BothN
            // 
            this.BothN.BackColor = System.Drawing.SystemColors.Window;
            this.BothN.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BothN.Location = new System.Drawing.Point(10, 98);
            this.BothN.Name = "BothN";
            this.BothN.Size = new System.Drawing.Size(246, 20);
            this.BothN.TabIndex = 9;
            this.BothN.Text = "0,0";
            this.BothN.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // BothD
            // 
            this.BothD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(255)))), ((int)(((byte)(210)))));
            this.BothD.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BothD.Location = new System.Drawing.Point(10, 124);
            this.BothD.Name = "BothD";
            this.BothD.Size = new System.Drawing.Size(246, 20);
            this.BothD.TabIndex = 10;
            this.BothD.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // BothM
            // 
            this.BothM.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(255)))));
            this.BothM.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BothM.Location = new System.Drawing.Point(10, 150);
            this.BothM.Name = "BothM";
            this.BothM.Size = new System.Drawing.Size(246, 20);
            this.BothM.TabIndex = 11;
            this.BothM.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // BothS
            // 
            this.BothS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.BothS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BothS.Location = new System.Drawing.Point(10, 176);
            this.BothS.Name = "BothS";
            this.BothS.Size = new System.Drawing.Size(246, 20);
            this.BothS.TabIndex = 12;
            this.BothS.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // Multi
            // 
            this.Multi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Multi.Location = new System.Drawing.Point(262, 98);
            this.Multi.Multiline = true;
            this.Multi.Name = "Multi";
            this.Multi.Size = new System.Drawing.Size(246, 72);
            this.Multi.TabIndex = 13;
            this.Multi.Validating += new System.ComponentModel.CancelEventHandler(this.LatN_Validating);
            // 
            // dsep
            // 
            this.dsep.BackColor = System.Drawing.SystemColors.Control;
            this.dsep.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dsep.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.dsep.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.dsep.FormattingEnabled = true;
            this.dsep.Items.AddRange(new object[] {
            ".",
            ","});
            this.dsep.Location = new System.Drawing.Point(461, 177);
            this.dsep.Name = "dsep";
            this.dsep.Size = new System.Drawing.Size(48, 21);
            this.dsep.TabIndex = 25;
            this.dsep.TabStop = false;
            this.dsep.SelectedIndexChanged += new System.EventHandler(this.dsep_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.SystemColors.Control;
            this.label11.Location = new System.Drawing.Point(262, 180);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(172, 13);
            this.label11.TabIndex = 26;
            this.label11.Text = "Change Point / Decimal Separator:";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Navy;
            this.panel2.Controls.Add(this.label2);
            this.panel2.Location = new System.Drawing.Point(10, 13);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(222, 2);
            this.panel2.TabIndex = 29;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(225, -9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 30;
            this.label2.Text = "Latitude";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(237, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 30;
            this.label3.Text = "Latitude";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Navy;
            this.panel3.Controls.Add(this.label4);
            this.panel3.Location = new System.Drawing.Point(286, 13);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(222, 2);
            this.panel3.TabIndex = 31;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(225, -9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "Latitude";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Navy;
            this.panel4.Controls.Add(this.label5);
            this.panel4.Location = new System.Drawing.Point(293, 50);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(214, 2);
            this.panel4.TabIndex = 34;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(225, -9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 30;
            this.label5.Text = "Latitude";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(233, 44);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 13);
            this.label6.TabIndex = 33;
            this.label6.Text = "Longitude";
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.Navy;
            this.panel5.Controls.Add(this.label7);
            this.panel5.Location = new System.Drawing.Point(10, 50);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(217, 2);
            this.panel5.TabIndex = 32;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(225, -9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 13);
            this.label7.TabIndex = 30;
            this.label7.Text = "Latitude";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Navy;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(319, 88);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(188, 2);
            this.panel1.TabIndex = 37;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(225, -9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 30;
            this.label1.Text = "Latitude";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(213, 82);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(104, 13);
            this.label8.TabIndex = 36;
            this.label8.Text = "Latitude && Longitude";
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.Navy;
            this.panel6.Controls.Add(this.label9);
            this.panel6.Location = new System.Drawing.Point(10, 88);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(201, 2);
            this.panel6.TabIndex = 35;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(225, -9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(45, 13);
            this.label9.TabIndex = 30;
            this.label9.Text = "Latitude";
            // 
            // WGSFormX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 210);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.BothS);
            this.Controls.Add(this.dsep);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.Multi);
            this.Controls.Add(this.BothM);
            this.Controls.Add(this.BothD);
            this.Controls.Add(this.BothN);
            this.Controls.Add(this.LonN);
            this.Controls.Add(this.LonS);
            this.Controls.Add(this.LonM);
            this.Controls.Add(this.LonD);
            this.Controls.Add(this.LatN);
            this.Controls.Add(this.LatS);
            this.Controls.Add(this.LatM);
            this.Controls.Add(this.LatD);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WGSFormX";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Lat & Lon Converter";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WGSFormX_FormClosed);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox LatD;
        private System.Windows.Forms.TextBox LatM;
        private System.Windows.Forms.TextBox LatS;
        private System.Windows.Forms.TextBox LatN;
        private System.Windows.Forms.TextBox LonN;
        private System.Windows.Forms.TextBox LonS;
        private System.Windows.Forms.TextBox LonM;
        private System.Windows.Forms.TextBox LonD;
        private System.Windows.Forms.TextBox BothN;
        private System.Windows.Forms.TextBox BothD;
        private System.Windows.Forms.TextBox BothM;
        private System.Windows.Forms.TextBox BothS;
        private System.Windows.Forms.TextBox Multi;
        private System.Windows.Forms.ComboBox dsep;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label9;
    }
}