using DocumentFormat.OpenXml.Office2013.Excel;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManageSystemApi.Model
{
    public class Penaltylist
    {
        [BsonElement("_id")]
        public long id { get; set; }
        public long lendingbooklistid { get; set; }
        public long userid { get; set; }
        public string? username { get; set; }
        public double penalty_number { get; set; }
        public bool isput { get; set; }
    }
}
