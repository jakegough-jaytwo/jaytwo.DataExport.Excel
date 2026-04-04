using System;
using System.Collections;
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

    public StylesWriter(StylesWriterContext context, XmlWriter writer)
        : base(writer)
    {
        Context = context;
        _styleRegistry = context.StyleRegistry;
    }

    public static int CellXfsStaticCount => CellXfs.StaticCount;

    protected StylesWriterContext Context { get; }

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

    protected override async Task WriteRootElementAsync(CancellationToken cancellationToken)
    {
        await using (CreateElementScope("styleSheet", "http://schemas.openxmlformats.org/spreadsheetml/2006/main"))
        {
            WriteAttributeString("xmlns", "mc", null, "http://schemas.openxmlformats.org/markup-compatibility/2006");
            WriteAttributeString("mc", "Ignorable", "http://schemas.openxmlformats.org/markup-compatibility/2006", "x14ac x16r2 xr");
            WriteAttributeString("xmlns", "x14ac", null, "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            WriteAttributeString("xmlns", "x16r2", null, "http://schemas.microsoft.com/office/spreadsheetml/2015/02/main");
            WriteAttributeString("xmlns", "xr", null, "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");

            await WriteNumFmts();
            await WriteFontsElementAsync();
            await WriteFillsElementAsync();
            await WriteBordersElementAsync();
            await WriteCellXfs();
            await WriteCellStyles();
        }
    }

    private async Task WriteFontsElementAsync()
    {
        await using (CreateElementScopeWithAttributes("fonts", new() { { "count", "2" } }))
        {
            await using (CreateElementScope("font"))
            {
            }

            await using (CreateElementScope("font"))
            {
                await using (CreateElementScope("b"))
                {
                }
            }
        }
    }

    private async Task WriteFillsElementAsync()
    {
        await using (CreateElementScopeWithAttributes("fills", new() { { "count", "3" } }))
        {
            await using (CreateElementScope("fill"))
            {
                await WriteElementWithAttributes("patternFill", new() { { "patternType", "none" } });
            }

            await using (CreateElementScope("fill"))
            {
                await WriteElementWithAttributes("patternFill", new() { { "patternType", "gray125" } });
            }

            await using (CreateElementScope("fill"))
            {
                await using (CreateElementScopeWithAttributes("patternFill", new() { { "patternType", "solid" } }))
                {
                    await WriteElementWithAttributes("fgColor", new() { { "rgb", "FFE6F0FA" } });
                    await WriteElementWithAttributes("bgColor", new() { { "indexed", "64" } });
                }
            }
        }
    }

    private async Task WriteBordersElementAsync()
    {
        await using (CreateElementScopeWithAttributes("borders", new() { { "count", "1" } }))
        {
            await WriteBorderElementAsync();
        }
    }

    private async Task WriteBorderElementAsync()
    {
        await using (CreateElementScope("border"))
        {
            await using (CreateElementScope("left"))
            {
            }

            await using (CreateElementScope("right"))
            {
            }

            await using (CreateElementScope("top"))
            {
            }

            await using (CreateElementScope("bottom"))
            {
            }

            await using (CreateElementScope("diagonal"))
            {
            }
        }
    }

    private async Task WriteNumFmts()
    {
        var numFmts = NumFmt.GetAll();
        var localeNumFmts = _styleRegistry.GetNumFmts();
        await using (CreateElementScopeWithAttributes("numFmts", new() { { "count", $"{numFmts.Length + localeNumFmts.Length}" } }))
        {
            foreach (var numFmt in numFmts)
            {
                await WriteElementWithAttributes("numFmt", new() { { "numFmtId", numFmt.NumFmtId }, { "formatCode", numFmt.FormatCode } });
            }

            foreach (var (numFmtId, formatCode) in localeNumFmts)
            {
                await WriteElementWithAttributes("numFmt", new() { { "numFmtId", numFmtId }, { "formatCode", formatCode } });
            }
        }
    }

    private async Task WriteCellXfs()
    {
        var localeEntries = _styleRegistry.GetCellXfs();
        await using (CreateElementScopeWithAttributes("cellXfs", new() { { "count", $"{CellXfs.All.Count + localeEntries.Length}" } }))
        {
            foreach (var cellXfs in CellXfs.All)
            {
                await using (CreateElementScopeWithAttributes("xf", new() { { "xfId", "0" } }))
                {
                    if (!string.IsNullOrEmpty(cellXfs.FontId))
                    {
                        WriteAttributeString("fontId", cellXfs.FontId);
                        WriteAttributeString("applyFont", "1");
                    }

                    if (!string.IsNullOrEmpty(cellXfs.FillId))
                    {
                        WriteAttributeString("fillId", cellXfs.FillId);
                        WriteAttributeString("applyFill", "1");
                    }

                    if (!string.IsNullOrEmpty(cellXfs.NumFmtId))
                    {
                        WriteAttributeString("numFmtId", cellXfs.NumFmtId);
                        WriteAttributeString("applyNumberFormat", "1");
                    }

                    // this needs to be last since it adds an inner element
                    if (!string.IsNullOrEmpty(cellXfs.AlignmentHorizontal))
                    {
                        WriteAttributeString("applyAlignment", "1");

                        await WriteElementWithAttributes("alignment", new() { { "horizontal", cellXfs.AlignmentHorizontal } });
                    }
                }
            }

            foreach (var entry in localeEntries)
            {
                await using (CreateElementScopeWithAttributes("xf", new() { { "xfId", "0" } }))
                {
                    if (!string.IsNullOrEmpty(entry.FontId))
                    {
                        WriteAttributeString("fontId", entry.FontId);
                        WriteAttributeString("applyFont", "1");
                    }

                    if (!string.IsNullOrEmpty(entry.FillId))
                    {
                        WriteAttributeString("fillId", entry.FillId);
                        WriteAttributeString("applyFill", "1");
                    }

                    WriteAttributeString("numFmtId", entry.NumFmtId);
                    WriteAttributeString("applyNumberFormat", "1");

                    // this needs to be last since it adds an inner element
                    if (!string.IsNullOrEmpty(entry.AlignmentHorizontal))
                    {
                        WriteAttributeString("applyAlignment", "1");

                        await WriteElementWithAttributes("alignment", new() { { "horizontal", entry.AlignmentHorizontal } });
                    }
                }
            }
        }
    }

    private async Task WriteCellStyles()
    {
        await using (CreateElementScopeWithAttributes("cellStyles", new() { { "count", "1" } }))
        {
            await WriteElementWithAttributes("cellStyle", new() { { "name", "Normal" }, { "xfId", "0" }, { "builtinId", "0" } });
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
