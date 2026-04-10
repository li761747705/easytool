using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Xunit;

namespace EasyTool.ConvertCategory.Tests
{
    public class CsvConvertUtilTests : IDisposable
    {
        private readonly string _tempDir;

        public CsvConvertUtilTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "CsvConvertUtilTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        #region Helper

        private class TestPerson
        {
            public string Name { get; set; } = "";
            public int Age { get; set; }
            public double Score { get; set; }
        }

        #endregion

        #region ToCsv<T> (object list to CSV string)

        [Fact]
        public void ToCsv_WithHeader_IncludesPropertyNames()
        {
            var list = new List<TestPerson>
            {
                new() { Name = "Alice", Age = 30, Score = 95.5 }
            };

            string csv = CsvConvertUtil.ToCsv(list);

            Assert.Contains("Name", csv);
            Assert.Contains("Age", csv);
            Assert.Contains("Score", csv);
            Assert.Contains("Alice", csv);
        }

        [Fact]
        public void ToCsv_WithoutHeader_OmitsPropertyNames()
        {
            var list = new List<TestPerson>
            {
                new() { Name = "Alice", Age = 30, Score = 95.5 }
            };

            string csv = CsvConvertUtil.ToCsv(list, includeHeader: false);

            Assert.DoesNotContain("Name", csv);
            Assert.Contains("Alice", csv);
        }

        [Fact]
        public void ToCsv_EmptyList_ReturnsOnlyHeader()
        {
            var list = new List<TestPerson>();

            string csv = CsvConvertUtil.ToCsv(list);

            Assert.Contains("Name", csv);
            Assert.Contains("Age", csv);
            // No data lines beyond header
            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Single(lines);
        }

        [Fact]
        public void ToCsv_EmptyList_NoHeader_ReturnsEmptyString()
        {
            var list = new List<TestPerson>();

            string csv = CsvConvertUtil.ToCsv(list, includeHeader: false);

            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Empty(lines);
        }

        [Fact]
        public void ToCsv_MultipleItems_AllItemsIncluded()
        {
            var list = new List<TestPerson>
            {
                new() { Name = "Alice", Age = 30, Score = 95.5 },
                new() { Name = "Bob", Age = 25, Score = 88.0 },
                new() { Name = "Charlie", Age = 35, Score = 72.3 }
            };

            string csv = CsvConvertUtil.ToCsv(list);

            Assert.Contains("Alice", csv);
            Assert.Contains("Bob", csv);
            Assert.Contains("Charlie", csv);
        }

        [Fact]
        public void ToCsv_SemicolonSeparator_UsesSemicolon()
        {
            var list = new List<TestPerson>
            {
                new() { Name = "Alice", Age = 30, Score = 95.5 }
            };

            string csv = CsvConvertUtil.ToCsv(list, separator: ';');

            Assert.Contains(";", csv);
            Assert.DoesNotContain(",", csv);
        }

        [Fact]
        public void ToCsv_FieldWithSeparator_EscapesWithQuotes()
        {
            var list = new List<TestPerson>
            {
                new() { Name = "Al,ice", Age = 30, Score = 95.5 }
            };

            string csv = CsvConvertUtil.ToCsv(list);

            Assert.Contains("\"Al,ice\"", csv);
        }

        [Fact]
        public void ToCsv_FieldWithQuotes_EscapesDoubleQuotes()
        {
            var list = new List<TestPerson>
            {
                new() { Name = "Al\"ice", Age = 30, Score = 95.5 }
            };

            string csv = CsvConvertUtil.ToCsv(list);

            Assert.Contains("\"Al\"\"ice\"", csv);
        }

        [Fact]
        public void ToCsv_NullPropertyValue_TreatedAsEmpty()
        {
            var list = new List<TestPerson>
            {
                new() { Name = null!, Age = 30, Score = 95.5 }
            };

            string csv = CsvConvertUtil.ToCsv(list);

            Assert.Contains("30", csv);
        }

        #endregion

        #region FromCsv<T> (CSV string to object list)

        [Fact]
        public void FromCsv_BasicParsing_ReturnsCorrectObjects()
        {
            string csv = "Name,Age,Score\r\nAlice,30,95.5\r\nBob,25,88";

            var result = CsvConvertUtil.FromCsv<TestPerson>(csv);

            Assert.Equal(2, result.Count);
            Assert.Equal("Alice", result[0].Name);
            Assert.Equal(30, result[0].Age);
            Assert.Equal("Bob", result[1].Name);
            Assert.Equal(25, result[1].Age);
        }

        [Fact]
        public void FromCsv_WithoutHeader_ParsesByPosition()
        {
            string csv = "Alice,30,95.5\r\nBob,25,88";

            var result = CsvConvertUtil.FromCsv<TestPerson>(csv, hasHeader: false);

            Assert.Equal(2, result.Count);
            Assert.Equal("Alice", result[0].Name);
            Assert.Equal(30, result[0].Age);
        }

        [Fact]
        public void FromCsv_EmptyString_ReturnsEmptyList()
        {
            string csv = "";

            var result = CsvConvertUtil.FromCsv<TestPerson>(csv);

            Assert.Empty(result);
        }

        [Fact]
        public void FromCsv_HeaderOnly_ReturnsEmptyList()
        {
            string csv = "Name,Age,Score";

            var result = CsvConvertUtil.FromCsv<TestPerson>(csv);

            Assert.Empty(result);
        }

        [Fact]
        public void FromCsv_EscapedFields_UnescapesCorrectly()
        {
            string csv = "Name,Age,Score\r\n\"Al,ice\",30,95.5";

            var result = CsvConvertUtil.FromCsv<TestPerson>(csv);

            Assert.Equal("Al,ice", result[0].Name);
        }

        [Fact]
        public void FromCsv_CaseInsensitiveHeader_MatchesProperties()
        {
            string csv = "name,age,score\r\nAlice,30,95.5";

            var result = CsvConvertUtil.FromCsv<TestPerson>(csv);

            Assert.Equal("Alice", result[0].Name);
            Assert.Equal(30, result[0].Age);
        }

        [Fact]
        public void FromCsv_DifferentSeparator_ParsesCorrectly()
        {
            string csv = "Name;Age;Score\r\nAlice;30;95.5";

            var result = CsvConvertUtil.FromCsv<TestPerson>(csv, separator: ';');

            Assert.Equal("Alice", result[0].Name);
            Assert.Equal(30, result[0].Age);
        }

        #endregion

        #region ToCsv (DataTable to CSV string)

        [Fact]
        public void ToCsv_DataTable_WithHeader_IncludesColumnNames()
        {
            var table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Age");
            table.Rows.Add("Alice", 30);
            table.Rows.Add("Bob", 25);

            string csv = CsvConvertUtil.ToCsv(table);

            Assert.Contains("Name", csv);
            Assert.Contains("Age", csv);
            Assert.Contains("Alice", csv);
            Assert.Contains("Bob", csv);
        }

        [Fact]
        public void ToCsv_DataTable_WithoutHeader_OmitsColumnNames()
        {
            var table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Age");
            table.Rows.Add("Alice", 30);

            string csv = CsvConvertUtil.ToCsv(table, includeHeader: false);

            Assert.DoesNotContain("Name", csv);
            Assert.Contains("Alice", csv);
        }

        [Fact]
        public void ToCsv_DataTable_EmptyTable_ReturnsOnlyHeader()
        {
            var table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Age");

            string csv = CsvConvertUtil.ToCsv(table);

            Assert.Contains("Name", csv);
            var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Single(lines);
        }

        [Fact]
        public void ToCsv_DataTable_NullValue_TreatedAsEmpty()
        {
            var table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Age");
            table.Rows.Add(DBNull.Value, 30);

            string csv = CsvConvertUtil.ToCsv(table);

            Assert.Contains("30", csv);
        }

        #endregion

        #region FromCsv (CSV string to DataTable)

        [Fact]
        public void FromCsv_DataTable_WithHeader_CreatesColumnsAndRows()
        {
            string csv = "Name,Age\r\nAlice,30\r\nBob,25";

            var table = CsvConvertUtil.FromCsv(csv);

            Assert.Equal(2, table.Columns.Count);
            Assert.Equal("Name", table.Columns[0].ColumnName);
            Assert.Equal("Age", table.Columns[1].ColumnName);
            Assert.Equal(2, table.Rows.Count);
            Assert.Equal("Alice", table.Rows[0][0]);
            Assert.Equal("30", table.Rows[0][1]);
        }

        [Fact]
        public void FromCsv_DataTable_WithoutHeader_GeneratesColumnNames()
        {
            string csv = "Alice,30\r\nBob,25";

            var table = CsvConvertUtil.FromCsv(csv, hasHeader: false);

            Assert.Equal("Column1", table.Columns[0].ColumnName);
            Assert.Equal("Column2", table.Columns[1].ColumnName);
            Assert.Equal(2, table.Rows.Count);
        }

        [Fact]
        public void FromCsv_DataTable_EmptyString_ReturnsEmptyTable()
        {
            string csv = "";

            var table = CsvConvertUtil.FromCsv(csv);

            Assert.Empty(table.Columns);
            Assert.Empty(table.Rows);
        }

        [Fact]
        public void FromCsv_DataTable_MoreColumnsInData_ExtraIgnored()
        {
            string csv = "Name\r\nAlice,extra,more";

            var table = CsvConvertUtil.FromCsv(csv);

            Assert.Single(table.Columns);
            Assert.Equal("Alice", table.Rows[0][0]);
        }

        [Fact]
        public void FromCsv_DataTable_FewerColumnsInData_MissingCellsEmpty()
        {
            string csv = "Name,Age\r\nAlice";

            var table = CsvConvertUtil.FromCsv(csv);

            Assert.Equal("Alice", table.Rows[0][0]);
            Assert.Equal(DBNull.Value, table.Rows[0][1]);
        }

        #endregion

        #region ToCsv (dictionary list to CSV)

        [Fact]
        public void ToCsv_DictionaryList_WithHeader_IncludesKeys()
        {
            var dicts = new List<Dictionary<string, object?>>
            {
                new() { ["Name"] = "Alice", ["Age"] = 30 }
            };

            string csv = CsvConvertUtil.ToCsv(dicts);

            Assert.Contains("Name", csv);
            Assert.Contains("Age", csv);
            Assert.Contains("Alice", csv);
        }

        [Fact]
        public void ToCsv_DictionaryList_WithoutHeader_OmitsKeys()
        {
            var dicts = new List<Dictionary<string, object?>>
            {
                new() { ["Name"] = "Alice", ["Age"] = 30 }
            };

            string csv = CsvConvertUtil.ToCsv(dicts, includeHeader: false);

            Assert.DoesNotContain("Name", csv);
            Assert.Contains("Alice", csv);
        }

        [Fact]
        public void ToCsv_DictionaryList_EmptyList_ReturnsEmptyString()
        {
            var dicts = new List<Dictionary<string, object?>>();

            string csv = CsvConvertUtil.ToCsv(dicts);

            Assert.Equal("", csv.Trim());
        }

        [Fact]
        public void ToCsv_DictionaryList_NullValue_TreatedAsEmpty()
        {
            var dicts = new List<Dictionary<string, object?>>
            {
                new() { ["Name"] = "Alice", ["Age"] = null }
            };

            string csv = CsvConvertUtil.ToCsv(dicts);

            Assert.Contains("Alice", csv);
        }

        [Fact]
        public void ToCsv_DictionaryList_MultipleDicts_AllIncluded()
        {
            var dicts = new List<Dictionary<string, object?>>
            {
                new() { ["Name"] = "Alice", ["Age"] = 30 },
                new() { ["Name"] = "Bob", ["Age"] = 25 }
            };

            string csv = CsvConvertUtil.ToCsv(dicts);

            Assert.Contains("Alice", csv);
            Assert.Contains("Bob", csv);
        }

        #endregion

        #region ToDictionaryList

        [Fact]
        public void ToDictionaryList_WithHeader_ReturnsDictsCorrectly()
        {
            string csv = "Name,Age\r\nAlice,30\r\nBob,25";

            var result = CsvConvertUtil.ToDictionaryList(csv);

            Assert.Equal(2, result.Count);
            Assert.Equal("Alice", result[0]["Name"]);
            Assert.Equal("30", result[0]["Age"]);
            Assert.Equal("Bob", result[1]["Name"]);
        }

        [Fact]
        public void ToDictionaryList_WithoutHeader_GeneratesColumnNames()
        {
            string csv = "Alice,30\r\nBob,25";

            var result = CsvConvertUtil.ToDictionaryList(csv, hasHeader: false);

            Assert.Equal(2, result.Count);
            Assert.Equal("Alice", result[0]["Column1"]);
        }

        [Fact]
        public void ToDictionaryList_EmptyString_ReturnsEmptyList()
        {
            string csv = "";

            var result = CsvConvertUtil.ToDictionaryList(csv);

            Assert.Empty(result);
        }

        [Fact]
        public void ToDictionaryList_HeaderOnly_ReturnsEmptyList()
        {
            string csv = "Name,Age";

            var result = CsvConvertUtil.ToDictionaryList(csv);

            Assert.Empty(result);
        }

        [Fact]
        public void ToDictionaryList_EscapedFields_UnescapesCorrectly()
        {
            string csv = "Name,Age\r\n\"Al,ice\",30";

            var result = CsvConvertUtil.ToDictionaryList(csv);

            Assert.Equal("Al,ice", result[0]["Name"]);
        }

        #endregion

        #region SaveToFile / LoadFromFile

        [Fact]
        public void SaveToFile_CreatesFileWithContent()
        {
            string filePath = Path.Combine(_tempDir, "test.csv");
            string csv = "Name,Age\r\nAlice,30";

            CsvConvertUtil.SaveToFile(csv, filePath);

            Assert.True(File.Exists(filePath));
            string content = File.ReadAllText(filePath);
            Assert.Equal(csv, content);
        }

        [Fact]
        public void SaveToFile_CreatesDirectory_IfNotExists()
        {
            string filePath = Path.Combine(_tempDir, "subdir", "nested", "test.csv");
            string csv = "Name,Age\r\nAlice,30";

            CsvConvertUtil.SaveToFile(csv, filePath);

            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public void SaveToFile_WithCustomEncoding_WritesCorrectly()
        {
            string filePath = Path.Combine(_tempDir, "encoding.csv");
            string csv = "Name,Age\r\nAlice,30";

            CsvConvertUtil.SaveToFile(csv, filePath, Encoding.ASCII);

            string content = File.ReadAllText(filePath, Encoding.ASCII);
            Assert.Equal(csv, content);
        }

        [Fact]
        public void LoadFromFile_ReadsFileContent()
        {
            string filePath = Path.Combine(_tempDir, "read.csv");
            string expected = "Name,Age\r\nAlice,30";
            File.WriteAllText(filePath, expected, Encoding.UTF8);

            string result = CsvConvertUtil.LoadFromFile(filePath);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void LoadFromFile_WithCustomEncoding_ReadsCorrectly()
        {
            string filePath = Path.Combine(_tempDir, "encoding_read.csv");
            string expected = "Name,Age\r\nAlice,30";
            File.WriteAllText(filePath, expected, Encoding.UTF8);

            string result = CsvConvertUtil.LoadFromFile(filePath, Encoding.UTF8);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void SaveToFile_And_LoadFromFile_RoundTrip()
        {
            string filePath = Path.Combine(_tempDir, "roundtrip.csv");
            string original = "Name,Age\r\nAlice,30\r\nBob,25";

            CsvConvertUtil.SaveToFile(original, filePath);
            string loaded = CsvConvertUtil.LoadFromFile(filePath);

            Assert.Equal(original, loaded);
        }

        #endregion

        #region Round-trip tests

        [Fact]
        public void ToCsv_FromCsv_ObjectList_RoundTrip()
        {
            var original = new List<TestPerson>
            {
                new() { Name = "Alice", Age = 30, Score = 95.5 },
                new() { Name = "Bob", Age = 25, Score = 88.0 }
            };

            string csv = CsvConvertUtil.ToCsv(original);
            var result = CsvConvertUtil.FromCsv<TestPerson>(csv);

            Assert.Equal(original.Count, result.Count);
            Assert.Equal(original[0].Name, result[0].Name);
            Assert.Equal(original[0].Age, result[0].Age);
            Assert.Equal(original[1].Name, result[1].Name);
            Assert.Equal(original[1].Age, result[1].Age);
        }

        [Fact]
        public void ToCsv_FromCsv_DataTable_RoundTrip()
        {
            var original = new DataTable();
            original.Columns.Add("Name", typeof(string));
            original.Columns.Add("Age", typeof(int));
            original.Rows.Add("Alice", 30);
            original.Rows.Add("Bob", 25);

            string csv = CsvConvertUtil.ToCsv(original);
            var result = CsvConvertUtil.FromCsv(csv);

            Assert.Equal(original.Rows.Count, result.Rows.Count);
            Assert.Equal(original.Rows[0][0].ToString(), result.Rows[0][0].ToString());
            Assert.Equal(original.Rows[1][0].ToString(), result.Rows[1][0].ToString());
        }

        [Fact]
        public void ToCsv_ToDictionaryList_RoundTrip()
        {
            var original = new List<Dictionary<string, object?>>
            {
                new() { ["Name"] = "Alice", ["Age"] = 30 },
                new() { ["Name"] = "Bob", ["Age"] = 25 }
            };

            string csv = CsvConvertUtil.ToCsv(original);
            var result = CsvConvertUtil.ToDictionaryList(csv);

            Assert.Equal(original.Count, result.Count);
            Assert.Equal("Alice", result[0]["Name"]);
            Assert.Equal("30", result[0]["Age"]);
        }

        #endregion
    }
}
