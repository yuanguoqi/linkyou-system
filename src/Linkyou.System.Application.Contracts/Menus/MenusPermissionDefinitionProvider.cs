using Linkyou.System.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Linkyou.System.Menus;

public class MenusPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(MenusPermissions.GroupName, L("Permission:Menus"));
        var perm = group.AddPermission(MenusPermissions.MenuItems.Default, L("Permission:MenuItems"));
        perm.AddChild(MenusPermissions.MenuItems.Create, L("Permission:MenuItems.Create"));
        perm.AddChild(MenusPermissions.MenuItems.Update, L("Permission:MenuItems.Update"));
        perm.AddChild(MenusPermissions.MenuItems.Delete, L("Permission:MenuItems.Delete"));
    }

    private static LocalizableString L(string name) =>
        LocalizableString.Create<LinkyouSystemResource>(name);
}
