using Volo.Abp.Localization;
using Volo.Abp.Settings;
using Volo.Abp.Timing;

namespace Linkyou.System.Settings;

public class LinkyouSystemSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        // 默认语言
        context.Add(
            new SettingDefinition(
                LocalizationSettingNames.DefaultLanguage,
                defaultValue: "zh-Hans",
                displayName: new FixedLocalizableString("默认语言"),
                description: new FixedLocalizableString("Default Language")
            ));

        // 时区
        context.Add(
            new SettingDefinition(
                TimingSettingNames.TimeZone,
                defaultValue: "Asia/Shanghai",
                displayName: new FixedLocalizableString("时区"),
                description: new FixedLocalizableString("Timezone")
            ));

        // 登录需要验证邮箱
        context.Add(
            new SettingDefinition(
                "Abp.Identity.SignIn.RequireConfirmedEmail",
                defaultValue: "False",
                displayName: new FixedLocalizableString("登录需要验证邮箱"),
                description: new FixedLocalizableString("Require Confirmed Email")
            ));

        // 登录需要验证手机号
        context.Add(
            new SettingDefinition(
                "Abp.Identity.SignIn.RequireConfirmedPhoneNumber",
                defaultValue: "False",
                displayName: new FixedLocalizableString("登录需要验证手机号"),
                description: new FixedLocalizableString("Require Confirmed Phone")
            ));
    }
}
