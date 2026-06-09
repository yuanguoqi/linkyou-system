using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace Linkyou.System.Settings;

[Authorize]
public class SettingAppService : ApplicationService, ISettingAppService
{
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;
    private readonly ISettingDefinitionManager _settingDefinitionManager;

    public SettingAppService(
        ISettingProvider settingProvider,
        ISettingManager settingManager,
        ISettingDefinitionManager settingDefinitionManager)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
        _settingDefinitionManager = settingDefinitionManager;
    }

    public async Task<List<SettingDto>> GetAsync()
    {
        var definitions = await _settingDefinitionManager.GetAllAsync();
        var result = new List<SettingDto>();

        foreach (var def in definitions)
        {
            var value = await _settingProvider.GetOrNullAsync(def.Name);
            result.Add(new SettingDto
            {
                Name = def.Name,
                Value = value,
                DisplayName = def.DisplayName.Localize(StringLocalizerFactory),
                Description = def.Description?.Localize(StringLocalizerFactory),
                IsEncrypted = def.IsEncrypted,
            });
        }

        return result;
    }

    public async Task UpdateAsync(List<UpdateSettingDto> input)
    {
        foreach (var item in input)
        {
            if (string.IsNullOrEmpty(item.Name)) continue;
            await _settingManager.SetGlobalAsync(item.Name, item.Value);
        }
    }
}
