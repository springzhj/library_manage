using DocumentFormat.OpenXml.InkML;
using MongoDB.Bson.Serialization.Attributes;
using LibraryManageSystemApi.Extension;

namespace LibraryManageSystemApi.Model
{
    public class Unit
    {
        [BsonElement("_id")]
        public long id { get; set; }
        public string? name { get; set; }
        public string? phone  { get; set; }
        public string? fax { get; set; }
        public string? contact { get; set; }
    }
}


