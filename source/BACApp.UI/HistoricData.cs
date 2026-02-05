using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.UI;

internal static class HistoricData
{
    public static IReadOnlyDictionary<(string Registration, DateTime MonthStart), double> BuildHistoricalMonthlyChargeHours()
    {
        // NOTE: These values are monthly totals (not cumulative).
        // If a value is blank in the provided table, it is intentionally omitted (= 0).
        var d = new Dictionary<(string, DateTime), double>();

        static DateTime M(int year, int month) => new(year, month, 1);

        // Feb-25 .. Jan-26
        d[("G-BASJ", M(2025, 2))] = 17.7;
        d[("G-BASJ", M(2025, 3))] = 32.7;
        d[("G-BASJ", M(2025, 4))] = 11.0;
        d[("G-BASJ", M(2025, 5))] = 29.9;
        d[("G-BASJ", M(2025, 6))] = 38.0;
        d[("G-BASJ", M(2025, 7))] = 26.6;
        d[("G-BASJ", M(2025, 8))] = 38.3;
        d[("G-BASJ", M(2025, 9))] = 34.8;
        d[("G-BASJ", M(2025, 10))] = 45.1;
        d[("G-BASJ", M(2025, 11))] = 21.0;
        d[("G-BASJ", M(2025, 12))] = 4.6;
        // Jan-26 blank for G-BASJ in provided table

        d[("G-BBXW", M(2025, 2))] = 29.5;
        d[("G-BBXW", M(2025, 3))] = 30.1;
        d[("G-BBXW", M(2025, 4))] = 28.6;
        d[("G-BBXW", M(2025, 5))] = 33.7;
        d[("G-BBXW", M(2025, 6))] = 42.8;
        d[("G-BBXW", M(2025, 7))] = 32.6;
        d[("G-BBXW", M(2025, 8))] = 48.5;
        d[("G-BBXW", M(2025, 9))] = 47.8;
        d[("G-BBXW", M(2025, 10))] = 29.0;
        d[("G-BBXW", M(2025, 11))] = 31.5;
        d[("G-BBXW", M(2025, 12))] = 0;
        // Jan-26 blank for G-BBXW in provided table

        d[("G-ARKS", M(2025, 2))] = 0.0;
        d[("G-ARKS", M(2025, 3))] = 0.0;
        d[("G-ARKS", M(2025, 4))] = 0.0;
        d[("G-ARKS", M(2025, 5))] = 0.0;
        d[("G-ARKS", M(2025, 6))] = 0.0;
        d[("G-ARKS", M(2025, 7))] = 0.0;
        d[("G-ARKS", M(2025, 8))] = 0.0;
        d[("G-ARKS", M(2025, 9))] = 0.0;
        d[("G-ARKS", M(2025, 10))] = 0.0;
        d[("G-ARKS", M(2025, 11))] = 0.0;
        d[("G-ARKS", M(2025, 12))] = 2.5;
        // Jan-26 blank for G-ARKS in provided table

        // Feb-24 .. Jan-25
        d[("G-BASJ", M(2024, 2))] = 20.6;
        d[("G-BASJ", M(2024, 3))] = 6.6;
        d[("G-BASJ", M(2024, 4))] = 19.3;
        d[("G-BASJ", M(2024, 5))] = 33.8;
        d[("G-BASJ", M(2024, 6))] = 29.8;
        d[("G-BASJ", M(2024, 7))] = 22.8;
        d[("G-BASJ", M(2024, 8))] = 21.5;
        d[("G-BASJ", M(2024, 9))] = 24.0;
        d[("G-BASJ", M(2024, 10))] = 17.0;
        d[("G-BASJ", M(2024, 11))] = 7.1;
        d[("G-BASJ", M(2024, 12))] = 6.4;
        d[("G-BASJ", M(2025, 1))] = 10.4;

        d[("G-BBXW", M(2024, 2))] = 19.6;
        d[("G-BBXW", M(2024, 3))] = 27.5;
        d[("G-BBXW", M(2024, 4))] = 22.1;
        d[("G-BBXW", M(2024, 5))] = 76.3;
        d[("G-BBXW", M(2024, 6))] = 33.6;
        d[("G-BBXW", M(2024, 7))] = 29.5;
        d[("G-BBXW", M(2024, 8))] = 23.7;
        d[("G-BBXW", M(2024, 9))] = 6.6;
        d[("G-BBXW", M(2024, 10))] = 19.0;
        d[("G-BBXW", M(2024, 11))] = 19.3;
        d[("G-BBXW", M(2024, 12))] = 9.6;
        d[("G-BBXW", M(2025, 1))] = 16.0;

        d[("G-ARKS", M(2024, 2))] = 0.0;
        d[("G-ARKS", M(2024, 3))] = 0.0;
        d[("G-ARKS", M(2024, 4))] = 0.0;
        d[("G-ARKS", M(2024, 5))] = 0.0;
        d[("G-ARKS", M(2024, 6))] = 0.0;
        d[("G-ARKS", M(2024, 7))] = 0.0;
        d[("G-ARKS", M(2024, 8))] = 0.0;
        d[("G-ARKS", M(2024, 9))] = 0.0;
        d[("G-ARKS", M(2024, 10))] = 0.0;
        d[("G-ARKS", M(2024, 11))] = 0.0;
        d[("G-ARKS", M(2024, 12))] = 0.0;
        d[("G-ARKS", M(2025, 1))] = 0.0;

        return d;
    }
}
