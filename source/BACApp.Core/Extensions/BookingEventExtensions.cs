using BACApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.Core.Extensions;

public static class BookingEventExtensions
{

    public static TimeSpan? GetFlightTime(this BookingEvent bookingEvent)
    {
        // Approximate flight time from booking duration
        var estimatedFlightTime = bookingEvent.Duration * 0.42;
        return estimatedFlightTime;
    }

    /// <summary>
    /// Provides a simple approximation of hours remaining until the next check
    /// Estimated flight time is not store by Cloudbase so approximated as 0.42 of
    /// booking duration becuase a 1hr lesson (typically 50mins airborne) takes a 
    /// 2hr booking.
    /// </summary>
    /// <param name="bookingEvent"></param>
    /// <returns></returns>
    public static TimeSpan? HoursToNextCheck(this BookingEvent bookingEvent)
    {
        // Approximate flight time from booking duration
        var estimatedFlightTime = bookingEvent.GetFlightTime();



        return estimatedFlightTime;
    }
}
