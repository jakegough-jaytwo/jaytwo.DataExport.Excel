using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

public class WorkbookRelationshipsWriter : RelationshipsWriter
{
    public WorkbookRelationshipsWriter(WorkbookRelationshipsWriterContext context, XmlWriter writer)
        : base(writer)
    {
        Context = context;
    }

    public WorkbookRelationshipsWriterContext Context { get; set; }

    protected override async Task WriteRelationshipElementsAsync()
    {
        foreach (var relationship in Context.Relationships)
        {
            await WriteRelationshipElementAsync(
                id: relationship.Id,
                type: "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet",
                target: relationship.Target);
        }
    }
}
