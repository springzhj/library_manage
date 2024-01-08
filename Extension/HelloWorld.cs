using PloliticalScienceSystemApi.GlobalSetting;

namespace PloliticalScienceSystemApi.Extension
{
    public class HelloWorld
    {
        public string Author { get; set; } = "smallwhitepassingby";
        public DateTime NowTime { get; set; } = AppTimeManager.GetAppTime();
        public string Version { get; set; } = "v 1.0.0";
        public string Startmsg { get; set; } = "教师预约后台接口API服务开始启动......";
    }
}
