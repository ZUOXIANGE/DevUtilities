using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using DevUtilities.Core.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DevUtilities.ViewModels;

public partial class XmlFormatterViewModel : BaseFormatterViewModel
{
    [ObservableProperty]
    private bool _removeComments = false;

    [ObservableProperty]
    private bool _removeWhitespace = false;

    [ObservableProperty]
    private bool _sortAttributes = false;

    public XmlFormatterViewModel()
    {
        Title = "XML格式化器";
        Description = "XML格式化和验证工具";
        Icon = "📄";
        ToolType = Models.ToolType.XmlFormatter;
        InputPlaceholder = "请输入要格式化的XML内容...";
        OutputPlaceholder = "格式化后的XML将显示在这里...";
    }

    protected override async Task<string> FormatContentAsync(string input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // 对于大文件，使用流式处理
            var inputSize = Encoding.UTF8.GetByteCount(input);
            if (inputSize > 512 * 1024) // 512KB以上使用流式处理
            {
                return await FormatLargeXmlAsync(input);
            }

            // 小文件使用标准处理
            return await FormatStandardXmlAsync(input);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException($"XML格式错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"格式化失败: {ex.Message}", ex);
        }
    }

    protected override async Task<string> OnCompressAsync(string input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            await Task.Delay(1); // 异步操作

            var doc = XDocument.Parse(input);
            
            // 移除所有空白节点
            RemoveWhitespaceNodes(doc.Root);
            
            // 移除注释
            if (RemoveComments)
            {
                doc.DescendantNodes().OfType<XComment>().Remove();
            }

            var settings = new XmlWriterSettings
            {
                Indent = false,
                NewLineHandling = NewLineHandling.None,
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            doc.Save(xmlWriter);
            
            return stringWriter.ToString();
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException($"XML格式错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"压缩失败: {ex.Message}", ex);
        }
    }

    private async Task<string> FormatStandardXmlAsync(string input)
    {
        await Task.Delay(1); // 异步操作

        var doc = XDocument.Parse(input);
        
        // 移除注释
        if (RemoveComments)
        {
            doc.DescendantNodes().OfType<XComment>().Remove();
        }

        // 移除多余空白
        if (RemoveWhitespace)
        {
            RemoveWhitespaceNodes(doc.Root);
        }

        // 排序属性
        if (SortAttributes)
        {
            SortElementAttributes(doc.Root);
        }

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = new string(' ', IndentSize),
            NewLineHandling = NewLineHandling.Replace,
            NewLineChars = "\n",
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);
        doc.Save(xmlWriter);
        
        return stringWriter.ToString();
    }

    private async Task<string> FormatLargeXmlAsync(string input)
    {
        await Task.Delay(1); // 异步操作

        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = RemoveWhitespace,
            IgnoreComments = RemoveComments
        };

        var writerSettings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = new string(' ', IndentSize),
            NewLineHandling = NewLineHandling.Replace,
            NewLineChars = "\n",
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        };

        using var stringReader = new StringReader(input);
        using var xmlReader = XmlReader.Create(stringReader, settings);
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, writerSettings);

        while (xmlReader.Read())
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    xmlWriter.WriteStartElement(xmlReader.LocalName, xmlReader.NamespaceURI);
                    
                    if (xmlReader.HasAttributes)
                    {
                        var attributes = new List<(string Name, string Value, string Namespace)>();
                        
                        while (xmlReader.MoveToNextAttribute())
                        {
                            attributes.Add((xmlReader.LocalName, xmlReader.Value, xmlReader.NamespaceURI));
                        }
                        
                        if (SortAttributes)
                        {
                            attributes = attributes.OrderBy(a => a.Name).ToList();
                        }
                        
                        foreach (var attr in attributes)
                        {
                            xmlWriter.WriteAttributeString(attr.Name, attr.Namespace, attr.Value);
                        }
                        
                        xmlReader.MoveToElement();
                    }
                    
                    if (xmlReader.IsEmptyElement)
                    {
                        xmlWriter.WriteEndElement();
                    }
                    break;
                    
                case XmlNodeType.Text:
                    xmlWriter.WriteString(xmlReader.Value);
                    break;
                    
                case XmlNodeType.CDATA:
                    xmlWriter.WriteCData(xmlReader.Value);
                    break;
                    
                case XmlNodeType.Comment:
                    if (!RemoveComments)
                    {
                        xmlWriter.WriteComment(xmlReader.Value);
                    }
                    break;
                    
                case XmlNodeType.EndElement:
                    xmlWriter.WriteEndElement();
                    break;
                    
                case XmlNodeType.XmlDeclaration:
                    // XmlWriter会自动处理XML声明
                    break;
            }
        }

        return stringWriter.ToString();
    }

    private void RemoveWhitespaceNodes(XElement element)
    {
        if (element == null) return;

        var whitespaceNodes = element.Nodes()
            .OfType<XText>()
            .Where(t => string.IsNullOrWhiteSpace(t.Value))
            .ToList();

        foreach (var node in whitespaceNodes)
        {
            node.Remove();
        }

        foreach (var child in element.Elements())
        {
            RemoveWhitespaceNodes(child);
        }
    }

    private void SortElementAttributes(XElement element)
    {
        if (element == null) return;

        var attributes = element.Attributes().ToList();
        if (attributes.Count > 1)
        {
            element.RemoveAttributes();
            foreach (var attr in attributes.OrderBy(a => a.Name.LocalName))
            {
                element.Add(attr);
            }
        }

        foreach (var child in element.Elements())
        {
            SortElementAttributes(child);
        }
    }

    /// <summary>
    /// 验证XML内容
    /// </summary>
    protected override Task<ValidationResult> OnValidateAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Task.FromResult(new ValidationResult(false, "请输入XML内容"));
        }

        try
        {
            var doc = XDocument.Parse(input);
            var elementCount = doc.Descendants().Count();
            var attributeCount = doc.Descendants().Sum(e => e.Attributes().Count());
            
            return Task.FromResult(new ValidationResult(true, $"XML格式正确 (元素: {elementCount}, 属性: {attributeCount})"));
        }
        catch (XmlException ex)
        {
            return Task.FromResult(new ValidationResult(false, $"XML格式错误: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ValidationResult(false, $"验证失败: {ex.Message}"));
        }
    }
}