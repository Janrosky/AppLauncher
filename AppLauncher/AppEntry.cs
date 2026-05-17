using System.Drawing;

namespace AppLauncher
{
    public class AppEntry
    {
        public string Name { get; set; } = "";
        public string InstallPath { get; set; } = "";
        public string? Publisher { get; set; }
        public string? Version { get; set; }
        public string? UninstallString { get; set; }
        public long SizeBytes { get; set; }
        public Icon? Icon { get; set; }
        public bool IsSystemApp { get; set; }
    }
}