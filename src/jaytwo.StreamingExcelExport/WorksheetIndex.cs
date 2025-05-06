using System;
using System.Collections.Generic;
using System.Linq;

namespace jaytwo.StreamingExcelExport;

public class WorksheetIndex
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
        var sheetTag = GetSheetTag(sheetName);
        var relationship = _relationships.AddSheet(sheetTag);

        var sheet = new WorksheetSpec(
            sheetName: sheetName,
            sheetTag: sheetTag,
            sheetId: GetNextSheetId(),
            relationshipId: relationship.Id);

        _sheets.Add(sheet);

        return sheet;
    }

    private string GetNextSheetId() => $"{_sheets.Count + 1}";

    private string GetSheetTag(string sheetName) => sheetName.ToLower();
}
