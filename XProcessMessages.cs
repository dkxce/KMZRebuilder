using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace KMZRebuilder
{
    public class ProcDataExchange
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(HandleRef hWnd);

        /// <summary>
        /// Handle used to send the message to all windows
        /// </summary>
        public static IntPtr HWND_BROADCAST = new IntPtr(0xffff);

        /// <summary>
        /// An application sends the WM_COPYDATA message to pass data to another application.
        /// </summary>
        public static uint WM_COPYDATA = 0x004A;

        /// <summary>
        /// Contains data to be passed to another application by the WM_COPYDATA message.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            /// <summary>
            /// User defined data to be passed to the receiving application.
            /// </summary>
            public IntPtr dwData;

            /// <summary>
            /// The size, in bytes, of the data pointed to by the lpData member.
            /// </summary>
            public int cbData;

            /// <summary>
            /// The data to be passed to the receiving application. This member can be IntPtr.Zero.
            /// </summary>
            public IntPtr lpData;

            public int DataType
            {
                get
                {
                    return (int)dwData;
                }
                set
                {
                    dwData = new IntPtr(value);
                }
            }

            public int DataLength
            {
                get
                {
                    return cbData;
                }
                set
                {
                    cbData = DataLength;
                }
            }

            public string DataUTF8String
            {
                get
                {
                    byte[] arr = new byte[this.cbData];
                    Marshal.Copy(this.lpData, arr, 0, arr.Length);
                    return System.Text.Encoding.UTF8.GetString(arr);
                }
                set
                {
                    byte[] arr = System.Text.Encoding.UTF8.GetBytes(value);
                    this.cbData = arr.Length;
                    this.lpData = Marshal.AllocHGlobal(arr.Length);
                    Marshal.Copy(arr, 0, this.lpData, arr.Length);

                }
            }

            public string DataString
            {
                get
                {
                    byte[] arr = new byte[this.cbData];
                    Marshal.Copy(this.lpData, arr, 0, arr.Length);
                    return System.Text.Encoding.GetEncoding(1251).GetString(arr);
                }
                set
                {
                    byte[] arr = System.Text.Encoding.GetEncoding(1251).GetBytes(value);
                    this.cbData = arr.Length;
                    this.lpData = Marshal.AllocHGlobal(arr.Length);
                    Marshal.Copy(arr, 0, this.lpData, arr.Length);

                }
            }

            public byte[] Data
            {
                get
                {
                    byte[] arr = new byte[this.cbData];
                    Marshal.Copy(this.lpData, arr, 0, arr.Length);
                    return arr;
                }
                set
                {
                    this.cbData = value.Length;
                    this.lpData = Marshal.AllocHGlobal(value.Length);
                    Marshal.Copy(value, 0, this.lpData, value.Length);

                }
            }
        }

        /// <summary>
        /// Sends the specified message to a window or windows.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
        /// If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level
        /// windows in the system.</param>
        /// <param name="Msg">The message to be sent.</param>
        /// <param name="wParam">Additional message-specific information.</param>
        /// <param name="lParam">Additional message-specific information.</param>
        /// <returns>The return value specifies the result of the message processing; 
        /// it depends on the message sent.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        public static void SendData(IntPtr destHandle, IntPtr srcHandle, int dataType, string data)
        {
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(data);

            COPYDATASTRUCT copyData = new COPYDATASTRUCT();
            copyData.dwData = new IntPtr(dataType);
            copyData.DataUTF8String = data;

            IntPtr ptrCopyData = Marshal.AllocCoTaskMem(Marshal.SizeOf(copyData));
            Marshal.StructureToPtr(copyData, ptrCopyData, false);

            SendMessage(destHandle, WM_COPYDATA, srcHandle, ptrCopyData);
        }

        public static void SendData(IntPtr destHandle, IntPtr srcHandle, int dataType, byte[] data)
        {
            COPYDATASTRUCT copyData = new COPYDATASTRUCT();
            copyData.dwData = new IntPtr(dataType);
            copyData.Data = data;

            IntPtr ptrCopyData = Marshal.AllocCoTaskMem(Marshal.SizeOf(copyData));
            Marshal.StructureToPtr(copyData, ptrCopyData, false);

            SendMessage(destHandle, WM_COPYDATA, srcHandle, ptrCopyData);
        }
    }
}