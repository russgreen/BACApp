using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public partial class BookingEvent : ObservableObject
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }

    [JsonPropertyName(name: "resourceId")]
    public int ResourceId { get; set; }

    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor { get; set; }

    [JsonPropertyName("borderColor")]
    public string? BorderColor { get; set; }

    [JsonPropertyName("textColor")]
    public string? TextColor { get; set; }

    [JsonPropertyName("rendering")]
    public string? Rendering { get; set; }

    [JsonIgnore]
    public DateTimeOffset StartTime => DateTimeOffset.Parse(Start);
    [JsonIgnore]
    public DateTimeOffset EndTime => DateTimeOffset.Parse(End);
    [JsonIgnore]
    public TimeSpan Duration => EndTime - StartTime;

    [JsonIgnore]
    public IBrush? BackgroundBrush => !string.IsNullOrEmpty(BackgroundColor) ? Brush.Parse(BackgroundColor) : null;
    [JsonIgnore]
    public IBrush? TextBrush => !string.IsNullOrEmpty(TextColor) ? Brush.Parse(TextColor) : null;
    [JsonIgnore]
    public IBrush? BorderBrush => !string.IsNullOrEmpty(BorderColor) ? Brush.Parse(BorderColor) : null;

    [ObservableProperty]
    [JsonIgnore]
    private string? _comment;
}
