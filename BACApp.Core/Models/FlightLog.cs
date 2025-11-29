namespace BACApp.Core.Models;

public class FlightLog
{
    public string Id { get; set; } = string.Empty;
    public string AircraftId { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string PilotInCommand { get; set; } = string.Empty;
    public double FlightTimeHours { get; set; }
}
