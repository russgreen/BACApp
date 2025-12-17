using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace BACApp.UI.Avalonia.ViewModels;

internal class BaseViewModel : ObservableValidator
{
    public event EventHandler ClosingRequest;

    protected void OnClosingRequest()
    {
        if (this.ClosingRequest != null)
        {
            this.ClosingRequest(this, EventArgs.Empty);
        }
    }
}
