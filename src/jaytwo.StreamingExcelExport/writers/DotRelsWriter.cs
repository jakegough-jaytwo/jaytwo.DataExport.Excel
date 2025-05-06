using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

public class DotRelsWriter : RelationshipsWriter
{
    public DotRelsWriter(DotRelsWriterContext context, XmlWriter writer)
        : base(writer)
    {
        Context = context;
    }

    public DotRelsWriterContext Context { get; }

    protected override async Task WriteRelationshipElementsAsync()
    {
        await WriteRelationshipElementAsync(
            id: "rId3",
            type: "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties",
            target: "docProps/app.xml");

        await WriteRelationshipElementAsync(
            id: "rId2",
            type: "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties",
            target: "docProps/core.xml");

        await WriteRelationshipElementAsync(
            id: "rId1",
            type: "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument",
            target: "xl/workbook.xml");
    }
}
