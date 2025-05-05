using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport;

public abstract class RelsWriter : XmlDocumentWriter
{
    public RelsWriter(XmlWriter writer)
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
