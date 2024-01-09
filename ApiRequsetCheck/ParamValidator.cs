using LibraryManageSystemApi.MiddlerWare;
using System.Reflection;

namespace LibraryManageSystemApi.ApiRequsetCheck
{

    public static class ParamValidator
    {
        public static bool CheckParamIsValid<T>(T obj)
        {
            foreach (var item in typeof(T).GetProperties())
            {
                if (item.GetCustomAttribute<RequeiredParamAttribute>(true) != null)
                {
                    if (!RequeiredParamAttribute.Valid(item.GetValue(obj).ToString()))
                    {
                        throw new InvalidParamError("参数非法");
                    }
                }
                var att = item.GetCustomAttribute<MaxMinControlParamAttribute>(true);
                if (att != null)
                {
                    if (!MaxMinControlParamAttribute.Valid(att, long.Parse(item.GetValue(obj).ToString())))
                        throw new InvalidParamError("参数非法");
                }

            }
            return true;
        }
    }
}
