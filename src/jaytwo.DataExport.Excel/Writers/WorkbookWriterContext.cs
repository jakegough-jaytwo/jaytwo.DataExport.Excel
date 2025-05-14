using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jaytwo.DataExport.Excel.OpenXml;

namespace jaytwo.DataExport.Excel.Writers;

internal class WorkbookWriterContext : IWriterContext
{
    public WorkbookWriterContext(WorksheetSpec[] sheets)
    {
        Sheets = sheets;
    }

    public string ZipPackagePath => "xl/workbook.xml";

    public WorksheetSpec[] Sheets { get; }

    public async Task WriteAsync(XmlWriter writer, CancellationToken cancellationToken)
        => await new WorkbookWriter(this, writer).WriteAsync(cancellationToken);
}
