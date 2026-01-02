using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public class Resource
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonIgnore]
    public string? Comment { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("background_color")]
    public string? BackgroundColor { get; set; }

    [JsonIgnore]
    public IBrush? BackgroundBrush => BackgroundColor != null ? Brush.Parse(BackgroundColor) : null;

    [JsonIgnore]
    public IBrush? ForegroundBrush => Color != null ? Brush.Parse(Color) : null;

}
