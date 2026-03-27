using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTool.AICategory
{
    /// <summary>
    /// 提示词构建器
    /// 提供构建和管理 AI 提示词的工具
    /// </summary>
    public class PromptBuilder
    {
        private readonly StringBuilder _systemPrompt = new();
        private readonly List<string> _examples = new();
        private readonly List<string> _context = new();
        private readonly List<string> _constraints = new();
        private string? _task;
        private string? _outputFormat;

        /// <summary>
        /// 设置系统提示词
        /// </summary>
        /// <param name="systemPrompt">系统提示词</param>
        /// <returns>当前实例</returns>
        public PromptBuilder SetSystemPrompt(string systemPrompt)
        {
            _systemPrompt.Clear();
            _systemPrompt.Append(systemPrompt);
            return this;
        }

        /// <summary>
        /// 添加系统提示词
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>当前实例</returns>
        public PromptBuilder AddSystemPrompt(string text)
        {
            _systemPrompt.AppendLine(text);
            return this;
        }

        /// <summary>
        /// 设置任务描述
        /// </summary>
        /// <param name="task">任务描述</param>
        /// <returns>当前实例</returns>
        public PromptBuilder SetTask(string task)
        {
            _task = task;
            return this;
        }

        /// <summary>
        /// 添加示例
        /// </summary>
        /// <param name="input">输入示例</param>
        /// <param name="output">输出示例</param>
        /// <returns>当前实例</returns>
        public PromptBuilder AddExample(string input, string output)
        {
            _examples.Add($"输入: {input}\n输出: {output}");
            return this;
        }

        /// <summary>
        /// 添加上下文
        /// </summary>
        /// <param name="context">上下文内容</param>
        /// <returns>当前实例</returns>
        public PromptBuilder AddContext(string context)
        {
            _context.Add(context);
            return this;
        }

        /// <summary>
        /// 添加约束条件
        /// </summary>
        /// <param name="constraint">约束条件</param>
        /// <returns>当前实例</returns>
        public PromptBuilder AddConstraint(string constraint)
        {
            _constraints.Add(constraint);
            return this;
        }

        /// <summary>
        /// 设置输出格式
        /// </summary>
        /// <param name="format">格式描述</param>
        /// <returns>当前实例</returns>
        public PromptBuilder SetOutputFormat(string format)
        {
            _outputFormat = format;
            return this;
        }

        /// <summary>
        /// 设置 JSON 输出格式
        /// </summary>
        /// <param name="schema">JSON Schema 或示例</param>
        /// <returns>当前实例</returns>
        public PromptBuilder SetJsonOutput(string? schema = null)
        {
            if (!string.IsNullOrEmpty(schema))
            {
                _outputFormat = $"请以 JSON 格式输出，格式如下:\n{schema}";
            }
            else
            {
                _outputFormat = "请以有效的 JSON 格式输出，不要添加任何其他文本或解释。";
            }
            return this;
        }

        /// <summary>
        /// 构建最终提示词
        /// </summary>
        /// <returns>构建的提示词</returns>
        public string Build()
        {
            var result = new StringBuilder();

            // 系统提示词
            if (_systemPrompt.Length > 0)
            {
                result.AppendLine(_systemPrompt.ToString());
                result.AppendLine();
            }

            // 任务描述
            if (!string.IsNullOrEmpty(_task))
            {
                result.AppendLine("## 任务");
                result.AppendLine(_task);
                result.AppendLine();
            }

            // 上下文
            if (_context.Count > 0)
            {
                result.AppendLine("## 上下文");
                foreach (var ctx in _context)
                {
                    result.AppendLine(ctx);
                }
                result.AppendLine();
            }

            // 示例
            if (_examples.Count > 0)
            {
                result.AppendLine("## 示例");
                foreach (var example in _examples)
                {
                    result.AppendLine(example);
                    result.AppendLine();
                }
            }

            // 约束条件
            if (_constraints.Count > 0)
            {
                result.AppendLine("## 约束条件");
                foreach (var constraint in _constraints)
                {
                    result.AppendLine($"- {constraint}");
                }
                result.AppendLine();
            }

            // 输出格式
            if (!string.IsNullOrEmpty(_outputFormat))
            {
                result.AppendLine("## 输出格式");
                result.AppendLine(_outputFormat);
            }

            return result.ToString().Trim();
        }

        /// <summary>
        /// 构建消息列表
        /// </summary>
        /// <param name="userInput">用户输入</param>
        /// <returns>消息列表</returns>
        public List<ChatMessage> BuildMessages(string userInput)
        {
            var messages = new List<ChatMessage>();

            // 系统消息
            var systemPrompt = Build();
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(new ChatMessage { Role = "system", Content = systemPrompt });
            }

            // 用户消息
            messages.Add(new ChatMessage { Role = "user", Content = userInput });

            return messages;
        }

        /// <summary>
        /// 清空所有内容
        /// </summary>
        /// <returns>当前实例</returns>
        public PromptBuilder Clear()
        {
            _systemPrompt.Clear();
            _examples.Clear();
            _context.Clear();
            _constraints.Clear();
            _task = null;
            _outputFormat = null;
            return this;
        }
    }

    /// <summary>
    /// 常用提示词模板
    /// </summary>
    public static class PromptTemplates
    {
        /// <summary>
        /// 代码审查提示词
        /// </summary>
        public static string CodeReview(string code, string? language = null)
        {
            var builder = new PromptBuilder()
                .AddSystemPrompt("你是一位经验丰富的代码审查专家。")
                .SetTask("审查以下代码并提供改进建议。")
                .AddConstraint("关注代码质量、性能、安全性")
                .AddConstraint("提供具体的改进建议")
                .AddConstraint("指出潜在的问题和风险");

            if (!string.IsNullOrEmpty(language))
            {
                builder.AddContext($"编程语言: {language}");
            }

            builder.SetOutputFormat("请按以下格式输出:\n1. 问题列表\n2. 改进建议\n3. 重构后的代码（如有必要）");

            var messages = builder.BuildMessages($"待审查的代码:\n```\n{code}\n```");
            return messages[0].Content + "\n\n" + messages[1].Content;
        }

        /// <summary>
        /// 翻译提示词
        /// </summary>
        public static string Translate(string text, string sourceLanguage, string targetLanguage)
        {
            var builder = new PromptBuilder()
                .AddSystemPrompt("你是一位专业的翻译专家，精通多种语言。")
                .SetTask($"将以下文本从{sourceLanguage}翻译成{targetLanguage}。")
                .AddConstraint("保持原文的语气和风格")
                .AddConstraint("确保翻译准确、自然、流畅")
                .AddConstraint("保留专业术语的准确性");

            var messages = builder.BuildMessages(text);
            return messages[0].Content + "\n\n" + messages[1].Content;
        }

        /// <summary>
        /// 摘要生成提示词
        /// </summary>
        public static string Summarize(string text, int? maxLength = null)
        {
            var builder = new PromptBuilder()
                .AddSystemPrompt("你是一位专业的文本摘要专家。")
                .SetTask("请为以下文本生成简洁的摘要。")
                .AddConstraint("保留关键信息和要点")
                .AddConstraint("语言简洁明了");

            if (maxLength.HasValue)
            {
                builder.AddConstraint($"摘要长度不超过{maxLength.Value}字");
            }

            var messages = builder.BuildMessages(text);
            return messages[0].Content + "\n\n" + messages[1].Content;
        }

        /// <summary>
        /// 数据提取提示词
        /// </summary>
        public static string ExtractData(string text, string[] fields)
        {
            var builder = new PromptBuilder()
                .AddSystemPrompt("你是一位数据提取专家。")
                .SetTask("从以下文本中提取指定的数据字段。")
                .SetJsonOutput($"{{\"fields\": [{string.Join(", ", fields)}]}}");

            var messages = builder.BuildMessages(text);
            return messages[0].Content + "\n\n" + messages[1].Content;
        }

        /// <summary>
        /// 问答提示词
        /// </summary>
        public static string QuestionAnswer(string context, string question)
        {
            var builder = new PromptBuilder()
                .AddSystemPrompt("你是一位知识渊博的助手，根据给定的上下文回答问题。")
                .SetTask("根据提供的上下文回答用户的问题。")
                .AddConstraint("只根据上下文内容回答")
                .AddConstraint("如果上下文中没有相关信息，请明确说明")
                .AddContext($"上下文:\n{context}");

            var messages = builder.BuildMessages(question);
            return messages[0].Content + "\n\n" + messages[1].Content;
        }
    }
}
