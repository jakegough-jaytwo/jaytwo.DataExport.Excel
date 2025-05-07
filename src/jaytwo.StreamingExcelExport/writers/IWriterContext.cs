using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.StreamingExcelExport.Writers;

internal interface IWriterContext
{
    string ZipPackagePath { get; }

    Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken);
}
