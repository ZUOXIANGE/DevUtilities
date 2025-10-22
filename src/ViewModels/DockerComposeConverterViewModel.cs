using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class DockerComposeConverterViewModel : ObservableObject
{
    [ObservableProperty]
    private string _dockerRunCommand = string.Empty;

    [ObservableProperty]
    private string _dockerComposeOutput = string.Empty;

    [ObservableProperty]
    private string _serviceName = "app";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private void ConvertToDockerCompose()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(DockerRunCommand))
            {
                StatusMessage = "请输入 docker run 命令";
                return;
            }

            var dockerComposeContent = ParseDockerRunCommand(DockerRunCommand.Trim());
            DockerComposeOutput = dockerComposeContent;
            StatusMessage = "转换成功";
        }
        catch (Exception ex)
        {
            StatusMessage = $"转换失败: {ex.Message}";
            DockerComposeOutput = string.Empty;
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        DockerRunCommand = string.Empty;
        DockerComposeOutput = string.Empty;
        ServiceName = "app";
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private void SwapContent()
    {
        var temp = DockerRunCommand;
        DockerRunCommand = DockerComposeOutput;
        DockerComposeOutput = temp;
    }

    [RelayCommand]
    private async Task CopyInput()
    {
        if (!string.IsNullOrEmpty(DockerRunCommand))
        {
            await CopyToClipboard(DockerRunCommand);
            StatusMessage = "输入内容已复制到剪贴板";
        }
    }

    [RelayCommand]
    private async Task CopyOutput()
    {
        if (!string.IsNullOrEmpty(DockerComposeOutput))
        {
            await CopyToClipboard(DockerComposeOutput);
            StatusMessage = "输出内容已复制到剪贴板";
        }
    }

    private async Task CopyToClipboard(string text)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text);
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"复制失败: {ex.Message}";
        }
    }

    private string ParseDockerRunCommand(string command)
    {
        // 移除 "docker run" 前缀
        var cleanCommand = Regex.Replace(command, @"^\s*docker\s+run\s+", "", RegexOptions.IgnoreCase).Trim();
        
        var sb = new StringBuilder();
        sb.AppendLine("version: '3.8'");
        sb.AppendLine();
        sb.AppendLine("services:");
        sb.AppendLine($"  {ServiceName}:");

        // 解析各种参数
        var parsedOptions = ParseCommandOptions(cleanCommand);

        // 镜像名称
        if (!string.IsNullOrEmpty(parsedOptions.Image))
        {
            sb.AppendLine($"    image: {parsedOptions.Image}");
        }

        // 容器名称
        if (!string.IsNullOrEmpty(parsedOptions.ContainerName))
        {
            sb.AppendLine($"    container_name: {parsedOptions.ContainerName}");
        }

        // 端口映射
        if (parsedOptions.Ports.Any())
        {
            sb.AppendLine("    ports:");
            foreach (var port in parsedOptions.Ports)
            {
                sb.AppendLine($"      - \"{port}\"");
            }
        }

        // 卷挂载
        if (parsedOptions.Volumes.Any())
        {
            sb.AppendLine("    volumes:");
            foreach (var volume in parsedOptions.Volumes)
            {
                sb.AppendLine($"      - {volume}");
            }
        }

        // 环境变量
        if (parsedOptions.Environment.Any())
        {
            sb.AppendLine("    environment:");
            foreach (var env in parsedOptions.Environment)
            {
                sb.AppendLine($"      - {env}");
            }
        }

        // 日志配置
        if (parsedOptions.LogOptions.Any())
        {
            sb.AppendLine("    logging:");
            sb.AppendLine("      driver: \"json-file\"");
            sb.AppendLine("      options:");
            foreach (var logOpt in parsedOptions.LogOptions)
            {
                var parts = logOpt.Split('=', 2);
                if (parts.Length == 2)
                {
                    sb.AppendLine($"        {parts[0]}: \"{parts[1]}\"");
                }
            }
        }

        // 网络
        if (parsedOptions.Networks.Any())
        {
            sb.AppendLine("    networks:");
            foreach (var network in parsedOptions.Networks)
            {
                sb.AppendLine($"      - {network}");
            }
        }

        // 重启策略
        if (!string.IsNullOrEmpty(parsedOptions.RestartPolicy))
        {
            sb.AppendLine($"    restart: {parsedOptions.RestartPolicy}");
        }

        // 工作目录
        if (!string.IsNullOrEmpty(parsedOptions.WorkingDirectory))
        {
            sb.AppendLine($"    working_dir: {parsedOptions.WorkingDirectory}");
        }

        // 用户
        if (!string.IsNullOrEmpty(parsedOptions.User))
        {
            sb.AppendLine($"    user: {parsedOptions.User}");
        }

        // 命令
        if (!string.IsNullOrEmpty(parsedOptions.Command))
        {
            sb.AppendLine($"    command: {parsedOptions.Command}");
        }

        // 分离模式
        if (parsedOptions.Detached)
        {
            sb.AppendLine("    # 原命令使用了 -d/--detach 参数");
        }

        // 交互模式
        if (parsedOptions.Interactive)
        {
            sb.AppendLine("    stdin_open: true");
        }

        // TTY
        if (parsedOptions.Tty)
        {
            sb.AppendLine("    tty: true");
        }

        // 特权模式
        if (parsedOptions.Privileged)
        {
            sb.AppendLine("    privileged: true");
        }

        // 只读文件系统
        if (parsedOptions.ReadOnly)
        {
            sb.AppendLine("    read_only: true");
        }

        // 自动删除容器的注释
        if (parsedOptions.RemoveContainer)
        {
            sb.AppendLine("    # 原命令使用了 --rm 参数，容器停止后会自动删除");
            sb.AppendLine("    # docker-compose 中可以使用 'docker-compose down' 来删除容器");
        }

        // 添加网络定义（如果有自定义网络）
        var customNetworks = parsedOptions.Networks.Where(n => n != "bridge" && n != "host" && n != "none").ToList();
        if (customNetworks.Any())
        {
            sb.AppendLine();
            sb.AppendLine("networks:");
            foreach (var network in customNetworks)
            {
                sb.AppendLine($"  {network}:");
                sb.AppendLine("    external: true");
            }
        }

        return sb.ToString();
    }

    private DockerRunOptions ParseCommandOptions(string command)
    {
        var options = new DockerRunOptions();
        
        // 使用正则表达式解析各种选项
        var tokens = TokenizeCommand(command);
        
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            
            switch (token)
            {
                case "-p" or "--publish":
                    if (i + 1 < tokens.Count)
                    {
                        options.Ports.Add(tokens[++i]);
                    }
                    break;
                    
                case "-v" or "--volume":
                    if (i + 1 < tokens.Count)
                    {
                        options.Volumes.Add(tokens[++i]);
                    }
                    break;
                    
                case "-e" or "--env":
                    if (i + 1 < tokens.Count)
                    {
                        options.Environment.Add(tokens[++i]);
                    }
                    break;
                    
                case "--name":
                    if (i + 1 < tokens.Count)
                    {
                        options.ContainerName = tokens[++i];
                    }
                    break;
                    
                case "--network":
                    if (i + 1 < tokens.Count)
                    {
                        options.Networks.Add(tokens[++i]);
                    }
                    break;
                    
                case "--restart":
                    if (i + 1 < tokens.Count)
                    {
                        options.RestartPolicy = tokens[++i];
                    }
                    break;
                    
                case "-w" or "--workdir":
                    if (i + 1 < tokens.Count)
                    {
                        options.WorkingDirectory = tokens[++i];
                    }
                    break;
                    
                case "-u" or "--user":
                    if (i + 1 < tokens.Count)
                    {
                        options.User = tokens[++i];
                    }
                    break;
                    
                case "-d" or "--detach":
                    options.Detached = true;
                    break;
                    
                case "-i" or "--interactive":
                    options.Interactive = true;
                    break;
                    
                case "-t" or "--tty":
                    options.Tty = true;
                    break;
                    
                case "--privileged":
                    options.Privileged = true;
                    break;
                    
                case "--read-only":
                    options.ReadOnly = true;
                    break;
                    
                case "--log-opt":
                    if (i + 1 < tokens.Count)
                    {
                        options.LogOptions.Add(tokens[++i]);
                    }
                    break;
                    
                case "--rm":
                    // docker-compose 中没有直接对应的选项，添加注释
                    options.RemoveContainer = true;
                    break;
                    
                default:
                    // 如果不是选项，可能是镜像名或命令
                    if (!token.StartsWith("-") && string.IsNullOrEmpty(options.Image))
                    {
                        options.Image = token;
                        // 剩余的参数作为命令
                        if (i + 1 < tokens.Count)
                        {
                            var commandParts = tokens.Skip(i + 1).ToList();
                            options.Command = string.Join(" ", commandParts);
                            break;
                        }
                    }
                    break;
            }
        }
        
        return options;
    }

    private List<string> TokenizeCommand(string command)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        char quoteChar = '"';
        
        for (int i = 0; i < command.Length; i++)
        {
            char c = command[i];
            
            if (!inQuotes && (c == '"' || c == '\''))
            {
                inQuotes = true;
                quoteChar = c;
            }
            else if (inQuotes && c == quoteChar)
            {
                inQuotes = false;
            }
            else if (!inQuotes && char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }
        
        if (current.Length > 0)
        {
            tokens.Add(current.ToString());
        }
        
        return tokens;
    }

    private class DockerRunOptions
    {
        public string Image { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public List<string> Ports { get; set; } = new();
        public List<string> Volumes { get; set; } = new();
        public List<string> Environment { get; set; } = new();
        public List<string> Networks { get; set; } = new();
        public List<string> LogOptions { get; set; } = new();
        public string RestartPolicy { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public bool Detached { get; set; }
        public bool Interactive { get; set; }
        public bool Tty { get; set; }
        public bool Privileged { get; set; }
        public bool ReadOnly { get; set; }
        public bool RemoveContainer { get; set; }
    }
}