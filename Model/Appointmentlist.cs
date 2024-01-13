using MongoDB.Bson.Serialization.Attributes;

namespace LibraryManageSystemApi.Model
{
    public class Appointmentlist
    {
        [BsonElement("_id")]
        public long id { get; set; }
        public long userid { get; set; }
        public string? username { get; set; }
        public long bookid { get; set; }
        public string? bookname { get; set; }
        public string? catalog_number { get; set; }
        public long appointtime { get; set; }
        public Status status { get;  set; }

        public enum Status {
            appointed=0,
            Borrowed=1,
            finished=2
        }

    }
}
