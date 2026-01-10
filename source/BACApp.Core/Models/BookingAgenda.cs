using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace BACApp.Core.Models;

public class BookingAgenda
{
    [JsonPropertyName("booking_agenda_id")]
    public int? BookingAgendaId { get; set; }

    [JsonPropertyName("company_id")]
    public int? CompanyId { get; set; }

    [JsonPropertyName("instructor_id")]
    public int? InstructorId { get; set; }

    [JsonPropertyName("aircraft_id")]
    public int? AircraftId { get; set; }

    [JsonPropertyName("user_id")]
    public int? UserId { get; set; }

    [JsonPropertyName("booking_type_id")]
    public int? BookingTypeId { get; set; }

    [JsonPropertyName("booking_status_id")]
    public int? BookingStatusId { get; set; }

    // Dates provided as strings in API
    [JsonPropertyName("starttime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("endtime")]
    public string? EndTime { get; set; }

    [JsonPropertyName("insstarttime")]
    public string? InsStartTime { get; set; }

    [JsonPropertyName("insendtime")]
    public string? InsEndTime { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("member_name")]
    public string? MemberName { get; set; }

    // values are "on" or "off"
    [JsonPropertyName("daymember")]
    public string? DayMember { get; set; }

    [JsonPropertyName("day_member_name")]
    public string? DayMemberName { get; set; }

    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    // values are "on" or "no"
    [JsonPropertyName("unlink")]
    public string? Unlink { get; set; }

    // values are "on" or "no"
    [JsonPropertyName("day_member_email")]
    public string? DayMemberEmail { get; set; }
}
