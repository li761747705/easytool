using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EasyTool.BusinessCategory
{
    /// <summary>
    /// 天气工具类
    /// 提供天气查询功能，支持多种免费天气API
    /// </summary>
    public static class WeatherUtil
    {
        private static readonly HttpClient _httpClient = new();

        #region 数据结构

        /// <summary>
        /// 天气信息
        /// </summary>
        public class WeatherInfo
        {
            /// <summary>
            /// 城市
            /// </summary>
            public string City { get; set; } = string.Empty;

            /// <summary>
            /// 天气状况（晴、多云、雨等）
            /// </summary>
            public string Weather { get; set; } = string.Empty;

            /// <summary>
            /// 天气图标
            /// </summary>
            public string? Icon { get; set; }

            /// <summary>
            /// 温度（摄氏度）
            /// </summary>
            public double Temperature { get; set; }

            /// <summary>
            /// 体感温度
            /// </summary>
            public double? FeelsLike { get; set; }

            /// <summary>
            /// 湿度（%）
            /// </summary>
            public int Humidity { get; set; }

            /// <summary>
            /// 风速（km/h）
            /// </summary>
            public double? WindSpeed { get; set; }

            /// <summary>
            /// 风向
            /// </summary>
            public string? WindDirection { get; set; }

            /// <summary>
            /// 气压（hPa）
            /// </summary>
            public double? Pressure { get; set; }

            /// <summary>
            /// 能见度（km）
            /// </summary>
            public double? Visibility { get; set; }

            /// <summary>
            /// 更新时间
            /// </summary>
            public DateTime UpdateTime { get; set; }

            /// <summary>
            /// 预警信息
            /// </summary>
            public string? Alert { get; set; }
        }

        /// <summary>
        /// 天气预报
        /// </summary>
        public class WeatherForecast
        {
            /// <summary>
            /// 日期
            /// </summary>
            public DateTime Date { get; set; }

            /// <summary>
            /// 星期
            /// </summary>
            public string DayOfWeek { get; set; } = string.Empty;

            /// <summary>
            /// 天气状况
            /// </summary>
            public string Weather { get; set; } = string.Empty;

            /// <summary>
            /// 最高温度
            /// </summary>
            public double TempMax { get; set; }

            /// <summary>
            /// 最低温度
            /// </summary>
            public double TempMin { get; set; }

            /// <summary>
            /// 降水概率（%）
            /// </summary>
            public int? Precipitation { get; set; }

            /// <summary>
            /// 风向
            /// </summary>
            public string? WindDirection { get; set; }

            /// <summary>
            /// 风力等级
            /// </summary>
            public string? WindScale { get; set; }
        }

        /// <summary>
        /// 空气质量信息
        /// </summary>
        public class AirQualityInfo
        {
            /// <summary>
            /// AQI指数
            /// </summary>
            public int Aqi { get; set; }

            /// <summary>
            /// 空气质量等级（优、良、轻度污染等）
            /// </summary>
            public string Level { get; set; } = string.Empty;

            /// <summary>
            /// 主要污染物
            /// </summary>
            public string? PrimaryPollutant { get; set; }

            /// <summary>
            /// PM2.5浓度（μg/m³）
            /// </summary>
            public double? Pm25 { get; set; }

            /// <summary>
            /// PM10浓度（μg/m³）
            /// </summary>
            public double? Pm10 { get; set; }
        }

        #endregion

        #region 配置

        /// <summary>
        /// 天气API配置
        /// </summary>
        public static class WeatherApiConfig
        {
            /// <summary>
            /// 和风天气API Key（免费版每天1000次）
            /// 注册地址：https://dev.qweather.com/
            /// </summary>
            public static string? QWeatherApiKey { get; set; }

            /// <summary>
            /// 心知天气API Key
            /// 注册地址：https://www.seniverse.com/
            /// </summary>
            public static string? SeniverseApiKey { get; set; }

            /// <summary>
            /// OpenWeatherMap API Key
            /// 注册地址：https://openweathermap.org/
            /// </summary>
            public static string? OpenWeatherMapApiKey { get; set; }
        }

        #endregion

        #region 和风天气API

        /// <summary>
        /// 获取实时天气（使用和风天气API）
        /// </summary>
        /// <param name="city">城市名称或城市ID</param>
        /// <returns>天气信息</returns>
        public static async Task<WeatherInfo?> GetWeatherAsync(string city)
        {
            if (string.IsNullOrEmpty(WeatherApiConfig.QWeatherApiKey))
            {
                throw new InvalidOperationException("请先设置 WeatherApiConfig.QWeatherApiKey");
            }

            try
            {
                var url = $"https://devapi.qweather.com/v7/weather/now?location={Uri.EscapeDataString(city)}&key={WeatherApiConfig.QWeatherApiKey}";
                var response = await _httpClient.GetStringAsync(url);
                var json = JsonDocument.Parse(response);

                var root = json.RootElement;
                if (root.GetProperty("code").GetString() != "200")
                    return null;

                var now = root.GetProperty("now");
                return new WeatherInfo
                {
                    City = city,
                    Weather = now.GetProperty("text").GetString() ?? "",
                    Temperature = double.Parse(now.GetProperty("temp").GetString() ?? "0"),
                    FeelsLike = double.Parse(now.GetProperty("feelsLike").GetString() ?? "0"),
                    Humidity = int.Parse(now.GetProperty("humidity").GetString() ?? "0"),
                    WindSpeed = double.Parse(now.GetProperty("windSpeed").GetString() ?? "0"),
                    WindDirection = now.GetProperty("windDir").GetString(),
                    Pressure = double.Parse(now.GetProperty("pressure").GetString() ?? "0"),
                    Visibility = double.Parse(now.GetProperty("vis").GetString() ?? "0"),
                    UpdateTime = DateTime.Parse(root.GetProperty("updateTime").GetString() ?? DateTime.Now.ToString())
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取天气预报（3天）
        /// </summary>
        /// <param name="city">城市名称或城市ID</param>
        /// <returns>天气预报列表</returns>
        public static async Task<List<WeatherForecast>> GetForecastAsync(string city)
        {
            var result = new List<WeatherForecast>();

            if (string.IsNullOrEmpty(WeatherApiConfig.QWeatherApiKey))
            {
                return result;
            }

            try
            {
                var url = $"https://devapi.qweather.com/v7/weather/3d?location={Uri.EscapeDataString(city)}&key={WeatherApiConfig.QWeatherApiKey}";
                var response = await _httpClient.GetStringAsync(url);
                var json = JsonDocument.Parse(response);

                var root = json.RootElement;
                if (root.GetProperty("code").GetString() != "200")
                    return result;

                var daily = root.GetProperty("daily");
                foreach (var item in daily.EnumerateArray())
                {
                    var date = DateTime.Parse(item.GetProperty("fxDate").GetString()!);
                    result.Add(new WeatherForecast
                    {
                        Date = date,
                        DayOfWeek = date.ToString("ddd"),
                        Weather = item.GetProperty("textDay").GetString() ?? "",
                        TempMax = double.Parse(item.GetProperty("tempMax").GetString() ?? "0"),
                        TempMin = double.Parse(item.GetProperty("tempMin").GetString() ?? "0"),
                        Precipitation = int.Parse(item.GetProperty("precip").GetString() ?? "0"),
                        WindDirection = item.GetProperty("windDirDay").GetString(),
                        WindScale = item.GetProperty("windScaleDay").GetString()
                    });
                }

                return result;
            }
            catch
            {
                return result;
            }
        }

        /// <summary>
        /// 获取空气质量
        /// </summary>
        /// <param name="city">城市名称或城市ID</param>
        /// <returns>空气质量信息</returns>
        public static async Task<AirQualityInfo?> GetAirQualityAsync(string city)
        {
            if (string.IsNullOrEmpty(WeatherApiConfig.QWeatherApiKey))
            {
                return null;
            }

            try
            {
                var url = $"https://devapi.qweather.com/v7/air/now?location={Uri.EscapeDataString(city)}&key={WeatherApiConfig.QWeatherApiKey}";
                var response = await _httpClient.GetStringAsync(url);
                var json = JsonDocument.Parse(response);

                var root = json.RootElement;
                if (root.GetProperty("code").GetString() != "200")
                    return null;

                var now = root.GetProperty("now");
                return new AirQualityInfo
                {
                    Aqi = int.Parse(now.GetProperty("aqi").GetString() ?? "0"),
                    Level = now.GetProperty("category").GetString() ?? "",
                    PrimaryPollutant = now.GetProperty("primary").GetString(),
                    Pm10 = double.Parse(now.GetProperty("pm10").GetString() ?? "0"),
                    Pm25 = double.Parse(now.GetProperty("pm2p5").GetString() ?? "0")
                };
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 天气提示

        /// <summary>
        /// 获取穿衣建议
        /// </summary>
        /// <param name="temperature">温度（摄氏度）</param>
        /// <returns>穿衣建议</returns>
        public static string GetClothingAdvice(double temperature)
        {
            return temperature switch
            {
                < -10 => "严寒，建议穿厚羽绒服、棉衣，戴帽子手套",
                < 0 => "寒冷，建议穿羽绒服、棉衣",
                < 10 => "较冷，建议穿厚外套、毛衣",
                < 15 => "微凉，建议穿薄外套、卫衣",
                < 20 => "舒适，建议穿长袖衬衫、薄外套",
                < 25 => "温暖，建议穿短袖、薄衬衫",
                < 30 => "较热，建议穿短袖、短裤、裙子",
                _ => "炎热，建议穿轻薄透气的衣物，注意防晒"
            };
        }

        /// <summary>
        /// 获取运动建议
        /// </summary>
        /// <param name="weather">天气状况</param>
        /// <param name="aqi">AQI指数</param>
        /// <returns>运动建议</returns>
        public static string GetExerciseAdvice(string weather, int aqi)
        {
            if (aqi > 150)
                return "空气质量较差，不建议户外运动";

            return weather switch
            {
                "晴" => "天气晴朗，适合户外运动",
                "多云" => "天气适宜，适合户外运动",
                "阴" => "天气阴沉，可进行适度户外运动",
                "小雨" => "有雨，建议室内运动",
                "中雨" or "大雨" or "暴雨" => "雨势较大，不建议户外运动",
                "雪" or "小雪" or "中雪" or "大雪" => "有雪，路面湿滑，建议室内运动",
                _ => "请根据实际情况决定是否户外运动"
            };
        }

        #endregion

        #region 城市搜索

        /// <summary>
        /// 搜索城市
        /// </summary>
        /// <param name="keyword">城市名称关键字</param>
        /// <returns>城市列表</returns>
        public static async Task<List<(string Id, string Name, string Province)>> SearchCityAsync(string keyword)
        {
            var result = new List<(string, string, string)>();

            if (string.IsNullOrEmpty(WeatherApiConfig.QWeatherApiKey) || string.IsNullOrEmpty(keyword))
            {
                return result;
            }

            try
            {
                var url = $"https://geoapi.qweather.com/v2/city/lookup?location={Uri.EscapeDataString(keyword)}&key={WeatherApiConfig.QWeatherApiKey}";
                var response = await _httpClient.GetStringAsync(url);
                var json = JsonDocument.Parse(response);

                var root = json.RootElement;
                if (root.GetProperty("code").GetString() != "200")
                    return result;

                var location = root.GetProperty("location");
                foreach (var item in location.EnumerateArray())
                {
                    result.Add((
                        item.GetProperty("id").GetString() ?? "",
                        item.GetProperty("name").GetString() ?? "",
                        item.GetProperty("adm1").GetString() ?? ""
                    ));
                }

                return result;
            }
            catch
            {
                return result;
            }
        }

        #endregion
    }
}