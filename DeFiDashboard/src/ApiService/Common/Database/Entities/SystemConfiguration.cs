namespace ApiService.Common.Database.Entities;

public class SystemConfiguration
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; }
}
