using BACApp.UI.Enums;
using BACApp.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BACApp.UI.ViewModels;

internal partial class PageViewModel : BaseViewModel
{
    [ObservableProperty]
    private ApplicationPageNames _pageName;

    protected PageViewModel(ApplicationPageNames pageName)
    {
        _pageName = pageName;
    }
}
