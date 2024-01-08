﻿

using PloliticalScienceSystemApi.GlobalSetting;

namespace PloliticalScienceSystemApi.Extension
{
    /// <summary>
    /// 通用跨域扩展
    /// </summary>
    public static class CorsExtension
    {
        public static IServiceCollection AddCorsService(this IServiceCollection services)
        {

            if (Appsettings.appBool("Cors_Enabled"))
            {
                services.AddCors(options => options.AddPolicy("CorsPolicy",//解决跨域问题
                builder =>
                {
                    builder.AllowAnyMethod()
                   .SetIsOriginAllowed(_ => true)
                   .AllowAnyHeader()
                   .AllowCredentials();
                }));
            }
            return services;
        }

        public static void UseCorsService(this IApplicationBuilder app)
        {
            if (Appsettings.appBool("Cors_Enabled"))
            {
                app.UseCors("CorsPolicy");
            }
            app.UseRouting();
        }

    }
}
