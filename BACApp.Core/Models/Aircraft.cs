using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public class Aircraft
{
    // integers
    [JsonPropertyName("aircraft_id")]
    public int? AircraftId { get; set; }

    [JsonPropertyName("company_id")]
    public int? CompanyId { get; set; }

    [JsonPropertyName("display_order")]
    public int? DisplayOrder { get; set; }

    [JsonPropertyName("aircraft_type_id")]
    public int? AircraftTypeId { get; set; }

    // strings
    [JsonPropertyName("registration")]
    public string Registration { get; set; } = string.Empty;

    [JsonPropertyName("aircraft_notes")]
    public string? AircraftNotes { get; set; }

    // dates provided as strings in API
    [JsonPropertyName("anual_permit_renewal_due")]
    public string? AnualPermitRenewalDue { get; set; }

    [JsonPropertyName("insurance_renewal_due")]
    public string? InsuranceRenewalDue { get; set; }

    [JsonPropertyName("arc_renewal_due")]
    public string? ArcRenewalDue { get; set; }

    [JsonPropertyName("radio_licence_renewal_due")]
    public string? RadioLicenceRenewalDue { get; set; }

    // numbers (floating)
    [JsonPropertyName("next_check_hours")]
    public double? NextCheckHours { get; set; }

    // date as string per API
    [JsonPropertyName("next_check_date")]
    public string? NextCheckDate { get; set; }

    // number for boolean flags (1/0)
    [JsonPropertyName("active")]
    public int? Active { get; set; }

    [JsonPropertyName("empty_weight")]
    public double? EmptyWeight { get; set; }

    [JsonPropertyName("empty_weight_level_arm")]
    public double? EmptyWeightLevelArm { get; set; }

    [JsonPropertyName("row1_seats_level_arm")]
    public double? Row1SeatsLevelArm { get; set; }

    [JsonPropertyName("row2_seats_level_arm")]
    public double? Row2SeatsLevelArm { get; set; }

    [JsonPropertyName("row3_seats_level_arm")]
    public double? Row3SeatsLevelArm { get; set; }

    [JsonPropertyName("row4_seats_level_arm")]
    public double? Row4SeatsLevelArm { get; set; }

    [JsonPropertyName("baggage_rear_level_arm")]
    public double? BaggageRearLevelArm { get; set; }

    [JsonPropertyName("baggage_forward_level_arm")]
    public double? BaggageForwardLevelArm { get; set; }

    [JsonPropertyName("fuel_level_arm")]
    public double? FuelLevelArm { get; set; }

    [JsonPropertyName("max_takeoff_weight")]
    public double? MaxTakeoffWeight { get; set; }

    [JsonPropertyName("fuel_consumption")]
    public double? FuelConsumption { get; set; }

    [JsonPropertyName("fuel_capacity")]
    public double? FuelCapacity { get; set; }

    [JsonPropertyName("use_brakes_time_to_invoice")]
    public int? UseBrakesTimeToInvoice { get; set; }

    // Retain schema spelling
    [JsonPropertyName("airframe_hours_engile_zero_hours1")]
    public double? AirframeHoursEngileZeroHours1 { get; set; }

    [JsonPropertyName("airframe_hours_engile_zero_hours2")]
    public double? AirframeHoursEngileZeroHours2 { get; set; }

    [JsonPropertyName("airframe_hours_engile_top_oh1")]
    public double? AirframeHoursEngileTopOh1 { get; set; }

    [JsonPropertyName("airframe_hours_engile_top_oh2")]
    public double? AirframeHoursEngileTopOh2 { get; set; }

    [JsonPropertyName("airframe_hours_prop_zero_hours1")]
    public double? AirframeHoursPropZeroHours1 { get; set; }

    [JsonPropertyName("airframe_hours_prop_zero_hours2")]
    public double? AirframeHoursPropZeroHours2 { get; set; }

    [JsonPropertyName("maintenance_inspection_completed_date")]
    public string? MaintenanceInspectionCompletedDate { get; set; }

    [JsonPropertyName("maintenance_inspection_completed_hours")]
    public double? MaintenanceInspectionCompletedHours { get; set; }

    [JsonPropertyName("next_inspection_type")]
    public string? NextInspectionType { get; set; }

    [JsonPropertyName("landing_charge")]
    public double? LandingCharge { get; set; }

    [JsonPropertyName("tgl_charge")]
    public double? TglCharge { get; set; }

    [JsonPropertyName("approach_charge")]
    public double? ApproachCharge { get; set; }

    [JsonPropertyName("hours_account_factor")]
    public double? HoursAccountFactor { get; set; }

    [JsonPropertyName("use_brakes_time_to_invoice_owner")]
    public int? UseBrakesTimeToInvoiceOwner { get; set; }

    [JsonPropertyName("exclude_maintenance_flights_owner")]
    public int? ExcludeMaintenanceFlightsOwner { get; set; }

    [JsonPropertyName("exclude_own_flights_owner")]
    public int? ExcludeOwnFlightsOwner { get; set; }

    [JsonPropertyName("out_of_phase_hours")]
    public double? OutOfPhaseHours { get; set; }

    [JsonPropertyName("out_of_phase_date")]
    public string? OutOfPhaseDate { get; set; }

    [JsonPropertyName("out_of_phase_task")]
    public string? OutOfPhaseTask { get; set; }

    [JsonPropertyName("time_adjust_minutes")]
    public int? TimeAdjustMinutes { get; set; }

    [JsonPropertyName("external")]
    public int? External { get; set; }

    [JsonPropertyName("maintenance_inspection_completed_details")]
    public string? MaintenanceInspectionCompletedDetails { get; set; }

    [JsonPropertyName("minimum_charge_time_minutes")]
    public int? MinimumChargeTimeMinutes { get; set; }

    [JsonPropertyName("envelope_flmw")]
    public double? EnvelopeFlmw { get; set; }

    [JsonPropertyName("envelope_mwfl")]
    public double? EnvelopeMwfl { get; set; }

    [JsonPropertyName("envelope_mwal")]
    public double? EnvelopeMwal { get; set; }

    [JsonPropertyName("envelope_mwalminw")]
    public double? EnvelopeMwalminw { get; set; }

    [JsonPropertyName("envelope_ewal")]
    public double? EnvelopeEwal { get; set; }

    [JsonPropertyName("display_on_calendar")]
    public int? DisplayOnCalendar { get; set; }
}
