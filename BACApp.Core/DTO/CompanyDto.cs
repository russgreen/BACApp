using System.Text.Json.Serialization;

namespace BACApp.Core.DTO;

public sealed class CompanyDto
{
    [JsonPropertyName("company_id")]
    public int CompanyId { get; set; }

    [JsonPropertyName("company_name")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("logo")]
    public string? Logo { get; set; }
}
