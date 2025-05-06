using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

public class CorePropertiesWriterContext : IWriterContext
{
    public CorePropertiesWriterContext(string creator, DateTime createdUtc)
        : this(creator, creator, createdUtc, createdUtc)
    {
    }

    public CorePropertiesWriterContext(string creator, string lastModifiedBy, DateTime createdUtc, DateTime modifiedUtc)
    {
        Creator = creator;
        LastModifiedBy = lastModifiedBy;
        CreatedUtc = createdUtc;
        ModifiedUtc = modifiedUtc;
    }

    public string ZipPackagePath => "docProps/core.xml";

    public string Creator { get; }

    public string LastModifiedBy { get; }

    public DateTime CreatedUtc { get; }

    public DateTime ModifiedUtc { get; }

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new CorePropertiesWriter(
            writer,
            creator: Creator,
            lastModifiedBy: LastModifiedBy,
            createdUtc: CreatedUtc,
            modifiedUtc: ModifiedUtc)
            .WriteAsync();
}
