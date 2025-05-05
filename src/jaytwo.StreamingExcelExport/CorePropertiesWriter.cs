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

    public CorePropertiesWriter(XmlWriter writer, string creator)
        : this(writer, creator, DateTime.UtcNow)
    {
    }

    public CorePropertiesWriter(XmlWriter writer, string creator, DateTime createdUtc)
        : this(writer, creator, creator, createdUtc, createdUtc)
    {
    }

    public CorePropertiesWriter(XmlWriter writer, string creator, string lastModifiedBy, DateTime createdUtc, DateTime modifiedUtc)
        : base(writer)
    {
        Creator = creator;
        LastModifiedBy = lastModifiedBy;
        CreatedUtc = createdUtc;
        ModifiedUtc = modifiedUtc;
    }

    public static string Path { get; } = "docProps/core.xml";

    public string Creator { get; }

    public string LastModifiedBy { get; }

    public DateTime CreatedUtc { get; }

    public DateTime ModifiedUtc { get; }

    protected override async Task WriteRootElementAsync()
    {
        await using (CreateElementScope("cp", "coreProperties", CPNamespace))
        {
            WriteAttributeString("xmlns", "dc", null, DCNamespace);
            WriteAttributeString("xmlns", "dcterms", null, DCTermsNamespace);
            WriteAttributeString("xmlns", "dcmitype", null, "http://purl.org/dc/dcmitype/");
            WriteAttributeString("xmlns", "xsi", null, XSINamespace);

            await WriteElementStringAsync("dc", "creator", DCNamespace, Creator);
            await WriteElementStringAsync("cp", "lastModifiedBy", CPNamespace, LastModifiedBy);

            await WriteDateAsync("created", CreatedUtc);
            await WriteDateAsync("modified", ModifiedUtc);
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
