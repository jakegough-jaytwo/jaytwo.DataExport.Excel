using System;
using System.Collections.Generic;
using System.Linq;

namespace jaytwo.StreamingExcelExport;

public class RelationshipIndex
{
    private const string WorksheetType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet";

    private readonly List<RelationshipSpec> _relationships;

    public RelationshipIndex()
    {
        _relationships = new List<RelationshipSpec>();
    }

    public RelationshipSpec[] Relationshnips
        => _relationships.AsReadOnly().ToArray();

    public RelationshipSpec AddSheet(string sheetTag)
        => Add($"worksheets/{sheetTag}.xml", WorksheetType);

    public RelationshipSpec Add(string target, string type)
    {
        var key = GetNextRelationshipId();
        var relationship = new RelationshipSpec(id: key, type: type, target: target);
        _relationships.Add(relationship);
        return relationship;
    }

    private string GetNextRelationshipId() => $"rId{_relationships.Count + 1}";
}
