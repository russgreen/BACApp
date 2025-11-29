using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BACApp.Core.Models;
using BACApp.Core.Services;

namespace BACApp.App.ViewModels;

public class FlightLogsViewModel : INotifyPropertyChanged
{
    private readonly IReportsService _reportsService;
    private string _aircraftId = string.Empty;
    private DateTime _from = DateTime.Today.AddDays(-7);
    private DateTime _to = DateTime.Today;
    private bool _isBusy;
    public ObservableCollection<FlightLog> Items { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public FlightLogsViewModel(IReportsService reportsService)
    {
        _reportsService = reportsService;
        RefreshCommand = new Command(async () => await RefreshAsync(), () => !IsBusy);
    }

    public string AircraftId
    {
        get => _aircraftId;
        set { _aircraftId = value; OnPropertyChanged(); }
    }

    public DateTime From
    {
        get => _from;
        set { _from = value; OnPropertyChanged(); }
    }

    public DateTime To
    {
        get => _to;
        set { _to = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); ((Command)RefreshCommand).ChangeCanExecute(); }
    }

    public ICommand RefreshCommand { get; }

    public async Task RefreshAsync()
    {
        if (string.IsNullOrWhiteSpace(AircraftId))
            return;
        IsBusy = true;
        try
        {
            Items.Clear();
            var data = await _reportsService.GetFlightLogsAsync(AircraftId, DateOnly.FromDateTime(From), DateOnly.FromDateTime(To));
            foreach (var x in data) Items.Add(x);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
