namespace jaytwo.DataExport.Excel.OpenXml;

internal record class WorksheetSpec
{
    public WorksheetSpec(string sheetName, string sheetTag, string sheetId, string relationshipId, string worksheetUid)
        => (SheetName, SheetTag, SheetId, RelationshipId, WorksheetUid) = (sheetName, sheetTag, sheetId, relationshipId, worksheetUid);

    public string SheetName { get; set; }

    public string SheetTag { get; set; }

    public string SheetId { get; set; }

    public string RelationshipId { get; set; }

    public string WorksheetUid { get; set; }
}
