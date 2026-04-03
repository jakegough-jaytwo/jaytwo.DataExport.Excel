using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class CorePropertiesWriterContext : IWriterContext
{
    public CorePropertiesWriterContext(string? creator, DateTime createdUtc)
        : this(creator, creator, createdUtc, createdUtc)
    {
    }

    public CorePropertiesWriterContext(string? creator, string? lastModifiedBy, DateTime createdUtc, DateTime modifiedUtc)
        : this(creator, lastModifiedBy, createdUtc, modifiedUtc, null, null, null, null, null)
    {
    }

    public CorePropertiesWriterContext(
        string? creator,
        string? lastModifiedBy,
        DateTime createdUtc,
        DateTime modifiedUtc,
        string? title,
        string? subject,
        string? description,
        string? keywords,
        string? category)
    {
        Creator = creator;
        LastModifiedBy = lastModifiedBy;
        CreatedUtc = createdUtc;
        ModifiedUtc = modifiedUtc;
        Title = title;
        Subject = subject;
        Description = description;
        Keywords = keywords;
        Category = category;
    }

    public string ZipPackagePath => "docProps/core.xml";

    public string? Creator { get; }

    public string? LastModifiedBy { get; }

    public DateTime CreatedUtc { get; }

    public DateTime ModifiedUtc { get; }

    public string? Title { get; }

    public string? Subject { get; }

    public string? Description { get; }

    public string? Keywords { get; }

    public string? Category { get; }

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new CorePropertiesWriter(
            writer,
            creator: Creator,
            lastModifiedBy: LastModifiedBy,
            createdUtc: CreatedUtc,
            modifiedUtc: ModifiedUtc,
            title: Title,
            subject: Subject,
            description: Description,
            keywords: Keywords,
            category: Category)
            .WriteAsync(cancellationToken);
}
