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

    [JsonPropertyName("colour")]
    public string? Color { get; set; }

    [JsonPropertyName("background_colour")]
    public string? BackgroundColor { get; set; }

    [JsonIgnore]
    public IBrush? BackgroundBrush => !string.IsNullOrEmpty(BackgroundColor) ? Brush.Parse(BackgroundColor) : Brushes.LightGray;

    [JsonIgnore]
    public IBrush? ForegroundBrush => !string.IsNullOrEmpty(Color) ? Brush.Parse(Color) : Brushes.Black;

}
