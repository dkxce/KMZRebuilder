namespace KMZRebuilder
{
    partial class AddTextAsPoly
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
            this.label1 = new System.Windows.Forms.Label();
            this.TextOut = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.fontSystem = new System.Windows.Forms.RadioButton();
            this.fontSysList = new System.Windows.Forms.ComboBox();
            this.fontCustom = new System.Windows.Forms.RadioButton();
            this.selFont = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.TextSize = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.TextAzimuth = new System.Windows.Forms.NumericUpDown();
            this.TextAlign = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.fontCustomList = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.AColor = new System.Windows.Forms.MaskedTextBox();
            this.LColor = new System.Windows.Forms.MaskedTextBox();
            this.AFill = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ABColor = new System.Windows.Forms.Button();
            this.AOpacity = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.LWidth = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.LBColor = new System.Windows.Forms.Button();
            this.LOpacity = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.pb = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.TextSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TextAzimuth)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AOpacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LOpacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Text:";
            // 
            // TextOut
            // 
            this.TextOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextOut.Location = new System.Drawing.Point(13, 28);
            this.TextOut.Name = "TextOut";
            this.TextOut.Size = new System.Drawing.Size(280, 20);
            this.TextOut.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Font:";
            // 
            // fontSystem
            // 
            this.fontSystem.AutoSize = true;
            this.fontSystem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fontSystem.Location = new System.Drawing.Point(15, 70);
            this.fontSystem.Name = "fontSystem";
            this.fontSystem.Size = new System.Drawing.Size(61, 17);
            this.fontSystem.TabIndex = 3;
            this.fontSystem.Text = "System:";
            this.fontSystem.UseVisualStyleBackColor = true;
            // 
            // fontSysList
            // 
            this.fontSysList.DropDownHeight = 150;
            this.fontSysList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fontSysList.DropDownWidth = 217;
            this.fontSysList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fontSysList.FormattingEnabled = true;
            this.fontSysList.IntegralHeight = false;
            this.fontSysList.Location = new System.Drawing.Point(76, 69);
            this.fontSysList.Name = "fontSysList";
            this.fontSysList.Size = new System.Drawing.Size(217, 21);
            this.fontSysList.TabIndex = 4;
            this.fontSysList.SelectedIndexChanged += new System.EventHandler(this.fontSysList_SelectedIndexChanged);
            // 
            // fontCustom
            // 
            this.fontCustom.AutoSize = true;
            this.fontCustom.Checked = true;
            this.fontCustom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fontCustom.Location = new System.Drawing.Point(15, 96);
            this.fontCustom.Name = "fontCustom";
            this.fontCustom.Size = new System.Drawing.Size(62, 17);
            this.fontCustom.TabIndex = 5;
            this.fontCustom.TabStop = true;
            this.fontCustom.Text = "Custom:";
            this.fontCustom.UseVisualStyleBackColor = true;
            // 
            // selFont
            // 
            this.selFont.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.selFont.Location = new System.Drawing.Point(262, 94);
            this.selFont.Name = "selFont";
            this.selFont.Size = new System.Drawing.Size(31, 21);
            this.selFont.TabIndex = 7;
            this.selFont.Text = "...";
            this.selFont.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.selFont.UseVisualStyleBackColor = true;
            this.selFont.Click += new System.EventHandler(this.selFont_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 123);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Size (height in meters):";
            // 
            // TextSize
            // 
            this.TextSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextSize.Location = new System.Drawing.Point(13, 139);
            this.TextSize.Maximum = new decimal(new int[] {
            40000,
            0,
            0,
            0});
            this.TextSize.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.TextSize.Name = "TextSize";
            this.TextSize.Size = new System.Drawing.Size(112, 20);
            this.TextSize.TabIndex = 9;
            this.TextSize.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(131, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Azimuth:";
            // 
            // TextAzimuth
            // 
            this.TextAzimuth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextAzimuth.Location = new System.Drawing.Point(134, 139);
            this.TextAzimuth.Maximum = new decimal(new int[] {
            359,
            0,
            0,
            0});
            this.TextAzimuth.Name = "TextAzimuth";
            this.TextAzimuth.Size = new System.Drawing.Size(44, 20);
            this.TextAzimuth.TabIndex = 12;
            this.TextAzimuth.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
            // 
            // TextAlign
            // 
            this.TextAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TextAlign.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TextAlign.FormattingEnabled = true;
            this.TextAlign.Items.AddRange(new object[] {
            "Center",
            "Top Middle",
            "Top Right",
            "Right Middle",
            "Bottom Right",
            "Bottom Middle",
            "Bottom Left",
            "Left Middle",
            "Top Left"});
            this.TextAlign.Location = new System.Drawing.Point(207, 138);
            this.TextAlign.Name = "TextAlign";
            this.TextAlign.Size = new System.Drawing.Size(86, 21);
            this.TextAlign.TabIndex = 13;
            this.TextAlign.SelectedIndexChanged += new System.EventHandler(this.TextAlign_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(181, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Text align to point:";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(137, 287);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 15;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(218, 287);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 16;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // fontCustomList
            // 
            this.fontCustomList.DropDownHeight = 150;
            this.fontCustomList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fontCustomList.DropDownWidth = 217;
            this.fontCustomList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fontCustomList.FormattingEnabled = true;
            this.fontCustomList.IntegralHeight = false;
            this.fontCustomList.Location = new System.Drawing.Point(76, 94);
            this.fontCustomList.Name = "fontCustomList";
            this.fontCustomList.Size = new System.Drawing.Size(180, 21);
            this.fontCustomList.TabIndex = 18;
            this.fontCustomList.SelectedIndexChanged += new System.EventHandler(this.fontCustomList_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.AColor);
            this.panel2.Controls.Add(this.LColor);
            this.panel2.Controls.Add(this.AFill);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.ABColor);
            this.panel2.Controls.Add(this.AOpacity);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.LWidth);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.LBColor);
            this.panel2.Controls.Add(this.LOpacity);
            this.panel2.Controls.Add(this.label12);
            this.panel2.Controls.Add(this.label13);
            this.panel2.Controls.Add(this.label14);
            this.panel2.Location = new System.Drawing.Point(7, 172);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(286, 105);
            this.panel2.TabIndex = 19;
            // 
            // AColor
            // 
            this.AColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AColor.Location = new System.Drawing.Point(79, 66);
            this.AColor.Mask = "\\#AAAAAA";
            this.AColor.Name = "AColor";
            this.AColor.Size = new System.Drawing.Size(64, 20);
            this.AColor.TabIndex = 35;
            this.AColor.Text = "000000";
            this.AColor.Validating += new System.ComponentModel.CancelEventHandler(this.LColor_Validating);
            this.AColor.TextChanged += new System.EventHandler(this.AColor_TextChanged);
            // 
            // LColor
            // 
            this.LColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LColor.Location = new System.Drawing.Point(79, 21);
            this.LColor.Mask = "\\#AAAAAA";
            this.LColor.Name = "LColor";
            this.LColor.Size = new System.Drawing.Size(64, 20);
            this.LColor.TabIndex = 34;
            this.LColor.Text = "000000";
            this.LColor.Validating += new System.ComponentModel.CancelEventHandler(this.LColor_Validating);
            this.LColor.TextChanged += new System.EventHandler(this.LColor_TextChanged);
            // 
            // AFill
            // 
            this.AFill.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AFill.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AFill.FormattingEnabled = true;
            this.AFill.Items.AddRange(new object[] {
            "No",
            "Yes"});
            this.AFill.Location = new System.Drawing.Point(225, 65);
            this.AFill.Name = "AFill";
            this.AFill.Size = new System.Drawing.Size(45, 21);
            this.AFill.TabIndex = 33;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(222, 50);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(22, 13);
            this.label8.TabIndex = 32;
            this.label8.Text = "Fill:";
            // 
            // ABColor
            // 
            this.ABColor.BackColor = System.Drawing.Color.Black;
            this.ABColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ABColor.Location = new System.Drawing.Point(142, 66);
            this.ABColor.Name = "ABColor";
            this.ABColor.Size = new System.Drawing.Size(27, 20);
            this.ABColor.TabIndex = 31;
            this.ABColor.Text = "...";
            this.ABColor.UseVisualStyleBackColor = false;
            this.ABColor.Click += new System.EventHandler(this.ABColor_Click);
            // 
            // AOpacity
            // 
            this.AOpacity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AOpacity.Location = new System.Drawing.Point(174, 66);
            this.AOpacity.Name = "AOpacity";
            this.AOpacity.Size = new System.Drawing.Size(45, 20);
            this.AOpacity.TabIndex = 30;
            this.AOpacity.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(171, 50);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 13);
            this.label6.TabIndex = 29;
            this.label6.Text = "Opacity:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(76, 50);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 13);
            this.label7.TabIndex = 28;
            this.label7.Text = "Color:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label10.Location = new System.Drawing.Point(3, 69);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 13);
            this.label10.TabIndex = 27;
            this.label10.Text = "Area Style";
            // 
            // LWidth
            // 
            this.LWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LWidth.Location = new System.Drawing.Point(225, 21);
            this.LWidth.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.LWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.LWidth.Name = "LWidth";
            this.LWidth.Size = new System.Drawing.Size(45, 20);
            this.LWidth.TabIndex = 26;
            this.LWidth.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(222, 5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(38, 13);
            this.label11.TabIndex = 25;
            this.label11.Text = "Width:";
            // 
            // LBColor
            // 
            this.LBColor.BackColor = System.Drawing.Color.Black;
            this.LBColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LBColor.Location = new System.Drawing.Point(142, 21);
            this.LBColor.Name = "LBColor";
            this.LBColor.Size = new System.Drawing.Size(27, 20);
            this.LBColor.TabIndex = 24;
            this.LBColor.Text = "...";
            this.LBColor.UseVisualStyleBackColor = false;
            this.LBColor.Click += new System.EventHandler(this.LBColor_Click);
            // 
            // LOpacity
            // 
            this.LOpacity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LOpacity.Location = new System.Drawing.Point(174, 21);
            this.LOpacity.Name = "LOpacity";
            this.LOpacity.Size = new System.Drawing.Size(45, 20);
            this.LOpacity.TabIndex = 23;
            this.LOpacity.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(171, 5);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(46, 13);
            this.label12.TabIndex = 22;
            this.label12.Text = "Opacity:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(76, 5);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(34, 13);
            this.label13.TabIndex = 21;
            this.label13.Text = "Color:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label14.Location = new System.Drawing.Point(3, 24);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 13);
            this.label14.TabIndex = 20;
            this.label14.Text = "Line Style";
            // 
            // pb
            // 
            this.pb.Location = new System.Drawing.Point(185, 138);
            this.pb.Name = "pb";
            this.pb.Size = new System.Drawing.Size(21, 21);
            this.pb.TabIndex = 20;
            this.pb.TabStop = false;
            // 
            // AddTextAsPoly
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(309, 320);
            this.Controls.Add(this.pb);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.fontCustomList);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.TextAlign);
            this.Controls.Add(this.TextAzimuth);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.TextSize);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.selFont);
            this.Controls.Add(this.fontCustom);
            this.Controls.Add(this.fontSysList);
            this.Controls.Add(this.fontSystem);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TextOut);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddTextAsPoly";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Text To Map as Shape";
            ((System.ComponentModel.ISupportInitialize)(this.TextSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TextAzimuth)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AOpacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LOpacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox TextOut;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.RadioButton fontSystem;
        public System.Windows.Forms.ComboBox fontSysList;
        public System.Windows.Forms.RadioButton fontCustom;
        private System.Windows.Forms.Button selFont;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.NumericUpDown TextSize;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.NumericUpDown TextAzimuth;
        public System.Windows.Forms.ComboBox TextAlign;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.ComboBox fontCustomList;
        public System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.MaskedTextBox AColor;
        public System.Windows.Forms.MaskedTextBox LColor;
        public System.Windows.Forms.ComboBox AFill;
        public System.Windows.Forms.Label label8;
        public System.Windows.Forms.Button ABColor;
        public System.Windows.Forms.NumericUpDown AOpacity;
        public System.Windows.Forms.Label label6;
        public System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label label10;
        public System.Windows.Forms.NumericUpDown LWidth;
        public System.Windows.Forms.Label label11;
        public System.Windows.Forms.Button LBColor;
        public System.Windows.Forms.NumericUpDown LOpacity;
        public System.Windows.Forms.Label label12;
        public System.Windows.Forms.Label label13;
        public System.Windows.Forms.Label label14;
        private System.Windows.Forms.PictureBox pb;
    }
}