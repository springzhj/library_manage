
using LibraryManageSystemApi.ApiRequsetCheck;
using LibraryManageSystemApi.Model;

namespace LibraryManageSystemApi.Controllers.Interview.Dto
{
    public class CheckordersDto
    {

        [RequeiredParam]
        public List<long>? orderids { get; set; }

        [RequeiredParam]
        public string? checkperson { get; set; }
        [RequeiredParam]
        public string? bookseller { get; set; }
        [RequeiredParam]
        public string? printshop { get; set; }
    }
}
