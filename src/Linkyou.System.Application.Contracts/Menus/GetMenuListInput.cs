using Volo.Abp.Application.Dtos;

namespace Linkyou.System.Menus;

public class GetMenuListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
