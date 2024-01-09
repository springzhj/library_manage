using LibraryManageSystemApi.ApiRequsetCheck;
using LibraryManageSystemApi.Model;

namespace LibraryManageSystemApi.Controllers.Usersys.Dto
{
    public class UsersysEnrollDto
    {
        [RequeiredParam]
        public string? account { get; set; }

        [RequeiredParam]
        public string? password { get; set; }

        [RequeiredParam]
        public string? name { get; set; }

        [RequeiredParam]
        public string? identification_number { get; set; }

        [RequeiredParam]
        public int level { get; set; }

    }

}
