using System.Collections.Generic;
using System.Threading.Tasks;
using Linkyou.System.Controllers;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Linkyou.System.Settings;

[RemoteService]
[Area("app")]
[Route("api/setting-management/global-settings")]
public class SettingController : LinkyouSystemController
{
    private readonly ISettingAppService _appService;

    public SettingController(ISettingAppService appService)
        => _appService = appService;

    [HttpGet]
    public Task<List<SettingDto>> GetGlobalSettingsAsync()
        => _appService.GetGlobalSettingsAsync();

    [HttpPut]
    public Task UpdateGlobalSettingsAsync([FromBody] List<UpdateSettingDto> input)
        => _appService.UpdateGlobalSettingsAsync(input);
}
