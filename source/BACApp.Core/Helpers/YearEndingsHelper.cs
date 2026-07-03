using System;
using System.Collections.Generic;
using System.Text;

namespace BACApp.Core.Helpers;

public static class YearEndingsHelper
{
    public static List<string> GetYearEndings()
    {
        return Enumerable.Range(DateTime.Now.Year - 1, 3)
            .Select(y => $"{y}")
            .ToList();
    }

    public static string CurrentYearEnding(List<string> yearEndings)
    {
        if(DateTime.Now.Month > 5)
        {
            return yearEndings[2];
        }

        return yearEndings[1];
    }
}
