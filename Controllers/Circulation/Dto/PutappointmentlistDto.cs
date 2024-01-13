using LibraryManageSystemApi.ApiRequsetCheck;

namespace LibraryManageSystemApi.Controllers.Circulation.Dto
{
    public class PutappointmentlistDto
    {
        [MaxMinControlParam(1, long.MaxValue)]
        public long userid { get; set; }
        [MaxMinControlParam(1, long.MaxValue)]
        public long bookid { get; set; }
    }
}
