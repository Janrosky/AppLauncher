using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AppLauncher
{
    internal static class Program
    {
        // Enable high-DPI awareness
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
