namespace PloliticalScienceSystemApi.ApiRequsetCheck
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MaxMinControlParamAttribute : Attribute
    {
        public long MinIndex { get; set; }
        public long MaxIndex { get; set; }
        public MaxMinControlParamAttribute(long minIndex, long maxIndex)
        {
            MaxIndex = maxIndex;
            MinIndex = minIndex;
        }
        public static bool Valid<T>(T attribute, long value)
        {
            var MinIndex = (long)typeof(MaxMinControlParamAttribute).GetProperty("MinIndex").GetValue(attribute);
            var MaxIndex = (long)typeof(MaxMinControlParamAttribute).GetProperty("MaxIndex").GetValue(attribute);
            return value <= MaxIndex && value >= MinIndex;
        }
    }
}
