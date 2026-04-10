using Xunit;
using EasyTool.MathCategory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.Tests
{
    public class MathUtilTests
    {
        #region Average Tests

        [Fact]
        public void Average_EmptyCollection_ReturnsZero()
        {
            var result = MathUtil.Average(Enumerable.Empty<double>());
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Average_SingleValue_ReturnsThatValue()
        {
            var result = MathUtil.Average(new[] { 5.0 });
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void Average_MultipleValues_ReturnsCorrectAverage()
        {
            var result = MathUtil.Average(new[] { 2.0, 4.0, 6.0, 8.0 });
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void Average_NegativeNumbers_ReturnsCorrectAverage()
        {
            var result = MathUtil.Average(new[] { -2.0, 2.0, -4.0, 4.0 });
            Assert.Equal(0.0, result);
        }

        #endregion

        #region StandardDeviation Tests

        [Fact]
        public void StandardDeviation_EmptyCollection_ReturnsZero()
        {
            var result = MathUtil.StandardDeviation(Enumerable.Empty<double>());
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void StandardDeviation_SameValues_ReturnsZero()
        {
            var result = MathUtil.StandardDeviation(new[] { 5.0, 5.0, 5.0, 5.0 });
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void StandardDeviation_NormalDistribution_ReturnsPositiveValue()
        {
            var result = MathUtil.StandardDeviation(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 });
            Assert.True(result > 0);
        }

        #endregion

        #region Variance Tests

        [Fact]
        public void Variance_EmptyCollection_ReturnsZero()
        {
            var result = MathUtil.Variance(Enumerable.Empty<double>());
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Variance_SameValues_ReturnsZero()
        {
            var result = MathUtil.Variance(new[] { 5.0, 5.0, 5.0 });
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Variance_DifferentValues_ReturnsPositiveValue()
        {
            var result = MathUtil.Variance(new[] { 1.0, 2.0, 3.0 });
            Assert.True(result > 0);
        }

        #endregion

        #region Median Tests

        [Fact]
        public void Median_EmptyCollection_ReturnsZero()
        {
            var result = MathUtil.Median(Enumerable.Empty<double>());
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Median_SingleValue_ReturnsThatValue()
        {
            var result = MathUtil.Median(new[] { 5.0 });
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void Median_OddCount_ReturnsMiddleValue()
        {
            var result = MathUtil.Median(new[] { 1.0, 3.0, 5.0 });
            Assert.Equal(3.0, result);
        }

        [Fact]
        public void Median_EvenCount_ReturnsAverageOfMiddleValues()
        {
            var result = MathUtil.Median(new[] { 1.0, 2.0, 3.0, 4.0 });
            Assert.Equal(2.5, result);
        }

        [Fact]
        public void Median_UnsortedList_ReturnsCorrectMedian()
        {
            var result = MathUtil.Median(new[] { 5.0, 1.0, 3.0, 2.0, 4.0 });
            Assert.Equal(3.0, result);
        }

        #endregion

        #region Mode Tests

        [Fact]
        public void Mode_EmptyCollection_ReturnsEmptyList()
        {
            var result = MathUtil.Mode(Enumerable.Empty<double>());
            Assert.Empty(result);
        }

        [Fact]
        public void Mode_SingleMode_ReturnsThatValue()
        {
            var result = MathUtil.Mode(new[] { 1.0, 2.0, 2.0, 3.0 });
            Assert.Single(result);
            Assert.Contains(2.0, result);
        }

        [Fact]
        public void Mode_MultipleModes_ReturnsAllModes()
        {
            var result = MathUtil.Mode(new[] { 1.0, 1.0, 2.0, 2.0, 3.0 });
            Assert.Equal(2, result.Count);
            Assert.Contains(1.0, result);
            Assert.Contains(2.0, result);
        }

        [Fact]
        public void Mode_AllUnique_ReturnsAllValues()
        {
            var result = MathUtil.Mode(new[] { 1.0, 2.0, 3.0 });
            Assert.Equal(3, result.Count);
        }

        #endregion

        #region Percentile Tests

        [Fact]
        public void Percentile_EmptyCollection_ReturnsZero()
        {
            var result = MathUtil.Percentile(Enumerable.Empty<double>(), 50);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Percentile_ZeroPercentile_ReturnsMinimum()
        {
            var result = MathUtil.Percentile(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 0);
            Assert.Equal(1.0, result);
        }

        [Fact]
        public void Percentile_HundredPercentile_ReturnsMaximum()
        {
            var result = MathUtil.Percentile(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 100);
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void Percentile_FiftiethPercentile_ReturnsMedian()
        {
            var result = MathUtil.Percentile(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 50);
            Assert.Equal(3.0, result);
        }

        #endregion

        #region Clamp Tests

        [Fact]
        public void Clamp_ValueInRange_ReturnsValue()
        {
            var result = MathUtil.Clamp(5.0, 0.0, 10.0);
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void Clamp_ValueBelowMin_ReturnsMin()
        {
            var result = MathUtil.Clamp(-5.0, 0.0, 10.0);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Clamp_ValueAboveMax_ReturnsMax()
        {
            var result = MathUtil.Clamp(15.0, 0.0, 10.0);
            Assert.Equal(10.0, result);
        }

        [Fact]
        public void Clamp_AtMinBoundary_ReturnsMin()
        {
            var result = MathUtil.Clamp(0.0, 0.0, 10.0);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Clamp_AtMaxBoundary_ReturnsMax()
        {
            var result = MathUtil.Clamp(10.0, 0.0, 10.0);
            Assert.Equal(10.0, result);
        }

        #endregion

        #region Lerp Tests

        [Fact]
        public void Lerp_ZeroT_ReturnsA()
        {
            var result = MathUtil.Lerp(10.0, 20.0, 0.0);
            Assert.Equal(10.0, result);
        }

        [Fact]
        public void Lerp_OneT_ReturnsB()
        {
            var result = MathUtil.Lerp(10.0, 20.0, 1.0);
            Assert.Equal(20.0, result);
        }

        [Fact]
        public void Lerp_HalfT_ReturnsMidpoint()
        {
            var result = MathUtil.Lerp(10.0, 20.0, 0.5);
            Assert.Equal(15.0, result);
        }

        [Fact]
        public void Lerp_TBelowZero_ClampsToA()
        {
            var result = MathUtil.Lerp(10.0, 20.0, -0.5);
            Assert.Equal(10.0, result);
        }

        [Fact]
        public void Lerp_TAboveOne_ClampsToB()
        {
            var result = MathUtil.Lerp(10.0, 20.0, 1.5);
            Assert.Equal(20.0, result);
        }

        #endregion

        #region InverseLerp Tests

        [Fact]
        public void InverseLerp_ValueAtA_ReturnsZero()
        {
            var result = MathUtil.InverseLerp(10.0, 20.0, 10.0);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void InverseLerp_ValueAtB_ReturnsOne()
        {
            var result = MathUtil.InverseLerp(10.0, 20.0, 20.0);
            Assert.Equal(1.0, result);
        }

        [Fact]
        public void InverseLerp_ValueMidpoint_ReturnsHalf()
        {
            var result = MathUtil.InverseLerp(10.0, 20.0, 15.0);
            Assert.Equal(0.5, result);
        }

        [Fact]
        public void InverseLerp_SameRange_ReturnsZero()
        {
            var result = MathUtil.InverseLerp(10.0, 10.0, 15.0);
            Assert.Equal(0.0, result);
        }

        #endregion

        #region Remap Tests

        [Fact]
        public void Remap_ValueInFirstRange_ReturnsValueInSecondRange()
        {
            var result = MathUtil.Remap(5.0, 0.0, 10.0, 0.0, 100.0);
            Assert.Equal(50.0, result);
        }

        [Fact]
        public void Remap_MinValue_ReturnsMinOfNewRange()
        {
            var result = MathUtil.Remap(0.0, 0.0, 10.0, 0.0, 100.0);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Remap_MaxValue_ReturnsMaxOfNewRange()
        {
            var result = MathUtil.Remap(10.0, 0.0, 10.0, 0.0, 100.0);
            Assert.Equal(100.0, result);
        }

        [Fact]
        public void Remap_NegativeToPositive_WorksCorrectly()
        {
            var result = MathUtil.Remap(0.0, -10.0, 10.0, 0.0, 1.0);
            Assert.Equal(0.5, result);
        }

        #endregion

        #region GCD Tests

        [Fact]
        public void GcdTest()
        {
            var result = MathUtil.Gcd(5, 20);
            Assert.Equal(5, result);
        }

        [Fact]
        public void Gcd_CoprimeNumbers_ReturnsOne()
        {
            var result = MathUtil.Gcd(7, 13);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Gcd_OneNumberZero_ReturnsOtherNumber()
        {
            var result = MathUtil.Gcd(0, 5);
            Assert.Equal(5, result);
        }

        [Fact]
        public void Gcd_BothZeros_ReturnsZero()
        {
            var result = MathUtil.Gcd(0, 0);
            Assert.Equal(0, result);
        }

        [Fact]
        public void Gcd_NegativeNumbers_ReturnsPositiveGcd()
        {
            var result = MathUtil.Gcd(-12, -18);
            Assert.Equal(6, result);
        }

        [Fact]
        public void Gcd_AliasMethod_ReturnsSameResult()
        {
            var result1 = MathUtil.GCD(12, 18);
            var result2 = MathUtil.Gcd(12, 18);
            Assert.Equal(result1, result2);
        }

        #endregion

        #region LCM Tests

        [Fact]
        public void Lcm_SimpleNumbers_ReturnsCorrectLcm()
        {
            var result = MathUtil.Lcm(4, 6);
            Assert.Equal(12, result);
        }

        [Fact]
        public void Lcm_OneNumberZero_ReturnsZero()
        {
            var result = MathUtil.Lcm(0, 5);
            Assert.Equal(0, result);
        }

        [Fact]
        public void Lcm_CoprimeNumbers_ReturnsProduct()
        {
            var result = MathUtil.Lcm(7, 13);
            Assert.Equal(91, result);
        }

        [Fact]
        public void Lcm_AliasMethod_ReturnsSameResult()
        {
            var result1 = MathUtil.LCM(4, 6);
            var result2 = MathUtil.Lcm(4, 6);
            Assert.Equal(result1, result2);
        }

        #endregion

        #region IsPrime Tests

        [Fact]
        public void IsPrime_LessThanTwo_ReturnsFalse()
        {
            Assert.False(MathUtil.IsPrime(0));
            Assert.False(MathUtil.IsPrime(1));
            Assert.False(MathUtil.IsPrime(-5));
        }

        [Fact]
        public void IsPrime_Two_ReturnsTrue()
        {
            Assert.True(MathUtil.IsPrime(2));
        }

        [Fact]
        public void IsPrime_EvenNumberGreaterThanTwo_ReturnsFalse()
        {
            Assert.False(MathUtil.IsPrime(4));
            Assert.False(MathUtil.IsPrime(100));
        }

        [Fact]
        public void IsPrime_OddPrime_ReturnsTrue()
        {
            Assert.True(MathUtil.IsPrime(3));
            Assert.True(MathUtil.IsPrime(7));
            Assert.True(MathUtil.IsPrime(97));
        }

        [Fact]
        public void IsPrime_OddComposite_ReturnsFalse()
        {
            Assert.False(MathUtil.IsPrime(9));
            Assert.False(MathUtil.IsPrime(15));
            Assert.False(MathUtil.IsPrime(100));
        }

        #endregion

        #region GetPrimeFactors Tests

        [Fact]
        public void GetPrimeFactors_One_ReturnsEmptyList()
        {
            var result = MathUtil.GetPrimeFactors(1);
            Assert.Empty(result);
        }

        [Fact]
        public void GetPrimeFactors_PrimeNumber_ReturnsSingleFactor()
        {
            var result = MathUtil.GetPrimeFactors(7);
            Assert.Single(result);
            Assert.Contains(7L, result);
        }

        [Fact]
        public void GetPrimeFactors_CompositeNumber_ReturnsAllFactors()
        {
            var result = MathUtil.GetPrimeFactors(12);
            Assert.Equal(3, result.Count);
            Assert.Contains(2L, result);
            Assert.Contains(3L, result);
        }

        [Fact]
        public void GetPrimeFactors_LargePower_ReturnsMultipleSameFactors()
        {
            var result = MathUtil.GetPrimeFactors(8);
            Assert.Equal(3, result.Count);
            Assert.All(result, factor => Assert.Equal(2L, factor));
        }

        [Fact]
        public void GetPrimeFactors_NegativeNumber_ReturnsFactorsOfAbsoluteValue()
        {
            var result = MathUtil.GetPrimeFactors(-12);
            Assert.Equal(3, result.Count);
        }

        #endregion

        #region Factorial Tests

        [Fact]
        public void Factorial_Zero_ReturnsOne()
        {
            var result = MathUtil.Factorial(0);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Factorial_One_ReturnsOne()
        {
            var result = MathUtil.Factorial(1);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Factorial_Five_Returns120()
        {
            var result = MathUtil.Factorial(5);
            Assert.Equal(120, result);
        }

        [Fact]
        public void Factorial_NegativeNumber_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => MathUtil.Factorial(-1));
        }

        #endregion

        #region Permutation Tests

        [Fact]
        public void Permutation_ZeroM_ReturnsOne()
        {
            var result = MathUtil.Permutation(5, 0);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Permutation_MGreaterThanN_ReturnsZero()
        {
            var result = MathUtil.Permutation(3, 5);
            Assert.Equal(0, result);
        }

        [Fact]
        public void Permutation_SimpleCase_ReturnsCorrectValue()
        {
            var result = MathUtil.Permutation(5, 3);
            Assert.Equal(60, result);
        }

        [Fact]
        public void Permutation_FullPermutation_ReturnsFactorial()
        {
            var result = MathUtil.Permutation(5, 5);
            Assert.Equal(120, result);
        }

        #endregion

        #region Combination Tests

        [Fact]
        public void Combination_ZeroM_ReturnsOne()
        {
            var result = MathUtil.Combination(5, 0);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Combination_MEqualsN_ReturnsOne()
        {
            var result = MathUtil.Combination(5, 5);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Combination_MGreaterThanN_ReturnsZero()
        {
            var result = MathUtil.Combination(3, 5);
            Assert.Equal(0, result);
        }

        [Fact]
        public void Combination_SimpleCase_ReturnsCorrectValue()
        {
            var result = MathUtil.Combination(5, 2);
            Assert.Equal(10, result);
        }

        [Fact]
        public void Combination_SymmetricValues_ReturnsSameResult()
        {
            var result1 = MathUtil.Combination(10, 3);
            var result2 = MathUtil.Combination(10, 7);
            Assert.Equal(result1, result2);
        }

        #endregion

        #region Fibonacci Tests

        [Fact]
        public void Fibonacci_Zero_ReturnsZero()
        {
            var result = MathUtil.Fibonacci(0);
            Assert.Equal(0, result);
        }

        [Fact]
        public void Fibonacci_One_ReturnsOne()
        {
            var result = MathUtil.Fibonacci(1);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Fibonacci_Ten_Returns55()
        {
            var result = MathUtil.Fibonacci(10);
            Assert.Equal(55, result);
        }

        [Fact]
        public void Fibonacci_NegativeNumber_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => MathUtil.Fibonacci(-1));
        }

        #endregion

        #region InRange Tests

        [Fact]
        public void InRange_ValueInRange_ReturnsTrue()
        {
            var result = MathUtil.InRange(5.0, 0.0, 10.0);
            Assert.True(result);
        }

        [Fact]
        public void InRange_ValueAtMinBoundary_ReturnsTrue()
        {
            var result = MathUtil.InRange(0.0, 0.0, 10.0);
            Assert.True(result);
        }

        [Fact]
        public void InRange_ValueAtMaxBoundary_ReturnsTrue()
        {
            var result = MathUtil.InRange(10.0, 0.0, 10.0);
            Assert.True(result);
        }

        [Fact]
        public void InRange_ValueBelowMin_ReturnsFalse()
        {
            var result = MathUtil.InRange(-1.0, 0.0, 10.0);
            Assert.False(result);
        }

        [Fact]
        public void InRange_ValueAboveMax_ReturnsFalse()
        {
            var result = MathUtil.InRange(11.0, 0.0, 10.0);
            Assert.False(result);
        }

        #endregion

        #region Approximately Tests

        [Fact]
        public void Approximately_EqualValues_ReturnsTrue()
        {
            var result = MathUtil.Approximately(1.0, 1.0);
            Assert.True(result);
        }

        [Fact]
        public void Approximately_VeryCloseValues_ReturnsTrue()
        {
            var result = MathUtil.Approximately(1.0, 1.00000000001);
            Assert.True(result);
        }

        [Fact]
        public void Approximately_DifferentValues_ReturnsFalse()
        {
            var result = MathUtil.Approximately(1.0, 2.0);
            Assert.False(result);
        }

        [Fact]
        public void Approximately_CustomEpsilon_UsesSpecifiedEpsilon()
        {
            var result = MathUtil.Approximately(1.0, 1.01, 0.1);
            Assert.True(result);
        }

        #endregion

        #region Distance Tests

        [Fact]
        public void Distance_SamePoint_ReturnsZero()
        {
            var result = MathUtil.Distance(0.0, 0.0, 0.0, 0.0);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Distance_HorizontalLine_ReturnsCorrectDistance()
        {
            var result = MathUtil.Distance(0.0, 0.0, 5.0, 0.0);
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void Distance_VerticalLine_ReturnsCorrectDistance()
        {
            var result = MathUtil.Distance(0.0, 0.0, 0.0, 5.0);
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void Distance_DiagonalLine_ReturnsCorrectDistance()
        {
            var result = MathUtil.Distance(0.0, 0.0, 3.0, 4.0);
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void Distance_NegativeCoordinates_ReturnsCorrectDistance()
        {
            var result = MathUtil.Distance(-1.0, -1.0, 2.0, 3.0);
            Assert.Equal(5.0, result);
        }

        #endregion

        #region Angle Tests

        [Fact]
        public void Angle_SamePoint_ReturnsZero()
        {
            var result = MathUtil.Angle(0.0, 0.0, 0.0, 0.0);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Angle_HorizontalRight_ReturnsZero()
        {
            var result = MathUtil.Angle(0.0, 0.0, 1.0, 0.0);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void Angle_VerticalUp_ReturnsPiOverTwo()
        {
            var result = MathUtil.Angle(0.0, 0.0, 0.0, 1.0);
            AssertApproximately(Math.PI / 2, result);
        }

        [Fact]
        public void Angle_HorizontalLeft_ReturnsPi()
        {
            var result = MathUtil.Angle(0.0, 0.0, -1.0, 0.0);
            AssertApproximately(Math.PI, result);
        }

        #endregion

        #region ToDegrees Tests

        [Fact]
        public void ToDegrees_ZeroRadians_ReturnsZeroDegrees()
        {
            var result = MathUtil.ToDegrees(0.0);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void ToDegrees_Pi_Returns180()
        {
            var result = MathUtil.ToDegrees(Math.PI);
            Assert.Equal(180.0, result);
        }

        [Fact]
        public void ToDegrees_TwoPi_Returns360()
        {
            var result = MathUtil.ToDegrees(2 * Math.PI);
            Assert.Equal(360.0, result);
        }

        #endregion

        #region ToRadians Tests

        [Fact]
        public void ToRadians_ZeroDegrees_ReturnsZeroRadians()
        {
            var result = MathUtil.ToRadians(0.0);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void ToRadians_180_ReturnsPi()
        {
            var result = MathUtil.ToRadians(180.0);
            AssertApproximately(Math.PI, result);
        }

        [Fact]
        public void ToRadians_360_ReturnsTwoPi()
        {
            var result = MathUtil.ToRadians(360.0);
            AssertApproximately(2 * Math.PI, result);
        }

        #endregion

        // Helper method for approximate comparison
        private void AssertApproximately(double expected, double actual, double tolerance = 1e-10)
        {
            Assert.True(Math.Abs(expected - actual) < tolerance,
                $"Expected {expected} but got {actual} (difference: {Math.Abs(expected - actual)})");
        }
    }
}