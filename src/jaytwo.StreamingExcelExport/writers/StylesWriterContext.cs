using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

internal class StylesWriterContext : IWriterContext
{
    public StylesWriterContext()
    {
    }

    public string ZipPackagePath => "xl/styles.xml";

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new StylesWriter(this, writer).WriteAsync(cancellationToken);
}
