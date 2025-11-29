using BACApp.App.ViewModels;

namespace BACApp.App.Pages;

public partial class FlightLogsPage : ContentPage
{
    public FlightLogsPage(FlightLogsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
