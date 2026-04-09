using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class DotRelsWriter : RelationshipsWriter
{
    public override string ZipPackagePath => "_rels/.rels";

    protected override async Task WriteRelationshipElementsAsync(XmlWriter writer)
    {
        await WriteRelationshipElementAsync(
            writer,
            id: "rId3",
            type: "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties",
            target: "docProps/app.xml");

        await WriteRelationshipElementAsync(
            writer,
            id: "rId2",
            type: "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties",
            target: "docProps/core.xml");

        await WriteRelationshipElementAsync(
            writer,
            id: "rId1",
            type: "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument",
            target: "xl/workbook.xml");
    }
}
