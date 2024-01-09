using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManageSystemApi.Model
{
    public class Returnlist
    {
        [BsonElement("_id")]
        public long id { get; set; }
        public long[]? orderlistids { get; set; }
        public string? checkperson { get; set; }
        public string? bookseller { get; set; }
        public string? printshop { get; set; }
        public string? return_reason { get; set; }
    }
}
