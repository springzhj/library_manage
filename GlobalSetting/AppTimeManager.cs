namespace PloliticalScienceSystemApi.GlobalSetting
{
    public static class AppTimeManager
    {
        public static DateTime GetAppTime()
        {
            return DateTime.Now.AddHours(8);
        }

        public static long GetAppTimeStamp()
        {
            DateTime now = DateTime.Now;
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan timeSpan = now - unixEpoch;
            long timestamp = (long)timeSpan.TotalSeconds * 1000;
            return timestamp;
        }
    }
}
