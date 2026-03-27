using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 模板工具类
    /// </summary>
    public static class TemplateUtil
    {
        /// <summary>
        /// 渲染模板（使用 ${var} 语法）
        /// </summary>
        public static string Render(string template, IDictionary<string, object?> variables)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = new StringBuilder(template);
            var start = 0;

            while ((start = result.ToString().IndexOf("${", start)) >= 0)
            {
                var end = result.ToString().IndexOf("}", start);
                if (end < 0) break;

                var varName = result.ToString().Substring(start + 2, end - start - 2).Trim();
                if (variables.TryGetValue(varName, out var value))
                {
                    result.Remove(start, end - start + 1);
                    result.Insert(start, value?.ToString() ?? "");
                }
                else
                {
                    start = end + 1;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 渲染模板（使用匿名对象）
        /// </summary>
        public static string Render(string template, object model)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var dict = new Dictionary<string, object?>();
            foreach (var prop in model.GetType().GetProperties())
            {
                dict[prop.Name] = prop.GetValue(model);
            }

            return Render(template, dict);
        }

        /// <summary>
        /// 渲染模板（使用 {{var}} 语法）
        /// </summary>
        public static string RenderMustache(string template, IDictionary<string, object?> variables)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = new StringBuilder(template);
            var start = 0;

            while ((start = result.ToString().IndexOf("{{", start)) >= 0)
            {
                var end = result.ToString().IndexOf("}}", start);
                if (end < 0) break;

                var varName = result.ToString().Substring(start + 2, end - start - 2).Trim();
                if (variables.TryGetValue(varName, out var value))
                {
                    result.Remove(start, end - start + 2);
                    result.Insert(start, value?.ToString() ?? "");
                }
                else
                {
                    start = end + 2;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 渲染模板（带默认值）
        /// </summary>
        public static string Render(string template, IDictionary<string, object?> variables, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = new StringBuilder(template);
            var start = 0;

            while ((start = result.ToString().IndexOf("${", start)) >= 0)
            {
                var end = result.ToString().IndexOf("}", start);
                if (end < 0) break;

                var varName = result.ToString().Substring(start + 2, end - start - 2).Trim();

                // 检查是否有默认值 (var:default)
                string? defaultValueLocal = null;
                var colonIndex = varName.IndexOf(':');
                if (colonIndex > 0)
                {
                    defaultValueLocal = varName.Substring(colonIndex + 1);
                    varName = varName.Substring(0, colonIndex);
                }

                object? value;
                if (variables.TryGetValue(varName, out value))
                {
                    result.Remove(start, end - start + 1);
                    result.Insert(start, value?.ToString() ?? defaultValueLocal ?? defaultValue);
                }
                else
                {
                    result.Remove(start, end - start + 1);
                    result.Insert(start, defaultValueLocal ?? defaultValue);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 提取模板中的变量名
        /// </summary>
        public static List<string> ExtractVariables(string template, string startTag = "${", string endTag = "}")
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(template))
                return result;

            var start = 0;
            while ((start = template.IndexOf(startTag, start)) >= 0)
            {
                var end = template.IndexOf(endTag, start + startTag.Length);
                if (end < 0) break;

                var varName = template.Substring(start + startTag.Length, end - start - startTag.Length).Trim();
                if (!string.IsNullOrEmpty(varName))
                {
                    // 移除默认值部分
                    var colonIndex = varName.IndexOf(':');
                    if (colonIndex > 0)
                        varName = varName.Substring(0, colonIndex);

                    if (!result.Contains(varName))
                        result.Add(varName);
                }

                start = end + endTag.Length;
            }

            return result;
        }

        /// <summary>
        /// 验证模板是否有未替换的变量
        /// </summary>
        public static bool HasUnresolvedVariables(string template, string startTag = "${", string endTag = "}")
        {
            return template.Contains(startTag) && template.Contains(endTag);
        }

        /// <summary>
        /// 格式化字符串（类似Python的f-string）
        /// </summary>
        public static string Format(string template, params object[] args)
        {
            if (string.IsNullOrEmpty(template) || args == null || args.Length == 0)
                return template;

            var result = new StringBuilder(template);
            for (int i = 0; i < args.Length; i++)
            {
                result.Replace($"{{{i}}}", args[i]?.ToString() ?? "");
            }

            return result.ToString();
        }

        /// <summary>
        /// 条件渲染
        /// </summary>
        public static string RenderConditional(string template, IDictionary<string, object?> variables)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = new StringBuilder(template);

            // 处理 {?condition}...{?} 条件块
            var start = 0;
            while ((start = result.ToString().IndexOf("{?", start)) >= 0)
            {
                var endCondition = result.ToString().IndexOf("}", start);
                if (endCondition < 0) break;

                var condition = result.ToString().Substring(start + 2, endCondition - start - 2).Trim();
                var endBlock = result.ToString().IndexOf("{?}", endCondition);
                if (endBlock < 0) break;

                var content = result.ToString().Substring(endCondition + 1, endBlock - endCondition - 1);

                bool shouldInclude = false;
                if (variables.TryGetValue(condition, out var value))
                {
                    shouldInclude = value is bool b ? b : value != null;
                }

                result.Remove(start, endBlock - start + 3);
                if (shouldInclude)
                {
                    result.Insert(start, content);
                }
            }

            // 渲染变量
            return Render(result.ToString(), variables);
        }

        /// <summary>
        /// 循环渲染
        /// </summary>
        public static string RenderLoop(string template, string varName, IEnumerable<object> items)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = new StringBuilder();

            // 找到循环块 {#var}...{/var}
            var startTag = $"{{#{varName}}}";
            var endTag = $"{{/{varName}}}";

            var start = template.IndexOf(startTag);
            if (start < 0) return template;

            var end = template.IndexOf(endTag, start);
            if (end < 0) return template;

            var prefix = template.Substring(0, start);
            var loopTemplate = template.Substring(start + startTag.Length, end - start - startTag.Length);
            var suffix = template.Substring(end + endTag.Length);

            result.Append(prefix);

            foreach (var item in items)
            {
                var dict = new Dictionary<string, object?>
                {
                    [varName] = item
                };

                // 如果item是匿名对象，展开其属性
                if (item != null)
                {
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        dict[$"{varName}.{prop.Name}"] = prop.GetValue(item);
                    }
                }

                result.Append(Render(loopTemplate, dict));
            }

            result.Append(suffix);
            return result.ToString();
        }
    }
}
