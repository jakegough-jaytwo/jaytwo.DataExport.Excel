namespace jaytwo.DataExport.Excel.Writers;

internal class LocaleCellXfsEntry
{
    public LocaleCellXfsEntry(string? fontId, string? fillId, string numFmtId, string? alignmentHorizontal)
    {
        FontId = fontId;
        FillId = fillId;
        NumFmtId = numFmtId;
        AlignmentHorizontal = alignmentHorizontal;
    }

    public string? FontId { get; }

    public string? FillId { get; }

    public string NumFmtId { get; }

    public string? AlignmentHorizontal { get; }
}
