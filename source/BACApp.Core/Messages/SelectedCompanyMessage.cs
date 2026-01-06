using BACApp.Core.DTO;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BACApp.Core.Messages;

public class SelectedCompanyMessage : ValueChangedMessage<CompanyDto>
{
    public SelectedCompanyMessage(CompanyDto value) : base(value)
    {
    }
}
