using LibraryManageSystemApi.ApiRequsetCheck;
using LibraryManageSystemApi.Model;
namespace LibraryManageSystemApi.Controllers.Catalog.Dto
{
    public class CatalogbooksDto
    {
        [RequeiredParam]
        public List<long>? checklistids { get; set; }

        [RequeiredParam]
        public List<string> ? catalog_numbers { get; set; }

    }
}
