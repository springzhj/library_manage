
using LibraryManageSystemApi.ApiRequsetCheck;
using LibraryManageSystemApi.Model;

namespace LibraryManageSystemApi.Controllers.Interview.Dto
{
    public class ChangeorderDto
    {
        public string? ISBN { get; set; }

        [MaxMinControlParam(1, long.MaxValue)]
        public long orderid { get; set; }

        [RequeiredParam]
        public Interviewlist.DocumenType documen_type { get; set; }
        [RequeiredParam]
        public string? title { get; set; }
        [RequeiredParam]
        public string? author { get; set; }
        [RequeiredParam]
        public string? publish_house { get; set; }
        [RequeiredParam]
        public string? order_price { get; set; }
        [RequeiredParam]
        public string? edition { get; set; }
        [RequeiredParam]
        public Interviewlist.Currency currency { get; set; }
        public string? description { get; set; }
    }
}
