using BACApp.Core.DTO;
using BACApp.Core.Messages;
using BACApp.Core.Services;
using BACApp.UI.Avalonia.Enums;
using BACApp.UI.Avalonia.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BACApp.UI.Avalonia.ViewModels;

internal partial class LoginPageViewModel : PageViewModel
{
    private readonly IAuthService _authService;
    private readonly ILogger<LoginPageViewModel> _logger;

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
    private bool _selectCompanyVisible = false;

    [ObservableProperty]
    private ObservableCollection<CompanyDto> _companies;

    [ObservableProperty]
    private CompanyDto _selectedCompany;

    [ObservableProperty]
    private string _message = string.Empty;

    public LoginPageViewModel(IAuthService authService,
        ILogger<LoginPageViewModel> logger)  : base(ApplicationPageNames.Login)
    {
        _authService = authService;
        _logger = logger;

        AutoLogin();
    }

    private void AutoLogin()
    {
        //check for saved credentials in a autologin.json file
        var autoLoginFile = System.IO.Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "autologin.json");

        if (System.IO.File.Exists(autoLoginFile))
        {
            var autologinJson = File.ReadAllText(autoLoginFile, Encoding.UTF8);

            if (autologinJson is not null)
            {
                try
                {
                    var autoLogin = System.Text.Json.JsonSerializer.Deserialize<AutoLogin>(autologinJson)!;

                    _logger.LogDebug("Optional autologin settings loaded successfully.");

                    if(autoLogin.Username is not null && autoLogin.Password is not null)
                    {
                        Username =  autoLogin.Username;
                        Password =  autoLogin.Password;
                        Debug.WriteLine("Auto login with saved credentials.");
                        LoginCommand.Execute(null);
                    }
                }
                catch (Exception ex)
                {
                   _logger.LogDebug(ex, "Error deserializing settings file.");
                }
            }
        }


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

                if(response.Companies is not null && response.Companies.Length > 1)
                {
                    Companies = new ObservableCollection<CompanyDto>(response.Companies);
                    SelectCompanyVisible = true;
                    Message = "Please select a company.";
                    return;
                }

                if(response.Companies is not null && response.Companies.Length == 1)
                {
                    WeakReferenceMessenger.Default.Send(new SelectedCompanyMessage(response.Companies[0]));

                   //WeakReferenceMessenger.Default.Send(new LoggedInMessage(true));
                }
                
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

    [RelayCommand]
    private void SelectCompany()
    {
        WeakReferenceMessenger.Default.Send(new SelectedCompanyMessage(SelectedCompany));
    }
}