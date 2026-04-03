using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.Writers.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class CorePropertiesWriter : XmlDocumentWriter
{
    public CorePropertiesWriter(
        XmlWriter writer,
        string? creator,
        DateTime? createdUtc = default,
        string? lastModifiedBy = default,
        DateTime? modifiedUtc = default,
        string? title = default,
        string? subject = default,
        string? description = default,
        string? keywords = default,
        string? category = default)
        : base(writer)
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

    public string? Creator { get; }

    public string? LastModifiedBy { get; }

    public DateTime CreatedUtc { get; }

    public DateTime ModifiedUtc { get; }

    public string? Title { get; }

    public string? Subject { get; }

    public string? Description { get; }

    public string? Keywords { get; }

    public string? Category { get; }

    protected override async Task WriteRootElementAsync(CancellationToken cancellationToken)
    {
        await using (CreateElementScope("cp", "coreProperties", Namespaces.cp))
        {
            WriteAttributeString("xmlns", "dc", null, Namespaces.dc);
            WriteAttributeString("xmlns", "dcterms", null, Namespaces.dcterms);
            WriteAttributeString("xmlns", "dcmitype", null, "http://purl.org/dc/dcmitype/");
            WriteAttributeString("xmlns", "xsi", null, Namespaces.xsi);

            if (!string.IsNullOrEmpty(Title))
            {
                await WriteElementStringAsync("dc", "title", Namespaces.dc, Title);
            }

            if (!string.IsNullOrEmpty(Subject))
            {
                await WriteElementStringAsync("dc", "subject", Namespaces.dc, Subject);
            }

            if (!string.IsNullOrEmpty(Description))
            {
                await WriteElementStringAsync("dc", "description", Namespaces.dc, Description);
            }

            if (!string.IsNullOrEmpty(Creator))
            {
                await WriteElementStringAsync("dc", "creator", Namespaces.dc, Creator);
            }

            if (!string.IsNullOrEmpty(Keywords))
            {
                await WriteElementStringAsync("cp", "keywords", Namespaces.cp, Keywords);
            }

            if (!string.IsNullOrEmpty(Category))
            {
                await WriteElementStringAsync("cp", "category", Namespaces.cp, Category);
            }

            if (!string.IsNullOrEmpty(LastModifiedBy))
            {
                await WriteElementStringAsync("cp", "lastModifiedBy", Namespaces.cp, LastModifiedBy);
            }

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
