<h1 align="center"> EasyTool </h1>
<div align="center">
一个开源的 .NET 工具库，对标 Java Hutool，让开发变得更加简单高效
</div>
<div align="center">

[![pull_request](https://github.com/dotnet-easy/easytool/actions/workflows/pull_request.yml/badge.svg)](https://github.com/dotnet-easy/easytool/actions/workflows/pull_request.yml)
[![](https://img.shields.io/nuget/v/EasyTool.Core.svg)](https://www.nuget.org/packages/EasyTool.Core)
[![](https://img.shields.io/badge/.NET-netstandard2.1-blue)](https://learn.microsoft.com/dotnet/standard/net-standard)
[![](https://img.shields.io/badge/测试-1069+-brightgreen)](https://github.com/dotnet-easy/easytool)
[![](https://img.shields.io/badge/工具类-300+-orange)](https://github.com/dotnet-easy/easytool)
<p>
    <span>中文</span> | <a href="README.EN-US.md">English</a>
</p>
</div>

## 📚 简介

EasyTool 是一个**轻量级、功能全面、中文友好**的 .NET 工具库，基于 `netstandard2.1` 开发，覆盖开发中绝大部分工具类需求。

### 🎯 核心特性

- ✅ **轻量级** - 核心包零外部依赖
- ✅ **全覆盖** - 300+ 工具类，涵盖编码、加密、集合、文本、网络、IO 等所有常见场景
- ✅ **中文友好** - 拼音转换、敏感词过滤、身份证/银行卡/手机号验证、农历节气等中国特色功能
- ✅ **高可靠** - 1069+ 单元测试，线程安全设计，ConfigureAwait(false) 全量覆盖
- ✅ **零侵入** - 基于 netstandard2.1，兼容 .NET Core 3.0+、.NET 5/6/7/8/9/10

### 📦 NuGet 包一览

| 包名 | 说明 | 依赖 |
|------|------|------|
| `EasyTool.Core` | 核心包（推荐） | 无外部依赖 |
| `EasyTool.All` | 整合包（包含所有模块） | 全部模块 |
| `EasyTool.Web` | Web 开发工具 | ASP.NET Core MVC |
| `EasyTool.System` | 系统工具（Windows） | 系统管理相关 |
| `EasyTool.Image` | 图像处理 | SkiaSharp |
| `EasyTool.NPOI` | Excel 操作 | NPOI |
| `EasyTool.Media` | 音视频处理 | 无外部依赖 |
| `EasyTool.EmitMapper` | 对象映射 | EmitMapper.Core |
| `EasyTool.AI` | AI / LLM 工具 | System.Text.Json |

## 🚀 快速开始

### 安装

```bash
# 核心包（推荐）
dotnet add package EasyTool.Core

# 整合包（包含所有模块）
dotnet add package EasyTool.All

# 按需安装
dotnet add package EasyTool.AI       # AI 模块
dotnet add package EasyTool.Media    # 媒体处理
dotnet add package EasyTool.System   # 系统工具
dotnet add package EasyTool.Image    # 图像处理（SkiaSharp）
dotnet add package EasyTool.NPOI     # Excel 操作
dotnet add package EasyTool.EmitMapper # 对象映射
dotnet add package EasyTool.Web      # Web 工具
```

### 使用示例

```csharp
using EasyTool.TextCategory;
using EasyTool.CodeCategory;
using EasyTool.BusinessCategory;
using EasyTool.IdentifierCategory;

// 汉字转拼音
var pinyin = PinyinUtil.GetPinyin("中国");        // "zhongguo"
var firstLetter = PinyinUtil.GetInitials("中国");  // "ZG"

// 敏感词过滤（DFA 算法）
SensitiveWordUtil.AddWords(new[] { "敏感词", "违规" });
var has = SensitiveWordUtil.Contains("这是一个敏感词");     // true
var filtered = SensitiveWordUtil.Replace("这是一个敏感词", '*'); // "这是一个***"

// 身份证验证与解析
var isValid = IdCardUtil.IsValid("110101199003077654");  // true
var info = IdCardUtil.GetInfo("110101199003077654");
// info.Province → "北京"  info.Birthday → "1990-03-07"  info.Gender → "男"

// 国密 SM4 加密
var encrypted = Sm4Util.EncryptString("key123456789012", "明文");
var decrypted = Sm4Util.DecryptString("key123456789012", encrypted);

// ID 生成
var snowflakeId = IdUtil.SnowflakeId();   // 雪花 ID
var ulid = IdUtil.ULID();                 // ULID
var objectId = IdUtil.ObjectId();         // ObjectId
var nanoId = IdUtil.NanoId(12);           // NanoId
var tsid = IdUtil.TSID();                 // TSID

// 哈希计算
var md5 = HashUtil.MD5("hello");              // MD5
var sha256 = HashUtil.SHA256("hello");        // SHA256
var murmur = MurmurHashUtil.ComputeHash32(data); // MurmurHash
var xxhash = XxHashUtil.ComputeHash64(data);     // XXHash

// Base 编码
var b32 = Base32Util.EncodeString("hello");   // Base32
var b58 = Base58Util.EncodeString("hello");   // Base58（比特币地址）
var b64url = Base64UrlUtil.EncodeString("hello"); // Base64Url
```

## ✨ 特色功能

### 🔐 加密编码（70+ 工具类）

**对称加密**：AES、DES、SM4、Blowfish、ChaCha20、IDEA、RC4、Salsa20、Serpent、Twofish、Rabbit、XOR、Camellia

**非对称加密**：RSA、SM2、ECDSA、ElGamal、Diffie-Hellman

**哈希算法**：MD5、SHA1/256/384/512、SM3、Blake2/3、MurmurHash、XXHash、CityHash、FarmHash、SipHash、Tiger、Whirlpool、RIPEMD160、Adler32、CRC、GOST

**密码哈希**：Bcrypt、Argon2、Scrypt、PBKDF2

**Base 编码**：Base32、Base45、Base58、Base64Url、Base85、Base91、Base92

**其他编码**：Hex、Punycode、Quoted-Printable、UUEncode、Baudot、GrayCode、MorseCode

**压缩**：GZip、Deflate、LZ4、Snappy、Zstd

**密钥派生**：PBKDF2、HKDF、Scrypt、KDF

**数字签名**：RSA、DSA、ECDSA

```csharp
// AES 加密
var enc = AesUtil.Encrypt("key16-bytes-key!", "明文");
var dec = AesUtil.Decrypt("key16-bytes-key!", enc);

// SM2 国密非对称加密
var (pub, pri) = Sm2Util.GenerateKeyPair();
var cipher = Sm2Util.Encrypt(pub, data);
var plain = Sm2Util.Decrypt(pri, cipher);

// Bcrypt 密码哈希
var hash = BcryptUtil.Hash("password");
var valid = BcryptUtil.Verify("password", hash);

// LZ4 压缩
var compressed = LZ4Util.CompressString("很长的文本...");
var original = LZ4Util.DecompressString(compressed);
```

### 🇨🇳 中国特色业务验证（40+ 工具类）

| 类型 | 工具类 | 主要方法 |
|------|--------|----------|
| 身份证 | `IdCardUtil` | `IsValid`、`GetProvince`、`GetBirthday`、`GetGender`、`GetAge`、`Mask` |
| 手机号 | `PhoneNumberUtil` | `IsValid`、`IsMobile`、`IsLandline`、`Format`、`Mask` |
| 手机号归属地 | `PhoneLocationUtil` | `GetLocation`、`GetCarrier`、`GetProvince`、`GetCity` |
| 银行卡 | `BankCardUtil` | `IsValid`、`GetBankName`、`GetCardType`、`Mask` |
| 信用卡 | `CreditCardUtil` | `IsValid`、`GetBrand`、`GetIssuer`、`Mask` |
| 统一社会信用代码 | `SocialCreditCodeUtil` | `IsValid`、`GetRegistrationAuthority`、`Mask` |
| 车牌号 | `LicensePlateUtil` | `IsValid`、`GetProvince`、`GetPlateType`、`Mask` |
| 护照 | `ForeignerIdUtil` | `IsValid`、`GetNationality`、`GetBirthday`、`GetGender` |
| 驾驶证 | `DrivingLicenseUtil` | `IsValid`、`GetLicenseType`、`Mask` |
| 港澳通行证 | `HKIdCardUtil` | `IsValid`、`GetPrefix`、`Format`、`Mask` |
| 台湾身份证 | `TwIdCardUtil` | `IsValid`、`GetCounty`、`GetGender`、`Mask` |
| QQ 号 | `QQUtil` | `IsValid`、`IsValidQQEmail`、`ToEmail`、`Mask` |
| 微信号 | `WeChatUtil` | `IsValid`、`IsValidOpenId`、`IsValidUnionId`、`Mask` |
| ISBN | `ISBNUtil` | `IsValid`、`GetGroup`、`GetPublisher`、`CalculateCheckDigit` |
| 车架号 VIN | `VINUtil` | `IsValid`、`GetWMI`、`GetModelYear`、`GetManufacturer` |
| IMEI | `IMEIUtil` | `IsValid`、`GetTAC`、`GetManufacturer`、`GenerateRandom` |
| ICCID | `ICCIDUtil` | `IsValid`、`GetCarrier`、`IsChinaMobile`、`GetCountry` |
| 股票代码 | `StockCodeUtil` | `IsValid`、`GetMarket`、`GetStockType`、`GetName` |
| SWIFT 代码 | `SwiftCodeUtil` | `IsValid`、`GetBankCode`、`GetCountryCode`、`IsChineseBank` |
| 社保号 | `SocialSecurityUtil` | `IsValid`、`GetBirthday`、`GetGender`、`GetAge` |
| 条形码 | `BarcodeUtil` | `IsValidEAN13`、`IsValidEAN8`、`IsValidUPC` |
| 邮箱 | `EmailUtil` | `IsValid`、`Normalize`、`GetProvider`、`IsEnterpriseEmail`、`GenerateRandom` |
| 域名 | `DomainUtil` | `IsValid`、`IsChinaDomain`、`GetTLD`、`GetMainDomain` |
| 端口 | `PortUtil` | `IsValid`、`GetPortInfo`、`IsWellKnownPort`、`GetPortCategory` |
| MAC 地址 | `MACAddressUtil` | `IsValid`、`GetManufacturer`、`Format`、`Mask` |
| 邮编 | `PostalCodeUtil` | `IsValid`、`GetProvince`、`GetCity` |
| IPv6 | `IPv6Util` | `IsValid`、`Compress`、`Expand`、`IsPrivate`、`ToIPv4` |

```csharp
// 身份证
var valid = IdCardUtil.IsValid("110101199003077654");
var info = IdCardUtil.GetInfo("110101199003077654");

// 银行卡
var isValid = BankCardUtil.IsValid("6222021234567890123");
var bankName = BankCardUtil.GetBankName("6222021234567890123");

// 手机号归属地
var loc = PhoneLocationUtil.GetLocation("13800138000");
// loc.Carrier → "中国移动"  loc.Province → "广东"  loc.City → "广州"

// 车牌号（含新能源）
var ok = LicensePlateUtil.IsValid("粤A12345D");
var province = LicensePlateUtil.GetProvince("粤A12345D");
```

### 📝 文本处理（30+ 工具类）

```csharp
// 汉字转拼音
ChinesePinyinUtil.ToPinyin("中国北京");           // "zhong guo bei jing"
ChinesePinyinUtil.GetPinyinInitial("中国北京");   // "ZGBJ"
ChinesePinyinUtil.ToPinyinWithTone("中国");       // "zhong1 guo2"

// 中文数字转换
ChineseNumberUtil.ToChinese(12345);       // "一万二千三百四十五"
ChineseNumberUtil.FromChinese("一万二");   // 12000

// 敏感词过滤（DFA 算法，高效）
SensitiveWordUtil.AddWords(new[] { "敏感词", "违规" });
SensitiveWordUtil.Contains("这是一个敏感词");         // true
SensitiveWordUtil.Replace("这是一个敏感词", '*');     // 替换
SensitiveWordUtil.FindAll("这是一个敏感词违规内容"); // 查找全部

// 文本相似度（多种算法）
var sim = TextSimilarityUtil.CosineSimilarity("hello", "hallo");
var sim2 = TextSimilarityUtil.JaroWinklerSimilarity("hello", "hallo");
var sim3 = TextSimilarityUtil.LevenshteinSimilarity("hello", "hallo");
var closest = TextSimilarityUtil.FindMostSimilar("helo", new[] { "hello", "world" });

// 文本差异
var diff = DiffUtil.Compute("hello world", "hello earth");
var html = DiffUtil.FormatHtml(diff);

// 数据脱敏
var masked = DesensitizedUtil.MaskPhone("13800138000");   // "138****8000"
var masked2 = DesensitizedUtil.MaskIdCard("110101199003077654"); // "1101********7654"
var masked3 = DesensitizedUtil.MaskEmail("test@qq.com");  // "t***@qq.com"

// 模板渲染
var result = TemplateUtil.Render("你好 {{name}}，欢迎来到 {{city}}", new Dictionary<string, object> {
    { "name", "张三" }, { "city", "北京" }
});

// 转义
var html = EscapeUtil.EscapeHtml("<script>alert('xss')</script>");
var json = EscapeUtil.EscapeJson("He said \"hello\"");
var url = EscapeUtil.EscapeUrl("hello world");

// 正则工具
var emails = RegexUtil.GetEmails(text);
var urls = RegexUtil.GetUrls(text);
var ips = RegexUtil.GetIpAddresses(text);
```

### 🗃️ 集合与数据结构（45+ 工具类）

```csharp
// LRU 缓存
var cache = LRUCacheUtil.Create<string, int>(100);
LRUCacheUtil.Put(cache, "key", 42);
var val = LRUCacheUtil.Get(cache, "key");

// 布隆过滤器
var filter = BloomFilterUtil.Create(10000, 0.01);
BloomFilterUtil.Add(filter, "hello");
var exists = BloomFilterUtil.Contains(filter, "hello");

// 前缀树 Trie
var trie = TrieUtil.Create();
TrieUtil.Insert(trie, "hello");
TrieUtil.Search(trie, "hello");     // true
TrieUtil.StartsWith(trie, "hel");   // true

// 并查集
var uf = UnionFindUtil.Create(10);
UnionFindUtil.Union(uf, 1, 2);
var connected = UnionFindUtil.IsConnected(uf, 1, 2); // true

// 图算法
var bfsOrder = GraphUtil.BFS(graph, startNode);
var dfsOrder = GraphUtil.DFS(graph, startNode);
var topo = GraphUtil.TopologicalSort(graph);

// 排列组合
var perms = PermutationUtil.GetPermutations(new[] { 1, 2, 3 }, 2);
var combos = CombinationUtil.GetCombinations(new[] { 1, 2, 3, 4 }, 2);

// Aho-Corasick 多模式匹配
var ac = AhoCorasickUtil.Build(new[] { "he", "she", "his" });
var results = AhoCorasickUtil.Search("ushers", ac);

// 分页
var page = PagedList.Create(data, 2, 10); // 第2页，每页10条

// 树结构构建
var tree = TreeBuildUtil.Build(flatList, x => x.Id, x => x.ParentId);
```

### 🆔 ID 生成器（10+ 方案）

```csharp
// 统一入口
IdUtil.SnowflakeId();    // 雪花 ID（分布式唯一）
IdUtil.ULID();           // ULID（字典序唯一）
IdUtil.TSID();           // TSID（时间排序）
IdUtil.ObjectId();       // ObjectId（MongoDB 风格）
IdUtil.NanoId(12);       // NanoId（短 ID）
IdUtil.ShortId(8);       // 短 ID
IdUtil.Xid();            // XID
IdUtil.KSUID();          // KSUID（Kubernetes 风格）
IdUtil.SonyflakeId();    // Sonyflake
IdUtil.UUID(UUIDStyle.Sequential); // 有序 UUID
IdUtil.Cuid();           // CUID
IdUtil.Cuid2();          // CUID2
var codes = IdUtil.SqidsEncode(new long[] { 1, 2, 3 }); // Sqids 编码
var ids = IdUtil.SqidsDecode(codes);                     // Sqids 解码
```

### 📅 日期时间（10+ 工具类）

```csharp
// 基础操作
var quarter = DateTimeUtil.GetQuarter(DateTime.Now);  // 1-4
var age = DateTimeUtil.GetAge(birthDate);             // 计算年龄
var week = DateTimeUtil.GetWeekOfYear(DateTime.Now);  // 年中第几周

// 时间戳
var ts = DateTimeUtil.ToTimestamp(DateTime.Now);       // Unix 秒
var dt = DateTimeUtil.FromTimestamp(ts);               // 还原 DateTime

// 农历
var lunar = LunarCalendarUtil.ToLunar(DateTime.Now);
var solar = LunarCalendarUtil.ToSolar(2026, 1, 15, false);
var animal = LunarCalendarUtil.GetAnimalYear(2026);    // "马"

// 中国节假日
var isHoliday = ChineseHolidayUtil.IsHoliday(DateTime.Today);
var isWorkday = ChineseHolidayUtil.IsWorkday(DateTime.Today);
var holidays = ChineseHolidayUtil.GetHolidays(2026);

// 节气
var term = SolarTermUtil.GetCurrentSolarTerm();
var next = SolarTermUtil.GetNextSolarTerm();

// Cron 表达式
var valid = CronUtil.IsValid("0 0 12 * * ?");
var nextRun = CronUtil.GetNextOccurrence("0 0 12 * * ?");
var desc = CronUtil.GetDescription("0 0 12 * * ?"); // "每天中午12:00"

// 工作日计算
var count = WorkdayUtil.GetWorkdayCount(start, end);
var future = WorkdayUtil.AddWorkdays(DateTime.Today, 10);
```

### 🌐 网络工具（20+ 工具类）

```csharp
// IP 工具
IpUtil.IsValidIPv4("192.168.1.1");
IpUtil.GetLocalIP();
IpUtil.GetPublicIP();
IpUtil.IsPrivateIP("192.168.1.1");
IpUtil.IsInSubnet("192.168.1.100", "192.168.1.0/24");

// HTTP 工具
var html = HttpUtil.Get("https://example.com");
var json = await HttpUtil.GetAsync("https://api.example.com/data");

// HTTP 重试
var response = await HttpRetryUtil.SendWithRetryAsync(request, maxRetries: 3);

// URL 工具
var isValid = URLUtil.IsValid("https://example.com/path?q=1");
var domain = URLUtil.GetDomain("https://example.com/path");
var encoded = URLUtil.Encode("hello world");

// DNS 查询
var ips = await DnsServerUtil.QueryAAsync("example.com");
var mx = await DnsServerUtil.QueryMxAsync("example.com");

// WebSocket
await WebSocketUtil.ConnectAsync("wss://example.com/ws");
await WebSocketUtil.SendStringAsync("hello");

// SSE (Server-Sent Events)
await SseUtil.SubscribeAsync("https://example.com/events", e => {
    Console.WriteLine(e.Data);
});

// 短链接
var code = ShortUrlUtil.Generate("https://example.com/very-long-url");

// 邮件
await MailUtil.SendAsync("to@example.com", "subject", "body", "from@example.com", "smtp.example.com");
```

### 📂 文件与 IO（35+ 工具类）

```csharp
// JSON
var json = JsonUtil.Serialize(obj);
var obj = JsonUtil.Deserialize<MyClass>(json);
var valid = JsonUtil.IsValid(jsonStr);
JsonUtil.Format(jsonStr);

// CSV
var data = CsvConvertUtil.FromCsv<MyClass>(csvText);
var csv = CsvConvertUtil.ToCsv(dataList);
CsvConvertUtil.SaveToFile(csv, "output.csv");

// Excel (EasyTool.NPOI)
var dt = ExcelUtil.Read("data.xlsx");
ExcelUtil.Write("output.xlsx", dataTable);
var names = ExcelUtil.GetSheetNames("data.xlsx");

// XML
var xml = XmlConvertUtil.ToXml(obj);
var obj = XmlConvertUtil.FromXml<MyClass>(xml);
XmlConvertUtil.FormatXml(xml);

// YAML
var yaml = YamlConvertUtil.Serialize(obj);
var obj = YamlConvertUtil.Deserialize<MyClass>(yaml);

// TOML
var toml = TomlConvertUtil.Serialize(obj);
var obj = TomlConvertUtil.Deserialize<MyClass>(toml);

// ZIP
ZipUtil.CreateZip("archive.zip", files);
ZipUtil.ExtractZip("archive.zip", "output/");
var entries = ZipUtil.ListEntries("archive.zip");

// 文件监控
WatchMonitor.Watch("log.txt", (path, changeType) => {
    Console.WriteLine($"{path} changed: {changeType}");
});

// 文件签名检测
var type = FileSignatureUtil.Detect("unknown.file"); // 检测真实文件类型
var isImage = FileSignatureUtil.IsImage("photo.jpg");

// 路径工具
PathUtil.Normalize("path/to/../file.txt");
PathUtil.EnsureDirectoryExists("output/dir");
PathUtil.GetRelativePath("base", "target");
```

### 🔄 流式扩展方法

```csharp
// 集合扩展（支持链式调用）
var result = list
    .Where(x => x.IsActive)
    .ForEach(x => x.Process())
    .DistinctBy(x => x.Id)
    .Batch(100)
    .JoinAsString(",");

// 判断集合状态
list.IsNullOrEmpty();     // 是否为空
list.IsNotNullOrEmpty();  // 是否不为空

// 随机操作
var element = list.RandomElement();
var shuffled = list.Shuffle();

// 字符串扩展
str.IsNullOrEmpty();
str.IsNullOrWhiteSpace();
str.EqualsIgnoreCase("HELLO");
str.ContainsIgnoreCase("world");
str.Left(10);
str.Right(10);
str.Truncate(50);
str.Reverse();
str.ToEnum<MyEnum>();

// DateTime 扩展
var dateStr = DateTime.Now.ToDateString();         // "2026-04-10"
var dateTimeStr = DateTime.Now.ToDateTimeString();  // "2026-04-10 12:30:00"
DateTime.Now.IsToday();
birthDate.GetAge();
DateTime.Now.GetQuarter();   // 1-4
DateTime.Now.ToTimestamp();  // Unix 时间戳（秒）

// 数字扩展
100.InRange(1, 200);
100.Clamp(50, 150);
12345.ToChinese();            // "一万二千三百四十五"
1024.ToFileSize();            // "1.00 KB"

// HttpClient 扩展
var data = await httpClient.GetAsync<MyData>("https://api.example.com/data");
await httpClient.PostAsync("https://api.example.com/data", payload);
```

### 🗃️ 对象池（减少 GC 压力）

```csharp
// StringBuilder 池
var result = StringBuilderPool.Use(sb => {
    sb.Append("Hello").Append(" World");
    return sb.ToString();
});

// 通用对象池
var pool = ObjectPool.Create<StringBuilder>(10, () => new StringBuilder());
var sb = ObjectPool.Get(pool);
try { /* 使用 */ }
finally { ObjectPool.Return(pool, sb); }
```

### 🔒 安全工具

```csharp
// XSS 防护
var safe = XssUtil.Encode("<script>alert('xss')</script>");
var clean = XssUtil.Sanitize(html);

// SQL 注入检测
var hasInjection = SqlInjectionUtil.HasSqlInjection(input);
var escaped = SqlInjectionUtil.EscapeString(input);

// JWT
var token = JwtUtil.Encode(new Dictionary<string, object> { { "sub", "user1" } }, "secret-key");
var payload = JwtUtil.Decode(token);
var valid = JwtUtil.Validate(token, "secret-key");

// 密码强度
var strength = PasswordStrengthUtil.CheckStrength("MyP@ss123!");  // Strong
var entropy = PasswordStrengthUtil.CalculateEntropy("password");
var crackTime = PasswordStrengthUtil.EstimateCrackTime("password");

// 证书
var cert = CertificateUtil.Load("cert.pfx", "password");
var thumbprint = CertificateUtil.GetThumbprint(cert);
var isValid = CertificateUtil.IsValid(cert);

// TLS
var protocols = TlsUtil.GetSupportedProtocols();
var valid = TlsUtil.IsCertificateValid("example.com");

// 安全随机数
var bytes = new byte[32];
SecureRandomUtil.NextBytes(bytes);
var num = SecureRandomUtil.NextInt(1, 100);
```

### 🤖 AI 模块

```csharp
// OpenAI 客户端
using var client = new OpenAIClient("api-key", "https://api.openai.com/v1");
var response = await client.ChatAsync(messages, "gpt-4");

// Token 估算
var tokens = TokenizerUtil.EstimateTokens("Hello, world!");
var maxTokens = TokenizerUtil.GetModelMaxTokens("gpt-4");

// 提示词构建
var prompt = new PromptBuilder()
    .SetSystemPrompt("你是一个翻译助手")
    .SetTask("将以下文本翻译为英文")
    .AddContext("原文：你好世界")
    .SetOutputFormat("JSON")
    .Build();

// 向量相似度
var sim = VectorSimilarity.CosineSimilarity(vec1, vec2);
var dist = VectorSimilarity.EuclideanDistance(vec1, vec2);
var topK = VectorSimilarity.FindMostSimilar(query, allVectors, 5);

// 向量存储
var store = new VectorStore();
store.Add("doc1", embedding, new Dictionary<string, string> { { "source", "web" } });
var results = store.Search(queryEmbedding, topK: 5, threshold: 0.8);
```

### 🎨 颜色工具

```csharp
// 颜色转换
var rgb = ColorUtil.HexToRgb("#FF5733");
var hex = ColorUtil.RgbToHex(255, 87, 51);
var hsl = ColorUtil.RgbToHsl(255, 87, 51);

// 颜色操作
var lighter = ColorUtil.Lighten(color, 0.2);
var darker = ColorUtil.Darken(color, 0.2);
var inverted = ColorUtil.Invert(color);
var gray = ColorUtil.GetGrayscale(color);

// 调色板
var analogous = ColorPaletteUtil.GenerateAnalogous(color, 5);
var triadic = ColorPaletteUtil.GenerateTriadic(color);
var shades = ColorPaletteUtil.GenerateShades(color, 5);
```

### 🌏 行政区划与地理

```csharp
// 省市区三级联动
var provinces = RegionUtil.GetProvinces();
var cities = RegionUtil.GetCities("440000");    // 广东省的城市
var districts = RegionUtil.GetDistricts("440100"); // 广州市的区

// 坐标转换（WGS84/GCJ02/BD09）
var (lng, lat) = CoordinateConvertUtil.WGS84ToGCJ02(116.397, 39.908);
var (lng2, lat2) = CoordinateConvertUtil.GCJ02ToBD09(lng, lat);
var dist = CoordinateConvertUtil.Distance(lat1, lng1, lat2, lng2);

// 距离计算
var km = DistanceUtil.Haversine(lat1, lng1, lat2, lng2);
var miles = DistanceUtil.DistanceInMiles(lat1, lng1, lat2, lng2);
```

### 🧮 数学工具

```csharp
// 基础数学
MathUtil.GCD(12, 8);                // 最大公约数 → 4
MathUtil.LCM(12, 8);                // 最小公倍数 → 24
MathUtil.Factorial(10);             // 阶乘
MathUtil.Fibonacci(10);             // 斐波那契
MathUtil.IsPrime(97);               // 素数判断
MathUtil.Clamp(150, 0, 100);        // 100
MathUtil.Lerp(0, 100, 0.5);         // 50

// 统计
var mean = StatisticsUtil.Mean(data);
var median = StatisticsUtil.Median(data);
var stdDev = StatisticsUtil.StandardDeviation(data);
var corr = StatisticsUtil.Correlation(x, y);

// 几何
var area = GeometryUtil.Area(rectangle);
var hull = GeometryUtil.ConvexHull(points);
var centroid = GeometryUtil.Centroid(points);

// 矩阵
var result = MatrixUtil.Multiply(a, b);
var det = MatrixUtil.Determinant(matrix);
var inv = MatrixUtil.Inverse(matrix);
```

### 🛡️ 通用工具

```csharp
// 异步重试
var result = await RetryUtil.ExecuteAsync(() => DoWork(), maxRetries: 3);

// 限流器
var limiter = RateLimiter.CreateTokenBucket(100, 10.0);
if (RateLimiter.TryAcquire(limiter)) { /* 允许请求 */ }

// 断路器
var cb = CircuitBreakerUtil.Create(5, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(30));
await CircuitBreakerUtil.ExecuteAsync(cb, async () => await CallExternalService());

// 异步锁
var asyncLock = AsyncLockUtil.Create();
using (await asyncLock.AcquireAsync()) { /* 互斥操作 */ }

// 事件总线
EventBus.Subscribe<MyEvent>(e => { /* 处理事件 */ });
EventBus.Publish(new MyEvent { Message = "hello" });

// 分页
var pagedList = PageUtil.CreatePagedList(allItems, page: 2, pageSize: 20);

// 控制台美化
ConsoleUtil.WriteSuccess("操作成功！");
ConsoleUtil.WriteError("出错了！");
ConsoleUtil.WriteProgressBar(70, 100);

// 性能基准
BenchmarkUtil.Measure("方法A", () => MethodA());
BenchmarkUtil.Compare("方法A", "方法B", () => MethodA(), () => MethodB());

// 验证器
var errors = ValidatorUtil.Validate(myObject);
var isEmail = ValidatorUtil.IsEmail("test@example.com");
var isPhone = ValidatorUtil.IsPhone("13800138000");

// 流式验证
var validator = FluentValidator.Create<User>()
    .RuleFor(x => x.Name, "姓名不能为空")
    .RuleFor(x => x.Age, "年龄必须大于0");
var errors = validator.Validate(user);
```

### 🖥️ 系统工具（Windows，EasyTool.System）

```csharp
// 硬件信息
var cpu = HardwareInfoUtil.GetCpuInfo();
var mem = HardwareInfoUtil.GetMemoryInfo();
var disk = HardwareInfoUtil.GetDiskInfo();
var gpu = HardwareInfoUtil.GetGpuInfo();

// 系统监控
var cpuUsage = SystemMonitorUtil.GetCpuUsage();
var memUsage = SystemMonitorUtil.GetMemoryUsage();

// 进程管理
var running = ProcessUtil.IsRunning("notepad");
ProcessUtil.Start("notepad");
ProcessUtil.Kill("notepad");

// 注册表
var val = RegistryUtil.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\MyApp", "Setting");
RegistryUtil.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\MyApp", "Setting", "value");

// Windows 服务
var isRunning = ServiceUtil.IsRunning("MyService");
ServiceUtil.Start("MyService");
ServiceUtil.Restart("MyService");

// 电源管理
var status = PowerUtil.GetPowerStatus();
var isCharging = PowerUtil.IsCharging();
PowerUtil.Shutdown();
PowerUtil.Restart();

// 屏幕截图
var screenshot = ScreenUtil.CaptureScreen();
var dpi = ScreenUtil.GetDpi();
var resolution = ScreenUtil.GetResolution();

// 键盘鼠标
var isDown = KeyboardUtil.IsKeyDown(VirtualKeyCode.VK_A);
var pos = MouseUtil.GetPosition();
MouseUtil.LeftClick();
```

### 🖼️ 图像处理（EasyTool.Image）

```csharp
// 基础操作
var resized = ImgUtil.ResizeImage(img, 800, 600);
var cropped = ImgUtil.CropImage(img, 0, 0, 200, 200);
var rotated = ImgUtil.RotateImage(img, 90);

// 水印
ImgUtil.AddTextWatermark(img, "EasyTool", font, brush, 10, 10);
ImgUtil.AddImageWatermark(img, watermarkImg, 0.5f, 10, 10);

// 调整
var bright = ImgUtil.AdjustBrightness(img, 1.2f);
var bw = ImgUtil.ConvertToBlackAndWhite(img);
var thumbnail = ImgUtil.MakeThumbnail(img, 100, 100);
```

### 🌐 Web 工具（EasyTool.Web）

```csharp
// 扫描 API 控制器生成 TypeScript 代码
var tsCode = BuildWebApiToTS.Build(assembly, "api/");
BuildWebApiToTS.BuildToFile(assembly, "api.ts");

// DTO 转 TypeScript
var tsCode = BuildDtoToTS.Build(assembly);
BuildDtoToTS.BuildToFile(assembly, "dto.ts");

// Option 枚举转 TypeScript
var tsCode = BuildOptionToTS.Build(assembly);
BuildOptionToTS.BuildToFile(assembly, "option.ts");
```

## 📁 项目结构

```
EasyTool/
├── 📁 EasyTool.Core                   # 核心包（零依赖，300+ 工具类）
│   ├── AICategory/                   # AI 工具（提示词、向量）
│   ├── BusinessCategory/             # 业务验证（40+ 中国特色验证器）
│   ├── CacheCategory/                # 缓存工具
│   ├── CodeCategory/                 # 编码加密（70+ 算法）
│   ├── CollectionsCategory/          # 集合与数据结构（45+ 工具）
│   ├── ColorCategory/                # 颜色工具
│   ├── ConvertCategory/              # 类型转换（CSV/XML/YAML/TOML/坐标）
│   ├── DataCategory/                 # 数据生成（Faker、QueryBuilder）
│   ├── DatabaseCategory/             # 数据库工具
│   ├── DateTimeCategory/             # 日期时间（农历/节气/节假日/Cron）
│   ├── IdentifierCategory/           # ID 生成（10+ 方案）
│   ├── IOCategory/                   # 文件操作（35+ 工具）
│   ├── MathCategory/                 # 数学工具（统计/矩阵/几何/插值）
│   ├── MediaCategory/                # 媒体基础
│   ├── NetCategory/                  # 网络工具（20+ HTTP/DNS/WebSocket/SSE）
│   ├── QueueCategory/                # 队列（Channel/延迟队列/优先队列）
│   ├── ReflectCategory/              # 反射工具
│   ├── SecurityCategory/             # 安全（XSS/SQL注入/JWT/证书/TLS）
│   ├── Standardization/              # 标准化（Option/Result/QueryPage）
│   ├── SystemCategory/               # 系统基础
│   ├── TextCategory/                 # 文本处理（30+ 拼音/敏感词/相似度）
│   ├── ToolCategory/                 # 通用工具（对象池/事件总线/限流/重试）
│   └── ValidationCategory/           # 验证器
├── 📁 EasyTool.Web                    # Web 开发（TypeScript 代码生成）
├── 📁 EasyTool.System                 # 系统工具（Windows 硬件/进程/服务）
├── 📁 EasyTool.Image                  # 图像处理（SkiaSharp）
├── 📁 EasyTool.NPOI                   # Excel 操作（NPOI）
├── 📁 EasyTool.Media                  # 音视频处理
├── 📁 EasyTool.EmitMapper             # 对象映射（EmitMapper）
├── 📁 EasyTool.AI                     # AI / LLM 工具
├── 📁 EasyTool.All                    # 整合包
└── 📁 EasyTool.UnitTests              # 单元测试（1069+ 测试）
```

## 📊 统计

| 指标 | 数值 |
|------|------|
| 源码文件 | 481 |
| 工具类 | 300+ |
| 公开方法 | 数千 |
| 单元测试 | 1069+ |
| 目标框架 | netstandard2.1 |
| 外部依赖（核心包） | 0 |

## 🔄 流式扩展方法完整列表

### 集合扩展

| 方法 | 说明 |
|------|------|
| `ForEach()` | 遍历并执行操作 |
| `DistinctBy()` | 按属性去重 |
| `Batch()` | 分批处理 |
| `Shuffle()` | 随机打乱 |
| `RandomElement()` | 随机取元素 |
| `JoinAsString()` | 拼接为字符串 |
| `IsNullOrEmpty()` | 是否为空 |
| `IsNotNullOrEmpty()` | 是否不为空 |

### 字符串扩展

| 方法 | 说明 |
|------|------|
| `EqualsIgnoreCase()` | 忽略大小写比较 |
| `ContainsIgnoreCase()` | 忽略大小写包含 |
| `Left()` / `Right()` | 取左/右 N 个字符 |
| `Truncate()` | 截断 |
| `Reverse()` | 反转 |
| `ToEnum<T>()` | 转枚举 |

### 数字扩展

| 方法 | 说明 |
|------|------|
| `InRange()` | 判断范围 |
| `Clamp()` | 限制范围 |
| `ToChinese()` | 转中文数字 |
| `ToMoneyChinese()` | 转中文金额 |
| `ToFileSize()` | 转文件大小 |
| `IsPrime()` | 素数判断 |
| `GCD()` / `LCM()` | 最大公约数/最小公倍数 |

### DateTime 扩展

| 方法 | 说明 |
|------|------|
| `ToDateString()` | 转日期字符串 |
| `ToDateTimeString()` | 转日期时间字符串 |
| `IsToday()` | 是否今天 |
| `IsWeekday()` | 是否工作日 |
| `GetAge()` | 计算年龄 |
| `GetQuarter()` | 获取季度 |
| `ToTimestamp()` | Unix 时间戳 |

### HttpClient 扩展

| 方法 | 说明 |
|------|------|
| `GetAsync<T>()` | GET 请求并反序列化 |
| `PostAsync<T>()` | POST 请求并反序列化 |
| `PutAsync<T>()` | PUT 请求并反序列化 |
| `DeleteAsync()` | DELETE 请求 |

## ❌ 我们不做

| 功能 | 成熟替代方案 |
|------|-------------|
| ORM/数据库 | EF Core, Dapper, SqlSugar |
| 日志 | Serilog, NLog |
| 缓存 | EasyCaching, Microsoft.Extensions.Caching |
| 依赖注入 | Microsoft.Extensions.DependencyInjection |
| 任务调度 | Quartz.NET, Hangfire |
| 消息队列 | MassTransit, CAP |
| WebSocket | SignalR |

## 🔗 相关链接

- [在线文档](https://easy-dotnet.com/pages/easytool/)
- [NuGet 包](https://www.nuget.org/packages/EasyTool.Core)
- [GitHub 仓库](https://github.com/dotnet-easy/easytool)

## 🤝 贡献

欢迎贡献！请查看 [贡献指南](CONTRIBUTING.md)。

## 📄 License

[MIT License](LICENSE)

---

> EasyTool - 让 .NET 开发更简单 ✨
