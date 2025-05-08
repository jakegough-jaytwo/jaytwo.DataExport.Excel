using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.StreamingExcelExport.OpenXml;
using jaytwo.StreamingExcelExport.Styles;
using jaytwo.StreamingExcelExport.Writers.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

internal class WorksheetWriter<T> : XmlDocumentWriter
{
    private const string XRNamespace = "http://schemas.microsoft.com/office/spreadsheetml/2014/revision";
    private const string RNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string MCNamespace = "http://schemas.openxmlformats.org/markup-compatibility/2006";

    private readonly IList<PropertyInfo> _props;
    private readonly IDictionary<string, (int ColumnNumber, ColumnLayout? ColumnLayout)> _propDictionary;

    public WorksheetWriter(WorksheetWriterContext<T> context, XmlWriter writer)
        : base(writer)
    {
        Context = context;

        var caseInsensitiveColumnLayouts = context.ColumnLayouts?
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        _props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        _propDictionary = Enumerable.Range(0, _props.Count).ToDictionary(
            i => _props[i].Name,
            i => (i + 1, GetColumnLayout(i)),
            StringComparer.OrdinalIgnoreCase);

        ColumnLayout? GetColumnLayout(int i)
        {
            var prop = _props[i];
            ColumnLayout? result = default;
            caseInsensitiveColumnLayouts?.TryGetValue(prop.Name, out result);
            return result;
        }
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

            if (Context.FreezeHeaderRow)
            {
                await using (CreateElementScope("sheetViews"))
                {
                    await using (CreateElementScopeWithAttributes("sheetView", new() { { "tabSelected", "1" }, { "workbookViewId", "0" } }))
                    {
                        await WriteElementWithAttributes("pane", new() { { "ySplit", "1" }, { "topLeftCell", "A2" }, { "activePane", "bottomLeft" }, { "state", "frozen" } });
                    }
                }
            }

            if (Context.ColumnLayouts != null && Context.ColumnLayouts.Any())
            {
                await using (CreateElementScope("cols"))
                {
                    foreach (var layout in Context.ColumnLayouts)
                    {
                        var columnLayout = layout.Value;
                        var columnNumber = _propDictionary[layout.Key].ColumnNumber;
                        var columnWidth = columnLayout.ColumnWidth;
                        await using (CreateElementScopeWithAttributes("col", new() { { "min", $"{columnNumber}" }, { "max", $"{columnNumber}" }, { "width", $"{columnWidth}" }, { "customWidth", "1" } }))
                        {
                        }
                    }
                }
            }

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

    private ColumnLayout? GetColumnLayout(int columnNumber)
        => GetColumnLayout(columnNumber, out _);

    private ColumnLayout? GetColumnLayout(int columnNumber, out string columnName)
    {
        var prop = _props[columnNumber - 1];
        columnName = prop.Name;

        ColumnLayout? result = default;
        Context.ColumnLayouts?.TryGetValue(columnName, out result);
        return result;
    }

    private async IAsyncEnumerable<object[]> GetCellData(
        IAsyncEnumerable<T> data,
        bool writeHeader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (writeHeader)
        {
            yield return _props.Select(x => x.Name).Cast<object>().ToArray();
        }

        await foreach (var item in data.WithCancellation(cancellationToken))
        {
            yield return _props.Select(x => x.GetValue(item)).Cast<object>().ToArray();
        }
    }

    private async Task WriteRowElementAsync(int rowNumber, object[] itemValues)
    {
        var bold = Context.IncludeHeader && rowNumber == 1;
        var zebraStripe = rowNumber % 2 == 0;

        await using (CreateElementScopeWithAttributes("row", new() { { "r", $"{rowNumber}" } }))
        {
            int columnNumber = 1;
            foreach (var value in itemValues)
            {
                var column = ToExcelColumnName(columnNumber);
                var cell = $"{column}{rowNumber}";

                var columnLayout = GetColumnLayout(columnNumber);
                var cellInfo = CellFormatInfo.FromValue(value, zebraStripe, bold, columnLayout?.HorizontalAlignment);

                await WriteCellElementAsync(cell, cellInfo);
                columnNumber++;
            }
        }
    }

    private async Task WriteCellElementAsync(string cell, CellFormatInfo cellInfo)
    {
        await using (CreateElementScopeWithAttributes("c", new() { { "r", cell } }))
        {
            if (!string.IsNullOrEmpty(cellInfo.Type))
            {
                WriteAttributeString("t", cellInfo.Type);
            }

            if (cellInfo.StyleIndex != null)
            {
                WriteAttributeString("s", $"{cellInfo.StyleIndex}");
            }

            var valueString = cellInfo.GetValueAsString();
            if (!string.IsNullOrEmpty(valueString))
            {
                await using (CreateElementScope("v"))
                {
                    Writer.WriteValue(valueString);
                }
            }
        }
    }
}
