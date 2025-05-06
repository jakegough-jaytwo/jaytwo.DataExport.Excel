using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

public abstract class RelationshipsWriter : XmlDocumentWriter
{
    public RelationshipsWriter(XmlWriter writer)
        : base(writer)
    {
    }

    protected override async Task WriteRootElementAsync()
    {
        await using (CreateElementScope("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships"))
        {
            await WriteRelationshipElementsAsync();
        }
    }

    protected abstract Task WriteRelationshipElementsAsync();

    protected async Task WriteRelationshipElementAsync(string id, string type, string target)
        => await WriteElementWithAttributes("Relationship", new() { { "Id", id }, { "Type", type }, { "Target", target } });
}
