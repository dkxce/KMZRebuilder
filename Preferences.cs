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

        private void LoadDefaults()
        {
            if (Properties == null)
            {
                Properties = new List<Property>();
                this["gpi_localization"] = "EN";
            };
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
