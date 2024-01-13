using LibraryManageSystemApi.ApiRequsetCheck;

namespace LibraryManageSystemApi.Controllers.Circulation.Dto
{
    public class PutbackbookDto
    {
        [MaxMinControlParam(1, long.MaxValue)]
        public long lendingbooklistid { get; set; }
    }
}
