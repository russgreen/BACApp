using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BACApp.Core.Messages;

public class AutoLoginMessage : ValueChangedMessage<bool>
{
    public AutoLoginMessage(bool value) : base(value)
    {
    }
}
