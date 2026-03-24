using System;

namespace EasyTool.ConvertCategory
{
    /// <summary>
    /// 坐标转换工具类
    /// 提供常见坐标系统之间的转换（WGS84、GCJ02、BD09）
    /// </summary>
    public static class CoordinateConvertUtil
    {
        // 坐标系常量
        private const double Pi = 3.1415926535897932384626;
        private const double A = 6378245.0;          // 长半轴
        private const double EE = 0.00669342162296594323; // 扁率

        /// <summary>
        /// 坐标系类型
        /// </summary>
        public enum CoordinateSystem
        {
            /// <summary>WGS84（GPS原始坐标）</summary>
            WGS84,
            /// <summary>GCJ02（国测局坐标/火星坐标）</summary>
            GCJ02,
            /// <summary>BD09（百度坐标）</summary>
            BD09
        }

        /// <summary>
        /// 经纬度坐标
        /// </summary>
        public struct GeoPoint
        {
            /// <summary>经度</summary>
            public double Longitude { get; set; }
            /// <summary>纬度</summary>
            public double Latitude { get; set; }
            /// <summary>坐标系</summary>
            public CoordinateSystem CoordinateSystem { get; set; }

            public GeoPoint(double longitude, double latitude, CoordinateSystem coordinateSystem = CoordinateSystem.WGS84)
            {
                Longitude = longitude;
                Latitude = latitude;
                CoordinateSystem = coordinateSystem;
            }

            public override string ToString() => $"({Longitude:F6}, {Latitude:F6})";
        }

        /// <summary>
        /// WGS84 转 GCJ02
        /// </summary>
        public static GeoPoint WGS84ToGCJ02(double longitude, double latitude)
        {
            if (OutOfChina(longitude, latitude))
            {
                return new GeoPoint(longitude, latitude, CoordinateSystem.GCJ02);
            }

            double dLat = TransformLat(longitude - 105.0, latitude - 35.0);
            double dLon = TransformLon(longitude - 105.0, latitude - 35.0);

            double radLat = latitude / 180.0 * Pi;
            double magic = Math.Sin(radLat);
            magic = 1 - EE * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);

            dLat = (dLat * 180.0) / ((A * (1 - EE)) / (magic * sqrtMagic) * Pi);
            dLon = (dLon * 180.0) / (A / sqrtMagic * Math.Cos(radLat) * Pi);

            double mgLat = latitude + dLat;
            double mgLon = longitude + dLon;

            return new GeoPoint(mgLon, mgLat, CoordinateSystem.GCJ02);
        }

        /// <summary>
        /// GCJ02 转 WGS84
        /// </summary>
        public static GeoPoint GCJ02ToWGS84(double longitude, double latitude)
        {
            if (OutOfChina(longitude, latitude))
            {
                return new GeoPoint(longitude, latitude, CoordinateSystem.WGS84);
            }

            double dLat = TransformLat(longitude - 105.0, latitude - 35.0);
            double dLon = TransformLon(longitude - 105.0, latitude - 35.0);

            double radLat = latitude / 180.0 * Pi;
            double magic = Math.Sin(radLat);
            magic = 1 - EE * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);

            dLat = (dLat * 180.0) / ((A * (1 - EE)) / (magic * sqrtMagic) * Pi);
            dLon = (dLon * 180.0) / (A / sqrtMagic * Math.Cos(radLat) * Pi);

            double mgLat = latitude + dLat;
            double mgLon = longitude + dLon;

            return new GeoPoint(longitude * 2 - mgLon, latitude * 2 - mgLat, CoordinateSystem.WGS84);
        }

        /// <summary>
        /// GCJ02 转 BD09
        /// </summary>
        public static GeoPoint GCJ02ToBD09(double longitude, double latitude)
        {
            double x = longitude;
            double y = latitude;

            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * Pi * 3000.0 / 180.0);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * Pi * 3000.0 / 180.0);

            double bdLon = z * Math.Cos(theta) + 0.0065;
            double bdLat = z * Math.Sin(theta) + 0.006;

            return new GeoPoint(bdLon, bdLat, CoordinateSystem.BD09);
        }

        /// <summary>
        /// BD09 转 GCJ02
        /// </summary>
        public static GeoPoint BD09ToGCJ02(double longitude, double latitude)
        {
            double x = longitude - 0.0065;
            double y = latitude - 0.006;

            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * Pi * 3000.0 / 180.0);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * Pi * 3000.0 / 180.0);

            double gcjLon = z * Math.Cos(theta);
            double gcjLat = z * Math.Sin(theta);

            return new GeoPoint(gcjLon, gcjLat, CoordinateSystem.GCJ02);
        }

        /// <summary>
        /// BD09 转 WGS84
        /// </summary>
        public static GeoPoint BD09ToWGS84(double longitude, double latitude)
        {
            var gcj02 = BD09ToGCJ02(longitude, latitude);
            return GCJ02ToWGS84(gcj02.Longitude, gcj02.Latitude);
        }

        /// <summary>
        /// WGS84 转 BD09
        /// </summary>
        public static GeoPoint WGS84ToBD09(double longitude, double latitude)
        {
            var gcj02 = WGS84ToGCJ02(longitude, latitude);
            return GCJ02ToBD09(gcj02.Longitude, gcj02.Latitude);
        }

        /// <summary>
        /// 通用坐标转换
        /// </summary>
        public static GeoPoint Convert(double longitude, double latitude, CoordinateSystem from, CoordinateSystem to)
        {
            if (from == to)
                return new GeoPoint(longitude, latitude, to);

            return (from, to) switch
            {
                (CoordinateSystem.WGS84, CoordinateSystem.GCJ02) => WGS84ToGCJ02(longitude, latitude),
                (CoordinateSystem.WGS84, CoordinateSystem.BD09) => WGS84ToBD09(longitude, latitude),
                (CoordinateSystem.GCJ02, CoordinateSystem.WGS84) => GCJ02ToWGS84(longitude, latitude),
                (CoordinateSystem.GCJ02, CoordinateSystem.BD09) => GCJ02ToBD09(longitude, latitude),
                (CoordinateSystem.BD09, CoordinateSystem.WGS84) => BD09ToWGS84(longitude, latitude),
                (CoordinateSystem.BD09, CoordinateSystem.GCJ02) => BD09ToGCJ02(longitude, latitude),
                _ => new GeoPoint(longitude, latitude, to)
            };
        }

        /// <summary>
        /// 计算两点之间的距离（米）
        /// 使用 Haversine 公式
        /// </summary>
        public static double Distance(double lon1, double lat1, double lon2, double lat2)
        {
            const double R = 6371000; // 地球半径（米）

            double dLat = (lat2 - lat1) * Pi / 180;
            double dLon = (lon2 - lon1) * Pi / 180;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Cos(lat1 * Pi / 180) * Math.Cos(lat2 * Pi / 180) *
                      Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        /// <summary>
        /// 计算两点之间的距离
        /// </summary>
        public static double Distance(GeoPoint p1, GeoPoint p2)
        {
            return Distance(p1.Longitude, p1.Latitude, p2.Longitude, p2.Latitude);
        }

        /// <summary>
        /// 计算方位角（从北向顺时针）
        /// </summary>
        public static double Bearing(double lon1, double lat1, double lon2, double lat2)
        {
            double dLon = (lon2 - lon1) * Pi / 180;

            double y = Math.Sin(dLon) * Math.Cos(lat2 * Pi / 180);
            double x = Math.Cos(lat1 * Pi / 180) * Math.Sin(lat2 * Pi / 180) -
                      Math.Sin(lat1 * Pi / 180) * Math.Cos(lat2 * Pi / 180) * Math.Cos(dLon);

            double bearing = Math.Atan2(y, x) * 180 / Pi;
            return (bearing + 360) % 360;
        }

        /// <summary>
        /// 根据起点、方位角和距离计算终点
        /// </summary>
        public static GeoPoint Destination(double lon1, double lat1, double bearing, double distance)
        {
            const double R = 6371000;

            double brng = bearing * Pi / 180;
            double d = distance / R;

            double lat1Rad = lat1 * Pi / 180;
            double lon1Rad = lon1 * Pi / 180;

            double lat2Rad = Math.Asin(Math.Sin(lat1Rad) * Math.Cos(d) +
                          Math.Cos(lat1Rad) * Math.Sin(d) * Math.Cos(brng));

            double lon2Rad = lon1Rad + Math.Atan2(
                Math.Sin(brng) * Math.Sin(d) * Math.Cos(lat1Rad),
                Math.Cos(d) - Math.Sin(lat1Rad) * Math.Sin(lat2Rad));

            return new GeoPoint(lon2Rad * 180 / Pi, lat2Rad * 180 / Pi);
        }

        /// <summary>
        /// 判断是否在中国境内
        /// </summary>
        public static bool OutOfChina(double longitude, double latitude)
        {
            if (longitude < 72.004 || longitude > 137.8347)
                return true;
            if (latitude < 0.8293 || latitude > 55.8271)
                return true;

            return false;
        }

        private static double TransformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * Pi) + 20.0 * Math.Sin(2.0 * x * Pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * Pi) + 40.0 * Math.Sin(y / 3.0 * Pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * Pi) + 320 * Math.Sin(y * Pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        private static double TransformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * Pi) + 20.0 * Math.Sin(2.0 * x * Pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * Pi) + 40.0 * Math.Sin(x / 3.0 * Pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * Pi) + 300.0 * Math.Sin(x / 30.0 * Pi)) * 2.0 / 3.0;
            return ret;
        }
    }
}
