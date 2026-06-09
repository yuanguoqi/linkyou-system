using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Services;

namespace Linkyou.System.Settings;

public interface ISettingAppService : IApplicationService
{
    [HttpGet]
    Task<List<SettingDto>> GetAsync();

    [HttpPut]
    Task UpdateAsync(List<UpdateSettingDto> input);
}
