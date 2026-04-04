using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class StylesWriterContext : IWriterContext
{
    public StylesWriterContext(StyleRegistry styleRegistry)
    {
        StyleRegistry = styleRegistry;
    }

    public StyleRegistry StyleRegistry { get; }

    public string ZipPackagePath => "xl/styles.xml";

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new StylesWriter(this, writer).WriteAsync(cancellationToken);
}
