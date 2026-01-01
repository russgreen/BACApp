using Avalonia.Media;
using BACApp.Core.Models;
using BACApp.UI.Avalonia.Controls;
using BACApp.UI.Avalonia.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BACApp.UI.Avalonia.ViewModels;

internal partial class CalendarPageViewModel : PageViewModel
{
    [ObservableProperty]
    private ObservableCollection<Resource> _resources = new();

    [ObservableProperty]
    private ObservableCollection<ResourceEvent> _events = new();

    [ObservableProperty]
    private DateOnly _selectedDay = DateOnly.FromDateTime(DateTime.Now);

    public CalendarPageViewModel() : base(ApplicationPageNames.Calendar)
    {
        Resources.Add(new Resource { Id = 1, Name = "Aircraft A" });
        Resources.Add(new Resource { Id = 2, Name = "Aircraft B" });
        Resources.Add(new Resource { Id = 3, Name = "Aircraft C" });


        var day = new DateTimeOffset(DateTime.Today, TimeSpan.Zero);
        Events.Add(new ResourceEvent
        {
            ResourceId = 1,
            Start = day.AddHours(8),
            End = day.AddHours(10),
            Brush = Brushes.ForestGreen,
            Title = "Flight 101"
        });
        Events.Add(new ResourceEvent
        {
            ResourceId = 2,
            Start = day.AddHours(9),
            End = day.AddHours(12),
            Brush = Brushes.CornflowerBlue,
            Title = "Flight 102"
        });
        Events.Add(new ResourceEvent
        {
            ResourceId = 3,
            Start = day.AddHours(5),
            End = day.AddHours(22),
            Brush = Brushes.Gray,
            Title = "Maintenance"
        });
        Events.Add(new ResourceEvent
        {
            ResourceId = 1,
            Start = day.AddHours(12),
            End = day.AddHours(14),
            Brush = Brushes.CornflowerBlue,
            Title = "Flight 103"
        });

    }

    [RelayCommand]
    private void ResourceClick(Resource resource)
    {
        if (resource == null)
        {
            return;
        }

        // Handle resource click
        System.Diagnostics.Debug.WriteLine($"Resource clicked: {resource.Name}");
        // e.g., navigate to resource details, show dialog, etc.
    }

    [RelayCommand]
    private void EventClick(ResourceEvent resourceEvent)
    {
        if (resourceEvent == null)
        {
            return;
        }

        // Handle event click
        System.Diagnostics.Debug.WriteLine($"Event clicked: {resourceEvent.Title}");
        // e.g., show event details, edit event, etc.
    }


}
