using BACApp.App.ViewModels;

namespace BACApp.App.Pages;

public partial class CalendarPage : ContentPage
{
    public CalendarPage(CalendarViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        Loaded += async (_, _) => await vm.RefreshAsync();
    }
}
