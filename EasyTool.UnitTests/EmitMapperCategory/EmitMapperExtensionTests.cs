using System;
using System.Collections.Generic;
using EasyTool.Extension;
using Xunit;

namespace EasyTool.UnitTests.EmitMapperCategory
{
    /// <summary>
    /// EmitMapperExtension 测试类
    /// </summary>
    public class EmitMapperExtensionTests
    {
        #region 测试数据类

        public class SourceClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Value { get; set; }
        }

        public class DestinationClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Value { get; set; }
        }

        public class SourceWithExtra
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Extra { get; set; }
        }

        public class DestinationWithLess
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class SourceWithNullable
        {
            public int? Id { get; set; }
            public string? Name { get; set; }
        }

        public class DestinationWithoutNullable
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region EmitMapTo 测试

        [Fact]
        public void EmitMapTo_SimpleObject_ReturnsMappedObject()
        {
            var source = new SourceClass
            {
                Id = 1,
                Name = "Test",
                Value = 3.14
            };

            var dest = source.EmitMapTo<SourceClass, DestinationClass>();

            Assert.Equal(1, dest.Id);
            Assert.Equal("Test", dest.Name);
            Assert.Equal(3.14, dest.Value);
        }

        [Fact]
        public void EmitMapTo_NullObject_ReturnsDefault()
        {
            SourceClass? source = null;
            var dest = source.EmitMapTo<SourceClass, DestinationClass>();

            Assert.Equal(default(DestinationClass), dest);
        }

        [Fact]
        public void EmitMapTo_DifferentProperties_MapsMatchingProperties()
        {
            var source = new SourceWithExtra
            {
                Id = 1,
                Name = "Test",
                Extra = "ExtraValue"
            };

            var dest = source.EmitMapTo<SourceWithExtra, DestinationWithLess>();

            Assert.Equal(1, dest.Id);
            Assert.Equal("Test", dest.Name);
            // Extra 属性不会映射
        }

        [Fact]
        public void EmitMapTo_NullableToInt_MapsCorrectly()
        {
            var source = new SourceWithNullable
            {
                Id = 5,
                Name = "Test"
            };

            var dest = source.EmitMapTo<SourceWithNullable, DestinationWithoutNullable>();

            Assert.Equal(5, dest.Id);
            Assert.Equal("Test", dest.Name);
        }

        [Theory]
        [InlineData(1, "Name1", 1.0)]
        [InlineData(2, "Name2", 2.0)]
        [InlineData(0, "", 0.0)]
        public void EmitMapTo_VariousValues_MapsCorrectly(int id, string name, double value)
        {
            var source = new SourceClass
            {
                Id = id,
                Name = name,
                Value = value
            };

            var dest = source.EmitMapTo<SourceClass, DestinationClass>();

            Assert.Equal(id, dest.Id);
            Assert.Equal(name, dest.Name);
            Assert.Equal(value, dest.Value);
        }

        #endregion

        #region EmitMapToList 测试

        [Fact]
        public void EmitMapToList_EmptyList_ReturnsEmptyList()
        {
            var sources = new List<SourceClass>();
            var dests = sources.EmitMapToList<SourceClass, DestinationClass>();

            Assert.Empty(dests);
        }

        [Fact]
        public void EmitMapToList_SingleItem_ReturnsMappedList()
        {
            var sources = new List<SourceClass>
            {
                new SourceClass { Id = 1, Name = "Test", Value = 1.0 }
            };

            var dests = sources.EmitMapToList<SourceClass, DestinationClass>();

            Assert.Single(dests);
            Assert.Equal(1, dests[0].Id);
            Assert.Equal("Test", dests[0].Name);
            Assert.Equal(1.0, dests[0].Value);
        }

        [Fact]
        public void EmitMapToList_MultipleItems_ReturnsMappedList()
        {
            var sources = new List<SourceClass>
            {
                new SourceClass { Id = 1, Name = "Test1", Value = 1.0 },
                new SourceClass { Id = 2, Name = "Test2", Value = 2.0 },
                new SourceClass { Id = 3, Name = "Test3", Value = 3.0 }
            };

            var dests = sources.EmitMapToList<SourceClass, DestinationClass>();

            Assert.Equal(3, dests.Count);
            Assert.Equal(1, dests[0].Id);
            Assert.Equal(2, dests[1].Id);
            Assert.Equal(3, dests[2].Id);
        }

        [Fact]
        public void EmitMapToList_ArraySource_ReturnsMappedList()
        {
            var sources = new SourceClass[]
            {
                new SourceClass { Id = 1, Name = "Test", Value = 1.0 }
            };

            var dests = sources.EmitMapToList<SourceClass, DestinationClass>();

            Assert.Single(dests);
            Assert.Equal(1, dests[0].Id);
        }

        #endregion

        #region 边界测试

        [Fact]
        public void EmitMapTo_WithNullStringProperty_MapsNull()
        {
            var source = new SourceClass
            {
                Id = 1,
                Name = null,
                Value = 0
            };

            var dest = source.EmitMapTo<SourceClass, DestinationClass>();

            Assert.Equal(1, dest.Id);
            Assert.Null(dest.Name);
            Assert.Equal(0, dest.Value);
        }

        [Fact]
        public void EmitMapToList_WithNullItems_ReturnsMappedList()
        {
            var sources = new List<SourceClass>
            {
                new SourceClass { Id = 1, Name = null, Value = 0 }
            };

            var dests = sources.EmitMapToList<SourceClass, DestinationClass>();

            Assert.Single(dests);
            Assert.Null(dests[0].Name);
        }

        #endregion
    }
}