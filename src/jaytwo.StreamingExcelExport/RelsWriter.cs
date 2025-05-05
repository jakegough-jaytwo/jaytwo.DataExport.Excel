using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport;

public abstract class RelsWriter : XmlDocumentWriter
{
    public RelsWriter(XmlWriter writer)
        : base(writer)
    {
    }

    public async Task WriteAsync()
    {
        await using (await CreateDocumentScopeAsync(standalone: true))
        using (CreateElementScope("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships"))
        {
            await WriteRelationshipELementsAsync();
        }
    }

    protected abstract Task WriteRelationshipELementsAsync();

    protected async Task WriteRelationshipElementAsync(string id, string type, string target)
        => await WriteElementWithAttributes("Relationship", new() { { "Id", id }, { "Type", type }, { "Target", target } });
}
