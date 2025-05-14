using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class ContentTypesWriterContext : IWriterContext
{
    public ContentTypesWriterContext(string[] sheetTags, bool hasStyleSheet)
    {
        SheetTags = sheetTags;
        HasStyleSheet = hasStyleSheet;
    }

    public string ZipPackagePath => "[Content_Types].xml";

    public string[] SheetTags { get; }

    public bool HasStyleSheet { get; }

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new ContentTypesWriter(this, writer).WriteAsync(cancellationToken);
}
