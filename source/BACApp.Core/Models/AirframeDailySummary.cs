using BACApp.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.Core.Models;

public class AirframeDailySummary
{
    public required DateOnly Date { get; init; }

    public int FlightCount { get; init; }

    [CsvIgnore]
    public required TimeSpan TotalFlightTime { get; init; }

    [CsvIgnore]
    public TimeSpan? EndOfDayAirframeTotal { get; init; }

    public int TotalFlightHours => (int)TotalFlightTime.TotalHours;

    public int TotalFlightMinutes => TotalFlightTime.Minutes;

    public int? EndOfDayAirframeHours => EndOfDayAirframeTotal is null
        ? null
        : (int)EndOfDayAirframeTotal.Value.TotalHours;

    public int? EndOfDayAirframeMinutes => EndOfDayAirframeTotal?.Minutes;
}
