using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 中文姓名生成器
    /// 支持随机生成真实中文姓名
    /// </summary>
    public static class ChineseNameUtil
    {
        #region 数据

        // 常用姓氏（前100大姓）
        private static readonly string[] CommonSurnames = {
            "王", "李", "张", "刘", "陈", "杨", "黄", "赵", "吴", "周",
            "徐", "孙", "马", "胡", "朱", "郭", "何", "罗", "高", "林",
            "郑", "梁", "谢", "宋", "唐", "许", "韩", "冯", "邓", "曹",
            "彭", "曾", "肖", "田", "董", "袁", "潘", "于", "蒋", "蔡",
            "余", "杜", "叶", "程", "苏", "魏", "吕", "丁", "任", "沈",
            "姚", "卢", "姜", "崔", "钟", "谭", "陆", "汪", "范", "金",
            "石", "廖", "贾", "夏", "韦", "付", "方", "白", "邹", "孟",
            "熊", "秦", "邱", "江", "尹", "薛", "闫", "段", "雷", "侯",
            "龙", "史", "陶", "黎", "贺", "顾", "毛", "郝", "龚", "邵",
            "万", "钱", "严", "覃", "武", "戴", "莫", "孔", "向", "汤"
        };

        // 复姓
        private static readonly string[] CompoundSurnames = {
            "欧阳", "上官", "皇甫", "司徒", "诸葛", "司马", "东方", "南宫",
            "西门", "北堂", "慕容", "公孙", "独孤", "令狐", "夏侯", "宇文"
        };

        // 男性常用名字用字
        private static readonly string[] MaleNameChars = {
            "伟", "强", "磊", "军", "勇", "涛", "明", "杰", "浩", "鹏",
            "华", "飞", "刚", "平", "波", "建", "国", "峰", "辉", "龙",
            "健", "俊", "毅", "威", "志", "斌", "宇", "超", "博", "文",
            "睿", "泽", "晨", "阳", "旭", "昊", "轩", "翔", "霖", "辰",
            "鑫", "宏", "亮", "宁", "坤", "哲", "成", "凯", "嘉", "瑞",
            "林", "松", "柏", "山", "海", "江", "河", "风", "云", "雨"
        };

        // 女性常用名字用字
        private static readonly string[] FemaleNameChars = {
            "芳", "娟", "敏", "静", "丽", "艳", "霞", "燕", "玲", "婷",
            "娜", "梅", "红", "萍", "琴", "英", "华", "慧", "琳", "洁",
            "颖", "雪", "琳", "倩", "欣", "怡", "月", "璐", "瑶", "佳",
            "娅", "莉", "蕾", "露", "薇", "瑾", "萱", "彤", "瑾", "馨",
            "梦", "琪", "珍", "依", "可", "妍", "茹", "欣", "彤", "琪",
            "蕾", "洁", "茜", "珊", "静", "淑", "惠", "珠", "翠", "雅"
        };

        // 中性名字用字
        private static readonly string[] NeutralNameChars = {
            "宁", "安", "晨", "雨", "雪", "涵", "睿", "航", "瑞", "辰",
            "阳", "旭", "昊", "轩", "翔", "霖", "宇", "文", "博", "超"
        };

        private static readonly Random Random = new Random();

        #endregion

        #region 生成方法

        /// <summary>
        /// 随机生成中文姓名
        /// </summary>
        /// <returns>中文姓名</returns>
        public static string Generate()
        {
            return Generate(null, null);
        }

        /// <summary>
        /// 随机生成中文姓名
        /// </summary>
        /// <param name="gender">性别</param>
        /// <returns>中文姓名</returns>
        public static string Generate(Gender? gender)
        {
            return Generate(gender, null);
        }

        /// <summary>
        /// 随机生成中文姓名
        /// </summary>
        /// <param name="gender">性别</param>
        /// <param name="nameLength">名字长度（1-2）</param>
        /// <returns>中文姓名</returns>
        public static string Generate(Gender? gender, int? nameLength)
        {
            // 随机选择姓氏（95%单姓，5%复姓）
            var surname = Random.NextDouble() < 0.95
                ? CommonSurnames[Random.Next(CommonSurnames.Length)]
                : CompoundSurnames[Random.Next(CompoundSurnames.Length)];

            // 确定名字长度
            var length = nameLength ?? (Random.NextDouble() < 0.6 ? 2 : 1);

            // 确定性别
            var actualGender = gender ?? (Random.NextDouble() < 0.5 ? Gender.Male : Gender.Female);

            // 生成名字
            var name = GenerateName(actualGender, length);

            return surname + name;
        }

        /// <summary>
        /// 生成单字名
        /// </summary>
        /// <param name="gender">性别</param>
        /// <returns>名字</returns>
        public static string GenerateSingleName(Gender? gender = null)
        {
            var actualGender = gender ?? (Random.NextDouble() < 0.5 ? Gender.Male : Gender.Female);
            return GenerateName(actualGender, 1);
        }

        /// <summary>
        /// 生成双字名
        /// </summary>
        /// <param name="gender">性别</param>
        /// <returns>名字</returns>
        public static string GenerateDoubleName(Gender? gender = null)
        {
            var actualGender = gender ?? (Random.NextDouble() < 0.5 ? Gender.Male : Gender.Female);
            return GenerateName(actualGender, 2);
        }

        /// <summary>
        /// 批量生成姓名
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="gender">性别（可选）</param>
        /// <returns>姓名列表</returns>
        public static List<string> GenerateBatch(int count, Gender? gender = null)
        {
            var names = new List<string>();
            for (var i = 0; i < count; i++)
            {
                names.Add(Generate(gender));
            }
            return names;
        }

        /// <summary>
        /// 生成全名（包含复姓）
        /// </summary>
        /// <returns>全名</returns>
        public static string GenerateFullName()
        {
            var surname = CompoundSurnames[Random.Next(CompoundSurnames.Length)];
            var gender = Random.NextDouble() < 0.5 ? Gender.Male : Gender.Female;
            var name = GenerateName(gender, 2);
            return surname + name;
        }

        #endregion

        #region 数据获取

        /// <summary>
        /// 获取常用姓氏列表
        /// </summary>
        /// <returns>姓氏列表</returns>
        public static string[] GetCommonSurnamesList()
        {
            return CommonSurnames.ToArray();
        }

        /// <summary>
        /// 获取复姓列表
        /// </summary>
        /// <returns>复姓列表</returns>
        public static string[] GetCompoundSurnamesList()
        {
            return CompoundSurnames.ToArray();
        }

        /// <summary>
        /// 获取随机姓氏
        /// </summary>
        /// <returns>姓氏</returns>
        public static string GetRandomSurname()
        {
            return CommonSurnames[Random.Next(CommonSurnames.Length)];
        }

        /// <summary>
        /// 获取随机复姓
        /// </summary>
        /// <returns>复姓</returns>
        public static string GetRandomCompoundSurname()
        {
            return CompoundSurnames[Random.Next(CompoundSurnames.Length)];
        }

        /// <summary>
        /// 判断是否为常见姓氏
        /// </summary>
        /// <param name="surname">姓氏</param>
        /// <returns>是否为常见姓氏</returns>
        public static bool IsCommonSurname(string surname)
        {
            return CommonSurnames.Contains(surname) || CompoundSurnames.Contains(surname);
        }

        #endregion

        #region 私有方法

        private static string GenerateName(Gender gender, int length)
        {
            var chars = gender == Gender.Male ? MaleNameChars : FemaleNameChars;
            var name = "";

            for (var i = 0; i < length; i++)
            {
                // 10%概率使用中性字
                if (Random.NextDouble() < 0.1)
                {
                    name += NeutralNameChars[Random.Next(NeutralNameChars.Length)];
                }
                else
                {
                    name += chars[Random.Next(chars.Length)];
                }
            }

            return name;
        }

        #endregion
    }

    /// <summary>
    /// 性别枚举
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// 男性
        /// </summary>
        Male,

        /// <summary>
        /// 女性
        /// </summary>
        Female
    }
}