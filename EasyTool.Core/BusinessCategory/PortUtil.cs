using System;
using System.Collections.Generic;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 端口号工具类
    /// </summary>
    public static class PortUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// 知名端口号范围（0-1023）
        /// </summary>
        public const int WellKnownPortMin = 0;

        /// <summary>
        /// 知名端口号范围上限
        /// </summary>
        public const int WellKnownPortMax = 1023;

        /// <summary>
        /// 注册端口号范围（1024-49151）
        /// </summary>
        public const int RegisteredPortMin = 1024;

        /// <summary>
        /// 注册端口号范围上限
        /// </summary>
        public const int RegisteredPortMax = 49151;

        /// <summary>
        /// 动态/私有端口号范围（49152-65535）
        /// </summary>
        public const int DynamicPortMin = 49152;

        /// <summary>
        /// 最大端口号
        /// </summary>
        public const int MaxPort = 65535;

        /// <summary>
        /// 常见端口与名称映射
        /// </summary>
        private static readonly Dictionary<int, PortInfo> CommonPorts = new()
        {
            // 文件传输
            { 20, new PortInfo("FTP Data", "文件传输协议数据端口", "FTP") },
            { 21, new PortInfo("FTP", "文件传输协议控制端口", "FTP") },

            // 远程连接
            { 22, new PortInfo("SSH", "安全外壳协议", "SSH") },
            { 23, new PortInfo("Telnet", "远程终端协议", "Telnet") },
            { 3389, new PortInfo("RDP", "远程桌面协议", "RDP") },

            // 邮件服务
            { 25, new PortInfo("SMTP", "简单邮件传输协议", "SMTP") },
            { 110, new PortInfo("POP3", "邮局协议第3版", "POP3") },
            { 143, new PortInfo("IMAP", "互联网消息访问协议", "IMAP") },
            { 465, new PortInfo("SMTPS", "SMTP安全协议", "SMTPS") },
            { 587, new PortInfo("SMTP(TLS)", "SMTP TLS协议", "SMTP") },
            { 993, new PortInfo("IMAPS", "IMAP安全协议", "IMAPS") },
            { 995, new PortInfo("POP3S", "POP3安全协议", "POP3S") },

            // Web服务
            { 80, new PortInfo("HTTP", "超文本传输协议", "HTTP") },
            { 443, new PortInfo("HTTPS", "HTTP安全协议", "HTTPS") },
            { 8080, new PortInfo("HTTP-Proxy", "HTTP代理/备用端口", "HTTP") },
            { 8443, new PortInfo("HTTPS-Alt", "HTTPS备用端口", "HTTPS") },

            // 域名服务
            { 53, new PortInfo("DNS", "域名系统", "DNS") },

            // 数据库
            { 1433, new PortInfo("MSSQL", "Microsoft SQL Server", "MSSQL") },
            { 1521, new PortInfo("Oracle", "Oracle数据库", "Oracle") },
            { 3306, new PortInfo("MySQL", "MySQL数据库", "MySQL") },
            { 5432, new PortInfo("PostgreSQL", "PostgreSQL数据库", "PostgreSQL") },
            { 6379, new PortInfo("Redis", "Redis缓存", "Redis") },
            { 27017, new PortInfo("MongoDB", "MongoDB数据库", "MongoDB") },
            { 9200, new PortInfo("Elasticsearch", "Elasticsearch搜索", "Elasticsearch") },

            // 消息队列
            { 5672, new PortInfo("RabbitMQ", "RabbitMQ消息队列", "RabbitMQ") },
            { 9092, new PortInfo("Kafka", "Kafka消息队列", "Kafka") },
            { 61616, new PortInfo("ActiveMQ", "ActiveMQ消息队列", "ActiveMQ") },

            // 网络服务
            { 67, new PortInfo("DHCP Server", "DHCP服务器", "DHCP") },
            { 68, new PortInfo("DHCP Client", "DHCP客户端", "DHCP") },
            { 69, new PortInfo("TFTP", "简单文件传输协议", "TFTP") },
            { 123, new PortInfo("NTP", "网络时间协议", "NTP") },
            { 161, new PortInfo("SNMP", "简单网络管理协议", "SNMP") },
            { 162, new PortInfo("SNMP Trap", "SNMP陷阱", "SNMP") },
            { 514, new PortInfo("Syslog", "系统日志", "Syslog") },

            // VPN
            { 500, new PortInfo("IKE", "Internet密钥交换", "VPN") },
            { 1194, new PortInfo("OpenVPN", "OpenVPN", "VPN") },
            { 1723, new PortInfo("PPTP", "点对点隧道协议", "VPN") },

            // 其他常用
            { 88, new PortInfo("Kerberos", "Kerberos认证", "Kerberos") },
            { 389, new PortInfo("LDAP", "轻量级目录访问协议", "LDAP") },
            { 636, new PortInfo("LDAPS", "LDAP安全协议", "LDAP") },
            { 4444, new PortInfo("Kerberos-Admin", "Kerberos管理", "Kerberos") },

            // 即时通讯
            { 5222, new PortInfo("XMPP", "XMPP客户端连接", "XMPP") },
            { 5269, new PortInfo("XMPP-Server", "XMPP服务器连接", "XMPP") },

            // 游戏服务
            { 25565, new PortInfo("Minecraft", "Minecraft服务器", "Minecraft") },
            { 27015, new PortInfo("Steam", "Steam游戏服务", "Steam") },

            // 文件共享
            { 139, new PortInfo("NetBIOS-SSN", "NetBIOS会话服务", "NetBIOS") },
            { 445, new PortInfo("SMB", "Server Message Block", "SMB") },
            { 2049, new PortInfo("NFS", "网络文件系统", "NFS") },

            // 代理服务
            { 1080, new PortInfo("SOCKS", "SOCKS代理", "SOCKS") },
            { 3128, new PortInfo("Squid", "Squid代理", "Squid") },

            // Java相关
            { 1099, new PortInfo("RMI", "Java RMI注册", "RMI") },
            { 8009, new PortInfo("AJP", "Apache JServ协议", "AJP") },

            // 监控
            { 9090, new PortInfo("Prometheus", "Prometheus监控", "Prometheus") },
            { 3000, new PortInfo("Grafana", "Grafana监控", "Grafana") },
            { 8500, new PortInfo("Consul", "Consul服务发现", "Consul") }
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证端口号是否有效
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(int port)
        {
            return port >= WellKnownPortMin && port <= MaxPort;
        }

        /// <summary>
        /// 验证端口号字符串是否有效
        /// </summary>
        /// <param name="port">端口号字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? port)
        {
            if (string.IsNullOrWhiteSpace(port))
            {
                return false;
            }

            if (!int.TryParse(port, out int portNum))
            {
                return false;
            }

            return IsValid(portNum);
        }

        #endregion

        #region 端口类型判断

        /// <summary>
        /// 判断是否为知名端口（0-1023）
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>是否为知名端口</returns>
        public static bool IsWellKnownPort(int port)
        {
            return port >= WellKnownPortMin && port <= WellKnownPortMax;
        }

        /// <summary>
        /// 判断是否为注册端口（1024-49151）
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>是否为注册端口</returns>
        public static bool IsRegisteredPort(int port)
        {
            return port >= RegisteredPortMin && port <= RegisteredPortMax;
        }

        /// <summary>
        /// 判断是否为动态/私有端口（49152-65535）
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>是否为动态端口</returns>
        public static bool IsDynamicPort(int port)
        {
            return port >= DynamicPortMin && port <= MaxPort;
        }

        /// <summary>
        /// 获取端口类型
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>端口类型</returns>
        public static PortType GetPortType(int port)
        {
            if (IsWellKnownPort(port))
            {
                return PortType.WellKnown;
            }
            else if (IsRegisteredPort(port))
            {
                return PortType.Registered;
            }
            else if (IsDynamicPort(port))
            {
                return PortType.Dynamic;
            }
            else
            {
                return PortType.Invalid;
            }
        }

        /// <summary>
        /// 获取端口类型名称
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>端口类型名称</returns>
        public static string? GetPortTypeName(int port)
        {
            return GetPortType(port) switch
            {
                PortType.WellKnown => "知名端口",
                PortType.Registered => "注册端口",
                PortType.Dynamic => "动态/私有端口",
                _ => null
            };
        }

        #endregion

        #region 端口信息

        /// <summary>
        /// 获取端口信息
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>端口信息</returns>
        public static PortInfo? GetPortInfo(int port)
        {
            if (!IsValid(port))
            {
                return null;
            }

            return CommonPorts.TryGetValue(port, out PortInfo? info) ? info : null;
        }

        /// <summary>
        /// 获取端口名称
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>端口名称</returns>
        public static string? GetPortName(int port)
        {
            return GetPortInfo(port)?.Name;
        }

        /// <summary>
        /// 获取端口描述
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>端口描述</returns>
        public static string? GetPortDescription(int port)
        {
            return GetPortInfo(port)?.Description;
        }

        /// <summary>
        /// 获取端口所属服务类别
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>服务类别</returns>
        public static string? GetPortCategory(int port)
        {
            return GetPortInfo(port)?.Category;
        }

        /// <summary>
        /// 判断是否为常见端口
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>是否为常见端口</returns>
        public static bool IsCommonPort(int port)
        {
            return CommonPorts.ContainsKey(port);
        }

        #endregion

        #region 范围操作

        /// <summary>
        /// 获取指定范围内的所有端口
        /// </summary>
        /// <param name="start">起始端口</param>
        /// <param name="end">结束端口</param>
        /// <returns>端口列表</returns>
        public static int[] GetPortRange(int start, int end)
        {
            if (start < WellKnownPortMin || end > MaxPort || start > end)
            {
                return Array.Empty<int>();
            }

            int[] ports = new int[end - start + 1];
            for (int i = 0; i < ports.Length; i++)
            {
                ports[i] = start + i;
            }
            return ports;
        }

        /// <summary>
        /// 获取所有知名端口
        /// </summary>
        /// <returns>知名端口数组</returns>
        public static int[] GetWellKnownPorts()
        {
            return GetPortRange(WellKnownPortMin, WellKnownPortMax);
        }

        /// <summary>
        /// 获取所有动态端口范围
        /// </summary>
        /// <returns>动态端口数组</returns>
        public static int[] GetDynamicPorts()
        {
            return GetPortRange(DynamicPortMin, MaxPort);
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化端口号为字符串
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>端口号字符串</returns>
        public static string? Format(int port)
        {
            if (!IsValid(port))
            {
                return null;
            }

            return port.ToString();
        }

        /// <summary>
        /// 格式化端口信息
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns>格式化的端口信息</returns>
        public static string? FormatWithInfo(int port)
        {
            if (!IsValid(port))
            {
                return null;
            }

            PortInfo? info = GetPortInfo(port);
            if (info != null)
            {
                return $"{port} ({info.Name})";
            }

            string? typeName = GetPortTypeName(port);
            return $"{port} ({typeName})";
        }

        #endregion
    }

    /// <summary>
    /// 端口类型枚举
    /// </summary>
    public enum PortType
    {
        /// <summary>
        /// 无效端口
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// 知名端口（0-1023）
        /// </summary>
        WellKnown = 1,

        /// <summary>
        /// 注册端口（1024-49151）
        /// </summary>
        Registered = 2,

        /// <summary>
        /// 动态/私有端口（49152-65535）
        /// </summary>
        Dynamic = 3
    }

    /// <summary>
    /// 端口信息类
    /// </summary>
    public class PortInfo
    {
        /// <summary>
        /// 端口名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 端口描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 服务类别
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PortInfo(string name, string description, string category)
        {
            Name = name;
            Description = description;
            Category = category;
        }
    }
}
