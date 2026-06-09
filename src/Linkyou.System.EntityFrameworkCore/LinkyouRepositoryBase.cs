using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Linkyou.System.EntityFrameworkCore;

/// <summary>
/// Repository 基类，封装分页查询通用方法
/// </summary>
public abstract class LinkyouRepositoryBase<TEntity, TKey> :
    EfCoreRepository<LinkyouSystemDbContext, TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    protected LinkyouRepositoryBase(IDbContextProvider<LinkyouSystemDbContext> dbContextProvider)
        : base(dbContextProvider) { }

    /// <summary>构建过滤查询（子类重写以添加自定义过滤条件）</summary>
    protected virtual IQueryable<TEntity> BuildQuery(IQueryable<TEntity> query) => query;

    /// <summary>分页列表查询</summary>
    protected async Task<global::System.Collections.Generic.List<TEntity>> QueryListAsync(
        IQueryable<TEntity> query, int skipCount, int maxResultCount,
        string sorting, CancellationToken ct) =>
        await BuildQuery(query)
            .AsNoTracking()
            .OrderBy(sorting)
            .Skip(skipCount).Take(maxResultCount)
            .ToListAsync(ct);

    /// <summary>统计查询</summary>
    protected async Task<long> QueryCountAsync(
        IQueryable<TEntity> query, CancellationToken ct) =>
        await BuildQuery(query).LongCountAsync(ct);
}
