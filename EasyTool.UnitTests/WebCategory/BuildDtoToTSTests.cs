using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using EasyTool.Web.Development;
using Xunit;

namespace EasyTool.UnitTests.WebCategory
{
    /// <summary>
    /// BuildDtoToTS 测试类
    /// 注意：GetDtos、CreateCode、GetTypeChain 是 internal 方法，无法从外部测试
    /// </summary>
    public class BuildDtoToTSTests
    {
        #region 测试数据类

        [DtoComments("用户信息")]
        public class TestUserDto
        {
            [Key]
            public int Id { get; set; }

            [Required]
            [StringLength(50)]
            [Display(Name = "用户名")]
            public string Name { get; set; }

            [EmailAddress]
            public string Email { get; set; }

            public int? Age { get; set; }

            public List<string> Tags { get; set; }

            public DateTime CreatedAt { get; set; }

            public bool IsActive { get; set; }

            public decimal Balance { get; set; }

            public Guid UserId { get; set; }
        }

        [DtoComments("产品信息")]
        public class TestProductDto
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }

            public double Price { get; set; }

            public TestUserDto Owner { get; set; }
        }

        #endregion

        #region Build 测试

        [Fact]
        public void Build_ValidAssembly_ReturnsTypeScriptCode()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var code = BuildDtoToTS.Build(assembly);

            Assert.NotNull(code);
            Assert.NotEmpty(code);
        }

        [Fact]
        public void Build_ContainsDtoClassNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var code = BuildDtoToTS.Build(assembly);

            Assert.Contains("TestUserDto", code);
            Assert.Contains("TestProductDto", code);
        }

        [Fact]
        public void Build_GeneratesExportInterface()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var code = BuildDtoToTS.Build(assembly);

            Assert.Contains("export interface", code);
        }

        [Fact]
        public void Build_ContainsCorrectTypeScriptTypes()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var code = BuildDtoToTS.Build(assembly);

            // 验证类型映射
            Assert.Contains("number", code); // int -> number
            Assert.Contains("string", code); // string -> string
            Assert.Contains("boolean", code); // bool -> boolean
            Assert.Contains("Date", code); // DateTime -> Date
        }

        [Fact]
        public void Build_ContainsArrayForList()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var code = BuildDtoToTS.Build(assembly);

            Assert.Contains("Array<", code); // List<T> -> Array<T>
        }

        [Fact]
        public void Build_ContainsNullableMark()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var code = BuildDtoToTS.Build(assembly);

            Assert.Contains("?", code); // nullable -> ? (可选属性)
        }

        [Fact]
        public void Build_ContainsComments()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var code = BuildDtoToTS.Build(assembly);

            Assert.Contains("/**", code); // TypeScript 注释
        }

        [Fact]
        public void Build_EmptyAssembly_ReturnsEmptyCode()
        {
            // 使用一个没有 DtoComments 标记类型的程序集
            var code = BuildDtoToTS.Build(typeof(object).Assembly);

            // 应返回空字符串或不包含 export interface
            Assert.DoesNotContain("TestUserDto", code);
        }

        #endregion

        #region BuildToFile 测试

        [Fact]
        public void BuildToFile_ValidAssembly_CreatesFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var tempPath = Path.Combine(
                Path.GetTempPath(),
                "test_dto.ts");

            BuildDtoToTS.BuildToFile(assembly, tempPath);

            Assert.True(File.Exists(tempPath));
            var content = File.ReadAllText(tempPath);
            Assert.NotEmpty(content);
            Assert.Contains("export interface", content);

            // 清理
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }

        [Fact]
        public void BuildToFile_ExistingFile_UpdatesIfDifferent()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var tempPath = Path.Combine(
                Path.GetTempPath(),
                "test_dto_update.ts");

            // 先写入旧内容
            File.WriteAllText(tempPath, "old content");

            BuildDtoToTS.BuildToFile(assembly, tempPath);

            var newContent = File.ReadAllText(tempPath);
            Assert.NotEqual("old content", newContent);
            Assert.Contains("export interface", newContent);

            // 清理
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }

        [Fact]
        public void BuildToFile_SameContent_DoesNotModify()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var tempPath = Path.Combine(
                Path.GetTempPath(),
                "test_dto_same.ts");

            // 先生成一次
            BuildDtoToTS.BuildToFile(assembly, tempPath);
            var originalContent = File.ReadAllText(tempPath);

            // 再次生成（内容相同）
            BuildDtoToTS.BuildToFile(assembly, tempPath);
            var newContent = File.ReadAllText(tempPath);

            Assert.Equal(originalContent, newContent);

            // 清理
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }

        #endregion

        #region DtoCommentsAttribute 测试

        [Fact]
        public void DtoCommentsAttribute_DefaultConstructor_EmptyTitle()
        {
            var attr = new DtoCommentsAttribute();

            Assert.Equal("", attr.Title);
        }

        [Fact]
        public void DtoCommentsAttribute_WithTitle_SetsTitle()
        {
            var attr = new DtoCommentsAttribute("测试标题");

            Assert.Equal("测试标题", attr.Title);
        }

        [Fact]
        public void DtoCommentsAttribute_TitleProperty_CanBeModified()
        {
            var attr = new DtoCommentsAttribute();
            attr.Title = "新标题";

            Assert.Equal("新标题", attr.Title);
        }

        #endregion

        #region 属性特性测试

        [Fact]
        public void TestDto_HasDtoCommentsAttribute()
        {
            var type = typeof(TestUserDto);
            var attr = type.GetCustomAttribute<DtoCommentsAttribute>();

            Assert.NotNull(attr);
            Assert.Equal("用户信息", attr.Title);
        }

        [Fact]
        public void TestDto_HasRequiredAttribute()
        {
            var property = typeof(TestUserDto).GetProperty("Name");
            var attr = property?.GetCustomAttribute<RequiredAttribute>();

            Assert.NotNull(attr);
        }

        [Fact]
        public void TestDto_HasStringLengthAttribute()
        {
            var property = typeof(TestUserDto).GetProperty("Name");
            var attr = property?.GetCustomAttribute<StringLengthAttribute>();

            Assert.NotNull(attr);
            Assert.Equal(50, attr.MaximumLength);
        }

        [Fact]
        public void TestDto_HasKeyAttribute()
        {
            var property = typeof(TestUserDto).GetProperty("Id");
            var attr = property?.GetCustomAttribute<KeyAttribute>();

            Assert.NotNull(attr);
        }

        [Fact]
        public void TestDto_HasDisplayAttribute()
        {
            var property = typeof(TestUserDto).GetProperty("Name");
            var attr = property?.GetCustomAttribute<DisplayAttribute>();

            Assert.NotNull(attr);
            Assert.Equal("用户名", attr.GetName());
        }

        #endregion
    }
}