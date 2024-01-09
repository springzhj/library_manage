using System.Runtime;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Text;

namespace LibraryManageSystemApi.Extension
{
    public static class HttpContextExtend
    {
        /// <summary>
        /// 判断是否为异步请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            string header = request.Headers["X-Requested-With"];
            return "XMLHttpRequest".Equals(header);
        }


        /// <summary>
        /// 通过鉴权完的token获取权限code
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static long GetUserId(this HttpContext httpContext)
        {
            long id = 0;
            try
            {
                id = long.Parse(httpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Sid)?.Value);
            }
            catch (Exception)
            {
                throw new Exception("用户登录信息解析失败，Sid信息失效");
            }
            return id;
        }
        /// <summary>
        /// 设置文件下载名称
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="fileName"></param>
        public static void FileInlineHandle(this HttpContext httpContext, string fileName)
        {
            string encodeFilename = System.Web.HttpUtility.UrlEncode(fileName, Encoding.GetEncoding("UTF-8"));
            httpContext.Response.Headers.Add("Content-Disposition", "inline;filename=" + encodeFilename);

        }

        /// <summary>
        /// 设置文件附件名称
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="fileName"></param>
        public static void FileAttachmentHandle(this HttpContext httpContext, string fileName)
        {
            string encodeFilename = System.Web.HttpUtility.UrlEncode(fileName, Encoding.GetEncoding("UTF-8"));
            httpContext.Response.Headers.Add("Content-Disposition", "attachment;filename=" + encodeFilename);

        }

        /// <summary>
        /// 获取语言种类
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetLanguage(this HttpContext httpContext)
        {
            string res = "zh-CN";
            var str = httpContext.Request.Headers["Accept-Language"].FirstOrDefault();
            if (str is not null)
            {
                res = str.Split(",")[0];
            }
            return res;

        }


        /// <summary>
        /// 获取请求Body参数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMethod"></param>
        /// <returns></returns>
        public static string GetRequestValue(this HttpContext context, string reqMethod)
        {
            string param;

            if (HttpMethods.IsPost(reqMethod) || HttpMethods.IsPut(reqMethod))
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                //需要使用异步方式才能获取
                param = reader.ReadToEndAsync().Result;
            }
            else
            {
                param = context.Request.QueryString.Value is null ? "" : context.Request.QueryString.Value.ToString();
            }
            return param;
        }


        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetClientIp(this HttpContext context)
        {
            if (context == null) return "";
            var result = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(result))
            {
                result = context.Connection.RemoteIpAddress?.ToString();
            }
            if (string.IsNullOrEmpty(result) || result.Contains("::1"))
                result = "127.0.0.1";

            result = result.Replace("::ffff:", "127.0.0.1");

            //Ip规则效验
            var regResult = Regex.IsMatch(result, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");

            result = regResult ? result : "127.0.0.1";
            return result;
        }

        /// <summary>
        /// 获取浏览器标识
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetUserAgent(this HttpContext context)
        {
            return context.Request.Headers["User-Agent"];
        }

    }

}
