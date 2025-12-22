using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BACApp.UI.Avalonia.Messages;

internal class LoggedInMessage : ValueChangedMessage<bool>
{
    public LoggedInMessage(bool value) : base(value)
    {
    }
}
