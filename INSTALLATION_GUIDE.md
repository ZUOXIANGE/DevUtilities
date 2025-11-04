# DevUtilities 安装指南

本指南将帮助您在不同操作系统上安装和配置 DevUtilities。

## 📋 系统要求

### 最低系统要求
| 组件 | 要求 |
|------|------|
| **操作系统** | Windows 10 (1903+), macOS 10.15+, Linux (Ubuntu 18.04+) |
| **.NET 运行时** | .NET 9.0 或更高版本 |
| **内存** | 512MB RAM (推荐 1GB+) |
| **存储空间** | 100MB 可用空间 |
| **显示器** | 1024x768 分辨率 (推荐 1920x1080+) |

### 推荐系统配置
| 组件 | 推荐配置 |
|------|----------|
| **操作系统** | Windows 11, macOS 12+, Ubuntu 22.04+ |
| **内存** | 2GB+ RAM |
| **存储空间** | 500MB+ 可用空间 |
| **显示器** | 1920x1080+ 分辨率，支持高 DPI |

## 🚀 安装方式

### 方式一：预编译版本安装（推荐）

#### Windows 安装
1. **下载安装包**
   - 访问 [GitHub Releases](https://github.com/yourusername/DevUtilities/releases)
   - 选择适合的版本：
     - `DevUtilities-win-x64.zip` (Intel/AMD 64位)
     - `DevUtilities-win-arm64.zip` (ARM64 处理器)

2. **安装步骤**
   ```powershell
   # 解压到指定目录
   Expand-Archive -Path "DevUtilities-win-x64.zip" -DestinationPath "C:\Program Files\DevUtilities"
   
   # 添加到 PATH 环境变量（可选）
   $env:PATH += ";C:\Program Files\DevUtilities"
   ```

3. **运行应用**
   - 双击 `DevUtilities.exe`
   - 或在命令行中运行：`DevUtilities.exe`

#### macOS 安装
1. **下载安装包**
   - 选择适合的版本：
     - `DevUtilities-osx-x64.zip` (Intel Mac)
     - `DevUtilities-osx-arm64.zip` (Apple Silicon)

2. **安装步骤**
   ```bash
   # 解压到应用程序目录
   unzip DevUtilities-osx-x64.zip -d /Applications/
   
   # 赋予执行权限
   chmod +x /Applications/DevUtilities.app/Contents/MacOS/DevUtilities
   
   # 移除隔离属性（如果需要）
   xattr -d com.apple.quarantine /Applications/DevUtilities.app
   ```

3. **运行应用**
   - 在 Launchpad 中找到 DevUtilities
   - 或在终端中运行：`open /Applications/DevUtilities.app`

#### Linux 安装
1. **下载安装包**
   - 选择适合的版本：
     - `DevUtilities-linux-x64.zip` (x86_64)
     - `DevUtilities-linux-arm64.zip` (ARM64)

2. **安装步骤**
   ```bash
   # 解压到用户目录
   unzip DevUtilities-linux-x64.zip -d ~/Applications/
   
   # 赋予执行权限
   chmod +x ~/Applications/DevUtilities/DevUtilities
   
   # 创建桌面快捷方式（可选）
   cat > ~/.local/share/applications/devutilities.desktop << EOF
   [Desktop Entry]
   Name=DevUtilities
   Comment=Developer Utilities Collection
   Exec=$HOME/Applications/DevUtilities/DevUtilities
   Icon=$HOME/Applications/DevUtilities/icon.png
   Terminal=false
   Type=Application
   Categories=Development;Utility;
   EOF
   ```

3. **运行应用**
   ```bash
   # 直接运行
   ~/Applications/DevUtilities/DevUtilities
   
   # 或添加到 PATH
   echo 'export PATH="$HOME/Applications/DevUtilities:$PATH"' >> ~/.bashrc
   source ~/.bashrc
   DevUtilities
   ```

### 方式二：从源码构建

#### 前置要求
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)

#### 构建步骤
```bash
# 1. 克隆仓库
git clone https://github.com/yourusername/DevUtilities.git
cd DevUtilities

# 2. 检查 .NET 版本
dotnet --version  # 应显示 9.0.x

# 3. 还原 NuGet 包
dotnet restore

# 4. 构建项目
dotnet build -c Release

# 5. 运行应用（开发模式）
dotnet run --project src/DevUtilities.csproj

# 6. 发布独立应用
dotnet publish src/DevUtilities.csproj -c Release -r win-x64 --self-contained true -o ./publish/win-x64
```

#### 构建所有平台版本
```bash
# Windows 版本
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish/win-x64
dotnet publish -c Release -r win-arm64 --self-contained true -o ./publish/win-arm64

# macOS 版本
dotnet publish -c Release -r osx-x64 --self-contained true -o ./publish/osx-x64
dotnet publish -c Release -r osx-arm64 --self-contained true -o ./publish/osx-arm64

# Linux 版本
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/linux-x64
dotnet publish -c Release -r linux-arm64 --self-contained true -o ./publish/linux-arm64
```

### 方式三：包管理器安装

#### Windows (Chocolatey)
```powershell
# 安装 Chocolatey（如果未安装）
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

# 安装 DevUtilities
choco install devutilities
```

#### Windows (Winget)
```powershell
winget install DevUtilities
```

#### macOS (Homebrew)
```bash
# 安装 Homebrew（如果未安装）
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# 添加 tap 并安装
brew tap yourusername/devutilities
brew install devutilities
```

#### Linux (Snap)
```bash
# Ubuntu/Debian
sudo snap install devutilities

# 或通过 APT（即将支持）
curl -fsSL https://packages.devutilities.com/gpg | sudo apt-key add -
echo "deb https://packages.devutilities.com/apt stable main" | sudo tee /etc/apt/sources.list.d/devutilities.list
sudo apt update
sudo apt install devutilities
```

#### .NET 全局工具
```bash
# 安装为全局工具
dotnet tool install --global DevUtilities

# 运行
devutilities
```

## ⚙️ 配置和设置

### 首次运行配置
1. **启动应用**
   - 首次启动时会自动创建配置目录
   - Windows: `%APPDATA%\DevUtilities`
   - macOS: `~/Library/Application Support/DevUtilities`
   - Linux: `~/.config/DevUtilities`

2. **基本设置**
   - 打开 **设置** 对话框（Ctrl+,）
   - 选择主题：浅色/深色/跟随系统
   - 设置语言：中文/英文
   - 配置字体大小和样式

3. **工具配置**
   - 每个工具都有独立的设置选项
   - 历史记录会自动保存
   - 可以导出/导入配置文件

### 高级配置

#### 配置文件位置
```
配置目录/
├── settings.json          # 应用程序设置
├── tools/                 # 工具特定配置
│   ├── json-formatter.json
│   ├── color-picker.json
│   └── ...
├── themes/                # 自定义主题
├── history/               # 历史记录
└── logs/                  # 日志文件
```

#### 自定义主题
```json
{
  "name": "Custom Theme",
  "colors": {
    "primary": "#007ACC",
    "secondary": "#F0F0F0",
    "background": "#FFFFFF",
    "surface": "#F8F9FA",
    "text": "#212529"
  },
  "fonts": {
    "default": "Inter",
    "monospace": "JetBrains Mono"
  }
}
```

## 🔧 故障排除

### 常见问题

#### 应用无法启动
1. **检查 .NET 运行时**
   ```bash
   dotnet --list-runtimes
   ```
   确保安装了 .NET 9.0 运行时

2. **检查权限**
   - Windows: 以管理员身份运行
   - macOS/Linux: 确保有执行权限

3. **清除配置**
   ```bash
   # 删除配置目录（会重置所有设置）
   rm -rf ~/.config/DevUtilities  # Linux
   rm -rf ~/Library/Application\ Support/DevUtilities  # macOS
   ```

#### 性能问题
1. **内存不足**
   - 关闭其他应用程序
   - 增加虚拟内存

2. **显示问题**
   - 更新显卡驱动
   - 调整 DPI 设置

#### 功能异常
1. **工具无法使用**
   - 检查网络连接（某些工具需要网络）
   - 查看日志文件获取详细错误信息

2. **文件操作失败**
   - 检查文件权限
   - 确保磁盘空间充足

### 日志和调试

#### 启用详细日志
```bash
# 设置环境变量
export DEVUTILITIES_LOG_LEVEL=Debug
DevUtilities

# Windows
set DEVUTILITIES_LOG_LEVEL=Debug
DevUtilities.exe
```

#### 查看日志文件
- Windows: `%APPDATA%\DevUtilities\logs\`
- macOS: `~/Library/Application Support/DevUtilities/logs/`
- Linux: `~/.config/DevUtilities/logs/`

## 🔄 更新和卸载

### 更新应用
1. **自动更新**
   - 应用会自动检查更新
   - 在设置中可以配置更新频率

2. **手动更新**
   - 下载最新版本覆盖安装
   - 或使用包管理器更新

### 卸载应用
1. **删除应用文件**
   ```bash
   # 删除应用程序
   rm -rf /path/to/DevUtilities
   
   # 删除配置文件（可选）
   rm -rf ~/.config/DevUtilities
   ```

2. **包管理器卸载**
   ```bash
   # Chocolatey
   choco uninstall devutilities
   
   # Homebrew
   brew uninstall devutilities
   
   # Snap
   sudo snap remove devutilities
   ```

## 📞 获取帮助

如果您在安装过程中遇到问题，可以通过以下方式获取帮助：

- 📖 查看 [FAQ 文档](./FAQ.md)
- 🐛 提交 [Issue](https://github.com/yourusername/DevUtilities/issues)
- 💬 加入 [讨论区](https://github.com/yourusername/DevUtilities/discussions)
- 📧 发送邮件至：support@devutilities.com

---

*最后更新：2024年12月*