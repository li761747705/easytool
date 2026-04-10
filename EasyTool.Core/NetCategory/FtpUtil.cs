using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// FTP 文件传输工具类
    /// 提供 FTP 文件上传、下载、删除、列表等功能
    /// </summary>
    public static class FtpUtil
    {
        #region 上传方法

        /// <summary>
        /// 上传文件到 FTP 服务器
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="localFilePath">本地文件路径</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <returns>是否成功</returns>
        public static bool Upload(FtpConfig config, string localFilePath, string remoteFilePath)
        {
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException("本地文件不存在", localFilePath);

            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.UploadFile);

            using var fileStream = File.OpenRead(localFilePath);
            using var requestStream = request.GetRequestStream();
            fileStream.CopyTo(requestStream);

            using var response = (FtpWebResponse)request.GetResponse();
            return response.StatusCode == FtpStatusCode.ClosingData;
        }

        /// <summary>
        /// 异步上传文件到 FTP 服务器
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="localFilePath">本地文件路径</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <returns>是否成功</returns>
        public static async Task<bool> UploadAsync(FtpConfig config, string localFilePath, string remoteFilePath)
        {
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException("本地文件不存在", localFilePath);

            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.UploadFile);

            using var fileStream = File.OpenRead(localFilePath);
            using var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false);
            await fileStream.CopyToAsync(requestStream).ConfigureAwait(false);

            using var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            return response.StatusCode == FtpStatusCode.ClosingData;
        }

        /// <summary>
        /// 上传数据到 FTP 服务器
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="data">数据</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <returns>是否成功</returns>
        public static bool UploadData(FtpConfig config, byte[] data, string remoteFilePath)
        {
            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.UploadFile);
            request.ContentLength = data.Length;

            using var requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);

            using var response = (FtpWebResponse)request.GetResponse();
            return response.StatusCode == FtpStatusCode.ClosingData;
        }

        /// <summary>
        /// 异步上传数据到 FTP 服务器
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="data">数据</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <returns>是否成功</returns>
        public static async Task<bool> UploadDataAsync(FtpConfig config, byte[] data, string remoteFilePath)
        {
            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.UploadFile);
            request.ContentLength = data.Length;

            using var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false);
            await requestStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);

            using var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            return response.StatusCode == FtpStatusCode.ClosingData;
        }

        #endregion

        #region 下载方法

        /// <summary>
        /// 从 FTP 服务器下载文件
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <param name="localFilePath">本地文件路径</param>
        /// <returns>是否成功</returns>
        public static bool Download(FtpConfig config, string remoteFilePath, string localFilePath)
        {
            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.DownloadFile);

            using var response = (FtpWebResponse)request.GetResponse();
            using var responseStream = response.GetResponseStream();
            using var fileStream = File.Create(localFilePath);
            responseStream?.CopyTo(fileStream);

            return response.StatusCode == FtpStatusCode.ClosingData;
        }

        /// <summary>
        /// 异步从 FTP 服务器下载文件
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <param name="localFilePath">本地文件路径</param>
        /// <returns>是否成功</returns>
        public static async Task<bool> DownloadAsync(FtpConfig config, string remoteFilePath, string localFilePath)
        {
            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.DownloadFile);

            using var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            using var responseStream = response.GetResponseStream();
            using var fileStream = File.Create(localFilePath);
            
            if (responseStream != null)
            {
                await responseStream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            return response.StatusCode == FtpStatusCode.ClosingData;
        }

        /// <summary>
        /// 从 FTP 服务器下载数据
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <returns>下载数据</returns>
        public static byte[] DownloadData(FtpConfig config, string remoteFilePath)
        {
            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.DownloadFile);

            using var response = (FtpWebResponse)request.GetResponse();
            using var responseStream = response.GetResponseStream();
            using var memoryStream = new MemoryStream();
            responseStream?.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// 异步从 FTP 服务器下载数据
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <returns>下载数据</returns>
        public static async Task<byte[]> DownloadDataAsync(FtpConfig config, string remoteFilePath)
        {
            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.DownloadFile);

            using var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            using var responseStream = response.GetResponseStream();
            using var memoryStream = new MemoryStream();
            
            if (responseStream != null)
            {
                await responseStream.CopyToAsync(memoryStream).ConfigureAwait(false);
            }
            
            return memoryStream.ToArray();
        }

        /// <summary>
        /// 下载文件内容为字符串
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>文件内容</returns>
        public static string DownloadString(FtpConfig config, string remoteFilePath, Encoding? encoding = null)
        {
            var data = DownloadData(config, remoteFilePath);
            encoding ??= Encoding.UTF8;
            return encoding.GetString(data);
        }

        /// <summary>
        /// 异步下载文件内容为字符串
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>文件内容</returns>
        public static async Task<string> DownloadStringAsync(FtpConfig config, string remoteFilePath, Encoding? encoding = null)
        {
            var data = await DownloadDataAsync(config, remoteFilePath).ConfigureAwait(false);
            encoding ??= Encoding.UTF8;
            return encoding.GetString(data);
        }

        #endregion

        #region 目录操作

        /// <summary>
        /// 列出目录内容
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remotePath">远程目录路径</param>
        /// <returns>文件列表</returns>
        public static List<FtpItem> ListDirectory(FtpConfig config, string remotePath)
        {
            var request = CreateRequest(config, remotePath, WebRequestMethods.Ftp.ListDirectoryDetails);
            var items = new List<FtpItem>();

            using var response = (FtpWebResponse)request.GetResponse();
            using var responseStream = response.GetResponseStream();
            using var reader = new StreamReader(responseStream ?? Stream.Null, Encoding.UTF8);

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var item = ParseFtpLine(line);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        /// <summary>
        /// 异步列出目录内容
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remotePath">远程目录路径</param>
        /// <returns>文件列表</returns>
        public static async Task<List<FtpItem>> ListDirectoryAsync(FtpConfig config, string remotePath)
        {
            var request = CreateRequest(config, remotePath, WebRequestMethods.Ftp.ListDirectoryDetails);
            var items = new List<FtpItem>();

            using var response = (FtpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            using var responseStream = response.GetResponseStream();
            using var reader = new StreamReader(responseStream ?? Stream.Null, Encoding.UTF8);

            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                var item = ParseFtpLine(line);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        /// <summary>
        /// 列出目录中的文件名
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remotePath">远程目录路径</param>
        /// <returns>文件名列表</returns>
        public static List<string> ListFileNames(FtpConfig config, string remotePath)
        {
            var request = CreateRequest(config, remotePath, WebRequestMethods.Ftp.ListDirectory);
            var names = new List<string>();

            using var response = (FtpWebResponse)request.GetResponse();
            using var responseStream = response.GetResponseStream();
            using var reader = new StreamReader(responseStream ?? Stream.Null, Encoding.UTF8);

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    names.Add(line.Trim());
                }
            }

            return names;
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remotePath">远程目录路径</param>
        /// <returns>是否成功</returns>
        public static bool CreateDirectory(FtpConfig config, string remotePath)
        {
            var request = CreateRequest(config, remotePath, WebRequestMethods.Ftp.MakeDirectory);

            using var response = (FtpWebResponse)request.GetResponse();
            return response.StatusCode == FtpStatusCode.PathnameCreated;
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remotePath">远程目录路径</param>
        /// <returns>是否成功</returns>
        public static bool DeleteDirectory(FtpConfig config, string remotePath)
        {
            var request = CreateRequest(config, remotePath, WebRequestMethods.Ftp.RemoveDirectory);

            using var response = (FtpWebResponse)request.GetResponse();
            return response.StatusCode == FtpStatusCode.FileActionOK;
        }

        #endregion

        #region 文件操作

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <returns>是否成功</returns>
        public static bool DeleteFile(FtpConfig config, string remoteFilePath)
        {
            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.DeleteFile);

            using var response = (FtpWebResponse)request.GetResponse();
            return response.StatusCode == FtpStatusCode.FileActionOK;
        }

        /// <summary>
        /// 重命名文件或目录
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="oldPath">原路径</param>
        /// <param name="newPath">新路径</param>
        /// <returns>是否成功</returns>
        public static bool Rename(FtpConfig config, string oldPath, string newPath)
        {
            var request = CreateRequest(config, oldPath, WebRequestMethods.Ftp.Rename);
            request.RenameTo = newPath;

            using var response = (FtpWebResponse)request.GetResponse();
            return response.StatusCode == FtpStatusCode.FileActionOK;
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <returns>是否存在</returns>
        public static bool FileExists(FtpConfig config, string remoteFilePath)
        {
            try
            {
                var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.GetFileSize);
                using var response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (WebException ex) when (ex.Response is FtpWebResponse response && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <returns>文件大小（字节）</returns>
        public static long GetFileSize(FtpConfig config, string remoteFilePath)
        {
            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.GetFileSize);

            using var response = (FtpWebResponse)request.GetResponse();
            return response.ContentLength;
        }

        /// <summary>
        /// 获取文件修改时间
        /// </summary>
        /// <param name="config">FTP 配置</param>
        /// <param name="remoteFilePath">远程文件路径</param>
        /// <returns>修改时间</returns>
        public static DateTime GetLastModified(FtpConfig config, string remoteFilePath)
        {
            var request = CreateRequest(config, remoteFilePath, WebRequestMethods.Ftp.GetDateTimestamp);

            using var response = (FtpWebResponse)request.GetResponse();
            return response.LastModified;
        }

        #endregion

        #region 私有方法

        private static FtpWebRequest CreateRequest(FtpConfig config, string remotePath, string method)
        {
            string url = config.Host;
            if (!url.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
            {
                url = "ftp://" + url;
            }
            if (!url.EndsWith("/") && !remotePath.StartsWith("/"))
            {
                url += "/";
            }
            url += remotePath.TrimStart('/');

            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Credentials = new NetworkCredential(config.UserName, config.Password);
            request.UseBinary = config.UseBinary;
            request.UsePassive = config.UsePassive;
            request.EnableSsl = config.EnableSsl;
            request.KeepAlive = config.KeepAlive;
            request.Timeout = config.Timeout;

            return request;
        }

        private static FtpItem? ParseFtpLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            // UNIX 风格: drwxr-xr-x  2 owner group 4096 Jan 1 12:00 name
            // Windows 风格: 01-01-24  12:00PM       <DIR>          name
            //               01-01-24  12:00PM              12345 name

            var item = new FtpItem();
            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 4)
                return null;

            // 检查是否为 Windows 风格
            if (parts[0].Contains("-") && parts[1].Contains(":"))
            {
                // Windows 风格
                if (parts[2] == "<DIR>")
                {
                    item.IsDirectory = true;
                    item.Name = string.Join(" ", parts.Skip(3));
                }
                else
                {
                    item.IsDirectory = false;
                    if (long.TryParse(parts[2], out long size))
                    {
                        item.Size = size;
                    }
                    item.Name = string.Join(" ", parts.Skip(3));
                }
                return item;
            }

            // UNIX 风格
            if (parts[0].StartsWith("d"))
            {
                item.IsDirectory = true;
            }
            else if (parts[0].StartsWith("-"))
            {
                item.IsDirectory = false;
                // 尝试解析大小
                if (parts.Length > 4 && long.TryParse(parts[4], out long size))
                {
                    item.Size = size;
                }
            }

            // 获取文件名（最后一个部分）
            item.Name = parts[parts.Length - 1];

            return item;
        }

        #endregion
    }

    #region 配置和结果类

    /// <summary>
    /// FTP 配置
    /// </summary>
    public class FtpConfig
    {
        /// <summary>
        /// FTP 服务器地址
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 是否使用二进制模式（默认true）
        /// </summary>
        public bool UseBinary { get; set; } = true;

        /// <summary>
        /// 是否使用被动模式（默认true）
        /// </summary>
        public bool UsePassive { get; set; } = true;

        /// <summary>
        /// 是否启用 SSL（默认false）
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// 是否保持连接（默认true）
        /// </summary>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// 超时时间（毫秒，默认30000）
        /// </summary>
        public int Timeout { get; set; } = 30000;

        /// <summary>
        /// 创建匿名 FTP 配置
        /// </summary>
        /// <param name="host">FTP 服务器地址</param>
        /// <returns>FTP 配置</returns>
        public static FtpConfig Anonymous(string host)
        {
            return new FtpConfig
            {
                Host = host,
                UserName = "anonymous",
                Password = "anonymous@anonymous.com"
            };
        }
    }

    /// <summary>
    /// FTP 文件项
    /// </summary>
    public class FtpItem
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 是否为目录
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 修改时间（如果可用）
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// 权限（如果可用）
        /// </summary>
        public string? Permissions { get; set; }

        public override string ToString()
        {
            return IsDirectory ? $"[{Name}]" : $"{Name} ({Size} bytes)";
        }
    }

    #endregion
}
