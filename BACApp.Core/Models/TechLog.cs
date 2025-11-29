namespace BACApp.Core.Models;

public class TechLog
{
    public string Id { get; set; } = string.Empty;
    public string AircraftId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Defect { get; set; } = string.Empty;
    public string Rectification { get; set; } = string.Empty;
    public string Engineer { get; set; } = string.Empty;
}
