using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport;

public class WorksheetWriter<T> : XmlDocumentWriter
{
    private const string XRNamespace = "http://schemas.microsoft.com/office/spreadsheetml/2014/revision";
    private const string RNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string MCNamespace = "http://schemas.openxmlformats.org/markup-compatibility/2006";

    private readonly IList<PropertyInfo> _props;

    public WorksheetWriter(XmlWriter writer, IAsyncEnumerable<T> data)
        : base(writer)
    {
        _props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Data = data;
    }

    public static string Path { get; } = "xl/worksheets/sheet1.xml";

    public IAsyncEnumerable<T> Data { get; }

    internal static string ToExcelColumnName(int columnNumber)
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

    protected override async Task WriteRootElementAsync()
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
                await foreach (var itemValues in GetCellData(Data, writeHeader: true))
                {
                    await WriteRowElementAsync(rowNumber++, itemValues);
                    //await FlushAsync();
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
        await using (CreateElementScope("c"))
        {
            WriteAttributeString("r", cell);
            WriteAttributeString("t", "str");

            await WriteValueElementAsync(value);
        }
    }

    private async Task WriteValueElementAsync(object value)
    {
        await using (CreateElementScope("v"))
        {
            Writer.WriteValue($"{value}");
        }
    }
}
