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
list.IsNullOrEmpty();    // 是否为空
list.IsNotNullOrEmpty(); // 是否不为空

// 随机操作
var element = list.RandomElement();
var shuffled = list.Shuffle();

// 日期时间扩展
var dateStr = DateTime.Now.ToDateString();        // "2026-04-10"
var dateTimeStr = DateTime.Now.ToDateTimeString(); // "2026-04-10 12:30:00"
DateTime.Now.IsToday();        // 是否今天
DateTime.Now.IsWeekday();      // 是否工作日
birthDate.GetAge();            // 计算年龄
DateTime.Now.GetQuarter();     // 获取季度（1-4）
DateTime.Now.ToTimestamp();    // Unix时间戳（秒）
DateTime.Now.ToTimestampMs();  // Unix时间戳（毫秒）

// 数字扩展
100.InRange(1, 200);           // 判断范围
100.Clamp(50, 150);            // 限制范围
12345.ToChinese();             // "一万二千三百四十五"
1234.56.ToMoneyChinese();      // "壹仟贰佰叁拾肆元伍角陆分"
1024.ToFileSize();             // "1.00 KB"
```

### 🗃️ 对象池（减少GC压力）

```csharp
// StringBuilder池
var result = StringBuilderPool.Use(sb => {
    sb.Append("Hello").Append(" World");
    return sb.ToString();
});

// MemoryStream池
var data = MemoryStreamPool.Use(ms => {
    // 写入数据
    ms.WriteByte(1);
    return ms.ToArray();
});

// 字节数组池（基于ArrayPool）
var buffer = ByteArrayPool.Rent(1024);
try {
    // 使用buffer
} finally {
    ByteArrayPool.Return(buffer);
}

// 或使用Use方法自动归还
var result = ByteArrayPool.Use(1024, buffer => {
    // 处理数据
    return ProcessBuffer(buffer);
});
```

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
├── 📁 Core                        # 核心包（轻量级，无外部依赖）
│   ├── BusinessCategory/         # 业务验证（身份证、银行卡、手机号等30+种）
│   │   ├── PasswordGenerator     # 密码生成器
│   │   ├── TwoFactorAuthUtil     # TOTP双因素认证
│   │   ├── WeatherUtil           # 天气查询
│   │   └── ...
│   ├── CodeCategory/             # 编码加密（Base系列、哈希、国密SM2/SM3/SM4）
│   ├── CollectionsCategory/      # 集合操作
│   ├── DataCategory/             # 数据工具
│   │   └── FakerUtil             # 模拟数据生成器
│   ├── DateTimeCategory/         # 日期时间
│   ├── IdentifierCategory/       # ID生成（Snowflake/ULID/TSID/ObjectId）
│   ├── IOCategory/               # 文件操作
│   ├── MathCategory/             # 数学工具
│   ├── NetCategory/              # 网络工具
│   │   ├── HttpRetryUtil         # HTTP重试与熔断
│   │   ├── ShortUrlUtil          # 短链接生成
│   │   └── ...
│   ├── ReflectCategory/          # 反射工具
│   ├── SecurityCategory/         # 安全（XSS、SQL注入）
│   ├── TextCategory/             # 文本处理（拼音、敏感词、相似度）
│   └── ToolCategory/             # 通用工具
├── 📁 Extensions                 # 扩展模块
│   ├── EasyTool.AI/              # AI模块
│   ├── EasyTool.EmitMapper/      # 对象映射
│   ├── EasyTool.Image/           # 图像处理
│   ├── EasyTool.Media/           # 媒体处理
│   ├── EasyTool.NPOI/            # Excel处理
│   ├── EasyTool.System/          # 系统工具
│   └── EasyTool.Web/             # Web相关
├── 📁 Integration                # 整合包
│   └── EasyTool.All/             # 全功能包（发布这个就行）
└── 📁 Tests                      # 测试项目
    └── EasyTool.UnitTests/       # 单元测试（318个测试）
```

## ✨ 特色功能

### 🔐 密码与安全

```csharp
// 密码生成器
var password = PasswordGenerator.Generate();                    // 12位随机密码
var strong = PasswordGenerator.GenerateStrong();               // 16位强密码
var pin = PasswordGenerator.GeneratePin(6);                    // 6位PIN码
var passphrase = PasswordGenerator.GeneratePassphrase(4);      // 密码短语

// 密码强度检测
var strength = PasswordGenerator.CheckStrength("Password123!");  // Strong

// TOTP双因素认证（兼容Google Authenticator）
var secret = TwoFactorAuthUtil.GenerateSecret();
var totp = TwoFactorAuthUtil.GenerateTotp(secret);
var isValid = TwoFactorAuthUtil.VerifyTotp(secret, totp);
var qrContent = TwoFactorAuthUtil.GetQrCodeContent("MyApp", "user@example.com", secret);
```

### 🌤️ 天气查询

```csharp
// 配置API密钥
WeatherApiConfig.QWeatherApiKey = "your-api-key";

// 查询天气
var weather = await WeatherUtil.GetCurrentWeatherAsync("广州");
var forecast = await WeatherUtil.GetForecastAsync("北京", 7);
var airQuality = await WeatherUtil.GetAirQualityAsync("深圳");
```

### 🔗 短链接生成

```csharp
// 生成随机短码
var code = ShortUrlUtil.GenerateCode(6);

// 基于URL生成短码（同一URL生成相同短码）
var code = ShortUrlUtil.GenerateCodeFromUrl("https://example.com/long-url");

// Base62编码（适合ID转短码）
var shortCode = ShortUrlUtil.EncodeBase62(123456789);
var id = ShortUrlUtil.DecodeBase62(shortCode);

// 第三方短链接服务
var shortUrl = await ShortUrlUtil.ShortenWithIsGdAsync("https://example.com");
```

### 🌐 HTTP重试与熔断

```csharp
// 指数退避重试
var response = await HttpRetryUtil.ExecuteWithRetryAsync(
    httpClient, request,
    new HttpRetryUtil.RetryOptions { MaxRetries = 3 });

// 熔断器模式
var circuitBreaker = new HttpRetryUtil.CircuitBreaker(failureThreshold: 5);
await circuitBreaker.ExecuteAsync(async () => await httpClient.GetAsync(url));
```

### 🎲 模拟数据生成

```csharp
// 中文姓名
var name = FakerUtil.ChineseName();           // "张明"
var maleName = FakerUtil.ChineseName("male"); // 男性名字

// 中国地址
var address = FakerUtil.ChineseAddress();     // "广东省广州市天河区中山大道100号..."

// 手机号
var phone = FakerUtil.PhoneNumber();          // "13812345678"

// 邮箱
var email = FakerUtil.Email();                // "abc123@qq.com"

// 随机数据
var num = FakerUtil.RandomInt(1, 100);
var money = FakerUtil.RandomMoney(1, 1000);
var date = FakerUtil.RandomDate(5);           // 最近5年内随机日期
```

### 🇨🇳 中国特色业务验证

支持 30+ 种中国特色号码验证：

| 类型 | 工具类 | 示例 |
|------|--------|------|
| 身份证 | `IdCardUtil` | 18位身份证验证、解析 |
| 手机号 | `PhoneNumberUtil` | 大陆/香港/台湾手机号 |
| 银行卡 | `BankCardUtil` | 银行卡号验证、BIN识别 |
| 统一社会信用代码 | `SocialCreditCodeUtil` | 18位信用代码验证、机构类型解析 |
| 车牌号 | `PlateNumberUtil` | 新能源/普通车牌验证、归属地查询 |
| 护照 | `PassportUtil` | 中国护照验证 |
| 驾驶证 | `DrivingLicenseUtil` | 驾驶证号验证 |
| 港澳通行证 | `HkMacaoPassUtil` | 港澳通行证验证 |
| 台湾身份证 | `TwIdCardUtil` | 台湾身份证验证 |
| ... | ... | 更多... |

### 🎉 中国特色数据生成

```csharp
// 中文姓名生成
var name = ChineseNameUtil.Generate();  // "张明华"
var maleName = ChineseNameUtil.Generate(Gender.Male);
var names = ChineseNameUtil.GenerateBatch(10);

// 中国大学信息
var univ = UniversityUtil.GetByCode("10001");  // 北京大学
var univs985 = UniversityUtil.Get985Universities();
var univsByProvince = UniversityUtil.GetByProvince("江苏");

// 手机号归属地
var location = PhoneLocationUtil.GetLocation("13800138000");
// location.Carrier = "中国移动", location.Province = "广东", location.City = "广州"

// 公司名称生成
var company = CompanyUtil.Generate();  // "华创科技有限公司"
var techCompany = CompanyUtil.GenerateTechCompany();

// 地址生成
var address = AddressUtil.Generate();  // "广东省广州市天河区中山大道100号阳光花园5栋1单元101室"
var addressInfo = AddressUtil.GenerateFullInfo();
```

### 📅 中国节假日工具

```csharp
// 判断工作日/节假日（含调休）
ChineseHolidayUtil.IsWorkday(DateTime.Today);
ChineseHolidayUtil.IsHoliday(DateTime.Today);

// 获取节假日信息
var holiday = ChineseHolidayUtil.GetHolidayInfo(date);
var nextHoliday = ChineseHolidayUtil.GetNextHoliday();
var daysToHoliday = ChineseHolidayUtil.GetDaysToNextHoliday();

// 计算工作日
var workdays = ChineseHolidayUtil.GetWorkdaysBetween(start, end);
var futureDate = ChineseHolidayUtil.AddWorkdays(DateTime.Today, 10);

// 传统节日
var lunarHoliday = ChineseHolidayUtil.GetTraditionalHoliday(date);  // "春节", "中秋"等
```

### 📝 文本处理

```csharp
// 汉字转拼音
ChinesePinyinUtil.ToPinyin("中国北京");           // "zhong guo bei jing"
ChinesePinyinUtil.GetPinyinInitial("中国北京");   // "ZGBJ"
ChinesePinyinUtil.ToPinyinWithTone("中国");       // "zhong1 guo2"

// 中文数字转换
ChineseNumberUtil.ToChinese(12345);       // "一万二千三百四十五"
ChineseNumberUtil.ToMoney(1234.56);       // "壹仟贰佰叁拾肆元伍角陆分"
ChineseNumberUtil.FromChinese("一万二");  // 12000

// 敏感词过滤（DFA算法，高效）
SensitiveWordUtil.Init(new[] { "敏感词", "违规" });
SensitiveWordUtil.Contains("这是一个敏感词");  // 检测
SensitiveWordUtil.Filter("这是一个敏感词", '*');  // 替换

// 文本相似度
var similarity = TextSimilarityUtil.Calculate("hello", "hallo", SimilarityAlgorithm.Levenshtein);
```

### 🌏 行政区划工具

```csharp
// 省市区三级联动
var provinces = RegionUtil.GetProvinces();
var cities = RegionUtil.GetCities("440000");  // 广东省的城市
var districts = RegionUtil.GetDistricts("440100");  // 广州市的区

// 行政区划查询
var info = RegionUtil.GetByCode("440106");  // 天河区
var path = RegionUtil.GetFullPath("440106");  // "广东-广州-天河"
var hierarchy = RegionUtil.GetHierarchy("440106");  // ("广东", "广州", "天河")
```

### 🌤️ 二十四节气

```csharp
// 节气查询
var term = SolarTermUtil.GetSolarTerm(DateTime.Today);
var nextTerm = SolarTermUtil.GetNextSolarTerm();
var prevTerm = SolarTermUtil.GetPrevSolarTerm();

// 季节判断
var season = SolarTermUtil.GetSeason(DateTime.Today);  // "春"/"夏"/"秋"/"冬"
SolarTermUtil.IsSpring(DateTime.Today);
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
| **BusinessCategory** | 35+ | 业务验证（身份证、银行卡、车牌、节假日等） |
| **CodeCategory** | 70+ | 编码加密（Base系列、哈希、国密SM2/SM3/SM4） |
| **TextCategory** | 30+ | 文本处理（拼音、中文数字、敏感词、相似度） |
| **CollectionsCategory** | 45+ | 集合操作（BloomFilter、Trie、LRU、图、堆等） |
| **DateTimeCategory** | 10+ | 日期时间（农历、节气、节假日） |
| **IdentifierCategory** | 7+ | ID生成（Snowflake/ULID/TSID/ObjectId/NanoId） |
| **IOCategory** | 30+ | 文件操作（压缩、监控、CSV、Excel） |
| **MathCategory** | 18+ | 数学工具（统计、矩阵、几何、插值） |
| **NetCategory** | 20+ | 网络工具（HTTP重试、短链接、DNS） |
| **SecurityCategory** | 10+ | 安全工具（XSS、SQL注入、TLS、JWT） |
| **ToolCategory** | 35+ | 通用工具（对象池、事件总线、熔断器） |

## 🔗 相关链接

- [在线文档](https://easy-dotnet.com/pages/easytool/)
- [NuGet包](https://www.nuget.org/packages/EasyTool.Core)
- [GitHub仓库](https://github.com/li761747705/easytool)

## 📄 License

[MIT License](LICENSE)

---

> EasyTool - 让开发更简单 ✨