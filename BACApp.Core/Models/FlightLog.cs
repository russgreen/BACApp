using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public class FlightLog
{
    [JsonPropertyName("flight_log_id")]
    public int? FlightLogId { get; set; }

    [JsonPropertyName("flight_record")]
    public int? FlightRecord { get; set; }

    [JsonPropertyName("aircraft")]
    public string? Aircraft { get; set; }

    [JsonPropertyName("flight_id")]
    public int? FlightId { get; set; }

    // Dates/times are provided as strings by the API
    [JsonPropertyName("flight_date")]
    public string? FlightDate { get; set; }

    [JsonPropertyName("brakes_off_time")]
    public string? BrakesOffTime { get; set; }

    [JsonPropertyName("take_off_time")]
    public string? TakeOffTime { get; set; }

    [JsonPropertyName("landing_time")]
    public string? LandingTime { get; set; }

    [JsonPropertyName("brakes_on_time")]
    public string? BrakesOnTime { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("autolog_id")]
    public int? AutologId { get; set; }

    [JsonPropertyName("power_on")]
    public string? PowerOn { get; set; }

    [JsonPropertyName("tacho_start")]
    public double? TachoStart { get; set; }

    [JsonPropertyName("tacho_stop")]
    public double? TachoStop { get; set; }
}
