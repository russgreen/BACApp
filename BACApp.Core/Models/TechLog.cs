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
    public string? FlightDuration { get; set; }

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
}
