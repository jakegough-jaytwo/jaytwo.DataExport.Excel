using System;
using System.Collections.Generic;
using System.Globalization;

namespace jaytwo.DataExport.Excel.Styles;

public static class ExcelFormatHelper
{
    public static string GetExcelCurrencyFormat(CultureInfo culture, int decimalPlaces = 2)
    {
        var symbol = culture.NumberFormat.CurrencySymbol;
        var lcidHex = culture.LCID.ToString("X4");
        var groupSep = culture.NumberFormat.CurrencyGroupSeparator;
        var decimalSep = culture.NumberFormat.CurrencyDecimalSeparator;

        // Excel format codes require literal characters to be enclosed in quotes if ambiguous
        // We'll quote the separators if needed
        var safeGroupSep = EscapeExcelFormatLiteral(groupSep);
        var safeDecimalSep = EscapeExcelFormatLiteral(decimalSep);

        // Build decimal part (e.g. .00 or ,00)
        string decimalPart = decimalPlaces > 0
            ? safeDecimalSep + new string('0', decimalPlaces)
            : string.Empty;

        // Combine
        return $"[$'{symbol}'-{lcidHex}]#{safeGroupSep}##0{decimalPart}";
    }

    public static string GetExcelNumberFormat(CultureInfo culture, int decimalPlaces = 2)
    {
        var groupSizes = culture.NumberFormat.NumberGroupSizes;
        var decimalSeparator = EscapeExcelFormatLiteral(culture.NumberFormat.NumberDecimalSeparator);
        var groupSeparator = EscapeExcelFormatLiteral(culture.NumberFormat.NumberGroupSeparator);

        // Build the integer part based on group sizes (e.g., "##,##,###" for India)
        var integerPart = BuildExcelGroupingFormat(groupSizes, groupSeparator);

        // Append decimal places
        var decimalPart = decimalPlaces > 0
            ? decimalSeparator + new string('0', decimalPlaces)
            : string.Empty;

        return integerPart + decimalPart;
    }

    public static string GetExcelFormatCode(int numFmtId, CultureInfo culture)
    {
        var numberFormat = culture.NumberFormat;
        string groupSep = EscapeExcelFormatLiteral(numberFormat.NumberGroupSeparator);
        string decimalSep = EscapeExcelFormatLiteral(numberFormat.NumberDecimalSeparator);
        string currencyGroupSep = EscapeExcelFormatLiteral(numberFormat.CurrencyGroupSeparator);
        string currencyDecimalSep = EscapeExcelFormatLiteral(numberFormat.CurrencyDecimalSeparator);
        string currencySymbol = numberFormat.CurrencySymbol;

        switch (numFmtId)
        {
            case 0: return "General";
            case 1: return "0";
            case 2: return $"0{decimalSep}00";
            case 3: return $"#{groupSep}##0";
            case 4: return $"#{groupSep}##0{decimalSep}00";
            case 5: return $"\"{currencySymbol}\"#{currencyGroupSep}##0_);(\"{currencySymbol}\"#{currencyGroupSep}##0)";
            case 6: return $"\"{currencySymbol}\"#{currencyGroupSep}##0_);[Red](\"{currencySymbol}\"#{currencyGroupSep}##0)";
            case 7: return $"\"{currencySymbol}\"#{currencyGroupSep}##0{currencyDecimalSep}00_);(\"{currencySymbol}\"#{currencyGroupSep}##0{currencyDecimalSep}00)";
            case 8: return $"\"{currencySymbol}\"#{currencyGroupSep}##0{currencyDecimalSep}00_);[Red](\"{currencySymbol}\"#{currencyGroupSep}##0{currencyDecimalSep}00)";
            //case 9: return "0%"; // not localized
            case 10: return $"0{decimalSep}00%";
            //case 11: return "0.00E+00"; // not localized
            case 12: return "# ?/?";
            case 13: return "# ??/??";
            case 14: return "m/d/yyyy"; // short date, can customize
            case 15: return "d-mmm-yy";
            case 16: return "d-mmm";
            case 17: return "mmm-yy";
            case 18: return "h:mm tt";
            case 19: return "h:mm:ss tt";
            case 20: return "H:mm"; // 24-hour
            case 21: return "H:mm:ss"; // 24-hour with seconds
            case 22: return "m/d/yyyy H:mm";
            case 37: return $"#{currencyGroupSep}##0_);(#{currencyGroupSep}##0)";
            case 38: return $"#{currencyGroupSep}##0_);[Red](#{currencyGroupSep}##0)";
            case 39: return $"#{currencyGroupSep}##0{decimalSep}00_);(#{currencyGroupSep}##0{decimalSep}00)";
            case 40: return $"#{currencyGroupSep}##0{decimalSep}00_);[Red](#{currencyGroupSep}##0{decimalSep}00)";
            case 41: return $"\"{currencySymbol}\"#{currencyGroupSep}##0_);(\"{currencySymbol}\"#{currencyGroupSep}##0)";
            case 42: return $"\"{currencySymbol}\"#{currencyGroupSep}##0_);[Red](\"{currencySymbol}\"#{currencyGroupSep}##0)";
            case 43: return $"\"{currencySymbol}\"#{currencyGroupSep}##0{currencyDecimalSep}00_);(\"{currencySymbol}\"#{currencyGroupSep}##0{currencyDecimalSep}00)";
            case 44: return $"\"{currencySymbol}\"#{currencyGroupSep}##0{currencyDecimalSep}00_);[Red](\"{currencySymbol}\"#{currencyGroupSep}##0{currencyDecimalSep}00)";
            case 45: return "mm:ss";
            case 46: return "[h]:mm:ss";
            case 47: return "mmss.0";
            case 48: return "##0.0E+0";
            case 49: return "@"; // Text format
            default: throw new NotSupportedException($"numFmtId {numFmtId} is not supported.");
        }
    }

    public static string GetExcelShortDateFormat(CultureInfo culture) // equivalent to excel's built-in ID 14
        => GetExcelDateFormat(culture.DateTimeFormat.ShortDatePattern);

    public static string GetExcelLongDateFormat(CultureInfo culture)
        => GetExcelDateFormat(culture.DateTimeFormat.LongDatePattern);

    public static string GetExcelFullDateTimeLongTimeFormat(CultureInfo culture)
        => GetExcelDateFormat(culture.DateTimeFormat.FullDateTimePattern);

    public static string GetExcelFullDateTimeShortTimeFormat(CultureInfo culture)
        => GetExcelDateFormat(culture.DateTimeFormat.LongDatePattern + " " + culture.DateTimeFormat.ShortTimePattern);

    public static string GetExcelLongTimeFormat(CultureInfo culture)
        => GetExcelDateFormat(culture.DateTimeFormat.LongTimePattern);

    public static string GetExcelMonthDayFormat(CultureInfo culture)
        => GetExcelDateFormat(culture.DateTimeFormat.MonthDayPattern);

    public static string GetExcelShortTimeFormat(CultureInfo culture)
        => GetExcelDateFormat(culture.DateTimeFormat.ShortTimePattern);

    public static string GetExcelYearMonthFormat(CultureInfo culture)
        => GetExcelDateFormat(culture.DateTimeFormat.YearMonthPattern);

    private static string GetExcelDateFormat(string pattern)
    {
        return pattern
            .Replace("MMMM", "mmmm")
            .Replace("MMM", "mmm")
            .Replace("MM", "mm")
            .Replace("M", "m")
            .Replace("HH", "hh")
            .Replace("H", "h")
            .Replace("tt", "AM/PM");
    }

    private static string BuildExcelGroupingFormat(int[] groupSizes, string groupSeparator)
    {
        // Excel only supports repeating one group size after the initial grouping
        // So: for India, we fake "##,##,###" as "#,##,##0", then Excel will repeat the final group (usually 3)
        var groups = new List<string>();
        int remaining = groupSizes.Length;

        for (int i = 0; i < groupSizes.Length; i++)
        {
            int size = groupSizes[i];
            if (size <= 0)
            {
                break; // Excel ignores or breaks on 0-length groups
            }

            if (i == 0)
            {
                groups.Add(new string('#', size));
            }
            else
            {
                groups.Add(groupSeparator + new string('#', size));
            }
        }

        groups[^1] = groupSeparator + new string('0', groupSizes[^1]); // ensure last group ends in 0

        return string.Join(string.Empty, groups);
    }

    private static string EscapeExcelFormatLiteral(string s)
    {
        return s switch
        {
            " " => "\" \"",
            "\u00A0" => "\"\u00A0\"",
            "\\" or "_" or "*" or "@" or "#" or "0" or "," or "." => $"\"{s}\"",
            _ => s,
        };
    }
}
