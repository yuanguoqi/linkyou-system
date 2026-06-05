using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Linkyou.System.Menus;

public interface IMenuRepository : IRepository<Menu, Guid>
{
    Task<Menu?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<Menu>> GetListAsync(string? filter = null, int skipCount = 0,
        int maxResultCount = 10, string sorting = nameof(Menu.Sort),
        CancellationToken cancellationToken = default);
    Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default);
}
