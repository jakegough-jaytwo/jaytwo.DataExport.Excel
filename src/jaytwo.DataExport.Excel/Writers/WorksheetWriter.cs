using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Abstractions;
using jaytwo.DataExport.Excel.OpenXml;
using jaytwo.DataExport.Excel.Styles;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class WorksheetWriter : XmlDocumentWriter
{
    private const string XRNamespace = "http://schemas.microsoft.com/office/spreadsheetml/2014/revision";
    private const string RNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string MCNamespace = "http://schemas.openxmlformats.org/markup-compatibility/2006";

    private readonly string _sheetTag;
    private readonly WorksheetOptions _options;
    private readonly string _worksheetUid;
    private readonly IList<string> _fieldNames;
    private readonly IDictionary<string, (int ColumnNumber, ColumnDefinition? ColumnLayout)> _fieldDictionary;
    private readonly StyleRegistry _styleRegistry;
    private readonly ITabularDataReader _reader;

    public WorksheetWriter(string sheetTag, string worksheetUid, WorksheetOptions options, ITabularDataReader reader, StyleRegistry styleRegistry)
    {
        _sheetTag = sheetTag;
        _options = options;
        _worksheetUid = worksheetUid;
        _reader = reader;
        _fieldDictionary = BuildFieldDictionary(reader.Schema, options);
        _fieldNames = _fieldDictionary.Keys.ToArray();
        _styleRegistry = styleRegistry;
    }

    public override string ZipPackagePath => $"xl/worksheets/{_sheetTag}.xml";

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

    protected override async Task WriteRootElementAsync(XmlWriter writer, CancellationToken cancellationToken)
    {
        await using (CreateElementScope(writer, "worksheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"))
        {
            WriteAttributeString(writer, "xmlns", "r", null, RNamespace);
            WriteAttributeString(writer, "xmlns", "mc", null, MCNamespace);
            WriteAttributeString(writer, "mc", "Ignorable", MCNamespace, "x14ac xr xr2 xr3");
            WriteAttributeString(writer, "xmlns", "x14ac", null, "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            WriteAttributeString(writer, "xmlns", "xr", null, XRNamespace);
            WriteAttributeString(writer, "xmlns", "xr2", null, "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");
            WriteAttributeString(writer, "xmlns", "xr3", null, "http://schemas.microsoft.com/office/spreadsheetml/2016/revision3");

            WriteAttributeString(writer, "xr", "uid", XRNamespace, _worksheetUid);

            if (_options.FreezeHeaderRow)
            {
                await using (CreateElementScope(writer, "sheetViews"))
                {
                    await using (CreateElementScopeWithAttributes(writer, "sheetView", new() { { "workbookViewId", "0" } }))
                    {
                        await WriteElementWithAttributes(writer, "pane", new() { { "ySplit", "1" }, { "topLeftCell", "A2" }, { "activePane", "bottomLeft" }, { "state", "frozen" } });
                    }
                }
            }

            if (_options.ColumnDefinitions.Any())
            {
                await using (CreateElementScope(writer, "cols"))
                {
                    foreach (var layout in _options.ColumnDefinitions)
                    {
                        var columnNumber = _fieldDictionary[layout.Key].ColumnNumber;
                        var columnWidth = layout.Value.Width;
                        await using (CreateElementScopeWithAttributes(writer, "col", new() { { "min", $"{columnNumber}" }, { "max", $"{columnNumber}" }, { "width", $"{columnWidth}" }, { "customWidth", "1" } }))
                        {
                        }
                    }
                }
            }

            await using (CreateElementScope(writer, "sheetData"))
            {
                int rowNumber = 1;
                await foreach (var itemValues in GetCellData(_options.IncludeHeader, cancellationToken))
                {
                    await WriteRowElementAsync(writer, rowNumber, itemValues);
                    rowNumber++;
                }
            }
        }
    }

    private static IDictionary<string, (int ColumnNumber, ColumnDefinition? ColumnLayout)> BuildFieldDictionary(ITabularSchema schema, WorksheetOptions options)
    {
        var columns = schema.GetColumns();
        return Enumerable.Range(0, columns.Length).ToDictionary(
            i => columns[i].Name,
            i =>
            {
                ColumnDefinition? layout = null;
                options.ColumnDefinitions?.TryGetValue(columns[i].Name, out layout);
                return (i + 1, layout);
            },
            StringComparer.OrdinalIgnoreCase);
    }

    private async IAsyncEnumerable<object?[]> GetCellData(bool writeHeader, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (writeHeader)
        {
            yield return _fieldNames.ToArray<object?>();
        }

        while (await _reader.ReadAsync(cancellationToken))
        {
            var values = new object?[_fieldNames.Count];
            for (var ordinal = 0; ordinal < values.Length; ordinal++)
            {
                var value = _reader.GetValue(ordinal);
                values[ordinal] = value is DBNull ? null : value;
            }

            yield return values;

            await Task.Yield(); // intentional: yields the scheduler so callers aren't starved during large exports
        }
    }

    private ColumnDefinition? GetColumnLayout(int columnNumber)
    {
        // intentional: _fieldDictionary is keyed by insertion order (ordinal sequence) matching _fieldNames,
        // so key order is guaranteed by construction in BuildFieldDictionary
        var fieldName = _fieldNames[columnNumber - 1];
        return _fieldDictionary.TryGetValue(fieldName, out var entry) ? entry.ColumnLayout : null;
    }

    private async Task WriteRowElementAsync(XmlWriter writer, int rowNumber, object?[] itemValues)
    {
        var bold = _options.IncludeHeader && _options.BoldHeaderRow && rowNumber == 1;
        var zebraStripe = _options.ApplyZebraStripe && rowNumber % 2 == 0;

        await using (CreateElementScopeWithAttributes(writer, "row", new() { { "r", $"{rowNumber}" } }))
        {
            int columnNumber = 1;
            foreach (var value in itemValues)
            {
                var column = ToExcelColumnName(columnNumber);
                var cell = $"{column}{rowNumber}";

                var columnLayout = GetColumnLayout(columnNumber);
                CellFormatInfo cellInfo;
                if (columnLayout?.LocaleFormat != null)
                {
                    var font = bold ? FontStyles.Bold : FontStyles.Default;
                    var fill = zebraStripe ? FillStyles.Stripe : FillStyles.Default;
                    var culture = columnLayout.Culture ?? CultureInfo.InvariantCulture;
                    var styleId = _styleRegistry.GetOrRegisterStyleIndex(
                        columnLayout.LocaleFormat.Value,
                        culture,
                        font,
                        fill,
                        columnLayout.HorizontalAlignment ?? HorizontalAlignmentStyles.Default);
                    var outValue = CellFormatInfo.PrepareValue(value!, out var dataType, out _);
                    cellInfo = new CellFormatInfo(dataType, outValue, styleId);
                }
                else
                {
                    cellInfo = CellFormatInfo.FromValue(
                        value!,
                        zebraStripe,
                        bold,
                        columnLayout?.NumberFormat,
                        columnLayout?.HorizontalAlignment);
                }

                await WriteCellElementAsync(writer, cell, cellInfo);
                columnNumber++;
            }
        }
    }

    private async Task WriteCellElementAsync(XmlWriter writer, string cell, CellFormatInfo cellInfo)
    {
        await using (CreateElementScopeWithAttributes(writer, "c", new() { { "r", cell } }))
        {
            if (!string.IsNullOrEmpty(cellInfo.Type))
            {
                WriteAttributeString(writer, "t", cellInfo.Type);
            }

            if (cellInfo.StyleIndex != null)
            {
                WriteAttributeString(writer, "s", $"{cellInfo.StyleIndex}");
            }

            var valueString = cellInfo.GetValueAsString();
            if (!string.IsNullOrEmpty(valueString))
            {
                await using (CreateElementScope(writer, "v"))
                {
                    writer.WriteValue(valueString);
                }
            }
        }
    }
}
