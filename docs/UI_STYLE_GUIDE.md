# DevUtilities 界面样式指南

## 概述

DevUtilities 采用现代化的 Fluent Design 设计语言，基于 Avalonia UI 框架构建。本文档详细描述了项目的界面样式规范、组件设计原则和布局标准。

## 设计原则

### 1. 一致性 (Consistency)
- 所有工具界面保持统一的视觉风格
- 相同功能的组件使用相同的样式
- 统一的间距、字体和颜色规范

### 2. 简洁性 (Simplicity)
- 界面简洁明了，避免不必要的装饰
- 突出核心功能，减少视觉干扰
- 合理的信息层次结构

### 3. 可用性 (Usability)
- 直观的操作流程
- 清晰的视觉反馈
- 良好的响应式设计

## 主题配置

### 基础主题
- **主题系统**: Avalonia Fluent Theme
- **主题变体**: Default (支持系统主题切换)
- **背景色**: `#F5F5F5` (浅灰色背景)

### 颜色规范

#### 主色调
- **主色**: `#4A90E2` (蓝色)
- **主色悬停**: `#357ABD` (深蓝色)
- **强调色**: `{DynamicResource SystemAccentColor}` (系统强调色)

#### 背景色
- **窗口背景**: `#F5F5F5`
- **卡片背景**: `{DynamicResource CardBackgroundFillColorDefaultBrush}`
- **控件背景**: `{DynamicResource ControlFillColorDefaultBrush}`
- **输入框背景**: `White`

#### 文本颜色
- **主文本**: `#333333`
- **次要文本**: `#666666`
- **辅助文本**: `{DynamicResource TextFillColorSecondaryBrush}`

#### 边框颜色
- **默认边框**: `#E0E0E0`
- **悬停边框**: `#4A90E2`

## 组件样式规范

### 1. 按钮组件

#### 工具按钮 (.tool-button)
```xml
<Style Selector="Button.tool-button">
  <Setter Property="Background" Value="White"/>
  <Setter Property="BorderBrush" Value="#E0E0E0"/>
  <Setter Property="BorderThickness" Value="1"/>
  <Setter Property="CornerRadius" Value="8"/>
  <Setter Property="Padding" Value="16,16"/>
  <Setter Property="Margin" Value="8"/>
  <Setter Property="Height" Value="140"/>
  <Setter Property="Width" Value="160"/>
</Style>
```

#### 主要按钮 (.primary-button)
```xml
<Style Selector="Button.primary-button">
  <Setter Property="Background" Value="#4A90E2"/>
  <Setter Property="Foreground" Value="White"/>
  <Setter Property="BorderThickness" Value="0"/>
  <Setter Property="CornerRadius" Value="4"/>
  <Setter Property="Padding" Value="16,8"/>
</Style>
```

#### 次要按钮 (.secondary-button)
```xml
<Style Selector="Button.secondary-button">
  <Setter Property="Background" Value="White"/>
  <Setter Property="Foreground" Value="#4A90E2"/>
  <Setter Property="BorderBrush" Value="#4A90E2"/>
  <Setter Property="BorderThickness" Value="1"/>
  <Setter Property="CornerRadius" Value="4"/>
</Style>
```

### 2. 文本组件

#### 工具标题 (.tool-title)
- **字体大小**: 14px
- **字重**: SemiBold
- **颜色**: `#333333`
- **对齐**: 居中

#### 工具描述 (.tool-description)
- **字体大小**: 11px
- **颜色**: `#666666`
- **对齐**: 居中
- **最大宽度**: 130px
- **高度**: 32px

#### 页面标题
- **字体大小**: 24-28px
- **字重**: Bold
- **颜色**: `{DynamicResource SystemAccentColor}`

#### 节标题
- **字体大小**: 16-18px
- **字重**: SemiBold

### 3. 输入组件

#### 输入框 (.input-field)
```xml
<Style Selector="TextBox.input-field">
  <Setter Property="Padding" Value="12"/>
  <Setter Property="BorderBrush" Value="#E0E0E0"/>
  <Setter Property="BorderThickness" Value="1"/>
  <Setter Property="CornerRadius" Value="4"/>
  <Setter Property="Background" Value="White"/>
</Style>
```

### 4. 容器组件

#### 卡片容器
- **背景**: `{DynamicResource CardBackgroundFillColorDefaultBrush}`
- **圆角**: 8-12px
- **内边距**: 16-24px
- **阴影**: `0 2 8 0 #10000000` (可选)

#### 内容面板 (.tool-content)
- **背景**: `White`
- **外边距**: 16px

## 布局规范

### 1. 间距系统

#### 标准间距
- **小间距**: 8px
- **中间距**: 16px
- **大间距**: 24px
- **超大间距**: 32px

#### 组件间距
- **StackPanel Spacing**: 16-24px
- **Grid RowSpacing/ColumnSpacing**: 16-20px
- **按钮组间距**: 10-12px

### 2. 页面布局

#### 标准页面结构
```xml
<ScrollViewer>
  <StackPanel Spacing="24" Margin="24">
    <!-- 标题区域 -->
    <StackPanel Spacing="8">
      <TextBlock Text="工具标题" FontSize="28" FontWeight="Bold"/>
      <TextBlock Text="工具描述" FontSize="14"/>
    </StackPanel>
    
    <!-- 控制面板 -->
    <Border Background="{DynamicResource CardBackgroundFillColorDefaultBrush}"
            CornerRadius="12" Padding="24">
      <!-- 控制选项 -->
    </Border>
    
    <!-- 输入输出区域 -->
    <Grid ColumnDefinitions="*,20,*">
      <!-- 输入区域 -->
      <Border Grid.Column="0"/>
      <!-- 输出区域 -->
      <Border Grid.Column="2"/>
    </Grid>
  </StackPanel>
</ScrollViewer>
```

### 3. 响应式设计

#### 窗口尺寸
- **默认尺寸**: 1200x800
- **最小尺寸**: 800x600
- **启动位置**: 屏幕居中

#### 网格布局
- **三列布局**: `ColumnDefinitions="*,*,*"`
- **两列布局**: `ColumnDefinitions="*,20,*"`
- **自适应**: 使用 `*` 和 `Auto` 进行自适应

## 字体规范

### 1. 字体族
- **代码字体**: `JetBrains Mono,Consolas,Monaco,monospace`
- **界面字体**: 系统默认字体

### 2. 字体大小层级
- **超大标题**: 28px
- **大标题**: 24px
- **中标题**: 18px
- **小标题**: 16px
- **正文**: 14px
- **小文本**: 12px
- **辅助文本**: 11px

### 3. 字重规范
- **Bold**: 用于主标题
- **SemiBold**: 用于节标题和重要文本
- **Medium**: 用于标签文本
- **Regular**: 用于正文内容

## 图标规范

### 1. 图标风格
- **风格**: Emoji 图标 + 文本描述
- **大小**: 20px (标题区域), 48px (占位区域)
- **颜色**: 继承文本颜色

### 2. 常用图标
- **信息提示**: 💡
- **文件操作**: 📁
- **示例代码**: 📝
- **设置**: ⚙️
- **搜索**: 🔍

## 动画和交互

### 1. 悬停效果
- **按钮悬停**: 背景色变化 + 边框色变化
- **过渡时间**: 200ms
- **缓动函数**: ease-in-out

### 2. 状态反馈
- **加载状态**: ProgressBar (高度4px)
- **错误状态**: 红色边框 + 错误背景色
- **成功状态**: 绿色提示

## 可访问性

### 1. 颜色对比度
- **文本对比度**: 至少 4.5:1
- **大文本对比度**: 至少 3:1

### 2. 键盘导航
- **Tab 顺序**: 逻辑顺序
- **焦点指示**: 清晰的焦点边框

### 3. 屏幕阅读器
- **语义化标签**: 使用适当的控件类型
- **替代文本**: 为图标提供文本描述

## 最佳实践

### 1. 代码组织
- **样式复用**: 使用样式类而非内联样式
- **资源引用**: 使用动态资源引用主题色彩
- **命名规范**: 使用语义化的样式类名

### 2. 性能优化
- **虚拟化**: 长列表使用虚拟化
- **资源管理**: 及时释放不需要的资源
- **图片优化**: 使用 SVG 格式图标

### 3. 维护性
- **文档更新**: 样式变更时同步更新文档
- **版本控制**: 重大样式变更记录版本
- **测试验证**: 在不同主题下测试界面效果

## 工具特定样式

### 1. 格式化工具
- **输入输出区域**: 等宽字体，代码高亮
- **选项面板**: 网格布局，三列对齐
- **示例区域**: 折叠面板，代码展示

### 2. 转换工具
- **双栏布局**: 输入输出并排显示
- **转换按钮**: 居中放置，突出显示
- **历史记录**: 列表形式，支持选择

### 3. 查看工具
- **数据表格**: 虚拟化，支持排序筛选
- **文件信息**: 网格布局，标签对齐
- **预览区域**: 自适应大小，支持缩放

---

*本文档将随着项目发展持续更新，确保界面设计的一致性和现代化。*