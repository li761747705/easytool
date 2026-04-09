using Xunit;
using EasyTool.Standardization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EasyTool.Tests
{
    
    public class OptionTests
    {
        [Fact]
        public void ToOptionsTest()
        {
            var options = new LogLevel().ToOptions();
            Assert.NotNull(options);
            Assert.Equal(4, options.Count);
            Assert.Equal("Debug", options[0].Value);
            Assert.Equal("调试", options[0].Text);

        }

        [Fact]
        public void GetOptionsTest()
        {
            var options = IOption.GetOptions<LogLevel>();
            Assert.NotNull(options);
            Assert.Equal(4, options.Count);
            Assert.Equal("Debug", options[0].Value);
            Assert.Equal("调试", options[0].Text);
        }

        public class LogLevel : IOption
        {
            [DisplayName("调试")]
            public static string Debug { get; set; } = nameof(Debug);
            [DisplayName("消息")]
            public static string Info { get; set; } = nameof(Info);
            [DisplayName("警告")]
            public static string Warning { get; set; } = nameof(Warning);
            [DisplayName("错误")]
            public static string Error { get; set; } = nameof(Error);
        }
    }
}

