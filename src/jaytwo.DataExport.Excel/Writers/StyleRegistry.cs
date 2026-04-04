using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using jaytwo.DataExport.Excel.Styles;

namespace jaytwo.DataExport.Excel.Writers;

internal class StyleRegistry
{
    private readonly Dictionary<(LocaleNumberFormatStyles Format, CultureInfo Culture), int> _numFmtIds = new();
    private readonly Dictionary<(LocaleNumberFormatStyles Format, CultureInfo Culture, FontStyles Font, FillStyles Fill, HorizontalAlignmentStyles Alignment), int> _styleIds = new();

    private int _nextNumFmtId = 180;
    private int _nextStyleIndex = StylesWriter.CellXfsStaticCount;

    public int GetOrRegisterStyleIndex(
        LocaleNumberFormatStyles format,
        CultureInfo culture,
        FontStyles font,
        FillStyles fill,
        HorizontalAlignmentStyles alignment)
    {
        var styleKey = (format, culture, font, fill, alignment);
        if (_styleIds.TryGetValue(styleKey, out var existingStyleId))
        {
            return existingStyleId;
        }

        var numFmtKey = (format, culture);
        if (!_numFmtIds.ContainsKey(numFmtKey))
        {
            _numFmtIds[numFmtKey] = _nextNumFmtId++;
        }

        var newStyleId = _nextStyleIndex++;
        _styleIds[styleKey] = newStyleId;
        return newStyleId;
    }

    public (string NumFmtId, string FormatCode)[] GetNumFmts()
    {
        return _numFmtIds
            .Select(kvp => ($"{kvp.Value}", GetFormatCode(kvp.Key.Format, kvp.Key.Culture)))
            .ToArray();
    }

    public LocaleCellXfsEntry[] GetCellXfs()
    {
        return _styleIds
            .OrderBy(kvp => kvp.Value)
            .Select(kvp =>
            {
                var (format, culture, font, fill, alignment) = kvp.Key;
                var numFmtId = _numFmtIds[(format, culture)];
                return new LocaleCellXfsEntry(
                    fontId: font == FontStyles.Bold ? "1" : null,
                    fillId: fill == FillStyles.Stripe ? "2" : null,
                    numFmtId: $"{numFmtId}",
                    alignmentHorizontal: alignment switch
                    {
                        HorizontalAlignmentStyles.Left => "left",
                        HorizontalAlignmentStyles.Center => "center",
                        HorizontalAlignmentStyles.Right => "right",
                        _ => null,
                    });
            })
            .ToArray();
    }

    private static string GetFormatCode(LocaleNumberFormatStyles format, CultureInfo culture)
        => format switch
        {
            LocaleNumberFormatStyles.DateShort => ExcelFormatHelper.GetExcelShortDateFormat(culture),
            LocaleNumberFormatStyles.DateLong => ExcelFormatHelper.GetExcelLongDateFormat(culture),
            LocaleNumberFormatStyles.YearMonth => ExcelFormatHelper.GetExcelYearMonthFormat(culture),
            LocaleNumberFormatStyles.MonthDay => ExcelFormatHelper.GetExcelMonthDayFormat(culture),
            LocaleNumberFormatStyles.ShortTime => ExcelFormatHelper.GetExcelShortTimeFormat(culture),
            LocaleNumberFormatStyles.LongTime => ExcelFormatHelper.GetExcelLongTimeFormat(culture),
            LocaleNumberFormatStyles.DateTime => ExcelFormatHelper.GetExcelFullDateTimeShortTimeFormat(culture),
            LocaleNumberFormatStyles.DateTimeFull => ExcelFormatHelper.GetExcelFullDateTimeLongTimeFormat(culture),
            LocaleNumberFormatStyles.Currency => ExcelFormatHelper.GetExcelCurrencyFormat(culture),
            LocaleNumberFormatStyles.Number => ExcelFormatHelper.GetExcelNumberFormat(culture),
            _ => throw new NotSupportedException($"LocaleNumberFormatStyles.{format} is not supported."),
        };
}
