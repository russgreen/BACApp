using System.Text.Json.Serialization;

namespace BACApp.Core.DTO;

public class UserDto
{
    [JsonPropertyName("company_name")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("company_id")]
    public int CompanyId { get; set; }

    [JsonPropertyName("calendar_type")]
    public string? CalendarType { get; set; }

    [JsonPropertyName("company_type")]
	public string? CompanyType { get; set; }

    [JsonPropertyName("role")]
	public string? Role { get; set; }

    [JsonPropertyName("aircraft")]
	public IReadOnlyList<string>? Aircraft { get; set; }
}
