using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace jaytwo.StreamingExcelExport.OpenXml;

internal class CellFormatInfo
{
    public const int DateStyleIndex = 1; // TODO: somehow this needs to exist in styles.xml

    public CellFormatInfo(string? type, string? value, int? styleIndex = null)
    {
        Type = type;
        Value = value;
        StyleIndex = styleIndex;
    }

    public static CellFormatInfo Empty => new CellFormatInfo(null, null, null); // Blank cell

    public string? Type { get; }

    public string? Value { get; }

    public int? StyleIndex { get; }

    public static CellFormatInfo FromValue(object value)
    {
        if (value == null)
        {
            return Empty;
        }

        return value switch
        {
            string s => new CellFormatInfo(Types.String, s),
            int i => new CellFormatInfo(Types.Numeric, i.ToString(CultureInfo.InvariantCulture)),
            long l => new CellFormatInfo(Types.Numeric, l.ToString(CultureInfo.InvariantCulture)),
            double d => new CellFormatInfo(Types.Numeric, d.ToString(CultureInfo.InvariantCulture)),
            float f => new CellFormatInfo(Types.Numeric, f.ToString(CultureInfo.InvariantCulture)),
            decimal m => new CellFormatInfo(Types.Numeric, m.ToString(CultureInfo.InvariantCulture)),
            bool b => new CellFormatInfo(Types.Boolean, b ? "1" : "0"),
            DateTime dt => new CellFormatInfo(Types.Numeric, GetExcelSerialDate(dt).ToString(CultureInfo.InvariantCulture), DateStyleIndex),
            _ => new CellFormatInfo(Types.String, value.ToString()),
        };
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

    private static class Types
    {
        public const string String = "str";
        public const string Numeric = "n";
        public const string Boolean = "b";
        public const string ISO8601Date = "d"; // "rarely used"
        public const string SharedString = "s";
    }
}
