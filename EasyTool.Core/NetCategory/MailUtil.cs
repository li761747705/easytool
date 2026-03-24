using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// 邮件发送工具类
    /// 支持 SMTP 协议发送邮件，包括附件、HTML 正文、抄送等功能
    /// </summary>
    public static class MailUtil
    {
        #region 快捷发送方法

        /// <summary>
        /// 发送简单文本邮件
        /// </summary>
        /// <param name="config">SMTP 配置</param>
        /// <param name="to">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="body">正文</param>
        public static void Send(SmtpConfig config, string to, string subject, string body)
        {
            Send(config, new[] { to }, subject, body);
        }

        /// <summary>
        /// 发送简单文本邮件
        /// </summary>
        /// <param name="config">SMTP 配置</param>
        /// <param name="to">收件人列表</param>
        /// <param name="subject">主题</param>
        /// <param name="body">正文</param>
        public static void Send(SmtpConfig config, IEnumerable<string> to, string subject, string body)
        {
            Send(config, new MailMessageOptions
            {
                To = to.ToList(),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            });
        }

        /// <summary>
        /// 发送 HTML 邮件
        /// </summary>
        /// <param name="config">SMTP 配置</param>
        /// <param name="to">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="htmlBody">HTML 正文</param>
        public static void SendHtml(SmtpConfig config, string to, string subject, string htmlBody)
        {
            Send(config, new MailMessageOptions
            {
                To = new List<string> { to },
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            });
        }

        /// <summary>
        /// 发送带附件的邮件
        /// </summary>
        /// <param name="config">SMTP 配置</param>
        /// <param name="to">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="body">正文</param>
        /// <param name="attachments">附件文件路径列表</param>
        public static void SendWithAttachments(SmtpConfig config, string to, string subject, string body, params string[] attachments)
        {
            Send(config, new MailMessageOptions
            {
                To = new List<string> { to },
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
                Attachments = attachments.ToList()
            });
        }

        #endregion

        #region 完整发送方法

        /// <summary>
        /// 发送邮件（完整选项）
        /// </summary>
        /// <param name="config">SMTP 配置</param>
        /// <param name="options">邮件选项</param>
        public static void Send(SmtpConfig config, MailMessageOptions options)
        {
            using var message = CreateMessage(config, options);
            using var client = CreateClient(config);
            client.Send(message);
        }

        /// <summary>
        /// 异步发送邮件
        /// </summary>
        /// <param name="config">SMTP 配置</param>
        /// <param name="options">邮件选项</param>
        /// <returns>Task</returns>
        public static async Task SendAsync(SmtpConfig config, MailMessageOptions options)
        {
            using var message = CreateMessage(config, options);
            using var client = CreateClient(config);
            await client.SendMailAsync(message);
        }

        /// <summary>
        /// 批量发送邮件
        /// </summary>
        /// <param name="config">SMTP 配置</param>
        /// <param name="messages">邮件选项列表</param>
        /// <param name="parallel">是否并行发送</param>
        /// <returns>发送结果列表</returns>
        public static List<SendResult> SendBatch(SmtpConfig config, List<MailMessageOptions> messages, bool parallel = false)
        {
            var results = new List<SendResult>();

            if (parallel)
            {
                var tasks = messages.Select(msg => Task.Run(() =>
                {
                    try
                    {
                        Send(config, msg);
                        return new SendResult { Success = true, Recipients = msg.To };
                    }
                    catch (Exception ex)
                    {
                        return new SendResult { Success = false, Recipients = msg.To, Error = ex.Message };
                    }
                })).ToArray();

                Task.WaitAll(tasks);
                results = tasks.Select(t => t.Result).ToList();
            }
            else
            {
                foreach (var msg in messages)
                {
                    try
                    {
                        Send(config, msg);
                        results.Add(new SendResult { Success = true, Recipients = msg.To });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new SendResult { Success = false, Recipients = msg.To, Error = ex.Message });
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 批量异步发送邮件
        /// </summary>
        /// <param name="config">SMTP 配置</param>
        /// <param name="messages">邮件选项列表</param>
        /// <param name="maxDegreeOfParallelism">最大并行度</param>
        /// <returns>发送结果列表</returns>
        public static async Task<List<SendResult>> SendBatchAsync(SmtpConfig config, List<MailMessageOptions> messages, int maxDegreeOfParallelism = 5)
        {
            var results = new List<SendResult>();
            var semaphore = new System.Threading.SemaphoreSlim(maxDegreeOfParallelism);

            var tasks = messages.Select(async msg =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await SendAsync(config, msg);
                    return new SendResult { Success = true, Recipients = msg.To };
                }
                catch (Exception ex)
                {
                    return new SendResult { Success = false, Recipients = msg.To, Error = ex.Message };
                }
                finally
                {
                    semaphore.Release();
                }
            });

            results = (await Task.WhenAll(tasks)).ToList();
            return results;
        }

        #endregion

        #region 模板发送

        /// <summary>
        /// 使用模板发送邮件
        /// </summary>
        /// <param name="config">SMTP 配置</param>
        /// <param name="to">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="template">模板内容（使用 {key} 占位符）</param>
        /// <param name="parameters">参数字典</param>
        /// <param name="isHtml">是否为 HTML 格式</param>
        public static void SendTemplate(SmtpConfig config, string to, string subject, string template, Dictionary<string, object> parameters, bool isHtml = true)
        {
            string body = RenderTemplate(template, parameters);
            Send(config, new MailMessageOptions
            {
                To = new List<string> { to },
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            });
        }

        /// <summary>
        /// 使用模板文件发送邮件
        /// </summary>
        /// <param name="config">SMTP 配置</param>
        /// <param name="to">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="templatePath">模板文件路径</param>
        /// <param name="parameters">参数字典</param>
        public static void SendTemplateFile(SmtpConfig config, string to, string subject, string templatePath, Dictionary<string, object> parameters)
        {
            if (!File.Exists(templatePath))
                throw new FileNotFoundException("模板文件不存在", templatePath);

            string template = File.ReadAllText(templatePath, Encoding.UTF8);
            bool isHtml = templatePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                          templatePath.EndsWith(".htm", StringComparison.OrdinalIgnoreCase);

            SendTemplate(config, to, subject, template, parameters, isHtml);
        }

        #endregion

        #region 私有方法

        private static SmtpClient CreateClient(SmtpConfig config)
        {
            var client = new SmtpClient(config.Host, config.Port)
            {
                EnableSsl = config.EnableSsl,
                Timeout = config.Timeout ?? 30000
            };

            if (!string.IsNullOrEmpty(config.UserName) && !string.IsNullOrEmpty(config.Password))
            {
                client.Credentials = new NetworkCredential(config.UserName, config.Password);
            }

            return client;
        }

        private static MailMessage CreateMessage(SmtpConfig config, MailMessageOptions options)
        {
            var message = new MailMessage
            {
                From = new MailAddress(options.From ?? config.DefaultFrom),
                Subject = options.Subject,
                Body = options.Body,
                IsBodyHtml = options.IsBodyHtml,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                Priority = options.Priority
            };

            // 添加收件人
            if (options.To != null)
            {
                foreach (var to in options.To)
                {
                    if (!string.IsNullOrEmpty(to))
                        message.To.Add(to);
                }
            }

            // 添加抄送
            if (options.Cc != null)
            {
                foreach (var cc in options.Cc)
                {
                    if (!string.IsNullOrEmpty(cc))
                        message.CC.Add(cc);
                }
            }

            // 添加密送
            if (options.Bcc != null)
            {
                foreach (var bcc in options.Bcc)
                {
                    if (!string.IsNullOrEmpty(bcc))
                        message.Bcc.Add(bcc);
                }
            }

            // 添加回复地址
            if (!string.IsNullOrEmpty(options.ReplyTo))
            {
                message.ReplyToList.Add(new MailAddress(options.ReplyTo));
            }

            // 添加附件
            if (options.Attachments != null)
            {
                foreach (var filePath in options.Attachments)
                {
                    if (File.Exists(filePath))
                    {
                        var attachment = new Attachment(filePath);
                        attachment.ContentDisposition!.CreationDate = File.GetCreationTime(filePath);
                        attachment.ContentDisposition.ModificationDate = File.GetLastWriteTime(filePath);
                        attachment.ContentDisposition.ReadDate = File.GetLastAccessTime(filePath);
                        message.Attachments.Add(attachment);
                    }
                }
            }

            // 添加内嵌资源（用于 HTML 邮件中的图片）
            if (options.EmbeddedResources != null)
            {
                foreach (var resource in options.EmbeddedResources)
                {
                    if (File.Exists(resource.Value))
                    {
                        var attachment = new LinkedResource(resource.Value)
                        {
                            ContentId = resource.Key
                        };
                        var htmlView = AlternateView.CreateAlternateViewFromString(options.Body, Encoding.UTF8, MediaTypeNames.Text.Html);
                        htmlView.LinkedResources.Add(attachment);
                        message.AlternateViews.Add(htmlView);
                    }
                }
            }

            // 添加自定义头部
            if (options.Headers != null)
            {
                foreach (var header in options.Headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }
            }

            return message;
        }

        private static string RenderTemplate(string template, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(template) || parameters == null)
                return template ?? string.Empty;

            string result = template;
            foreach (var kvp in parameters)
            {
                string placeholder = "{" + kvp.Key + "}";
                string value = kvp.Value?.ToString() ?? string.Empty;
                result = result.Replace(placeholder, value);
            }

            return result;
        }

        #endregion
    }

    #region 配置和选项类

    /// <summary>
    /// SMTP 配置
    /// </summary>
    public class SmtpConfig
    {
        /// <summary>
        /// SMTP 服务器地址
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// SMTP 服务器端口（默认25）
        /// </summary>
        public int Port { get; set; } = 25;

        /// <summary>
        /// 是否启用 SSL
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 默认发件人地址
        /// </summary>
        public string? DefaultFrom { get; set; }

        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// 创建 QQ 邮箱配置
        /// </summary>
        /// <param name="userName">QQ 邮箱</param>
        /// <param name="authCode">授权码</param>
        /// <returns>SMTP 配置</returns>
        public static SmtpConfig ForQQ(string userName, string authCode)
        {
            return new SmtpConfig
            {
                Host = "smtp.qq.com",
                Port = 587,
                EnableSsl = true,
                UserName = userName,
                Password = authCode,
                DefaultFrom = userName
            };
        }

        /// <summary>
        /// 创建 163 邮箱配置
        /// </summary>
        /// <param name="userName">163 邮箱</param>
        /// <param name="authCode">授权码</param>
        /// <returns>SMTP 配置</returns>
        public static SmtpConfig For163(string userName, string authCode)
        {
            return new SmtpConfig
            {
                Host = "smtp.163.com",
                Port = 465,
                EnableSsl = true,
                UserName = userName,
                Password = authCode,
                DefaultFrom = userName
            };
        }

        /// <summary>
        /// 创建 Gmail 配置
        /// </summary>
        /// <param name="userName">Gmail 地址</param>
        /// <param name="appPassword">应用专用密码</param>
        /// <returns>SMTP 配置</returns>
        public static SmtpConfig ForGmail(string userName, string appPassword)
        {
            return new SmtpConfig
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                UserName = userName,
                Password = appPassword,
                DefaultFrom = userName
            };
        }

        /// <summary>
        /// 创建 Outlook 配置
        /// </summary>
        /// <param name="userName">Outlook 地址</param>
        /// <param name="password">密码</param>
        /// <returns>SMTP 配置</returns>
        public static SmtpConfig ForOutlook(string userName, string password)
        {
            return new SmtpConfig
            {
                Host = "smtp-mail.outlook.com",
                Port = 587,
                EnableSsl = true,
                UserName = userName,
                Password = password,
                DefaultFrom = userName
            };
        }
    }

    /// <summary>
    /// 邮件消息选项
    /// </summary>
    public class MailMessageOptions
    {
        /// <summary>
        /// 发件人（可选，使用配置中的默认值）
        /// </summary>
        public string? From { get; set; }

        /// <summary>
        /// 收件人列表
        /// </summary>
        public List<string>? To { get; set; }

        /// <summary>
        /// 抄送列表
        /// </summary>
        public List<string>? Cc { get; set; }

        /// <summary>
        /// 密送列表
        /// </summary>
        public List<string>? Bcc { get; set; }

        /// <summary>
        /// 回复地址
        /// </summary>
        public string? ReplyTo { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 正文
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// 是否为 HTML 正文
        /// </summary>
        public bool IsBodyHtml { get; set; }

        /// <summary>
        /// 附件文件路径列表
        /// </summary>
        public List<string>? Attachments { get; set; }

        /// <summary>
        /// 内嵌资源（ContentId -> 文件路径）
        /// </summary>
        public Dictionary<string, string>? EmbeddedResources { get; set; }

        /// <summary>
        /// 自定义邮件头
        /// </summary>
        public Dictionary<string, string>? Headers { get; set; }

        /// <summary>
        /// 邮件优先级
        /// </summary>
        public MailPriority Priority { get; set; } = MailPriority.Normal;
    }

    /// <summary>
    /// 发送结果
    /// </summary>
    public class SendResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 收件人列表
        /// </summary>
        public List<string>? Recipients { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Error { get; set; }

        public override string ToString()
        {
            return Success
                ? $"成功发送至: {string.Join(", ", Recipients ?? new List<string>())}"
                : $"发送失败: {Error}";
        }
    }

    #endregion
}
