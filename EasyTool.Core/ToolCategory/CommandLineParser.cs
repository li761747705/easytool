using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 命令行参数解析器
    /// </summary>
    public class CommandLineParser
    {
        private readonly Dictionary<string, string?> _options = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _arguments = new();
        private readonly HashSet<string> _flags = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 获取选项数量
        /// </summary>
        public int OptionCount => _options.Count;

        /// <summary>
        /// 获取参数数量
        /// </summary>
        public int ArgumentCount => _arguments.Count;

        /// <summary>
        /// 获取标志数量
        /// </summary>
        public int FlagCount => _flags.Count;

        /// <summary>
        /// 解析命令行参数
        /// </summary>
        public static CommandLineParser Parse(string[] args, CommandLineOptions? options = null)
        {
            var parser = new CommandLineParser();
            options ??= new CommandLineOptions();

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.StartsWith("--"))
                {
                    // 长选项 --option 或 --option=value
                    var option = arg.Substring(2);
                    var eqIndex = option.IndexOf('=');

                    if (eqIndex >= 0)
                    {
                        var name = option.Substring(0, eqIndex);
                        var value = option.Substring(eqIndex + 1);
                        parser._options[name] = value;
                    }
                    else if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        parser._options[option] = args[++i];
                    }
                    else
                    {
                        parser._flags.Add(option);
                    }
                }
                else if (arg.StartsWith("-"))
                {
                    // 短选项 -o 或 -o value
                    var option = arg.Substring(1);

                    if (option.Length == 1)
                    {
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                        {
                            parser._options[option] = args[++i];
                        }
                        else
                        {
                            parser._flags.Add(option);
                        }
                    }
                    else
                    {
                        // 组合短选项 -abc 相当于 -a -b -c
                        foreach (var c in option)
                        {
                            parser._flags.Add(c.ToString());
                        }
                    }
                }
                else
                {
                    parser._arguments.Add(arg);
                }
            }

            return parser;
        }

        /// <summary>
        /// 获取选项值
        /// </summary>
        public string? GetOption(string name, string? defaultValue = null)
        {
            return _options.TryGetValue(name, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 获取选项值（转换为指定类型）
        /// </summary>
        public T? GetOption<T>(string name, T? defaultValue = default)
        {
            var value = GetOption(name);
            if (value == null) return defaultValue;

            try
            {
                return (T?)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 检查是否有选项
        /// </summary>
        public bool HasOption(string name)
        {
            return _options.ContainsKey(name) || _flags.Contains(name);
        }

        /// <summary>
        /// 检查是否有标志
        /// </summary>
        public bool HasFlag(string flag)
        {
            return _flags.Contains(flag);
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        public string? GetArgument(int index, string? defaultValue = null)
        {
            return index >= 0 && index < _arguments.Count ? _arguments[index] : defaultValue;
        }

        /// <summary>
        /// 获取参数（转换为指定类型）
        /// </summary>
        public T? GetArgument<T>(int index, T? defaultValue = default)
        {
            var value = GetArgument(index);
            if (value == null) return defaultValue;

            try
            {
                return (T?)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 获取所有参数
        /// </summary>
        public IReadOnlyList<string> GetArguments()
        {
            return _arguments.AsReadOnly();
        }

        /// <summary>
        /// 获取所有选项
        /// </summary>
        public IReadOnlyDictionary<string, string?> GetOptions()
        {
            return _options;
        }

        /// <summary>
        /// 获取所有标志
        /// </summary>
        public IReadOnlyCollection<string> GetFlags()
        {
            return _flags.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// 命令行解析选项
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// 是否允许组合短选项
        /// </summary>
        public bool AllowCombinedShortOptions { get; set; } = true;

        /// <summary>
        /// 是否忽略未知选项
        /// </summary>
        public bool IgnoreUnknownOptions { get; set; } = true;
    }

    /// <summary>
    /// 参数构建器
    /// </summary>
    public class ArgumentBuilder
    {
        private readonly List<string> _args = new();

        /// <summary>
        /// 添加参数
        /// </summary>
        public ArgumentBuilder Add(string value)
        {
            _args.Add(value);
            return this;
        }

        /// <summary>
        /// 添加选项
        /// </summary>
        public ArgumentBuilder AddOption(string name, string? value = null)
        {
            _args.Add($"--{name}");
            if (value != null)
            {
                _args.Add(value);
            }
            return this;
        }

        /// <summary>
        /// 添加短选项
        /// </summary>
        public ArgumentBuilder AddShortOption(char name, string? value = null)
        {
            _args.Add($"-{name}");
            if (value != null)
            {
                _args.Add(value);
            }
            return this;
        }

        /// <summary>
        /// 添加标志
        /// </summary>
        public ArgumentBuilder AddFlag(string name)
        {
            _args.Add($"--{name}");
            return this;
        }

        /// <summary>
        /// 添加多个参数
        /// </summary>
        public ArgumentBuilder AddRange(IEnumerable<string> values)
        {
            _args.AddRange(values);
            return this;
        }

        /// <summary>
        /// 构建参数数组
        /// </summary>
        public string[] Build()
        {
            return _args.ToArray();
        }

        /// <summary>
        /// 构建命令行字符串
        /// </summary>
        public string BuildCommandLine()
        {
            return string.Join(" ", _args.Select(QuoteIfNeeded));
        }

        private static string QuoteIfNeeded(string arg)
        {
            if (arg.Contains(' ') || arg.Contains('"'))
            {
                return $"\"{arg.Replace("\"", "\\\"")}\"";
            }
            return arg;
        }

        public override string ToString()
        {
            return BuildCommandLine();
        }
    }
}
