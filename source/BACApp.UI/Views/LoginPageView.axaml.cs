using Avalonia.Controls;

namespace BACApp.UI.Views;

public partial class LoginPageView : UserControl
{
    public LoginPageView()
    {
        InitializeComponent();

        if (System.OperatingSystem.IsIOS())
        {
            SharedLoginControls.IsVisible = false;
        }
    }
}