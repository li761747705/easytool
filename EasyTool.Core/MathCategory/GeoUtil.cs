using System;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 地理坐标工具类
    /// 提供距离计算、坐标转换等功能
    /// </summary>
    public static class GeoUtil
    {
        /// <summary>
        /// 地球半径（米）
        /// </summary>
        public const double EarthRadius = 6371000;

        /// <summary>
        /// 计算两点之间的距离（Haversine公式）
        /// </summary>
        /// <param name="lat1">纬度1</param>
        /// <param name="lon1">经度1</param>
        /// <param name="lat2">纬度2</param>
        /// <param name="lon2">经度2</param>
        /// <returns>距离（米）</returns>
        public static double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadius * c;
        }

        /// <summary>
        /// 计算两点之间的方位角（从北向顺时针）
        /// </summary>
        public static double Bearing(double lat1, double lon1, double lat2, double lon2)
        {
            var dLon = ToRadians(lon2 - lon1);
            var lat1Rad = ToRadians(lat1);
            var lat2Rad = ToRadians(lat2);

            var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
            var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                    Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);

            var bearing = Math.Atan2(y, x);
            return (ToDegrees(bearing) + 360) % 360;
        }

        /// <summary>
        /// 根据起点、方位角和距离计算终点
        /// </summary>
        public static (double Latitude, double Longitude) Destination(
            double startLat, double startLon, double bearing, double distance)
        {
            var bearingRad = ToRadians(bearing);
            var lat1 = ToRadians(startLat);
            var lon1 = ToRadians(startLon);

            var angularDistance = distance / EarthRadius;

            var lat2 = Math.Asin(
                Math.Sin(lat1) * Math.Cos(angularDistance) +
                Math.Cos(lat1) * Math.Sin(angularDistance) * Math.Cos(bearingRad));

            var lon2 = lon1 + Math.Atan2(
                Math.Sin(bearingRad) * Math.Sin(angularDistance) * Math.Cos(lat1),
                Math.Cos(angularDistance) - Math.Sin(lat1) * Math.Sin(lat2));

            return (ToDegrees(lat2), ToDegrees(lon2));
        }

        /// <summary>
        /// 计算矩形边界（用于数据库范围查询）
        /// </summary>
        public static (double MinLat, double MinLon, double MaxLat, double MaxLon) GetBoundingBox(
            double centerLat, double centerLon, double radiusInMeters)
        {
            var latChange = radiusInMeters / EarthRadius * (180 / Math.PI);
            var lonChange = radiusInMeters / (EarthRadius * Math.Cos(ToRadians(centerLat))) * (180 / Math.PI);

            return (
                centerLat - latChange,
                centerLon - lonChange,
                centerLat + latChange,
                centerLon + lonChange
            );
        }

        /// <summary>
        /// 判断点是否在矩形范围内
        /// </summary>
        public static bool IsInBoundingBox(
            double lat, double lon,
            double minLat, double minLon, double maxLat, double maxLon)
        {
            return lat >= minLat && lat <= maxLat && lon >= minLon && lon <= maxLon;
        }

        /// <summary>
        /// 判断点是否在圆形范围内
        /// </summary>
        public static bool IsInCircle(
            double lat, double lon,
            double centerLat, double centerLon, double radiusInMeters)
        {
            return Distance(lat, lon, centerLat, centerLon) <= radiusInMeters;
        }

        /// <summary>
        /// 判断点是否在多边形内
        /// </summary>
        public static bool IsInPolygon(double lat, double lon, params (double Lat, double Lon)[] polygon)
        {
            if (polygon == null || polygon.Length < 3)
                return false;

            var inside = false;
            var j = polygon.Length - 1;

            for (int i = 0; i < polygon.Length; j = i++)
            {
                var xi = polygon[i].Lon;
                var yi = polygon[i].Lat;
                var xj = polygon[j].Lon;
                var yj = polygon[j].Lat;

                var intersect = ((yi > lat) != (yj > lat)) &&
                    (lon < (xj - xi) * (lat - yi) / (yj - yi) + xi);

                if (intersect)
                    inside = !inside;
            }

            return inside;
        }

        /// <summary>
        /// 计算多边形面积（平方米）
        /// </summary>
        public static double PolygonArea(params (double Lat, double Lon)[] polygon)
        {
            if (polygon == null || polygon.Length < 3)
                return 0;

            var area = 0.0;
            var j = polygon.Length - 1;

            for (int i = 0; i < polygon.Length; j = i++)
            {
                var xi = ToRadians(polygon[i].Lon);
                var yi = ToRadians(polygon[i].Lat);
                var xj = ToRadians(polygon[j].Lon);
                var yj = ToRadians(polygon[j].Lat);

                area += (xj - xi) * (2 + Math.Sin(yi) + Math.Sin(yj));
            }

            return Math.Abs(area * EarthRadius * EarthRadius / 2);
        }

        #region 坐标转换

        /// <summary>
        /// WGS84转GCJ02（火星坐标）
        /// </summary>
        public static (double Lat, double Lon) Wgs84ToGcj02(double wgsLat, double wgsLon)
        {
            var dLat = TransformLat(wgsLon - 105.0, wgsLat - 35.0);
            var dLon = TransformLon(wgsLon - 105.0, wgsLat - 35.0);

            var radLat = wgsLat / 180.0 * Math.PI;
            var magic = Math.Sin(radLat);
            magic = 1 - 0.00669342162296594323 * magic * magic;
            var sqrtMagic = Math.Sqrt(magic);

            dLat = (dLat * 180.0) / ((EarthRadius / 1000) * (1 - 0.00669342162296594323) * sqrtMagic * Math.PI);
            dLon = (dLon * 180.0) / ((EarthRadius / 1000) * sqrtMagic * Math.Cos(radLat) * Math.PI);

            return (wgsLat + dLat, wgsLon + dLon);
        }

        /// <summary>
        /// GCJ02转WGS84
        /// </summary>
        public static (double Lat, double Lon) Gcj02ToWgs84(double gcjLat, double gcjLon)
        {
            var dLat = TransformLat(gcjLon - 105.0, gcjLat - 35.0);
            var dLon = TransformLon(gcjLon - 105.0, gcjLat - 35.0);

            var radLat = gcjLat / 180.0 * Math.PI;
            var magic = Math.Sin(radLat);
            magic = 1 - 0.00669342162296594323 * magic * magic;
            var sqrtMagic = Math.Sqrt(magic);

            dLat = (dLat * 180.0) / ((EarthRadius / 1000) * (1 - 0.00669342162296594323) * sqrtMagic * Math.PI);
            dLon = (dLon * 180.0) / ((EarthRadius / 1000) * sqrtMagic * Math.Cos(radLat) * Math.PI);

            return (gcjLat - dLat, gcjLon - dLon);
        }

        /// <summary>
        /// BD09转GCJ02
        /// </summary>
        public static (double Lat, double Lon) Bd09ToGcj02(double bdLat, double bdLon)
        {
            var x = bdLon - 0.0065;
            var y = bdLat - 0.006;
            var z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * Math.PI * 3000.0 / 180.0);
            var theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * Math.PI * 3000.0 / 180.0);

            return (z * Math.Sin(theta), z * Math.Cos(theta));
        }

        /// <summary>
        /// GCJ02转BD09
        /// </summary>
        public static (double Lat, double Lon) Gcj02ToBd09(double gcjLat, double gcjLon)
        {
            var z = Math.Sqrt(gcjLon * gcjLon + gcjLat * gcjLat) + 0.00002 * Math.Sin(gcjLat * Math.PI * 3000.0 / 180.0);
            var theta = Math.Atan2(gcjLat, gcjLon) + 0.000003 * Math.Cos(gcjLon * Math.PI * 3000.0 / 180.0);

            return (z * Math.Sin(theta) + 0.006, z * Math.Cos(theta) + 0.0065);
        }

        /// <summary>
        /// BD09转WGS84
        /// </summary>
        public static (double Lat, double Lon) Bd09ToWgs84(double bdLat, double bdLon)
        {
            var gcj = Bd09ToGcj02(bdLat, bdLon);
            return Gcj02ToWgs84(gcj.Lat, gcj.Lon);
        }

        /// <summary>
        /// WGS84转BD09
        /// </summary>
        public static (double Lat, double Lon) Wgs84ToBd09(double wgsLat, double wgsLon)
        {
            var gcj = Wgs84ToGcj02(wgsLat, wgsLon);
            return Gcj02ToBd09(gcj.Lat, gcj.Lon);
        }

        private static double TransformLat(double x, double y)
        {
            var ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * Math.PI) + 20.0 * Math.Sin(2.0 * x * Math.PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * Math.PI) + 40.0 * Math.Sin(y / 3.0 * Math.PI)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * Math.PI) + 320 * Math.Sin(y * Math.PI / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        private static double TransformLon(double x, double y)
        {
            var ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * Math.PI) + 20.0 * Math.Sin(2.0 * x * Math.PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * Math.PI) + 40.0 * Math.Sin(x / 3.0 * Math.PI)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * Math.PI) + 300.0 * Math.Sin(x / 30.0 * Math.PI)) * 2.0 / 3.0;
            return ret;
        }

        #endregion

        #region 辅助方法

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private static double ToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        #endregion
    }
}
