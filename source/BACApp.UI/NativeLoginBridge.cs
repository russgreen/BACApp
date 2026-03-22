using BACApp.Core.DTO;
using BACApp.Core.Messages;
using BACApp.Core.Services;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace BACApp.UI;

public static class NativeLoginBridge
{
    public static Task<LoginResponse> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        return Host.GetService<IAuthService>()
            .LoginAsync(username, password, cancellationToken);
    }

    public static void CompleteLogin(CompanyDto company)
    {
        WeakReferenceMessenger.Default.Send(new SelectedCompanyMessage(company));
    }
}