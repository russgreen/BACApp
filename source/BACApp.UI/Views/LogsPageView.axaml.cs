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
using System.Transactions;

namespace BACApp.UI.Views;

public partial class LogsPageView : UserControl
{
    public LogsPageView()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) =>
        {
            if (DataContext is LogsPageViewModel vm)
            {
                vm.PickExportFilePathAsync = PickExportFilePathAsync;
            }
        };
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var dataGrid = sender as DataGrid;
        var viewModel = this.DataContext as LogsPageViewModel;
        if (viewModel != null && dataGrid != null)
        {
            viewModel.SelectedFlightLogs = new System.Collections.ObjectModel.ObservableCollection<FlightLog>(dataGrid.SelectedItems.Cast<FlightLog>());
        }
    }

    private async Task<string?> PickExportFilePathAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return null;
        }

        var aircraftReg = (DataContext as LogsPageViewModel)?.SelectedAircraft?.Registration ?? "unknown";
        var fromDate = (DataContext as LogsPageViewModel)?.FromDate ?? DateTime.Now.AddMonths(-1);
        var toDate = (DataContext as LogsPageViewModel)?.ToDate ?? DateTime.Now;

        var suggestedName = $"FlightLogs_{aircraftReg}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

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