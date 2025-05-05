using System;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport;

public class CorePropertiesWriter : XmlDocumentWriter
{
    private const string DCNamespace = "http://purl.org/dc/elements/1.1/";
    private const string CPNamespace = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
    private const string XSINamespace = "http://www.w3.org/2001/XMLSchema-instance";
    private const string DCTermsNamespace = "http://purl.org/dc/terms/";

    public CorePropertiesWriter(XmlWriter writer)
        : base(writer)
    {
    }

    public static string Path { get; } = "docProps/core.xml";

    public async Task WriteAsync(string creator)
        => await WriteAsync(creator, DateTime.UtcNow);

    public async Task WriteAsync(string creator, DateTime createdUtc)
        => await WriteAsync(creator, creator, createdUtc, createdUtc);

    public async Task WriteAsync(string creator, string lastModifiedBy, DateTime createdUtc, DateTime modifiedUtc)
    {
        await using (await CreateDocumentScopeAsync(standalone: true))
        await using (CreateElementScope("cp", "coreProperties", CPNamespace))
        {
            WriteAttributeString("xmlns", "dc", null, DCNamespace);
            WriteAttributeString("xmlns", "dcterms", null, DCTermsNamespace);
            WriteAttributeString("xmlns", "dcmitype", null, "http://purl.org/dc/dcmitype/");
            WriteAttributeString("xmlns", "xsi", null, XSINamespace);

            await WriteElementStringAsync("dc", "creator", DCNamespace, creator);
            await WriteElementStringAsync("cp", "lastModifiedBy", CPNamespace, lastModifiedBy);

            await WriteDateAsync("created", createdUtc);
            await WriteDateAsync("modified", modifiedUtc);
        }
    }

    private async Task WriteDateAsync(string element, DateTime value)
    {
        await using (CreateElementScope("dcterms", element, DCTermsNamespace))
        {
            WriteAttributeString("xsi", "type", XSINamespace, "dcterms:W3CDTF");
            await WriteStringAsync(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }
}
