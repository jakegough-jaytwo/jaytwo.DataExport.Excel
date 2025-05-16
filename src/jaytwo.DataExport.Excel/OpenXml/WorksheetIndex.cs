using System;
using System.Collections.Generic;
using System.Linq;

namespace jaytwo.DataExport.Excel.OpenXml;

internal class WorksheetIndex
{
    private readonly RelationshipIndex _relationships;
    private readonly List<WorksheetSpec> _sheets = new List<WorksheetSpec>();

    public WorksheetIndex(RelationshipIndex relationships)
    {
        _relationships = relationships;
    }

    public WorksheetSpec[] Sheets => _sheets.AsReadOnly().ToArray();

    public string[] SheetNames => _sheets.Select(x => x.SheetName).ToArray();

    public string[] SheetTags => _sheets.Select(x => x.SheetTag).ToArray();

    public WorksheetSpec Add(string sheetName)
    {
        var sheetId = GetNextSheetId();
        var sheetTag = $"sheet{sheetId}";
        var relationship = _relationships.AddSheet(sheetTag);

        var sheet = new WorksheetSpec(
            sheetName: sheetName,
            sheetTag: sheetTag,
            sheetId: $"{sheetId}",
            relationshipId: relationship.Id);

        _sheets.Add(sheet);

        return sheet;
    }

    private int GetNextSheetId() => _sheets.Count + 1;
}
