using System.Globalization;
using jaytwo.DataExport.Excel.Styles;

namespace jaytwo.DataExport.Excel;

/// <summary>
/// Defines per-column formatting options such as width, number format, alignment, and locale-aware format codes.
/// </summary>
public class ColumnDefinition
{
    /// <summary>
    /// Gets or sets the column width in character units. When null, the column uses Excel's default width.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets an Excel built-in number format to apply to this column. Excel interprets these
    /// formats using its own locale at render time. For explicit locale control, use <see cref="LocaleFormat"/>
    /// with <see cref="Culture"/> instead.
    /// </summary>
    public NumberFormatStyles? NumberFormat { get; set; }

    /// <summary>
    /// Gets or sets the horizontal alignment of cell content in this column.
    /// When null, Excel's default alignment is used (generally left for text, right for numbers).
    /// </summary>
    public HorizontalAlignmentStyles? HorizontalAlignment { get; set; }

    /// <summary>
    /// Gets or sets a locale-aware format to apply to this column. When set, an explicit format
    /// code is written into the stylesheet based on <see cref="Culture"/>, rather than relying on
    /// Excel's built-in format interpretation. Takes precedence over <see cref="NumberFormat"/>.
    /// </summary>
    public LocaleNumberFormatStyles? LocaleFormat { get; set; }

    /// <summary>
    /// Gets or sets the culture used to generate the format code when <see cref="LocaleFormat"/> is set.
    /// Defaults to <see cref="CultureInfo.InvariantCulture"/> when not specified.
    /// </summary>
    public CultureInfo? Culture { get; set; }
}
