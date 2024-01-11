using LibraryManageSystemApi.ApiRequsetCheck;
using LibraryManageSystemApi.Model;
namespace LibraryManageSystemApi.Controllers.Catalog.Dto
{
    public class CatalogbookDto
    {
        [MaxMinControlParam(1, long.MaxValue)]
        public long checklistid { get; set; }
        [RequeiredParam]
        public string? catalog_number { get; set; }

    }
}
