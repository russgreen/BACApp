using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BACApp.Core.Models;

public class MaintenanceData
{
    [JsonPropertyName("airframe_hours_brought_forward")]
    public string? AirframeHoursBroughtForward { get; set; }

    [JsonPropertyName("next_check_hours_brought_forward")]
    public string? NextCheckHoursBroughtForward { get; set; }

    [JsonPropertyName("hours_flown_on_date")]
    public string? HoursFlownOnDate { get; set; }   

    [JsonPropertyName("total_airframe_hours")]
    public string? TotalAirframeHours { get; set; }

    [JsonPropertyName("total_next_check_hours")]
    public string? TotalNextCheckHours { get; set; }

    [JsonPropertyName("maintenance_inspection_completed_date")]
    public string? MaintenanceInspectionCompletedDate    { get; set; }

    [JsonPropertyName("maintenance_inspection_completed_hours")]
    public double? MaintenanceInspectionCompletedHours { get; set; }

    [JsonPropertyName("next_check_date")]
    public string? NextCheckDate { get; set; }

    [JsonPropertyName("next_inspection_type")]
    public string? NextInspectionType { get; set; }

}
