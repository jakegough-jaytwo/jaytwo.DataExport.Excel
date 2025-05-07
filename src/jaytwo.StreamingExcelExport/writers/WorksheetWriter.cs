using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.StreamingExcelExport.Writers.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

internal class WorksheetWriter<T> : XmlDocumentWriter
{
    private const string XRNamespace = "http://schemas.microsoft.com/office/spreadsheetml/2014/revision";
    private const string RNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string MCNamespace = "http://schemas.openxmlformats.org/markup-compatibility/2006";

    private readonly IList<PropertyInfo> _props;

    public WorksheetWriter(WorksheetWriterContext<T> context, XmlWriter writer)
        : base(writer)
    {
        _props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Context = context;
    }

    public WorksheetWriterContext<T> Context { get; }

    public static string ToExcelColumnName(int columnNumber)
    {
        if (columnNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(columnNumber), "Must be >= 1");
        }

        var columnName = string.Empty;

        while (columnNumber > 0)
        {
            columnNumber--; // Make it 0-based
            columnName = (char)('A' + (columnNumber % 26)) + columnName;
            columnNumber /= 26;
        }

        return columnName;
    }

    public static double? ToExcelSerialDate(DateTime? date)
        => (date == null) ? null : ToExcelSerialDate(date.Value);

    public static double ToExcelSerialDate(DateTime date)
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

    protected override async Task WriteRootElementAsync(CancellationToken cancellationToken)
    {
        await using (CreateElementScope("worksheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"))
        {
            WriteAttributeString("xmlns", "r", null, RNamespace);
            WriteAttributeString("xmlns", "mc", null, MCNamespace);
            WriteAttributeString("mc", "Ignorable", MCNamespace, "x14ac xr xr2 xr3");
            WriteAttributeString("xmlns", "x14ac", null, "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            WriteAttributeString("xmlns", "xr", null, XRNamespace);
            WriteAttributeString("xmlns", "xr2", null, "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");
            WriteAttributeString("xmlns", "xr3", null, "http://schemas.microsoft.com/office/spreadsheetml/2016/revision3");

            WriteAttributeString("xr", "uid", XRNamespace, "{5805E005-BFE0-4B3C-AB23-D8BB48A5F78F}");

            await using (CreateElementScope("sheetData"))
            {
                int rowNumber = 1;
                await foreach (var itemValues in GetCellData(Context.Data, writeHeader: Context.IncludeHeader).WithCancellation(cancellationToken))
                {
                    await WriteRowElementAsync(rowNumber++, itemValues);
                }
            }
        }
    }

    private async IAsyncEnumerable<object[]> GetCellData(
        IAsyncEnumerable<T> data,
        bool writeHeader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (writeHeader)
        {
            yield return _props.Select(x => x.Name).ToArray();
        }

        await foreach (var item in data.WithCancellation(cancellationToken))
        {
            yield return _props.Select(x => x.GetValue(item)?.ToString() ?? string.Empty).ToArray();
        }
    }

    private async Task WriteRowElementAsync(int rowNumber, object[] itemValues)
    {
        await using (CreateElementScope("row"))
        {
            WriteAttributeString("r", $"{rowNumber}");

            int columnNumber = 1;
            foreach (var value in itemValues)
            {
                var column = ToExcelColumnName(columnNumber++);
                var cell = $"{column}{rowNumber}";
                await WriteCellElementAsync(cell, value);
            }
        }
    }

    private async Task WriteCellElementAsync(string cell, object value)
    {
        FormatObject(value, out var formattedValue, out var type);

        await using (CreateElementScope("c"))
        {
            WriteAttributeString("r", cell);
            WriteAttributeString("t", type);

            await using (CreateElementScope("v"))
            {
                Writer.WriteValue(formattedValue);
            }
        }
    }

    private void FormatObject(object value, out string result, out string type)
    {
        if (value is string asString)
        {
            type = Types.String;
            result = asString;
        }
        else if (value is int asInt)
        {
            type = Types.Numeric;
            result = asInt.ToString(CultureInfo.InvariantCulture);
        }
        else if (value is double asDouble)
        {
            type = Types.Numeric;
            result = asDouble.ToString(CultureInfo.InvariantCulture);
        }
        else if (value is DateTime asDateTime)
        {
            type = Types.Numeric;
            result = ToExcelSerialDate(asDateTime).ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            type = Types.String;
            result = $"{value}";
        }
    }

    public class Types
    {
        public const string String = "str";
        public const string Numeric = "n";
    }
}
