using System.Globalization;
using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public class TechLog
{
    [JsonPropertyName("dtl_id")]
    public int? DtlId { get; set; }

    [JsonPropertyName("aircraft_id")]
    public int? AircraftId { get; set; }

    [JsonPropertyName("flight_log_id")]
    public int? FlightLogId { get; set; }

    // Dates/times are strings per API examples
    [JsonPropertyName("tech_log_date")]
    public string? TechLogDate { get; set; }

    [JsonPropertyName("flight_date")]
    public string? Flight_Date { get; set; }

    [JsonPropertyName("brakes_off_time")]
    public string? Brakes_Off_Time { get; set; }

    [JsonPropertyName("take_off_time")]
    public string? Take_Off_Time { get; set; }

    [JsonPropertyName("landing_time")]
    public string? Landing_Time { get; set; }

    [JsonPropertyName("brakes_on_time")]
    public string? Brakes_On_Time { get; set; }

    [JsonPropertyName("pic_id")]
    public int? PicId { get; set; }

    [JsonPropertyName("student_id")]
    public int? StudentId { get; set; }

    [JsonPropertyName("location_from")]
    public string? LocationFrom { get; set; }

    [JsonPropertyName("location_to")]
    public string? LocationTo { get; set; }

    [JsonPropertyName("authorised_by_id")]
    public int? AuthorisedById { get; set; }

    [JsonPropertyName("initials_authorisation")]
    public string? InitialsAuthorisation { get; set; }

    [JsonPropertyName("tod_authorisation")]
    public string? TodAuthorisation { get; set; }

    [JsonPropertyName("duration_authorisation")]
    public string? DurationAuthorisation { get; set; }

    [JsonPropertyName("initials_pic_pre_flight")]
    public string? InitialsPicPreFlight { get; set; }

    [JsonPropertyName("flight_duration")]
    public string? Flight_Duration { get; set; }

    [JsonPropertyName("flight_type")]
    public string? FlightType { get; set; }

    [JsonPropertyName("initials_pic_post_flight")]
    public string? InitialsPicPostFlight { get; set; }

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }

    [JsonPropertyName("fuel_last_flight")]
    public string? FuelLastFlight { get; set; }

    [JsonPropertyName("fuel_uplift_port")]
    public string? FuelUpliftPort { get; set; }

    [JsonPropertyName("fuel_uplift_starboard")]
    public string? FuelUpliftStarboard { get; set; }

    [JsonPropertyName("fuel_on_dep_port")]
    public string? FuelOnDepPort { get; set; }

    [JsonPropertyName("fuel_on_dep_starboard")]
    public string? FuelOnDepStarboard { get; set; }

    [JsonPropertyName("fuel_uplift_total")]
    public string? FuelUpliftTotal { get; set; }

    [JsonPropertyName("fuel_on_dep_total")]
    public string? FuelOnDepTotal { get; set; }

    [JsonPropertyName("oil_uplift_total")]
    public int? OilUpliftTotal { get; set; }

    [JsonPropertyName("oil_on_dep_total")]
    public string? OilOnDepTotal { get; set; }

    [JsonPropertyName("fuel_end_of_flight")]
    public string? FuelEndOfFlight { get; set; }

    [JsonPropertyName("initials_pic_fuel")]
    public string? InitialsPicFuel { get; set; }

    [JsonPropertyName("pic_signature")]
    public string? PicSignature { get; set; }

    [JsonPropertyName("oil_uplift_port")]
    public string? OilUpliftPort { get; set; }

    [JsonPropertyName("oil_uplift_starboard")]
    public string? OilUpliftStarboard { get; set; }

    [JsonPropertyName("oil_on_dep_port")]
    public string? OilOnDepPort { get; set; }

    [JsonPropertyName("oil_on_dep_starboard")]
    public string? OilOnDepStarboard { get; set; }

    [JsonPropertyName("passenger_name")]
    public string? PassengerName { get; set; }

    // 1/0 per example
    [JsonPropertyName("is_maintenance")]
    public int? IsMaintenance { get; set; }

    [JsonPropertyName("oil_end_of_flight")]
    public string? OilEndOfFlight { get; set; }

    [JsonPropertyName("fuel_uplift_post_flight")]
    public string? FuelUpliftPostFlight { get; set; }

    [JsonPropertyName("oil_uplift_post_flight")]
    public string? OilUpliftPostFlight { get; set; }

    [JsonPropertyName("notes_pre_flight")]
    public string? NotesPreFlight { get; set; }

    [JsonPropertyName("landings")]
    public double? Landings { get; set; }

    [JsonPropertyName("registration")]
    public string? Registration { get; set; }

    [JsonPropertyName("pic_name")]
    public string? PicName { get; set; }

    [JsonPropertyName("student_name")]
    public string? StudentName { get; set; }

    [JsonPropertyName("authoriser_name")]
    public string? AuthoriserName { get; set; }

    [JsonPropertyName("approaches")]
    public double? Approaches { get; set; }

    [JsonPropertyName("company_id")]
    public int? CompanyId { get; set; }

    public DateTime FlightDate => TryParseDateTime(Flight_Date, out var value) ? value : DateTime.MinValue;

    public DateTime BrakesOffTime => TryParseDateTime(Brakes_Off_Time, out var brakesOff) ? brakesOff : DateTime.MinValue;
    public DateTime TakeOffTime => TryParseDateTime(Take_Off_Time, out var takeOff) ? takeOff : DateTime.MinValue;
    public DateTime LandingTime => TryParseDateTime(Landing_Time, out var landing) ? landing : DateTime.MinValue;
    public DateTime BrakesOnTime => TryParseDateTime(Brakes_On_Time, out var brakesOn) ? brakesOn : DateTime.MinValue;

    public TimeSpan BlockTime => TryGetDuration(Brakes_Off_Time, Brakes_On_Time, out var duration)
        ? duration
        : TimeSpan.Zero;

    public TimeSpan FlightTime => TryGetDuration(Take_Off_Time, Landing_Time, out var duration)
        ? duration
        : TimeSpan.Zero;

    public TimeSpan BlockTimeRounded => RoundToNearestMinute(BlockTime);

    public TimeSpan FlightTimeRounded => RoundToNearestMinute(FlightTime);

    public double BlockTimeDecimal => Math.Round(BlockTimeRounded.TotalHours, 2);

    public double FlightTimeDecimal => Math.Round(FlightTimeRounded.TotalHours, 2);

    private static bool TryGetDuration(string? startValue, string? endValue, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;

        if (!TryParseDateTime(startValue, out var start) || !TryParseDateTime(endValue, out var end))
        {
            return false;
        }

        if (end < start)
        {
            return false;
        }

        duration = end - start;
        return true;
    }

    private static bool TryParseDateTime(string? value, out DateTime result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result);
    }

    private static TimeSpan RoundToNearestMinute(TimeSpan value)
    {
        // Add 30 seconds and truncate to minute boundary
        var adjusted = value + TimeSpan.FromSeconds(30);
        return TimeSpan.FromMinutes(Math.Floor(adjusted.TotalMinutes));
    }
}
