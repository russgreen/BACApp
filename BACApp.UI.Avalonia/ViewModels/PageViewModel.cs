using BACApp.UI.Avalonia.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BACApp.UI.Avalonia.ViewModels;

internal partial class PageViewModel : BaseViewModel
{
    [ObservableProperty]
    private ApplicationPageNames _pageName;

    protected PageViewModel(ApplicationPageNames pageName)
    {
        _pageName = pageName;
    }
}
