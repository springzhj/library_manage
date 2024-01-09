namespace LibraryManageSystemApi.GlobalSetting
{
    public static class AppEnvironment
    {
        public static IWebHostEnvironment environment { get; set; }
        public static readonly bool UseTransaction = false;
        public static void RegistEnvironment(this WebApplicationBuilder buider)
        {
            environment = buider.Environment;
        }
        public static IWebHostEnvironment GetEnvironment()
        {
            return environment;
        }
    }
}
