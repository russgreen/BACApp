using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using BACApp.Core.Models;
using BACApp.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BACApp.UI.Views;

public partial class LogsAirframePageView : UserControl
{
    public LogsAirframePageView()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) =>
        {
            if (DataContext is LogsAirframePageViewModel vm)
            {
                vm.PickExportFilePathAsync = PickExportFilePathAsync;
            }
        };
    }

    private async Task<string?> PickExportFilePathAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return null;
        }

        var aircraftReg = (DataContext as LogsAirframePageViewModel)?.SelectedAircraft?.Registration ?? "unknown";
        var fromDate = (DataContext as LogsAirframePageViewModel)?.FromDate ?? DateTime.Now.AddMonths(-1);
        var toDate = (DataContext as LogsAirframePageViewModel)?.ToDate ?? DateTime.Now;

        var suggestedName = $"AirframeTechLog_{aircraftReg}_{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.csv";

        var options = new FilePickerSaveOptions
        {
            Title = "Export CSV",
            SuggestedFileName = suggestedName,
            DefaultExtension = "csv",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new("CSV")
                {
                    Patterns = new[] { "*.csv" },
                    MimeTypes = new[] { "text/csv" }
                }
            }
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        if (file is null)
        {
            return null;
        }

        // Prefer LocalPath when available; fall back to path from URI.
        if (!string.IsNullOrWhiteSpace(file.Path.LocalPath))
        {
            return file.Path.LocalPath;
        }

        return file.Path.IsAbsoluteUri ? file.Path.LocalPath : file.Path.ToString();
    }
}