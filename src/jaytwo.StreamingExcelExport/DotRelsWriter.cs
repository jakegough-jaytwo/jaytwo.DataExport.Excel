using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport;

public class DotRelsWriter : RelsWriter
{
    public DotRelsWriter(XmlWriter writer)
        : base(writer)
    {
    }

    public static string Path { get; } = "_rels/.rels";

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
