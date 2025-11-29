using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BACApp.Core.Services;

namespace BACApp.App.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _isBusy;
    private string _error = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
    }

    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); ((Command)LoginCommand).ChangeCanExecute(); }
    }

    public string Error
    {
        get => _error;
        set { _error = value; OnPropertyChanged(); }
    }

    public ICommand LoginCommand { get; }

    private async Task LoginAsync()
    {
        Error = string.Empty;
        IsBusy = true;
        try
        {
            var ok = await _authService.LoginAsync(Username, Password);
            if (ok)
            {
                await Shell.Current.GoToAsync("//MainTabs");
            }
            else
            {
                Error = "Login failed. Check credentials.";
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
