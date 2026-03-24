using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.ToolCategory
{
    /// <summary>
    /// 版本号工具类
    /// 提供版本号解析、比较和验证功能
    /// </summary>
    public static class VersionUtil
    {
        /// <summary>
        /// 解析版本号字符串
        /// </summary>
        /// <param name="version">版本号字符串（如 "1.2.3" 或 "v1.2.3-beta"）</param>
        /// <returns>版本信息对象</returns>
        public static VersionInfo Parse(string? version)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("版本号不能为空");

            // 移除 v 或 V 前缀
            var normalized = version.TrimStart('v', 'V');

            // 分离预发布标签
            string? preRelease = null;
            var preReleaseIndex = normalized.IndexOf('-');
            if (preReleaseIndex >= 0)
            {
                preRelease = normalized.Substring(preReleaseIndex + 1);
                normalized = normalized.Substring(0, preReleaseIndex);
            }

            // 分离构建元数据
            string? buildMetadata = null;
            var buildIndex = normalized.IndexOf('+');
            if (buildIndex >= 0)
            {
                buildMetadata = normalized.Substring(buildIndex + 1);
                normalized = normalized.Substring(0, buildIndex);
            }

            // 解析版本号部分
            var parts = normalized.Split('.');
            if (parts.Length == 0 || parts.Length > 4)
                throw new FormatException($"无效的版本号格式: {version}");

            var info = new VersionInfo
            {
                Original = version,
                PreRelease = preRelease,
                BuildMetadata = buildMetadata
            };

            if (int.TryParse(parts[0], out var major))
                info.Major = major;
            else
                throw new FormatException($"无效的主版本号: {parts[0]}");

            if (parts.Length > 1)
            {
                if (int.TryParse(parts[1], out var minor))
                    info.Minor = minor;
                else
                    throw new FormatException($"无效的次版本号: {parts[1]}");
            }

            if (parts.Length > 2)
            {
                if (int.TryParse(parts[2], out var patch))
                    info.Patch = patch;
                else
                    throw new FormatException($"无效的补丁版本号: {parts[2]}");
            }

            if (parts.Length > 3)
            {
                if (int.TryParse(parts[3], out var revision))
                    info.Revision = revision;
                else
                    throw new FormatException($"无效的修订版本号: {parts[3]}");
            }

            return info;
        }

        /// <summary>
        /// 尝试解析版本号
        /// </summary>
        /// <param name="version">版本号字符串</param>
        /// <param name="info">解析结果</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParse(string? version, out VersionInfo? info)
        {
            info = null;
            if (string.IsNullOrWhiteSpace(version))
                return false;

            try
            {
                info = Parse(version);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 比较两个版本号
        /// </summary>
        /// <param name="version1">版本号1</param>
        /// <param name="version2">版本号2</param>
        /// <returns>比较结果：-1表示小于，0表示等于，1表示大于</returns>
        public static int Compare(string? version1, string? version2)
        {
            if (!TryParse(version1, out var info1))
                info1 = new VersionInfo();
            if (!TryParse(version2, out var info2))
                info2 = new VersionInfo();

            return Compare(info1!, info2!);
        }

        /// <summary>
        /// 比较两个版本信息
        /// </summary>
        /// <param name="v1">版本1</param>
        /// <param name="v2">版本2</param>
        /// <returns>比较结果</returns>
        public static int Compare(VersionInfo v1, VersionInfo v2)
        {
            if (v1.Major != v2.Major)
                return v1.Major.CompareTo(v2.Major);
            if (v1.Minor != v2.Minor)
                return v1.Minor.CompareTo(v2.Minor);
            if (v1.Patch != v2.Patch)
                return v1.Patch.CompareTo(v2.Patch);
            if (v1.Revision != v2.Revision)
                return v1.Revision.CompareTo(v2.Revision);

            // 主版本号相同时，比较预发布标签
            // 没有预发布标签的版本高于有预发布标签的版本
            if (string.IsNullOrEmpty(v1.PreRelease) && !string.IsNullOrEmpty(v2.PreRelease))
                return 1;
            if (!string.IsNullOrEmpty(v1.PreRelease) && string.IsNullOrEmpty(v2.PreRelease))
                return -1;

            if (!string.IsNullOrEmpty(v1.PreRelease) && !string.IsNullOrEmpty(v2.PreRelease))
                return string.Compare(v1.PreRelease, v2.PreRelease, StringComparison.Ordinal);

            return 0;
        }

        /// <summary>
        /// 判断版本号是否在指定范围内
        /// </summary>
        /// <param name="version">要检查的版本号</param>
        /// <param name="min">最小版本号（包含）</param>
        /// <param name="max">最大版本号（包含）</param>
        /// <returns>是否在范围内</returns>
        public static bool IsInRange(string? version, string? min, string? max)
        {
            var info = Parse(version);
            var minInfo = string.IsNullOrEmpty(min) ? null : Parse(min);
            var maxInfo = string.IsNullOrEmpty(max) ? null : Parse(max);

            if (minInfo != null && Compare(info, minInfo) < 0)
                return false;
            if (maxInfo != null && Compare(info, maxInfo) > 0)
                return false;

            return true;
        }

        /// <summary>
        /// 获取下一个版本号
        /// </summary>
        /// <param name="version">当前版本号</param>
        /// <param name="level">递增级别：Major, Minor, Patch</param>
        /// <returns>下一个版本号</returns>
        public static string Next(string? version, VersionLevel level = VersionLevel.Patch)
        {
            var info = TryParse(version, out var v) ? v! : new VersionInfo();

            return level switch
            {
                VersionLevel.Major => $"{info!.Major + 1}.0.0",
                VersionLevel.Minor => $"{info!.Major}.{info.Minor + 1}.0",
                VersionLevel.Patch => $"{info!.Major}.{info.Minor}.{info.Patch + 1}",
                VersionLevel.Revision => $"{info!.Major}.{info.Minor}.{info.Patch}.{info.Revision + 1}",
                _ => info!.ToString()
            };
        }

        /// <summary>
        /// 获取版本号之间的差异描述
        /// </summary>
        /// <param name="oldVersion">旧版本</param>
        /// <param name="newVersion">新版本</param>
        /// <returns>差异描述</returns>
        public static VersionDiff GetDiff(string? oldVersion, string? newVersion)
        {
            var oldInfo = TryParse(oldVersion, out var old) ? old! : new VersionInfo();
            var newInfo = TryParse(newVersion, out var newV) ? newV! : new VersionInfo();

            var diff = new VersionDiff
            {
                OldVersion = oldInfo,
                NewVersion = newInfo,
                MajorDiff = newInfo.Major - oldInfo.Major,
                MinorDiff = newInfo.Minor - oldInfo.Minor,
                PatchDiff = newInfo.Patch - oldInfo.Patch,
                RevisionDiff = newInfo.Revision - oldInfo.Revision
            };

            if (diff.MajorDiff != 0)
                diff.ChangeLevel = VersionLevel.Major;
            else if (diff.MinorDiff != 0)
                diff.ChangeLevel = VersionLevel.Minor;
            else if (diff.PatchDiff != 0)
                diff.ChangeLevel = VersionLevel.Patch;
            else if (diff.RevisionDiff != 0)
                diff.ChangeLevel = VersionLevel.Revision;

            return diff;
        }

        /// <summary>
        /// 从列表中找到最接近目标版本号的版本
        /// </summary>
        /// <param name="versions">版本号列表</param>
        /// <param name="target">目标版本号</param>
        /// <returns>最接近的版本号</returns>
        public static string? FindClosest(IEnumerable<string> versions, string target)
        {
            if (versions == null || string.IsNullOrEmpty(target))
                return null;

            var targetInfo = Parse(target);
            string? closest = null;
            var minDiff = int.MaxValue;

            foreach (var version in versions)
            {
                if (!TryParse(version, out var info))
                    continue;

                var diff = Math.Abs(Compare(info!, targetInfo));
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closest = version;
                }
            }

            return closest;
        }

        /// <summary>
        /// 验证版本号是否符合语义化版本规范（SemVer）
        /// </summary>
        /// <param name="version">版本号字符串</param>
        /// <returns>是否有效</returns>
        public static bool IsValidSemVer(string? version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return false;

            // SemVer 正则：主版本.次版本.补丁版本[-预发布标识][+构建元数据]
            var pattern = @"^v?(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";
            return System.Text.RegularExpressions.Regex.IsMatch(version, pattern);
        }

        /// <summary>
        /// 将版本号转换为 System.Version
        /// </summary>
        /// <param name="version">版本号字符串</param>
        /// <returns>System.Version 对象</returns>
        public static Version ToVersion(string? version)
        {
            var info = Parse(version);
            return new Version(info.Major, info.Minor, info.Patch, info.Revision);
        }

        /// <summary>
        /// 从 System.Version 转换为 VersionInfo
        /// </summary>
        /// <param name="version">System.Version 对象</param>
        /// <returns>VersionInfo 对象</returns>
        public static VersionInfo FromVersion(Version version)
        {
            return new VersionInfo
            {
                Major = version.Major,
                Minor = version.Minor,
                Patch = version.Build >= 0 ? version.Build : 0,
                Revision = version.Revision >= 0 ? version.Revision : 0
            };
        }
    }

    /// <summary>
    /// 版本信息
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// 原始版本号字符串
        /// </summary>
        public string? Original { get; set; }

        /// <summary>
        /// 主版本号
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// 次版本号
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// 补丁版本号
        /// </summary>
        public int Patch { get; set; }

        /// <summary>
        /// 修订版本号
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        /// 预发布标识（如 alpha, beta, rc.1）
        /// </summary>
        public string? PreRelease { get; set; }

        /// <summary>
        /// 构建元数据
        /// </summary>
        public string? BuildMetadata { get; set; }

        /// <summary>
        /// 是否为预发布版本
        /// </summary>
        public bool IsPreRelease => !string.IsNullOrEmpty(PreRelease);

        /// <summary>
        /// 是否为稳定版本
        /// </summary>
        public bool IsStable => string.IsNullOrEmpty(PreRelease);

        public override string ToString()
        {
            var result = $"{Major}.{Minor}.{Patch}";
            if (Revision > 0)
                result += $".{Revision}";
            if (!string.IsNullOrEmpty(PreRelease))
                result += $"-{PreRelease}";
            if (!string.IsNullOrEmpty(BuildMetadata))
                result += $"+{BuildMetadata}";
            return result;
        }

        public override bool Equals(object? obj)
        {
            if (obj is VersionInfo other)
            {
                return Major == other.Major &&
                       Minor == other.Minor &&
                       Patch == other.Patch &&
                       Revision == other.Revision &&
                       PreRelease == other.PreRelease;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch, Revision, PreRelease);
        }
    }

    /// <summary>
    /// 版本差异
    /// </summary>
    public class VersionDiff
    {
        /// <summary>
        /// 旧版本
        /// </summary>
        public VersionInfo OldVersion { get; set; } = new();

        /// <summary>
        /// 新版本
        /// </summary>
        public VersionInfo NewVersion { get; set; } = new();

        /// <summary>
        /// 主版本差异
        /// </summary>
        public int MajorDiff { get; set; }

        /// <summary>
        /// 次版本差异
        /// </summary>
        public int MinorDiff { get; set; }

        /// <summary>
        /// 补丁版本差异
        /// </summary>
        public int PatchDiff { get; set; }

        /// <summary>
        /// 修订版本差异
        /// </summary>
        public int RevisionDiff { get; set; }

        /// <summary>
        /// 变更级别
        /// </summary>
        public VersionLevel ChangeLevel { get; set; }

        /// <summary>
        /// 是否为升级
        /// </summary>
        public bool IsUpgrade =>
            MajorDiff > 0 ||
            (MajorDiff == 0 && MinorDiff > 0) ||
            (MajorDiff == 0 && MinorDiff == 0 && PatchDiff > 0) ||
            (MajorDiff == 0 && MinorDiff == 0 && PatchDiff == 0 && RevisionDiff > 0);

        /// <summary>
        /// 是否为降级
        /// </summary>
        public bool IsDowngrade =>
            MajorDiff < 0 ||
            (MajorDiff == 0 && MinorDiff < 0) ||
            (MajorDiff == 0 && MinorDiff == 0 && PatchDiff < 0) ||
            (MajorDiff == 0 && MinorDiff == 0 && PatchDiff == 0 && RevisionDiff < 0);

        /// <summary>
        /// 是否无变化
        /// </summary>
        public bool IsUnchanged => MajorDiff == 0 && MinorDiff == 0 && PatchDiff == 0 && RevisionDiff == 0;
    }

    /// <summary>
    /// 版本级别
    /// </summary>
    public enum VersionLevel
    {
        /// <summary>
        /// 主版本
        /// </summary>
        Major,

        /// <summary>
        /// 次版本
        /// </summary>
        Minor,

        /// <summary>
        /// 补丁版本
        /// </summary>
        Patch,

        /// <summary>
        /// 修订版本
        /// </summary>
        Revision
    }
}
