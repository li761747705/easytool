using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 文件监视器
    /// 提供文件变更监视功能
    /// </summary>
    public class FileWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly Dictionary<string, DateTime> _lastWriteTimes = new();
        private readonly object _lock = new();
        private int _debounceMilliseconds = 100;

        /// <summary>
        /// 文件变更事件
        /// </summary>
        public event EventHandler<FileChangedEventArgs>? FileChanged;

        /// <summary>
        /// 文件创建事件
        /// </summary>
        public event EventHandler<FileChangedEventArgs>? FileCreated;

        /// <summary>
        /// 文件删除事件
        /// </summary>
        public event EventHandler<FileChangedEventArgs>? FileDeleted;

        /// <summary>
        /// 文件重命名事件
        /// </summary>
        public event EventHandler<FileRenamedEventArgs>? FileRenamed;

        /// <summary>
        /// 错误事件
        /// </summary>
        public event EventHandler<ErrorEventArgs>? Error;

        /// <summary>
        /// 监视路径
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 监视筛选器
        /// </summary>
        public string Filter
        {
            get => _watcher.Filter;
            set => _watcher.Filter = value;
        }

        /// <summary>
        /// 是否包含子目录
        /// </summary>
        public bool IncludeSubdirectories
        {
            get => _watcher.IncludeSubdirectories;
            set => _watcher.IncludeSubdirectories = value;
        }

        /// <summary>
        /// 防抖时间（毫秒）
        /// </summary>
        public int DebounceMilliseconds
        {
            get => _debounceMilliseconds;
            set => _debounceMilliseconds = Math.Max(0, value);
        }

        /// <summary>
        /// 是否正在监视
        /// </summary>
        public bool IsWatching => _watcher.EnableRaisingEvents;

        /// <summary>
        /// 创建文件监视器
        /// </summary>
        /// <param name="path">监视路径</param>
        public FileWatcher(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"目录不存在: {path}");

            Path = path;
            _watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName |
                               NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;
        }

        /// <summary>
        /// 创建文件监视器
        /// </summary>
        /// <param name="path">监视路径</param>
        /// <param name="filter">文件筛选器</param>
        public FileWatcher(string path, string filter) : this(path)
        {
            Filter = filter;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            // 防抖处理
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                if (_lastWriteTimes.TryGetValue(e.FullPath, out var lastWrite))
                {
                    if ((now - lastWrite).TotalMilliseconds < _debounceMilliseconds)
                        return;
                }
                _lastWriteTimes[e.FullPath] = now;
            }

            FileChanged?.Invoke(this, new FileChangedEventArgs(e.FullPath, e.Name!, ChangeType.Changed));
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            FileCreated?.Invoke(this, new FileChangedEventArgs(e.FullPath, e.Name!, ChangeType.Created));
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            lock (_lock)
            {
                _lastWriteTimes.Remove(e.FullPath);
            }
            FileDeleted?.Invoke(this, new FileChangedEventArgs(e.FullPath, e.Name!, ChangeType.Deleted));
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            lock (_lock)
            {
                _lastWriteTimes.Remove(e.OldFullPath);
            }
            FileRenamed?.Invoke(this, new FileRenamedEventArgs(e.FullPath, e.Name!, e.OldFullPath, e.OldName!));
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        /// <summary>
        /// 开始监视
        /// </summary>
        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// 停止监视
        /// </summary>
        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnChanged;
            _watcher.Created -= OnCreated;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;
            _watcher.Error -= OnError;
            _watcher.Dispose();
        }
    }

    /// <summary>
    /// 文件变更类型
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// 已更改
        /// </summary>
        Changed,

        /// <summary>
        /// 已创建
        /// </summary>
        Created,

        /// <summary>
        /// 已删除
        /// </summary>
        Deleted,

        /// <summary>
        /// 已重命名
        /// </summary>
        Renamed
    }

    /// <summary>
    /// 文件变更事件参数
    /// </summary>
    public class FileChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 完整路径
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 变更类型
        /// </summary>
        public ChangeType ChangeType { get; }

        /// <summary>
        /// 创建事件参数
        /// </summary>
        public FileChangedEventArgs(string fullPath, string name, ChangeType changeType)
        {
            FullPath = fullPath;
            Name = name;
            ChangeType = changeType;
        }
    }

    /// <summary>
    /// 文件重命名事件参数
    /// </summary>
    public class FileRenamedEventArgs : EventArgs
    {
        /// <summary>
        /// 新完整路径
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// 新文件名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 旧完整路径
        /// </summary>
        public string OldFullPath { get; }

        /// <summary>
        /// 旧文件名
        /// </summary>
        public string OldName { get; }

        /// <summary>
        /// 创建事件参数
        /// </summary>
        public FileRenamedEventArgs(string fullPath, string name, string oldFullPath, string oldName)
        {
            FullPath = fullPath;
            Name = name;
            OldFullPath = oldFullPath;
            OldName = oldName;
        }
    }

    /// <summary>
    /// 目录监视器
    /// </summary>
    public class DirectoryWatcher : IDisposable
    {
        private readonly List<FileWatcher> _watchers = new();

        /// <summary>
        /// 文件变更事件
        /// </summary>
        public event EventHandler<FileChangedEventArgs>? FileChanged;

        /// <summary>
        /// 文件创建事件
        /// </summary>
        public event EventHandler<FileChangedEventArgs>? FileCreated;

        /// <summary>
        /// 文件删除事件
        /// </summary>
        public event EventHandler<FileChangedEventArgs>? FileDeleted;

        /// <summary>
        /// 文件重命名事件
        /// </summary>
        public event EventHandler<FileRenamedEventArgs>? FileRenamed;

        /// <summary>
        /// 创建目录监视器
        /// </summary>
        /// <param name="paths">监视路径列表</param>
        public DirectoryWatcher(params string[] paths)
        {
            foreach (var path in paths)
            {
                AddPath(path);
            }
        }

        /// <summary>
        /// 添加监视路径
        /// </summary>
        /// <param name="path">路径</param>
        public void AddPath(string path)
        {
            var watcher = new FileWatcher(path);
            watcher.FileChanged += (s, e) => FileChanged?.Invoke(s, e);
            watcher.FileCreated += (s, e) => FileCreated?.Invoke(s, e);
            watcher.FileDeleted += (s, e) => FileDeleted?.Invoke(s, e);
            watcher.FileRenamed += (s, e) => FileRenamed?.Invoke(s, e);
            _watchers.Add(watcher);
        }

        /// <summary>
        /// 添加监视路径
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="filter">筛选器</param>
        public void AddPath(string path, string filter)
        {
            var watcher = new FileWatcher(path, filter);
            watcher.FileChanged += (s, e) => FileChanged?.Invoke(s, e);
            watcher.FileCreated += (s, e) => FileCreated?.Invoke(s, e);
            watcher.FileDeleted += (s, e) => FileDeleted?.Invoke(s, e);
            watcher.FileRenamed += (s, e) => FileRenamed?.Invoke(s, e);
            _watchers.Add(watcher);
        }

        /// <summary>
        /// 开始监视所有路径
        /// </summary>
        public void StartAll()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Start();
            }
        }

        /// <summary>
        /// 停止监视所有路径
        /// </summary>
        public void StopAll()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Stop();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
            _watchers.Clear();
        }
    }
}
