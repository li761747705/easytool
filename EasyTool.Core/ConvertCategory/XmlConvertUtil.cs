using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace EasyTool.ConvertCategory
{
    /// <summary>
    /// XML转换工具类
    /// </summary>
    public static class XmlConvertUtil
    {
        #region 对象序列化

        /// <summary>
        /// 对象序列化为XML字符串
        /// </summary>
        public static string ToXml<T>(T obj, bool indent = true, bool omitXmlDeclaration = false)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings
            {
                Indent = indent,
                OmitXmlDeclaration = omitXmlDeclaration,
                Encoding = Encoding.UTF8
            };

            using var writer = new StringWriter();
            using var xmlWriter = XmlWriter.Create(writer, settings);
            serializer.Serialize(xmlWriter, obj);
            return writer.ToString();
        }

        /// <summary>
        /// XML字符串反序列化为对象
        /// </summary>
        public static T? FromXml<T>(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return default;

            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xml);
            return (T?)serializer.Deserialize(reader);
        }

        /// <summary>
        /// 对象序列化为XML文件
        /// </summary>
        public static void ToXmlFile<T>(T obj, string filePath, bool indent = true)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings
            {
                Indent = indent,
                Encoding = Encoding.UTF8
            };

            using var writer = XmlWriter.Create(filePath, settings);
            serializer.Serialize(writer, obj);
        }

        /// <summary>
        /// XML文件反序列化为对象
        /// </summary>
        public static T? FromXmlFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);

            var serializer = new XmlSerializer(typeof(T));
            using var reader = XmlReader.Create(filePath);
            return (T?)serializer.Deserialize(reader);
        }

        #endregion

        #region 字典转换

        /// <summary>
        /// 字典转XML
        /// </summary>
        public static string DictionaryToXml(Dictionary<string, string> dict, string rootName = "root", string itemName = "item")
        {
            var doc = new XDocument(new XElement(rootName));
            var root = doc.Root!;

            foreach (var kvp in dict)
            {
                root.Add(new XElement(itemName,
                    new XAttribute("key", kvp.Key),
                    new XAttribute("value", kvp.Value)));
            }

            return doc.ToString();
        }

        /// <summary>
        /// XML转字典
        /// </summary>
        public static Dictionary<string, string> XmlToDictionary(string xml, string itemName = "item")
        {
            var dict = new Dictionary<string, string>();
            var doc = XDocument.Parse(xml);

            foreach (var element in doc.Descendants(itemName))
            {
                var key = element.Attribute("key")?.Value;
                var value = element.Attribute("value")?.Value;
                if (key != null)
                    dict[key] = value ?? "";
            }

            return dict;
        }

        #endregion

        #region 列表转换

        /// <summary>
        /// 列表转XML
        /// </summary>
        public static string ListToXml<T>(List<T> list, string rootName = "root", string itemName = "item")
        {
            var doc = new XDocument(new XElement(rootName));
            var root = doc.Root!;

            foreach (var item in list)
            {
                root.Add(new XElement(itemName, item?.ToString()));
            }

            return doc.ToString();
        }

        /// <summary>
        /// XML转列表
        /// </summary>
        public static List<string> XmlToList(string xml, string itemName = "item")
        {
            var list = new List<string>();
            var doc = XDocument.Parse(xml);

            foreach (var element in doc.Descendants(itemName))
            {
                list.Add(element.Value);
            }

            return list;
        }

        #endregion

        #region 格式化

        /// <summary>
        /// 格式化XML
        /// </summary>
        public static string FormatXml(string xml, string indent = "  ")
        {
            var doc = XDocument.Parse(xml);
            return doc.ToString();
        }

        /// <summary>
        /// 压缩XML（移除空白）
        /// </summary>
        public static string MinifyXml(string xml)
        {
            var doc = XDocument.Parse(xml);
            return doc.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// 验证XML格式
        /// </summary>
        public static bool IsValidXml(string xml)
        {
            try
            {
                XDocument.Parse(xml);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region XPath查询

        /// <summary>
        /// XPath查询
        /// </summary>
        public static List<string> SelectNodes(string xml, string xpath)
        {
            var results = new List<string>();
            var doc = XDocument.Parse(xml);
            var nodes = doc.XPathSelectElements(xpath);

            foreach (var node in nodes)
            {
                results.Add(node.Value);
            }

            return results;
        }

        /// <summary>
        /// XPath查询单个节点
        /// </summary>
        public static string? SelectSingleNode(string xml, string xpath)
        {
            var doc = XDocument.Parse(xml);
            var node = doc.XPathSelectElement(xpath);
            return node?.Value;
        }

        #endregion

        #region 节点操作

        /// <summary>
        /// 获取节点值
        /// </summary>
        public static string? GetNodeValue(string xml, string nodeName)
        {
            var doc = XDocument.Parse(xml);
            return doc.Root?.Element(nodeName)?.Value;
        }

        /// <summary>
        /// 设置节点值
        /// </summary>
        public static string SetNodeValue(string xml, string nodeName, string value)
        {
            var doc = XDocument.Parse(xml);
            var node = doc.Root?.Element(nodeName);
            if (node != null)
                node.Value = value;
            return doc.ToString();
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public static string? GetAttributeValue(string xml, string nodeName, string attributeName)
        {
            var doc = XDocument.Parse(xml);
            return doc.Root?.Element(nodeName)?.Attribute(attributeName)?.Value;
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        public static string SetAttributeValue(string xml, string nodeName, string attributeName, string value)
        {
            var doc = XDocument.Parse(xml);
            var node = doc.Root?.Element(nodeName);
            if (node != null)
                node.SetAttributeValue(attributeName, value);
            return doc.ToString();
        }

        #endregion
    }
}
