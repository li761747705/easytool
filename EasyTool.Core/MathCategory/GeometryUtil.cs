using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.MathCategory
{
    /// <summary>
    /// 几何工具类
    /// 提供点、线、面、多边形等几何计算功能
    /// </summary>
    public static class GeometryUtil
    {
        #region 点

        /// <summary>
        /// 计算两点之间的距离
        /// </summary>
        public static double Distance(Point2D p1, Point2D p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        /// <summary>
        /// 计算两点之间的距离（3D）
        /// </summary>
        public static double Distance(Point3D p1, Point3D p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.Z - p1.Z, 2));
        }

        /// <summary>
        /// 获取两点之间的中点
        /// </summary>
        public static Point2D Midpoint(Point2D p1, Point2D p2)
        {
            return new Point2D((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }

        /// <summary>
        /// 点是否在线段上
        /// </summary>
        public static bool IsPointOnLine(Point2D point, Line2D line, double tolerance = 1e-10)
        {
            // 使用叉积判断
            double cross = (point.Y - line.Start.Y) * (line.End.X - line.Start.X) -
                          (point.X - line.Start.X) * (line.End.Y - line.Start.Y);

            if (Math.Abs(cross) > tolerance) return false;

            // 检查是否在线段范围内
            return point.X >= Math.Min(line.Start.X, line.End.X) - tolerance &&
                   point.X <= Math.Max(line.Start.X, line.End.X) + tolerance &&
                   point.Y >= Math.Min(line.Start.Y, line.End.Y) - tolerance &&
                   point.Y <= Math.Max(line.Start.Y, line.End.Y) + tolerance;
        }

        /// <summary>
        /// 点到直线的距离
        /// </summary>
        public static double PointToLineDistance(Point2D point, Line2D line)
        {
            double A = line.End.Y - line.Start.Y;
            double B = line.Start.X - line.End.X;
            double C = line.End.X * line.Start.Y - line.Start.X * line.End.Y;

            return Math.Abs(A * point.X + B * point.Y + C) / Math.Sqrt(A * A + B * B);
        }

        /// <summary>
        /// 点到线段的最近点
        /// </summary>
        public static Point2D ClosestPointOnSegment(Point2D point, Line2D line)
        {
            double dx = line.End.X - line.Start.X;
            double dy = line.End.Y - line.Start.Y;

            if (Math.Abs(dx) < 1e-10 && Math.Abs(dy) < 1e-10)
                return line.Start;

            double t = ((point.X - line.Start.X) * dx + (point.Y - line.Start.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));

            return new Point2D(line.Start.X + t * dx, line.Start.Y + t * dy);
        }

        #endregion

        #region 线

        /// <summary>
        /// 计算线段长度
        /// </summary>
        public static double Length(Line2D line)
        {
            return Distance(line.Start, line.End);
        }

        /// <summary>
        /// 两条线段是否相交
        /// </summary>
        public static bool Intersects(Line2D line1, Line2D line2)
        {
            return GetIntersection(line1, line2) != null;
        }

        /// <summary>
        /// 获取两条线段的交点
        /// </summary>
        public static Point2D? GetIntersection(Line2D line1, Line2D line2)
        {
            double x1 = line1.Start.X, y1 = line1.Start.Y;
            double x2 = line1.End.X, y2 = line1.End.Y;
            double x3 = line2.Start.X, y3 = line2.Start.Y;
            double x4 = line2.End.X, y4 = line2.End.Y;

            double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            if (Math.Abs(denom) < 1e-10) return null; // 平行

            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
            double u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denom;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                return new Point2D(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
            }

            return null;
        }

        /// <summary>
        /// 计算两直线的夹角（弧度）
        /// </summary>
        public static double AngleBetween(Line2D line1, Line2D line2)
        {
            double dx1 = line1.End.X - line1.Start.X;
            double dy1 = line1.End.Y - line1.Start.Y;
            double dx2 = line2.End.X - line2.Start.X;
            double dy2 = line2.End.Y - line2.Start.Y;

            double dot = dx1 * dx2 + dy1 * dy2;
            double len1 = Math.Sqrt(dx1 * dx1 + dy1 * dy1);
            double len2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);

            if (len1 < 1e-10 || len2 < 1e-10) return 0;

            double cos = dot / (len1 * len2);
            cos = Math.Max(-1, Math.Min(1, cos));

            return Math.Acos(cos);
        }

        #endregion

        #region 多边形

        /// <summary>
        /// 计算多边形周长
        /// </summary>
        public static double Perimeter(Polygon polygon)
        {
            double perimeter = 0;
            var points = polygon.Points;
            for (int i = 0; i < points.Count; i++)
            {
                int next = (i + 1) % points.Count;
                perimeter += Distance(points[i], points[next]);
            }
            return perimeter;
        }

        /// <summary>
        /// 计算多边形面积（使用鞋带公式）
        /// </summary>
        public static double Area(Polygon polygon)
        {
            double area = 0;
            var points = polygon.Points;

            for (int i = 0; i < points.Count; i++)
            {
                int next = (i + 1) % points.Count;
                area += points[i].X * points[next].Y;
                area -= points[next].X * points[i].Y;
            }

            return Math.Abs(area) / 2;
        }

        /// <summary>
        /// 判断多边形是否为凸多边形
        /// </summary>
        public static bool IsConvex(Polygon polygon)
        {
            var points = polygon.Points;
            if (points.Count < 3) return false;

            bool? sign = null;
            for (int i = 0; i < points.Count; i++)
            {
                int prev = (i - 1 + points.Count) % points.Count;
                int next = (i + 1) % points.Count;

                double cross = CrossProduct(
                    points[prev], points[i], points[next]);

                if (cross != 0)
                {
                    if (sign == null)
                        sign = cross > 0;
                    else if (sign != cross > 0)
                        return false;
                }
            }

            return true;
        }

        private static double CrossProduct(Point2D o, Point2D a, Point2D b)
        {
            return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
        }

        /// <summary>
        /// 判断点是否在多边形内（射线法）
        /// </summary>
        public static bool IsPointInPolygon(Point2D point, Polygon polygon)
        {
            var points = polygon.Points;
            int n = points.Count;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                if (((points[i].Y > point.Y) != (points[j].Y > point.Y)) &&
                    (point.X < (points[j].X - points[i].X) * (point.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        /// 计算多边形质心
        /// </summary>
        public static Point2D Centroid(Polygon polygon)
        {
            var points = polygon.Points;
            double cx = 0, cy = 0;

            foreach (var p in points)
            {
                cx += p.X;
                cy += p.Y;
            }

            return new Point2D(cx / points.Count, cy / points.Count);
        }

        /// <summary>
        /// 计算凸包（Graham 扫描算法）
        /// </summary>
        public static Polygon ConvexHull(List<Point2D> points)
        {
            if (points.Count < 3) return new Polygon(points);

            // 找到最下方的点（y最小，y相同取x最小）
            var start = points.OrderBy(p => p.Y).ThenBy(p => p.X).First();
            var sorted = points.Where(p => p != start).ToList();

            // 按极角排序
            sorted.Sort((a, b) =>
            {
                double angleA = Math.Atan2(a.Y - start.Y, a.X - start.X);
                double angleB = Math.Atan2(b.Y - start.Y, b.X - start.X);
                if (Math.Abs(angleA - angleB) < 1e-10)
                {
                    return Distance(start, a).CompareTo(Distance(start, b));
                }
                return angleA.CompareTo(angleB);
            });

            var hull = new List<Point2D> { start };

            foreach (var point in sorted)
            {
                while (hull.Count > 1 && CrossProduct(hull[hull.Count - 2], hull[hull.Count - 1], point) <= 0)
                {
                    hull.RemoveAt(hull.Count - 1);
                }
                hull.Add(point);
            }

            return new Polygon(hull);
        }

        /// <summary>
        /// 多边形简化（Douglas-Peucker 算法）
        /// </summary>
        public static Polygon Simplify(Polygon polygon, double tolerance)
        {
            var points = polygon.Points;
            if (points.Count < 3) return polygon;

            var result = DouglasPeucker(points, tolerance);
            return new Polygon(result);
        }

        private static List<Point2D> DouglasPeucker(List<Point2D> points, double tolerance)
        {
            if (points.Count <= 2) return points;

            double maxDist = 0;
            int maxIndex = 0;
            var line = new Line2D(points[0], points[points.Count - 1]);

            for (int i = 1; i < points.Count - 1; i++)
            {
                double dist = PointToLineDistance(points[i], line);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    maxIndex = i;
                }
            }

            if (maxDist > tolerance)
            {
                var left = DouglasPeucker(points.GetRange(0, maxIndex + 1), tolerance);
                var right = DouglasPeucker(points.GetRange(maxIndex, points.Count - maxIndex), tolerance);

                var result = new List<Point2D>(left);
                result.AddRange(right.Skip(1));
                return result;
            }

            return new List<Point2D> { points[0], points[points.Count - 1] };
        }

        #endregion

        #region 圆

        /// <summary>
        /// 计算圆的周长
        /// </summary>
        public static double Circumference(Circle circle)
        {
            return 2 * Math.PI * circle.Radius;
        }

        /// <summary>
        /// 计算圆的面积
        /// </summary>
        public static double Area(Circle circle)
        {
            return Math.PI * circle.Radius * circle.Radius;
        }

        /// <summary>
        /// 判断点是否在圆内
        /// </summary>
        public static bool IsPointInCircle(Point2D point, Circle circle)
        {
            return Distance(point, circle.Center) <= circle.Radius;
        }

        /// <summary>
        /// 获取圆与直线的交点
        /// </summary>
        public static List<Point2D> GetCircleLineIntersections(Circle circle, Line2D line)
        {
            var result = new List<Point2D>();

            double dx = line.End.X - line.Start.X;
            double dy = line.End.Y - line.Start.Y;

            double fx = line.Start.X - circle.Center.X;
            double fy = line.Start.Y - circle.Center.Y;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = fx * fx + fy * fy - circle.Radius * circle.Radius;

            double discriminant = b * b - 4 * a * c;

            if (discriminant < 0) return result;

            discriminant = Math.Sqrt(discriminant);

            double t1 = (-b - discriminant) / (2 * a);
            double t2 = (-b + discriminant) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
                result.Add(new Point2D(line.Start.X + t1 * dx, line.Start.Y + t1 * dy));

            if (t2 >= 0 && t2 <= 1 && Math.Abs(t1 - t2) > 1e-10)
                result.Add(new Point2D(line.Start.X + t2 * dx, line.Start.Y + t2 * dy));

            return result;
        }

        #endregion

        #region 三角形

        /// <summary>
        /// 计算三角形面积（海伦公式）
        /// </summary>
        public static double TriangleArea(Point2D a, Point2D b, Point2D c)
        {
            double ab = Distance(a, b);
            double bc = Distance(b, c);
            double ca = Distance(c, a);
            double s = (ab + bc + ca) / 2;

            return Math.Sqrt(s * (s - ab) * (s - bc) * (s - ca));
        }

        /// <summary>
        /// 判断点是否在三角形内
        /// </summary>
        public static bool IsPointInTriangle(Point2D p, Point2D a, Point2D b, Point2D c)
        {
            double area = TriangleArea(a, b, c);
            double area1 = TriangleArea(p, b, c);
            double area2 = TriangleArea(a, p, c);
            double area3 = TriangleArea(a, b, p);

            return Math.Abs(area - (area1 + area2 + area3)) < 1e-10;
        }

        #endregion
    }

    #region 几何类型定义

    /// <summary>
    /// 二维点
    /// </summary>
    public struct Point2D : IEquatable<Point2D>
    {
        /// <summary>X坐标</summary>
        public double X { get; set; }
        /// <summary>Y坐标</summary>
        public double Y { get; set; }

        public Point2D(double x, double y) { X = x; Y = y; }

        public static Point2D operator +(Point2D a, Point2D b) => new(a.X + b.X, a.Y + b.Y);
        public static Point2D operator -(Point2D a, Point2D b) => new(a.X - b.X, a.Y - b.Y);
        public static Point2D operator *(Point2D p, double scalar) => new(p.X * scalar, p.Y * scalar);
        public static bool operator ==(Point2D left, Point2D right) => left.Equals(right);
        public static bool operator !=(Point2D left, Point2D right) => !left.Equals(right);

        public double Length => Math.Sqrt(X * X + Y * Y);
        public Point2D Normalize => this * (1 / Length);

        public bool Equals(Point2D other) => Math.Abs(X - other.X) < 1e-10 && Math.Abs(Y - other.Y) < 1e-10;
        public override bool Equals(object? obj) => obj is Point2D other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X:F2}, {Y:F2})";
    }

    /// <summary>
    /// 三维点
    /// </summary>
    public struct Point3D
    {
        /// <summary>X坐标</summary>
        public double X { get; set; }
        /// <summary>Y坐标</summary>
        public double Y { get; set; }
        /// <summary>Z坐标</summary>
        public double Z { get; set; }

        public Point3D(double x, double y, double z) { X = x; Y = y; Z = z; }

        public static Point3D operator +(Point3D a, Point3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Point3D operator -(Point3D a, Point3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }

    /// <summary>
    /// 二维线段
    /// </summary>
    public struct Line2D
    {
        /// <summary>起点</summary>
        public Point2D Start { get; set; }
        /// <summary>终点</summary>
        public Point2D End { get; set; }

        public Line2D(Point2D start, Point2D end) { Start = start; End = end; }
        public Line2D(double x1, double y1, double x2, double y2)
            : this(new Point2D(x1, y1), new Point2D(x2, y2)) { }

        public double Length => GeometryUtil.Distance(Start, End);

        public override string ToString() => $"[{Start} -> {End}]";
    }

    /// <summary>
    /// 多边形
    /// </summary>
    public class Polygon
    {
        /// <summary>顶点列表</summary>
        public List<Point2D> Points { get; }

        public Polygon(IEnumerable<Point2D> points)
        {
            Points = new List<Point2D>(points);
        }

        public int VertexCount => Points.Count;
        public double Perimeter => GeometryUtil.Perimeter(this);
        public double Area => GeometryUtil.Area(this);
        public bool IsConvex => GeometryUtil.IsConvex(this);
        public Point2D Centroid => GeometryUtil.Centroid(this);

        public override string ToString() => $"Polygon[{VertexCount} vertices, Area={Area:F2}]";
    }

    /// <summary>
    /// 圆
    /// </summary>
    public struct Circle
    {
        /// <summary>圆心</summary>
        public Point2D Center { get; set; }
        /// <summary>半径</summary>
        public double Radius { get; set; }

        public Circle(Point2D center, double radius) { Center = center; Radius = radius; }
        public Circle(double x, double y, double radius)
            : this(new Point2D(x, y), radius) { }

        public double Circumference => GeometryUtil.Circumference(this);
        public double Area => GeometryUtil.Area(this);

        public override string ToString() => $"Circle[Center={Center}, R={Radius:F2}]";
    }

    /// <summary>
    /// 矩形
    /// </summary>
    public struct Rectangle2D
    {
        /// <summary>左上角X</summary>
        public double X { get; set; }
        /// <summary>左上角Y</summary>
        public double Y { get; set; }
        /// <summary>宽度</summary>
        public double Width { get; set; }
        /// <summary>高度</summary>
        public double Height { get; set; }

        public Rectangle2D(double x, double y, double width, double height)
        {
            X = x; Y = y; Width = width; Height = height;
        }

        public double Left => X;
        public double Top => Y;
        public double Right => X + Width;
        public double Bottom => Y + Height;

        public Point2D TopLeft => new(X, Y);
        public Point2D TopRight => new(Right, Y);
        public Point2D BottomLeft => new(X, Bottom);
        public Point2D BottomRight => new(Right, Bottom);
        public Point2D Center => new(X + Width / 2, Y + Height / 2);

        public double Perimeter => 2 * (Width + Height);
        public double Area => Width * Height;

        public bool Contains(Point2D point) =>
            point.X >= X && point.X <= Right && point.Y >= Y && point.Y <= Bottom;

        public bool Intersects(Rectangle2D other) =>
            X < other.Right && Right > other.X && Y < other.Bottom && Bottom > other.Y;

        public override string ToString() => $"Rect[X={X}, Y={Y}, W={Width}, H={Height}]";
    }

    #endregion
}
