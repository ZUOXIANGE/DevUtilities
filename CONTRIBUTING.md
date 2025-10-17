# 贡献指南

感谢您对 DevUtilities 项目的关注！我们欢迎所有形式的贡献，包括但不限于：

- 🐛 Bug 报告
- 💡 功能建议
- 📝 文档改进
- 🔧 代码贡献
- 🌐 翻译工作

## 📋 目录

- [开发环境设置](#开发环境设置)
- [贡献流程](#贡献流程)
- [代码规范](#代码规范)
- [提交规范](#提交规范)
- [Pull Request 指南](#pull-request-指南)
- [问题报告](#问题报告)
- [功能请求](#功能请求)

## 🛠️ 开发环境设置

### 系统要求

- **.NET 9.0 SDK** 或更高版本
- **Git** 版本控制系统
- **IDE**：Visual Studio 2022、VS Code 或 JetBrains Rider

### 环境配置

1. **Fork 仓库**
   ```bash
   # 在 GitHub 上 Fork 本仓库
   # 然后克隆您的 Fork
   git clone https://github.com/yourusername/DevUtilities.git
   cd DevUtilities
   ```

2. **添加上游仓库**
   ```bash
   git remote add upstream https://github.com/originalowner/DevUtilities.git
   ```

3. **安装依赖**
   ```bash
   dotnet restore
   ```

4. **验证构建**
   ```bash
   dotnet build
   dotnet run
   ```

## 🔄 贡献流程

### 1. 准备工作

```bash
# 同步最新代码
git checkout main
git pull upstream main

# 创建新分支
git checkout -b feature/your-feature-name
# 或
git checkout -b fix/your-bug-fix
```

### 2. 开发阶段

- 编写代码
- 添加测试（如适用）
- 更新文档
- 确保代码符合规范

### 3. 提交更改

```bash
# 添加更改
git add .

# 提交更改（遵循提交规范）
git commit -m "feat: add new feature description"

# 推送到您的 Fork
git push origin feature/your-feature-name
```

### 4. 创建 Pull Request

1. 在 GitHub 上打开您的 Fork
2. 点击 "New Pull Request"
3. 填写 PR 模板
4. 等待代码审查

## 📝 代码规范

### C# 编码规范

我们遵循 Microsoft 的 C# 编码约定：

#### 命名规范

```csharp
// 类名：PascalCase
public class QrCodeViewModel { }

// 方法名：PascalCase
public async Task GenerateQrCodeAsync() { }

// 属性：PascalCase
public string InputText { get; set; }

// 字段：camelCase，私有字段使用下划线前缀
private string _inputText;

// 常量：PascalCase
public const string DefaultEncoding = "UTF-8";

// 接口：I + PascalCase
public interface IQrCodeService { }
```

#### 代码格式

```csharp
// 使用 4 个空格缩进
public class ExampleClass
{
    // 属性之间空一行
    public string Property1 { get; set; }
    
    public string Property2 { get; set; }
    
    // 方法之间空一行
    public void Method1()
    {
        // 方法体
    }
    
    public void Method2()
    {
        // 方法体
    }
}
```

### XAML 规范

```xml
<!-- 属性按字母顺序排列 -->
<Button Command="{Binding GenerateCommand}"
        Content="生成二维码"
        IsEnabled="{Binding CanGenerate}"
        Width="120" />

<!-- 复杂控件使用多行格式 -->
<Grid ColumnDefinitions="*,Auto,*"
      RowDefinitions="Auto,*,Auto">
    <!-- 内容 -->
</Grid>
```

### 项目结构规范

```
DevUtilities/
├── Models/           # 数据模型
├── ViewModels/       # 视图模型（MVVM）
├── Views/           # 用户界面
├── Services/        # 业务服务
├── Converters/      # 数据转换器
├── Assets/          # 资源文件
└── Utils/           # 工具类
```

## 📋 提交规范

我们使用 [Conventional Commits](https://www.conventionalcommits.org/) 规范：

### 提交格式

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### 提交类型

- **feat**: 新功能
- **fix**: Bug 修复
- **docs**: 文档更新
- **style**: 代码格式（不影响功能）
- **refactor**: 代码重构
- **perf**: 性能优化
- **test**: 测试相关
- **chore**: 构建过程或辅助工具的变动

### 示例

```bash
# 新功能
git commit -m "feat(qrcode): add QR code generation with custom colors"

# Bug 修复
git commit -m "fix(crypto): resolve AES encryption padding issue"

# 文档更新
git commit -m "docs: update installation instructions"

# 重构
git commit -m "refactor(viewmodel): extract common base class"
```

## 🔍 Pull Request 指南

### PR 标题

使用与提交信息相同的格式：

```
feat(qrcode): add batch QR code generation
fix(crypto): resolve memory leak in encryption service
docs: add API documentation
```

### PR 描述模板

```markdown
## 📝 变更描述

简要描述此 PR 的目的和内容。

## 🔧 变更类型

- [ ] Bug 修复
- [ ] 新功能
- [ ] 重构
- [ ] 文档更新
- [ ] 性能优化
- [ ] 其他

## 🧪 测试

- [ ] 已添加单元测试
- [ ] 已进行手动测试
- [ ] 所有现有测试通过

## 📸 截图（如适用）

如果涉及 UI 变更，请提供截图。

## 📋 检查清单

- [ ] 代码遵循项目规范
- [ ] 已更新相关文档
- [ ] 已添加必要的测试
- [ ] 提交信息符合规范
```

### 代码审查

所有 PR 都需要经过代码审查：

1. **自动检查**：CI/CD 流水线会自动运行
2. **人工审查**：至少需要一位维护者的批准
3. **反馈处理**：及时响应审查意见

## 🐛 问题报告

### Bug 报告模板

使用 GitHub Issues 报告 Bug，请包含：

```markdown
## 🐛 Bug 描述

清晰简洁地描述 Bug。

## 🔄 复现步骤

1. 打开应用程序
2. 点击 '...'
3. 输入 '...'
4. 看到错误

## 🎯 期望行为

描述您期望发生的行为。

## 📸 截图

如果适用，添加截图来帮助解释问题。

## 🖥️ 环境信息

- OS: [e.g. Windows 11]
- .NET Version: [e.g. 9.0]
- App Version: [e.g. 1.0.0]

## 📋 附加信息

添加任何其他相关信息。
```

## 💡 功能请求

### 功能请求模板

```markdown
## 🚀 功能描述

清晰简洁地描述您想要的功能。

## 🎯 问题背景

描述这个功能要解决的问题。

## 💭 解决方案

描述您希望的解决方案。

## 🔄 替代方案

描述您考虑过的其他替代方案。

## 📋 附加信息

添加任何其他相关信息或截图。
```

## 🌐 国际化

如果您想为项目添加新的语言支持：

1. 在 `Resources` 文件夹中添加新的资源文件
2. 翻译所有字符串
3. 更新语言选择器
4. 测试新语言的显示效果

## 🏷️ 发布流程

### 版本号规范

我们使用 [Semantic Versioning](https://semver.org/)：

- **MAJOR**: 不兼容的 API 更改
- **MINOR**: 向后兼容的功能添加
- **PATCH**: 向后兼容的 Bug 修复

### 发布检查清单

- [ ] 更新版本号
- [ ] 更新 CHANGELOG.md
- [ ] 创建 Git 标签
- [ ] 构建发布版本
- [ ] 更新文档

## 🤝 社区准则

### 行为准则

我们致力于为每个人提供友好、安全和欢迎的环境：

- **尊重他人**：尊重不同的观点和经验
- **建设性反馈**：提供有帮助的、建设性的反馈
- **包容性**：欢迎所有背景的贡献者
- **专业性**：保持专业和礼貌的交流

### 沟通渠道

- **GitHub Issues**: Bug 报告和功能请求
- **GitHub Discussions**: 一般讨论和问题
- **Pull Requests**: 代码审查和讨论

## 📞 获取帮助

如果您在贡献过程中遇到问题：

1. 查看现有的 Issues 和 Discussions
2. 搜索相关文档
3. 创建新的 Issue 或 Discussion
4. 联系项目维护者

## 🙏 致谢

感谢所有为 DevUtilities 项目做出贡献的开发者！

---

**再次感谢您的贡献！** 🎉

每一个贡献，无论大小，都让 DevUtilities 变得更好。