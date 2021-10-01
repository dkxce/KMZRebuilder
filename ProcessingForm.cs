using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KMZViewer
{
    public partial class RunProcStdOutForm : Form
    {
        private System.Diagnostics.Process proc;
        private string _stdText = "";
        private bool run_proc = false;
     
        public RunProcStdOutForm(string caption)
        {
            InitializeComponent();
            this.Text = caption;
            this.DialogResult = DialogResult.Cancel;
        }

        public void SetText(string text)
        {
            std.Text = text;
            std.SelectionStart = std.Text.Length;
        }

        public void Write(string text)
        {
            std.Text += text;
            std.SelectionStart = std.Text.Length;
        }

        public void WriteLine(string line)
        {
            std.Text += line + "\r\n";
            std.SelectionStart = std.Text.Length;
        }

        public void StdOutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            _stdText += e.Data + "\r\n";
            try
            {
                this.Invoke(new DoText(WriteLine), new object[] { e.Data });
            }
            catch { };
        }

        public string StdText
        {
            get
            {
                return _stdText;
            }
        }

        public DialogResult StartProcAndShowWhileRunning(System.Diagnostics.ProcessStartInfo psi)
        {
            proc = new System.Diagnostics.Process();
            proc.StartInfo = psi;
            proc.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(StdOutputDataReceived);
            proc.Start();
            proc.BeginOutputReadLine();
            this.run_proc = true;
            return this.ShowDialog();            
        }

        public delegate void DoText(string text);

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (run_proc)
            {
                if (proc.HasExited)
                {
                    try { _stdText = proc.StandardOutput.ReadToEnd(); } catch { };
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                //proc.WaitForExit();
            };
        }
    }
}