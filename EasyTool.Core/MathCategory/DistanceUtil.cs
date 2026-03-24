using System;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 距离计算工具类
    /// 提供基于经纬度的距离计算、地理编码等功能
    /// </summary>
    public static class DistanceUtil
    {
        /// <summary>
        /// 地球半径（千米）
        /// </summary>
        public const double EarthRadiusKm = 6371.0;

        /// <summary>
        /// 地球半径（米）
        /// </summary>
        public const double EarthRadiusM = 6371000.0;

        /// <summary>
        /// 地球半径（英里）
        /// </summary>
        public const double EarthRadiusMile = 3958.8;

        #region Haversine 距离计算

        /// <summary>
        /// 使用 Haversine 公式计算两个坐标之间的球面距离
        /// </summary>
        /// <param name="lat1">起点纬度</param>
        /// <param name="lon1">起点经度</param>
        /// <param name="lat2">终点纬度</param>
        /// <param name="lon2">终点经度</param>
        /// <returns>距离（千米）</returns>
        public static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        /// <summary>
        /// 计算两个坐标之间的距离（米）
        /// </summary>
        public static double DistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            return Haversine(lat1, lon1, lat2, lon2) * 1000;
        }

        /// <summary>
        /// 计算两个坐标之间的距离（英里）
        /// </summary>
        public static double DistanceInMiles(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusMile * c;
        }

        #endregion

        #region 方位角计算

        /// <summary>
        /// 计算从起点到终点的方位角（初始方位角）
        /// </summary>
        /// <param name="lat1">起点纬度</param>
        /// <param name="lon1">起点经度</param>
        /// <param name="lat2">终点纬度</param>
        /// <param name="lon2">终点经度</param>
        /// <returns>方位角（度数，0-360，正北为0）</returns>
        public static double Bearing(double lat1, double lon1, double lat2, double lon2)
        {
            var dLon = ToRadians(lon2 - lon1);
            var lat1Rad = ToRadians(lat1);
            var lat2Rad = ToRadians(lat2);

            var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
            var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                    Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);

            var bearing = Math.Atan2(y, x);
            bearing = ToDegrees(bearing);
            bearing = (bearing + 360) % 360;

            return bearing;
        }

        /// <summary>
        /// 根据方位角获取方向描述
        /// </summary>
        /// <param name="bearing">方位角</param>
        /// <returns>方向描述</returns>
        public static string GetDirectionName(double bearing)
        {
            bearing = ((bearing % 360) + 360) % 360;

            return bearing switch
            {
                >= 337.5 or < 22.5 => "正北",
                >= 22.5 and < 67.5 => "东北",
                >= 67.5 and < 112.5 => "正东",
                >= 112.5 and < 157.5 => "东南",
                >= 157.5 and < 202.5 => "正南",
                >= 202.5 and < 247.5 => "西南",
                >= 247.5 and < 292.5 => "正西",
                >= 292.5 and < 337.5 => "西北",
                _ => "未知"
            };
        }

        #endregion

        #region 目标点计算

        /// <summary>
        /// 根据起点、方位角和距离计算终点坐标
        /// </summary>
        /// <param name="lat">起点纬度</param>
        /// <param name="lon">起点经度</param>
        /// <param name="bearing">方位角（度）</param>
        /// <param name="distanceKm">距离（千米）</param>
        /// <returns>终点坐标（纬度，经度）</returns>
        public static (double Latitude, double Longitude) DestinationPoint(
            double lat, double lon, double bearing, double distanceKm)
        {
            var bearingRad = ToRadians(bearing);
            var lat1 = ToRadians(lat);
            var lon1 = ToRadians(lon);
            var d = distanceKm / EarthRadiusKm;

            var lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d) +
                                  Math.Cos(lat1) * Math.Sin(d) * Math.Cos(bearingRad));

            var lon2 = lon1 + Math.Atan2(
                Math.Sin(bearingRad) * Math.Sin(d) * Math.Cos(lat1),
                Math.Cos(d) - Math.Sin(lat1) * Math.Sin(lat2));

            return (ToDegrees(lat2), ToDegrees(lon2));
        }

        /// <summary>
        /// 计算指定距离处的边界框（用于数据库查询）
        /// </summary>
        /// <param name="lat">中心点纬度</param>
        /// <param name="lon">中心点经度</param>
        /// <param name="distanceKm">距离（千米）</param>
        /// <returns>边界框（最小纬度，最小经度，最大纬度，最大经度）</returns>
        public static (double MinLat, double MinLon, double MaxLat, double MaxLon) BoundingBox(
            double lat, double lon, double distanceKm)
        {
            var latRad = ToRadians(lat);
            var d = distanceKm / EarthRadiusKm;

            // 纬度变化
            var dLat = d;
            var dLon = Math.Asin(Math.Sin(d) / Math.Cos(latRad));

            var minLat = lat - ToDegrees(dLat);
            var maxLat = lat + ToDegrees(dLat);
            var minLon = lon - ToDegrees(dLon);
            var maxLon = lon + ToDegrees(dLon);

            return (minLat, minLon, maxLat, maxLon);
        }

        #endregion

        #region 中点计算

        /// <summary>
        /// 计算两个坐标之间的中点
        /// </summary>
        public static (double Latitude, double Longitude) Midpoint(
            double lat1, double lon1, double lat2, double lon2)
        {
            var lat1Rad = ToRadians(lat1);
            var lat2Rad = ToRadians(lat2);
            var lon1Rad = ToRadians(lon1);
            var dLon = ToRadians(lon2 - lon1);

            var bx = Math.Cos(lat2Rad) * Math.Cos(dLon);
            var by = Math.Cos(lat2Rad) * Math.Sin(dLon);

            var lat3 = Math.Atan2(
                Math.Sin(lat1Rad) + Math.Sin(lat2Rad),
                Math.Sqrt((Math.Cos(lat1Rad) + bx) * (Math.Cos(lat1Rad) + bx) + by * by));

            var lon3 = lon1Rad + Math.Atan2(by, Math.Cos(lat1Rad) + bx);

            return (ToDegrees(lat3), ToDegrees(lon3));
        }

        #endregion

        #region 直线距离估算

        /// <summary>
        /// 使用勾股定理近似计算短距离（适用于小范围）
        /// </summary>
        /// <param name="lat1">起点纬度</param>
        /// <param name="lon1">起点经度</param>
        /// <param name="lat2">终点纬度</param>
        /// <param name="lon2">终点经度</param>
        /// <returns>距离（米）</returns>
        public static double EuclideanDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var avgLat = ToRadians((lat1 + lat2) / 2);
            var latDist = ToRadians(lat2 - lat1) * EarthRadiusM;
            var lonDist = ToRadians(lon2 - lon1) * EarthRadiusM * Math.Cos(avgLat);

            return Math.Sqrt(latDist * latDist + lonDist * lonDist);
        }

        #endregion

        #region 驾驶距离估算

        /// <summary>
        /// 估算驾驶距离（直线距离乘以系数）
        /// </summary>
        /// <param name="lat1">起点纬度</param>
        /// <param name="lon1">起点经度</param>
        /// <param name="lat2">终点纬度</param>
        /// <param name="lon2">终点经度</param>
        /// <param name="factor">系数（默认1.4，城市间约为1.2-1.3，城市内约为1.4-1.6）</param>
        /// <returns>估算驾驶距离（千米）</returns>
        public static double EstimatedDrivingDistance(
            double lat1, double lon1, double lat2, double lon2, double factor = 1.4)
        {
            return Haversine(lat1, lon1, lat2, lon2) * factor;
        }

        #endregion

        #region 坐标转换

        /// <summary>
        /// 度转弧度
        /// </summary>
        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// 弧度转度
        /// </summary>
        public static double ToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        /// <summary>
        /// 度分秒转十进制度
        /// </summary>
        /// <param name="degrees">度</param>
        /// <param name="minutes">分</param>
        /// <param name="seconds">秒</param>
        /// <returns>十进制度</returns>
        public static double DmsToDecimal(int degrees, int minutes, double seconds)
        {
            return degrees + minutes / 60.0 + seconds / 3600.0;
        }

        /// <summary>
        /// 十进制度转度分秒
        /// </summary>
        /// <param name="decimalDegrees">十进制度</param>
        /// <returns>度分秒元组</returns>
        public static (int Degrees, int Minutes, double Seconds) DecimalToDms(double decimalDegrees)
        {
            var degrees = (int)decimalDegrees;
            var remainder = (decimalDegrees - degrees) * 60;
            var minutes = (int)remainder;
            var seconds = (remainder - minutes) * 60;

            return (degrees, minutes, seconds);
        }

        #endregion

        #region 坐标验证

        /// <summary>
        /// 验证经度是否有效
        /// </summary>
        public static bool IsValidLongitude(double longitude)
        {
            return longitude >= -180 && longitude <= 180;
        }

        /// <summary>
        /// 验证纬度是否有效
        /// </summary>
        public static bool IsValidLatitude(double latitude)
        {
            return latitude >= -90 && latitude <= 90;
        }

        /// <summary>
        /// 验证坐标是否有效
        /// </summary>
        public static bool IsValidCoordinate(double latitude, double longitude)
        {
            return IsValidLatitude(latitude) && IsValidLongitude(longitude);
        }

        /// <summary>
        /// 标准化经度到 -180 到 180 范围
        /// </summary>
        public static double NormalizeLongitude(double longitude)
        {
            while (longitude > 180) longitude -= 360;
            while (longitude < -180) longitude += 360;
            return longitude;
        }

        #endregion

        #region 格式化

        /// <summary>
        /// 格式化坐标为字符串
        /// </summary>
        /// <param name="latitude">纬度</param>
        /// <param name="longitude">经度</param>
        /// <param name="decimalPlaces">小数位数</param>
        /// <returns>格式化后的字符串</returns>
        public static string Format(double latitude, double longitude, int decimalPlaces = 6)
        {
            var latDir = latitude >= 0 ? "N" : "S";
            var lonDir = longitude >= 0 ? "E" : "W";

            return $"{Math.Abs(latitude).ToString("F" + decimalPlaces)}°{latDir}, {Math.Abs(longitude).ToString("F" + decimalPlaces)}°{lonDir}";
        }

        /// <summary>
        /// 格式化为度分秒
        /// </summary>
        public static string FormatDms(double latitude, double longitude)
        {
            var (latDeg, latMin, latSec) = DecimalToDms(Math.Abs(latitude));
            var (lonDeg, lonMin, lonSec) = DecimalToDms(Math.Abs(longitude));

            var latDir = latitude >= 0 ? "N" : "S";
            var lonDir = longitude >= 0 ? "E" : "W";

            return $"{latDeg}°{latMin}'{latSec:F2}\"{latDir}, {lonDeg}°{lonMin}'{lonSec:F2}\"{lonDir}";
        }

        #endregion
    }
}
