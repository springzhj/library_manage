using LibraryManageSystemApi.MiddlerWare;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryManageSystemApi.Extension
{
    public class NumberConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            //Console.WriteLine();
            try
            {
                //if (reader.TokenType == JsonTokenType.String)
                //{
                var li = reader.GetString();
                return long.Parse(li ?? "0");
                //}
                //else
                //{
                //return reader.GetInt64();
                //}
            }
            catch (Exception)
            {
                new InvalidParamError("参数非法");
            }
            return 0;
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

}
