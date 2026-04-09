using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.Styles;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class StylesWriter : XmlDocumentWriter
{
    private readonly StyleRegistry _styleRegistry;

    public StylesWriter(StyleRegistry styleRegistry)
    {
        _styleRegistry = styleRegistry;
    }

    public static int CellXfsStaticCount => CellXfs.StaticCount;

    public override string ZipPackagePath => "xl/styles.xml";

    public static int GetStyleIndex(
        FontStyles? fontStyle,
        FillStyles? fillStyle,
        NumberFormatStyles? numberFormatStyle,
        HorizontalAlignmentStyles? horizontalAlignmentStyle)
        => CellXfs.GetStyleIndex(
            fontStyle ?? FontStyles.Default,
            fillStyle ?? FillStyles.Default,
            numberFormatStyle ?? NumberFormatStyles.General,
            horizontalAlignmentStyle ?? HorizontalAlignmentStyles.Default);

    protected override async Task WriteRootElementAsync(XmlWriter writer, CancellationToken cancellationToken)
    {
        await using (CreateElementScope(writer, "styleSheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"))
        {
            WriteAttributeString(writer, "xmlns", "mc", null, "http://schemas.openxmlformats.org/markup-compatibility/2006");
            WriteAttributeString(writer, "mc", "Ignorable", "http://schemas.openxmlformats.org/markup-compatibility/2006", "x14ac x16r2 xr");
            WriteAttributeString(writer, "xmlns", "x14ac", null, "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            WriteAttributeString(writer, "xmlns", "x16r2", null, "http://schemas.microsoft.com/office/spreadsheetml/2015/02/main");
            WriteAttributeString(writer, "xmlns", "xr", null, "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");

            await WriteNumFmts(writer);
            await WriteFontsElementAsync(writer);
            await WriteFillsElementAsync(writer);
            await WriteBordersElementAsync(writer);
            await WriteCellXfs(writer);
            await WriteCellStyles(writer);
        }
    }

    private async Task WriteFontsElementAsync(XmlWriter writer)
    {
        await using (CreateElementScopeWithAttributes(writer, "fonts", new() { { "count", "2" } }))
        {
            await using (CreateElementScope(writer, "font"))
            {
            }

            await using (CreateElementScope(writer, "font"))
            {
                await using (CreateElementScope(writer, "b"))
                {
                }
            }
        }
    }

    private async Task WriteFillsElementAsync(XmlWriter writer)
    {
        await using (CreateElementScopeWithAttributes(writer, "fills", new() { { "count", "3" } }))
        {
            await using (CreateElementScope(writer, "fill"))
            {
                await WriteElementWithAttributes(writer, "patternFill", new() { { "patternType", "none" } });
            }

            await using (CreateElementScope(writer, "fill"))
            {
                await WriteElementWithAttributes(writer, "patternFill", new() { { "patternType", "gray125" } });
            }

            await using (CreateElementScope(writer, "fill"))
            {
                await using (CreateElementScopeWithAttributes(writer, "patternFill", new() { { "patternType", "solid" } }))
                {
                    await WriteElementWithAttributes(writer, "fgColor", new() { { "rgb", "FFE6F0FA" } });
                    await WriteElementWithAttributes(writer, "bgColor", new() { { "indexed", "64" } });
                }
            }
        }
    }

    private async Task WriteBordersElementAsync(XmlWriter writer)
    {
        await using (CreateElementScopeWithAttributes(writer, "borders", new() { { "count", "1" } }))
        {
            await WriteBorderElementAsync(writer);
        }
    }

    private async Task WriteBorderElementAsync(XmlWriter writer)
    {
        await using (CreateElementScope(writer, "border"))
        {
            await using (CreateElementScope(writer, "left"))
            {
            }

            await using (CreateElementScope(writer, "right"))
            {
            }

            await using (CreateElementScope(writer, "top"))
            {
            }

            await using (CreateElementScope(writer, "bottom"))
            {
            }

            await using (CreateElementScope(writer, "diagonal"))
            {
            }
        }
    }

    private async Task WriteNumFmts(XmlWriter writer)
    {
        var numFmts = NumFmt.GetAll();
        var localeNumFmts = _styleRegistry.GetNumFmts();
        await using (CreateElementScopeWithAttributes(writer, "numFmts", new() { { "count", $"{numFmts.Length + localeNumFmts.Length}" } }))
        {
            foreach (var numFmt in numFmts)
            {
                await WriteElementWithAttributes(writer, "numFmt", new() { { "numFmtId", numFmt.NumFmtId }, { "formatCode", numFmt.FormatCode } });
            }

            foreach (var (numFmtId, formatCode) in localeNumFmts)
            {
                await WriteElementWithAttributes(writer, "numFmt", new() { { "numFmtId", numFmtId }, { "formatCode", formatCode } });
            }
        }
    }

    private async Task WriteCellXfs(XmlWriter writer)
    {
        var localeEntries = _styleRegistry.GetCellXfs();
        await using (CreateElementScopeWithAttributes(writer, "cellXfs", new() { { "count", $"{CellXfs.All.Count + localeEntries.Length}" } }))
        {
            foreach (var cellXfs in CellXfs.All)
            {
                await WriteXfElementAsync(writer, cellXfs.FontId, cellXfs.FillId, cellXfs.NumFmtId, cellXfs.AlignmentHorizontal);
            }

            foreach (var entry in localeEntries)
            {
                await WriteXfElementAsync(writer, entry.FontId, entry.FillId, entry.NumFmtId, entry.AlignmentHorizontal);
            }
        }
    }

    private async Task WriteXfElementAsync(XmlWriter writer, string? fontId, string? fillId, string? numFmtId, string? alignmentHorizontal)
    {
        await using (CreateElementScopeWithAttributes(writer, "xf", new() { { "xfId", "0" } }))
        {
            if (!string.IsNullOrEmpty(fontId))
            {
                WriteAttributeString(writer, "fontId", fontId);
                WriteAttributeString(writer, "applyFont", "1");
            }

            if (!string.IsNullOrEmpty(fillId))
            {
                WriteAttributeString(writer, "fillId", fillId);
                WriteAttributeString(writer, "applyFill", "1");
            }

            if (!string.IsNullOrEmpty(numFmtId))
            {
                WriteAttributeString(writer, "numFmtId", numFmtId);
                WriteAttributeString(writer, "applyNumberFormat", "1");
            }

            // this needs to be last since it adds an inner element
            if (!string.IsNullOrEmpty(alignmentHorizontal))
            {
                WriteAttributeString(writer, "applyAlignment", "1");

                await WriteElementWithAttributes(writer, "alignment", new() { { "horizontal", alignmentHorizontal } });
            }
        }
    }

    private async Task WriteCellStyles(XmlWriter writer)
    {
        await using (CreateElementScopeWithAttributes(writer, "cellStyles", new() { { "count", "1" } }))
        {
            await WriteElementWithAttributes(writer, "cellStyle", new() { { "name", "Normal" }, { "xfId", "0" }, { "builtinId", "0" } });
        }
    }

    private class CellXfs
    {
        public CellXfs(
            FontStyles? fontStyle = null,
            string? fontId = null,
            FillStyles? fillStyle = null,
            string? fillId = null,
            NumberFormatStyles? numberFormatStyle = null,
            string? numFmtId = null,
            HorizontalAlignmentStyles? horizontalAlignmentStyle = null,
            string? alignmentHorizontal = null)
        {
            FontStyle = fontStyle ?? FontStyles.Default;
            FontId = fontId;
            FillStyle = fillStyle ?? FillStyles.Default;
            FillId = fillId;
            NumberFormatStyle = numberFormatStyle ?? NumberFormatStyles.General;
            NumFmtId = numFmtId;
            HorizontalAlignmentStyle = horizontalAlignmentStyle ?? HorizontalAlignmentStyles.Default;
            AlignmentHorizontal = alignmentHorizontal;
        }

        public static IList<CellXfs> All { get; } = CreateAllVariants();

        public static int StaticCount => All.Count;

        public string? FontId { get; }

        public FontStyles FontStyle { get; }

        public string? FillId { get; }

        public FillStyles FillStyle { get; }

        public string? NumFmtId { get; }

        public NumberFormatStyles NumberFormatStyle { get; }

        public string? AlignmentHorizontal { get; }

        public HorizontalAlignmentStyles HorizontalAlignmentStyle { get; }

        public static int GetStyleIndex(
            FontStyles fontStyle,
            FillStyles fillStyle,
            NumberFormatStyles numberFormatStyle,
            HorizontalAlignmentStyles horizontalAlignmentStyle)
        {
            var found = All
                .Where(x => x.FontStyle == fontStyle)
                .Where(x => x.FillStyle == fillStyle)
                .Where(x => x.NumberFormatStyle == numberFormatStyle)
                .Where(x => x.HorizontalAlignmentStyle == horizontalAlignmentStyle)
                .Single();

            return All.IndexOf(found);
        }

        private static IList<CellXfs> CreateAllVariants()
        {
            var fontVariants = new[]
            {
                new CellXfs(fontStyle: FontStyles.Default),
                new CellXfs(fontStyle: FontStyles.Bold, fontId: "1"),
            };

            var fillVariants = new[]
            {
                new CellXfs(fillStyle: FillStyles.Default),
                new CellXfs(fillStyle: FillStyles.Stripe, fillId: "2"),
            };

            var numberFormatVariants = Enum.GetValues(typeof(NumberFormatStyles))
                .Cast<NumberFormatStyles>()
                .Select(x => new CellXfs(numberFormatStyle: x, numFmtId: $"{(int)x}"))
                .ToArray();

            var alignmentHorizontalVariants = new[]
            {
                new CellXfs(horizontalAlignmentStyle: HorizontalAlignmentStyles.Default),
                new CellXfs(horizontalAlignmentStyle: HorizontalAlignmentStyles.Left, alignmentHorizontal: "left"),
                new CellXfs(horizontalAlignmentStyle: HorizontalAlignmentStyles.Center, alignmentHorizontal: "center"),
                new CellXfs(horizontalAlignmentStyle: HorizontalAlignmentStyles.Right, alignmentHorizontal: "right"),
            };

            var result = new List<CellXfs>();
            foreach (var fontVariant in fontVariants)
            {
                foreach (var filLVariant in fillVariants)
                {
                    foreach (var numberFormatVariant in numberFormatVariants)
                    {
                        foreach (var alignmentHorizontalVariant in alignmentHorizontalVariants)
                        {
                            result.Add(new CellXfs(
                                fontStyle: fontVariant.FontStyle,
                                fontId: fontVariant.FontId,
                                fillStyle: filLVariant.FillStyle,
                                fillId: filLVariant.FillId,
                                numberFormatStyle: numberFormatVariant.NumberFormatStyle,
                                numFmtId: numberFormatVariant.NumFmtId,
                                horizontalAlignmentStyle: alignmentHorizontalVariant.HorizontalAlignmentStyle,
                                alignmentHorizontal: alignmentHorizontalVariant.AlignmentHorizontal));
                        }
                    }
                }
            }

            return result;
        }
    }

    private class NumFmt
    {
        public NumFmt(string numFmtId, string formatCode)
        {
            NumFmtId = numFmtId;
            FormatCode = formatCode;
        }

        public string NumFmtId { get; }

        public string FormatCode { get; }

        public static NumFmt[] GetAll() => new[]
        {
            new NumFmt(numFmtId: $"{(int)NumberFormatStyles.DateSortable}",        formatCode: "yyyy-mm-dd"),
            new NumFmt(numFmtId: $"{(int)NumberFormatStyles.DateTimeSortable}",    formatCode: "yyyy-mm-dd\"T\"hh:mm:ss"),
            new NumFmt(numFmtId: $"{(int)NumberFormatStyles.DateDayOfWeek}",       formatCode: "dddd"),
            new NumFmt(numFmtId: $"{(int)NumberFormatStyles.DateDayOfWeekShort}",  formatCode: "ddd"),
            new NumFmt(numFmtId: $"{(int)NumberFormatStyles.DateYearMonth}",       formatCode: ExcelFormatHelper.GetExcelYearMonthFormat(CultureInfo.InvariantCulture)),
            new NumFmt(numFmtId: $"{(int)NumberFormatStyles.DateMonthDay}",        formatCode: ExcelFormatHelper.GetExcelMonthDayFormat(CultureInfo.InvariantCulture)),
        };
    }
}
