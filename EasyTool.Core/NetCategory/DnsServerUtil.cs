using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// DNS 记录类型
    /// </summary>
    public enum DnsRecordType
    {
        A = 1,
        NS = 2,
        CNAME = 5,
        SOA = 6,
        PTR = 12,
        MX = 15,
        TXT = 16,
        AAAA = 28
    }

    /// <summary>
    /// DNS 记录
    /// </summary>
    public class DnsRecord
    {
        /// <summary>
        /// 记录名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 记录类型
        /// </summary>
        public DnsRecordType Type { get; set; }

        /// <summary>
        /// TTL（秒）
        /// </summary>
        public int Ttl { get; set; }

        /// <summary>
        /// 记录值
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// MX 优先级（仅 MX 记录）
        /// </summary>
        public int? Priority { get; set; }

        public override string ToString()
        {
            var priority = Priority.HasValue ? $" {Priority}" : "";
            return $"{Name} {Ttl} IN {Type} {priority}{Value}";
        }
    }

    /// <summary>
    /// DNS 查询选项
    /// </summary>
    public class DnsQueryOptions
    {
        /// <summary>
        /// DNS 服务器地址
        /// </summary>
        public IPAddress DnsServer { get; set; } = IPAddress.Parse("8.8.8.8");

        /// <summary>
        /// DNS 服务器端口
        /// </summary>
        public int Port { get; set; } = 53;

        /// <summary>
        /// 查询超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 是否使用 TCP
        /// </summary>
        public bool UseTcp { get; set; }

        /// <summary>
        /// 是否递归查询
        /// </summary>
        public bool RecursionDesired { get; set; } = true;
    }

    /// <summary>
    /// DNS 工具类
    /// 提供 DNS 查询和解析功能
    /// </summary>
    public static class DnsServerUtil
    {
        private static readonly Random _random = new();

        /// <summary>
        /// 查询 A 记录
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="options">查询选项</param>
        /// <returns>IP 地址列表</returns>
        public static async Task<List<IPAddress>> QueryAAsync(string domain, DnsQueryOptions? options = null)
        {
            options ??= new DnsQueryOptions();
            var records = await QueryAsync(domain, DnsRecordType.A, options).ConfigureAwait(false);

            return records
                .Select(r => IPAddress.TryParse(r.Value, out var ip) ? ip : null)
                .Where(ip => ip != null)
                .Cast<IPAddress>()
                .ToList();
        }

        /// <summary>
        /// 查询 AAAA 记录
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="options">查询选项</param>
        /// <returns>IPv6 地址列表</returns>
        public static async Task<List<IPAddress>> QueryAaaaAsync(string domain, DnsQueryOptions? options = null)
        {
            options ??= new DnsQueryOptions();
            var records = await QueryAsync(domain, DnsRecordType.AAAA, options).ConfigureAwait(false);

            return records
                .Select(r => IPAddress.TryParse(r.Value, out var ip) ? ip : null)
                .Where(ip => ip != null)
                .Cast<IPAddress>()
                .ToList();
        }

        /// <summary>
        /// 查询 MX 记录
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="options">查询选项</param>
        /// <returns>MX 记录列表</returns>
        public static async Task<List<(int Priority, string MailServer)>> QueryMxAsync(string domain, DnsQueryOptions? options = null)
        {
            options ??= new DnsQueryOptions();
            var records = await QueryAsync(domain, DnsRecordType.MX, options).ConfigureAwait(false);

            return records
                .Where(r => r.Priority.HasValue)
                .Select(r => (Priority: r.Priority!.Value, MailServer: r.Value))
                .OrderBy(r => r.Priority)
                .ToList();
        }

        /// <summary>
        /// 查询 TXT 记录
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="options">查询选项</param>
        /// <returns>TXT 记录列表</returns>
        public static async Task<List<string>> QueryTxtAsync(string domain, DnsQueryOptions? options = null)
        {
            options ??= new DnsQueryOptions();
            var records = await QueryAsync(domain, DnsRecordType.TXT, options).ConfigureAwait(false);
            return records.Select(r => r.Value).ToList();
        }

        /// <summary>
        /// 查询 CNAME 记录
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="options">查询选项</param>
        /// <returns>CNAME 目标</returns>
        public static async Task<string?> QueryCnameAsync(string domain, DnsQueryOptions? options = null)
        {
            options ??= new DnsQueryOptions();
            var records = await QueryAsync(domain, DnsRecordType.CNAME, options).ConfigureAwait(false);
            return records.FirstOrDefault()?.Value;
        }

        /// <summary>
        /// 查询 NS 记录
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="options">查询选项</param>
        /// <returns>NS 服务器列表</returns>
        public static async Task<List<string>> QueryNsAsync(string domain, DnsQueryOptions? options = null)
        {
            options ??= new DnsQueryOptions();
            var records = await QueryAsync(domain, DnsRecordType.NS, options).ConfigureAwait(false);
            return records.Select(r => r.Value).ToList();
        }

        /// <summary>
        /// 反向查询（IP 到域名）
        /// </summary>
        /// <param name="ipAddress">IP 地址</param>
        /// <param name="options">查询选项</param>
        /// <returns>域名列表</returns>
        public static async Task<List<string>> ReverseQueryAsync(IPAddress ipAddress, DnsQueryOptions? options = null)
        {
            options ??= new DnsQueryOptions();

            // 构建反向查询域名
            var bytes = ipAddress.GetAddressBytes();
            Array.Reverse(bytes);
            var ptrDomain = $"{string.Join(".", bytes)}.in-addr.arpa";

            var records = await QueryAsync(ptrDomain, DnsRecordType.PTR, options).ConfigureAwait(false);
            return records.Select(r => r.Value).ToList();
        }

        /// <summary>
        /// 通用 DNS 查询
        /// </summary>
        /// <param name="domain">域名</param>
        /// <param name="recordType">记录类型</param>
        /// <param name="options">查询选项</param>
        /// <returns>DNS 记录列表</returns>
        public static async Task<List<DnsRecord>> QueryAsync(string domain, DnsRecordType recordType, DnsQueryOptions? options = null)
        {
            options ??= new DnsQueryOptions();

            // 构建查询包
            var queryPacket = BuildQueryPacket(domain, recordType, options.RecursionDesired);

            // 发送查询
            byte[] responseBytes;

            if (options.UseTcp)
            {
                responseBytes = await QueryOverTcpAsync(queryPacket, options).ConfigureAwait(false);
            }
            else
            {
                responseBytes = await QueryOverUdpAsync(queryPacket, options).ConfigureAwait(false);
            }

            // 解析响应
            return ParseResponse(responseBytes);
        }

        /// <summary>
        /// 批量查询
        /// </summary>
        /// <param name="domains">域名列表</param>
        /// <param name="recordType">记录类型</param>
        /// <param name="options">查询选项</param>
        /// <returns>域名到记录列表的映射</returns>
        public static async Task<Dictionary<string, List<DnsRecord>>> QueryManyAsync(
            IEnumerable<string> domains,
            DnsRecordType recordType,
            DnsQueryOptions? options = null)
        {
            var result = new Dictionary<string, List<DnsRecord>>();

            foreach (var domain in domains)
            {
                result[domain] = await QueryAsync(domain, recordType, options).ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// 获取本机 DNS 服务器
        /// </summary>
        /// <returns>DNS 服务器列表</returns>
        public static List<IPAddress> GetLocalDnsServers()
        {
            var servers = new List<IPAddress>();

            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var iface in interfaces)
                {
                    if (iface.OperationalStatus != OperationalStatus.Up)
                        continue;

                    var ipProps = iface.GetIPProperties();
                    var dnsAddresses = ipProps.DnsAddresses;
                    foreach (var dns in dnsAddresses)
                    {
                        if (!servers.Contains(dns))
                        {
                            servers.Add(dns);
                        }
                    }
                }
            }
            catch
            {
                // 返回默认 DNS 服务器
                servers.Add(IPAddress.Parse("8.8.8.8"));
                servers.Add(IPAddress.Parse("8.8.4.4"));
            }

            return servers;
        }

        #region 私有方法

        private static byte[] BuildQueryPacket(string domain, DnsRecordType recordType, bool recursionDesired)
        {
            using var stream = new System.IO.MemoryStream();
            using var writer = new BinaryWriter(stream);

            // Transaction ID
            writer.Write((ushort)_random.Next(0, 65536));

            // Flags
            var flags = (ushort)0x0100; // Standard query
            if (recursionDesired)
                flags |= 0x0100;
            writer.Write(flags);

            // Questions count
            writer.Write((ushort)1);

            // Answer, Authority, Additional counts
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);

            // Question section
            WriteDomainName(writer, domain);
            writer.Write((ushort)recordType);
            writer.Write((ushort)1); // Class IN

            return stream.ToArray();
        }

        private static void WriteDomainName(BinaryWriter writer, string domain)
        {
            var parts = domain.Split('.');
            foreach (var part in parts)
            {
                var bytes = Encoding.ASCII.GetBytes(part);
                writer.Write((byte)bytes.Length);
                writer.Write(bytes);
            }
            writer.Write((byte)0);
        }

        private static async Task<byte[]> QueryOverUdpAsync(byte[] query, DnsQueryOptions options)
        {
            using var client = new UdpClient();
            client.Client.ReceiveTimeout = (int)options.Timeout.TotalMilliseconds;

            await client.SendAsync(query, query.Length, options.DnsServer.ToString(), options.Port).ConfigureAwait(false);

            var result = await client.ReceiveAsync().ConfigureAwait(false);
            return result.Buffer;
        }

        private static async Task<byte[]> QueryOverTcpAsync(byte[] query, DnsQueryOptions options)
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(options.DnsServer, options.Port);

            if (await Task.WhenAny(connectTask, Task.Delay(options.Timeout)).ConfigureAwait(false) != connectTask)
            {
                throw new TimeoutException("DNS 查询超时");
            }

            await connectTask.ConfigureAwait(false);

            var stream = client.GetStream();
            stream.ReadTimeout = (int)options.Timeout.TotalMilliseconds;
            stream.WriteTimeout = (int)options.Timeout.TotalMilliseconds;

            // TCP DNS 需要在前面加 2 字节长度
            var lengthBytes = BitConverter.GetBytes((ushort)query.Length);
            Array.Reverse(lengthBytes); // Big-endian

            await stream.WriteAsync(lengthBytes, 0, 2).ConfigureAwait(false);
            await stream.WriteAsync(query, 0, query.Length).ConfigureAwait(false);

            // 读取响应
            var responseLengthBytes = new byte[2];
            await stream.ReadAsync(responseLengthBytes, 0, 2).ConfigureAwait(false);
            Array.Reverse(responseLengthBytes);
            var responseLength = BitConverter.ToUInt16(responseLengthBytes, 0);

            var response = new byte[responseLength];
            await stream.ReadAsync(response, 0, responseLength).ConfigureAwait(false);

            return response;
        }

        private static List<DnsRecord> ParseResponse(byte[] response)
        {
            var records = new List<DnsRecord>();

            using var stream = new System.IO.MemoryStream(response);
            using var reader = new BinaryReader(stream);

            // 跳过 Header (12 bytes)
            reader.ReadBytes(12);

            // Question count
            var questionCount = reader.ReadUInt16();
            for (int i = 0; i < questionCount; i++)
            {
                ReadDomainName(reader);
                reader.ReadUInt16(); // Type
                reader.ReadUInt16(); // Class
            }

            // Answer count
            var answerCount = reader.ReadUInt16();
            for (int i = 0; i < answerCount; i++)
            {
                var name = ReadDomainName(reader);
                var type = (DnsRecordType)reader.ReadUInt16();
                reader.ReadUInt16(); // Class
                var ttl = (int)reader.ReadUInt32();
                var dataLength = reader.ReadUInt16();
                var dataPosition = stream.Position;

                var record = new DnsRecord
                {
                    Name = name,
                    Type = type,
                    Ttl = ttl
                };

                switch (type)
                {
                    case DnsRecordType.A:
                        var aBytes = reader.ReadBytes(4);
                        record.Value = new IPAddress(aBytes).ToString();
                        break;

                    case DnsRecordType.AAAA:
                        var aaaaBytes = reader.ReadBytes(16);
                        record.Value = new IPAddress(aaaaBytes).ToString();
                        break;

                    case DnsRecordType.CNAME:
                    case DnsRecordType.NS:
                    case DnsRecordType.PTR:
                        record.Value = ReadDomainName(reader);
                        break;

                    case DnsRecordType.MX:
                        record.Priority = reader.ReadUInt16();
                        record.Value = ReadDomainName(reader);
                        break;

                    case DnsRecordType.TXT:
                        var txtLength = reader.ReadByte();
                        record.Value = Encoding.ASCII.GetString(reader.ReadBytes(txtLength));
                        break;

                    default:
                        stream.Position = dataPosition + dataLength;
                        break;
                }

                records.Add(record);
            }

            return records;
        }

        private static string ReadDomainName(BinaryReader reader)
        {
            var labels = new List<string>();
            var visited = new HashSet<int>();

            while (true)
            {
                var length = reader.ReadByte();

                if (length == 0)
                    break;

                // 指针压缩
                if ((length & 0xC0) == 0xC0)
                {
                    var offset = ((length & 0x3F) << 8) | reader.ReadByte();
                    if (visited.Contains(offset))
                        break;

                    visited.Add(offset);
                    var currentPos = reader.BaseStream.Position;
                    reader.BaseStream.Position = offset;

                    var pointerLabel = ReadDomainName(reader);
                    labels.Add(pointerLabel);

                    reader.BaseStream.Position = currentPos;
                    break;
                }

                labels.Add(Encoding.ASCII.GetString(reader.ReadBytes(length)));
            }

            return string.Join(".", labels);
        }

        #endregion
    }
}
