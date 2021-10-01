namespace KMZRebuilder
{
    partial class LineAreaStyleForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.LOpacity = new System.Windows.Forms.NumericUpDown();
            this.LBColor = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.LWidth = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.ABColor = new System.Windows.Forms.Button();
            this.AOpacity = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.AFill = new System.Windows.Forms.ComboBox();
            this.LColor = new System.Windows.Forms.MaskedTextBox();
            this.AColor = new System.Windows.Forms.MaskedTextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.ApplyTo = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.LOpacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AOpacity)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(8, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Line Style";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(81, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Color:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(176, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Opacity:";
            // 
            // LOpacity
            // 
            this.LOpacity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LOpacity.Location = new System.Drawing.Point(179, 23);
            this.LOpacity.Name = "LOpacity";
            this.LOpacity.Size = new System.Drawing.Size(45, 20);
            this.LOpacity.TabIndex = 4;
            this.LOpacity.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // LBColor
            // 
            this.LBColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.LBColor.Location = new System.Drawing.Point(147, 23);
            this.LBColor.Name = "LBColor";
            this.LBColor.Size = new System.Drawing.Size(27, 20);
            this.LBColor.TabIndex = 5;
            this.LBColor.Text = "...";
            this.LBColor.UseVisualStyleBackColor = true;
            this.LBColor.Click += new System.EventHandler(this.LBColor_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(227, 7);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Width:";
            // 
            // LWidth
            // 
            this.LWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LWidth.Location = new System.Drawing.Point(230, 23);
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
            this.LWidth.TabIndex = 7;
            this.LWidth.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(8, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Area Style";
            // 
            // ABColor
            // 
            this.ABColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ABColor.Location = new System.Drawing.Point(147, 68);
            this.ABColor.Name = "ABColor";
            this.ABColor.Size = new System.Drawing.Size(27, 20);
            this.ABColor.TabIndex = 13;
            this.ABColor.Text = "...";
            this.ABColor.UseVisualStyleBackColor = true;
            this.ABColor.Click += new System.EventHandler(this.ABColor_Click);
            // 
            // AOpacity
            // 
            this.AOpacity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AOpacity.Location = new System.Drawing.Point(179, 68);
            this.AOpacity.Name = "AOpacity";
            this.AOpacity.Size = new System.Drawing.Size(45, 20);
            this.AOpacity.TabIndex = 12;
            this.AOpacity.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(176, 52);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Opacity:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(81, 52);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Color:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(227, 52);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(22, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Fill:";
            // 
            // AFill
            // 
            this.AFill.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AFill.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AFill.FormattingEnabled = true;
            this.AFill.Items.AddRange(new object[] {
            "No",
            "Yes"});
            this.AFill.Location = new System.Drawing.Point(230, 67);
            this.AFill.Name = "AFill";
            this.AFill.Size = new System.Drawing.Size(45, 21);
            this.AFill.TabIndex = 15;
            // 
            // LColor
            // 
            this.LColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LColor.Location = new System.Drawing.Point(84, 23);
            this.LColor.Mask = "\\#AAAAAA";
            this.LColor.Name = "LColor";
            this.LColor.Size = new System.Drawing.Size(64, 20);
            this.LColor.TabIndex = 16;
            this.LColor.Text = "FFFFFF";
            this.LColor.Validating += new System.ComponentModel.CancelEventHandler(this.LColor_Validating);
            this.LColor.TextChanged += new System.EventHandler(this.LColor_TextChanged);
            // 
            // AColor
            // 
            this.AColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AColor.Location = new System.Drawing.Point(84, 68);
            this.AColor.Mask = "\\#AAAAAA";
            this.AColor.Name = "AColor";
            this.AColor.Size = new System.Drawing.Size(64, 20);
            this.AColor.TabIndex = 17;
            this.AColor.Text = "FFFFFF";
            this.AColor.Validating += new System.ComponentModel.CancelEventHandler(this.LColor_Validating);
            this.AColor.TextChanged += new System.EventHandler(this.AColor_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label9.Location = new System.Drawing.Point(10, 114);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(57, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Apply to:";
            // 
            // ApplyTo
            // 
            this.ApplyTo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ApplyTo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ApplyTo.FormattingEnabled = true;
            this.ApplyTo.Items.AddRange(new object[] {
            "Current Placemark Only",
            "All Placemarks With Same Style"});
            this.ApplyTo.Location = new System.Drawing.Point(84, 111);
            this.ApplyTo.Name = "ApplyTo";
            this.ApplyTo.Size = new System.Drawing.Size(191, 21);
            this.ApplyTo.TabIndex = 19;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(119, 146);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 20;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button2.Location = new System.Drawing.Point(200, 146);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 21;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // LineAreaStyleForm
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(292, 182);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ApplyTo);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.AColor);
            this.Controls.Add(this.LColor);
            this.Controls.Add(this.AFill);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.ABColor);
            this.Controls.Add(this.AOpacity);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.LWidth);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.LBColor);
            this.Controls.Add(this.LOpacity);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LineAreaStyleForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Line/Area Style";
            ((System.ComponentModel.ISupportInitialize)(this.LOpacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AOpacity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.NumericUpDown LOpacity;
        public System.Windows.Forms.Button LBColor;
        public System.Windows.Forms.NumericUpDown LWidth;
        public System.Windows.Forms.Button ABColor;
        public System.Windows.Forms.NumericUpDown AOpacity;
        public System.Windows.Forms.ComboBox AFill;
        public System.Windows.Forms.MaskedTextBox LColor;
        public System.Windows.Forms.MaskedTextBox AColor;
        public System.Windows.Forms.ComboBox ApplyTo;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label label6;
        public System.Windows.Forms.Label label7;
        public System.Windows.Forms.Label label8;
        public System.Windows.Forms.Label label9;
        public System.Windows.Forms.Button button1;
        public System.Windows.Forms.Button button2;
    }
}