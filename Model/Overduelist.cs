using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManageSystemApi.Model
{
    public class Overduelist
    {
        [BsonElement("_id")]
        public long id { get; set; }
        public long lendingbookslistid { get; set; }
        public long lendstarttime { get; set; }
        public long lendendtime { get; set; }
        public long returntime { get; set; }
        public long appointmentid { get; set; }
        public long userid { get; set; }
        public string? username { get; set; }
        public long bookid { get; set; }
        public string? bookname { get; set; }
        public string? catalog_number { get; set; }
       
    }
}
