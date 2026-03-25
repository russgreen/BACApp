using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BACApp.Core.Messages;

public sealed class NativeLoginVisibilityMessage : ValueChangedMessage<bool>
{
    public NativeLoginVisibilityMessage(bool value) : base(value)
    {
    }
}