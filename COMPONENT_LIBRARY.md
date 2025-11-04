# DevUtilities 组件库文档

## 概述

本文档详细描述了 DevUtilities 项目中各个功能模块的界面组件、布局结构和交互设计。每个工具都遵循统一的设计规范，确保用户体验的一致性。

## 组件分类

### 1. 布局组件

#### 主窗口 (MainWindow)
**文件**: `Views/MainWindow.axaml`

**结构**:
```
Grid
├── Home View (工具列表)
│   ├── Header (标题区域)
│   ├── Search Controls (搜索和排序)
│   └── Tools Grid (工具网格)
└── Tool View (具体工具界面)
```

**特性**:
- 窗口尺寸: 1200x800 (最小 800x600)
- 启动位置: 屏幕居中
- 背景色: `#F5F5F5`
- 支持工具切换和返回主页

#### 工具模板 (FormatterViewTemplate)
**文件**: `Views/Shared/FormatterViewTemplate.axaml`

**用途**: 为格式化工具提供统一的界面模板

**结构**:
```
ScrollViewer
└── StackPanel (Spacing="24", Margin="24")
    ├── 标题区域
    ├── 控制面板
    ├── 输入输出区域
    └── 使用说明区域
```

### 2. 输入输出组件

#### 文本输入区域
**样式特征**:
- 字体: `JetBrains Mono,Consolas,Monaco,monospace`
- 字体大小: 14px
- 行高: 20px
- 背景: 透明或卡片背景
- 边框: 圆角 12px

#### 代码输出区域
**特性**:
- 只读文本块 (SelectableTextBlock)
- 等宽字体显示
- 支持文本选择和复制
- 自动换行控制

#### 双栏布局
**应用场景**: 转换类工具
```xml
<Grid ColumnDefinitions="*,20,*">
  <Border Grid.Column="0"><!-- 输入区域 --></Border>
  <Border Grid.Column="2"><!-- 输出区域 --></Border>
</Grid>
```

### 3. 控制组件

#### 选项面板
**布局**: 三列网格布局
```xml
<Grid ColumnDefinitions="*,*,*" RowDefinitions="Auto,Auto,Auto" 
      RowSpacing="16" ColumnSpacing="20">
```

**组件类型**:
- ComboBox: 下拉选择
- CheckBox: 开关选项
- RadioButton: 单选选项
- Slider: 数值调节

#### 操作按钮组
**样式类**:
- `.primary-button`: 主要操作 (蓝色背景)
- `.secondary-button`: 次要操作 (白色背景，蓝色边框)
- `.accent`: Avalonia 内置强调样式

## 功能模块组件

### 1. 格式化工具

#### JSON 格式化器 (JsonFormatterView)
**特色组件**:
- 缩进大小选择器 (ComboBox)
- 排序选项 (CheckBox)
- 压缩/格式化切换 (RadioButton)
- 语法验证指示器

**布局特点**:
- 标题区域: 28px 字体，系统强调色
- 控制面板: 卡片样式，12px 圆角
- 双栏输入输出: 等宽字体，代码高亮

#### SQL 格式化器 (SqlFormatterView)
**特色组件**:
- SQL 方言选择 (ComboBox)
- 关键字大小写选项 (ComboBox)
- 缩进样式选择 (ComboBox)

#### HTML 格式化器 (HtmlFormatterView)
**特色组件**:
- 文档类型选择 (ComboBox)
- 属性格式化选项 (CheckBox)
- 示例 HTML 展示区域

### 2. 编码转换工具

#### Base64 编码器 (Base64EncoderView)
**特色组件**:
- 模式切换 (RadioButton): 文本/图片
- 图片预览区域 (Image + 拖拽提示)
- 文件选择按钮
- 拖拽上传支持

**布局结构**:
```
StackPanel
├── 模式选择区域
├── 输入区域 (文本/图片)
├── 操作按钮组
└── 输出区域
```

#### URL 工具 (UrlToolsView)
**特色组件**:
- 编码类型选择 (ComboBox)
- URL 解析结果展示 (ItemsControl)
- 详细解析结果 (Expander)

### 3. 数据查看工具

#### Parquet 查看器 (ParquetViewerView)
**特色组件**:
- 文件操作按钮组
- 文件信息展示 (Grid 布局)
- 数据表格 (DataGrid)
- 加载进度指示器 (ProgressBar)

**状态管理**:
- 加载状态: 进度条显示
- 错误状态: 红色边框提示
- 空状态: 占位符显示

#### HTTP 请求工具 (HttpRequestView)
**特色组件**:
- 请求方法选择 (ComboBox)
- URL 输入框 (TextBox)
- 请求头编辑器
- 响应结果展示

### 4. 生成工具

#### 密码生成器 (PasswordGeneratorView)
**特色组件**:
- 长度滑块 (Slider)
- 字符类型选择 (CheckBox 组)
- 生成按钮 (突出显示)
- 密码强度指示器

#### UUID 生成器 (UuidGeneratorView)
**特色组件**:
- UUID 版本选择
- 批量生成选项
- 历史记录列表
- 一键复制功能

#### 二维码生成器 (QrCodeView)
**特色组件**:
- 内容输入区域
- 二维码预览 (Image)
- 尺寸调节选项
- 保存功能

### 5. 转换工具

#### 时间戳转换器 (TimestampConverterView)
**特色组件**:
- 时间戳格式选择
- 时区选择器
- 当前时间按钮
- 双向转换支持

#### 进制转换器 (BaseConverterView)
**特色组件**:
- 进制选择器 (2-36进制)
- 多进制同时显示
- 实时转换更新

#### 单位转换器 (UnitConverterView)
**特色组件**:
- 单位类别选择
- 源单位和目标单位选择
- 数值输入验证
- 转换历史记录

### 6. 文本处理工具

#### 正则表达式测试器 (RegexTesterView)
**特色组件**:
- 正则表达式输入
- 测试文本输入
- 匹配结果高亮显示
- 常用正则模板

#### 文本对比工具 (TextDiffView)
**特色组件**:
- 双栏文本输入
- 差异高亮显示
- 行号显示
- 对比模式选择

#### 字符串转义工具 (StringEscapeView)
**特色组件**:
- 转义类型选择
- 双向转换支持
- 特殊字符处理

### 7. 加密工具

#### 加密工具集 (CryptoToolsView)
**特色组件**:
- 算法选择器 (MD5, SHA1, SHA256等)
- 密钥输入 (对称加密)
- 编码格式选择
- 批量处理支持

#### JWT 编码器 (JwtEncoderView)
**特色组件**:
- Header 编辑器 (JSON)
- Payload 编辑器 (JSON)
- 密钥输入
- 算法选择
- Token 验证

### 8. 网络工具

#### IP 查询工具 (IpQueryView)
**特色组件**:
- IP 地址输入验证
- 地理位置信息展示
- ISP 信息显示
- 查询历史记录

### 9. 系统工具

#### Cron 表达式工具 (CronExpressionView)
**特色组件**:
- Cron 表达式输入
- 可视化编辑器
- 执行时间预览
- 表达式验证

#### 颜色选择器 (ColorPickerView)
**特色组件**:
- RGB 滑块组
- 颜色预览区域
- 预设颜色面板
- 颜色历史记录
- 多格式输出 (HEX, RGB, HSL)

#### 十六进制转换器 (HexConverterView)
**特色组件**:
- 十六进制输入
- 文本转换
- 字符编码选择
- 格式化选项

## 通用组件模式

### 1. 卡片容器模式
```xml
<Border Background="{DynamicResource CardBackgroundFillColorDefaultBrush}"
        CornerRadius="12"
        Padding="24"
        BoxShadow="0 2 8 0 #10000000">
```

### 2. 标题区域模式
```xml
<StackPanel Spacing="8">
  <TextBlock Text="工具标题" 
             FontSize="28" 
             FontWeight="Bold" 
             Foreground="{DynamicResource SystemAccentColor}"/>
  <TextBlock Text="工具描述" 
             FontSize="14" 
             Foreground="{DynamicResource TextFillColorSecondaryBrush}"/>
</StackPanel>
```

### 3. 操作按钮组模式
```xml
<StackPanel Orientation="Horizontal" Spacing="12">
  <Button Content="主要操作" Classes="primary-button"/>
  <Button Content="次要操作" Classes="secondary-button"/>
</StackPanel>
```

### 4. 状态指示器模式
```xml
<!-- 加载状态 -->
<ProgressBar IsIndeterminate="{Binding IsLoading}" 
             IsVisible="{Binding IsLoading}"
             Height="4"/>

<!-- 错误状态 -->
<Border IsVisible="{Binding HasError}"
        Background="#FFEBEE"
        BorderBrush="#F44336"
        BorderThickness="1"
        CornerRadius="4">
```

## 响应式设计

### 1. 网格自适应
- 使用 `*` 进行比例分配
- 使用 `Auto` 进行内容自适应
- 设置最小宽度避免过度压缩

### 2. 文本自适应
- 长文本使用 `TextWrapping="Wrap"`
- 设置 `MaxWidth` 控制最大宽度
- 使用 `TextTrimming` 处理溢出

### 3. 滚动容器
- 外层使用 `ScrollViewer`
- 设置合适的 `MaxHeight`
- 控制滚动条显示策略

## 可访问性支持

### 1. 键盘导航
- 合理的 Tab 顺序
- 快捷键支持
- 焦点指示器

### 2. 屏幕阅读器
- 语义化控件使用
- 适当的标签关联
- 状态变化通知

### 3. 高对比度
- 动态资源引用
- 系统主题适配
- 颜色对比度验证

---

*本文档详细描述了项目中各个组件的设计和实现，为开发和维护提供参考。*