using Xunit;
using System;

namespace EasyTool.ReflectCategory.Tests
{
    public class ReflectUtilTests
    {
#pragma warning disable CS0067 // Event never used
#pragma warning disable CS0169 // Field never used
        private class TestClass
        {
            public int PublicField;
            private string _privateField;
            public string PublicProperty { get; set; }
            private int PrivateProperty { get; set; }

            public TestClass() { }
            public TestClass(int value) { PublicField = value; }

            public void PublicMethod() { }
            private void PrivateMethod() { }

            public event EventHandler? TestEvent;
        }
#pragma warning restore CS0169
#pragma warning restore CS0067

        [Fact]
        public void GetConstructors_ReturnsAllConstructors()
        {
            var constructors = ReflectUtil.GetConstructors(typeof(TestClass));
            Assert.True(constructors.Length >= 2);
        }

        [Fact]
        public void GetProperties_ReturnsAllProperties()
        {
            var properties = ReflectUtil.GetProperties(typeof(TestClass));
            Assert.Contains(properties, p => p.Name == "PublicProperty");
        }

        [Fact]
        public void GetFields_ReturnsAllFields()
        {
            var fields = ReflectUtil.GetFields(typeof(TestClass));
            Assert.Contains(fields, f => f.Name == "PublicField");
        }

        [Fact]
        public void GetMethods_ReturnsAllMethods()
        {
            var methods = ReflectUtil.GetMethods(typeof(TestClass));
            Assert.Contains(methods, m => m.Name == "PublicMethod");
        }

        [Fact]
        public void GetEvents_ReturnsAllEvents()
        {
            var events = ReflectUtil.GetEvents(typeof(TestClass));
            Assert.Contains(events, e => e.Name == "TestEvent");
        }

        [Fact]
        public void GetPropertyNames_ReturnsNames()
        {
            var names = ReflectUtil.GetPropertyNames(typeof(TestClass));
            Assert.Contains("PublicProperty", names);
        }

        [Fact]
        public void GetFieldNames_ReturnsNames()
        {
            var names = ReflectUtil.GetFieldNames(typeof(TestClass));
            Assert.Contains("PublicField", names);
        }

        [Fact]
        public void GetMethodNames_ReturnsNames()
        {
            var names = ReflectUtil.GetMethodNames(typeof(TestClass));
            Assert.Contains("PublicMethod", names);
        }

        [Fact]
        public void GetEventNames_ReturnsNames()
        {
            var names = ReflectUtil.GetEventNames(typeof(TestClass));
            Assert.Contains("TestEvent", names);
        }
    }
}