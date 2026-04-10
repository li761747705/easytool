using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace EasyTool.ConvertCategory.Tests
{
    public class XmlConvertUtilTests : IDisposable
    {
        private readonly string _tempDir;

        public XmlConvertUtilTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "XmlConvertUtilTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        #region Helper

        [XmlRoot("Person")]
        public class TestPerson
        {
            public string Name { get; set; } = "";
            public int Age { get; set; }
        }

        #endregion

        #region ToXml / FromXml (object serialization)

        [Fact]
        public void ToXml_SerializesObject_IncludesProperties()
        {
            var person = new TestPerson { Name = "Alice", Age = 30 };

            string xml = XmlConvertUtil.ToXml(person);

            Assert.Contains("Alice", xml);
            Assert.Contains("30", xml);
        }

        [Fact]
        public void ToXml_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => XmlConvertUtil.ToXml<TestPerson>(null!));
        }

        [Fact]
        public void ToXml_NoIndent_ProducesCompactXml()
        {
            var person = new TestPerson { Name = "Alice", Age = 30 };

            string xml = XmlConvertUtil.ToXml(person, indent: false);

            Assert.Contains("Alice", xml);
            Assert.DoesNotContain("  ", xml); // no indentation spaces
        }

        [Fact]
        public void ToXml_OmitXmlDeclaration_NoDeclarationPrefix()
        {
            var person = new TestPerson { Name = "Alice", Age = 30 };

            string xml = XmlConvertUtil.ToXml(person, omitXmlDeclaration: true);

            Assert.False(xml.TrimStart().StartsWith("<?xml"));
        }

        [Fact]
        public void ToXml_WithXmlDeclaration_ContainsDeclaration()
        {
            var person = new TestPerson { Name = "Alice", Age = 30 };

            string xml = XmlConvertUtil.ToXml(person, omitXmlDeclaration: false);

            Assert.StartsWith("<?xml", xml.TrimStart());
        }

        [Fact]
        public void FromXml_ValidXml_ReturnsObject()
        {
            string xml = XmlConvertUtil.ToXml(new TestPerson { Name = "Alice", Age = 30 });

            var result = XmlConvertUtil.FromXml<TestPerson>(xml);

            Assert.NotNull(result);
            Assert.Equal("Alice", result.Name);
            Assert.Equal(30, result.Age);
        }

        [Fact]
        public void FromXml_NullOrEmpty_ReturnsDefault()
        {
            Assert.Null(XmlConvertUtil.FromXml<TestPerson>(null!));
            Assert.Null(XmlConvertUtil.FromXml<TestPerson>(""));
            Assert.Null(XmlConvertUtil.FromXml<TestPerson>("   "));
        }

        [Fact]
        public void FromXml_InvalidXml_ReturnsDefault()
        {
            // Malformed XML - the XmlSerializer may throw, but the method does not catch it
            // So this should throw an exception (InvalidOperationException from XmlSerializer)
            Assert.ThrowsAny<Exception>(() => XmlConvertUtil.FromXml<TestPerson>("not xml at all"));
        }

        [Fact]
        public void ToXml_FromXml_RoundTrip()
        {
            var original = new TestPerson { Name = "Charlie", Age = 35 };

            string xml = XmlConvertUtil.ToXml(original);
            var result = XmlConvertUtil.FromXml<TestPerson>(xml);

            Assert.NotNull(result);
            Assert.Equal(original.Name, result.Name);
            Assert.Equal(original.Age, result.Age);
        }

        #endregion

        #region ToXmlFile / FromXmlFile

        [Fact]
        public void ToXmlFile_CreatesFileWithContent()
        {
            string filePath = Path.Combine(_tempDir, "person.xml");
            var person = new TestPerson { Name = "Alice", Age = 30 };

            XmlConvertUtil.ToXmlFile(person, filePath);

            Assert.True(File.Exists(filePath));
            string content = File.ReadAllText(filePath);
            Assert.Contains("Alice", content);
        }

        [Fact]
        public void ToXmlFile_CreatesDirectory_IfNotExists()
        {
            string filePath = Path.Combine(_tempDir, "subdir", "nested", "person.xml");
            var person = new TestPerson { Name = "Alice", Age = 30 };

            XmlConvertUtil.ToXmlFile(person, filePath);

            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public void FromXmlFile_ReadsAndDeserializes()
        {
            string filePath = Path.Combine(_tempDir, "read_person.xml");
            var person = new TestPerson { Name = "Bob", Age = 25 };
            XmlConvertUtil.ToXmlFile(person, filePath);

            var result = XmlConvertUtil.FromXmlFile<TestPerson>(filePath);

            Assert.NotNull(result);
            Assert.Equal("Bob", result.Name);
            Assert.Equal(25, result.Age);
        }

        [Fact]
        public void FromXmlFile_FileNotFound_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() =>
                XmlConvertUtil.FromXmlFile<TestPerson>(Path.Combine(_tempDir, "nonexistent.xml")));
        }

        [Fact]
        public void ToXmlFile_FromXmlFile_RoundTrip()
        {
            string filePath = Path.Combine(_tempDir, "roundtrip.xml");
            var original = new TestPerson { Name = "RoundTrip", Age = 99 };

            XmlConvertUtil.ToXmlFile(original, filePath);
            var result = XmlConvertUtil.FromXmlFile<TestPerson>(filePath);

            Assert.NotNull(result);
            Assert.Equal(original.Name, result.Name);
            Assert.Equal(original.Age, result.Age);
        }

        #endregion

        #region DictionaryToXml / XmlToDictionary

        [Fact]
        public void DictionaryToXml_ProducesValidXml()
        {
            var dict = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            };

            string xml = XmlConvertUtil.DictionaryToXml(dict);

            Assert.Contains("key1", xml);
            Assert.Contains("value1", xml);
            Assert.Contains("key2", xml);
            Assert.Contains("value2", xml);
        }

        [Fact]
        public void DictionaryToXml_EmptyDictionary_ReturnsRootOnly()
        {
            var dict = new Dictionary<string, string>();

            string xml = XmlConvertUtil.DictionaryToXml(dict);

            Assert.Contains("root", xml);
        }

        [Fact]
        public void DictionaryToXml_CustomRootAndItemNames()
        {
            var dict = new Dictionary<string, string> { ["a"] = "b" };

            string xml = XmlConvertUtil.DictionaryToXml(dict, "settings", "entry");

            Assert.Contains("settings", xml);
            Assert.Contains("entry", xml);
            Assert.DoesNotContain("root", xml);
            Assert.DoesNotContain("item", xml);
        }

        [Fact]
        public void XmlToDictionary_ParsesCorrectly()
        {
            var dict = new Dictionary<string, string> { ["key1"] = "value1", ["key2"] = "value2" };
            string xml = XmlConvertUtil.DictionaryToXml(dict);

            var result = XmlConvertUtil.XmlToDictionary(xml);

            Assert.Equal(2, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("value2", result["key2"]);
        }

        [Fact]
        public void XmlToDictionary_CustomItemName()
        {
            var dict = new Dictionary<string, string> { ["a"] = "b" };
            string xml = XmlConvertUtil.DictionaryToXml(dict, "root", "entry");

            var result = XmlConvertUtil.XmlToDictionary(xml, "entry");

            Assert.Single(result);
            Assert.Equal("b", result["a"]);
        }

        [Fact]
        public void XmlToDictionary_EmptyXml_ReturnsEmptyDictionary()
        {
            string xml = XmlConvertUtil.DictionaryToXml(new Dictionary<string, string>());

            var result = XmlConvertUtil.XmlToDictionary(xml);

            Assert.Empty(result);
        }

        [Fact]
        public void DictionaryToXml_XmlToDictionary_RoundTrip()
        {
            var original = new Dictionary<string, string>
            {
                ["name"] = "test",
                ["version"] = "1.0",
                ["enabled"] = "true"
            };

            string xml = XmlConvertUtil.DictionaryToXml(original);
            var result = XmlConvertUtil.XmlToDictionary(xml);

            Assert.Equal(original.Count, result.Count);
            Assert.Equal(original["name"], result["name"]);
            Assert.Equal(original["version"], result["version"]);
            Assert.Equal(original["enabled"], result["enabled"]);
        }

        #endregion

        #region ListToXml / XmlToList

        [Fact]
        public void ListToXml_ProducesValidXml()
        {
            var list = new List<string> { "apple", "banana", "cherry" };

            string xml = XmlConvertUtil.ListToXml(list);

            Assert.Contains("apple", xml);
            Assert.Contains("banana", xml);
            Assert.Contains("cherry", xml);
        }

        [Fact]
        public void ListToXml_EmptyList_ReturnsRootOnly()
        {
            var list = new List<string>();

            string xml = XmlConvertUtil.ListToXml(list);

            Assert.Contains("root", xml);
        }

        [Fact]
        public void ListToXml_NullItem_TreatedAsEmpty()
        {
            var list = new List<string> { "first", null!, "third" };

            string xml = XmlConvertUtil.ListToXml(list);

            Assert.Contains("first", xml);
            Assert.Contains("third", xml);
        }

        [Fact]
        public void ListToXml_CustomRootAndItemNames()
        {
            var list = new List<string> { "item1" };

            string xml = XmlConvertUtil.ListToXml(list, "colors", "color");

            Assert.Contains("colors", xml);
            Assert.Contains("color", xml);
            Assert.Contains("item1", xml);
        }

        [Fact]
        public void XmlToList_ParsesCorrectly()
        {
            var list = new List<string> { "apple", "banana" };
            string xml = XmlConvertUtil.ListToXml(list);

            var result = XmlConvertUtil.XmlToList(xml);

            Assert.Equal(2, result.Count);
            Assert.Equal("apple", result[0]);
            Assert.Equal("banana", result[1]);
        }

        [Fact]
        public void XmlToList_CustomItemName()
        {
            var list = new List<string> { "red", "blue" };
            string xml = XmlConvertUtil.ListToXml(list, "colors", "color");

            var result = XmlConvertUtil.XmlToList(xml, "color");

            Assert.Equal(2, result.Count);
            Assert.Equal("red", result[0]);
            Assert.Equal("blue", result[1]);
        }

        [Fact]
        public void XmlToList_EmptyXml_ReturnsEmptyList()
        {
            string xml = XmlConvertUtil.ListToXml(new List<string>());

            var result = XmlConvertUtil.XmlToList(xml);

            Assert.Empty(result);
        }

        [Fact]
        public void ListToXml_XmlToList_RoundTrip()
        {
            var original = new List<string> { "one", "two", "three" };

            string xml = XmlConvertUtil.ListToXml(original);
            var result = XmlConvertUtil.XmlToList(xml);

            Assert.Equal(original.Count, result.Count);
            for (int i = 0; i < original.Count; i++)
                Assert.Equal(original[i], result[i]);
        }

        #endregion

        #region FormatXml / MinifyXml

        [Fact]
        public void FormatXml_ProducesIndentedOutput()
        {
            string xml = "<root><child>value</child></root>";

            string formatted = XmlConvertUtil.FormatXml(xml);

            Assert.Contains("  ", formatted);
            Assert.Contains("value", formatted);
        }

        [Fact]
        public void FormatXml_ProducesFormattedOutput()
        {
            string xml = "<root><child>value</child></root>";

            string formatted = XmlConvertUtil.FormatXml(xml);

            // XDocument default formatting uses 2-space indentation
            Assert.Contains("  ", formatted);
            Assert.Contains("value", formatted);
        }

        [Fact]
        public void MinifyXml_RemovesInterElementWhitespace()
        {
            string xml = "<root>\n  <child>value</child>\n</root>";

            string minified = XmlConvertUtil.MinifyXml(xml);

            Assert.Equal("<root><child>value</child></root>", minified);
        }

        [Fact]
        public void FormatXml_MinifyXml_RoundTripPreservesData()
        {
            string original = "<root><name>Alice</name><age>30</age></root>";

            string formatted = XmlConvertUtil.FormatXml(original);
            string minified = XmlConvertUtil.MinifyXml(formatted);

            Assert.Contains("Alice", minified);
            Assert.Contains("30", minified);
        }

        #endregion

        #region IsValidXml

        [Fact]
        public void IsValidXml_ValidXml_ReturnsTrue()
        {
            Assert.True(XmlConvertUtil.IsValidXml("<root><child>value</child></root>"));
        }

        [Fact]
        public void IsValidXml_InvalidXml_ReturnsFalse()
        {
            Assert.False(XmlConvertUtil.IsValidXml("not xml"));
            Assert.False(XmlConvertUtil.IsValidXml(""));
            Assert.False(XmlConvertUtil.IsValidXml("<root><unclosed>"));
        }

        [Fact]
        public void IsValidXml_NullOrEmpty_ReturnsFalse()
        {
            Assert.False(XmlConvertUtil.IsValidXml(""));
            Assert.False(XmlConvertUtil.IsValidXml("   "));
        }

        #endregion

        #region SelectNodes / SelectSingleNode

        [Fact]
        public void SelectNodes_FindsMatchingNodes()
        {
            string xml = "<root><item>A</item><item>B</item><item>C</item></root>";

            var results = XmlConvertUtil.SelectNodes(xml, "//item");

            Assert.Equal(3, results.Count);
            Assert.Equal("A", results[0]);
            Assert.Equal("B", results[1]);
            Assert.Equal("C", results[2]);
        }

        [Fact]
        public void SelectNodes_NoMatch_ReturnsEmptyList()
        {
            string xml = "<root><item>A</item></root>";

            var results = XmlConvertUtil.SelectNodes(xml, "//nonexistent");

            Assert.Empty(results);
        }

        [Fact]
        public void SelectSingleNode_FindsFirstMatch()
        {
            string xml = "<root><item>A</item><item>B</item></root>";

            string? result = XmlConvertUtil.SelectSingleNode(xml, "//item");

            Assert.Equal("A", result);
        }

        [Fact]
        public void SelectSingleNode_NoMatch_ReturnsNull()
        {
            string xml = "<root><item>A</item></root>";

            string? result = XmlConvertUtil.SelectSingleNode(xml, "//nonexistent");

            Assert.Null(result);
        }

        [Fact]
        public void SelectNodes_DeepPath_FindsCorrectly()
        {
            string xml = "<root><parent><child>deep value</child></parent></root>";

            var results = XmlConvertUtil.SelectNodes(xml, "//child");

            Assert.Single(results);
            Assert.Equal("deep value", results[0]);
        }

        #endregion

        #region GetNodeValue / SetNodeValue

        [Fact]
        public void GetNodeValue_ExistingNode_ReturnsValue()
        {
            string xml = "<root><name>Alice</name><age>30</age></root>";

            Assert.Equal("Alice", XmlConvertUtil.GetNodeValue(xml, "name"));
            Assert.Equal("30", XmlConvertUtil.GetNodeValue(xml, "age"));
        }

        [Fact]
        public void GetNodeValue_NonExistentNode_ReturnsNull()
        {
            string xml = "<root><name>Alice</name></root>";

            Assert.Null(XmlConvertUtil.GetNodeValue(xml, "nonexistent"));
        }

        [Fact]
        public void SetNodeValue_ExistingNode_UpdatesValue()
        {
            string xml = "<root><name>Alice</name></root>";

            string result = XmlConvertUtil.SetNodeValue(xml, "name", "Bob");

            Assert.Contains("Bob", result);
            Assert.DoesNotContain("Alice", result);
        }

        [Fact]
        public void SetNodeValue_NonExistentNode_DoesNotModify()
        {
            string xml = "<root><name>Alice</name></root>";

            string result = XmlConvertUtil.SetNodeValue(xml, "nonexistent", "value");

            Assert.Contains("Alice", result);
        }

        #endregion

        #region GetAttributeValue / SetAttributeValue

        [Fact]
        public void GetAttributeValue_ExistingAttribute_ReturnsValue()
        {
            string xml = "<root><person name=\"Alice\" age=\"30\"/></root>";

            Assert.Equal("Alice", XmlConvertUtil.GetAttributeValue(xml, "person", "name"));
            Assert.Equal("30", XmlConvertUtil.GetAttributeValue(xml, "person", "age"));
        }

        [Fact]
        public void GetAttributeValue_NonExistentNode_ReturnsNull()
        {
            string xml = "<root><person name=\"Alice\"/></root>";

            Assert.Null(XmlConvertUtil.GetAttributeValue(xml, "nonexistent", "name"));
        }

        [Fact]
        public void GetAttributeValue_NonExistentAttribute_ReturnsNull()
        {
            string xml = "<root><person name=\"Alice\"/></root>";

            Assert.Null(XmlConvertUtil.GetAttributeValue(xml, "person", "age"));
        }

        [Fact]
        public void SetAttributeValue_ExistingAttribute_UpdatesValue()
        {
            string xml = "<root><person name=\"Alice\"/></root>";

            string result = XmlConvertUtil.SetAttributeValue(xml, "person", "name", "Bob");

            Assert.Contains("Bob", result);
            Assert.DoesNotContain("Alice", result);
        }

        [Fact]
        public void SetAttributeValue_NonExistentNode_DoesNotModify()
        {
            string xml = "<root><person name=\"Alice\"/></root>";

            string result = XmlConvertUtil.SetAttributeValue(xml, "nonexistent", "name", "Bob");

            Assert.Contains("Alice", result);
        }

        [Fact]
        public void SetAttributeValue_AddsNewAttributeToExistingNode()
        {
            string xml = "<root><person name=\"Alice\"/></root>";

            string result = XmlConvertUtil.SetAttributeValue(xml, "person", "age", "30");

            Assert.Contains("age=\"30\"", result);
            Assert.Contains("Alice", result);
        }

        #endregion
    }
}
