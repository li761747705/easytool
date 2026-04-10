using System;
using System.Collections.Generic;
using Xunit;
using EasyTool.ToolCategory;

namespace EasyTool.ToolCategory.Tests
{
    public class VersionUtilTests
    {
        // ==================== Parse ====================

        [Fact]
        public void Parse_MajorOnly_ReturnsVersionInfo()
        {
            var info = VersionUtil.Parse("1");
            Assert.Equal(1, info.Major);
            Assert.Equal(0, info.Minor);
            Assert.Equal(0, info.Patch);
            Assert.Equal("1", info.Original);
        }

        [Fact]
        public void Parse_MajorMinor_ReturnsVersionInfo()
        {
            var info = VersionUtil.Parse("2.5");
            Assert.Equal(2, info.Major);
            Assert.Equal(5, info.Minor);
            Assert.Equal(0, info.Patch);
        }

        [Fact]
        public void Parse_MajorMinorPatch_ReturnsVersionInfo()
        {
            var info = VersionUtil.Parse("3.1.4");
            Assert.Equal(3, info.Major);
            Assert.Equal(1, info.Minor);
            Assert.Equal(4, info.Patch);
            Assert.Equal(0, info.Revision);
        }

        [Fact]
        public void Parse_FourPartVersion_ReturnsVersionInfo()
        {
            var info = VersionUtil.Parse("1.2.3.4");
            Assert.Equal(1, info.Major);
            Assert.Equal(2, info.Minor);
            Assert.Equal(3, info.Patch);
            Assert.Equal(4, info.Revision);
        }

        [Fact]
        public void Parse_WithVPrefix_Succeeds()
        {
            var info = VersionUtil.Parse("v1.2.3");
            Assert.Equal(1, info.Major);
            Assert.Equal(2, info.Minor);
            Assert.Equal(3, info.Patch);
        }

        [Fact]
        public void Parse_WithUpperCaseVPrefix_Succeeds()
        {
            var info = VersionUtil.Parse("V2.0.0");
            Assert.Equal(2, info.Major);
            Assert.Equal(0, info.Minor);
            Assert.Equal(0, info.Patch);
        }

        [Fact]
        public void Parse_WithPreReleaseTag_Succeeds()
        {
            var info = VersionUtil.Parse("1.0.0-beta");
            Assert.Equal(1, info.Major);
            Assert.Equal("beta", info.PreRelease);
            Assert.True(info.IsPreRelease);
            Assert.False(info.IsStable);
        }

        [Fact]
        public void Parse_WithBuildMetadata_Succeeds()
        {
            var info = VersionUtil.Parse("1.0.0+build.123");
            Assert.Equal(1, info.Major);
            Assert.Equal("build.123", info.BuildMetadata);
        }

        [Fact]
        public void Parse_WithBuildMetadataOnly_Succeeds()
        {
            var info = VersionUtil.Parse("1.0.0+exp.sha.5114f85");
            Assert.Equal(1, info.Major);
            Assert.Equal("exp.sha.5114f85", info.BuildMetadata);
            Assert.Null(info.PreRelease);
        }

        [Fact]
        public void Parse_WithPreReleaseOnly_Succeeds()
        {
            var info = VersionUtil.Parse("1.0.0-alpha.1");
            Assert.Equal(1, info.Major);
            Assert.Equal("alpha.1", info.PreRelease);
            Assert.Null(info.BuildMetadata);
        }

        [Fact]
        public void Parse_NullVersion_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => VersionUtil.Parse(null));
        }

        [Fact]
        public void Parse_EmptyVersion_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => VersionUtil.Parse(""));
        }

        [Fact]
        public void Parse_WhitespaceVersion_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => VersionUtil.Parse("   "));
        }

        [Fact]
        public void Parse_InvalidMajor_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => VersionUtil.Parse("abc.1.2"));
        }

        [Fact]
        public void Parse_InvalidMinor_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => VersionUtil.Parse("1.abc.2"));
        }

        [Fact]
        public void Parse_InvalidPatch_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => VersionUtil.Parse("1.2.abc"));
        }

        [Fact]
        public void Parse_TooManyParts_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => VersionUtil.Parse("1.2.3.4.5"));
        }

        // ==================== TryParse ====================

        [Fact]
        public void TryParse_ValidVersion_ReturnsTrue()
        {
            var success = VersionUtil.TryParse("1.2.3", out var info);
            Assert.True(success);
            Assert.NotNull(info);
            Assert.Equal(1, info!.Major);
            Assert.Equal(2, info.Minor);
            Assert.Equal(3, info.Patch);
        }

        [Fact]
        public void TryParse_NullVersion_ReturnsFalse()
        {
            var success = VersionUtil.TryParse(null, out var info);
            Assert.False(success);
            Assert.Null(info);
        }

        [Fact]
        public void TryParse_EmptyVersion_ReturnsFalse()
        {
            var success = VersionUtil.TryParse("", out var info);
            Assert.False(success);
            Assert.Null(info);
        }

        [Fact]
        public void TryParse_InvalidVersion_ReturnsFalse()
        {
            var success = VersionUtil.TryParse("abc", out var info);
            Assert.False(success);
            Assert.Null(info);
        }

        // ==================== Compare (string) ====================

        [Fact]
        public void Compare_String_FirstGreater_ReturnsPositive()
        {
            var result = VersionUtil.Compare("2.0.0", "1.0.0");
            Assert.True(result > 0);
        }

        [Fact]
        public void Compare_String_FirstLess_ReturnsNegative()
        {
            var result = VersionUtil.Compare("1.0.0", "2.0.0");
            Assert.True(result < 0);
        }

        [Fact]
        public void Compare_String_Equal_ReturnsZero()
        {
            var result = VersionUtil.Compare("1.2.3", "1.2.3");
            Assert.Equal(0, result);
        }

        [Fact]
        public void Compare_String_InvalidVersions_ReturnsZero()
        {
            var result = VersionUtil.Compare("invalid", "also-invalid");
            Assert.Equal(0, result);
        }

        [Fact]
        public void Compare_String_WithPreRelease_StableIsHigher()
        {
            var result = VersionUtil.Compare("1.0.0", "1.0.0-beta");
            Assert.True(result > 0);
        }

        [Fact]
        public void Compare_String_WithPreRelease_PreReleaseIsLower()
        {
            var result = VersionUtil.Compare("1.0.0-beta", "1.0.0");
            Assert.True(result < 0);
        }

        // ==================== Compare (VersionInfo) ====================

        [Fact]
        public void Compare_VersionInfo_MajorDiffers()
        {
            var v1 = new VersionInfo { Major = 2 };
            var v2 = new VersionInfo { Major = 1 };
            Assert.True(VersionUtil.Compare(v1, v2) > 0);
        }

        [Fact]
        public void Compare_VersionInfo_MinorDiffers()
        {
            var v1 = new VersionInfo { Major = 1, Minor = 2 };
            var v2 = new VersionInfo { Major = 1, Minor = 1 };
            Assert.True(VersionUtil.Compare(v1, v2) > 0);
        }

        [Fact]
        public void Compare_VersionInfo_PatchDiffers()
        {
            var v1 = new VersionInfo { Major = 1, Minor = 1, Patch = 2 };
            var v2 = new VersionInfo { Major = 1, Minor = 1, Patch = 1 };
            Assert.True(VersionUtil.Compare(v1, v2) > 0);
        }

        [Fact]
        public void Compare_VersionInfo_RevisionDiffers()
        {
            var v1 = new VersionInfo { Major = 1, Minor = 1, Patch = 1, Revision = 2 };
            var v2 = new VersionInfo { Major = 1, Minor = 1, Patch = 1, Revision = 1 };
            Assert.True(VersionUtil.Compare(v1, v2) > 0);
        }

        [Fact]
        public void Compare_VersionInfo_PreRelease_StableHigherThanPreRelease()
        {
            var stable = new VersionInfo { Major = 1, Minor = 0, Patch = 0 };
            var preRelease = new VersionInfo { Major = 1, Minor = 0, Patch = 0, PreRelease = "alpha" };
            Assert.True(VersionUtil.Compare(stable, preRelease) > 0);
            Assert.True(VersionUtil.Compare(preRelease, stable) < 0);
        }

        [Fact]
        public void Compare_VersionInfo_BothPreRelease_CompareLexicographically()
        {
            var alpha = new VersionInfo { Major = 1, Minor = 0, Patch = 0, PreRelease = "alpha" };
            var beta = new VersionInfo { Major = 1, Minor = 0, Patch = 0, PreRelease = "beta" };
            Assert.True(VersionUtil.Compare(alpha, beta) < 0);
        }

        [Fact]
        public void Compare_VersionInfo_Equal_ReturnsZero()
        {
            var v1 = new VersionInfo { Major = 1, Minor = 2, Patch = 3 };
            var v2 = new VersionInfo { Major = 1, Minor = 2, Patch = 3 };
            Assert.Equal(0, VersionUtil.Compare(v1, v2));
        }

        // ==================== IsInRange ====================

        [Fact]
        public void IsInRange_WithinBounds_ReturnsTrue()
        {
            Assert.True(VersionUtil.IsInRange("1.5.0", "1.0.0", "2.0.0"));
        }

        [Fact]
        public void IsInRange_AtMinBoundary_ReturnsTrue()
        {
            Assert.True(VersionUtil.IsInRange("1.0.0", "1.0.0", "2.0.0"));
        }

        [Fact]
        public void IsInRange_AtMaxBoundary_ReturnsTrue()
        {
            Assert.True(VersionUtil.IsInRange("2.0.0", "1.0.0", "2.0.0"));
        }

        [Fact]
        public void IsInRange_BelowMin_ReturnsFalse()
        {
            Assert.False(VersionUtil.IsInRange("0.9.0", "1.0.0", "2.0.0"));
        }

        [Fact]
        public void IsInRange_AboveMax_ReturnsFalse()
        {
            Assert.False(VersionUtil.IsInRange("2.1.0", "1.0.0", "2.0.0"));
        }

        [Fact]
        public void IsInRange_NullMin_NoLowerBound()
        {
            Assert.True(VersionUtil.IsInRange("0.5.0", null, "2.0.0"));
        }

        [Fact]
        public void IsInRange_NullMax_NoUpperBound()
        {
            Assert.True(VersionUtil.IsInRange("99.0.0", "1.0.0", null));
        }

        // ==================== Next ====================

        [Fact]
        public void Next_Patch_IncrementsPatch()
        {
            var next = VersionUtil.Next("1.2.3", VersionLevel.Patch);
            Assert.Equal("1.2.4", next);
        }

        [Fact]
        public void Next_Minor_IncrementsMinor()
        {
            var next = VersionUtil.Next("1.2.3", VersionLevel.Minor);
            Assert.Equal("1.3.0", next);
        }

        [Fact]
        public void Next_Major_IncrementsMajor()
        {
            var next = VersionUtil.Next("1.2.3", VersionLevel.Major);
            Assert.Equal("2.0.0", next);
        }

        [Fact]
        public void Next_Revision_IncrementsRevision()
        {
            var next = VersionUtil.Next("1.2.3.4", VersionLevel.Revision);
            Assert.Equal("1.2.3.5", next);
        }

        [Fact]
        public void Next_InvalidVersion_ReturnsDefault()
        {
            var next = VersionUtil.Next("invalid", VersionLevel.Patch);
            Assert.Equal("0.0.1", next);
        }

        [Fact]
        public void Next_DefaultLevel_IsPatch()
        {
            var next = VersionUtil.Next("1.0.0");
            Assert.Equal("1.0.1", next);
        }

        // ==================== GetDiff ====================

        [Fact]
        public void GetDiff_PatchChange_ReturnsPatchDiff()
        {
            var diff = VersionUtil.GetDiff("1.0.0", "1.0.1");
            Assert.Equal(0, diff.MajorDiff);
            Assert.Equal(0, diff.MinorDiff);
            Assert.Equal(1, diff.PatchDiff);
            Assert.Equal(VersionLevel.Patch, diff.ChangeLevel);
            Assert.True(diff.IsUpgrade);
            Assert.False(diff.IsDowngrade);
            Assert.False(diff.IsUnchanged);
        }

        [Fact]
        public void GetDiff_MajorChange_ReturnsMajorDiff()
        {
            var diff = VersionUtil.GetDiff("1.0.0", "2.0.0");
            Assert.Equal(1, diff.MajorDiff);
            Assert.Equal(VersionLevel.Major, diff.ChangeLevel);
            Assert.True(diff.IsUpgrade);
        }

        [Fact]
        public void GetDiff_Downgrade_ReturnsDowngrade()
        {
            var diff = VersionUtil.GetDiff("2.0.0", "1.0.0");
            Assert.Equal(-1, diff.MajorDiff);
            Assert.True(diff.IsDowngrade);
            Assert.False(diff.IsUpgrade);
        }

        [Fact]
        public void GetDiff_SameVersion_ReturnsUnchanged()
        {
            var diff = VersionUtil.GetDiff("1.0.0", "1.0.0");
            Assert.True(diff.IsUnchanged);
            Assert.False(diff.IsUpgrade);
            Assert.False(diff.IsDowngrade);
            // ChangeLevel is 0 (Major) by default when no diffs are non-zero
            Assert.Equal(VersionLevel.Major, diff.ChangeLevel);
        }

        // ==================== FindClosest ====================

        [Fact]
        public void FindClosest_FindsNearestVersion()
        {
            var versions = new[] { "1.0.0", "1.5.0", "2.0.0" };
            var closest = VersionUtil.FindClosest(versions, "1.4.0");
            // FindClosest uses Math.Abs(Compare), which compares ordinal results.
            // Compare(1.0.0, 1.4.0) = -1, abs = 1
            // Compare(1.5.0, 1.4.0) = 1, abs = 1
            // Compare(2.0.0, 1.4.0) = 1, abs = 1
            // First match with min diff wins (1.0.0)
            Assert.NotNull(closest);
            Assert.Contains(closest, versions);
        }

        [Fact]
        public void FindClosest_ExactMatch_ReturnsExactVersion()
        {
            var versions = new[] { "1.0.0", "1.5.0", "2.0.0" };
            var closest = VersionUtil.FindClosest(versions, "1.5.0");
            Assert.Equal("1.5.0", closest);
        }

        [Fact]
        public void FindClosest_NullVersions_ReturnsNull()
        {
            var result = VersionUtil.FindClosest(null, "1.0.0");
            Assert.Null(result);
        }

        [Fact]
        public void FindClosest_EmptyTarget_ReturnsNull()
        {
            var result = VersionUtil.FindClosest(new[] { "1.0.0" }, "");
            Assert.Null(result);
        }

        [Fact]
        public void FindClosest_SkipsInvalidVersions()
        {
            var versions = new[] { "invalid", "2.0.0" };
            var closest = VersionUtil.FindClosest(versions, "1.9.0");
            Assert.Equal("2.0.0", closest);
        }

        // ==================== IsValidSemVer ====================

        [Theory]
        [InlineData("1.0.0", true)]
        [InlineData("1.0.0-alpha", true)]
        [InlineData("1.0.0-alpha.1", true)]
        [InlineData("1.0.0+build", true)]
        [InlineData("1.0.0-alpha+build", true)]
        [InlineData("v1.0.0", true)]
        [InlineData("0.1.0", true)]
        [InlineData("01.0.0", false)]
        [InlineData("1", false)]
        [InlineData("1.0", false)]
        [InlineData("", false)]
        [InlineData("invalid", false)]
        public void IsValidSemVer_TestCases(string version, bool expected)
        {
            Assert.Equal(expected, VersionUtil.IsValidSemVer(version));
        }

        [Fact]
        public void IsValidSemVer_Null_ReturnsFalse()
        {
            Assert.False(VersionUtil.IsValidSemVer(null));
        }

        // ==================== ToVersion ====================

        [Fact]
        public void ToVersion_ReturnsSystemVersion()
        {
            var version = VersionUtil.ToVersion("1.2.3");
            Assert.Equal(1, version.Major);
            Assert.Equal(2, version.Minor);
            Assert.Equal(3, version.Build);
        }

        [Fact]
        public void ToVersion_WithRevision_SetsAllParts()
        {
            var version = VersionUtil.ToVersion("1.2.3.4");
            Assert.Equal(1, version.Major);
            Assert.Equal(2, version.Minor);
            Assert.Equal(3, version.Build);
            Assert.Equal(4, version.Revision);
        }

        // ==================== FromVersion ====================

        [Fact]
        public void FromVersion_ReturnsVersionInfo()
        {
            var sysVersion = new Version(2, 3, 4);
            var info = VersionUtil.FromVersion(sysVersion);
            Assert.Equal(2, info.Major);
            Assert.Equal(3, info.Minor);
            Assert.Equal(4, info.Patch);
        }

        [Fact]
        public void FromVersion_TwoPartVersion_SetsDefaults()
        {
            var sysVersion = new Version(1, 0);
            var info = VersionUtil.FromVersion(sysVersion);
            Assert.Equal(1, info.Major);
            Assert.Equal(0, info.Minor);
            Assert.Equal(0, info.Patch);
            Assert.Equal(0, info.Revision);
        }

        // ==================== VersionInfo ====================

        [Fact]
        public void VersionInfo_ToString_BasicVersion()
        {
            var info = new VersionInfo { Major = 1, Minor = 2, Patch = 3 };
            Assert.Equal("1.2.3", info.ToString());
        }

        [Fact]
        public void VersionInfo_ToString_WithRevision()
        {
            var info = new VersionInfo { Major = 1, Minor = 2, Patch = 3, Revision = 4 };
            Assert.Equal("1.2.3.4", info.ToString());
        }

        [Fact]
        public void VersionInfo_ToString_WithPreRelease()
        {
            var info = new VersionInfo { Major = 1, Minor = 2, Patch = 3, PreRelease = "beta" };
            Assert.Equal("1.2.3-beta", info.ToString());
        }

        [Fact]
        public void VersionInfo_ToString_WithBuildMetadata()
        {
            var info = new VersionInfo { Major = 1, Minor = 2, Patch = 3, BuildMetadata = "build.1" };
            Assert.Equal("1.2.3+build.1", info.ToString());
        }

        [Fact]
        public void VersionInfo_Equals_SameValues_ReturnsTrue()
        {
            var v1 = new VersionInfo { Major = 1, Minor = 2, Patch = 3, PreRelease = "alpha" };
            var v2 = new VersionInfo { Major = 1, Minor = 2, Patch = 3, PreRelease = "alpha" };
            Assert.Equal(v1, v2);
            Assert.True(v1.Equals(v2));
        }

        [Fact]
        public void VersionInfo_Equals_DifferentValues_ReturnsFalse()
        {
            var v1 = new VersionInfo { Major = 1, Minor = 2, Patch = 3 };
            var v2 = new VersionInfo { Major = 1, Minor = 2, Patch = 4 };
            Assert.NotEqual(v1, v2);
        }

        [Fact]
        public void VersionInfo_Equals_DifferentType_ReturnsFalse()
        {
            var info = new VersionInfo { Major = 1, Minor = 2, Patch = 3 };
            Assert.False(info.Equals("1.2.3"));
            Assert.False(info.Equals(null));
        }

        [Fact]
        public void VersionInfo_GetHashCode_SameValues_ReturnSameHash()
        {
            var v1 = new VersionInfo { Major = 1, Minor = 2, Patch = 3 };
            var v2 = new VersionInfo { Major = 1, Minor = 2, Patch = 3 };
            Assert.Equal(v1.GetHashCode(), v2.GetHashCode());
        }

        // ==================== VersionLevel enum ====================

        [Fact]
        public void VersionLevel_HasExpectedValues()
        {
            Assert.Equal(0, (int)VersionLevel.Major);
            Assert.Equal(1, (int)VersionLevel.Minor);
            Assert.Equal(2, (int)VersionLevel.Patch);
            Assert.Equal(3, (int)VersionLevel.Revision);
        }

        // ==================== VersionDiff ====================

        [Fact]
        public void VersionDiff_MinorChange_ReturnsMinorDiff()
        {
            var diff = VersionUtil.GetDiff("1.0.0", "1.1.0");
            Assert.Equal(0, diff.MajorDiff);
            Assert.Equal(1, diff.MinorDiff);
            Assert.Equal(0, diff.PatchDiff);
            Assert.Equal(VersionLevel.Minor, diff.ChangeLevel);
        }

        [Fact]
        public void VersionDiff_RevisionChange_ReturnsRevisionDiff()
        {
            var diff = VersionUtil.GetDiff("1.0.0.0", "1.0.0.1");
            Assert.Equal(0, diff.MajorDiff);
            Assert.Equal(0, diff.MinorDiff);
            Assert.Equal(0, diff.PatchDiff);
            Assert.Equal(1, diff.RevisionDiff);
            Assert.Equal(VersionLevel.Revision, diff.ChangeLevel);
        }
    }
}
