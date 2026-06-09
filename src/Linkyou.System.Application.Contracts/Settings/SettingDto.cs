namespace Linkyou.System.Settings;

public class SettingDto
{
    public string Name { get; set; } = null!;
    public string? Value { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; }
}
