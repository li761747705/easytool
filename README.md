<h1 align="center"> EasyTool </h1>
<div align="center">
一个开源的 .NET 工具库, 使得开发变得更加有效率
</div>
<div align="center">

[![pull_request](https://github.com/dotnet-easy/easytool/actions/workflows/pull_request.yml/badge.svg)](https://github.com/dotnet-easy/easytool/actions/workflows/pull_request.yml/badge.svg)
[![](https://img.shields.io/nuget/v/EasyTool.Core.svg)](https://www.nuget.org/packages/EasyTool.Core)
<p>
    <span>中文</span> | <a href="README.EN-US.md">English</a>
</p>
</div>

## 📚 简介

EasyTool 是一个**轻量级、零依赖、填补空白、中文友好**的 .NET 工具库，专注于提供成熟框架没有的功能。

### 🎯 设计理念

- ✅ **轻量级** - 核心包无外部依赖
- ✅ **零依赖** - 不引入第三方包
- ✅ **填补空白** - 只做成熟框架没有的功能
- ✅ **中文友好** - 中国特色业务验证、拼音转换、敏感词过滤

### ❌ 我们不做

| 功能 | 成熟替代方案 |
|------|-------------|
| ORM/数据库 | EF Core, Dapper, SqlSugar |
| 日志 | Serilog, NLog |
| 缓存 | EasyCaching, Microsoft.Extensions.Caching |
| HTTP客户端 | RestSharp, Flurl, Refit |
| JSON | System.Text.Json, Newtonsoft.Json |
| 验证 | FluentValidation |
| 对象映射 | AutoMapper, Mapster |
| 任务调度 | Quartz.NET, Hangfire |
| 限流/熔断 | Polly |
| 邮件 | MailKit, FluentEmail |
| 消息队列 | MassTransit, CAP |
| WebSocket | SignalR |
| JWT | System.IdentityModel.Tokens.Jwt |
| 二维码 | QRCoder, ZXing.Net |

## 🚀 快速开始

### 安装

**核心包（推荐）**
~~~
PM> Install-Package EasyTool.Core
~~~

**整合包（包含所有模块）**
~~~
PM> Install-Package EasyTool.All
~~~

**按需安装模块**
~~~
PM> Install-Package EasyTool.AI       # AI模块
PM> Install-Package EasyTool.Media    # 媒体处理
PM> Install-Package EasyTool.System   # 系统工具
~~~

### 使用示例

```csharp
using EasyTool.TextCategory;
using EasyTool.CodeCategory;
using EasyTool.BusinessCategory;

// 汉字转拼音
var pinyin = PinyinUtil.GetPinyin("中国");  // "zhongguo"
var firstLetter = PinyinUtil.GetFirstLetter("中国");  // "ZG"

// 敏感词过滤
SensitiveWordUtil.Init(new[] { "敏感词", "违规" });
var hasSensitive = SensitiveWordUtil.Contains("这是一个敏感词");  // true
var filtered = SensitiveWordUtil.Filter("这是一个敏感词", '*');  // "这是一个***"

// 身份证验证
var isValid = IdCardUtil.IsValid("110101199003077654");  // true
var info = IdCardUtil.GetInfo("110101199003077654");
// info.Province, info.City, info.Birthday, info.Gender...

// 国密SM4加密
var encrypted = Sm4Util.EncryptEcb("key123456789012", "明文");
var decrypted = Sm4Util.DecryptEcb("key123456789012", encrypted);

// ID生成
var snowflakeId = IdUtil.SnowflakeId();  // 雪花ID
var ulid = IdUtil.ULID();  // ULID
var objectId = IdUtil.ObjectId();  // ObjectId
```

## 📁 项目结构

```
EasyTool/
├── EasyTool.Core/           # 核心包（轻量级，无外部依赖）
│   ├── BusinessCategory/    # 业务验证（身份证、银行卡、手机号等30+种）
│   ├── CodeCategory/        # 编码加密（Base系列、哈希、国密SM2/SM3/SM4）
│   ├── CollectionsCategory/ # 集合操作
│   ├── DateTimeCategory/    # 日期时间
│   ├── IdentifierCategory/  # ID生成（Snowflake/ULID/TSID/ObjectId）
│   ├── IOCategory/          # 文件操作
│   ├── MathCategory/        # 数学工具
│   ├── NetCategory/         # 网络工具
│   ├── ReflectCategory/     # 反射工具
│   ├── SecurityCategory/    # 安全（XSS、SQL注入）
│   ├── TextCategory/        # 文本处理（拼音、敏感词、相似度）
│   └── ToolCategory/        # 通用工具
├── EasyTool.AI/             # AI模块
├── EasyTool.Media/          # 媒体处理
├── EasyTool.System/         # 系统工具
├── EasyTool.All/            # 整合包
├── EasyTool.Image/          # 图像处理
├── EasyTool.NPOI/           # Excel处理
└── EasyTool.Web/            # Web相关
```

## ✨ 特色功能

### 🇨🇳 中国特色业务验证

支持 30+ 种中国特色号码验证：

| 类型 | 工具类 | 示例 |
|------|--------|------|
| 身份证 | `IdCardUtil` | 18位身份证验证、解析 |
| 手机号 | `PhoneNumberUtil` | 大陆/香港/台湾手机号 |
| 银行卡 | `BankCardUtil` | 银行卡号验证、BIN识别 |
| 统一社会信用代码 | `CreditCodeUtil` | 18位信用代码验证 |
| 车牌号 | `LicensePlateUtil` | 新能源/普通车牌 |
| 护照 | `PassportUtil` | 中国护照验证 |
| 驾驶证 | `DrivingLicenseUtil` | 驾驶证号验证 |
| 港澳通行证 | `HkMacaoPassUtil` | 港澳通行证验证 |
| 台湾身份证 | `TwIdCardUtil` | 台湾身份证验证 |
| ... | ... | 更多... |

### 📝 文本处理

```csharp
// 汉字转拼音
PinyinUtil.GetPinyin("中国北京");           // "zhongguobeijing"
PinyinUtil.GetFirstLetter("中国北京");       // "ZGBJ"

// 敏感词过滤（DFA算法，高效）
SensitiveWordUtil.Init(new[] { "敏感词", "违规" });
SensitiveWordUtil.Contains("这是一个敏感词");  // 检测
SensitiveWordUtil.Filter("这是一个敏感词", '*');  // 替换

// 文本相似度
var similarity = TextSimilarityUtil.Calculate("hello", "hallo", SimilarityAlgorithm.Levenshtein);
```

### 🔐 加密编码

**Base编码系列**（成熟框架没有）
```csharp
Base32Util.Encode(data);
Base45Util.Encode(data);   // ISO/IEC 18004
Base58Util.Encode(data);   // 比特币地址
Base85Util.Encode(data);   // Ascii85
Base91Util.Encode(data);
Base92Util.Encode(data);
```

**哈希算法**
```csharp
HashUtil.MD5(text);
HashUtil.SHA256(text);
MurmurHashUtil.Hash32(data);  // 高性能非加密哈希
XxHashUtil.Hash32(data);      // 极速哈希
CityHashUtil.Hash64(data);
```

**国密算法**
```csharp
// SM2 非对称加密
Sm2Util.Encrypt(publicKey, data);
Sm2Util.Decrypt(privateKey, encrypted);

// SM3 哈希
Sm3Util.Hash(data);

// SM4 对称加密
Sm4Util.EncryptEcb(key, data);
Sm4Util.EncryptCbc(key, iv, data);
```

### 🆔 ID生成器

```csharp
// 雪花ID（分布式唯一ID）
var snowflakeId = IdUtil.SnowflakeId();

// ULID（字典序唯一ID）
var ulid = IdUtil.ULID();

// TSID（时间排序ID）
var tsid = IdUtil.TSID();

// ObjectId（MongoDB风格）
var objectId = IdUtil.ObjectId();

// 有序UUID
var orderedUuid = IdUtil.UUID(UUIDStyle.Sequential);
```

### 🌐 网络工具

```csharp
// IP地址处理
IpUtil.IsIpv4("192.168.1.1");
IpUtil.IsIpv6("2001:db8::1");
IpUtil.GetLocalIp();

// HTTP重试机制
var result = await HttpUtil.WithExponentialBackoffAsync(
    async () => await httpClient.GetStringAsync(url),
    maxRetries: 3
);
```

### 🤖 AI模块

```csharp
// OpenAI客户端
var client = new OpenAIClient("api-key");
var response = await client.ChatSimpleAsync("你好！");

// Token计数
var tokens = TokenizerUtil.CountTokens("Hello, world!", "gpt-4");

// 向量相似度
var similarity = VectorSimilarity.Cosine(vector1, vector2);
```

## 📊 文件统计

| 分类 | 文件数 | 说明 |
|------|--------|------|
| **BusinessCategory** | 5 | 业务验证（身份证、银行卡、手机号等） |
| **CodeCategory** | 25+ | 编码加密（Base系列、哈希、国密） |
| **TextCategory** | 25+ | 文本处理（拼音、敏感词、相似度） |
| **CollectionsCategory** | 10+ | 集合操作 |
| **DateTimeCategory** | 5 | 日期时间 |
| **IdentifierCategory** | 3 | ID生成 |
| **IOCategory** | 10+ | 文件操作 |
| **SecurityCategory** | 5 | 安全工具 |
| **ToolCategory** | 10+ | 通用工具 |

## 🔗 相关链接

- [在线文档](https://easy-dotnet.com/pages/easytool/)
- [NuGet包](https://www.nuget.org/packages/EasyTool.Core)
- [GitHub仓库](https://github.com/li761747705/easytool)

## 📄 License

[MIT License](LICENSE)

---

> EasyTool - 让开发更简单 ✨