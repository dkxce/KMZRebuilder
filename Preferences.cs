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
            if (Properties == null) Properties = new List<Property>();
            if ((Properties.Count > 0) && (String.IsNullOrEmpty(Properties[0].comm))) Properties = new List<Property>();
            if (!this.Contains("gpi_localization")) Properties.Add(new Property("gpi_localization", "EN", 0, "2-symbols string, Language, ISO-639 code", 0, 2));
            if (!this.Contains("gpireader_save_media")) Properties.Add(new Property("gpireader_save_media", "no", 1, "Save media from GPI"));
            if (!this.Contains("gpireader_poi_image_from_jpeg")) Properties.Add(new Property("gpireader_poi_image_from_jpeg", "no", 1, "If yes - POI image sets from JPEG, if no - from Bitmap"));
            if (!this.Contains("gpiwriter_format_version")) Properties.Add(new Property("gpiwriter_format_version", "1", 2, "GPI File Format Version (00 or 01)", 0, 1));
            if (!this.Contains("gpiwriter_set_descriptions")) Properties.Add(new Property("gpiwriter_set_descriptions", "yes", 1, "Save descriptions to GPI"));
            if (!this.Contains("gpiwriter_set_alerts")) Properties.Add(new Property("gpiwriter_set_alerts", "no", 1, "Save alerts to GPI"));
            if (!this.Contains("gpiwriter_default_alert_ison")) Properties.Add(new Property("gpiwriter_default_alert_ison", "yes", 1, "Set alerts default is on"));
            if (!this.Contains("gpiwriter_default_alert_type")) Properties.Add(new Property("gpiwriter_default_alert_type", "proximity", 0, "Default alert type: proximity, along_road, toure_guide", 0, 11));
            if (!this.Contains("gpiwriter_default_alert_sound")) Properties.Add(new Property("gpiwriter_default_alert_sound", "4", 2, "Default predefined alert sound: 0 - beep, 1 - tone, 2 - three beeps, 3 - silence, 4-plung, 5-plungplung", 0, 5));
            if (!this.Contains("gpiwriter_alert_help_file")) Properties.Add(new Property("gpiwriter_alert_help_file", "gpiwriter_alert_help.txt", 3));
            if (!this.Contains("gpiwriter_comaddcon_help_file")) Properties.Add(new Property("gpiwriter_comaddcon_help_file", "gpiwriter_comaddcon_help.txt", 3));
            if (!this.Contains("gpiwriter_image_max_side")) Properties.Add(new Property("gpiwriter_image_max_side", "22", 2, "Max Image width/height in pixels (16..48)", 16, 48));
            if (!this.Contains("gpiwriter_image_transp_color")) Properties.Add(new Property("gpiwriter_image_transp_color", "#FEFEFE", 0, "Transparent color in web format HEX: #FEFEFE", 0, 7));
            if (!this.Contains("gpiwriter_save_images_jpeg")) Properties.Add(new Property("gpiwriter_save_images_jpeg", "no", 1, "Save to each POI original image as jpeg (optional)"));
            if (!this.Contains("gpiwriter_save_only_local_lang")) Properties.Add(new Property("gpiwriter_save_only_local_lang", "no", 1, "Save text only in local language"));            
            if (!this.Contains("gpiwriter_alert_datetime_maxcount")) Properties.Add(new Property("gpiwriter_alert_datetime_maxcount", "16", 2, "Max Alert DateTime Triggers Count (1..32)", 1, 32));
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
            [XmlAttribute]
            public byte cat; // 0 - string, 1 - boolean, 2 - number, 3 - disable
            [XmlAttribute]
            public string comm;
            [XmlAttribute]
            public ushort min = ushort.MinValue;
            [XmlAttribute]
            public ushort max = ushort.MaxValue;
            [XmlText]
            public string value;

            public Property() { }
            public Property(string name) { this.name = name; }
            public Property(string name, string value) { this.name = name; this.value = value; }
            public Property(string name, string value, byte cat) { this.name = name; this.value = value; this.cat = cat; }
            public Property(string name, string value, byte cat, string comm) { this.name = name; this.value = value; this.cat = cat; this.comm = comm; }
            public Property(string name, string value, byte cat, string comm, byte min, byte max) { this.name = name; this.value = value; this.cat = cat; this.comm = comm; this.min = min; this.max = max; }

            public override string ToString()
            {
                return String.Format("{0} = {1}", name, value);
            }
        }

        public void ShowChangeDialog()
        {
            LoadDefaults();
            Form form = new Form();
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ShowIcon = false;
            form.ShowInTaskbar = false;
            form.Width = 400;
            form.Height = 420;
            form.Text = "Preferences";
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            Label lab = new Label();
            form.Controls.Add(lab);
            lab.Text = "Double click on item or press Space/Enter to edit/change value:";
            lab.AutoSize = true;
            lab.Left = 8;
            lab.Top = 5;
            Label hel = new Label();
            form.Controls.Add(hel);
            hel.Text = "-- item is not selected --";
            hel.Width = form.Width - 26;
            hel.Height = 32;
            hel.Left = 8;
            hel.Top = 25 + form.Height - 116;
            ListBox lb = new ListBox();
            form.Controls.Add(lb);
            lb.Width = form.Width - 26;
            lb.Left = 10;
            lb.Top = 25;
            lb.Height = form.Height - 110;
            lb.BorderStyle = BorderStyle.FixedSingle;
            if (Properties == null) LoadDefaults();
            foreach (Property prop in Properties) lb.Items.Add(prop);
            lb.DoubleClick += (delegate(object sender, EventArgs e) { OnChangeItem(lb); });
            lb.KeyPress += (delegate(object sender, KeyPressEventArgs e) { if ((e.KeyChar == (char)32) || (e.KeyChar == (char)13)) OnChangeItem(lb); });
            lb.SelectedIndexChanged += (delegate(object sender, EventArgs e) { if (lb.SelectedIndex < 0) hel.Text = "-- item is not selected --"; else hel.Text = ((Property)lb.SelectedItem).comm; });
            Button okbtn = new Button();
            form.Controls.Add(okbtn);
            okbtn.Left = form.Width / 2 - okbtn.Width / 2;
            okbtn.Top = lb.Top + lb.Height + 26;
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
                Property p = (Property)lb.SelectedItem;
                if (p.cat == 3) return;
                string caption = "Edit value";
                string nval = p.value;
                if (p.cat == 1) // boolean value
                {
                    int ifl = 0;
                    List<string> yn = new List<string>(new string[]{"no","yes"});
                    if (nval == "yes") ifl = 1;
                    if (InputBox.Show(caption, p.name + ":", yn.ToArray(), ref ifl) == DialogResult.OK)
                    {                        
                        p.value = yn[ifl];
                        this[p.name] = p.value;
                        lb.Items[si] = p;
                    };
                }
                else if (p.cat == 2) // number value
                {
                    int ifl = int.Parse(nval);
                    if (InputBox.Show(caption, p.name + ":", ref ifl, p.min, p.max) == DialogResult.OK)
                    {
                        p.value = ifl.ToString();
                        this[p.name] = p.value;
                        lb.Items[si] = p;
                    };
                }
                else // text value
                {
                    if (InputBox.Show(caption, p.name + ":", ref nval,"R^.{"+p.min+","+p.max+"}$") == DialogResult.OK)
                    {
                        p.value = nval.Trim();
                        this[p.name] = p.value;
                        lb.Items[si] = p;
                    };
                };
            };
        }
    }
}
