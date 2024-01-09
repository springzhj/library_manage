using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection.Metadata;
using System.Text;

namespace LibraryManageSystemApi.Extension
{
    public static class JsonHelper
    {
        public const string TIME_FORMAT_FULL = "yyyy-MM-dd HH:mm:ss";
        public const string TIME_FORMAT_FULL_2 = "MM-dd HH:mm:ss";
        /// <summary>
        /// 将JSONstring转换回对象class【单个对象】
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="JsonStr"></param>
        /// <returns></returns>
        public static T GetModelFromJsonString<T>(this string JsonStr, string timeFormatStr = TIME_FORMAT_FULL)
        {
            try
            {
                var serializerSettings = new JsonSerializerSettings
                {
                    // 设置为驼峰命名
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateFormatString = timeFormatStr
                };
                return JsonConvert.DeserializeObject<T>(JsonStr, serializerSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "=>" + ex.StackTrace);
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ApiResult"></typeparam>
        /// <param name="apiResult"></param>
        /// <param name="timeFormatStr"></param>
        /// <returns></returns>
        public static string GetJsonStr<ApiResult>(ApiResult apiResult, string timeFormatStr = null)
        {
            if (string.IsNullOrEmpty(timeFormatStr))
            {
                timeFormatStr = TIME_FORMAT_FULL;
            }
            var serializerSettings = new JsonSerializerSettings
            {
                // 设置为驼峰命名
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatString = timeFormatStr
            };
            return JsonConvert.SerializeObject(apiResult, Formatting.Indented, serializerSettings);
        }

        /// <summary>
        /// 将序列化的json字符串内容写入Json文件，并且保存
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="jsonConents">Json内容</param>
        public static async void WriteJsonFile(string path, string jsonConents)
        {
            await File.WriteAllTextAsync(path, jsonConents, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// 获取到本地的Json文件并且解析返回对应的json字符串
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <returns></returns>
        public static async Task<string> GetJsonFile(string filepath)
        {
            string json = string.Empty;
            using (FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    json = await sr.ReadToEndAsync();
                }
            }

            return json;
        }
    }
}
