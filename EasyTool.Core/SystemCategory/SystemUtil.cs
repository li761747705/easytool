using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// 系统工具类，提供系统、进程、内存、网络等相关功能
    /// </summary>
    public static class SystemUtil
    {
        #region DLL 工具

        /// <summary>
        /// 根据类型名称创建实例，并返回一个 Object 对象
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="typeName">类型名称</param>
        /// <param name="parameters">实例化类型所需要的参数</param>
        /// <returns>返回创建的实例对象</returns>
        public static object? CreateInstanceFromAssembly(Assembly assembly, string typeName, params object[] parameters)
        {
            Type? type = assembly.GetType(typeName);
            if (type != null)
            {
                return Activator.CreateInstance(type, parameters);
            }
            return null;
        }

        /// <summary>
        /// 调用对象的方法，并返回调用结果
        /// </summary>
        /// <param name="instance">要调用方法的对象</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameters">方法所需要的参数</param>
        /// <returns>返回调用结果</returns>
        public static object? InvokeMethod(object instance, string methodName, params object[] parameters)
        {
            Type type = instance.GetType();
            MethodInfo? methodInfo = type.GetMethod(methodName);
            if (methodInfo != null)
            {
                return methodInfo.Invoke(instance, parameters);
            }
            return null;
        }

        /// <summary>
        /// 从指定目录中加载所有的 DLL 文件，并返回一个 Assembly[] 数组
        /// </summary>
        /// <param name="directory">要加载 DLL 文件的目录</param>
        /// <returns>返回一个 Assembly[] 数组，数组中每个元素代表一个 DLL 程序集</returns>
        public static Assembly[] LoadAllDllsFromDirectory(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    throw new Exception("LoadAllDllsFromDirectory Error: Directory not exist.");
                }

                string[] dllFiles = Directory.GetFiles(directory, "*.dll");
                if (dllFiles.Length == 0)
                {
                    throw new Exception("LoadAllDllsFromDirectory Error: No DLL file found.");
                }

                Assembly[] assemblies = new Assembly[dllFiles.Length];
                for (int i = 0; i < dllFiles.Length; i++)
                {
                    assemblies[i] = Assembly.LoadFile(dllFiles[i]);
                }
                return assemblies;
            }
            catch (Exception ex)
            {
                throw new Exception("LoadAllDllsFromDirectory Error: " + ex.Message);
            }
        }

        #endregion

        #region 进程工具

        /// <summary>
        /// 通过进程名称获取进程
        /// </summary>
        /// <param name="processName">进程名称</param>
        /// <returns>进程</returns>
        public static Process? GetProcessByName(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                return processes[0];
            }
            return null;
        }

        /// <summary>
        /// 判断进程是否存在
        /// </summary>
        /// <param name="processName">进程名称</param>
        /// <returns>是否存在</returns>
        public static bool IsProcessExists(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }

        /// <summary>
        /// 暂停进程
        /// </summary>
        /// <param name="process">进程</param>
        public static void SuspendProcess(Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    break;
                }
                SuspendThread(pOpenThread);
                CloseHandle(pOpenThread);
            }
        }

        /// <summary>
        /// 恢复进程
        /// </summary>
        /// <param name="process">进程</param>
        public static void ResumeProcess(Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    break;
                }
                ResumeThread(pOpenThread);
                CloseHandle(pOpenThread);
            }
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern IntPtr CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        static extern uint ResumeThread(IntPtr hThread);

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        #endregion

        #region 运行时工具

        /// <summary>
        /// 获取当前运行的处理器架构
        /// </summary>
        /// <returns>处理器架构</returns>
        public static string GetProcessArchitecture()
        {
            return Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        }

        /// <summary>
        /// 获取当前应用程序内存使用量
        /// </summary>
        /// <returns>内存使用量（字节）</returns>
        public static long GetCurrentMemoryUsage()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return GC.GetTotalMemory(true);
        }

        /// <summary>
        /// 关闭当前应用程序
        /// </summary>
        public static void ExitApplication()
        {
            Environment.Exit(0);
        }

        #endregion

        #region 网络工具

        /// <summary>
        /// 对指定主机进行 Ping 测试，返回是否成功
        /// </summary>
        /// <param name="host">主机名或IP地址</param>
        /// <returns>是否成功</returns>
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

        /// <summary>
        /// 获取指定主机的IP地址
        /// </summary>
        /// <param name="host">主机名</param>
        /// <returns>IP地址</returns>
        public static IPAddress? GetIpAddress(string host)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(host);
                foreach (IPAddress address in hostEntry.AddressList)
                {
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

        /// <summary>
        /// 检查给定IP地址上的端口是否开放
        /// </summary>
        /// <param name="host">IP地址</param>
        /// <param name="port">端口号</param>
        /// <returns>端口是否开放</returns>
        public static bool IsPortOpen(string host, int port)
        {
            try
            {
                IPAddress ipAddress = GetIpAddress(host);
                if (ipAddress == null)
                {
                    return false;
                }

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

        /// <summary>
        /// 发送HTTP GET请求并返回响应
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>响应内容</returns>
        public static async Task<string> HttpGetAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        }

        /// <summary>
        /// 发送HTTP POST请求并返回响应
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="data">要发送的数据</param>
        /// <returns>响应内容</returns>
        public static async Task<string> HttpPostAsync(string url, string data)
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.PostAsync(url, content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        #endregion
    }
}
