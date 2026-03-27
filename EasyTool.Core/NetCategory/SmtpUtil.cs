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
    /// SMTP 邮件发送工具类
    /// </summary>
    public static class SmtpUtil
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="options">SMTP 配置</param>
        /// <param name="message">邮件消息</param>
        public static void Send(SmtpOptions options, EmailMessage message)
        {
            using var smtpClient = CreateSmtpClient(options);
            using var mailMessage = CreateMailMessage(message);
            smtpClient.Send(mailMessage);
        }

        /// <summary>
        /// 异步发送邮件
        /// </summary>
        public static async Task SendAsync(SmtpOptions options, EmailMessage message)
        {
            using var smtpClient = CreateSmtpClient(options);
            using var mailMessage = CreateMailMessage(message);
            await smtpClient.SendMailAsync(mailMessage);
        }

        /// <summary>
        /// 批量发送邮件
        /// </summary>
        public static void SendBatch(SmtpOptions options, IEnumerable<EmailMessage> messages)
        {
            using var smtpClient = CreateSmtpClient(options);

            foreach (var message in messages)
            {
                using var mailMessage = CreateMailMessage(message);
                smtpClient.Send(mailMessage);
            }
        }

        /// <summary>
        /// 发送简单邮件
        /// </summary>
        /// <param name="options">SMTP 配置</param>
        /// <param name="to">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="body">正文</param>
        /// <param name="isHtml">是否为 HTML</param>
        public static void SendSimple(SmtpOptions options, string to, string subject, string body, bool isHtml = false)
        {
            var message = new EmailMessage
            {
                To = new List<string> { to },
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            Send(options, message);
        }

        /// <summary>
        /// 发送带附件的邮件
        /// </summary>
        public static void SendWithAttachment(SmtpOptions options, string to, string subject, string body, string attachmentPath, bool isHtml = false)
        {
            var message = new EmailMessage
            {
                To = new List<string> { to },
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment { FilePath = attachmentPath }
                }
            };

            Send(options, message);
        }

        /// <summary>
        /// 发送模板邮件
        /// </summary>
        /// <param name="options">SMTP 配置</param>
        /// <param name="to">收件人</param>
        /// <param name="subject">主题</param>
        /// <param name="template">模板内容</param>
        /// <param name="parameters">模板参数</param>
        /// <param name="isHtml">是否为 HTML</param>
        public static void SendTemplate(SmtpOptions options, string to, string subject, string template, Dictionary<string, object> parameters, bool isHtml = false)
        {
            var body = template;
            foreach (var kvp in parameters)
            {
                body = body.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);
            }

            SendSimple(options, to, subject, body, isHtml);
        }

        /// <summary>
        /// 测试 SMTP 连接
        /// </summary>
        public static bool TestConnection(SmtpOptions options)
        {
            try
            {
                using var smtpClient = CreateSmtpClient(options);
                smtpClient.Send(new MailMessage(options.Username, options.Username, "Test", "Test connection"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static SmtpClient CreateSmtpClient(SmtpOptions options)
        {
            var client = new SmtpClient(options.Host, options.Port)
            {
                EnableSsl = options.EnableSsl,
                Timeout = options.Timeout * 1000
            };

            if (!string.IsNullOrEmpty(options.Username))
            {
                client.Credentials = new NetworkCredential(options.Username, options.Password);
            }

            return client;
        }

        private static MailMessage CreateMailMessage(EmailMessage message)
        {
            var mailMessage = new MailMessage
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsBodyHtml,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8
            };

            // 发件人
            if (!string.IsNullOrEmpty(message.From))
            {
                mailMessage.From = new MailAddress(message.From, message.FromName, Encoding.UTF8);
            }

            // 收件人
            foreach (var to in message.To ?? Enumerable.Empty<string>())
            {
                mailMessage.To.Add(new MailAddress(to));
            }

            // 抄送
            foreach (var cc in message.Cc ?? Enumerable.Empty<string>())
            {
                mailMessage.CC.Add(new MailAddress(cc));
            }

            // 密送
            foreach (var bcc in message.Bcc ?? Enumerable.Empty<string>())
            {
                mailMessage.Bcc.Add(new MailAddress(bcc));
            }

            // 回复地址
            if (!string.IsNullOrEmpty(message.ReplyTo))
            {
                mailMessage.ReplyToList.Add(new MailAddress(message.ReplyTo));
            }

            // 附件
            foreach (var attachment in message.Attachments ?? Enumerable.Empty<EmailAttachment>())
            {
                Attachment mailAttachment;

                if (!string.IsNullOrEmpty(attachment.FilePath))
                {
                    mailAttachment = new Attachment(attachment.FilePath, GetMimeType(attachment.FilePath));
                }
                else if (attachment.Content != null && !string.IsNullOrEmpty(attachment.FileName))
                {
                    var stream = new MemoryStream(attachment.Content);
                    mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType ?? GetMimeType(attachment.FileName));
                }
                else
                {
                    continue;
                }

                mailAttachment.ContentDisposition!.DispositionType = DispositionTypeNames.Attachment;
                if (!string.IsNullOrEmpty(attachment.ContentId))
                {
                    mailAttachment.ContentId = attachment.ContentId;
                }

                mailMessage.Attachments.Add(mailAttachment);
            }

            // 优先级
            mailMessage.Priority = message.Priority switch
            {
                EmailPriority.High => MailPriority.High,
                EmailPriority.Low => MailPriority.Low,
                _ => MailPriority.Normal
            };

            return mailMessage;
        }

        private static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                _ => "application/octet-stream"
            };
        }
    }

    /// <summary>
    /// SMTP 配置选项
    /// </summary>
    public class SmtpOptions
    {
        /// <summary>
        /// SMTP 服务器地址
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; } = 25;

        /// <summary>
        /// 是否启用 SSL
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// 用户名
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int Timeout { get; set; } = 30;
    }

    /// <summary>
    /// 邮件消息
    /// </summary>
    public class EmailMessage
    {
        /// <summary>
        /// 发件人地址
        /// </summary>
        public string? From { get; set; }

        /// <summary>
        /// 发件人名称
        /// </summary>
        public string? FromName { get; set; }

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
        /// 是否为 HTML 格式
        /// </summary>
        public bool IsBodyHtml { get; set; }

        /// <summary>
        /// 附件列表
        /// </summary>
        public List<EmailAttachment>? Attachments { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public EmailPriority Priority { get; set; } = EmailPriority.Normal;
    }

    /// <summary>
    /// 邮件附件
    /// </summary>
    public class EmailAttachment
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// 文件内容（字节）
        /// </summary>
        public byte[]? Content { get; set; }

        /// <summary>
        /// 内容类型（MIME 类型）
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// 内容 ID（用于嵌入图片）
        /// </summary>
        public string? ContentId { get; set; }
    }

    /// <summary>
    /// 邮件优先级
    /// </summary>
    public enum EmailPriority
    {
        Low,
        Normal,
        High
    }
}
