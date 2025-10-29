# DevUtilities

<div align="center">

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia%20UI-11.1.3-red.svg)](https://avaloniaui.net/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey.svg)](https://github.com/AvaloniaUI/Avalonia)

**🛠️ 跨平台开发者工具集**

*集成31种实用工具，一站式解决开发需求*

[快速开始](#-快速开始) • [功能特性](#-功能特性) • [下载安装](#-安装使用) • [文档](#-文档)

</div>

## 📖 项目简介

DevUtilities 是一个基于 Avalonia UI 开发的跨平台桌面应用，专为开发者打造。集成了编码转换、格式化、加密解密、二维码生成等31种常用开发工具，界面简洁美观，操作便捷高效。

## 📁 项目结构

```
DevUtilities/
├── src/                    # 源代码
│   ├── ViewModels/        # MVVM 视图模型
│   ├── Views/             # AXAML 视图
│   ├── Models/            # 数据模型
│   ├── Converters/        # 值转换器
│   └── DevUtilities.csproj # 项目文件
├── docs/                   # 文档
├── tests/                 # 测试项目
├── scripts/               # 构建脚本
└── README.md              # 项目说明
```

## ✨ 功能特性

### 🔧 核心工具 (31种)

<details>
<summary><strong>📝 文本处理工具</strong></summary>

- **Base64编码器** - Base64编码/解码，支持文本和文件
- **进制转换器** - 二进制、八进制、十进制、十六进制互转
- **十六进制转换器** - 十六进制与文本互转
- **字符串转义** - 字符串转义和反转义
- **文本对比** - 文本差异对比工具
- **正则测试** - 正则表达式测试和验证

</details>

<details>
<summary><strong>🎨 格式化工具</strong></summary>

- **JSON格式化器** - JSON格式化、压缩、验证
- **HTML格式化器** - HTML代码格式化和美化
- **XML格式化器** - XML代码格式化和验证
- **SQL格式化器** - SQL语句格式化和美化
- **JSON/YAML转换** - JSON和YAML格式互相转换

</details>

<details>
<summary><strong>🔐 安全加密工具</strong></summary>

- **加密工具** - AES、DES、RSA等多种加密算法
- **JWT编码器** - JWT令牌生成、解析、验证
- **哈希生成** - MD5、SHA1、SHA256、SHA512哈希计算
- **文本加解密** - AES/DES/3DES文本加解密
- **密码生成器** - 安全密码生成，支持自定义规则

</details>

<details>
<summary><strong>🌐 网络工具</strong></summary>

- **URL工具** - URL编码/解码、解析
- **HTTP请求** - HTTP请求测试工具
- **IP查询** - IP地址查询工具

</details>

<details>
<summary><strong>🔧 开发工具</strong></summary>

- **二维码工具** - 二维码生成和识别
- **UUID生成器** - UUID/GUID生成器
- **ULID生成器** - ULID生成器
- **Sqids生成器** - Sqids ID生成器
- **颜色选择器** - RGB、HEX、HSL颜色格式转换
- **时间戳转换器** - Unix时间戳与日期时间互转
- **单位转换器** - 长度、重量、温度等单位转换
- **Cron表达式** - Cron表达式生成和解析
- **chmod计算器** - Linux文件权限计算与转换

</details>

<details>
<summary><strong>📊 数据工具</strong></summary>

- **Parquet查看器** - Parquet文件查看器
- **JSON示例生成** - 根据类定义生成JSON示例
- **Docker Compose转换** - Docker run命令转换为docker-compose文件

</details>

## 🚀 快速开始

### 系统要求
- **.NET 9.0** 或更高版本
- **操作系统**: Windows 10/11, macOS 10.15+, Linux (Ubuntu 18.04+)
- **内存**: 最低 512MB RAM，推荐 1GB+
- **存储空间**: 约 100MB 可用空间

## 📦 安装使用

### 方式一：下载发布版本（推荐）
1. 访问 [Releases 页面](https://github.com/ZUOXIANGE/DevUtilities/releases)
2. 根据操作系统下载对应版本
3. 解压并运行可执行文件

### 方式二：从源码构建
```bash
# 克隆仓库
git clone https://github.com/ZUOXIANGE/DevUtilities.git
cd DevUtilities

# 还原依赖并构建
dotnet restore
dotnet build -c Release

# 运行应用
dotnet run --project src/DevUtilities.csproj
```

## 🛠️ 技术栈

- **框架**: .NET 9.0 + Avalonia UI 11.1.3
- **架构**: MVVM (Model-View-ViewModel)
- **依赖注入**: CommunityToolkit.Mvvm
- **主要库**: QRCoder, ZXing.Net, Newtonsoft.Json

## 📚 文档

- 📖 [安装指南](./docs/INSTALLATION_GUIDE.md) - 详细安装说明
- 🎯 [使用示例](./docs/USAGE_EXAMPLES.md) - 各工具使用方法
- 🏗️ [架构设计](./docs/architecture.md) - 技术架构详解
- 🎨 [UI 样式指南](./docs/UI_STYLE_GUIDE.md) - 界面设计规范

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详细信息。

---

<div align="center">

**⭐ 如果这个项目对您有帮助，请给我们一个 Star！**

[GitHub](https://github.com/ZUOXIANGE/DevUtilities) • [Issues](https://github.com/ZUOXIANGE/DevUtilities/issues) • [Releases](https://github.com/ZUOXIANGE/DevUtilities/releases)

<sub>Powered by Trae AI</sub>

</div>