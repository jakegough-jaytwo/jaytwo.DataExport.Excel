using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal class DotRelsWriterContext : IWriterContext
{
    public string ZipPackagePath => "_rels/.rels";

    public Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => new DotRelsWriter(this, writer).WriteAsync(cancellationToken);
}
