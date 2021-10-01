//////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////
//
// Milok Zbrozek InputBox Class
// milokz@gmail.com
// Last Modified: 30.09.2021
//    +QueryPass
//    +QueryDateTime
//
//////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public class InputBox
    {
        public static string pOk_Text = "OK";
        public static string pCancel_Text = "Cancel";
        public static string pOk_Yes = "Yes";
        public static string pOk_No = "No";
        public static string pOk_Abort = "Прервать";
        public static string pOk_Retry = "Повтор";
        public static string pOk_Ignore = "Пропуск";
        public static bool pShowInTaskBar = false;
        public static int defWidth = 300;

        private string _title;
        private string _promptText;
        private string[] _values;
        private int _valueIndex = 0;
        private string _prevValue;
        private bool _readOnly;
        private string _inputMaskOrRegex;
        private System.Drawing.Image _icon;
        private ImageList _imlist;
        private DialogResult _result = DialogResult.None;
        private object[] _additData = new object[6];

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }
        public string PromptText
        {
            get
            {
                return this._promptText;
            }
            set
            {
                this._promptText = value;
            }
        }
        public string Value
        {
            get
            {
                return _values[this._valueIndex];
            }
            set
            {
                if (_values.Length == 1)
                    this._values[0] = value;
                else
                {
                    for (int i = 0; i < this._values.Length; i++)
                        if (this._values[i] == value)
                            this._valueIndex = i;
                };
            }
        }
        public string[] Values
        {
            get
            {
                return this._values;
            }
            set
            {
                if (value == null) throw new Exception("Invalid length");
                if (value.Length == 0) throw new Exception("Invalid length");
                this._values = value;
                this._valueIndex = 0;
            }
        }
        public int SelectedIndex
        {
            get
            {
                return this._valueIndex;
            }
            set
            {
                if ((this._values.Length > 1) && (value >= 0) && (value < this._values.Length))
                    this._valueIndex = value;
            }
        }
        public bool ReadOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                this._readOnly = value;
            }
        }
        public string InputMaskOrRegex
        {
            get
            {
                return this._inputMaskOrRegex;
            }
            set
            {
                this._inputMaskOrRegex = value;
            }
        }
        public string InputMask
        {
            get
            {
                if (String.IsNullOrEmpty(this._inputMaskOrRegex))
                    return this._inputMaskOrRegex;

                string[] mr = this._inputMaskOrRegex.Split(new char[] { '\0' }, 2);
                for (int i = 0; i < mr.Length; i++)
                    if (mr[i].StartsWith("M"))
                        return mr[i].Substring(1);
                return "";
            }
            set
            {
                this._inputMaskOrRegex = "M" + value;
            }
        }
        public string InputRegex
        {
            get
            {
                if (String.IsNullOrEmpty(this._inputMaskOrRegex))
                    return this._inputMaskOrRegex;

                string[] mr = this._inputMaskOrRegex.Split(new char[] { '\0' }, 2);
                for (int i = 0; i < mr.Length; i++)
                    if (mr[i].StartsWith("R"))
                        return mr[i].Substring(1);
                return "";
            }
            set
            {
                this._inputMaskOrRegex = "R" + value;
            }
        }
        public ImageList IconList
        {
            get
            {
                return this._imlist;
            }
            set
            {
                this._imlist = value;
            }
        }
        public System.Drawing.Image Icon
        {
            get
            {
                return this._icon;
            }
            set
            {
                this._icon = value;
            }
        }
        public DialogResult Result
        {
            get
            {
                return _result;
            }
        }

        private InputBox(string Title, string PromptText)
        {
            this._title = Title;
            this._promptText = PromptText;
        }

        private InputBox(string Title, string PromptText, string Value)
        {
            this._title = Title;
            this._promptText = PromptText;
            this._values = new string[] { Value };
        }

        private InputBox(string Title, string PromptText, string[] Values)
        {
            this._title = Title;
            this._promptText = PromptText;
            this.Values = Values;
        }

        private InputBox(string Title, string PromptText, string[] Values, int SelectedIndex)
        {
            this._title = Title;
            this._promptText = PromptText;
            this.Values = Values;
            this.SelectedIndex = SelectedIndex;
        }

        private DialogResult Show()
        {
            if (this._values.Length == 1)
                return ShowMaskedTextBoxed();
            else
                return ShowComboBoxed();
        }

        private DialogResult ShowNumericBoxed(ref int val, int min, int max)
        {
            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label label = new Label();
            NumericUpDown digitBox = new NumericUpDown();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            form.Text = _title;
            label.Text = _promptText;
            digitBox.BorderStyle = BorderStyle.FixedSingle;
            digitBox.Minimum = min;
            digitBox.Maximum = max;
            digitBox.Value = val;
            digitBox.Select(0, digitBox.Value.ToString().Length);
            if (_icon != null) picture.Image = _icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, defWidth - 24, 13);
            digitBox.SetBounds(12, 36, defWidth - 24, 20);
            buttonOk.SetBounds(defWidth - 168, 72, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 72, 75, 23);
            picture.SetBounds(12, 72, 22, 22);

            label.AutoSize = true;
            digitBox.Anchor = digitBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 107);
            form.Controls.AddRange(new Control[] { label, digitBox, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            _result = form.ShowDialog();
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            _values[0] = ((val = (int)digitBox.Value)).ToString();
            return _result;
        }

        private DialogResult ShowMaskedTextBoxed()
        {
            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label label = new Label();
            MaskedTextBox textBox = new MaskedTextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            form.Text = _title;
            label.Text = _promptText;
            textBox.Text = _prevValue = _values[0];
            if (!String.IsNullOrEmpty(this.InputMask))
                textBox.Mask = this.InputMask;
            textBox.SelectionStart = 0;
            textBox.SelectionLength = textBox.Text.Length;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            Color bc = textBox.BackColor;
            if (_readOnly) textBox.ReadOnly = true;
            textBox.BackColor = bc;
            textBox.TextChanged += new EventHandler(MaskOrComboTextChanged);
            if (_icon != null) picture.Image = _icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, defWidth - 24, 13);
            textBox.SetBounds(12, 36, defWidth - 24, 20);
            buttonOk.SetBounds(defWidth - 168, 72, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 72, 75, 23);
            picture.SetBounds(12, 72, 22, 22);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            _result = form.ShowDialog();
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            _values[0] = textBox.Text;
            return _result;
        }

        private DialogResult ShowMultiline()
        {
            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            form.Text = _title;
            label.Text = _promptText;
            textBox.Text = _prevValue = _values[0];
            textBox.SelectionStart = 0;
            textBox.SelectionLength = textBox.Text.Length;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Both;
            Color bc = textBox.BackColor;
            if (_readOnly) textBox.ReadOnly = true;
            textBox.BackColor = bc;
            textBox.TextChanged += new EventHandler(MaskOrComboTextChanged);
            if (_icon != null) picture.Image = _icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, defWidth - 24, 13);
            textBox.SetBounds(12, 36, defWidth - 24, 200);
            buttonOk.SetBounds(defWidth - 168, 252, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 252, 75, 23);
            picture.SetBounds(12, 252, 22, 22);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 287);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            _result = form.ShowDialog();
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            _values[0] = textBox.Text;
            return _result;
        }

        private DialogResult ShowRegex(string testerText, bool allow_new, string test)
        {
            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label label = new Label();
            Label labed = new Label();
            TextBox textBox = new TextBox();
            ComboBox comboBox = new ComboBox();
            TextBox testBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            if (this._values.Length == 1)
            {
                textBox.Text = _prevValue = _values[0];
                textBox.SelectionStart = 0;
                textBox.SelectionLength = textBox.Text.Length;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.TextChanged += new EventHandler(testBox_TextChanged);
                _additData[0] = textBox;
                textBox.SetBounds(12, 36, defWidth - 24, 20);
                textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            }
            else
            {
                comboBox.FlatStyle = FlatStyle.Flat;
                comboBox.DropDownHeight = 200;
                if (this._readOnly)
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                else
                    comboBox.DropDownStyle = ComboBoxStyle.DropDown;
                foreach (string str in this._values)
                    comboBox.Items.Add(str);
                comboBox.SelectedIndex = this._valueIndex;
                this._prevValue = comboBox.Text;
                comboBox.TextChanged += new EventHandler(testBox_TextChanged);
                _additData[0] = comboBox;
                comboBox.SetBounds(12, 36, defWidth - 24, 20);
                comboBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            }

            form.Text = _title;
            label.Text = _promptText;
            labed.Text = testerText;
            testBox.Text = test;
            testBox.BorderStyle = BorderStyle.FixedSingle;
            testBox.TextChanged += new EventHandler(testBox_TextChanged);
            if (_icon != null) picture.Image = _icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, defWidth - 24, 13);
            labed.SetBounds(9, 60, defWidth - 24, 13);
            testBox.SetBounds(12, 76, defWidth - 24, 20);
            buttonOk.SetBounds(defWidth - 168, 112, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 112, 75, 23);
            picture.SetBounds(12, 112, 22, 22);

            label.AutoSize = true;
            labed.AutoSize = true;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 147);
            form.Controls.AddRange(new Control[] { label, this._values.Length == 1 ? (Control)textBox : (Control)comboBox, labed, testBox, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            _additData[1] = testBox;

            _result = form.ShowDialog();
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            if (this._values.Length == 1)
                _values[0] = textBox.Text;
            else
            {
                if (comboBox.SelectedIndex == -1)
                {
                    List<string> tmp = new List<string>(this._values);
                    tmp.Add(comboBox.Text);
                    this._values = tmp.ToArray();
                    this._valueIndex = this._values.Length - 1;
                }
                else
                    this._valueIndex = comboBox.SelectedIndex;
            };
            if (!String.IsNullOrEmpty(test))
            {
                _values[0] += (char)164;
                _values[0] += testBox.Text;
            };
            return _result;
        }

        private void testBox_TextChanged(object sender, EventArgs e)
        {
            if ((sender is TextBox) || (sender is ComboBox))
            {
                Control ctrlBox = (Control)_additData[0];
                TextBox testBox = (TextBox)_additData[1];

                try
                {
                    Regex rx = new Regex(ctrlBox.Text.Trim());
                    testBox.BackColor = rx.IsMatch(testBox.Text.Trim()) ? Color.LightGreen : Color.LightPink;
                }
                catch
                {
                    testBox.BackColor = Color.LightPink;
                };
            };
        }

        private DialogResult ShowComboBoxed()
        {
            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label label = new Label();
            ComboBox comboBox = new ComboBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            if (this.IconList != null)
                comboBox = new ComboIcons();

            form.Text = _title;
            label.Text = _promptText;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.DropDownHeight = 200;
            if (this._readOnly)
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            else
                comboBox.DropDownStyle = ComboBoxStyle.DropDown;
            for (int i = 0; i < this._values.Length; i++)
            {
                string str = this._values[i];
                comboBox.Items.Add(new DropDownItem(str, (this._imlist == null) || (i >= this._imlist.Images.Count) ? null : this._imlist.Images[i]));
            };
            comboBox.SelectedIndex = this._valueIndex;
            this._prevValue = comboBox.Text;
            comboBox.TextChanged += new EventHandler(MaskOrComboTextChanged);
            if (_icon != null) picture.Image = _icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, defWidth - 24, 13);
            comboBox.SetBounds(12, 36, defWidth - 24, 20);
            buttonOk.SetBounds(defWidth - 168, 72, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 72, 75, 23);
            picture.SetBounds(12, 72, 22, 22);

            label.AutoSize = true;
            comboBox.Anchor = comboBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 107);
            form.Controls.AddRange(new Control[] { label, comboBox, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            _result = form.ShowDialog();
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            if (comboBox.SelectedIndex == -1)
            {
                List<string> tmp = new List<string>(this._values);
                tmp.Add(comboBox.Text);
                this._values = tmp.ToArray();
                this._valueIndex = this._values.Length - 1;
            }
            else
                this._valueIndex = comboBox.SelectedIndex;
            return _result;
        }

        private DialogResult ShowSelectDir()
        {
            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label label = new Label();
            MaskedTextBox textBox = new MaskedTextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            Button buttonAddit = new Button();
            PictureBox picture = new PictureBox();

            form.Text = _title;
            label.Text = _promptText;
            textBox.Text = _values[0];
            textBox.SelectionStart = 0;
            textBox.SelectionLength = textBox.Text.Length;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            Color bc = textBox.BackColor;
            if (_readOnly) textBox.ReadOnly = true;
            textBox.BackColor = bc;
            if (_icon != null) picture.Image = _icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;
            buttonAddit.Click += new EventHandler(buttonAdditD_Click);
            _additData[0] = textBox;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonAddit.Text = "..";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, defWidth - 24, 13);
            textBox.SetBounds(12, 36, defWidth - 52, 20);
            buttonOk.SetBounds(defWidth - 168, 72, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 72, 75, 23);
            buttonAddit.SetBounds(defWidth - 36, 36, 24, 20);
            picture.SetBounds(12, 72, 22, 22);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAddit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonAddit, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            _result = form.ShowDialog();
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            _values[0] = textBox.Text;
            return _result;
        }

        private DialogResult ShowSelectFile(string filter)
        {
            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label label = new Label();
            MaskedTextBox textBox = new MaskedTextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            Button buttonAddit = new Button();
            PictureBox picture = new PictureBox();

            form.Text = _title;
            label.Text = _promptText;
            textBox.Text = _values[0];
            textBox.SelectionStart = 0;
            textBox.SelectionLength = textBox.Text.Length;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            Color bc = textBox.BackColor;
            if (_readOnly) textBox.ReadOnly = true;
            textBox.BackColor = bc;
            if (_icon != null) picture.Image = _icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;
            buttonAddit.Click += new EventHandler(buttonAdditF_Click);
            _additData[0] = textBox;
            if (String.IsNullOrEmpty(filter))
                _additData[1] = "All Types|*.*";
            else
                _additData[1] = filter;
            _additData[2] = _title;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonAddit.Text = "..";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, defWidth - 24, 13);
            textBox.SetBounds(12, 36, defWidth - 52, 20);
            buttonOk.SetBounds(defWidth - 168, 72, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 72, 75, 23);
            buttonAddit.SetBounds(defWidth - 36, 36, 24, 20);
            picture.SetBounds(12, 72, 22, 22);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAddit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonAddit, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            _result = form.ShowDialog();
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            _values[0] = textBox.Text;
            return _result;
        }

        private DialogResult ShowSelectColor(ref Color color)
        {
            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label label = new Label();
            MaskedTextBox textBox = new MaskedTextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            Button buttonAddit = new Button();
            PictureBox picture = new PictureBox();

            form.Text = _title;
            label.Text = _promptText;
            textBox.Text = HexConverter(color);
            textBox.SelectionStart = 0;
            textBox.SelectionLength = textBox.Text.Length;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Mask = @"\#AAAAAA";
            this.InputRegex = @"^(#[\dA-Fa-f]{0,6})$";
            textBox.TextChanged += new EventHandler(colorBox_TextChanged);
            if (_icon != null) picture.Image = _icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;
            buttonAddit.Text = "..";
            buttonAddit.FlatStyle = FlatStyle.Flat;
            buttonAddit.BackColor = color;
            buttonAddit.Click += new EventHandler(buttonAdditC_Click);
            _additData[0] = textBox;
            _additData[1] = buttonAddit;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonAddit.Text = "..";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, defWidth - 24, 13);
            textBox.SetBounds(12, 36, defWidth - 52, 20);
            buttonOk.SetBounds(defWidth - 168, 72, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 72, 75, 23);
            buttonAddit.SetBounds(defWidth - 36, 36, 24, 20);
            picture.SetBounds(12, 72, 22, 22);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAddit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonAddit, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            _result = form.ShowDialog();
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            color = RGBConverter(textBox.Text);
            return _result;
        }

        private void colorBox_TextChanged(object sender, EventArgs e)
        {
            ((Button)_additData[1]).BackColor = RGBConverter((string)((MaskedTextBox)_additData[0]).Text);
        }

        private void buttonAdditC_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.FullOpen = true;
            cd.Color = RGBConverter((string)((MaskedTextBox)_additData[0]).Text);
            if (cd.ShowDialog() == DialogResult.OK)
            {
                ((MaskedTextBox)_additData[0]).Text = HexConverter(cd.Color);
                ((Button)_additData[1]).BackColor = cd.Color;
            };
            cd.Dispose();
        }

        private void buttonAdditF_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = ((MaskedTextBox)_additData[0]).Text;
            ofd.Title = (string)_additData[2];
            ofd.Filter = (string)_additData[1];
            ofd.FileName = (string)((MaskedTextBox)_additData[0]).Text;
            try
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                    ((MaskedTextBox)_additData[0]).Text = ofd.FileName;
            }
            catch
            {
                ofd.FileName = "";
                if (ofd.ShowDialog() == DialogResult.OK)
                    ((MaskedTextBox)_additData[0]).Text = ofd.FileName;
            };
            ofd.Dispose();
        }

        private void buttonAdditD_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = (string)((MaskedTextBox)_additData[0]).Text;
            if (fbd.ShowDialog() == DialogResult.OK)
                ((MaskedTextBox)_additData[0]).Text = fbd.SelectedPath;
            fbd.Dispose();
        }

        private void MaskOrComboTextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(InputRegex)) return;

            if (sender is MaskedTextBox)
            {
                MaskedTextBox tb = (MaskedTextBox)sender;
                int index = tb.SelectionStart > 0 ? tb.SelectionStart - 1 : 0;
                if (String.IsNullOrEmpty(tb.Text)) return;
                if (Regex.IsMatch(tb.Text, InputRegex))
                {
                    _prevValue = tb.Text;
                    return;
                }
                else
                {
                    tb.Text = _prevValue;
                    tb.SelectionStart = index;
                };
            };
            if (sender is ComboBox)
            {
                ComboBox cb = (ComboBox)sender;
                int index = cb.SelectionStart > 0 ? cb.SelectionStart - 1 : 0;
                if (String.IsNullOrEmpty(cb.Text)) return;
                if (Regex.IsMatch(cb.Text, InputRegex))
                {
                    _prevValue = cb.Text;
                    return;
                }
                else
                {
                    cb.Text = _prevValue;
                    cb.SelectionStart = index;
                };
            };
        }

        /// <summary>
        ///     Show ReadOnly Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>6
        public static DialogResult Show(string title, string promptText, string value)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            value = ib.Value;
            return dr;
        }
        /// <summary>
        ///     Show ReadOnly Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Icon Image</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.ReadOnly = true;
            ib.Icon = icon;
            DialogResult dr = ib.Show();
            value = ib.Value;
            return dr;
        }
        /// <summary>
        ///     Show Editable Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref string value)
        {
            InputBox ib = new InputBox(title, promptText, value);
            DialogResult dr = ib.Show();
            value = ib.Value;
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Icon Image</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref string value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.Icon = icon;
            DialogResult dr = ib.Show();
            value = ib.Value;
            return dr;

        }
        /// <summary>
        ///     Show Editable Masked Input Box Dialog 
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="InputMaskOrRegex">Input Mask or Regex for Value
        /// <para>If text is Input Mask it starts from M. If text is Regex it starts from R.</para>
        /// <para>Input Mask symbols: 0 - digits; 9 - digits and spaces; # - digits, spaces, +, -</para>
        /// <para>  L - letter; ? - letters if need; A - letter or digit; . - decimal separator;</para>
        /// <para>  , - space for digits; / - date separator; $ - currency symbol</para>
        /// <para>Regex: http://regexstorm.net/reference</para>
        /// </param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref string value, string InputMaskOrRegex)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.InputMaskOrRegex = InputMaskOrRegex;
            DialogResult dr = ib.Show();
            value = ib.Value;
            return dr;
        }
        /// <summary>
        ///     Show Editable Masked Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="InputMaskOrRegex">Input Mask or Regex for Value
        /// <para>If text is Input Mask it starts from M. If text is Regex it starts from R.</para>
        /// <para>Input Mask symbols: 0 - digits; 9 - digits and spaces; # - digits, spaces, +, -</para>
        /// <para>  L - letter; ? - letters if need; A - letter or digit; . - decimal separator;</para>
        /// <para>  , - space for digits; / - date separator; $ - currency symbol</para>
        /// <para>Regex: http://regexstorm.net/reference</para>
        /// </param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref string value, string InputMaskOrRegex, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.Icon = icon;
            DialogResult dr = ib.Show();
            value = ib.Value.ToString();
            return dr;
        }

        public static DialogResult QueryText(string title, string promptText, ref string value)
        {
            InputBox ib = new InputBox(title, promptText, value);
            DialogResult dr = ib.ShowMultiline();
            value = ib.Value;
            return dr;
        }

        public static DialogResult QueryText(string title, string promptText, string value)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.ReadOnly = true;
            DialogResult dr = ib.ShowMultiline();
            return dr;
        }

        public static DialogResult QueryText(string title, string promptText, string value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.ReadOnly = true;
            ib.Icon = icon;
            DialogResult dr = ib.ShowMultiline();
            return dr;
        }

        public static DialogResult QueryText(string title, string promptText, ref string value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.Icon = icon;
            DialogResult dr = ib.ShowMultiline();
            value = ib.Value;
            return dr;
        }

        public static DialogResult QueryPass(string title, string promptText, ref string value)
        {
            InputBox ib = new InputBox(title, promptText, value);
            DialogResult dr = ib.ShowPass();
            value = ib.Value;
            return dr;
        }


        private DialogResult ShowPass()
        {
            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label label = new Label();
            MaskedTextBox textBox = new MaskedTextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            form.Text = _title;
            label.Text = _promptText;
            textBox.Text = _prevValue = _values[0];
            if (!String.IsNullOrEmpty(this.InputMask))
                textBox.Mask = this.InputMask;
            textBox.SelectionStart = 0;
            textBox.SelectionLength = textBox.Text.Length;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.PasswordChar = '*';
            Color bc = textBox.BackColor;
            if (_readOnly) textBox.ReadOnly = true;
            textBox.BackColor = bc;
            textBox.TextChanged += new EventHandler(MaskOrComboTextChanged);
            if (_icon != null) picture.Image = _icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, defWidth - 24, 13);
            textBox.SetBounds(12, 36, defWidth - 24, 20);
            buttonOk.SetBounds(defWidth - 168, 72, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 72, 75, 23);
            picture.SetBounds(12, 72, 22, 22);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            _result = form.ShowDialog();
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            _values[0] = textBox.Text;
            return _result;
        }

        public static DialogResult QueryDateTime(string title, string promptText, ref DateTime value)
        {
            return QueryDateTime(title, promptText, null, ref value);
        }

        public static DialogResult QueryDate(string title, string promptText, ref DateTime value)
        {
            return QueryDateTime(title, promptText, "dd.MM.yyyy", ref value);
        }

        public static DialogResult QueryTime(string title, string promptText, ref DateTime value)
        {
            return QueryDateTime(title, promptText, "HH:mm", ref value);
        }

        public static DialogResult QueryDateTime(string title, string promptText, string format, ref DateTime value)
        {
            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label label = new Label();
            DateTimePicker dtBox = new DateTimePicker();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            form.Text = String.IsNullOrEmpty(title) ? "Select Date/Time" : title;
            label.Text = String.IsNullOrEmpty(promptText) ? "Select Date/Time" : promptText;
            if (value == null)
                dtBox.Value = DateTime.Today;
            else
                dtBox.Value = value;
            if (!String.IsNullOrEmpty(format))
            {
                dtBox.Format = DateTimePickerFormat.Custom;
                dtBox.CustomFormat = format;
            };

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, defWidth - 24, 13);
            dtBox.SetBounds(12, 36, defWidth - 24, 20);
            buttonOk.SetBounds(defWidth - 168, 72, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 72, 75, 23);
            picture.SetBounds(12, 72, 22, 22);

            label.AutoSize = true;
            dtBox.Anchor = dtBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 107);
            form.Controls.AddRange(new Control[] { label, dtBox, buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult _result = form.ShowDialog();
            form.Dispose();
            value = dtBox.Value;
            return _result;
        }

        public static DialogResult QueryRegexBox(string title, string promptText, string testerText, ref string value)
        {
            InputBox ib = new InputBox(title, promptText, value);
            DialogResult dr = ib.ShowRegex(testerText, false, "");
            value = ib.Value;
            return dr;
        }

        public static DialogResult QueryReplaceBox(string title, string promptText, string testerText, ref string value, ref string test)
        {
            InputBox ib = new InputBox(title, promptText, value);
            DialogResult dr = ib.ShowRegex(testerText, false, test);
            string[] vt = ib.Value.Split(new char[] { (char)164 });
            value = vt[0];
            if (vt.Length > 1)
                test = vt[1];
            else
                test = "";
            return dr;
        }

        public static DialogResult QueryRegexBox(string title, string promptText, string testerText, ref string value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.Icon = icon;
            DialogResult dr = ib.ShowRegex(testerText, false, "");
            value = ib.Value;
            return dr;
        }

        public static DialogResult QueryRegexBox(string title, string promptText, string testerText, string[] options, ref int selectedValue)
        {
            InputBox ib = new InputBox(title, promptText, options, selectedValue);
            ib.ReadOnly = true;
            DialogResult dr = ib.ShowRegex(testerText, false, "");
            selectedValue = ib.SelectedIndex;
            return dr;
        }

        public static DialogResult QueryRegexBox(string title, string promptText, string testerText, string[] options, ref int selectedValue, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, options, selectedValue);
            ib.ReadOnly = true;
            ib.Icon = icon;
            DialogResult dr = ib.ShowRegex(testerText, false, "");
            selectedValue = ib.SelectedIndex;
            return dr;
        }

        public static DialogResult QueryRegexBox(string title, string promptText, string testerText, string[] options, ref string selectedValue)
        {
            InputBox ib = new InputBox(title, promptText, options);
            ib.ReadOnly = true;
            ib.Value = selectedValue;
            DialogResult dr = ib.ShowRegex(testerText, false, "");
            selectedValue = ib.Value;
            return dr;
        }

        public static DialogResult QueryRegexBox(string title, string promptText, string testerText, string[] options, ref string selectedValue, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, options);
            ib.ReadOnly = true;
            ib.Value = selectedValue;
            ib.Icon = icon;
            DialogResult dr = ib.ShowRegex(testerText, false, "");
            selectedValue = ib.Value;
            return dr;
        }

        public static DialogResult QueryRegexBox(string title, string promptText, string testerText, string[] options, ref string selectedValue, bool allowNewValue)
        {
            InputBox ib = new InputBox(title, promptText, options);
            ib.ReadOnly = !allowNewValue;
            ib.Value = selectedValue;
            DialogResult dr = ib.ShowRegex(testerText, true, "");
            selectedValue = ib.Value;
            return dr;
        }

        public static DialogResult QueryRegexBox(string title, string promptText, string testerText, string[] options, ref string selectedValue, bool allowNewValue, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, options);
            ib.ReadOnly = !allowNewValue;
            ib.Value = selectedValue;
            ib.Icon = icon;
            DialogResult dr = ib.ShowRegex(testerText, true, "");
            selectedValue = ib.Value;
            return dr;
        }

        public static DialogResult QueryMultiple(string title, string[] prompts, string[] values, string inputMask, bool readOnly, Bitmap icon)
        {
            if ((prompts == null) || (values == null) || (prompts.Length == 0) || (values.Length == 0) || (values.Length != prompts.Length))
                return DialogResult.None;

            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label[] labels = new Label[prompts.Length];
            MaskedTextBox[] textBoxes = new MaskedTextBox[values.Length];
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            form.Text = title;
            int lRight = 0;
            for (int i = 0; i < prompts.Length; i++)
            {
                labels[i] = new Label();
                labels[i].Text = prompts[i];

                textBoxes[i] = new MaskedTextBox();
                textBoxes[i].Text = values[i];
                textBoxes[i].SelectionStart = 0;
                textBoxes[i].SelectionLength = textBoxes[i].Text.Length;
                textBoxes[i].BorderStyle = BorderStyle.FixedSingle;
                if (!String.IsNullOrEmpty(inputMask))
                    textBoxes[i].Mask = inputMask;
                Color bc = textBoxes[i].BackColor;
                if (readOnly) textBoxes[i].ReadOnly = true;
                textBoxes[i].BackColor = bc;
                // textBoxes[i].TextChanged += new EventHandler(MaskOrComboTextChanged);

                labels[i].SetBounds(9, 20 + 40 * i, defWidth - 24, 13);
                labels[i].AutoSize = true;
                textBoxes[i].SetBounds(12, 36 + 40 * i, defWidth - 24, 20);
                textBoxes[i].Anchor = textBoxes[i].Anchor | AnchorStyles.Right;

                if (labels[i].Right > lRight) lRight = labels[i].Right;
            };

            if (icon != null) picture.Image = icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;


            buttonOk.SetBounds(defWidth - 168, 32 + 40 * prompts.Length, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 32 + 40 * prompts.Length, 75, 23);
            picture.SetBounds(12, 32 + 40 * prompts.Length, 22, 22);

            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 67 + 40 * prompts.Length);
            form.Controls.AddRange(labels);
            form.Controls.AddRange(textBoxes);
            form.Controls.AddRange(new Control[] { buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, lRight + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult _result = form.ShowDialog();
            for (int i = 0; i < prompts.Length; i++)
                values[i] = textBoxes[i].Text;
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            return _result;
        }

        public static DialogResult QueryMultiple(string title, string[] prompts, string[] values, string inputMask, bool readOnly, Bitmap icon, MessageBoxButtons buttons, string[] buttonsText)
        {
            if ((prompts == null) || (values == null) || (prompts.Length == 0) || (values.Length == 0) || (values.Length != prompts.Length))
                return DialogResult.None;

            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label[] labels = new Label[prompts.Length];
            MaskedTextBox[] textBoxes = new MaskedTextBox[values.Length];
            int btnCount = 1;
            if ((buttons == MessageBoxButtons.OKCancel) || (buttons == MessageBoxButtons.YesNo) || (buttons == MessageBoxButtons.RetryCancel)) btnCount = 2;
            if ((buttons == MessageBoxButtons.AbortRetryIgnore) || (buttons == MessageBoxButtons.YesNoCancel)) btnCount = 3;
            Button[] Buttons = new Button[btnCount];
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            form.Text = title;
            int lRight = 0;
            for (int i = 0; i < prompts.Length; i++)
            {
                labels[i] = new Label();
                labels[i].Text = prompts[i];

                textBoxes[i] = new MaskedTextBox();
                textBoxes[i].Text = values[i];
                textBoxes[i].SelectionStart = 0;
                textBoxes[i].SelectionLength = textBoxes[i].Text.Length;
                textBoxes[i].BorderStyle = BorderStyle.FixedSingle;
                if (!String.IsNullOrEmpty(inputMask))
                    textBoxes[i].Mask = inputMask;
                Color bc = textBoxes[i].BackColor;
                if (readOnly) textBoxes[i].ReadOnly = true;
                textBoxes[i].BackColor = bc;
                // textBoxes[i].TextChanged += new EventHandler(MaskOrComboTextChanged);

                labels[i].SetBounds(9, 20 + 40 * i, defWidth - 24, 13);
                labels[i].AutoSize = true;
                textBoxes[i].SetBounds(12, 36 + 40 * i, defWidth - 24, 20);
                textBoxes[i].Anchor = textBoxes[i].Anchor | AnchorStyles.Right;

                if (labels[i].Right > lRight) lRight = labels[i].Right;
            };

            if (icon != null) picture.Image = icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            for (int i = 0; i < btnCount; i++)
            {
                Buttons[i] = new Button();
                Buttons[i].Text = pOk_Text;
                if (i == 0)
                {
                    if (buttons == MessageBoxButtons.OK) { Buttons[i].Text = pOk_Text; Buttons[i].DialogResult = DialogResult.OK; };
                    if (buttons == MessageBoxButtons.OKCancel) { Buttons[i].Text = pOk_Text; Buttons[i].DialogResult = DialogResult.OK; };
                    if (buttons == MessageBoxButtons.AbortRetryIgnore) { Buttons[i].Text = "Abort"; Buttons[i].DialogResult = DialogResult.Abort; };
                    if (buttons == MessageBoxButtons.YesNoCancel) { Buttons[i].Text = "Yes"; Buttons[i].DialogResult = DialogResult.Yes; };
                    if (buttons == MessageBoxButtons.YesNo) { Buttons[i].Text = "Yes"; Buttons[i].DialogResult = DialogResult.Yes; };
                    if (buttons == MessageBoxButtons.RetryCancel) { Buttons[i].Text = "Retry"; Buttons[i].DialogResult = DialogResult.Retry; };
                    if ((buttonsText != null) && (buttonsText.Length > 0)) Buttons[i].Text = buttonsText[i];
                };
                if (i == 1)
                {
                    if (buttons == MessageBoxButtons.OKCancel) { Buttons[i].Text = pCancel_Text; Buttons[i].DialogResult = DialogResult.Cancel; };
                    if (buttons == MessageBoxButtons.AbortRetryIgnore) { Buttons[i].Text = "Retry"; Buttons[i].DialogResult = DialogResult.Retry; };
                    if (buttons == MessageBoxButtons.YesNoCancel) { Buttons[i].Text = "No"; Buttons[i].DialogResult = DialogResult.No; };
                    if (buttons == MessageBoxButtons.YesNo) { Buttons[i].Text = "No"; Buttons[i].DialogResult = DialogResult.No; };
                    if (buttons == MessageBoxButtons.RetryCancel) { Buttons[i].Text = pCancel_Text; Buttons[i].DialogResult = DialogResult.Cancel; };
                    if ((buttonsText != null) && (buttonsText.Length > 1)) Buttons[i].Text = buttonsText[i];
                };
                if (i == 2)
                {
                    if (buttons == MessageBoxButtons.AbortRetryIgnore) { Buttons[i].Text = "Ignore"; Buttons[i].DialogResult = DialogResult.Ignore; };
                    if (buttons == MessageBoxButtons.YesNoCancel) { Buttons[i].Text = pCancel_Text; Buttons[i].DialogResult = DialogResult.Cancel; };
                    if ((buttonsText != null) && (buttonsText.Length > 2)) Buttons[i].Text = buttonsText[i];
                };
                Buttons[i].SetBounds(defWidth - 87 - 81 * (btnCount - i - 1), 32 + 40 * prompts.Length, 75, 23);
                Buttons[i].Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            };

            buttonCancel.SetBounds(defWidth - 87, 32 + 40 * prompts.Length, 75, 23);
            picture.SetBounds(12, 32 + 40 * prompts.Length, 22, 22);


            form.ClientSize = new Size(defWidth, 67 + 40 * prompts.Length);
            form.Controls.AddRange(labels);
            form.Controls.AddRange(textBoxes);
            form.Controls.AddRange(Buttons);
            form.Controls.AddRange(new Control[] { picture });
            form.ClientSize = new Size(Math.Max(defWidth, lRight + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = Buttons[0];
            form.CancelButton = Buttons[btnCount - 1];

            DialogResult _result = form.ShowDialog();
            for (int i = 0; i < prompts.Length; i++)
                values[i] = textBoxes[i].Text;
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            return _result;
        }

        public static DialogResult QueryMultiple(string title, string[] prompts, int[] values, bool readOnly, Bitmap icon)
        {
            return QueryMultiple(title, prompts, values, int.MinValue, int.MaxValue, readOnly, icon);
        }

        public static DialogResult QueryMultiple(string title, string[] prompts, int[] values, int minValue, int maxValue, bool readOnly, Bitmap icon)
        {
            if ((prompts == null) || (values == null) || (prompts.Length == 0) || (values.Length == 0) || (values.Length != prompts.Length))
                return DialogResult.None;

            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label[] labels = new Label[prompts.Length];
            NumericUpDown[] textBoxes = new NumericUpDown[values.Length];
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            form.Text = title;
            int lRight = 0;
            for (int i = 0; i < prompts.Length; i++)
            {
                labels[i] = new Label();
                labels[i].Text = prompts[i];

                textBoxes[i] = new NumericUpDown();
                textBoxes[i].Minimum = minValue;
                textBoxes[i].Maximum = maxValue;
                textBoxes[i].Value = values[i];
                textBoxes[i].BorderStyle = BorderStyle.FixedSingle;
                Color bc = textBoxes[i].BackColor;
                if (readOnly) textBoxes[i].ReadOnly = true;
                textBoxes[i].BackColor = bc;

                labels[i].SetBounds(9, 20 + 40 * i, defWidth - 24, 13);
                labels[i].AutoSize = true;
                textBoxes[i].SetBounds(12, 36 + 40 * i, defWidth - 24, 20);
                textBoxes[i].Anchor = textBoxes[i].Anchor | AnchorStyles.Right;

                if (labels[i].Right > lRight) lRight = labels[i].Right;
            };

            if (icon != null) picture.Image = icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;


            buttonOk.SetBounds(defWidth - 168, 32 + 40 * prompts.Length, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 32 + 40 * prompts.Length, 75, 23);
            picture.SetBounds(12, 32 + 40 * prompts.Length, 22, 22);

            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 67 + 40 * prompts.Length);
            form.Controls.AddRange(labels);
            form.Controls.AddRange(textBoxes);
            form.Controls.AddRange(new Control[] { buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, lRight + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult _result = form.ShowDialog();
            for (int i = 0; i < prompts.Length; i++)
                values[i] = (int)textBoxes[i].Value;
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            return _result;
        }

        public static DialogResult QueryMultiple(string title, string[] prompts, double[] values, bool readOnly, Bitmap icon)
        {
            return QueryMultiple(title, prompts, values, double.MinValue, double.MinValue, readOnly, icon);
        }

        private static double[] qm_d_mima = new double[] { double.MinValue, double.MaxValue };
        public static DialogResult QueryMultiple(string title, string[] prompts, double[] values, double minValue, double maxValue, bool readOnly, Bitmap icon)
        {
            if ((prompts == null) || (values == null) || (prompts.Length == 0) || (values.Length == 0) || (values.Length != prompts.Length))
                return DialogResult.None;

            qm_d_mima[0] = minValue;
            qm_d_mima[1] = maxValue;

            Form form = new InputBoxForm();
            form.ShowInTaskbar = pShowInTaskBar;
            Label[] labels = new Label[prompts.Length];
            MaskedTextBox[] textBoxes = new MaskedTextBox[values.Length];
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            PictureBox picture = new PictureBox();

            form.Text = title;
            int lRight = 0;
            for (int i = 0; i < prompts.Length; i++)
            {
                labels[i] = new Label();
                labels[i].Text = prompts[i];

                textBoxes[i] = new MaskedTextBox();
                textBoxes[i].Text = values[i].ToString(System.Globalization.CultureInfo.InvariantCulture);
                textBoxes[i].SelectionStart = 0;
                textBoxes[i].SelectionLength = textBoxes[i].Text.Length;
                textBoxes[i].BorderStyle = BorderStyle.FixedSingle;
                Color bc = textBoxes[i].BackColor;
                if (readOnly) textBoxes[i].ReadOnly = true;
                textBoxes[i].BackColor = bc;
                textBoxes[i].Validating += new System.ComponentModel.CancelEventHandler(InputBox_Validating);

                labels[i].SetBounds(9, 20 + 40 * i, defWidth - 24, 13);
                labels[i].AutoSize = true;
                textBoxes[i].SetBounds(12, 36 + 40 * i, defWidth - 24, 20);
                textBoxes[i].Anchor = textBoxes[i].Anchor | AnchorStyles.Right;

                if (labels[i].Right > lRight) lRight = labels[i].Right;
            };

            if (icon != null) picture.Image = icon;
            picture.SizeMode = PictureBoxSizeMode.StretchImage;

            buttonOk.Text = pOk_Text;
            buttonCancel.Text = pCancel_Text;
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;


            buttonOk.SetBounds(defWidth - 168, 32 + 40 * prompts.Length, 75, 23);
            buttonCancel.SetBounds(defWidth - 87, 32 + 40 * prompts.Length, 75, 23);
            picture.SetBounds(12, 32 + 40 * prompts.Length, 22, 22);

            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(defWidth, 67 + 40 * prompts.Length);
            form.Controls.AddRange(labels);
            form.Controls.AddRange(textBoxes);
            form.Controls.AddRange(new Control[] { buttonOk, buttonCancel, picture });
            form.ClientSize = new Size(Math.Max(defWidth, lRight + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult _result = form.ShowDialog();
            for (int i = 0; i < prompts.Length; i++)
                values[i] = double.Parse(textBoxes[i].Text, System.Globalization.CultureInfo.InvariantCulture);
            if (picture.Image != null) picture.Image.Dispose();
            form.Dispose();
            return _result;
        }

        private static void InputBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is MaskedTextBox)
            {
                double d = 0;
                if (double.TryParse((sender as MaskedTextBox).Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d)) ;
                if (d < qm_d_mima[0]) d = qm_d_mima[0];
                if (d > qm_d_mima[1]) d = qm_d_mima[1];
                (sender as MaskedTextBox).Text = d.ToString(System.Globalization.CultureInfo.InvariantCulture);
            };
        }

        /// <summary>
        ///     Show Editable Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryStringBox(string title, string promptText, ref string value)
        {
            InputBox ib = new InputBox(title, promptText, value);
            DialogResult dr = ib.Show();
            value = ib.Value;
            return dr;
        }
        /// <summary>
        ///     Show Editable Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Icon Image</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryStringBox(string title, string promptText, ref string value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.Icon = icon;
            DialogResult dr = ib.Show();
            value = ib.Value;
            return dr;

        }
        /// <summary>
        ///     Show Editable Masked Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="InputMaskOrRegex">Input Mask or Regex for Value
        /// <para>If text is Input Mask it starts from M. If text is Regex it starts from R.</para>
        /// <para>Input Mask symbols: 0 - digits; 9 - digits and spaces; # - digits, spaces, +, -</para>
        /// <para>  L - letter; ? - letters if need; A - letter or digit; . - decimal separator;</para>
        /// <para>  , - space for digits; / - date separator; $ - currency symbol</para>
        /// <para>Regex: http://regexstorm.net/reference</para>
        /// </param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryStringBox(string title, string promptText, ref string value, string InputMaskOrRegex)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.InputMaskOrRegex = InputMaskOrRegex;
            DialogResult dr = ib.Show();
            value = ib.Value;
            return dr;
        }
        /// <summary>
        ///     Show Editable Masked Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="InputMaskOrRegex">Input Mask or Regex for Value
        /// <para>If text is Input Mask it starts from M. If text is Regex it starts from R.</para>
        /// <para>Input Mask symbols: 0 - digits; 9 - digits and spaces; # - digits, spaces, +, -</para>
        /// <para>  L - letter; ? - letters if need; A - letter or digit; . - decimal separator;</para>
        /// <para>  , - space for digits; / - date separator; $ - currency symbol</para>
        /// <para>Regex: http://regexstorm.net/reference</para>
        /// </param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryStringBox(string title, string promptText, ref string value, string InputMaskOrRegex, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value);
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            value = ib.Value.ToString();
            return dr;
        }

        /// <summary>
        ///     Show ReadOnly Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, int value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString());
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            return dr;

        }
        /// <summary>
        ///     Show ReadOnly Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, int value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString());
            ib.Icon = icon;
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref int value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString());
            DialogResult dr = ib.ShowNumericBoxed(ref value, int.MinValue, int.MaxValue);
            value = int.Parse(ib.Value);
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref int value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString());
            ib.Icon = icon;
            DialogResult dr = ib.ShowNumericBoxed(ref value, int.MinValue, int.MaxValue);
            value = int.Parse(ib.Value);
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="min">Minimum Allowed Value</param>
        /// <param name="max">Maximum Allowed Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref int value, int min, int max)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString());
            DialogResult dr = ib.ShowNumericBoxed(ref value, min, max);
            value = int.Parse(ib.Value);
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="min">Minimum Allowed Value</param>
        /// <param name="max">Maximum Allowed Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref int value, int min, int max, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString());
            ib.Icon = icon;
            DialogResult dr = ib.ShowNumericBoxed(ref value, min, max);
            value = int.Parse(ib.Value);
            return dr;

        }

        /// <summary>
        ///     Show Editable Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, ref int value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString());
            DialogResult dr = ib.ShowNumericBoxed(ref value, int.MinValue, int.MaxValue);
            value = int.Parse(ib.Value);
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, ref int value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString());
            ib.Icon = icon;
            DialogResult dr = ib.ShowNumericBoxed(ref value, int.MinValue, int.MaxValue);
            value = int.Parse(ib.Value);
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="min">Minimum Allowed Value</param>
        /// <param name="max">Maximum Allowed Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, ref int value, int min, int max)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString());
            DialogResult dr = ib.ShowNumericBoxed(ref value, min, max);
            value = int.Parse(ib.Value);
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="min">Minimum Allowed Value</param>
        /// <param name="max">Maximum Allowed Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, ref int value, int min, int max, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString());
            ib.Icon = icon;
            DialogResult dr = ib.ShowNumericBoxed(ref value, min, max);
            value = int.Parse(ib.Value);
            return dr;

        }


        /// <summary>
        ///     Show Selectable List Box
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Index</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string[] values, ref int selectedValue)
        {
            InputBox ib = new InputBox(title, promptText, values, selectedValue);
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            selectedValue = ib.SelectedIndex;
            return dr;

        }
        /// <summary>
        ///     Show Selectable List Box with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Index</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string[] values, ref int selectedValue, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, values, selectedValue);
            ib.ReadOnly = true;
            ib.Icon = icon;
            DialogResult dr = ib.Show();
            selectedValue = ib.SelectedIndex;
            return dr;

        }
        /// <summary>
        ///     Show Selectable List Box with Icons
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Index</param>
        /// <param name="icons">ImageList</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string[] values, ref int selectedValue, ImageList icons)
        {
            InputBox ib = new InputBox(title, promptText, values, selectedValue);
            ib.ReadOnly = true;
            ib.IconList = icons;
            DialogResult dr = ib.Show();
            selectedValue = ib.SelectedIndex;
            return dr;

        }
        /// <summary>
        ///     Show Selectable List Box
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string[] values, ref string selectedValue)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.ReadOnly = true;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;

        }
        /// <summary>
        ///     Show Selectable List Box with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string[] values, ref string selectedValue, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.ReadOnly = true;
            ib.Icon = icon;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;

        }
        /// <summary>
        ///     Show Selectable List Box with Icons
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="icons">ImageList</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string[] values, ref string selectedValue, ImageList icons)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.ReadOnly = true;
            ib.IconList = icons;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;
        }

        /// <summary>
        ///     Show Changable List Box 
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="InputMaskOrRegex">Input Mask or Regex for Value
        /// <para>If text is Input Mask it starts from M. If text is Regex it starts from R.</para>
        /// <para>Input Mask symbols: 0 - digits; 9 - digits and spaces; # - digits, spaces, +, -</para>
        /// <para>  L - letter; ? - letters if need; A - letter or digit; . - decimal separator;</para>
        /// <para>  , - space for digits; / - date separator; $ - currency symbol</para>
        /// <para>Regex: http://regexstorm.net/reference</para>
        /// </param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string[] values, ref string selectedValue, string InputMaskOrRegex)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.InputMaskOrRegex = InputMaskOrRegex;
            ib.ReadOnly = false;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;
        }
        /// <summary>
        ///     Show Changable List Box with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="InputMaskOrRegex">Input Mask or Regex for Value
        /// <para>If text is Input Mask it starts from M. If text is Regex it starts from R.</para>
        /// <para>Input Mask symbols: 0 - digits; 9 - digits and spaces; # - digits, spaces, +, -</para>
        /// <para>  L - letter; ? - letters if need; A - letter or digit; . - decimal separator;</para>
        /// <para>  , - space for digits; / - date separator; $ - currency symbol</para>
        /// <para>Regex: http://regexstorm.net/reference</para>
        /// </param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string[] values, ref string selectedValue, string InputMaskOrRegex, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.InputMaskOrRegex = InputMaskOrRegex;
            ib.ReadOnly = false;
            ib.Icon = icon;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;
        }
        /// <summary>
        ///      Show Listable or Changable List
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="allowNewValue">Changable or Editable</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string[] values, ref string selectedValue, bool allowNewValue)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.ReadOnly = !allowNewValue;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;
        }
        /// <summary>
        ///     Show Listable or Changable List with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="allowNewValue">Changable or Editable</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, string[] values, ref string selectedValue, bool allowNewValue, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.ReadOnly = !allowNewValue;
            ib.Icon = icon;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;
        }

        /// <summary>
        ///     Show Selectable List Box
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Index</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryListBox(string title, string promptText, string[] values, ref int selectedValue)
        {
            InputBox ib = new InputBox(title, promptText, values, selectedValue);
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            selectedValue = ib.SelectedIndex;
            return dr;

        }
        /// <summary>
        ///     Show Selectable List Box with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Index</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryListBox(string title, string promptText, string[] values, ref int selectedValue, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, values, selectedValue);
            ib.ReadOnly = true;
            ib.Icon = icon;
            DialogResult dr = ib.Show();
            selectedValue = ib.SelectedIndex;
            return dr;

        }
        /// <summary>
        ///     Show Selectable List Box
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryListBox(string title, string promptText, string[] values, ref string selectedValue)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.ReadOnly = true;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;

        }
        /// <summary>
        ///     Show Selectable List Box with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryListBox(string title, string promptText, string[] values, ref string selectedValue, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.ReadOnly = true;
            ib.Icon = icon;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;

        }
        /// <summary>
        ///     Show Changable List Box 
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="InputMaskOrRegex">Input Mask or Regex for Value
        /// <para>If text is Input Mask it starts from M. If text is Regex it starts from R.</para>
        /// <para>Input Mask symbols: 0 - digits; 9 - digits and spaces; # - digits, spaces, +, -</para>
        /// <para>  L - letter; ? - letters if need; A - letter or digit; . - decimal separator;</para>
        /// <para>  , - space for digits; / - date separator; $ - currency symbol</para>
        /// <para>Regex: http://regexstorm.net/reference</para>
        /// </param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryListBox(string title, string promptText, string[] values, ref string selectedValue, string InputMaskOrRegex)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.InputMaskOrRegex = InputMaskOrRegex;
            ib.ReadOnly = false;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;
        }
        /// <summary>
        ///     Show Changable List Box with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="InputMaskOrRegex">Input Mask or Regex for Value
        /// <para>If text is Input Mask it starts from M. If text is Regex it starts from R.</para>
        /// <para>Input Mask symbols: 0 - digits; 9 - digits and spaces; # - digits, spaces, +, -</para>
        /// <para>  L - letter; ? - letters if need; A - letter or digit; . - decimal separator;</para>
        /// <para>  , - space for digits; / - date separator; $ - currency symbol</para>
        /// <para>Regex: http://regexstorm.net/reference</para>
        /// </param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryListBox(string title, string promptText, string[] values, ref string selectedValue, string InputMaskOrRegex, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.InputMaskOrRegex = InputMaskOrRegex;
            ib.ReadOnly = false;
            ib.Icon = icon;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;
        }
        /// <summary>
        ///      Show Listable or Changable List
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="allowNewValue">Changable or Editable</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryListBox(string title, string promptText, string[] values, ref string selectedValue, bool allowNewValue)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.ReadOnly = !allowNewValue;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;
        }
        /// <summary>
        ///     Show Listable or Changable List with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="values">List Values</param>
        /// <param name="selectedValue">Selected Value Text</param>
        /// <param name="allowNewValue">Changable or Editable</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryListBox(string title, string promptText, string[] values, ref string selectedValue, bool allowNewValue, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, values);
            ib.ReadOnly = !allowNewValue;
            ib.Icon = icon;
            ib.Value = selectedValue;
            DialogResult dr = ib.Show();
            selectedValue = ib.Value;
            return dr;
        }

        /// <summary>
        ///     Show Two-List DropBox
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">boolean value</param>
        /// <param name="textFalse">Text for False Value</param>
        /// <param name="textTrue">Text for True Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref bool value, string textFalse, string textTrue)
        {
            InputBox ib = new InputBox(title, promptText, new string[] { textFalse, textTrue }, value ? 1 : 0);
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            value = ib.SelectedIndex == 1;
            return dr;
        }
        /// <summary>
        ///     Show Two-List DropBox with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">boolean value</param>
        /// <param name="textFalse">Text for False Value</param>
        /// <param name="textTrue">Text for True Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref bool value, string textFalse, string textTrue, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, new string[] { textFalse, textTrue }, value ? 1 : 0);
            ib.Icon = icon;
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            value = ib.SelectedIndex == 1;
            return dr;
        }

        /// <summary>
        ///     Show ReadOnly Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, float value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            return dr;

        }
        /// <summary>
        ///     Show ReadOnly Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, float value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.Icon = icon;
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref float value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.InputMaskOrRegex = "^(([+-]?)|([+-]?[0-9]{1,}[.]?[0-9]{0,})|([+-]?[.][0-9]{0,}))$";
            DialogResult dr = ib.Show();
            if ((ib.Value == "-") || (ib.Value == "."))
                value = 0;
            else
                value = float.Parse(ib.Value, System.Globalization.CultureInfo.InvariantCulture);
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref float value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.InputMaskOrRegex = "^(([+-]?)|([+-]?[0-9]{1,}[.]?[0-9]{0,})|([+-]?[.][0-9]{0,}))$";
            ib.Icon = icon;
            DialogResult dr = ib.Show();
            if ((ib.Value == "-") || (ib.Value == "."))
                value = 0;
            else
                value = float.Parse(ib.Value, System.Globalization.CultureInfo.InvariantCulture);
            return dr;
        }

        /// <summary>
        ///     Show ReadOnly Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, float value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            return dr;

        }
        /// <summary>
        ///     Show ReadOnly Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, float value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.Icon = icon;
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, ref float value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.InputMaskOrRegex = "^(([+-]?)|([+-]?[0-9]{1,}[.]?[0-9]{0,})|([+-]?[.][0-9]{0,}))$";
            DialogResult dr = ib.Show();
            if ((ib.Value == "-") || (ib.Value == "."))
                value = 0;
            else
                value = float.Parse(ib.Value, System.Globalization.CultureInfo.InvariantCulture);
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, ref float value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.InputMaskOrRegex = "^(([+-]?)|([+-]?[0-9]{1,}[.]?[0-9]{0,})|([+-]?[.][0-9]{0,}))$";
            ib.Icon = icon;
            DialogResult dr = ib.Show();
            if ((ib.Value == "-") || (ib.Value == "."))
                value = 0;
            else
                value = float.Parse(ib.Value, System.Globalization.CultureInfo.InvariantCulture);
            return dr;
        }

        /// <summary>
        ///     Show ReadOnly Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, double value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            return dr;

        }
        /// <summary>
        ///     Show ReadOnly Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, double value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.Icon = icon;
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref double value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.InputMaskOrRegex = "^(([+-]?)|([+-]?[0-9]{1,}[.]?[0-9]{0,})|([+-]?[.][0-9]{0,}))$";
            DialogResult dr = ib.Show();
            if ((ib.Value == "-") || (ib.Value == "."))
                value = 0;
            else
                value = double.Parse(ib.Value, System.Globalization.CultureInfo.InvariantCulture);
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, ref double value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.InputMaskOrRegex = "^(([+-]?)|([+-]?[0-9]{1,}[.]?[0-9]{0,})|([+-]?[.][0-9]{0,}))$";
            ib.Icon = icon;
            DialogResult dr = ib.Show();
            if ((ib.Value == "-") || (ib.Value == "."))
                value = 0;
            else
                value = double.Parse(ib.Value, System.Globalization.CultureInfo.InvariantCulture);
            return dr;
        }

        /// <summary>
        ///     Show ReadOnly Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, double value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            return dr;

        }
        /// <summary>
        ///     Show ReadOnly Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, double value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.Icon = icon;
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, ref double value)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.InputMaskOrRegex = "^(([+-]?)|([+-]?[0-9]{1,}[.]?[0-9]{0,})|([+-]?[.][0-9]{0,}))$";
            DialogResult dr = ib.Show();
            if ((ib.Value == "-") || (ib.Value == "."))
                value = 0;
            else
                value = double.Parse(ib.Value, System.Globalization.CultureInfo.InvariantCulture);
            return dr;

        }
        /// <summary>
        ///     Show Editable Input Box Dialog with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="value">Parameter Value</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryNumberBox(string title, string promptText, ref double value, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ib.InputMaskOrRegex = "^(([+-]?)|([+-]?[0-9]{1,}[.]?[0-9]{0,})|([+-]?[.][0-9]{0,}))$";
            ib.Icon = icon;
            DialogResult dr = ib.Show();
            if ((ib.Value == "-") || (ib.Value == "."))
                value = 0;
            else
                value = double.Parse(ib.Value, System.Globalization.CultureInfo.InvariantCulture);
            return dr;
        }

        /// <summary>
        ///     Show Listable Input Box Dialog for typeof(Enum)
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="enumType">Type of Enum</param>
        /// <param name="selectedValue">Selected Value</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, Type enumType, ref object selectedValue)
        {
            Array vls = Enum.GetValues((enumType));

            List<string> vals = new List<string>();
            foreach (string element in Enum.GetNames(enumType))
                vals.Add(element);

            InputBox ib = new InputBox(title, promptText, vals.ToArray());
            ib.Value = selectedValue.ToString();
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            selectedValue = vls.GetValue(ib.SelectedIndex);
            return dr;
        }
        /// <summary>
        ///     Show Listable Input Box Dialog for typeof(Enum)
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="enumType">Type of Enum</param>
        /// <param name="selectedValue">Selected Value</param>
        /// /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult Show(string title, string promptText, Type enumType, ref object selectedValue, Bitmap icon)
        {
            Array vls = Enum.GetValues((enumType));

            List<string> vals = new List<string>();
            foreach (string element in Enum.GetNames(enumType))
                vals.Add(element);

            InputBox ib = new InputBox(title, promptText, vals.ToArray());
            ib.Icon = icon;
            ib.Value = selectedValue.ToString();
            ib.ReadOnly = true;
            DialogResult dr = ib.Show();
            selectedValue = vls.GetValue(ib.SelectedIndex);
            return dr;
        }

        /// <summary>
        ///     Show Editable Directory Input Box With Browse Button
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="path">DIrectory</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryDirectoryBox(string title, string promptText, ref string path)
        {
            InputBox ib = new InputBox(title, promptText, path);
            DialogResult dr = ib.ShowSelectDir();
            path = ib.Value;
            return dr;
        }
        /// <summary>
        ///     Show Editable Directory Input Box With Browse Button
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="path">DIrectory</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryDirectoryBox(string title, string promptText, ref string path, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, path);
            ib.Icon = icon;
            DialogResult dr = ib.ShowSelectDir();
            path = ib.Value;
            return dr;
        }

        /// <summary>
        ///     Show Editable File Input Box
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="file">File Path</param>
        /// <param name="filter">OpenFileDialog Filter</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryFileBox(string title, string promptText, ref string file, string filter)
        {
            InputBox ib = new InputBox(title, promptText, file);
            DialogResult dr = ib.ShowSelectFile(filter);
            file = ib.Value;
            return dr;
        }

        /// <summary>
        ///     Show Editable File Input Box with Icon
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="file">File Path</param>
        /// <param name="filter">OpenFileDialog Filter</param>
        /// <param name="icon">Image Icon</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryFileBox(string title, string promptText, ref string file, string filter, Bitmap icon)
        {
            InputBox ib = new InputBox(title, promptText, file);
            DialogResult dr = ib.ShowSelectFile(filter);
            file = ib.Value;
            return dr;
        }

        /// <summary>
        ///     Show Editable Color Box
        /// </summary>
        /// <param name="title">Dialog Window Title</param>
        /// <param name="promptText">Parameter Prompt Text</param>
        /// <param name="color">Color</param>
        /// <returns>DialogResult</returns>
        public static DialogResult QueryColorBox(string title, string promptText, ref Color color)
        {
            InputBox ib = new InputBox(title, promptText);
            DialogResult dr = ib.ShowSelectColor(ref color);
            return dr;
        }

        private static String HexConverter(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private static Color RGBConverter(string hex)
        {
            Color rtn = Color.Black;
            try
            {
                return Color.FromArgb(
                    int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                    int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber));
            }
            catch { };

            return rtn;
        }

        private static TextBox query_info_box = null;
        public static DialogResult QueryInfoBox(string title, string topText, string bottomText, string mainText)
        {
            return QueryInfoBox(title, topText, bottomText, mainText, true, false, MessageBoxButtons.OK, true);
        }
        public static DialogResult QueryInfoBox(string title, string topText, string bottomText, string mainText, bool readOnly, bool allowNewLoadSave, MessageBoxButtons buttons, bool closeButton)
        {
            Form form = closeButton ? new Form() : new InputBoxForm();
            form.DialogResult = DialogResult.Cancel;
            form.ShowInTaskbar = pShowInTaskBar;
            form.Text = title;
            form.ClientSize = new Size(defWidth, 107);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ClientSize = new Size(500, 400);

            Panel panel1 = new Panel();
            Label label1 = new Label();
            Panel panel2 = new Panel();
            Label label2 = new Label();
            Panel panel3 = new Panel();
            Panel panel4 = new Panel();
            TextBox textBox1 = new TextBox();
            Panel panel5 = new Panel();
            Panel panel6 = new Panel();
            Button button4 = new Button();
            Button button3 = new Button();
            Button button5 = new Button();

            int btnCount = 1;
            if ((buttons == MessageBoxButtons.OKCancel) || (buttons == MessageBoxButtons.YesNo) || (buttons == MessageBoxButtons.RetryCancel)) btnCount = 2;
            if ((buttons == MessageBoxButtons.AbortRetryIgnore) || (buttons == MessageBoxButtons.YesNoCancel)) btnCount = 3;
            Button[] Buttons = new Button[btnCount];
            for (int i = 0; i < btnCount; i++)
            {
                Buttons[i] = new Button();
                Buttons[i].Text = pOk_Text;
                if (i == 0)
                {
                    if (buttons == MessageBoxButtons.OK) { Buttons[i].Text = pOk_Text; Buttons[i].DialogResult = DialogResult.OK; };
                    if (buttons == MessageBoxButtons.OKCancel) { Buttons[i].Text = pOk_Text; Buttons[i].DialogResult = DialogResult.OK; };
                    if (buttons == MessageBoxButtons.AbortRetryIgnore) { Buttons[i].Text = pOk_Abort; Buttons[i].DialogResult = DialogResult.Abort; };
                    if (buttons == MessageBoxButtons.YesNoCancel) { Buttons[i].Text = pOk_Yes; Buttons[i].DialogResult = DialogResult.Yes; };
                    if (buttons == MessageBoxButtons.YesNo) { Buttons[i].Text = pOk_Yes; Buttons[i].DialogResult = DialogResult.Yes; };
                    if (buttons == MessageBoxButtons.RetryCancel) { Buttons[i].Text = pOk_Retry; Buttons[i].DialogResult = DialogResult.Retry; };
                };
                if (i == 1)
                {
                    if (buttons == MessageBoxButtons.OKCancel) { Buttons[i].Text = pCancel_Text; Buttons[i].DialogResult = DialogResult.Cancel; };
                    if (buttons == MessageBoxButtons.AbortRetryIgnore) { Buttons[i].Text = pOk_Retry; Buttons[i].DialogResult = DialogResult.Retry; };
                    if (buttons == MessageBoxButtons.YesNoCancel) { Buttons[i].Text = pOk_No; Buttons[i].DialogResult = DialogResult.No; };
                    if (buttons == MessageBoxButtons.YesNo) { Buttons[i].Text = pOk_No; Buttons[i].DialogResult = DialogResult.No; };
                    if (buttons == MessageBoxButtons.RetryCancel) { Buttons[i].Text = pCancel_Text; Buttons[i].DialogResult = DialogResult.Cancel; };
                };
                if (i == 2)
                {
                    if (buttons == MessageBoxButtons.AbortRetryIgnore) { Buttons[i].Text = pOk_Ignore; Buttons[i].DialogResult = DialogResult.Ignore; };
                    if (buttons == MessageBoxButtons.YesNoCancel) { Buttons[i].Text = pCancel_Text; Buttons[i].DialogResult = DialogResult.Cancel; };
                };
                Buttons[i].BackColor = System.Drawing.SystemColors.Control;
                Buttons[i].Location = new System.Drawing.Point(9 + 81 * i, 10);
                Buttons[i].Size = new System.Drawing.Size(75, 23);
            };
            panel3.Controls.AddRange(Buttons);

            panel1.BackColor = System.Drawing.SystemColors.Window;
            panel1.Controls.Add(label1);
            panel1.Dock = System.Windows.Forms.DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Size = new System.Drawing.Size(578, 45);
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 14);
            label1.Size = new System.Drawing.Size(174, 26);
            label1.Text = topText;
            panel2.BackColor = System.Drawing.SystemColors.Window;
            panel2.Controls.Add(label2);
            panel2.Controls.Add(panel3);
            panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel2.Location = new System.Drawing.Point(0, 432);
            panel2.Size = new System.Drawing.Size(578, 45);
            panel3.Dock = System.Windows.Forms.DockStyle.Right;
            panel3.Location = new System.Drawing.Point(400, 0);
            panel3.Size = new System.Drawing.Size(34 + 81 * btnCount, 45);
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 16);
            label2.Size = new System.Drawing.Size(12, 13);
            label2.Text = bottomText;
            panel4.Controls.Add(textBox1);
            panel4.Controls.Add(panel6);
            panel4.Controls.Add(panel5);
            panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            panel4.Location = new System.Drawing.Point(0, 135);
            panel4.Size = new System.Drawing.Size(578, 297);
            panel5.BackColor = System.Drawing.SystemColors.Window;
            panel5.Dock = System.Windows.Forms.DockStyle.Left;
            panel5.Location = new System.Drawing.Point(0, 0);
            panel5.Size = new System.Drawing.Size(34, 297);
            textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox1.Location = new System.Drawing.Point(34, 0);
            textBox1.Multiline = true;
            textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textBox1.Size = new System.Drawing.Size(510, 297);
            textBox1.Text = mainText;
            textBox1.SelectionStart = 0;
            Color bc = textBox1.BackColor;
            textBox1.ReadOnly = readOnly;
            textBox1.BackColor = bc;
            panel6.BackColor = System.Drawing.SystemColors.Window;
            panel6.Controls.Add(button5);
            panel6.Controls.Add(button4);
            panel6.Controls.Add(button3);
            panel6.Dock = System.Windows.Forms.DockStyle.Right;
            panel6.Location = new System.Drawing.Point(544, 0);
            panel6.Size = new System.Drawing.Size(34, 297);
            button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button4.Location = new System.Drawing.Point(6, 54);
            button4.Size = new System.Drawing.Size(22, 23);
            button4.Text = "S";
            button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button3.Location = new System.Drawing.Point(6, 30);
            button3.Size = new System.Drawing.Size(22, 23);
            button3.Text = readOnly ? "" : "L";
            button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button5.Location = new System.Drawing.Point(6, 6);
            button5.Size = new System.Drawing.Size(22, 23);
            button5.TabIndex = 3;
            button5.Text = readOnly ? "" : "N";
            if (!readOnly)
            {
                button3.Click += new EventHandler(buttonLNS_Click);
                button4.Click += new EventHandler(buttonLNS_Click);
                button5.Click += new EventHandler(buttonLNS_Click);
            };
            form.Controls.Add(panel4);
            form.Controls.Add(panel1);
            form.Controls.Add(panel2);
            form.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            {
                //button1.Text = fullview ? pCancel_Text : pOk_Text;
                //button2.Visible = fullview;
                button3.Visible = allowNewLoadSave;
                button4.Visible = allowNewLoadSave;
                button5.Visible = allowNewLoadSave;

                button3.Enabled = !readOnly;
                button4.Enabled = !String.IsNullOrEmpty(mainText);
                button5.Enabled = !readOnly;
            };

            query_info_box = textBox1;
            DialogResult dr = form.ShowDialog();
            query_info_box = null;
            form.Dispose();
            return dr;
        }

        private static void buttonLNS_Click(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                if (query_info_box == null) return;
                Button btn = (Button)sender;
                if (btn.Text == "N")
                    query_info_box.Text = "";
                if (btn.Text == "L")
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Filter = "Text Files (*.txt)|*.txt|All Types (*.*)|*.*";
                    ofd.DefaultExt = ".txt";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
                            StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
                            query_info_box.Text = sr.ReadToEnd();
                            sr.Close();
                            fs.Close();
                        }
                        catch { };
                    };
                    ofd.Dispose();
                };
                if (btn.Text == "S")
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "Text Files (*.txt)|*.txt|All Types (*.*)|*.*";
                    sfd.DefaultExt = ".txt";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding(1251));
                            sw.Write(query_info_box.Text);
                            sw.Close();
                            fs.Close();
                        }
                        catch { };
                    };
                    sfd.Dispose();
                };
            };
        }
    }

    public class InputBoxForm : Form
    {
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }
    }

    public class ComboIcons : ComboBox
    {
        public ComboIcons()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            if (e.Index >= 0 && e.Index < Items.Count)
            {
                DropDownItem item = (DropDownItem)Items[e.Index];
                e.Graphics.DrawImage(item.Image, e.Bounds.Left, e.Bounds.Top);
                e.Graphics.DrawString(item.Value, e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + item.Image.Width + 1, e.Bounds.Top + 2);
            };
            base.OnDrawItem(e);
        }
    }

    public class DropDownItem
    {
        private string value;
        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        private Image img;
        public Image Image
        {
            get { return this.img; }
            set { this.img = value; }
        }

        public DropDownItem() : this("", Color.Black) { }

        public DropDownItem(string val) : this(val, Color.Black) { }

        public DropDownItem(string val, Color color)
        {
            this.value = val;
            this.img = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(this.img))
            {
                using (Brush b = new SolidBrush(color))
                {
                    g.DrawRectangle(Pens.White, 0, 0, Image.Width, Image.Height);
                    g.FillRectangle(b, 1, 1, Image.Width - 1, Image.Height - 1);
                };
            };
        }

        public DropDownItem(string val, Image im)
        {
            this.value = val;
            this.img = im;
        }

        public override string ToString()
        {
            return value;
        }
    }
}
