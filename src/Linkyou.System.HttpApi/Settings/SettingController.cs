using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Linkyou.System.Settings;

[RemoteService]
[Area("app")]
[Route("api/setting-management/global-settings")]
public class SettingController : Linkyou.System.Controllers.LinkyouSystemController, ISettingAppService
{
    private readonly ISettingAppService _appService;

    public SettingController(ISettingAppService appService)
        => _appService = appService;

    [HttpGet]
    public Task<List<SettingDto>> GetAsync() => _appService.GetAsync();

    [HttpPut]
    public Task UpdateAsync([FromBody] List<UpdateSettingDto> input) => _appService.UpdateAsync(input);
}
