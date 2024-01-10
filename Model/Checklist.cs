using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManageSystemApi.Model
{
    public class Checklist
    {
        [BsonElement("_id")]
        public long id { get; set; }
        public long orderlistid { get; set; }
        public string? checkperson { get; set; }
        public string? bookseller { get; set; }
        public string? printshop { get; set; }
        public bool iscataloged { get; set; }
        

    }
}
