using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BACApp.Core.Models;
using BACApp.UI.ViewModels;
using System.Linq;
using System.Transactions;

namespace BACApp.UI.Views;

public partial class LogsPageView : UserControl
{
    public LogsPageView()
    {
        InitializeComponent();
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
}