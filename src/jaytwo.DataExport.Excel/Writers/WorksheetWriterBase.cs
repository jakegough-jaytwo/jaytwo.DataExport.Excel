using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.OpenXml;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal abstract class WorksheetWriterBase : XmlDocumentWriter
{
    private const string XRNamespace = "http://schemas.microsoft.com/office/spreadsheetml/2014/revision";
    private const string RNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string MCNamespace = "http://schemas.openxmlformats.org/markup-compatibility/2006";

    private readonly WorksheetOptions _options;
    private readonly string _worksheetUid;
    private readonly IList<string> _fieldNames;
    private readonly IDictionary<string, (int ColumnNumber, ColumnDefinition? ColumnLayout)> _fieldDictionary;

    protected WorksheetWriterBase(
        WorksheetOptions options,
        string worksheetUid,
        XmlWriter writer,
        IDictionary<string, (int ColumnNumber, ColumnDefinition? ColumnLayout)> fieldDictionary)
        : base(writer)
    {
        _options = options;
        _worksheetUid = worksheetUid;
        _fieldDictionary = fieldDictionary;
        _fieldNames = fieldDictionary.Keys.ToArray();
    }

    protected IList<string> FieldNames => _fieldNames;

    public static string ToExcelColumnName(int columnNumber)
    {
        if (columnNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(columnNumber), "Must be >= 1");
        }

        var columnName = string.Empty;

        while (columnNumber > 0)
        {
            columnNumber--;
            columnName = (char)('A' + (columnNumber % 26)) + columnName;
            columnNumber /= 26;
        }

        return columnName;
    }

    protected abstract IAsyncEnumerable<object[]> GetCellData(bool writeHeader, CancellationToken cancellationToken);

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

            WriteAttributeString("xr", "uid", XRNamespace, _worksheetUid);

            if (_options.FreezeHeaderRow)
            {
                await using (CreateElementScope("sheetViews"))
                {
                    await using (CreateElementScopeWithAttributes("sheetView", new() { { "workbookViewId", "0" } }))
                    {
                        await WriteElementWithAttributes("pane", new() { { "ySplit", "1" }, { "topLeftCell", "A2" }, { "activePane", "bottomLeft" }, { "state", "frozen" } });
                    }
                }
            }

            if (_options.ColumnDefinitions != null && _options.ColumnDefinitions.Any())
            {
                await using (CreateElementScope("cols"))
                {
                    foreach (var layout in _options.ColumnDefinitions)
                    {
                        var columnNumber = _fieldDictionary[layout.Key].ColumnNumber;
                        var columnWidth = layout.Value.Width;
                        await using (CreateElementScopeWithAttributes("col", new() { { "min", $"{columnNumber}" }, { "max", $"{columnNumber}" }, { "width", $"{columnWidth}" }, { "customWidth", "1" } }))
                        {
                        }
                    }
                }
            }

            await using (CreateElementScope("sheetData"))
            {
                int rowNumber = 1;
                await foreach (var itemValues in GetCellData(_options.IncludeHeader, cancellationToken))
                {
                    await WriteRowElementAsync(rowNumber, itemValues);
                    rowNumber++;
                }
            }
        }
    }

    private ColumnDefinition? GetColumnLayout(int columnNumber)
    {
        var fieldName = _fieldNames[columnNumber - 1];
        return _fieldDictionary.TryGetValue(fieldName, out var entry) ? entry.ColumnLayout : null;
    }

    private async Task WriteRowElementAsync(int rowNumber, object[] itemValues)
    {
        var bold = _options.IncludeHeader && _options.BoldHeaderRow && rowNumber == 1;
        var zebraStripe = _options.ApplyZebraStripe && rowNumber % 2 == 0;

        await using (CreateElementScopeWithAttributes("row", new() { { "r", $"{rowNumber}" } }))
        {
            int columnNumber = 1;
            foreach (var value in itemValues)
            {
                var column = ToExcelColumnName(columnNumber);
                var cell = $"{column}{rowNumber}";

                var columnLayout = GetColumnLayout(columnNumber);
                var cellInfo = CellFormatInfo.FromValue(
                    value,
                    zebraStripe,
                    bold,
                    columnLayout?.NumberFormat,
                    columnLayout?.HorizontalAlignment);

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
