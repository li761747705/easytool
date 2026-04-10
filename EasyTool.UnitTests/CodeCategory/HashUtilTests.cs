using Xunit;
using EasyTool.CodeCategory;
using System;

namespace EasyTool.CodeCategory.Tests
{
    public class HashUtilTests
    {
        [Fact]
        public void AdditiveHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.AdditiveHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void AdditiveHash_NullString_ReturnsZero()
        {
            var result = HashUtil.AdditiveHash(null);
            Assert.Equal(0u, result);
        }

        [Fact]
        public void AdditiveHash_SameInput_ReturnsSameHash()
        {
            var input = "test";
            var hash1 = HashUtil.AdditiveHash(input);
            var hash2 = HashUtil.AdditiveHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void AdditiveHash_DifferentInput_ReturnsDifferentHash()
        {
            var hash1 = HashUtil.AdditiveHash("test1");
            var hash2 = HashUtil.AdditiveHash("test2");
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void RotatingHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.RotatingHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void RotatingHash_ConsistentResults()
        {
            var input = "consistency";
            var hash1 = HashUtil.RotatingHash(input);
            var hash2 = HashUtil.RotatingHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void OneByOneHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.OneByOneHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void OneByOneHash_ConsistentResults()
        {
            var input = "onebyone";
            var hash1 = HashUtil.OneByOneHash(input);
            var hash2 = HashUtil.OneByOneHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void Bernstein_EmptyString_ReturnsZero()
        {
            var result = HashUtil.Bernstein("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void Bernstein_ConsistentResults()
        {
            var input = "bernstein";
            var hash1 = HashUtil.Bernstein(input);
            var hash2 = HashUtil.Bernstein(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void Universal_EmptyString_ReturnsZero()
        {
            var result = HashUtil.Universal("", 1009, 10, 5, 3);
            Assert.Equal(0u, result);
        }

        [Fact]
        public void Universal_ZeroPrime_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HashUtil.Universal("test", 0, 10, 5, 3));
        }

        [Fact]
        public void Universal_ZeroBuckets_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HashUtil.Universal("test", 1009, 0, 5, 3));
        }

        [Fact]
        public void Universal_ValidParameters_ReturnsHash()
        {
            var result = HashUtil.Universal("test", 1009, 10, 5, 3);
            Assert.True(result >= 0 && result < 10);
        }

        [Fact]
        public void Zobrist_EmptyString_ReturnsZero()
        {
            var table = new uint[] { 1, 2, 3, 4, 5 };
            var result = HashUtil.Zobrist("", table);
            Assert.Equal(0u, result);
        }

        [Fact]
        public void Zobrist_NullTable_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HashUtil.Zobrist("test", null));
        }

        [Fact]
        public void Zobrist_EmptyTable_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HashUtil.Zobrist("test", Array.Empty<uint>()));
        }

        [Fact]
        public void Zobrist_ValidInput_ReturnsHash()
        {
            var table = new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var result = HashUtil.Zobrist("test", table);
            // The result should be deterministic (same input = same hash)
            var result2 = HashUtil.Zobrist("test", table);
            Assert.Equal(result, result2);
        }

        [Fact]
        public void FnvHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.FnvHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void FnvHash_ConsistentResults()
        {
            var input = "fnv";
            var hash1 = HashUtil.FnvHash(input);
            var hash2 = HashUtil.FnvHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void IntHash_ConsistentResults()
        {
            var key = 12345u;
            var hash1 = HashUtil.IntHash(key);
            var hash2 = HashUtil.IntHash(key);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void IntHash_DifferentInput_ReturnsDifferentHash()
        {
            var hash1 = HashUtil.IntHash(12345u);
            var hash2 = HashUtil.IntHash(54321u);
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void RsHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.RsHash("", 255, 131);
            Assert.Equal(0u, result);
        }

        [Fact]
        public void RsHash_ZeroB_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HashUtil.RsHash("test", 0, 131));
        }

        [Fact]
        public void RsHash_ValidParameters_ReturnsHash()
        {
            var result = HashUtil.RsHash("test", 255, 131);
            Assert.NotEqual(0u, result);
        }

        [Fact]
        public void JsHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.JsHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void JsHash_ConsistentResults()
        {
            var input = "jshash";
            var hash1 = HashUtil.JsHash(input);
            var hash2 = HashUtil.JsHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void PjwHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.PjwHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void PjwHash_ConsistentResults()
        {
            var input = "pjwhash";
            var hash1 = HashUtil.PjwHash(input);
            var hash2 = HashUtil.PjwHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void ElfHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.ElfHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void ElfHash_ConsistentResults()
        {
            var input = "elfhash";
            var hash1 = HashUtil.ElfHash(input);
            var hash2 = HashUtil.ElfHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void BkdrHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.BkdrHash("", 131);
            Assert.Equal(0u, result);
        }

        [Fact]
        public void BkdrHash_ZeroSeed_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HashUtil.BkdrHash("test", 0));
        }

        [Fact]
        public void BkdrHash_ValidParameters_ReturnsHash()
        {
            var result = HashUtil.BkdrHash("test", 131);
            Assert.NotEqual(0u, result);
        }

        [Fact]
        public void SdbmHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.SdbmHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void SdbmHash_ConsistentResults()
        {
            var input = "sdbm";
            var hash1 = HashUtil.SdbmHash(input);
            var hash2 = HashUtil.SdbmHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void DjbHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.DjbHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void DjbHash_ConsistentResults()
        {
            var input = "djbhash";
            var hash1 = HashUtil.DjbHash(input);
            var hash2 = HashUtil.DjbHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void DekHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.DekHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void DekHash_ConsistentResults()
        {
            var input = "dekhash";
            var hash1 = HashUtil.DekHash(input);
            var hash2 = HashUtil.DekHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void ApHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.ApHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void ApHash_ConsistentResults()
        {
            var input = "aphash";
            var hash1 = HashUtil.ApHash(input);
            var hash2 = HashUtil.ApHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void TianlHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.TianlHash("", 100);
            Assert.Equal(0u, result);
        }

        [Fact]
        public void TianlHash_ZeroLength_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => HashUtil.TianlHash("test", 0));
        }

        [Fact]
        public void TianlHash_ValidParameters_ReturnsHash()
        {
            var result = HashUtil.TianlHash("test", 100);
            Assert.True(result >= 0 && result < 100);
        }

        [Fact]
        public void JavaDefaultHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.JavaDefaultHash("");
            Assert.Equal(0u, result);
        }

        [Fact]
        public void JavaDefaultHash_ConsistentResults()
        {
            var input = "javahash";
            var hash1 = HashUtil.JavaDefaultHash(input);
            var hash2 = HashUtil.JavaDefaultHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void MixHash_EmptyString_ReturnsZero()
        {
            var result = HashUtil.MixHash("");
            Assert.Equal(0ul, result);
        }

        [Fact]
        public void MixHash_ConsistentResults()
        {
            var input = "mixhash";
            var hash1 = HashUtil.MixHash(input);
            var hash2 = HashUtil.MixHash(input);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void MixHash_DifferentInput_ReturnsDifferentHash()
        {
            var hash1 = HashUtil.MixHash("test1");
            var hash2 = HashUtil.MixHash("test2");
            Assert.NotEqual(hash1, hash2);
        }
    }
}
