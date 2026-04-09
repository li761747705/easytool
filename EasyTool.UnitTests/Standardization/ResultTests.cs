using Xunit;
using EasyTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EasyTool.Tests
{
    
    public class ResultTests
    {
        [Fact]
        public void ResultTest()
        {
            var ok = Result.Ok("成功啦");
            Assert.True(ok.IsOK && ok.Message == "成功啦");

            var okData = Result.Ok<DateTime>(DateTime.Now.Date);
            Assert.True(okData.IsOK && okData.Data == DateTime.Now.Date);

            var okDataSet = Result.OkSet<int>(new List<int>() { 1, 2, 3 }, 10);
            Assert.True(okDataSet.IsOK && okDataSet.Data.Sum() == 6 && okDataSet.Total == 10);

            var fail = Result.Fail("失败啦");
            Assert.True(fail.IsOK == false && fail.Message == "失败啦");
        }
    }
}

