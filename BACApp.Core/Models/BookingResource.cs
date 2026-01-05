using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace BACApp.Core.Models;

public partial class BookingResource : ObservableObject
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("colour")]
    public string? Color { get; set; }

    [JsonPropertyName("background_colour")]
    public string? BackgroundColor { get; set; }

    [JsonIgnore]
    public IBrush? BackgroundBrush => !string.IsNullOrEmpty(BackgroundColor) ? Brush.Parse(BackgroundColor) : Brushes.LightGray;

    [JsonIgnore]
    public IBrush? ForegroundBrush => !string.IsNullOrEmpty(Color) ? Brush.Parse(Color) : Brushes.Black;

    [ObservableProperty]
    [JsonIgnore]
    private string? _comment;

}
