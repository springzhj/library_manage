using IdGen;
using LibraryManageSystemApi.GlobalSetting;
using System.Reflection.Emit;

namespace LibraryManageSystemApi.Extension
{
    public static class IdGenFunc
    {
        private static IdGenerator IdGen { set; get; }

        public static void IdGenInit(this IServiceCollection services)
        {
            IdGen = new IdGenerator(int.Parse(Appsettings.app("WorkId") ?? "0"));
            services.AddSingleton(IdGen);
        }
        public static long CreateOneId()
        {
            return IdGen.CreateId();
        }
        public static IEnumerable<long> CreateManyId(int counts)
        {
            return IdGen.Take(counts);
        }
    }
}
