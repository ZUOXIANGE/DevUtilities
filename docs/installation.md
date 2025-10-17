# 安装指南

## 系统要求

### 最低要求
- .NET 9.0 Runtime 或更高版本
- 支持的操作系统：
  - **Windows**: Windows 10 (1903+) / Windows 11
    - 架构：x64, x86, ARM64
  - **Linux**: 
    - Ubuntu 18.04+, 20.04+, 22.04+
    - CentOS 7+, 8+
    - Debian 9+, 10+, 11+
    - Fedora 33+
    - openSUSE 15+
    - 架构：x64, ARM64
  - **macOS**: 
    - macOS 10.15 (Catalina) 或更高版本
    - 架构：x64 (Intel), ARM64 (Apple Silicon)
- 内存：4GB RAM (推荐 8GB)
- 存储空间：100MB 可用磁盘空间

### 推荐配置
- .NET 9.0 SDK (用于开发)
- 8GB+ RAM
- SSD 存储
- 1920x1080 或更高分辨率显示器

## 安装方式

### 方式一：预编译版本 (推荐)

#### Windows
1. 访问 [GitHub Releases](https://github.com/yourusername/DevUtilities/releases)
2. 下载 `DevUtilities-win-x64.zip`
3. 解压到任意目录
4. 双击 `DevUtilities.exe` 运行

#### Linux
```bash
# 下载并解压
wget https://github.com/yourusername/DevUtilities/releases/latest/download/DevUtilities-linux-x64.tar.gz
tar -xzf DevUtilities-linux-x64.tar.gz
cd DevUtilities-linux-x64

# 添加执行权限并运行
chmod +x DevUtilities
./DevUtilities
```

#### macOS
```bash
# 下载并解压
curl -L -o DevUtilities-osx-x64.tar.gz https://github.com/yourusername/DevUtilities/releases/latest/download/DevUtilities-osx-x64.tar.gz
tar -xzf DevUtilities-osx-x64.tar.gz
cd DevUtilities-osx-x64

# 运行应用
./DevUtilities
```

### 方式二：从源码构建

#### 前置要求
- .NET 9.0 SDK
- Git

#### 构建步骤
```bash
# 1. 克隆仓库
git clone https://github.com/yourusername/DevUtilities.git
cd DevUtilities

# 2. 恢复依赖
dotnet restore

# 3. 构建项目
dotnet build --configuration Release

# 4. 运行应用
dotnet run --project DevUtilities.csproj

# 或者发布为独立应用
# Windows
dotnet publish -r win-x64 --self-contained false -o ./publish/win-x64

# Linux
dotnet publish -r linux-x64 --self-contained false -o ./publish/linux-x64

# macOS
dotnet publish -r osx-x64 --self-contained false -o ./publish/osx-x64
```

## 依赖项说明

### 运行时依赖
- **.NET 9.0 Runtime**: 核心运行时环境
- **Avalonia UI**: 跨平台 UI 框架
- **SkiaSharp**: 跨平台 2D 图形库 (替代 System.Drawing)

### 平台特定依赖
- **Windows**: 
  - `libHarfBuzzSharp.dll` - 文本渲染
  - `libSkiaSharp.dll` - 图形渲染
  - `av_libglesv2.dll` - OpenGL ES 支持
- **Linux**: 
  - `libHarfBuzzSharp.so` - 文本渲染
  - `libSkiaSharp.so` - 图形渲染
  - `libnironcompress.so` - 压缩支持
- **macOS**: 
  - `libHarfBuzzSharp.dylib` - 文本渲染
  - `libSkiaSharp.dylib` - 图形渲染
  - `libAvaloniaNative.dylib` - 原生 UI 支持

## 故障排除

### 常见问题

#### 1. 应用无法启动
**Windows**:
- 确保已安装 .NET 9.0 Runtime
- 检查 Windows Defender 是否阻止了应用
- 以管理员身份运行

**Linux**:
```bash
# 检查 .NET 运行时
dotnet --version

# 安装缺失的依赖
sudo apt update
sudo apt install libicu-dev libssl-dev

# 检查执行权限
chmod +x DevUtilities
```

**macOS**:
- 首次运行时，右键点击应用选择"打开"以绕过 Gatekeeper
- 确保已安装 .NET 9.0 Runtime

#### 2. 图形渲染问题
- 更新显卡驱动程序
- 在 Linux 上确保安装了适当的图形库：
```bash
sudo apt install libgl1-mesa-dev libglu1-mesa-dev
```

#### 3. 字体渲染问题
- 确保系统安装了基本字体
- Linux 用户可能需要安装字体包：
```bash
sudo apt install fonts-liberation fonts-dejavu-core
```

### 获取帮助
如果遇到问题，请：
1. 查看 [FAQ](https://github.com/yourusername/DevUtilities/wiki/FAQ)
2. 搜索现有的 [Issues](https://github.com/yourusername/DevUtilities/issues)
3. 创建新的 Issue 并提供详细信息

## 卸载

### Windows
- 删除应用程序文件夹
- 清理注册表项（如果有）

### Linux/macOS
```bash
# 删除应用程序文件
rm -rf /path/to/DevUtilities

# 清理配置文件（可选）
rm -rf ~/.config/DevUtilities
```

### 方式三：发布独立版本

#### 创建独立可执行文件
```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true

# macOS x64
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
```

#### 运行独立版本
```bash
# 进入发布目录
cd bin/Release/net9.0/[runtime]/publish/

# 运行可执行文件
./DevUtilities  # Linux/macOS
DevUtilities.exe  # Windows
```

## 🔧 .NET 运行时安装

如果您的系统没有安装 .NET 9.0 运行时：

### Windows
1. **下载 .NET 9.0**
   ```
   https://dotnet.microsoft.com/download/dotnet/9.0
   ```

2. **安装运行时**
   - 下载 "ASP.NET Core Runtime 9.0.x"
   - 运行安装程序
   - 重启计算机

### Linux (Ubuntu/Debian)
```bash
# 添加 Microsoft 包源
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# 安装 .NET 运行时
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-9.0
```

### Linux (CentOS/RHEL/Fedora)
```bash
# 添加 Microsoft 包源
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm

# 安装 .NET 运行时
sudo yum install aspnetcore-runtime-9.0
```

### macOS
```bash
# 使用 Homebrew
brew install --cask dotnet

# 或手动下载安装
# https://dotnet.microsoft.com/download/dotnet/9.0
```

## 🐳 Docker 部署

### 使用预构建镜像
```bash
# 拉取镜像
docker pull devutilities/devutilities:latest

# 运行容器
docker run -d -p 8080:80 --name devutilities devutilities/devutilities:latest
```

### 从源码构建镜像
```bash
# 克隆仓库
git clone https://github.com/yourusername/DevUtilities.git
cd DevUtilities

# 构建镜像
docker build -t devutilities .

# 运行容器
docker run -d -p 8080:80 --name devutilities devutilities
```

### Docker Compose
```yaml
version: '3.8'
services:
  devutilities:
    image: devutilities/devutilities:latest
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    restart: unless-stopped
```

## 📦 包管理器安装

### Windows (Chocolatey)
```powershell
# 安装 Chocolatey（如果未安装）
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

# 安装 DevUtilities
choco install devutilities
```

### Windows (Winget)
```powershell
winget install DevUtilities.DevUtilities
```

### Linux (Snap)
```bash
sudo snap install devutilities
```

### macOS (Homebrew)
```bash
# 添加 tap
brew tap devutilities/tap

# 安装应用
brew install devutilities
```

## ⚙️ 配置

### 首次启动配置
1. **启动应用程序**
2. **选择语言**（如果支持多语言）
3. **配置主题**（如果支持主题切换）
4. **设置默认工具**

### 配置文件位置
- **Windows**: `%APPDATA%\DevUtilities\config.json`
- **Linux**: `~/.config/DevUtilities/config.json`
- **macOS**: `~/Library/Application Support/DevUtilities/config.json`

### 配置选项
```json
{
  "theme": "light",
  "language": "zh-CN",
  "defaultTool": "qrcode",
  "autoSave": true,
  "checkUpdates": true
}
```

## 🔄 更新

### 自动更新
- 应用程序会自动检查更新
- 在设置中可以启用/禁用自动更新

### 手动更新
1. **检查新版本**
   ```
   https://github.com/yourusername/DevUtilities/releases
   ```

2. **下载新版本**
3. **备份配置文件**
4. **安装新版本**
5. **恢复配置文件**

### 命令行更新
```bash
# 使用包管理器更新
choco upgrade devutilities  # Windows
brew upgrade devutilities   # macOS
snap refresh devutilities   # Linux
```

## 🗑️ 卸载

### Windows
1. **控制面板卸载**
   - 打开"程序和功能"
   - 找到"DevUtilities"
   - 点击"卸载"

2. **命令行卸载**
   ```powershell
   choco uninstall devutilities
   # 或
   winget uninstall DevUtilities.DevUtilities
   ```

### Linux
```bash
# 如果使用包管理器安装
sudo apt remove devutilities  # Debian/Ubuntu
sudo yum remove devutilities   # CentOS/RHEL
snap remove devutilities       # Snap

# 如果手动安装
rm -rf ~/DevUtilities
rm -rf ~/.config/DevUtilities
```

### macOS
```bash
# 如果使用 Homebrew
brew uninstall devutilities

# 如果手动安装
rm -rf /Applications/DevUtilities.app
rm -rf ~/Library/Application\ Support/DevUtilities
```

## 🚨 故障排除

### 常见问题

#### 应用程序无法启动
1. **检查 .NET 运行时**
   ```bash
   dotnet --version
   ```

2. **检查系统要求**
3. **查看错误日志**
   - Windows: `%APPDATA%\DevUtilities\logs\`
   - Linux/macOS: `~/.config/DevUtilities/logs/`

#### 功能异常
1. **重置配置文件**
2. **重新安装应用程序**
3. **检查权限设置**

#### 性能问题
1. **检查系统资源**
2. **关闭不必要的功能**
3. **更新到最新版本**

### 获取帮助
- **查看日志文件**
- **提交 Issue**: https://github.com/yourusername/DevUtilities/issues
- **社区讨论**: https://github.com/yourusername/DevUtilities/discussions

## 📞 技术支持

如果您在安装过程中遇到问题：

1. **查看文档**: [用户手册](user-guide.md)
2. **搜索问题**: [GitHub Issues](https://github.com/yourusername/DevUtilities/issues)
3. **提交问题**: [新建 Issue](https://github.com/yourusername/DevUtilities/issues/new)
4. **联系支持**: support@devutilities.com

---

**安装成功后，请查看 [用户手册](user-guide.md) 了解如何使用各项功能。**