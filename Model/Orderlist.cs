using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManageSystemApi.Model
{
    public class Orderlist
    {
        [BsonElement("_id")]
        public long id { get; set; }
        public long checklistid { get; set; }
        public long returnlistid { get; set; }

        public string? order_serial_number { get; set; }
        public long order_date { get; set; }
        public string? ISBN { get; set; }
        public Interviewlist.DocumenType documen_type { get; set; }
        public string? title { get; set; }
        public string? author { get; set; }
        public string? publish_house { get; set; }
        public string? order_price { get; set; }
        public string? edition { get; set; }
        public Interviewlist.Currency currency { get; set; }
        public Status status { get; set; }
        public bool effective { get; set; }
        public string? return_reason { get; set; }
        public string? description { get; set; }

        public enum Status {
            interview =0,
            order =1,
            returned =2,
            check=3,
            catalog=4
        }

    }
}
