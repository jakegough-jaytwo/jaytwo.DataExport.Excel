using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class CorePropertiesWriter : XmlDocumentWriter
{
    public CorePropertiesWriter(
        string? creator,
        DateTime? createdUtc = default,
        string? lastModifiedBy = default,
        DateTime? modifiedUtc = default,
        string? title = default,
        string? subject = default,
        string? description = default,
        string? keywords = default,
        string? category = default)
    {
        DateTime createdAtOrDefault = createdUtc ?? DateTime.UtcNow;

        Creator = creator;
        LastModifiedBy = lastModifiedBy;
        CreatedUtc = createdUtc ?? createdAtOrDefault;
        ModifiedUtc = modifiedUtc ?? createdAtOrDefault;
        Title = title;
        Subject = subject;
        Description = description;
        Keywords = keywords;
        Category = category;
    }

    public override string ZipPackagePath => "docProps/core.xml";

    public string? Creator { get; }

    public string? LastModifiedBy { get; }

    public DateTime CreatedUtc { get; }

    public DateTime ModifiedUtc { get; }

    public string? Title { get; }

    public string? Subject { get; }

    public string? Description { get; }

    public string? Keywords { get; }

    public string? Category { get; }

    protected override async Task WriteRootElementAsync(XmlWriter writer, CancellationToken cancellationToken)
    {
        await using (CreateElementScope(writer, "cp", "coreProperties", Namespaces.cp))
        {
            WriteAttributeString(writer, "xmlns", "dc", null, Namespaces.dc);
            WriteAttributeString(writer, "xmlns", "dcterms", null, Namespaces.dcterms);
            WriteAttributeString(writer, "xmlns", "dcmitype", null, "http://purl.org/dc/dcmitype/");
            WriteAttributeString(writer, "xmlns", "xsi", null, Namespaces.xsi);

            if (!string.IsNullOrEmpty(Title))
            {
                await WriteElementStringAsync(writer, "dc", "title", Namespaces.dc, Title);
            }

            if (!string.IsNullOrEmpty(Subject))
            {
                await WriteElementStringAsync(writer, "dc", "subject", Namespaces.dc, Subject);
            }

            if (!string.IsNullOrEmpty(Description))
            {
                await WriteElementStringAsync(writer, "dc", "description", Namespaces.dc, Description);
            }

            if (!string.IsNullOrEmpty(Creator))
            {
                await WriteElementStringAsync(writer, "dc", "creator", Namespaces.dc, Creator);
            }

            if (!string.IsNullOrEmpty(Keywords))
            {
                await WriteElementStringAsync(writer, "cp", "keywords", Namespaces.cp, Keywords);
            }

            if (!string.IsNullOrEmpty(Category))
            {
                await WriteElementStringAsync(writer, "cp", "category", Namespaces.cp, Category);
            }

            if (!string.IsNullOrEmpty(LastModifiedBy))
            {
                await WriteElementStringAsync(writer, "cp", "lastModifiedBy", Namespaces.cp, LastModifiedBy);
            }

            await WriteDateAsync(writer, "created", CreatedUtc);
            await WriteDateAsync(writer, "modified", ModifiedUtc);
        }
    }

    private async Task WriteDateAsync(XmlWriter writer, string element, DateTime value)
    {
        await using (CreateElementScope(writer, "dcterms", element, Namespaces.dcterms))
        {
            WriteAttributeString(writer, "xsi", "type", Namespaces.xsi, "dcterms:W3CDTF");
            await WriteStringAsync(writer, value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
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
