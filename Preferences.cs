using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace KMZRebuilder
{
    [Serializable]
    public class Preferences: XMLSaved<Preferences>
    {
        [XmlArray("configuration")]
        [XmlArrayItem("property")]
        public List<Property> Properties;

        [XmlIgnore]
        private bool DefaultsIsLoaded = false;

        [XmlIgnore]
        public string this[string name]
        {
            get
            {
                LoadDefaults();
                if (Properties.Count == 0) return "";
                foreach (Property prop in Properties)
                    if (prop.name == name)
                        return prop.value;
                return "";
            }
            set
            {
                LoadDefaults();
                foreach (Property prop in Properties)
                    if (prop.name == name)
                    {
                        prop.value = value;
                        return;
                    };
                Properties.Add(new Property(name, value));
            }
        }

        public bool GetBoolValue(string name)
        {
            LoadDefaults();
            if (Properties.Count == 0) return false;
            foreach (Property prop in Properties)
                if (prop.name == name)
                {
                    string pv = prop.value;
                    if (String.IsNullOrEmpty(pv)) return false;
                    pv = pv.ToLower();
                    return (pv == "1") || (pv == "yes") || (pv == "true");
                };
            return false;
        }

        private void LoadDefaults()
        {
            if (DefaultsIsLoaded) return;
            if (Properties == null)
            {
                Properties = new List<Property>();
                this["gpi_localization"] = "EN";
            };
            if (!this.Contains("gpireader_save_media")) Properties.Add(new Property("gpireader_save_media", "no"));
            if (!this.Contains("gpiwriter_set_descriptions")) Properties.Add(new Property("gpiwriter_set_descriptions", "yes"));
            if (!this.Contains("gpiwriter_set_alerts")) Properties.Add(new Property("gpiwriter_set_alerts", "no"));
            if (!this.Contains("gpiwriter_default_alert_ison")) Properties.Add(new Property("gpiwriter_default_alert_ison", "yes"));
            if (!this.Contains("gpiwriter_default_alert_type")) Properties.Add(new Property("gpiwriter_default_alert_type", "proximity"));
            if (!this.Contains("gpiwriter_default_alert_sound")) Properties.Add(new Property("gpiwriter_default_alert_sound", "4"));
            if (!this.Contains("gpiwriter_alert_help_file")) Properties.Add(new Property("gpiwriter_alert_help_file", "gpiwriter_alert_help.txt"));
            DefaultsIsLoaded = true;
        }

        private bool Contains(string name)
        {
            foreach (Property prop in Properties)
                if (prop.name == name) return true;
            return false;
        }

        public static Preferences Load()
        {
            string fName = KMZRebuilederForm.CurrentDirectory()+@"\KMZRebuilder.config";
            if (File.Exists(fName))
            {
                try { return Preferences.Load(fName); } catch { };
            };
            return new Preferences();
        }

        public void Save()
        {
            string fName = KMZRebuilederForm.CurrentDirectory()+@"\KMZRebuilder.config";
            this.LoadDefaults();
            try { Preferences.Save(fName, this); } catch { };
        }

        [Serializable]
        public class Property
        {
            [XmlAttribute]
            public string name;
            [XmlText]
            public string value;

            public Property() { }
            public Property(string name) { this.name = name; }
            public Property(string name, string value) { this.name = name; this.value = value; }

            public override string ToString()
            {
                return String.Format("{0}={1}", name, value);
            }
        }

        public void ShowChangeDialog()
        {
            Form form = new Form();
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ShowIcon = false;
            form.ShowInTaskbar = false;
            form.Width = 400;
            form.Height = 380;
            form.Text = "Preferences";
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            Label lab = new Label();
            form.Controls.Add(lab);
            lab.Text = "Double click on item or press Space to edit/change value:";
            lab.AutoSize = true;
            lab.Left = 8;
            lab.Top = 5;
            ListBox lb = new ListBox();
            form.Controls.Add(lb);
            lb.Width = form.Width - 26;
            lb.Left = 10;
            lb.Top = 25;
            lb.Height = form.Height - 90;
            lb.BorderStyle = BorderStyle.FixedSingle;
            foreach (Property prop in Properties) lb.Items.Add(prop);
            lb.DoubleClick += (delegate(object sender, EventArgs e) { OnChangeItem(lb); });
            lb.KeyPress += (delegate(object sender, KeyPressEventArgs e) { if (e.KeyChar == (char)32) OnChangeItem(lb); });
            Button okbtn = new Button();
            form.Controls.Add(okbtn);
            okbtn.Left = form.Width / 2 - okbtn.Width / 2;
            okbtn.Top = lb.Top + lb.Height + 6;
            okbtn.Text = "OK";
            okbtn.Click += (delegate(object sender, EventArgs e) { form.Close(); });
            form.ShowDialog();            
            form.Dispose();
        }

        private void OnChangeItem(ListBox lb)
        {
            int si = lb.SelectedIndex;
            if (si >= 0)
            {
                Property p = (Property)lb.Items[si];
                string nval = p.value;
                if (InputBox.Show("Edit value", p.name + ":", ref nval) == DialogResult.OK)
                {
                    p.value = nval.Trim();
                    this[p.name] = p.value;
                    lb.Items.RemoveAt(si);
                    lb.Items.Insert(si, p);
                    lb.SetSelected(si, true);
                };
            };
        }
    }
}
