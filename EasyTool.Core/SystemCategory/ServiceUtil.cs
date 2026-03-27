using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace EasyTool.SystemCategory
{
    /// <summary>
    /// Windows服务工具类
    /// </summary>
    public static class ServiceUtil
    {
        /// <summary>
        /// 获取所有服务
        /// </summary>
        /// <returns>服务列表</returns>
        public static List<ServiceController> GetAllServices()
        {
            return new List<ServiceController>(ServiceController.GetServices());
        }

        /// <summary>
        /// 获取指定服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>服务控制器</returns>
        public static ServiceController? GetService(string serviceName)
        {
            try
            {
                return new ServiceController(serviceName);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 检查服务是否存在
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>是否存在</returns>
        public static bool ServiceExists(string serviceName)
        {
            try
            {
                var services = ServiceController.GetServices();
                foreach (var service in services)
                {
                    if (service.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                    {
                        service.Dispose();
                        return true;
                    }
                    service.Dispose();
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否成功</returns>
        public static bool StartService(string serviceName, TimeSpan? timeout = null)
        {
            using var service = GetService(serviceName);
            if (service == null) return false;

            try
            {
                if (service.Status == ServiceControllerStatus.Running)
                    return true;

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout ?? TimeSpan.FromMinutes(1));
                return service.Status == ServiceControllerStatus.Running;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否成功</returns>
        public static bool StopService(string serviceName, TimeSpan? timeout = null)
        {
            using var service = GetService(serviceName);
            if (service == null) return false;

            try
            {
                if (service.Status == ServiceControllerStatus.Stopped)
                    return true;

                if (!service.CanStop)
                    return false;

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout ?? TimeSpan.FromMinutes(1));
                return service.Status == ServiceControllerStatus.Stopped;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 重启服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否成功</returns>
        public static bool RestartService(string serviceName, TimeSpan? timeout = null)
        {
            if (!StopService(serviceName, timeout))
                return false;

            System.Threading.Thread.Sleep(1000);
            return StartService(serviceName, timeout);
        }

        /// <summary>
        /// 暂停服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否成功</returns>
        public static bool PauseService(string serviceName, TimeSpan? timeout = null)
        {
            using var service = GetService(serviceName);
            if (service == null) return false;

            try
            {
                if (!service.CanPauseAndContinue)
                    return false;

                service.Pause();
                service.WaitForStatus(ServiceControllerStatus.Paused, timeout ?? TimeSpan.FromMinutes(1));
                return service.Status == ServiceControllerStatus.Paused;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 继续服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否成功</returns>
        public static bool ContinueService(string serviceName, TimeSpan? timeout = null)
        {
            using var service = GetService(serviceName);
            if (service == null) return false;

            try
            {
                if (!service.CanPauseAndContinue)
                    return false;

                service.Continue();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout ?? TimeSpan.FromMinutes(1));
                return service.Status == ServiceControllerStatus.Running;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>服务状态</returns>
        public static ServiceControllerStatus? GetServiceStatus(string serviceName)
        {
            using var service = GetService(serviceName);
            return service?.Status;
        }

        /// <summary>
        /// 检查服务是否正在运行
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>是否正在运行</returns>
        public static bool IsServiceRunning(string serviceName)
        {
            return GetServiceStatus(serviceName) == ServiceControllerStatus.Running;
        }

        /// <summary>
        /// 检查服务是否已停止
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>是否已停止</returns>
        public static bool IsServiceStopped(string serviceName)
        {
            return GetServiceStatus(serviceName) == ServiceControllerStatus.Stopped;
        }

        /// <summary>
        /// 获取服务信息
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>服务信息</returns>
        public static ServiceInfo? GetServiceInfo(string serviceName)
        {
            using var service = GetService(serviceName);
            if (service == null) return null;

            return new ServiceInfo
            {
                ServiceName = service.ServiceName,
                DisplayName = service.DisplayName,
                Status = service.Status,
                CanStop = service.CanStop,
                CanPauseAndContinue = service.CanPauseAndContinue,
                ServiceType = service.ServiceType,
                MachineName = service.MachineName
            };
        }

        /// <summary>
        /// 按状态获取服务列表
        /// </summary>
        /// <param name="status">服务状态</param>
        /// <returns>服务列表</returns>
        public static List<ServiceInfo> GetServicesByStatus(ServiceControllerStatus status)
        {
            var result = new List<ServiceInfo>();
            var services = ServiceController.GetServices();

            foreach (var service in services)
            {
                if (service.Status == status)
                {
                    result.Add(new ServiceInfo
                    {
                        ServiceName = service.ServiceName,
                        DisplayName = service.DisplayName,
                        Status = service.Status,
                        CanStop = service.CanStop,
                        CanPauseAndContinue = service.CanPauseAndContinue,
                        ServiceType = service.ServiceType
                    });
                }
                service.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 获取正在运行的服务
        /// </summary>
        /// <returns>服务列表</returns>
        public static List<ServiceInfo> GetRunningServices()
        {
            return GetServicesByStatus(ServiceControllerStatus.Running);
        }

        /// <summary>
        /// 获取已停止的服务
        /// </summary>
        /// <returns>服务列表</returns>
        public static List<ServiceInfo> GetStoppedServices()
        {
            return GetServicesByStatus(ServiceControllerStatus.Stopped);
        }

        /// <summary>
        /// 等待服务达到指定状态
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="targetStatus">目标状态</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>是否成功</returns>
        public static bool WaitForStatus(string serviceName, ServiceControllerStatus targetStatus, TimeSpan? timeout = null)
        {
            using var service = GetService(serviceName);
            if (service == null) return false;

            try
            {
                service.WaitForStatus(targetStatus, timeout ?? TimeSpan.FromMinutes(1));
                return service.Status == targetStatus;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 异步启动服务
        /// </summary>
        public static Task<bool> StartServiceAsync(string serviceName, TimeSpan? timeout = null)
        {
            return Task.Run(() => StartService(serviceName, timeout));
        }

        /// <summary>
        /// 异步停止服务
        /// </summary>
        public static Task<bool> StopServiceAsync(string serviceName, TimeSpan? timeout = null)
        {
            return Task.Run(() => StopService(serviceName, timeout));
        }

        /// <summary>
        /// 异步重启服务
        /// </summary>
        public static Task<bool> RestartServiceAsync(string serviceName, TimeSpan? timeout = null)
        {
            return Task.Run(() => RestartService(serviceName, timeout));
        }
    }

    /// <summary>
    /// 服务信息
    /// </summary>
    public class ServiceInfo
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 服务状态
        /// </summary>
        public ServiceControllerStatus Status { get; set; }

        /// <summary>
        /// 是否可以停止
        /// </summary>
        public bool CanStop { get; set; }

        /// <summary>
        /// 是否可以暂停和继续
        /// </summary>
        public bool CanPauseAndContinue { get; set; }

        /// <summary>
        /// 服务类型
        /// </summary>
        public ServiceType ServiceType { get; set; }

        /// <summary>
        /// 机器名
        /// </summary>
        public string MachineName { get; set; } = ".";

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => Status == ServiceControllerStatus.Running;

        /// <summary>
        /// 是否已停止
        /// </summary>
        public bool IsStopped => Status == ServiceControllerStatus.Stopped;

        public override string ToString()
        {
            return $"{ServiceName} ({DisplayName}) - {Status}";
        }
    }
}
