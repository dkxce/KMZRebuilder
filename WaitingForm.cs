using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace KMZRebuilder
{
    public class WaitingBoxForm
    {
        private class WaitingForm : Form
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

            public WaitingForm()
            {
                this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
                this.label1 = new System.Windows.Forms.Label();
                this.progressBar1 = new System.Windows.Forms.ProgressBar();
                this.tableLayoutPanel1.SuspendLayout();
                this.SuspendLayout();
                // 
                // tableLayoutPanel1
                // 
                this.tableLayoutPanel1.ColumnCount = 1;
                this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
                this.tableLayoutPanel1.Controls.Add(this.progressBar1, 0, 0);
                this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
                this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
                this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
                this.tableLayoutPanel1.Name = "tableLayoutPanel1";
                this.tableLayoutPanel1.RowCount = 3;
                this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
                this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
                this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
                this.tableLayoutPanel1.Size = new System.Drawing.Size(492, 145);
                this.tableLayoutPanel1.TabIndex = 0;
                // 
                // label1
                // 
                this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
                this.label1.AutoSize = true;
                this.label1.Location = new System.Drawing.Point(209, 0);
                this.label1.Name = "label1";
                this.label1.Size = new System.Drawing.Size(73, 13);
                this.label1.TabIndex = 3;
                this.label1.Text = "Please Wait...";
                // 
                // progressBar1
                // 
                this.progressBar1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
                this.progressBar1.Location = new System.Drawing.Point(2, 2);
                this.progressBar1.Name = "progressBar1";
                this.progressBar1.Size = new System.Drawing.Size(490, 23);
                this.progressBar1.TabIndex = 2;
                // 
                // WaitingForm
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.StartPosition = FormStartPosition.CenterParent;
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(492, 55);
                this.Controls.Add(this.tableLayoutPanel1);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
                this.Name = "WaitingForm";
                this.Text = "Working in the background";
                this.Load += new System.EventHandler(this.WaitingForm_Load);
                this.tableLayoutPanel1.ResumeLayout(false);
                this.tableLayoutPanel1.PerformLayout();
                this.ShowInTaskbar = false;
                this.ResumeLayout(false);
            }

            private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
            private System.Windows.Forms.ProgressBar progressBar1;
            private System.Windows.Forms.Label label1;

            private void WaitingForm_Load(object sender, EventArgs e)
            {
                progressBar1.Style = ProgressBarStyle.Marquee;
                try
                {
                    this.BringToFront();
                    if(this.StartPosition != FormStartPosition.Manual)
                        this.CenterToScreen();
                }
                catch { };
            }

            internal string Label
            {
                get
                {
                    return label1.Text;
                }
                set
                {
                    label1.Text = value;
                }
            }
        }        
        
        private Thread showThread;
        private bool showForm = false;
        private string formCaption = "Working..";
        private string formText = "Please wait...";
        private Point parentCenter;
        private Form parent;
        private IntPtr parentWnd = IntPtr.Zero;
        private bool isModal = true;
                                
        public WaitingBoxForm(){}

        public WaitingBoxForm(Form parent) 
        {
            if (parent != null)
            {
                this.parentWnd = parent.Handle;
                this.parent = parent;
                this.parentCenter = new Point(parent.DesktopLocation.X + (int)parent.Width / 2, parent.DesktopLocation.Y + (int)parent.Height / 2);
            };    
        }

        public WaitingBoxForm(string Caption, string Text)
        {
            this.formCaption = Caption;
            this.formText = Text;
        }

        public WaitingBoxForm(string Caption, string Text, Form parent)
        {
            this.formCaption = Caption;
            this.formText = Text;
            if (parent != null)
            {
                this.parentWnd = parent.Handle;
                this.parent = parent;
                this.parentCenter = new Point(parent.DesktopLocation.X + (int)parent.Width / 2, parent.DesktopLocation.Y + (int)parent.Height / 2);
            };
        }

        public string Caption
        {
            get
            {
                return this.formCaption;
            }
            set
            {
                this.formCaption = value;
            }
        }

        public string Text
        {
            get
            {
                return this.formText;
            }
            set
            {
                this.formText = value;
            }
        }

        public bool Modal
        {
            get
            {
                return isModal;
            }
            set
            {
                isModal = value;
            }
        }
        
        public bool Activated
        {
            get { return showForm; }
        }

        private bool ApplicationIsActive(out IntPtr foregroundWindow)
        {
            foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero) return false;

            int foregroundWindowProcessID;
            GetWindowThreadProcessId(foregroundWindow, out foregroundWindowProcessID);
            
            return foregroundWindowProcessID == System.Diagnostics.Process.GetCurrentProcess().Id;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern UInt32 GetWindowLong(IntPtr hWnd, int index);

        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", ExactSpelling = true)]
        static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);

        private void ShowThread()
        {
            WaitingForm waitingform = new WaitingForm();
            waitingform.Text = this.formCaption;
            waitingform.Label = this.formText;
            IntPtr mainWindowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

            if (this.parentCenter != null)
            {
                waitingform.StartPosition = FormStartPosition.Manual;
                waitingform.Location = new Point(parentCenter.X - waitingform.Width / 2, parentCenter.Y - waitingform.Height / 2);
            };

            waitingform.Show();
            waitingform.Refresh();

            while (showForm)
            {
                Application.DoEvents();

                if (waitingform.Text != this.formCaption) waitingform.Text = this.formCaption;
                if (waitingform.Label != this.formText) waitingform.Label = this.formText;
                waitingform.Refresh();

                IntPtr topw;
                if (isModal && ApplicationIsActive(out topw))
                {
                    bool skip = topw == waitingform.Handle;
                    if ((!skip) && (topw != mainWindowHandle))
                    {                                                
                        string clnm = GetWindowClass(topw);
                        if (clnm.StartsWith("#")) skip = true; // Dialog window on top

                        if (!skip)
                        {
                            IntPtr rHwd = GetAncestor(topw, 3); // Top Window through childs                     
                            if ((mainWindowHandle != IntPtr.Zero) && (rHwd == IntPtr.Zero)) skip = true; // Not Application, it may be .Net Error Window
                            if ((mainWindowHandle != IntPtr.Zero) && (rHwd == topw)) skip = true; // Not Our Application, it may be .Net Error Window
                        };

                        if (!skip)
                        {
                            IntPtr pHwd = GetParent(topw);
                            if ((pHwd == IntPtr.Zero) && (parentWnd != IntPtr.Zero) && (topw != parentWnd) && (topw != mainWindowHandle)
                                && (GetWindowText(topw) == waitingform.Text))
                                    skip = true; // .Net Standard Error Window
                        };
                    };

                    if (!skip)
                    {
                        waitingform.BringToFront();
                        waitingform.Activate();
                        waitingform.Focus();
                        waitingform.Refresh();
                    };
                };

                System.Threading.Thread.Sleep(50);
            };
            waitingform.Close();
            waitingform.Dispose();
            waitingform = null;
        }

        private static string GetWindowText(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder text = new StringBuilder(length + 1);
            GetWindowText(hWnd, text, text.Capacity);
            return text.ToString();
        }

        private static string GetWindowClass(IntPtr hWnd)
        {
            StringBuilder text = new StringBuilder(256);
            GetClassName(hWnd, text, text.Capacity);
            return text.ToString();
        }

        private static bool GetWindowIsTool(IntPtr hWnd)
        {                                  
            // https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles
            int GWL_STYLE = -16;  
            uint WS_SYSMENU = 0x00080000;
            uint WS_BORDER = 0x00800000;
            uint WS_CHILD = 0x40000000;
            uint WS_POPUP = 0x80000000;
            uint WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;

            // https://docs.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles
            int GWL_EXSTYLE = -20;
            uint WS_EX_TOOLWINDOW = 0x00000080;

            uint res = GetWindowLong(hWnd, GWL_EXSTYLE);
            return (res & WS_EX_TOOLWINDOW) > 0;
        }

        public void Show()
        {
            if (this.showThread != null)
            {
                this.showForm = false;
                this.showThread.Join();
            };
            if(this.parent != null) this.parentCenter = new Point(parent.DesktopLocation.X + (int)parent.Width / 2, parent.DesktopLocation.Y + (int)parent.Height / 2);
            this.showThread = new Thread(new ThreadStart(ShowThread));            
            showForm = true;
            showThread.Start();
        }

        public void Show(Form parent)
        {
            this.parent = parent;
            if (parent != null)
                this.parentCenter = new Point(parent.DesktopLocation.X + (int)parent.Width / 2, parent.DesktopLocation.Y + (int)parent.Height / 2);
            else
                this.parentCenter = Point.Empty;
            this.Show();
        }

        public void Show(string Caption, string Text)
        {
            this.formCaption = Caption;
            this.formText = Text;
            if(!this.Activated)
                this.Show();
        }

        public void Show(string Caption, string Text, Form parent)
        {
            this.formCaption = Caption;
            this.formText = Text;
            this.parent = parent;
            if (parent != null)
                this.parentCenter = new Point(parent.DesktopLocation.X + (int)parent.Width / 2, parent.DesktopLocation.Y + (int)parent.Height / 2);
            else
                this.parentCenter = Point.Empty;
            this.Show();
        }

        public void Hide()
        {
            this.showForm = false;
            if(this.showThread != null) this.showThread.Join();
            this.showThread = null;
            if (this.parent != null)
            {
                this.parent.BringToFront();
                this.parent.Activate();
                this.parent.Focus();
                this.parent.Refresh();
            };            
        }

    }
}