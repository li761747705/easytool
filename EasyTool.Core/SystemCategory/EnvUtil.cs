using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 环境工具
    /// </summary>
    public static class EnvUtil
    {
        #region 系统信息

        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <returns>系统信息字符串</returns>
        public static string GetSystemInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("操作系统版本：" + Environment.OSVersion.ToString());
            sb.AppendLine("系统位数：" + (Environment.Is64BitOperatingSystem ? "64 位" : "32 位"));
            sb.AppendLine("系统目录：" + Environment.SystemDirectory);
            sb.AppendLine("处理器数量：" + Environment.ProcessorCount);
            sb.AppendLine("计算机名：" + Environment.MachineName);
            sb.AppendLine("用户名：" + Environment.UserName);
            sb.AppendLine("用户域名：" + Environment.UserDomainName);
            sb.AppendLine("当前目录：" + Environment.CurrentDirectory);
            sb.AppendLine("CLR版本：" + Environment.Version.ToString());
            return sb.ToString();
        }

        /// <summary>
        /// 判断当前系统是否为Windows操作系统
        /// </summary>
        /// <returns>当前系统是否为Windows操作系统</returns>
        public static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        /// <summary>
        /// 判断当前系统是否为Linux操作系统
        /// </summary>
        /// <returns>当前系统是否为Linux操作系统</returns>
        public static bool IsLinux()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        /// <summary>
        /// 判断当前系统是否为macOS操作系统
        /// </summary>
        /// <returns>当前系统是否为macOS操作系统</returns>
        public static bool IsMacOS()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }

        #endregion

        #region 网络时间

        /*
        /// <summary>
        /// 获取网络时间
        /// </summary>
        /// <returns>网络时间</returns>
        public static DateTime GetNetworkTime()
        {
            const string ntpServer = "time.windows.com";
            byte[] ntpData = new byte[48];
            ntpData[0] = 0x1B;
            IPAddress[] addresses = Dns.GetHostEntry(ntpServer).AddressList;
            IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);
                socket.Send(ntpData);
                socket.Receive(ntpData);
            }
            const byte offsetTransmitTime = 40;
            uint intPart = BitConverter.ToUInt32(ntpData, offsetTransmitTime);
            uint fractPart = BitConverter.ToUInt32(ntpData, offsetTransmitTime + 4);
            ulong milliseconds = (ulong)(intPart * 1000) + ((ulong)fractPart * 1000) / 0x100000000L);
            return new DateTime(1900, 1, 1, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)milliseconds).ToLocalTime();
        }
        */

        #endregion
    }
}
