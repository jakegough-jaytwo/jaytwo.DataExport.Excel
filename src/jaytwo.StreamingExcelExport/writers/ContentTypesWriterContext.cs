using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

public class ContentTypesWriterContext : IWriterContext
{
    public ContentTypesWriterContext(string[] sheetTags)
    {
        SheetTags = sheetTags;
    }

    public string ZipPackagePath => "[Content_Types].xml";

    public string[] SheetTags { get; }

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new ContentTypesWriter(this, writer).WriteAsync();
}
