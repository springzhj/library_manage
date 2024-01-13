using LibraryManageSystemApi.ApiRequsetCheck;

namespace LibraryManageSystemApi.Controllers.Circulation.Dto
{
    public class PutlendingbooklistDto
    {
        [MaxMinControlParam(1, long.MaxValue)]
        public long appointmentid { get; set; }
    }
}
