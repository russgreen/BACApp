using Avalonia.Media;
using System;
using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public class Event
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

    public DateTimeOffset StartTime => DateTimeOffset.Parse(Start);
    public DateTimeOffset EndTime => DateTimeOffset.Parse(End);

    public IBrush? BackgroundBrush => BackgroundColor != null ? Brush.Parse(BackgroundColor) : null;
    public IBrush? TextBrush => TextColor != null ? Brush.Parse(TextColor) : null;
    public IBrush? BorderBrush => BorderColor != null ? Brush.Parse(BorderColor) : null;

}
