namespace KMZRebuilder
{
    partial class KMNumeratorForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.incVal = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.startVal = new System.Windows.Forms.NumericUpDown();
            this.button2 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.statusText = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.skipVal = new System.Windows.Forms.NumericUpDown();
            this.textBox1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.incVal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startVal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.skipVal)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(550, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 20);
            this.button1.TabIndex = 2;
            this.button1.Text = "Browse...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "XPath:";
            // 
            // comboBox1
            // 
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(57, 25);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(487, 21);
            this.comboBox1.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(212, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Each flag step (κμ):";
            // 
            // incVal
            // 
            this.incVal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.incVal.Location = new System.Drawing.Point(319, 51);
            this.incVal.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.incVal.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.incVal.Name = "incVal";
            this.incVal.Size = new System.Drawing.Size(50, 20);
            this.incVal.TabIndex = 6;
            this.incVal.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(375, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(104, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "First flag km number:";
            // 
            // startVal
            // 
            this.startVal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.startVal.Location = new System.Drawing.Point(485, 51);
            this.startVal.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.startVal.Name = "startVal";
            this.startVal.Size = new System.Drawing.Size(59, 20);
            this.startVal.TabIndex = 8;
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(550, 27);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 44);
            this.button2.TabIndex = 9;
            this.button2.Text = "RUN";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.ForeColor = System.Drawing.Color.Maroon;
            this.progressBar1.Location = new System.Drawing.Point(0, 96);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(642, 20);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 10;
            // 
            // statusText
            // 
            this.statusText.AutoSize = true;
            this.statusText.BackColor = System.Drawing.Color.Transparent;
            this.statusText.Location = new System.Drawing.Point(11, 78);
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(85, 13);
            this.statusText.TabIndex = 11;
            this.statusText.Text = "Status: waiting...";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Skip X km from start:";
            // 
            // skipVal
            // 
            this.skipVal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.skipVal.Location = new System.Drawing.Point(122, 51);
            this.skipVal.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.skipVal.Name = "skipVal";
            this.skipVal.Size = new System.Drawing.Size(84, 20);
            this.skipVal.TabIndex = 13;
            // 
            // textBox1
            // 
            this.textBox1.AutoSize = true;
            this.textBox1.ForeColor = System.Drawing.Color.Maroon;
            this.textBox1.Location = new System.Drawing.Point(57, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(43, 13);
            this.textBox1.TabIndex = 14;
            this.textBox1.Text = "Choose";
            // 
            // KMNumerrer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(642, 116);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.skipVal);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.statusText);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.startVal);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.incVal);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "KMNumerrer";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Km Flag Numerator (GPX/KML -> KML Points)";
            ((System.ComponentModel.ISupportInitialize)(this.incVal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startVal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.skipVal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown incVal;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown startVal;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label statusText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown skipVal;
        private System.Windows.Forms.Label textBox1;
    }
}

