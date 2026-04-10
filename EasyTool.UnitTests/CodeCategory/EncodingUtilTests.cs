using Xunit;
using EasyTool.CodeCategory;
using System;
using System.Linq;
using System.Text;

namespace EasyTool.CodeCategory.Tests
{
    public class EncodingUtilTests
    {
        #region Base32 Tests

        [Fact]
        public void Base32Encode_EmptyArray_ReturnsEmptyString()
        {
            var result = EncodingUtil.Base32Encode(Array.Empty<byte>());
            Assert.Equal("", result);
        }

        [Fact]
        public void Base32Encode_NullArray_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => EncodingUtil.Base32Encode(null));
        }

        [Fact]
        public void Base32Encode_SimpleString_ReturnsEncodedString()
        {
            var input = Encoding.UTF8.GetBytes("Hello");
            var encoded = EncodingUtil.Base32Encode(input);
            Assert.NotNull(encoded);
            Assert.NotEmpty(encoded);
        }

        [Fact]
        public void Base32Encode_SpecialCharacters_ReturnsEncodedString()
        {
            var input = Encoding.UTF8.GetBytes("测试@#$%");
            var encoded = EncodingUtil.Base32Encode(input);
            Assert.NotNull(encoded);
            Assert.NotEmpty(encoded);
        }

        [Fact]
        public void Base32Encode_MultipleBytes_ReturnsEncodedString()
        {
            var input = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var encoded = EncodingUtil.Base32Encode(input);
            Assert.NotNull(encoded);
            Assert.NotEmpty(encoded);
        }

        [Fact]
        public void Base32Decode_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EncodingUtil.Base32Decode(""));
        }

        [Fact]
        public void Base32Decode_InvalidLength_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EncodingUtil.Base32Decode("INVALID"));
        }

        [Fact]
        public void Base32Decode_InvalidCharacter_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EncodingUtil.Base32Decode("A======="));
        }

        [Fact]
        public void Base32Encode_SameInput_SameOutput()
        {
            var input = Encoding.UTF8.GetBytes("consistent");
            var encoded1 = EncodingUtil.Base32Encode(input);
            var encoded2 = EncodingUtil.Base32Encode(input);
            Assert.Equal(encoded1, encoded2);
        }

        [Fact]
        public void Base32Encode_SimpleString_Roundtrip()
        {
            var original = Encoding.UTF8.GetBytes("Hello");
            var encoded = EncodingUtil.Base32Encode(original);
            var decoded = EncodingUtil.Base32Decode(encoded);
            Assert.Equal(original, decoded);
        }

        [Fact]
        public void Base32Encode_SpecialCharacters_Roundtrip()
        {
            var original = Encoding.UTF8.GetBytes("测试@#$%");
            var encoded = EncodingUtil.Base32Encode(original);
            var decoded = EncodingUtil.Base32Decode(encoded);
            Assert.Equal(original, decoded);
        }

        [Fact]
        public void Base32Encode_MultipleBytes_Roundtrip()
        {
            var original = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var encoded = EncodingUtil.Base32Encode(original);
            var decoded = EncodingUtil.Base32Decode(encoded);
            Assert.Equal(original, decoded);
        }

        #endregion

        #region Base62 Tests

        [Fact]
        public void Base62Encode_Zero_ReturnsFirstChar()
        {
            var result = EncodingUtil.Base62Encode(0);
            Assert.Equal("0", result);
        }

        [Fact]
        public void Base62Encode_PositiveNumber_ReturnsEncodedString()
        {
            var result = EncodingUtil.Base62Encode(12345);
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Base62Encode_NegativeNumber_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EncodingUtil.Base62Encode(-1));
        }

        [Fact]
        public void Base62Encode_Decode_Roundtrip()
        {
            var original = 987654321L;
            var encoded = EncodingUtil.Base62Encode(original);
            var decoded = EncodingUtil.Base62Decode(encoded);
            Assert.Equal(original, decoded);
        }

        [Fact]
        public void Base62Decode_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EncodingUtil.Base62Decode(""));
        }

        [Fact]
        public void Base62Decode_NullString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EncodingUtil.Base62Decode(null));
        }

        [Fact]
        public void Base62Decode_InvalidCharacter_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EncodingUtil.Base62Decode("invalid@char"));
        }

        [Fact]
        public void Base62Encode_DifferentNumbers_DifferentEncodings()
        {
            var encoded1 = EncodingUtil.Base62Encode(123);
            var encoded2 = EncodingUtil.Base62Encode(456);
            Assert.NotEqual(encoded1, encoded2);
        }

        [Fact]
        public void Base62Encode_LargeNumber_ReturnsValidEncoding()
        {
            var largeNumber = long.MaxValue;
            var encoded = EncodingUtil.Base62Encode(largeNumber);
            var decoded = EncodingUtil.Base62Decode(encoded);
            Assert.Equal(largeNumber, decoded);
        }

        #endregion

        #region ROT Encryption Tests

        [Fact]
        public void RotEncrypt_EmptyString_ReturnsEmptyString()
        {
            var result = EncodingUtil.RotEncrypt("", 13);
            Assert.Equal("", result);
        }

        [Fact]
        public void RotEncrypt_NullString_ReturnsNull()
        {
            var result = EncodingUtil.RotEncrypt(null, 13);
            Assert.Null(result);
        }

        [Fact]
        public void RotEncrypt_Rot13_KnownValue()
        {
            var input = "HELLO";
            var result = EncodingUtil.RotEncrypt(input, 13);
            Assert.Equal("URYYB", result);
        }

        [Fact]
        public void RotEncrypt_NonAlphabeticalCharacters_Unchanged()
        {
            var input = "A1B!C";
            var result = EncodingUtil.RotEncrypt(input, 5);
            Assert.Equal("F1G!H", result);
        }

        [Fact]
        public void RotEncrypt_Lowercase_ConvertedToUppercase()
        {
            var input = "hello";
            var result = EncodingUtil.RotEncrypt(input, 13);
            Assert.Equal("URYYB", result);
        }

        [Fact]
        public void RotEncrypt_Rot26_ReturnsSameText()
        {
            var input = "HELLO";
            var result = EncodingUtil.RotEncrypt(input, 26);
            Assert.Equal("HELLO", result);
        }

        [Fact]
        public void RotEncrypt_Rot0_ReturnsSameText()
        {
            var input = "HELLO";
            var result = EncodingUtil.RotEncrypt(input, 0);
            Assert.Equal("HELLO", result);
        }

        [Fact]
        public void RotDecrypt_EmptyString_ReturnsEmptyString()
        {
            var result = EncodingUtil.RotDecrypt("", 13);
            Assert.Equal("", result);
        }

        [Fact]
        public void RotEncrypt_Decrypt_Roundtrip()
        {
            var original = "HELLO WORLD";
            var encrypted = EncodingUtil.RotEncrypt(original, 13);
            var decrypted = EncodingUtil.RotDecrypt(encrypted, 13);
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void RotEncrypt_LargeRotation_WrapsCorrectly()
        {
            var input = "A";
            var result = EncodingUtil.RotEncrypt(input, 27);
            Assert.Equal("B", result);
        }

        [Fact]
        public void RotEncrypt_VeryLargeRotation_WrapsMultipleTimes()
        {
            var input = "A";
            var result = EncodingUtil.RotEncrypt(input, 53);
            Assert.Equal("B", result);
        }

        #endregion

        #region Morse Code Tests

        [Fact]
        public void MorseEncode_EmptyString_ReturnsEmptyString()
        {
            var result = EncodingUtil.MorseEncode("");
            Assert.Equal("", result);
        }

        [Fact]
        public void MorseEncode_NullString_ReturnsEmptyString()
        {
            var result = EncodingUtil.MorseEncode(null);
            Assert.Equal("", result);
        }

        [Fact]
        public void MorseEncode_SingleLetter_ReturnsCorrectCode()
        {
            var result = EncodingUtil.MorseEncode("A");
            Assert.Equal(".-", result);
        }

        [Fact]
        public void MorseEncode_Word_ReturnsCodesSeparatedBySpaces()
        {
            var result = EncodingUtil.MorseEncode("SOS");
            Assert.Equal("... --- ...", result);
        }

        [Fact]
        public void MorseEncode_Lowercase_ConvertedToUppercase()
        {
            var result1 = EncodingUtil.MorseEncode("SOS");
            var result2 = EncodingUtil.MorseEncode("sos");
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void MorseEncode_Numbers_ReturnsCorrectCodes()
        {
            var result = EncodingUtil.MorseEncode("123");
            Assert.Equal(".---- ..--- ...--", result);
        }

        [Fact]
        public void MorseEncode_Spaces_IncludedInOutput()
        {
            var result = EncodingUtil.MorseEncode("A B");
            // Space between A and B is encoded as "/" in the morse code
            // ".-" (A) + " " (separator) + "/" (space character) + " " (separator) + "-..." (B)
            // = ".- / -..."
            Assert.Equal(".- / -...", result);
        }

        [Fact]
        public void MorseEncode_SpecialCharacters_Ignored()
        {
            var result = EncodingUtil.MorseEncode("A@B");
            Assert.Equal(".- -...", result);
        }

        [Fact]
        public void MorseDecode_EmptyString_ReturnsEmptyString()
        {
            var result = EncodingUtil.MorseDecode("");
            Assert.Equal("", result);
        }

        [Fact]
        public void MorseDecode_NullString_ReturnsEmptyString()
        {
            var result = EncodingUtil.MorseDecode(null);
            Assert.Equal("", result);
        }

        [Fact]
        public void MorseDecode_SingleLetter_ReturnsCorrectLetter()
        {
            var result = EncodingUtil.MorseDecode(".-");
            Assert.Equal("A", result);
        }

        [Fact]
        public void MorseDecode_Word_ReturnsCorrectWord()
        {
            var result = EncodingUtil.MorseDecode("... --- ...");
            Assert.Equal("SOS", result);
        }

        [Fact]
        public void MorseEncode_Decode_Roundtrip()
        {
            var original = "HELLO WORLD";
            var encoded = EncodingUtil.MorseEncode(original);
            var decoded = EncodingUtil.MorseDecode(encoded);
            // With the "/" character for spaces, roundtrip now works correctly
            Assert.Equal(original, decoded);
        }

        [Fact]
        public void MorseDecode_Numbers_ReturnsCorrectNumbers()
        {
            var result = EncodingUtil.MorseDecode(".---- ..--- ...--");
            Assert.Equal("123", result);
        }

        [Fact]
        public void MorseDecode_WithSpaces_ReturnsCorrectString()
        {
            var result = EncodingUtil.MorseDecode(".- -... ...");
            Assert.Equal("ABS", result);
        }

        [Fact]
        public void MorseEncode_AlphanumericSentence_ReturnsCorrectCode()
        {
            var result = EncodingUtil.MorseEncode("TEST 123");
            // Space is now encoded as "/" instead of " "
            Assert.Equal("- . ... - / .---- ..--- ...--", result);
        }

        [Fact]
        public void MorseDecode_ComplexMessage_ReturnsDecodedString()
        {
            var morse = "- . ... - / .---- ..--- ...--";
            var result = EncodingUtil.MorseDecode(morse);
            // The "/" character is now the space character in Morse code
            // Split by spaces: ["-", ".", "...", "-", "/", ".----", "..---", "...--"]
            // "/" maps to space character ' '
            Assert.Equal("TEST 123", result);
        }

        #endregion
    }
}
