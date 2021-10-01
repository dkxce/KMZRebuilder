namespace KMZRebuilder
{
    partial class FindReplaceDlg
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
            this.FindText = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ReplaceText = new System.Windows.Forms.TextBox();
            this.FindButton = new System.Windows.Forms.Button();
            this.ReplaceButton = new System.Windows.Forms.Button();
            this.ReplaceALL = new System.Windows.Forms.Button();
            this.IgnoreCase = new System.Windows.Forms.CheckBox();
            this.UP = new System.Windows.Forms.RadioButton();
            this.DOWN = new System.Windows.Forms.RadioButton();
            this.OnlyChecked = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.REPF = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Find:";
            // 
            // FindText
            // 
            this.FindText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FindText.Location = new System.Drawing.Point(66, 31);
            this.FindText.Name = "FindText";
            this.FindText.Size = new System.Drawing.Size(179, 20);
            this.FindText.TabIndex = 1;
            this.FindText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FindText_KeyDown);
            this.FindText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindText_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Replace:";
            // 
            // ReplaceText
            // 
            this.ReplaceText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ReplaceText.Location = new System.Drawing.Point(66, 56);
            this.ReplaceText.Name = "ReplaceText";
            this.ReplaceText.Size = new System.Drawing.Size(179, 20);
            this.ReplaceText.TabIndex = 2;
            this.ReplaceText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ReplaceText_KeyPress);
            // 
            // FindButton
            // 
            this.FindButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FindButton.Location = new System.Drawing.Point(251, 30);
            this.FindButton.Name = "FindButton";
            this.FindButton.Size = new System.Drawing.Size(75, 23);
            this.FindButton.TabIndex = 3;
            this.FindButton.Text = "Find Next";
            this.FindButton.UseVisualStyleBackColor = true;
            this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
            // 
            // ReplaceButton
            // 
            this.ReplaceButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReplaceButton.Location = new System.Drawing.Point(251, 55);
            this.ReplaceButton.Name = "ReplaceButton";
            this.ReplaceButton.Size = new System.Drawing.Size(75, 23);
            this.ReplaceButton.TabIndex = 4;
            this.ReplaceButton.Text = "Replace";
            this.ReplaceButton.UseVisualStyleBackColor = true;
            this.ReplaceButton.Click += new System.EventHandler(this.ReplaceButton_Click);
            // 
            // ReplaceALL
            // 
            this.ReplaceALL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReplaceALL.Location = new System.Drawing.Point(327, 55);
            this.ReplaceALL.Name = "ReplaceALL";
            this.ReplaceALL.Size = new System.Drawing.Size(94, 23);
            this.ReplaceALL.TabIndex = 6;
            this.ReplaceALL.Text = "Replace All";
            this.ReplaceALL.UseVisualStyleBackColor = true;
            this.ReplaceALL.Click += new System.EventHandler(this.ReplaceALL_Click);
            // 
            // IgnoreCase
            // 
            this.IgnoreCase.AutoSize = true;
            this.IgnoreCase.Checked = true;
            this.IgnoreCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.IgnoreCase.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.IgnoreCase.Location = new System.Drawing.Point(246, 6);
            this.IgnoreCase.Name = "IgnoreCase";
            this.IgnoreCase.Size = new System.Drawing.Size(80, 17);
            this.IgnoreCase.TabIndex = 9;
            this.IgnoreCase.Text = "Ignore Case";
            this.IgnoreCase.UseVisualStyleBackColor = true;
            // 
            // UP
            // 
            this.UP.AutoSize = true;
            this.UP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UP.Location = new System.Drawing.Point(66, 8);
            this.UP.Name = "UP";
            this.UP.Size = new System.Drawing.Size(38, 17);
            this.UP.TabIndex = 7;
            this.UP.Text = "Up";
            this.UP.UseVisualStyleBackColor = true;
            // 
            // DOWN
            // 
            this.DOWN.AutoSize = true;
            this.DOWN.Checked = true;
            this.DOWN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DOWN.Location = new System.Drawing.Point(110, 8);
            this.DOWN.Name = "DOWN";
            this.DOWN.Size = new System.Drawing.Size(52, 17);
            this.DOWN.TabIndex = 8;
            this.DOWN.TabStop = true;
            this.DOWN.Text = "Down";
            this.DOWN.UseVisualStyleBackColor = true;
            // 
            // OnlyChecked
            // 
            this.OnlyChecked.AutoSize = true;
            this.OnlyChecked.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OnlyChecked.Location = new System.Drawing.Point(330, 6);
            this.OnlyChecked.Name = "OnlyChecked";
            this.OnlyChecked.Size = new System.Drawing.Size(90, 17);
            this.OnlyChecked.TabIndex = 10;
            this.OnlyChecked.Text = "Only Checked";
            this.OnlyChecked.UseVisualStyleBackColor = true;
            this.OnlyChecked.CheckedChanged += new System.EventHandler(this.OnlyChecked_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Direction:";
            // 
            // REPF
            // 
            this.REPF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.REPF.Location = new System.Drawing.Point(327, 30);
            this.REPF.Name = "REPF";
            this.REPF.Size = new System.Drawing.Size(94, 23);
            this.REPF.TabIndex = 5;
            this.REPF.Text = "Replace && Find";
            this.REPF.UseVisualStyleBackColor = true;
            this.REPF.Click += new System.EventHandler(this.button1_Click);
            // 
            // FindReplaceDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 89);
            this.Controls.Add(this.REPF);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.OnlyChecked);
            this.Controls.Add(this.DOWN);
            this.Controls.Add(this.UP);
            this.Controls.Add(this.IgnoreCase);
            this.Controls.Add(this.ReplaceALL);
            this.Controls.Add(this.ReplaceButton);
            this.Controls.Add(this.FindButton);
            this.Controls.Add(this.ReplaceText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FindText);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindReplaceDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find & Replace";
            this.Activated += new System.EventHandler(this.FindReplaceDlg_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FindReplaceDlg_FormClosed);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindReplaceDlg_KeyPress);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.CheckBox IgnoreCase;
        public System.Windows.Forms.RadioButton UP;
        public System.Windows.Forms.RadioButton DOWN;
        public System.Windows.Forms.CheckBox OnlyChecked;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Button FindButton;
        public System.Windows.Forms.Button ReplaceButton;
        public System.Windows.Forms.Button ReplaceALL;
        public System.Windows.Forms.Button REPF;
        public System.Windows.Forms.TextBox FindText;
        public System.Windows.Forms.TextBox ReplaceText;
    }
}