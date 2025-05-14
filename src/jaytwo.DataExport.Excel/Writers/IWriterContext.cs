using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace jaytwo.DataExport.Excel.Writers;

internal interface IWriterContext
{
    string ZipPackagePath { get; }

    Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken);
}
