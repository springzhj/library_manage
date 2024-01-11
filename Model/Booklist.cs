using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManageSystemApi.Model
{
    public class Booklist
    {
        [BsonElement("_id")]
        public long id { get; set; }
        public string? ISBN { get; set; }
        public Interviewlist.DocumenType documen_type { get; set; }
        public string? title { get; set; }
        public string? author { get; set; }
        public string? publish_house { get; set; }
        public string? edition { get; set; }
        public Interviewlist.Currency currency { get; set; }
        public string? order_price { get; set; }
        public string? catalog_number { get; set; }
        public Status status { get; set; }
        public int order_number { get; set; }
        public long  catalog_time { get; set; }
        public enum Status
        {
            no_borrow = 0,
            borrowed = 1,
            logout = 2
        }
    }
}
