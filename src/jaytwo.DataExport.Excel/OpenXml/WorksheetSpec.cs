namespace jaytwo.DataExport.Excel.OpenXml;

internal record class WorksheetSpec
{
    public WorksheetSpec(string sheetName, string sheetTag, string sheetId, string relationshipId)
        => (SheetName, SheetTag, SheetId, RelationshipId) = (sheetName, sheetTag, sheetId, relationshipId);

    public string SheetName { get; set; }

    public string SheetTag { get; set; }

    public string SheetId { get; set; }

    public string RelationshipId { get; set; }
}
