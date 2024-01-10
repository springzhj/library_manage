
using LibraryManageSystemApi.ApiRequsetCheck;
using LibraryManageSystemApi.Model;

namespace LibraryManageSystemApi.Controllers.Interview.Dto
{
    public class ReturnorderDto
    {

        [MaxMinControlParam(1, long.MaxValue)]
        public long orderid { get; set; }

        [RequeiredParam]
        public string? checkperson { get; set; }
        [RequeiredParam]
        public string? bookseller { get; set; }
        [RequeiredParam]
        public string? printshop { get; set; }
        public string? return_reason { get; set; }
    }
}
