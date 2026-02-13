using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace EasyTool.Web.Development
{
    public class BuildDtoToTS
    {
        public static string Build(Assembly assembly)
        {
            List<DtoClass> dtos = GetDtos(assembly);
            string code = CreateCode(dtos);
            return code.ToString();
        }

        public static void BuildToFile(Assembly assembly, string path)
        {
            var code = Build(assembly);
            string existsCode = "";
            if (System.IO.File.Exists(path) == true)
                existsCode = System.IO.File.ReadAllText(path);

            if (existsCode != code)
            {
                System.IO.File.WriteAllText(path, code);
            }
        }

        #region 构造代码

        public static string CreateCode(List<DtoClass> dtos)
        {
            StringBuilder code = new StringBuilder();
            foreach (var dto in dtos)
            {
                code.AppendLine($"/** {dto.Title}  {dto.Namespace}*/");

                code.AppendLine($"export interface {dto.Name} {{");

                foreach (var property in dto.Propertys)
                {
                    if (property.IsInverseProperty) continue;

                    string comment = $"  /** <Title><Dept> */";

                    comment = comment.Replace("<Title>", property.Title ?? "");
                    if (property.IsKey)
                        comment = comment.Replace("<Dept>", $"\r\n  @Key<Dept>");
                    if (property.IsRequired)
                        comment = comment.Replace("<Dept>", $"\r\n  @Required<Dept>");
                    if (property.StringLength > 0)
                        comment = comment.Replace("<Dept>", $"\r\n  @StringLength {property.StringLength}<Dept>");

                    comment = comment.Replace("<Dept>", "");
                    code.AppendLine(comment);

                    string fieldCode = $"{property.Name}<Nullable>: <Type>,";

                    List<Type> typeChain = new List<Type>();
                    GetTypeChain(property.Type, typeChain);
                    foreach (var type in typeChain)
                    {
                        if (type == typeof(List<>))
                        {
                            fieldCode = fieldCode.Replace("<Type>", "Array<<Type>>");
                        }
                        else if (type == typeof(Nullable<>))
                        {
                            fieldCode = fieldCode.Replace("<Nullable>", "?");
                        }
                        else if (type == typeof(string) || type == typeof(Guid))
                        {
                            fieldCode = fieldCode.Replace("<Type>", "string");
                        }
                        else if (type == typeof(int) || type == typeof(long) || type == typeof(decimal) || type == typeof(float) || type == typeof(double))
                        {
                            fieldCode = fieldCode.Replace("<Type>", "number");
                        }
                        else if (type == typeof(bool))
                        {
                            fieldCode = fieldCode.Replace("<Type>", "boolean");
                        }
                        else if (type == typeof(DateTime))
                        {
                            fieldCode = fieldCode.Replace("<Type>", "Date");
                        }
                        else
                        {
                            if (dtos.Any(x => x.Name == type.Name))//如果某个类型是自己定义的类型，那么我们直接引用那个类型
                                fieldCode = fieldCode.Replace("<Type>", type.Name);
                            else
                                fieldCode = fieldCode.Replace("<Type>", "any");
                        }
                    }

                    if (property.IsRequired == false && property.Type == typeof(string))
                    {
                        fieldCode = fieldCode.Replace("<Nullable>", "?");
                    }

                    fieldCode = fieldCode.Replace("<Nullable>", "");//对于不为空的，取消空占位符
                    code.AppendLine($"  {fieldCode}");
                }


                code.AppendLine($"}}");
                code.AppendLine($"");
            }

            return code.ToString();
        }

        //获得真时的类型
        public static void GetTypeChain(Type type, List<Type> typeChain)
        {
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                typeChain.Add(genericType);
            }
            else
            {
                typeChain.Add(type);
            }

            var sonType = type.GetGenericArguments();
            if (sonType.Length == 0) return;

            GetTypeChain(sonType[0], typeChain);
        }


        public static List<DtoClass> GetDtos(Assembly assembly)
        {
            List<DtoClass> dtos = new List<DtoClass>();

            var dtoCommentTypes = assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(DtoCommentsAttribute), false).Count() > 0);
            foreach (var dtoCommentType in dtoCommentTypes)
            {

                var dto = new DtoClass(dtoCommentType.Name, dtoCommentType.Namespace);

                dto.Title = dtoCommentType.GetCustomAttribute<DtoCommentsAttribute>()?.Title ?? "";

                var propertyTypes = dtoCommentType.GetProperties();
                foreach (var propertyType in propertyTypes)
                {
                    var property = new DtoProperty(propertyType.PropertyType, propertyType.Name);

                    // 优先使用 DisplayAttribute 或 DescriptionAttribute，然后是 DisplayNameAttribute
                    property.Title = propertyType.GetCustomAttribute<DisplayAttribute>()?.GetName()
                                  ?? propertyType.GetCustomAttribute<DescriptionAttribute>()?.Description
                                  ?? propertyType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                                  ?? "";

                    property.IsInverseProperty = propertyType.GetCustomAttribute<InversePropertyAttribute>() != null;
                    property.IsKey = propertyType.GetCustomAttribute<KeyAttribute>() != null;
                    property.IsRequired = propertyType.GetCustomAttribute<RequiredAttribute>() != null;
                    property.StringLength = propertyType.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength ?? 0;

                    // 支持更多 .NET 约定特性
                    property.IsEditable = propertyType.GetCustomAttribute<System.ComponentModel.ReadOnlyAttribute>() == null;
                    property.DataType = GetDataType(propertyType.GetCustomAttribute<DataTypeAttribute>());
                    property.RegularExpression = propertyType.GetCustomAttribute<RegularExpressionAttribute>()?.Pattern;
                    property.RangeMinimum = propertyType.GetCustomAttribute<RangeAttribute>()?.Minimum as double?;
                    property.RangeMaximum = propertyType.GetCustomAttribute<RangeAttribute>()?.Maximum as double?;
                    property.IsForeignKey = propertyType.GetCustomAttribute<ForeignKeyAttribute>() != null;

                    dto.Propertys.Add(property);
                }

                dtos.Add(dto);
            }

            return dtos;
        }


        #endregion

        public class DtoClass
        {
            public DtoClass(string name, string _namespace)
            {
                Name = name;
                Namespace = _namespace;
            }

            public string Name { get; set; }
            public string Namespace { get; set; }

            public string Title { get; set; }//类名称

            public List<DtoProperty> Propertys { get; set; } = new List<DtoProperty>();

        }

        /// <summary>
        /// DTO 属性信息，支持标准 .NET DataAnnotations 特性
        /// </summary>
        public class DtoProperty
        {
            public DtoProperty(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            /// <summary>
            /// 属性类型
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// 属性名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 显示名称（支持 DisplayAttribute、DescriptionAttribute、DisplayNameAttribute）
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// 是否关联属性（InversePropertyAttribute）
            /// </summary>
            public bool IsInverseProperty { get; set; }

            /// <summary>
            /// 是否必填（RequiredAttribute）
            /// </summary>
            public bool IsRequired { get; set; }

            /// <summary>
            /// 字符串长度（StringLengthAttribute）
            /// </summary>
            public int StringLength { get; set; }

            /// <summary>
            /// 是否主键（KeyAttribute）
            /// </summary>
            public bool IsKey { get; set; }

            /// <summary>
            /// 是否可编辑（EditableAttribute）
            /// </summary>
            public bool IsEditable { get; set; } = true;

            /// <summary>
            /// 数据类型（DataTypeAttribute）
            /// </summary>
            public string DataType { get; set; }

            /// <summary>
            /// 正则表达式验证（RegularExpressionAttribute）
            /// </summary>
            public string RegularExpression { get; set; }

            /// <summary>
            /// 范围最小值（RangeAttribute）
            /// </summary>
            public double? RangeMinimum { get; set; }

            /// <summary>
            /// 范围最大值（RangeAttribute）
            /// </summary>
            public double? RangeMaximum { get; set; }

            /// <summary>
            /// 是否外键（ForeignKeyAttribute）
            /// </summary>
            public bool IsForeignKey { get; set; }
        }

        /// <summary>
        /// 获取 DataTypeAttribute 的数据类型名称
        /// </summary>
        private static string GetDataType(DataTypeAttribute attribute)
        {
            return attribute?.DataType.ToString() ?? string.Empty;
        }
 
    }

    public class DtoCommentsAttribute : Attribute
    {
        public string Title { get; set; }
        public DtoCommentsAttribute(string title = "")
        {
            Title = title;
        }
    }
}
