using PloliticalScienceSystemApi.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace PloliticalScienceSystemApi.MiddlerWare
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly LoggerHelper logger;

        public ErrorMiddleware(RequestDelegate next, LoggerHelper logger)
        {
            _next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (UnAuthrorizeError ex)
            {
                httpContext.Response.StatusCode = (int)ResultcodeEnum.NoPermission;
                logger.Error($"请求发生错误：{ex.Message},发生位置：{ex.StackTrace}");
                await HandleExceptionAsync(httpContext, httpContext.Response.StatusCode, "访问的资源权限不足");
            }
            catch (InvalidParamError ex)
            {
                logger.Error($"请求发生错误：{ex.Message},发生位置：{ex.StackTrace}");
                httpContext.Response.StatusCode = (int)ResultcodeEnum.BadRequest;
                await HandleExceptionAsync(httpContext, httpContext.Response.StatusCode, ex.Message ?? "请求参数错误，无法解析");
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, httpContext.Response.StatusCode, string.Empty);
                logger.Error($"请求发生错误：{ex.Message},发生位置：{ex.StackTrace}");
            }
            finally
            {
                var statusCode = httpContext.Response.StatusCode;
                var msg = "";

                switch (statusCode)
                {
                    case 401: msg = "访问的资源权限不足"; break;
                    case 403: msg = "访问的资源未授权"; break;
                    case 404: msg = "访问的资源未找到服务"; break;
                    case 415: msg = "参数列表type不对应"; break;
                    case 502: msg = "访问的资源请求错误"; break;
                    case 405: msg = "请求方法不允许"; break;
                    case 500: msg = "服务器内部错误"; break;
                    case 503: msg = "服务不可用"; break;
                    case 504: msg = "网关超时"; break;
                }
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    await HandleExceptionAsync(httpContext, statusCode, msg);
                }
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, int statuscode, string msg)
        {
            Result result = new();
            if (statuscode == (int)ResultcodeEnum.NoPermission)
            {
                result.SetMsg(msg).SetCode(ResultcodeEnum.NoPermission).SetData(new { result = false });
            }
            else if (statuscode == (int)ResultcodeEnum.BadRequest)
            {
                result.SetMsg(msg).SetCode(ResultcodeEnum.BadRequest).SetData(new { result = false });
            }
            else if (statuscode == (int)ResultcodeEnum.NotFound)
            {

                result.SetMsg("请求的资源未找到").SetCode(ResultcodeEnum.NotFound).SetData(new { result = false });
            }
            else if (statuscode == (int)ResultcodeEnum.Forbidden)
            {
                result.SetMsg("请求的资源不允许访问").SetCode(ResultcodeEnum.Forbidden).SetData(new { result = false });
            }
            else
            {
                result.SetMsg(msg).SetCode(ResultcodeEnum.NotSuccess).SetData(new { result = false });
            }
            context.Response.ContentType = "application/json;charset=utf-8";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ErrorMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorMiddleware>();
        }
    }

    public class Result
    {
        public ResultcodeEnum code { get; set; }
        public string? message { get; set; }
        public object? data { get; set; }
        public static Result Expire(ResultcodeEnum code, string msg = "")
        {
            return new Result() { code = code, message = Get(msg, "token_expiration") };
        }
        public static Result Error(string msg = "")
        {
            return new Result() { code = ResultcodeEnum.NotSuccess, message = Get(msg, "fail") };
        }
        public static Result Success(string msg = "")
        {
            return new Result() { code = ResultcodeEnum.Success, message = Get(msg, "succeed") };
        }
        public static Result SuccessError(string msg = "")
        {
            return new Result() { code = ResultcodeEnum.Success, message = Get(msg, "fail") };
        }
        public static Result UnAuthorize(string msg = "")
        {
            return new Result() { code = ResultcodeEnum.NoPermission, message = Get(msg, "unAuthorize") };
        }
        public Result SetCode(ResultcodeEnum newcode)
        {
            this.code = newcode;
            return this;
        }
        public Result SetData(object? obj)
        {
            this.data = obj;
            return this;
        }
        public Result SetMsg(string msg = "")
        {
            this.message = Get(msg, "错误");
            return this;
        }
        public static string Get(string msg, string msg2)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = msg2;
            }
            return msg;
        }
    }
    public enum ResultcodeEnum
    {
        /// <summary>
        /// 操作成功。
        /// </summary>
        Success = 200,
        /// <summary>
        /// 操作不成功
        /// </summary>
        NotSuccess = 500,
        /// <summary>
        /// 无效请求
        /// </summary>
        BadRequest = 400,
        /// <summary>
        /// 无权限
        /// </summary>
        NoPermission = 401,
        /// <summary>
        /// 权限不足
        /// </summary>
        Forbidden = 403,
        /// <summary>
        /// 未找到
        /// </summary>
        NotFound = 404,
        /// <summary>
        ///  Access过期
        /// </summary>
        AccessTokenExpire = 1001,

        /// <summary>
        /// Refresh过期
        /// </summary>
        RefreshTokenExpire = 1002,

        /// <summary>
        /// 没有角色登录
        /// </summary>
        NoRoleLogin = 1003,
    }

}
