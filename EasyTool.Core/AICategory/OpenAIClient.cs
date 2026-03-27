using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EasyTool.AICategory
{
    /// <summary>
    /// OpenAI API 工具类
    /// 提供 GPT、DALL-E、Whisper 等 AI 服务的集成
    /// </summary>
    public class OpenAIClient
    {
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 创建 OpenAI 客户端
        /// </summary>
        /// <param name="apiKey">API Key</param>
        /// <param name="baseUrl">API 基础 URL（默认 OpenAI 官方）</param>
        public OpenAIClient(string apiKey, string? baseUrl = null)
        {
            _apiKey = apiKey;
            _baseUrl = baseUrl ?? "https://api.openai.com/v1";
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        #region Chat Completions

        /// <summary>
        /// 发送聊天请求
        /// </summary>
        /// <param name="messages">消息列表</param>
        /// <param name="model">模型名称</param>
        /// <param name="temperature">温度（0-2）</param>
        /// <param name="maxTokens">最大令牌数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>响应结果</returns>
        public async Task<ChatResponse> ChatAsync(List<ChatMessage> messages, string model = "gpt-3.5-turbo", double temperature = 0.7, int? maxTokens = null, CancellationToken cancellationToken = default)
        {
            var requestBody = new Dictionary<string, object>
            {
                ["model"] = model,
                ["messages"] = messages,
                ["temperature"] = temperature
            };

            if (maxTokens.HasValue)
                requestBody["max_tokens"] = maxTokens.Value;

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content, cancellationToken);
            var responseJson = await ReadContentAsStringAsync(response.Content);

            if (!response.IsSuccessStatusCode)
            {
                throw new OpenAIException($"API 请求失败: {response.StatusCode}", responseJson);
            }

            return JsonSerializer.Deserialize<ChatResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new OpenAIException("无法解析响应");
        }

        /// <summary>
        /// 发送简单聊天请求
        /// </summary>
        /// <param name="prompt">提示词</param>
        /// <param name="model">模型名称</param>
        /// <param name="temperature">温度</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>响应文本</returns>
        public async Task<string> ChatSimpleAsync(string prompt, string model = "gpt-3.5-turbo", double temperature = 0.7, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            };

            var response = await ChatAsync(messages, model, temperature, cancellationToken: cancellationToken);
            return response.Choices[0].Message.Content;
        }

        /// <summary>
        /// 流式聊天请求
        /// </summary>
        public async IAsyncEnumerable<string> ChatStreamAsync(List<ChatMessage> messages, string model = "gpt-3.5-turbo", double temperature = 0.7, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var requestBody = new Dictionary<string, object>
            {
                ["model"] = model,
                ["messages"] = messages,
                ["temperature"] = temperature,
                ["stream"] = true
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions")
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await ReadContentAsStreamAsync(response.Content);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                    continue;

                var data = line.Substring(6);
                if (data == "[DONE]")
                    break;

                var chunkResponse = JsonSerializer.Deserialize<ChatStreamResponse>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (chunkResponse?.Choices?[0]?.Delta?.Content != null)
                {
                    yield return chunkResponse.Choices[0].Delta.Content;
                }
            }
        }

        #endregion

        #region Embeddings

        /// <summary>
        /// 获取文本嵌入向量
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="model">模型名称</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>嵌入向量</returns>
        public async Task<float[]> GetEmbeddingAsync(string text, string model = "text-embedding-ada-002", CancellationToken cancellationToken = default)
        {
            var requestBody = new Dictionary<string, object>
            {
                ["model"] = model,
                ["input"] = text
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/embeddings", content, cancellationToken);
            var responseJson = await ReadContentAsStringAsync(response.Content);

            if (!response.IsSuccessStatusCode)
            {
                throw new OpenAIException($"API 请求失败: {response.StatusCode}", responseJson);
            }

            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return embeddingResponse?.Data?[0]?.Embedding ?? Array.Empty<float>();
        }

        /// <summary>
        /// 批量获取嵌入向量
        /// </summary>
        public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts, string model = "text-embedding-ada-002", CancellationToken cancellationToken = default)
        {
            var requestBody = new Dictionary<string, object>
            {
                ["model"] = model,
                ["input"] = texts
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/embeddings", content, cancellationToken);
            var responseJson = await ReadContentAsStringAsync(response.Content);

            if (!response.IsSuccessStatusCode)
            {
                throw new OpenAIException($"API 请求失败: {response.StatusCode}", responseJson);
            }

            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var result = new List<float[]>();
            if (embeddingResponse?.Data != null)
            {
                foreach (var item in embeddingResponse.Data)
                {
                    result.Add(item.Embedding ?? Array.Empty<float>());
                }
            }

            return result;
        }

        #endregion

        #region Image Generation

        /// <summary>
        /// 生成图像
        /// </summary>
        /// <param name="prompt">提示词</param>
        /// <param name="size">尺寸（256x256, 512x512, 1024x1024）</param>
        /// <param name="n">生成数量</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>图像 URL 列表</returns>
        public async Task<List<string>> GenerateImageAsync(string prompt, string size = "1024x1024", int n = 1, CancellationToken cancellationToken = default)
        {
            var requestBody = new Dictionary<string, object>
            {
                ["prompt"] = prompt,
                ["size"] = size,
                ["n"] = n
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/images/generations", content, cancellationToken);
            var responseJson = await ReadContentAsStringAsync(response.Content);

            if (!response.IsSuccessStatusCode)
            {
                throw new OpenAIException($"API 请求失败: {response.StatusCode}", responseJson);
            }

            var imageResponse = JsonSerializer.Deserialize<ImageResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var result = new List<string>();
            if (imageResponse?.Data != null)
            {
                foreach (var item in imageResponse.Data)
                {
                    if (!string.IsNullOrEmpty(item.Url))
                        result.Add(item.Url);
                }
            }

            return result;
        }

        #endregion

        #region Audio

        /// <summary>
        /// 语音转文字
        /// </summary>
        /// <param name="audioFilePath">音频文件路径</param>
        /// <param name="model">模型名称</param>
        /// <param name="language">语言（如 "zh", "en"）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>转录文本</returns>
        public async Task<string> TranscribeAsync(string audioFilePath, string model = "whisper-1", string? language = null, CancellationToken cancellationToken = default)
        {
            using var formContent = new MultipartFormDataContent();
            formContent.Add(new StreamContent(File.OpenRead(audioFilePath)), "file", Path.GetFileName(audioFilePath));
            formContent.Add(new StringContent(model), "model");

            if (!string.IsNullOrEmpty(language))
                formContent.Add(new StringContent(language), "language");

            var response = await _httpClient.PostAsync($"{_baseUrl}/audio/transcriptions", formContent, cancellationToken);
            var responseJson = await ReadContentAsStringAsync(response.Content);

            if (!response.IsSuccessStatusCode)
            {
                throw new OpenAIException($"API 请求失败: {response.StatusCode}", responseJson);
            }

            var transcriptionResponse = JsonSerializer.Deserialize<TranscriptionResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return transcriptionResponse?.Text ?? string.Empty;
        }

        /// <summary>
        /// 文字转语音
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="outputFilePath">输出文件路径</param>
        /// <param name="model">模型名称</param>
        /// <param name="voice">声音（alloy, echo, fable, onyx, nova, shimmer）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否成功</returns>
        public async Task<bool> TextToSpeechAsync(string text, string outputFilePath, string model = "tts-1", string voice = "alloy", CancellationToken cancellationToken = default)
        {
            var requestBody = new Dictionary<string, object>
            {
                ["model"] = model,
                ["input"] = text,
                ["voice"] = voice
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/audio/speech", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await ReadContentAsStringAsync(response.Content);
                throw new OpenAIException($"API 请求失败: {response.StatusCode}", errorJson);
            }

            var audioData = await ReadContentAsByteArrayAsync(response.Content);
            await File.WriteAllBytesAsync(outputFilePath, audioData, cancellationToken);

            return true;
        }

        #endregion

        #region Helper Methods

        private static async Task<string> ReadContentAsStringAsync(HttpContent content)
        {
#if NETSTANDARD2_1
            return await content.ReadAsStringAsync();
#else
            return await content.ReadAsStringAsync(default);
#endif
        }

        private static async Task<Stream> ReadContentAsStreamAsync(HttpContent content)
        {
#if NETSTANDARD2_1
            return await content.ReadAsStreamAsync();
#else
            return await content.ReadAsStreamAsync(default);
#endif
        }

        private static async Task<byte[]> ReadContentAsByteArrayAsync(HttpContent content)
        {
#if NETSTANDARD2_1
            return await content.ReadAsByteArrayAsync();
#else
            return await content.ReadAsByteArrayAsync(default);
#endif
        }

        #endregion
    }

    #region 数据模型

    /// <summary>
    /// 聊天消息
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// 角色（system, user, assistant）
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// 聊天响应
    /// </summary>
    public class ChatResponse
    {
        /// <summary>
        /// 响应 ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 选择列表
        /// </summary>
        public List<ChatChoice> Choices { get; set; } = new();

        /// <summary>
        /// 使用情况
        /// </summary>
        public UsageInfo? Usage { get; set; }
    }

    /// <summary>
    /// 聊天选择
    /// </summary>
    public class ChatChoice
    {
        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public ChatMessage Message { get; set; } = new();

        /// <summary>
        /// 结束原因
        /// </summary>
        public string? FinishReason { get; set; }
    }

    /// <summary>
    /// 流式响应
    /// </summary>
    public class ChatStreamResponse
    {
        public List<ChatStreamChoice>? Choices { get; set; }
    }

    /// <summary>
    /// 流式选择
    /// </summary>
    public class ChatStreamChoice
    {
        public ChatStreamDelta? Delta { get; set; }
    }

    /// <summary>
    /// 流式增量
    /// </summary>
    public class ChatStreamDelta
    {
        public string? Content { get; set; }
    }

    /// <summary>
    /// 使用情况
    /// </summary>
    public class UsageInfo
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// 嵌入响应
    /// </summary>
    public class EmbeddingResponse
    {
        public List<EmbeddingData>? Data { get; set; }
    }

    /// <summary>
    /// 嵌入数据
    /// </summary>
    public class EmbeddingData
    {
        public float[]? Embedding { get; set; }
    }

    /// <summary>
    /// 图像响应
    /// </summary>
    public class ImageResponse
    {
        public List<ImageData>? Data { get; set; }
    }

    /// <summary>
    /// 图像数据
    /// </summary>
    public class ImageData
    {
        public string? Url { get; set; }
    }

    /// <summary>
    /// 转录响应
    /// </summary>
    public class TranscriptionResponse
    {
        public string? Text { get; set; }
    }

    /// <summary>
    /// OpenAI 异常
    /// </summary>
    public class OpenAIException : Exception
    {
        public string? ResponseJson { get; }

        public OpenAIException(string message, string? responseJson = null) : base(message)
        {
            ResponseJson = responseJson;
        }
    }

    #endregion
}