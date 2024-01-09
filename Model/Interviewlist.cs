using DocumentFormat.OpenXml.VariantTypes;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManageSystemApi.Model
{
    public class Interviewlist
    {
        [BsonElement("_id")]
        public long id { get; set; }
        public long inter_personid { get; set; }
        public string? inter_personname { get; set; }
        public string? title { get; set; }
        public string? author { get; set; }
        public string? ISBN { get; set; }
        public string? publish_house { get; set; }
        public string? edition { get; set; }
        public Currency currency { get; set; }
        public string? order_price { get; set; }
        public int order_number { get; set; }
        public string? allprice { get; set; }
        public DocumenType documen_type { get; set; }
        public Status status { get; set; }

        public enum DocumenType
        {
            J = 1,
            M = 2,
            C1 = 3,
            D = 4,
            P = 5,
            S = 6,
            N = 7,
            R = 8,
            C2 = 9,
        }
        public enum Currency
        {
           // 0:人民币 1：美元 2：日元 3：欧元
           RMB=0,
           Dollar = 1,
           Yen = 2,
           Euro = 3
        }
        public enum Status {
            interview=0,
            order =1
        }


    }
}
