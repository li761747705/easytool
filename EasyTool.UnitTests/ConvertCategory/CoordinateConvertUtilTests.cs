using Xunit;

namespace EasyTool.ConvertCategory.Tests
{
    public class CoordinateConvertUtilTests
    {
        // Beijing coordinates in WGS84 (approximate)
        private const double BeijingLon = 116.404;
        private const double BeijingLat = 39.915;

        #region WGS84 <-> GCJ02

        [Fact]
        public void WGS84ToGCJ02_ReturnsGCJ02CoordinateSystem()
        {
            var result = CoordinateConvertUtil.WGS84ToGCJ02(BeijingLon, BeijingLat);

            Assert.Equal(CoordinateConvertUtil.CoordinateSystem.GCJ02, result.CoordinateSystem);
        }

        [Fact]
        public void WGS84ToGCJ02_OutsideChina_ReturnsUnchanged()
        {
            // New York (outside China)
            double lon = -74.006;
            double lat = 40.7128;

            var result = CoordinateConvertUtil.WGS84ToGCJ02(lon, lat);

            Assert.Equal(lon, result.Longitude);
            Assert.Equal(lat, result.Latitude);
        }

        [Fact]
        public void WGS84ToGCJ02_InsideChina_OffsetsApplied()
        {
            var result = CoordinateConvertUtil.WGS84ToGCJ02(BeijingLon, BeijingLat);

            // GCJ02 should differ from WGS84 within China
            Assert.NotEqual(BeijingLon, result.Longitude);
            Assert.NotEqual(BeijingLat, result.Latitude);
        }

        [Fact]
        public void GCJ02ToWGS84_ReturnsWGS84CoordinateSystem()
        {
            var gcj = CoordinateConvertUtil.WGS84ToGCJ02(BeijingLon, BeijingLat);
            var result = CoordinateConvertUtil.GCJ02ToWGS84(gcj.Longitude, gcj.Latitude);

            Assert.Equal(CoordinateConvertUtil.CoordinateSystem.WGS84, result.CoordinateSystem);
        }

        [Fact]
        public void WGS84ToGCJ02_GCJ02ToWGS84_RoundTrip()
        {
            var gcj = CoordinateConvertUtil.WGS84ToGCJ02(BeijingLon, BeijingLat);
            var wgs84 = CoordinateConvertUtil.GCJ02ToWGS84(gcj.Longitude, gcj.Latitude);

            // Round-trip should be close to original (within ~1 meter)
            Assert.InRange(wgs84.Longitude, BeijingLon - 0.00001, BeijingLon + 0.00001);
            Assert.InRange(wgs84.Latitude, BeijingLat - 0.00001, BeijingLat + 0.00001);
        }

        [Fact]
        public void GCJ02ToWGS84_OutsideChina_ReturnsUnchanged()
        {
            double lon = -74.006;
            double lat = 40.7128;

            var result = CoordinateConvertUtil.GCJ02ToWGS84(lon, lat);

            Assert.Equal(lon, result.Longitude);
            Assert.Equal(lat, result.Latitude);
        }

        #endregion

        #region GCJ02 <-> BD09

        [Fact]
        public void GCJ02ToBD09_ReturnsBD09CoordinateSystem()
        {
            var result = CoordinateConvertUtil.GCJ02ToBD09(BeijingLon, BeijingLat);

            Assert.Equal(CoordinateConvertUtil.CoordinateSystem.BD09, result.CoordinateSystem);
        }

        [Fact]
        public void GCJ02ToBD09_OffsetsApplied()
        {
            var result = CoordinateConvertUtil.GCJ02ToBD09(BeijingLon, BeijingLat);

            Assert.NotEqual(BeijingLon, result.Longitude);
            Assert.NotEqual(BeijingLat, result.Latitude);
        }

        [Fact]
        public void BD09ToGCJ02_ReturnsGCJ02CoordinateSystem()
        {
            var bd = CoordinateConvertUtil.GCJ02ToBD09(BeijingLon, BeijingLat);
            var result = CoordinateConvertUtil.BD09ToGCJ02(bd.Longitude, bd.Latitude);

            Assert.Equal(CoordinateConvertUtil.CoordinateSystem.GCJ02, result.CoordinateSystem);
        }

        [Fact]
        public void GCJ02ToBD09_BD09ToGCJ02_RoundTrip()
        {
            var bd = CoordinateConvertUtil.GCJ02ToBD09(BeijingLon, BeijingLat);
            var gcj = CoordinateConvertUtil.BD09ToGCJ02(bd.Longitude, bd.Latitude);

            Assert.InRange(gcj.Longitude, BeijingLon - 0.00001, BeijingLon + 0.00001);
            Assert.InRange(gcj.Latitude, BeijingLat - 0.00001, BeijingLat + 0.00001);
        }

        #endregion

        #region WGS84 <-> BD09

        [Fact]
        public void WGS84ToBD09_ReturnsBD09CoordinateSystem()
        {
            var result = CoordinateConvertUtil.WGS84ToBD09(BeijingLon, BeijingLat);

            Assert.Equal(CoordinateConvertUtil.CoordinateSystem.BD09, result.CoordinateSystem);
        }

        [Fact]
        public void BD09ToWGS84_ReturnsWGS84CoordinateSystem()
        {
            var bd = CoordinateConvertUtil.WGS84ToBD09(BeijingLon, BeijingLat);
            var result = CoordinateConvertUtil.BD09ToWGS84(bd.Longitude, bd.Latitude);

            Assert.Equal(CoordinateConvertUtil.CoordinateSystem.WGS84, result.CoordinateSystem);
        }

        [Fact]
        public void WGS84ToBD09_BD09ToWGS84_RoundTrip()
        {
            var bd = CoordinateConvertUtil.WGS84ToBD09(BeijingLon, BeijingLat);
            var wgs84 = CoordinateConvertUtil.BD09ToWGS84(bd.Longitude, bd.Latitude);

            Assert.InRange(wgs84.Longitude, BeijingLon - 0.00001, BeijingLon + 0.00001);
            Assert.InRange(wgs84.Latitude, BeijingLat - 0.00001, BeijingLat + 0.00001);
        }

        #endregion

        #region Convert (generic)

        [Fact]
        public void Convert_SameFromAndTo_ReturnsUnchanged()
        {
            var result = CoordinateConvertUtil.Convert(
                BeijingLon, BeijingLat,
                CoordinateConvertUtil.CoordinateSystem.WGS84,
                CoordinateConvertUtil.CoordinateSystem.WGS84);

            Assert.Equal(BeijingLon, result.Longitude);
            Assert.Equal(BeijingLat, result.Latitude);
            Assert.Equal(CoordinateConvertUtil.CoordinateSystem.WGS84, result.CoordinateSystem);
        }

        [Fact]
        public void Convert_WGS84ToGCJ02_MatchesDirectCall()
        {
            var direct = CoordinateConvertUtil.WGS84ToGCJ02(BeijingLon, BeijingLat);
            var convert = CoordinateConvertUtil.Convert(
                BeijingLon, BeijingLat,
                CoordinateConvertUtil.CoordinateSystem.WGS84,
                CoordinateConvertUtil.CoordinateSystem.GCJ02);

            Assert.Equal(direct.Longitude, convert.Longitude);
            Assert.Equal(direct.Latitude, convert.Latitude);
        }

        [Fact]
        public void Convert_WGS84ToBD09_MatchesDirectCall()
        {
            var direct = CoordinateConvertUtil.WGS84ToBD09(BeijingLon, BeijingLat);
            var convert = CoordinateConvertUtil.Convert(
                BeijingLon, BeijingLat,
                CoordinateConvertUtil.CoordinateSystem.WGS84,
                CoordinateConvertUtil.CoordinateSystem.BD09);

            Assert.Equal(direct.Longitude, convert.Longitude);
            Assert.Equal(direct.Latitude, convert.Latitude);
        }

        [Fact]
        public void Convert_GCJ02ToWGS84_MatchesDirectCall()
        {
            var direct = CoordinateConvertUtil.GCJ02ToWGS84(BeijingLon, BeijingLat);
            var convert = CoordinateConvertUtil.Convert(
                BeijingLon, BeijingLat,
                CoordinateConvertUtil.CoordinateSystem.GCJ02,
                CoordinateConvertUtil.CoordinateSystem.WGS84);

            Assert.Equal(direct.Longitude, convert.Longitude);
            Assert.Equal(direct.Latitude, convert.Latitude);
        }

        [Fact]
        public void Convert_GCJ02ToBD09_MatchesDirectCall()
        {
            var direct = CoordinateConvertUtil.GCJ02ToBD09(BeijingLon, BeijingLat);
            var convert = CoordinateConvertUtil.Convert(
                BeijingLon, BeijingLat,
                CoordinateConvertUtil.CoordinateSystem.GCJ02,
                CoordinateConvertUtil.CoordinateSystem.BD09);

            Assert.Equal(direct.Longitude, convert.Longitude);
            Assert.Equal(direct.Latitude, convert.Latitude);
        }

        [Fact]
        public void Convert_BD09ToWGS84_MatchesDirectCall()
        {
            var direct = CoordinateConvertUtil.BD09ToWGS84(BeijingLon, BeijingLat);
            var convert = CoordinateConvertUtil.Convert(
                BeijingLon, BeijingLat,
                CoordinateConvertUtil.CoordinateSystem.BD09,
                CoordinateConvertUtil.CoordinateSystem.WGS84);

            Assert.Equal(direct.Longitude, convert.Longitude);
            Assert.Equal(direct.Latitude, convert.Latitude);
        }

        [Fact]
        public void Convert_BD09ToGCJ02_MatchesDirectCall()
        {
            var direct = CoordinateConvertUtil.BD09ToGCJ02(BeijingLon, BeijingLat);
            var convert = CoordinateConvertUtil.Convert(
                BeijingLon, BeijingLat,
                CoordinateConvertUtil.CoordinateSystem.BD09,
                CoordinateConvertUtil.CoordinateSystem.GCJ02);

            Assert.Equal(direct.Longitude, convert.Longitude);
            Assert.Equal(direct.Latitude, convert.Latitude);
        }

        #endregion

        #region GeoPoint

        [Fact]
        public void GeoPoint_DefaultConstructor_SetsWGS84()
        {
            var point = new CoordinateConvertUtil.GeoPoint(116.0, 39.0);

            Assert.Equal(116.0, point.Longitude);
            Assert.Equal(39.0, point.Latitude);
            Assert.Equal(CoordinateConvertUtil.CoordinateSystem.WGS84, point.CoordinateSystem);
        }

        [Fact]
        public void GeoPoint_WithCoordinateSystem_SetsCorrectly()
        {
            var point = new CoordinateConvertUtil.GeoPoint(116.0, 39.0, CoordinateConvertUtil.CoordinateSystem.BD09);

            Assert.Equal(CoordinateConvertUtil.CoordinateSystem.BD09, point.CoordinateSystem);
        }

        [Fact]
        public void GeoPoint_ToString_FormatsCorrectly()
        {
            var point = new CoordinateConvertUtil.GeoPoint(116.123456, 39.654321);

            string result = point.ToString();

            Assert.Contains("116.123456", result);
            Assert.Contains("39.654321", result);
        }

        #endregion

        #region Distance

        [Fact]
        public void Distance_SamePoint_ReturnsZero()
        {
            double distance = CoordinateConvertUtil.Distance(BeijingLon, BeijingLat, BeijingLon, BeijingLat);

            Assert.Equal(0, distance, 5);
        }

        [Fact]
        public void Distance_KnownDistance_BeijingToShanghai()
        {
            // Beijing to Shanghai is approximately 1068 km
            double distance = CoordinateConvertUtil.Distance(116.404, 39.915, 121.474, 31.230);

            // Within 5% tolerance
            Assert.InRange(distance, 1010000, 1130000);
        }

        [Fact]
        public void Distance_GeoPointOverload_MatchesScalarOverload()
        {
            var p1 = new CoordinateConvertUtil.GeoPoint(116.404, 39.915);
            var p2 = new CoordinateConvertUtil.GeoPoint(121.474, 31.230);

            double scalarDist = CoordinateConvertUtil.Distance(p1.Longitude, p1.Latitude, p2.Longitude, p2.Latitude);
            double pointDist = CoordinateConvertUtil.Distance(p1, p2);

            Assert.Equal(scalarDist, pointDist, 5);
        }

        #endregion

        #region Bearing

        [Fact]
        public void Bearing_North_ReturnsZero()
        {
            // From origin heading due north
            double bearing = CoordinateConvertUtil.Bearing(0, 0, 0, 1);

            Assert.Equal(0, bearing, 1);
        }

        [Fact]
        public void Bearing_East_ReturnsNinety()
        {
            // Heading due east
            double bearing = CoordinateConvertUtil.Bearing(0, 0, 1, 0);

            Assert.Equal(90, bearing, 1);
        }

        [Fact]
        public void Bearing_SamePoint_ReturnsZero()
        {
            double bearing = CoordinateConvertUtil.Bearing(BeijingLon, BeijingLat, BeijingLon, BeijingLat);

            Assert.Equal(0, bearing, 1);
        }

        [Fact]
        public void Bearing_West_ReturnsTwoSeventy()
        {
            // Heading due west
            double bearing = CoordinateConvertUtil.Bearing(0, 0, -1, 0);

            Assert.Equal(270, bearing, 1);
        }

        [Fact]
        public void Bearing_South_ReturnsOneEighty()
        {
            // Heading due south
            double bearing = CoordinateConvertUtil.Bearing(0, 0, 0, -1);

            Assert.Equal(180, bearing, 1);
        }

        [Fact]
        public void Bearing_AlwaysReturnsInRange()
        {
            // Test a few random-ish points
            double bearing = CoordinateConvertUtil.Bearing(10, 20, 30, 40);

            Assert.InRange(bearing, 0, 360);
        }

        #endregion

        #region Destination

        [Fact]
        public void Destination_ZeroDistance_ReturnsSamePoint()
        {
            var result = CoordinateConvertUtil.Destination(BeijingLon, BeijingLat, 0, 0);

            Assert.Equal(BeijingLon, result.Longitude, 5);
            Assert.Equal(BeijingLat, result.Latitude, 5);
        }

        [Fact]
        public void Destination_North_ThenDistanceMatches()
        {
            // Go 1000m due north
            var dest = CoordinateConvertUtil.Destination(BeijingLon, BeijingLat, 0, 1000);
            double distance = CoordinateConvertUtil.Distance(BeijingLon, BeijingLat, dest.Longitude, dest.Latitude);

            Assert.InRange(distance, 999, 1001);
        }

        [Fact]
        public void Destination_East_ThenDistanceMatches()
        {
            // Go 1000m due east
            var dest = CoordinateConvertUtil.Destination(BeijingLon, BeijingLat, 90, 1000);
            double distance = CoordinateConvertUtil.Distance(BeijingLon, BeijingLat, dest.Longitude, dest.Latitude);

            Assert.InRange(distance, 999, 1001);
        }

        #endregion

        #region OutOfChina

        [Fact]
        public void OutOfChina_InsideBeijing_ReturnsFalse()
        {
            Assert.False(CoordinateConvertUtil.OutOfChina(BeijingLon, BeijingLat));
        }

        [Fact]
        public void OutOfChina_NewYork_ReturnsTrue()
        {
            Assert.True(CoordinateConvertUtil.OutOfChina(-74.006, 40.7128));
        }

        [Fact]
        public void OutOfChina_London_ReturnsTrue()
        {
            Assert.True(CoordinateConvertUtil.OutOfChina(-0.1276, 51.5074));
        }

        [Fact]
        public void OutOfChina_Tokyo_ReturnsTrue()
        {
            Assert.True(CoordinateConvertUtil.OutOfChina(139.6917, 35.6895));
        }

        [Fact]
        public void OutOfChina_BoundaryLonMin_ReturnsTrue()
        {
            // Just west of 72.004
            Assert.True(CoordinateConvertUtil.OutOfChina(71.0, 30.0));
        }

        [Fact]
        public void OutOfChina_BoundaryLonMax_ReturnsTrue()
        {
            // Just east of 137.8347
            Assert.True(CoordinateConvertUtil.OutOfChina(138.0, 30.0));
        }

        [Fact]
        public void OutOfChina_BoundaryLatMin_ReturnsTrue()
        {
            Assert.True(CoordinateConvertUtil.OutOfChina(100.0, 0.0));
        }

        [Fact]
        public void OutOfChina_BoundaryLatMax_ReturnsTrue()
        {
            Assert.True(CoordinateConvertUtil.OutOfChina(100.0, 60.0));
        }

        [Fact]
        public void OutOfChina_InsideChina_ReturnsFalse()
        {
            // Shanghai
            Assert.False(CoordinateConvertUtil.OutOfChina(121.474, 31.230));
            // Guangzhou
            Assert.False(CoordinateConvertUtil.OutOfChina(113.264, 23.129));
            // Urumqi
            Assert.False(CoordinateConvertUtil.OutOfChina(87.617, 43.793));
        }

        #endregion
    }
}
