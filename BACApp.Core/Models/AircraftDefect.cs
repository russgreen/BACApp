using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public class AircraftDefect
{
    [JsonPropertyName("aircraft_defect_id")]
    public int? AircraftDefectId { get; set; }

    [JsonPropertyName("aircraft_id")]
    public int? AircraftId { get; set; }

    [JsonPropertyName("user_id")]
    public int? UserId { get; set; }

    [JsonPropertyName("defect_details")]
    public string? DefectDetails { get; set; }

    [JsonPropertyName("comments")]
    public string? Comments { get; set; }

    // 1 (New), 2 (AIRCRAFT GROUNDED), 3 (Not Actioned), 4 (Repair Done), 5 (Deferred until next check), 6 (In progress), 7 (Deferred, awaiting parts)
    [JsonPropertyName("aircraft_defects_status_id")]
    public int? AircraftDefectsStatusId { get; set; }

    // Link serious aircraft defect with a techlog
    [JsonPropertyName("dtl_id")]
    public int? DtlId { get; set; }

    // Values: Minor, Beaware, Serious
    [JsonPropertyName("defect_type")]
    public string? DefectType { get; set; }

    // API uses boolean but examples show 1/0; keep number to match wire format
    [JsonPropertyName("display_tech_log")]
    public int? DisplayTechLog { get; set; }

    [JsonPropertyName("display_aircraft")]
    public int? DisplayAircraft { get; set; }

    // Name of the defect's author
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("registration")]
    public string? Registration { get; set; }
}