using System;
using System.Globalization;
using jaytwo.DataExport.Excel.Styles;
using jaytwo.DataExport.Excel.Writers;

namespace jaytwo.DataExport.Excel.OpenXml;

internal class CellFormatInfo
{
    public CellFormatInfo(string? type, object? value, int? styleIndex = null)
    {
        Type = type;
        Value = value;
        StyleIndex = styleIndex;
    }

    public string? Type { get; }

    public object? Value { get; }

    public int? StyleIndex { get; }

    public static CellFormatInfo FromValue(
        object value,
        bool zebraStripe,
        bool bold,
        NumberFormatStyles? numberFormat = null,
        HorizontalAlignmentStyles? alignment = null)
    {
        var outValue = PrepareValue(value, out var dataType, out var detectedNumberFormat);
        numberFormat ??= detectedNumberFormat;
        var fill = zebraStripe ? FillStyles.Stripe : FillStyles.Default;
        var font = bold ? FontStyles.Bold : FontStyles.Default;
        var styleId = StylesWriter.GetStyleIndex(font, fill, numberFormat, alignment);

        return new CellFormatInfo(dataType, outValue, styleId);
    }

    public static object PrepareValue(object value, out string? dataType, out NumberFormatStyles? numberFormat)
    {
        dataType = Types.Numeric;
        numberFormat = null;

        switch (value)
        {
            case short:
            case ushort:
            case int:
            case uint:
            case long:
            case ulong:
            case float:
            case double:
            case decimal:
                return value;

            case string:
                dataType = Types.String;
                return value;

            case TimeSpan ts:
                numberFormat = NumberFormatStyles.TimeElapsed;
                return ts.TotalSeconds;

            case DateTime dt:
                numberFormat = GetNumberFormat(dt);
                return GetExcelSerialDate(dt);

            case DateTimeOffset dto:
                numberFormat = NumberFormatStyles.DateTime;
                return GetExcelSerialDate(dto.UtcDateTime);

#if NET5_0_OR_GREATER
            case DateOnly d:
                numberFormat = NumberFormatStyles.DateShort;
                return GetExcelSerialDate(d);

            case TimeOnly t:
                numberFormat = NumberFormatStyles.Time24Hour;
                return GetExcelSerialDate(t);
#endif

            default:
                dataType = Types.String;
                return value;
        }
    }

#if NET5_0_OR_GREATER
    public static double GetExcelSerialDate(DateOnly date)
        => GetExcelSerialDate(date.ToDateTime(TimeOnly.MinValue));

    public static double GetExcelSerialDate(TimeOnly time)
        => GetExcelSerialDate(new DateOnly(1900, 1, 1).ToDateTime(time, DateTimeKind.Unspecified));
#endif

    public static NumberFormatStyles GetNumberFormat(DateTime date)
    {
        if (date.TimeOfDay == TimeSpan.Zero)
        {
            return NumberFormatStyles.DateShort;
        }
        else
        {
            return NumberFormatStyles.DateTime;
        }
    }

    public static int GetStyleIndex(bool isDate = false, bool isDateTime = false, bool zebraStripe = false, bool bold = false)
    {
        if (zebraStripe)
        {
            if (isDate)
            {
                return CellStyles.GrayBackgroundDateOnly;
            }
            else if (isDateTime)
            {
                return CellStyles.GrayBackgroundDateTime;
            }
            else
            {
                return CellStyles.GrayBackground;
            }
        }
        else if (bold)
        {
            return CellStyles.Bold;
        }
        else if (isDate)
        {
            return CellStyles.DateOnly;
        }
        else if (isDateTime)
        {
            return CellStyles.DateTime;
        }

        return CellStyles.Default;
    }

    public static double GetExcelSerialDate(DateTime date)
    {
        var baseDate = new DateTime(1899, 12, 31); // Excel's day 1 = Jan 1, 1900

        var serial = (date - baseDate).TotalDays;

        // Excel incorrectly includes Feb 29, 1900, which didn't exist
        // So for any date >= Mar 1, 1900, add 1 to compensate
        if (date >= new DateTime(1900, 3, 1))
        {
            serial += 1;
        }

        return serial;
    }

    public string? GetValueAsString() => Convert.ToString(Value, CultureInfo.InvariantCulture);

    private static class Types
    {
        public const string String = "str";
        public const string? Numeric = null; // it's technically "n" but the spec is "assume numeric if omitted"
        public const string Boolean = "b";
        //public const string ISO8601Date = "d"; // "rarely used"
        //public const string SharedString = "s";
    }
}
