using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Linkyou.System.Menus;

public interface IMenuAppService : IApplicationService
{
    Task<MenuDto> GetAsync(Guid id);
    Task<PagedResultDto<MenuDto>> GetListAsync(GetMenuListInput input);
    Task<MenuDto> CreateAsync(CreateMenuDto input);
    Task<MenuDto> UpdateAsync(Guid id, UpdateMenuDto input);
    Task DeleteAsync(Guid id);
}
