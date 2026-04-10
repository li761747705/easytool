using System;
using Xunit;

namespace EasyTool.ConvertCategory.Tests
{
    public class UnitConvertUtilTests
    {
        #region Length

        [Fact]
        public void ConvertLength_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertLength(1.0, UnitConvertUtil.LengthUnit.Meter, UnitConvertUtil.LengthUnit.Meter));
            Assert.Equal(5.0, UnitConvertUtil.ConvertLength(5.0, UnitConvertUtil.LengthUnit.Kilometer, UnitConvertUtil.LengthUnit.Kilometer));
        }

        [Fact]
        public void ConvertLength_MeterToKilometer()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertLength(1000, UnitConvertUtil.LengthUnit.Meter, UnitConvertUtil.LengthUnit.Kilometer));
            Assert.Equal(0.001, UnitConvertUtil.ConvertLength(1, UnitConvertUtil.LengthUnit.Meter, UnitConvertUtil.LengthUnit.Kilometer));
        }

        [Fact]
        public void ConvertLength_KilometerToMeter()
        {
            Assert.Equal(1000, UnitConvertUtil.ConvertLength(1, UnitConvertUtil.LengthUnit.Kilometer, UnitConvertUtil.LengthUnit.Meter));
        }

        [Fact]
        public void ConvertLength_CentimeterToMeter()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertLength(100, UnitConvertUtil.LengthUnit.Centimeter, UnitConvertUtil.LengthUnit.Meter));
        }

        [Fact]
        public void ConvertLength_MillimeterToMeter()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertLength(1000, UnitConvertUtil.LengthUnit.Millimeter, UnitConvertUtil.LengthUnit.Meter));
        }

        [Fact]
        public void ConvertLength_InchToCentimeter()
        {
            Assert.Equal(2.54, UnitConvertUtil.ConvertLength(1, UnitConvertUtil.LengthUnit.Inch, UnitConvertUtil.LengthUnit.Centimeter), 2);
        }

        [Fact]
        public void ConvertLength_FootToMeter()
        {
            Assert.Equal(0.3048, UnitConvertUtil.ConvertLength(1, UnitConvertUtil.LengthUnit.Foot, UnitConvertUtil.LengthUnit.Meter), 4);
        }

        [Fact]
        public void ConvertLength_MileToKilometer()
        {
            Assert.Equal(1.609344, UnitConvertUtil.ConvertLength(1, UnitConvertUtil.LengthUnit.Mile, UnitConvertUtil.LengthUnit.Kilometer), 5);
        }

        [Fact]
        public void ConvertLength_YardToMeter()
        {
            Assert.Equal(0.9144, UnitConvertUtil.ConvertLength(1, UnitConvertUtil.LengthUnit.Yard, UnitConvertUtil.LengthUnit.Meter), 4);
        }

        [Fact]
        public void ConvertLength_NanometerToMeter()
        {
            Assert.Equal(1e-9, UnitConvertUtil.ConvertLength(1, UnitConvertUtil.LengthUnit.Nanometer, UnitConvertUtil.LengthUnit.Meter), 15);
        }

        [Fact]
        public void ConvertLength_MicrometerToMeter()
        {
            Assert.Equal(1e-6, UnitConvertUtil.ConvertLength(1, UnitConvertUtil.LengthUnit.Micrometer, UnitConvertUtil.LengthUnit.Meter), 10);
        }

        [Fact]
        public void ConvertLength_DecimeterToMeter()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertLength(10, UnitConvertUtil.LengthUnit.Decimeter, UnitConvertUtil.LengthUnit.Meter));
        }

        [Fact]
        public void ConvertLength_ZeroValue_ReturnsZero()
        {
            Assert.Equal(0, UnitConvertUtil.ConvertLength(0, UnitConvertUtil.LengthUnit.Meter, UnitConvertUtil.LengthUnit.Kilometer));
        }

        [Fact]
        public void ConvertLength_NegativeValue_ConvertsCorrectly()
        {
            double result = UnitConvertUtil.ConvertLength(-1000, UnitConvertUtil.LengthUnit.Meter, UnitConvertUtil.LengthUnit.Kilometer);
            Assert.Equal(-1.0, result);
        }

        [Fact]
        public void GetLengthUnits_ReturnsAllUnits()
        {
            var units = UnitConvertUtil.GetLengthUnits();

            Assert.Contains(UnitConvertUtil.LengthUnit.Millimeter, units);
            Assert.Contains(UnitConvertUtil.LengthUnit.Kilometer, units);
            Assert.Contains(UnitConvertUtil.LengthUnit.Mile, units);
            Assert.True(units.Length > 5);
        }

        #endregion

        #region Weight

        [Fact]
        public void ConvertWeight_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertWeight(1.0, UnitConvertUtil.WeightUnit.Kilogram, UnitConvertUtil.WeightUnit.Kilogram));
        }

        [Fact]
        public void ConvertWeight_KilogramToGram()
        {
            Assert.Equal(1000, UnitConvertUtil.ConvertWeight(1, UnitConvertUtil.WeightUnit.Kilogram, UnitConvertUtil.WeightUnit.Gram));
        }

        [Fact]
        public void ConvertWeight_GramToKilogram()
        {
            Assert.Equal(0.001, UnitConvertUtil.ConvertWeight(1, UnitConvertUtil.WeightUnit.Gram, UnitConvertUtil.WeightUnit.Kilogram));
        }

        [Fact]
        public void ConvertWeight_TonToKilogram()
        {
            Assert.Equal(1000, UnitConvertUtil.ConvertWeight(1, UnitConvertUtil.WeightUnit.Ton, UnitConvertUtil.WeightUnit.Kilogram));
        }

        [Fact]
        public void ConvertWeight_PoundToKilogram()
        {
            Assert.Equal(0.45359237, UnitConvertUtil.ConvertWeight(1, UnitConvertUtil.WeightUnit.Pound, UnitConvertUtil.WeightUnit.Kilogram), 5);
        }

        [Fact]
        public void ConvertWeight_OunceToGram()
        {
            Assert.Equal(28.349523125, UnitConvertUtil.ConvertWeight(1, UnitConvertUtil.WeightUnit.Ounce, UnitConvertUtil.WeightUnit.Gram), 5);
        }

        [Fact]
        public void ConvertWeight_JinToGram()
        {
            Assert.Equal(500, UnitConvertUtil.ConvertWeight(1, UnitConvertUtil.WeightUnit.Jin, UnitConvertUtil.WeightUnit.Gram));
        }

        [Fact]
        public void ConvertWeight_LiangToGram()
        {
            Assert.Equal(50, UnitConvertUtil.ConvertWeight(1, UnitConvertUtil.WeightUnit.Liang, UnitConvertUtil.WeightUnit.Gram));
        }

        [Fact]
        public void ConvertWeight_MilligramToGram()
        {
            Assert.Equal(0.001, UnitConvertUtil.ConvertWeight(1, UnitConvertUtil.WeightUnit.Milligram, UnitConvertUtil.WeightUnit.Gram));
        }

        [Fact]
        public void ConvertWeight_ZeroValue_ReturnsZero()
        {
            Assert.Equal(0, UnitConvertUtil.ConvertWeight(0, UnitConvertUtil.WeightUnit.Kilogram, UnitConvertUtil.WeightUnit.Gram));
        }

        #endregion

        #region Temperature

        [Fact]
        public void ConvertTemperature_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(100, UnitConvertUtil.ConvertTemperature(100, UnitConvertUtil.TemperatureUnit.Celsius, UnitConvertUtil.TemperatureUnit.Celsius));
        }

        [Fact]
        public void ConvertTemperature_CelsiusToFahrenheit()
        {
            // 0 C = 32 F
            Assert.Equal(32, UnitConvertUtil.ConvertTemperature(0, UnitConvertUtil.TemperatureUnit.Celsius, UnitConvertUtil.TemperatureUnit.Fahrenheit), 5);
            // 100 C = 212 F
            Assert.Equal(212, UnitConvertUtil.ConvertTemperature(100, UnitConvertUtil.TemperatureUnit.Celsius, UnitConvertUtil.TemperatureUnit.Fahrenheit), 5);
        }

        [Fact]
        public void ConvertTemperature_FahrenheitToCelsius()
        {
            // 32 F = 0 C
            Assert.Equal(0, UnitConvertUtil.ConvertTemperature(32, UnitConvertUtil.TemperatureUnit.Fahrenheit, UnitConvertUtil.TemperatureUnit.Celsius), 5);
            // 212 F = 100 C
            Assert.Equal(100, UnitConvertUtil.ConvertTemperature(212, UnitConvertUtil.TemperatureUnit.Fahrenheit, UnitConvertUtil.TemperatureUnit.Celsius), 5);
        }

        [Fact]
        public void ConvertTemperature_CelsiusToKelvin()
        {
            // 0 C = 273.15 K
            Assert.Equal(273.15, UnitConvertUtil.ConvertTemperature(0, UnitConvertUtil.TemperatureUnit.Celsius, UnitConvertUtil.TemperatureUnit.Kelvin), 5);
        }

        [Fact]
        public void ConvertTemperature_KelvinToCelsius()
        {
            // 273.15 K = 0 C
            Assert.Equal(0, UnitConvertUtil.ConvertTemperature(273.15, UnitConvertUtil.TemperatureUnit.Kelvin, UnitConvertUtil.TemperatureUnit.Celsius), 5);
        }

        [Fact]
        public void ConvertTemperature_FahrenheitToKelvin()
        {
            // 32 F = 273.15 K
            Assert.Equal(273.15, UnitConvertUtil.ConvertTemperature(32, UnitConvertUtil.TemperatureUnit.Fahrenheit, UnitConvertUtil.TemperatureUnit.Kelvin), 5);
        }

        [Fact]
        public void ConvertTemperature_KelvinToFahrenheit()
        {
            // 273.15 K = 32 F
            Assert.Equal(32, UnitConvertUtil.ConvertTemperature(273.15, UnitConvertUtil.TemperatureUnit.Kelvin, UnitConvertUtil.TemperatureUnit.Fahrenheit), 5);
        }

        [Fact]
        public void ConvertTemperature_NegativeCelsius_ConvertsCorrectly()
        {
            // -40 C = -40 F (intersection point)
            Assert.Equal(-40, UnitConvertUtil.ConvertTemperature(-40, UnitConvertUtil.TemperatureUnit.Celsius, UnitConvertUtil.TemperatureUnit.Fahrenheit), 5);
        }

        [Fact]
        public void ConvertTemperature_AbsoluteZero_Kelvin()
        {
            // 0 K = -273.15 C
            Assert.Equal(-273.15, UnitConvertUtil.ConvertTemperature(0, UnitConvertUtil.TemperatureUnit.Kelvin, UnitConvertUtil.TemperatureUnit.Celsius), 5);
        }

        [Fact]
        public void ConvertTemperature_RoundTrip_Celsius_Fahrenheit()
        {
            double original = 37.5;
            double f = UnitConvertUtil.ConvertTemperature(original, UnitConvertUtil.TemperatureUnit.Celsius, UnitConvertUtil.TemperatureUnit.Fahrenheit);
            double back = UnitConvertUtil.ConvertTemperature(f, UnitConvertUtil.TemperatureUnit.Fahrenheit, UnitConvertUtil.TemperatureUnit.Celsius);

            Assert.Equal(original, back, 10);
        }

        #endregion

        #region Area

        [Fact]
        public void ConvertArea_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertArea(1.0, UnitConvertUtil.AreaUnit.SquareMeter, UnitConvertUtil.AreaUnit.SquareMeter));
        }

        [Fact]
        public void ConvertArea_SquareMeterToSquareKilometer()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertArea(1000000, UnitConvertUtil.AreaUnit.SquareMeter, UnitConvertUtil.AreaUnit.SquareKilometer));
        }

        [Fact]
        public void ConvertArea_SquareKilometerToSquareMeter()
        {
            Assert.Equal(1000000, UnitConvertUtil.ConvertArea(1, UnitConvertUtil.AreaUnit.SquareKilometer, UnitConvertUtil.AreaUnit.SquareMeter));
        }

        [Fact]
        public void ConvertArea_HectareToSquareMeter()
        {
            Assert.Equal(10000, UnitConvertUtil.ConvertArea(1, UnitConvertUtil.AreaUnit.Hectare, UnitConvertUtil.AreaUnit.SquareMeter));
        }

        [Fact]
        public void ConvertArea_AcreToSquareMeter()
        {
            Assert.Equal(4046.8564224, UnitConvertUtil.ConvertArea(1, UnitConvertUtil.AreaUnit.Acre, UnitConvertUtil.AreaUnit.SquareMeter), 5);
        }

        [Fact]
        public void ConvertArea_MuToSquareMeter()
        {
            Assert.Equal(666.66666666667, UnitConvertUtil.ConvertArea(1, UnitConvertUtil.AreaUnit.Mu, UnitConvertUtil.AreaUnit.SquareMeter), 5);
        }

        [Fact]
        public void ConvertArea_SquareFootToSquareMeter()
        {
            Assert.Equal(0.09290304, UnitConvertUtil.ConvertArea(1, UnitConvertUtil.AreaUnit.SquareFoot, UnitConvertUtil.AreaUnit.SquareMeter), 8);
        }

        [Fact]
        public void ConvertArea_SquareInchToSquareCentimeter()
        {
            double sqCm = UnitConvertUtil.ConvertArea(1, UnitConvertUtil.AreaUnit.SquareInch, UnitConvertUtil.AreaUnit.SquareCentimeter);
            Assert.Equal(6.4516, sqCm, 4);
        }

        [Fact]
        public void ConvertArea_ZeroValue_ReturnsZero()
        {
            Assert.Equal(0, UnitConvertUtil.ConvertArea(0, UnitConvertUtil.AreaUnit.Hectare, UnitConvertUtil.AreaUnit.SquareMeter));
        }

        #endregion

        #region Volume

        [Fact]
        public void ConvertVolume_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertVolume(1.0, UnitConvertUtil.VolumeUnit.Liter, UnitConvertUtil.VolumeUnit.Liter));
        }

        [Fact]
        public void ConvertVolume_LiterToMilliliter()
        {
            Assert.Equal(1000, UnitConvertUtil.ConvertVolume(1, UnitConvertUtil.VolumeUnit.Liter, UnitConvertUtil.VolumeUnit.Milliliter));
        }

        [Fact]
        public void ConvertVolume_CubicMeterToLiter()
        {
            Assert.Equal(1000, UnitConvertUtil.ConvertVolume(1, UnitConvertUtil.VolumeUnit.CubicMeter, UnitConvertUtil.VolumeUnit.Liter));
        }

        [Fact]
        public void ConvertVolume_GallonUSToLiter()
        {
            Assert.Equal(3.785411784, UnitConvertUtil.ConvertVolume(1, UnitConvertUtil.VolumeUnit.GallonUS, UnitConvertUtil.VolumeUnit.Liter), 5);
        }

        [Fact]
        public void ConvertVolume_GallonUKToLiter()
        {
            Assert.Equal(4.54609, UnitConvertUtil.ConvertVolume(1, UnitConvertUtil.VolumeUnit.GallonUK, UnitConvertUtil.VolumeUnit.Liter), 5);
        }

        [Fact]
        public void ConvertVolume_CubicFootToLiter()
        {
            Assert.Equal(28.316846592, UnitConvertUtil.ConvertVolume(1, UnitConvertUtil.VolumeUnit.CubicFoot, UnitConvertUtil.VolumeUnit.Liter), 5);
        }

        [Fact]
        public void ConvertVolume_PintUSToLiter()
        {
            Assert.Equal(0.473176473, UnitConvertUtil.ConvertVolume(1, UnitConvertUtil.VolumeUnit.PintUS, UnitConvertUtil.VolumeUnit.Liter), 5);
        }

        [Fact]
        public void ConvertVolume_FluidOunceUSToMilliliter()
        {
            double ml = UnitConvertUtil.ConvertVolume(1, UnitConvertUtil.VolumeUnit.FluidOunceUS, UnitConvertUtil.VolumeUnit.Milliliter);
            Assert.Equal(29.5735295625, ml, 5);
        }

        [Fact]
        public void ConvertVolume_CubicMillimeterToLiter()
        {
            Assert.Equal(0.000001, UnitConvertUtil.ConvertVolume(1, UnitConvertUtil.VolumeUnit.CubicMillimeter, UnitConvertUtil.VolumeUnit.Liter), 10);
        }

        [Fact]
        public void ConvertVolume_ZeroValue_ReturnsZero()
        {
            Assert.Equal(0, UnitConvertUtil.ConvertVolume(0, UnitConvertUtil.VolumeUnit.Liter, UnitConvertUtil.VolumeUnit.GallonUS));
        }

        #endregion

        #region Speed

        [Fact]
        public void ConvertSpeed_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertSpeed(1.0, UnitConvertUtil.SpeedUnit.MeterPerSecond, UnitConvertUtil.SpeedUnit.MeterPerSecond));
        }

        [Fact]
        public void ConvertSpeed_KmhToMs()
        {
            // 3.6 km/h = 1 m/s
            Assert.Equal(1.0, UnitConvertUtil.ConvertSpeed(3.6, UnitConvertUtil.SpeedUnit.KilometerPerHour, UnitConvertUtil.SpeedUnit.MeterPerSecond), 5);
        }

        [Fact]
        public void ConvertSpeed_MsToKmh()
        {
            // 1 m/s = 3.6 km/h
            Assert.Equal(3.6, UnitConvertUtil.ConvertSpeed(1, UnitConvertUtil.SpeedUnit.MeterPerSecond, UnitConvertUtil.SpeedUnit.KilometerPerHour), 5);
        }

        [Fact]
        public void ConvertSpeed_MphToKmh()
        {
            // 1 mph ~= 1.60934 km/h
            double kmh = UnitConvertUtil.ConvertSpeed(1, UnitConvertUtil.SpeedUnit.MilePerHour, UnitConvertUtil.SpeedUnit.KilometerPerHour);
            Assert.Equal(1.609344, kmh, 5);
        }

        [Fact]
        public void ConvertSpeed_KnotToKmh()
        {
            // 1 knot ~= 1.852 km/h
            double kmh = UnitConvertUtil.ConvertSpeed(1, UnitConvertUtil.SpeedUnit.Knot, UnitConvertUtil.SpeedUnit.KilometerPerHour);
            Assert.Equal(1.852, kmh, 2);
        }

        [Fact]
        public void ConvertSpeed_FootPerSecondToMs()
        {
            Assert.Equal(0.3048, UnitConvertUtil.ConvertSpeed(1, UnitConvertUtil.SpeedUnit.FootPerSecond, UnitConvertUtil.SpeedUnit.MeterPerSecond), 4);
        }

        [Fact]
        public void ConvertSpeed_ZeroValue_ReturnsZero()
        {
            Assert.Equal(0, UnitConvertUtil.ConvertSpeed(0, UnitConvertUtil.SpeedUnit.KilometerPerHour, UnitConvertUtil.SpeedUnit.MeterPerSecond));
        }

        #endregion

        #region Time

        [Fact]
        public void ConvertTime_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertTime(1.0, UnitConvertUtil.TimeUnit.Hour, UnitConvertUtil.TimeUnit.Hour));
        }

        [Fact]
        public void ConvertTime_MinuteToSecond()
        {
            Assert.Equal(60, UnitConvertUtil.ConvertTime(1, UnitConvertUtil.TimeUnit.Minute, UnitConvertUtil.TimeUnit.Second));
        }

        [Fact]
        public void ConvertTime_HourToSecond()
        {
            Assert.Equal(3600, UnitConvertUtil.ConvertTime(1, UnitConvertUtil.TimeUnit.Hour, UnitConvertUtil.TimeUnit.Second));
        }

        [Fact]
        public void ConvertTime_DayToHour()
        {
            Assert.Equal(24, UnitConvertUtil.ConvertTime(1, UnitConvertUtil.TimeUnit.Day, UnitConvertUtil.TimeUnit.Hour));
        }

        [Fact]
        public void ConvertTime_DayToSecond()
        {
            Assert.Equal(86400, UnitConvertUtil.ConvertTime(1, UnitConvertUtil.TimeUnit.Day, UnitConvertUtil.TimeUnit.Second));
        }

        [Fact]
        public void ConvertTime_WeekToDay()
        {
            Assert.Equal(7, UnitConvertUtil.ConvertTime(1, UnitConvertUtil.TimeUnit.Week, UnitConvertUtil.TimeUnit.Day));
        }

        [Fact]
        public void ConvertTime_MillisecondToSecond()
        {
            Assert.Equal(0.001, UnitConvertUtil.ConvertTime(1, UnitConvertUtil.TimeUnit.Millisecond, UnitConvertUtil.TimeUnit.Second));
        }

        [Fact]
        public void ConvertTime_SecondToMillisecond()
        {
            Assert.Equal(1000, UnitConvertUtil.ConvertTime(1, UnitConvertUtil.TimeUnit.Second, UnitConvertUtil.TimeUnit.Millisecond));
        }

        [Fact]
        public void ConvertTime_YearToDay()
        {
            double days = UnitConvertUtil.ConvertTime(1, UnitConvertUtil.TimeUnit.Year, UnitConvertUtil.TimeUnit.Day);
            Assert.Equal(365.25, days, 1);
        }

        [Fact]
        public void ConvertTime_CenturyToYear()
        {
            double years = UnitConvertUtil.ConvertTime(1, UnitConvertUtil.TimeUnit.Century, UnitConvertUtil.TimeUnit.Year);
            Assert.Equal(100, years, 1);
        }

        [Fact]
        public void ConvertTime_ZeroValue_ReturnsZero()
        {
            Assert.Equal(0, UnitConvertUtil.ConvertTime(0, UnitConvertUtil.TimeUnit.Hour, UnitConvertUtil.TimeUnit.Minute));
        }

        #endregion

        #region Pressure

        [Fact]
        public void ConvertPressure_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertPressure(1.0, UnitConvertUtil.PressureUnit.Pascal, UnitConvertUtil.PressureUnit.Pascal));
        }

        [Fact]
        public void ConvertPressure_KilopascalToPascal()
        {
            Assert.Equal(1000, UnitConvertUtil.ConvertPressure(1, UnitConvertUtil.PressureUnit.Kilopascal, UnitConvertUtil.PressureUnit.Pascal));
        }

        [Fact]
        public void ConvertPressure_MegapascalToPascal()
        {
            Assert.Equal(1000000, UnitConvertUtil.ConvertPressure(1, UnitConvertUtil.PressureUnit.Megapascal, UnitConvertUtil.PressureUnit.Pascal));
        }

        [Fact]
        public void ConvertPressure_BarToPascal()
        {
            Assert.Equal(100000, UnitConvertUtil.ConvertPressure(1, UnitConvertUtil.PressureUnit.Bar, UnitConvertUtil.PressureUnit.Pascal));
        }

        [Fact]
        public void ConvertPressure_BarToKilopascal()
        {
            Assert.Equal(100, UnitConvertUtil.ConvertPressure(1, UnitConvertUtil.PressureUnit.Bar, UnitConvertUtil.PressureUnit.Kilopascal));
        }

        [Fact]
        public void ConvertPressure_AtmToPascal()
        {
            Assert.Equal(101325, UnitConvertUtil.ConvertPressure(1, UnitConvertUtil.PressureUnit.Atm, UnitConvertUtil.PressureUnit.Pascal));
        }

        [Fact]
        public void ConvertPressure_PsiToPascal()
        {
            double pa = UnitConvertUtil.ConvertPressure(1, UnitConvertUtil.PressureUnit.Psi, UnitConvertUtil.PressureUnit.Pascal);
            Assert.Equal(6894.757293168, pa, 5);
        }

        [Fact]
        public void ConvertPressure_TorrToPascal()
        {
            double pa = UnitConvertUtil.ConvertPressure(1, UnitConvertUtil.PressureUnit.Torr, UnitConvertUtil.PressureUnit.Pascal);
            Assert.Equal(133.3223684211, pa, 5);
        }

        [Fact]
        public void ConvertPressure_ZeroValue_ReturnsZero()
        {
            Assert.Equal(0, UnitConvertUtil.ConvertPressure(0, UnitConvertUtil.PressureUnit.Bar, UnitConvertUtil.PressureUnit.Pascal));
        }

        #endregion

        #region Angle

        [Fact]
        public void ConvertAngle_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertAngle(1.0, UnitConvertUtil.AngleUnit.Degree, UnitConvertUtil.AngleUnit.Degree));
        }

        [Fact]
        public void ConvertAngle_DegreeToRadian()
        {
            // 180 degrees = PI radians
            double rad = UnitConvertUtil.ConvertAngle(180, UnitConvertUtil.AngleUnit.Degree, UnitConvertUtil.AngleUnit.Radian);
            Assert.Equal(Math.PI, rad, 10);
        }

        [Fact]
        public void ConvertAngle_RadianToDegree()
        {
            // PI radians = 180 degrees
            double deg = UnitConvertUtil.ConvertAngle(Math.PI, UnitConvertUtil.AngleUnit.Radian, UnitConvertUtil.AngleUnit.Degree);
            Assert.Equal(180, deg, 10);
        }

        [Fact]
        public void ConvertAngle_DegreeToGradian()
        {
            // 100 gradian = 90 degrees
            double grad = UnitConvertUtil.ConvertAngle(90, UnitConvertUtil.AngleUnit.Degree, UnitConvertUtil.AngleUnit.Gradian);
            Assert.Equal(100, grad, 10);
        }

        [Fact]
        public void ConvertAngle_GradianToDegree()
        {
            // 200 gradian = 180 degrees
            double deg = UnitConvertUtil.ConvertAngle(200, UnitConvertUtil.AngleUnit.Gradian, UnitConvertUtil.AngleUnit.Degree);
            Assert.Equal(180, deg, 10);
        }

        [Fact]
        public void ConvertAngle_DegreeToTurn()
        {
            // 360 degrees = 1 turn
            double turn = UnitConvertUtil.ConvertAngle(360, UnitConvertUtil.AngleUnit.Degree, UnitConvertUtil.AngleUnit.Turn);
            Assert.Equal(1.0, turn, 10);
        }

        [Fact]
        public void ConvertAngle_TurnToDegree()
        {
            // 1 turn = 360 degrees
            double deg = UnitConvertUtil.ConvertAngle(1, UnitConvertUtil.AngleUnit.Turn, UnitConvertUtil.AngleUnit.Degree);
            Assert.Equal(360, deg, 10);
        }

        [Fact]
        public void ConvertAngle_RadianToTurn()
        {
            // 2*PI radians = 1 turn
            double turn = UnitConvertUtil.ConvertAngle(2 * Math.PI, UnitConvertUtil.AngleUnit.Radian, UnitConvertUtil.AngleUnit.Turn);
            Assert.Equal(1.0, turn, 10);
        }

        [Fact]
        public void ConvertAngle_ZeroValue_ReturnsZero()
        {
            Assert.Equal(0, UnitConvertUtil.ConvertAngle(0, UnitConvertUtil.AngleUnit.Degree, UnitConvertUtil.AngleUnit.Radian));
        }

        [Fact]
        public void ConvertAngle_FullCircle_DegreeToRadianToDegree()
        {
            double original = 45.0;
            double rad = UnitConvertUtil.ConvertAngle(original, UnitConvertUtil.AngleUnit.Degree, UnitConvertUtil.AngleUnit.Radian);
            double back = UnitConvertUtil.ConvertAngle(rad, UnitConvertUtil.AngleUnit.Radian, UnitConvertUtil.AngleUnit.Degree);

            Assert.Equal(original, back, 10);
        }

        #endregion

        #region Data

        [Fact]
        public void ConvertData_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertData(1.0, UnitConvertUtil.DataUnit.Byte, UnitConvertUtil.DataUnit.Byte));
        }

        [Fact]
        public void ConvertData_ByteToKilobyte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(1000, UnitConvertUtil.DataUnit.Byte, UnitConvertUtil.DataUnit.Kilobyte));
        }

        [Fact]
        public void ConvertData_KilobyteToMegabyte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(1000, UnitConvertUtil.DataUnit.Kilobyte, UnitConvertUtil.DataUnit.Megabyte));
        }

        [Fact]
        public void ConvertData_MegabyteToGigabyte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(1000, UnitConvertUtil.DataUnit.Megabyte, UnitConvertUtil.DataUnit.Gigabyte));
        }

        [Fact]
        public void ConvertData_GigabyteToTerabyte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(1000, UnitConvertUtil.DataUnit.Gigabyte, UnitConvertUtil.DataUnit.Terabyte));
        }

        [Fact]
        public void ConvertData_TerabyteToPetabyte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(1000, UnitConvertUtil.DataUnit.Terabyte, UnitConvertUtil.DataUnit.Petabyte));
        }

        [Fact]
        public void ConvertData_BitToByte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(8, UnitConvertUtil.DataUnit.Bit, UnitConvertUtil.DataUnit.Byte));
        }

        [Fact]
        public void ConvertData_ByteToKibibyte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(1024, UnitConvertUtil.DataUnit.Byte, UnitConvertUtil.DataUnit.Kibibyte));
        }

        [Fact]
        public void ConvertData_KibibyteToMebibyte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(1024, UnitConvertUtil.DataUnit.Kibibyte, UnitConvertUtil.DataUnit.Mebibyte));
        }

        [Fact]
        public void ConvertData_MebibyteToGibibyte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(1024, UnitConvertUtil.DataUnit.Mebibyte, UnitConvertUtil.DataUnit.Gibibyte));
        }

        [Fact]
        public void ConvertData_GibibyteToTebibyte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(1024, UnitConvertUtil.DataUnit.Gibibyte, UnitConvertUtil.DataUnit.Tebibyte));
        }

        [Fact]
        public void ConvertData_TebibyteToPebibyte()
        {
            Assert.Equal(1, UnitConvertUtil.ConvertData(1024, UnitConvertUtil.DataUnit.Tebibyte, UnitConvertUtil.DataUnit.Pebibyte));
        }

        [Fact]
        public void ConvertData_DecimalVsBinary()
        {
            // 1 KB (1000 bytes) vs 1 KiB (1024 bytes)
            double kb = UnitConvertUtil.ConvertData(1, UnitConvertUtil.DataUnit.Kilobyte, UnitConvertUtil.DataUnit.Byte);
            double kib = UnitConvertUtil.ConvertData(1, UnitConvertUtil.DataUnit.Kibibyte, UnitConvertUtil.DataUnit.Byte);

            Assert.Equal(1000, kb);
            Assert.Equal(1024, kib);
            Assert.True(kib > kb);
        }

        [Fact]
        public void ConvertData_ZeroValue_ReturnsZero()
        {
            Assert.Equal(0, UnitConvertUtil.ConvertData(0, UnitConvertUtil.DataUnit.Byte, UnitConvertUtil.DataUnit.Kilobyte));
        }

        [Fact]
        public void FormatDataSize_Bytes_FormatsCorrectly()
        {
            Assert.Equal("100.00 B", UnitConvertUtil.FormatDataSize(100));
        }

        [Fact]
        public void FormatDataSize_Kilobytes_FormatsCorrectly()
        {
            Assert.Equal("1.00 KB", UnitConvertUtil.FormatDataSize(1024));
        }

        [Fact]
        public void FormatDataSize_Megabytes_FormatsCorrectly()
        {
            Assert.Equal("1.00 MB", UnitConvertUtil.FormatDataSize(1024 * 1024));
        }

        [Fact]
        public void FormatDataSize_Gigabytes_FormatsCorrectly()
        {
            Assert.Equal("1.00 GB", UnitConvertUtil.FormatDataSize(1024L * 1024 * 1024));
        }

        [Fact]
        public void FormatDataSize_Terabytes_FormatsCorrectly()
        {
            Assert.Equal("1.00 TB", UnitConvertUtil.FormatDataSize(1024L * 1024 * 1024 * 1024));
        }

        [Fact]
        public void FormatDataSize_Petabytes_FormatsCorrectly()
        {
            Assert.Equal("1.00 PB", UnitConvertUtil.FormatDataSize(1024L * 1024 * 1024 * 1024 * 1024));
        }

        [Fact]
        public void FormatDataSize_ZeroBytes_FormatsCorrectly()
        {
            Assert.Equal("0.00 B", UnitConvertUtil.FormatDataSize(0));
        }

        [Fact]
        public void FormatDataSize_LessThanOneKB_FormatsAsBytes()
        {
            Assert.Equal("512.00 B", UnitConvertUtil.FormatDataSize(512));
        }

        [Fact]
        public void FormatDataSize_FractionalKB_FormatsCorrectly()
        {
            // 1536 bytes = 1.5 KB
            string result = UnitConvertUtil.FormatDataSize(1536);
            Assert.Equal("1.50 KB", result);
        }

        #endregion

        #region Energy

        [Fact]
        public void ConvertEnergy_SameUnit_ReturnsSameValue()
        {
            Assert.Equal(1.0, UnitConvertUtil.ConvertEnergy(1.0, UnitConvertUtil.EnergyUnit.Joule, UnitConvertUtil.EnergyUnit.Joule));
        }

        [Fact]
        public void ConvertEnergy_KilojouleToJoule()
        {
            Assert.Equal(1000, UnitConvertUtil.ConvertEnergy(1, UnitConvertUtil.EnergyUnit.Kilojoule, UnitConvertUtil.EnergyUnit.Joule));
        }

        [Fact]
        public void ConvertEnergy_MegajouleToJoule()
        {
            Assert.Equal(1000000, UnitConvertUtil.ConvertEnergy(1, UnitConvertUtil.EnergyUnit.Megajoule, UnitConvertUtil.EnergyUnit.Joule));
        }

        [Fact]
        public void ConvertEnergy_CalorieToJoule()
        {
            Assert.Equal(4.184, UnitConvertUtil.ConvertEnergy(1, UnitConvertUtil.EnergyUnit.Calorie, UnitConvertUtil.EnergyUnit.Joule), 5);
        }

        [Fact]
        public void ConvertEnergy_KilocalorieToJoule()
        {
            Assert.Equal(4184, UnitConvertUtil.ConvertEnergy(1, UnitConvertUtil.EnergyUnit.Kilocalorie, UnitConvertUtil.EnergyUnit.Joule));
        }

        [Fact]
        public void ConvertEnergy_WattHourToJoule()
        {
            Assert.Equal(3600, UnitConvertUtil.ConvertEnergy(1, UnitConvertUtil.EnergyUnit.WattHour, UnitConvertUtil.EnergyUnit.Joule));
        }

        [Fact]
        public void ConvertEnergy_KilowattHourToJoule()
        {
            Assert.Equal(3600000, UnitConvertUtil.ConvertEnergy(1, UnitConvertUtil.EnergyUnit.KilowattHour, UnitConvertUtil.EnergyUnit.Joule));
        }

        [Fact]
        public void ConvertEnergy_KilowattHourToKilocalorie()
        {
            double kcal = UnitConvertUtil.ConvertEnergy(1, UnitConvertUtil.EnergyUnit.KilowattHour, UnitConvertUtil.EnergyUnit.Kilocalorie);
            Assert.Equal(860.421, kcal, 2);
        }

        [Fact]
        public void ConvertEnergy_BtuToJoule()
        {
            Assert.Equal(1055.06, UnitConvertUtil.ConvertEnergy(1, UnitConvertUtil.EnergyUnit.BritishThermalUnit, UnitConvertUtil.EnergyUnit.Joule), 2);
        }

        [Fact]
        public void ConvertEnergy_ZeroValue_ReturnsZero()
        {
            Assert.Equal(0, UnitConvertUtil.ConvertEnergy(0, UnitConvertUtil.EnergyUnit.Joule, UnitConvertUtil.EnergyUnit.Kilocalorie));
        }

        #endregion
    }
}
