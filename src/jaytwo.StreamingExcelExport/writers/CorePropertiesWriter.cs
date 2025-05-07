using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.StreamingExcelExport.Writers.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

internal class CorePropertiesWriter : XmlDocumentWriter
{
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

    public string Creator { get; }

    public string LastModifiedBy { get; }

    public DateTime CreatedUtc { get; }

    public DateTime ModifiedUtc { get; }

    protected override async Task WriteRootElementAsync(CancellationToken cancellationToken)
    {
        await using (CreateElementScope("cp", "coreProperties", Namespaces.cp))
        {
            WriteAttributeString("xmlns", "dc", null, Namespaces.dc);
            WriteAttributeString("xmlns", "dcterms", null, Namespaces.dcterms);
            WriteAttributeString("xmlns", "dcmitype", null, "http://purl.org/dc/dcmitype/");
            WriteAttributeString("xmlns", "xsi", null, Namespaces.xsi);

            await WriteElementStringAsync("dc", "creator", Namespaces.dc, Creator);
            await WriteElementStringAsync("cp", "lastModifiedBy", Namespaces.cp, LastModifiedBy);

            await WriteDateAsync("created", CreatedUtc);
            await WriteDateAsync("modified", ModifiedUtc);
        }
    }

    private async Task WriteDateAsync(string element, DateTime value)
    {
        await using (CreateElementScope("dcterms", element, Namespaces.dcterms))
        {
            WriteAttributeString("xsi", "type", Namespaces.xsi, "dcterms:W3CDTF");
            await WriteStringAsync(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }

    private class Namespaces
    {
#pragma warning disable SA1303 // Const field names should begin with upper-case letter
        public const string dc = "http://purl.org/dc/elements/1.1/";
        public const string cp = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
        public const string xsi = "http://www.w3.org/2001/XMLSchema-instance";
        public const string dcterms = "http://purl.org/dc/terms/";
#pragma warning restore SA1303 // Const field names should begin with upper-case letter
    }
}
