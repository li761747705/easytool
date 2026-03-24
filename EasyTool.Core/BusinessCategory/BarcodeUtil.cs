using System;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 条形码类型枚举
    /// </summary>
    public enum BarcodeType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// EAN-13（13位国际商品条码）
        /// </summary>
        EAN13 = 1,

        /// <summary>
        /// EAN-8（8位商品条码）
        /// </summary>
        EAN8 = 2,

        /// <summary>
        /// UPC-A（12位北美商品条码）
        /// </summary>
        UPCA = 3,

        /// <summary>
        /// UPC-E（6位压缩商品条码）
        /// </summary>
        UPCE = 4,

        /// <summary>
        /// ITF-14（14位物流包装条码）
        /// </summary>
        ITF14 = 5,

        /// <summary>
        /// Code128（可变长度工业条码）
        /// </summary>
        Code128 = 6
    }

    /// <summary>
    /// 条形码工具类
    /// </summary>
    public static class BarcodeUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// EAN-13正则表达式
        /// </summary>
        private static readonly Regex EAN13Regex = new(@"^\d{13}$", RegexOptions.Compiled);

        /// <summary>
        /// EAN-8正则表达式
        /// </summary>
        private static readonly Regex EAN8Regex = new(@"^\d{8}$", RegexOptions.Compiled);

        /// <summary>
        /// UPC-A正则表达式
        /// </summary>
        private static readonly Regex UPCARegex = new(@"^\d{12}$", RegexOptions.Compiled);

        /// <summary>
        /// UPC-E正则表达式
        /// </summary>
        private static readonly Regex UPCERegex = new(@"^\d{6}$", RegexOptions.Compiled);

        /// <summary>
        /// ITF-14正则表达式
        /// </summary>
        private static readonly Regex ITF14Regex = new(@"^\d{14}$", RegexOptions.Compiled);

        /// <summary>
        /// 国家代码（GS1前缀）与地区映射
        /// </summary>
        private static readonly (string Prefix, string Region)[] Gs1PrefixMap =
        {
            ("000", "美国/加拿大"), ("001", "美国/加拿大"), ("019", "美国/加拿大"),
            ("020", "店内码"), ("029", "店内码"),
            ("030", "美国/加拿大"), ("039", "美国/加拿大"),
            ("040", "店内码"), ("049", "店内码"),
            ("050", "优惠券"), ("099", "优惠券"),
            ("100", "美国/加拿大"), ("139", "美国/加拿大"),
            ("200", "店内码"), ("299", "店内码"),
            ("300", "法国"), ("379", "法国"),
            ("380", "保加利亚"),
            ("383", "斯洛文尼亚"),
            ("385", "克罗地亚"),
            ("387", "波黑"),
            ("400", "德国"), ("440", "德国"),
            ("450", "日本"), ("459", "日本"),
            ("460", "俄罗斯"), ("469", "俄罗斯"),
            ("470", "吉尔吉斯斯坦"),
            ("471", "台湾"),
            ("474", "爱沙尼亚"),
            ("475", "拉脱维亚"),
            ("476", "阿塞拜疆"),
            ("477", "立陶宛"),
            ("478", "乌兹别克斯坦"),
            ("479", "斯里兰卡"),
            ("480", "菲律宾"),
            ("481", "白俄罗斯"),
            ("482", "乌克兰"),
            ("483", "土库曼斯坦"),
            ("484", "摩尔多瓦"),
            ("485", "亚美尼亚"),
            ("486", "格鲁吉亚"),
            ("487", "哈萨克斯坦"),
            ("488", "塔吉克斯坦"),
            ("489", "香港"),
            ("490", "日本"), ("499", "日本"),
            ("500", "英国"), ("509", "英国"),
            ("520", "希腊"),
            ("528", "黎巴嫩"),
            ("529", "塞浦路斯"),
            ("530", "阿尔巴尼亚"),
            ("531", "马其顿"),
            ("535", "马耳他"),
            ("539", "爱尔兰"),
            ("540", "比利时/卢森堡"), ("549", "比利时/卢森堡"),
            ("560", "葡萄牙"),
            ("569", "冰岛"),
            ("570", "丹麦"), ("579", "丹麦"),
            ("590", "波兰"),
            ("594", "罗马尼亚"),
            ("599", "匈牙利"),
            ("600", "南非"), ("601", "南非"),
            ("603", "加纳"),
            ("604", "塞内加尔"),
            ("608", "巴林"),
            ("609", "毛里求斯"),
            ("611", "摩洛哥"),
            ("613", "阿尔及利亚"),
            ("615", "尼日利亚"),
            ("616", "肯尼亚"),
            ("618", "科特迪瓦"),
            ("619", "突尼斯"),
            ("621", "叙利亚"),
            ("622", "埃及"),
            ("624", "利比亚"),
            ("625", "约旦"),
            ("626", "伊朗"),
            ("627", "科威特"),
            ("628", "沙特阿拉伯"),
            ("629", "阿联酋"),
            ("640", "芬兰"), ("649", "芬兰"),
            ("690", "中国"), ("699", "中国"),
            ("700", "挪威"), ("709", "挪威"),
            ("729", "以色列"),
            ("730", "瑞典"), ("739", "瑞典"),
            ("740", "危地马拉"),
            ("741", "萨尔瓦多"),
            ("742", "洪都拉斯"),
            ("743", "尼加拉瓜"),
            ("744", "哥斯达黎加"),
            ("745", "巴拿马"),
            ("746", "多米尼加"),
            ("750", "墨西哥"),
            ("754", "加拿大"), ("755", "加拿大"),
            ("759", "委内瑞拉"),
            ("760", "瑞士"), ("769", "瑞士"),
            ("770", "哥伦比亚"),
            ("773", "乌拉圭"),
            ("775", "秘鲁"),
            ("777", "玻利维亚"),
            ("779", "阿根廷"),
            ("780", "智利"),
            ("784", "巴拉圭"),
            ("786", "厄瓜多尔"),
            ("789", "巴西"), ("790", "巴西"),
            ("800", "意大利"), ("839", "意大利"),
            ("840", "美国"), ("849", "美国"),
            ("850", "古巴"),
            ("858", "斯洛伐克"),
            ("859", "捷克"),
            ("860", "塞尔维亚"),
            ("865", "蒙古"),
            ("867", "朝鲜"),
            ("868", "土耳其"), ("869", "土耳其"),
            ("870", "荷兰"), ("879", "荷兰"),
            ("880", "韩国"),
            ("884", "柬埔寨"),
            ("885", "泰国"),
            ("888", "新加坡"),
            ("890", "印度"),
            ("893", "越南"),
            ("896", "巴基斯坦"),
            ("899", "印度尼西亚"),
            ("900", "奥地利"), ("919", "奥地利"),
            ("930", "澳大利亚"), ("939", "澳大利亚"),
            ("940", "新西兰"), ("949", "新西兰"),
            ("950", "国际组织"),
            ("951", "国际组织"),
            ("955", "马来西亚"),
            ("958", "澳门")
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证条形码是否有效（自动识别类型）
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? barcode)
        {
            return IsValidEAN13(barcode) || IsValidEAN8(barcode) ||
                   IsValidUPCA(barcode) || IsValidUPCE(barcode) ||
                   IsValidITF14(barcode);
        }

        /// <summary>
        /// 验证EAN-13条形码是否有效
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidEAN13(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode) || !EAN13Regex.IsMatch(barcode))
            {
                return false;
            }

            return ValidateChecksum(barcode, 13);
        }

        /// <summary>
        /// 验证EAN-8条形码是否有效
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidEAN8(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode) || !EAN8Regex.IsMatch(barcode))
            {
                return false;
            }

            return ValidateChecksum(barcode, 8);
        }

        /// <summary>
        /// 验证UPC-A条形码是否有效
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidUPCA(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode) || !UPCARegex.IsMatch(barcode))
            {
                return false;
            }

            return ValidateChecksum(barcode, 12);
        }

        /// <summary>
        /// 验证UPC-E条形码是否有效
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidUPCE(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode) || !UPCERegex.IsMatch(barcode))
            {
                return false;
            }

            // UPC-E需要展开为UPC-A后验证
            string? expanded = ExpandUPCE(barcode);
            return expanded != null && IsValidUPCA(expanded);
        }

        /// <summary>
        /// 验证ITF-14条形码是否有效
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidITF14(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode) || !ITF14Regex.IsMatch(barcode))
            {
                return false;
            }

            return ValidateChecksum(barcode, 14);
        }

        /// <summary>
        /// 验证校验位
        /// </summary>
        private static bool ValidateChecksum(string barcode, int length)
        {
            int sum = 0;
            for (int i = 0; i < length - 1; i++)
            {
                int digit = barcode[i] - '0';
                // 从右向左，偶数位权重为3，奇数位权重为1
                int weight = ((length - 1 - i) % 2 == 1) ? 3 : 1;
                sum += digit * weight;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == (barcode[length - 1] - '0');
        }

        /// <summary>
        /// 计算校验位
        /// </summary>
        /// <param name="barcodeWithoutCheck">不含校验位的条形码</param>
        /// <returns>校验位（0-9），计算失败返回-1</returns>
        public static int CalculateCheckDigit(string? barcodeWithoutCheck)
        {
            if (string.IsNullOrWhiteSpace(barcodeWithoutCheck))
            {
                return -1;
            }

            int length = barcodeWithoutCheck.Length;
            int sum = 0;
            for (int i = 0; i < length; i++)
            {
                if (!char.IsDigit(barcodeWithoutCheck[i]))
                {
                    return -1;
                }
                int digit = barcodeWithoutCheck[i] - '0';
                // 从右向左，偶数位权重为3
                int weight = ((length - i) % 2 == 0) ? 3 : 1;
                sum += digit * weight;
            }

            return (10 - (sum % 10)) % 10;
        }

        #endregion

        #region 类型识别

        /// <summary>
        /// 获取条形码类型
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>条形码类型</returns>
        public static BarcodeType GetBarcodeType(string? barcode)
        {
            if (IsValidEAN13(barcode)) return BarcodeType.EAN13;
            if (IsValidEAN8(barcode)) return BarcodeType.EAN8;
            if (IsValidUPCA(barcode)) return BarcodeType.UPCA;
            if (IsValidUPCE(barcode)) return BarcodeType.UPCE;
            if (IsValidITF14(barcode)) return BarcodeType.ITF14;
            return BarcodeType.Unknown;
        }

        /// <summary>
        /// 获取条形码类型名称
        /// </summary>
        /// <param name="type">条形码类型</param>
        /// <returns>类型名称</returns>
        public static string GetBarcodeTypeName(BarcodeType type)
        {
            return type switch
            {
                BarcodeType.EAN13 => "EAN-13",
                BarcodeType.EAN8 => "EAN-8",
                BarcodeType.UPCA => "UPC-A",
                BarcodeType.UPCE => "UPC-E",
                BarcodeType.ITF14 => "ITF-14",
                BarcodeType.Code128 => "Code 128",
                _ => "未知"
            };
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取国家/地区（根据GS1前缀）
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>国家/地区名称</returns>
        public static string? GetRegion(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode) || barcode.Length < 3)
            {
                return null;
            }

            string prefix3 = barcode.Substring(0, 3);
            string prefix2 = barcode.Substring(0, 2);
            string prefix1 = barcode.Substring(0, 1);

            // 先匹配3位前缀
            foreach (var mapping in Gs1PrefixMap)
            {
                if (mapping.Prefix == prefix3)
                {
                    return mapping.Region;
                }
            }

            // 再匹配2位前缀
            foreach (var mapping in Gs1PrefixMap)
            {
                if (mapping.Prefix == prefix2)
                {
                    return mapping.Region;
                }
            }

            // 最后匹配1位前缀
            foreach (var mapping in Gs1PrefixMap)
            {
                if (mapping.Prefix == prefix1)
                {
                    return mapping.Region;
                }
            }

            return null;
        }

        /// <summary>
        /// 判断是否为中国商品条码
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>是否为中国商品条码</returns>
        public static bool IsChinaBarcode(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode) || barcode.Length < 3)
            {
                return false;
            }

            string prefix = barcode.Substring(0, 3);
            return prefix.CompareTo("690") >= 0 && prefix.CompareTo("699") <= 0;
        }

        /// <summary>
        /// 获取厂商识别代码（EAN-13的前7-9位）
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>厂商识别代码</returns>
        public static string? GetManufacturerCode(string? barcode)
        {
            if (!IsValidEAN13(barcode))
            {
                return null;
            }

            // EAN-13：前缀(2-3位) + 厂商代码(4-5位) + 商品代码(5位) + 校验位
            // 简化处理：返回前8位（不含校验位）
            return barcode!.Substring(0, 8);
        }

        /// <summary>
        /// 获取商品项目代码（EAN-13的第9-12位）
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>商品项目代码</returns>
        public static string? GetProductCode(string? barcode)
        {
            if (!IsValidEAN13(barcode))
            {
                return null;
            }

            return barcode!.Substring(8, 4);
        }

        #endregion

        #region 转换方法

        /// <summary>
        /// 将UPC-E转换为UPC-A
        /// </summary>
        /// <param name="upce">UPC-E条形码</param>
        /// <returns>UPC-A条形码，转换失败返回null</returns>
        public static string? ExpandUPCE(string? upce)
        {
            if (string.IsNullOrWhiteSpace(upce) || upce.Length != 6 || !UPCERegex.IsMatch(upce))
            {
                return null;
            }

            char lastDigit = upce[5];
            string result;

            switch (lastDigit)
            {
                case '0':
                    result = upce[0] + upce[1].ToString() + "00000" + upce[2] + upce[3] + upce[4];
                    break;
                case '1':
                    result = upce[0] + upce[1].ToString() + "10000" + upce[2] + upce[3] + upce[4];
                    break;
                case '2':
                    result = upce[0] + upce[1].ToString() + "20000" + upce[2] + upce[3] + upce[4];
                    break;
                case '3':
                    result = upce[0] + upce[1].ToString() + upce[2] + "00000" + upce[3] + upce[4];
                    break;
                case '4':
                    result = upce[0] + upce[1].ToString() + upce[2] + upce[3] + "00000" + upce[4];
                    break;
                default:
                    result = upce[0] + upce[1].ToString() + upce[2] + upce[3] + upce[4] + "0000" + lastDigit;
                    break;
            }

            // 添加系统字符(0)和计算校验位
            string fullCode = "0" + result;
            int checkDigit = CalculateCheckDigit(fullCode);
            return checkDigit >= 0 ? fullCode + checkDigit : null;
        }

        /// <summary>
        /// 将UPC-A转换为EAN-13
        /// </summary>
        /// <param name="upca">UPC-A条形码</param>
        /// <returns>EAN-13条形码</returns>
        public static string? ConvertUPCAToEAN13(string? upca)
        {
            if (!IsValidUPCA(upca))
            {
                return null;
            }

            return "0" + upca;
        }

        /// <summary>
        /// 将EAN-13转换为EAN-8（仅当适用于短码时）
        /// </summary>
        /// <param name="ean13">EAN-13条形码</param>
        /// <returns>EAN-8条形码，不适用返回null</returns>
        public static string? ConvertEAN13ToEAN8(string? ean13)
        {
            if (!IsValidEAN13(ean13))
            {
                return null;
            }

            // 只有特定前缀的EAN-13才能转换为EAN-8
            // 简化处理：仅支持部分转换
            return null;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 格式化条形码（去除非数字字符）
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>格式化后的条形码</returns>
        public static string? Normalize(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return null;
            }

            string cleaned = Regex.Replace(barcode, @"\D", "");
            return cleaned.Length >= 6 ? cleaned : null;
        }

        /// <summary>
        /// 格式化EAN-13（X-XXXXXX-XXXXX-X）
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>格式化后的条形码</returns>
        public static string? FormatEAN13(string? barcode)
        {
            if (!IsValidEAN13(barcode))
            {
                return null;
            }

            return $"{barcode![0]}-{barcode.Substring(1, 6)}-{barcode.Substring(7, 5)}-{barcode[12]}";
        }

        /// <summary>
        /// 条形码脱敏：69****1234
        /// </summary>
        /// <param name="barcode">条形码</param>
        /// <returns>脱敏后的条形码</returns>
        public static string? Mask(string? barcode)
        {
            string? normalized = Normalize(barcode);
            if (normalized == null || normalized.Length < 6)
            {
                return null;
            }

            int len = normalized.Length;
            int prefixLen = Math.Min(2, len / 3);
            int suffixLen = Math.Min(4, len / 3);

            return normalized.Substring(0, prefixLen) +
                   new string('*', len - prefixLen - suffixLen) +
                   normalized.Substring(len - suffixLen);
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机EAN-13条形码（仅供测试使用）
        /// </summary>
        /// <param name="prefix">前缀（可选，默认690-中国）</param>
        /// <returns>EAN-13条形码</returns>
        public static string GenerateRandomEAN13(string? prefix = null)
        {
            string pre = prefix ?? "690";
            while (pre.Length < 12)
            {
                pre += MathCategory.RandomUtil.RandomInt(0, 10).ToString();
            }

            pre = pre.Substring(0, 12);
            int checkDigit = CalculateCheckDigit(pre);
            return pre + checkDigit;
        }

        /// <summary>
        /// 生成随机ITF-14条形码（仅供测试使用）
        /// </summary>
        /// <returns>ITF-14条形码</returns>
        public static string GenerateRandomITF14()
        {
            string code = MathCategory.RandomUtil.RandomDigitString(13);
            int checkDigit = CalculateCheckDigit(code);
            return code + checkDigit;
        }

        #endregion
    }
}
