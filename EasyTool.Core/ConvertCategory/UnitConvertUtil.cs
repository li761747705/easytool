using System;
using System.Collections.Generic;

namespace EasyTool.ConvertCategory
{
    /// <summary>
    /// 单位转换工具类
    /// 提供长度、重量、温度、面积、体积等常用单位转换
    /// </summary>
    public static class UnitConvertUtil
    {
        #region 长度

        /// <summary>
        /// 长度单位
        /// </summary>
        public enum LengthUnit
        {
            Millimeter, Centimeter, Meter, Kilometer,
            Inch, Foot, Yard, Mile,
            Nanometer, Micrometer, Decimeter
        }

        private static readonly Dictionary<LengthUnit, double> LengthToMeter = new()
        {
            { LengthUnit.Nanometer, 1e-9 },
            { LengthUnit.Micrometer, 1e-6 },
            { LengthUnit.Millimeter, 0.001 },
            { LengthUnit.Centimeter, 0.01 },
            { LengthUnit.Decimeter, 0.1 },
            { LengthUnit.Meter, 1 },
            { LengthUnit.Kilometer, 1000 },
            { LengthUnit.Inch, 0.0254 },
            { LengthUnit.Foot, 0.3048 },
            { LengthUnit.Yard, 0.9144 },
            { LengthUnit.Mile, 1609.344 }
        };

        /// <summary>
        /// 长度转换
        /// </summary>
        public static double ConvertLength(double value, LengthUnit from, LengthUnit to)
        {
            double meters = value * LengthToMeter[from];
            return meters / LengthToMeter[to];
        }

        /// <summary>
        /// 获取所有可转换的长度单位
        /// </summary>
        public static LengthUnit[] GetLengthUnits() => (LengthUnit[])Enum.GetValues(typeof(LengthUnit));

        #endregion

        #region 重量

        /// <summary>
        /// 重量单位
        /// </summary>
        public enum WeightUnit
        {
            Milligram, Gram, Kilogram, Ton,
            Ounce, Pound, Jin, Liang
        }

        private static readonly Dictionary<WeightUnit, double> WeightToGram = new()
        {
            { WeightUnit.Milligram, 0.001 },
            { WeightUnit.Gram, 1 },
            { WeightUnit.Kilogram, 1000 },
            { WeightUnit.Ton, 1000000 },
            { WeightUnit.Ounce, 28.349523125 },
            { WeightUnit.Pound, 453.59237 },
            { WeightUnit.Jin, 500 },       // 市斤
            { WeightUnit.Liang, 50 }       // 市两
        };

        /// <summary>
        /// 重量转换
        /// </summary>
        public static double ConvertWeight(double value, WeightUnit from, WeightUnit to)
        {
            double grams = value * WeightToGram[from];
            return grams / WeightToGram[to];
        }

        #endregion

        #region 温度

        /// <summary>
        /// 温度单位
        /// </summary>
        public enum TemperatureUnit
        {
            Celsius, Fahrenheit, Kelvin
        }

        /// <summary>
        /// 温度转换
        /// </summary>
        public static double ConvertTemperature(double value, TemperatureUnit from, TemperatureUnit to)
        {
            // 先转换为摄氏度
            double celsius = from switch
            {
                TemperatureUnit.Celsius => value,
                TemperatureUnit.Fahrenheit => (value - 32) * 5 / 9,
                TemperatureUnit.Kelvin => value - 273.15,
                _ => throw new ArgumentException("无效的温度单位")
            };

            // 再从摄氏度转换为目标单位
            return to switch
            {
                TemperatureUnit.Celsius => celsius,
                TemperatureUnit.Fahrenheit => celsius * 9 / 5 + 32,
                TemperatureUnit.Kelvin => celsius + 273.15,
                _ => throw new ArgumentException("无效的温度单位")
            };
        }

        #endregion

        #region 面积

        /// <summary>
        /// 面积单位
        /// </summary>
        public enum AreaUnit
        {
            SquareMillimeter, SquareCentimeter, SquareMeter, SquareKilometer,
            SquareInch, SquareFoot, SquareYard, SquareMile,
            Hectare, Acre, Mu
        }

        private static readonly Dictionary<AreaUnit, double> AreaToSquareMeter = new()
        {
            { AreaUnit.SquareMillimeter, 0.000001 },
            { AreaUnit.SquareCentimeter, 0.0001 },
            { AreaUnit.SquareMeter, 1 },
            { AreaUnit.SquareKilometer, 1000000 },
            { AreaUnit.SquareInch, 0.00064516 },
            { AreaUnit.SquareFoot, 0.09290304 },
            { AreaUnit.SquareYard, 0.83612736 },
            { AreaUnit.SquareMile, 2589988.110336 },
            { AreaUnit.Hectare, 10000 },
            { AreaUnit.Acre, 4046.8564224 },
            { AreaUnit.Mu, 666.66666666667 } // 市亩
        };

        /// <summary>
        /// 面积转换
        /// </summary>
        public static double ConvertArea(double value, AreaUnit from, AreaUnit to)
        {
            double sqMeters = value * AreaToSquareMeter[from];
            return sqMeters / AreaToSquareMeter[to];
        }

        #endregion

        #region 体积

        /// <summary>
        /// 体积单位
        /// </summary>
        public enum VolumeUnit
        {
            CubicMillimeter, CubicCentimeter, CubicMeter, CubicKilometer,
            Milliliter, Liter,
            CubicInch, CubicFoot, CubicYard,
            GallonUS, GallonUK, PintUS, PintUK,
            FluidOunceUS, FluidOunceUK
        }

        private static readonly Dictionary<VolumeUnit, double> VolumeToLiter = new()
        {
            { VolumeUnit.CubicMillimeter, 0.000001 },
            { VolumeUnit.CubicCentimeter, 0.001 },
            { VolumeUnit.CubicMeter, 1000 },
            { VolumeUnit.CubicKilometer, 1e12 },
            { VolumeUnit.Milliliter, 0.001 },
            { VolumeUnit.Liter, 1 },
            { VolumeUnit.CubicInch, 0.016387064 },
            { VolumeUnit.CubicFoot, 28.316846592 },
            { VolumeUnit.CubicYard, 764.554857984 },
            { VolumeUnit.GallonUS, 3.785411784 },
            { VolumeUnit.GallonUK, 4.54609 },
            { VolumeUnit.PintUS, 0.473176473 },
            { VolumeUnit.PintUK, 0.56826125 },
            { VolumeUnit.FluidOunceUS, 0.0295735295625 },
            { VolumeUnit.FluidOunceUK, 0.0284130625 }
        };

        /// <summary>
        /// 体积转换
        /// </summary>
        public static double ConvertVolume(double value, VolumeUnit from, VolumeUnit to)
        {
            double liters = value * VolumeToLiter[from];
            return liters / VolumeToLiter[to];
        }

        #endregion

        #region 速度

        /// <summary>
        /// 速度单位
        /// </summary>
        public enum SpeedUnit
        {
            MeterPerSecond, KilometerPerHour, MilePerHour,
            Knot, FootPerSecond
        }

        private static readonly Dictionary<SpeedUnit, double> SpeedToMps = new()
        {
            { SpeedUnit.MeterPerSecond, 1 },
            { SpeedUnit.KilometerPerHour, 1000.0 / 3600 },
            { SpeedUnit.MilePerHour, 0.44704 },
            { SpeedUnit.Knot, 0.514444444 },
            { SpeedUnit.FootPerSecond, 0.3048 }
        };

        /// <summary>
        /// 速度转换
        /// </summary>
        public static double ConvertSpeed(double value, SpeedUnit from, SpeedUnit to)
        {
            double mps = value * SpeedToMps[from];
            return mps / SpeedToMps[to];
        }

        #endregion

        #region 时间

        /// <summary>
        /// 时间单位
        /// </summary>
        public enum TimeUnit
        {
            Millisecond, Second, Minute, Hour, Day, Week,
            Month, Year, Decade, Century
        }

        private static readonly Dictionary<TimeUnit, double> TimeToSecond = new()
        {
            { TimeUnit.Millisecond, 0.001 },
            { TimeUnit.Second, 1 },
            { TimeUnit.Minute, 60 },
            { TimeUnit.Hour, 3600 },
            { TimeUnit.Day, 86400 },
            { TimeUnit.Week, 604800 },
            { TimeUnit.Month, 2629746 },    // 平均月份
            { TimeUnit.Year, 31556952 },    // 平均年
            { TimeUnit.Decade, 315569520 },
            { TimeUnit.Century, 3155695200 }
        };

        /// <summary>
        /// 时间转换
        /// </summary>
        public static double ConvertTime(double value, TimeUnit from, TimeUnit to)
        {
            double seconds = value * TimeToSecond[from];
            return seconds / TimeToSecond[to];
        }

        #endregion

        #region 压力

        /// <summary>
        /// 压力单位
        /// </summary>
        public enum PressureUnit
        {
            Pascal, Kilopascal, Megapascal, Bar,
            Psi, Atm, Torr, MmHg
        }

        private static readonly Dictionary<PressureUnit, double> PressureToPascal = new()
        {
            { PressureUnit.Pascal, 1 },
            { PressureUnit.Kilopascal, 1000 },
            { PressureUnit.Megapascal, 1000000 },
            { PressureUnit.Bar, 100000 },
            { PressureUnit.Psi, 6894.757293168 },
            { PressureUnit.Atm, 101325 },
            { PressureUnit.Torr, 133.3223684211 },
            { PressureUnit.MmHg, 133.322 }
        };

        /// <summary>
        /// 压力转换
        /// </summary>
        public static double ConvertPressure(double value, PressureUnit from, PressureUnit to)
        {
            double pascals = value * PressureToPascal[from];
            return pascals / PressureToPascal[to];
        }

        #endregion

        #region 角度

        /// <summary>
        /// 角度单位
        /// </summary>
        public enum AngleUnit
        {
            Degree, Radian, Gradian, Turn
        }

        /// <summary>
        /// 角度转换
        /// </summary>
        public static double ConvertAngle(double value, AngleUnit from, AngleUnit to)
        {
            // 先转换为度
            double degrees = from switch
            {
                AngleUnit.Degree => value,
                AngleUnit.Radian => value * 180 / Math.PI,
                AngleUnit.Gradian => value * 0.9,
                AngleUnit.Turn => value * 360,
                _ => throw new ArgumentException("无效的角度单位")
            };

            // 再从度转换为目标单位
            return to switch
            {
                AngleUnit.Degree => degrees,
                AngleUnit.Radian => degrees * Math.PI / 180,
                AngleUnit.Gradian => degrees / 0.9,
                AngleUnit.Turn => degrees / 360,
                _ => throw new ArgumentException("无效的角度单位")
            };
        }

        #endregion

        #region 数据大小

        /// <summary>
        /// 数据大小单位
        /// </summary>
        public enum DataUnit
        {
            Bit, Byte,
            Kilobyte, Megabyte, Gigabyte, Terabyte, Petabyte,
            Kibibyte, Mebibyte, Gibibyte, Tebibyte, Pebibyte
        }

        private static readonly Dictionary<DataUnit, double> DataToByte = new()
        {
            { DataUnit.Bit, 0.125 },
            { DataUnit.Byte, 1 },
            { DataUnit.Kilobyte, 1000 },
            { DataUnit.Megabyte, 1000000 },
            { DataUnit.Gigabyte, 1e9 },
            { DataUnit.Terabyte, 1e12 },
            { DataUnit.Petabyte, 1e15 },
            { DataUnit.Kibibyte, 1024 },
            { DataUnit.Mebibyte, 1048576 },
            { DataUnit.Gibibyte, 1073741824 },
            { DataUnit.Tebibyte, 1099511627776 },
            { DataUnit.Pebibyte, 1125899906842624 }
        };

        /// <summary>
        /// 数据大小转换
        /// </summary>
        public static double ConvertData(double value, DataUnit from, DataUnit to)
        {
            double bytes = value * DataToByte[from];
            return bytes / DataToByte[to];
        }

        /// <summary>
        /// 自动格式化数据大小
        /// </summary>
        public static string FormatDataSize(double bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB" };
            int unitIndex = 0;

            while (bytes >= 1024 && unitIndex < units.Length - 1)
            {
                bytes /= 1024;
                unitIndex++;
            }

            return $"{bytes:F2} {units[unitIndex]}";
        }

        #endregion

        #region 能量

        /// <summary>
        /// 能量单位
        /// </summary>
        public enum EnergyUnit
        {
            Joule, Kilojoule, Megajoule, Calorie, Kilocalorie,
            WattHour, KilowattHour, BritishThermalUnit
        }

        private static readonly Dictionary<EnergyUnit, double> EnergyToJoule = new()
        {
            { EnergyUnit.Joule, 1 },
            { EnergyUnit.Kilojoule, 1000 },
            { EnergyUnit.Megajoule, 1000000 },
            { EnergyUnit.Calorie, 4.184 },
            { EnergyUnit.Kilocalorie, 4184 },
            { EnergyUnit.WattHour, 3600 },
            { EnergyUnit.KilowattHour, 3600000 },
            { EnergyUnit.BritishThermalUnit, 1055.06 }
        };

        /// <summary>
        /// 能量转换
        /// </summary>
        public static double ConvertEnergy(double value, EnergyUnit from, EnergyUnit to)
        {
            double joules = value * EnergyToJoule[from];
            return joules / EnergyToJoule[to];
        }

        #endregion
    }
}
