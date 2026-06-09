using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Linkyou.System.Settings;

public interface ISettingAppService : IApplicationService
{
    Task<List<SettingDto>> GetGlobalSettingsAsync();
    Task UpdateGlobalSettingsAsync(List<UpdateSettingDto> input);
}
