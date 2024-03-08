using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Re_Hippocamp.Misc
{
    public static class MHippocamp
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void show(string n)
        {
            Process[] processes = Process.GetProcessesByName(n.Replace(".exe", ""));
            ShowWindow(processes[0].MainWindowHandle, 9);
            ShowWindow(processes[0].MainWindowHandle, 10);
            ShowWindow(processes[0].MainWindowHandle, 1);
            ShowWindow(processes[0].MainWindowHandle, 5);
            SetForegroundWindow(processes[0].MainWindowHandle);
        }
    }
}
