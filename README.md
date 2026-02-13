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

Easytool 是一个功能丰富且易用的 .NET 工具库，旨在帮助开发者快速、便捷地完成各类开发任务。 这些封装的工具涵盖了字符串、数字、集合、编码、日期、文件、IO、加密、JSON、HTTP客户端等一系列操作， 可以满足各种不同的开发需求。

> [More information](https://easy-dotnet.com/pages/easytool/)

## 🚀 快速开始

### 安装

~~~
PM> Install-Package EasyTool.Core
~~~
或者 .NET CLI 👇
~~~
dotnet add package EasyTool.Core
~~~

### 使用

复制文件或者目录
~~~csharp
FileUtil.Copy(sourceDir, destinationDir, isOverwrite)
~~~
克隆对象
~~~csharp
var a = CloneUtil.Clone<Person>(person);
~~~

## 🛠️ 目录

Easytool 封装了开发过程中一些常用的方法

---

## 📁 项目结构（最新更新：2025-02-13）

EasyTool.Core 采用**模块化分类结构**，所有工具按功能领域清晰划分到 15 个分类目录中：

### 📂 分类概览

| 分类 | 文件数 | 功能描述 |
|------|--------|----------|
| **BusinessCategory** | 1 | 业务数据处理（社会信用代码） |
| **CodeCategory** | 5 | 加密/编码工具（AES/DES/编码/哈希/十六进制） |
| **CollectionsCategory** | 7 | 集合扩展操作（数组/字典/链表/列表/队列/栈） |
| **ColorCategory** | 1 | 颜色处理扩展 |
| **ConvertCategory** | 1 | 数据类型转换工具 |
| **DateTimeCategory** | 4 | 日期时间处理（扩展/工具/日历/计时器） |
| **IdentifierCategory** | 1 | 标识符生成工具（UUID/ObjectId/Snowflake） |
| **IOCategory** | 7 | 文件/流/压缩操作（文件系统/文件类型/流/监控/ZIP） |
| **MathCategory** | 4 | 数学工具（计算/预测/随机数） |
| **NetCategory** | 3 | 网络工具（HTTP/IP/URL） |
| **ReflectCategory** | 3 | 反射/类型/属性扩展 |
| **Standardization** | 3 | 标准化类型（Option/QueryPage/Result） |
| **SystemCategory** | 2 | 系统环境工具（环境变量/系统信息） |
| **TextCategory** | 9 | 文本处理工具（正则/字符串/分割/XML/表情/脱敏） |
| **ToolCategory** | 8 | 通用扩展方法（委托/枚举/异常/GUID/对象/映射/任务/分页） |

### 📋 各分类详细说明

#### **BusinessCategory** - 业务数据处理
```
CreditCodeUtil.cs - 中国社会信用代码的验证和处理工具
```

#### **CodeCategory** - 加密/编码工具
```
AesUtil.cs - AES 加密/解密（支持 ECB/CBC 模式）
DesUtil.cs - DES 加密/解密（支持 ECB 模式）
EncodingUtil.cs - 编码转换工具
HashUtil.cs - 17 种哈希算法（加法/旋转/Bernstein/FNV/DJB/BKDR 等）
HexUtil.cs - 十六进制转换工具
```

#### **CollectionsCategory** - 集合扩展操作
```
ArrayExtension.cs - 数组操作扩展
DictionaryExtension.cs - 字典操作扩展
IEnumerableExtensions.cs - IEnumerable 集合遍历扩展
LinkedListUtil.cs - 链表操作工具
ListExtension.cs - 列表操作扩展
QueueUtil.cs - 队列操作工具
StackUtil.cs - 栈操作工具
```

#### **ColorCategory** - 颜色处理
```
ColorExtension.cs - 颜色扩展（RGB/HSV/HEX 转换）
```

#### **ConvertCategory** - 数据类型转换
```
ConvertExtension.cs - 通用数据类型转换（ToByte/ToShort/ToInt/ToLong/ToFloat/ToDouble/ToDecimal）
```

#### **DateTimeCategory** - 日期时间处理
```
DateTimeExtension.cs - DateTime 类型扩展方法
DateTimeUtil.cs - 日期时间工具类
LunarCalendarUtil.cs - 农历工具
TimerUtil.cs - 计时器工具
```

#### **IdentifierCategory** - 标识符生成
```
IdUtil.cs - ID 生成工具（有序 UUID/ObjectId/Snowflake ID）
```

#### **IOCategory** - 文件/流/压缩
```
FileSystemExtension.cs - 文件系统操作扩展
FileTypeExtension.cs - 文件类型判断
FileUtil.cs - 文件操作工具
StreamExtension.cs - 流操作扩展
Tailer.cs - 文件尾部追踪工具
WatchMonitor.cs - 文件监控工具
ZipUtil.cs - ZIP 压缩工具
```

#### **MathCategory** - 数学工具
```
MathUtil.cs - 数学计算工具
NumberExtension.cs - 数字类型扩展（偶数/质数/二进制/十六进制）
PredictUtil.cs - 预测算法工具
RandomUtil.cs - 随机数生成工具
```

#### **NetCategory** - 网络工具
```
HttpClientExtension.cs - HttpClient 扩展
IpUtil.cs - IP 地址处理工具
URLUtil.cs - URL 处理工具
```

#### **ReflectCategory** - 反射/类型/属性扩展
```
PropertyInfoExtension.cs - PropertyInfo 扩展（值获取/设置/特性检查）
ReflectUtil.cs - 反射工具类
TypeExtension.cs - Type 类型扩展（类型判断/友好名称/默认值）
```

#### **Standardization** - 标准化类型
```
Option.cs - 选项对象（用于前端下拉）
QueryPage.cs - 分页查询对象
Result.cs - 统一结果对象
```

#### **SystemCategory** - 系统环境工具
```
EnvUtil.cs - 环境变量工具
SystemUtil.cs - 系统信息工具
```

#### **TextCategory** - 文本处理工具（9个文件）
```
RegexUtil.cs - 正则表达式工具
StringBuilderExtension.cs - StringBuilder 扩展
StringComparisonExtension.cs - 字符串比较扩展
StringExtension.cs - 字符串验证扩展（邮箱/手机/URL/身份证等）
StrSplitter.cs - 字符串分割工具
StrUtil.cs - 字符串处理工具（命名转换/空格处理）
XmlUtil.cs - XML 处理工具
EmojiUtil.cs - 表情符号处理工具
DesensitizedUtil.cs - 数据脱敏工具（手机号/身份证/银行卡等）
```

#### **ToolCategory** - 通用扩展方法（8个文件）
```
DelegateExtension.cs - 委托扩展（安全调用）
EnumExtension.cs - 枚举扩展（获取描述）
ExceptionExtension.cs - 异常扩展（获取完整异常信息）
GuidExtension.cs - Guid 扩展（空值判断）
ObjectExtension.cs - 对象扩展（深拷贝/JSON序列化）
SimpleMapExtension.cs - 简单对象映射扩展
TaskExtension.cs - Task 异步扩展（Fire-and-Forget）
PageUtil.cs - 分页工具（支持多种数据源和排序方式）
```

---

### 📈 优化历程

本次更新主要完成了以下结构优化工作：

1. ✅ **ReflectCategory 扩展** - PropertyInfoExtension、TypeExtension 移入
2. ✅ **TextCategory 扩展** - StringComparisonExtension、StringExtension、EmojiUtil、DesensitizedUtil、StringBuilderExtension 移入
3. ✅ **CollectionsCategory 扩展** - IEnumerableExtensions 合并
4. ✅ **IdentifierCategory 新建** - ID 生成工具独立分类
5. ✅ **BusinessCategory 新建** - 业务数据处理独立分类
6. ✅ **ColorCategory 精简** - 颜色处理单独分类
7. ✅ **ToolCategory 优化** - SimpleMapExtension、PageUtil 移入
8. ✅ **空壳文件清理** - 删除仅含 Obsolete 方法的文件

**最终状态**：**15 个分类，55 个源文件**，结构清晰、功能明确、无重复代码。

---

> 项目采用模块化设计，每个分类职责单一，便于查找和维护。所有工具类都使用静态方法，无需实例化即可使用。

## 代码共享
