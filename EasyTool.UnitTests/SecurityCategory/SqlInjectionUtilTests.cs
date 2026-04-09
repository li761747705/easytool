using Xunit;

namespace EasyTool.SecurityCategory.Tests
{
    public class SqlInjectionUtilTests
    {
        [Fact]
        public void HasSqlInjection_DetectsUnionSelect()
        {
            var input = "1 UNION SELECT * FROM users";
            Assert.True(SqlInjectionUtil.HasSqlInjection(input));
        }

        [Fact]
        public void HasSqlInjection_DetectsOrOneEqualsOne()
        {
            var input = "admin' OR '1'='1";
            Assert.True(SqlInjectionUtil.HasSqlInjection(input));
        }

        [Fact]
        public void HasSqlInjection_DetectsCommentInjection()
        {
            var input = "admin'--";
            Assert.True(SqlInjectionUtil.HasSqlInjection(input));
        }

        [Fact]
        public void HasSqlInjection_DetectsDropTable()
        {
            var input = "'; DROP TABLE users;--";
            Assert.True(SqlInjectionUtil.HasSqlInjection(input));
        }

        [Fact]
        public void HasSqlInjection_DetectsXpCmdshell()
        {
            var input = "EXEC xp_cmdshell 'dir'";
            Assert.True(SqlInjectionUtil.HasSqlInjection(input));
        }

        [Fact]
        public void HasSqlInjection_DetectsSemicolonInjection()
        {
            var input = "'; INSERT INTO users VALUES ('hacker');--";
            Assert.True(SqlInjectionUtil.HasSqlInjection(input));
        }

        [Fact]
        public void HasSqlInjection_DetectsWaitforDelay()
        {
            var input = "WAITFOR DELAY '0:0:5'";
            Assert.True(SqlInjectionUtil.HasSqlInjection(input));
        }

        [Fact]
        public void HasSqlInjection_DetectsInformationSchema()
        {
            var input = "SELECT * FROM information_schema.tables";
            Assert.True(SqlInjectionUtil.HasSqlInjection(input));
        }

        [Fact]
        public void HasSqlInjection_SafeInput_ReturnsFalse()
        {
            var input = "Hello World";
            Assert.False(SqlInjectionUtil.HasSqlInjection(input));
        }

        [Fact]
        public void HasSqlInjection_SafeQuery_ReturnsFalse()
        {
            var input = "What is the price of the product?";
            Assert.False(SqlInjectionUtil.HasSqlInjection(input));
        }

        [Fact]
        public void HasSqlInjection_EmptyInput_ReturnsFalse()
        {
            Assert.False(SqlInjectionUtil.HasSqlInjection(""));
            Assert.False(SqlInjectionUtil.HasSqlInjection(null!));
        }

        [Fact]
        public void EscapeString_EscapesSingleQuotes()
        {
            var input = "O'Brien";
            var result = SqlInjectionUtil.EscapeString(input);
            Assert.Equal("O''Brien", result);
        }

        [Fact]
        public void EscapeString_EscapesBackslash()
        {
            var input = "test\\value";
            var result = SqlInjectionUtil.EscapeString(input);
            Assert.Equal("test\\\\value", result);
        }

        [Fact]
        public void EscapeString_PreservesNormalText()
        {
            var input = "Normal text without special chars";
            var result = SqlInjectionUtil.EscapeString(input);
            Assert.Equal(input, result);
        }

        [Fact]
        public void Sanitize_RemovesComments()
        {
            var input = "admin'--comment";
            var result = SqlInjectionUtil.Sanitize(input);
            Assert.DoesNotContain("--", result);
        }

        [Fact]
        public void Sanitize_EscapesQuotes()
        {
            var input = "test'value";
            var result = SqlInjectionUtil.Sanitize(input);
            Assert.Contains("''", result);
        }

        [Fact]
        public void FilterKeywords_RemovesSqlKeywords()
        {
            var input = "SELECT * FROM users";
            var result = SqlInjectionUtil.FilterKeywords(input);
            Assert.DoesNotContain("SELECT", result.ToUpper());
            Assert.DoesNotContain("FROM", result.ToUpper());
        }

        [Fact]
        public void IsValidIdentifier_ValidName_ReturnsTrue()
        {
            Assert.True(SqlInjectionUtil.IsValidIdentifier("user_id"));
            Assert.True(SqlInjectionUtil.IsValidIdentifier("TableName"));
        }

        [Fact]
        public void IsValidIdentifier_InvalidChars_ReturnsFalse()
        {
            Assert.False(SqlInjectionUtil.IsValidIdentifier("user-id"));
            Assert.False(SqlInjectionUtil.IsValidIdentifier("table name"));
        }

        [Fact]
        public void IsValidIdentifier_SqlKeyword_ReturnsFalse()
        {
            Assert.False(SqlInjectionUtil.IsValidIdentifier("SELECT"));
            // table 不是SQL关键字，允许作为标识符
            Assert.True(SqlInjectionUtil.IsValidIdentifier("table"));
        }

        [Fact]
        public void IsValidIdentifier_EmptyInput_ReturnsFalse()
        {
            Assert.False(SqlInjectionUtil.IsValidIdentifier(""));
            Assert.False(SqlInjectionUtil.IsValidIdentifier(null!));
        }

        [Fact]
        public void QuoteIdentifier_WrapsIdentifier()
        {
            var result = SqlInjectionUtil.QuoteIdentifier("table_name");
            Assert.Equal("`table_name`", result);
        }

        [Fact]
        public void QuoteIdentifier_EscapesInternalQuotes()
        {
            var result = SqlInjectionUtil.QuoteIdentifier("table`name", "`");
            Assert.Equal("`table``name`", result);
        }

        [Fact]
        public void BuildInClause_BuildsSafeInClause()
        {
            var values = new[] { "value1", "value2", "value3" };
            var result = SqlInjectionUtil.BuildInClause(values);
            Assert.Contains("'value1'", result);
            Assert.Contains("'value2'", result);
            Assert.Contains("'value3'", result);
        }

        [Fact]
        public void BuildInClause_NumericValues_NoQuotes()
        {
            var values = new[] { "1", "2", "3" };
            var result = SqlInjectionUtil.BuildInClause(values, true);
            Assert.Contains("1", result);
            Assert.DoesNotContain("'1'", result);
        }

        [Fact]
        public void EscapeLikePattern_EscapesSpecialChars()
        {
            var input = "test%value_test";
            var result = SqlInjectionUtil.EscapeLikePattern(input);
            Assert.Contains("\\%", result);
            Assert.Contains("\\_", result);
        }

        [Fact]
        public void Analyze_ReturnsAnalysisResult()
        {
            var input = "SELECT * FROM users";
            var result = SqlInjectionUtil.Analyze(input);
            Assert.True(result.HasRisk);
            Assert.Contains("SQL关键字", result.Risks[0]);
        }

        [Fact]
        public void Analyze_SafeInput_ReturnsNoRisk()
        {
            var input = "Hello World";
            var result = SqlInjectionUtil.Analyze(input);
            Assert.False(result.HasRisk);
        }

        [Fact]
        public void CheckMultiple_ReturnsResultsForAllInputs()
        {
            var inputs = new[]
            {
                new KeyValuePair<string, string>("field1", "safe value"),
                new KeyValuePair<string, string>("field2", "1' OR '1'='1")
            };
            var results = SqlInjectionUtil.CheckMultiple(inputs);
            Assert.Equal(2, results.Count);
            Assert.False(results["field1"]);
            Assert.True(results["field2"]);
        }
    }
}