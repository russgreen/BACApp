using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BACApp.Core.Messages;

public class LoggedInMessage : ValueChangedMessage<bool>
{
    public LoggedInMessage(bool value) : base(value)
    {
    }
}
