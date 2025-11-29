using BACApp.App.ViewModels;

namespace BACApp.App.Pages;

public partial class TechLogsPage : ContentPage
{
    public TechLogsPage(TechLogsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
