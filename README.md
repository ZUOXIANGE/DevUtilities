# DevUtilities

<div align="center">

![DevUtilities Logo](https://img.shields.io/badge/DevUtilities-v1.0.0-blue.svg)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia%20UI-11.1.3-red.svg)](https://avaloniaui.net/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)](https://github.com/AvaloniaUI/Avalonia)

**一个功能丰富的跨平台开发者工具集**

*集成19个实用工具，提升开发效率*

</div>

## 📖 项目简介

DevUtilities 是一个基于 Avalonia UI 框架开发的跨平台桌面应用程序，专为开发者设计。它集成了19个常用的开发工具，包括编码转换、格式化、加密解密、二维码生成等功能，旨在提高开发者的工作效率。

## 📁 项目结构

```
DevUtilities/
├── src/                    # Source code
│   ├── ViewModels/        # MVVM ViewModels
│   ├── Views/             # AXAML Views
│   ├── Models/            # Data models
│   ├── Converters/        # Value converters
│   ├── bin/               # Build output
│   ├── obj/               # Build artifacts
│   └── DevUtilities.csproj # Project file
├── assets/                 # Static assets
│   └── images/            # Images and icons
├── docs/                   # Documentation
├── samples/               # Sample files and examples
├── tests/                 # Test projects
├── scripts/               # Build and utility scripts
├── tools/                 # Tools and releases
│   └── releases/          # Published binaries
├── .github/               # GitHub workflows and templates
├── README.md              # This file
├── LICENSE                # License file
├── CHANGELOG.md           # Change log
└── DevUtilities.sln       # Solution file
```

## ✨ 功能特性

### 🔧 核心工具

| 工具名称 | 功能描述 | 状态 |
|---------|---------|------|
| **Base64编码器** | Base64编码/解码，支持文本和文件 | ✅ |
| **进制转换器** | 二进制、八进制、十进制、十六进制互转 | ✅ |
| **颜色选择器** | RGB、HEX、HSL颜色格式转换 | ✅ |
| **加密工具** | AES、DES、RSA等多种加密算法 | ✅ |
| **十六进制转换器** | 十六进制与文本互转 | ✅ |
| **HTML格式化器** | HTML代码格式化和压缩 | ✅ |
| **JSON格式化器** | JSON格式化、压缩、验证 | ✅ |
| **JWT编码器** | JWT令牌生成、解析、验证 | ✅ |
| **密码生成器** | 安全密码生成，支持自定义规则 | ✅ |
| **二维码工具** | 二维码生成和扫描 | ✅ |
| **正则表达式测试器** | 正则表达式测试和匹配 | ✅ |
| **SQL格式化器** | SQL语句格式化和美化 | ✅ |
| **时间戳转换器** | Unix时间戳与日期时间互转 | ✅ |
| **单位转换器** | 长度、重量、温度等单位转换 | ✅ |
| **URL工具** | URL编码/解码、解析 | ✅ |
| **UUID生成器** | UUID/GUID生成器 | ✅ |

### 🎯 二维码工具特色功能

- **生成功能**
  - 支持多种内容类型（文本、URL、WiFi等）
  - 可调节纠错级别（L、M、Q、H）
  - 自定义颜色（前景色、背景色）
  - 可调节图片尺寸（100-800px）
  - 支持静默区设置
  - 多种编码格式支持

- **扫描功能**
  - 支持多种图片格式（PNG、JPG、JPEG、BMP、GIF）
  - 文件选择器集成
  - 实时解码结果显示
  - 完善的错误处理

### 🔐 加密工具特色功能

- **对称加密**：AES、DES、3DES
- **非对称加密**：RSA
- **哈希算法**：MD5、SHA1、SHA256、SHA512
- **编码模式**：CBC、ECB、CFB、OFB
- **填充方式**：PKCS7、Zero、None
- **多种编码格式**：UTF-8、ASCII、Base64、Hex

## 🚀 快速开始

### 系统要求

- **.NET 9.0** 或更高版本
- **支持的操作系统**：
  - Windows 10/11 (x64, x86, ARM64)
  - Linux (x64, ARM64) - Ubuntu 18.04+, CentOS 7+, Debian 9+
  - macOS (x64, ARM64) - macOS 10.15+
- **内存**：4GB RAM (推荐 8GB)
- **存储空间**：100MB 可用磁盘空间

### 安装方式

#### 系统要求
- **操作系统**: Windows 10/11, macOS 10.15+, Linux (Ubuntu 18.04+)
- **.NET 运行时**: .NET 9.0 或更高版本
- **内存**: 最低 512MB RAM，推荐 1GB+
- **存储空间**: 约 100MB 可用空间

#### 方式一：从发布版本下载（推荐）
1. 访问 [Releases 页面](https://github.com/yourusername/DevUtilities/releases)
2. 根据您的操作系统选择对应版本：
   - **Windows**: `DevUtilities-win-x64.zip` 或 `DevUtilities-win-arm64.zip`
   - **macOS**: `DevUtilities-osx-x64.zip` 或 `DevUtilities-osx-arm64.zip`
   - **Linux**: `DevUtilities-linux-x64.zip` 或 `DevUtilities-linux-arm64.zip`
3. 解压下载的文件到任意目录
4. 运行可执行文件：
   - **Windows**: 双击 `DevUtilities.exe`
   - **macOS/Linux**: 在终端中运行 `./DevUtilities`

#### 方式二：从源码构建
```bash
# 1. 确保已安装 .NET 9.0 SDK
dotnet --version  # 应显示 9.0.x

# 2. 克隆仓库
git clone https://github.com/yourusername/DevUtilities.git
cd DevUtilities

# 3. 还原依赖包
dotnet restore

# 4. 构建项目
dotnet build -c Release

# 5. 运行应用
dotnet run --project src/DevUtilities.csproj

# 或者发布为独立应用
dotnet publish -c Release -r win-x64 --self-contained true
```

#### 方式三：使用包管理器安装
```bash
# 通过 .NET 工具安装（即将支持）
dotnet tool install --global DevUtilities

# 通过 Chocolatey 安装 (Windows)
choco install devutilities

# 通过 Homebrew 安装 (macOS)
brew install devutilities

# 通过 Snap 安装 (Linux)
sudo snap install devutilities
```

#### 首次运行配置
1. 启动应用后，系统会自动创建配置目录
2. 可以通过 **设置** 菜单自定义主题、语言等选项
3. 所有工具的历史记录和设置会自动保存

### 使用说明

1. **启动应用程序**
   ```bash
   dotnet run
   ```

2. **选择工具**
   - 在主界面点击相应的工具图标
   - 每个工具都有独立的界面和功能

3. **二维码工具使用**
   - **生成**：输入内容，选择参数，点击"生成二维码"
   - **扫描**：点击"加载图片"，选择包含二维码的图片文件

4. **加密工具使用**
   - 选择加密算法和模式
   - 输入密钥和初始向量（如需要）
   - 输入要加密/解密的内容
   - 点击相应的操作按钮

## 🏗️ 项目结构

```
DevUtilities/
├── Assets/                 # 资源文件
├── Converters/            # 数据转换器
│   ├── BooleanConverters.cs
│   ├── ColorConverter.cs
│   └── StringConverters.cs
├── Models/                # 数据模型
│   └── ToolInfo.cs
├── ViewModels/            # 视图模型（MVVM）
│   ├── Base64EncoderViewModel.cs
│   ├── QrCodeViewModel.cs
│   ├── CryptoToolsViewModel.cs
│   └── ...
├── Views/                 # 用户界面
│   ├── MainWindow.axaml
│   ├── QrCodeView.axaml
│   ├── CryptoToolsView.axaml
│   └── ...
├── App.axaml             # 应用程序入口
├── Program.cs            # 主程序
└── DevUtilities.csproj   # 项目文件
```

## 🛠️ 技术栈

- **框架**：.NET 9.0
- **UI框架**：Avalonia UI 11.1.3
- **架构模式**：MVVM (Model-View-ViewModel)
- **依赖注入**：CommunityToolkit.Mvvm
- **二维码库**：QRCoder + ZXing.Net
- **加密库**：System.Security.Cryptography
- **JSON处理**：Newtonsoft.Json + System.Text.Json

## 📦 主要依赖

```xml
<PackageReference Include="Avalonia" Version="11.1.3" />
<PackageReference Include="Avalonia.Desktop" Version="11.1.3" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
<PackageReference Include="QRCoder" Version="1.6.0" />
<PackageReference Include="ZXing.Net" Version="0.16.10" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="System.Drawing.Common" Version="9.0.0" />
```

## 📚 文档

### 用户文档
- 📖 [安装指南](./docs/INSTALLATION_GUIDE.md) - 详细的安装和配置说明
- 🎯 [使用示例](./docs/USAGE_EXAMPLES.md) - 各工具的详细使用方法和最佳实践
- 🖼️ [界面截图](./docs/SCREENSHOTS.md) - 应用程序界面展示和功能说明
- ❓ [常见问题](./docs/FAQ.md) - 常见问题解答和故障排除

### 开发者文档
- 🏗️ [架构设计](./docs/ARCHITECTURE.md) - 技术架构和设计模式详解
- 🎨 [UI 样式指南](./docs/UI_STYLE_GUIDE.md) - 界面设计规范和样式指导
- 🧩 [组件库](./docs/COMPONENT_LIBRARY.md) - UI 组件使用说明和规范
- 🔧 [开发指南](./docs/DEVELOPMENT.md) - 开发环境搭建和贡献指南
- 📋 [API 文档](./docs/API.md) - 内部 API 和扩展接口说明

### 项目管理
- 📝 [更新日志](./CHANGELOG.md) - 版本更新记录和新功能说明
- 🚀 [路线图](./ROADMAP.md) - 未来功能规划和开发计划
- 🤝 [贡献指南](./CONTRIBUTING.md) - 如何参与项目开发
- 📄 [许可证](./LICENSE) - 开源许可证信息

## 🤝 贡献指南

我们欢迎所有形式的贡献！请查看 [CONTRIBUTING.md](CONTRIBUTING.md) 了解详细信息。

### 贡献方式

1. **Fork** 本仓库
2. 创建您的特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交您的更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开一个 **Pull Request**

### 开发环境设置

```bash
# 克隆您的fork
git clone https://github.com/yourusername/DevUtilities.git

# 添加上游仓库
git remote add upstream https://github.com/originalowner/DevUtilities.git

# 安装依赖
dotnet restore

# 运行测试
dotnet test
```

## 📝 更新日志

查看 [CHANGELOG.md](CHANGELOG.md) 了解版本更新历史。

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详细信息。

## 🙏 致谢

- [Avalonia UI](https://avaloniaui.net/) - 跨平台UI框架
- [QRCoder](https://github.com/codebude/QRCoder) - 二维码生成库
- [ZXing.Net](https://github.com/micjahn/ZXing.Net) - 二维码扫描库
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM工具包

## 📞 联系方式

- **项目主页**：[GitHub Repository](https://github.com/yourusername/DevUtilities)
- **问题反馈**：[Issues](https://github.com/yourusername/DevUtilities/issues)
- **功能请求**：[Feature Requests](https://github.com/yourusername/DevUtilities/issues/new?template=feature_request.md)

## ⭐ 支持项目

如果这个项目对您有帮助，请给我们一个 ⭐ Star！

---

<div align="center">

**[⬆ 回到顶部](#devutilities)**

Made with ❤️ by developers, for developers

</div>