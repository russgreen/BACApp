using BACApp.Core.Services;
using BACApp.UI.Avalonia.Enums;
using BACApp.UI.Avalonia.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BACApp.UI.Avalonia.ViewModels;

internal partial class LoginPageViewModel : PageViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    [EmailAddress]
    private string _username = string.Empty;

    [ObservableProperty]
    [Required]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _loginEnabled = true;

    [ObservableProperty]
    private string _message = string.Empty;

    public LoginPageViewModel(IAuthService authService) : base(ApplicationPageNames.Login)
    {
        _authService = authService;
    }


    [RelayCommand]
    private async Task Login()
    {
        Message = string.Empty;
        IsBusy = true;
        LoginEnabled = false;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            Message = "Please enter both username/email and password.";
            return;
        }

        Message = "Attempting login...";

        try
        {
            var response = await _authService.LoginAsync(Username, Password);
            if (response is null)
            {
                Message = "Login failed. Check credentials.";
                LoginEnabled = true;
            }
            else
            {
                Message = $"Logged in : {response.First_Name} {response.Last_Name}";

                WeakReferenceMessenger.Default.Send(new LoggedInMessage(true));
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            LoginEnabled = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}