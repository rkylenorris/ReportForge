using System;
using System.Globalization;

namespace ReportForge.Config.Environment;

public class ConvertEnvString
{
    public static object ToCorrectType(string input)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input string cannot be null or empty.", nameof(input));
        }

        // 1. Try to parse as Boolean
        if (bool.TryParse(input, out bool boolValue))
        {
            return boolValue;
        }

        // 2. Try to parse as Integer (int)
        // Use InvariantCulture to handle numeric formats consistently
        if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
        {
            return intValue;
        }

        // 3. Try to parse as Double
        if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
        {
            return doubleValue;
        }

        // 4. Try to parse as DateTime
        // TryParse handles standard formats and current culture by default
        if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
        {
            return dateValue;
        }

        // If all else fails, return the original string
        return input;
    }
}






