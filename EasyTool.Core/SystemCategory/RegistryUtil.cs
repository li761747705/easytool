using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EasyTool.NetCategory
{
    /// <summary>
    /// 注册表工具类
    /// </summary>
    public static class RegistryUtil
    {
        /// <summary>
        /// 读取注册表值
        /// </summary>
        public static string? GetValue(string path, string name)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path);
            return key?.GetValue(name)?.ToString();
        }

        /// <summary>
        /// 读取注册表值（指定根键）
        /// </summary>
        public static string? GetValue(Microsoft.Win32.RegistryHive hive, string path, string name)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            using var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey(hive, Microsoft.Win32.RegistryView.Default);
            using var key = baseKey.OpenSubKey(path);
            return key?.GetValue(name)?.ToString();
        }

        /// <summary>
        /// 设置注册表值
        /// </summary>
        public static void SetValue(string path, string name, object value, Microsoft.Win32.RegistryValueKind valueKind = Microsoft.Win32.RegistryValueKind.String)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            using var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(path);
            key?.SetValue(name, value, valueKind);
        }

        /// <summary>
        /// 设置注册表值（指定根键）
        /// </summary>
        public static void SetValue(Microsoft.Win32.RegistryHive hive, string path, string name, object value, Microsoft.Win32.RegistryValueKind valueKind = Microsoft.Win32.RegistryValueKind.String)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            using var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey(hive, Microsoft.Win32.RegistryView.Default);
            using var key = baseKey.CreateSubKey(path);
            key?.SetValue(name, value, valueKind);
        }

        /// <summary>
        /// 删除注册表值
        /// </summary>
        public static void DeleteValue(string path, string name)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path, true);
            key?.DeleteValue(name, false);
        }

        /// <summary>
        /// 删除注册表键
        /// </summary>
        public static void DeleteSubKey(string path)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            var parentPath = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            var keyName = System.IO.Path.GetFileName(path);
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(parentPath, true);
            key?.DeleteSubKey(keyName, false);
        }

        /// <summary>
        /// 检查键是否存在
        /// </summary>
        public static bool KeyExists(string path)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path);
            return key != null;
        }

        /// <summary>
        /// 检查值是否存在
        /// </summary>
        public static bool ValueExists(string path, string name)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path);
            return key?.GetValue(name) != null;
        }

        /// <summary>
        /// 获取所有子键名称
        /// </summary>
        public static string[] GetSubKeyNames(string path)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path);
            return key?.GetSubKeyNames() ?? Array.Empty<string>();
        }

        /// <summary>
        /// 获取所有值名称
        /// </summary>
        public static string[] GetValueNames(string path)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path);
            return key?.GetValueNames() ?? Array.Empty<string>();
        }

        /// <summary>
        /// 获取开机启动项列表
        /// </summary>
        public static System.Collections.Generic.Dictionary<string, string> GetStartupPrograms()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            var programs = new System.Collections.Generic.Dictionary<string, string>();
            
            var paths = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run"
            };

            foreach (var path in paths)
            {
                var names = GetValueNames(path);
                foreach (var name in names)
                {
                    var value = GetValue(path, name);
                    if (value != null && !programs.ContainsKey(name))
                    {
                        programs[name] = value;
                    }
                }
            }

            return programs;
        }

        /// <summary>
        /// 添加开机启动项
        /// </summary>
        public static void AddStartupProgram(string name, string command)
        {
            SetValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", name, command);
        }

        /// <summary>
        /// 删除开机启动项
        /// </summary>
        public static void RemoveStartupProgram(string name)
        {
            DeleteValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", name);
        }

        /// <summary>
        /// 获取已安装软件列表
        /// </summary>
        public static System.Collections.Generic.List<InstalledProgram> GetInstalledPrograms()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException("此功能仅支持 Windows 平台");
            }

            var programs = new System.Collections.Generic.List<InstalledProgram>();
            
            var paths = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var path in paths)
            {
                var subKeys = GetSubKeyNames(path);
                foreach (var subKey in subKeys)
                {
                    var programPath = $@"{path}\{subKey}";
                    var name = GetValue(programPath, "DisplayName");
                    if (name != null)
                    {
                        programs.Add(new InstalledProgram
                        {
                            Name = name,
                            Version = GetValue(programPath, "DisplayVersion"),
                            Publisher = GetValue(programPath, "Publisher"),
                            InstallDate = GetValue(programPath, "InstallDate"),
                            UninstallString = GetValue(programPath, "UninstallString"),
                            InstallLocation = GetValue(programPath, "InstallLocation")
                        });
                    }
                }
            }

            return programs;
        }

        /// <summary>
        /// 设置文件关联
        /// </summary>
        public static void SetFileAssociation(string extension, string programPath, string description)
        {
            var progId = extension.TrimStart('.');
            SetValue($@"SOFTWARE\Classes\{extension}", "", progId);
            SetValue($@"SOFTWARE\Classes\{progId}", "", description);
            SetValue($@"SOFTWARE\Classes\{progId}\shell\open\command", "", $"\"{programPath}\" \"%1\"");
        }

        /// <summary>
        /// 添加右键菜单项
        /// </summary>
        public static void AddContextMenu(string name, string command, string? iconPath = null)
        {
            var path = $@"SOFTWARE\Classes\*\shell\{name}";
            SetValue(path, "", name);
            SetValue($@"{path}\command", "", command);
            if (iconPath != null)
            {
                SetValue(path, "Icon", iconPath);
            }
        }

        /// <summary>
        /// 删除右键菜单项
        /// </summary>
        public static void RemoveContextMenu(string name)
        {
            DeleteSubKey($@"SOFTWARE\Classes\*\shell\{name}\command");
            DeleteSubKey($@"SOFTWARE\Classes\*\shell\{name}");
        }
    }

    /// <summary>
    /// 已安装程序信息
    /// </summary>
    public class InstalledProgram
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// 发布者
        /// </summary>
        public string? Publisher { get; set; }

        /// <summary>
        /// 安装日期
        /// </summary>
        public string? InstallDate { get; set; }

        /// <summary>
        /// 卸载命令
        /// </summary>
        public string? UninstallString { get; set; }

        /// <summary>
        /// 安装位置
        /// </summary>
        public string? InstallLocation { get; set; }

        public override string ToString()
        {
            return $"{Name} {Version}";
        }
    }
}
