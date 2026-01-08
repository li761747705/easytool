using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace EasyTool
{
    /// <summary>
    /// 网络工具
    /// </summary>
    public class NetUtil
    {
        // Ping a host and return true if the ping was successful
        // 对指定主机进行Ping测试，返回是否成功
        public static bool Ping(string host)
        {
            try
            {
                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(host);

                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        // Resolve the IP address of a host
        // 获取指定主机的IP地址
        public static IPAddress? GetIpAddress(string host)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(host);

                foreach (IPAddress address in hostEntry.AddressList)
                {
                    // 返回IPv4地址
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return address;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Check if a port is open on a given IP address
        // 检查给定IP地址上的端口是否开放
        public static bool IsPortOpen(string host, int port)
        {
            try
            {
                // 获取IP地址
                IPAddress ipAddress = GetIpAddress(host);

                if (ipAddress == null)
                {
                    return false;
                }

                // 创建套接字，连接端口
                IPEndPoint endpoint = new IPEndPoint(ipAddress, port);
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Connect(endpoint);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Send an HTTP GET request and return the response
        // 发送HTTP GET请求并返回响应
        public static string? HttpGet(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    return client.GetStringAsync(url).GetAwaiter().GetResult();
                }
            }
            catch
            {
                return null;
            }
        }

        // Send an HTTP POST request and return the response
        // 发送HTTP POST请求并返回响应
        public static string? HttpPost(string url, string data)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    StringContent content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                    HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                    return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
