using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using BACApp.Core.Attributes;

namespace BACApp.Core.Services;

public class CsvExportService : ICsvExportService
{
    // Uses reflection to produce a flat CSV from public readable properties.
    // Handles nulls, escapes quotes, and includes a header row.
    public void Export<T>(IEnumerable<T> items, string fullPath)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (string.IsNullOrWhiteSpace(fullPath))
        {
            throw new ArgumentException("Full path is required.", nameof(fullPath));
        }

        var materialized = items as IList<T> ?? items.ToList();

        // If no items, still create an empty file with headers based on T's public props
        var type = typeof(T);
        var props = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead)
            .Where(p => !Attribute.IsDefined(p, typeof(CsvIgnoreAttribute)))
            .ToArray();

        var dir = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrWhiteSpace(dir))
        {
            throw new ArgumentException("Full path must include a directory.", nameof(fullPath));
        }

        Directory.CreateDirectory(dir);

        // Use UTF-8 with BOM to improve compatibility with Excel
        using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        // Header
        writer.WriteLine(string.Join(",", props.Select(p => Escape(p.Name))));

        // Rows
        var fmt = CultureInfo.InvariantCulture;
        for (int i = 0; i < materialized.Count; i++)
        {
            var item = materialized[i];
            var values = new string[props.Length];
            for (int j = 0; j < props.Length; j++)
            {
                var val = props[j].GetValue(item);
                values[j] = Escape(FormatValue(val, fmt));
            }

            writer.WriteLine(string.Join(",", values));
        }
    }

    private string FormatValue(object? value, IFormatProvider fmt)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return value switch
        {
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", fmt),
            DateOnly d => d.ToString("yyyy-MM-dd", fmt),
            TimeOnly t => t.ToString("HH:mm", fmt),
            IFormattable f => f.ToString(null, fmt),
            _ => value.ToString() ?? string.Empty
        };
    }

    // RFC4180-style minimal escaping:
    // - Wrap in quotes 
    // - Escape quotes by doubling them
    // - Preserve commas/newlines
    private string Escape(string input)
    {
        if (input is null)
        {
            return string.Empty;
        }

        var needsQuotes = input.Contains(',') || input.Contains('\n') || input.Contains('\r') || input.Contains('"');
        if (!needsQuotes)
        {
            return input;
        }

        var escaped = input.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}
