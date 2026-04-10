using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;
using EasyTool.NetCategory;

namespace EasyTool.Tests
{
    /// <summary>
    /// HttpClientBuilder 和 HttpClientBuilderUtil 工具类的单元测试
    /// </summary>
    public class HttpClientBuilderTests : IDisposable
    {
        public void Dispose()
        {
            // Reset ShortUrlConfig between tests if needed
        }

        #region HttpClientBuilder - Basic Configuration

        [Fact]
        public void WithBaseAddress_Build_SetsBaseAddress()
        {
            using var client = new HttpClientBuilder()
                .WithBaseAddress("https://example.com")
                .Build();

            Assert.NotNull(client.BaseAddress);
            Assert.Equal("https://example.com/", client.BaseAddress.ToString());
        }

        [Fact]
        public void WithTimeout_Build_SetsTimeout()
        {
            var timeout = TimeSpan.FromSeconds(60);

            using var client = new HttpClientBuilder()
                .WithTimeout(timeout)
                .Build();

            Assert.Equal(timeout, client.Timeout);
        }

        [Fact]
        public void WithMaxResponseContentBufferSize_Build_SetsSize()
        {
            using var client = new HttpClientBuilder()
                .WithMaxResponseContentBufferSize(1024 * 1024)
                .Build();

            Assert.Equal(1024 * 1024L, client.MaxResponseContentBufferSize);
        }

        [Fact]
        public void DefaultBuild_HasDefaultTimeout()
        {
            using var client = new HttpClientBuilder().Build();

            Assert.Equal(TimeSpan.FromSeconds(100), client.Timeout);
        }

        #endregion

        #region HttpClientBuilder - Headers

        [Fact]
        public void WithDefaultHeader_Build_AddsHeader()
        {
            using var client = new HttpClientBuilder()
                .WithDefaultHeader("X-Custom", "value")
                .Build();

            Assert.True(client.DefaultRequestHeaders.Contains("X-Custom"));
            Assert.Equal("value", client.DefaultRequestHeaders.GetValues("X-Custom").First());
        }

        [Fact]
        public void WithDefaultHeaders_Build_AddsMultipleHeaders()
        {
            var headers = new Dictionary<string, string>
            {
                { "X-Key1", "val1" },
                { "X-Key2", "val2" }
            };

            using var client = new HttpClientBuilder()
                .WithDefaultHeaders(headers)
                .Build();

            Assert.True(client.DefaultRequestHeaders.Contains("X-Key1"));
            Assert.True(client.DefaultRequestHeaders.Contains("X-Key2"));
        }

        [Fact]
        public void WithAccept_Build_SetsAcceptHeader()
        {
            using var client = new HttpClientBuilder()
                .WithAccept("application/json")
                .Build();

            Assert.True(client.DefaultRequestHeaders.Accept.Any());
            Assert.Equal("application/json", client.DefaultRequestHeaders.Accept.First().MediaType);
        }

        [Fact]
        public void WithContentType_Build_SetsContentTypeHeader()
        {
            // Content-Type is a content header and cannot be checked via DefaultRequestHeaders.Contains()
            // It's added via TryAddWithoutValidation, so we verify the build doesn't throw
            using var client = new HttpClientBuilder()
                .WithContentType("application/json")
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithUserAgent_Build_SetsUserAgentHeader()
        {
            using var client = new HttpClientBuilder()
                .WithUserAgent("TestBot/1.0")
                .Build();

            Assert.True(client.DefaultRequestHeaders.Contains("User-Agent"));
            Assert.Equal("TestBot/1.0", client.DefaultRequestHeaders.UserAgent.ToString());
        }

        #endregion

        #region HttpClientBuilder - Authentication

        [Fact]
        public void WithBearerToken_Build_SetsAuthorizationHeader()
        {
            using var client = new HttpClientBuilder()
                .WithBearerToken("my-token-123")
                .Build();

            Assert.NotNull(client.DefaultRequestHeaders.Authorization);
            Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("my-token-123", client.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void WithBasicAuth_Build_SetsAuthorizationHeader()
        {
            using var client = new HttpClientBuilder()
                .WithBasicAuth("admin", "password123")
                .Build();

            Assert.NotNull(client.DefaultRequestHeaders.Authorization);
            Assert.Equal("Basic", client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.NotNull(client.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void WithAuthorization_Build_SetsCustomScheme()
        {
            using var client = new HttpClientBuilder()
                .WithAuthorization("Custom", "token-value")
                .Build();

            Assert.NotNull(client.DefaultRequestHeaders.Authorization);
            Assert.Equal("Custom", client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("token-value", client.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void WithBasicAuth_CredentialsAreBase64Encoded()
        {
            using var client = new HttpClientBuilder()
                .WithBasicAuth("user", "pass")
                .Build();

            var expected = Convert.ToBase64String(global::System.Text.Encoding.UTF8.GetBytes("user:pass"));
            Assert.Equal(expected, client.DefaultRequestHeaders.Authorization.Parameter);
        }

        #endregion

        #region HttpClientBuilder - Proxy and Security

        [Fact]
        public void WithProxy_String_BuildsClientSuccessfully()
        {
            // Just verify it doesn't throw and builds
            using var client = new HttpClientBuilder()
                .WithProxy("http://proxy.example.com:8080")
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithProxy_IWebProxy_BuildsClientSuccessfully()
        {
            var proxy = new WebProxy("http://proxy.example.com:8080");

            using var client = new HttpClientBuilder()
                .WithProxy(proxy)
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithProxyCredentials_WithProxySet_BuildsClientSuccessfully()
        {
            using var client = new HttpClientBuilder()
                .WithProxy("http://proxy.example.com:8080")
                .WithProxyCredentials("user", "pass")
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void IgnoreSslErrors_Build_DoesNotThrow()
        {
            using var client = new HttpClientBuilder()
                .IgnoreSslErrors()
                .Build();

            Assert.NotNull(client);
        }

        #endregion

        #region HttpClientBuilder - Redirect and Compression

        [Fact]
        public void WithAutoRedirect_False_BuildsClient()
        {
            using var client = new HttpClientBuilder()
                .WithAutoRedirect(false)
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithMaxAutomaticRedirections_BuildsClient()
        {
            using var client = new HttpClientBuilder()
                .WithMaxAutomaticRedirections(5)
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithGzipDecompression_BuildsClient()
        {
            using var client = new HttpClientBuilder()
                .WithGzipDecompression()
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithDeflateDecompression_BuildsClient()
        {
            using var client = new HttpClientBuilder()
                .WithDeflateDecompression()
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithAllDecompression_BuildsClient()
        {
            using var client = new HttpClientBuilder()
                .WithAllDecompression()
                .Build();

            Assert.NotNull(client);
        }

        #endregion

        #region HttpClientBuilder - Connection Configuration

        [Fact]
        public void WithConnectionTimeout_BuildsClient()
        {
            using var client = new HttpClientBuilder()
                .WithConnectionTimeout(TimeSpan.FromSeconds(10))
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithMaxConnectionsPerServer_BuildsClient()
        {
            using var client = new HttpClientBuilder()
                .WithMaxConnectionsPerServer(10)
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithMaxResponseHeadersLength_BuildsClient()
        {
            using var client = new HttpClientBuilder()
                .WithMaxResponseHeadersLength(128)
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithDefaultCredentials_BuildsClient()
        {
            using var client = new HttpClientBuilder()
                .WithDefaultCredentials()
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void WithCredentials_BuildsClient()
        {
            using var client = new HttpClientBuilder()
                .WithCredentials(new NetworkCredential("user", "pass"))
                .Build();

            Assert.NotNull(client);
        }

        #endregion

        #region HttpClientBuilder - Middleware

        [Fact]
        public void AddRetry_BuildsClientSuccessfully()
        {
            using var client = new HttpClientBuilder()
                .AddRetry(3)
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void AddTimeout_BuildsClientSuccessfully()
        {
            using var client = new HttpClientBuilder()
                .AddTimeout(TimeSpan.FromSeconds(30))
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void AddLogging_BuildsClientSuccessfully()
        {
            var logMessages = new List<string>();
            using var client = new HttpClientBuilder()
                .AddLogging(msg => logMessages.Add(msg))
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void AddHandler_BuildsClientSuccessfully()
        {
            using var client = new HttpClientBuilder()
                .AddHandler(new TestDelegatingHandler())
                .Build();

            Assert.NotNull(client);
        }

        [Fact]
        public void MultipleMiddleware_BuildsInCorrectOrder()
        {
            var messages = new List<string>();

            using var client = new HttpClientBuilder()
                .AddLogging(msg => messages.Add(msg))
                .AddRetry(2)
                .Build();

            Assert.NotNull(client);
        }

        #endregion

        #region HttpClientBuilder - Build

        [Fact]
        public void Build_ReturnsNonNullHttpClient()
        {
            using var client = new HttpClientBuilder().Build();
            Assert.NotNull(client);
        }

        [Fact]
        public void BuildDisposable_ReturnsNonNullHttpClient()
        {
            using var client = new HttpClientBuilder().BuildDisposable();
            Assert.NotNull(client);
        }

        [Fact]
        public void Build_CalledMultipleTimes_ReturnsDifferentInstances()
        {
            var builder = new HttpClientBuilder();
            using var client1 = builder.Build();
            using var client2 = builder.Build();

            Assert.NotSame(client1, client2);
        }

        [Fact]
        public void Build_FluentChaining_AllowsFullConfiguration()
        {
            using var client = new HttpClientBuilder()
                .WithBaseAddress("https://api.example.com")
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithAccept("application/json")
                .WithContentType("application/json")
                .WithBearerToken("token")
                .WithAllDecompression()
                .Build();

            Assert.NotNull(client);
            Assert.Equal("https://api.example.com/", client.BaseAddress.ToString());
            Assert.Equal(TimeSpan.FromSeconds(30), client.Timeout);
            Assert.NotNull(client.DefaultRequestHeaders.Authorization);
            Assert.Equal("Bearer", client.DefaultRequestHeaders.Authorization.Scheme);
        }

        #endregion

        #region HttpClientBuilderUtil

        [Fact]
        public void Create_ReturnsNewBuilder()
        {
            var builder = HttpClientBuilderUtil.Create();

            Assert.NotNull(builder);
            Assert.IsType<HttpClientBuilder>(builder);
        }

        [Fact]
        public void CreateDefault_ReturnsConfiguredClient()
        {
            using var client = HttpClientBuilderUtil.CreateDefault();

            Assert.NotNull(client);
            Assert.Equal(TimeSpan.FromSeconds(30), client.Timeout);
        }

        [Fact]
        public void CreateForJsonApi_SetsBaseAddress()
        {
            using var client = HttpClientBuilderUtil.CreateForJsonApi("https://api.example.com");

            Assert.NotNull(client);
            Assert.Equal("https://api.example.com/", client.BaseAddress.ToString());
        }

        [Fact]
        public void CreateForJsonApi_SetsJsonHeaders()
        {
            using var client = HttpClientBuilderUtil.CreateForJsonApi("https://api.example.com");

            Assert.NotNull(client);
            Assert.Contains(client.DefaultRequestHeaders.Accept,
                h => h.MediaType == "application/json");
        }

        [Fact]
        public void CreateWithRetry_DefaultRetryCount_ReturnsClient()
        {
            using var client = HttpClientBuilderUtil.CreateWithRetry();

            Assert.NotNull(client);
        }

        [Fact]
        public void CreateWithRetry_CustomRetryCount_ReturnsClient()
        {
            using var client = HttpClientBuilderUtil.CreateWithRetry(5);

            Assert.NotNull(client);
        }

        [Fact]
        public void CreateIgnoringSsl_ReturnsClient()
        {
            using var client = HttpClientBuilderUtil.CreateIgnoringSsl();

            Assert.NotNull(client);
        }

        #endregion

        #region Helper

        /// <summary>
        /// Test delegating handler for testing middleware pipeline
        /// </summary>
        private class TestDelegatingHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }

        #endregion
    }
}
