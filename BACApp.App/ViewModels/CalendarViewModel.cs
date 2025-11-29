using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BACApp.Core.Services;

namespace BACApp.App.ViewModels;

public class CalendarViewModel : INotifyPropertyChanged
{
    private readonly ICalendarService _calendarService;
    private DateTime _date = DateTime.Today;
    private int _resourceCount;
    private int _eventCount;
    private bool _isBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    public CalendarViewModel(ICalendarService calendarService)
    {
        _calendarService = calendarService;
        RefreshCommand = new Command(async () => await RefreshAsync(), () => !IsBusy);
    }

    public DateTime Date
    {
        get => _date;
        set { _date = value; OnPropertyChanged(); }
    }

    public int ResourceCount
    {
        get => _resourceCount;
        set { _resourceCount = value; OnPropertyChanged(); }
    }

    public int EventCount
    {
        get => _eventCount;
        set { _eventCount = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); ((Command)RefreshCommand).ChangeCanExecute(); }
    }

    public ICommand RefreshCommand { get; }

    public async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            var d = DateOnly.FromDateTime(Date);
            var resources = await _calendarService.GetResourcesAsync(d);
            var events = await _calendarService.GetEventsAsync(d);
            ResourceCount = resources.Count;
            EventCount = events.Count;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
