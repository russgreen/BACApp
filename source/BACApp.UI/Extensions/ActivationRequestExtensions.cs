using System;
using System.Collections.Generic;
using System.Text;
using static BACApp.UI.SingleInstanceCoordinator;

namespace BACApp.UI.Extensions;

internal static class ActivationRequestExtensions
{
    public static string ToWireValue(this ActivationRequest request) =>
    request switch
    {
        ActivationRequest.Activate => "activate",
        ActivationRequest.Maximize => "maximize",
        _ => "activate",
    };

    public static bool TryParse(string? value, out ActivationRequest request)
    {
        if (string.Equals(value, "maximize", StringComparison.OrdinalIgnoreCase))
        {
            request = ActivationRequest.Maximize;
            return true;
        }

        if (string.Equals(value, "activate", StringComparison.OrdinalIgnoreCase))
        {
            request = ActivationRequest.Activate;
            return true;
        }

        request = ActivationRequest.Activate;
        return false;
    }
}
