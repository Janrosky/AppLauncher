using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppLauncher
{
    public class MainForm : Form
    {
        // ── Controls ──────────────────────────────────────────────────
        private Panel topBar = null!;
        private Panel filterBar = null!;
        private Panel statusBar = null!;
        private FlowLayoutPanel gridPanel = null!;
        private TextBox txtSearch = null!;
        private Label lblStatus = null!;
        private Label lblCount = null!;
        private Button btnAll = null!;
        private Button btnUser = null!;
        private Button btnSystem = null!;
        private Button btnSortSize = null!;

        // ── Data ──────────────────────────────────────────────────────
        private List<AppEntry> allApps = new();
        private string currentFilter = "all";
        private string currentSearch = "";
        private bool sortBySize = false;

        // ── Colors ────────────────────────────────────────────────────
        private static readonly Color BgBase = Color.FromArgb(13, 13, 18);
        private static readonly Color BgCard = Color.FromArgb(22, 22, 30);
        private static readonly Color BgCardHov = Color.FromArgb(32, 32, 44);
        private static readonly Color Accent1 = Color.FromArgb(82, 130, 255);
        private static readonly Color Accent2 = Color.FromArgb(130, 80, 255);
        private static readonly Color AccentRed = Color.FromArgb(220, 60, 60);
        private static readonly Color TextPrim = Color.FromArgb(230, 230, 245);
        private static readonly Color TextSec = Color.FromArgb(120, 120, 150);
        private static readonly Color TextSize = Color.FromArgb(100, 180, 120);
        private static readonly Color Border = Color.FromArgb(40, 40, 58);

        public MainForm()
        {
            BuildUI();
            LoadAppsAsync();
        }

        // ══════════════════════════════════════════════════════════════
        //  BUILD UI
        // ══════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            Text = "AppLauncher";
            Size = new Size(1280, 820);
            MinimumSize = new Size(900, 600);
            BackColor = BgBase;
            ForeColor = TextPrim;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9f);
            DoubleBuffered = true;

            BuildTopBar();
            BuildFilterBar();
            BuildGrid();
            BuildStatusBar();
        }

        private void BuildTopBar()
        {
            topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.FromArgb(18, 18, 26)
            };
            topBar.Paint += (s, e) =>
            {
                var r = new Rectangle(0, topBar.Height - 1, topBar.Width, 1);
                using var br = new LinearGradientBrush(r, Accent1, Accent2, LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(br, r);
            };

            var title = new Label
            {
                Text = "AppLauncher",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = TextPrim,
                AutoSize = true,
                Location = new Point(24, 20)
            };

            var searchBg = new Panel
            {
                Width = 340,
                Height = 38,
                BackColor = Color.FromArgb(30, 30, 42)
            };
            PositionCenter(searchBg, topBar, 17);
            topBar.Resize += (s, e) => PositionCenter(searchBg, topBar, 17);

            txtSearch = new TextBox
            {
                PlaceholderText = "Search applications...",
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(30, 30, 42),
                ForeColor = TextPrim,
                Font = new Font("Segoe UI", 10f),
                Width = 300,
                Location = new Point(20, 10)
            };
            txtSearch.TextChanged += (s, e) => { currentSearch = txtSearch.Text; RefreshGrid(); };

            searchBg.Controls.Add(txtSearch);
            topBar.Controls.Add(title);
            topBar.Controls.Add(searchBg);
            Controls.Add(topBar);
        }

        private void BuildFilterBar()
        {
            filterBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(16, 16, 22)
            };

            btnAll = MakeFilterBtn("All Apps", "all", 24);
            btnUser = MakeFilterBtn("User Apps", "user", 134);
            btnSystem = MakeFilterBtn("System Apps", "system", 244);

            // Sort by size button (toggle)
            btnSortSize = new Button
            {
                Text = "Sort by Size",
                Width = 110,
                Height = 30,
                Location = new Point(364, 10),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(26, 26, 36),
                ForeColor = TextSec
            };
            btnSortSize.FlatAppearance.BorderSize = 1;
            btnSortSize.FlatAppearance.BorderColor = Border;
            btnSortSize.Click += (s, e) =>
            {
                sortBySize = !sortBySize;
                btnSortSize.BackColor = sortBySize ? TextSize : Color.FromArgb(26, 26, 36);
                btnSortSize.ForeColor = sortBySize ? Color.Black : TextSec;
                btnSortSize.FlatAppearance.BorderColor = sortBySize ? TextSize : Border;
                RefreshGrid();
            };

            MarkActive(btnAll);
            filterBar.Controls.AddRange(new Control[] { btnAll, btnUser, btnSystem, btnSortSize });
            Controls.Add(filterBar);
        }

        private Button MakeFilterBtn(string text, string tag, int x)
        {
            var btn = new Button
            {
                Text = text,
                Tag = tag,
                Width = 100,
                Height = 30,
                Location = new Point(x, 10),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(26, 26, 36),
                ForeColor = TextSec
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Border;
            btn.Click += (s, e) =>
            {
                currentFilter = (string)btn.Tag!;
                MarkActive(btn);
                RefreshGrid();
            };
            return btn;
        }

        private void MarkActive(Button active)
        {
            foreach (var b in new[] { btnAll, btnUser, btnSystem })
            {
                if (b == null) continue;
                b.BackColor = Color.FromArgb(26, 26, 36);
                b.ForeColor = TextSec;
                b.FlatAppearance.BorderColor = Border;
            }
            active.BackColor = Accent1;
            active.ForeColor = Color.White;
            active.FlatAppearance.BorderColor = Accent1;
        }

        private void BuildGrid()
        {
            gridPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BgBase,
                AutoScroll = true,
                Padding = new Padding(16),
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            Controls.Add(gridPanel);
        }

        private void BuildStatusBar()
        {
            statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                BackColor = Color.FromArgb(18, 18, 26)
            };

            lblStatus = new Label
            {
                Text = "Loading...",
                ForeColor = TextSec,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(20, 8)
            };

            lblCount = new Label
            {
                ForeColor = Accent1,
                Font = new Font("Segoe UI Semibold", 8.5f),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            statusBar.Controls.AddRange(new Control[] { lblStatus, lblCount });
            statusBar.Resize += (s, e) => lblCount.Location = new Point(statusBar.Width - 140, 8);
            Controls.Add(statusBar);
        }

        // ══════════════════════════════════════════════════════════════
        //  DATA
        // ══════════════════════════════════════════════════════════════
        private async void LoadAppsAsync()
        {
            Status("Scanning registry...", Color.Yellow);
            await Task.Run(() => allApps = ScanRegistry());
            RefreshGrid();
            Status("Ready - " + allApps.Count + " apps found", Accent1);
        }

        private List<AppEntry> ScanRegistry()
        {
            var dict = new Dictionary<string, AppEntry>(StringComparer.OrdinalIgnoreCase);

            var keys = new (string path, bool isUser)[]
            {
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false),
                (@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", false),
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true),
            };

            foreach (var (path, isUser) in keys)
            {
                var hive = isUser ? Registry.CurrentUser : Registry.LocalMachine;
                using var root = hive.OpenSubKey(path);
                if (root == null) continue;

                foreach (var subName in root.GetSubKeyNames())
                {
                    using var sub = root.OpenSubKey(subName);
                    if (sub == null) continue;

                    var name = sub.GetValue("DisplayName") as string;
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var installPath = sub.GetValue("InstallLocation") as string
                                   ?? sub.GetValue("DisplayIcon") as string
                                   ?? "";

                    if (installPath.Contains(','))
                        installPath = installPath.Split(',')[0].Trim();
                    installPath = installPath.Trim('"');

                    var pub = sub.GetValue("Publisher") as string;
                    var ver = sub.GetValue("DisplayVersion") as string;
                    var uninstallStr = sub.GetValue("UninstallString") as string;
                    var sysCmp = sub.GetValue("SystemComponent");
                    var winInst = sub.GetValue("WindowsInstaller");

                    // Size in MB (EstimatedSize is in KB)
                    long sizeBytes = 0;
                    var sizeKb = sub.GetValue("EstimatedSize");
                    if (sizeKb is int kb && kb > 0)
                        sizeBytes = (long)kb * 1024;

                    bool isSystem = (sysCmp is int sc && sc == 1)
                                 || (winInst is int wi && wi == 1)
                                 || string.IsNullOrWhiteSpace(pub);

                    if (!dict.ContainsKey(name))
                        dict[name] = new AppEntry
                        {
                            Name = name,
                            InstallPath = installPath,
                            Publisher = pub,
                            Version = ver,
                            UninstallString = uninstallStr,
                            SizeBytes = sizeBytes,
                            IsSystemApp = isSystem
                        };
                }
            }

            var list = dict.Values.OrderBy(a => a.Name).ToList();

            Parallel.ForEach(list, new ParallelOptions { MaxDegreeOfParallelism = 8 }, entry =>
            {
                entry.Icon = GetIcon(entry.InstallPath);
            });

            return list;
        }

        private Icon? GetIcon(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return null;
                if (File.Exists(path)) return Icon.ExtractAssociatedIcon(path);
                if (Directory.Exists(path))
                {
                    var exe = Directory.GetFiles(path, "*.exe", SearchOption.TopDirectoryOnly)
                                       .FirstOrDefault();
                    if (exe != null) return Icon.ExtractAssociatedIcon(exe);
                }
            }
            catch { }
            return null;
        }

        // ══════════════════════════════════════════════════════════════
        //  GRID
        // ══════════════════════════════════════════════════════════════
        private void RefreshGrid()
        {
            if (InvokeRequired) { Invoke(RefreshGrid); return; }

            gridPanel.SuspendLayout();
            gridPanel.Controls.Clear();

            IEnumerable<AppEntry> query = allApps
                .Where(a => currentFilter == "all"
                         || (currentFilter == "user" && !a.IsSystemApp)
                         || (currentFilter == "system" && a.IsSystemApp))
                .Where(a => string.IsNullOrEmpty(currentSearch)
                         || a.Name.Contains(currentSearch, StringComparison.OrdinalIgnoreCase)
                         || (a.Publisher?.Contains(currentSearch, StringComparison.OrdinalIgnoreCase) ?? false));

            // Sort by size descending when toggled, otherwise alphabetical
            var list = sortBySize
                ? query.OrderByDescending(a => a.SizeBytes).ToList()
                : query.OrderBy(a => a.Name).ToList();

            foreach (var app in list)
                gridPanel.Controls.Add(BuildCard(app));

            gridPanel.ResumeLayout();
            if (lblCount != null)
                lblCount.Text = list.Count + " / " + allApps.Count + " apps";
        }

        private Panel BuildCard(AppEntry app)
        {
            // Layout (Y positions):
            //  8  → icon (60px tall)
            //  74 → name (36px tall)
            // 114 → size (18px tall)
            // 138 → buttons (26px tall)
            // 170 → bottom
            const int W = 145, H = 170;

            var card = new Panel
            {
                Width = W,
                Height = H,
                Margin = new Padding(8),
                BackColor = BgCard,
                Cursor = Cursors.Hand,
                Tag = app
            };

            // ── Icon ──────────────────────────────────────────────────
            var pic = new PictureBox
            {
                Size = new Size(60, 60),
                Location = new Point((W - 60) / 2, 8),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = app.Icon != null ? app.Icon.ToBitmap() : MakePlaceholder(app.Name)
            };

            // ── Name ──────────────────────────────────────────────────
            var lblName = new Label
            {
                Text = app.Name,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = TextPrim,
                Width = W - 8,
                Height = 36,
                Location = new Point(4, 74),
                TextAlign = ContentAlignment.TopCenter,
                AutoSize = false
            };

            // ── Size label ────────────────────────────────────────────
            var lblSize = new Label
            {
                Text = FormatSize(app.SizeBytes),
                Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                ForeColor = TextSize,
                Width = W - 8,
                Height = 18,
                Location = new Point(4, 114),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            // ── Action buttons ────────────────────────────────────────
            var btnFolder = MakeCardBtn("📁", 6, 138, 46, AccentBlue: false);
            var btnUninstall = MakeCardBtn("🗑", W - 52, 138, 46, AccentBlue: false);
            btnFolder.ForeColor = Color.FromArgb(160, 180, 255);
            btnUninstall.ForeColor = AccentRed;

            var tip = new ToolTip();
            tip.SetToolTip(btnFolder, "Open install folder");
            tip.SetToolTip(btnUninstall, "Uninstall");

            btnFolder.Click += (s, e) => OpenFolder(app);
            btnUninstall.Click += (s, e) => Uninstall(app, card);

            card.Controls.AddRange(new Control[] { pic, lblName, lblSize, btnFolder, btnUninstall });

            // ── Context menu (right-click) ────────────────────────────
            var ctx = new ContextMenuStrip();
            ctx.BackColor = Color.FromArgb(28, 28, 40);
            ctx.ForeColor = TextPrim;
            ctx.RenderMode = ToolStripRenderMode.System;

            var miLaunch = new ToolStripMenuItem("▶  Launch");
            var miFolder = new ToolStripMenuItem("📁  Open folder");
            var miUninstall = new ToolStripMenuItem("🗑  Uninstall");
            miUninstall.ForeColor = AccentRed;

            miLaunch.Click += (s, e) => Launch(app);
            miFolder.Click += (s, e) => OpenFolder(app);
            miUninstall.Click += (s, e) => Uninstall(app, card);

            ctx.Items.AddRange(new ToolStripItem[] { miLaunch, miFolder, new ToolStripSeparator(), miUninstall });

            // ── Hover + events on all children ────────────────────────
            var tip2 = new ToolTip();
            tip2.SetToolTip(card, app.Name + "\n"
                + (app.Publisher ?? "Unknown") + "\n"
                + "v" + (app.Version ?? "?") + "\n"
                + FormatSize(app.SizeBytes));

            foreach (Control c in new Control[] { card, pic, lblName, lblSize })
            {
                c.MouseEnter += (s, e) => card.BackColor = BgCardHov;
                c.MouseLeave += (s, e) => card.BackColor = BgCard;
                c.DoubleClick += (s, e) => Launch(app);
                c.MouseUp += (s, e) =>
                {
                    if (((MouseEventArgs)e).Button == MouseButtons.Right)
                        ctx.Show(card, ((MouseEventArgs)e).Location);
                };
            }

            return card;
        }

        private Button MakeCardBtn(string text, int x, int y, int w, bool AccentBlue)
        {
            var btn = new Button
            {
                Text = text,
                Width = w,
                Height = 24,
                Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(35, 35, 50)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Bitmap MakePlaceholder(string name)
        {
            var bmp = new Bitmap(56, 56);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var br = new LinearGradientBrush(
                new Rectangle(0, 0, 56, 56), Accent1, Accent2, 135f);
            g.FillEllipse(br, 2, 2, 52, 52);

            var letter = name.Length > 0 ? name[0].ToString().ToUpper() : "?";
            using var font = new Font("Segoe UI Black", 20f, FontStyle.Bold);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(letter, font, Brushes.White, new RectangleF(0, 0, 56, 56), sf);
            return bmp;
        }

        // ══════════════════════════════════════════════════════════════
        //  ACTIONS
        // ══════════════════════════════════════════════════════════════
        private void Launch(AppEntry app)
        {
            string? exe = null;

            if (File.Exists(app.InstallPath)
                && app.InstallPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                exe = app.InstallPath;
            }
            else if (Directory.Exists(app.InstallPath))
            {
                exe = Directory.GetFiles(app.InstallPath, "*.exe", SearchOption.TopDirectoryOnly)
                               .FirstOrDefault();
            }

            if (exe == null)
            {
                Status("Could not find executable for: " + app.Name, Color.OrangeRed);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
                Status("Launched: " + app.Name, Color.LightGreen);
            }
            catch (Exception ex)
            {
                Status("Error: " + ex.Message, Color.OrangeRed);
            }
        }

        private void OpenFolder(AppEntry app)
        {
            // Resolve best folder path
            string? folder = null;

            if (Directory.Exists(app.InstallPath))
                folder = app.InstallPath;
            else if (File.Exists(app.InstallPath))
                folder = Path.GetDirectoryName(app.InstallPath);

            if (folder == null)
            {
                Status("Install folder not found for: " + app.Name, Color.OrangeRed);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", folder) { UseShellExecute = true });
                Status("Opened folder: " + folder, TextSec);
            }
            catch (Exception ex)
            {
                Status("Error opening folder: " + ex.Message, Color.OrangeRed);
            }
        }

        private void Uninstall(AppEntry app, Panel card)
        {
            var result = MessageBox.Show(
                "Are you sure you want to uninstall:\n\n" + app.Name + "?",
                "Uninstall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            if (string.IsNullOrWhiteSpace(app.UninstallString))
            {
                Status("No uninstaller found for: " + app.Name, Color.OrangeRed);
                return;
            }

            try
            {
                // UninstallString can be "MsiExec.exe /I{GUID}" or a direct path
                var uninstall = app.UninstallString.Trim('"');
                string fileName, args = "";

                if (uninstall.StartsWith("MsiExec", StringComparison.OrdinalIgnoreCase)
                    || uninstall.StartsWith("msiexec", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = "msiexec.exe";
                    // Replace /I with /X to trigger uninstall
                    args = uninstall.Replace("MsiExec.exe", "", StringComparison.OrdinalIgnoreCase)
                                    .Replace("/I", "/X", StringComparison.OrdinalIgnoreCase)
                                    .Trim();
                }
                else if (uninstall.Contains(".exe"))
                {
                    // Split path from arguments
                    var parts = uninstall.Split(new[] { ".exe" }, 2, StringSplitOptions.None);
                    fileName = parts[0] + ".exe";
                    args = parts.Length > 1 ? parts[1].Trim() : "";
                }
                else
                {
                    fileName = uninstall;
                }

                Process.Start(new ProcessStartInfo(fileName, args) { UseShellExecute = true });
                Status("Uninstalling: " + app.Name + " - complete the uninstaller wizard", Color.Yellow);

                // Remove from list optimistically
                allApps.Remove(app);
                gridPanel.Controls.Remove(card);
                card.Dispose();
                lblCount.Text = gridPanel.Controls.Count + " / " + allApps.Count + " apps";
            }
            catch (Exception ex)
            {
                Status("Uninstall error: " + ex.Message, Color.OrangeRed);
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════
        private static string FormatSize(long bytes)
        {
            if (bytes <= 0) return "Size unknown";
            if (bytes < 1024) return bytes + " B";
            double kb = bytes / 1024.0;
            if (kb < 1024) return kb.ToString("0.0") + " KB";
            double mb = kb / 1024.0;
            if (mb < 1024) return mb.ToString("0.0") + " MB";
            double gb = mb / 1024.0;
            return gb.ToString("0.00") + " GB";
        }

        private void Status(string msg, Color color)
        {
            if (InvokeRequired) { Invoke(() => Status(msg, color)); return; }
            lblStatus.Text = msg;
            lblStatus.ForeColor = color;
        }

        private void InitializeComponent()
        {

        }

        private static void PositionCenter(Control child, Control parent, int top)
        {
            child.Location = new Point((parent.Width - child.Width) / 2, top);
        }
    }
}