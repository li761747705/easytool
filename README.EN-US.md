<h1 align="center"> EasyTool </h1>
<div align="center">
An open-source .NET utility library inspired by Java Hutool, making development simpler and more efficient
</div>
<div align="center">

[![pull_request](https://github.com/li761747705/easytool/actions/workflows/pull_request.yml/badge.svg)](https://github.com/li761747705/easytool/actions/workflows/pull_request.yml)
[![](https://img.shields.io/nuget/v/EasyTool.Core.svg)](https://www.nuget.org/packages/EasyTool.Core)
[![](https://img.shields.io/badge/.NET-netstandard2.1-blue)](https://learn.microsoft.com/dotnet/standard/net-standard)
[![](https://img.shields.io/badge/Tests-1069+-brightgreen)](https://github.com/li761747705/easytool)
[![](https://img.shields.io/badge/Utilities-300+-orange)](https://github.com/li761747705/easytool)
<p>
    <a href="README.md">中文</a> | <span>English</span>
</p>
</div>

## 📚 Introduction

EasyTool is a **lightweight, comprehensive, Chinese-friendly** .NET utility library built on `netstandard2.1`, covering most utility needs in daily development.

### 🎯 Key Features

- ✅ **Lightweight** - Core package has zero external dependencies
- ✅ **Comprehensive** - 300+ utility classes covering encoding, encryption, collections, text, networking, IO and more
- ✅ **Chinese-Friendly** - Pinyin conversion, sensitive word filtering, ID card/bank card/phone validation, lunar calendar, solar terms
- ✅ **Reliable** - 1069+ unit tests, thread-safe design, full ConfigureAwait(false) coverage
- ✅ **Non-intrusive** - Based on netstandard2.1, compatible with .NET Core 3.0+, .NET 5/6/7/8/9/10

### 📦 NuGet Packages

| Package | Description | Dependencies |
|---------|-------------|--------------|
| `EasyTool.Core` | Core package (recommended) | No external dependencies |
| `EasyTool.All` | All-in-one package | All modules |
| `EasyTool.Web` | Web development tools | ASP.NET Core MVC |
| `EasyTool.System` | System tools (Windows) | System management |
| `EasyTool.Image` | Image processing | SkiaSharp |
| `EasyTool.NPOI` | Excel operations | NPOI |
| `EasyTool.Media` | Audio/video processing | No external dependencies |
| `EasyTool.EmitMapper` | Object mapping | EmitMapper.Core |
| `EasyTool.AI` | AI / LLM tools | System.Text.Json |

## 🚀 Quick Start

### Installation

```bash
# Core package (recommended)
dotnet add package EasyTool.Core

# All-in-one package
dotnet add package EasyTool.All

# Install as needed
dotnet add package EasyTool.AI
dotnet add package EasyTool.Media
dotnet add package EasyTool.System
dotnet add package EasyTool.Image
dotnet add package EasyTool.NPOI
dotnet add package EasyTool.EmitMapper
dotnet add package EasyTool.Web
```

### Usage Examples

```csharp
using EasyTool.TextCategory;
using EasyTool.CodeCategory;
using EasyTool.BusinessCategory;
using EasyTool.IdentifierCategory;

// Chinese Pinyin
var pinyin = PinyinUtil.GetPinyin("中国");        // "zhongguo"
var firstLetter = PinyinUtil.GetInitials("中国");  // "ZG"

// Sensitive word filtering (DFA algorithm)
SensitiveWordUtil.AddWords(new[] { "敏感词", "违规" });
var has = SensitiveWordUtil.Contains("这是一个敏感词");
var filtered = SensitiveWordUtil.Replace("这是一个敏感词", '*');

// ID Card validation
var isValid = IdCardUtil.IsValid("110101199003077654");  // true
var info = IdCardUtil.GetInfo("110101199003077654");

// SM4 Encryption (Chinese national standard)
var encrypted = Sm4Util.EncryptString("key123456789012", "plaintext");
var decrypted = Sm4Util.DecryptString("key123456789012", encrypted);

// ID Generation
var snowflakeId = IdUtil.SnowflakeId();
var ulid = IdUtil.ULID();
var objectId = IdUtil.ObjectId();
var nanoId = IdUtil.NanoId(12);
var tsid = IdUtil.TSID();

// Hash computation
var md5 = HashUtil.MD5("hello");
var sha256 = HashUtil.SHA256("hello");
var murmur = MurmurHashUtil.ComputeHash32(data);

// Base encoding
var b32 = Base32Util.EncodeString("hello");
var b58 = Base58Util.EncodeString("hello");
var b64url = Base64UrlUtil.EncodeString("hello");
```

## ✨ Feature Highlights

### 🔐 Encryption & Encoding (70+ utilities)

**Symmetric**: AES, DES, SM4, Blowfish, ChaCha20, IDEA, RC4, Salsa20, Serpent, Twofish, Rabbit, XOR, Camellia

**Asymmetric**: RSA, SM2, ECDSA, ElGamal, Diffie-Hellman

**Hashing**: MD5, SHA1/256/384/512, SM3, Blake2/3, MurmurHash, XXHash, CityHash, FarmHash, SipHash, Tiger, Whirlpool, RIPEMD160, Adler32, CRC, GOST

**Password Hashing**: Bcrypt, Argon2, Scrypt, PBKDF2

**Base Encoding**: Base32, Base45, Base58, Base64Url, Base85, Base91, Base92

**Other**: Hex, Punycode, Quoted-Printable, UUEncode, Baudot, GrayCode, MorseCode

**Compression**: GZip, Deflate, LZ4, Snappy, Zstd

**Key Derivation**: PBKDF2, HKDF, Scrypt, KDF

**Digital Signatures**: RSA, DSA, ECDSA

```csharp
// AES encryption
var enc = AesUtil.Encrypt("key16-bytes-key!", "plaintext");
var dec = AesUtil.Decrypt("key16-bytes-key!", enc);

// SM2 (Chinese national standard)
var (pub, pri) = Sm2Util.GenerateKeyPair();
var cipher = Sm2Util.Encrypt(pub, data);
var plain = Sm2Util.Decrypt(pri, cipher);

// Bcrypt password hashing
var hash = BcryptUtil.Hash("password");
var valid = BcryptUtil.Verify("password", hash);

// LZ4 compression
var compressed = LZ4Util.CompressString("long text...");
var original = LZ4Util.DecompressString(compressed);
```

### 🇨🇳 Chinese Business Validation (40+ utilities)

| Type | Utility | Key Methods |
|------|---------|-------------|
| ID Card | `IdCardUtil` | `IsValid`, `GetProvince`, `GetBirthday`, `GetGender`, `GetAge`, `Mask` |
| Phone | `PhoneNumberUtil` | `IsValid`, `IsMobile`, `IsLandline`, `Format`, `Mask` |
| Phone Location | `PhoneLocationUtil` | `GetLocation`, `GetCarrier`, `GetProvince`, `GetCity` |
| Bank Card | `BankCardUtil` | `IsValid`, `GetBankName`, `GetCardType`, `Mask` |
| Credit Card | `CreditCardUtil` | `IsValid`, `GetBrand`, `GetIssuer`, `Mask` |
| Social Credit Code | `SocialCreditCodeUtil` | `IsValid`, `GetRegistrationAuthority`, `Mask` |
| License Plate | `LicensePlateUtil` | `IsValid`, `GetProvince`, `GetPlateType`, `Mask` |
| Passport | `ForeignerIdUtil` | `IsValid`, `GetNationality`, `GetBirthday`, `GetGender` |
| Driving License | `DrivingLicenseUtil` | `IsValid`, `GetLicenseType`, `Mask` |
| HK ID Card | `HKIdCardUtil` | `IsValid`, `GetPrefix`, `Format`, `Mask` |
| Taiwan ID | `TwIdCardUtil` | `IsValid`, `GetCounty`, `GetGender`, `Mask` |
| QQ | `QQUtil` | `IsValid`, `IsValidQQEmail`, `ToEmail`, `Mask` |
| WeChat | `WeChatUtil` | `IsValid`, `IsValidOpenId`, `IsValidUnionId`, `Mask` |
| ISBN | `ISBNUtil` | `IsValid`, `GetGroup`, `GetPublisher` |
| VIN | `VINUtil` | `IsValid`, `GetWMI`, `GetModelYear`, `GetManufacturer` |
| IMEI | `IMEIUtil` | `IsValid`, `GetTAC`, `GetManufacturer` |
| Stock Code | `StockCodeUtil` | `IsValid`, `GetMarket`, `GetStockType`, `GetName` |
| SWIFT | `SwiftCodeUtil` | `IsValid`, `GetBankCode`, `GetCountryCode` |
| Email | `EmailUtil` | `IsValid`, `Normalize`, `GetProvider`, `IsEnterpriseEmail` |
| Domain | `DomainUtil` | `IsValid`, `IsChinaDomain`, `GetTLD`, `GetMainDomain` |
| IPv6 | `IPv6Util` | `IsValid`, `Compress`, `Expand`, `IsPrivate`, `ToIPv4` |
| MAC Address | `MACAddressUtil` | `IsValid`, `GetManufacturer`, `Format`, `Mask` |

### 📝 Text Processing (30+ utilities)

```csharp
// Pinyin conversion
ChinesePinyinUtil.ToPinyin("中国北京");         // "zhong guo bei jing"
ChinesePinyinUtil.GetPinyinInitial("中国北京"); // "ZGBJ"

// Chinese number conversion
ChineseNumberUtil.ToChinese(12345);     // "一万二千三百四十五"
ChineseNumberUtil.FromChinese("一万二"); // 12000

// Sensitive word filtering (DFA algorithm)
SensitiveWordUtil.AddWords(new[] { "bad", "evil" });
SensitiveWordUtil.Contains("this is bad");         // true
SensitiveWordUtil.Replace("this is bad", '*');     // replace
SensitiveWordUtil.FindAll("this is bad and evil"); // find all

// Text similarity (multiple algorithms)
var sim = TextSimilarityUtil.CosineSimilarity("hello", "hallo");
var sim2 = TextSimilarityUtil.JaroWinklerSimilarity("hello", "hallo");
var closest = TextSimilarityUtil.FindMostSimilar("helo", candidates);

// Data masking
DesensitizedUtil.MaskPhone("13800138000");        // "138****8000"
DesensitizedUtil.MaskEmail("test@qq.com");        // "t***@qq.com"
DesensitizedUtil.MaskIdCard("110101199003077654"); // "1101********7654"

// Template rendering
var result = TemplateUtil.Render("Hello {{name}}", dict);

// Escape utilities
EscapeUtil.EscapeHtml("<script>alert('xss')</script>");
EscapeUtil.EscapeJson("He said \"hello\"");
EscapeUtil.EscapeUrl("hello world");
```

### 🗃️ Collections & Data Structures (45+ utilities)

```csharp
// LRU Cache
var cache = LRUCacheUtil.Create<string, int>(100);
LRUCacheUtil.Put(cache, "key", 42);

// Bloom Filter
var filter = BloomFilterUtil.Create(10000, 0.01);
BloomFilterUtil.Add(filter, "hello");
BloomFilterUtil.Contains(filter, "hello"); // true

// Trie (prefix tree)
var trie = TrieUtil.Create();
TrieUtil.Insert(trie, "hello");
TrieUtil.Search(trie, "hello");     // true
TrieUtil.StartsWith(trie, "hel");   // true

// Union-Find
var uf = UnionFindUtil.Create(10);
UnionFindUtil.Union(uf, 1, 2);
UnionFindUtil.IsConnected(uf, 1, 2); // true

// Graph algorithms
var bfsOrder = GraphUtil.BFS(graph, startNode);
var topo = GraphUtil.TopologicalSort(graph);

// Permutations & Combinations
var perms = PermutationUtil.GetPermutations(items, 2);
var combos = CombinationUtil.GetCombinations(items, 2);

// Aho-Corasick multi-pattern matching
var ac = AhoCorasickUtil.Build(new[] { "he", "she", "his" });
var results = AhoCorasickUtil.Search("ushers", ac);
```

### 🆔 ID Generators (10+ schemes)

```csharp
IdUtil.SnowflakeId();    // Snowflake ID
IdUtil.ULID();           // ULID
IdUtil.TSID();           // TSID
IdUtil.ObjectId();       // MongoDB ObjectId
IdUtil.NanoId(12);       // NanoId
IdUtil.ShortId(8);       // Short ID
IdUtil.Xid();            // XID
IdUtil.KSUID();          // KSUID
IdUtil.SonyflakeId();    // Sonyflake
IdUtil.UUID(UUIDStyle.Sequential); // Ordered UUID
IdUtil.Cuid();           // CUID
IdUtil.Cuid2();          // CUID2
```

### 📅 Date & Time (10+ utilities)

```csharp
// Basics
var quarter = DateTimeUtil.GetQuarter(DateTime.Now);
var age = DateTimeUtil.GetAge(birthDate);
var week = DateTimeUtil.GetWeekOfYear(DateTime.Now);

// Timestamps
var ts = DateTimeUtil.ToTimestamp(DateTime.Now);
var dt = DateTimeUtil.FromTimestamp(ts);

// Lunar Calendar
var lunar = LunarCalendarUtil.ToLunar(DateTime.Now);
var animal = LunarCalendarUtil.GetAnimalYear(2026); // "马"

// Chinese Holidays
var isHoliday = ChineseHolidayUtil.IsHoliday(DateTime.Today);
var isWorkday = ChineseHolidayUtil.IsWorkday(DateTime.Today);

// Solar Terms
var term = SolarTermUtil.GetCurrentSolarTerm();
var next = SolarTermUtil.GetNextSolarTerm();

// Cron expressions
var valid = CronUtil.IsValid("0 0 12 * * ?");
var nextRun = CronUtil.GetNextOccurrence("0 0 12 * * ?");
var desc = CronUtil.GetDescription("0 0 12 * * ?");

// Workday calculations
var count = WorkdayUtil.GetWorkdayCount(start, end);
var future = WorkdayUtil.AddWorkdays(DateTime.Today, 10);
```

### 🌐 Networking (20+ utilities)

```csharp
// IP tools
IpUtil.IsValidIPv4("192.168.1.1");
IpUtil.GetLocalIP();
IpUtil.IsPrivateIP("192.168.1.1");

// HTTP tools
var html = HttpUtil.Get("https://example.com");
var json = await HttpUtil.GetAsync("https://api.example.com/data");

// HTTP retry
var response = await HttpRetryUtil.SendWithRetryAsync(request, maxRetries: 3);

// URL tools
var isValid = URLUtil.IsValid("https://example.com/path?q=1");
var domain = URLUtil.GetDomain("https://example.com/path");

// DNS queries
var ips = await DnsServerUtil.QueryAAsync("example.com");
var mx = await DnsServerUtil.QueryMxAsync("example.com");

// WebSocket
await WebSocketUtil.ConnectAsync("wss://example.com/ws");
await WebSocketUtil.SendStringAsync("hello");

// SSE (Server-Sent Events)
await SseUtil.SubscribeAsync(url, e => Console.WriteLine(e.Data));

// Short URL
var code = ShortUrlUtil.Generate("https://example.com/long-url");
```

### 📂 File & IO (35+ utilities)

```csharp
// JSON
var json = JsonUtil.Serialize(obj);
var obj = JsonUtil.Deserialize<MyClass>(json);

// CSV
var data = CsvConvertUtil.FromCsv<MyClass>(csvText);
var csv = CsvConvertUtil.ToCsv(dataList);

// Excel (EasyTool.NPOI)
var dt = ExcelUtil.Read("data.xlsx");
ExcelUtil.Write("output.xlsx", dataTable);

// XML
var xml = XmlConvertUtil.ToXml(obj);
var obj = XmlConvertUtil.FromXml<MyClass>(xml);

// YAML
var yaml = YamlConvertUtil.Serialize(obj);
var obj = YamlConvertUtil.Deserialize<MyClass>(yaml);

// TOML
var toml = TomlConvertUtil.Serialize(obj);
var obj = TomlConvertUtil.Deserialize<MyClass>(toml);

// ZIP
ZipUtil.CreateZip("archive.zip", files);
ZipUtil.ExtractZip("archive.zip", "output/");

// File monitoring
WatchMonitor.Watch("log.txt", (path, changeType) => {
    Console.WriteLine($"{path} changed: {changeType}");
});

// File signature detection
var type = FileSignatureUtil.Detect("unknown.file");
var isImage = FileSignatureUtil.IsImage("photo.jpg");
```

### 🔄 Fluent Extension Methods

```csharp
// Collection extensions
list.Where(x => x.IsActive)
    .ForEach(x => x.Process())
    .DistinctBy(x => x.Id)
    .Batch(100)
    .JoinAsString(",");

list.IsNullOrEmpty();
list.RandomElement();
list.Shuffle();

// String extensions
str.EqualsIgnoreCase("HELLO");
str.ContainsIgnoreCase("world");
str.Left(10);
str.Truncate(50);

// DateTime extensions
DateTime.Now.ToDateString();         // "2026-04-10"
DateTime.Now.ToDateTimeString();     // "2026-04-10 12:30:00"
birthDate.GetAge();
DateTime.Now.GetQuarter();
DateTime.Now.ToTimestamp();

// Number extensions
100.InRange(1, 200);
100.Clamp(50, 150);
12345.ToChinese();
1024.ToFileSize();

// HttpClient extensions
var data = await httpClient.GetAsync<MyData>(url);
await httpClient.PostAsync(url, payload);
```

### 🛡️ General Utilities

```csharp
// Async retry
var result = await RetryUtil.ExecuteAsync(() => DoWork(), maxRetries: 3);

// Rate limiter
var limiter = RateLimiter.CreateTokenBucket(100, 10.0);
if (RateLimiter.TryAcquire(limiter)) { /* allowed */ }

// Circuit breaker
var cb = CircuitBreakerUtil.Create(5, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(30));
await CircuitBreakerUtil.ExecuteAsync(cb, async () => await CallService());

// Event bus
EventBus.Subscribe<MyEvent>(e => { /* handle */ });
EventBus.Publish(new MyEvent { Message = "hello" });

// Validation
var errors = ValidatorUtil.Validate(myObject);
var isEmail = ValidatorUtil.IsEmail("test@example.com");

// Console helpers
ConsoleUtil.WriteSuccess("Done!");
ConsoleUtil.WriteError("Error!");
ConsoleUtil.WriteProgressBar(70, 100);

// Benchmarking
BenchmarkUtil.Measure("MethodA", () => MethodA());
BenchmarkUtil.Compare("MethodA", "MethodB", () => MethodA(), () => MethodB());
```

### 🔒 Security

```csharp
// XSS protection
var safe = XssUtil.Encode("<script>alert('xss')</script>");
var clean = XssUtil.Sanitize(html);

// SQL injection detection
var hasInjection = SqlInjectionUtil.HasSqlInjection(input);
var escaped = SqlInjectionUtil.EscapeString(input);

// JWT
var token = JwtUtil.Encode(payload, "secret-key");
var decoded = JwtUtil.Decode(token);
var valid = JwtUtil.Validate(token, "secret-key");

// Password strength
var strength = PasswordStrengthUtil.CheckStrength("MyP@ss123!");
var entropy = PasswordStrengthUtil.CalculateEntropy("password");

// Certificates
var cert = CertificateUtil.Load("cert.pfx", "password");
var thumbprint = CertificateUtil.GetThumbprint(cert);
```

### 🤖 AI Module

```csharp
// OpenAI client
using var client = new OpenAIClient("api-key", "https://api.openai.com/v1");
var response = await client.ChatAsync(messages, "gpt-4");

// Token estimation
var tokens = TokenizerUtil.EstimateTokens("Hello, world!");

// Prompt builder
var prompt = new PromptBuilder()
    .SetSystemPrompt("You are a translator")
    .SetTask("Translate to English")
    .AddContext("Original: Hello World")
    .Build();

// Vector similarity
var sim = VectorSimilarity.CosineSimilarity(vec1, vec2);
var topK = VectorSimilarity.FindMostSimilar(query, allVectors, 5);

// Vector store
var store = new VectorStore();
store.Add("doc1", embedding);
var results = store.Search(queryEmbedding, topK: 5);
```

## 📁 Project Structure

```
EasyTool/
├── 📁 EasyTool.Core                   # Core (zero dependencies, 300+ utilities)
│   ├── AICategory/                   # AI tools (prompt, vectors)
│   ├── BusinessCategory/             # Business validation (40+ Chinese validators)
│   ├── CacheCategory/                # Cache tools
│   ├── CodeCategory/                 # Encoding & encryption (70+ algorithms)
│   ├── CollectionsCategory/          # Collections & data structures (45+)
│   ├── ColorCategory/                # Color tools
│   ├── ConvertCategory/              # Type conversion (CSV/XML/YAML/TOML/Coordinates)
│   ├── DataCategory/                 # Data generation (Faker, QueryBuilder)
│   ├── DatabaseCategory/             # Database tools
│   ├── DateTimeCategory/             # Date & time (Lunar/SolarTerms/Holidays/Cron)
│   ├── IdentifierCategory/           # ID generation (10+ schemes)
│   ├── IOCategory/                   # File operations (35+ tools)
│   ├── MathCategory/                 # Math (Statistics/Matrix/Geometry/Interpolation)
│   ├── MediaCategory/                # Media basics
│   ├── NetCategory/                  # Networking (20+ HTTP/DNS/WebSocket/SSE)
│   ├── QueueCategory/                # Queues (Channel/DelayQueue/PriorityQueue)
│   ├── ReflectCategory/              # Reflection tools
│   ├── SecurityCategory/             # Security (XSS/SQLi/JWT/Cert/TLS)
│   ├── Standardization/              # Standard types (Option/Result/QueryPage)
│   ├── SystemCategory/               # System basics
│   ├── TextCategory/                 # Text processing (30+ Pinyin/SensitiveWord/Similarity)
│   ├── ToolCategory/                 # General (ObjectPool/EventBus/RateLimiter/Retry)
│   └── ValidationCategory/           # Validators
├── 📁 EasyTool.Web                    # Web tools (TypeScript code generation)
├── 📁 EasyTool.System                 # System tools (Windows hardware/process/service)
├── 📁 EasyTool.Image                  # Image processing (SkiaSharp)
├── 📁 EasyTool.NPOI                   # Excel operations (NPOI)
├── 📁 EasyTool.Media                  # Audio/video processing
├── 📁 EasyTool.EmitMapper             # Object mapping (EmitMapper)
├── 📁 EasyTool.AI                     # AI / LLM tools
├── 📁 EasyTool.All                    # All-in-one package
└── 📁 EasyTool.UnitTests              # Unit tests (1069+)
```

## 📊 Statistics

| Metric | Count |
|--------|-------|
| Source files | 481 |
| Utility classes | 300+ |
| Public methods | Thousands |
| Unit tests | 1069+ |
| Target framework | netstandard2.1 |
| External dependencies (core) | 0 |

## ❌ What We Don't Do

| Feature | Use Instead |
|---------|-------------|
| ORM/Database | EF Core, Dapper, SqlSugar |
| Logging | Serilog, NLog |
| Caching | EasyCaching, Microsoft.Extensions.Caching |
| DI | Microsoft.Extensions.DependencyInjection |
| Scheduling | Quartz.NET, Hangfire |
| Message Queue | MassTransit, CAP |
| WebSocket | SignalR |

## 🔗 Links

- [Documentation](https://github.com/li761747705/easytool#readme)
- [NuGet](https://www.nuget.org/packages/EasyTool.Core)
- [GitHub](https://github.com/li761747705/easytool)

## 🤝 Contributing

Contributions welcome! See [Contributing Guide](CONTRIBUTING.md).

## 📄 License

[MIT License](LICENSE)

---

> EasyTool - Making .NET development easier ✨
