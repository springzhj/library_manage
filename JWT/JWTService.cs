using LibraryManageSystemApi.Extension;
using LibraryManageSystemApi.GlobalSetting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using LibraryManageSystemApi.JwtExtension;

namespace LibraryManageSystemApi.ServiceInject
{
    public static class JWTService
    {
        public const string PolicyName = "JWTAuth";
        public static IServiceCollection AddJwtService(this IServiceCollection services)
        {
            services.Configure<JWTTokenOptions>(Appsettings.appConfiguration("JwtAuthorize")!);
            services.AddTransient<JwtInvorker>();
            var jwtOptions = Appsettings.app<JWTTokenOptions>("JwtAuthorize");
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                             .AddJwtBearer(options =>
                             {
                                 options.Events = new JwtBearerEvents
                                 {
                                     OnAuthenticationFailed = (context) =>
                                     {
                                         return Task.CompletedTask;
                                     },
                                     OnMessageReceived = (context) =>
                                     {
                                         return Task.CompletedTask;
                                     },
                                     OnChallenge = (context) =>
                                     {
                                         return Task.CompletedTask;
                                     },
                                 };

                                 options.TokenValidationParameters = new TokenValidationParameters
                                 {
                                     ClockSkew = TimeSpan.Zero,//过期缓冲时间
                                     ValidateIssuer = true,//是否验证Issuer
                                     ValidateAudience = true,//是否验证Audience
                                     ValidateLifetime = true,//是否验证失效时间
                                     ValidateIssuerSigningKey = true,//是否验证SecurityKey
                                     ValidAudience = jwtOptions.Audience,//Audience
                                     ValidIssuer = jwtOptions.Issuer,//Issuer，这两项和前面签发jwt的设置一致
                                     IssuerSigningKey = new RsaSecurityKey(RSAFileHelper.GetPublicKey()),

                                 };
                             });
            return services;
        }

        public static IServiceCollection AddJWTAuthorizationService(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyName, polic =>
                {
                    polic.RequireClaim(ClaimTypes.Sid).RequireClaim(ClaimTypes.Name).RequireClaim(ClaimTypes.Role);
                    ///添加鉴权方式
                });
            });


            //注册自定义鉴权处理hander
            //services.AddSingleton<IAuthorizationHandler, CustomAuthorizationHandler>();
            return services;
        }


        public static void UseJWTService(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }

}
