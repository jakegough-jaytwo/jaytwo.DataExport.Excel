using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal abstract class RelationshipsWriter : XmlDocumentWriter
{
    protected override async Task WriteRootElementAsync(XmlWriter writer, CancellationToken cancellationToken)
    {
        await using (CreateElementScope(writer, "Relationships", "http://schemas.openxmlformats.org/package/2006/relationships"))
        {
            await WriteRelationshipElementsAsync(writer);
        }
    }

    protected abstract Task WriteRelationshipElementsAsync(XmlWriter writer);

    protected async Task WriteRelationshipElementAsync(XmlWriter writer, string id, string type, string target)
        => await WriteElementWithAttributes(writer, "Relationship", new() { { "Id", id }, { "Type", type }, { "Target", target } });
}
