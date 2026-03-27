using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EasyTool.IOCategory
{
    /// <summary>
    /// Excel工具类（轻量级实现，不依赖第三方库）
    /// 支持读取和写入xlsx格式文件
    /// </summary>
    public static class ExcelUtil
    {
        private const string NS_SS = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string NS_R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly string[] ColumnNames = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,AA,AB,AC,AD,AE,AF,AG,AH,AI,AJ,AK,AL,AM,AN,AO,AP,AQ,AR,AS,AT,AU,AV,AW,AX,AY,AZ,BA,BB,BC,BD,BE,BF,BG,BH,BI,BJ,BK,BL,BM,BN,BO,BP,BQ,BR,BS,BT,BU,BV,BW,BX,BY,BZ".Split(',');

        #region 读取Excel

        /// <summary>
        /// 读取Excel文件为DataTable
        /// </summary>
        public static DataTable Read(string filePath, int sheetIndex = 0, bool hasHeader = true)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);

            using var stream = File.OpenRead(filePath);
            return Read(stream, sheetIndex, hasHeader);
        }

        /// <summary>
        /// 从流读取Excel为DataTable
        /// </summary>
        public static DataTable Read(Stream stream, int sheetIndex = 0, bool hasHeader = true)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            var sharedStrings = LoadSharedStrings(archive);
            var sheetEntry = GetSheetEntry(archive, sheetIndex);
            
            if (sheetEntry == null)
                throw new ArgumentException($"工作表索引 {sheetIndex} 不存在");

            return ParseWorksheet(sheetEntry, sharedStrings, hasHeader);
        }

        /// <summary>
        /// 获取所有工作表名称
        /// </summary>
        public static List<string> GetSheetNames(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);

            using var stream = File.OpenRead(filePath);
            return GetSheetNames(stream);
        }

        /// <summary>
        /// 从流获取所有工作表名称
        /// </summary>
        public static List<string> GetSheetNames(Stream stream)
        {
            var names = new List<string>();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            
            var workbookEntry = archive.GetEntry("xl/workbook.xml");
            if (workbookEntry == null) return names;

            using var reader = new StreamReader(workbookEntry.Open());
            var doc = XDocument.Load(reader);
            XNamespace ns = NS_SS;

            var sheets = doc.Root?.Element(ns + "sheets")?.Elements(ns + "sheet");
            if (sheets != null)
            {
                foreach (var sheet in sheets)
                {
                    var name = sheet.Attribute("name")?.Value;
                    if (!string.IsNullOrEmpty(name))
                        names.Add(name);
                }
            }

            return names;
        }

        private static Dictionary<int, string> LoadSharedStrings(ZipArchive archive)
        {
            var strings = new Dictionary<int, string>();
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return strings;

            using var reader = new StreamReader(entry.Open());
            var doc = XDocument.Load(reader);
            XNamespace ns = NS_SS;

            var siElements = doc.Root?.Elements(ns + "si");
            if (siElements == null) return strings;

            int index = 0;
            foreach (var si in siElements)
            {
                var text = si.Element(ns + "t")?.Value ?? "";
                strings[index++] = text;
            }

            return strings;
        }

        private static ZipArchiveEntry? GetSheetEntry(ZipArchive archive, int sheetIndex)
        {
            var entries = new List<ZipArchiveEntry>();
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.StartsWith("xl/worksheets/sheet") && entry.FullName.EndsWith(".xml"))
                    entries.Add(entry);
            }

            entries.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
            return sheetIndex < entries.Count ? entries[sheetIndex] : null;
        }

        private static DataTable ParseWorksheet(ZipArchiveEntry entry, Dictionary<int, string> sharedStrings, bool hasHeader)
        {
            var table = new DataTable();

            using var reader = new StreamReader(entry.Open());
            var doc = XDocument.Load(reader);
            XNamespace ns = NS_SS;

            var sheetData = doc.Root?.Element(ns + "sheetData");
            if (sheetData == null) return table;

            var rows = sheetData.Elements(ns + "row").ToList();
            if (rows.Count == 0) return table;

            // 解析所有行数据
            var allData = new List<List<string>>();
            int maxCols = 0;

            foreach (var row in rows)
            {
                var rowData = new List<string>();
                var cells = row.Elements(ns + "c");

                foreach (var cell in cells)
                {
                    var refAttr = cell.Attribute("r")?.Value ?? "";
                    var type = cell.Attribute("t")?.Value;
                    var value = cell.Element(ns + "v")?.Value ?? "";

                    if (type == "s" && int.TryParse(value, out int sharedIndex))
                    {
                        value = sharedStrings.TryGetValue(sharedIndex, out var s) ? s : "";
                    }

                    rowData.Add(value);
                }

                if (rowData.Count > maxCols)
                    maxCols = rowData.Count;

                allData.Add(rowData);
            }

            // 创建列
            if (hasHeader && allData.Count > 0)
            {
                var headers = allData[0];
                for (int i = 0; i < maxCols; i++)
                {
                    var colName = i < headers.Count && !string.IsNullOrEmpty(headers[i]) 
                        ? headers[i] 
                        : $"Column{i + 1}";
                    table.Columns.Add(colName, typeof(string));
                }
                allData.RemoveAt(0);
            }
            else
            {
                for (int i = 0; i < maxCols; i++)
                    table.Columns.Add($"Column{i + 1}", typeof(string));
            }

            // 添加数据行
            foreach (var rowData in allData)
            {
                var row = table.NewRow();
                for (int i = 0; i < Math.Min(rowData.Count, maxCols); i++)
                {
                    row[i] = rowData[i];
                }
                table.Rows.Add(row);
            }

            return table;
        }

        #endregion

        #region 写入Excel

        /// <summary>
        /// 将DataTable写入Excel文件
        /// </summary>
        public static void Write(string filePath, DataTable dataTable, string sheetName = "Sheet1")
        {
            using var stream = File.Create(filePath);
            Write(stream, dataTable, sheetName);
        }

        /// <summary>
        /// 将DataTable写入Excel流
        /// </summary>
        public static void Write(Stream stream, DataTable dataTable, string sheetName = "Sheet1")
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

            // 创建必要的文件结构
            CreateContentType(archive);
            CreateRels(archive);
            CreateWorkbook(archive, sheetName);
            CreateWorkbookRels(archive);
            CreateWorksheet(archive, dataTable);
            CreateStyles(archive);
        }

        private static void CreateContentType(ZipArchive archive)
        {
            var entry = archive.CreateEntry("[Content_Types].xml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
  <Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
  <Override PartName=""/xl/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml""/>
</Types>");
        }

        private static void CreateRels(ZipArchive archive)
        {
            var entry = archive.CreateEntry("_rels/.rels");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>");
        }

        private static void CreateWorkbook(ZipArchive archive, string sheetName)
        {
            var entry = archive.CreateEntry("xl/workbook.xml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write($@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""{NS_SS}"" xmlns:r=""{NS_R}"">
  <sheets>
    <sheet name=""{SecurityElement.Escape(sheetName)}"" sheetId=""1"" r:id=""rId1""/>
  </sheets>
</workbook>");
        }

        private static void CreateWorkbookRels(ZipArchive archive)
        {
            var entry = archive.CreateEntry("xl/_rels/workbook.xml.rels");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
  <Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/>
</Relationships>");
        }

        private static void CreateWorksheet(ZipArchive archive, DataTable dataTable)
        {
            var entry = archive.CreateEntry("xl/worksheets/sheet1.xml");
            using var writer = new StreamWriter(entry.Open());
            
            writer.Write($@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<worksheet xmlns=""{NS_SS}"">
<sheetData>");

            for (int r = 0; r < dataTable.Rows.Count; r++)
            {
                writer.Write($"<row r=\"{r + 1}\">");
                
                for (int c = 0; c < dataTable.Columns.Count; c++)
                {
                    var cellRef = GetColumnName(c) + (r + 1);
                    var value = dataTable.Rows[r][c]?.ToString() ?? "";
                    
                    // 尝试解析为数字
                    if (double.TryParse(value, out double numValue))
                    {
                        writer.Write($"<c r=\"{cellRef}\"><v>{numValue}</v></c>");
                    }
                    else
                    {
                        writer.Write($"<c r=\"{cellRef}\" t=\"inlineStr\"><is><t>{SecurityElement.Escape(value)}</t></is></c>");
                    }
                }
                
                writer.Write("</row>");
            }

            writer.Write("</sheetData></worksheet>");
        }

        private static void CreateStyles(ZipArchive archive)
        {
            var entry = archive.CreateEntry("xl/styles.xml");
            using var writer = new StreamWriter(entry.Open());
            writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
  <fonts count=""1""><font><sz val=""11""/><name val=""Calibri""/></font></fonts>
  <fills count=""1""><fill><patternFill patternType=""none""/></fill></fills>
  <borders count=""1""><border><left/><right/><top/><bottom/><diagonal/></border></borders>
  <cellStyleXfs count=""1""><xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/></cellStyleXfs>
  <cellXfs count=""1""><xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0""/></cellXfs>
</styleSheet>");
        }

        private static string GetColumnName(int index)
        {
            if (index < ColumnNames.Length)
                return ColumnNames[index];
            
            var name = new StringBuilder();
            index++;
            while (index > 0)
            {
                index--;
                name.Insert(0, (char)('A' + index % 26));
                index /= 26;
            }
            return name.ToString();
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 将List转换为DataTable
        /// </summary>
        public static DataTable ToDataTable<T>(IEnumerable<T> list)
        {
            var table = new DataTable();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
                table.Columns.Add(prop.Name, typeof(object));

            foreach (var item in list)
            {
                var row = table.NewRow();
                foreach (var prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// 将DataTable转换为List
        /// </summary>
        public static List<T> ToList<T>(DataTable table) where T : new()
        {
            var list = new List<T>();
            var properties = typeof(T).GetProperties();

            foreach (DataRow row in table.Rows)
            {
                var item = new T();
                foreach (var prop in properties)
                {
                    if (table.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        var value = Convert.ChangeType(row[prop.Name], prop.PropertyType);
                        prop.SetValue(item, value);
                    }
                }
                list.Add(item);
            }

            return list;
        }

        #endregion
    }
}