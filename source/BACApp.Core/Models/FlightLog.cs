using System.Numerics;
using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public class FlightLog
{
    [JsonPropertyName("flight_log_id")]
    public int FlightLogId { get; set; }

    [JsonPropertyName("flight_record")]
    public long? FlightRecord { get; set; } 

    [JsonPropertyName("aircraft")]
    public string? Aircraft { get; set; }

    [JsonPropertyName("flight_id")]
    public int? FlightId { get; set; }

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


    // Dates/times are provided as strings by the API
    [JsonPropertyName("flight_date")]
    public string? Flight_Date { private get; set; }

    [JsonPropertyName("brakes_off_time")]
    public string? Brakes_Off_Time { private get; set; }

    [JsonPropertyName("take_off_time")]
    public string? Take_Off_Time { private get; set; }

    [JsonPropertyName("landing_time")]
    public string? Landing_Time { private get; set; }

    [JsonPropertyName("brakes_on_time")]
    public string? Brakes_On_Time { private get; set; }

    // Convenience properties to parse date/time strings
    public DateTime FlightDate => DateTime.Parse(Flight_Date ?? string.Empty);

    public DateTime BrakesOffTime => DateTime.Parse(Brakes_Off_Time ?? string.Empty);
    public DateTime TakeOffTime => DateTime.Parse(Take_Off_Time ?? string.Empty);
    public DateTime LandingTime => DateTime.Parse(Landing_Time ?? string.Empty);
    public DateTime BrakesOnTime => DateTime.Parse(Brakes_On_Time ?? string.Empty);


    public TimeSpan BlockTime => BrakesOnTime - BrakesOffTime;

    public TimeSpan FlightTime => LandingTime - TakeOffTime;

    public TimeSpan ChargeTime => FlightTime.Add(TimeSpan.FromMinutes(10));

    // Rounded to nearest minute for display (e.g., 1:24:30 -> 1:25)
    public TimeSpan BlockTimeRounded => RoundToNearestMinute(BlockTime);

    public TimeSpan FlightTimeRounded => RoundToNearestMinute(FlightTime);

    public TimeSpan ChargeTimeRounded => RoundToNearestMinute(ChargeTime);

    public double BlockTimeDecimal => Math.Round(BlockTimeRounded.TotalHours, 2);

    public double FlightTimeDecimal => Math.Round(FlightTimeRounded.TotalHours, 2);

    public double ChargeTimeDecimal => Math.Round(ChargeTimeRounded.TotalHours, 2);


    private static TimeSpan RoundToNearestMinute(TimeSpan value)
    {
        // Add 30 seconds and truncate to minute boundary
        var adjusted = value + TimeSpan.FromSeconds(30);
        return TimeSpan.FromMinutes(Math.Floor(adjusted.TotalMinutes));
    }
}
