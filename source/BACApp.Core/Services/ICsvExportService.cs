namespace BACApp.Core.Services;

public interface ICsvExportService
{
    /// <summary>
    /// Exports the provided data items to a CSV file at the specified full path.
    /// </summary>
    /// <typeparam name="T">Type of items to export.</typeparam>
    /// <param name="items">The list of items to export.</param>
    /// <param name="fullPath">The full file path, including filename and extension (e.g. C:\Temp\logs.csv).</param>
    void Export<T>(System.Collections.Generic.IEnumerable<T> items, string fullPath);
}
