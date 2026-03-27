using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// 文件监听器增强版
    /// </summary>
    public class FileWatcherEx : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly Dictionary<string, DateTime> _lastEvents = new();
        private readonly TimeSpan _debounceInterval;
        private readonly object _lock = new();

        /// <summary>
        /// 文件创建事件
        /// </summary>
        public event EventHandler<FileChangedEventArgs>? FileCreated;

        /// <summary>
        /// 文件删除事件
        /// </summary>
        public event EventHandler<FileChangedEventArgs>? FileDeleted;

        /// <summary>
        /// 文件修改事件
        /// </summary>
        public event EventHandler<FileChangedEventArgs>? FileChanged;

        /// <summary>
        /// 文件重命名事件
        /// </summary>
        public event EventHandler<FileRenamedEventArgs>? FileRenamed;

        /// <summary>
        /// 错误事件
        /// </summary>
        public event EventHandler<ErrorEventArgs>? Error;

        /// <summary>
        /// 监视的目录路径
        /// </summary>
        public string Path => _watcher.Path;

        /// <summary>
        /// 监视的文件过滤器
        /// </summary>
        public string Filter => _watcher.Filter;

        /// <summary>
        /// 是否包含子目录
        /// </summary>
        public bool IncludeSubdirectories => _watcher.IncludeSubdirectories;

        /// <summary>
        /// 创建文件监听器
        /// </summary>
        /// <param name="path">监视目录</param>
        /// <param name="filter">文件过滤器</param>
        /// <param name="includeSubdirectories">包含子目录</param>
        /// <param name="debounceInterval">防抖间隔</param>
        public FileWatcherEx(string path, string filter = "*.*", bool includeSubdirectories = true, TimeSpan? debounceInterval = null)
        {
            _debounceInterval = debounceInterval ?? TimeSpan.FromMilliseconds(100);
            _watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = filter,
                IncludeSubdirectories = includeSubdirectories,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName |
                               NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Changed += OnChanged;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;
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

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (ShouldProcess(e.FullPath, "Created"))
            {
                FileCreated?.Invoke(this, new FileChangedEventArgs(e.FullPath, e.Name ?? string.Empty, ChangeType.Created));
            }
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (ShouldProcess(e.FullPath, "Deleted"))
            {
                FileDeleted?.Invoke(this, new FileChangedEventArgs(e.FullPath, e.Name ?? string.Empty, ChangeType.Deleted));
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (ShouldProcess(e.FullPath, "Changed"))
            {
                FileChanged?.Invoke(this, new FileChangedEventArgs(e.FullPath, e.Name ?? string.Empty, ChangeType.Changed));
            }
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (ShouldProcess(e.FullPath, "Renamed"))
            {
                FileRenamed?.Invoke(this, new FileRenamedEventArgs(e.FullPath, e.Name ?? string.Empty, e.OldFullPath, e.OldName ?? string.Empty));
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        private bool ShouldProcess(string path, string eventType)
        {
            lock (_lock)
            {
                var key = $"{path}|{eventType}";
                var now = DateTime.UtcNow;

                if (_lastEvents.TryGetValue(key, out var lastTime))
                {
                    if (now - lastTime < _debounceInterval)
                    {
                        return false;
                    }
                }

                _lastEvents[key] = now;
                return true;
            }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }

    /// <summary>
    /// 目录监视器
    /// </summary>
    public class DirectoryMonitor : IDisposable
    {
        private readonly string _path;
        private readonly FileWatcherEx _watcher;
        private readonly Dictionary<string, FileInfo> _files = new();
        private readonly object _lock = new();

        /// <summary>
        /// 文件添加事件
        /// </summary>
        public event EventHandler<FileInfo>? FileAdded;

        /// <summary>
        /// 文件移除事件
        /// </summary>
        public event EventHandler<FileInfo>? FileRemoved;

        /// <summary>
        /// 文件修改事件
        /// </summary>
        public event EventHandler<FileInfo>? FileModified;

        /// <summary>
        /// 当前文件列表
        /// </summary>
        public IReadOnlyList<FileInfo> CurrentFiles
        {
            get
            {
                lock (_lock)
                {
                    return _files.Values.ToList().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// 创建目录监视器
        /// </summary>
        public DirectoryMonitor(string path, string filter = "*.*", bool includeSubdirectories = true)
        {
            _path = path;
            _watcher = new FileWatcherEx(path, filter, includeSubdirectories);
            _watcher.FileCreated += OnFileCreated;
            _watcher.FileDeleted += OnFileDeleted;
            _watcher.FileChanged += OnFileChanged;
        }

        /// <summary>
        /// 开始监视
        /// </summary>
        public void Start()
        {
            // 初始化现有文件
            InitializeFiles();
            _watcher.Start();
        }

        /// <summary>
        /// 停止监视
        /// </summary>
        public void Stop()
        {
            _watcher.Stop();
        }

        private void InitializeFiles()
        {
            lock (_lock)
            {
                _files.Clear();

                if (Directory.Exists(_path))
                {
                    foreach (var file in Directory.GetFiles(_path, "*", SearchOption.AllDirectories))
                    {
                        var info = new FileInfo(file);
                        _files[file] = info;
                    }
                }
            }
        }

        private void OnFileCreated(object? sender, FileChangedEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                var info = new FileInfo(e.FullPath);
                lock (_lock)
                {
                    _files[e.FullPath] = info;
                }
                FileAdded?.Invoke(this, info);
            }
        }

        private void OnFileDeleted(object? sender, FileChangedEventArgs e)
        {
            FileInfo? info;
            lock (_lock)
            {
                if (_files.TryGetValue(e.FullPath, out info))
                {
                    _files.Remove(e.FullPath);
                }
            }

            if (info != null)
            {
                FileRemoved?.Invoke(this, info);
            }
        }

        private void OnFileChanged(object? sender, FileChangedEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                var info = new FileInfo(e.FullPath);
                lock (_lock)
                {
                    _files[e.FullPath] = info;
                }
                FileModified?.Invoke(this, info);
            }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}
