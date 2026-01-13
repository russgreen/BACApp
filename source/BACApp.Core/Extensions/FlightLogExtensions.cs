using BACApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.Core.Extensions;

public static class FlightLogExtensions
{
    public static void SetChargeTime(this FlightLog flightLog, int? useBlockTime, int? minutesAdjust)
    {
        var chargeTime = flightLog.FlightTime;

        if (Convert.ToBoolean(useBlockTime))
        {
            chargeTime = flightLog.BlockTime;
        }

        if(minutesAdjust != 0)
        {
            chargeTime = chargeTime.Add(TimeSpan.FromMinutes(Convert.ToDouble(minutesAdjust)));
        }

        flightLog.ChargeTime = chargeTime;
    }
}
