using Microsoft.Extensions.Configuration;
using PloliticalScienceSystemApi.GlobalSetting;
using PloliticalScienceSystemApi.Extension;

namespace PloliticalScienceSystemApi.ServiceInject
{
    public static class AppsettingService
    {
        public static void InjectAppsettingConf(this IServiceCollection services)
        {
            IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var appsettings = new Appsettings(configuration);
            services.AddSingleton(appsettings);
            services.AddSingleton<QrCodeHelper>();
        }
    }
}
