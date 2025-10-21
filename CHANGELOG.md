# 更新日志

所有重要的项目更改都将记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
并且本项目遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [1.0.0] - 2025-01-XX

### 新增
- 🎉 初始版本发布
- ✨ 多种开发者工具集合
- 🔧 QR码生成和扫描功能
- 🔐 加密解密工具 (AES, DES, RSA, MD5, SHA系列)
- 📝 格式化工具 (JSON, XML, SQL, HTML)
- 🔢 编码转换 (Base64, Hex, URL编码)
- 🎨 颜色选择器和转换工具
- 🔑 密码生成器
- 📅 时间戳转换器
- 🔍 正则表达式测试器
- 🆔 UUID生成器
- 📐 单位转换器
- 🌐 JWT编码解码器
- 📊 进制转换器
- 🌍 **跨平台支持** - Windows, Linux, macOS
- 🏗️ 基于 Avalonia UI 的现代界面
- 📱 响应式设计，支持多种屏幕尺寸

### 技术特性
- 🔄 **跨平台兼容性**
  - Windows 10/11 (x64, x86, ARM64)
  - Linux (x64, ARM64) - Ubuntu, CentOS, Debian, Fedora
  - macOS (x64, ARM64) - Intel 和 Apple Silicon
- 🎨 使用 SkiaSharp 替代 System.Drawing.Common 确保跨平台图形渲染
- ⚡ .NET 9.0 性能优化
- 🔧 MVVM 架构模式
- 📦 单文件发布支持
- 🛡️ 安全的加密实现

### 开发者体验
- 📚 完整的文档系统
- 🤝 贡献指南和行为准则
- 🔄 CI/CD 自动化流水线
- 📋 GitHub 模板 (Issue, PR)
- 🎯 代码质量检查和格式化
- 🧪 跨平台构建测试

### 依赖更新
- 移除 `System.Drawing.Common` (Windows 特定)
- 新增 `SkiaSharp` 2.88.8 (跨平台图形)
- 更新 `Avalonia` 到 11.1.3
- 更新 `QRCoder` 到 1.6.0
- 更新 `ZXing.Net` 到 0.16.10

### 构建配置
- 支持多运行时标识符 (RID)
- 优化发布配置
- 跨平台构建脚本
- 自动化测试流程

### 技术实现
- 🏗️ **架构**
  - MVVM 架构模式
  - 使用 CommunityToolkit.Mvvm
  - 模块化设计
- 📦 **依赖管理**
  - .NET 9.0 框架
  - Avalonia UI 11.1.3
  - QRCoder 1.6.0（二维码生成）
  - ZXing.Net 0.16.10（二维码扫描）
  - Newtonsoft.Json 13.0.3
  - System.Drawing.Common 9.0.0
- 🔧 **构建系统**
  - 支持跨平台构建
  - 单文件发布选项
  - 框架依赖发布选项

### 修复
- 🐛 解决了 ZXing.Net 包冲突问题
- 🐛 修复了 BitmapLuminanceSource 在 .NET Core 中的兼容性问题
- 🐛 解决了二维码解码的语法错误
- 🐛 修复了重复 catch 块的编译错误

### 优化
- ⚡ 使用 RGBLuminanceSource 替代 BitmapLuminanceSource 提高兼容性
- ⚡ 优化了包依赖，移除了不必要的绑定包
- ⚡ 改进了错误处理和用户反馈机制

## [0.9.0] - 2025-01-16

### 新增
- 🚧 **开发阶段**
  - 项目初始化
  - 基础架构搭建
  - 核心工具实现

### 技术债务
- 📝 需要添加单元测试
- 📝 需要完善文档
- 📝 需要优化性能

## 版本说明

### 版本号格式
我们使用 [语义化版本](https://semver.org/lang/zh-CN/) 格式：`主版本号.次版本号.修订号`

- **主版本号**：不兼容的 API 修改
- **次版本号**：向下兼容的功能性新增
- **修订号**：向下兼容的问题修正

### 变更类型

- **新增** - 新功能
- **修改** - 对现有功能的变更
- **弃用** - 即将移除的功能
- **移除** - 已移除的功能
- **修复** - 问题修复
- **安全** - 安全相关的修复

### 发布周期

- **主版本**：根据需要发布，通常包含重大变更
- **次版本**：每月发布，包含新功能和改进
- **修订版本**：根据需要发布，主要用于 Bug 修复

## 升级指南

### 从 0.x 升级到 1.0

1. **备份数据**：升级前请备份重要数据
2. **检查依赖**：确保 .NET 9.0 已安装
3. **重新安装**：建议完全重新安装应用程序
4. **配置迁移**：配置文件格式可能有变化

### 兼容性说明

- **向前兼容**：新版本保持对旧版本数据的兼容
- **API 稳定性**：1.0 版本后 API 将保持稳定
- **配置文件**：配置文件格式在主版本间可能变化

## 已知问题

### 当前版本 (1.0.0)

- 🔍 **平台兼容性警告**：在 Windows 环境下会显示 CA1416 警告，但不影响功能
- 🔍 **内存使用**：大文件处理时内存使用较高
- 🔍 **启动时间**：首次启动可能较慢

### 计划修复

- 📋 优化内存使用
- 📋 改进启动性能
- 📋 添加进度指示器

## 贡献者

感谢所有为 DevUtilities 项目做出贡献的开发者！

### 核心团队
- **项目维护者**：负责项目整体规划和维护
- **开发团队**：负责功能开发和 Bug 修复
- **测试团队**：负责质量保证和测试

### 社区贡献
- Bug 报告和修复
- 功能建议和实现
- 文档改进
- 翻译工作

## 反馈和支持

### 问题报告
- 🐛 [Bug 报告](https://github.com/yourusername/DevUtilities/issues/new?template=bug_report.md)
- 💡 [功能请求](https://github.com/yourusername/DevUtilities/issues/new?template=feature_request.md)

### 社区支持
- 📖 [文档](https://github.com/yourusername/DevUtilities/wiki)
- 💬 [讨论区](https://github.com/yourusername/DevUtilities/discussions)
- 📧 [联系我们](mailto:support@devutilities.com)

---

**注意**：此更新日志将持续更新，记录项目的所有重要变更。建议定期查看以了解最新动态。