using CommunityToolkit.Mvvm.ComponentModel;

namespace DevUtilities.ViewModels;

// 基础ViewModel类，用于未实现的工具
public partial class BaseToolViewModel : ObservableObject
{
    [ObservableProperty]
    private string message = "此功能正在开发中，敬请期待...";
}

// AI聊天工具
public partial class AiChatViewModel : BaseToolViewModel
{
    public AiChatViewModel()
    {
        Message = "AI聊天功能正在开发中，将支持智能对话和代码助手功能...";
    }
}

// AI翻译工具
public partial class AiTranslateViewModel : BaseToolViewModel
{
    public AiTranslateViewModel()
    {
        Message = "AI翻译功能正在开发中，将支持多语言智能翻译...";
    }
}

// 单位转换器 - 现在有完整实现，移除基础实现
// public partial class UnitConverterViewModel : BaseToolViewModel

// 进制转换器 - 现在有完整实现，移除基础实现
// public partial class BaseConverterViewModel : BaseToolViewModel

// SQL格式化器 - 现在有完整实现，移除基础实现
// public partial class SqlFormatterViewModel : BaseToolViewModel
// {
//     public SqlFormatterViewModel()
//     {
//         Message = "SQL格式化功能正在开发中，将支持SQL语句美化和验证...";
//     }
// }

// HTML格式化器 - 现在有完整实现，移除基础实现
// public partial class HtmlFormatterViewModel : BaseToolViewModel
// {
//     public HtmlFormatterViewModel()
//     {
//         Message = "HTML格式化功能正在开发中，将支持HTML代码美化和验证...";
//     }
// }

// 十六进制转换器 - 现在有完整实现，移除基础实现
// public partial class HexConverterViewModel : BaseToolViewModel
// {
//     public HexConverterViewModel()
//     {
//         Message = "十六进制转换功能正在开发中，将支持十六进制与字符串互转...";
//     }
// }

// JWT编码器 - 现在有完整实现，移除基础实现
// public partial class JwtEncoderViewModel : BaseToolViewModel
// {
//     public JwtEncoderViewModel()
//     {
//         Message = "JWT编码功能正在开发中，将支持JWT令牌的编码、解码和验证...";
//     }
// }

// 正则表达式测试器
// public partial class RegexTesterViewModel : BaseToolViewModel
// {
//     public RegexTesterViewModel()
//     {
//         Message = "正则表达式测试功能正在开发中，将支持正则匹配测试和语法高亮...";
//     }
// }

// UUID生成器 - 现在有完整实现，移除基础实现
// public partial class UuidGeneratorViewModel : BaseToolViewModel

// URL工具 - 现在有完整实现，移除基础实现
// public partial class UrlToolsViewModel : BaseToolViewModel
// {
//     public UrlToolsViewModel()
//     {
//         Message = "URL工具功能正在开发中，将支持URL编码解码和解析...";
//     }
// }

// HTTP请求工具
public partial class HttpRequestViewModel : BaseToolViewModel
{
    public HttpRequestViewModel()
    {
        Message = "HTTP请求功能正在开发中，将支持REST API测试和调试...";
    }
}

// IP查询工具
public partial class IpQueryViewModel : BaseToolViewModel
{
    public IpQueryViewModel()
    {
        Message = "IP查询功能正在开发中，将支持IP地址查询和地理位置定位...";
    }
}

// 二维码工具
// public partial class QrCodeViewModel : BaseToolViewModel
// {
//     public QrCodeViewModel()
//     {
//         Message = "二维码功能正在开发中，将支持二维码生成和扫描...";
//     }
// }

// Parquet查看器
public partial class ParquetViewerViewModel : BaseToolViewModel
{
    public ParquetViewerViewModel()
    {
        Message = "Parquet查看器功能正在开发中，将支持Parquet文件数据预览...";
    }
}

// 加密工具 - 现在有完整实现，移除基础实现
// public partial class CryptoToolsViewModel : BaseToolViewModel
// {
//     public CryptoToolsViewModel()
//     {
//         Message = "加密工具功能正在开发中，将支持各种加密算法和哈希计算...";
//     }
// }

// Base64编码器 - 已有完整实现
// public class Base64EncoderViewModel : ViewModelBase
// {
//     public string Title => "Base64编码器";
// }

// 颜色选择器 - 已有完整实现
// public class ColorPickerViewModel : ViewModelBase
// {
//     public string Title => "颜色选择器";
// }

// 时间戳转换器 - 已有完整实现
// public class TimestampConverterViewModel : ViewModelBase
// {
//     public string Title => "时间戳转换器";
// }