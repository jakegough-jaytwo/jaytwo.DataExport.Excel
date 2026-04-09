using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.OpenXml;

namespace jaytwo.DataExport.Excel.Writers;

internal class WorkbookRelationshipsWriter : RelationshipsWriter
{
    private readonly IList<RelationshipSpec> _relationships;

    public WorkbookRelationshipsWriter(IList<RelationshipSpec> relationships)
    {
        _relationships = relationships;
    }

    public override string ZipPackagePath => "xl/_rels/workbook.xml.rels";

    protected override async Task WriteRelationshipElementsAsync(XmlWriter writer)
    {
        foreach (var relationship in _relationships)
        {
            await WriteRelationshipElementAsync(writer, id: relationship.Id, type: relationship.Type, target: relationship.Target);
        }
    }
}
