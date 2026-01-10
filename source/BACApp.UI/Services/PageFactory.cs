using BACApp.UI.ViewModels;
using System;

namespace BACApp.UI.Services;

internal class PageFactory(Func<Type, PageViewModel> factory)
{
    public PageViewModel GetPageViewModel<T>(Action<T>? afterCreation = null)
    where T : PageViewModel
    {
        var viewModel = factory(typeof(T));

        afterCreation?.Invoke((T)viewModel);

        return viewModel;
    }
}
