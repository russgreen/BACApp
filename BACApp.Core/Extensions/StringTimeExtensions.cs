using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BACApp.Core.Extensions;

public static class StringTimeExtensions
{
    /// <summary>
    /// Normalizes time strings like "hh:mm:ss" (or "hh:mm") to "hh:mm".
    /// Hours may exceed 24. Returns <see cref="string.Empty"/> if input is null/whitespace.
    /// Throws <see cref="FormatException"/> if the format is not h:mm, hh:mm, hh:mm:ss, etc.
    /// </summary>
    public static string ToHoursMinutes(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var s = value.Trim();
        var parts = s.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length is < 2 or > 3)
        {
            throw new FormatException($"Invalid time format '{value}'. Expected 'hh:mm' or 'hh:mm:ss'.");
        }

        if (!int.TryParse(parts[0], out var hours) || hours < 0)
        {
            throw new FormatException($"Invalid hours component in '{value}'.");
        }

        if (!int.TryParse(parts[1], out var minutes) || minutes is < 0 or > 59)
        {
            throw new FormatException($"Invalid minutes component in '{value}'.");
        }

        if (parts.Length == 3 && (!int.TryParse(parts[2], out var seconds) || seconds is < 0 or > 59))
        {
            throw new FormatException($"Invalid seconds component in '{value}'.");
        }

        return $"{hours:00}:{minutes:00}";
    }

}