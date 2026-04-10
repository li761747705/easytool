using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace EasyTool.DataCategory
{
    /// <summary>
    /// 模拟数据生成器
    /// 类似于Java的Faker，用于生成测试数据
    /// </summary>
    public static class FakerUtil
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        #region 中文姓名

        private static readonly string[] Surnames = {
            "王", "李", "张", "刘", "陈", "杨", "黄", "赵", "吴", "周",
            "徐", "孙", "马", "朱", "胡", "郭", "何", "林", "罗", "高"
        };

        private static readonly string[] MaleNames = {
            "伟", "强", "磊", "军", "勇", "杰", "涛", "明", "超", "华",
            "刚", "辉", "鹏", "斌", "俊", "宇", "浩", "凯", "峰", "毅"
        };

        private static readonly string[] FemaleNames = {
            "芳", "娟", "敏", "静", "丽", "艳", "娜", "秀", "英", "玲",
            "红", "梅", "燕", "霞", "婷", "莉", "琳", "萍", "雪", "倩"
        };

        /// <summary>
        /// 生成中文姓名
        /// </summary>
        public static string ChineseName(string? gender = null)
        {
            var surname = Surnames[RandomInt(Surnames.Length)];
            var isMale = gender?.ToLower() == "female" ? false :
                         gender?.ToLower() == "male" ? true :
                         RandomInt(2) == 0;
            var namePool = isMale ? MaleNames : FemaleNames;
            var name = namePool[RandomInt(namePool.Length)];
            return surname + name;
        }

        #endregion

        #region 地址

        private static readonly string[] Provinces = {
            "北京市", "上海市", "广东省", "江苏省", "浙江省", "山东省", "四川省", "湖北省", "河南省", "福建省"
        };

        private static readonly string[] Cities = {
            "广州", "深圳", "杭州", "南京", "苏州", "成都", "武汉", "青岛", "厦门", "福州"
        };

        /// <summary>
        /// 生成中国地址
        /// </summary>
        public static string ChineseAddress()
        {
            var province = Provinces[RandomInt(Provinces.Length)];
            var city = Cities[RandomInt(Cities.Length)];
            var street = "中山大道";
            var number = RandomInt(1, 999);
            var building = RandomInt(1, 20);
            var room = RandomInt(101, 2505);
            return $"{province}{city}市{street}{number}号{building}栋{room}室";
        }

        #endregion

        #region 手机号

        private static readonly string[] PhonePrefixes = {
            "130", "131", "132", "133", "134", "135", "136", "137", "138", "139",
            "150", "151", "152", "153", "155", "156", "157", "158", "159",
            "180", "181", "182", "183", "184", "185", "186", "187", "188", "189"
        };

        /// <summary>
        /// 生成手机号
        /// </summary>
        public static string PhoneNumber()
        {
            var prefix = PhonePrefixes[RandomInt(PhonePrefixes.Length)];
            return prefix + RandomNumberString(8);
        }

        #endregion

        #region 邮箱

        private static readonly string[] EmailDomains = {
            "qq.com", "163.com", "126.com", "gmail.com", "outlook.com"
        };

        /// <summary>
        /// 生成邮箱
        /// </summary>
        public static string Email()
        {
            var prefix = RandomString(8, true);
            var domain = EmailDomains[RandomInt(EmailDomains.Length)];
            return $"{prefix}@{domain}";
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 随机整数
        /// </summary>
        /// <param name="max">最大值（不包含）</param>
        /// <returns>0 到 max-1 之间的随机整数</returns>
        /// <exception cref="ArgumentException">当 max 小于等于 0 时抛出</exception>
        public static int RandomInt(int max)
        {
            if (max <= 0)
            {
                throw new ArgumentException($"参数 max 必须大于 0，当前值: {max}", nameof(max));
            }
            return RandomInt(0, max);
        }

        /// <summary>
        /// 随机整数（指定范围）
        /// 使用拒绝采样法消除模偏差，避免 int.MinValue 溢出
        /// </summary>
        /// <param name="min">最小值（包含）</param>
        /// <param name="max">最大值（不包含）</param>
        /// <returns>min 到 max-1 之间的随机整数</returns>
        /// <exception cref="ArgumentException">当 min 大于或等于 max 时抛出</exception>
        public static int RandomInt(int min, int max)
        {
            if (min >= max)
            {
                throw new ArgumentException($"参数 min 必须小于 max，当前: min={min}, max={max}");
            }
            var range = (uint)(max - min);
            var bytes = new byte[4];

            // 拒绝采样：排除会导致模偏差的值
            var maxValid = uint.MaxValue - (uint.MaxValue % range);
            uint value;
            do
            {
                _rng.GetBytes(bytes);
                value = BitConverter.ToUInt32(bytes, 0);
            } while (value >= maxValid);

            return (int)(value % range) + min;
        }

        /// <summary>
        /// 随机数字字符串
        /// </summary>
        public static string RandomNumberString(int length)
        {
            var chars = "0123456789";
            var result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = chars[RandomInt(10)];
            return new string(result);
        }

        /// <summary>
        /// 随机字符串
        /// </summary>
        public static string RandomString(int length, bool lowerCase = false)
        {
            var chars = lowerCase ? "abcdefghijklmnopqrstuvwxyz0123456789" : "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = chars[RandomInt(chars.Length)];
            return new string(result);
        }

        /// <summary>
        /// 随机选择
        /// </summary>
        /// <param name="items">元素集合</param>
        /// <returns>随机选中的元素</returns>
        /// <exception cref="ArgumentException">当集合为空时抛出</exception>
        public static T RandomChoice<T>(IEnumerable<T> items)
        {
            var list = items.ToList();
            if (list.Count == 0)
            {
                throw new ArgumentException("集合必须包含至少一个元素", nameof(items));
            }
            return list[RandomInt(list.Count)];
        }

        /// <summary>
        /// 随机布尔值
        /// </summary>
        public static bool RandomBool() => RandomInt(2) == 1;

        /// <summary>
        /// 随机日期
        /// </summary>
        /// <param name="pastYears">过去年数</param>
        /// <param name="futureYears">未来年数</param>
        /// <returns>随机日期</returns>
        /// <exception cref="ArgumentException">当 pastYears 和 futureYears 都为 0 时抛出</exception>
        public static DateTime RandomDate(int pastYears = 10, int futureYears = 0)
        {
            if (pastYears <= 0 && futureYears <= 0)
            {
                throw new ArgumentException("pastYears 和 futureYears 不能同时小于等于 0");
            }
            var start = DateTime.UtcNow.AddYears(-pastYears);
            var range = (pastYears + futureYears) * 365;
            return start.AddDays(RandomInt(range));
        }

        /// <summary>
        /// 随机金额
        /// </summary>
        /// <param name="min">最小金额</param>
        /// <param name="max">最大金额</param>
        /// <returns>随机金额</returns>
        /// <exception cref="ArgumentException">当 min 大于或等于 max 时抛出</exception>
        public static decimal RandomMoney(decimal min = 1, decimal max = 10000)
        {
            if (min >= max)
            {
                throw new ArgumentException($"参数 min 必须小于 max，当前: min={min}, max={max}");
            }
            var value = RandomInt((int)(min * 100), (int)(max * 100));
            return value / 100m;
        }

        #endregion
    }
}