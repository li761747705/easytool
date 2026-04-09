# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[1.1.0]: https://github.com/dotnet-easy/easytool/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/dotnet-easy/easytool/releases/tag/v1.0.0