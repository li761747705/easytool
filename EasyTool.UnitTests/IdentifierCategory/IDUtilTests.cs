using Xunit;
using EasyTool.IdentifierCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EasyTool.Tests
{
    
    public class IdUtilTests
    {
        [Fact]
        public void NextSequenceUUID_AreGreaterThan()
        {
            var uuid1 = IdUtil.UUID(UUIDStyle.Sequence);
            Thread.Sleep(10);
            var uuid2 = IdUtil.UUID(UUIDStyle.Sequence);

            Assert.True(string.Compare(uuid1.ToString(), uuid2.ToString(), StringComparison.Ordinal) < 0);
        }
    }
}