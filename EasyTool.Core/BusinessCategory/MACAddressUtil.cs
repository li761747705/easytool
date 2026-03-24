using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// MAC地址工具类
    /// </summary>
    public static class MACAddressUtil
    {
        #region 常量与私有字段

        /// <summary>
        /// MAC地址正则表达式（多种格式）
        /// </summary>
        private static readonly Regex[] MACRegexes =
        {
            // XX:XX:XX:XX:XX:XX
            new(@"^([0-9A-Fa-f]{2}[:-]){5}[0-9A-Fa-f]{2}$", RegexOptions.Compiled),
            // XX-XX-XX-XX-XX-XX
            new(@"^([0-9A-Fa-f]{2}[-]){5}[0-9A-Fa-f]{2}$", RegexOptions.Compiled),
            // XXXX.XXXX.XXXX (Cisco格式)
            new(@"^([0-9A-Fa-f]{4}\.){2}[0-9A-Fa-f]{4}$", RegexOptions.Compiled),
            // XXXXXXXXXXXX (无分隔符)
            new(@"^[0-9A-Fa-f]{12}$", RegexOptions.Compiled)
        };

        /// <summary>
        /// OUI（组织唯一标识符）与厂商映射（部分）
        /// </summary>
        private static readonly (string Prefix, string Vendor)[] OuiPrefixMap =
        {
            // Apple
            ("00:03:93", "Apple"), ("00:05:02", "Apple"), ("00:0A:27", "Apple"),
            ("00:0A:95", "Apple"), ("00:0D:93", "Apple"), ("00:0E:B2", "Apple"),
            ("00:11:24", "Apple"), ("00:14:51", "Apple"), ("00:16:CB", "Apple"),
            ("00:17:F2", "Apple"), ("00:19:E3", "Apple"), ("00:1B:63", "Apple"),
            ("00:1C:B3", "Apple"), ("00:1D:4F", "Apple"), ("00:1E:52", "Apple"),
            ("00:1F:5B", "Apple"), ("00:1F:F3", "Apple"), ("00:22:41", "Apple"),
            ("00:23:12", "Apple"), ("00:23:32", "Apple"), ("00:23:6C", "Apple"),
            ("00:23:DF", "Apple"), ("00:24:36", "Apple"), ("00:25:00", "Apple"),
            ("00:25:4B", "Apple"), ("00:25:BC", "Apple"), ("00:26:08", "Apple"),
            ("00:26:4A", "Apple"), ("00:26:B0", "Apple"), ("00:26:BB", "Apple"),
            ("00:26:DB", "Apple"), ("A4:83:E7", "Apple"), ("AC:87:A3", "Apple"),
            ("DC:A9:04", "Apple"), ("F0:DB:E2", "Apple"),

            // Samsung
            ("00:07:AB", "Samsung"), ("00:0D:E5", "Samsung"), ("00:12:47", "Samsung"),
            ("00:13:77", "Samsung"), ("00:15:99", "Samsung"), ("00:16:6B", "Samsung"),
            ("00:17:C9", "Samsung"), ("00:18:AF", "Samsung"), ("00:1A:8A", "Samsung"),
            ("00:1B:59", "Samsung"), ("00:1D:2B", "Samsung"), ("00:1E:7D", "Samsung"),
            ("00:1F:36", "Samsung"), ("00:22:43", "Samsung"), ("00:24:90", "Samsung"),
            ("00:25:38", "Samsung"), ("08:EC:A9", "Samsung"), ("30:07:4D", "Samsung"),
            ("34:23:87", "Samsung"), ("38:2D:D8", "Samsung"), ("40:4D:7F", "Samsung"),
            ("54:88:0E", "Samsung"), ("5C:8D:4E", "Samsung"), ("64:5D:86", "Samsung"),
            ("6C:5C:14", "Samsung"), ("7C:1E:52", "Samsung"), ("88:36:6C", "Samsung"),
            ("8C:10:D4", "Samsung"), ("94:8B:C1", "Samsung"), ("98:F0:AB", "Samsung"),
            ("A0:10:81", "Samsung"), ("B0:DF:3A", "Samsung"), ("CC:61:E5", "Samsung"),
            ("D8:A2:5E", "Samsung"), ("E0:D5:5E", "Samsung"), ("E8:50:8B", "Samsung"),
            ("F0:27:65", "Samsung"),

            // Huawei
            ("00:0F:B5", "Huawei"), ("00:18:82", "Huawei"), ("00:1E:10", "Huawei"),
            ("00:25:68", "Huawei"), ("08:57:00", "Huawei"), ("0C:96:BF", "Huawei"),
            ("10:1D:59", "Huawei"), ("18:3E:0F", "Huawei"), ("28:ED:6A", "Huawei"),
            ("2C:B0:5D", "Huawei"), ("34:A3:95", "Huawei"), ("38:F8:5B", "Huawei"),
            ("40:4E:36", "Huawei"), ("44:8B:CE", "Huawei"), ("48:37:B9", "Huawei"),
            ("4C:FB:9F", "Huawei"), ("50:01:BB", "Huawei"), ("54:BF:64", "Huawei"),
            ("58:00:E3", "Huawei"), ("5C:FE:45", "Huawei"), ("64:9A:BE", "Huawei"),
            ("68:DB:CA", "Huawei"), ("6C:5D:43", "Huawei"), ("70:19:0F", "Huawei"),
            ("74:6A:D8", "Huawei"), ("78:44:76", "Huawei"), ("7C:1E:52", "Huawei"),
            ("80:8D:3F", "Huawei"), ("84:10:0D", "Huawei"), ("88:43:E1", "Huawei"),
            ("8C:71:F8", "Huawei"), ("90:2B:D2", "Huawei"), ("94:FE:22", "Huawei"),
            ("98:6F:1A", "Huawei"), ("9C:2E:A1", "Huawei"), ("A0:8F:85", "Huawei"),
            ("A4:4E:31", "Huawei"), ("B0:5B:48", "Huawei"), ("B4:69:21", "Huawei"),
            ("C0:BD:D1", "Huawei"), ("C4:4E:AC", "Huawei"), ("C8:5B:76", "Huawei"),
            ("CC:34:29", "Huawei"), ("D0:59:E4", "Huawei"), ("D4:7A:34", "Huawei"),
            ("DC:72:9B", "Huawei"), ("E0:37:BF", "Huawei"), ("E4:0D:73", "Huawei"),
            ("E8:4E:CE", "Huawei"), ("EC:9A:74", "Huawei"), ("F0:FE:6B", "Huawei"),
            ("FC:2C:55", "Huawei"),

            // Xiaomi
            ("00:BB:3E", "Xiaomi"), ("10:2A:B3", "Xiaomi"), ("18:59:36", "Xiaomi"),
            ("20:82:C0", "Xiaomi"), ("24:F9:A3", "Xiaomi"), ("28:ED:E1", "Xiaomi"),
            ("34:80:B3", "Xiaomi"), ("38:1A:21", "Xiaomi"), ("3C:BD:D8", "Xiaomi"),
            ("40:31:3C", "Xiaomi"), ("44:6F:D1", "Xiaomi"), ("48:88:CA", "Xiaomi"),
            ("4C:18:D6", "Xiaomi"), ("50:1E:2D", "Xiaomi"), ("50:EC:50", "Xiaomi"),
            ("58:44:98", "Xiaomi"), ("64:90:C1", "Xiaomi"), ("6C:5C:14", "Xiaomi"),
            ("6C:8D:C1", "Xiaomi"), ("74:A3:E4", "Xiaomi"), ("78:02:F8", "Xiaomi"),
            ("7C:1D:D9", "Xiaomi"), ("7C:8B:CA", "Xiaomi"), ("88:0F:10", "Xiaomi"),
            ("8C:4C:4B", "Xiaomi"), ("8C:F6:79", "Xiaomi"), ("90:82:37", "Xiaomi"),
            ("94:87:E0", "Xiaomi"), ("98:0C:82", "Xiaomi"), ("9C:2E:A1", "Xiaomi"),
            ("9C:99:A0", "Xiaomi"), ("A0:CB:FD", "Xiaomi"), ("A4:4E:31", "Xiaomi"),
            ("AC:29:3A", "Xiaomi"), ("B0:E2:35", "Xiaomi"), ("B8:C1:11", "Xiaomi"),
            ("C0:26:0D", "Xiaomi"), ("C0:EE:FB", "Xiaomi"), ("C4:0B:CB", "Xiaomi"),
            ("C4:4C:CA", "Xiaomi"), ("C8:1E:E7", "Xiaomi"), ("C8:94:BB", "Xiaomi"),
            ("CC:AF:78", "Xiaomi"), ("D0:D2:B0", "Xiaomi"), ("D4:5D:64", "Xiaomi"),
            ("D8:1C:79", "Xiaomi"), ("D8:96:95", "Xiaomi"), ("DC:A6:32", "Xiaomi"),
            ("E0:46:44", "Xiaomi"), ("E4:B2:1F", "Xiaomi"), ("EC:3A:FD", "Xiaomi"),
            ("EC:41:18", "Xiaomi"), ("F0:B4:29", "Xiaomi"), ("F4:28:53", "Xiaomi"),
            ("F8:A4:5F", "Xiaomi"), ("FC:6D:B3", "Xiaomi"), ("FC:A6:67", "Xiaomi"),

            // Intel
            ("00:02:B3", "Intel"), ("00:03:47", "Intel"), ("00:04:23", "Intel"),
            ("00:07:E9", "Intel"), ("00:0B:DB", "Intel"), ("00:0D:DA", "Intel"),
            ("00:0E:0C", "Intel"), ("00:0E:35", "Intel"), ("00:0E:A6", "Intel"),
            ("00:0F:B0", "Intel"), ("00:0F:EE", "Intel"), ("00:10:E0", "Intel"),
            ("00:11:0A", "Intel"), ("00:11:11", "Intel"), ("00:11:43", "Intel"),
            ("00:11:F5", "Intel"), ("00:12:3F", "Intel"), ("00:13:20", "Intel"),
            ("00:13:CE", "Intel"), ("00:13:E8", "Intel"), ("00:14:22", "Intel"),
            ("00:14:78", "Intel"), ("00:14:A5", "Intel"), ("00:15:17", "Intel"),
            ("00:15:C5", "Intel"), ("00:16:76", "Intel"), ("00:16:B6", "Intel"),
            ("00:17:08", "Intel"), ("00:17:9A", "Intel"), ("00:17:C2", "Intel"),
            ("00:18:13", "Intel"), ("00:18:68", "Intel"), ("00:18:DE", "Intel"),
            ("00:19:D1", "Intel"), ("00:1B:21", "Intel"), ("00:1C:BD", "Intel"),
            ("00:1D:72", "Intel"), ("00:1E:64", "Intel"), ("00:1E:67", "Intel"),
            ("00:1F:16", "Intel"), ("00:1F:29", "Intel"), ("00:21:5C", "Intel"),
            ("00:21:CC", "Intel"), ("00:22:FA", "Intel"), ("00:23:14", "Intel"),
            ("00:23:7E", "Intel"), ("00:23:AE", "Intel"), ("00:24:D7", "Intel"),
            ("00:25:66", "Intel"), ("00:26:B7", "Intel"), ("00:26:C6", "Intel"),
            ("00:26:C7", "Intel"), ("00:27:0E", "Intel"), ("00:30:1B", "Intel"),

            // Cisco
            ("00:00:0C", "Cisco"), ("00:01:42", "Cisco"), ("00:01:43", "Cisco"),
            ("00:01:63", "Cisco"), ("00:01:64", "Cisco"), ("00:01:96", "Cisco"),
            ("00:01:97", "Cisco"), ("00:01:C7", "Cisco"), ("00:02:16", "Cisco"),
            ("00:02:17", "Cisco"), ("00:02:4A", "Cisco"), ("00:02:7D", "Cisco"),
            ("00:02:7E", "Cisco"), ("00:02:FD", "Cisco"), ("00:03:6B", "Cisco"),
            ("00:03:6F", "Cisco"), ("00:03:E3", "Cisco"), ("00:04:27", "Cisco"),
            ("00:04:C1", "Cisco"), ("00:05:30", "Cisco"), ("00:05:32", "Cisco"),
            ("00:05:59", "Cisco"), ("00:05:85", "Cisco"), ("00:05:9A", "Cisco"),
            ("00:05:DC", "Cisco"), ("00:06:28", "Cisco"), ("00:06:52", "Cisco"),
            ("00:06:53", "Cisco"), ("00:07:0D", "Cisco"), ("00:07:0E", "Cisco"),
            ("00:07:0F", "Cisco"), ("00:07:50", "Cisco"), ("00:07:EC", "Cisco"),
            ("00:08:21", "Cisco"), ("00:08:22", "Cisco"), ("00:08:24", "Cisco"),
            ("00:08:2C", "Cisco"), ("00:08:A3", "Cisco"), ("00:09:0C", "Cisco"),
            ("00:09:0D", "Cisco"), ("00:09:41", "Cisco"), ("00:09:43", "Cisco"),
            ("00:09:44", "Cisco"), ("00:09:7C", "Cisco"), ("00:09:B7", "Cisco"),
            ("00:0A:B8", "Cisco"), ("00:0A:F4", "Cisco"), ("00:0B:5F", "Cisco"),
            ("00:0B:BE", "Cisco"), ("00:0B:FD", "Cisco"), ("00:0C:0C", "Cisco"),
            ("00:0C:30", "Cisco"), ("00:0C:31", "Cisco"), ("00:0C:CE", "Cisco"),
            ("00:0D:28", "Cisco"), ("00:0D:29", "Cisco"), ("00:0D:62", "Cisco"),
            ("00:0D:63", "Cisco"), ("00:0D:64", "Cisco"), ("00:0D:BD", "Cisco"),
            ("00:0D:BE", "Cisco"), ("00:0D:BF", "Cisco"), ("00:0D:C0", "Cisco"),
            ("00:0E:0C", "Cisco"), ("00:0E:38", "Cisco"), ("00:0E:39", "Cisco"),
            ("00:0E:3A", "Cisco"), ("00:0E:3B", "Cisco"), ("00:0E:3C", "Cisco"),
            ("00:0E:84", "Cisco"), ("00:0F:23", "Cisco"), ("00:0F:24", "Cisco"),
            ("00:0F:34", "Cisco"), ("00:0F:35", "Cisco"), ("00:0F:F7", "Cisco"),
            ("00:0F:F8", "Cisco"), ("00:10:0C", "Cisco"), ("00:10:0D", "Cisco"),
            ("00:10:0E", "Cisco"), ("00:10:0F", "Cisco"), ("00:10:54", "Cisco"),
            ("00:10:58", "Cisco"), ("00:10:7A", "Cisco"), ("00:10:7B", "Cisco"),
            ("00:10:E8", "Cisco"), ("00:10:F3", "Cisco"), ("00:10:F6", "Cisco"),
            ("00:11:1B", "Cisco"), ("00:11:20", "Cisco"), ("00:11:21", "Cisco"),
            ("00:11:2F", "Cisco"), ("00:11:30", "Cisco"), ("00:11:90", "Cisco"),
            ("00:11:91", "Cisco"), ("00:11:92", "Cisco"), ("00:11:93", "Cisco"),
            ("00:11:BB", "Cisco"), ("00:11:BC", "Cisco"), ("00:11:BD", "Cisco"),
            ("00:11:BE", "Cisco"), ("00:11:BF", "Cisco"), ("00:11:FA", "Cisco"),
            ("00:11:FB", "Cisco"), ("00:11:FC", "Cisco"), ("00:11:FD", "Cisco"),
            ("00:11:FE", "Cisco"), ("00:12:00", "Cisco"), ("00:12:01", "Cisco"),
            ("00:12:17", "Cisco"), ("00:12:1C", "Cisco"), ("00:12:1D", "Cisco"),
            ("00:12:40", "Cisco"), ("00:12:41", "Cisco"), ("00:12:43", "Cisco"),
            ("00:12:7F", "Cisco"), ("00:12:80", "Cisco"), ("00:12:DA", "Cisco"),
            ("00:12:DB", "Cisco"), ("00:12:DC", "Cisco"), ("00:12:F9", "Cisco"),
            ("00:12:FA", "Cisco"), ("00:13:1A", "Cisco"), ("00:13:1B", "Cisco"),
            ("00:13:1C", "Cisco"), ("00:13:19", "Cisco"), ("00:13:46", "Cisco"),
            ("00:13:47", "Cisco"), ("00:13:48", "Cisco"), ("00:13:49", "Cisco"),
            ("00:13:5F", "Cisco"), ("00:13:60", "Cisco"), ("00:13:61", "Cisco"),
            ("00:13:7F", "Cisco"), ("00:13:80", "Cisco"), ("00:13:81", "Cisco"),
            ("00:13:C3", "Cisco"), ("00:13:C4", "Cisco"), ("00:13:C5", "Cisco"),
            ("00:13:E8", "Cisco"), ("00:13:F7", "Cisco"), ("00:14:1B", "Cisco"),
            ("00:14:69", "Cisco"), ("00:14:6A", "Cisco"), ("00:14:6B", "Cisco"),
            ("00:14:97", "Cisco"), ("00:14:9A", "Cisco"), ("00:14:A1", "Cisco"),
            ("00:14:A2", "Cisco"), ("00:14:BF", "Cisco"), ("00:14:F1", "Cisco"),
            ("00:14:F2", "Cisco"), ("00:15:0C", "Cisco"), ("00:15:17", "Cisco"),
            ("00:15:1B", "Cisco"), ("00:15:1C", "Cisco"), ("00:15:2B", "Cisco"),
            ("00:15:60", "Cisco"), ("00:15:61", "Cisco"), ("00:15:62", "Cisco"),
            ("00:15:63", "Cisco"), ("00:15:FA", "Cisco"), ("00:15:FB", "Cisco"),
            ("00:15:FC", "Cisco"), ("00:15:FD", "Cisco"), ("00:16:35", "Cisco"),
            ("00:16:36", "Cisco"), ("00:16:37", "Cisco"), ("00:16:46", "Cisco"),
            ("00:16:47", "Cisco"), ("00:16:48", "Cisco"), ("00:16:78", "Cisco"),
            ("00:16:79", "Cisco"), ("00:16:9D", "Cisco"), ("00:16:9E", "Cisco"),
            ("00:16:C6", "Cisco"), ("00:16:C7", "Cisco"), ("00:16:C8", "Cisco"),
            ("00:17:0D", "Cisco"), ("00:17:0E", "Cisco"), ("00:17:0F", "Cisco"),
            ("00:17:59", "Cisco"), ("00:17:5A", "Cisco"), ("00:17:5B", "Cisco"),
            ("00:17:84", "Cisco"), ("00:17:85", "Cisco"), ("00:17:86", "Cisco"),
            ("00:17:94", "Cisco"), ("00:17:95", "Cisco"), ("00:17:96", "Cisco"),
            ("00:17:DF", "Cisco"), ("00:17:E0", "Cisco"), ("00:17:E1", "Cisco"),
            ("00:18:71", "Cisco"), ("00:18:72", "Cisco"), ("00:18:73", "Cisco"),
            ("00:18:81", "Cisco"), ("00:18:82", "Cisco"), ("00:18:83", "Cisco"),
            ("00:18:AF", "Cisco"), ("00:18:B9", "Cisco"), ("00:18:BA", "Cisco"),
            ("00:18:BB", "Cisco"), ("00:19:06", "Cisco"), ("00:19:07", "Cisco"),
            ("00:19:2F", "Cisco"), ("00:19:30", "Cisco"), ("00:19:55", "Cisco"),
            ("00:19:56", "Cisco"), ("00:19:57", "Cisco"), ("00:19:68", "Cisco"),
            ("00:19:69", "Cisco"), ("00:19:6A", "Cisco"), ("00:19:85", "Cisco"),
            ("00:19:86", "Cisco"), ("00:19:87", "Cisco"), ("00:19:A9", "Cisco"),
            ("00:19:AA", "Cisco"), ("00:19:AB", "Cisco"), ("00:19:E7", "Cisco"),
            ("00:19:E8", "Cisco"), ("00:19:E9", "Cisco"), ("00:1A:0D", "Cisco"),
            ("00:1A:0E", "Cisco"), ("00:1A:0F", "Cisco"), ("00:1A:2F", "Cisco"),
            ("00:1A:30", "Cisco"), ("00:1A:31", "Cisco"), ("00:1A:6B", "Cisco"),
            ("00:1A:6C", "Cisco"), ("00:1A:6D", "Cisco"), ("00:1A:A0", "Cisco"),
            ("00:1A:A1", "Cisco"), ("00:1A:A2", "Cisco"), ("00:1A:A3", "Cisco"),
            ("00:1A:E1", "Cisco"), ("00:1A:E2", "Cisco"), ("00:1A:E3", "Cisco"),
            ("00:1B:0D", "Cisco"), ("00:1B:0E", "Cisco"), ("00:1B:0F", "Cisco"),
            ("00:1B:53", "Cisco"), ("00:1B:54", "Cisco"), ("00:1B:55", "Cisco"),
            ("00:1B:8C", "Cisco"), ("00:1B:8D", "Cisco"), ("00:1B:8E", "Cisco"),
            ("00:1B:D4", "Cisco"), ("00:1B:D5", "Cisco"), ("00:1B:D6", "Cisco"),
            ("00:1C:0E", "Cisco"), ("00:1C:0F", "Cisco"), ("00:1C:10", "Cisco"),
            ("00:1C:58", "Cisco"), ("00:1C:59", "Cisco"), ("00:1C:5A", "Cisco"),
            ("00:1C:B0", "Cisco"), ("00:1C:B1", "Cisco"), ("00:1C:B2", "Cisco"),
            ("00:1C:F0", "Cisco"), ("00:1C:F1", "Cisco"), ("00:1C:F2", "Cisco"),
            ("00:1D:0F", "Cisco"), ("00:1D:10", "Cisco"), ("00:1D:11", "Cisco"),
            ("00:1D:45", "Cisco"), ("00:1D:46", "Cisco"), ("00:1D:47", "Cisco"),
            ("00:1D:9C", "Cisco"), ("00:1D:9D", "Cisco"), ("00:1D:9E", "Cisco"),
            ("00:1D:E2", "Cisco"), ("00:1D:E3", "Cisco"), ("00:1D:E4", "Cisco"),
            ("00:1E:13", "Cisco"), ("00:1E:14", "Cisco"), ("00:1E:15", "Cisco"),
            ("00:1E:49", "Cisco"), ("00:1E:4A", "Cisco"), ("00:1E:4B", "Cisco"),
            ("00:1E:79", "Cisco"), ("00:1E:7A", "Cisco"), ("00:1E:7B", "Cisco"),
            ("00:1E:B4", "Cisco"), ("00:1E:B5", "Cisco"), ("00:1E:B6", "Cisco"),
            ("00:1F:1D", "Cisco"), ("00:1F:1E", "Cisco"), ("00:1F:1F", "Cisco"),
            ("00:1F:6C", "Cisco"), ("00:1F:6D", "Cisco"), ("00:1F:6E", "Cisco"),
            ("00:1F:9D", "Cisco"), ("00:1F:9E", "Cisco"), ("00:1F:9F", "Cisco"),
            ("00:1F:C8", "Cisco"), ("00:1F:C9", "Cisco"), ("00:1F:CA", "Cisco"),
            ("00:21:0D", "Cisco"), ("00:21:0E", "Cisco"), ("00:21:0F", "Cisco"),
            ("00:21:55", "Cisco"), ("00:21:56", "Cisco"), ("00:21:57", "Cisco"),
            ("00:21:A0", "Cisco"), ("00:21:A1", "Cisco"), ("00:21:A2", "Cisco"),
            ("00:21:D5", "Cisco"), ("00:21:D6", "Cisco"), ("00:21:D7", "Cisco"),
            ("00:22:55", "Cisco"), ("00:22:56", "Cisco"), ("00:22:57", "Cisco"),
            ("00:22:90", "Cisco"), ("00:22:91", "Cisco"), ("00:22:92", "Cisco"),
            ("00:22:BD", "Cisco"), ("00:22:BE", "Cisco"), ("00:22:BF", "Cisco"),
            ("00:23:04", "Cisco"), ("00:23:05", "Cisco"), ("00:23:06", "Cisco"),
            ("00:23:33", "Cisco"), ("00:23:34", "Cisco"), ("00:23:35", "Cisco"),
            ("00:23:5C", "Cisco"), ("00:23:5D", "Cisco"), ("00:23:5E", "Cisco"),
            ("00:23:EB", "Cisco"), ("00:23:EC", "Cisco"), ("00:23:ED", "Cisco"),
            ("00:24:13", "Cisco"), ("00:24:14", "Cisco"), ("00:24:15", "Cisco"),
            ("00:24:50", "Cisco"), ("00:24:51", "Cisco"), ("00:24:52", "Cisco"),
            ("00:24:97", "Cisco"), ("00:24:98", "Cisco"), ("00:24:99", "Cisco"),
            ("00:24:B2", "Cisco"), ("00:24:B3", "Cisco"), ("00:24:B4", "Cisco"),
            ("00:24:F7", "Cisco"), ("00:24:F8", "Cisco"), ("00:24:F9", "Cisco"),
            ("00:25:1B", "Cisco"), ("00:25:1C", "Cisco"), ("00:25:1D", "Cisco"),
            ("00:25:2A", "Cisco"), ("00:25:2B", "Cisco"), ("00:25:2C", "Cisco"),
            ("00:25:61", "Cisco"), ("00:25:62", "Cisco"), ("00:25:63", "Cisco"),
            ("00:25:84", "Cisco"), ("00:25:85", "Cisco"), ("00:25:86", "Cisco"),
            ("00:25:B5", "Cisco"), ("00:25:B6", "Cisco"), ("00:25:B7", "Cisco"),
            ("00:26:0B", "Cisco"), ("00:26:0C", "Cisco"), ("00:26:0D", "Cisco"),
            ("00:26:51", "Cisco"), ("00:26:52", "Cisco"), ("00:26:53", "Cisco"),
            ("00:26:88", "Cisco"), ("00:26:89", "Cisco"), ("00:26:8A", "Cisco"),
            ("00:26:99", "Cisco"), ("00:26:9A", "Cisco"), ("00:26:9B", "Cisco"),
            ("00:26:CA", "Cisco"), ("00:26:CB", "Cisco"), ("00:26:CC", "Cisco"),
            ("00:50:56", "VMware"), ("00:0C:29", "VMware"), ("00:05:69", "VMware"),
            ("00:1C:14", "VMware"), ("00:50:56", "VMware")
        };

        #endregion

        #region 验证方法

        /// <summary>
        /// 验证MAC地址是否有效
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>是否有效</returns>
        public static bool IsValid(string? mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
            {
                return false;
            }

            foreach (var regex in MACRegexes)
            {
                if (regex.IsMatch(mac))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 信息提取

        /// <summary>
        /// 获取OUI（组织唯一标识符，前3字节）
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>OUI</returns>
        public static string? GetOUI(string? mac)
        {
            if (!IsValid(mac))
            {
                return null;
            }

            string clean = Clean(mac)!;
            return clean.Substring(0, 6).ToUpper();
        }

        /// <summary>
        /// 获取设备标识符（后3字节）
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>设备标识符</returns>
        public static string? GetDeviceId(string? mac)
        {
            if (!IsValid(mac))
            {
                return null;
            }

            string clean = Clean(mac)!;
            return clean.Substring(6, 6).ToUpper();
        }

        /// <summary>
        /// 获取厂商名称
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>厂商名称</returns>
        public static string? GetVendor(string? mac)
        {
            string? oui = GetOUI(mac);
            if (oui == null)
            {
                return null;
            }

            // 格式化为XX:XX:XX格式进行查找
            string formattedOui = $"{oui.Substring(0, 2)}:{oui.Substring(2, 2)}:{oui.Substring(4, 2)}".ToUpper();

            foreach (var mapping in OuiPrefixMap)
            {
                if (mapping.Prefix.Equals(formattedOui, StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.Vendor;
                }
            }

            return null;
        }

        /// <summary>
        /// 判断是否为组播地址
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>是否为组播地址</returns>
        public static bool IsMulticast(string? mac)
        {
            if (!IsValid(mac))
            {
                return false;
            }

            string clean = Clean(mac)!;
            // 第一个字节的最低位为1表示组播
            int firstByte = Convert.ToInt32(clean.Substring(0, 2), 16);
            return (firstByte & 0x01) == 1;
        }

        /// <summary>
        /// 判断是否为广播地址（FF:FF:FF:FF:FF:FF）
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>是否为广播地址</returns>
        public static bool IsBroadcast(string? mac)
        {
            string? clean = Clean(mac);
            return clean == "FFFFFFFFFFFF";
        }

        /// <summary>
        /// 判断是否为本地管理地址
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>是否为本地管理地址</returns>
        public static bool IsLocallyAdministered(string? mac)
        {
            if (!IsValid(mac))
            {
                return false;
            }

            string clean = Clean(mac)!;
            // 第一个字节的次低位为1表示本地管理
            int firstByte = Convert.ToInt32(clean.Substring(0, 2), 16);
            return (firstByte & 0x02) == 2;
        }

        #endregion

        #region 格式化方法

        /// <summary>
        /// 清理MAC地址（去除分隔符）
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>12位十六进制字符串</returns>
        public static string? Clean(string? mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
            {
                return null;
            }

            string cleaned = Regex.Replace(mac, @"[^\dA-Fa-f]", "").ToUpper();
            return cleaned.Length == 12 ? cleaned : null;
        }

        /// <summary>
        /// 格式化为标准格式（XX:XX:XX:XX:XX:XX）
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>格式化后的MAC地址</returns>
        public static string? Format(string? mac)
        {
            string? clean = Clean(mac);
            if (clean == null)
            {
                return null;
            }

            return $"{clean.Substring(0, 2)}:{clean.Substring(2, 2)}:{clean.Substring(4, 2)}:" +
                   $"{clean.Substring(6, 2)}:{clean.Substring(8, 2)}:{clean.Substring(10, 2)}";
        }

        /// <summary>
        /// 格式化为横线分隔（XX-XX-XX-XX-XX-XX）
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>格式化后的MAC地址</returns>
        public static string? FormatWithHyphens(string? mac)
        {
            return Format(mac)?.Replace(':', '-');
        }

        /// <summary>
        /// 格式化为Cisco格式（XXXX.XXXX.XXXX）
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>格式化后的MAC地址</returns>
        public static string? FormatCisco(string? mac)
        {
            string? clean = Clean(mac);
            if (clean == null)
            {
                return null;
            }

            return $"{clean.Substring(0, 4)}.{clean.Substring(4, 4)}.{clean.Substring(8, 4)}";
        }

        /// <summary>
        /// MAC地址脱敏：AA:BB:**:**:**:FF
        /// </summary>
        /// <param name="mac">MAC地址</param>
        /// <returns>脱敏后的MAC地址</returns>
        public static string? Mask(string? mac)
        {
            string? clean = Clean(mac);
            if (clean == null)
            {
                return null;
            }

            return $"{clean.Substring(0, 2)}:{clean.Substring(2, 2)}:**:**:**:{clean.Substring(10, 2)}";
        }

        #endregion

        #region 生成方法

        /// <summary>
        /// 生成随机MAC地址（仅供测试使用）
        /// </summary>
        /// <param name="vendor">厂商OUI（可选，默认随机生成）</param>
        /// <returns>MAC地址</returns>
        public static string GenerateRandom(string? vendor = null)
        {
            string oui;
            string deviceId;

            if (!string.IsNullOrWhiteSpace(vendor) && vendor.Length >= 6)
            {
                oui = vendor.Substring(0, 6).ToUpper();
            }
            else
            {
                // 随机生成OUI（设置本地管理位）
                int firstByte = MathCategory.RandomUtil.RandomInt(0, 255) | 0x02; // 设置本地管理位
                oui = firstByte.ToString("X2") + MathCategory.RandomUtil.RandomInt(0, 255).ToString("X2") +
                      MathCategory.RandomUtil.RandomInt(0, 255).ToString("X2");
            }

            // 随机生成设备ID
            deviceId = MathCategory.RandomUtil.RandomInt(0, 255).ToString("X2") +
                       MathCategory.RandomUtil.RandomInt(0, 255).ToString("X2") +
                       MathCategory.RandomUtil.RandomInt(0, 255).ToString("X2");

            string clean = oui + deviceId;
            return $"{clean.Substring(0, 2)}:{clean.Substring(2, 2)}:{clean.Substring(4, 2)}:" +
                   $"{clean.Substring(6, 2)}:{clean.Substring(8, 2)}:{clean.Substring(10, 2)}";
        }

        #endregion
    }
}
