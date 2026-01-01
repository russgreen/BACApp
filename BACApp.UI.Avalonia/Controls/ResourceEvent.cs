using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.UI.Avalonia.Controls;

public class ResourceEvent
{
    public int ResourceId { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public IBrush? Brush { get; set; }
    public string? Title { get; set; }
}
