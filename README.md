# ⚡ AppLauncher

<div align="center">

![.NET](https://img.shields.io/badge/.NET_8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Windows Forms](https://img.shields.io/badge/Windows_Forms-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Active-brightgreen?style=for-the-badge)

**A modern, dark-themed application launcher for Windows built with C# and .NET 8.**  
Browse, launch, and manage all your installed apps from one clean interface.

[Features](#-features) • [Getting Started](#-getting-started) • [Screenshots](#-screenshots) • [Contributing](#-contributing) • [Roadmap](#-roadmap)

</div>



## ✨ Features

- **📦 410+ apps detected** — reads directly from the Windows Registry (HKLM + HKCU, 32 & 64-bit)
- **🖼 Icon grid** — displays each app's real icon; gradient letter placeholder when unavailable
- **🔍 Live search** — filters by name or publisher as you type
- **🏷 Category filters** — All Apps / User Apps / System Apps
- **⬇ Sort by size** — order apps from largest to smallest install size
- **📁 Open install folder** — jump straight to where the app lives on disk
- **🗑 Uninstall** — triggers the native uninstaller with a confirmation dialog
- **🌙 / ☀️ Theme toggle** — switch between dark and light mode instantly
- **👤 User greeting** — shows your Windows username, editable with double-click
- **🕐 Live clock** — real-time system clock in the top bar
- **⚡ Fast loading** — parallel icon extraction with up to 8 threads

---

## 🚀 Getting Started

### Prerequisites

| Tool | Version |
|------|---------|
| [Visual Studio 2022](https://visualstudio.microsoft.com/) | 17.8 or later |
| [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0+ |
| Windows | 10 / 11 |

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/Janrosky/AppLauncher.git

# 2. Open the solution in Visual Studio
cd AppLauncher
start AppLauncher.sln
```

Then press `Ctrl + F5` to build and run.

### Project Structure

```
AppLauncher/
├── AppLauncher.sln
└── AppLauncher/
    ├── AppLauncher.csproj   # Project config (.NET 8, WinForms)
    ├── app.manifest         # DPI awareness + UAC settings
    ├── Program.cs           # Entry point
    ├── AppEntry.cs          # Data model (name, path, size, icon...)
    └── MainForm.cs          # All UI logic — top bar, grid, actions
```

---

## 🎮 Usage

| Action | How |
|--------|-----|
| **Launch an app** | Double-click its card |
| **Open install folder** | Click 📁 on the card, or right-click → Open folder |
| **Uninstall** | Click 🗑 on the card, or right-click → Uninstall |
| **Search** | Type in the search bar (filters in real time) |
| **Filter by type** | Click All Apps / User Apps / System Apps |
| **Sort by size** | Click ⬇ Sort by Size (toggles on/off) |
| **Switch theme** | Click 🌙 / ☀️ in the top-right corner |
| **Edit your name** | Double-click the 👤 username label |

---

## 🛠 Tech Stack

- **Language:** C# 12
- **Framework:** .NET 8 — Windows Forms
- **Registry access:** `Microsoft.Win32.Registry`
- **Parallelism:** `Parallel.ForEach` for icon loading
- **Rendering:** GDI+ (`System.Drawing`) for placeholder icons and gradient accents

---

## 🗺 Roadmap

- [ ] Persist theme preference and username between sessions
- [ ] Support pinning favorite apps to the top
- [ ] Add launch count tracking ("Most used" filter)
- [ ] Microsoft Store / UWP app support
- [ ] Export app list to CSV
- [ ] Keyboard navigation (arrow keys + Enter to launch)
- [ ] App categories / custom tags

---

## 🤝 Contributing

Contributions are welcome! Here's how to get involved:

1. **Fork** the repository
2. **Create** a feature branch
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Commit** your changes
   ```bash
   git commit -m "feat: add your feature description"
   ```
4. **Push** to your fork
   ```bash
   git push origin feature/your-feature-name
   ```
5. **Open a Pull Request** — describe what you changed and why

### Commit convention

| Prefix | Use for |
|--------|---------|
| `feat:` | New feature |
| `fix:` | Bug fix |
| `ui:` | Visual / layout changes |
| `refactor:` | Code cleanup without behavior change |
| `docs:` | README or comments |

### Good first issues to tackle

- Add a `Settings` panel to persist preferences
- Improve uninstall detection for edge-case installers
- Add smooth card animations on hover
- Write unit tests for `ScanRegistry()`

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Alejandro Chacón Garita**  
[![GitHub](https://img.shields.io/badge/GitHub-Janrosky-181717?style=flat&logo=github)](https://github.com/Janrosky)

---

<div align="center">
  <sub>Built with ☕ and C# in Costa Rica 🇨🇷</sub>
</div>
