# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.0] - 2026-04-10

### 🐛 Bug Fixes (CRITICAL)

- **RingBuffer.TryRead** - 修复返回值永远为 `true` 的逻辑错误，`_count >= 0` 恒为 true 导致 `||` 短路，现已在 lock 内完整重写读取逻辑
- **EscapeUtil.UnescapeXml** - 修复 XML 反转义顺序错误，`&amp;` 必须最后替换，否则 `&amp;lt;` 会被错误解码为 `<`
- **TextCleaner.UnescapeXml** - 同上，修复 XML 反转义顺序
- **AesUtil** - 默认加密模式从不安全的 ECB 改为 CBC；修复 `IsLegalSize` 使用字符长度而非 UTF-8 字节长度校验密钥的 Bug
- **DesUtil** - 修复 IV 复用 Key 的安全问题，改用 `GenerateIV()`；默认模式从 ECB 改为 CBC
- **TwoFactorAuthUtil.Base32Decode** - 添加输入校验，非法字符抛出 `FormatException` 而非 `IndexOutOfRangeException`；添加输出边界检查

### ⚡ Performance Improvements

- **StringExtension** - `IsEmail`、`IsPhoneNumber`、`IsUrl`、`IsIPv4`、`IsIdCard` 等 5 个正则表达式提取为 `static readonly Regex` 编译缓存，避免每次调用重新编译
- **PasswordUtil.GenerateRandom** - 循环内字符串拼接 `password += char` 改为 `char[]` + `new string()`，减少 GC 压力
- **CollUtil.Random** - `OrderBy(random.Next())` O(n log n) 改为 Fisher-Yates 部分洗牌 O(n)
- **DateTimeUtil.GetMonthDays** - 使用 `DateTime.DaysInMonth` 预分配 `List<DateTime>` 容量，减少扩容

### 🛡️ Security & Safety

- **FakerUtil.RandomInt** - 修复模偏差（modulo bias）和 `int.MinValue` 溢出问题，改用拒绝采样法（rejection sampling）生成均匀分布的随机数
- **KeywordExtractor** - 修复 `AddStopWords` 修改静态集合的线程安全问题，`HashSet` 改为 `ConcurrentDictionary`
- **ReflectUtil.InvokeGenericMethod** - 添加 `null` 检查，方法未找到时抛出 `MissingMethodException` 而非 `NullReferenceException`
- **JsonUtil.DefaultOptions** - 懒加载改为 `Lazy<T>`，保证线程安全

### 🔄 Changed (Breaking Changes)

- **命名空间统一** - 以下类从根命名空间 `EasyTool` 移入对应的 Category 命名空间：
  - `RsaUtil`、`EcdsaUtil` → `EasyTool.CodeCategory`
  - `ArrayUtil`、`CollUtil`、`MapUtil` → `EasyTool.CollectionsCategory`
  - `JsonUtil`、`TextCleaner`、`EscapeUtil` → `EasyTool.TextCategory`
  - `BeanUtil`、`RecordUtil`、`ConsoleUtil` → `EasyTool.ToolCategory`
  - `QueryBuilder` → `EasyTool.DataCategory`
  - `RingBuffer` → `EasyTool.QueueCategory`
  - `FileTypeUtil` → `EasyTool.IOCategory`
  - `ModifierUtil` → `EasyTool.ReflectCategory`

### 📝 Code Quality

- **错误消息统一** - 全项目 47 个文件的英文错误消息统一翻译为中文，保持错误消息语言一致性
- **AesUtil** - 移除未使用的 `using static System.Net.Mime.MediaTypeNames`

### 🧪 Tests

- 更新 AES/DES 测试用例，适配 CBC 默认模式（使用带 IV 的重载）
- 更新 TwoFactorAuthUtil 测试，异常类型从 `IndexOutOfRangeException` 改为 `FormatException`
- 新增 AES/DES 字节数组版本测试
- 总测试数从 288 增至 318（全部通过）

### ⚠️ Migration Guide (1.2.x → 1.3.0)

#### 命名空间变更

```csharp
// Before (1.2.x)
using EasyTool;  // RsaUtil, CollUtil, ArrayUtil, JsonUtil 等

// After (1.3.0)
using EasyTool.CodeCategory;       // RsaUtil, EcdsaUtil
using EasyTool.CollectionsCategory; // ArrayUtil, CollUtil, MapUtil
using EasyTool.TextCategory;       // JsonUtil, TextCleaner, EscapeUtil
using EasyTool.ToolCategory;       // BeanUtil, RecordUtil, ConsoleUtil
using EasyTool.DataCategory;       // QueryBuilder
using EasyTool.QueueCategory;      // RingBuffer
using EasyTool.IOCategory;         // FileTypeUtil
using EasyTool.ReflectCategory;    // ModifierUtil
```

#### AES/DES 默认模式变更

```csharp
// Before (1.2.x) - 无 IV 的重载默认 ECB，每次加密结果相同
var encrypted = AesUtil.Encrypt(text, key);

// After (1.3.0) - 无 IV 的重载默认 CBC + 随机 IV，每次加密结果不同
// 推荐使用带 IV 的重载以确保可解密
var encrypted = AesUtil.Encrypt(text, key, iv);
var decrypted = AesUtil.Decrypt(encrypted, key, iv);
```

---

## [1.2.1] - 2026-04-10

### 🔄 Changed

- **Nullable Reference Types**
  - Upgraded from `annotations` to `enable` for full null-safety checking
  - Build passes with 0 errors

### ✨ Added

#### Fluent Extensions

- **CollectionExtensions** - Collection manipulation extensions
  - `ForEach()` with chain support
  - `IsNullOrEmpty()` / `IsNotNullOrEmpty()`
  - `JoinAsString()` for easy string joining
  - `DistinctBy()` for property-based deduplication
  - `Batch()` for chunked processing
  - `RandomElement()` and `Shuffle()` for random operations

- **DateTimeExtensions** - Date/time extensions
  - `ToDateString()` / `ToDateTimeString()` formatting
  - `IsToday()`, `IsWeekday()` checks
  - `GetAge()`, `GetQuarter()` utilities
  - `ToTimestamp()` / `ToTimestampMs()` conversions

- **NumberExtensions** - Number extensions
  - `InRange()` and `Clamp()` for range operations
  - `ToChinese()` for Chinese number conversion
  - `ToMoneyChinese()` for money amount in Chinese
  - `ToFileSize()` for human-readable file sizes

#### Object Pool Enhancements

- **StringBuilderPool** - Pooled StringBuilder for reduced GC pressure
- **MemoryStreamPool** - Pooled MemoryStream for stream operations
- **ByteArrayPool** - ArrayPool&lt;byte&gt; wrapper for byte arrays
- **CharArrayPool** - ArrayPool&lt;char&gt; wrapper for char arrays

### 🛡️ Safety Improvements

- **FakerUtil** - Added parameter validation with friendly error messages
  - `RandomInt()` validates `max > 0` and `min < max`
  - `RandomMoney()` validates `min < max`
  - `RandomDate()` validates valid year range
  - `RandomChoice()` validates non-empty collection

### ⚡ Performance Optimizations

- **Regex Compilation** - Added `RegexOptions.Compiled` to frequently used patterns
  - `DesensitizedUtil` - 5 regex patterns compiled
  - `KeywordExtractor` - 6 regex patterns compiled

- **String Operations** - Replaced string concatenation with StringBuilder
  - `TaxNumberUtil.GenerateRandomCode()` optimized

### 📝 Code Quality

- **EditorConfig** - Added comprehensive code style rules
  - Naming conventions (PascalCase, _camelCase, IPascalCase)
  - Code style settings (var usage, expression-bodied members)
  - Indentation and spacing rules

- **.gitignore** - Added missing ignore patterns
  - Environment files (.env)
  - OS generated files (.DS_Store, Thumbs.db)
  - Merge conflict backups
  - User secrets

- **Test Project** - Disabled XML documentation generation
  - Reduced warnings from 1525 to 1176

---

## [1.2.0] - 2026-04-10

### ✨ Added

#### Security & Authentication

- **PasswordGenerator** - Secure password generator
  - Configurable length and character sets
  - Password strength checking (Weak/Fair/Good/Strong/VeryStrong)
  - PIN code generation
  - Passphrase generation with word combinations
  - Batch generation support

- **TwoFactorAuthUtil** - TOTP two-factor authentication
  - Compatible with Google Authenticator, Authy, etc.
  - Base32 secret generation
  - 6/8 digit TOTP code generation
  - Code verification with time tolerance
  - QR code content generation for easy setup

#### Network Utilities

- **HttpRetryUtil** - HTTP retry with exponential backoff
  - Configurable retry count and delays
  - Jitter support for distributed systems
  - Circuit breaker pattern implementation
  - Automatic request cloning for retries

- **ShortUrlUtil** - Short URL generation
  - Random short code generation
  - URL-based deterministic short codes
  - Base62 encoding for numeric IDs
  - Third-party service integration (is.gd, v.gd, tinyurl)

#### Data Generation

- **FakerUtil** - Chinese mock data generator
  - Chinese name generation (male/female)
  - Chinese address generation with realistic components
  - Phone number generation with valid prefixes
  - Email generation with common domains
  - Random utilities (int, string, money, date, bool)

#### Business Utilities

- **WeatherUtil** - Weather query utility
  - Current weather query
  - 7-day forecast
  - Air quality index
  - Supports QWeather (和风天气) API

- **PdfUtil** - PDF manipulation utility (placeholder)
  - PDF merge, split, watermark support
  - Requires iTextSharp or PdfSharp NuGet package

### 🔄 Changed

- **Solution Structure Optimization**
  - Reorganized into solution folders: Core, Extensions, Integration, Tests
  - Added `.Solution Items` for configuration files

- **Central Package Management**
  - Introduced `Directory.Packages.props` for unified NuGet package versions
  - All project files updated to use centralized version management

- **.NET Standard 2.1 Compatibility**
  - Fixed `Convert.ToHexString` (not available in .NET Standard 2.1)
  - Fixed `ReadAsStringAsync(cancellationToken)` overload issue
  - Fixed switch expression type inference

### 🧪 Tests

- Added comprehensive unit tests for new utilities
  - PasswordGeneratorTests (14 tests)
  - TwoFactorAuthUtilTests (12 tests)
  - FakerUtilTests (17 tests)
- Total test count: 288 (all passing)

---

## [1.1.1] - 2026-04-09

### ✨ Added

#### Chinese-Specific Business Utilities

- **ChineseNameUtil** - Chinese name generator
  - Random generation with common surnames (100+) and compound surnames (16)
  - Gender-specific name characters
  - Batch generation support

- **UniversityUtil** - Chinese university information
  - 985/211/Double FirstClass flags
  - Search by code, name, province, city
  - University type and level classification

- **PhoneLocationUtil** - Phone number location lookup
  - Carrier identification (Mobile/Unicom/Telecom)
  - Province and city lookup by phone number
  - Area code and zip code information

- **CompanyUtil** - Company name generator
  - Industry-specific name generation
  - Company type variations
  - Full company info generation with address

- **AddressUtil** - Chinese address generator
  - Province/City/District hierarchy support
  - Realistic road and community names
  - Building and commercial area names

#### Chinese Text Utilities

- **ChineseNumberUtil** - Chinese number conversion
  - Number to Chinese (简体/繁体)
  - Chinese to number
  - Money amount to Chinese uppercase (金额大写)

- **RegionUtil** - Administrative region utilities
  - Province/City/District three-level hierarchy
  - Code lookup and name search
  - Full path generation

- **ChineseHolidayUtil** - Chinese holiday utilities
  - Legal holiday and workday determination
  - Adjusted workday (调休) support
  - Traditional holiday detection
  - Workday calculation between dates

- **ChinesePinyinUtil** - Chinese pinyin conversion
  - Hanzi to pinyin conversion
  - Pinyin initial extraction
  - Tone number support

- **PlateNumberUtil** - Vehicle plate number utilities
  - Plate number validation
  - Location lookup (province/city)
  - New energy plate detection

- **SolarTermUtil** - 24 solar terms utilities
  - Solar term query for specific date
  - Next/previous solar term
  - Season determination

- **SocialCreditCodeUtil** - Unified social credit code utilities
  - Credit code validation
  - Institution type parsing
  - Department and region extraction

### 🔄 Changed

- **Test Project Consolidation**
  - Merged `EasyTool.CoreTests` into `EasyTool.UnitTests`
  - Converted MSTest format to xUnit format
  - Removed duplicate test project

### 🐛 Fixed

- Fixed XML comment format errors in `NPOIUtil.cs` (escaped `<T>` to `&lt;T&gt;`)
- Fixed async test warnings in `SpellCheckerUtilExtendedTests.cs` (changed `.Wait()` to `await`)

### 📚 Documentation

- Updated project structure documentation
- Clarified project positioning: lightweight, zero-dependency, filling gaps, Chinese-friendly

---

## [1.1.0] - 2026-04-07

### 🎉 Major Changes

This release brings a major modular restructuring and significant feature enhancements.

### ✨ Added

#### New Modules

- **EasyTool.AI** - AI integration module
  - `ILLMClient` - Unified LLM client interface
  - `OpenAIClient` - OpenAI API client with chat, embeddings, image generation, TTS/STT
  - `AzureOpenAIClient` - Azure OpenAI service client
  - `OllamaClient` - Local LLM client for Ollama
  - `LLMClientFactory` - Factory for creating LLM clients
  - `TokenizerUtil` - Token counting for GPT models
  - `VectorSimilarity` - Vector similarity calculations
  - `EmbeddingUtil` - Text embedding utilities
  - `PromptBuilder` - Prompt template builder
  - `KeywordExtractor` - Keyword extraction
  - `TextSummarizer` - Text summarization

- **EasyTool.Media** - Media processing module
  - `ImageUtil` - Image processing (resize, crop, watermark, compress, format conversion)
  - `VideoUtil` - Video processing (convert, compress, trim, merge, GIF creation, screenshots)
  - `AudioUtil` - Audio processing (convert, trim, merge, volume adjustment)
  - `QrCodeUtil` - QR code generation and reading
  - `ImageMetadataUtil` - Image metadata extraction

- **EasyTool.System** - System utilities module
  - System information, process management, hardware info
  - Clipboard, keyboard/mouse simulation

- **EasyTool.All** - Integration package that references all modules

#### New Features in Core

- `RsaUtil` - RSA encryption/decryption and signing
- `EcdsaUtil` - ECDSA digital signature
- `IpLocationUtil` - IP geolocation lookup
- `UrlBuilder` - URL builder utility
- `TempFileManager` - Temporary file management
- `CsvExporter` - CSV export utility
- `QueryBuilder` - SQL query builder
- `IdCardGenerator` - ID card number generator (for testing)
- `MockDataGenerator` - Mock data generator
- `DynamicBuilder` - Dynamic type builder
- `SensitiveWordFilter` - Sensitive word filtering
- `TextCleaner` - Text cleaning utility
- `JsonUtil` - JSON utilities
- `RingBuffer` - Ring buffer (moved from CollectionsCategory to QueueCategory)

### 🔄 Changed

- **Module Restructuring**
  - `AICategory` moved from Core to `EasyTool.AI` module
  - `MediaCategory` moved from Core to `EasyTool.Media` module
  - `SystemCategory` moved from Core to `EasyTool.System` module
  - `ConcurrencyCategory` merged into `ToolCategory`
  - `PerformanceCategory` merged into `ToolCategory`

- **Code Improvements**
  - `IdCardUtil` - Enhanced with more validation methods
  - `BankCardUtil` - Improved BIN code database
  - `HttpUtil` - Simplified and optimized
  - All crypto utilities now support nullable reference types

### ❌ Removed

- `CacheCategory` - Use Microsoft.Extensions.Caching directly
- `DatabaseCategory` - Use Dapper/EF Core directly
- `FtpUtil` - Use FluentFTP library directly
- `GrpcUtil` - Use Grpc.Net.Client directly
- `MailUtil` / `SmtpUtil` - Use MailKit directly
- `WebSocketUtil` - Use System.Net.WebSockets directly
- `SseUtil` - Use custom implementation or SignalR
- `WebhookUtil` - Too application-specific
- `ProxyUtil` - Too application-specific
- `HttpClientBuilder` / `HttpClientPool` / `HttpClientExtension` - Use IHttpClientFactory

### 🐛 Fixed

- Fixed token counting edge cases in `TokenizerUtil`
- Fixed age calculation in `IdCardUtil` tests
- Fixed regex pattern in `PasswordStrengthUtil`

### 🔒 Security

- All crypto utilities now use constant-time comparison
- Improved random number generation security

---

## [1.0.0] - 2026-01-08

### Added

- Initial release with core utilities
- CodeCategory: Base encoding, hashing, encryption, national cryptography (SM2/SM3/SM4)
- BusinessCategory: 30+ validation types (ID card, phone, bank card, email, etc.)
- TextCategory: Pinyin, sensitive words, desensitization, regex utilities
- CollectionsCategory: Pagination, deduplication, Bloom filter, Trie tree
- DateTimeCategory: Lunar calendar, holidays, Cron expressions
- IOCategory: File operations, compression, monitoring
- MathCategory: Random numbers, combinations, statistics
- NetCategory: HTTP client, DNS, IP utilities
- SecurityCategory: XSS filtering, SQL injection prevention
- And more...

---

## Migration Guide

### From 1.0.x to 1.1.0

#### Namespace Changes

```csharp
// Before
using EasyTool.AICategory;
using EasyTool.MediaCategory;
using EasyTool.SystemCategory;

// After
using EasyTool.AI;
using EasyTool.Media;
using EasyTool.System;
```

#### Removed Features

If you were using removed features, here are the recommended alternatives:

| Removed | Alternative |
|---------|-------------|
| `CacheCategory` | `Microsoft.Extensions.Caching.Memory` |
| `DatabaseCategory` | `Dapper` or `EF Core` |
| `FtpUtil` | `FluentFTP` NuGet package |
| `MailUtil` | `MailKit` NuGet package |
| `WebSocketUtil` | `System.Net.WebSockets` |

#### New AI Module

```csharp
// OpenAI
var client = new OpenAIClient("api-key");
var response = await client.ChatSimpleAsync("Hello!");

// Azure OpenAI
var azureClient = new AzureOpenAIClient(
    "https://your-resource.openai.azure.com/",
    "api-key",
    "gpt-4-deployment");

// Ollama (local)
var ollamaClient = new OllamaClient("http://localhost:11434", "llama2");
var localResponse = await ollamaClient.ChatSimpleAsync("Hello!");
```

---

[1.1.0]: https://github.com/li761747705/easytool/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/li761747705/easytool/releases/tag/v1.0.0