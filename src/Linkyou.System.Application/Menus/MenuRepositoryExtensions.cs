using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Linkyou.System.Menus;

public static class MenuRepositoryExtensions
{
    public static async Task<(List<Menu> Items, long Count)> GetPagedListAsync(
        this IMenuRepository repo, GetMenuListInput input)
    {
        var sorting = input.Sorting ?? nameof(Menu.Sort);
        return (
            await repo.GetListAsync(input.Filter, input.SkipCount, input.MaxResultCount, sorting),
            await repo.GetCountAsync(input.Filter)
        );
    }
}
