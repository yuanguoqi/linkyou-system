using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Linkyou.System.Controllers;

/// <summary>
/// 所有自定义控制器的基类
/// 提供统一的本地化资源注入和命名空间
/// </summary>
public abstract class LinkyouSystemController : AbpControllerBase
{
    protected LinkyouSystemController()
    {
        // LocalizationResource 指向 Domain.Shared 中定义的资源
        LocalizationResource = typeof(LinkyouSystemResource);
    }
}
