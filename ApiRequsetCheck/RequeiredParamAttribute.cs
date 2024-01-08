namespace PloliticalScienceSystemApi.ApiRequsetCheck
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequeiredParamAttribute : Attribute
    {
        public static bool Valid(string value)
        {
            return !string.IsNullOrEmpty(value);
        }
    }
}
