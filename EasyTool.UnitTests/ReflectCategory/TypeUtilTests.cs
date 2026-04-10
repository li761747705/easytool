using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyTool.ReflectCategory.Tests
{
    public class TypeUtilTests
    {
        #region Test Helpers

        private class SampleClass
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int PublicField;
#pragma warning disable CS0169
            private string _privateField = string.Empty;
#pragma warning restore CS0169

            public SampleClass() { }
            public SampleClass(int id, string name) { Id = id; Name = name; }

            public int Add(int a, int b) => a + b;
            public static string GetDescription() => "SampleClass";
        }

        private enum SampleEnum { A, B, C }

        private struct SampleStruct
        {
            public int Value;
        }

        private class DerivedClass : SampleClass
        {
            public string Extra { get; set; } = string.Empty;
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        private class TestAttribute : Attribute
        {
            public string Value { get; }
            public TestAttribute(string value) { Value = value; }
        }

        [Test("class-level")]
        private class AttributedClass
        {
            [Test("property-level")]
            public string AttributedProperty { get; set; } = string.Empty;

            [Test("method-level")]
            public void AttributedMethod() { }
        }

        #endregion

        #region IsSimpleType

        [Fact]
        public void IsSimpleType_PrimitiveTypes_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsSimpleType(typeof(int)));
            Assert.True(TypeUtil.IsSimpleType(typeof(bool)));
            Assert.True(TypeUtil.IsSimpleType(typeof(double)));
            Assert.True(TypeUtil.IsSimpleType(typeof(char)));
            Assert.True(TypeUtil.IsSimpleType(typeof(long)));
        }

        [Fact]
        public void IsSimpleType_String_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsSimpleType(typeof(string)));
        }

        [Fact]
        public void IsSimpleType_OtherSimpleTypes_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsSimpleType(typeof(decimal)));
            Assert.True(TypeUtil.IsSimpleType(typeof(DateTime)));
            Assert.True(TypeUtil.IsSimpleType(typeof(DateTimeOffset)));
            Assert.True(TypeUtil.IsSimpleType(typeof(TimeSpan)));
            Assert.True(TypeUtil.IsSimpleType(typeof(Guid)));
            Assert.True(TypeUtil.IsSimpleType(typeof(byte[])));
        }

        [Fact]
        public void IsSimpleType_Enum_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsSimpleType(typeof(SampleEnum)));
        }

        [Fact]
        public void IsSimpleType_NullableSimpleType_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsSimpleType(typeof(int?)));
            Assert.True(TypeUtil.IsSimpleType(typeof(DateTime?)));
            Assert.True(TypeUtil.IsSimpleType(typeof(Guid?)));
        }

        [Fact]
        public void IsSimpleType_ComplexType_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsSimpleType(typeof(SampleClass)));
            Assert.False(TypeUtil.IsSimpleType(typeof(List<int>)));
            Assert.False(TypeUtil.IsSimpleType(typeof(Dictionary<string, int>)));
        }

        [Fact]
        public void IsSimpleType_Null_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsSimpleType(null!));
        }

        #endregion

        #region IsNullableType

        [Fact]
        public void IsNullableType_NullableValueTypes_ReturnTrue()
        {
            Assert.True(TypeUtil.IsNullableType(typeof(int?)));
            Assert.True(TypeUtil.IsNullableType(typeof(DateTime?)));
            Assert.True(TypeUtil.IsNullableType(typeof(SampleEnum?)));
        }

        [Fact]
        public void IsNullableType_NonNullableTypes_ReturnFalse()
        {
            Assert.False(TypeUtil.IsNullableType(typeof(int)));
            Assert.False(TypeUtil.IsNullableType(typeof(string)));
            Assert.False(TypeUtil.IsNullableType(typeof(SampleClass)));
        }

        [Fact]
        public void IsNullableType_Null_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsNullableType(null!));
        }

        #endregion

        #region IsCollectionType

        [Fact]
        public void IsCollectionType_ListAndArray_ReturnTrue()
        {
            Assert.True(TypeUtil.IsCollectionType(typeof(List<int>)));
            Assert.True(TypeUtil.IsCollectionType(typeof(int[])));
            Assert.True(TypeUtil.IsCollectionType(typeof(IEnumerable<string>)));
        }

        [Fact]
        public void IsCollectionType_String_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsCollectionType(typeof(string)));
        }

        [Fact]
        public void IsCollectionType_NonCollection_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsCollectionType(typeof(int)));
            Assert.False(TypeUtil.IsCollectionType(typeof(SampleClass)));
        }

        [Fact]
        public void IsCollectionType_Null_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsCollectionType(null!));
        }

        #endregion

        #region IsDictionaryType

        [Fact]
        public void IsDictionaryType_Dictionary_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsDictionaryType(typeof(Dictionary<string, int>)));
        }

        [Fact]
        public void IsDictionaryType_NonDictionary_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsDictionaryType(typeof(List<int>)));
            Assert.False(TypeUtil.IsDictionaryType(typeof(SampleClass)));
            Assert.False(TypeUtil.IsDictionaryType(typeof(int)));
        }

        [Fact]
        public void IsDictionaryType_Null_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsDictionaryType(null!));
        }

        #endregion

        #region IsTupleType

        [Fact]
        public void IsTupleType_Tuple_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsTupleType(typeof(Tuple<int, string>)));
            Assert.True(TypeUtil.IsTupleType(typeof(Tuple<int, string, bool>)));
            Assert.True(TypeUtil.IsTupleType(typeof(ValueTuple<int, string>)));
        }

        [Fact]
        public void IsTupleType_ValueTuple_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsTupleType(typeof(ValueTuple<int>)));
            Assert.True(TypeUtil.IsTupleType(typeof(ValueTuple<int, string>)));
            Assert.True(TypeUtil.IsTupleType(typeof(ValueTuple<int, string, bool, double>)));
        }

        [Fact]
        public void IsTupleType_NonTuple_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsTupleType(typeof(SampleClass)));
            Assert.False(TypeUtil.IsTupleType(typeof(int)));
            Assert.False(TypeUtil.IsTupleType(typeof(List<int>)));
        }

        [Fact]
        public void IsTupleType_Null_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsTupleType(null!));
        }

        #endregion

        #region GetUnderlyingType

        [Fact]
        public void GetUnderlyingType_NullableType_ReturnsUnderlyingType()
        {
            Assert.Equal(typeof(int), TypeUtil.GetUnderlyingType(typeof(int?)));
            Assert.Equal(typeof(DateTime), TypeUtil.GetUnderlyingType(typeof(DateTime?)));
        }

        [Fact]
        public void GetUnderlyingType_NonNullableType_ReturnsNull()
        {
            Assert.Null(TypeUtil.GetUnderlyingType(typeof(int)));
            Assert.Null(TypeUtil.GetUnderlyingType(typeof(string)));
        }

        #endregion

        #region GetElementType

        [Fact]
        public void GetElementType_Array_ReturnsElementType()
        {
            Assert.Equal(typeof(int), TypeUtil.GetElementType(typeof(int[])));
            Assert.Equal(typeof(string), TypeUtil.GetElementType(typeof(string[])));
        }

        [Fact]
        public void GetElementType_GenericList_ReturnsElementType()
        {
            Assert.Equal(typeof(int), TypeUtil.GetElementType(typeof(List<int>)));
            Assert.Equal(typeof(string), TypeUtil.GetElementType(typeof(IEnumerable<string>)));
        }

        [Fact]
        public void GetElementType_NonCollection_ReturnsNull()
        {
            Assert.Null(TypeUtil.GetElementType(typeof(int)));
            Assert.Null(TypeUtil.GetElementType(typeof(SampleClass)));
        }

        [Fact]
        public void GetElementType_Null_ReturnsNull()
        {
            Assert.Null(TypeUtil.GetElementType(null!));
        }

        #endregion

        #region CreateInstance

        [Fact]
        public void CreateInstance_Parameterless_CreatesInstance()
        {
            var instance = TypeUtil.CreateInstance(typeof(SampleClass));
            Assert.NotNull(instance);
            Assert.IsType<SampleClass>(instance);
        }

        [Fact]
        public void CreateInstance_WithParameters_CreatesInstance()
        {
            var instance = TypeUtil.CreateInstance(typeof(SampleClass), 42, "test");
            Assert.NotNull(instance);
            var obj = Assert.IsType<SampleClass>(instance);
            Assert.Equal(42, obj.Id);
            Assert.Equal("test", obj.Name);
        }

        [Fact]
        public void CreateInstance_NullType_ReturnsNull()
        {
            Assert.Null(TypeUtil.CreateInstance(null!));
        }

        #endregion

        #region CreateGenericInstance

        [Fact]
        public void CreateGenericInstance_CreatesGenericInstance()
        {
            var instance = TypeUtil.CreateGenericInstance(typeof(List<>), new[] { typeof(int) });
            Assert.NotNull(instance);
            Assert.IsType<List<int>>(instance);
        }

        [Fact]
        public void CreateGenericInstance_WithArgs_PassesArgs()
        {
            // List<int> has a constructor that takes an int (capacity)
            var instance = TypeUtil.CreateGenericInstance(typeof(List<>), new[] { typeof(int) }, new object[] { 10 });
            Assert.NotNull(instance);
            var list = Assert.IsType<List<int>>(instance);
            Assert.Equal(10, list.Capacity);
        }

        [Fact]
        public void CreateGenericInstance_NonGenericType_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                TypeUtil.CreateGenericInstance(typeof(string), new[] { typeof(int) }));
        }

        [Fact]
        public void CreateGenericInstance_NullType_ReturnsNull()
        {
            Assert.Null(TypeUtil.CreateGenericInstance(null!, new[] { typeof(int) }));
        }

        [Fact]
        public void CreateGenericInstance_NullTypeArgs_ReturnsNull()
        {
            Assert.Null(TypeUtil.CreateGenericInstance(typeof(List<>), null!));
        }

        #endregion

        #region GetProperties

        [Fact]
        public void GetProperties_ReturnsPublicInstanceProperties()
        {
            var properties = TypeUtil.GetProperties(typeof(SampleClass));
            var names = properties.Select(p => p.Name).ToList();

            Assert.Contains("Id", names);
            Assert.Contains("Name", names);
        }

        [Fact]
        public void GetProperties_NullType_ReturnsEmpty()
        {
            Assert.Empty(TypeUtil.GetProperties(null!));
        }

        #endregion

        #region GetProperty

        [Fact]
        public void GetProperty_ExistingProperty_ReturnsProperty()
        {
            var property = TypeUtil.GetProperty(typeof(SampleClass), "Id");
            Assert.NotNull(property);
            Assert.Equal("Id", property!.Name);
        }

        [Fact]
        public void GetProperty_NonExistentProperty_ReturnsNull()
        {
            var property = TypeUtil.GetProperty(typeof(SampleClass), "NonExistent");
            Assert.Null(property);
        }

        #endregion

        #region GetPropertyValue / SetPropertyValue

        [Fact]
        public void GetPropertyValue_ReturnsPropertyValue()
        {
            var obj = new SampleClass { Id = 42, Name = "test" };
            var value = TypeUtil.GetPropertyValue(obj, "Id");
            Assert.Equal(42, value);
        }

        [Fact]
        public void GetPropertyValue_CaseInsensitive_Works()
        {
            var obj = new SampleClass { Name = "hello" };
            var value = TypeUtil.GetPropertyValue(obj, "name");
            Assert.Equal("hello", value);
        }

        [Fact]
        public void GetPropertyValue_NonExistent_ReturnsNull()
        {
            var obj = new SampleClass();
            var value = TypeUtil.GetPropertyValue(obj, "NonExistent");
            Assert.Null(value);
        }

        [Fact]
        public void GetPropertyValue_NullObject_ReturnsNull()
        {
            Assert.Null(TypeUtil.GetPropertyValue(null!, "Name"));
        }

        [Fact]
        public void SetPropertyValue_SetsPropertyValue()
        {
            var obj = new SampleClass();
            TypeUtil.SetPropertyValue(obj, "Id", 99);
            Assert.Equal(99, obj.Id);
        }

        [Fact]
        public void SetPropertyValue_CaseInsensitive_Works()
        {
            var obj = new SampleClass();
            TypeUtil.SetPropertyValue(obj, "name", "updated");
            Assert.Equal("updated", obj.Name);
        }

        [Fact]
        public void SetPropertyValue_NullObject_DoesNotThrow()
        {
            TypeUtil.SetPropertyValue(null!, "Name", "test");
        }

        #endregion

        #region GetFields

        [Fact]
        public void GetFields_ReturnsPublicInstanceFields()
        {
            var fields = TypeUtil.GetFields(typeof(SampleClass));
            var names = fields.Select(f => f.Name).ToList();
            Assert.Contains("PublicField", names);
        }

        [Fact]
        public void GetFields_NullType_ReturnsEmpty()
        {
            Assert.Empty(TypeUtil.GetFields(null!));
        }

        #endregion

        #region GetFieldValue / SetFieldValue

        [Fact]
        public void GetFieldValue_ReturnsFieldValue()
        {
            var obj = new SampleClass { PublicField = 123 };
            var value = TypeUtil.GetFieldValue(obj, "PublicField");
            Assert.Equal(123, value);
        }

        [Fact]
        public void GetFieldValue_CaseInsensitive_Works()
        {
            var obj = new SampleClass { PublicField = 456 };
            var value = TypeUtil.GetFieldValue(obj, "publicfield");
            Assert.Equal(456, value);
        }

        [Fact]
        public void GetFieldValue_NullObject_ReturnsNull()
        {
            Assert.Null(TypeUtil.GetFieldValue(null!, "PublicField"));
        }

        [Fact]
        public void SetFieldValue_SetsFieldValue()
        {
            var obj = new SampleClass();
            TypeUtil.SetFieldValue(obj, "PublicField", 789);
            Assert.Equal(789, obj.PublicField);
        }

        [Fact]
        public void SetFieldValue_NullObject_DoesNotThrow()
        {
            TypeUtil.SetFieldValue(null!, "PublicField", 1);
        }

        #endregion

        #region GetMethods

        [Fact]
        public void GetMethods_ReturnsPublicInstanceMethods()
        {
            var methods = TypeUtil.GetMethods(typeof(SampleClass));
            var names = methods.Select(m => m.Name).ToList();
            Assert.Contains("Add", names);
            Assert.Contains("get_Id", names);
        }

        [Fact]
        public void GetMethods_NullType_ReturnsEmpty()
        {
            Assert.Empty(TypeUtil.GetMethods(null!));
        }

        #endregion

        #region GetMethod

        [Fact]
        public void GetMethod_ExistingMethod_ReturnsMethod()
        {
            var method = TypeUtil.GetMethod(typeof(SampleClass), "Add");
            Assert.NotNull(method);
            Assert.Equal("Add", method!.Name);
        }

        [Fact]
        public void GetMethod_WithParameterTypes_ReturnsOverload()
        {
            var method = TypeUtil.GetMethod(typeof(SampleClass), "Add", new[] { typeof(int), typeof(int) });
            Assert.NotNull(method);
            Assert.Equal(2, method!.GetParameters().Length);
        }

        [Fact]
        public void GetMethod_NonExistent_ReturnsNull()
        {
            var method = TypeUtil.GetMethod(typeof(SampleClass), "NonExistentMethod");
            Assert.Null(method);
        }

        [Fact]
        public void GetMethod_NullType_ReturnsNull()
        {
            Assert.Null(TypeUtil.GetMethod(null!, "Add"));
        }

        #endregion

        #region InvokeMethod

        [Fact]
        public void InvokeMethod_CallsMethodAndReturnsResult()
        {
            var obj = new SampleClass();
            var result = TypeUtil.InvokeMethod(obj, "Add", 3, 4);
            Assert.Equal(7, result);
        }

        [Fact]
        public void InvokeMethod_VoidMethod_ReturnsNull()
        {
            var obj = new AttributedClass();
            var result = TypeUtil.InvokeMethod(obj, "AttributedMethod");
            Assert.Null(result);
        }

        [Fact]
        public void InvokeMethod_NullObject_ReturnsNull()
        {
            Assert.Null(TypeUtil.InvokeMethod(null!, "Add", 1, 2));
        }

        #endregion

        #region InvokeStaticMethod

        [Fact]
        public void InvokeStaticMethod_CallsStaticMethod()
        {
            var result = TypeUtil.InvokeStaticMethod(typeof(SampleClass), "GetDescription");
            Assert.Equal("SampleClass", result);
        }

        [Fact]
        public void InvokeStaticMethod_NullType_ReturnsNull()
        {
            Assert.Null(TypeUtil.InvokeStaticMethod(null!, "GetDescription"));
        }

        #endregion

        #region IsAssignableTo

        [Fact]
        public void IsAssignableTo_DerivedFromBase_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsAssignableTo(typeof(DerivedClass), typeof(SampleClass)));
        }

        [Fact]
        public void IsAssignableTo_SameType_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsAssignableTo(typeof(SampleClass), typeof(SampleClass)));
        }

        [Fact]
        public void IsAssignableTo_UnrelatedTypes_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsAssignableTo(typeof(SampleClass), typeof(int)));
        }

        [Fact]
        public void IsAssignableTo_NullTarget_ReturnsFalse()
        {
            Assert.False(TypeUtil.IsAssignableTo(typeof(SampleClass), null!));
        }

        [Fact]
        public void IsAssignableTo_InterfaceAssignment_ReturnsTrue()
        {
            Assert.True(TypeUtil.IsAssignableTo(typeof(List<int>), typeof(IEnumerable<int>)));
        }

        #endregion

        #region GetBaseType

        [Fact]
        public void GetBaseType_ReturnsBaseType()
        {
            Assert.Equal(typeof(SampleClass), TypeUtil.GetBaseType(typeof(DerivedClass)));
        }

        [Fact]
        public void GetBaseType_Object_ReturnsNull()
        {
            Assert.Null(TypeUtil.GetBaseType(typeof(object)));
        }

        [Fact]
        public void GetBaseType_Null_ReturnsNull()
        {
            Assert.Null(TypeUtil.GetBaseType(null!));
        }

        #endregion

        #region GetInterfaces

        [Fact]
        public void GetInterfaces_List_ReturnsInterfaces()
        {
            var interfaces = TypeUtil.GetInterfaces(typeof(List<int>));
            Assert.Contains(typeof(IEnumerable<int>), interfaces);
            Assert.Contains(typeof(IList<int>), interfaces);
        }

        [Fact]
        public void GetInterfaces_Null_ReturnsEmpty()
        {
            Assert.Empty(TypeUtil.GetInterfaces(null!));
        }

        #endregion

        #region GetInheritanceHierarchy

        [Fact]
        public void GetInheritanceHierarchy_ReturnsFullHierarchy()
        {
            var hierarchy = TypeUtil.GetInheritanceHierarchy(typeof(DerivedClass)).ToList();
            Assert.Contains(typeof(DerivedClass), hierarchy);
            Assert.Contains(typeof(SampleClass), hierarchy);
            Assert.Contains(typeof(object), hierarchy);
        }

        [Fact]
        public void GetInheritanceHierarchy_ObjectType_ReturnsEmpty()
        {
            // The implementation yields types while current != typeof(object),
            // then yields typeof(object) only if type != typeof(object).
            // So typeof(object) returns nothing.
            var hierarchy = TypeUtil.GetInheritanceHierarchy(typeof(object)).ToList();
            Assert.Empty(hierarchy);
        }

        [Fact]
        public void GetInheritanceHierarchy_Null_ReturnsEmpty()
        {
            Assert.Empty(TypeUtil.GetInheritanceHierarchy(null!));
        }

        #endregion

        #region GetAttribute / GetAttributes / HasAttribute

        [Fact]
        public void GetAttribute_ClassWithAttribute_ReturnsAttribute()
        {
            var attr = TypeUtil.GetAttribute<TestAttribute>(typeof(AttributedClass));
            Assert.NotNull(attr);
            Assert.Equal("class-level", attr!.Value);
        }

        [Fact]
        public void GetAttribute_ClassWithoutAttribute_ReturnsNull()
        {
            var attr = TypeUtil.GetAttribute<TestAttribute>(typeof(SampleClass));
            Assert.Null(attr);
        }

        [Fact]
        public void GetAttribute_PropertyWithAttribute_ReturnsAttribute()
        {
            var property = typeof(AttributedClass).GetProperty("AttributedProperty")!;
            var attr = TypeUtil.GetAttribute<TestAttribute>(property);
            Assert.NotNull(attr);
            Assert.Equal("property-level", attr!.Value);
        }

        [Fact]
        public void GetAttributes_MultipleAttributes_ReturnsAll()
        {
            var attributes = TypeUtil.GetAttributes<TestAttribute>(typeof(AttributedClass)).ToList();
            Assert.Single(attributes);
            Assert.Equal("class-level", attributes[0].Value);
        }

        [Fact]
        public void GetAttributes_NullMember_ReturnsEmpty()
        {
            Assert.Empty(TypeUtil.GetAttributes<TestAttribute>(null!));
        }

        [Fact]
        public void HasAttribute_ClassWithAttribute_ReturnsTrue()
        {
            Assert.True(TypeUtil.HasAttribute<TestAttribute>(typeof(AttributedClass)));
        }

        [Fact]
        public void HasAttribute_ClassWithoutAttribute_ReturnsFalse()
        {
            Assert.False(TypeUtil.HasAttribute<TestAttribute>(typeof(SampleClass)));
        }

        [Fact]
        public void HasAttribute_MethodWithAttribute_ReturnsTrue()
        {
            var method = typeof(AttributedClass).GetMethod("AttributedMethod")!;
            Assert.True(TypeUtil.HasAttribute<TestAttribute>(method));
        }

        [Fact]
        public void HasAttribute_NullMember_ReturnsFalse()
        {
            Assert.False(TypeUtil.HasAttribute<TestAttribute>(null!));
        }

        #endregion

        #region GetFriendlyName

        [Fact]
        public void GetFriendlyName_SimpleType_ReturnsName()
        {
            Assert.Equal("Int32", TypeUtil.GetFriendlyName(typeof(int)));
            Assert.Equal("String", TypeUtil.GetFriendlyName(typeof(string)));
        }

        [Fact]
        public void GetFriendlyName_GenericType_ReturnsFriendlyName()
        {
            var name = TypeUtil.GetFriendlyName(typeof(List<int>));
            Assert.Equal("List<Int32>", name);
        }

        [Fact]
        public void GetFriendlyName_NestedGenericType_ReturnsFriendlyName()
        {
            var name = TypeUtil.GetFriendlyName(typeof(Dictionary<string, List<int>>));
            Assert.Contains("Dictionary", name);
            Assert.Contains("String", name);
            Assert.Contains("List", name);
            Assert.Contains("Int32", name);
        }

        [Fact]
        public void GetFriendlyName_Null_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, TypeUtil.GetFriendlyName(null!));
        }

        #endregion

        #region GetDefaultValue

        [Fact]
        public void GetDefaultValue_ValueType_ReturnsDefault()
        {
            Assert.Equal(0, TypeUtil.GetDefaultValue(typeof(int)));
            Assert.Equal(false, TypeUtil.GetDefaultValue(typeof(bool)));
            Assert.Equal(0.0, TypeUtil.GetDefaultValue(typeof(double)));
        }

        [Fact]
        public void GetDefaultValue_ReferenceType_ReturnsNull()
        {
            Assert.Null(TypeUtil.GetDefaultValue(typeof(string)));
            Assert.Null(TypeUtil.GetDefaultValue(typeof(SampleClass)));
        }

        [Fact]
        public void GetDefaultValue_NullType_ReturnsNull()
        {
            Assert.Null(TypeUtil.GetDefaultValue(null!));
        }

        [Fact]
        public void GetDefaultValue_Struct_ReturnsDefault()
        {
            var result = TypeUtil.GetDefaultValue(typeof(SampleStruct));
            Assert.NotNull(result);
            var structValue = (SampleStruct)result!;
            Assert.Equal(0, structValue.Value);
        }

        #endregion
    }
}
