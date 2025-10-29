using System;
using System.Collections.Generic;
using DevUtilities.Models;

namespace DevUtilities.Services;

/// <summary>
/// 字符级别差异检测服务
/// </summary>
public class CharacterDiffService
{
    /// <summary>
    /// 计算两个字符串之间的字符级别差异
    /// </summary>
    /// <param name="left">左侧文本</param>
    /// <param name="right">右侧文本</param>
    /// <param name="ignoreCase">是否忽略大小写</param>
    /// <returns>字符级别差异结果</returns>
    public (List<CharacterDiffSegment> leftDiffs, List<CharacterDiffSegment> rightDiffs) 
        ComputeCharacterDiff(string left, string right, bool ignoreCase = false)
    {
        if (string.IsNullOrEmpty(left) && string.IsNullOrEmpty(right))
        {
            return (new List<CharacterDiffSegment>(), new List<CharacterDiffSegment>());
        }

        if (string.IsNullOrEmpty(left))
        {
            return (new List<CharacterDiffSegment>(), 
                   new List<CharacterDiffSegment> 
                   { 
                       new CharacterDiffSegment 
                       { 
                           Type = CharacterDiffType.Added, 
                           Text = right, 
                           StartIndex = 0, 
                           EndIndex = right.Length - 1 
                       } 
                   });
        }

        if (string.IsNullOrEmpty(right))
        {
            return (new List<CharacterDiffSegment> 
                   { 
                       new CharacterDiffSegment 
                       { 
                           Type = CharacterDiffType.Deleted, 
                           Text = left, 
                           StartIndex = 0, 
                           EndIndex = left.Length - 1 
                       } 
                   }, 
                   new List<CharacterDiffSegment>());
        }

        // 使用最长公共子序列算法计算字符级别差异
        var lcs = ComputeLCS(left, right, ignoreCase);
        var leftDiffs = GenerateDiffSegments(left, lcs, true, ignoreCase);
        var rightDiffs = GenerateDiffSegments(right, lcs, false, ignoreCase);

        return (leftDiffs, rightDiffs);
    }

    /// <summary>
    /// 计算最长公共子序列
    /// </summary>
    private List<(int leftIndex, int rightIndex, char character)> ComputeLCS(string left, string right, bool ignoreCase)
    {
        var leftChars = left.ToCharArray();
        var rightChars = right.ToCharArray();
        
        if (ignoreCase)
        {
            leftChars = left.ToLowerInvariant().ToCharArray();
            rightChars = right.ToLowerInvariant().ToCharArray();
        }

        var m = leftChars.Length;
        var n = rightChars.Length;
        var dp = new int[m + 1, n + 1];

        // 构建LCS长度表
        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (leftChars[i - 1] == rightChars[j - 1])
                {
                    dp[i, j] = dp[i - 1, j - 1] + 1;
                }
                else
                {
                    dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                }
            }
        }

        // 回溯构建LCS
        var lcs = new List<(int leftIndex, int rightIndex, char character)>();
        int x = m, y = n;
        
        while (x > 0 && y > 0)
        {
            if (leftChars[x - 1] == rightChars[y - 1])
            {
                lcs.Insert(0, (x - 1, y - 1, left[x - 1])); // 使用原始字符
                x--;
                y--;
            }
            else if (dp[x - 1, y] > dp[x, y - 1])
            {
                x--;
            }
            else
            {
                y--;
            }
        }

        return lcs;
    }

    /// <summary>
    /// 根据LCS生成差异片段
    /// </summary>
    private List<CharacterDiffSegment> GenerateDiffSegments(string text, 
        List<(int leftIndex, int rightIndex, char character)> lcs, 
        bool isLeft, bool ignoreCase)
    {
        var segments = new List<CharacterDiffSegment>();
        var currentIndex = 0;

        foreach (var (leftIndex, rightIndex, character) in lcs)
        {
            var targetIndex = isLeft ? leftIndex : rightIndex;

            // 添加差异部分
            if (currentIndex < targetIndex)
            {
                var diffText = text.Substring(currentIndex, targetIndex - currentIndex);
                segments.Add(new CharacterDiffSegment
                {
                    Type = isLeft ? CharacterDiffType.Deleted : CharacterDiffType.Added,
                    Text = diffText,
                    StartIndex = currentIndex,
                    EndIndex = targetIndex - 1
                });
            }

            // 添加相同部分
            segments.Add(new CharacterDiffSegment
            {
                Type = CharacterDiffType.Unchanged,
                Text = character.ToString(),
                StartIndex = targetIndex,
                EndIndex = targetIndex
            });

            currentIndex = targetIndex + 1;
        }

        // 添加剩余的差异部分
        if (currentIndex < text.Length)
        {
            var diffText = text.Substring(currentIndex);
            segments.Add(new CharacterDiffSegment
            {
                Type = isLeft ? CharacterDiffType.Deleted : CharacterDiffType.Added,
                Text = diffText,
                StartIndex = currentIndex,
                EndIndex = text.Length - 1
            });
        }

        return segments;
    }
}