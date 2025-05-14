using jaytwo.DataExport.Excel.Styles;

namespace jaytwo.DataExport.Excel;

public class ColumnDefinition
{
    public int? Width { get; set; }

    public NumberFormatStyles? NumberFormat { get; set; }

    public HorizontalAlignmentStyles? HorizontalAlignment { get; set; }
}
