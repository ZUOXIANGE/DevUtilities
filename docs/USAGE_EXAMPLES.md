# DevUtilities 使用示例

本文档提供了 DevUtilities 中各个工具的详细使用示例和最佳实践。

## 📋 目录

- [文本处理工具](#文本处理工具)
- [数据转换工具](#数据转换工具)
- [加密解密工具](#加密解密工具)
- [网络工具](#网络工具)
- [开发工具](#开发工具)
- [实用工具](#实用工具)

## 🔤 文本处理工具

### JSON 格式化工具

#### 基本用法
```json
// 输入（压缩的 JSON）
{"name":"张三","age":25,"skills":["JavaScript","Python","C#"],"address":{"city":"北京","district":"朝阳区"}}

// 输出（格式化后）
{
  "name": "张三",
  "age": 25,
  "skills": [
    "JavaScript",
    "Python",
    "C#"
  ],
  "address": {
    "city": "北京",
    "district": "朝阳区"
  }
}
```

#### 高级功能
- **压缩模式**: 移除所有空白字符
- **排序键**: 按字母顺序排列对象键
- **验证**: 检查 JSON 语法错误
- **路径查询**: 使用 JSONPath 查询特定数据

#### 使用场景
- API 响应数据格式化
- 配置文件美化
- JSON 数据验证和调试

### XML 格式化工具

#### 基本用法
```xml
<!-- 输入（压缩的 XML） -->
<root><person><name>张三</name><age>25</age></person></root>

<!-- 输出（格式化后） -->
<?xml version="1.0" encoding="UTF-8"?>
<root>
  <person>
    <name>张三</name>
    <age>25</age>
  </person>
</root>
```

#### 功能特性
- 自动缩进和换行
- XML 语法验证
- 命名空间处理
- 属性格式化

### SQL 格式化工具

#### 基本用法
```sql
-- 输入（压缩的 SQL）
SELECT u.id,u.name,p.title FROM users u JOIN posts p ON u.id=p.user_id WHERE u.active=1 ORDER BY p.created_at DESC

-- 输出（格式化后）
SELECT 
    u.id,
    u.name,
    p.title
FROM users u
JOIN posts p ON u.id = p.user_id
WHERE u.active = 1
ORDER BY p.created_at DESC
```

#### 支持的 SQL 方言
- MySQL
- PostgreSQL
- SQL Server
- Oracle
- SQLite

## 🔄 数据转换工具

### Base64 编码/解码

#### 文本编码
```
输入文本: Hello, 世界!
Base64 编码: SGVsbG8sIOS4lueVjCE=
解码结果: Hello, 世界!
```

#### 文件编码
- 支持图片、文档等文件的 Base64 编码
- 可以直接拖拽文件到工具中
- 支持批量处理

#### 使用场景
- 在 JSON/XML 中嵌入二进制数据
- 邮件附件编码
- 数据传输编码

### URL 编码/解码

#### 基本用法
```
原始 URL: https://example.com/search?q=编程 教程&type=video
编码后: https://example.com/search?q=%E7%BC%96%E7%A8%8B%20%E6%95%99%E7%A8%8B&type=video
解码后: https://example.com/search?q=编程 教程&type=video
```

#### 功能特性
- 支持完整 URL 编码
- 仅编码查询参数
- 批量处理多个 URL

### HTML 编码/解码

#### 基本用法
```html
<!-- 原始 HTML -->
<div class="content">这是一个 <strong>重要</strong> 的消息</div>

<!-- HTML 实体编码 -->
&lt;div class=&quot;content&quot;&gt;这是一个 &lt;strong&gt;重要&lt;/strong&gt; 的消息&lt;/div&gt;

<!-- 解码后 -->
<div class="content">这是一个 <strong>重要</strong> 的消息</div>
```

## 🔐 加密解密工具

### MD5 哈希

#### 文本哈希
```
输入: Hello World
MD5: b10a8db164e0754105b7a99be72e3fe5
```

#### 文件哈希
- 支持拖拽文件计算 MD5
- 可用于文件完整性验证
- 支持大文件处理

### SHA 哈希系列

#### SHA-1
```
输入: Hello World
SHA-1: 0a4d55a8d778e5022fab701977c5d840bbc486d0
```

#### SHA-256
```
输入: Hello World
SHA-256: a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e
```

#### SHA-512
```
输入: Hello World
SHA-512: 2c74fd17edafd80e8447b0d46741ee243b7eb74dd2149a0ab1b9246fb30382f27e853d8585719e0e67cbda0daa8f51671064615d645ae27acb15bfb1447f459b
```

### AES 加密/解密

#### 基本用法
```
明文: 这是需要加密的敏感信息
密钥: mySecretKey123456
加密结果: U2FsdGVkX1+8QGqjQJ5/8vQjQJ5/8vQjQJ5/8vQjQJ5
解密结果: 这是需要加密的敏感信息
```

#### 支持的模式
- ECB (Electronic Codebook)
- CBC (Cipher Block Chaining)
- CFB (Cipher Feedback)
- OFB (Output Feedback)

## 🌐 网络工具

### HTTP 请求工具

#### GET 请求示例
```http
GET https://api.github.com/users/octocat
Headers:
  User-Agent: DevUtilities/1.0
  Accept: application/json

Response:
{
  "login": "octocat",
  "id": 1,
  "name": "The Octocat",
  "company": "GitHub"
}
```

#### POST 请求示例
```http
POST https://httpbin.org/post
Headers:
  Content-Type: application/json
  Authorization: Bearer token123

Body:
{
  "name": "测试用户",
  "email": "test@example.com"
}

Response:
{
  "json": {
    "name": "测试用户",
    "email": "test@example.com"
  },
  "headers": {
    "Content-Type": "application/json"
  }
}
```

#### 功能特性
- 支持所有 HTTP 方法 (GET, POST, PUT, DELETE, etc.)
- 自定义请求头
- 请求体支持 JSON, XML, Form Data
- 响应历史记录
- 导出为 cURL 命令

### URL 工具集

#### URL 解析
```
输入 URL: https://user:pass@example.com:8080/path/to/resource?param1=value1&param2=value2#section

解析结果:
- 协议: https
- 用户名: user
- 密码: pass
- 主机: example.com
- 端口: 8080
- 路径: /path/to/resource
- 查询参数: param1=value1, param2=value2
- 锚点: section
```

#### URL 构建
通过表单输入各个组件，自动生成完整的 URL。

## 🛠️ 开发工具

### 二维码生成器

#### 文本二维码
```
输入文本: https://github.com/DevUtilities
生成二维码: [QR Code Image]
格式: PNG, SVG, PDF
尺寸: 256x256 像素
```

#### 高级选项
- 错误纠正级别: L, M, Q, H
- 边框大小: 0-10 像素
- 前景/背景颜色自定义
- Logo 嵌入支持

### JWT 令牌工具

#### JWT 解码
```json
// JWT Token
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c

// 解码后的 Header
{
  "alg": "HS256",
  "typ": "JWT"
}

// 解码后的 Payload
{
  "sub": "1234567890",
  "name": "John Doe",
  "iat": 1516239022
}
```

#### JWT 生成
- 自定义 Header 和 Payload
- 支持多种签名算法 (HS256, RS256, etc.)
- 过期时间设置
- 签名验证

### Cron 表达式工具

#### 表达式解析
```
Cron 表达式: 0 30 14 * * MON-FRI
解释: 每个工作日的下午 2:30 执行

下次执行时间:
- 2024-12-20 14:30:00
- 2024-12-23 14:30:00
- 2024-12-24 14:30:00
```

#### 可视化编辑器
- 分钟、小时、日期、月份、星期的下拉选择
- 实时预览执行时间
- 常用表达式模板

## 🎨 实用工具

### 颜色选择器

#### RGB 颜色
```
RGB: (255, 128, 0)
HEX: #FF8000
HSL: (30°, 100%, 50%)
HSV: (30°, 100%, 100%)
CMYK: (0%, 50%, 100%, 0%)
```

#### 功能特性
- 实时颜色预览
- 多种颜色格式转换
- 颜色历史记录
- 预设颜色面板
- 取色器功能

### 时间戳转换

#### Unix 时间戳
```
时间戳: 1703001600
转换结果: 2023-12-20 00:00:00 (UTC)
本地时间: 2023-12-20 08:00:00 (UTC+8)
```

#### 支持格式
- Unix 时间戳 (秒)
- Unix 时间戳 (毫秒)
- ISO 8601 格式
- 自定义日期格式

### 正则表达式测试

#### 基本用法
```
正则表达式: \b\w+@\w+\.\w+\b
测试文本: 
联系我们：support@example.com 或 admin@test.org
如有问题请发邮件到 invalid-email

匹配结果:
✓ support@example.com (位置: 4-21)
✓ admin@test.org (位置: 24-36)
```

#### 功能特性
- 实时匹配高亮
- 捕获组显示
- 常用正则表达式库
- 替换功能
- 性能分析

### 文本差异对比

#### 基本对比
```
文本 A:
Hello World
This is line 2
This is line 3

文本 B:
Hello Universe
This is line 2
This is line 4

差异结果:
- Hello World
+ Hello Universe
  This is line 2
- This is line 3
+ This is line 4
```

#### 对比模式
- 逐行对比
- 逐字符对比
- 忽略空白字符
- 忽略大小写

## 📊 数据处理工具

### Parquet 文件查看器

#### 文件信息
```
文件: data.parquet
大小: 2.5 MB
行数: 10,000
列数: 15
压缩: SNAPPY
```

#### 数据预览
| ID | Name | Age | City | Salary |
|----|------|-----|------|--------|
| 1 | 张三 | 25 | 北京 | 8000 |
| 2 | 李四 | 30 | 上海 | 12000 |
| 3 | 王五 | 28 | 深圳 | 10000 |

#### 功能特性
- 数据分页浏览
- 列排序和筛选
- 数据类型显示
- 导出为 CSV/JSON

## 💡 使用技巧

### 快捷键
- `Ctrl + N`: 新建/清空当前工具
- `Ctrl + O`: 打开文件
- `Ctrl + S`: 保存结果
- `Ctrl + C`: 复制结果
- `Ctrl + V`: 粘贴输入
- `Ctrl + Z`: 撤销操作
- `Ctrl + ,`: 打开设置
- `F11`: 全屏模式

### 批量操作
1. **文件拖拽**: 支持拖拽多个文件进行批量处理
2. **历史记录**: 所有操作都会保存历史记录
3. **导出功能**: 可以将结果导出为文件
4. **模板保存**: 常用的配置可以保存为模板

### 性能优化
1. **大文件处理**: 使用流式处理，支持 GB 级文件
2. **内存管理**: 自动释放不需要的资源
3. **并行处理**: 多核 CPU 并行计算
4. **缓存机制**: 智能缓存提高响应速度

## 🔧 故障排除

### 常见问题

#### 工具无响应
1. 检查输入数据格式是否正确
2. 对于大文件，请耐心等待处理完成
3. 重启应用程序

#### 结果不正确
1. 验证输入参数设置
2. 检查数据编码格式
3. 查看错误提示信息

#### 性能问题
1. 关闭不需要的其他应用程序
2. 增加系统内存
3. 使用 SSD 硬盘提高 I/O 性能

---

*更多使用技巧和最佳实践，请参考 [官方文档](https://devutilities.com/docs) 或加入我们的 [社区讨论](https://github.com/DevUtilities/discussions)。*