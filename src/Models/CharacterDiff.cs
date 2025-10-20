using System.Collections.Generic;
using DevUtilities.ViewModels;

namespace DevUtilities.Models;

/// <summary>
/// 表示字符级别的差异片段
/// </summary>
public class CharacterDiffSegment
{
    /// <summary>
    /// 差异类型
    /// </summary>
    public CharacterDiffType Type { get; set; }
    
    /// <summary>
    /// 文本内容
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// 在原始文本中的起始位置
    /// </summary>
    public int StartIndex { get; set; }
    
    /// <summary>
    /// 在原始文本中的结束位置
    /// </summary>
    public int EndIndex { get; set; }
}

/// <summary>
/// 字符级别差异类型
/// </summary>
public enum CharacterDiffType
{
    /// <summary>
    /// 相同的字符
    /// </summary>
    Unchanged,
    
    /// <summary>
    /// 新增的字符
    /// </summary>
    Added,
    
    /// <summary>
    /// 删除的字符
    /// </summary>
    Deleted,
    
    /// <summary>
    /// 修改的字符
    /// </summary>
    Modified
}

/// <summary>
/// 增强的差异行，包含字符级别的差异信息
/// </summary>
public class EnhancedDiffLine
{
    public int LineNumber { get; set; }
    public string LeftContent { get; set; } = string.Empty;
    public string RightContent { get; set; } = string.Empty;
    public DiffType DiffType { get; set; }
    public int? LeftLineNumber { get; set; }
    public int? RightLineNumber { get; set; }
    
    /// <summary>
    /// 左侧文本的字符级别差异片段
    /// </summary>
    public List<CharacterDiffSegment> LeftCharacterDiffs { get; set; } = new();
    
    /// <summary>
    /// 右侧文本的字符级别差异片段
    /// </summary>
    public List<CharacterDiffSegment> RightCharacterDiffs { get; set; } = new();
}