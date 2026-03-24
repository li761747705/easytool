using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTool.TextCategory
{
    /// <summary>
    /// 模板渲染工具类
    /// 支持变量替换、条件渲染、循环等
    /// </summary>
    public static class TemplateUtil
    {
        #region 简单变量替换

        /// <summary>
        /// 渲染模板（使用 ${variable} 或 {{variable}} 语法）
        /// </summary>
        /// <param name="template">模板字符串</param>
        /// <param name="variables">变量字典</param>
        /// <returns>渲染后的字符串</returns>
        public static string Render(string template, Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            if (variables == null || variables.Count == 0)
                return template;

            var result = template;

            // 支持 ${variable} 格式
            result = Regex.Replace(result, @"\$\{(\w+)\}", match =>
            {
                var key = match.Groups[1].Value;
                return variables.TryGetValue(key, out var value) ? value?.ToString() ?? "" : "";
            });

            // 支持 {{variable}} 格式
            result = Regex.Replace(result, @"\{\{(\w+)\}\}", match =>
            {
                var key = match.Groups[1].Value;
                return variables.TryGetValue(key, out var value) ? value?.ToString() ?? "" : "";
            });

            return result;
        }

        /// <summary>
        /// 渲染模板（使用匿名对象）
        /// </summary>
        /// <param name="template">模板字符串</param>
        /// <param name="model">数据模型</param>
        /// <returns>渲染后的字符串</returns>
        public static string Render(string template, object model)
        {
            if (model == null)
                return template;

            var variables = new Dictionary<string, object>();
            var properties = model.GetType().GetProperties();

            foreach (var prop in properties)
            {
                variables[prop.Name] = prop.GetValue(model) ?? "";
            }

            return Render(template, variables);
        }

        #endregion

        #region 带默认值的渲染

        /// <summary>
        /// 渲染模板（支持默认值）
        /// 语法：${variable:default} 或 {{variable|default}}
        /// </summary>
        /// <param name="template">模板字符串</param>
        /// <param name="variables">变量字典</param>
        /// <returns>渲染后的字符串</returns>
        public static string RenderWithDefault(string template, Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = template;

            // ${variable:default} 格式
            result = Regex.Replace(result, @"\$\{(\w+):([^}]*)\}", match =>
            {
                var key = match.Groups[1].Value;
                var defaultValue = match.Groups[2].Value;

                if (variables != null && variables.TryGetValue(key, out var value) && value != null)
                {
                    return value.ToString();
                }

                return defaultValue;
            });

            // {{variable|default}} 格式
            result = Regex.Replace(result, @"\{\{(\w+)\|([^}]*)\}\}", match =>
            {
                var key = match.Groups[1].Value;
                var defaultValue = match.Groups[2].Value;

                if (variables != null && variables.TryGetValue(key, out var value) && value != null)
                {
                    return value.ToString();
                }

                return defaultValue;
            });

            return result;
        }

        #endregion

        #region 条件渲染

        /// <summary>
        /// 条件渲染
        /// 语法：{{#if condition}}...{{/if}}
        ///        {{#if condition}}...{{else}}...{{/if}}
        /// </summary>
        public static string RenderConditional(string template, Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = template;

            // 处理 if-else 结构
            var ifElsePattern = @"\{\{#if\s+(\w+)\}\}(.*?)\{\{else\}\}(.*?)\{\{/if\}\}";
            result = Regex.Replace(result, ifElsePattern, match =>
            {
                var condition = match.Groups[1].Value;
                var trueContent = match.Groups[2].Value;
                var falseContent = match.Groups[3].Value;

                if (variables != null && variables.TryGetValue(condition, out var value))
                {
                    var isTrue = value switch
                    {
                        bool b => b,
                        string s => !string.IsNullOrEmpty(s),
                        int i => i != 0,
                        null => false,
                        _ => true
                    };

                    return isTrue ? trueContent : falseContent;
                }

                return falseContent;
            }, RegexOptions.Singleline);

            // 处理简单 if 结构
            var ifPattern = @"\{\{#if\s+(\w+)\}\}(.*?)\{\{/if\}\}";
            result = Regex.Replace(result, ifPattern, match =>
            {
                var condition = match.Groups[1].Value;
                var content = match.Groups[2].Value;

                if (variables != null && variables.TryGetValue(condition, out var value))
                {
                    var isTrue = value switch
                    {
                        bool b => b,
                        string s => !string.IsNullOrEmpty(s),
                        int i => i != 0,
                        null => false,
                        _ => true
                    };

                    return isTrue ? content : "";
                }

                return "";
            }, RegexOptions.Singleline);

            return result;
        }

        #endregion

        #region 循环渲染

        /// <summary>
        /// 循环渲染
        /// 语法：{{#each items}}...{{this}}...{{/each}}
        /// </summary>
        public static string RenderLoop(string template, Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = template;
            var eachPattern = @"\{\{#each\s+(\w+)\}\}(.*?)\{\{/each\}\}";

            result = Regex.Replace(result, eachPattern, match =>
            {
                var listName = match.Groups[1].Value;
                var itemTemplate = match.Groups[2].Value;

                if (variables == null || !variables.TryGetValue(listName, out var listValue))
                    return "";

                var items = listValue as IEnumerable<object>;
                if (items == null)
                    return "";

                var sb = new StringBuilder();
                var index = 0;

                foreach (var item in items)
                {
                    var itemResult = itemTemplate;

                    // 替换 {{this}}
                    itemResult = itemResult.Replace("{{this}}", item?.ToString() ?? "");

                    // 替换 {{@index}}
                    itemResult = itemResult.Replace("{{@index}}", index.ToString());

                    // 替换 {{@first}}
                    itemResult = itemResult.Replace("{{@first}}", (index == 0).ToString().ToLower());

                    // 替换 {{@last}}
                    var isLast = index == (items as ICollection<object>)?.Count - 1;
                    itemResult = itemResult.Replace("{{@last}}", isLast.ToString().ToLower());

                    // 如果是对象，替换其属性
                    if (item != null && !(item is string))
                    {
                        var props = item.GetType().GetProperties();
                        foreach (var prop in props)
                        {
                            itemResult = itemResult.Replace($"{{{{{prop.Name}}}}}", prop.GetValue(item)?.ToString() ?? "");
                        }
                    }

                    sb.Append(itemResult);
                    index++;
                }

                return sb.ToString();
            }, RegexOptions.Singleline);

            return result;
        }

        #endregion

        #region 完整渲染

        /// <summary>
        /// 完整渲染（包含变量、条件、循环）
        /// </summary>
        /// <param name="template">模板字符串</param>
        /// <param name="variables">变量字典</param>
        /// <returns>渲染后的字符串</returns>
        public static string RenderFull(string template, Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = template;

            // 先处理循环
            result = RenderLoop(result, variables);

            // 再处理条件
            result = RenderConditional(result, variables);

            // 最后处理变量
            result = RenderWithDefault(result, variables);

            return result;
        }

        #endregion

        #region 模板缓存

        private static readonly Dictionary<string, string> _templateCache = new();
        private static readonly object _cacheLock = new();

        /// <summary>
        /// 缓存模板
        /// </summary>
        /// <param name="name">模板名称</param>
        /// <param name="template">模板内容</param>
        public static void CacheTemplate(string name, string template)
        {
            lock (_cacheLock)
            {
                _templateCache[name] = template;
            }
        }

        /// <summary>
        /// 从缓存加载模板
        /// </summary>
        /// <param name="name">模板名称</param>
        /// <returns>模板内容</returns>
        public static string? GetCachedTemplate(string name)
        {
            lock (_cacheLock)
            {
                return _templateCache.TryGetValue(name, out var template) ? template : null;
            }
        }

        /// <summary>
        /// 渲染缓存的模板
        /// </summary>
        /// <param name="name">模板名称</param>
        /// <param name="variables">变量字典</param>
        /// <returns>渲染后的字符串</returns>
        public static string RenderCached(string name, Dictionary<string, object> variables)
        {
            var template = GetCachedTemplate(name);
            if (template == null)
                throw new KeyNotFoundException($"模板 '{name}' 不存在");

            return RenderFull(template, variables);
        }

        /// <summary>
        /// 清除模板缓存
        /// </summary>
        public static void ClearCache()
        {
            lock (_cacheLock)
            {
                _templateCache.Clear();
            }
        }

        #endregion

        #region 文件模板

        /// <summary>
        /// 从文件渲染模板
        /// </summary>
        /// <param name="filePath">模板文件路径</param>
        /// <param name="variables">变量字典</param>
        /// <returns>渲染后的字符串</returns>
        public static string RenderFromFile(string filePath, Dictionary<string, object> variables)
        {
            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException($"模板文件不存在: {filePath}");

            var template = System.IO.File.ReadAllText(filePath);
            return RenderFull(template, variables);
        }

        #endregion
    }
}
